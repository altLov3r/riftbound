using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        [MenuItem("Riftbound/Emberford/Run Complete Frontier Sites, Seamless Cavern & Sign Checks (EditMode)")]
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
            GameObject cavernRoot = null;
            GameObject fqsGo = null;
            GameObject signGo = null;

            try
            {
                var scene = EditorSceneManager.OpenScene(WorldRegions.ScenePath, OpenSceneMode.Single);
                var terrains = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>()).ToArray();
                Physics.SyncTransforms();

                // 1. All new questlines registered (Pinewatch 3-step + Ashenmaw Cavern 2-step)
                fqsGo = new GameObject("TempFqsCheck");
                var fqs = fqsGo.AddComponent<FactionQuestSystem>();
                typeof(FactionQuestSystem)
                    .GetMethod("BuildCatalog", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(fqs, null);

                var pw1 = FactionQuestSystem.GetQuest("pinewatch_clear_woods");
                var pw2 = FactionQuestSystem.GetQuest("pinewatch_recover_line");
                var pw3 = FactionQuestSystem.GetQuest("pinewatch_warchief_boss");
                var am1 = FactionQuestSystem.GetQuest("ashenmaw_tribe_cull");
                var am2 = FactionQuestSystem.GetQuest("ashenmaw_bear_guardian");
                bool questsOk = pw1 != null && pw2 != null && pw3 != null &&
                                am1 != null && am2 != null &&
                                am1.giverNpc == AshenmawCavernZone.QuestGiverName &&
                                am2.prerequisiteQuestId == "ashenmaw_tribe_cull" &&
                                am2.targetId == AshenmawCavernZone.BossId &&
                                ItemCatalog.Get("pinewatch-warden-crest") != null &&
                                ItemCatalog.Get("ursolak-spirit-pelt") != null;
                Check("pinewatch_and_ashenmaw_questlines_registered", questsOk,
                    $"pw=({pw1 != null},{pw2 != null},{pw3 != null}), ashenmaw=({am1 != null},{am2 != null}), items=({ItemCatalog.Get("pinewatch-warden-crest") != null},{ItemCatalog.Get("ursolak-spirit-pelt") != null})");

                // 2. Pinewatch Timberfang Woods camps + Warchief Grimgnaw boss
                PinewatchWoodsZone.Ensure();
                woodsRoot = GameObject.Find(PinewatchWoodsZone.RootName);
                var woodsEnemies = woodsRoot ? woodsRoot.GetComponentsInChildren<EnemyController>(true) : Array.Empty<EnemyController>();
                var pwBoss = woodsEnemies.FirstOrDefault(e => e.enemyId == PinewatchWoodsZone.BossId);
                Check("pinewatch_woods_camps_and_boss",
                    woodsEnemies.Length == 10 && pwBoss != null && pwBoss.isBoss,
                    $"enemies={woodsEnemies.Length}, boss={(pwBoss != null)}");

                // 3. Seamless Ashenmaw Cavern (no loading screen) + Briarclaw Tribe (8) + Ursolak Bear Guardian (480 HP) + Warden Kaelen + Reliquary
                AshenmawCavernZone.Ensure();
                cavernRoot = GameObject.Find(AshenmawCavernZone.RootName);
                var cavernEnemies = cavernRoot ? cavernRoot.GetComponentsInChildren<EnemyController>(true) : Array.Empty<EnemyController>();
                int tribeCount = cavernEnemies.Count(e => e.enemyId.StartsWith("ashenmaw-tribe"));
                var bearBoss = cavernEnemies.FirstOrDefault(e => e.enemyId == AshenmawCavernZone.BossId);
                var bearStats = bearBoss ? bearBoss.GetComponent<Combatant>() : null;
                var kaelen = cavernRoot ? cavernRoot.GetComponentsInChildren<TownNPC>(true).FirstOrDefault(n => n.npcName == AshenmawCavernZone.QuestGiverName) : null;
                var reliquary = cavernRoot ? cavernRoot.GetComponentsInChildren<LootChest>(true).FirstOrDefault(c => c.chestId == AshenmawCavernZone.BossChestId) : null;
                bool cavernOk = tribeCount == 8 && bearBoss != null && bearBoss.isBoss && bearStats != null && bearStats.maxHealth >= 480 && kaelen != null && reliquary != null;
                Check("ashenmaw_seamless_cavern_tribe_and_bear_guardian", cavernOk,
                    $"tribe={tribeCount}, bearBoss={(bearBoss != null)}, bearHp={(bearStats ? bearStats.maxHealth : 0)}, kaelen={(kaelen != null)}, chest={(reliquary != null)}");

                // 4. Sign back-face culling check (WorldSignText attaches SignFrontFaceCuller & disables WorldLabel)
                signGo = new GameObject("TempSignCheck");
                var tm = signGo.AddComponent<TextMesh>();
                tm.text = "EMBERWATCH OUTPOST";
                var wl = signGo.AddComponent<WorldLabel>();
                bool shown = WorldSignText.Show(tm);
                bool signOk = shown && !wl.enabled && signGo.GetComponent<SignFrontFaceCuller>() != null;
                Check("sign_not_readable_from_back_culler", signOk,
                    $"shown={shown}, billboardDisabled={(!wl.enabled)}, culler={(signGo.GetComponent<SignFrontFaceCuller>() != null)}");

                // 5. Stage Pinewatch + Monumental Breaker Works + Grounded Whisperwood + SE Cinderroad Ironworks + SW Thornwatch Citadel
                pinewatchStaged = PinewatchPreviewBuild.StagePinewatch(scene, new StringBuilder());
                frontierStaged = FrontierSitesPreviewBuild.StageAll(scene, new StringBuilder());
                Physics.SyncTransforms();

                // 6. Breaker Works monumental arena floor clearance (R < 19m zero blockers)
                var breakerRoot = frontierStaged.transform.Find("Breaker Works Arena");
                var breakerCols = breakerRoot ? breakerRoot.GetComponentsInChildren<Collider>(true) : Array.Empty<Collider>();
                int innerFloorBlockers = 0;
                foreach (var c in breakerCols)
                {
                    if (!c || c.isTrigger) continue;
                    float d = Vector2.Distance(new Vector2(c.bounds.center.x, c.bounds.center.z), new Vector2(180f, 300f));
                    if (d < 19.0f) innerFloorBlockers++;
                }
                bool breakerOk = innerFloorBlockers == 0 &&
                                 breakerRoot != null &&
                                 breakerRoot.Find("PressureTower_Drum") != null &&
                                 breakerRoot.Find("FurnaceArch_Lintel") != null;
                Check("breaker_works_monumental_arena_clear", breakerOk, $"innerFloorBlockers={innerFloorBlockers}");

                // 7. Whisperwood Exchange grounded check (Quartermaster Brev & foundation flush on PhysicsSurface) + 3 walkers
                var wwRoot = frontierStaged.transform.Find("Whisperwood Exchange");
                var brev = wwRoot ? wwRoot.GetComponentsInChildren<TownNPC>(true).FirstOrDefault(n => n.npcName == "Quartermaster Brev") : null;
                float brevGroundDelta = brev ? Mathf.Abs(brev.transform.position.y - FrontierSitesPreviewBuild.PhysicsSurface(brev.transform.position, terrains).y) : 99f;
                var wwWalkers = wwRoot ? wwRoot.GetComponentsInChildren<CivilianRouteWalker>(true) : Array.Empty<CivilianRouteWalker>();
                bool wwWalkersOk = ValidateWalkers(wwWalkers, terrains, out string wwDetail);
                Check("whisperwood_grounded_zero_floating_and_walkers",
                    brev != null && brevGroundDelta <= 0.05f && wwWalkers.Length == 3 && wwWalkersOk,
                    $"brevDelta={brevGroundDelta:F3}m, {wwDetail}");

                // 8. SE Corner Mega-Site: Cinderroad Ironworks Bastion + 3 walkers
                var crRoot = frontierStaged.transform.Find("Cinderroad Waystation");
                var crWalkers = crRoot ? crRoot.GetComponentsInChildren<CivilianRouteWalker>(true) : Array.Empty<CivilianRouteWalker>();
                bool crWalkersOk = ValidateWalkers(crWalkers, terrains, out string crDetail);
                bool crOk = crRoot != null && crRoot.Find("Smelter_Chimney") != null && crRoot.Find("BastionGate_Lintel") != null && crWalkers.Length == 3 && crWalkersOk;
                Check("cinderroad_ironworks_se_corner_and_walkers", crOk, crDetail);

                // 9. SW Corner Mega-Site: Thornwatch Citadel + 3 patrols
                var twRoot = frontierStaged.transform.Find("Thornwatch Citadel");
                var twWalkers = twRoot ? twRoot.GetComponentsInChildren<CivilianRouteWalker>(true) : Array.Empty<CivilianRouteWalker>();
                bool twWalkersOk = ValidateWalkers(twWalkers, terrains, out string twDetail);
                bool twOk = twRoot != null && twRoot.Find("Thornwatch_KeepShaft") != null && twRoot.Find("Thornwatch_GateLintel") != null && twWalkers.Length == 3 && twWalkersOk;
                Check("thornwatch_citadel_sw_corner_and_patrols", twOk, twDetail);
            }
            catch (Exception ex)
            {
                Check("exception", false, ex.ToString());
            }
            finally
            {
                if (signGo) Object.DestroyImmediate(signGo);
                if (fqsGo) Object.DestroyImmediate(fqsGo);
                if (cavernRoot) Object.DestroyImmediate(cavernRoot);
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
                        float g = FrontierSitesPreviewBuild.PhysicsSurface(p, terrains).y;
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
                bool ok = walker.HasValidRoute && blocked == 0 && maxDelta <= 0.25f;
                if (!ok) allOk = false;
                parts.Add($"{walker.name}(blocked={blocked},delta={maxDelta:F3}m)");
            }
            detail = string.Join("; ", parts);
            return allOk;
        }
    }
}
