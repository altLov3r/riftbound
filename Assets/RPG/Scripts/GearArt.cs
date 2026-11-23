using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    internal static class GearArt
    {
        static readonly Dictionary<string, Texture2D> Icons = new();

        internal static string ResolveArtId(string id) => id switch
        {
            "pinewatch-warden-crest" => "emberwatch-sigil",
            "ursolak-spirit-pelt" => "ember-drake-scale",
            "sunstone-reliquary-staff" => "scarlet-blade",
            "ironmaw-cleaver" => "bloodrock-waraxe",
            _ => id
        };

        internal static Texture2D Icon(GearItem item)
        {
            if (item == null) return null;
            if (!Icons.TryGetValue(item.id, out var icon))
            {
                icon = Resources.Load<Texture2D>("Gear/Icons/" + item.id)
                    ?? Resources.Load<Texture2D>("Gear/Icons/" + ResolveArtId(item.id));
                Icons.Add(item.id, icon);
            }
            return icon;
        }
    }

    [DisallowMultipleComponent]
    internal sealed class GearWeaponPreview : MonoBehaviour
    {
        const int Layer = 31;
        GameObject stage, model;
        Transform turntable;
        Camera view;
        RenderTexture target;
        PlayerController owner;
        string displayed;

        internal RenderTexture Show(PlayerController player, GearItem item)
        {
            owner = player;
            string id = item?.slot == GearSlot.Weapon ? item.id : null;
            if (displayed != id)
            {
                displayed = id;
                if (model) Destroy(model);
                model = null;
                if (id != null)
                {
                    var prefab = Resources.Load<GameObject>("Gear/Weapons/" + id)
                        ?? Resources.Load<GameObject>("Gear/Weapons/" + GearArt.ResolveArtId(id));
                    if (prefab) Prepare(prefab);
                }
            }
            if (view) view.enabled = model && IsVisible;
            return model ? target : null;
        }

        bool IsVisible => owner && owner.InventoryOpen && !FrontendMenu.IsOpen;

        void Prepare(GameObject prefab)
        {
            if (!stage)
            {
                stage = new GameObject("Inventory weapon inspection") { layer = Layer };
                stage.transform.position = new Vector3(0, -4096, 0);
                turntable = new GameObject("Weapon turntable").transform;
                turntable.SetParent(stage.transform, false);
                target = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32)
                    { name = "Inventory weapon preview" };
                view = new GameObject("Weapon inspection camera").AddComponent<Camera>();
                view.transform.SetParent(stage.transform, false);
                view.transform.localPosition = new Vector3(0, .1f, -3f);
                view.transform.localRotation = Quaternion.LookRotation(new Vector3(0, -.1f, 3f));
                view.orthographic = true;
                view.nearClipPlane = .1f;
                view.farClipPlane = 8f;
                view.clearFlags = CameraClearFlags.SolidColor;
                view.backgroundColor = new Color(.06f, .10f, .12f, 1);
                view.cullingMask = 1 << Layer;
                view.targetTexture = target;
                view.allowHDR = false;
                var key = new GameObject("Weapon inspection light").AddComponent<Light>();
                key.transform.SetParent(stage.transform, false);
                key.transform.localPosition = new Vector3(-1.3f, 1.8f, -2f);
                key.type = LightType.Point;
                key.cullingMask = 1 << Layer;
                key.intensity = 6f;
                key.range = 5f;
            }

            turntable.localRotation = Quaternion.identity;
            model = Instantiate(prefab, turntable, false);
            foreach (var node in model.GetComponentsInChildren<Transform>(true)) node.gameObject.layer = Layer;
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Destroy(model);
                model = null;
                return;
            }
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            model.transform.position += turntable.position - bounds.center;
            view.orthographicSize = Mathf.Max(.18f, bounds.size.magnitude * .59f);
            turntable.localRotation = Quaternion.Euler(0, -20, -31);
        }

        void LateUpdate()
        {
            if (!view) return;
            view.enabled = model && IsVisible;
            if (view.enabled) turntable.Rotate(Vector3.up, Time.unscaledDeltaTime * 19f, Space.World);
        }

        void OnDisable()
        {
            if (view) view.enabled = false;
        }

        void OnDestroy()
        {
            if (stage) Destroy(stage);
            if (target)
            {
                target.Release();
                Destroy(target);
            }
        }
    }
}
