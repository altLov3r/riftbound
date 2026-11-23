using UnityEngine;

namespace Riftbound
{
    /// <summary>
    /// Seamless walk-in cavern ("Ashenmaw Cavern") with zero loading screen or teleport.
    /// Houses the Briarclaw Tribe (8 hostile NPCs) and Ursolak, Ancient Bear Guardian boss (480 HP),
    /// plus Warden Kaelen outside the cave mouth for a 2-step lore-accurate questline.
    /// Replaces the old teleporting BrassHollows cave.
    /// </summary>
    public static class AshenmawCavernZone
    {
        public const string RootName = "Ashenmaw Seamless Cavern";
        public const string QuestGiverName = "Warden Kaelen";
        public const string BossId = "ursolak-bear-guardian";
        public const string BossName = "Ursolak, Bear Guardian";
        public const string BossChestId = "ashenmaw-guardian-chest";

        public static readonly Vector3 EntrancePos = new Vector3(-115f, 0f, 444f);
        public static readonly Vector3 CampfirePos = new Vector3(-115f, 0f, 434f);
        public static readonly Vector3 SanctumPos = new Vector3(-115f, 0f, 504f);

        public static bool Inside(Vector3 point)
        {
            return point.x >= EntrancePos.x - 15f && point.x <= EntrancePos.x + 15f &&
                   point.z >= EntrancePos.z + 1f && point.z <= SanctumPos.z + 18f;
        }

