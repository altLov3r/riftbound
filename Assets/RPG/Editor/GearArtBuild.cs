using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    public static class GearArtBuild
    {
        const string Root = "Assets/RPG/Resources/Gear/";
        const string Models = "Assets/RPG/Art/GearWeapons/";
        const string Materials = "Assets/RPG/ProceduralKit/Materials/";

        public static void Run()
        {
            try
            {
                if (!Application.isBatchMode) throw new InvalidOperationException("Build the gear preview in batch mode.");
                if (!AssetDatabase.IsValidFolder(Root + "Weapons"))
                    AssetDatabase.CreateFolder(Root.TrimEnd('/'), "Weapons");

                int icons = 0, weapons = 0;
                foreach (var item in ItemCatalog.All)
                {
                    PrepareIcon(item.id);
                    icons++;
                    if (item.slot == GearSlot.Weapon)
                    {
                        PrepareWeapon(item.id);
                        weapons++;
                    }
                }
                Debug.Log($"Gear art verified: {icons} item icons and {weapons} weapon prefabs.");
                PinewatchPreviewBuild.Run();
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }

        static void PrepareIcon(string id)
        {
            string path = Root + "Icons/" + id + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) throw new InvalidOperationException("Missing gear icon " + path);
            if (!importer.alphaIsTransparency || importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp)
            {
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
            var icon = Resources.Load<Texture2D>("Gear/Icons/" + id);
            if (!icon || icon.width != 256 || icon.height != 256)
                throw new InvalidOperationException("Invalid 256px gear icon " + id);
        }

        static void PrepareWeapon(string id)
        {
            string path = Root + "Weapons/" + id + ".prefab";
            if (!File.Exists(path))
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Models + id + ".fbx");
                if (!model) throw new InvalidOperationException("Missing weapon model " + id);
                var root = new GameObject(id);
                try
                {
                    PrefabUtility.InstantiatePrefab(model, root.transform);
                    foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
                        renderer.sharedMaterials = renderer.sharedMaterials.Select(material =>
                        {
                            if (!material) throw new InvalidOperationException("Unassigned weapon material on " + id);
                            var finish = AssetDatabase.LoadAssetAtPath<Material>(Materials + material.name.Replace(' ', '_') + ".mat");
                            if (!finish) throw new InvalidOperationException("Unknown weapon finish " + material.name);
                            return finish;
                        }).ToArray();
                    if (!PrefabUtility.SaveAsPrefabAsset(root, path))
                        throw new InvalidOperationException("Failed to save weapon " + id);
                }
                finally { Object.DestroyImmediate(root); }
            }

            var prefab = Resources.Load<GameObject>("Gear/Weapons/" + id);
            if (!prefab) throw new InvalidOperationException("Weapon resource failed to load " + id);
            var mesh = prefab.GetComponentInChildren<MeshFilter>();
            if (!mesh || !mesh.sharedMesh || mesh.sharedMesh.vertexCount == 0 ||
                prefab.GetComponentsInChildren<Collider>().Length != 0)
                throw new InvalidOperationException("Invalid inspected weapon mesh " + id);
        }
    }
}
