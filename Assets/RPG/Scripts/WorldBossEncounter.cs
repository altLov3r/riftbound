using System;
using System.Collections;
using UnityEngine;

namespace Riftbound
{
    public class WorldBossEncounter : MonoBehaviour
    {
        public static WorldBossEncounter Instance { get; private set; }
        public static bool IsBossDefeatedAuthoritative { get; private set; }

        public const string BossEnemyId = "worldboss-gorgonax";
        public const string HoardChestId = "worldboss-gorgonax-chest";
        public const float WakeRadius = 35f;
        public const float StompRadius = 14f;
        public const float StompDamage = 28f;
        public const float StompInterval = 14f;
        public const float MeteorRange = 25f;
        public const float MeteorImpactRadius = 6.0f;
        public const float MeteorDamage = 45f;
        public const float MeteorWarningSeconds = 2.4f;
        public const float MeteorInterval = 22f;
        public const float EnrageHealthFraction = 0.35f;
        public const int DefeatXp = 150;
        public const int DefeatGold = 65;

        public const byte EventStomp = 1;
        public const byte EventMeteorWarn = 2;
        public const byte EventMeteorImpact = 3;
        public const byte EventEnrage = 4;
        public const byte EventDefeated = 5;
        public const byte EventReset = 6;

        public Combatant bossCombatant;
        public EnemyController bossController;
        public Vector3 spawnPosition = new Vector3(10f, 0f, 185f);

        bool enraged = false;
        bool meteorActive = false;
        float nextStompTime = 0f;
        float nextMeteorTime = 0f;
        bool lootSpawned = false;
        bool hasBaseStats = false;
        float baseAttackInterval = 1.6f;
        float baseAttackDamage = 28f;
        LootChest spawnedHoardChest;
        Vector3 hoardChestPosition;

        public bool IsEnraged => enraged;
        public bool IsMeteorActive => meteorActive;
        public bool HasSpawnedHoard => lootSpawned;
        public LootChest SpawnedHoardChest => spawnedHoardChest;
        public Vector3 HoardChestPosition => hoardChestPosition;
        public float BaseAttackInterval => baseAttackInterval;
        public float BaseAttackDamage => baseAttackDamage;
        public int StompCountForTest { get; private set; }
        public int MeteorWarnCountForTest { get; private set; }
        public int MeteorImpactCountForTest { get; private set; }
        public int ResetCountForTest { get; private set; }
        public byte LastReplicatedEventKind { get; private set; }

        PlayerController player;

        GUIStyle bossNameStyle, skullStyle, subTitleStyle, pctStyle;
        Texture2D barBgTex, hpTex, borderTex, skullBgTex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
            IsBossDefeatedAuthoritative = false;
        }

