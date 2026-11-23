using UnityEngine;

namespace Riftbound
{
    /// <summary>
    /// Hostile woods quest zone ("Timberfang Woods") west-northwest of Pinewatch Relay.
    /// Houses a 3-step questline from Watchkeeper Halen culminating in Warchief Grimgnaw
    /// and a boss-locked spoils chest. Safe in both offline solo and headless dedicated server.
    /// </summary>
    public static class PinewatchWoodsZone
    {
        public const string RootName = "Pinewatch Timberfang Woods";
        public const string BossId = "pinewatch-woods-boss";
        public const string BossName = "Warchief Grimgnaw";
        public const string BossChestId = "pinewatch-boss-chest";

        public static readonly Vector3 Camp1Position = new Vector3(-248f, 0f, 410f);
        public static readonly Vector3 Camp2Position = new Vector3(-268f, 0f, 430f);
        public static readonly Vector3 BossPosition = new Vector3(-286f, 0f, 450f);

        public static void Ensure()
        {
            if (!WorldRegions.Expanded || GameObject.Find(RootName)) return;

            var player = Object.FindFirstObjectByType<PlayerController>();
            var root = new GameObject(RootName).transform;

            // --- Camp 1: Logging Road Ambush (Step 1: Defeat 4 Timberfang Raiders) ---
            BuildCampProps(root, "Timberfang Ambush Camp", Camp1Position, isBossCamp: false);
            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-raider-1", "Timberfang Cutthroat", "Human", "Rogue", Camp1Position + new Vector3(-4.5f, 0f, -2.5f), false);
            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-raider-2", "Timberfang Cutthroat", "Orc", "Warrior", Camp1Position + new Vector3(4.2f, 0f, -3.0f), false);
            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-raider-3", "Timberfang Bowman", "Human", "Rogue", Camp1Position + new Vector3(-2.0f, 0f, 5.0f), false, ranged: true);
            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-raider-4", "Timberfang Skirmisher", "Orc", "Rogue", Camp1Position + new Vector3(5.2f, 0f, 3.8f), false);

            // --- Camp 2: Poacher Glade (Step 2: Defeat 3 Timberfang Poachers) ---
            BuildCampProps(root, "Timberfang Poacher Glade", Camp2Position, isBossCamp: false);
            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-poacher-1", "Timberfang Poacher", "Human", "Warrior", Camp2Position + new Vector3(-5.0f, 0f, -2.0f), false);
            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-poacher-2", "Timberfang Poacher", "Orc", "Rogue", Camp2Position + new Vector3(4.8f, 0f, -2.8f), false);
            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-poacher-3", "Timberfang Hexer", "Undead", "Mage", Camp2Position + new Vector3(0.5f, 0f, 5.2f), false, ranged: true);

            // --- Camp 3: Briarhollow Boss Den (Step 3: Defeat Warchief Grimgnaw) ---
            BuildCampProps(root, "Briarhollow Boss Den", BossPosition, isBossCamp: true);
            RiverReaverCamp.CreateEnemy(root, player, BossId, BossName, "Orc", "Warrior", BossPosition, boss: true);
            var bossGo = root.Find(BossName);
            if (bossGo)
            {
                bossGo.localScale = Vector3.one * 1.24f;
                var stats = bossGo.GetComponent<Combatant>();
                if (stats)
                {
                    stats.ConfigureVitals(340, 3);
                    stats.Restore();
                }
                var ctrl = bossGo.GetComponent<EnemyController>();
                if (ctrl)
                {
                    ctrl.attackDamage = 18f;
                    ctrl.attackInterval = 2.15f;
                }
            }

            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-boss-guard-1", "Briarhollow Bruiser", "Orc", "Warrior", BossPosition + new Vector3(-6.2f, 0f, -3.5f), false);
            RiverReaverCamp.CreateEnemy(root, player, "pinewatch-boss-guard-2", "Briarhollow Stalker", "Human", "Rogue", BossPosition + new Vector3(6.2f, 0f, -3.5f), false);

            // Boss-locked spoils chest
            var prefab = Resources.Load<GameObject>("Props/TreasureChest");
            if (prefab)
            {
                Vector3 chestPos = Ground(BossPosition + new Vector3(0f, 0f, 4.8f));
                var instance = Object.Instantiate(prefab, chestPos, Quaternion.Euler(0f, 180f, 0f), root);
                instance.name = "Grimgnaw's Spoils Chest";
                var chest = instance.GetComponent<LootChest>() ?? instance.AddComponent<LootChest>();
                chest.chestId = BossChestId;
                chest.gold = 85;
                chest.experience = 120;
            }
        }

        static void BuildCampProps(Transform parent, string campName, Vector3 center, bool isBossCamp)
        {
            var camp = new GameObject(campName).transform;
            camp.SetParent(parent, false);
            Vector3 gCenter = Ground(center);

            // Campfire stone ring + ember core (non-colliding so combat stays smooth)
            CreateProp(camp, "FirePit", gCenter + Vector3.up * 0.14f, new Vector3(1.4f, 0.28f, 1.4f), new Color(0.28f, 0.26f, 0.25f), collider: false);
            CreateProp(camp, "Embers", gCenter + Vector3.up * 0.30f, new Vector3(0.75f, 0.22f, 0.75f), new Color(0.95f, 0.42f, 0.10f), collider: false, emissive: true);

            // Timber crates & log barricades on camp flanks
            CreateProp(camp, "StolenCrateA", Ground(center + new Vector3(-3.8f, 0f, 3.2f)) + Vector3.up * 0.45f, new Vector3(1.1f, 0.9f, 1.1f), new Color(0.42f, 0.28f, 0.16f), collider: true);
            CreateProp(camp, "StolenCrateB", Ground(center + new Vector3(3.9f, 0f, 2.8f)) + Vector3.up * 0.40f, new Vector3(1.0f, 0.8f, 1.3f), new Color(0.38f, 0.25f, 0.15f), collider: true);

            if (isBossCamp)
            {
                // War-banner totem poles framing Briarhollow Boss Den
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector3 postPos = Ground(center + new Vector3(i * 5.8f, 0f, 3.8f));
                    CreateProp(camp, "BossTotemPost", postPos + Vector3.up * 2.1f, new Vector3(0.30f, 4.2f, 0.30f), new Color(0.26f, 0.18f, 0.12f), collider: true);
                    CreateProp(camp, "BossBanner", postPos + new Vector3(0f, 3.1f, -0.25f), new Vector3(1.1f, 1.5f, 0.12f), new Color(0.62f, 0.14f, 0.12f), collider: false);
                }
                // Low timber palisade backstop behind the spoils chest
                Vector3 backPos = Ground(center + new Vector3(0f, 0f, 7.6f));
                CreateProp(camp, "DenBackstop", backPos + Vector3.up * 1.15f, new Vector3(7.5f, 2.3f, 0.55f), new Color(0.30f, 0.20f, 0.13f), collider: true);
            }
        }

        static void CreateProp(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, bool collider, bool emissive = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (!collider)
            {
                var c = go.GetComponent<Collider>();
                if (c) Object.Destroy(c);
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
                        mat.SetColor("_EmissionColor", color * 1.8f);
                    }
                    r.sharedMaterial = mat;
                }
            }
        }

        static Vector3 Ground(Vector3 point)
        {
            point.y = WorldRegions.GroundHeight(point) + 0.03f;
            return point;
        }
    }
}
