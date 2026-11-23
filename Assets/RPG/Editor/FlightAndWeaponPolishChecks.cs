using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    public static class FlightAndWeaponPolishChecks
    {
        const string ReportPath = ".code-check/emberford-r1/flight-weapon-polish-checks.txt";

        [MenuItem("Riftbound/Emberford/Run Flight Routes, Perched Gryphons, Horses & Live Weapon Checks (EditMode)")]
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
            GameObject testActor = null;

            try
            {
                var scene = EditorSceneManager.OpenScene(WorldRegions.ScenePath, OpenSceneMode.Single);
                Physics.SyncTransforms();

                pinewatchStaged = PinewatchPreviewBuild.StagePinewatch(scene);
                frontierStaged = FrontierSitesPreviewBuild.StageAll(scene);
                Physics.SyncTransforms();

                // 1. All 11 x 10 = 110 Flight Routes clear terrain AND building/mountain colliders
                var nodes = FlightMaster.WorldFlightNodes;
                int routesTested = 0;
                int routeViolations = 0;
                float globalMinClearance = float.MaxValue;
                string worstRoute = "";

                for (int i = 0; i < nodes.Length; i++)
                {
                    Vector3 start = FlightMaster.ResolveLandingPos(nodes[i]);
                    for (int j = 0; j < nodes.Length; j++)
                    {
                        if (i == j) continue;
                        Vector3 end = FlightMaster.ResolveLandingPos(nodes[j]);
                        var path = FlightMaster.BuildSafeFlightPath(start, end);
                        routesTested++;

                        // Verify cruising span t in [0.12 .. 0.88] stays well above terrain & static colliders
                        for (float t = 0.12f; t <= 0.88f; t += 0.04f)
                        {
                            Vector3 pt = FlightMaster.EvaluateSafeSpline(path, t);
                            float obstacleY = FlightMaster.SampleObstacleCeiling(pt);
                            float clearance = pt.y - obstacleY;
                            if (clearance < globalMinClearance)
                            {
                                globalMinClearance = clearance;
                                worstRoute = $"{nodes[i].name}->{nodes[j].name}@t={t:F2}";
                            }
                            if (clearance < 12.0f)
                                routeViolations++;
                        }
                    }
                }
                Check("all_110_flight_routes_clear_terrain_and_buildings",
                    nodes.Length >= 11 && routesTested >= 110 && routeViolations == 0 && globalMinClearance >= 12.0f,
                    $"nodes={nodes.Length}, routes={routesTested}, violations={routeViolations}, minClearance={globalMinClearance:F1}m ({worstRoute})");

                // 2. Every FlightMaster has a grounded Perched Gryphon nearby
                var flightMasters = Object.FindObjectsByType<FlightMaster>(FindObjectsInactive.Include);
                int gryphonsOk = 0;
                string firstBadFm = "";
                foreach (var fm in flightMasters)
                {
                    var g = fm.EnsurePerchedGryphon();
                    if (!g) { if (string.IsNullOrEmpty(firstBadFm)) firstBadFm = fm.nodeName + ":null"; continue; }
                    float horizDist = Vector2.Distance(
                        new Vector2(fm.transform.position.x, fm.transform.position.z),
                        new Vector2(g.transform.position.x, g.transform.position.z));
                    float groundY = FlightMaster.SampleStaticGround(g.transform.position).y;
                    float yDelta = Mathf.Abs(g.transform.position.y - groundY);
                    int meshCount = g.GetComponentsInChildren<MeshRenderer>(true).Length;
                    if (horizDist >= 1.5f && horizDist <= 5.5f && yDelta <= 0.40f && meshCount >= 5)
                        gryphonsOk++;
                    else if (string.IsNullOrEmpty(firstBadFm))
                        firstBadFm = $"{fm.nodeName}(d={horizDist:F1}m,dy={yDelta:F2}m,m={meshCount})";
                }
                Check("every_flight_master_has_grounded_perched_gryphon",
                    flightMasters.Length >= 11 && gryphonsOk == flightMasters.Length,
                    $"flightMasters={flightMasters.Length}, perchedOk={gryphonsOk}, firstBad={firstBadFm}");

                // 3. Zero giant horses in Frontier towns (height <= 2.45m)
                float maxHorseH = 0f;
                int horseCount = 0;
                string tallestHorse = "";
                foreach (var t in frontierStaged.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name != "WW_Horse" && t.name != "TW_HorseA") continue;
                    horseCount++;
                    var rs = t.GetComponentsInChildren<Renderer>(true);
                    if (rs.Length == 0) continue;
                    Bounds b = rs[0].bounds;
                    for (int k = 1; k < rs.Length; k++) b.Encapsulate(rs[k].bounds);
                    if (b.size.y > maxHorseH) { maxHorseH = b.size.y; tallestHorse = t.name; }
                }
                Check("town_horses_properly_scaled_no_giants",
                    horseCount >= 2 && maxHorseH > 1.0f && maxHorseH <= 2.45f,
                    $"horses={horseCount}, maxHeight={maxHorseH:F2}m ({tallestHorse})");

                // 4. All 12 unarmed combatant variants exist & have fewer vertices than baked-weapon originals
                string[] unarmedNames =
                {
                    "Emberwatch_Knight", "Emberwatch_Mage", "Emberwatch_Rogue", "Emberwatch_Paladin",
                    "Briar_Knight", "Briar_Mage", "Briar_Rogue", "Briar_Paladin",
                    "Briar_Brute", "Cutpurse_Goblin", "Quarry_Sentinel", "Gloam_Wraith"
                };
                int unarmedOk = 0;
                string firstUnarmedIssue = "";
                foreach (string nm in unarmedNames)
                {
                    var uPrefab = Resources.Load<GameObject>("Actors/Unarmed/" + nm + "_Unarmed");
                    var origPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/RPG/ProceduralKit/Prefabs/{nm}.prefab");
                    if (!uPrefab || !origPrefab)
                    {
                        if (string.IsNullOrEmpty(firstUnarmedIssue)) firstUnarmedIssue = nm + ":missing";
                        continue;
                    }
                    var uSkin = uPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(s => s.name.EndsWith("_LOD0"));
                    var oSkin = origPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(s => s.name.EndsWith("_LOD0"));
                    bool hasHandR = uSkin && uSkin.bones.Any(b => b && b.name == "hand.R");
                    bool stripped = uSkin && oSkin && uSkin.sharedMesh && oSkin.sharedMesh &&
                                    uSkin.sharedMesh.vertexCount < oSkin.sharedMesh.vertexCount;
                    if (hasHandR && stripped) unarmedOk++;
                    else if (string.IsNullOrEmpty(firstUnarmedIssue))
                        firstUnarmedIssue = $"{nm}(handR={hasHandR},uV={uSkin?.sharedMesh?.vertexCount},oV={oSkin?.sharedMesh?.vertexCount})";
                }
                Check("all_12_unarmed_variants_stripped_and_rigged",
                    unarmedOk == unarmedNames.Length,
                    $"unarmedOk={unarmedOk}/{unarmedNames.Length}, firstIssue={firstUnarmedIssue}");

                // 5. All 15 ItemCatalog weapons + mage-arcane-staff have real 3D prefabs & icons + live hand socket swap
                var weaponIds = ItemCatalog.All.Where(it => it.slot == GearSlot.Weapon).Select(it => it.id).Concat(new[] { "mage-arcane-staff" }).Distinct().ToArray();
                int prefabsOk = 0;
                int iconsOk = 0;
                foreach (string wid in weaponIds)
                {
                    var wp = GearArt.LoadWeaponPrefab(wid);
                    if (wp && wp.GetComponentInChildren<MeshFilter>(true)?.sharedMesh != null) prefabsOk++;
                    if (Resources.Load<Texture2D>("Gear/Icons/" + wid) != null) iconsOk++;
                }

                testActor = CharacterVisualFactory.CreateVisualInstance("Human", "Warrior", null, isPreview: false);
                var pav = testActor ? testActor.GetComponent<ProceduralActorVisual>() : null;
                var testSwapIds = new[] { "starter-axe", "truesilver-blade", "worldbreaker-maul", "sunstone-reliquary-staff", "ironmaw-cleaver", "duskwood-shadow-dagger" };
                var distinctMeshes = new HashSet<Mesh>();
                bool socketParentOk = false;
                bool hideRestoreOk = false;
                if (pav && pav.UsingUnarmedVariant)
                {
                    foreach (string wid in testSwapIds)
                    {
                        pav.EquipWeaponVisual(wid, null, ItemCatalog.Get(wid)?.rarity);
                        var mf = pav.RightWeaponInstance ? pav.RightWeaponInstance.GetComponentInChildren<MeshFilter>(true) : null;
                        if (mf && mf.sharedMesh) distinctMeshes.Add(mf.sharedMesh);
                    }
                    socketParentOk = pav.RightWeaponSocket != null && pav.RightWeaponSocket.parent != null && pav.RightWeaponSocket.parent.name == "hand.R";
                    pav.SetWeaponVisualVisible(false);
                    bool hidden = pav.RightWeaponInstance != null && !pav.RightWeaponInstance.activeSelf;
                    pav.SetWeaponVisualVisible(true);
                    bool restored = pav.RightWeaponInstance != null && pav.RightWeaponInstance.activeSelf;
                    hideRestoreOk = hidden && restored;
                }
                Check("live_3d_weapons_all_16_prefabs_socket_swap_and_mount_sheathe",
                    prefabsOk == weaponIds.Length && iconsOk == weaponIds.Length &&
                    pav != null && pav.UsingUnarmedVariant &&
                    distinctMeshes.Count == testSwapIds.Length && socketParentOk && hideRestoreOk,
                    $"prefabs={prefabsOk}/{weaponIds.Length}, icons={iconsOk}/{weaponIds.Length}, distinctSwaps={distinctMeshes.Count}/{testSwapIds.Length}, handSocket={socketParentOk}, sheathe={hideRestoreOk}");
            }
            catch (Exception ex)
            {
                Check("unexpected_exception", false, ex.ToString());
            }
            finally
            {
                if (testActor) Object.DestroyImmediate(testActor);
                if (frontierStaged) Object.DestroyImmediate(frontierStaged);
                if (pinewatchStaged) Object.DestroyImmediate(pinewatchStaged);
                lines.Add($"TOTAL PASS={passed} FAIL={failed} RESULT={(failed == 0 ? "PASS" : "FAIL")}");
                Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
                File.WriteAllText(ReportPath, string.Join("\n", lines) + "\n");
                Debug.Log($"[FlightAndWeaponPolishChecks] PASS={passed} FAIL={failed}");
                if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
            }
        }
    }
}
