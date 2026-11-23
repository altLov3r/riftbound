using UnityEngine;

namespace Riftbound
{
    /// <summary>
    /// Far-East outlaw quarry stronghold ("Ironmaw Quarry") at (385, 90) with Mine Overseer Rusk's
    /// bounty quest, 4 Ironmaw Outlaws, boss Ironjaw Krag (450 HP), and a boss-locked Ore Vault chest.
    /// Works in both offline solo and headless dedicated server.
    /// </summary>
    public static class IronmawQuarryZone
    {
        public const string RootName = "Ironmaw Quarry Stronghold";
        public const string QuestGiverName = "Overseer Rusk";
        public const string BossId = "ironmaw-quarry-boss";
        public const string BossName = "Ironjaw Krag";
        public const string BossChestId = "ironmaw-vault-chest";

        public static readonly Vector3 CampPos = new Vector3(338f, 0f, 90f);
        public static readonly Vector3 QuarryPos = new Vector3(390f, 0f, 90f);

        public static void Ensure()
        {
            if (!WorldRegions.Expanded || GameObject.Find(RootName)) return;

            var player = Object.FindFirstObjectByType<PlayerController>();
            var root = new GameObject(RootName).transform;

            // Safe overlook camp west of the quarry (Overseer Rusk + campfire + Atlas Waystop)
            Vector3 campG = Ground(CampPos);
            Prop(root, "Rusk_FirePit", campG + Vector3.up * 0.14f, new Vector3(1.4f, 0.28f, 1.4f), new Color(0.28f, 0.26f, 0.25f), false);
            var flame = Prop(root, "Rusk_Flame", campG + Vector3.up * 0.34f, new Vector3(0.7f, 0.28f, 0.7f), new Color(0.98f, 0.50f, 0.12f), false, emissive: true);
            if (!Application.isBatchMode)
            {
                var light = flame.AddComponent<Light>();
                light.type = LightType.Point; light.color = new Color(0.98f, 0.50f, 0.12f); light.range = 14f; light.intensity = 2.2f;
            }

            var npcGo = new GameObject(QuestGiverName);
            npcGo.transform.SetParent(root, false);
            npcGo.transform.position = Ground(CampPos + new Vector3(2.0f, 0f, 1.2f));
            npcGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            CharacterVisualFactory.CreateVisualInstance("Dwarf", "Warrior", npcGo.transform);
            var col = npcGo.AddComponent<CapsuleCollider>();
            col.height = 1.65f; col.radius = 0.36f; col.center = Vector3.up * 0.82f;
            var npc = npcGo.AddComponent<TownNPC>();
            npc.npcName = QuestGiverName;
            npc.roleTag = "Ironmaw Overseer";
            npc.greeting = "Ironjaw Krag and his outlaws seized the eastern pit and locked our ore vault. Break his hold on Ironmaw Quarry and I'll pay you handsomely!";

            var pin = new GameObject("Ironmaw Quarry map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root, false);
            pin.transform.position = Ground(CampPos + new Vector3(-2.2f, 0f, 0.8f));
            pin.displayName = "Ironmaw Quarry";
            pin.notice = "Ironmaw Quarry — Far-east iron mine seized by Ironjaw Krag's outlaws. Speak with Overseer Rusk at the western ridge.";

            // 4 Ironmaw Outlaws guarding the quarry pit
            RiverReaverCamp.CreateEnemy(root, player, "ironmaw-outlaw-1", "Ironmaw Cutthroat", "Orc", "Rogue", QuarryPos + new Vector3(-8f, 0f, -5f), false);
            RiverReaverCamp.CreateEnemy(root, player, "ironmaw-outlaw-2", "Ironmaw Sapper", "Human", "Rogue", QuarryPos + new Vector3(-7f, 0f, 6f), false, ranged: true);
            RiverReaverCamp.CreateEnemy(root, player, "ironmaw-outlaw-3", "Ironmaw Enforcer", "Orc", "Warrior", QuarryPos + new Vector3(3f, 0f, -7f), false);
            RiverReaverCamp.CreateEnemy(root, player, "ironmaw-outlaw-4", "Ironmaw Enforcer", "Orc", "Warrior", QuarryPos + new Vector3(4f, 0f, 6.5f), false);

            // Boss: Ironjaw Krag (450 HP)
            RiverReaverCamp.CreateEnemy(root, player, BossId, BossName, "Orc", "Warrior", QuarryPos + new Vector3(4f, 0f, 0f), boss: true);
            var bossGo = root.Find(BossName);
            if (bossGo)
            {
                bossGo.localScale = Vector3.one * 1.28f;
                var stats = bossGo.GetComponent<Combatant>();
                if (stats) { stats.ConfigureVitals(450, 4); stats.Restore(); }
                var ctrl = bossGo.GetComponent<EnemyController>();
                if (ctrl) { ctrl.attackDamage = 20f; ctrl.attackInterval = 2.1f; }
            }

            // Boss-locked Ore Vault Chest
            var prefab = Resources.Load<GameObject>("Props/TreasureChest");
            if (prefab)
            {
                Vector3 chestPos = Ground(QuarryPos + new Vector3(9.5f, 0f, 0f));
                var instance = Object.Instantiate(prefab, chestPos, Quaternion.Euler(0f, -90f, 0f), root);
                instance.name = "Ironmaw Ore Vault";
                var chest = instance.GetComponent<LootChest>() ?? instance.AddComponent<LootChest>();
                chest.chestId = BossChestId;
                chest.gold = 130;
                chest.experience = 175;
            }
        }

        static GameObject Prop(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, bool collider, bool emissive = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent, false); go.transform.position = pos; go.transform.localScale = scale;
            if (!collider) { var c = go.GetComponent<Collider>(); if (c) { if (Application.isPlaying) Object.Destroy(c); else Object.DestroyImmediate(c); } }
            var r = go.GetComponent<MeshRenderer>();
            if (r)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (sh)
                {
                    var mat = new Material(sh) { color = color };
                    if (emissive && mat.HasProperty("_EmissionColor")) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", color * 2.0f); }
                    r.sharedMaterial = mat;
                }
            }
            return go;
        }

        static Vector3 Ground(Vector3 xz)
        {
            var hits = Physics.RaycastAll(new Vector3(xz.x, 260f, xz.z), Vector3.down, 420f);
            float best = float.MinValue; bool found = false;
            foreach (var h in hits) if (h.collider is TerrainCollider && h.point.y > best) { best = h.point.y; found = true; }
            xz.y = (found ? best : WorldRegions.GroundHeight(xz)) + 0.03f;
            return xz;
        }
    }
}
