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

        static int castStarts;
        static int castCancels;
        static int castReleases;
        static ServerCastEvent lastCastEv;
        static float checkEnemyHpSnapshot;
        static int checkPlayerHpSnapshot;
        static int checkReleaseCountSnapshot;
        static Vector3 moveCheckStartPos;
        static int moveStepIndex;
        static float moveLastStepTime;
        static float moveAttemptedDist;
        static float castStartGameTime;
        static FieldInfo moveBudgetField;
        static FieldInfo blinkMetersField;
        static FieldInfo cooldownsField;
        static Dictionary<string, float> serverCooldowns;

        static ServerAbilityUnlockChecks()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
        }

        static void OnPauseStateChanged(PauseState state)
        {
            if (SessionState.GetBool(PlayKey, false) && state == PauseState.Paused)
            {
                Debug.Log($"[ServerAbilityUnlockChecks] Unexpected PauseState.Paused at stage={stage}, t={Time.time:F3}; unpausing.");
                EditorApplication.isPaused = false;
            }
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

            // Server-timed cast configuration
            bool castConfigOk = RiftboundCoopPlayer.IsCastTimedAbility("fireball") &&
                                RiftboundCoopPlayer.IsCastTimedAbility("holy_light") &&
                                !RiftboundCoopPlayer.IsCastTimedAbility("melee") &&
                                !RiftboundCoopPlayer.IsCastTimedAbility("sprint") &&
                                !RiftboundCoopPlayer.IsCastTimedAbility("arcane_ward") &&
                                Mathf.Approximately(RiftboundCoopPlayer.GetServerCastDuration("fireball"), RiftboundCoopPlayer.FireballCastDuration) &&
                                Mathf.Approximately(RiftboundCoopPlayer.GetServerCastDuration("holy_light"), RiftboundCoopPlayer.HolyLightCastDuration) &&
                                RiftboundCoopPlayer.FireballCastDuration >= 1.0f &&
                                RiftboundCoopPlayer.HolyLightCastDuration >= 1.0f &&
                                RiftboundCoopPlayer.CastMoveInterruptThreshold > 0f &&
                                RiftboundCoopPlayer.CastMoveInterruptThreshold <= 0.5f;
            Check("cast_timing_constants", castConfigOk,
                $"fb={RiftboundCoopPlayer.FireballCastDuration:F2}, hl={RiftboundCoopPlayer.HolyLightCastDuration:F2}, moveThresh={RiftboundCoopPlayer.CastMoveInterruptThreshold:F2}");

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
                Application.runInBackground = true;
                Time.timeScale = 1f;
                Time.captureDeltaTime = 1f / 30f;
                EditorApplication.isPaused = false;
                Lines.Clear();
                stage = 0;
                passed = 0;
                failedCount = 0;
                castStarts = 0;
                castCancels = 0;
                castReleases = 0;
                lastCastEv = default;
                RiftboundCoopPlayer.OnServerCastEvent += OnServerCastEventObserved;
                dueAt = EditorApplication.timeSinceStartup + 0.4;
                timeoutAt = EditorApplication.timeSinceStartup + 35.0;
                EditorApplication.update += Tick;
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                Time.captureDeltaTime = 0f;
                RiftboundCoopPlayer.OnServerCastEvent -= OnServerCastEventObserved;
                bool failed = SessionState.GetBool(FailedKey, true);
                SessionState.SetBool(PlayKey, false);
                bool prevDirect = EditorPrefs.GetBool(PrevDirectPlayKey, false);
                EditorPrefs.SetBool("Riftbound.DirectPlayInEditor", prevDirect);
                CoopSessionConfig.ResetToOffline();
                if (Application.isBatchMode) EditorApplication.Exit(failed ? 1 : 0);
            }
        }

        static void OnServerCastEventObserved(ServerCastEvent ev)
        {
            lastCastEv = ev;
            if (ev.Phase == ServerCastPhase.Started) castStarts++;
            else if (ev.Phase == ServerCastPhase.Canceled) castCancels++;
            else if (ev.Phase == ServerCastPhase.Released) castReleases++;
        }

        static void MoveLocalControllerForTest(PlayerController pc, RiftboundCoopPlayer cp, Vector3 nextPos)
        {
            if (pc)
            {
                var cc = pc.GetComponent<CharacterController>();
                if (cc) cc.enabled = false;
                pc.transform.position = nextPos;
                if (cc) cc.enabled = true;
            }
            if (cp) cp.SendSpoofedPositionForTest(nextPos);
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
                if (EditorApplication.isPaused) EditorApplication.isPaused = false;
                Time.timeScale = 1f;
                EditorApplication.QueuePlayerLoopUpdate();
                if (EditorApplication.timeSinceStartup > timeoutAt)
                    throw new TimeoutException($"ServerAbilityUnlockChecks timeout at stage {stage} (t={Time.time:F3}, castStart={castStartGameTime:F3}, frame={Time.frameCount}, ts={Time.timeScale:F2}, paused={EditorApplication.isPaused}, srvCast={RiftboundCoopPlayer.LocalOwner?.HasActiveServerCast}, rel={castReleases}, cancel={castCancels})");
                if (EditorApplication.timeSinceStartup < dueAt) return;

                var cp = RiftboundCoopPlayer.LocalOwner;
                var enemy = EnemyController.FindByKey("road-raider-1");
                var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

                if (stage == 0)
                {
                    if (cp == null || !cp.IsSpawned || !cp.IsServer || enemy == null || pc == null)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.15;
                        return;
                    }

                    // Position cleanly next to road-raider-1 with clear LOS and damage player slightly so heal checks are testable
                    Vector3 anchor = enemy.transform.position + new Vector3(2.2f, 0f, 0f);
                    MoveLocalControllerForTest(pc, null, anchor);
                    cp.AuthorizeHostTeleport(anchor);
                    cp.ResourcePct.Value = 85;
                    cp.Health.Value = 40;
                    cp.MaxHealth.Value = 130;

                    moveBudgetField = typeof(RiftboundCoopPlayer).GetField("serverMoveBudget", BindingFlags.NonPublic | BindingFlags.Instance);
                    blinkMetersField = typeof(RiftboundCoopPlayer).GetField("serverBlinkAllowanceMeters", BindingFlags.NonPublic | BindingFlags.Instance);
                    cooldownsField = typeof(RiftboundCoopPlayer).GetField("serverAbilityCooldowns", BindingFlags.NonPublic | BindingFlags.Instance);
                    serverCooldowns = (Dictionary<string, float>)cooldownsField.GetValue(cp);

                    stage = 1;
                    dueAt = EditorApplication.timeSinceStartup + 0.25;
                }
                else if (stage == 1)
                {
                    var eStats = enemy.GetComponent<Combatant>();
                    cp.SendSpoofedPositionForTest(pc.transform.position); // clear any pending hostTeleportAuthorized flag

                    // --- Class instant unlock matrix verification across Levels 1..5 ---
                    string[] classes = { "Mage", "Warrior", "Rogue", "Paladin" };
                    string[] slot1Attack = { "melee", "melee", "melee", "melee" };
                    string[] slot2Attack = { "frostnova", "whirlwind", "eviscerate", "consecration" };
                    string[] slot3InstantHeal = { "arcane_ward", "second_wind", "", "" };

                    for (int idx = 0; idx < classes.Length; idx++)
                    {
                        string cls = classes[idx];
                        string ab1 = slot1Attack[idx];
                        string ab2 = slot2Attack[idx];
                        string ab3 = slot3InstantHeal[idx];

                        cp.ClassName.Value = new FixedString32Bytes(cls);
                        eStats.Restore();
                        serverCooldowns.Clear();
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
                            float blinkBefore = (float)blinkMetersField.GetValue(cp);
                            cp.ClearLastLockedRejectionForTest();
                            cp.RequestAbilityHeal(40, ab3);
                            bool l1s3Rej = cp.Health.Value == pHpBefore &&
                                             cp.GetServerCooldownRemainingForTest("heal:" + ab3) == 0f &&
                                             Mathf.Approximately((float)blinkMetersField.GetValue(cp), blinkBefore) &&
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

                        // Level 2: Slot 2 STILL locked
                        serverCooldowns.Clear();
                        eStats.Restore();
                        cp.SetServerLevelForTest(2);
                        float hpBeforeL2S2 = eStats.Health;
                        cp.ClearLastLockedRejectionForTest();
                        cp.RequestAbilityAttack(eStats, ab2);
                        bool l2s2Rej = Mathf.Approximately(eStats.Health, hpBeforeL2S2) &&
                                         cp.GetServerCooldownRemainingForTest($"{ab2}:{enemy.NetworkKey}") == 0f &&
                                         cp.LastLockedRequiredLevel == 3 &&
                                         cp.LastLockedServerLevel == 2;
                        Check($"{cls}_lv2_slot2_still_locked", l2s2Rej, $"hp={eStats.Health}/{hpBeforeL2S2}");

                        // Level 3: Slot 2 immediately allowed
                        cp.SetServerLevelForTest(3);
                        float hpBeforeL3S2 = eStats.Health;
                        cp.RequestAbilityAttack(eStats, ab2);
                        Check($"{cls}_lv3_slot2_unlocked_immediately", eStats.Health < hpBeforeL3S2,
                            $"hp={eStats.Health}/{hpBeforeL3S2}");

                        // Spoof local client XP to high level while Server Level stays 4 -> Slot 3 stays rejected
                        if (!string.IsNullOrEmpty(ab3))
                        {
                            pc.AddExperience(1000);
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

                            // Level 5: Slot 3 immediately allowed
                            cp.SetServerLevelForTest(5);
                            int pHpL5 = cp.Health.Value;
                            cp.RequestAbilityHeal(40, ab3);
                            Check($"{cls}_lv5_slot3_unlocked_immediately", cp.Health.Value > pHpL5 && cp.GetServerCooldownRemainingForTest("heal:" + ab3) > 0f,
                                $"hp={pHpL5}->{cp.Health.Value}");
                        }
                    }

                    // Reset cooldowns/blink state and test Arcane Ward movement unlock (Lv 4 vs Lv 5 even at full HP)
                    serverCooldowns.Clear();
                    blinkMetersField.SetValue(cp, 0f);
                    cp.ClassName.Value = new FixedString32Bytes("Mage");
                    cp.Health.Value = cp.MaxHealth.Value;
                    cp.SetServerLevelForTest(4);
                    moveBudgetField.SetValue(cp, 1.0f);
                    moveCheckStartPos = cp.transform.position;

                    cp.ClearLastLockedRejectionForTest();
                    cp.RequestAbilityHeal(40, "arcane_ward");
                    bool awLockedRej = cp.LastLockedAbilityRejection == "arcane_ward" &&
                                       cp.LastLockedRequiredLevel == 5 &&
                                       Mathf.Approximately((float)blinkMetersField.GetValue(cp), 0f) &&
                                       cp.GetServerCooldownRemainingForTest("heal:arcane_ward") == 0f;

                    MoveLocalControllerForTest(pc, cp, moveCheckStartPos + Vector3.forward * 7.5f);
                    float blinkLockedDist = Vector3.Distance(cp.transform.position, moveCheckStartPos);
                    Check("arcane_ward_lv4_locked_blink_clamped", awLockedRej && blinkLockedDist < 3.5f,
                        $"rej={awLockedRej}, dist={blinkLockedDist:F2}");

                    // Re-anchor and test Lv 5 Arcane Ward one-time Blink allowance + clamp of second un-casted jump
                    MoveLocalControllerForTest(pc, null, moveCheckStartPos);
                    cp.AuthorizeHostTeleport(moveCheckStartPos);
                    stage = 2;
                    dueAt = EditorApplication.timeSinceStartup + 0.15;
                }
                else if (stage == 2)
                {
                    cp.SendSpoofedPositionForTest(pc.transform.position);
                    moveBudgetField.SetValue(cp, 1.0f);
                    cp.SetServerLevelForTest(5);
                    cp.Health.Value = cp.MaxHealth.Value; // verify Blink works even at full HP once unlocked
                    moveCheckStartPos = cp.transform.position;

                    cp.RequestAbilityHeal(40, "arcane_ward");
                    float blinkCredit = (float)blinkMetersField.GetValue(cp);
                    MoveLocalControllerForTest(pc, cp, moveCheckStartPos + Vector3.forward * 7.5f);
                    float blinkAllowedDist = Vector3.Distance(cp.transform.position, moveCheckStartPos);

                    // Second un-casted 7.5m Blink jump right away must be clamped
                    Vector3 postBlinkPos = cp.transform.position;
                    MoveLocalControllerForTest(pc, cp, postBlinkPos + Vector3.forward * 7.5f);
                    float secondJumpDist = Vector3.Distance(cp.transform.position, postBlinkPos);

                    Check("arcane_ward_lv5_unlocked_one_blink_accepted_second_clamped",
                        blinkCredit >= 8.0f &&
                        cp.GetServerCooldownRemainingForTest("heal:arcane_ward") > 0f &&
                        blinkAllowedDist >= 7.0f &&
                        secondJumpDist < 3.5f,
                        $"credit={blinkCredit:F1}, firstDist={blinkAllowedDist:F2}, secondDist={secondJumpDist:F2}");

                    // Prepare Sprint movement unlock tests (Rogue Lv 4 locked vs Lv 5 unlocked)
                    serverCooldowns.Clear();
                    cp.ClassName.Value = new FixedString32Bytes("Rogue");
                    cp.SetServerLevelForTest(4);
                    cp.ClearLastLockedRejectionForTest();
                    cp.RequestSprint();
                    bool sprintLv4Rejected = cp.LastLockedAbilityRejection == "sprint" &&
                                             cp.LastLockedRequiredLevel == 5 &&
                                             !cp.IsSprinting.Value &&
                                             !cp.IsServerSprinting &&
                                             cp.GetServerCooldownRemainingForTest("move:sprint") == 0f;
                    Check("sprint_lv4_locked_rejected_no_cd_or_buff", sprintLv4Rejected,
                        $"rej={cp.LastLockedAbilityRejection}, isSprinting={cp.IsSprinting.Value}");

                    MoveLocalControllerForTest(pc, null, moveCheckStartPos);
                    cp.AuthorizeHostTeleport(moveCheckStartPos);
                    moveStepIndex = 0;
                    stage = 3;
                    dueAt = EditorApplication.timeSinceStartup + 0.15;
                }
                else if (stage == 3)
                {
                    // Send sustained 13.0 m/s steps (0.10s * 8) as Lv 4 Rogue without Sprint
                    if (moveStepIndex == 0)
                    {
                        cp.SendSpoofedPositionForTest(pc.transform.position);
                        moveBudgetField.SetValue(cp, 1.0f);
                        moveCheckStartPos = cp.transform.position;
                        moveLastStepTime = Time.time;
                        moveAttemptedDist = 0f;
                    }
                    else
                    {
                        float dt = Mathf.Max(0.08f, Time.time - moveLastStepTime);
                        moveLastStepTime = Time.time;
                        float stepDist = 13.0f * dt;
                        moveAttemptedDist += stepDist;
                        MoveLocalControllerForTest(pc, cp, cp.transform.position + Vector3.forward * stepDist);
                    }

                    moveStepIndex++;
                    if (moveStepIndex <= 8)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.10;
                        return;
                    }

                    float movedDist = Vector3.Distance(cp.transform.position, moveCheckStartPos);
                    Check("sprint_lv4_overspeed_clamped", movedDist <= moveAttemptedDist - 1.25f && cp.MoveSpeed.Value <= 9.5f,
                        $"moved={movedDist:F2}m (attempted={moveAttemptedDist:F2}m), maxRepSpd={cp.MoveSpeed.Value:F2}");

                    // Re-anchor for Lv 5 Rogue Sprint allowed check
                    MoveLocalControllerForTest(pc, null, moveCheckStartPos);
                    cp.AuthorizeHostTeleport(moveCheckStartPos);
                    cp.SetServerLevelForTest(5);
                    serverCooldowns.Clear();
                    moveStepIndex = 0;
                    stage = 4;
                    dueAt = EditorApplication.timeSinceStartup + 0.15;
                }
                else if (stage == 4)
                {
                    if (moveStepIndex == 0)
                    {
                        cp.SendSpoofedPositionForTest(pc.transform.position);
                        moveBudgetField.SetValue(cp, 1.0f);
                        cp.RequestSprint();
                        bool sprintLv5Ok = cp.IsSprinting.Value && cp.IsServerSprinting && cp.GetServerCooldownRemainingForTest("move:sprint") > 0f;
                        Check("sprint_lv5_unlocked_buff_and_cd_active", sprintLv5Ok,
                            $"sprinting={cp.IsSprinting.Value}, cd={cp.GetServerCooldownRemainingForTest("move:sprint"):F2}");
                        moveCheckStartPos = cp.transform.position;
                        moveLastStepTime = Time.time;
                        moveAttemptedDist = 0f;
                    }
                    else
                    {
                        float dt = Mathf.Clamp(Time.time - moveLastStepTime, 0.08f, 0.14f);
                        moveLastStepTime = Time.time;
                        float stepDist = 11.5f * dt;
                        moveAttemptedDist += stepDist;
                        MoveLocalControllerForTest(pc, cp, cp.transform.position + Vector3.forward * stepDist);
                    }

                    moveStepIndex++;
                    if (moveStepIndex <= 7)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.10;
                        return;
                    }

                    float sprintDist = Vector3.Distance(cp.transform.position, moveCheckStartPos);
                    Check("sprint_lv5_overspeed_accepted", sprintDist >= moveAttemptedDist - 0.30f && cp.MoveSpeed.Value > 9.5f,
                        $"sprintMoved={sprintDist:F2}m (attempted={moveAttemptedDist:F2}m), repSpd={cp.MoveSpeed.Value:F2}");

                    // Prepare Server-Timed Fireball tests next to road-raider-1
                    Vector3 combatAnchor = enemy.transform.position + new Vector3(2.2f, 0f, 0f);
                    MoveLocalControllerForTest(pc, null, combatAnchor);
                    cp.AuthorizeHostTeleport(combatAnchor);
                    cp.ClassName.Value = new FixedString32Bytes("Mage");
                    cp.SetServerLevelForTest(1);
                    serverCooldowns.Clear();
                    var eStats = enemy.GetComponent<Combatant>();
                    eStats.Restore();
                    cp.Health.Value = cp.MaxHealth.Value;
                    stage = 5;
                    dueAt = EditorApplication.timeSinceStartup + 0.20;
                }
                else if (stage == 5)
                {
                    // Start Fireball cast + attempt duplicate start/instant bypass mid-cast
                    var eStats = enemy.GetComponent<Combatant>();
                    cp.SendSpoofedPositionForTest(pc.transform.position);
                    castStarts = 0; castCancels = 0; castReleases = 0;
                    checkEnemyHpSnapshot = eStats.Health;
                    checkReleaseCountSnapshot = castReleases;

                    cp.RequestStartCast("fireball", eStats);
                    castStartGameTime = Time.time;
                    bool startedImmediately = castStarts == 1 &&
                                              lastCastEv.Phase == ServerCastPhase.Started &&
                                              lastCastEv.AbilityId == "fireball" &&
                                              Mathf.Approximately(lastCastEv.Duration, RiftboundCoopPlayer.FireballCastDuration) &&
                                              cp.HasActiveClientCast &&
                                              cp.HasActiveServerCast &&
                                              Mathf.Approximately(eStats.Health, checkEnemyHpSnapshot) &&
                                              cp.GetServerCooldownRemainingForTest("fireball") == 0f;
                    Check("fireball_server_cast_started_not_instant", startedImmediately,
                        $"starts={castStarts}, dur={lastCastEv.Duration:F2}, hp={eStats.Health}/{checkEnemyHpSnapshot}");

                    stage = 6;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 6)
                {
                    if (Time.time < castStartGameTime + 0.45f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    // Mid-cast check (0.45s < 1.20s) + anti-spam / bypass check
                    var eStats = enemy.GetComponent<Combatant>();
                    cp.RequestAbilityAttack(eStats, "fireball");
                    cp.RequestStartCast("fireball", eStats);
                    bool midHoldOk = castStarts == 1 &&
                                     castReleases == 0 &&
                                     cp.HasActiveServerCast &&
                                     Mathf.Approximately(eStats.Health, checkEnemyHpSnapshot) &&
                                     cp.GetServerCooldownRemainingForTest("fireball") == 0f;
                    Check("fireball_midcast_holds_and_ignores_duplicate_start", midHoldOk,
                        $"starts={castStarts}, releases={castReleases}, hp={eStats.Health}/{checkEnemyHpSnapshot}");

                    stage = 7;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 7)
                {
                    if (Time.time < castStartGameTime + RiftboundCoopPlayer.FireballCastDuration + 0.08f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    // Completion + One-time release check
                    var eStats = enemy.GetComponent<Combatant>();
                    bool releaseOnceOk = castStarts == 1 &&
                                         castCancels == 0 &&
                                         castReleases == 1 &&
                                         lastCastEv.Phase == ServerCastPhase.Released &&
                                         lastCastEv.AbilityId == "fireball" &&
                                         !cp.HasActiveClientCast &&
                                         !cp.HasActiveServerCast &&
                                         eStats.Health < checkEnemyHpSnapshot &&
                                         cp.GetServerCooldownRemainingForTest("fireball") > 0f;
                    Check("fireball_server_timed_one_time_release", releaseOnceOk,
                        $"releases={castReleases}, hp={eStats.Health}/{checkEnemyHpSnapshot}, cd={cp.GetServerCooldownRemainingForTest("fireball"):F2}");

                    // Start Fireball -> Move Interrupt check
                    serverCooldowns.Clear();
                    eStats.Restore();
                    checkEnemyHpSnapshot = eStats.Health;
                    cp.RequestStartCast("fireball", eStats);
                    castStartGameTime = Time.time;
                    MoveLocalControllerForTest(pc, cp, pc.transform.position + Vector3.forward * 1.1f);
                    bool moveCancelled = castCancels == 1 &&
                                         lastCastEv.Phase == ServerCastPhase.Canceled &&
                                         lastCastEv.CancelReason == ServerCastCancelReason.Moved &&
                                         !cp.HasActiveClientCast &&
                                         !cp.HasActiveServerCast;
                    Check("fireball_interrupted_on_move", moveCancelled,
                        $"cancels={castCancels}, reason={lastCastEv.CancelReason}");

                    stage = 8;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 8)
                {
                    if (Time.time < castStartGameTime + RiftboundCoopPlayer.FireballCastDuration + 0.08f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    // Confirm interrupted move cast NEVER releases after full duration
                    var eStats = enemy.GetComponent<Combatant>();
                    bool noPostMoveRelease = castReleases == 1 &&
                                             castCancels == 1 &&
                                             !cp.HasActiveServerCast &&
                                             !cp.HasActiveClientCast &&
                                             cp.GetServerCooldownRemainingForTest("fireball") == 0f;
                    Check("fireball_moved_cancel_never_releases_or_starts_cd", noPostMoveRelease,
                        $"releases={castReleases}, cancels={castCancels}, cd={cp.GetServerCooldownRemainingForTest("fireball"):F2}");

                    // Re-anchor, start Fireball -> Damage Interrupt check
                    Vector3 combatAnchor = enemy.transform.position + new Vector3(2.2f, 0f, 0f);
                    MoveLocalControllerForTest(pc, null, combatAnchor);
                    cp.AuthorizeHostTeleport(combatAnchor);
                    stage = 9;
                    dueAt = EditorApplication.timeSinceStartup + 0.15;
                }
                else if (stage == 9)
                {
                    var eStats = enemy.GetComponent<Combatant>();
                    cp.SendSpoofedPositionForTest(pc.transform.position);
                    cp.RequestStartCast("fireball", eStats);
                    RiftboundCoopPlayer.DispatchEnemyHitToPlayer(cp.OwnerClientId, 12f, "road-raider-1");
                    bool dmgCancelled = castCancels == 2 &&
                                        lastCastEv.Phase == ServerCastPhase.Canceled &&
                                        lastCastEv.CancelReason == ServerCastCancelReason.Damaged &&
                                        !cp.HasActiveServerCast;
                    Check("fireball_interrupted_on_damage", dmgCancelled,
                        $"cancels={castCancels}, reason={lastCastEv.CancelReason}");

                    // Manual cancel check
                    cp.RequestStartCast("fireball", eStats);
                    cp.RequestCancelCast();
                    bool manualCancelled = castCancels == 3 &&
                                           lastCastEv.Phase == ServerCastPhase.Canceled &&
                                           lastCastEv.CancelReason == ServerCastCancelReason.ManualCancel &&
                                           !cp.HasActiveServerCast &&
                                           cp.GetServerCooldownRemainingForTest("fireball") == 0f;
                    Check("fireball_interrupted_on_manual_cancel", manualCancelled,
                        $"cancels={castCancels}, reason={lastCastEv.CancelReason}");

                    // Re-anchor safely outside combat before Paladin Holy Light unlock/timing/interrupt tests
                    Vector3 safeAnchorPre = enemy.transform.position + new Vector3(22f, 0f, 0f);
                    MoveLocalControllerForTest(pc, null, safeAnchorPre);
                    cp.AuthorizeHostTeleport(safeAnchorPre);
                    stage = 10;
                    dueAt = EditorApplication.timeSinceStartup + 0.15;
                }
                else if (stage == 10)
                {
                    cp.SendSpoofedPositionForTest(pc.transform.position);
                    cp.ClassName.Value = new FixedString32Bytes("Paladin");
                    cp.Health.Value = 35;
                    cp.MaxHealth.Value = 130;
                    serverCooldowns.Clear();
                    checkPlayerHpSnapshot = cp.Health.Value;
                    checkReleaseCountSnapshot = castReleases;

                    cp.SetServerLevelForTest(4);
                    cp.ClearLastLockedRejectionForTest();
                    int startsBeforeLockedHl = castStarts;
                    cp.RequestStartCast("holy_light");
                    bool hlLockedRej = castStarts == startsBeforeLockedHl &&
                                       cp.LastLockedAbilityRejection == "holy_light" &&
                                       cp.LastLockedRequiredLevel == 5 &&
                                       cp.Health.Value == checkPlayerHpSnapshot;
                    Check("holy_light_lv4_locked_rejected_no_cast_or_heal", hlLockedRej,
                        $"req={cp.LastLockedRequiredLevel}, hp={cp.Health.Value}");

                    cp.SetServerLevelForTest(5);
                    cp.RequestStartCast("holy_light");
                    castStartGameTime = Time.time;
                    bool hlStarted = castStarts == startsBeforeLockedHl + 1 &&
                                     lastCastEv.Phase == ServerCastPhase.Started &&
                                     lastCastEv.AbilityId == "holy_light" &&
                                     lastCastEv.ServerStartTime > 0.0 &&
                                     Mathf.Approximately(lastCastEv.Duration, RiftboundCoopPlayer.HolyLightCastDuration) &&
                                     cp.Health.Value == checkPlayerHpSnapshot;
                    Check("holy_light_lv5_server_cast_started", hlStarted,
                        $"dur={lastCastEv.Duration:F2}, hp={cp.Health.Value}");

                    stage = 11;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 11)
                {
                    if (Time.time < castStartGameTime + 0.55f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    // Mid-cast hold + movement interrupt check on Holy Light
                    bool hlMidHold = castReleases == checkReleaseCountSnapshot &&
                                     cp.Health.Value == checkPlayerHpSnapshot &&
                                     cp.GetServerCooldownRemainingForTest("heal:holy_light") == 0f &&
                                     cp.HasActiveServerCast;
                    Check("holy_light_midcast_holds_before_duration", hlMidHold,
                        $"hp={cp.Health.Value}/{checkPlayerHpSnapshot}, rel={castReleases}");

                    MoveLocalControllerForTest(pc, cp, pc.transform.position + Vector3.right * 1.1f);
                    bool hlMoveCancelled = lastCastEv.Phase == ServerCastPhase.Canceled &&
                                           lastCastEv.CancelReason == ServerCastCancelReason.Moved &&
                                           !cp.HasActiveServerCast &&
                                           cp.GetServerCooldownRemainingForTest("heal:holy_light") == 0f;
                    Check("holy_light_interrupted_on_move", hlMoveCancelled,
                        $"reason={lastCastEv.CancelReason}, hp={cp.Health.Value}");

                    // Re-anchor safely outside melee aggro for uninterrupted Holy Light release test
                    Vector3 safeAnchor = enemy.transform.position + new Vector3(22f, 0f, 0f);
                    MoveLocalControllerForTest(pc, null, safeAnchor);
                    cp.AuthorizeHostTeleport(safeAnchor);
                    stage = 12;
                    dueAt = EditorApplication.timeSinceStartup + 0.15;
                }
                else if (stage == 12)
                {
                    cp.SendSpoofedPositionForTest(pc.transform.position);
                    cp.MaxHealth.Value = 130;
                    cp.Health.Value = 35;
                    checkPlayerHpSnapshot = cp.Health.Value;
                    checkReleaseCountSnapshot = castReleases;
                    cp.RequestAbilityHeal(60, "holy_light"); // Also verifies RequestAbilityHeal routes holy_light through server timer
                    castStartGameTime = Time.time;
                    stage = 13;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 13)
                {
                    if (Time.time < castStartGameTime + RiftboundCoopPlayer.HolyLightCastDuration + 0.08f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    bool hlReleasedOnce = castReleases == checkReleaseCountSnapshot + 1 &&
                                          lastCastEv.Phase == ServerCastPhase.Released &&
                                          lastCastEv.AbilityId == "holy_light" &&
                                          !cp.HasActiveClientCast &&
                                          !cp.HasActiveServerCast &&
                                          cp.Health.Value == Mathf.Min(cp.MaxHealth.Value, checkPlayerHpSnapshot + 60) &&
                                          cp.GetServerCooldownRemainingForTest("heal:holy_light") > 0f;
                    Check("holy_light_server_timed_one_time_release", hlReleasedOnce,
                        $"releases={castReleases}, hp={checkPlayerHpSnapshot}->{cp.Health.Value}, cd={cp.GetServerCooldownRemainingForTest("heal:holy_light"):F2}");

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
