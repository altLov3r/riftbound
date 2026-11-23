using System;
using System.Collections.Generic;
using System.IO;
using Riftbound.Network;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Riftbound.Editor
{
    [InitializeOnLoad]
    public static class AshenmawCavernWalkChecks
    {
        const string PlayKey = "Riftbound.AshenmawWalkChecks.Active";
        const string PrevDirectKey = "Riftbound.AshenmawWalkChecks.PrevDirect";
        const string FailedKey = "Riftbound.AshenmawWalkChecks.Failed";
        const string ReportPath = ".code-check/emberford-r1/ashenmaw-walk-checks.txt";

        static readonly List<string> Lines = new List<string>();
        static int passed;
        static int failedCount;
        static double dueAt;
        static double timeoutAt;

        static AshenmawCavernWalkChecks()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
        }

        static void OnPauseStateChanged(PauseState state)
        {
            if (SessionState.GetBool(PlayKey, false) && state == PauseState.Paused)
                EditorApplication.isPaused = false;
        }

        [MenuItem("Riftbound/Emberford/Run Ashenmaw Walk-In Underground Cavern Physics Checks (PlayMode)")]
        public static void RunPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Start AshenmawCavernWalkChecks outside Play Mode.");

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            EditorPrefs.SetBool(PrevDirectKey, EditorPrefs.GetBool("Riftbound.DirectPlayInEditor", false));
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
            PlayerController.SavePathOverride = Path.Combine(Path.GetTempPath(), $"rb_ashenmaw_walk_{Guid.NewGuid():N}.json");
            CoopSessionConfig.ResetToOffline();
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
                passed = 0;
                failedCount = 0;
                dueAt = EditorApplication.timeSinceStartup + 0.5;
                timeoutAt = EditorApplication.timeSinceStartup + 25.0;
                EditorApplication.update += Tick;
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                Time.captureDeltaTime = 0f;
                bool failed = SessionState.GetBool(FailedKey, true);
                SessionState.SetBool(PlayKey, false);
                EditorPrefs.SetBool("Riftbound.DirectPlayInEditor", EditorPrefs.GetBool(PrevDirectKey, false));
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
                if (EditorApplication.isPaused) EditorApplication.isPaused = false;
                Time.timeScale = 1f;
                EditorApplication.QueuePlayerLoopUpdate();
                if (EditorApplication.timeSinceStartup > timeoutAt)
                    throw new TimeoutException("AshenmawCavernWalkChecks timed out waiting for cavern setup.");
                if (EditorApplication.timeSinceStartup < dueAt) return;

                AshenmawCavernZone.Ensure();
                var cavernRoot = GameObject.Find(AshenmawCavernZone.RootName);
                var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
                if (!cavernRoot || !pc)
                {
                    dueAt = EditorApplication.timeSinceStartup + 0.15;
                    return;
                }

                Physics.SyncTransforms();

                // Step every 1.0m from Warden Kaelen (Z=395) -> Mouth (Z=415) -> Terrain Hole (416..434) -> Subterranean Tunnel -> Ursolak Sanctum (Z=480)
                float startZ = AshenmawCavernZone.CampfirePos.z;
                float endZ = AshenmawCavernZone.SanctumPos.z + 5f;
                float x = AshenmawCavernZone.EntrancePos.x;
                float entranceGroundY = WorldRegions.GroundHeight(AshenmawCavernZone.EntrancePos);

                int floorHits = 0;
                int floorMisses = 0;
                int holeTerrainHits = 0;
                int deepFloorOk = 0;
                int ceilingHitsInside = 0;
                int undergroundInsideCount = 0;
                int capsuleBlocks = 0;
                float maxStepDelta = 0f;
                float maxStepZ = startZ;
                float prevY = float.NaN;
                string firstFloorMiss = "";
                string firstHoleTerrain = "";
                string firstBlock = "";

                for (float z = startZ; z <= endZ; z += 1.0f)
                {
                    float probeTop;
                    if (z < AshenmawCavernZone.EntrancePos.z - 1.5f)
                    {
                        probeTop = entranceGroundY + 8.0f;
                    }
                    else
                    {
                        float t = Mathf.Clamp01((z - AshenmawCavernZone.EntrancePos.z) / 29.0f);
                        float expectedFloor = Mathf.Lerp(entranceGroundY + 0.04f, entranceGroundY - 4.20f, t);
                        probeTop = expectedFloor + 2.6f;
                    }

                    if (RaycastStatic(new Vector3(x, probeTop, z), Vector3.down, 80f, out var downHit))
                    {
                        floorHits++;
                        float y = downHit.point.y;
                        if (z >= 416f && z <= 434f && downHit.collider is TerrainCollider)
                        {
                            holeTerrainHits++;
                            if (string.IsNullOrEmpty(firstHoleTerrain)) firstHoleTerrain = $"z={z:F0}";
                        }

                        if (!float.IsNaN(prevY))
                        {
                            float d = Mathf.Abs(y - prevY);
                            if (d > maxStepDelta)
                            {
                                maxStepDelta = d;
                                maxStepZ = z;
                            }
                        }
                        prevY = y;

                        // Capsule clearance at player body height (ignore runtime actors/chest)
                        var overlaps = Physics.OverlapCapsule(
                            new Vector3(x, y + 0.55f, z),
                            new Vector3(x, y + 1.65f, z),
                            0.34f,
                            ~0,
                            QueryTriggerInteraction.Ignore);
                        foreach (var col in overlaps)
                        {
                            if (!col || IsRuntimeActorCollider(col)) continue;
                            capsuleBlocks++;
                            if (string.IsNullOrEmpty(firstBlock)) firstBlock = $"z={z:F0} ({col.name})";
                            break;
                        }

                        // Deep subterranean cavern (Z=443..478, 36 samples): floor ~-4.20m, rock ceiling above, mountain terrain +4m overhead
                        if (z >= 443f && z <= 478f)
                        {
                            if (Mathf.Abs(y - (entranceGroundY - 4.20f)) <= 0.45f)
                                deepFloorOk++;
                            if (RaycastStatic(new Vector3(x, y + 1.2f, z), Vector3.up, 20f, out var upHit) &&
                                upHit.collider is MeshCollider &&
                                upHit.distance >= 3.5f && upHit.distance <= 13.0f)
                                ceilingHitsInside++;
                            float terrainOver = WorldRegions.GroundHeight(new Vector3(x, 0f, z));
                            if (terrainOver >= y + 4.0f)
                                undergroundInsideCount++;
                        }
                    }
                    else
                    {
                        floorMisses++;
                        if (string.IsNullOrEmpty(firstFloorMiss)) firstFloorMiss = $"z={z:F0}";
                    }
                }

                Check("continuous_walkable_floor_across_hole_and_tunnel",
                    floorMisses == 0 && floorHits >= 85 && maxStepDelta <= 0.55f && holeTerrainHits == 0,
                    $"hits={floorHits}, misses={floorMisses}, maxStepDelta={maxStepDelta:F3}m@z={maxStepZ:F0}, holeTerrainHits={holeTerrainHits}({firstHoleTerrain}), firstMiss={firstFloorMiss}");

                Check("unobstructed_player_capsule_through_cave",
                    capsuleBlocks == 0,
                    $"blocks={capsuleBlocks}, firstBlock={firstBlock}");

                Check("subterranean_tunnel_has_overhead_rock_ceiling_inside_mountain",
                    deepFloorOk >= 35 && ceilingHitsInside >= 35 && undergroundInsideCount >= 35,
                    $"deepFloorOk={deepFloorOk}, ceilingHits={ceilingHitsInside}, undergroundSamples={undergroundInsideCount}");

                Finish(failedCount > 0);
            }
            catch (Exception ex)
            {
                Lines.Add($"FAIL [exception] {ex}");
                failedCount++;
                Finish(true);
            }
        }

        static bool IsRuntimeActorCollider(Collider col)
        {
            return col.GetComponentInParent<PlayerController>() != null ||
                   col.GetComponentInParent<EnemyController>() != null ||
                   col.GetComponentInParent<TownNPC>() != null ||
                   col.GetComponentInParent<LootChest>() != null;
        }

        static bool RaycastStatic(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit best)
        {
            best = default;
            var hits = Physics.RaycastAll(origin, direction, maxDistance, ~0, QueryTriggerInteraction.Ignore);
            float bestDist = float.MaxValue;
            bool found = false;
            foreach (var h in hits)
            {
                if (!h.collider || IsRuntimeActorCollider(h.collider)) continue;
                if (h.distance < bestDist)
                {
                    bestDist = h.distance;
                    best = h;
                    found = true;
                }
            }
            return found;
        }

        static void Finish(bool failed)
        {
            EditorApplication.update -= Tick;
            Lines.Add($"TOTAL PASS={passed} FAIL={failedCount} RESULT={(failed ? "FAIL" : "PASS")}");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, string.Join("\n", Lines) + "\n");
            Debug.Log($"[AshenmawCavernWalkChecks.PlayMode] PASS={passed} FAIL={failedCount}");
            SessionState.SetBool(FailedKey, failed);
            EditorApplication.isPlaying = false;
        }
    }
}
