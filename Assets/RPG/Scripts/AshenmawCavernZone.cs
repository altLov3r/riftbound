using UnityEngine;

namespace Riftbound
{
    /// <summary>
    /// Seamless walk-in mountain cavern ("Ashenmaw Cavern") carved directly INTO the Wyrmcrest
    /// western cliffside at (155, 415 -> 490) via in-memory TerrainData.SetHoles (never modifying
    /// authored terrain assets on disk). Uses sculpted double-walled Blender FBX geometry
    /// (Ashenmaw_Cavern_Complete.fbx) with a solid stone approach apron and animated Ursolak,
    /// Ancient Bear Guardian FBX boss (Ursolak_BearGuardian.fbx).
    /// </summary>
    public static class AshenmawCavernZone
    {
        public const string RootName = "Ashenmaw Seamless Cavern";
        public const string QuestGiverName = "Warden Kaelen";
        public const string BossId = "ursolak-bear-guardian";
        public const string BossName = "Ursolak, Bear Guardian";
        public const string BossChestId = "ashenmaw-guardian-chest";

        public static readonly Vector3 CampfirePos = new Vector3(155f, 0f, 395f);
        public static readonly Vector3 EntrancePos = new Vector3(155f, 0f, 415f);
        public static readonly Vector3 SanctumPos = new Vector3(155f, 0f, 475f);

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
            float undergroundFloorY = entranceY - 4.20f;

            // 1. Instantiate sculpted double-walled Blender cavern shell & auto-verify +Z orientation
            bool rotated180 = BuildOrganicCavernFromFbx(root, entranceY, undergroundFloorY);

            // 2. Build solid stone approach steps & plinth connecting Campfire approach terrain to cave apron (zero gap!)
            BuildApproachApron(root, entranceY);

            // 3. Carve a REAL hole into an in-memory clone of the terrain tile inside the stone archway
            //    where the solid stone cavern floor sits cleanly beneath it.
            CarveTerrainEntranceHole(EntrancePos, halfWidthMeters: 4.6f, startZOffset: 0.2f, lengthMeters: 20.0f);

            // 4. Safe Outside Watch Camp at Z=395 (20m south of cave mouth, >=48m from all hostile tribe members)
            BuildOutsideCamp(root, entranceY);

