using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riftbound
{
    static class EmberfordHouseSurfaces
    {
        const string ResourceFolder = "EmberfordHouseSurfaces";
        static readonly HashSet<string> Buildings = new(StringComparer.Ordinal)
        {
            "Timber_Cottage", "Wayfarer_Inn", "Emberford_Smithy", "Bell_Chapel",
            "Briar_Camp_Hut", "Briar_War_Hall", "Highspire_Citadel_Hall"
        };
        static readonly Dictionary<Scene, List<Material>> SceneMaterials = new();
        static readonly int Albedo = Shader.PropertyToID("_BaseMap");
        static readonly int Normal = Shader.PropertyToID("_BumpMap");
        static readonly int NormalScale = Shader.PropertyToID("_BumpScale");

#if !UNITY_SERVER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= Apply;
            SceneManager.sceneLoaded += Apply;
            SceneManager.sceneUnloaded -= Release;
            SceneManager.sceneUnloaded += Release;
        }
#endif

        static void Apply(Scene scene, LoadSceneMode _)
        {
            if (scene.name != "Outpost-Expanded" || SceneMaterials.ContainsKey(scene)) return;

            var textures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
            foreach (var texture in Resources.LoadAll<Texture2D>(ResourceFolder))
                textures[texture.name] = texture;

            var replacements = new Dictionary<Material, Material>();
            int upgradedRenderers = 0;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var lodGroup in root.GetComponentsInChildren<LODGroup>(true))
            {
                if (!Buildings.Contains(lodGroup.name)) continue;
                foreach (var renderer in lodGroup.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var materials = renderer.sharedMaterials;
                    bool changed = false;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        var source = materials[i];
                        if (!source) continue;
                        if (!replacements.TryGetValue(source, out var replacement))
                        {
                            replacement = CreateReplacement(source, textures);
                            replacements.Add(source, replacement);
                        }
                        if (!replacement) continue;
                        materials[i] = replacement;
                        changed = true;
                    }
                    if (!changed) continue;

                    renderer.sharedMaterials = materials;
                    upgradedRenderers++;
                }
            }

            var owned = new List<Material>();
            foreach (var material in replacements.Values)
                if (material) owned.Add(material);
            SceneMaterials.Add(scene, owned);
            Debug.Log($"Emberford house surfaces: applied detailed textures to {upgradedRenderers} building renderers.");
        }

        static Material CreateReplacement(Material source, Dictionary<string, Texture2D> textures)
        {
            if (!source.HasProperty(Albedo) || !source.HasProperty(Normal)) return null;
            var currentAlbedo = source.GetTexture(Albedo);
            if (!currentAlbedo || (currentAlbedo.name != "stone_albedo" && currentAlbedo.name != "wood_albedo")) return null;

            string name = " " + source.name.Replace('_', ' ');
            string finish;
            if (name.EndsWith(" Plaster", StringComparison.Ordinal))
                finish = "plaster";
            else if (name.EndsWith(" Oak", StringComparison.Ordinal) || name.EndsWith(" Oak Edge", StringComparison.Ordinal) ||
                name.EndsWith(" Dark Wood", StringComparison.Ordinal))
                finish = "timber";
            else if (name.EndsWith(" Slate", StringComparison.Ordinal) || name.EndsWith(" Slate Edge", StringComparison.Ordinal) ||
                name.EndsWith(" Ember Tile", StringComparison.Ordinal) || name.EndsWith(" Ember Edge", StringComparison.Ordinal))
                finish = "roof";
            else if (name.EndsWith(" Limestone", StringComparison.Ordinal) || name.EndsWith(" Pale Stone", StringComparison.Ordinal) ||
                name.EndsWith(" Basalt", StringComparison.Ordinal))
                finish = "masonry";
            else return null;

            if (!textures.TryGetValue(finish + "_albedo", out var albedo) ||
                !textures.TryGetValue(finish + "_normal", out var normal))
                return null;

            var updated = new Material(source) { name = source.name + " House Detail" };
            updated.SetTexture(Albedo, albedo);
            updated.SetTexture(Normal, normal);
            updated.SetFloat(NormalScale, .48f);
            updated.EnableKeyword("_NORMALMAP");
            return updated;
        }

        static void Release(Scene scene)
        {
            if (!SceneMaterials.Remove(scene, out var owned)) return;
            foreach (var material in owned)
                UnityEngine.Object.Destroy(material);
        }
    }
}
