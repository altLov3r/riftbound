using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    public static class WorldSignText
    {
        // Match complete authored captions so quest prompts and floating zone labels stay separate.
        static readonly Dictionary<string, string> Copy = new Dictionary<string, string>
        {
            ["NORTH: GREENHOLLOW\nWEST: DUSKWOOD / EAST: BLOODROCK"] =
                "NORTH  GREENHOLLOW POST\nWEST  DUSKWOOD / EAST  REDRIDGE",
            ["HIGHSPIRE KEEP\nWESTBANK RIVER / FROSTPEAK"] =
                "HIGHSPIRE ROAD\nFrostpeak Foothills / Highspire Keep\nWestbank via Highspire",
            ["HIGHSPIRE CITADEL\nSeat of the Grand Alliance"] =
                "HIGHSPIRE KEEP\nAlliance capital",
            ["REDRIDGE CANYON\nHorde Frontier & Ashen Crags"] =
                "REDRIDGE CANYONS\nRoad to Rustwater Scrapworks",
            ["BLOODROCK PASS\nRUSTWATER / GOR'KRESH / WYRM CREST"] =
                "CAMP BLOODROCK\nVia Rustwater Scrapworks",
            ["WYRM CREST\nMountain switchback trail"] =
                "WYRMCREST CRAGS\nSwitchbacks to Wyrmcrest Summit",
            ["WYRMCREST PASS\nDanger: Apex Drake Nesting Grounds"] =
                "WYRMCREST PASS\nDanger: Apex Drake at the summit",
            ["DUSKWOOD BORDER\nCursed Woods & Cathedral Grounds"] =
                "DUSKWOOD BORDER\nDeathknell Hollow / Duskwood Cathedral",
            ["WILLOW REST\nWestbank river trail"] =
                "WILLOW REST\nWestbank Bridge",
            ["GREENHOLLOW RIDING POST"] = "GREENHOLLOW POST",
            ["[Z] Summon horse  /  [V] Change mount"] = "[Z] Summon or dismount  /  [V] Cycle mount",
            ["TRAINING\nResets health / no rewards"] = "TRAINING\nHealth resets / No rewards",
            ["EMBERWATCH OUTPOST"] = "EMBERWATCH OUTPOST",
            ["ASHWOOD CAMP"] = "ASHWOOD CAMP",
            ["THE RUINED WATCHTOWER"] = "THE RUINED WATCHTOWER",
            ["WESTERN CRYPT"] = "WESTERN CRYPT",
            ["SPLIT-SEAL LAYBY"] = "SPLIT-SEAL LAYBY",
            ["TALLYKEEPERS’ REST"] = "TALLYKEEPERS’ REST"
        };

        public static bool Show(TextMesh text)
        {
            if (string.IsNullOrEmpty(text.text)) return false;
            if (text.transform.parent && !text.transform.parent.gameObject.activeInHierarchy)
                return false;

            string caption = text.text.Replace("\r\n", "\n");
            if (!Copy.TryGetValue(caption, out var copy))
            {
                if (!Copy.ContainsValue(caption)) return false;
                copy = caption;
            }
            text.text = copy;
            text.alignment = TextAlignment.Center;
            var renderer = text.GetComponent<MeshRenderer>();
            if (renderer) renderer.enabled = true;
            var billboard = text.GetComponent<WorldLabel>();
            if (billboard) billboard.enabled = false;
            if (!text.GetComponent<SignFrontFaceCuller>())
                text.gameObject.AddComponent<SignFrontFaceCuller>();
            text.gameObject.SetActive(true);
            return true;
        }
    }

    /// <summary>
    /// Hides 3D sign TextMesh renderers when viewed from behind (TextMesh faces -transform.forward)
    /// or beyond readable range so signs are never readable backwards from the rear.
    /// </summary>
    public sealed class SignFrontFaceCuller : MonoBehaviour
    {
        MeshRenderer textRenderer;

        void Awake()
        {
            textRenderer = GetComponent<MeshRenderer>();
        }

        void LateUpdate()
        {
            if (!textRenderer) textRenderer = GetComponent<MeshRenderer>();
            if (!textRenderer) return;
            var cam = Camera.main;
            if (!cam) return;

            Vector3 toCam = cam.transform.position - transform.position;
            float sqrDist = toCam.sqrMagnitude;
            if (sqrDist > 900f)
            {
                textRenderer.enabled = false;
                return;
            }

            // Unity TextMesh glyphs face -transform.forward; require camera to be in front of the sign face.
            bool frontFacing = Vector3.Dot(toCam.normalized, -transform.forward) > 0.08f;
            textRenderer.enabled = frontFacing;
        }
    }
}
