using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Riftbound.Network;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Riftbound.Editor
{
    [InitializeOnLoad]
    public static class ServerAbilityUnlockChecks
    {
        const string PlayKey = "Riftbound.ServerAbilityUnlockChecks.Active";
        const string PrevDirectPlayKey = "Riftbound.ServerAbilityUnlockChecks.PrevDirect";
        const string FailedKey = "Riftbound.ServerAbilityUnlockChecks.Failed";
        const string EditReportPath = ".code-check/emberford-r1/server-ability-unlock-editmode.txt";
        const string PlayReportPath = ".code-check/emberford-r1/server-ability-unlock-playmode.txt";

        static readonly List<string> Lines = new List<string>();
        static int stage;
        static int passed;
        static int failedCount;
        static double dueAt;
        static double timeoutAt;

        static ServerAbilityUnlockChecks()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Riftbound/Emberford/Run Server Ability Unlock Unit Checks (EditMode)")]
        public static void RunEditMode()
        {
            var report = new List<string>();
            int pass = 0;
            int fail = 0;

            void Check(string name, bool ok, string detail)
            {
                report.Add($"{(ok ? "PASS" : "FAIL")} [{name}] {detail}");
                if (ok) pass++; else fail++;
            }

            // Threshold constants
            Check("unlock_level_constants",
                RiftboundCoopPlayer.Slot1UnlockLevel == 1 &&
                RiftboundCoopPlayer.Slot2UnlockLevel == 3 &&
                RiftboundCoopPlayer.Slot3UnlockLevel == 5,
                $"s1={RiftboundCoopPlayer.Slot1UnlockLevel}, s2={RiftboundCoopPlayer.Slot2UnlockLevel}, s3={RiftboundCoopPlayer.Slot3UnlockLevel}");

            // Slot 1 abilities unlock at level 1 (rejected at level 0)
            string[] slot1Ids = { "melee", "fireball", "interrupt" };
            bool s1Ok = true;
            foreach (var id in slot1Ids)
            {
                bool reqOk = RiftboundCoopPlayer.TryGetAbilitySlotRequirement(id, out int slot, out int req) && slot == 1 && req == 1;
                bool matrixOk = !RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 0) &&
                                RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 1) &&
                                RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 3) &&
                                RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 5);
                if (!reqOk || !matrixOk) s1Ok = false;
            }
            Check("slot1_unlocks_at_level_1", s1Ok, string.Join(",", slot1Ids));

            // Slot 2 abilities unlock at level 3 (locked at levels 1 and 2)
            string[] slot2Ids = { "frostnova", "eviscerate", "consecration", "whirlwind" };
            bool s2Ok = true;
            foreach (var id in slot2Ids)
            {
                bool reqOk = RiftboundCoopPlayer.TryGetAbilitySlotRequirement(id, out int slot, out int req) && slot == 2 && req == 3;
                bool matrixOk = !RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 1) &&
                                !RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 2) &&
                                RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 3) &&
                                RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 5);
                if (!reqOk || !matrixOk) s2Ok = false;
            }
            Check("slot2_unlocks_at_level_3", s2Ok, string.Join(",", slot2Ids));

            // Slot 3 abilities unlock at level 5 (locked at levels 1, 2, 3, and 4)
            string[] slot3Ids = { "arcane_ward", "sprint", "holy_light", "second_wind" };
            bool s3Ok = true;
            foreach (var id in slot3Ids)
            {
                bool reqOk = RiftboundCoopPlayer.TryGetAbilitySlotRequirement(id, out int slot, out int req) && slot == 3 && req == 5;
                bool matrixOk = !RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 1) &&
                                !RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 2) &&
                                !RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 3) &&
                                !RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 4) &&
                                RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 5) &&
                                RiftboundCoopPlayer.IsAbilityUnlockedForLevel(id, 6);
                if (!reqOk || !matrixOk) s3Ok = false;
            }
            Check("slot3_unlocks_at_level_5", s3Ok, string.Join(",", slot3Ids));

            // Unknown/empty abilities fail closed at all levels
            bool unknownRejected = !RiftboundCoopPlayer.IsAbilityUnlockedForLevel("unknown_ability", 99) &&
                                   !RiftboundCoopPlayer.IsAbilityUnlockedForLevel("", 99) &&
                                   !RiftboundCoopPlayer.IsAbilityUnlockedForLevel(null, 99);
            Check("unknown_abilities_fail_closed", unknownRejected, $"unknownRejected={unknownRejected}");

            Directory.CreateDirectory(Path.GetDirectoryName(EditReportPath));
            report.Add($"TOTAL PASS={pass} FAIL={fail} RESULT={(fail == 0 ? "PASS" : "FAIL")}");
            File.WriteAllText(EditReportPath, string.Join("\n", report) + "\n");
            Debug.Log($"[ServerAbilityUnlockChecks.EditMode] PASS={pass} FAIL={fail}");
            if (Application.isBatchMode) EditorApplication.Exit(fail == 0 ? 0 : 1);
        }

        [MenuItem("Riftbound/Emberford/Run Server Ability Unlock PlayMode Checks")]
        public static void RunPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Start ServerAbilityUnlockChecks outside Play Mode.");

            Directory.CreateDirectory(Path.GetDirectoryName(PlayReportPath));
            EditorPrefs.SetBool(PrevDirectPlayKey, EditorPrefs.GetBool("Riftbound.DirectPlayInEditor", false));
            EditorPrefs.SetBool("Riftbound.DirectPlayInEditor", true);
            SessionState.SetBool(PlayKey, true);
            SessionState.SetBool(FailedKey, false);
            EditorApplication.isPaused = false;
            EditorSceneManager.OpenScene(WorldRegions.ScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ConfigurePlayModeSession()
        {
            if (!SessionState.GetBool(PlayKey, false)) return;
            PlayerController.SavePathOverride = Path.Combine(Path.GetTempPath(), $"rb_unlock_play_{Guid.NewGuid():N}.json");
            CoopSessionConfig.Mode = CoopSessionMode.HostLan;
            CoopSessionConfig.Port = 7791;
            Application.runInBackground = true;
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (!SessionState.GetBool(PlayKey, false)) return;

            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                Lines.Clear();
                stage = 0;
                passed = 0;
                failedCount = 0;
                dueAt = EditorApplication.timeSinceStartup + 0.5;
                timeoutAt = EditorApplication.timeSinceStartup + 35.0;
                EditorApplication.update += Tick;
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                bool failed = SessionState.GetBool(FailedKey, true);
                SessionState.SetBool(PlayKey, false);
                bool prevDirect = EditorPrefs.GetBool(PrevDirectPlayKey, false);
                EditorPrefs.SetBool("Riftbound.DirectPlayInEditor", prevDirect);
                CoopSessionConfig.ResetToOffline();
                if (Application.isBatchMode) EditorApplication.Exit(failed ? 1 : 0);
            }
        }

        static void Check(string name, bool ok, string detail)
        {
            Lines.Add($"{(ok ? "PASS" : "FAIL")} [{name}] {detail}");
            if (ok) passed++; else failedCount++;
        }

        static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > timeoutAt)
                    throw new TimeoutException("ServerAbilityUnlockChecks timeout at stage " + stage);
                if (EditorApplication.isPaused) EditorApplication.isPaused = false;
                Time.timeScale = 1f;
                if (EditorApplication.timeSinceStartup < dueAt) return;

                if (stage == 0)
                {
                    var cp = RiftboundCoopPlayer.LocalOwner;
                    var enemy = EnemyController.FindByKey("road-raider-1");
                    var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
                    if (cp == null || !cp.IsSpawned || !cp.IsServer || enemy == null || pc == null)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.15;
                        return;
                    }

                    // Position cleanly next to road-raider-1 with clear LOS and damage player slightly so heal checks are testable
                    var cc = pc.GetComponent<CharacterController>();
                    if (cc) cc.enabled = false;
                    pc.transform.position = enemy.transform.position + new Vector3(2.2f, 0f, 0f);
                    if (cc) cc.enabled = true;
                    cp.AuthorizeHostTeleport(pc.transform.position);
                    cp.ResourcePct.Value = 85;
                    cp.Health.Value = 40;
                    cp.MaxHealth.Value = 130;

                    stage = 1;
                    dueAt = EditorApplication.timeSinceStartup + 0.15;
                }
                else if (stage == 1)
                {
                    var cp = RiftboundCoopPlayer.LocalOwner;
                    var enemy = EnemyController.FindByKey("road-raider-1");
                    var eStats = enemy.GetComponent<Combatant>();
                    var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

                    // LastServerMoveTime field via reflection to verify Arcane Ward Blink leeway isn't mutated on rejection
                    var moveTimeField = typeof(RiftboundCoopPlayer).GetField("lastServerMoveTime", BindingFlags.NonPublic | BindingFlags.Instance);
                    var cooldownsField = typeof(RiftboundCoopPlayer).GetField("serverAbilityCooldowns", BindingFlags.NonPublic | BindingFlags.Instance);
                    var cooldowns = (Dictionary<string, float>)cooldownsField.GetValue(cp);

                    // --- Class matrix verification across Levels 1..5 ---
                    string[] classes = { "Mage", "Warrior", "Rogue", "Paladin" };
                    string[] slot1Attack = { "fireball", "melee", "melee", "melee" };
                    string[] slot2Attack = { "frostnova", "whirlwind", "eviscerate", "consecration" };
                    string[] slot3Heal = { "arcane_ward", "second_wind", "", "holy_light" };

                    for (int idx = 0; idx < classes.Length; idx++)
                    {
                        string cls = classes[idx];
                        string ab1 = slot1Attack[idx];
                        string ab2 = slot2Attack[idx];
                        string ab3 = slot3Heal[idx];

                        cp.ClassName.Value = new FixedString32Bytes(cls);
                        eStats.Restore();
                        cooldowns.Clear();
                        cp.ResourcePct.Value = 85;
                        cp.Health.Value = 40;
                        cp.MaxHealth.Value = 130;

                        // Level 1: Slot 2 locked (req 3), Slot 3 locked (req 5), Slot 1 allowed
                        cp.SetServerLevelForTest(1);
                        float hpBeforeL1S2 = eStats.Health;
                        byte resBefore = cp.ResourcePct.Value;
                        cp.ClearLastLockedRejectionForTest();
                        cp.RequestAbilityAttack(eStats, ab2);
                        bool l1s2Rej = Mathf.Approximately(eStats.Health, hpBeforeL1S2) &&
                                         cp.GetServerCooldownRemainingForTest($"{ab2}:{enemy.NetworkKey}") == 0f &&
                                         cp.GetServerCooldownRemainingForTest(ab2) == 0f &&
                                         cp.ResourcePct.Value == resBefore &&
                                         cp.LastLockedAbilityRejection == ab2 &&
                                         cp.LastLockedRequiredLevel == 3 &&
                                         cp.LastLockedServerLevel == 1;
                        Check($"{cls}_lv1_slot2_rejected_no_cd_or_resource_spend", l1s2Rej,
                            $"hp={eStats.Health}/{hpBeforeL1S2}, req={cp.LastLockedRequiredLevel}");

                        if (!string.IsNullOrEmpty(ab3))
                        {
                            int pHpBefore = cp.Health.Value;
                            float moveBefore = (float)moveTimeField.GetValue(cp);
                            cp.ClearLastLockedRejectionForTest();
                            cp.RequestAbilityHeal(40, ab3);
                            bool l1s3Rej = cp.Health.Value == pHpBefore &&
                                             cp.GetServerCooldownRemainingForTest("heal:" + ab3) == 0f &&
                                             Mathf.Approximately((float)moveTimeField.GetValue(cp), moveBefore) &&
                                             cp.ResourcePct.Value == resBefore &&
                                             cp.LastLockedAbilityRejection == ab3 &&
                                             cp.LastLockedRequiredLevel == 5 &&
                                             cp.LastLockedServerLevel == 1;
                            Check($"{cls}_lv1_slot3_rejected_no_cd_or_heal_or_blink", l1s3Rej,
                                $"hp={cp.Health.Value}/{pHpBefore}, req={cp.LastLockedRequiredLevel}");
                        }

                        float hpBeforeL1S1 = eStats.Health;
                        cp.RequestAbilityAttack(eStats, ab1);
                        Check($"{cls}_lv1_slot1_allowed", eStats.Health < hpBeforeL1S1 && cp.GetServerCooldownRemainingForTest(ab1) > 0f,
                            $"hp={eStats.Health}/{hpBeforeL1S1}");

                        // Level 2: Slot 2 STILL locked (no cooldown started)
                        cooldowns.Clear();
                        eStats.Restore();
                        cp.SetServerLevelForTest(2);
                        float hpBeforeL2S2 = eStats.Health;
                        cp.ClearLastLockedRejectionForTest();
                        cp.RequestAbilityAttack(eStats, ab2);
                        bool l2s2Rej = Mathf.Approximately(eStats.Health, hpBeforeL2S2) &&
                                         cp.GetServerCooldownRemainingForTest($"{ab2}:{enemy.NetworkKey}") == 0f &&
                                         cp.GetServerCooldownRemainingForTest(ab2) == 0f &&
                                         cp.LastLockedRequiredLevel == 3 &&
                                         cp.LastLockedServerLevel == 2;
                        Check($"{cls}_lv2_slot2_still_locked", l2s2Rej, $"hp={eStats.Health}/{hpBeforeL2S2}");

                        // Level 3: Slot 2 immediately allowed (proves Lv1/2 attempts did NOT start cooldown!)
                        cp.SetServerLevelForTest(3);
                        float hpBeforeL3S2 = eStats.Health;
                        cp.RequestAbilityAttack(eStats, ab2);
                        Check($"{cls}_lv3_slot2_unlocked_immediately", eStats.Health < hpBeforeL3S2,
                            $"hp={eStats.Health}/{hpBeforeL3S2}");

                        // Spoof local client XP to high level while Server Level stays 3/4 -> Slot 3 MUST stay rejected
                        if (!string.IsNullOrEmpty(ab3))
                        {
                            pc.AddExperience(1000); // Client local level now > 5
                            cp.SetServerLevelForTest(4);
                            int pHpL4 = cp.Health.Value;
                            cp.ClearLastLockedRejectionForTest();
                            cp.RequestAbilityHeal(40, ab3);
                            bool l4s3Rej = cp.Health.Value == pHpL4 &&
                                             cp.GetServerCooldownRemainingForTest("heal:" + ab3) == 0f &&
                                             cp.LastLockedRequiredLevel == 5 &&
                                             cp.LastLockedServerLevel == 4;
                            Check($"{cls}_lv4_client_tamper_ignored_slot3_rejected", l4s3Rej,
                                $"localLv={pc.Level}, srvLv={cp.Level.Value}, hp={cp.Health.Value}/{pHpL4}");

                            // Level 5: Slot 3 immediately heals (proves Lv1/4 rejections did NOT start 8s heal cooldown!)
                            cp.SetServerLevelForTest(5);
                            int pHpL5 = cp.Health.Value;
                            cp.RequestAbilityHeal(40, ab3);
                            Check($"{cls}_lv5_slot3_unlocked_immediately", cp.Health.Value > pHpL5 && cp.GetServerCooldownRemainingForTest("heal:" + ab3) > 0f,
                                $"hp={pHpL5}->{cp.Health.Value}");
                        }
                    }

                    Finish(failedCount > 0);
                }
            }
            catch (Exception ex)
            {
                Lines.Add($"FAIL [exception] {ex}");
                failedCount++;
                Finish(true);
            }
        }

        static void Finish(bool failed)
        {
            EditorApplication.update -= Tick;
            var nm = NetworkManager.Singleton;
            if (nm != null)
            {
                if (nm.IsListening) nm.Shutdown();
                UnityEngine.Object.DestroyImmediate(nm.gameObject);
            }

            Lines.Add($"TOTAL PASS={passed} FAIL={failedCount} RESULT={(failed ? "FAIL" : "PASS")}");
            Directory.CreateDirectory(Path.GetDirectoryName(PlayReportPath));
            File.WriteAllText(PlayReportPath, string.Join("\n", Lines) + "\n");
            Debug.Log($"[ServerAbilityUnlockChecks.PlayMode] PASS={passed} FAIL={failedCount}");

            SessionState.SetBool(FailedKey, failed);
            EditorApplication.isPlaying = false;
        }
    }
}