        public static void Ensure()
        {
            if (!WorldRegions.Expanded || GameObject.Find(RootName)) return;

            var player = Object.FindFirstObjectByType<PlayerController>();
            var root = new GameObject(RootName).transform;

            float entranceY = SampleGround(EntrancePos);
            // Keep cavern floor slightly above the highest terrain point along the gallery so terrain never pokes through
            float floorY = entranceY + 0.15f;
            for (float z = EntrancePos.z; z <= SanctumPos.z + 14f; z += 4f)
            {
                float h = SampleGround(new Vector3(EntrancePos.x, 0f, z));
                if (h + 0.15f > floorY) floorY = h + 0.15f;
            }

            // 1. Outside Quest Camp (Warden Kaelen + Campfire + Atlas Waystop)
            BuildOutsideCamp(root, entranceY, floorY);

            // 2. Seamless Enclosed Cavern Geometry (Entrance Arch -> Outer Gallery -> Ritual Chamber -> Boss Sanctum)
            BuildSeamlessCavernShell(root, entranceY, floorY);

            // 3. Briarclaw Tribe (8 hostile NPCs across Outer Gallery, Ritual Chamber, and Sanctum Guard)
            SpawnTribeMember(root, player, "ashenmaw-tribe-1", "Briarclaw Ravager", "Orc", "Warrior", new Vector3(EntrancePos.x - 2.6f, floorY, EntrancePos.z + 14f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-2", "Briarclaw Bone-Thrower", "Orc", "Rogue", new Vector3(EntrancePos.x + 2.8f, floorY, EntrancePos.z + 18f), true);
            SpawnTribeMember(root, player, "ashenmaw-tribe-3", "Briarclaw Ravager", "Human", "Warrior", new Vector3(EntrancePos.x - 1.8f, floorY, EntrancePos.z + 24f), false);

            SpawnTribeMember(root, player, "ashenmaw-tribe-4", "Briarclaw Blood-Shaman", "Undead", "Mage", new Vector3(EntrancePos.x - 4.5f, floorY, EntrancePos.z + 34f), true);
            SpawnTribeMember(root, player, "ashenmaw-tribe-5", "Briarclaw Mauler", "Orc", "Warrior", new Vector3(EntrancePos.x + 4.2f, floorY, EntrancePos.z + 35f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-6", "Briarclaw Stalker", "Orc", "Rogue", new Vector3(EntrancePos.x + 0.5f, floorY, EntrancePos.z + 41f), false);

            SpawnTribeMember(root, player, "ashenmaw-tribe-7", "Briarclaw Totem-Guard", "Orc", "Warrior", new Vector3(EntrancePos.x - 5.2f, floorY, SanctumPos.z - 8f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-8", "Briarclaw Totem-Guard", "Orc", "Warrior", new Vector3(EntrancePos.x + 5.2f, floorY, SanctumPos.z - 8f), false);

            // 4. Ursolak, Ancient Bear Guardian Boss (480 HP, custom bear visual) + Boss-Locked Reliquary Chest
            SpawnBearGuardianBoss(root, player, new Vector3(SanctumPos.x, floorY, SanctumPos.z + 2f));

            var prefab = Resources.Load<GameObject>("Props/TreasureChest");
            if (prefab)
            {
                Vector3 chestPos = new Vector3(SanctumPos.x, floorY + 0.05f, SanctumPos.z + 9.5f);
                var instance = Object.Instantiate(prefab, chestPos, Quaternion.Euler(0f, 180f, 0f), root);
                instance.name = "Ursolak's Spirit Reliquary";
                var chest = instance.GetComponent<LootChest>() ?? instance.AddComponent<LootChest>();
                chest.chestId = BossChestId;
                chest.gold = 120;
                chest.experience = 180;
            }
        }

        static void BuildOutsideCamp(Transform root, float entranceY, float floorY)
        {
            Vector3 campPos = new Vector3(CampfirePos.x, SampleGround(CampfirePos), CampfirePos.z);

            // Campfire + warm glow
            Block(root, "Kaelen_FireRing", campPos + Vector3.up * 0.12f, new Vector3(1.5f, 0.28f, 1.5f), new Color(0.24f, 0.24f, 0.25f), false);
            Block(root, "Kaelen_Embers", campPos + Vector3.up * 0.30f, new Vector3(0.75f, 0.26f, 0.75f), new Color(0.98f, 0.45f, 0.12f), false, emissive: true);

            // Warden Kaelen (Quest Giver for the 2-step Ashenmaw Cavern questline)
            Vector3 npcPos = new Vector3(CampfirePos.x + 2.4f, SampleGround(CampfirePos + new Vector3(2.4f, 0f, 0.5f)), CampfirePos.z + 0.5f);
            var npcGo = new GameObject(QuestGiverName);
            npcGo.transform.SetParent(root, false);
            npcGo.transform.position = npcPos;
            npcGo.transform.rotation = Quaternion.Euler(0f, 195f, 0f);
            CharacterVisualFactory.CreateVisualInstance("Elf", "Warrior", npcGo.transform);
            var col = npcGo.AddComponent<CapsuleCollider>();
            col.height = 1.9f;
            col.radius = 0.36f;
            col.center = Vector3.up * 0.95f;
            var npc = npcGo.AddComponent<TownNPC>();
            npc.npcName = QuestGiverName;
            npc.roleTag = "Cavern Warden";
            npc.greeting = "The Briarclaw Tribe broke into Ashenmaw Cavern and roused Ursolak, the Ancient Bear Guardian. Walk straight through the cave mouth—no portal stands between you and the deep sanctum.";

            // Atlas Waystop so players can find it immediately via M -> Places -> Ashenmaw Cavern
            var pin = new GameObject("Ashenmaw Cavern map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root, false);
            pin.transform.position = campPos + new Vector3(-2.5f, 0f, 1.0f);
            pin.displayName = "Ashenmaw Cavern";
            pin.notice = "Ashenmaw Cavern — Seamless subterranean den of the Briarclaw Tribe and Ursolak, Ancient Bear Guardian.";

            // Smooth stone entry ramp connecting outside terrain seamlessly up to the cavern floor
            Vector3 rampStart = new Vector3(EntrancePos.x, entranceY - 0.05f, EntrancePos.z - 5.5f);
            Vector3 rampEnd = new Vector3(EntrancePos.x, floorY, EntrancePos.z + 2.5f);
            Vector3 rise = rampEnd - rampStart;
            var ramp = Block(root, "Cavern_EntryRamp", (rampStart + rampEnd) * 0.5f - Vector3.up * 0.25f,
                new Vector3(9.5f, 0.50f, rise.magnitude + 1.0f), new Color(0.22f, 0.23f, 0.25f), true);
            ramp.transform.rotation = Quaternion.LookRotation(rise.normalized, Vector3.up);
        }

        static void BuildSeamlessCavernShell(Transform root, float entranceY, float floorY)
        {
            var shell = new GameObject("Cavern Architecture").transform;
            shell.SetParent(root, false);

            Color rockDark = new Color(0.15f, 0.16f, 0.18f);
            Color rockMid = new Color(0.21f, 0.22f, 0.25f);
            Color floorCol = new Color(0.19f, 0.20f, 0.22f);
            Color jadeGlow = new Color(0.18f, 0.88f, 0.62f);
            Color amberGlow = new Color(0.95f, 0.50f, 0.14f);

            // Continuous walkable stone floor from Z = 445 to Z = 518
            float totalLen = (SanctumPos.z + 15f) - EntrancePos.z;
            float midZ = EntrancePos.z + totalLen * 0.5f;
            Block(shell, "CavernFloor_Main", new Vector3(EntrancePos.x, floorY - 0.5f, midZ), new Vector3(26f, 1.0f, totalLen + 4f), floorCol, true);

            // Monumental Outer Mountain Rock Arch at Entrance (Z = 444)
            Block(shell, "Mouth_LeftCliff", new Vector3(EntrancePos.x - 8.0f, floorY + 2.5f, EntrancePos.z), new Vector3(7.5f, 13f, 8.5f), rockDark, true);
            Block(shell, "Mouth_RightCliff", new Vector3(EntrancePos.x + 8.0f, floorY + 2.5f, EntrancePos.z), new Vector3(7.5f, 13f, 8.5f), rockDark, true);
            Block(shell, "Mouth_Brow", new Vector3(EntrancePos.x, floorY + 8.5f, EntrancePos.z + 0.5f), new Vector3(21f, 5.5f, 9.5f), rockDark, true);
            // Entrance braziers
            AddBrazier(shell, new Vector3(EntrancePos.x - 4.8f, floorY, EntrancePos.z - 1.5f), amberGlow);
            AddBrazier(shell, new Vector3(EntrancePos.x + 4.8f, floorY, EntrancePos.z - 1.5f), amberGlow);

            // Vaulted Cavern Segments from Z = 448 to Z = 516
            for (int i = 0; i < 12; i++)
            {
                float z = EntrancePos.z + 5f + i * 6.0f;
                bool isRitual = z >= EntrancePos.z + 30f && z <= EntrancePos.z + 44f;
                bool isSanctum = z >= SanctumPos.z - 10f;
                float halfW = isSanctum ? 12.0f : (isRitual ? 8.5f : 5.8f);
                float ceilH = isSanctum ? 10.5f : (isRitual ? 8.0f : 6.8f);

                // Side cave walls (sunk deep below floorY so zero gaps)
                Block(shell, $"WallL_{i}", new Vector3(EntrancePos.x - halfW - 2.2f, floorY + ceilH * 0.45f - 1.0f, z),
                    new Vector3(4.6f, ceilH + 4.5f, 7.2f), (i % 2 == 0) ? rockDark : rockMid, true);
                Block(shell, $"WallR_{i}", new Vector3(EntrancePos.x + halfW + 2.2f, floorY + ceilH * 0.45f - 1.0f, z),
                    new Vector3(4.6f, ceilH + 4.5f, 7.2f), (i % 2 == 0) ? rockMid : rockDark, true);

                // Vaulted ceiling slab + overhanging stalactites
                Block(shell, $"Ceiling_{i}", new Vector3(EntrancePos.x, floorY + ceilH + 1.4f, z),
                    new Vector3(halfW * 2f + 9.5f, 3.2f, 7.4f), rockDark, true);

                if (i % 2 == 1)
                {
                    Block(shell, $"StalactiteA_{i}", new Vector3(EntrancePos.x - halfW * 0.45f, floorY + ceilH - 1.1f, z),
                        new Vector3(0.9f, 2.2f, 0.9f), rockMid, false);
                    Block(shell, $"StalactiteB_{i}", new Vector3(EntrancePos.x + halfW * 0.50f, floorY + ceilH - 0.9f, z + 1.2f),
                        new Vector3(0.75f, 1.8f, 0.75f), rockMid, false);
                }

                // Glowing spirit crystals & braziers along the cavern walls
                if (i % 2 == 0)
                {
                    AddCrystalCluster(shell, new Vector3(EntrancePos.x - halfW + 0.6f, floorY, z), isSanctum ? jadeGlow : amberGlow);
                    AddCrystalCluster(shell, new Vector3(EntrancePos.x + halfW - 0.6f, floorY, z + 1.5f), isSanctum ? jadeGlow : amberGlow);
                }
            }

            // Back Sanctum Wall sealing the end of the cave (Z = 518)
            Block(shell, "Sanctum_BackWall", new Vector3(EntrancePos.x, floorY + 5.0f, SanctumPos.z + 15.5f),
                new Vector3(30f, 14f, 5.5f), rockDark, true);

            // Ritual Hearth Fire in mid-chamber
            Vector3 hearth = new Vector3(EntrancePos.x, floorY, EntrancePos.z + 36f);
            Block(shell, "RitualHearth_Base", hearth + Vector3.up * 0.16f, new Vector3(2.4f, 0.34f, 2.4f), rockMid, false);
            Block(shell, "RitualHearth_Flame", hearth + Vector3.up * 0.48f, new Vector3(1.2f, 0.55f, 1.2f), amberGlow, false, emissive: true);

            // Ancient Ursol Spirit Altar behind Ursolak
            Vector3 altar = new Vector3(SanctumPos.x, floorY, SanctumPos.z + 11.5f);
            Block(shell, "UrsolAltar_Dais", altar + Vector3.up * 0.35f, new Vector3(8.5f, 0.75f, 4.5f), rockMid, true);
            Block(shell, "UrsolAltar_Monolith", altar + new Vector3(0f, 3.2f, 1.1f), new Vector3(3.4f, 5.6f, 1.2f), rockDark, true);
            Block(shell, "UrsolAltar_RuneCore", altar + new Vector3(0f, 3.6f, 0.45f), new Vector3(1.3f, 2.4f, 0.25f), jadeGlow, false, emissive: true);
            AddBrazier(shell, altar + new Vector3(-4.2f, 0.7f, -0.5f), jadeGlow);
            AddBrazier(shell, altar + new Vector3(4.2f, 0.7f, -0.5f), jadeGlow);
        }

        static void SpawnTribeMember(Transform parent, PlayerController player, string id, string name, string race, string cls, Vector3 pos, bool ranged)
        {
            RiverReaverCamp.CreateEnemy(parent, player, id, name, race, cls, pos, boss: false, ranged: ranged);
            var go = parent.Find(name);
            // Force exact cavern floor Y (overrides Terrain height)
            foreach (var ec in parent.GetComponentsInChildren<EnemyController>(true))
            {
                if (ec.enemyId != id) continue;
                ec.transform.position = pos + Vector3.up * 0.05f;
                var stats = ec.GetComponent<Combatant>();
                if (stats) { stats.ConfigureVitals(110, 1); stats.Restore(); }
                break;
            }
        }

        static void SpawnBearGuardianBoss(Transform parent, PlayerController player, Vector3 pos)
        {
            var root = new GameObject(BossName);
            root.SetActive(false);
            root.transform.SetParent(parent, false);
            root.transform.position = pos + Vector3.up * 0.05f;
            root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            // Sculpted Ancient Bear Guardian visual (unmistakable massive bear silhouette with glowing jade spirit-runes)
            BuildBearGuardianVisual(root.transform);

            var enemy = root.AddComponent<EnemyController>();
            enemy.enemyId = BossId;
            enemy.player = player;
            enemy.isBoss = true;
            enemy.heavyAttacker = true;
            enemy.campGuard = true;
            enemy.attackDamage = 20f;
            enemy.attackInterval = 2.0f;
            enemy.attackRange = 3.2f;
            enemy.movementMultiplier = 0.75f;

            var motor = root.GetComponent<CharacterController>();
            motor.height = 2.6f;
            motor.radius = 1.05f;
            motor.center = Vector3.up * 1.3f;
            motor.slopeLimit = 45f;
            motor.stepOffset = 0.35f;

            var stats = root.GetComponent<Combatant>();
            stats.displayName = BossName;
            stats.ConfigureVitals(480, 4);
            stats.Restore();

            root.SetActive(!player || !Application.isPlaying || !player.HasDefeated(BossId));
        }

        static void BuildBearGuardianVisual(Transform parent)
        {
            var vis = new GameObject("BearGuardianVisual").transform;
            vis.SetParent(parent, false);

            Color furDark = new Color(0.18f, 0.14f, 0.11f);
            Color furLight = new Color(0.28f, 0.22f, 0.17f);
            Color clawCol = new Color(0.75f, 0.72f, 0.65f);
            Color runeJade = new Color(0.20f, 0.95f, 0.65f);
            Color ironCol = new Color(0.25f, 0.26f, 0.28f);

            // Broad bear torso & shoulder hump
            Block(vis, "Bear_Body", new Vector3(0f, 1.35f, 0f), new Vector3(1.65f, 1.35f, 2.55f), furDark, false);
            Block(vis, "Bear_ShoulderHump", new Vector3(0f, 1.95f, 0.55f), new Vector3(1.55f, 0.85f, 1.35f), furLight, false);

            // Massive head, snout & ears
            Block(vis, "Bear_Head", new Vector3(0f, 1.85f, 1.55f), new Vector3(1.05f, 0.95f, 1.05f), furDark, false);
            Block(vis, "Bear_Snout", new Vector3(0f, 1.65f, 2.15f), new Vector3(0.58f, 0.52f, 0.62f), furLight, false);
            Block(vis, "Bear_EarL", new Vector3(-0.42f, 2.38f, 1.45f), new Vector3(0.24f, 0.28f, 0.22f), furDark, false);
            Block(vis, "Bear_EarR", new Vector3(0.42f, 2.38f, 1.45f), new Vector3(0.24f, 0.28f, 0.22f), furDark, false);

            // Glowing spirit eyes & ancient jade spirit-rune spine markings
            Block(vis, "Bear_EyeL", new Vector3(-0.28f, 1.98f, 2.05f), new Vector3(0.16f, 0.12f, 0.12f), runeJade, false, emissive: true);
            Block(vis, "Bear_EyeR", new Vector3(0.28f, 1.98f, 2.05f), new Vector3(0.16f, 0.12f, 0.12f), runeJade, false, emissive: true);
            Block(vis, "Bear_SpiritCollar", new Vector3(0f, 1.82f, 1.05f), new Vector3(1.42f, 0.32f, 0.36f), ironCol, false);
            Block(vis, "Bear_SpiritRuneBack", new Vector3(0f, 2.40f, 0.45f), new Vector3(0.45f, 0.14f, 1.25f), runeJade, false, emissive: true);

            // Four heavy bear legs + paws/claws
            Vector3[] legOffsets =
            {
                new Vector3(-0.62f, 0.55f, 0.85f),
                new Vector3(0.62f, 0.55f, 0.85f),
                new Vector3(-0.62f, 0.55f, -0.85f),
                new Vector3(0.62f, 0.55f, -0.85f)
            };
            for (int i = 0; i < legOffsets.Length; i++)
            {
                Block(vis, $"Bear_Leg_{i}", legOffsets[i], new Vector3(0.48f, 1.15f, 0.52f), furDark, false);
                Block(vis, $"Bear_Paw_{i}", new Vector3(legOffsets[i].x, 0.12f, legOffsets[i].z + 0.18f), new Vector3(0.50f, 0.22f, 0.45f), clawCol, false);
            }
        }

        static void AddBrazier(Transform parent, Vector3 pos, Color flameColor)
        {
            Block(parent, "Brazier_Stand", pos + Vector3.up * 0.65f, new Vector3(0.75f, 1.3f, 0.75f), new Color(0.22f, 0.23f, 0.25f), true);
            Block(parent, "Brazier_Fire", pos + Vector3.up * 1.48f, new Vector3(0.55f, 0.50f, 0.55f), flameColor, false, emissive: true);
        }

        static void AddCrystalCluster(Transform parent, Vector3 pos, Color crystalColor)
        {
            var c1 = Block(parent, "SpiritCrystalA", pos + Vector3.up * 0.85f, new Vector3(0.42f, 1.7f, 0.42f), crystalColor, false, emissive: true);
            c1.transform.rotation = Quaternion.Euler(10f, 15f, -12f);
            var c2 = Block(parent, "SpiritCrystalB", pos + new Vector3(0.35f, 0.55f, 0.22f), new Vector3(0.30f, 1.1f, 0.30f), crystalColor, false, emissive: true);
            c2.transform.rotation = Quaternion.Euler(-12f, -20f, 16f);
        }

        static GameObject Block(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, bool collider, bool emissive = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (!collider)
            {
                var c = go.GetComponent<Collider>();
                if (c)
                {
                    if (Application.isPlaying) Object.Destroy(c);
                    else Object.DestroyImmediate(c);
                }
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
                        mat.SetColor("_EmissionColor", color * 2.1f);
                    }
                    r.sharedMaterial = mat;
                }
            }
            return go;
        }

        static float SampleGround(Vector3 xz)
        {
            var hits = Physics.RaycastAll(new Vector3(xz.x, 260f, xz.z), Vector3.down, 420f);
            float best = float.MinValue;
            bool found = false;
            foreach (var h in hits)
            {
                if (h.collider is TerrainCollider && h.point.y > best)
                {
                    best = h.point.y;
                    found = true;
                }
            }
            return found ? best : WorldRegions.GroundHeight(xz);
        }
    }
}