        static bool IsAuthoritativeRuntime =>
            !Network.CoopSessionConfig.IsMultiplayer ||
            (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsServer);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(this); else DestroyImmediate(this);
                return;
            }
            Instance = this;
            player = GetComponent<PlayerController>();
            if (WorldRegions.Expanded) spawnPosition = WorldRegions.WorldBoss;
        }

        void Start()
        {
            FindBossReference();
        }

        void OnDestroy()
        {
            if (bossController != null)
                bossController.OnEvadeReset -= ResetEncounterState;
            if (Instance == this)
                Instance = null;
        }

        public void FindBossReference()
        {
            if (bossCombatant != null && bossController != null) return;
            foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            {
                if (enemy.enemyId == BossEnemyId)
                {
                    bossController = enemy;
                    bossCombatant = enemy.GetComponent<Combatant>();
                    spawnPosition = enemy.transform.position;
                    if (!hasBaseStats)
                    {
                        baseAttackInterval = bossController.attackInterval;
                        baseAttackDamage = bossController.attackDamage;
                        hasBaseStats = true;
                    }
                    bossController.OnEvadeReset -= ResetEncounterState;
                    bossController.OnEvadeReset += ResetEncounterState;
                    break;
                }
            }
        }

        public void ResetEncounterState()
        {
            StopAllCoroutines();
            enraged = false;
            meteorActive = false;
            nextStompTime = 0f;
            nextMeteorTime = 0f;
            if (bossController != null && hasBaseStats)
            {
                bossController.attackInterval = baseAttackInterval;
                bossController.attackDamage = baseAttackDamage;
            }
            ResetCountForTest++;
            if (Network.CoopSessionConfig.IsMultiplayer && IsAuthoritativeRuntime && bossCombatant != null)
                Network.RealmWorldChannel.DispatchWorldBossEvent(EventReset, bossCombatant.transform.position, 0f, 0f);
        }

        public void ResetForTestFull()
        {
            ResetEncounterState();
            lootSpawned = false;
            IsBossDefeatedAuthoritative = false;
            StompCountForTest = 0;
            MeteorWarnCountForTest = 0;
            MeteorImpactCountForTest = 0;
            ResetCountForTest = 0;
            LastReplicatedEventKind = 0;
            if (spawnedHoardChest)
            {
                Destroy(spawnedHoardChest.gameObject);
                spawnedHoardChest = null;
            }
        }

        bool TryGetNearestAuthoritativeTarget(out Vector3 targetPos, out float bestDist)
        {
            targetPos = Vector3.zero;
            bestDist = float.MaxValue;
            if (!bossCombatant) return false;

            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                bool found = false;
                foreach (var cp in Network.RiftboundCoopPlayer.AllPlayers)
                {
                    if (!cp || !cp.IsSpawned || cp.Health.Value <= 0) continue;
                    float d = Vector3.Distance(cp.transform.position, bossCombatant.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        targetPos = cp.transform.position;
                        found = true;
                    }
                }
                return found;
            }

            if (!player) player = FindFirstObjectByType<PlayerController>();
            if (!player || !player.Stats || !player.Stats.Alive) return false;
            bestDist = Vector3.Distance(player.transform.position, bossCombatant.transform.position);
            targetPos = player.transform.position;
            return true;
        }

        void Update()
        {
            if (bossCombatant == null || bossController == null)
            {
                if (Time.frameCount % 90 == 0) FindBossReference();
                return;
            }

            // Multiplayer non-server clients only render replicated boss events & HUD; server owns all timers, damage, and loot.
            if (!IsAuthoritativeRuntime) return;

            if (!bossCombatant.Alive)
            {
                if (!lootSpawned)
                {
                    lootSpawned = true;
                    IsBossDefeatedAuthoritative = true;
                    OnBossDefeatedAuthoritative();
                }
                return;
            }

            // Belt-and-suspenders reset if boss returned to full HP while flagged enraged
            if (enraged && bossCombatant.Health >= bossCombatant.maxHealth * 0.99f && !bossController.IsChasing)
                ResetEncounterState();

            if (!TryGetNearestAuthoritativeTarget(out Vector3 nearestTargetPos, out float dist))
                return;

            // Quiet bypass protection: do not tick Stomp/Meteor/Enrage outside wake radius or while idle/unengaged
            if (dist > WakeRadius) return;
            bool engaged = bossController.IsChasing || bossCombatant.Health < bossCombatant.maxHealth;
            if (!engaged) return;

            // Phase 1 / Recurring: Seismic Stomp (Every 14s)
            if (Time.time >= nextStompTime)
            {
                nextStompTime = Time.time + StompInterval;
                TriggerSeismicStomp();
            }

            // Phase 2: Meteor Strike (Every 22s when in range)
            if (Time.time >= nextMeteorTime && dist < MeteorRange && !meteorActive)
            {
                nextMeteorTime = Time.time + MeteorInterval;
                StartCoroutine(MeteorStrikeRoutine(nearestTargetPos));
            }

            // Phase 3: Molten Enrage below 35% HP
            float hpPct = (float)bossCombatant.Health / Mathf.Max(1f, bossCombatant.maxHealth);
            if (hpPct <= EnrageHealthFraction && !enraged)
            {
                enraged = true;
                TriggerEnrage();
            }
        }

        void TriggerSeismicStomp()
        {
            StompCountForTest++;
            Vector3 bossPos = bossCombatant.transform.position;

            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                Network.RealmWorldChannel.DispatchWorldBossEvent(EventStomp, bossPos, StompRadius, 0f);
                foreach (var cp in Network.RiftboundCoopPlayer.AllPlayers)
                {
                    if (!cp || !cp.IsSpawned || cp.Health.Value <= 0) continue;
                    if (Vector3.Distance(cp.transform.position, bossPos) < StompRadius)
                        Network.RiftboundCoopPlayer.DispatchEnemyHitToPlayer(cp.OwnerClientId, StompDamage, BossEnemyId);
                }
                if (!Network.CoopSessionConfig.IsDedicatedServer)
                    PlayStompPresentation(bossPos);
                return;
            }

            PlayStompPresentation(bossPos);
            if (!player) player = FindFirstObjectByType<PlayerController>();
            if (!player) return;

            float d = Vector3.Distance(player.transform.position, bossPos);
            if (d < StompRadius && player.Stats.Alive)
            {
                player.Stats.Damage(StompDamage);
                CombatTextManager.Spawn(player.transform.position + Vector3.up * 1.5f, "-28 Fire (Stomp)", new Color(1f, 0.25f, 0.2f), false);
            }
        }

        IEnumerator MeteorStrikeRoutine(Vector3 targetPos)
        {
            meteorActive = true;
            MeteorWarnCountForTest++;

            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                Network.RealmWorldChannel.DispatchWorldBossEvent(EventMeteorWarn, targetPos, MeteorImpactRadius, MeteorWarningSeconds);
                if (!Network.CoopSessionConfig.IsDedicatedServer)
                    PlayMeteorWarnPresentation(targetPos);
            }
            else
            {
                PlayMeteorWarnPresentation(targetPos);
            }

            yield return new WaitForSeconds(MeteorWarningSeconds);
            if (!bossCombatant || !bossCombatant.Alive || (bossController != null && !bossController.IsChasing && bossCombatant.Health >= bossCombatant.maxHealth))
            {
                meteorActive = false;
                yield break;
            }

            MeteorImpactCountForTest++;
            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                Network.RealmWorldChannel.DispatchWorldBossEvent(EventMeteorImpact, targetPos, MeteorImpactRadius, 0f);
                foreach (var cp in Network.RiftboundCoopPlayer.AllPlayers)
                {
                    if (!cp || !cp.IsSpawned || cp.Health.Value <= 0) continue;
                    if (Vector3.Distance(cp.transform.position, targetPos) < MeteorImpactRadius)
                        Network.RiftboundCoopPlayer.DispatchEnemyHitToPlayer(cp.OwnerClientId, MeteorDamage, BossEnemyId);
                }
                if (!Network.CoopSessionConfig.IsDedicatedServer)
                    PlayMeteorImpactPresentation(targetPos);
            }
            else
            {
                if (!player) player = FindFirstObjectByType<PlayerController>();
                float distToImpact = player ? Vector3.Distance(player.transform.position, targetPos) : float.MaxValue;
                if (player && distToImpact < MeteorImpactRadius && player.Stats.Alive)
                {
                    player.Stats.Damage(MeteorDamage);
                    CombatTextManager.Spawn(player.transform.position + Vector3.up * 2.0f, "-45 METEOR IMPACT!", new Color(1f, 0.1f, 0.1f), true);
                }
                else
                {
                    CombatTextManager.Spawn(targetPos + Vector3.up * 1.0f, "Meteor Evaded!", new Color(0.4f, 1f, 0.4f), false);
                }
            }
            meteorActive = false;
        }

        void TriggerEnrage()
        {
            if (bossController != null)
            {
                bossController.attackInterval = 1.05f; // Rapid strikes
                bossController.attackDamage = 32f;
            }
            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                Network.RealmWorldChannel.DispatchWorldBossEvent(EventEnrage, bossCombatant.transform.position, 0f, 0f);
                if (!Network.CoopSessionConfig.IsDedicatedServer)
                    PlayEnragePresentation(bossCombatant.transform.position);
            }
            else
            {
                PlayEnragePresentation(bossCombatant.transform.position);
            }
        }

        Vector3 ComputeHoardChestPosition()
        {
            Vector3 chestPos = spawnPosition + new Vector3(0f, 0.6f, 3.5f);
            if (Physics.Raycast(chestPos + Vector3.up * 5f, Vector3.down, out var gHit, 15f, ~(1 << 2), QueryTriggerInteraction.Ignore))
                chestPos.y = gHit.point.y;
            return chestPos;
        }

        void OnBossDefeatedAuthoritative()
        {
            hoardChestPosition = ComputeHoardChestPosition();
            EnsureHoardChestSpawned(hoardChestPosition);

            string[] epicDrops = { "worldbreaker-maul", "crown-of-molten-ruin", "cinderfury-signet" };

            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                Network.RealmWorldChannel.DispatchWorldBossEvent(EventDefeated, hoardChestPosition, 0f, 0f);
                foreach (var cp in Network.RiftboundCoopPlayer.AllPlayers)
                {
                    if (!cp || !cp.IsSpawned || cp.Health.Value <= 0) continue;
                    if (Vector3.Distance(cp.transform.position, spawnPosition) <= 45f)
                    {
                        string chosen = epicDrops[UnityEngine.Random.Range(0, epicDrops.Length)];
                        cp.GrantServerWorldBossKillReward(DefeatXp, DefeatGold, chosen, "Titanbreaker");
                    }
                }
                if (!Network.CoopSessionConfig.IsDedicatedServer)
                    PlayDefeatedPresentation();
                return;
            }

            PlayDefeatedPresentation();
            if (!player) player = FindFirstObjectByType<PlayerController>();
            if (player)
            {
                player.AddExperience(DefeatXp);
                player.AddGoldDirect(DefeatGold);
                string chosen = epicDrops[UnityEngine.Random.Range(0, epicDrops.Length)];
                player.GrantItem(chosen);
                var item = ItemCatalog.Get(chosen);
                if (item != null)
                {
                    ChatSystem.LogLoot($"World Boss Loot: Received [{item.name}] ({item.rarity} {item.slot})!");
                    CombatTextManager.Spawn(player.transform.position + Vector3.up * 3.0f, $"Looted: {item.name}!", item.color, true);
                }
            }
            PlayerTitleSystem.SetActiveTitle("Titanbreaker");
            ChatSystem.LogSystem("<color=#e0b034>★ Title Unlocked: 'Titanbreaker'!</color>");
        }

        void EnsureHoardChestSpawned(Vector3 chestPos)
        {
            if (spawnedHoardChest != null) return;
            foreach (var existing in FindObjectsByType<LootChest>(FindObjectsSortMode.None))
            {
                if (existing && existing.chestId == HoardChestId)
                {
                    spawnedHoardChest = existing;
                    return;
                }
            }

            var chestPrefab = Resources.Load<GameObject>("Props/TreasureChest");
            GameObject chestObj;
            if (chestPrefab != null)
            {
                chestObj = Instantiate(chestPrefab, chestPos, Quaternion.identity);
                chestObj.name = "Molten Worldbreaker Hoard Chest";
                chestObj.transform.localScale = Vector3.one * 1.35f;
            }
            else
            {
                chestObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chestObj.name = "Molten Worldbreaker Hoard Chest";
                chestObj.transform.position = chestPos + Vector3.up * 0.55f;
                chestObj.transform.localScale = new Vector3(1.8f, 1.1f, 1.2f);
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    var chestMat = new Material(shader);
                    chestMat.SetColor("_BaseColor", new Color(0.55f, 0.35f, 0.12f));
                    chestMat.SetFloat("_Metallic", 0.35f);
                    chestMat.SetFloat("_Smoothness", 0.45f);
                    chestObj.GetComponent<Renderer>().sharedMaterial = chestMat;
                }
            }

            var chest = chestObj.GetComponent<LootChest>() ?? chestObj.AddComponent<LootChest>();
            chest.chestId = HoardChestId;
            spawnedHoardChest = chest;

            if (!Network.CoopSessionConfig.IsDedicatedServer)
            {
                var lt = chestObj.AddComponent<Light>();
                lt.type = LightType.Point;
                lt.color = new Color(1.0f, 0.45f, 0.05f);
                lt.range = 14f;
                lt.intensity = 4.2f;
            }
        }

        public void ApplyReplicatedBossEvent(byte eventKind, Vector3 pos, float radius, float duration)
        {
            LastReplicatedEventKind = eventKind;
            switch (eventKind)
            {
                case EventStomp:
                    PlayStompPresentation(pos);
                    break;
                case EventMeteorWarn:
                    PlayMeteorWarnPresentation(pos);
                    break;
                case EventMeteorImpact:
                    PlayMeteorImpactPresentation(pos);
                    break;
                case EventEnrage:
                    enraged = true;
                    PlayEnragePresentation(pos);
                    break;
                case EventDefeated:
                    lootSpawned = true;
                    IsBossDefeatedAuthoritative = true;
                    hoardChestPosition = pos;
                    EnsureHoardChestSpawned(pos);
                    PlayDefeatedPresentation();
                    break;
                case EventReset:
                    enraged = false;
                    meteorActive = false;
                    break;
            }
        }

        public void ApplyReplicatedBossSnapshot(bool defeated, Vector3 chestPos, bool bossEnraged)
        {
            enraged = bossEnraged;
            if (defeated)
            {
                lootSpawned = true;
                IsBossDefeatedAuthoritative = true;
                hoardChestPosition = chestPos;
                EnsureHoardChestSpawned(chestPos);
            }
        }

        void PlayStompPresentation(Vector3 bossPos)
        {
            ChatSystem.LogCombat("<color=#ff6633>[Gorgonax]</color> slams the earth! The ground violently shakes!");
            CombatTextManager.Spawn(bossPos + Vector3.up * 4.2f, "★ SEISMIC STOMP! ★", new Color(1f, 0.45f, 0.1f), true);
            OrbitCamera.Shake(0.28f, 0.35f);
        }

        void PlayMeteorWarnPresentation(Vector3 targetPos)
        {
            ChatSystem.LogCombat("<color=#ff4422>[Gorgonax]</color> summons a Molten Meteor! MOVE!");
            CombatTextManager.Spawn(targetPos + Vector3.up * 0.5f, "⚠ METEOR IMPACT ZONE ⚠", new Color(1f, 0.2f, 0.2f), true);
            SpellFX.CreateHeavySlam(targetPos, MeteorImpactRadius);
        }

        void PlayMeteorImpactPresentation(Vector3 targetPos)
        {
            SpellFX.CreateHeavySlam(targetPos, MeteorImpactRadius);
            OrbitCamera.Shake(0.22f, 0.25f);
        }

        void PlayEnragePresentation(Vector3 bossPos)
        {
            ChatSystem.LogCombat("<color=#ff2222><b>[Gorgonax the Worldbreaker]</b></color> enters a MOLTEN FRENZY! Attack speed increased!");
            CombatTextManager.Spawn(bossPos + Vector3.up * 4.5f, "🔥 MOLTEN ENRAGE! 🔥", Color.red, true);
        }

        void PlayDefeatedPresentation()
        {
            ChatSystem.LogSystem("<color=#ffd700><b>[WORLD EVENT]</b></color> Gorgonax the Worldbreaker has been vanquished!");
            if (!player) player = FindFirstObjectByType<PlayerController>();
            if (player)
                CombatTextManager.Spawn(player.transform.position + Vector3.up * 2.5f, "WORLD BOSS DEFEATED! +150 XP", new Color(1f, 0.85f, 0.2f), true);
            AudioEffects.PlayLootCoin();
        }

        void InitTextures()
        {
            if (barBgTex != null) return;
            barBgTex = MakeTex(new Color(0.04f, 0.05f, 0.07f, 0.95f));
            borderTex = MakeTex(new Color(0.85f, 0.65f, 0.22f, 0.98f)); // Ornate gold
            hpTex = MakeTex(new Color(0.85f, 0.15f, 0.15f, 1f));       // Crimson
            skullBgTex = MakeTex(new Color(0.18f, 0.04f, 0.04f, 0.98f));

            bossNameStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            bossNameStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);

            skullStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            skullStyle.normal.textColor = new Color(1f, 0.2f, 0.2f);

            subTitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            subTitleStyle.normal.textColor = new Color(0.95f, 0.55f, 0.25f);

            pctStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            pctStyle.normal.textColor = Color.white;
        }

        Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        public void DrawBossHealthBar(float screenW, float screenH)
        {
            if (bossCombatant == null || !bossCombatant.Alive || FrontendMenu.IsOpen) return;
            if (!player) player = FindFirstObjectByType<PlayerController>();
            if (!player) return;
            float dist = Vector3.Distance(player.transform.position, bossCombatant.transform.position);
            if (dist > 35f) return;

            InitTextures();

            float barW = 440f;
            float barH = 34f;
            float x = (screenW - barW) * 0.5f;
            float y = 55f;

            // Gold Dragon Ornate Outer Border
            GUI.DrawTexture(new Rect(x - 3, y - 3, barW + 6, barH + 6), borderTex);
            GUI.DrawTexture(new Rect(x, y, barW, barH), barBgTex);

            // Skull Level Badge on the left
            float skullSize = barH + 8;
            GUI.DrawTexture(new Rect(x - skullSize - 4, y - 4, skullSize, skullSize), borderTex);
            GUI.DrawTexture(new Rect(x - skullSize - 2, y - 2, skullSize - 4, skullSize - 4), skullBgTex);
            GUI.Label(new Rect(x - skullSize - 2, y + 2, skullSize - 4, skullSize - 8), "☠ 10", skullStyle);

            // Health Bar Fill
            float pct = Mathf.Clamp01((float)bossCombatant.Health / bossCombatant.maxHealth);
            GUI.DrawTexture(new Rect(x + 2, y + 2, (barW - 4) * pct, barH - 4), hpTex);

            // Boss Title & Subtitle
            GUI.Label(new Rect(x, y - 24, barW, 22), "GORGONAX THE WORLDBREAKER", bossNameStyle);
            GUI.Label(new Rect(x, y + 2, barW, 16), "World Boss — Magma Colossus", subTitleStyle);
            GUI.Label(new Rect(x, y + 16, barW, 16), $"{Mathf.CeilToInt(bossCombatant.Health)} / {bossCombatant.maxHealth} ({Mathf.RoundToInt(pct * 100)}%)", pctStyle);
        }
    }
}