            // 5. Briarclaw Tribe (8 hostile NPCs inside subterranean tunnel, ritual chamber & sanctum throat)
            SpawnTribeMember(root, player, "ashenmaw-tribe-1", "Briarclaw Ravager", "Orc", "Warrior", new Vector3(EntrancePos.x + 0.6f, undergroundFloorY, EntrancePos.z + 29.0f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-2", "Briarclaw Bone-Thrower", "Orc", "Rogue", new Vector3(EntrancePos.x - 2.4f, undergroundFloorY, EntrancePos.z + 31.0f), true);
            SpawnTribeMember(root, player, "ashenmaw-tribe-3", "Briarclaw Ravager", "Human", "Warrior", new Vector3(EntrancePos.x + 2.8f, undergroundFloorY, EntrancePos.z + 32.5f), false);

            SpawnTribeMember(root, player, "ashenmaw-tribe-4", "Briarclaw Blood-Shaman", "Undead", "Mage", new Vector3(EntrancePos.x - 4.2f, undergroundFloorY, EntrancePos.z + 36.5f), true);
            SpawnTribeMember(root, player, "ashenmaw-tribe-5", "Briarclaw Mauler", "Orc", "Warrior", new Vector3(EntrancePos.x + 3.6f, undergroundFloorY, EntrancePos.z + 37.5f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-6", "Briarclaw Stalker", "Orc", "Rogue", new Vector3(EntrancePos.x - 0.5f, undergroundFloorY, EntrancePos.z + 44.0f), false);

            SpawnTribeMember(root, player, "ashenmaw-tribe-7", "Briarclaw Totem-Guard", "Orc", "Warrior", new Vector3(EntrancePos.x - 5.2f, undergroundFloorY, SanctumPos.z - 8.5f), false);
            SpawnTribeMember(root, player, "ashenmaw-tribe-8", "Briarclaw Totem-Guard", "Orc", "Warrior", new Vector3(EntrancePos.x + 5.2f, undergroundFloorY, SanctumPos.z - 8.5f), false);

            // 6. Ursolak, Ancient Bear Guardian Boss (480 HP, real sculpted Blender FBX V3)
            SpawnBearGuardianBoss(root, player, new Vector3(SanctumPos.x, undergroundFloorY, SanctumPos.z), rotated180);

            // 7. Boss-Locked Reliquary Chest at the Spirit Altar
            var prefab = Resources.Load<GameObject>("Props/TreasureChest");
            if (prefab)
            {
                Vector3 chestPos = new Vector3(SanctumPos.x, undergroundFloorY + 0.06f, SanctumPos.z + 6.5f);
                var instance = Object.Instantiate(prefab, chestPos, Quaternion.Euler(0f, 180f, 0f), root);
                instance.name = "Ursolak's Spirit Reliquary";
                var chest = instance.GetComponent<LootChest>() ?? instance.AddComponent<LootChest>();
                chest.chestId = BossChestId;
                chest.gold = 120;
                chest.experience = 180;
            }
        }

        static bool BuildOrganicCavernFromFbx(Transform root, float entranceY, float undergroundFloorY)
        {
            bool rotated180 = false;
            var fbx = Resources.Load<GameObject>("Ashenmaw/Ashenmaw_Cavern_Complete");
            if (fbx)
            {
                var shell = Object.Instantiate(fbx, new Vector3(EntrancePos.x, entranceY, EntrancePos.z), Quaternion.identity, root);
                shell.name = "Ashenmaw_OrganicCavern";

                // Auto-verify FBX tunnel direction using combined renderer bounds: tunnel MUST extend north (+Z)!
                Bounds b = MeasureBounds(shell);
                if (b.center.z < EntrancePos.z + 5.0f)
                {
                    shell.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    rotated180 = true;
                }

                foreach (var mf in shell.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (!mf.sharedMesh) continue;
                    var mc = mf.gameObject.GetComponent<MeshCollider>();
                    if (!mc) mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
                ApplyCavernMaterials(shell);
            }

            if (!Application.isBatchMode)
            {
                AddCaveLight(root, "CaveLight_Mouth", new Vector3(EntrancePos.x, entranceY + 2.8f, EntrancePos.z + 2f), new Color(1.0f, 0.58f, 0.22f), 20f, 3.0f);
                AddCaveLight(root, "CaveLight_Descent", new Vector3(EntrancePos.x + 0.5f, entranceY - 0.1f, EntrancePos.z + 16f), new Color(0.24f, 0.90f, 0.65f), 20f, 2.6f);
                AddCaveLight(root, "CaveLight_Hearth", new Vector3(EntrancePos.x - 0.5f, undergroundFloorY + 3.5f, EntrancePos.z + 34f), new Color(1.0f, 0.52f, 0.16f), 24f, 3.4f);
                AddCaveLight(root, "CaveLight_Sanctum", new Vector3(SanctumPos.x, undergroundFloorY + 5.2f, SanctumPos.z + 2f), new Color(0.20f, 0.96f, 0.68f), 30f, 4.0f);
            }
            return rotated180;
        }

        static void BuildApproachApron(Transform root, float entranceY)
        {
            // Solid stone bridge-ramp from natural terrain at Z=397.5 smoothly to the sloped courtyard apron edge at Z=407.0
            Vector3 startPos = new Vector3(EntrancePos.x, SampleGround(new Vector3(EntrancePos.x, 0f, EntrancePos.z - 17.5f)) - 0.06f, EntrancePos.z - 17.5f);
            Vector3 endPos = new Vector3(EntrancePos.x, entranceY - 2.60f, EntrancePos.z - 8.0f);
            Vector3 span = endPos - startPos;
            Quaternion rot = Quaternion.LookRotation(span.normalized, Vector3.up);

            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Ashenmaw_ApproachRamp";
            ramp.transform.SetParent(root, false);
            ramp.transform.position = (startPos + endPos) * 0.5f + rot * Vector3.down * 0.625f;
            ramp.transform.localScale = new Vector3(8.8f, 1.25f, span.magnitude + 0.25f);
            ramp.transform.rotation = rot;

            // Solid masonry plinth beneath the outdoor courtyard apron (top kept safely below sloped flagstones)
            var skirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            skirt.name = "Ashenmaw_ApronPlinth";
            skirt.transform.SetParent(root, false);
            skirt.transform.position = new Vector3(EntrancePos.x, entranceY - 4.70f, EntrancePos.z - 3.9f);
            skirt.transform.localScale = new Vector3(9.2f, 4.0f, 8.0f);

            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (sh)
            {
                var mat = new Material(sh) { color = new Color(0.22f, 0.24f, 0.26f) };
                ramp.GetComponent<MeshRenderer>().sharedMaterial = mat;
                skirt.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
        }

        static void CarveTerrainEntranceHole(Vector3 mouthXZ, float halfWidthMeters, float startZOffset, float lengthMeters)
        {
            if (!Application.isPlaying) return;
            var tile = WorldRegions.TerrainAt(mouthXZ, Terrain.activeTerrains);
            if (!tile || !tile.terrainData) return;

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
            bool[,] holes = new bool[h, w];
            cloned.SetHoles(minX, minZ, holes);
        }

        static void BuildOutsideCamp(Transform root, float entranceY)
        {
            Vector3 campPos = new Vector3(CampfirePos.x, SampleGround(CampfirePos), CampfirePos.z);

            if (!Application.isBatchMode)
                AddCaveLight(root, "Kaelen_CampLight", campPos + Vector3.up * 1.4f, new Color(1.0f, 0.54f, 0.16f), 14f, 2.4f);

            // Warden Kaelen cleanly positioned at the safe watch camp (Z=395)
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
            npc.greeting = "The Briarclaw Tribe broke into Ashenmaw Cavern inside the Wyrmcrest cliffside ahead and roused Ursolak, the Ancient Bear Guardian. Follow the stone apron straight into the cave mouth—no portal or loading screen awaits.";

            var pin = new GameObject("Ashenmaw Cavern map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root, false);
            pin.transform.position = campPos + new Vector3(-2.5f, 0f, 1.0f);
            pin.displayName = "Ashenmaw Cavern";
            pin.notice = "Ashenmaw Cavern — Seamless mountain cavern carved into Wyrmcrest's western cliff. Home to the Briarclaw Tribe and Ursolak, Ancient Bear Guardian.";
        }

        static void SpawnTribeMember(Transform parent, PlayerController player, string id, string name, string race, string cls, Vector3 pos, bool ranged)
        {
            RiverReaverCamp.CreateEnemy(parent, player, id, name, race, cls, pos, boss: false, ranged: ranged);
            foreach (var ec in parent.GetComponentsInChildren<EnemyController>(true))
            {
                if (ec.enemyId != id) continue;
                ec.SetHomePosition(pos + Vector3.up * 0.08f);
                var stats = ec.GetComponent<Combatant>();
                if (stats) { stats.ConfigureVitals(115, 1); stats.Restore(); }
                break;
            }
        }

        static void SpawnBearGuardianBoss(Transform parent, PlayerController player, Vector3 pos, bool mirrorForward)
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
                vis.transform.localRotation = Quaternion.Euler(0f, mirrorForward ? 180f : 0f, 0f);
                vis.transform.localScale = Vector3.one * 1.15f;
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
            enemy.SetHomePosition(pos + Vector3.up * 0.08f, Quaternion.Euler(0f, 180f, 0f));
        }

        static Bounds MeasureBounds(GameObject go)
        {
            Physics.SyncTransforms();
            var renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        static void ApplyCavernMaterials(GameObject shell)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (!sh) return;
            var basaltMat = new Material(sh) { color = new Color(0.19f, 0.21f, 0.24f) };
            var slateMat = new Material(sh) { color = new Color(0.26f, 0.30f, 0.34f) };
            var stoneMat = new Material(sh) { color = new Color(0.56f, 0.54f, 0.48f) };
            var amberMat = new Material(sh) { color = new Color(1.0f, 0.65f, 0.24f) };
            var jadeMat = new Material(sh) { color = new Color(0.16f, 0.92f, 0.64f) };
            if (amberMat.HasProperty("_EmissionColor")) { amberMat.EnableKeyword("_EMISSION"); amberMat.SetColor("_EmissionColor", amberMat.color * 2.0f); }
            if (jadeMat.HasProperty("_EmissionColor")) { jadeMat.EnableKeyword("_EMISSION"); jadeMat.SetColor("_EmissionColor", jadeMat.color * 2.2f); }

            foreach (var r in shell.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                var replaced = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    string n = mats[i] ? mats[i].name : "";
                    if (n.Contains("Jade")) replaced[i] = jadeMat;
                    else if (n.Contains("Amber")) replaced[i] = amberMat;
                    else if (n.Contains("Pale")) replaced[i] = stoneMat;
                    else if (n.Contains("Slate")) replaced[i] = slateMat;
                    else replaced[i] = basaltMat;
                }
                r.sharedMaterials = replaced;
            }
        }

        static void ApplyBearMaterials(GameObject bear)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (!sh) return;
            var darkFur = new Material(sh) { color = new Color(0.17f, 0.13f, 0.10f) };
            var maneFur = new Material(sh) { color = new Color(0.32f, 0.22f, 0.15f) };
            var bone = new Material(sh) { color = new Color(0.86f, 0.82f, 0.72f) };
            var iron = new Material(sh) { color = new Color(0.20f, 0.21f, 0.25f) };
            var jade = new Material(sh) { color = new Color(0.18f, 0.95f, 0.66f) };
            if (jade.HasProperty("_EmissionColor")) { jade.EnableKeyword("_EMISSION"); jade.SetColor("_EmissionColor", jade.color * 2.4f); }

            foreach (var r in bear.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                var replaced = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    string n = mats[i] ? mats[i].name : "";
                    if (n.Contains("Jade")) replaced[i] = jade;
                    else if (n.Contains("Bone")) replaced[i] = bone;
                    else if (n.Contains("Black Iron")) replaced[i] = iron;
                    else if (n.Contains("Oak")) replaced[i] = maneFur;
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
        Quaternion baseLocalRot;

        void Awake()
        {
            enemy = GetComponentInParent<EnemyController>();
            baseLocalPos = transform.localPosition;
            baseLocalRot = transform.localRotation;
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
            transform.localRotation = baseLocalRot * Quaternion.Euler(pitch, 0f, roll);
        }
    }
}
