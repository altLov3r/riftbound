using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Riftbound.WorldAmbient;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    public static class PinewatchActivityChecks
    {
        const string ReportPath = ".code-check/emberford-r1/pinewatch-activity-checks.txt";

        [MenuItem("Riftbound/Emberford/Run Pinewatch Activity & Route Checks (EditMode)")]
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

            GameObject staged = null;
            try
            {
                var scene = EditorSceneManager.OpenScene(WorldRegions.ScenePath, OpenSceneMode.Single);
                var terrains = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>()).ToArray();
                var stageNotes = new StringBuilder();
                staged = PinewatchPreviewBuild.StagePinewatch(scene, stageNotes);
                Physics.SyncTransforms();

                // 1. Walkers count & valid routes
                var walkers = staged.GetComponentsInChildren<CivilianRouteWalker>(true);
                Check("pinewatch_two_civilian_walkers_present",
                    walkers.Length == 2 && walkers.All(w => w.HasValidRoute && w.ValidWaypointCount >= 4),
                    $"count={walkers.Length}, names={string.Join(",", walkers.Select(w => w.name))}");

                // 2. 8-sample-per-segment capsule clearance, ground support & mounted-bypass separation
                bool allRoutesClear = walkers.Length == 2;
                var routeDetails = new List<string>();
                foreach (var walker in walkers)
                {
                    var so = new SerializedObject(walker);
                    var wpProp = so.FindProperty("waypoints");
                    var pts = new List<Vector3>();
                    for (int i = 0; i < wpProp.arraySize; i++)
                    {
                        var tr = wpProp.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                        if (tr) pts.Add(tr.position);
                    }

                    int blockedSamples = 0;
                    float maxGroundDelta = 0f;
                    float maxX = float.MinValue;
                    float totalLen = 0f;

                    for (int i = 0; i < pts.Count; i++)
                    {
                        Vector3 a = pts[i];
                        Vector3 b = pts[(i + 1) % pts.Count];
                        totalLen += Vector3.Distance(a, b);
                        if (a.x > maxX) maxX = a.x;

                        for (int s = 0; s < 8; s++)
                        {
                            float t = s / 8f;
                            Vector3 p = Vector3.Lerp(a, b, t);
                            float g = WorldRegions.GroundHeight(p, terrains);
                            float delta = Mathf.Abs(p.y - g);
                            if (delta > maxGroundDelta) maxGroundDelta = delta;

                            var hits = Physics.OverlapCapsule(p + Vector3.up * 0.45f, p + Vector3.up * 1.55f, 0.30f);
                            foreach (var h in hits)
                            {
                                if (!h || h is TerrainCollider || h.isTrigger) continue;
                                if (h.transform.IsChildOf(walker.transform)) continue;
                                blockedSamples++;
                                break;
                            }
                        }
                    }

                    bool ok = blockedSamples == 0 && maxGroundDelta <= 0.16f && maxX <= -214.0f && totalLen >= 12f;
                    if (!ok) allRoutesClear = false;
                    routeDetails.Add($"{walker.name}(len={totalLen:F1}m, blocked={blockedSamples}, maxGroundDelta={maxGroundDelta:F3}m, maxX={maxX:F1})");
                }
                Check("pinewatch_walker_capsule_ground_and_lane_clearance", allRoutesClear, string.Join("; ", routeDetails));

                // 3. Three environmental clues + map pin waystop
                var waystops = staged.GetComponentsInChildren<SettlementWaystop>(true);
                bool hasClue1 = waystops.Any(w => w.gameObject.name == "Clue_SnappedHaulLine" && !string.IsNullOrWhiteSpace(w.notice));
                bool hasClue2 = waystops.Any(w => w.gameObject.name == "Clue_RepairBenchTally" && !string.IsNullOrWhiteSpace(w.notice));
                bool hasClue3 = waystops.Any(w => w.gameObject.name == "Clue_GreenhollowSightline" && !string.IsNullOrWhiteSpace(w.notice));
                bool hasPin = waystops.Any(w => w.displayName == "Pinewatch Relay" && !string.IsNullOrWhiteSpace(w.notice));
                Check("pinewatch_three_clues_and_waystop", hasClue1 && hasClue2 && hasClue3 && hasPin,
                    $"waystops={waystops.Length}, clue1={hasClue1}, clue2={hasClue2}, clue3={hasClue3}, pin={hasPin}");

                // 4. Watchkeeper Halen optional read-only NPC dialogue (zero quest/save mutation)
                var npcs = staged.GetComponentsInChildren<TownNPC>(true);
                var halen = npcs.FirstOrDefault(n => n.npcName == "Watchkeeper Halen");
                bool halenOk = halen != null &&
                               !string.IsNullOrWhiteSpace(halen.greeting) &&
                               halen.GetComponent<Collider>() != null;
                Check("pinewatch_watchkeeper_halen_optional_npc", halenOk,
                    $"found={(halen != null)}, role={(halen ? halen.roleTag : "")}");

                // 5. Mounted & foot bypass route still unobstructed with walkers & clues staged
                var player = Object.FindFirstObjectByType<PlayerController>()?.GetComponent<CharacterController>();
                var bypassReport = new StringBuilder();
                var route = new WorldTraversalVerification.Route("Pinewatch local bypass",
                    new Vector3(-212, 0, 389), new Vector3(-210, 0, 399), new Vector3(-212, 0, 410));
                foreach (bool reverse in new[] { false, true })
                foreach (bool mounted in new[] { false, true })
                    WorldTraversalVerification.Traverse(route, mounted, reverse, player, bypassReport);
                bool bypassOk = !bypassReport.ToString().Contains("BLOCKED");
                Check("pinewatch_mounted_and_foot_bypass_clear", bypassOk,
                    $"blocked={(!bypassOk)}");
            }
            catch (Exception ex)
            {
                Check("exception", false, ex.ToString());
            }
            finally
            {
                if (staged) Object.DestroyImmediate(staged);
            }

            lines.Add($"TOTAL PASS={passed} FAIL={failed} RESULT={(failed == 0 ? "PASS" : "FAIL")}");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, string.Join("\n", lines) + "\n");
            Debug.Log($"[PinewatchActivityChecks] PASS={passed} FAIL={failed}");
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
    }
}
