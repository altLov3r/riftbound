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

        [MenuItem("Riftbound/Emberford/Run Complete Frontier Towns, Safe Separation, Models & Expedition Checks (EditMode)")]
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
            GameObject abbeyRoot = null;
            GameObject quarryRoot = null;
            GameObject heraldRoot = null;
            GameObject fqsGo = null;
            GameObject signGo = null;

            try
            {
                var scene = EditorSceneManager.OpenScene(WorldRegions.ScenePath, OpenSceneMode.Single);
                var terrains = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>()).ToArray();
                Physics.SyncTransforms();

                // 1. Custom Blender FBX models exist and load cleanly
                var fbxPaths = new[]
                {
                    "Assets/RPG/Resources/Ashenmaw/Ashenmaw_Cavern_Complete.fbx",
                    "Assets/RPG/Resources/Ashenmaw/Ursolak_BearGuardian.fbx",
                    "Assets/RPG/Art/FrontierOverhaul/BreakerWorks_MagmaArch.fbx",
                    "Assets/RPG/Art/FrontierOverhaul/BreakerWorks_PressureTower.fbx",
                    "Assets/RPG/Art/FrontierOverhaul/BreakerWorks_RimBastion.fbx",
                    "Assets/RPG/Art/FrontierOverhaul/Frontier_CoastalLighthouse.fbx"
                };
                int loadedFbx = fbxPaths.Count(p => AssetDatabase.LoadAssetAtPath<GameObject>(p) != null);
                var lhAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPG/Art/FrontierOverhaul/Frontier_CoastalLighthouse.fbx");
                var lhFilter = lhAsset ? lhAsset.GetComponentInChildren<MeshFilter>(true) : null;
                bool lhUpright = lhFilter && lhFilter.sharedMesh && lhFilter.sharedMesh.bounds.size.y >= 15f;
                Check("blender_overhaul_fbx_assets_present_and_upright", loadedFbx == fbxPaths.Length && lhUpright,
                    $"loaded={loadedFbx}/{fbxPaths.Length}, lhUprightY={(lhFilter && lhFilter.sharedMesh ? lhFilter.sharedMesh.bounds.size.y : 0f):F1}m");

                // 2. All local questlines + all 7 Grand Frontier Expedition quests + Herald + GearArt icons
                fqsGo = new GameObject("TempFqsCheck");
                var fqs = fqsGo.AddComponent<FactionQuestSystem>();
                typeof(FactionQuestSystem)
                    .GetMethod("BuildCatalog", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(fqs, null);

                FrontierExpeditionHerald.Ensure();
                heraldRoot = GameObject.Find(FrontierExpeditionHerald.RootName);
                var veyra = heraldRoot ? heraldRoot.GetComponentInChildren<TownNPC>(true) : null;

                var expeditionIds = new[]
                {
                    "expedition_whisperwood", "expedition_pinewatch", "expedition_ashenmaw",
                    "expedition_sunstone", "expedition_cinderroad", "expedition_ironmaw", "expedition_thornwatch"
                };
                bool expOk = expeditionIds.All(id => FactionQuestSystem.GetQuest(id) != null) &&
                             veyra != null && veyra.npcName == FrontierExpeditionHerald.NpcName;

                var rewardIds = new[] { "pinewatch-warden-crest", "ursolak-spirit-pelt", "sunstone-reliquary-staff", "ironmaw-cleaver", "worldbreaker-maul" };
                bool gearIconsOk = rewardIds.All(id => ItemCatalog.Get(id) != null && GearArt.Icon(ItemCatalog.Get(id)) != null);
                Check("grand_expedition_chain_herald_and_gear_icons", expOk && gearIconsOk,
                    $"expedition=({expeditionIds.Count(id => FactionQuestSystem.GetQuest(id) != null)}/7), veyra={(veyra != null)}, gearIcons={gearIconsOk}");

                // 3. Instantiate all 4 hostile quest zones + check Ashenmaw bounds orientation
                PinewatchWoodsZone.Ensure();
                woodsRoot = GameObject.Find(PinewatchWoodsZone.RootName);

                AshenmawCavernZone.Ensure();
                cavernRoot = GameObject.Find(AshenmawCavernZone.RootName);

                SunstoneAbbeyZone.Ensure();
                abbeyRoot = GameObject.Find(SunstoneAbbeyZone.RootName);

                IronmawQuarryZone.Ensure();
                quarryRoot = GameObject.Find(IronmawQuarryZone.RootName);

                var caveShell = cavernRoot ? cavernRoot.transform.Find("Ashenmaw_OrganicCavern") : null;
                Bounds caveBounds = new Bounds();
                if (caveShell)
                {
                    var rs = caveShell.GetComponentsInChildren<MeshRenderer>(true);
                    if (rs.Length > 0)
                    {
                        caveBounds = rs[0].bounds;
                        for (int i = 1; i < rs.Length; i++) caveBounds.Encapsulate(rs[i].bounds);
                    }
                }
                float entranceGroundY = FrontierSitesPreviewBuild.PhysicsSurface(AshenmawCavernZone.EntrancePos, terrains).y;
                bool caveBoundsOk = caveShell != null &&
                                    caveBounds.max.z >= AshenmawCavernZone.EntrancePos.z + 55f &&
                                    caveBounds.min.y <= entranceGroundY - 2.0f;
                Check("ashenmaw_cavern_fbx_orientation_and_depth", caveBoundsOk,
                    $"boundsZ=({caveBounds.min.z:F1}..{caveBounds.max.z:F1}), minY={caveBounds.min.y:F1}, entY={entranceGroundY:F1}");

                // 4. Safe-Town & Safe-Camp Separation Audit (every hostile >=36m from giver & >=48m from towns!)
                var safeTowns = new[]
                {
                    ("Pinewatch Relay", new Vector2(-223f, 400f)),
                    ("Whisperwood Exchange", new Vector2(35f, 250f)),
                    ("Cinderroad Ironworks", new Vector2(262f, -55f)),
                    ("Thornwatch Citadel", new Vector2(-275f, -110f)),
                    ("Skyreach Aerie", new Vector2(355f, 420f)),
                    ("Frostpine Watch", new Vector2(-270f, 520f)),
                    ("Redtide Harbor", new Vector2(365f, -110f)),
                    ("Willowmere Mill", new Vector2(-270f, 125f))
                };

                float minTownDist = float.MaxValue;
                float minGiverDist = float.MaxValue;
                string worstViolation = "";

                void CheckSeparation(GameObject zoneRoot, Vector3 giverXZ)
                {
                    if (!zoneRoot) return;
                    foreach (var ec in zoneRoot.GetComponentsInChildren<EnemyController>(true))
                    {
                        Vector2 ep = new Vector2(ec.transform.position.x, ec.transform.position.z);
                        float dg = Vector2.Distance(ep, new Vector2(giverXZ.x, giverXZ.z));
                        if (dg < minGiverDist) { minGiverDist = dg; if (dg < 36f) worstViolation = $"{ec.enemyId}->giver={dg:F1}m"; }
                        foreach (var (tName, tPos) in safeTowns)
                        {
                            float dt = Vector2.Distance(ep, tPos);
                            if (dt < minTownDist) { minTownDist = dt; if (dt < 48f) worstViolation = $"{ec.enemyId}->{tName}={dt:F1}m"; }
                        }
                    }
                }
                CheckSeparation(woodsRoot, new Vector3(-218.4f, 0f, 397f));
                CheckSeparation(cavernRoot, AshenmawCavernZone.CampfirePos);
                CheckSeparation(abbeyRoot, SunstoneAbbeyZone.CampPos);
                CheckSeparation(quarryRoot, IronmawQuarryZone.CampPos);

                Check("hostile_camps_spaced_from_safe_towns_and_givers",
                    minGiverDist >= 36f && minTownDist >= 48f,
                    $"minGiver={minGiverDist:F1}m, minTown={minTownDist:F1}m{(string.IsNullOrEmpty(worstViolation) ? "" : ", violation=" + worstViolation)}");

                // 5. Bosses & Locked Chests
                var bearBoss = cavernRoot?.GetComponentsInChildren<EnemyController>(true).FirstOrDefault(e => e.enemyId == AshenmawCavernZone.BossId);
                var grimgnaw = woodsRoot?.GetComponentsInChildren<EnemyController>(true).FirstOrDefault(e => e.enemyId == PinewatchWoodsZone.BossId);
                var malrik = abbeyRoot?.GetComponentsInChildren<EnemyController>(true).FirstOrDefault(e => e.enemyId == SunstoneAbbeyZone.BossId);
                var krag = quarryRoot?.GetComponentsInChildren<EnemyController>(true).FirstOrDefault(e => e.enemyId == IronmawQuarryZone.BossId);
                Check("four_frontier_bosses_spawned",
                    bearBoss != null && grimgnaw != null && malrik != null && krag != null,
                    $"ursolak={(bearBoss != null)}, grimgnaw={(grimgnaw != null)}, malrik={(malrik != null)}, krag={(krag != null)}");

                // 6. Sign back-face culling check
                signGo = new GameObject("TempSignCheck");
                var tm = signGo.AddComponent<TextMesh>();
                tm.text = "EMBERWATCH OUTPOST";
                var wl = signGo.AddComponent<WorldLabel>();
                bool shown = WorldSignText.Show(tm);
                Check("sign_not_readable_from_back_culler",
                    shown && !wl.enabled && signGo.GetComponent<SignFrontFaceCuller>() != null,
                    $"shown={shown}, billboardDisabled={(!wl.enabled)}");

                // 7. Stage Pinewatch + Hero-Building Frontier Small Towns
                pinewatchStaged = PinewatchPreviewBuild.StagePinewatch(scene, new StringBuilder());
                frontierStaged = FrontierSitesPreviewBuild.StageAll(scene, new StringBuilder());
                Physics.SyncTransforms();

                // 8. Breaker Works arena clear (R < 19m zero blockers)
                var breakerRoot = frontierStaged.transform.Find("Breaker Works Arena");
                int innerFloorBlockers = 0;
                if (breakerRoot)
                {
                    foreach (var c in breakerRoot.GetComponentsInChildren<Collider>(true))
                    {
                        if (!c || c.isTrigger) continue;
                        if (Vector2.Distance(new Vector2(c.bounds.center.x, c.bounds.center.z), new Vector2(180f, 300f)) < 19.0f)
                            innerFloorBlockers++;
                    }
                }
                Check("breaker_works_fbx_arena_floor_clear", innerFloorBlockers == 0, $"innerBlockers={innerFloorBlockers}");

                // 9. Whisperwood normal-mapped Hero Buildings + Grounded Brev + 3 walkers
                var wwRoot = frontierStaged.transform.Find("Whisperwood Exchange");
                bool wwHeroBuildings = wwRoot && wwRoot.Find("Wayfarer_Inn") != null && wwRoot.Find("Emberford_Smithy") != null && wwRoot.Find("Timber_Cottage") != null;
                var brev = wwRoot ? wwRoot.GetComponentsInChildren<TownNPC>(true).FirstOrDefault(n => n.npcName == "Quartermaster Brev") : null;
                float brevDelta = brev ? Mathf.Abs(brev.transform.position.y - FrontierSitesPreviewBuild.PhysicsSurface(brev.transform.position, terrains).y) : 99f;
                var wwWalkers = wwRoot ? wwRoot.GetComponentsInChildren<CivilianRouteWalker>(true) : Array.Empty<CivilianRouteWalker>();
                string wwDetail = "";
                bool wwOk = wwHeroBuildings && brev != null && brevDelta <= 0.05f && wwWalkers.Length == 3 && ValidateWalkers(wwWalkers, terrains, out wwDetail);
                Check("whisperwood_hero_town_grounded_and_walkers", wwOk, $"hero={wwHeroBuildings}, brevDelta={brevDelta:F3}m, {wwDetail}");

                // 10. Cinderroad + Thornwatch + Other 5 Hero-Building Small Towns
                var crRoot = frontierStaged.transform.Find("Cinderroad Waystation");
                var crWalkers = crRoot ? crRoot.GetComponentsInChildren<CivilianRouteWalker>(true) : Array.Empty<CivilianRouteWalker>();
                string crDetail = "";
                bool crOk = crRoot && crRoot.Find("Wayfarer_Inn") != null && crRoot.Find("Emberford_Smithy") != null &&
                            crWalkers.Length == 3 && ValidateWalkers(crWalkers, terrains, out crDetail);
                Check("cinderroad_ironworks_hero_town_and_walkers", crOk, crDetail);

                var twRoot = frontierStaged.transform.Find("Thornwatch Citadel");
                var twWalkers = twRoot ? twRoot.GetComponentsInChildren<CivilianRouteWalker>(true) : Array.Empty<CivilianRouteWalker>();
                string twDetail = "";
                bool twOk = twRoot && twRoot.Find("Highspire_Citadel_Hall") != null && twRoot.Find("Wayfarer_Inn") != null &&
                            twWalkers.Length == 3 && ValidateWalkers(twWalkers, terrains, out twDetail);
                Check("thornwatch_citadel_hero_town_and_patrols", twOk, twDetail);

                bool otherTownsOk = frontierStaged.transform.Find("Skyreach Aerie")?.Find("Bell_Chapel") != null &&
                                    frontierStaged.transform.Find("Frostpine Watch")?.Find("Wayfarer_Inn") != null &&
                                    frontierStaged.transform.Find("Redtide Harbor")?.Find("Redtide_Lighthouse") != null &&
                                    frontierStaged.transform.Find("Willowmere Mill")?.Find("Wayfarer_Inn") != null &&
                                    frontierStaged.transform.Find("Sunstone Abbey Ruins")?.Find("Bell_Chapel") != null &&
                                    frontierStaged.transform.Find("Ironmaw Quarry Architecture")?.Find("Briar_War_Hall") != null;
                Check("remaining_six_hero_building_towns_staged", otherTownsOk, $"allHeroTowns={otherTownsOk}");
            }
            catch (Exception ex)
            {
                Check("exception", false, ex.ToString());
            }
            finally
            {
                if (signGo) Object.DestroyImmediate(signGo);
                if (fqsGo) Object.DestroyImmediate(fqsGo);
                if (heraldRoot) Object.DestroyImmediate(heraldRoot);
                if (quarryRoot) Object.DestroyImmediate(quarryRoot);
                if (abbeyRoot) Object.DestroyImmediate(abbeyRoot);
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
