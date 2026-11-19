using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riftbound.Editor
{
    public sealed class SettlementPreviewBuild : IProcessSceneWithReport
    {
        const string Product = "Riftbound Settlement Preview";
        static bool buildingPreview;

        public int callbackOrder => 0;

        public static void Run()
        {
            if (!Application.isBatchMode || PlayerSettings.productName != Product)
            {
                Debug.LogError($"Build the settlement preview from an isolated project named {Product}.");
                EditorApplication.Exit(1);
                return;
            }

            buildingPreview = true;
            try { GameplayCandidateBuild.Run(); }
            finally { buildingPreview = false; }
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (!buildingPreview || report == null || scene.path != WorldRegions.ScenePath) return;

            var previous = SceneManager.GetActiveScene();
            if (scene != previous) SceneManager.SetActiveScene(scene);
            try
            {
                var placement = SettlementPlacementPreview.Create(new System.Text.StringBuilder());
                foreach (var model in placement.Models)
                {
                    if (!model || model.gameObject.scene != scene || !model.sharedMesh)
                        throw new BuildFailedException("An expected settlement model was not included in the build scene.");
                }
                Debug.Log($"Settlement preview: staged {placement.Models.Length} models in {scene.name}.");
                if (Array.IndexOf(Environment.GetCommandLineArgs(), "-includeCoilbreakYard") >= 0)
                {
                    var yard = CoilbreakYardPreview.Create(new System.Text.StringBuilder());
                    foreach (var part in yard)
                        if (!part || part.gameObject.scene != scene)
                            throw new BuildFailedException("An expected Coilbreak Yard placement was missing from the build scene.");
                    Debug.Log($"Settlement preview: staged {yard.Count} Coilbreak Yard placements in {scene.name}.");
                }
            }
            finally
            {
                if (previous != scene && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }
    }
}
