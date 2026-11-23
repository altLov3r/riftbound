using UnityEngine;

namespace Riftbound
{
    /// <summary>
    /// Ruined southern frontier abbey ("Sunstone Abbey") at (115, -115) with Brother Aldric's
    /// 2-step cultist questline, 5 Ashen Cultists, Arch-Inquisitor Malrik (420 HP boss),
    /// and a boss-locked reliquary chest. Works in offline solo and headless dedicated server.
    /// </summary>
    public static class SunstoneAbbeyZone
    {
        public const string RootName = "Sunstone Abbey Quest Zone";
        public const string QuestGiverName = "Brother Aldric";
        public const string BossId = "sunstone-abbey-boss";
        public const string BossName = "Arch-Inquisitor Malrik";
        public const string BossChestId = "sunstone-abbey-chest";

        public static readonly Vector3 CampPos = new Vector3(115f, 0f, -86f);
        public static readonly Vector3 NavePos = new Vector3(115f, 0f, -110f);
        public static readonly Vector3 AltarPos = new Vector3(115f, 0f, -128f);

        public static void Ensure()
        {
            if (!WorldRegions.Expanded || GameObject.Find(RootName)) return;

            var player = Object.FindFirstObjectByType<PlayerController>();
            var root = new GameObject(RootName).transform;

            // 1. Safe Watch Camp north of the ruins (Brother Aldric + campfire + Atlas Waystop)
            Vector3 campG = Ground(CampPos);
            Prop(root, "Aldric_FirePit", campG + Vector3.up * 0.14f, new Vector3(1.4f, 0.28f, 1.4f), new Color(0.28f, 0.26f, 0.25f), false);
            var flame = Prop(root, "Aldric_Flame", campG + Vector3.up * 0.34f, new Vector3(0.7f, 0.28f, 0.7f), new Color(0.98f, 0.55f, 0.14f), false, emissive: true);
            AddPointLight(flame, new Color(0.98f, 0.55f, 0.14f), 14f, 2.2f);

            var npcGo = new GameObject(QuestGiverName);
            npcGo.transform.SetParent(root, false);
            npcGo.transform.position = Ground(CampPos + new Vector3(2.2f, 0f, 0.4f));
            npcGo.transform.rotation = Quaternion.Euler(0f, 185f, 0f);
            CharacterVisualFactory.CreateVisualInstance("Human", "Mage", npcGo.transform);
            var col = npcGo.AddComponent<CapsuleCollider>();
            col.height = 1.88f; col.radius = 0.36f; col.center = Vector3.up * 0.94f;
            var npc = npcGo.AddComponent<TownNPC>();
            npc.npcName = QuestGiverName;
            npc.roleTag = "Abbey Chronicler";
            npc.greeting = "Ashen Cultists have seized the ruined nave of Sunstone Abbey south of my fire. Arch-Inquisitor Malrik channels dark fire at the broken high altar.";

            var pin = new GameObject("Sunstone Abbey map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root, false);
            pin.transform.position = Ground(CampPos + new Vector3(-2.4f, 0f, 0.6f));
            pin.displayName = "Sunstone Abbey";
            pin.notice = "Sunstone Abbey Ruins — Seized by the Ashen Cult and Arch-Inquisitor Malrik. Speak with Brother Aldric at the northern campfire.";

            // 2. Cultist Courtyard & Nave (Step 1: Defeat 5 Ashen Cultists)
            RiverReaverCamp.CreateEnemy(root, player, "sunstone-cultist-1", "Ashen Zealot", "Human", "Warrior", NavePos + new Vector3(-4.5f, 0f, 4.0f), false);
            RiverReaverCamp.CreateEnemy(root, player, "sunstone-cultist-2", "Ashen Pyromancer", "Undead", "Mage", NavePos + new Vector3(4.5f, 0f, 3.5f), false, ranged: true);
            RiverReaverCamp.CreateEnemy(root, player, "sunstone-cultist-3", "Ashen Zealot", "Human", "Rogue", NavePos + new Vector3(-5.2f, 0f, -3.5f), false);
            RiverReaverCamp.CreateEnemy(root, player, "sunstone-cultist-4", "Ashen Shadowcaster", "Undead", "Mage", NavePos + new Vector3(5.0f, 0f, -4.0f), false, ranged: true);
            RiverReaverCamp.CreateEnemy(root, player, "sunstone-cultist-5", "Ashen Flagellant", "Human", "Warrior", NavePos + new Vector3(0.5f, 0f, -0.5f), false);

            // 3. Broken High Altar Boss Encounter (Step 2: Defeat Arch-Inquisitor Malrik, 420 HP)
            RiverReaverCamp.CreateEnemy(root, player, BossId, BossName, "Undead", "Mage", AltarPos, boss: true, ranged: true);
            var bossGo = root.Find(BossName);
            if (bossGo)
            {
                bossGo.localScale = Vector3.one * 1.25f;
                var stats = bossGo.GetComponent<Combatant>();
                if (stats) { stats.ConfigureVitals(420, 4); stats.Restore(); }
                var ctrl = bossGo.GetComponent<EnemyController>();
                if (ctrl) { ctrl.attackDamage = 19f; ctrl.attackInterval = 2.0f; ctrl.attackRange = 12f; }
            }
            RiverReaverCamp.CreateEnemy(root, player, "sunstone-guard-1", "Malrik's Acolyte", "Undead", "Warrior", AltarPos + new Vector3(-5.5f, 0f, 3.2f), false);
            RiverReaverCamp.CreateEnemy(root, player, "sunstone-guard-2", "Malrik's Acolyte", "Undead", "Warrior", AltarPos + new Vector3(5.5f, 0f, 3.2f), false);

            // Arcane Obelisk light at the high altar
            Vector3 altarG = Ground(AltarPos + new Vector3(0f, 0f, -4.5f));
            var crystal = Prop(root, "Malrik_ArcaneObelisk", altarG + Vector3.up * 2.4f, new Vector3(1.1f, 4.8f, 1.1f), new Color(0.65f, 0.22f, 0.95f), true, emissive: true);
            AddPointLight(crystal, new Color(0.65f, 0.22f, 0.95f), 18f, 2.8f);

            // Boss-locked Reliquary Chest
            var prefab = Resources.Load<GameObject>("Props/TreasureChest");
            if (prefab)
            {
                Vector3 chestPos = Ground(AltarPos + new Vector3(0f, 0f, -2.2f));
                var instance = Object.Instantiate(prefab, chestPos, Quaternion.identity, root);
                instance.name = "Malrik's Reliquary Chest";
                var chest = instance.GetComponent<LootChest>() ?? instance.AddComponent<LootChest>();
                chest.chestId = BossChestId;
                chest.gold = 105;
                chest.experience = 150;
            }
        }

        static GameObject Prop(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, bool collider, bool emissive = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (!collider)
            {
                var c = go.GetComponent<Collider>();
                if (c) { if (Application.isPlaying) Object.Destroy(c); else Object.DestroyImmediate(c); }
            }
            var r = go.GetComponent<MeshRenderer>();
            if (r)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (sh)
                {
                    var mat = new Material(sh) { color = color };
                    if (emissive && mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", color * 2.0f);
                    }
                    r.sharedMaterial = mat;
                }
            }
            return go;
        }

        static void AddPointLight(GameObject go, Color color, float range, float intensity)
        {
            if (!go || Application.isBatchMode) return;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
        }

        static Vector3 Ground(Vector3 xz)
        {
            var hits = Physics.RaycastAll(new Vector3(xz.x, 260f, xz.z), Vector3.down, 420f);
            float best = float.MinValue;
            bool found = false;
            foreach (var h in hits)
            {
                if (h.collider is TerrainCollider && h.point.y > best) { best = h.point.y; found = true; }
            }
            xz.y = (found ? best : WorldRegions.GroundHeight(xz)) + 0.03f;
            return xz;
        }
    }
}
