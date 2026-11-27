using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Riftbound.Network;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    public static class GearAuditAndPersistenceChecks
    {
        const string ReportPath = ".code-check/emberford-r1/gear-audit-persistence-checks.txt";

        [MenuItem("Riftbound/Emberford/Run Gear Audit, Progression & Persistence Checks (EditMode)")]
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

            string tempDir = Path.Combine(Path.GetTempPath(), $"rb_gear_audit_{Guid.NewGuid():N}");
            string offlineSavePath = Path.Combine(tempDir, "offline_save.json");
            GameObject tempRoot = null;
            string prevOverride = PlayerController.SavePathOverride;

            try
            {
                Directory.CreateDirectory(tempDir);
                tempRoot = new GameObject("GearAuditTempRoot");

                // 1. ItemCatalog integrity + zero broken quest/chest/enemy/set IDs
                var catalogIds = new HashSet<string>(ItemCatalog.All.Select(i => i.id), StringComparer.Ordinal);
                bool hasNewCatalog = catalogIds.Contains("alliance-war-banner") && catalogIds.Contains("sunforged-breastplate");

                var fqs = tempRoot.AddComponent<FactionQuestSystem>();
                typeof(FactionQuestSystem).GetMethod("BuildCatalog", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(fqs, null);

                var questRewardIds = new List<string>();
                var catalogField = typeof(FactionQuestSystem).GetField("catalog", BindingFlags.Static | BindingFlags.NonPublic);
                if (catalogField?.GetValue(null) is System.Collections.IEnumerable qEnum)
                {
                    foreach (var q in qEnum)
                    {
                        var rf = q.GetType().GetField("rewardItem");
                        string r = rf?.GetValue(q) as string;
                        if (!string.IsNullOrEmpty(r)) questRewardIds.Add(r);
                    }
                }
                var brokenQuestIds = questRewardIds.Where(id => !catalogIds.Contains(id)).Distinct().ToArray();

                // Verify all SharedRewardCatalog chest & enemy drops exist in ItemCatalog.All
                string[] allChestIds =
                {
                    "pinewatch-boss-chest", "ashenmaw-guardian-chest", "sunstone-abbey-chest",
                    "ironmaw-vault-chest", "river-reaver-lockbox", "worldboss-gorgonax-chest",
                    "cathedral-altar-chest", "wyrmcrest-hoard-chest", "crypt-reliquary",
                    "village-cache", "camp-cache", "tower-cache"
                };
                string[] allEnemyDropIds =
                {
                    "vermithrax-boss", "redridge-outcast-boss", "boss-abbot-vael", "boss-malakar",
                    "crypt-warden", "ashwood-captain", "road-raider-2", "camp-guard-1"
                };
                var brokenShared = new List<string>();
                foreach (var cid in allChestIds)
                    foreach (var iid in SharedRewardCatalog.GetChestItemIds(cid))
                        if (!catalogIds.Contains(iid)) brokenShared.Add($"{cid}->{iid}");
                foreach (var eid in allEnemyDropIds)
                    foreach (var iid in SharedRewardCatalog.GetEnemyDropItemIds(eid))
                        if (!catalogIds.Contains(iid)) brokenShared.Add($"{eid}->{iid}");

                Check("catalog_integrity_and_zero_broken_reward_ids",
                    hasNewCatalog && brokenQuestIds.Length == 0 && brokenShared.Count == 0,
                    $"catalogCount={catalogIds.Count}, brokenQuest={brokenQuestIds.Length}, brokenShared={brokenShared.Count}");

                // 2. Unreachable/mismatched sources fixed + Gorgonax 3-epic deterministic completion + crafting exclusivity
                bool bloodrockOk = SharedRewardCatalog.GetEnemyDropItemId("redridge-outcast-boss") == "bloodrock-waraxe";
                var vaelDrops = SharedRewardCatalog.GetEnemyDropItemIds("boss-abbot-vael");
                bool vaelOk = vaelDrops.Contains("scarlet-aegis") && vaelDrops.Contains("duskwood-shadow-dagger");
                bool vermithraxOk = SharedRewardCatalog.GetEnemyDropItemId("vermithrax-boss") == "wyrmtooth-edge";
                bool allSixFrontierChestsMapped =
                    SharedRewardCatalog.GetChestItemIds("pinewatch-boss-chest").Count > 0 &&
                    SharedRewardCatalog.GetChestItemIds("ashenmaw-guardian-chest").Count > 0 &&
                    SharedRewardCatalog.GetChestItemIds("sunstone-abbey-chest").Count > 0 &&
                    SharedRewardCatalog.GetChestItemIds("ironmaw-vault-chest").Count > 0 &&
                    SharedRewardCatalog.GetChestItemIds("river-reaver-lockbox").Count > 0 &&
                    SharedRewardCatalog.GetChestItemIds("worldboss-gorgonax-chest").Count >= 2;

                // Gorgonax deterministic 3-epic completion
                var ownedSim = new HashSet<string>(StringComparer.Ordinal);
                string gKill = SharedRewardCatalog.ResolveGorgonaxKillItem(id => ownedSim.Contains(id));
                ownedSim.Add(gKill);
                foreach (var ci in SharedRewardCatalog.ResolveGorgonaxChestItems(id => ownedSim.Contains(id)))
                    ownedSim.Add(ci);
                bool gorgonaxAll3 = ownedSim.Contains("worldbreaker-maul") &&
                                    ownedSim.Contains("crown-of-molten-ruin") &&
                                    ownedSim.Contains("cinderfury-signet");

                // Crafting exclusivity: copper-dagger, bronze-broadsword, iron-buckler, truesilver-blade NOT in quests/chests/enemies
                var craftExclusive = new[] { "copper-dagger", "bronze-broadsword", "iron-buckler", "truesilver-blade" };
                bool craftClean = !questRewardIds.Any(id => craftExclusive.Contains(id)) &&
                                  !allChestIds.SelectMany(SharedRewardCatalog.GetChestItemIds).Any(id => craftExclusive.Contains(id)) &&
                                  !allEnemyDropIds.SelectMany(SharedRewardCatalog.GetEnemyDropItemIds).Any(id => craftExclusive.Contains(id));

                Check("all_sources_reachable_gorgonax_3_epics_and_crafting_exclusivity",
                    bloodrockOk && vaelOk && vermithraxOk && allSixFrontierChestsMapped && gorgonaxAll3 && craftClean,
                    $"bloodrock={bloodrockOk}, vael={vaelOk}, vermithrax={vermithraxOk}, chests6={allSixFrontierChestsMapped}, gorgonax3={gorgonaxAll3}, craftExclusive={craftClean}");

                // 3. Monotonic weapon progression: wyrmtooth-edge (30) > worldbreaker-maul (25) > ironmaw-cleaver (23) > truesilver-blade (21) > bronze-broadsword (9) > raider-cleaver (6) > copper-dagger (4) > starter-axe (0)
                int Dmg(string id) => ItemCatalog.Get(id)?.damage ?? -1;
                bool progressionOk =
                    Dmg("wyrmtooth-edge") > Dmg("worldbreaker-maul") &&
                    Dmg("worldbreaker-maul") > Dmg("ironmaw-cleaver") &&
                    Dmg("ironmaw-cleaver") > Dmg("truesilver-blade") &&
                    Dmg("truesilver-blade") > Dmg("bronze-broadsword") &&
                    Dmg("bronze-broadsword") > Dmg("raider-cleaver") &&
                    Dmg("raider-cleaver") > Dmg("copper-dagger") &&
                    Dmg("copper-dagger") > Dmg("starter-axe");
                Check("starter_to_crafted_to_boss_weapon_progression",
                    progressionOk,
                    $"wyrmtooth={Dmg("wyrmtooth-edge")} > worldbreaker={Dmg("worldbreaker-maul")} > ironmaw={Dmg("ironmaw-cleaver")} > truesilver={Dmg("truesilver-blade")} > bronze={Dmg("bronze-broadsword")} > raider={Dmg("raider-cleaver")} > copper={Dmg("copper-dagger")} > starter={Dmg("starter-axe")}");

                // 4. Emberford ServerCharacterSave persists ownedItems + equippedWeaponId + equippedArmorId + equippedTrinketId across restart
                RealmPersistenceService.Initialize(tempDir);
                RealmPersistenceService.RegisterOrUpdateAccount("gear_tester", "secret-pw", RealmRole.Player);
                var ch = RealmPersistenceService.LoadOrCreateCharacter("gear_tester", "Vaelorn", "Human", "Warrior", "Alliance");
                ch.ownedItems.Add("worldbreaker-maul");
                ch.ownedItems.Add("crown-of-molten-ruin");
                ch.ownedItems.Add("cinderfury-signet");
                ch.equippedWeaponId = "worldbreaker-maul";
                ch.equippedArmorId = "crown-of-molten-ruin";
                ch.equippedTrinketId = "cinderfury-signet";
                RealmPersistenceService.SaveCharacter(ch);
                RealmPersistenceService.TryClaimBossKillOnce("gear_tester", "Vaelorn", "worldboss-gorgonax", 65, 150);
                RealmPersistenceService.TryClaimChestOnce("gear_tester", "Vaelorn", "worldboss-gorgonax-chest", 25, 40);

                // Full server restart simulation
                RealmPersistenceService.ShutdownForTest();
                RealmPersistenceService.Initialize(tempDir);
                var reloaded = RealmPersistenceService.LoadOrCreateCharacter("gear_tester", "Vaelorn");

                bool serverGearPersisted =
                    reloaded != null &&
                    reloaded.equippedWeaponId == "worldbreaker-maul" &&
                    reloaded.equippedArmorId == "crown-of-molten-ruin" &&
                    reloaded.equippedTrinketId == "cinderfury-signet" &&
                    reloaded.ownedItems.Contains("worldbreaker-maul") &&
                    reloaded.ownedItems.Contains("crown-of-molten-ruin") &&
                    reloaded.ownedItems.Contains("cinderfury-signet") &&
                    !RealmPersistenceService.TryClaimBossKillOnce("gear_tester", "Vaelorn", "worldboss-gorgonax", 65, 150) &&
                    !RealmPersistenceService.TryClaimChestOnce("gear_tester", "Vaelorn", "worldboss-gorgonax-chest", 25, 40);

                Check("emberford_server_gear_and_equip_persists_across_restart_without_reset_to_starter_axe",
                    serverGearPersisted,
                    $"weapon={reloaded?.equippedWeaponId}, armor={reloaded?.equippedArmorId}, trinket={reloaded?.equippedTrinketId}, owned={reloaded?.ownedItems?.Count}");

                // 5. Offline PlayerController loot + equip + SaveAdventure + RestoreEquipment roundtrip
                CoopSessionConfig.ResetToOffline();
                PlayerController.SavePathOverride = offlineSavePath;
                var pGo = new GameObject("OfflineGearTestPlayer");
                pGo.transform.SetParent(tempRoot.transform);
                var stats = pGo.AddComponent<Combatant>();
                stats.ConfigureVitals(150, 0);
                stats.Restore();
                var pc = pGo.AddComponent<PlayerController>();
                typeof(PlayerController).GetProperty("Stats")?.SetValue(pc, stats);

                pc.GrantItem("ironmaw-cleaver");
                pc.GrantItem("ursolak-spirit-pelt");
                pc.GrantItem("pinewatch-warden-crest");
                pc.EquipIfOwned("ironmaw-cleaver");
                pc.EquipIfOwned("ursolak-spirit-pelt");
                pc.EquipIfOwned("pinewatch-warden-crest");
                bool rejectedUnowned = !pc.EquipIfOwned("wyrmtooth-edge") && pc.EquippedWeaponId == "ironmaw-cleaver";
                bool savedOffline = pc.SaveAdventure(false);

                // Fresh PlayerController instance loading the saved offline JSON
                var pGo2 = new GameObject("OfflineGearTestPlayer2");
                pGo2.transform.SetParent(tempRoot.transform);
                var stats2 = pGo2.AddComponent<Combatant>();
                stats2.ConfigureVitals(150, 0);
                stats2.Restore();
                var pc2 = pGo2.AddComponent<PlayerController>();
                typeof(PlayerController).GetProperty("Stats")?.SetValue(pc2, stats2);
                pc2.LoadAdventure();

                bool offlineRoundtripOk =
                    savedOffline && rejectedUnowned &&
                    pc2.EquippedWeaponId == "ironmaw-cleaver" &&
                    pc2.EquippedArmorId == "ursolak-spirit-pelt" &&
                    pc2.EquippedTrinketId == "pinewatch-warden-crest" &&
                    pc2.OwnsItem("ironmaw-cleaver") &&
                    pc2.OwnsItem("ursolak-spirit-pelt") &&
                    pc2.OwnsItem("pinewatch-warden-crest");

                Check("offline_loot_equip_unowned_rejection_and_save_reload_roundtrip",
                    offlineRoundtripOk,
                    $"saved={savedOffline}, rejectedUnowned={rejectedUnowned}, w={pc2.EquippedWeaponId}, a={pc2.EquippedArmorId}, t={pc2.EquippedTrinketId}");
            }
            catch (Exception ex)
            {
                Check("unexpected_exception", false, ex.ToString());
            }
            finally
            {
                PlayerController.SavePathOverride = prevOverride;
                RealmPersistenceService.ShutdownForTest();
                if (tempRoot) Object.DestroyImmediate(tempRoot);
                try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }

                lines.Add($"TOTAL PASS={passed} FAIL={failed} RESULT={(failed == 0 ? "PASS" : "FAIL")}");
                Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
                File.WriteAllText(ReportPath, string.Join("\n", lines) + "\n");
                Debug.Log($"[GearAuditAndPersistenceChecks] PASS={passed} FAIL={failed}");
                if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
            }
        }
    }
}
