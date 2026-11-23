using UnityEngine;

namespace Riftbound
{
    /// <summary>
    /// Seamless subterranean cavern ("Ashenmaw Cavern") carved directly INTO the terrain hillside
    /// via an in-memory TerrainData.SetHoles cutout (never modifying authored terrain assets on disk).
    /// Loads sculpted Blender FBX meshes for both the organic rock cavern (Ashenmaw_CavernShell.fbx)
    /// and Ursolak, Ancient Bear Guardian (Ursolak_BearGuardian.fbx). Zero loading screen, zero teleport.
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
            return point.x >= EntrancePos.x - 16f && point.x <= EntrancePos.x + 16f &&
                   point.z >= EntrancePos.z + 1f && point.z <= SanctumPos.z + 18f;
        }

        public static void Ensure()
        {
            if (!WorldRegions.Expanded || GameObject.Find(RootName)) return;

            var player = Object.FindFirstObjectByType<PlayerController>();
            var root = new GameObject(RootName).transform;

            float entranceY = SampleGround(EntrancePos);
            float undergroundFloorY = entranceY - 7.5f;

            // 1. Carve a REAL hole into an in-memory clone of the terrain tile at the cave mouth
            CarveTerrainEntranceHole(EntrancePos, halfWidthMeters: 5.2f, startZOffset: 0.5f, lengthMeters: 16.5f);

            // 2. Instantiate sculpted organic Blender cavern shell descending underneath the terrain
            BuildOrganicCavernFromFbx(root, entranceY, undergroundFloorY);

            // 3. Outside Quest Camp (Warden Kaelen + Campfire + Atlas Waystop)
            BuildOutsideCamp(root, entranceY);

            // 4. Briarclaw Tribe (8 hostile NPCs inside the descending tunnel, ritual chamber & sanctum)
            SpawnTribeMember(root, player, "ashenmaw-tribe-1", "Briarclaw Ravager", "Orc", "Warrior", new Vector3(EntrancePos.x + 1.2f, entranceY - 4.8f, EntrancePos.z + 15.5f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-2", "Briarclaw Bone-Thrower", "Orc", "Rogue", new Vector3(EntrancePos.x + 2.2f, entranceY - 6.8f, EntrancePos.z + 21.5f), true);
            SpawnTribeMember(root, player, "ashenmaw-tribe-3", "Briarclaw Ravager", "Human", "Warrior", new Vector3(EntrancePos.x - 2.4f, undergroundFloorY, EntrancePos.z + 29.0f), false);

            SpawnTribeMember(root, player, "ashenmaw-tribe-4", "Briarclaw Blood-Shaman", "Undead", "Mage", new Vector3(EntrancePos.x - 4.2f, undergroundFloorY, EntrancePos.z + 34.5f), true);
            SpawnTribeMember(root, player, "ashenmaw-tribe-5", "Briarclaw Mauler", "Orc", "Warrior", new Vector3(EntrancePos.x + 3.6f, undergroundFloorY, EntrancePos.z + 36.0f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-6", "Briarclaw Stalker", "Orc", "Rogue", new Vector3(EntrancePos.x - 0.5f, undergroundFloorY, EntrancePos.z + 43.0f), false);

            SpawnTribeMember(root, player, "ashenmaw-tribe-7", "Briarclaw Totem-Guard", "Orc", "Warrior", new Vector3(EntrancePos.x - 5.2f, undergroundFloorY, SanctumPos.z - 8f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-8", "Briarclaw Totem-Guard", "Orc", "Warrior", new Vector3(EntrancePos.x + 5.2f, undergroundFloorY, SanctumPos.z - 8f), false);

            // 5. Ursolak, Ancient Bear Guardian Boss (480 HP, real sculpted Blender FBX model)
            SpawnBearGuardianBoss(root, player, new Vector3(SanctumPos.x, undergroundFloorY, SanctumPos.z + 2f));

            // 6. Boss-Locked Reliquary Chest at the back of the Sanctum
            var prefab = Resources.Load<GameObject>("Props/TreasureChest");
            if (prefab)
            {
                Vector3 chestPos = new Vector3(SanctumPos.x, undergroundFloorY + 0.08f, SanctumPos.z + 10.0f);
                var instance = Object.Instantiate(prefab, chestPos, Quaternion.Euler(0f, 180f, 0f), root);
                instance.name = "Ursolak's Spirit Reliquary";
                var chest = instance.GetComponent<LootChest>() ?? instance.AddComponent<LootChest>();
                chest.chestId = BossChestId;
                chest.gold = 120;
                chest.experience = 180;
            }
        }

        static void CarveTerrainEntranceHole(Vector3 mouthXZ, float halfWidthMeters, float startZOffset, float lengthMeters)
        {
            if (!Application.isPlaying) return; // Never mutate authored terrain in EditMode
            var tile = WorldRegions.TerrainAt(mouthXZ, Terrain.activeTerrains);
            if (!tile || !tile.terrainData) return;

            // Clone TerrainData in memory so authored .asset files stay 100% untouched
            var cloned = Object.Instantiate(tile.terrainData);
            tile.terrainData = cloned;
            var tc = tile.GetComponent<TerrainCollider>();
            if (tc) tc.terrainData = cloned;

            Vector3 origin = tile.transform.position;
            Vector3 size = cloned.size;
            int res = cloned.holesResolution;

            int minX = Mathf.Clamp(Mathf.FloorToInt(((mouthXZ.x - halfWidthMeters) - origin.x) / size.x * res), 0, res - 1);
            int maxX = Mathf.Clamp(Mathf.CeilToInt(((mouthXZ.x + halfWidthMeters) - origin.x) / size.x * res), 0, res - 1);
            int minZ = Mathf.Clamp(Mathf.FloorToInt(((mouthXZ.z + startZOffset) - origin.z) / size.z * res), 0, res - 1);
            int maxZ = Mathf.Clamp(Mathf.CeilToInt(((mouthXZ.z + startZOffset + lengthMeters) - origin.z) / size.z * res), 0, res - 1);

            int w = Mathf.Max(1, maxX - minX + 1);
            int h = Mathf.Max(1, maxZ - minZ + 1);
            bool[,] holes = new bool[h, w]; // false = carved hole!
            cloned.SetHoles(minX, minZ, holes);
        }

        static void BuildOrganicCavernFromFbx(Transform root, float entranceY, float undergroundFloorY)
        {
            var fbx = Resources.Load<GameObject>("Ashenmaw/Ashenmaw_CavernShell");
            if (fbx)
            {
                var shell = Object.Instantiate(fbx, new Vector3(EntrancePos.x, entranceY, EntrancePos.z), Quaternion.identity, root);
                shell.name = "Ashenmaw_OrganicCavern";
                foreach (var mf in shell.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (!mf.sharedMesh) continue;
                    var mc = mf.gameObject.GetComponent<MeshCollider>();
                    if (!mc) mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
                ApplyCavernMaterials(shell);
            }

            // Atmospheric point lighting inside the subterranean cavern (skipped in headless dedicated server)
            if (!Application.isBatchMode)
            {
                AddCaveLight(root, "CaveLight_Mouth", new Vector3(EntrancePos.x, entranceY + 2.6f, EntrancePos.z + 2f), new Color(1.0f, 0.56f, 0.20f), 18f, 2.8f);
                AddCaveLight(root, "CaveLight_Descent", new Vector3(EntrancePos.x + 1.2f, entranceY - 2.2f, EntrancePos.z + 14f), new Color(0.25f, 0.90f, 0.65f), 18f, 2.4f);
                AddCaveLight(root, "CaveLight_Hearth", new Vector3(EntrancePos.x - 0.5f, undergroundFloorY + 3.2f, EntrancePos.z + 34f), new Color(1.0f, 0.50f, 0.16f), 22f, 3.2f);
                AddCaveLight(root, "CaveLight_Sanctum", new Vector3(SanctumPos.x, undergroundFloorY + 4.8f, SanctumPos.z + 3f), new Color(0.20f, 0.95f, 0.68f), 28f, 3.8f);
            }
        }

        static void BuildOutsideCamp(Transform root, float entranceY)
        {
            Vector3 campPos = new Vector3(CampfirePos.x, SampleGround(CampfirePos), CampfirePos.z);

            if (!Application.isBatchMode)
                AddCaveLight(root, "Kaelen_CampLight", campPos + Vector3.up * 1.4f, new Color(1.0f, 0.54f, 0.16f), 14f, 2.4f);

            // Warden Kaelen (Quest Giver for the 2-step Ashenmaw Cavern questline)
            Vector3 npcPos = new Vector3(CampfirePos.x + 2.4f, SampleGround(CampfirePos + new Vector3(2.4f, 0f, 0.5f)), CampfirePos.z + 0.5f);
            var npcGo = new GameObject(QuestGiverName);
            npcGo.transform.SetParent(root, false);
            npcGo.transform.position = npcPos;
            npcGo.transform.rotation = Quaternion.Euler(0f, 195f, 0f);
            CharacterVisualFactory.CreateVisualInstance("Elf", "Warrior", npcGo.transform);
            var col = npcGo.AddComponent<CapsuleCollider>();
            col.height = 1.9f; col.radius = 0.36f; col.center = Vector3.up * 0.95f;
            var npc = npcGo.AddComponent<TownNPC>();
            npc.npcName = QuestGiverName;
            npc.roleTag = "Cavern Warden";
            npc.greeting = "The Briarclaw Tribe broke into Ashenmaw Cavern beneath this ridge and roused Ursolak, the Ancient Bear Guardian. Walk straight down through the cave mouth into the earth.";

            var pin = new GameObject("Ashenmaw Cavern map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root, false);
            pin.transform.position = campPos + new Vector3(-2.5f, 0f, 1.0f);
            pin.displayName = "Ashenmaw Cavern";
            pin.notice = "Ashenmaw Cavern — Seamless subterranean cavern carved beneath the northern ridge. Home to the Briarclaw Tribe and Ursolak, Ancient Bear Guardian.";
        }

        static void SpawnTribeMember(Transform parent, PlayerController player, string id, string name, string race, string cls, Vector3 pos, bool ranged)
        {
            RiverReaverCamp.CreateEnemy(parent, player, id, name, race, cls, pos, boss: false, ranged: ranged);
            foreach (var ec in parent.GetComponentsInChildren<EnemyController>(true))
            {
                if (ec.enemyId != id) continue;
                ec.transform.position = pos + Vector3.up * 0.08f;
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
            root.transform.position = pos + Vector3.up * 0.08f;
            root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var bearFbx = Resources.Load<GameObject>("Ashenmaw/Ursolak_BearGuardian");
            if (bearFbx)
            {
                var vis = Object.Instantiate(bearFbx, root.transform);
                vis.name = "BearGuardianVisual";
                vis.transform.localPosition = Vector3.zero;
                vis.transform.localRotation = Quaternion.identity;
                vis.transform.localScale = Vector3.one * 1.18f;
                ApplyBearMaterials(vis);
                vis.AddComponent<BearGuardianAnimator>();
            }

            var enemy = root.AddComponent<EnemyController>();
            enemy.enemyId = BossId;
            enemy.player = player;
            enemy.isBoss = true;
            enemy.heavyAttacker = true;
            enemy.campGuard = true;
            enemy.attackDamage = 20f;
            enemy.attackInterval = 2.0f;
            enemy.attackRange = 3.4f;
            enemy.movementMultiplier = 0.78f;

            var motor = root.GetComponent<CharacterController>();
            motor.height = 2.6f; motor.radius = 1.05f; motor.center = Vector3.up * 1.3f;
            motor.slopeLimit = 45f; motor.stepOffset = 0.35f;

            var stats = root.GetComponent<Combatant>();
            stats.displayName = BossName;
            stats.ConfigureVitals(480, 4);
            stats.Restore();

            root.SetActive(!player || !Application.isPlaying || !player.HasDefeated(BossId));
        }

        static void ApplyCavernMaterials(GameObject shell)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (!sh) return;
            var rockMat = new Material(sh) { color = new Color(0.18f, 0.19f, 0.21f) };
            var jadeMat = new Material(sh) { color = new Color(0.16f, 0.92f, 0.64f) };
            if (jadeMat.HasProperty("_EmissionColor")) { jadeMat.EnableKeyword("_EMISSION"); jadeMat.SetColor("_EmissionColor", new Color(0.16f, 0.92f, 0.64f) * 2.2f); }
            foreach (var r in shell.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                var replaced = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                    replaced[i] = (mats[i] && mats[i].name.Contains("Jade")) ? jadeMat : rockMat;
                r.sharedMaterials = replaced;
            }
        }

        static void ApplyBearMaterials(GameObject bear)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (!sh) return;
            var darkFur = new Material(sh) { color = new Color(0.17f, 0.13f, 0.10f) };
            var maneFur = new Material(sh) { color = new Color(0.28f, 0.22f, 0.16f) };
            var bone = new Material(sh) { color = new Color(0.84f, 0.80f, 0.70f) };
            var iron = new Material(sh) { color = new Color(0.20f, 0.21f, 0.23f) };
            var jade = new Material(sh) { color = new Color(0.18f, 0.95f, 0.66f) };
            if (jade.HasProperty("_EmissionColor")) { jade.EnableKeyword("_EMISSION"); jade.SetColor("_EmissionColor", new Color(0.18f, 0.95f, 0.66f) * 2.4f); }

            foreach (var r in bear.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                var replaced = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    string n = mats[i] ? mats[i].name : "";
                    if (n.Contains("Jade")) replaced[i] = jade;
                    else if (n.Contains("Bone")) replaced[i] = bone;
                    else if (n.Contains("Iron")) replaced[i] = iron;
                    else if (n.Contains("Mane")) replaced[i] = maneFur;
                    else replaced[i] = darkFur;
                }
                r.sharedMaterials = replaced;
            }
        }

        static void AddCaveLight(Transform parent, string name, Vector3 pos, Color color, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
        }

        static float SampleGround(Vector3 xz)
        {
            var hits = Physics.RaycastAll(new Vector3(xz.x, 260f, xz.z), Vector3.down, 420f);
            float best = float.MinValue;
            bool found = false;
            foreach (var h in hits)
            {
                if (h.collider is TerrainCollider && h.point.y > best) { best = h.point.y; found = true; }
            }
            return found ? best : WorldRegions.GroundHeight(xz);
        }
    }

    /// <summary>
    /// Procedural breathing, stride sway, and combat rearing motion for Ursolak, Ancient Bear Guardian.
    /// </summary>
    public sealed class BearGuardianAnimator : MonoBehaviour
    {
        EnemyController enemy;
        Vector3 baseLocalPos;

        void Awake()
        {
            enemy = GetComponentInParent<EnemyController>();
            baseLocalPos = transform.localPosition;
        }

        void Update()
        {
            float t = Time.time;
            bool chasing = enemy != null && enemy.IsChasing;
            float speed = chasing ? 6.5f : 2.1f;
            float bob = Mathf.Sin(t * speed) * (chasing ? 0.12f : 0.045f);
            float pitch = chasing ? Mathf.Sin(t * speed * 0.5f) * 7.5f : Mathf.Sin(t * 1.6f) * 2.0f;
            float roll = chasing ? Mathf.Cos(t * speed) * 4.5f : 0f;
            transform.localPosition = baseLocalPos + new Vector3(0f, Mathf.Abs(bob), 0f);
            transform.localRotation = Quaternion.Euler(pitch, 0f, roll);
        }
    }
}
