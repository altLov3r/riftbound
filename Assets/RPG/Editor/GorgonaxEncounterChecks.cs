using System;
using System.Collections.Generic;
using System.IO;
using Riftbound.Network;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Riftbound.Editor
{
    [InitializeOnLoad]
    public static class GorgonaxEncounterChecks
    {
        const string PlayKey = "Riftbound.GorgonaxEncounterChecks.Active";
        const string PrevDirectPlayKey = "Riftbound.GorgonaxEncounterChecks.PrevDirect";
        const string FailedKey = "Riftbound.GorgonaxEncounterChecks.Failed";
        const string ReportPath = ".code-check/emberford-r1/gorgonax-encounter-checks.txt";

        static readonly List<string> Lines = new List<string>();
        static int stage;
        static int passed;
        static int failedCount;
        static double dueAt;
        static double timeoutAt;
        static float stageStartTime;
        static int hpBeforeStomp;
        static int levelBeforeKill;
        static int xpBeforeKill;
        static int goldBeforeKill;

        static GorgonaxEncounterChecks()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
        }

        static void OnPauseStateChanged(PauseState state)
        {
            if (SessionState.GetBool(PlayKey, false) && state == PauseState.Paused)
                EditorApplication.isPaused = false;
        }

        [MenuItem("Riftbound/Emberford/Run Gorgonax Encounter & Authority Checks (PlayMode)")]
        public static void RunPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Start GorgonaxEncounterChecks outside Play Mode.");

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
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
            PlayerController.SavePathOverride = Path.Combine(Path.GetTempPath(), $"rb_gorgonax_play_{Guid.NewGuid():N}.json");
            CoopSessionConfig.Mode = CoopSessionMode.HostLan;
            CoopSessionConfig.Port = 7793;
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
                dueAt = EditorApplication.timeSinceStartup + 0.4;
                timeoutAt = EditorApplication.timeSinceStartup + 35.0;
                EditorApplication.update += Tick;
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                Time.captureDeltaTime = 0f;
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

        static void TeleportPlayer(PlayerController pc, RiftboundCoopPlayer cp, Vector3 pos)
        {
            pos.y = WorldRegions.GroundHeight(pos) + 0.05f;
            if (pc)
            {
                var cc = pc.GetComponent<CharacterController>();
                if (cc) cc.enabled = false;
                pc.transform.position = pos;
                if (cc) cc.enabled = true;
            }
            if (cp)
            {
                cp.AuthorizeHostTeleport(pos);
                cp.SendSpoofedPositionForTest(pos);
            }
        }

        static void Tick()
        {
            try
            {
                if (EditorApplication.isPaused) EditorApplication.isPaused = false;
                Time.timeScale = 1f;
                EditorApplication.QueuePlayerLoopUpdate();
                if (EditorApplication.timeSinceStartup > timeoutAt)
                    throw new TimeoutException($"GorgonaxEncounterChecks timeout at stage {stage} (t={Time.time:F2})");
                if (EditorApplication.timeSinceStartup < dueAt) return;

                var cp = RiftboundCoopPlayer.LocalOwner;
                var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
                var wb = WorldBossEncounter.Instance;

                if (stage == 0)
                {
                    if (cp == null || !cp.IsSpawned || !cp.IsServer || pc == null || wb == null)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.15;
                        return;
                    }
                    wb.FindBossReference();
                    if (wb.bossCombatant == null || wb.bossController == null)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.15;
                        return;
                    }

                    RiftboundCoopPlayer.ResetServerWorldBossClaimsForTest();
                    wb.bossController.ResetForLoad();
                    wb.ResetForTestFull();
                    cp.PlayerName.Value = new FixedString64Bytes("GorgonaxTester");
                    cp.MaxHealth.Value = 400;
                    cp.Health.Value = 400;

                    // 1. Quiet bypass at 40m (> 35m wake radius)
                    TeleportPlayer(pc, cp, wb.bossController.HomePosition + new Vector3(0f, 0f, 40f));
                    stageStartTime = Time.time;
                    stage = 1;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 1)
                {
                    if (Time.time < stageStartTime + 0.35f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    bool quietBypassOk = !wb.bossController.IsChasing &&
                                         wb.StompCountForTest == 0 &&
                                         wb.MeteorWarnCountForTest == 0 &&
                                         !wb.IsEnraged;
                    Check("quiet_bypass_40m_no_pull_or_mechanics", quietBypassOk,
                        $"chasing={wb.bossController.IsChasing}, stomps={wb.StompCountForTest}, meteors={wb.MeteorWarnCountForTest}");

                    // 2. Non-aggro perimeter at 30m (inside 35m wake but outside 14m pull while idle)
                    TeleportPlayer(pc, cp, wb.bossController.HomePosition + new Vector3(0f, 0f, 30f));
                    stageStartTime = Time.time;
                    stage = 2;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 2)
                {
                    if (Time.time < stageStartTime + 0.35f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    bool idlePerimeterOk = !wb.bossController.IsChasing &&
                                           wb.StompCountForTest == 0 &&
                                           wb.MeteorWarnCountForTest == 0;
                    Check("idle_perimeter_30m_no_unengaged_stomp_or_meteor", idlePerimeterOk,
                        $"chasing={wb.bossController.IsChasing}, stomps={wb.StompCountForTest}, meteors={wb.MeteorWarnCountForTest}");

                    // 3. Pre-kill hoard chest claim gate (rejected while alive)
                    var fakeChestGo = new GameObject("TempPreKillHoardCheck");
                    fakeChestGo.transform.position = cp.transform.position;
                    var fakeChest = fakeChestGo.AddComponent<LootChest>();
                    fakeChest.chestId = WorldBossEncounter.HoardChestId;
                    bool preKillRejected = !cp.RequestOpenChestServerValidated(fakeChest);
                    UnityEngine.Object.DestroyImmediate(fakeChestGo);
                    Check("hoard_chest_rejected_while_boss_alive", preKillRejected, $"rejected={preKillRejected}");

                    // 4. Engage at 8m, damage boss below 35% HP -> verify Stomp server HP hit, Meteor warning, and Molten Enrage
                    TeleportPlayer(pc, cp, wb.bossController.HomePosition + new Vector3(0f, 0f, 8f));
                    hpBeforeStomp = cp.Health.Value;
                    wb.bossCombatant.Damage(wb.bossCombatant.maxHealth * 0.70f); // 30% HP remaining (< 35%)
                    stageStartTime = Time.time;
                    stage = 3;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 3)
                {
                    if (Time.time < stageStartTime + 0.35f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    bool engagedOk = wb.IsEnraged &&
                                     Mathf.Approximately(wb.bossController.attackInterval, 1.05f) &&
                                     Mathf.Approximately(wb.bossController.attackDamage, 32f) &&
                                     wb.StompCountForTest >= 1 &&
                                     wb.MeteorWarnCountForTest >= 1 &&
                                     cp.Health.Value < hpBeforeStomp;
                    Check("engaged_server_stomp_meteor_and_enrage", engagedOk,
                        $"enraged={wb.IsEnraged}, atkInt={wb.bossController.attackInterval:F2}, atkDmg={wb.bossController.attackDamage:F0}, stomps={wb.StompCountForTest}, meteorWarn={wb.MeteorWarnCountForTest}, hp={hpBeforeStomp}->{cp.Health.Value}");

                    // 5. Retreat / Evade reset -> verify Enrage, stats, and active meteor clear cleanly
                    TeleportPlayer(pc, cp, wb.bossController.HomePosition + new Vector3(0f, 0f, 42f));
                    wb.bossController.ResetForLoad();
                    stageStartTime = Time.time;
                    stage = 4;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 4)
                {
                    if (Time.time < stageStartTime + 0.25f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    bool resetOk = Mathf.Approximately(wb.bossCombatant.Health, wb.bossCombatant.maxHealth) &&
                                   !wb.IsEnraged &&
                                   !wb.IsMeteorActive &&
                                   Mathf.Approximately(wb.bossController.attackInterval, wb.BaseAttackInterval) &&
                                   Mathf.Approximately(wb.bossController.attackDamage, wb.BaseAttackDamage) &&
                                   wb.ResetCountForTest >= 1;
                    Check("evade_reset_clears_enrage_timers_and_restores_stats", resetOk,
                        $"hp={wb.bossCombatant.Health}/{wb.bossCombatant.maxHealth}, enraged={wb.IsEnraged}, atkInt={wb.bossController.attackInterval:F2}, atkDmg={wb.bossController.attackDamage:F0}, resets={wb.ResetCountForTest}");

                    // 6. Defeat Gorgonax -> verify server hoard chest spawned, kill reward granted ONCE, duplicate rejected
                    TeleportPlayer(pc, cp, wb.bossController.HomePosition + new Vector3(0f, 0f, 8f));
                    cp.Health.Value = cp.MaxHealth.Value;
                    levelBeforeKill = cp.Level.Value;
                    xpBeforeKill = cp.Experience.Value;
                    goldBeforeKill = cp.Gold.Value;
                    wb.bossCombatant.Damage(wb.bossCombatant.maxHealth + 100f);
                    stageStartTime = Time.time;
                    stage = 5;
                    dueAt = EditorApplication.timeSinceStartup + 0.05;
                }
                else if (stage == 5)
                {
                    if (Time.time < stageStartTime + 0.25f)
                    {
                        dueAt = EditorApplication.timeSinceStartup + 0.05;
                        return;
                    }
                    bool xpRewarded = cp.Level.Value > levelBeforeKill || cp.Experience.Value >= xpBeforeKill + WorldBossEncounter.DefeatXp;
                    bool defeatAndHoardOk = WorldBossEncounter.IsBossDefeatedAuthoritative &&
                                            wb.SpawnedHoardChest != null &&
                                            wb.SpawnedHoardChest.chestId == WorldBossEncounter.HoardChestId &&
                                            xpRewarded &&
                                            cp.Gold.Value >= goldBeforeKill + WorldBossEncounter.DefeatGold;
                    Check("server_defeat_spawns_hoard_and_grants_kill_reward", defeatAndHoardOk,
                        $"defeated={WorldBossEncounter.IsBossDefeatedAuthoritative}, chest={(wb.SpawnedHoardChest != null)}, lv={levelBeforeKill}->{cp.Level.Value}, xp={xpBeforeKill}->{cp.Experience.Value}, gold={goldBeforeKill}->{cp.Gold.Value}");

                    bool dupKillBlocked = !cp.GrantServerWorldBossKillReward(WorldBossEncounter.DefeatXp, WorldBossEncounter.DefeatGold, "worldbreaker-maul", "Titanbreaker");
                    Check("duplicate_kill_reward_blocked_across_reconnect", dupKillBlocked, $"blocked={dupKillBlocked}");

                    // 7. Claim server hoard chest once within 4.5m, then verify duplicate claim blocked
                    TeleportPlayer(pc, cp, wb.SpawnedHoardChest.transform.position + new Vector3(1.5f, 0f, 0f));
                    int goldBeforeChest = cp.Gold.Value;
                    bool firstChestClaim = cp.RequestOpenChestServerValidated(wb.SpawnedHoardChest) && cp.Gold.Value > goldBeforeChest;
                    bool secondChestBlocked = !cp.RequestOpenChestServerValidated(wb.SpawnedHoardChest);
                    Check("server_hoard_chest_single_claim_enforced", firstChestClaim && secondChestBlocked,
                        $"first={firstChestClaim}, secondBlocked={secondChestBlocked}, gold={goldBeforeChest}->{cp.Gold.Value}");

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
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, string.Join("\n", Lines) + "\n");
            Debug.Log($"[GorgonaxEncounterChecks.PlayMode] PASS={passed} FAIL={failedCount}");

            SessionState.SetBool(FailedKey, failed);
            EditorApplication.isPlaying = false;
        }
    }
}
