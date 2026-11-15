using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    public static class OvernightWorldPass
    {
        const string Evidence = ".code-check/overnight/";

        static void OpenWorld()
        {
            if (!Application.isBatchMode || !File.Exists("Docs/Overnight/OWN_CODEX_RUN.md"))
                throw new InvalidOperationException("Use the isolated Codex batch checkout.");
            Directory.CreateDirectory(Evidence);
            EditorSceneManager.OpenScene(WorldRegions.ScenePath);
            Physics.SyncTransforms();
        }

        public static void Baseline()
        {
            try
            {
                OpenWorld();
                Phase1WorldAudit.AuditCurrentScene();
                Capture("Baseline");
                var report = new StringBuilder($"BASELINE UTC={DateTime.UtcNow:O}\n");
                foreach (var t in Terrain.activeTerrains)
                {
                    string path = AssetDatabase.GetAssetPath(t.terrainData);
                    using (var sha = SHA256.Create())
                        report.AppendLine($"TERRAIN {path} SHA256={BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "")} trees={t.terrainData.treeInstanceCount}");
                }
                var cache = Object.FindObjectsByType<LootChest>(FindObjectsSortMode.None).Single(c => c.chestId == "willow-rest-cache");
                report.AppendLine($"CACHE id={cache.chestId} root={cache.transform.position} bounds={BoundsOf(cache.transform)}");
                BridgeProfile(report);
                File.WriteAllText(Evidence + "baseline.txt", report.ToString());
                EditorApplication.Exit(0);
            }
            catch (Exception e) { Fail("baseline", e); }
        }

        static Bounds BoundsOf(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("Object has no renderer bounds: " + root.name);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        static void BridgeProfile(StringBuilder report)
        {
            var deck = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(t => t.name == "Westbank bridge deck");
            report.AppendLine($"DECK position={deck.position} scale={deck.lossyScale} colliderBounds={deck.GetComponent<Collider>().bounds}");
            foreach (float z in new[] { 49.3f, 50f, 50.7f })
                for (float x = -282; x <= -232; x += 1)
                {
                    var p = new Vector3(x, 0, z);
                    float terrain = WorldRegions.GroundHeight(p);
                    var bridgeHits = Physics.RaycastAll(new Vector3(x, 100, z), Vector3.down, 120, ~0, QueryTriggerInteraction.Ignore)
                        .Where(h => h.collider.name == "Westbank bridge deck").ToArray();
                    string top = bridgeHits.Length > 0 ? bridgeHits.Max(h => h.point.y).ToString("F3") : "none";
                    report.AppendLine(FormattableString.Invariant($"BRIDGE x={x:F1} z={z:F1} terrain={terrain:F3} deck={top}"));
                }
        }

        public static void GroundCache()
        {
            try
            {
                OpenWorld();
                if (File.Exists(Evidence + "cache-repair.txt")) throw new InvalidOperationException("Cache repair already applied; verify instead of repeating.");
                var cache = Object.FindObjectsByType<LootChest>(FindObjectsSortMode.None).Single(c => c.chestId == "willow-rest-cache");
                var before = cache.transform.position; var bounds = BoundsOf(cache.transform);
                float bottomOffset = bounds.min.y - before.y;
                Vector3 selected = default; float best = float.MaxValue, selectedSpread = 0, selectedMin = 0, selectedMax = 0;
                for (float x = -299; x <= -278; x += .5f)
                    for (float z = 48; z <= 64; z += .5f)
                    {
                        var point = new Vector3(x, 0, z);
                        var start = new Vector2(-285, 50); var end = new Vector2(-283, 78);
                        var line = end - start; var flat = new Vector2(x, z);
                        float fraction = Mathf.Clamp01(Vector2.Dot(flat - start, line) / line.sqrMagnitude);
                        if (Vector2.Distance(flat, start + line * fraction) < 3.2f) continue;
                        if (Mathf.Abs(z - 50) < 2.5f) continue;
                        float min = float.MaxValue, max = float.MinValue;
                        foreach (float dx in new[] { -.85f, 0f, .85f })
                            foreach (float dz in new[] { -.8f, 0f, .8f })
                            {
                                float h = WorldRegions.GroundHeight(point + new Vector3(dx, 0, dz));
                                min = Mathf.Min(min, h); max = Mathf.Max(max, h);
                            }
                        if (max - min > .85f) continue;
                        point.y = max - bottomOffset + .025f;
                        var colliders = Physics.OverlapBox(point + Vector3.up * .7f, new Vector3(.75f, .6f, .75f), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                        if (colliders.Any(c => !(c is TerrainCollider) && !c.transform.IsChildOf(cache.transform))) continue;
                        float cost = Vector2.Distance(flat, new Vector2(before.x, before.z)) + (max - min) * 20;
                        if (cost >= best) continue;
                        best = cost; selected = point; selectedSpread = max - min; selectedMin = min; selectedMax = max;
                    }
                if (best == float.MaxValue) throw new InvalidOperationException("No flat off-path cache position found; inspect candidates.");
                if (GameObject.Find("Willow Rest cache footing")) throw new InvalidOperationException("Footing exists; inspect before repeating placement.");
                var footing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                footing.name = "Willow Rest cache footing";
                footing.transform.SetParent(cache.transform.parent, false);
                float baseY = selectedMin - .18f;
                footing.transform.position = new Vector3(selected.x, (selectedMax + baseY) * .5f, selected.z);
                footing.transform.localScale = new Vector3(1.7f, selectedMax - baseY, 1.6f);
                footing.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/RPG/Materials/Foundation.mat");
                footing.isStatic = true;
                cache.transform.position = selected;
                Physics.SyncTransforms();
                var finalBounds = BoundsOf(cache.transform);
                if (finalBounds.min.y < WorldRegions.GroundHeight(finalBounds.center) - .05f) throw new Exception("Cache still buried.");
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                File.WriteAllText(Evidence + "cache-repair.txt", $"PASS cache root {before} -> {selected}; supporting footing height spread={selectedSpread:F3}m; ID={cache.chestId}; rewards/hinge untouched. Footing, not terrain flattening, supports the complete chest footprint.\n");
                Capture("Cache");
                EditorApplication.Exit(0);
            }
            catch (Exception e) { Fail("cache", e); }
        }

        public static void RepairBridgeBanks()
        {
            try
            {
                OpenWorld();
                const string target = "Assets/RPG/Terrain/ReaverCamp/Westbank crossing protected.asset";
                if (File.Exists(target) || File.Exists(target + ".meta")) throw new InvalidOperationException("Bridge terrain copy already exists; inspect/verify instead of overwriting.");
                var terrain = WorldRegions.TerrainAt(new Vector3(-255, 6, 50));
                var source = terrain.terrainData;
                if (source.treeInstanceCount != 813 || source.treePrototypes.Length != 15) throw new Exception("Unexpected vegetation baseline.");
                var footprint = new Rect(-274, 45, 37, 10);
                int n = source.heightmapResolution, edits = 0, bedChanges = 0;
                var heights = source.GetHeights(0, 0, n, n);
                var original = (float[,])heights.Clone();
                var origin = terrain.transform.position;
                for (int z = 0; z < n; z++)
                    for (int x = 0; x < n; x++)
                    {
                        float wx = origin.x + x * source.size.x / (n - 1), wz = origin.z + z * source.size.z / (n - 1);
                        if (wx < footprint.xMin || wx > footprint.xMax || wz < footprint.yMin || wz > footprint.yMax) continue;
                        float along = Mathf.Clamp01(Mathf.Min((wx - footprint.xMin) / 3f, (footprint.xMax - wx) / 3f));
                        float across = Mathf.Clamp01((5f - Mathf.Abs(wz - 50)) / 1.25f);
                        float weight = Mathf.SmoothStep(0, 1, Mathf.Min(along, across));
                        float oldY = origin.y + original[z, x] * source.size.y;
                        float nextY = Mathf.Lerp(oldY, Mathf.Min(oldY, 5.9f), weight);
                        if (Mathf.Abs(nextY - oldY) < .001f) continue;
                        if (oldY < 2.5f) bedChanges++;
                        heights[z, x] = (nextY - origin.y) / source.size.y; edits++;
                    }
                if (edits == 0 || bedChanges != 0) throw new Exception("Unexpected bank patch or riverbed modification.");
                if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source), target)) throw new Exception("Protected bridge copy failed.");
                var candidate = AssetDatabase.LoadAssetAtPath<TerrainData>(target);
                candidate.SetHeights(0, 0, heights); EditorUtility.SetDirty(candidate); AssetDatabase.SaveAssetIfDirty(candidate);
                AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
                candidate = AssetDatabase.LoadAssetAtPath<TerrainData>(target);
                var report = new StringBuilder($"BRIDGE BANK REPAIR UTC={DateTime.UtcNow:O} footprint={footprint} changedHeightSamples={edits} riverbedChanges={bedChanges}\n");
                ReaverRepairPreparation.VerifyPreservedData(source, candidate, report, footprint);
                terrain.terrainData = candidate; terrain.GetComponent<TerrainCollider>().terrainData = candidate;
                terrain.Flush(); Physics.SyncTransforms();
                var supports = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name == "Westbank bridge support").ToArray();
                foreach (var support in supports)
                {
                    var point = support.position;
                    float baseY = Mathf.Min(WorldRegions.GroundHeight(point) - .15f, 5.65f);
                    point.y = (baseY + 7.1f) * .5f;
                    support.position = point;
                    var scale = support.localScale; scale.y = 7.1f - baseY; support.localScale = scale;
                }
                Physics.SyncTransforms();
                report.AppendLine($"Retained original deck/rails; grounded/extended {supports.Length} original supports. Reaver three-lane grade={ReaverCampTerrainSetup.CheckApproach():F2}deg.");
                BridgeProfile(report);
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                File.WriteAllText(Evidence + "bridge-repair.txt", report.ToString());
                Capture("Bridge");
                EditorApplication.Exit(0);
            }
            catch (Exception e) { Fail("bridge", e); }
        }

        static void Capture(string stage)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Debug.Log("NOT RUN: matched GPU capture in headless pass; geometry evidence remains separate.");
                return;
            }
            WorldPresentationSetup.RenderPreview("Overnight-" + stage + "-Westbank", new Vector3(-237, 22, 27), new Vector3(-262, 6, 50), false);
            WorldPresentationSetup.RenderPreview("Overnight-" + stage + "-Greenhollow", new Vector3(-14, 27, 368), new Vector3(-40, 13, 400), false);
            WorldPresentationSetup.RenderPreview("Overnight-" + stage + "-Highspire", new Vector3(-186, 37, 12), new Vector3(-204, 12, -36), false);
            WorldPresentationSetup.RenderPreview("Overnight-" + stage + "-Emberwatch", new Vector3(-35, 14, -18), new Vector3(-15, 0, 1), false);
        }

        static void Fail(string packet, Exception e)
        {
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Evidence + packet + "-error.txt", e.ToString());
            Debug.LogException(e); EditorApplication.Exit(1);
        }
    }
}
