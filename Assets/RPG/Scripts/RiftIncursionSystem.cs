using System;
using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    /// <summary>
    /// Dynamic open-world "Rift Incursion" events across safe frontier clearings.
    /// Brings the Riftbound title to life with a towering sky-beam portal, 2-wave escalation
    /// (3 Rift Vanguard invaders -> Valkor, Rift Herald mini-boss with telegraphed Void Nova),
    /// pulsing minimap beacon, compact HUD event tracker, and Rift Stabilized rewards.
    /// </summary>
    public class RiftIncursionSystem : MonoBehaviour
    {
        public enum IncursionPhase
        {
            Idle = 0,
            Wave1Vanguard = 1,
            Wave2Herald = 2,
            StabilizedCooldown = 3
        }

        public readonly struct RiftSite
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly Vector3 Position;

            public RiftSite(string id, string displayName, Vector3 position)
            {
                Id = id;
                DisplayName = displayName;
                Position = position;
            }
        }

        public static RiftIncursionSystem Instance { get; private set; }

        public const float InitialAutoOpenDelay = 45f;
        public const float NextIncursionCooldown = 180f;
        public const float VoidNovaInterval = 8.5f;
        public const float VoidNovaTelegraphDuration = 2.2f;
        public const float VoidNovaRadius = 8.0f;
        public const float VoidNovaDamage = 26f;
        public const float RiftSurgeDuration = 60f;
        public const int RewardXp = 140;
        public const int RewardGold = 55;

        public static readonly RiftSite[] Sites =
        {
            new RiftSite("emberwatch-ridge", "Emberwatch Outskirts Ridge", new Vector3(18f, 0f, 22f)),
            new RiftSite("ashwood-glade", "Ashwood Crossroads Glade", new Vector3(15f, 0f, 118f)),
            new RiftSite("silverglen-meadow", "Silverglen Outer Meadow", new Vector3(-92f, 0f, 62f)),
            new RiftSite("greenhollow-basin", "Greenhollow Southern Basin", new Vector3(28f, 0f, 328f)),
        };

        public IncursionPhase Phase { get; private set; } = IncursionPhase.Idle;
        public int ActiveSiteIndex { get; private set; } = -1;
        public Vector3 ActiveRiftPosition { get; private set; }
        public string ActiveSiteName => ActiveSiteIndex >= 0 && ActiveSiteIndex < Sites.Length ? Sites[ActiveSiteIndex].DisplayName : "";
        public bool IsActive => Phase == IncursionPhase.Wave1Vanguard || Phase == IncursionPhase.Wave2Herald;
        public int TotalRiftsClosed { get; private set; }
        public int VanguardTotal => vanguardEnemies.Count;
        public int VanguardDefeated => CountDefeated(vanguardEnemies);
        public bool IsVoidNovaCharging => voidNovaActive;
        public float VoidNovaProgress => voidNovaActive ? Mathf.Clamp01((Time.time - voidNovaStartTime) / VoidNovaTelegraphDuration) : 0f;
        public bool HasRiftSurge => Time.time < riftSurgeUntil;
        public float RiftSurgeRemaining => Mathf.Max(0f, riftSurgeUntil - Time.time);

        // Automated verification counters
        public int VoidNovasTriggeredForTest { get; private set; }
        public int VoidNovasDodgedForTest { get; private set; }
        public int VoidNovasHitForTest { get; private set; }
        public EnemyController ActiveHerald => heraldEnemy;
        public IReadOnlyList<EnemyController> ActiveVanguard => vanguardEnemies;

        PlayerController player;
        GameObject riftRoot;
        Transform outerRing;
        Transform innerRing;
        Transform coreOrb;
        Transform skyBeam;
        readonly List<Transform> orbitingShards = new List<Transform>();
        readonly List<EnemyController> vanguardEnemies = new List<EnemyController>();
        EnemyController heraldEnemy;

        GameObject novaRingRoot;
        Transform novaFillDisc;
        Vector3 novaCenter;
        bool voidNovaActive;
        float voidNovaStartTime;
        float nextVoidNovaTime;

        GameObject shockwaveObj;
        float shockwaveStartTime = -99f;

        float nextAutoOpenTime = -1f;
        float bannerUntil = -1f;
        string bannerTitle = "";
        string bannerSubtitle = "";
        float riftSurgeUntil = -1f;
        int nextCycleIndex;

        Texture2D panelTex;
        Texture2D fillBgTex;
        Texture2D fillVioletTex;
        Texture2D fillCrimsonTex;
        Texture2D borderTex;
        GUIStyle titleStyle;
        GUIStyle bodyStyle;
        GUIStyle smallBtnStyle;
        GUIStyle bannerTitleStyle;
        GUIStyle bannerSubStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(this);
                else DestroyImmediate(this);
                return;
            }
            Instance = this;
            player = GetComponent<PlayerController>() ?? FindFirstObjectByType<PlayerController>();
            nextAutoOpenTime = Time.time + InitialAutoOpenDelay;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            CleanupRiftObjects();
            if (shockwaveObj) Destroy(shockwaveObj);
        }

        void Update()
        {
            if (!player) player = FindFirstObjectByType<PlayerController>();

            // Test-branch hotkeys (ignored while typing or in FrontendMenu)
            if (Application.isPlaying && !Application.isBatchMode && !FrontendMenu.IsOpen && GUIUtility.keyboardControl == 0)
            {
                if (Input.GetKeyDown(KeyCode.F8))
                {
                    bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    if (shift && IsActive)
                    {
                        WarpPlayerNearActiveRift();
                    }
                    else
                    {
                        int idx = ActiveSiteIndex >= 0 ? (ActiveSiteIndex + 1) % Sites.Length : nextCycleIndex;
                        StartIncursion(idx);
                    }
                }
            }

            UpdateShockwaveVisual();

            // Auto-cycle only during live play (not in headless automated batch runs)
            if (!IsActive && Application.isPlaying && !Application.isBatchMode && !FrontendMenu.IsOpen && player && player.Stats && player.Stats.Alive)
            {
                if (nextAutoOpenTime > 0f && Time.time >= nextAutoOpenTime)
                {
                    StartIncursion(nextCycleIndex);
                }
            }

            if (!IsActive) return;

            AnimateRiftPortal();

            if (Phase == IncursionPhase.Wave1Vanguard)
            {
                if (vanguardEnemies.Count > 0 && VanguardDefeated >= vanguardEnemies.Count)
                {
                    BeginWave2Herald();
                }
            }
            else if (Phase == IncursionPhase.Wave2Herald)
            {
                UpdateHeraldMechanics();
                if (!heraldEnemy || !heraldEnemy.GetComponent<Combatant>() || !heraldEnemy.GetComponent<Combatant>().Alive)
                {
                    CompleteIncursion();
                }
            }
        }

        public bool StartIncursion(int siteIndex = -1)
        {
            if (Sites.Length == 0) return false;
            if (!player) player = FindFirstObjectByType<PlayerController>();

            CleanupRiftObjects();

            if (siteIndex < 0 || siteIndex >= Sites.Length)
                siteIndex = nextCycleIndex % Sites.Length;

            ActiveSiteIndex = siteIndex;
            nextCycleIndex = (siteIndex + 1) % Sites.Length;

            RiftSite site = Sites[siteIndex];
            Vector3 grounded = SampleGroundPoint(site.Position);
            ActiveRiftPosition = grounded;

            BuildRiftPortalVisuals(grounded);
            SpawnWave1Vanguard(grounded);

            Phase = IncursionPhase.Wave1Vanguard;
            voidNovaActive = false;
            ShowBanner("RIFT INCURSION OPENED", $"{site.DisplayName} — Defeat the Rift Vanguard!");
            return true;
        }

        public void ForceCancelIncursion()
        {
            CleanupRiftObjects();
            Phase = IncursionPhase.Idle;
            ActiveSiteIndex = -1;
            nextAutoOpenTime = Time.time + NextIncursionCooldown;
        }

        public void WarpPlayerNearActiveRift()
        {
            if (!IsActive || !player) return;
            Vector3 target = SampleGroundPoint(ActiveRiftPosition + new Vector3(0f, 0f, -18f));
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            player.transform.position = target + Vector3.up * 0.25f;
            player.transform.LookAt(new Vector3(ActiveRiftPosition.x, player.transform.position.y, ActiveRiftPosition.z));
            if (cc) cc.enabled = true;
            ShowBanner("WARPED TO RIFT", ActiveSiteName);
        }

        public void TriggerVoidNovaNowForTest()
        {
            if (Phase != IncursionPhase.Wave2Herald || !heraldEnemy) return;
            StartVoidNova(heraldEnemy.transform.position);
        }

        public void ResolveVoidNovaNowForTest()
        {
            if (!voidNovaActive) return;
            ResolveVoidNova();
        }

        void SpawnWave1Vanguard(Vector3 center)
        {
            vanguardEnemies.Clear();
            int level = player ? Mathf.Max(1, player.Level) : 1;
            int vanguardHp = 85 + level * 14;

            var specs = new (string suffix, string name, string race, string cls, Vector3 offset, bool ranged)[]
            {
                ("vanguard-1", "Riftbound Stalker", "Undead", "Rogue", new Vector3(-5.2f, 0f, -4.4f), false),
                ("vanguard-2", "Riftbound Spellblade", "Elf", "Mage", new Vector3(5.2f, 0f, -4.4f), true),
                ("vanguard-3", "Riftbound Marauder", "Orc", "Warrior", new Vector3(0f, 0f, 5.6f), false),
            };

            string prefix = $"rift-{Sites[ActiveSiteIndex].Id}-{TotalRiftsClosed}";
            foreach (var s in specs)
            {
                Vector3 pos = SampleGroundPoint(center + s.offset);
                string enemyId = $"{prefix}-{s.suffix}";
                RiverReaverCamp.CreateEnemy(riftRoot.transform, player, enemyId, s.name, s.race, s.cls, pos, boss: false, ranged: s.ranged);

                EnemyController spawned = FindEnemyById(riftRoot.transform, enemyId);
                if (spawned)
                {
                    spawned.SetHomePosition(pos + Vector3.up * 0.08f);
                    spawned.campGuard = true;
                    spawned.attackDamage = 9f + level * 1.4f;
                    var stats = spawned.GetComponent<Combatant>();
                    if (stats)
                    {
                        stats.displayName = s.name;
                        stats.ConfigureVitals(vanguardHp, Mathf.Max(1, level / 2));
                        stats.Restore();
                    }
                    AttachRiftAura(spawned.transform, new Color(0.72f, 0.24f, 1.0f), 0.72f);
                    vanguardEnemies.Add(spawned);
                }
            }
        }

        void BeginWave2Herald()
        {
            Phase = IncursionPhase.Wave2Herald;
            int level = player ? Mathf.Max(1, player.Level) : 1;
            int heraldHp = 210 + level * 28;
            Vector3 pos = SampleGroundPoint(ActiveRiftPosition + new Vector3(0f, 0f, -3.5f));
            string heraldId = $"rift-{Sites[ActiveSiteIndex].Id}-{TotalRiftsClosed}-herald";

            RiverReaverCamp.CreateEnemy(riftRoot.transform, player, heraldId, "Valkor, Rift Herald", "Undead", "Warrior", pos, boss: true, ranged: false);
            heraldEnemy = FindEnemyById(riftRoot.transform, heraldId);
            if (heraldEnemy)
            {
                heraldEnemy.transform.localScale = Vector3.one * 1.22f;
                heraldEnemy.SetHomePosition(pos + Vector3.up * 0.08f);
                heraldEnemy.isBoss = true;
                heraldEnemy.heavyAttacker = true;
                heraldEnemy.campGuard = true;
                heraldEnemy.attackDamage = 15f + level * 2.0f;
                heraldEnemy.attackInterval = 1.85f;
                var stats = heraldEnemy.GetComponent<Combatant>();
                if (stats)
                {
                    stats.displayName = "Valkor, Rift Herald";
                    stats.ConfigureVitals(heraldHp, Mathf.Max(2, level));
                    stats.Restore();
                }
                AttachRiftAura(heraldEnemy.transform, new Color(1.0f, 0.18f, 0.56f), 1.05f);
            }

            nextVoidNovaTime = Time.time + 4.5f;
            ShowBanner("WAVE 2: RIFT HERALD", "Valkor, Rift Herald emerges — dodge the Void Nova ring!");
        }

        void UpdateHeraldMechanics()
        {
            if (!heraldEnemy) return;
            var stats = heraldEnemy.GetComponent<Combatant>();
            if (!stats || !stats.Alive) return;

            if (voidNovaActive)
            {
                float progress = VoidNovaProgress;
                if (novaFillDisc)
                {
                    float scale = Mathf.Lerp(0.4f, VoidNovaRadius * 2f, progress);
                    novaFillDisc.localScale = new Vector3(scale, 0.04f, scale);
                }
                if (Time.time >= voidNovaStartTime + VoidNovaTelegraphDuration)
                {
                    ResolveVoidNova();
                }
                return;
            }

            if (player && player.Stats && player.Stats.Alive && Time.time >= nextVoidNovaTime)
            {
                float dist = Vector3.Distance(player.transform.position, heraldEnemy.transform.position);
                bool engaged = heraldEnemy.IsChasing || stats.Health < stats.maxHealth || dist <= 16f;
                if (engaged && dist <= 22f)
                {
                    StartVoidNova(heraldEnemy.transform.position);
                }
            }
        }

        void StartVoidNova(Vector3 center)
        {
            CleanupNovaVisual();
            novaCenter = SampleGroundPoint(center);
            voidNovaActive = true;
            voidNovaStartTime = Time.time;
            VoidNovasTriggeredForTest++;

            novaRingRoot = new GameObject("VoidNovaTelegraph");
            novaRingRoot.transform.position = novaCenter + Vector3.up * 0.12f;

            // Outer boundary ring
            var outer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            outer.name = "NovaOuterRing";
            DestroyCollider(outer);
            outer.transform.SetParent(novaRingRoot.transform, false);
            outer.transform.localScale = new Vector3(VoidNovaRadius * 2f, 0.02f, VoidNovaRadius * 2f);
            ApplyGlowMaterial(outer, new Color(0.95f, 0.15f, 0.45f, 0.35f), transparent: true);

            // Expanding inner charge disc
            var fill = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fill.name = "NovaFillDisc";
            DestroyCollider(fill);
            fill.transform.SetParent(novaRingRoot.transform, false);
            fill.transform.localPosition = Vector3.up * 0.03f;
            fill.transform.localScale = new Vector3(0.5f, 0.04f, 0.5f);
            ApplyGlowMaterial(fill, new Color(1.0f, 0.25f, 0.85f, 0.62f), transparent: true);
            novaFillDisc = fill.transform;
        }

        void ResolveVoidNova()
        {
            voidNovaActive = false;
            nextVoidNovaTime = Time.time + VoidNovaInterval;
            CleanupNovaVisual();

            var pStats = player ? (player.Stats ? player.Stats : player.GetComponent<Combatant>()) : null;
            if (!player || !pStats || !pStats.Alive) return;

            Vector3 delta = player.transform.position - novaCenter;
            delta.y = 0f;
            if (delta.magnitude <= VoidNovaRadius)
            {
                VoidNovasHitForTest++;
                pStats.Damage(VoidNovaDamage);
                ShowBanner("VOID NOVA HIT!", $"-{Mathf.RoundToInt(VoidNovaDamage)} Void damage — move outside the crimson ring!");
            }
            else
            {
                VoidNovasDodgedForTest++;
                ShowBanner("DODGED VOID NOVA!", "Clean evade outside the blast radius!");
            }
        }

        void CompleteIncursion()
        {
            Vector3 collapsePos = ActiveRiftPosition;
            string siteName = ActiveSiteName;

            CleanupRiftObjects();
            Phase = IncursionPhase.StabilizedCooldown;
            TotalRiftsClosed++;
            riftSurgeUntil = Time.time + RiftSurgeDuration;
            nextAutoOpenTime = Time.time + NextIncursionCooldown;

            SpawnCollapseShockwave(collapsePos);

            if (player)
            {
                player.AddExperience(RewardXp);
                player.AddGoldDirect(RewardGold);
                if (player.Stats && player.Stats.Alive)
                    player.Stats.Restore();
            }

            var prof = ProfessionSystem.Instance != null ? ProfessionSystem.Instance : FindAnyObjectByType<ProfessionSystem>();
            if (prof != null && prof.reagents != null)
            {
                AddReagent(prof.reagents, "Iron Ore", 2);
                AddReagent(prof.reagents, "Kingsblood", 2);
            }

            ShowBanner("RIFT STABILIZED!", $"{siteName} sealed! +{RewardXp} XP, +{RewardGold}g, +2 Iron Ore, +2 Kingsblood");
        }

        static void AddReagent(Dictionary<string, int> bag, string key, int amount)
        {
            if (bag == null || string.IsNullOrEmpty(key) || amount <= 0) return;
            bag[key] = bag.TryGetValue(key, out int cur) ? cur + amount : amount;
        }

        void BuildRiftPortalVisuals(Vector3 pos)
        {
            riftRoot = new GameObject($"RiftIncursion_{Sites[ActiveSiteIndex].Id}");
            riftRoot.transform.position = pos;

            // Ground scorch plinth
            var plinth = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plinth.name = "RiftScorchPlinth";
            DestroyCollider(plinth);
            plinth.transform.SetParent(riftRoot.transform, false);
            plinth.transform.localPosition = Vector3.up * 0.05f;
            plinth.transform.localScale = new Vector3(7.4f, 0.08f, 7.4f);
            ApplyGlowMaterial(plinth, new Color(0.18f, 0.05f, 0.28f, 0.85f), transparent: true);

            // Tall 85m Sky-Beam visible across the frontier
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "RiftSkyBeam";
            DestroyCollider(beam);
            beam.transform.SetParent(riftRoot.transform, false);
            beam.transform.localPosition = Vector3.up * 42.5f;
            beam.transform.localScale = new Vector3(1.55f, 42.5f, 1.55f);
            ApplyGlowMaterial(beam, new Color(0.72f, 0.22f, 1.0f, 0.28f), transparent: true);
            skyBeam = beam.transform;

            // Core Rift Orb
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "RiftCoreOrb";
            DestroyCollider(orb);
            orb.transform.SetParent(riftRoot.transform, false);
            orb.transform.localPosition = Vector3.up * 3.8f;
            orb.transform.localScale = new Vector3(1.9f, 2.7f, 1.9f);
            ApplyGlowMaterial(orb, new Color(0.55f, 0.08f, 0.92f, 0.95f), transparent: false);
            coreOrb = orb.transform;

            // Outer & Inner rotating segmented arcane rings
            outerRing = BuildFacetedRing(riftRoot.transform, "RiftOuterRing", Vector3.up * 3.8f, 3.1f, 12, new Color(0.92f, 0.28f, 1.0f));
            innerRing = BuildFacetedRing(riftRoot.transform, "RiftInnerRing", Vector3.up * 3.8f, 2.25f, 10, new Color(1.0f, 0.35f, 0.62f));

            // 6 Orbiting Void Shards
            orbitingShards.Clear();
            for (int i = 0; i < 6; i++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = $"VoidShard_{i}";
                DestroyCollider(shard);
                shard.transform.SetParent(riftRoot.transform, false);
                shard.transform.localScale = new Vector3(0.38f, 0.95f, 0.38f);
                ApplyGlowMaterial(shard, new Color(0.86f, 0.34f, 1.0f), transparent: false);
                orbitingShards.Add(shard.transform);
            }

            if (!Application.isBatchMode)
            {
                var lightGo = new GameObject("RiftPortalLight");
                lightGo.transform.SetParent(riftRoot.transform, false);
                lightGo.transform.localPosition = Vector3.up * 4.2f;
                var l = lightGo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(0.78f, 0.24f, 1.0f);
                l.range = 24f;
                l.intensity = 3.2f;
            }
        }

        static Transform BuildFacetedRing(Transform parent, string name, Vector3 localCenter, float radius, int segments, Color color)
        {
            var ring = new GameObject(name).transform;
            ring.SetParent(parent, false);
            ring.localPosition = localCenter;
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                DestroyCollider(seg);
                seg.transform.SetParent(ring, false);
                seg.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                seg.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
                seg.transform.localScale = new Vector3(0.24f, (2f * Mathf.PI * radius / segments) * 0.82f, 0.24f);
                ApplyGlowMaterial(seg, color, transparent: false);
            }
            return ring;
        }

        void AnimateRiftPortal()
        {
            float t = Time.time;
            if (outerRing) outerRing.localRotation = Quaternion.Euler(18f, t * 28f, t * 44f);
            if (innerRing) innerRing.localRotation = Quaternion.Euler(-24f, -t * 42f, -t * 62f);
            if (coreOrb)
            {
                float pulse = 1f + Mathf.Sin(t * 4.5f) * 0.12f;
                coreOrb.localScale = new Vector3(1.9f * pulse, 2.7f * pulse, 1.9f * pulse);
            }
            if (skyBeam)
            {
                float beamPulse = 1.45f + Mathf.Sin(t * 3.1f) * 0.22f;
                skyBeam.localScale = new Vector3(beamPulse, 42.5f, beamPulse);
            }
            for (int i = 0; i < orbitingShards.Count; i++)
            {
                var shard = orbitingShards[i];
                if (!shard) continue;
                float a = t * 1.15f + i * (Mathf.PI * 2f / orbitingShards.Count);
                float y = 2.6f + Mathf.Sin(t * 2.2f + i) * 0.9f;
                shard.localPosition = new Vector3(Mathf.Cos(a) * 4.2f, y, Mathf.Sin(a) * 4.2f);
                shard.localRotation = Quaternion.Euler(t * 50f + i * 25f, a * Mathf.Rad2Deg, 22f);
            }
        }

        void SpawnCollapseShockwave(Vector3 pos)
        {
            if (shockwaveObj) Destroy(shockwaveObj);
            shockwaveObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shockwaveObj.name = "RiftCollapseShockwave";
            DestroyCollider(shockwaveObj);
            shockwaveObj.transform.position = pos + Vector3.up * 2.5f;
            shockwaveObj.transform.localScale = Vector3.one * 1.2f;
            ApplyGlowMaterial(shockwaveObj, new Color(1.0f, 0.86f, 0.32f, 0.55f), transparent: true);
            shockwaveStartTime = Time.time;
        }

        void UpdateShockwaveVisual()
        {
            if (!shockwaveObj) return;
            float elapsed = Time.time - shockwaveStartTime;
            if (elapsed >= 1.1f)
            {
                Destroy(shockwaveObj);
                shockwaveObj = null;
                return;
            }
            float s = Mathf.Lerp(1.2f, 22f, elapsed / 1.1f);
            shockwaveObj.transform.localScale = Vector3.one * s;
        }

        static void AttachRiftAura(Transform enemyRoot, Color color, float radius)
        {
            var halo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            halo.name = "RiftAuraRing";
            DestroyCollider(halo);
            halo.transform.SetParent(enemyRoot, false);
            halo.transform.localPosition = Vector3.up * 0.08f;
            halo.transform.localScale = new Vector3(radius * 2f, 0.025f, radius * 2f);
            ApplyGlowMaterial(halo, new Color(color.r, color.g, color.b, 0.45f), transparent: true);
        }

        void CleanupNovaVisual()
        {
            if (novaRingRoot)
            {
                if (Application.isPlaying) Destroy(novaRingRoot);
                else DestroyImmediate(novaRingRoot);
                novaRingRoot = null;
                novaFillDisc = null;
            }
        }

        void CleanupRiftObjects()
        {
            CleanupNovaVisual();
            vanguardEnemies.Clear();
            heraldEnemy = null;
            orbitingShards.Clear();
            outerRing = null;
            innerRing = null;
            coreOrb = null;
            skyBeam = null;
            if (riftRoot)
            {
                if (Application.isPlaying) Destroy(riftRoot);
                else DestroyImmediate(riftRoot);
                riftRoot = null;
            }
        }

        void ShowBanner(string title, string subtitle)
        {
            bannerTitle = title;
            bannerSubtitle = subtitle;
            bannerUntil = Time.time + 5.0f;
        }

        static EnemyController FindEnemyById(Transform root, string id)
        {
            if (!root) return null;
            foreach (var ec in root.GetComponentsInChildren<EnemyController>(true))
            {
                if (ec.enemyId == id) return ec;
            }
            return null;
        }

        static int CountDefeated(List<EnemyController> list)
        {
            int defeated = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var ec = list[i];
                if (!ec) { defeated++; continue; }
                var c = ec.GetComponent<Combatant>();
                if (!c || !c.Alive) defeated++;
            }
            return defeated;
        }

        public static Vector3 SampleGroundPoint(Vector3 pos)
        {
            float y = WorldRegions.Expanded ? WorldRegions.GroundHeight(pos) : pos.y;
            if (!WorldRegions.Expanded && Terrain.activeTerrain)
                y = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.transform.position.y;
            return new Vector3(pos.x, y, pos.z);
        }

        static void DestroyCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }
        }

        static void ApplyGlowMaterial(GameObject go, Color color, bool transparent)
        {
            var renderer = go.GetComponent<Renderer>();
            if (!renderer) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Standard");
            if (!shader) return;
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (transparent)
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }
            renderer.sharedMaterial = mat;
        }

        void EnsureGuiStyles()
        {
            if (panelTex) return;
            panelTex = MakeTex(new Color(0.08f, 0.05f, 0.14f, 0.88f));
            fillBgTex = MakeTex(new Color(0.16f, 0.10f, 0.24f, 0.95f));
            fillVioletTex = MakeTex(new Color(0.68f, 0.24f, 1.0f, 0.96f));
            fillCrimsonTex = MakeTex(new Color(0.95f, 0.20f, 0.42f, 0.98f));
            borderTex = MakeTex(new Color(0.76f, 0.36f, 1.0f, 0.72f));

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.72f, 1.0f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.93f, 0.91f, 0.98f) }
            };
            smallBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            bannerTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.96f, 0.76f, 1.0f) }
            };
            bannerSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1.0f, 0.94f, 0.82f) }
            };
        }

        static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        public void DrawIncursionHUD(float screenWidth, float screenHeight)
        {
            if (Application.isBatchMode || FrontendMenu.IsOpen || !player || !player.Stats) return;
            EnsureGuiStyles();

            // Center-top dramatic event banner (5s)
            if (Time.time < bannerUntil && !string.IsNullOrEmpty(bannerTitle))
            {
                float bw = 500f;
                float bh = 58f;
                Rect br = new Rect((screenWidth - bw) * 0.5f, 84f, bw, bh);
                GUI.DrawTexture(br, panelTex);
                GUI.DrawTexture(new Rect(br.x, br.y, br.width, 2f), borderTex);
                GUI.DrawTexture(new Rect(br.x, br.yMax - 2f, br.width, 2f), borderTex);
                GUI.Label(new Rect(br.x + 8f, br.y + 4f, br.width - 16f, 26f), bannerTitle, bannerTitleStyle);
                GUI.Label(new Rect(br.x + 8f, br.y + 30f, br.width - 16f, 22f), bannerSubtitle, bannerSubStyle);
            }

            // Compact top-right Rift Tracker below minimap
            if (IsActive)
            {
                float w = 250f;
                float h = 116f;
                Rect r = new Rect(screenWidth - w - 16f, 268f, w, h);
                GUI.DrawTexture(r, panelTex);
                GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2f), borderTex);
                GUI.DrawTexture(new Rect(r.x, r.yMax - 2f, r.width, 2f), borderTex);

                float dist = Vector3.Distance(player.transform.position, ActiveRiftPosition);
                GUI.Label(new Rect(r.x + 10f, r.y + 6f, r.width - 20f, 18f), $"RIFT INCURSION  ({Mathf.RoundToInt(dist)} m)", titleStyle);
                GUI.Label(new Rect(r.x + 10f, r.y + 24f, r.width - 20f, 18f), ActiveSiteName, bodyStyle);

                float progress = 0f;
                string statusText;
                if (Phase == IncursionPhase.Wave1Vanguard)
                {
                    int def = VanguardDefeated;
                    int tot = Mathf.Max(1, VanguardTotal);
                    progress = def / (float)tot;
                    statusText = $"Wave 1: Rift Vanguard ({def}/{tot})";
                }
                else
                {
                    var hc = heraldEnemy ? heraldEnemy.GetComponent<Combatant>() : null;
                    progress = (hc && hc.maxHealth > 0f) ? Mathf.Clamp01(hc.Health / hc.maxHealth) : 0f;
                    statusText = voidNovaActive
                        ? "WARNING: VOID NOVA — DODGE RING!"
                        : $"Wave 2: Valkor, Rift Herald ({Mathf.RoundToInt(progress * 100f)}%)";
                }

                GUI.Label(new Rect(r.x + 10f, r.y + 44f, r.width - 20f, 18f), statusText, bodyStyle);

                Rect bar = new Rect(r.x + 10f, r.y + 66f, r.width - 20f, 12f);
                GUI.DrawTexture(bar, fillBgTex);
                GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * progress, bar.height), voidNovaActive ? fillCrimsonTex : fillVioletTex);

                if (GUI.Button(new Rect(r.x + 10f, r.y + 84f, 114f, 24f), "Warp (Shift+F8)", smallBtnStyle))
                {
                    WarpPlayerNearActiveRift();
                }
                if (GUI.Button(new Rect(r.x + 130f, r.y + 84f, 110f, 24f), "Next Rift (F8)", smallBtnStyle))
                {
                    StartIncursion((ActiveSiteIndex + 1) % Sites.Length);
                }
            }
            else if (HasRiftSurge)
            {
                float w = 230f;
                float h = 34f;
                Rect r = new Rect(screenWidth - w - 16f, 268f, w, h);
                GUI.DrawTexture(r, panelTex);
                GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2f), borderTex);
                GUI.Label(new Rect(r.x + 10f, r.y + 7f, r.width - 20f, 20f), $"Riftbreaker's Surge ({Mathf.CeilToInt(RiftSurgeRemaining)}s) · F8 Rift", bodyStyle);
            }
        }
    }
}
