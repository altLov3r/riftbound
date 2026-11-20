using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riftbound
{
    /// <summary>Connects the retained ferry ramp to the reshaped bank when the updated crossing is loaded.</summary>
    public static class WestbankFerryAccess
    {
        const string ObjectName = "Westbank bank connector";
        const string Resource = "Westbank/Westbank_BankConnector";
        static readonly Vector3 Origin = new Vector3(-265.8f, 5.70f, 40.60f);
        static readonly Vector3 RampHead = new Vector3(-261.4f, 0, 40.60f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode _) => Ensure(scene);

        public static GameObject Ensure(Scene scene)
        {
            if (scene.name != "Outpost-Expanded" || !scene.isLoaded) return null;
            var landing = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Westbank Ferry Landing");
            if (!landing) return null;
            var existing = landing.transform.Find(ObjectName);
            if (existing) return existing.gameObject;
            if (WorldRegions.GroundHeight(RampHead) > 4.7f) return null;

            var dock = landing.GetComponentsInChildren<MeshRenderer>()
                .FirstOrDefault(renderer => renderer.name.Contains("LandingDock", StringComparison.Ordinal));
            var model = Resources.Load<GameObject>(Resource);
            if (!dock || !dock.sharedMaterial || !model)
            {
                Debug.LogError("Westbank ferry access requires the connector model and the existing dock material.");
                return null;
            }

            var connector = new GameObject(ObjectName);
            connector.transform.SetParent(landing.transform, false);
            connector.transform.SetPositionAndRotation(Origin, Quaternion.identity);
            var visual = UnityEngine.Object.Instantiate(model, connector.transform, false);
            var part = visual.GetComponentInChildren<MeshFilter>();
            part.GetComponent<MeshRenderer>().sharedMaterial = dock.sharedMaterial;
            part.gameObject.AddComponent<MeshCollider>().sharedMesh = part.sharedMesh;
            return connector;
        }
    }
}
