using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Riftbound.Network;
using UnityEditor;
using UnityEngine;

namespace Riftbound.Editor
{
    public static class EmberfordR2AuthPersistenceChecks
    {
        const string ReportPath = ".code-check/emberford-r1/emberford-r2-checks.txt";

        [MenuItem("Riftbound/Emberford/Run R2 Auth, Persistence & Role Audit Checks (EditMode)")]
        public static void Run()
        {
            var lines = new List<string>();
            int passed = 0;
            int failed = 0;

            void Check(string name, bool ok, string detail)
            {
                lines.Add($"{(ok ? "PASS" : "FAIL")} [{name}] {detail}");
                if (ok) passed++; else failed++;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), $"rb_emberford_r2_{Guid.NewGuid():N}");
            try
            {
                // 1. Initialize server data dir & register accounts across all 5 roles + 1 banned account
                RealmPersistenceService.Initialize(tempDir);
                RealmPersistenceService.RegisterOrUpdateAccount("alice", "alice-secret-123", RealmRole.Player);
                RealmPersistenceService.RegisterOrUpdateAccount("guide_bob", "guide-secret", RealmRole.Guide);
                RealmPersistenceService.RegisterOrUpdateAccount("mod_carol", "mod-secret", RealmRole.Moderator);
                RealmPersistenceService.RegisterOrUpdateAccount("gm_dave", "gm-secret", RealmRole.GameMaster);
                RealmPersistenceService.RegisterOrUpdateAccount("admin_eve", "admin-secret", RealmRole.RealmAdmin);
                RealmPersistenceService.RegisterOrUpdateAccount("banned_mallory", "mallory-secret", RealmRole.Player, isBanned: true);

                // 2. Authenticated realm entry checks
                bool validOk = RealmPersistenceService.Authenticate("alice", "alice-secret-123", true, out var aliceAcc, out _);
                bool badPassDenied = !RealmPersistenceService.Authenticate("alice", "wrong-pass", true, out _, out string badPassErr);
                bool unknownDenied = !RealmPersistenceService.Authenticate("ghost_user", "any", true, out _, out string unknownErr);
                bool bannedDenied = !RealmPersistenceService.Authenticate("banned_mallory", "mallory-secret", true, out _, out string bannedErr);
                Check("r2_authenticated_realm_entry",
                    validOk && aliceAcc != null && badPassDenied && unknownDenied && bannedDenied,
                    $"valid={validOk}, badPassDenied={badPassDenied}, unknownDenied={unknownDenied}, bannedDenied={bannedDenied}");

                // 3. Server-owned character progress & one-time reward claims before server restart
                var ch = RealmPersistenceService.LoadOrCreateCharacter("alice", "Aelindra", "Elf", "Mage", "Alliance");
                ch.posX = -223f; ch.posY = 14.5f; ch.posZ = 400f;
                RealmPersistenceService.SaveCharacter(ch);

                bool bossClaim1 = RealmPersistenceService.TryClaimBossKillOnce("alice", "Aelindra", "worldboss-gorgonax", 65, 150);
                bool chestClaim1 = RealmPersistenceService.TryClaimChestOnce("alice", "Aelindra", "worldboss-gorgonax-chest", 25, 40);
                bool questClaim1 = RealmPersistenceService.TryClaimQuestOnce("alice", "Aelindra", "ashenmaw_bear_guardian", 160, 300);

                int levelBeforeRestart = ch.level;
                int xpBeforeRestart = ch.experience;
                int goldBeforeRestart = ch.gold;

                // 4. SIMULATE FULL SERVER RESTART (wipe all in-memory caches and re-initialize from disk!)
                RealmPersistenceService.ShutdownForTest();
                RealmPersistenceService.Initialize(tempDir);

                var reloaded = RealmPersistenceService.LoadOrCreateCharacter("alice", "Aelindra");
                bool survivedRestart = reloaded.level == levelBeforeRestart &&
                                       reloaded.experience == xpBeforeRestart &&
                                       reloaded.gold == goldBeforeRestart &&
                                       Mathf.Approximately(reloaded.posX, -223f) &&
                                       Mathf.Approximately(reloaded.posZ, 400f);
                Check("r2_character_progress_survives_server_restart", survivedRestart,
                    $"lv={levelBeforeRestart}->{reloaded.level}, xp={xpBeforeRestart}->{reloaded.experience}, gold={goldBeforeRestart}->{reloaded.gold}");

                // 5. Verify zero duplicate rewards AFTER server restart
                bool bossDupBlocked = !RealmPersistenceService.TryClaimBossKillOnce("alice", "Aelindra", "worldboss-gorgonax", 65, 150);
                bool chestDupBlocked = !RealmPersistenceService.TryClaimChestOnce("alice", "Aelindra", "worldboss-gorgonax-chest", 25, 40);
                bool questDupBlocked = !RealmPersistenceService.TryClaimQuestOnce("alice", "Aelindra", "ashenmaw_bear_guardian", 160, 300);
                bool noExtraGold = reloaded.gold == goldBeforeRestart && reloaded.level == levelBeforeRestart && reloaded.experience == xpBeforeRestart;
                Check("r2_no_duplicate_rewards_after_restart",
                    bossClaim1 && chestClaim1 && questClaim1 && bossDupBlocked && chestDupBlocked && questDupBlocked && noExtraGold,
                    $"bossDupBlocked={bossDupBlocked}, chestDupBlocked={chestDupBlocked}, questDupBlocked={questDupBlocked}, goldUnchanged={noExtraGold}");

                // 6. Server-enforced 5-tier role hierarchy checks (Player < Guide < Moderator < GameMaster < RealmAdmin)
                bool playerDeniedGuide = !RealmPersistenceService.TryAuthorizeStaffAction("alice", RealmStaffAction.AnnounceHelp, "realm", "hi", out _);
                bool guideAllowedHelp = RealmPersistenceService.TryAuthorizeStaffAction("guide_bob", RealmStaffAction.AnnounceHelp, "realm", "tip", out _);
                bool guideDeniedKick = !RealmPersistenceService.TryAuthorizeStaffAction("guide_bob", RealmStaffAction.KickPlayer, "alice", "test", out _);
                bool modAllowedKick = RealmPersistenceService.TryAuthorizeStaffAction("mod_carol", RealmStaffAction.KickPlayer, "alice", "spam", out _);
                bool modDeniedTp = !RealmPersistenceService.TryAuthorizeStaffAction("mod_carol", RealmStaffAction.TeleportPlayer, "alice", "tp", out _);
                bool gmAllowedTp = RealmPersistenceService.TryAuthorizeStaffAction("gm_dave", RealmStaffAction.TeleportPlayer, "alice", "unstuck", out _);
                bool gmDeniedSetRole = !RealmPersistenceService.TryAuthorizeStaffAction("gm_dave", RealmStaffAction.SetRole, "alice", "promote", out _);
                bool adminPromotedAlice = RealmPersistenceService.TrySetAccountRole("admin_eve", "alice", RealmRole.Moderator, out _);

                // Restart again to verify role change persisted on disk!
                RealmPersistenceService.ShutdownForTest();
                RealmPersistenceService.Initialize(tempDir);
                bool aliceNowMod = RealmPersistenceService.GetRole("alice") == RealmRole.Moderator;

                Check("r2_five_tier_role_enforcement_and_persistence",
                    playerDeniedGuide && guideAllowedHelp && guideDeniedKick && modAllowedKick && modDeniedTp && gmAllowedTp && gmDeniedSetRole && adminPromotedAlice && aliceNowMod,
                    $"pDen={playerDeniedGuide}, gOk={guideAllowedHelp}, gDen={guideDeniedKick}, mOk={modAllowedKick}, mDen={modDeniedTp}, gmOk={gmAllowedTp}, gmDen={gmDeniedSetRole}, adminProm={adminPromotedAlice}, persistedMod={aliceNowMod}");

                // 7. Append-only audit.jsonl contains both ALLOWED and DENIED records
                string[] auditLines = File.Exists(RealmPersistenceService.AuditLogPath)
                    ? File.ReadAllLines(RealmPersistenceService.AuditLogPath)
                    : Array.Empty<string>();
                bool hasAllowed = auditLines.Any(l => l.Contains("\"result\":\"ALLOWED\""));
                bool hasDenied = auditLines.Any(l => l.Contains("\"result\":\"DENIED\""));
                bool hasRoleAudit = auditLines.Any(l => l.Contains("\"action\":\"SetRole\""));
                Check("r2_append_only_audit_log_recorded",
                    auditLines.Length >= 10 && hasAllowed && hasDenied && hasRoleAudit,
                    $"lines={auditLines.Length}, allowed={hasAllowed}, denied={hasDenied}, setRole={hasRoleAudit}");

                // 8. Legacy v1 profession migration (omits professionInitialized/skills/reagents/ownedItems) + v2 empty-reagent persistence
                string v1Path = Path.Combine(RealmPersistenceService.CharactersDirectory, "alice__legacyv1.json");
                Directory.CreateDirectory(RealmPersistenceService.CharactersDirectory);
                File.WriteAllText(v1Path,
                    "{\"accountId\":\"alice\",\"characterName\":\"LegacyV1\",\"race\":\"Human\",\"className\":\"Warrior\",\"faction\":\"Alliance\",\"level\":3,\"experience\":120,\"gold\":50,\"claimedChests\":[\"village-cache\"]}");

                var v1Loaded = RealmPersistenceService.LoadOrCreateCharacter("alice", "LegacyV1");
                bool v1Migrated = v1Loaded != null &&
                                  v1Loaded.professionInitialized &&
                                  v1Loaded.alchemySkill == Riftbound.ProfessionSystem.DefaultAlchemySkill &&
                                  v1Loaded.blacksmithingSkill == Riftbound.ProfessionSystem.DefaultBlacksmithingSkill &&
                                  v1Loaded.engineeringSkill == Riftbound.ProfessionSystem.DefaultEngineeringSkill &&
                                  v1Loaded.reagents != null &&
                                  v1Loaded.reagents.Count == Riftbound.ProfessionSystem.StarterReagents.Length &&
                                  v1Loaded.reagents.Contains("Peacebloom=6") &&
                                  v1Loaded.reagents.Contains("Silver Ore=2") &&
                                  v1Loaded.ownedItems != null &&
                                  v1Loaded.ownedItems.Contains("starter-axe") &&
                                  v1Loaded.ownedItems.Contains("leather-vest");

                // Reload after restart -> idempotent, no further migration
                RealmPersistenceService.ShutdownForTest();
                RealmPersistenceService.Initialize(tempDir);
                var v1Reloaded = RealmPersistenceService.LoadOrCreateCharacter("alice", "LegacyV1");
                bool v1Idempotent = v1Reloaded != null &&
                                    v1Reloaded.professionInitialized &&
                                    v1Reloaded.reagents != null &&
                                    v1Reloaded.reagents.Count == Riftbound.ProfessionSystem.StarterReagents.Length &&
                                    !RealmPersistenceService.NormalizeCharacterSave(v1Reloaded);

                // Case B: new v2 character defaults
                bool v2DefaultsOk = ch.professionInitialized &&
                                    ch.reagents != null &&
                                    ch.reagents.Count == Riftbound.ProfessionSystem.StarterReagents.Length;

                // Case C: v2 character with professionInitialized=true and all reagents spent (reagents=[]) must NOT re-seed on reload
                var spent = RealmPersistenceService.LoadOrCreateCharacter("alice", "SpentV2", "Human", "Warrior", "Alliance");
                spent.reagents.Clear();
                spent.professionInitialized = true;
                RealmPersistenceService.SaveCharacter(spent);
                RealmPersistenceService.ShutdownForTest();
                RealmPersistenceService.Initialize(tempDir);
                var spentReloaded = RealmPersistenceService.LoadOrCreateCharacter("alice", "SpentV2");
                bool v2EmptyNotReseeded = spentReloaded != null &&
                                          spentReloaded.professionInitialized &&
                                          spentReloaded.reagents != null &&
                                          spentReloaded.reagents.Count == 0;

                Check("r2_legacy_v1_profession_migration_and_v2_empty_reagents",
                    v1Migrated && v1Idempotent && v2DefaultsOk && v2EmptyNotReseeded,
                    $"v1Migrated={v1Migrated}, v1Idempotent={v1Idempotent}, v2Defaults={v2DefaultsOk}, v2EmptyNotReseeded={v2EmptyNotReseeded}");
            }
            catch (Exception ex)
            {
                Check("exception", false, ex.ToString());
            }
            finally
            {
                RealmPersistenceService.ShutdownForTest();
                try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }
            }

            lines.Add($"TOTAL PASS={passed} FAIL={failed} RESULT={(failed == 0 ? "PASS" : "FAIL")}");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, string.Join("\n", lines) + "\n");
            Debug.Log($"[EmberfordR2AuthPersistenceChecks] PASS={passed} FAIL={failed}");
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
    }
}
