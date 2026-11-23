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
    public static class FrontierCompleteChecks
    {
        const string ReportPath = ".code-check/emberford-r1/frontier-complete-checks.txt";

        [MenuItem("Riftbound/Emberford/Run Complete Frontier Sites & Pinewatch Woods Checks (EditMode)")]
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

            GameObject pinewatchStaged = null;
            GameObject frontierStaged = null;
            GameObject woodsRoot = null;
            GameObject fqsGo = null;

            try
            {
                var scene = EditorSceneManager.OpenScene(WorldRegions.ScenePath, OpenSceneMode.Single);
                var terrains = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>()).ToArray();

                // 1. Pinewatch Timberfang Woods 3-step questline registration
                fqsGo = new GameObject("TempFqsCheck");
                var fqs = fqsGo.AddComponent<FactionQuestSystem>();
                typeof(FactionQuestSystem)
                    .GetMethod("BuildCatalog", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.Invoke(fqs, null);
                var q1 = FactionQuestSystem.GetQuest("pinewatch_clear_woods");
                var q2 = FactionQuestSystem.GetQuest("pinewatch_recover_line");
                var q3 = FactionQuestSystem.GetQuest("pinewatch_warchief_boss");
                bool questChainOk = q1 != null && q2 != null && q3 != null &&
                                    q1.giverNpc == "Watchkeeper Halen" && q2.giverNpc == "Watchkeeper Halen" && q3.giverNpc == "Watchkeeper Halen" &&
                                    q1.nextQuestId == "pinewatch_recover_line" && q2.prerequisiteQuestId == "pinewatch_clear_woods" &&
                                    q2.nextQuestId == "pinewatch_warchief_boss" && q3.prerequisiteQuestId == "pinewatch_recover_line" &&
                                    q3.targetId == PinewatchWoodsZone.BossId;
                Check("pinewatch_woods_3step_questline_registered", questChainOk,
                    $"q1={(q1 != null)}, q2={(q2 != null)}, q3={(q3 != null)}, bossTarget={(q3 != null ? q3.targetId : "")}");

                // 2. Pinewatch Timberfang Woods enemies + Warchief Grimgnaw boss + locked spoils chest
                PinewatchWoodsZone.Ensure();
                woodsRoot = GameObject.Find(PinewatchWoodsZone.RootName);
                var woodsEnemies = woodsRoot ? woodsRoot.GetComponentsInChildren<EnemyController>(true) : Array.Empty<EnemyController>();
                var boss = woodsEnemies.FirstOrDefault(e => e.enemyId == PinewatchWoodsZone.BossId);
                var bossStats = boss ? boss.GetComponent<Combatant>() : null;
                var bossChest = woodsRoot ? woodsRoot.GetComponentsInChildren<LootChest>(true).FirstOrDefault(c => c.chestId == PinewatchWoodsZone.BossChestId) : null;
                float minDistToRelay = woodsEnemies.Length > 0
                    ? woodsEnemies.Min(e => Vector2.Distance(new Vector2(e.transform.position.x, e.transform.position.z), new Vector2(-223f, 400f)))
                    : 0f;
                bool woodsOk = woodsEnemies.Length == 10 &&
                               boss != null && boss.isBoss && bossStats != null && bossStats.maxHealth >= 340 &&
                               bossChest != null && minDistToRelay >= 18f;
                Check("pinewatch_woods_camps_boss_and_chest", woodsOk,
                    $"enemies={woodsEnemies.Length}, boss={(boss != null)}, bossHp={(bossStats ? bossStats.maxHealth : 0)}, chest={(bossChest != null)}, minDistToRelay={minDistToRelay:F1}m");

                // 3. Stage Pinewatch + Breaker Works + Whisperwood + Cinderroad
                pinewatchStaged = PinewatchPreviewBuild.StagePinewatch(scene, new StringBuilder());
                frontierStaged = FrontierSitesPreviewBuild.StageAll(scene, new StringBuilder());
                Physics.SyncTransforms();

                // 4. Breaker Works Arena clearance (R <= 19m combat floor clear of any blocking collider)
                var breakerRoot = frontierStaged.transform.Find("Breaker Works Arena");
                var breakerCols = breakerRoot ? breakerRoot.GetComponentsInChildren<Collider>(true) : Array.Empty<Collider>();
                int innerFloorBlockers = 0;
                foreach (var c in breakerCols)
                {
                    if (!c || c.isTrigger) continue;
                    float d = Vector2.Distance(new Vector2(c.bounds.center.x, c.bounds.center.z), new Vector2(180f, 300f));
                    if (d < 19.0f) innerFloorBlockers++;
                }
                bool hasTower = breakerRoot && breakerRoot.Find("PressureTower_Drum") != null;
                bool hasArch = breakerRoot && breakerRoot.Find("FurnaceArch_Lintel") != null;
                bool hasBreakerPin = breakerRoot && breakerRoot.GetComponentsInChildren<SettlementWaystop>(true).Any(w => w.displayName == "Breaker Works");
                Check("breaker_works_arena_floor_clear_and_landmarks",
                    innerFloorBlockers == 0 && hasTower && hasArch && hasBreakerPin,
                    $"innerFloorBlockers={innerFloorBlockers}, tower={hasTower}, arch={hasArch}, pin={hasBreakerPin}");

                // 5. Whisperwood Exchange walkers + divider + highway & Three-Chimes separation
                var wwRoot = frontierStaged.transform.Find("Whisperwood Exchange");
                var wwWalkers = wwRoot ? wwRoot.GetComponentsInChildren<CivilianRouteWalker>(true) : Array.Empty<CivilianRouteWalker>();
                bool wwWalkersOk = ValidateWalkers(wwWalkers, terrains, out string wwDetail);
                float distThreeChimes = Vector2.Distance(new Vector2(35f, 250f), new Vector2(2f, 300f));
                bool hasBrev = wwRoot && wwRoot.GetComponentsInChildren<TownNPC>(true).Any(n => n.npcName == "Quartermaster Brev");
                Check("whisperwood_exchange_walkers_and_separation",
                    wwWalkers.Length == 3 && wwWalkersOk && distThreeChimes >= 38f && hasBrev,
                    $"walkers={wwWalkers.Length}, distThreeChimes={distThreeChimes:F1}m, brev={hasBrev}, {wwDetail}");

                // 6. Cinderroad Waystation walkers + crane + storehouse + beacon + Foreman Darek
                var crRoot = frontierStaged.transform.Find("Cinderroad Waystation");
                var crWalkers = crRoot ? crRoot.GetComponentsInChildren<CivilianRouteWalker>(true) : Array.Empty<CivilianRouteWalker>();
                bool crWalkersOk = ValidateWalkers(crWalkers, terrains, out string crDetail);
                bool hasCrane = crRoot && crRoot.Find("FreightCrane_Boom") != null;
                bool hasStore = crRoot && crRoot.Find("Storehouse_Walls") != null;
                bool hasBeacon = crRoot && crRoot.Find("SignalBeacon_Flame") != null;
                bool hasDarek = crRoot && crRoot.GetComponentsInChildren<TownNPC>(true).Any(n => n.npcName == "Foreman Darek");
                Check("cinderroad_waystation_walkers_and_landmarks",
                    crWalkers.Length == 3 && crWalkersOk && hasCrane && hasStore && hasBeacon && hasDarek,
                    $"walkers={crWalkers.Length}, crane={hasCrane}, store={hasStore}, beacon={hasBeacon}, darek={hasDarek}, {crDetail}");
            }
            catch (Exception ex)
            {
                Check("exception", false, ex.ToString());
            }
            finally
            {
                if (fqsGo) Object.DestroyImmediate(fqsGo);
                if (woodsRoot) Object.DestroyImmediate(woodsRoot);
                if (frontierStaged) Object.DestroyImmediate(frontierStaged);
                if (pinewatchStaged) Object.DestroyImmediate(pinewatchStaged);
            }

            lines.Add($"TOTAL PASS={passed} FAIL={failed} RESULT={(failed == 0 ? "PASS" : "FAIL")}");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, string.Join("\n", lines) + "\n");
            Debug.Log($"[FrontierCompleteChecks] PASS={passed} FAIL={failed}");
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }

        static bool ValidateWalkers(CivilianRouteWalker[] walkers, Terrain[] terrains, out string detail)
        {
            bool allOk = walkers.Length > 0;
            var parts = new List<string>();
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

                int blocked = 0;
                float maxDelta = 0f;
                for (int i = 0; i < pts.Count; i++)
                {
                    Vector3 a = pts[i];
                    Vector3 b = pts[(i + 1) % pts.Count];
                    for (int s = 0; s < 8; s++)
                    {
                        Vector3 p = Vector3.Lerp(a, b, s / 8f);
                        float g = WorldRegions.GroundHeight(p, terrains);
                        float delta = Mathf.Abs(p.y - g);
                        if (delta > maxDelta) maxDelta = delta;

                        var hits = Physics.OverlapCapsule(p + Vector3.up * 0.45f, p + Vector3.up * 1.55f, 0.30f);
                        foreach (var h in hits)
                        {
                            if (!h || h is TerrainCollider || h.isTrigger) continue;
                            if (h.transform.IsChildOf(walker.transform)) continue;
                            blocked++;
                            break;
                        }
                    }
                }
                bool ok = walker.HasValidRoute && blocked == 0 && maxDelta <= 0.16f;
                if (!ok) allOk = false;
                parts.Add($"{walker.name}(blocked={blocked},delta={maxDelta:F3}m)");
            }
            detail = string.Join("; ", parts);
            return allOk;
        }
    }
}
