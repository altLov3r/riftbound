using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Riftbound
{
    public class LootDropSystem : MonoBehaviour
    {
        public static LootDropSystem Instance { get; private set; }

        public class LootCorpse
        {
            public string id;
            public string name;
            public Vector3 position;
            public int gold;
            public int boneShards;
            public List<string> items = new List<string>();
            public float spawnTime;
            public GameObject markerObject;
        }

        List<LootCorpse> activeCorpses = new List<LootCorpse>();
        public List<LootCorpse> ActiveCorpses => activeCorpses;
        public LootCorpse CurrentLooting { get; private set; }

        PlayerController player;
        GUIStyle windowStyle, titleStyle, itemStyle, coinStyle, btnStyle;
        Texture2D winBgTex, borderTex;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(this); else DestroyImmediate(this);
                return;
            }
            Instance = this;
            player = GetComponent<PlayerController>();
        }

        public static void SpawnLoot(Vector3 pos, string enemyName, int gold, int boneShards, string itemId)
        {
            if (!Instance)
            {
                var go = new GameObject("LootDropSystem");
                Instance = go.AddComponent<LootDropSystem>();
            }

            var corpse = new LootCorpse
            {
                id = System.Guid.NewGuid().ToString(),
                name = enemyName,
                position = pos,
                gold = gold,
                boneShards = boneShards,
                spawnTime = Time.time
            };

            if (!string.IsNullOrEmpty(itemId))
            {
                corpse.items.Add(itemId);
            }

            // Create sparkling visual beacon
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = $"LootSparkle_{enemyName}";
            marker.transform.position = pos + Vector3.up * 0.45f;
            marker.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            var col = marker.GetComponent<Collider>();
            if (col) Destroy(col);

            var light = marker.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.85f, 0.25f);
            light.range = 3.5f;
            light.intensity = 2.0f;

            var renderer = marker.GetComponent<Renderer>();
            if (renderer)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetColor("_BaseColor", new Color(1f, 0.88f, 0.35f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 1.5f);
                renderer.sharedMaterial = mat;
            }

            marker.AddComponent<LootSparkleAnim>();
            corpse.markerObject = marker;

            Instance.activeCorpses.Add(corpse);
        }

        void Update()
        {
            if (!player) player = GetComponent<PlayerController>();
            if (!player || FrontendMenu.IsOpen) return;

            // Despawn old unlooted corpses after 120s
            for (int i = activeCorpses.Count - 1; i >= 0; i--)
            {
                var c = activeCorpses[i];
                if (Time.time - c.spawnTime > 120f)
                {
                    if (c.markerObject) Destroy(c.markerObject);
                    activeCorpses.RemoveAt(i);
                }
            }

            // Close looting if moved away
            if (CurrentLooting != null)
            {
                if (Vector3.Distance(player.transform.position, CurrentLooting.position) > 4.2f || !player.Stats.Alive)
                {
                    CloseLoot();
                }
                else if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
                {
                    CloseLoot();
                }
            }
        }

        public LootCorpse GetNearbyCorpse(Vector3 playerPos, float maxDist = 3.2f)
        {
            LootCorpse closest = null;
            float bestDist = maxDist;
            for (int i = 0; i < activeCorpses.Count; i++)
            {
                float d = Vector3.Distance(playerPos, activeCorpses[i].position);
                if (d < bestDist)
                {
                    bestDist = d;
                    closest = activeCorpses[i];
                }
            }
            return closest;
        }

        public void OpenLoot(LootCorpse corpse)
        {
            CurrentLooting = corpse;
            AudioEffects.PlayClick();
        }

        public void CloseLoot()
        {
            CurrentLooting = null;
        }

        public void LootAll(LootCorpse corpse)
        {
            if (corpse == null || !player) return;

            if (corpse.gold > 0)
            {
                int g = corpse.gold;
                if (player.CharacterRace == "Human") g = Mathf.RoundToInt(g * 1.25f);
                player.AddGoldDirect(g);
                ChatSystem.LogLoot($"{g} Gold", "common");
                corpse.gold = 0;
            }

            if (corpse.boneShards > 0)
            {
                player.AddBoneShardsDirect(corpse.boneShards);
                ChatSystem.LogLoot($"{corpse.boneShards} Bone Shards", "common");
                corpse.boneShards = 0;
            }

            for (int i = corpse.items.Count - 1; i >= 0; i--)
            {
                string id = corpse.items[i];
                var gear = ItemCatalog.Get(id);
                player.GrantItem(id);
                ChatSystem.LogLoot(gear != null ? gear.name : id, gear != null ? gear.rarity.ToLowerInvariant() : "rare");
                corpse.items.RemoveAt(i);
            }

            AudioEffects.PlayLootCoin();

            // Cleanup corpse
            if (corpse.markerObject) Destroy(corpse.markerObject);
            activeCorpses.Remove(corpse);
            CurrentLooting = null;
        }

        public void LootItem(LootCorpse corpse, string itemId)
        {
            if (corpse == null || !player) return;
            var gear = ItemCatalog.Get(itemId);
            player.GrantItem(itemId);
            ChatSystem.LogLoot(gear != null ? gear.name : itemId, gear != null ? gear.rarity.ToLowerInvariant() : "rare");
            corpse.items.Remove(itemId);
            AudioEffects.PlayLootCoin();

            if (corpse.gold <= 0 && corpse.boneShards <= 0 && corpse.items.Count == 0)
            {
                if (corpse.markerObject) Destroy(corpse.markerObject);
                activeCorpses.Remove(corpse);
                CurrentLooting = null;
            }
        }

        public void LootCoins(LootCorpse corpse)
        {
            if (corpse == null || !player || corpse.gold <= 0) return;
            int g = corpse.gold;
            if (player.CharacterRace == "Human") g = Mathf.RoundToInt(g * 1.25f);
            player.AddGoldDirect(g);
            ChatSystem.LogLoot($"{g} Gold", "common");
            corpse.gold = 0;
            AudioEffects.PlayLootCoin();

            if (corpse.boneShards <= 0 && corpse.items.Count == 0)
            {
                if (corpse.markerObject) Destroy(corpse.markerObject);
                activeCorpses.Remove(corpse);
                CurrentLooting = null;
            }
        }

        public void DrawLootGUI(float screenW, float screenH)
        {
            if (CurrentLooting == null || FrontendMenu.IsOpen) return;

            InitStyles();

            float winW = 280f;
            float winH = 200f;
            float x = screenW / 2f - winW / 2f;
            float y = screenH / 2f - winH / 2f;

            // Background & border
            GUI.Box(new Rect(x, y, winW, winH), GUIContent.none, windowStyle);

            // Title
            GUI.Label(new Rect(x + 12f, y + 8f, winW - 50f, 24f), CurrentLooting.name.ToUpperInvariant() + " - LOOT", titleStyle);
            if (GUI.Button(new Rect(x + winW - 32f, y + 8f, 22f, 22f), "X", btnStyle))
            {
                CloseLoot();
                return;
            }

            float rowY = y + 38f;

            // Gold Coins Row
            if (CurrentLooting.gold > 0)
            {
                GUI.Label(new Rect(x + 14f, rowY, winW - 90f, 24f), $"● {CurrentLooting.gold} Gold Coins", coinStyle);
                if (GUI.Button(new Rect(x + winW - 74f, rowY, 60f, 22f), "Take", btnStyle))
                {
                    LootCoins(CurrentLooting);
                }
                rowY += 28f;
            }

            // Bone Shards Row
            if (CurrentLooting.boneShards > 0)
            {
                GUI.Label(new Rect(x + 14f, rowY, winW - 90f, 24f), $"● {CurrentLooting.boneShards} Bone Shards", itemStyle);
                rowY += 28f;
            }

            // Items Rows
            for (int i = 0; i < CurrentLooting.items.Count; i++)
            {
                string id = CurrentLooting.items[i];
                var gear = ItemCatalog.Get(id);
                string itemName = gear != null ? gear.name : id;

                itemStyle.normal.textColor = gear != null ? gear.color : Color.white;
                GUI.Label(new Rect(x + 14f, rowY, winW - 90f, 24f), $"◆ {itemName}", itemStyle);
                if (GUI.Button(new Rect(x + winW - 74f, rowY, 60f, 22f), "Take", btnStyle))
                {
                    LootItem(CurrentLooting, id);
                    break;
                }
                rowY += 28f;
            }

            // [Loot All] button at bottom
            if (GUI.Button(new Rect(x + 14f, y + winH - 36f, winW - 28f, 28f), "LOOT ALL [E]", btnStyle))
            {
                LootAll(CurrentLooting);
            }
        }

        void InitStyles()
        {
            if (windowStyle != null) return;

            winBgTex = new Texture2D(1, 1);
            winBgTex.SetPixel(0, 0, new Color(0.08f, 0.09f, 0.12f, 0.94f));
            winBgTex.Apply();

            borderTex = new Texture2D(1, 1);
            borderTex.SetPixel(0, 0, new Color(0.35f, 0.28f, 0.15f, 0.95f));
            borderTex.Apply();

            windowStyle = new GUIStyle();
            windowStyle.normal.background = winBgTex;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.normal.textColor = new Color(1.0f, 0.85f, 0.35f);

            itemStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };
            itemStyle.normal.textColor = Color.white;

            coinStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            coinStyle.normal.textColor = new Color(1.0f, 0.88f, 0.25f);

            btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }

    public class LootSparkleAnim : MonoBehaviour
    {
        Vector3 basePos;
        void Start() { basePos = transform.position; }
        void Update()
        {
            transform.position = basePos + Vector3.up * (Mathf.Sin(Time.time * 3.5f) * 0.12f);
            transform.Rotate(0f, 65f * Time.deltaTime, 0f);
        }
    }
}
