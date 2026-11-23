using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    public static class UnarmedAndWeaponAssetSetup
    {
        const string UnarmedFbxRoot = "Assets/RPG/Art/UnarmedActors/";
        const string UnarmedPrefabRoot = "Assets/RPG/Resources/Actors/Unarmed/";
        const string WeaponFbxRoot = "Assets/RPG/Art/GearWeapons/";
        const string WeaponPrefabRoot = "Assets/RPG/Resources/Gear/Weapons/";
        const string IconRoot = "Assets/RPG/Resources/Gear/Icons/";
        const string MaterialsRoot = "Assets/RPG/ProceduralKit/Materials/";

        static readonly string[] NewWeaponIds =
        {
            "sunstone-reliquary-staff",
            "ironmaw-cleaver",
            "mage-arcane-staff"
        };

        static readonly string[] UnarmedActorNames =
        {
            "Emberwatch_Knight", "Emberwatch_Mage", "Emberwatch_Rogue", "Emberwatch_Paladin",
            "Briar_Knight", "Briar_Mage", "Briar_Rogue", "Briar_Paladin",
            "Briar_Brute", "Cutpurse_Goblin", "Quarry_Sentinel", "Gloam_Wraith"
        };

        [MenuItem("Riftbound/Emberford/Build Unarmed Actor & New Weapon Prefabs")]
        public static void Run()
        {
            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                EnsureFolders();

                int weaponsBuilt = 0;
                foreach (string id in NewWeaponIds)
                {
                    ConfigureIcon(id);
                    BuildWeaponPrefab(id);
                    weaponsBuilt++;
                }

                int actorsBuilt = 0;
                foreach (string actorName in UnarmedActorNames)
                {
                    BuildUnarmedActorPrefab(actorName);
                    actorsBuilt++;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Debug.Log($"[UnarmedAndWeaponAssetSetup] PASS built {actorsBuilt} unarmed actor prefabs and {weaponsBuilt} new weapon prefabs/icons.");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/RPG/Resources/Actors"))
                AssetDatabase.CreateFolder("Assets/RPG/Resources", "Actors");
            if (!AssetDatabase.IsValidFolder("Assets/RPG/Resources/Actors/Unarmed"))
                AssetDatabase.CreateFolder("Assets/RPG/Resources/Actors", "Unarmed");
            if (!AssetDatabase.IsValidFolder("Assets/RPG/Resources/Gear/Weapons"))
                AssetDatabase.CreateFolder("Assets/RPG/Resources/Gear", "Weapons");
        }

        static void ConfigureIcon(string id)
        {
            string path = IconRoot + id + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) throw new InvalidOperationException("Missing icon PNG: " + path);
            if (!importer.alphaIsTransparency || importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp)
            {
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
        }

        static void BuildWeaponPrefab(string id)
        {
            string fbxPath = WeaponFbxRoot + id + ".fbx";
            string prefabPath = WeaponPrefabRoot + id + ".prefab";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (!model) throw new InvalidOperationException("Missing weapon FBX: " + fbxPath);

            var root = new GameObject(id);
            try
            {
                PrefabUtility.InstantiatePrefab(model, root.transform);
                BindMaterials(root);
                foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
                if (!PrefabUtility.SaveAsPrefabAsset(root, prefabPath))
                    throw new InvalidOperationException("Failed saving weapon prefab: " + prefabPath);
            }
            finally { Object.DestroyImmediate(root); }
        }

        static void BuildUnarmedActorPrefab(string actorName)
        {
            string fbxPath = UnarmedFbxRoot + actorName + "_Unarmed.fbx";
            string prefabPath = UnarmedPrefabRoot + actorName + "_Unarmed.prefab";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (!model) throw new InvalidOperationException("Missing unarmed actor FBX: " + fbxPath);

            var root = new GameObject(actorName + "_Unarmed");
            try
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;

                BindMaterials(root);
                foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);

                var skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var lod0 = skins.FirstOrDefault(s => s.name.EndsWith("_LOD0", StringComparison.Ordinal));
                var lod1 = skins.FirstOrDefault(s => s.name.EndsWith("_LOD1", StringComparison.Ordinal));
                var lod2 = skins.FirstOrDefault(s => s.name.EndsWith("_LOD2", StringComparison.Ordinal));
                if (lod0 && lod1 && lod2)
                {
                    var group = root.AddComponent<LODGroup>();
                    group.SetLODs(new[]
                    {
                        new LOD(0.24f, new Renderer[] { lod0 }),
                        new LOD(0.102f, new Renderer[] { lod1 }),
                        new LOD(0.017f, new Renderer[] { lod2 })
                    });
                    group.RecalculateBounds();
                }

                if (!PrefabUtility.SaveAsPrefabAsset(root, prefabPath))
                    throw new InvalidOperationException("Failed saving unarmed prefab: " + prefabPath);
            }
            finally { Object.DestroyImmediate(root); }
        }

        static void BindMaterials(GameObject root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                r.sharedMaterials = r.sharedMaterials.Select(mat =>
                {
                    if (!mat) return mat;
                    string key = mat.name.Replace(' ', '_') + ".mat";
                    var finish = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + key);
                    return finish ? finish : mat;
                }).ToArray();
            }
        }
    }
}
