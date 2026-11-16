using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Riftbound.Network
{
    public class RiftboundCoopPlayer : NetworkBehaviour
    {
        public static readonly List<RiftboundCoopPlayer> AllPlayers = new List<RiftboundCoopPlayer>();
        public static RiftboundCoopPlayer LocalOwner { get; private set; }

        public static RiftboundCoopPlayer FindByClientId(ulong clientId)
        {
            foreach (var p in AllPlayers)
                if (p && p.IsSpawned && p.OwnerClientId == clientId) return p;
            return null;
        }

        public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
            new FixedString64Bytes("Adventurer"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<FixedString32Bytes> Race = new NetworkVariable<FixedString32Bytes>(
            new FixedString32Bytes("Human"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<FixedString32Bytes> ClassName = new NetworkVariable<FixedString32Bytes>(
            new FixedString32Bytes("Warrior"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> Level = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Experience = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Gold = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> IsMounted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<FixedString32Bytes> MountId = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes("horse"), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> SpawnSlot = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> QuestIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> QuestAccepted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> QuestKills = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<byte> ReadyStatus = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // 0=None, 1=Ready, 2=NotReady

        PlayerController localPlayer;
        GameObject remoteVisualRoot;
        GameObject remoteMountVisual;
        Animator remoteAnimator;
        string currentRemoteAnim = "";
        string builtRace = "";
        string builtClass = "";
        float syncTimer;
        float lastServerMoveTime;
        bool hostTeleportAuthorized;
        Vector3 authorizedTeleportTarget;
        float actionAnimUntil;
        Vector3 lastLocalPos;
        bool hasLastLocalPos;
        bool spawnPositioned;
        readonly Dictionary<string, float> serverAbilityCooldowns = new Dictionary<string, float>();
        readonly HashSet<string> serverDefeatedEnemyKeys = new HashSet<string>();
        readonly HashSet<string> serverCompletedEvents = new HashSet<string>();
        readonly HashSet<string> serverDiscoveredLandmarks = new HashSet<string>();
        readonly HashSet<string> serverCompletedFactionQuests = new HashSet<string>();
        int serverPotions = 3;
        bool serverTemperedWeapon;
        float lastServerPotionTime = -10f;
        float lastServerRespawnTime = -10f;
        static GUIStyle nameplateStyle;

        public override void OnNetworkSpawn()
        {
            if (!AllPlayers.Contains(this)) AllPlayers.Add(this);
            lastServerMoveTime = Time.time;

            if (IsServer)
            {
                SpawnSlot.Value = OwnerClientId == NetworkManager.ServerClientId ? 0 : AllPlayers.Count - 1;
                MaxHealth.Value = 100;
                Health.Value = 100;
                Level.Value = 1;
                if (OwnerClientId != NetworkManager.ServerClientId && transform.position.sqrMagnitude < 0.1f)
                {
                    var lp = FindFirstObjectByType<PlayerController>();
                    Vector3 basePos = lp ? lp.transform.position : new Vector3(0f, 1f, 10f);
                    Vector3 slotPos = ComputeSpawnPosition(basePos, SpawnSlot.Value);
                    AuthorizeHostTeleport(slotPos);
                }
            }

            if (IsOwner)
            {
                LocalOwner = this;
                Gold.OnValueChanged += OnAuthoritativeGoldChanged;
                BindToLocalPlayer();
            }
            else
            {
                Race.OnValueChanged += OnIdentityChanged;
                ClassName.OnValueChanged += OnIdentityChanged;
                IsMounted.OnValueChanged += OnMountChanged;
                RebuildRemoteVisual();
            }
        }

        public override void OnNetworkDespawn()
        {
            AllPlayers.Remove(this);
            if (LocalOwner == this) LocalOwner = null;
            Gold.OnValueChanged -= OnAuthoritativeGoldChanged;
            Race.OnValueChanged -= OnIdentityChanged;
            ClassName.OnValueChanged -= OnIdentityChanged;
            IsMounted.OnValueChanged -= OnMountChanged;
            if (remoteVisualRoot) Destroy(remoteVisualRoot);
            if (remoteMountVisual) Destroy(remoteMountVisual);
        }

        void OnAuthoritativeGoldChanged(int prev, int next)
        {
            if (IsOwner && localPlayer) localPlayer.SetAuthoritativeGold(next);
        }

        void OnIdentityChanged(FixedString32Bytes prev, FixedString32Bytes next)
        {
            if (!IsOwner) RebuildRemoteVisual();
        }

        void OnMountChanged(bool prev, bool next)
        {
            if (!IsOwner) UpdateRemoteMountVisual();
        }

        void BindToLocalPlayer()
        {
            localPlayer = FindFirstObjectByType<PlayerController>();
            if (!localPlayer) return;

            if (!spawnPositioned)
            {
                spawnPositioned = true;
                if (OwnerClientId != NetworkManager.ServerClientId)
                {
                    var cc = localPlayer.GetComponent<CharacterController>();
                    if (cc) cc.enabled = false;
                    localPlayer.transform.position = transform.position;
                    if (cc) cc.enabled = true;
                }
                else
                {
                    transform.position = localPlayer.transform.position;
                    transform.rotation = localPlayer.transform.rotation;
                }
            }

            string pName = string.IsNullOrWhiteSpace(localPlayer.CharacterName) ? "Adventurer" : localPlayer.CharacterName;
            string pRace = string.IsNullOrWhiteSpace(localPlayer.CharacterRace) ? "Human" : localPlayer.CharacterRace;
            string pClass = string.IsNullOrWhiteSpace(localPlayer.CharacterClass) ? "Warrior" : localPlayer.CharacterClass;
            SubmitCharacterIdentityServerRpc(pName, pRace, pClass);
        }

        public static Vector3 ComputeSpawnPosition(Vector3 basePos, int slot)
        {
            if (slot <= 0) return basePos;
            float angle = (slot - 1) * 65f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * 2.6f, 0f, Mathf.Sin(angle) * 2.6f);
            Vector3 candidate = basePos + offset;
            if (Terrain.activeTerrain)
            {
                float h = Terrain.activeTerrain.SampleHeight(candidate) + Terrain.activeTerrain.transform.position.y;
                candidate.y = Mathf.Max(h + 0.05f, basePos.y);
            }
            return candidate;
        }

        void RebuildRemoteVisual()
        {
            string r = Race.Value.ToString();
            string c = ClassName.Value.ToString();
            if (string.IsNullOrWhiteSpace(r)) r = "Human";
            if (string.IsNullOrWhiteSpace(c)) c = "Warrior";
            if (remoteVisualRoot && builtRace == r && builtClass == c) return;

            if (remoteVisualRoot) Destroy(remoteVisualRoot);
            builtRace = r;
            builtClass = c;
            remoteVisualRoot = CharacterVisualFactory.CreateVisualInstance(r, c, transform);
            if (remoteVisualRoot)
            {
                remoteVisualRoot.name = $"CoopVisual_{r}_{c}";
                remoteVisualRoot.transform.localPosition = Vector3.zero;
                remoteVisualRoot.transform.localRotation = Quaternion.identity;
                foreach (var cam in remoteVisualRoot.GetComponentsInChildren<Camera>(true)) Destroy(cam);
                foreach (var al in remoteVisualRoot.GetComponentsInChildren<AudioListener>(true)) Destroy(al);
                remoteAnimator = remoteVisualRoot.GetComponentInChildren<Animator>();
            }
            UpdateRemoteMountVisual();
        }

        void UpdateRemoteMountVisual()
        {
            if (IsMounted.Value)
            {
                if (!remoteMountVisual)
                {
                    remoteMountVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    remoteMountVisual.name = "RemoteMountSteed";
                    var col = remoteMountVisual.GetComponent<Collider>();
                    if (col) Destroy(col);
                    remoteMountVisual.transform.SetParent(transform, false);
                    remoteMountVisual.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                    remoteMountVisual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    remoteMountVisual.transform.localScale = new Vector3(0.65f, 0.95f, 0.65f);
                    var rend = remoteMountVisual.GetComponent<Renderer>();
                    if (rend)
                    {
                        string mid = MountId.Value.ToString().ToLowerInvariant();
                        Color c = mid.Contains("zebra") ? Color.white : (mid.Contains("griffon") || mid.Contains("wyvern") ? new Color(0.85f, 0.7f, 0.3f) : new Color(0.45f, 0.28f, 0.16f));
                        rend.material.color = c;
                    }
                }
                if (remoteVisualRoot) remoteVisualRoot.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            }
            else
            {
                if (remoteMountVisual) { Destroy(remoteMountVisual); remoteMountVisual = null; }
                if (remoteVisualRoot) remoteVisualRoot.transform.localPosition = Vector3.zero;
            }
        }

        void Update()
        {
            if (!IsSpawned) return;

            if (IsOwner)
            {
                if (!localPlayer) BindToLocalPlayer();
                if (!localPlayer) return;

                syncTimer += Time.deltaTime;
                if (syncTimer >= 0.05f)
                {
                    float dt = syncTimer;
                    syncTimer = 0f;
                    Vector3 curPos = localPlayer.transform.position;
                    float spd = hasLastLocalPos ? Vector3.Distance(curPos, lastLocalPos) / Mathf.Max(0.01f, dt) : 0f;
                    lastLocalPos = curPos;
                    hasLastLocalPos = true;
                    SubmitMoveStateServerRpc(curPos, localPlayer.transform.eulerAngles.y, spd);
                }
            }
            else
            {
                UpdateRemoteAnimation();
            }
        }

        void UpdateRemoteAnimation()
        {
            if (!remoteAnimator || Time.time < actionAnimUntil) return;
            string desired = Health.Value <= 0
                ? "Hit"
                : (IsMounted.Value ? "Ride" : (MoveSpeed.Value > 4.2f ? "Run" : (MoveSpeed.Value > 0.25f ? "Walk" : "Idle")));
            if (desired == currentRemoteAnim) return;
            if (remoteAnimator.HasState(0, Animator.StringToHash(desired)))
            {
                remoteAnimator.CrossFadeInFixedTime(desired, 0.14f, 0, 0f);
                currentRemoteAnim = desired;
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void SubmitCharacterIdentityServerRpc(string pName, string race, string cls)
        {
            // Only accept valid race/class strings; host owns starting combat vitals/progression
            string cleanRace = string.IsNullOrWhiteSpace(race) ? "Human" : race.Trim();
            string cleanClass = string.IsNullOrWhiteSpace(cls) ? "Warrior" : cls.Trim();
            PlayerName.Value = new FixedString64Bytes(string.IsNullOrWhiteSpace(pName) ? $"Player {OwnerClientId}" : pName.Trim());
            Race.Value = new FixedString32Bytes(cleanRace);
            ClassName.Value = new FixedString32Bytes(cleanClass);
            int baseHp = cleanClass == "Warrior" || cleanClass == "Paladin" ? 130 : (cleanClass == "Mage" ? 90 : 105);
            MaxHealth.Value = baseHp + (Level.Value - 1) * 15;
            Health.Value = MaxHealth.Value;
        }

        // ── O-01 Authoritative Movement ─────────────────────────────────────────────────

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void SubmitMoveStateServerRpc(Vector3 pos, float yaw, float speed)
        {
            float dt = Mathf.Clamp(Time.time - lastServerMoveTime, 0.02f, 0.5f);
            lastServerMoveTime = Time.time;

            if (Health.Value <= 0) return;

            float maxSpeed = IsMounted.Value ? 15.5f : 9.5f;
            float maxStep = maxSpeed * dt + 1.5f;
            float dist = Vector3.Distance(transform.position, pos);

            if (hostTeleportAuthorized)
            {
                if (Vector3.Distance(pos, authorizedTeleportTarget) <= 3.5f)
                {
                    hostTeleportAuthorized = false;
                    transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
                }
                else
                {
                    return; // Ignore stale pre-teleport movement packets until client acknowledges teleport
                }
            }
            else if (dist <= maxStep)
            {
                transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            }
            else
            {
                Vector3 dir = (pos - transform.position).normalized;
                Vector3 clamped = transform.position + dir * maxStep;
                transform.SetPositionAndRotation(clamped, Quaternion.Euler(0f, yaw, 0f));
                CorrectPositionClientRpc(clamped, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }

            MoveSpeed.Value = Mathf.Clamp(speed, 0f, maxSpeed);
        }

        public void AuthorizeHostTeleport(Vector3 destination)
        {
            if (!IsServer) return;
            hostTeleportAuthorized = true;
            authorizedTeleportTarget = destination;
            transform.position = destination;
            CorrectPositionClientRpc(destination, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void CorrectPositionClientRpc(Vector3 authoritativePos, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            var cc = localPlayer.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            localPlayer.transform.position = authoritativePos;
            if (cc) cc.enabled = true;
        }

        // ── O-01 Authoritative Mounts & Animations ──────────────────────────────────────

        public void SendSpoofedPositionForTest(Vector3 targetPos)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || !IsOwner) return;
            SubmitMoveStateServerRpc(targetPos, transform.eulerAngles.y, 10f);
        }

        public void RequestSetMount(bool mounted, string mountId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            RequestSetMountServerRpc(mounted, string.IsNullOrEmpty(mountId) ? "horse" : mountId);
        }

        int ServerGreenhollowCount() =>
            (serverDiscoveredLandmarks.Contains("willow-crossing") ? 1 : 0) +
            (serverDiscoveredLandmarks.Contains("greenhollow-post") ? 1 : 0) +
            (serverDiscoveredLandmarks.Contains("ancient-grove") ? 1 : 0);

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestSetMountServerRpc(bool mounted, string mountId)
        {
            if (Health.Value <= 0) { IsMounted.Value = false; return; }
            string normalized = string.IsNullOrEmpty(mountId) ? "horse" : mountId.ToLowerInvariant();
            if (mounted && normalized == "zebra" && ServerGreenhollowCount() < 3) return;
            IsMounted.Value = mounted;
            MountId.Value = new FixedString32Bytes(normalized);
        }

        public void TriggerActionAnim(string animName)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            if (IsServer) BroadcastActionAnimClientRpc(animName);
            else RequestActionAnimServerRpc(animName);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestActionAnimServerRpc(string animName)
        {
            BroadcastActionAnimClientRpc(animName);
        }

        [Rpc(SendTo.Everyone)]
        void BroadcastActionAnimClientRpc(string animName)
        {
            if (IsOwner || !remoteAnimator || string.IsNullOrEmpty(animName)) return;
            if (remoteAnimator.HasState(0, Animator.StringToHash(animName)))
            {
                remoteAnimator.CrossFadeInFixedTime(animName, 0.08f, 0, 0f);
                actionAnimUntil = Time.time + 0.55f;
                currentRemoteAnim = animName;
            }
        }

        // ── O-01 Host-Authoritative Combat ──────────────────────────────────────────────

        public void RequestAbilityAttack(Combatant targetCombatant, string abilityId = "melee")
        {
            if (!targetCombatant || !targetCombatant.Alive) return;
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                return;
            }

            var enemy = targetCombatant.GetComponent<EnemyController>();
            if (!enemy) return;

            if (IsServer)
            {
                ValidateAndExecuteAbilityOnServer(enemy, abilityId);
            }
            else
            {
                RequestAbilityAttackServerRpc(enemy.NetworkKey, abilityId);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestAbilityAttackServerRpc(string enemyKey, string abilityId)
        {
            var enemy = EnemyController.FindByKey(enemyKey);
            if (!enemy) return;
            ValidateAndExecuteAbilityOnServer(enemy, abilityId);
        }

        void ValidateAndExecuteAbilityOnServer(EnemyController enemy, string abilityId)
        {
            if (!IsServer || Health.Value <= 0 || !enemy) return;
            var stats = enemy.GetComponent<Combatant>();
            if (!stats || !stats.Alive) return;

            string key = string.IsNullOrEmpty(abilityId) ? "melee" : abilityId.ToLowerInvariant();
            string cls = ClassName.Value.ToString();
            if ((key == "fireball" || key == "frostnova") && cls != "Mage") return;
            if (key == "eviscerate" && cls != "Rogue") return;
            if (key == "consecration" && cls != "Paladin") return;
            if (key == "whirlwind" && cls != "Warrior") return;

            bool isAoe = key == "frostnova" || key == "consecration" || key == "whirlwind";
            string cdKey = isAoe ? $"{key}:{enemy.NetworkKey}" : key;
            float cd = key == "melee" ? 0.35f : 0.8f;
            if (serverAbilityCooldowns.TryGetValue(cdKey, out float readyAt) && Time.time < readyAt) return;
            serverAbilityCooldowns[cdKey] = Time.time + cd;

            float maxRange = key == "fireball" ? 30f : (isAoe ? 8.5f : 5.0f);
            if (Vector3.Distance(transform.position, enemy.transform.position) > maxRange) return;

            // LOS check (ignore triggers/characters)
            Vector3 eye = transform.position + Vector3.up * 1.2f;
            Vector3 tgt = enemy.transform.position + Vector3.up * 1.1f;
            if (Physics.Linecast(eye, tgt, out RaycastHit hit, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                if (hit.collider && !hit.collider.transform.IsChildOf(enemy.transform) && !hit.collider.transform.IsChildOf(transform))
                    return;
            }

            if (key == "interrupt")
            {
                CastBarSystem.InterruptTarget(stats, "Interrupt");
                BroadcastActionAnimClientRpc("Attack");
                return;
            }

            float baseDmg = key switch
            {
                "fireball" => 34f,
                "frostnova" => 24f,
                "consecration" => 24f,
                "whirlwind" => 28f,
                "eviscerate" => 46f,
                _ => 18f
            };
            float computedDamage = baseDmg + (Level.Value - 1) * 4f;
            if (Race.Value.ToString() == "Brute") computedDamage *= 1.15f;

            stats.Damage(computedDamage);
            BroadcastActionAnimClientRpc(key == "fireball" || key == "frostnova" || key == "consecration" ? "Cast" : "Attack");
            BroadcastEnemyState(enemy);
        }

        public void RequestAbilityHeal(int amount, string abilityId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || amount <= 0) return;
            if (IsServer) ApplyServerAbilityHeal(amount, abilityId);
            else RequestAbilityHealServerRpc(amount, abilityId);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestAbilityHealServerRpc(int amount, string abilityId) => ApplyServerAbilityHeal(amount, abilityId);

        void ApplyServerAbilityHeal(int amount, string abilityId)
        {
            if (!IsServer || Health.Value <= 0) return;
            string key = string.IsNullOrEmpty(abilityId) ? "heal" : abilityId.ToLowerInvariant();
            string cls = ClassName.Value.ToString();
            if (key == "arcane_ward" && cls != "Mage") return;
            if (key == "holy_light" && cls != "Paladin") return;
            if (key == "second_wind" && cls != "Warrior") return;

            if (serverAbilityCooldowns.TryGetValue("heal:" + key, out float readyAt) && Time.time < readyAt) return;
            serverAbilityCooldowns["heal:" + key] = Time.time + 8.0f;

            int clamped = Mathf.Clamp(amount, 1, 65);
            Health.Value = Mathf.Min(MaxHealth.Value, Health.Value + clamped);
        }

        public void BroadcastEnemyState(EnemyController enemy)
        {
            if (!IsServer || !IsSpawned || !enemy) return;
            var c = enemy.GetComponent<Combatant>();
            if (!c) return;
            BroadcastEnemyStateClientRpc(enemy.NetworkKey, c.Health, c.maxHealth, c.Alive, enemy.transform.position, enemy.transform.eulerAngles.y);
        }

        [Rpc(SendTo.Everyone)]
        void BroadcastEnemyStateClientRpc(string enemyKey, float hp, float maxHp, bool alive, Vector3 pos, float yaw)
        {
            if (IsServer) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedState(hp, maxHp, alive, pos, yaw);
        }

        public static void DispatchEnemyHitToPlayer(ulong targetClientId, float damage, string enemyKey)
        {
            var avatar = FindByClientId(targetClientId);
            if (!avatar || !avatar.IsServer) return;

            int dmgInt = Mathf.Max(1, Mathf.RoundToInt(damage));
            avatar.Health.Value = Mathf.Max(0, avatar.Health.Value - dmgInt);
            avatar.ReceiveEnemyHitClientRpc(damage, enemyKey, avatar.RpcTarget.Single(targetClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveEnemyHitClientRpc(float damage, string enemyKey, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer || !localPlayer.Stats) return;
            localPlayer.Stats.Damage(damage);
        }

        public void RequestRespawn()
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            if (IsServer) ExecuteAuthoritativeRespawn();
            else RequestRespawnServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestRespawnServerRpc() => ExecuteAuthoritativeRespawn();

        void ExecuteAuthoritativeRespawn()
        {
            if (!IsServer) return;
            bool fellOutOfWorld = transform.position.y < -10f;
            if (Health.Value > 0 && !fellOutOfWorld) return;
            if (Time.time < lastServerRespawnTime + 1.5f) return;
            lastServerRespawnTime = Time.time;
            if (Health.Value <= 0)
                Health.Value = MaxHealth.Value;
            Vector3 baseSpawn = new Vector3(0f, 1f, 10f);
            var lp = FindFirstObjectByType<PlayerController>();
            if (lp) baseSpawn = lp.GetRacialSpawnPosition();
            Vector3 slotSpawn = ComputeSpawnPosition(baseSpawn, SpawnSlot.Value);
            AuthorizeHostTeleport(slotSpawn);
            if (IsOwner && localPlayer)
                localPlayer.ApplyAuthorizedRespawn(slotSpawn, Health.Value);
            else
                ApproveRespawnClientRpc(slotSpawn, Health.Value, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveRespawnClientRpc(Vector3 safeSpawn, int serverHp, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.ApplyAuthorizedRespawn(safeSpawn, serverHp);
        }

        public void RequestUsePotion()
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            if (IsServer) ApplyServerPotionHeal();
            else RequestUsePotionServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestUsePotionServerRpc() => ApplyServerPotionHeal();

        void ApplyServerPotionHeal()
        {
            if (!IsServer || Health.Value <= 0 || Health.Value >= MaxHealth.Value) return;
            if (serverPotions <= 0 || Time.time < lastServerPotionTime + 1.5f) return;
            serverPotions--;
            lastServerPotionTime = Time.time;
            Health.Value = Mathf.Min(MaxHealth.Value, Health.Value + 70);
            if (IsOwner && localPlayer)
                localPlayer.ApplyAuthorizedPotionUse(serverPotions, Health.Value);
            else
                ApproveUsePotionClientRpc(serverPotions, Health.Value, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveUsePotionClientRpc(int remainingPotions, int newHealth, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.ApplyAuthorizedPotionUse(remainingPotions, newHealth);
        }

        public void RequestTransaction(string txId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || string.IsNullOrEmpty(txId)) return;
            if (IsServer)
            {
                if (ExecuteServerTransaction(txId) && localPlayer)
                    localPlayer.ApplyAuthorizedTransaction(txId, Gold.Value);
            }
            else
            {
                RequestTransactionServerRpc(txId);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestTransactionServerRpc(string txId)
        {
            if (ExecuteServerTransaction(txId))
                ApproveTransactionClientRpc(txId, Gold.Value, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        }

        bool ExecuteServerTransaction(string txId)
        {
            if (!IsServer || Health.Value <= 0) return false;
            if (txId == "buy_potion")
            {
                if (Gold.Value < 10 || serverPotions >= 10) return false;
                serverPotions++;
                Gold.Value -= 10;
                return true;
            }
            if (txId == "temper_weapon")
            {
                if (Gold.Value < 45 || serverTemperedWeapon) return false;
                serverTemperedWeapon = true;
                Gold.Value -= 45;
                return true;
            }
            if (txId == "rest")
            {
                if (Gold.Value < 5 || Health.Value >= MaxHealth.Value) return false;
                Gold.Value -= 5;
                Health.Value = MaxHealth.Value;
                return true;
            }
            return false;
        }

        public void RequestDiscoverLandmark(string landmarkId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || string.IsNullOrEmpty(landmarkId)) return;
            if (IsServer)
            {
                if (TryValidateLandmarkOnServer(landmarkId, out int xpGranted) && localPlayer)
                {
                    localPlayer.ApplyAuthorizedDiscovery(landmarkId, Gold.Value, xpGranted);
                }
            }
            else
            {
                RequestDiscoverLandmarkServerRpc(landmarkId);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestDiscoverLandmarkServerRpc(string landmarkId)
        {
            if (TryValidateLandmarkOnServer(landmarkId, out int xpGranted))
                ApproveDiscoverLandmarkClientRpc(landmarkId, Gold.Value, xpGranted, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        }

        bool TryValidateLandmarkOnServer(string landmarkId, out int xpGranted)
        {
            xpGranted = 0;
            if (!IsServer || Health.Value <= 0 || string.IsNullOrEmpty(landmarkId)) return false;

            Landmark match = null;
            foreach (var lm in FindObjectsByType<Landmark>(FindObjectsSortMode.None))
            {
                if (lm && lm.landmarkId == landmarkId) { match = lm; break; }
            }
            if (!match || Vector3.Distance(transform.position, match.transform.position) > 14.0f) return false;
            if (!serverDiscoveredLandmarks.Add(landmarkId)) return false;

            bool greenhollowComplete = (landmarkId == "willow-crossing" || landmarkId == "greenhollow-post" || landmarkId == "ancient-grove") && ServerGreenhollowCount() == 3;
            xpGranted = (landmarkId == "scarlet-crypts" || landmarkId == "redridge-frontier") ? 60 : (greenhollowComplete ? 85 : 35);
            GrantAuthoritativeProgress(xpGranted, 15);
            return true;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveDiscoverLandmarkClientRpc(string landmarkId, int newGold, int xpGranted, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.ApplyAuthorizedDiscovery(landmarkId, newGold, xpGranted);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveTransactionClientRpc(string txId, int newGold, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.ApplyAuthorizedTransaction(txId, newGold);
        }

        // ── O-01 Cooperative Progression (Host-Validated) ───────────────────────────────

        public static void DispatchEnemyDefeatedToEligiblePlayers(EnemyController enemy)
        {
            if (!enemy) return;
            foreach (var cp in AllPlayers)
            {
                if (!cp || !cp.IsSpawned || cp.Health.Value <= 0) continue;
                if (Vector3.Distance(cp.transform.position, enemy.transform.position) <= 55f)
                {
                    if (CoopWorldManager.Instance == null || CoopWorldManager.Instance.TryCreditKillForClient(cp.OwnerClientId, enemy.NetworkKey))
                    {
                        cp.serverDefeatedEnemyKeys.Add(enemy.NetworkKey);
                        if (cp.QuestAccepted.Value) cp.QuestKills.Value++;
                        cp.GrantAuthoritativeProgress(25, 8);
                        cp.ReceiveEnemyDefeatedClientRpc(enemy.NetworkKey, cp.RpcTarget.Single(cp.OwnerClientId, RpcTargetUse.Temp));
                    }
                }
            }
        }

        void GrantAuthoritativeProgress(int xp, int gold)
        {
            if (!IsServer) return;
            Experience.Value += Mathf.Max(0, xp);
            Gold.Value += Mathf.Max(0, gold);
            int xpNeeded = Level.Value * 100;
            while (Experience.Value >= xpNeeded)
            {
                Experience.Value -= xpNeeded;
                Level.Value++;
                MaxHealth.Value += 15;
                Health.Value = MaxHealth.Value;
                xpNeeded = Level.Value * 100;
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveEnemyDefeatedClientRpc(string enemyKey, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) localPlayer.RecordEnemyDefeat(enemy);
        }

        public bool RequestOpenChestServerValidated(LootChest chest)
        {
            if (!chest || string.IsNullOrEmpty(chest.chestId)) return false;
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return true;

            if (IsServer)
                return ValidateChestClaimOnServer(chest);

            RequestOpenChestServerRpc(chest.chestId);
            return false;
        }

        bool ValidateChestClaimOnServer(LootChest chest)
        {
            if (Health.Value <= 0 || !chest || string.IsNullOrEmpty(chest.chestId)) return false;
            if (Vector3.Distance(transform.position, chest.transform.position) > 4.5f) return false;
            if (chest.chestId == RiverReaverCamp.CacheId && !serverDefeatedEnemyKeys.Contains(RiverReaverCamp.CaptainId)) return false;
            if (chest.requiresCaptainDefeated && !serverDefeatedEnemyKeys.Contains("ashwood-captain")) return false;
            if (chest.requiresWardenDefeated && !serverDefeatedEnemyKeys.Contains("crypt-warden")) return false;

            if (CoopWorldManager.Instance != null && !CoopWorldManager.Instance.TryClaimChestForClient(OwnerClientId, chest.chestId))
                return false;

            GrantAuthoritativeProgress(chest.experience, chest.gold);
            return true;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestOpenChestServerRpc(string chestId)
        {
            LootChest targetChest = null;
            foreach (var c in FindObjectsByType<LootChest>(FindObjectsSortMode.None))
                if (c && c.chestId == chestId) { targetChest = c; break; }

            if (ValidateChestClaimOnServer(targetChest))
            {
                ApproveOpenChestClientRpc(chestId, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveOpenChestClientRpc(string chestId, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            foreach (var c in FindObjectsByType<LootChest>(FindObjectsSortMode.None))
            {
                if (c && c.chestId == chestId)
                {
                    localPlayer.ApplyAuthorizedChestOpen(c);
                    c.Refresh();
                    break;
                }
            }
        }

        public void RequestAcceptQuest()
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            if (IsServer) ValidateAcceptQuestOnServer();
            else RequestAcceptQuestServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestAcceptQuestServerRpc() => ValidateAcceptQuestOnServer();

        void ValidateAcceptQuestOnServer()
        {
            if (!IsServer || Health.Value <= 0 || QuestAccepted.Value || QuestIndex.Value >= 3) return;
            if (!IsNearQuestGiver()) return;
            QuestAccepted.Value = true;
            QuestKills.Value = 0;
        }

        bool IsNearQuestGiver()
        {
            foreach (var npc in FindObjectsByType<TownNPC>(FindObjectsSortMode.None))
                if (npc && Vector3.Distance(transform.position, npc.transform.position) <= 6.5f) return true;
            return false;
        }

        public bool RequestCompleteQuestServerValidated(int questIndex)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return true;

            if (IsServer)
                return ValidateCompleteQuestOnServer(questIndex);

            RequestCompleteQuestServerRpc(questIndex);
            return false;
        }

        bool ValidateCompleteQuestOnServer(int questIndex)
        {
            if (!IsServer || Health.Value <= 0 || !QuestAccepted.Value || QuestIndex.Value != questIndex || questIndex < 0 || questIndex > 2)
                return false;
            if (!IsNearQuestGiver()) return false;

            int requiredKills = questIndex == 2 ? 1 : 3;
            if (QuestKills.Value < requiredKills) return false;

            if (CoopWorldManager.Instance != null && !CoopWorldManager.Instance.TryClaimQuestForClient(OwnerClientId, questIndex))
                return false;

            int gold = questIndex == 0 ? 25 : (questIndex == 1 ? 35 : 60);
            int xp = questIndex == 0 ? 80 : (questIndex == 1 ? 100 : 180);
            GrantAuthoritativeProgress(xp, gold);
            QuestIndex.Value++;
            QuestAccepted.Value = false;
            QuestKills.Value = 0;
            Health.Value = MaxHealth.Value;
            return true;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestCompleteQuestServerRpc(int questIndex)
        {
            if (ValidateCompleteQuestOnServer(questIndex))
            {
                ApproveCompleteQuestClientRpc(questIndex, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveCompleteQuestClientRpc(int questIndex, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.ApplyAuthorizedCompleteQuest(questIndex);
        }

        public bool RequestCompleteFactionQuestServerValidated(string questId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || string.IsNullOrEmpty(questId)) return false;
            if (IsServer)
            {
                if (TryValidateFactionQuestOnServer(questId, out int goldGain, out int xpGain))
                {
                    if (FactionQuestSystem.Instance && localPlayer)
                        FactionQuestSystem.Instance.ApplyAuthorizedComplete(localPlayer, questId, Gold.Value, goldGain, xpGain);
                    return true;
                }
                return false;
            }
            RequestCompleteFactionQuestServerRpc(questId);
            return false;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestCompleteFactionQuestServerRpc(string questId)
        {
            if (TryValidateFactionQuestOnServer(questId, out int goldGain, out int xpGain))
            {
                ApproveCompleteFactionQuestClientRpc(questId, Gold.Value, goldGain, xpGain, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        bool TryValidateFactionQuestOnServer(string questId, out int goldGain, out int xpGain)
        {
            goldGain = 0;
            xpGain = 0;
            if (!IsServer || Health.Value <= 0 || string.IsNullOrEmpty(questId)) return false;

            var q = FactionQuestSystem.GetQuest(questId);
            if (q == null || serverCompletedFactionQuests.Contains(questId)) return false;
            if (!string.IsNullOrEmpty(q.prerequisiteQuestId) && !serverCompletedFactionQuests.Contains(q.prerequisiteQuestId))
                return false;

            string targetNpc = q.targetType == "talk_npc" ? q.targetId : q.giverNpc;
            bool nearNpc = false;
            foreach (var npc in FindObjectsByType<TownNPC>(FindObjectsSortMode.None))
            {
                if (npc && string.Equals(npc.npcName, targetNpc, System.StringComparison.OrdinalIgnoreCase) &&
                    Vector3.Distance(transform.position, npc.transform.position) <= 7.0f)
                {
                    nearNpc = true;
                    break;
                }
            }
            if (!nearNpc) return false;

            if (q.id == "frontier_cartography")
            {
                if (!serverDiscoveredLandmarks.Contains("westbank-overlook") ||
                    !serverDiscoveredLandmarks.Contains("bloodrock-vista") ||
                    !serverDiscoveredLandmarks.Contains("wyrmcrest-lookout"))
                    return false;
            }
            else if (q.targetType == "defeat_enemy")
            {
                bool exactBoss = q.targetId == RiverReaverCamp.CaptainId || q.targetId == "redridge-outcast-boss" || q.targetId == "vermithrax-boss";
                if (exactBoss ? !serverDefeatedEnemyKeys.Contains(q.targetId) : serverDefeatedEnemyKeys.Count < q.requiredCount)
                    return false;
            }

            if (!serverCompletedFactionQuests.Add(questId)) return false;

            goldGain = Race.Value.ToString() == "Human" ? Mathf.RoundToInt(q.rewardGold * 1.25f) : q.rewardGold;
            xpGain = q.rewardXP;
            GrantAuthoritativeProgress(xpGain, goldGain);
            Health.Value = MaxHealth.Value;
            return true;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveCompleteFactionQuestClientRpc(string questId, int newServerGold, int goldGain, int xpGain, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer || !FactionQuestSystem.Instance) return;
            FactionQuestSystem.Instance.ApplyAuthorizedComplete(localPlayer, questId, newServerGold, goldGain, xpGain);
        }

        // ── O-01 Shared World Events & Late-Join Snapshot ───────────────────────────────

        public void SendWorldSnapshotToClient(ulong clientId)
        {
            if (!IsServer || !IsSpawned) return;
            var rpcParams = RpcTarget.Single(clientId, RpcTargetUse.Temp);
            foreach (var kv in EnemyController.AllByKey)
            {
                var enemy = kv.Value;
                if (!enemy || enemy.trainingDummy) continue;
                var c = enemy.GetComponent<Combatant>();
                if (!c) continue;
                if (!c.Alive || c.Health < c.maxHealth || !enemy.gameObject.activeInHierarchy)
                {
                    SendEnemySnapshotClientRpc(enemy.NetworkKey, c.Health, c.maxHealth, c.Alive, enemy.transform.position, enemy.transform.eulerAngles.y, rpcParams);
                }
            }
            if (CoopWorldManager.Instance != null && CoopWorldManager.Instance.TryGetActiveReadyCheck(out string rcInitiator))
            {
                SendReadyCheckSnapshotClientRpc(rcInitiator, rpcParams);
            }
            if (LivingWorldEvents.Instance != null)
            {
                LivingWorldEvents.Instance.SendEventSnapshotToClient(this, rpcParams);
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void SendReadyCheckSnapshotClientRpc(string initiatorName, RpcParams rpcParams = default)
        {
            if (CoopWorldManager.Instance) CoopWorldManager.Instance.OpenReadyCheck(initiatorName);
        }

        public void SendEventSnapshotRpc(string eventId, bool active, Vector3 pos, RpcParams rpcParams)
        {
            SendEventSnapshotClientRpc(eventId, active, pos, rpcParams);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void SendEventSnapshotClientRpc(string eventId, bool active, Vector3 pos, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            var lwe = LivingWorldEvents.Instance ? LivingWorldEvents.Instance : FindFirstObjectByType<LivingWorldEvents>();
            if (lwe) lwe.ApplyClientEventState(eventId, active, pos);
        }

        public static void BroadcastWorldEventState(string eventId, bool active, Vector3 pos)
        {
            if (LocalOwner && LocalOwner.IsServer && LocalOwner.IsSpawned)
                LocalOwner.BroadcastWorldEventStateClientRpc(eventId, active, pos);
        }

        [Rpc(SendTo.Everyone)]
        void BroadcastWorldEventStateClientRpc(string eventId, bool active, Vector3 pos)
        {
            if (IsServer) return;
            var lwe = LivingWorldEvents.Instance ? LivingWorldEvents.Instance : FindFirstObjectByType<LivingWorldEvents>();
            if (lwe) lwe.ApplyClientEventState(eventId, active, pos);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void SendEnemySnapshotClientRpc(string enemyKey, float hp, float maxHp, bool alive, Vector3 pos, float yaw, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedState(hp, maxHp, alive, pos, yaw);
        }

        public static void BroadcastWorldEventReward(string eventId, Vector3 eventCenter, int baseXp, int baseGold)
        {
            foreach (var cp in AllPlayers)
            {
                if (!cp || !cp.IsSpawned || cp.Health.Value <= 0) continue;
                if (Vector3.Distance(cp.transform.position, eventCenter) <= 65f)
                {
                    bool firstClear = cp.serverCompletedEvents.Add(eventId);
                    int xp = firstClear ? baseXp : Mathf.Max(1, baseXp / 2);
                    int gold = firstClear ? baseGold : Mathf.Max(1, baseGold / 2);
                    cp.GrantAuthoritativeProgress(xp, gold);
                    cp.ReceiveWorldEventRewardClientRpc(eventId, xp, gold, firstClear, cp.RpcTarget.Single(cp.OwnerClientId, RpcTargetUse.Temp));
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveWorldEventRewardClientRpc(string eventId, int xp, int gold, bool firstClear, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.AddExperience(xp);
            localPlayer.SetAuthoritativeGold(Gold.Value);
            string tag = firstClear ? "Event Complete" : "Bounty Repeat";
            localPlayer.Message = $"{tag}: +{xp} XP, +{gold} Gold!";
        }

        // ── Deliverables #5: Map Pings, Ready Check & Ally Revive ───────────────────────

        float lastServerPingTime = -10f;
        float lastLocalPingTime = -10f;

        public void RequestMapPing(Vector3 worldPos)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || Health.Value <= 0) return;
            if (Time.time < lastLocalPingTime + 2.0f) return;
            lastLocalPingTime = Time.time;
            RequestMapPingServerRpc(worldPos);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestMapPingServerRpc(Vector3 worldPos)
        {
            if (Health.Value <= 0 || Time.time < lastServerPingTime + 2.0f) return;
            lastServerPingTime = Time.time;
            BroadcastMapPingClientRpc(PlayerName.Value.ToString(), worldPos);
        }

        [Rpc(SendTo.Everyone)]
        void BroadcastMapPingClientRpc(string senderName, Vector3 worldPos)
        {
            if (CoopWorldManager.Instance) CoopWorldManager.Instance.ShowMapPing(senderName, worldPos);
        }

        public void RequestReadyCheck()
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            RequestReadyCheckServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestReadyCheckServerRpc()
        {
            foreach (var p in AllPlayers) if (p) p.ReadyStatus.Value = 0;
            ReadyStatus.Value = 1;
            BroadcastReadyCheckClientRpc(PlayerName.Value.ToString());
        }

        [Rpc(SendTo.Everyone)]
        void BroadcastReadyCheckClientRpc(string initiatorName)
        {
            if (CoopWorldManager.Instance) CoopWorldManager.Instance.OpenReadyCheck(initiatorName);
        }

        public void RespondReadyCheck(bool ready)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            RespondReadyCheckServerRpc(ready);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RespondReadyCheckServerRpc(bool ready)
        {
            ReadyStatus.Value = (byte)(ready ? 1 : 2);
        }

        public void RequestReviveAlly(ulong targetClientId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            RequestReviveAllyServerRpc(targetClientId);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestReviveAllyServerRpc(ulong targetClientId)
        {
            if (Health.Value <= 0) return;
            var target = FindByClientId(targetClientId);
            if (!target || target == this || target.Health.Value > 0) return;
            if (Vector3.Distance(transform.position, target.transform.position) > 5.0f) return;

            target.Health.Value = Mathf.Max(1, target.MaxHealth.Value / 2);
            target.ReceiveAllyReviveClientRpc(PlayerName.Value.ToString(), target.Health.Value, target.RpcTarget.Single(targetClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveAllyReviveClientRpc(string reviverName, int newHp, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer || !localPlayer.Stats) return;
            int effectiveHp = Health.Value > 0 ? Mathf.Max(Health.Value, newHp) : newHp;
            localPlayer.Stats.SetReplicatedHealth(effectiveHp, localPlayer.Stats.maxHealth);
            localPlayer.Message = $"Revived by {reviverName}!";
            SpellFX.CreateArcaneBurst(localPlayer.transform.position);
        }

        void OnGUI()
        {
            if (!IsSpawned || IsOwner || FrontendMenu.IsOpen) return;
            var cam = Camera.main;
            if (!cam) return;

            Vector3 worldHead = transform.position + Vector3.up * (IsMounted.Value ? 2.95f : 2.25f);
            float dist = Vector3.Distance(cam.transform.position, worldHead);
            if (dist > 45f) return;

            Vector3 screenPos = cam.WorldToScreenPoint(worldHead);
            if (screenPos.z <= 0.1f) return;

            if (nameplateStyle == null)
            {
                nameplateStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
            }

            float guiX = screenPos.x;
            float guiY = Screen.height - screenPos.y;
            string mountTag = IsMounted.Value ? $" [{MountId.Value}]" : "";
            string label = $"{PlayerName.Value} (Lv {Level.Value}){mountTag}";

            var oldColor = GUI.color;
            nameplateStyle.normal.textColor = Health.Value <= 0 ? new Color(0.75f, 0.35f, 0.35f) : new Color(0.55f, 0.92f, 1f);
            GUI.Label(new Rect(guiX - 100f, guiY - 24f, 200f, 20f), label, nameplateStyle);

            float hpPct = MaxHealth.Value > 0 ? Mathf.Clamp01((float)Health.Value / MaxHealth.Value) : 1f;
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(new Rect(guiX - 42f, guiY - 4f, 84f, 7f), Texture2D.whiteTexture);
            GUI.color = new Color(0.25f, 0.85f, 0.35f, 0.95f);
            GUI.DrawTexture(new Rect(guiX - 41f, guiY - 3f, 82f * hpPct, 5f), Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
    }
}
