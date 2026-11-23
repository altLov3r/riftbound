using System;
using System.IO;
using System.Linq;
using System.Text;
using Riftbound.WorldAmbient;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    public sealed class PinewatchPreviewBuild : IProcessSceneWithReport
    {
        const string Models = "Assets/RPG/Art/Pinewatch/";
        const string Materials = "Assets/RPG/ProceduralKit/Materials/";
        const string Receipt = ".code-check/pinewatch/staging.txt";
        static bool staging;
        public int callbackOrder => 12;

        public static void Run()
        {
            int exit = 1;
            string product = PlayerSettings.productName;
            try
            {
                if (!Application.isBatchMode) throw new InvalidOperationException("Use a batch preview build.");
                var scene = EditorSceneManager.OpenScene(WorldRegions.ScenePath);
                var report = new StringBuilder("Pinewatch edit-mode geometry checks. Art and actors staged in memory.\n");
                var preview = Stage(scene, report, out var deck, out var rampFoot);
                try
                {
                    Physics.SyncTransforms();
                    var player = Object.FindFirstObjectByType<PlayerController>().GetComponent<CharacterController>();
                    var route = new WorldTraversalVerification.Route("Pinewatch local bypass",
                        new Vector3(-212, 0, 389), new Vector3(-210, 0, 399), new Vector3(-212, 0, 410));
                    foreach (bool reverse in new[] { false, true })
                    foreach (bool mounted in new[] { false, true })
                        WorldTraversalVerification.Traverse(route, mounted, reverse, player, report);
                    var access = new WorldTraversalVerification.Route("Pinewatch overlook ramp",
                        Vector3.Lerp(rampFoot, deck, -.17f), rampFoot, deck);
                    WorldTraversalVerification.Traverse(access, false, false, player, report);
                    Directory.CreateDirectory(Path.GetDirectoryName(Receipt));
                    File.WriteAllText(Receipt, report.ToString());
                    if (report.ToString().Contains("BLOCKED")) throw new InvalidOperationException("Pinewatch route check failed. See " + Receipt);
                }
                finally { Object.DestroyImmediate(preview); }

                staging = true;
                PlayerSettings.productName = "Riftbound Pinewatch Preview";
                exit = GameplayCandidateBuild.Build();
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
            finally
            {
                PlayerSettings.productName = product;
                staging = false;
            }
            EditorApplication.Exit(exit);
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (!staging || report == null || scene.path != WorldRegions.ScenePath) return;
            VillageRemodelPreviewBuild.StageRemodels(scene);
            var notes = new StringBuilder();
            Stage(scene, notes, out _, out _);
            Debug.Log("Pinewatch preview staged: " + notes);
        }

        static GameObject Stage(Scene scene, StringBuilder report, out Vector3 deck, out Vector3 rampFoot)
        {
            var terrains = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Terrain>()).ToArray();
            if (terrains.Length == 0) throw new InvalidOperationException("No authored terrain in the preview scene.");
            report.AppendLine($"Scene terrain components={terrains.Length}");
            var root = new GameObject("Pinewatch Relay");
            SceneManager.MoveGameObjectToScene(root, scene);

            var shelter = Place(root.transform, "Pinewatch Shelter", -223, 400, 65, 3.08f, 2.18f, terrains, report);
            AddPiers(shelter.transform, new[] { -3.05f, 3.05f }, new[] { -2.1f, 0, 2.1f }, .52f, "Limestone", terrains);
            var overlook = Place(root.transform, "Pinewatch Overlook", -233, 393, 48, 2.9f, 1.95f, terrains, report);
            AddPiers(overlook.transform, new[] { -2.77f, 2.77f }, new[] { -1.83f, 1.83f }, .28f, "Dark Wood", terrains);
            AddRamp(overlook, terrains, report, out deck, out rampFoot);

            Place(root.transform, "Pinewatch Trail Lantern", -216, 391, 32, .32f, .30f, terrains, report);
            Place(root.transform, "Pinewatch Trail Lantern", -216, 407, 148, .32f, .30f, terrains, report);
            var notice = new GameObject("Pinewatch map pin").AddComponent<SettlementWaystop>();
            notice.transform.SetParent(root.transform);
            notice.transform.position = Surface(new Vector3(-217, 0, 400), terrains);
            notice.displayName = "Pinewatch Relay";
            notice.notice = "Pinewatch Relay. The wind snapped the east signal line. Halen is fitting a replacement at the shelter.";
            AddWatchkeeper(root.transform, terrains);
            return root;
        }

        static GameObject Place(Transform parent, string name, float x, float z, float yaw,
            float halfWidth, float halfDepth, Terrain[] terrains, StringBuilder report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Models + name + ".fbx");
            if (!asset) throw new InvalidOperationException("Missing model " + name);
            var placed = new GameObject(name);
            placed.transform.SetParent(parent, false);
            PrefabUtility.InstantiatePrefab(asset, placed.transform);
            var position = new Vector3(x, 0, z);
            var tile = WorldRegions.TerrainAt(position, terrains);
            if (!tile) throw new InvalidOperationException("No terrain under " + name);
            var rotation = Quaternion.Euler(0, yaw, 0);
            var samples = from px in new[] { -halfWidth, 0, halfWidth }
                          from pz in new[] { -halfDepth, 0, halfDepth }
                          select Surface(position + rotation * new Vector3(px, 0, pz), terrains).y;
            var heights = samples.ToArray();
            var nearestTree = tile.terrainData.treeInstances.Min(tree =>
                Vector2.Distance(new Vector2(x,z), Xz(tile.transform.position + Vector3.Scale(tree.position, tile.terrainData.size))));
            if (nearestTree < Mathf.Sqrt(halfWidth * halfWidth + halfDepth * halfDepth) + 1f)
                throw new InvalidOperationException($"{name} is too close to a terrain tree. Distance {nearestTree:F1} m.");
            placed.transform.SetPositionAndRotation(new Vector3(x, heights.Max() + .01f, z), rotation);
            foreach (var mesh in placed.GetComponentsInChildren<MeshFilter>())
            {
                var renderer = mesh.GetComponent<MeshRenderer>();
                if (!renderer || !mesh.sharedMesh || renderer.sharedMaterials.Any(material => !material))
                    throw new InvalidOperationException("Mesh and material import incomplete for " + name);
                renderer.sharedMaterials = renderer.sharedMaterials.Select(material => LoadMaterial(material.name)).ToArray();
                mesh.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh.sharedMesh;
            }
            report.AppendLine($"{name} ({x},{z}) ground={heights.Min():F2}..{heights.Max():F2}m; nearest tree root={nearestTree:F1}m");
            return placed;
        }

        static void AddRamp(GameObject overlook, Terrain[] terrains, StringBuilder report, out Vector3 deck, out Vector3 foot)
        {
            Physics.SyncTransforms();
            var deckMesh = overlook.GetComponentInChildren<MeshCollider>();
            var origin = overlook.transform.position;
            RaycastHit surface = default;
            // Probe both boards beside the centre seam.
            bool supported = new[] { .13f, -.12f }.Any(offset => deckMesh.Raycast(
                new Ray(origin + Vector3.up * 2 + overlook.transform.right * offset, Vector3.down), out surface, 2.2f));
            if (!supported)
                throw new InvalidOperationException($"Missing deck support at {origin:F2}, model bounds {deckMesh.bounds}.");
            Vector3 forward = overlook.transform.forward;
            var floor = new Vector3(origin.x, surface.point.y, origin.z);
            bool Closed(Vector3 direction) => deckMesh.Raycast(
                new Ray(floor + Vector3.up * .6f + direction * 2.55f, -direction), out _, 1.3f);
            if (Closed(forward)) forward = -forward;
            if (Closed(forward)) throw new InvalidOperationException("The lookout deck needs an open access side.");

            Vector3 top = floor + forward * 1.94f;
            foot = Surface(floor + forward * 7.1f, terrains) - Vector3.up * .015f;
            Vector3 rise = top - foot;
            float angle = Vector3.Angle(rise, Vector3.ProjectOnPlane(rise, Vector3.up));
            if (angle > 24f) throw new InvalidOperationException($"Overlook ramp slope {angle:F1}°.");
            var rotation = Quaternion.LookRotation(rise, Vector3.up);
            var ramp = Solid(overlook.transform.parent, "Overlook access ramp",
                (foot + top) * .5f - rotation * Vector3.up * .085f,
                new Vector3(1.75f, .17f, rise.magnitude), "Oak");
            ramp.transform.rotation = rotation;
            for (int i = 1; i < 3; i++)
            {
                Vector3 at = Vector3.Lerp(foot, top, i / 3f);
                float ground = Surface(at, terrains).y - .08f;
                float height = at.y - .15f - ground;
                if (height > .05f)
                    Solid(overlook.transform.parent, "Ramp timber footing", new Vector3(at.x, ground + height / 2, at.z),
                        new Vector3(.28f, height, .27f), "Dark Wood");
            }
            deck = floor;
            report.AppendLine($"Overlook access surface={floor.y:F2}m, ramp={angle:F1}°, length={rise.magnitude:F1}m");
        }

        static void AddPiers(Transform root, float[] xs, float[] zs, float width, string material, Terrain[] terrains)
        {
            foreach (float x in xs)
            foreach (float z in zs)
            {
                var top = root.TransformPoint(new Vector3(x, .035f, z));
                float floor = Surface(top, terrains).y - .08f;
                float height = top.y - floor;
                if (height <= .09f) continue;
                var pier = Solid(root, "Terrain-fit footing", new Vector3(top.x, floor + height * .5f, top.z),
                    new Vector3(width, height, width), material);
                pier.transform.rotation = root.rotation;
            }
        }

        static void AddWatchkeeper(Transform parent, Terrain[] terrains)
        {
            var actor = new GameObject("Watchkeeper Halen");
            actor.transform.SetParent(parent);
            actor.transform.position = Surface(new Vector3(-218.4f, 0, 397), terrains);
            CharacterVisualFactory.CreateVisualInstance("Human", "Warrior", actor.transform);
            var body = actor.AddComponent<CapsuleCollider>();
            body.height = 1.88f;
            body.radius = .36f;
            body.center = Vector3.up * .94f;
            var npc = actor.AddComponent<TownNPC>();
            npc.npcName = "Watchkeeper Halen";
            npc.roleTag = "Pinewatch Relay";
            npc.greeting = "The east line snapped in the night. Greenhollow's waiting for our signal. You can see their ridge from the overlook.";
        }

        static GameObject Solid(Transform parent, string name, Vector3 position, Vector3 size, string material)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = name;
            prop.transform.SetParent(parent);
            prop.transform.position = position;
            prop.transform.localScale = size;
            prop.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(material);
            return prop;
        }

        static Material LoadMaterial(string name)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Materials + name.Replace(' ', '_') + ".mat");
            if (!material) throw new InvalidOperationException("Missing project material " + name);
            return material;
        }

        static Vector3 Surface(Vector3 point, Terrain[] terrain)
        {
            point.y = WorldRegions.GroundHeight(point, terrain);
            return point;
        }

        static Vector2 Xz(Vector3 point) => new(point.x, point.z);
    }
}
