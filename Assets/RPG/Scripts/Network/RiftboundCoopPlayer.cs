using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Riftbound.Network
{
    public struct RealmHydrationHeader : INetworkSerializable
    {
        public uint snapshotSeq;
        public FixedString64Bytes accountId;
        public FixedString64Bytes charName;
        public FixedString64Bytes activeTitle;
        public FixedString32Bytes race;
        public FixedString32Bytes className;
        public FixedString32Bytes faction;
        public FixedString64Bytes equippedWeaponId;
        public FixedString64Bytes equippedArmorId;
        public FixedString64Bytes equippedTrinketId;
        public FixedString64Bytes mountId;
        public int level;
        public int remainderXp;
        public int gold;
        public int health;
        public int maxHealth;
        public int potions;
        public int boneShards;
        public int questIndex;
        public int questKills;
        public int alchemySkill;
        public int blacksmithingSkill;
        public int engineeringSkill;
        public bool temperedWeapon;
        public bool questAccepted;
        public bool expectChunks;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref snapshotSeq);
            serializer.SerializeValue(ref accountId);
            serializer.SerializeValue(ref charName);
            serializer.SerializeValue(ref activeTitle);
            serializer.SerializeValue(ref race);
            serializer.SerializeValue(ref className);
            serializer.SerializeValue(ref faction);
            serializer.SerializeValue(ref equippedWeaponId);
            serializer.SerializeValue(ref equippedArmorId);
            serializer.SerializeValue(ref equippedTrinketId);
            serializer.SerializeValue(ref mountId);
            serializer.SerializeValue(ref level);
            serializer.SerializeValue(ref remainderXp);
            serializer.SerializeValue(ref gold);
            serializer.SerializeValue(ref health);
            serializer.SerializeValue(ref maxHealth);
            serializer.SerializeValue(ref potions);
            serializer.SerializeValue(ref boneShards);
            serializer.SerializeValue(ref questIndex);
            serializer.SerializeValue(ref questKills);
            serializer.SerializeValue(ref alchemySkill);
            serializer.SerializeValue(ref blacksmithingSkill);
            serializer.SerializeValue(ref engineeringSkill);
            serializer.SerializeValue(ref temperedWeapon);
            serializer.SerializeValue(ref questAccepted);
            serializer.SerializeValue(ref expectChunks);
        }
    }
    public enum ServerCastPhase : byte
    {
        None = 0,
        Started = 1,
        Canceled = 2,
        Released = 3
    }

    public enum ServerCastCancelReason : byte
    {
        None = 0,
        Moved = 1,
        Damaged = 2,
        Died = 3,
        ManualCancel = 4,
        Mounted = 5,
        InvalidTarget = 6,
        LineOfSight = 7,
        OutOfRange = 8,
        Replaced = 9
    }

    public readonly struct ServerCastEvent
    {
        public readonly RiftboundCoopPlayer Caster;
        public readonly bool IsLocalOwner;
        public readonly uint CastSeq;
        public readonly string AbilityId;
        public readonly string TargetEnemyKey;
        public readonly ServerCastPhase Phase;
        public readonly ServerCastCancelReason CancelReason;
        public readonly float Duration;
        public readonly double ServerStartTime;

        public ServerCastEvent(
            RiftboundCoopPlayer caster,
            bool isLocalOwner,
            uint castSeq,
            string abilityId,
            string targetEnemyKey,
            ServerCastPhase phase,
            ServerCastCancelReason cancelReason,
            float duration,
            double serverStartTime)
        {
            Caster = caster;
            IsLocalOwner = isLocalOwner;
            CastSeq = castSeq;
            AbilityId = abilityId ?? string.Empty;
            TargetEnemyKey = targetEnemyKey ?? string.Empty;
            Phase = phase;
            CancelReason = cancelReason;
            Duration = duration;
            ServerStartTime = serverStartTime;
        }
    }

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
        public NetworkVariable<bool> IsSprinting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<FixedString32Bytes> MountId = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes("horse"), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<FixedString64Bytes> EquippedWeaponId = new NetworkVariable<FixedString64Bytes>(new FixedString64Bytes("starter-axe"), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<FixedString64Bytes> EquippedArmorId = new NetworkVariable<FixedString64Bytes>(new FixedString64Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<FixedString64Bytes> EquippedTrinketId = new NetworkVariable<FixedString64Bytes>(new FixedString64Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<byte> LocomotionFlags = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // bit0=airborne, bit1=walking, bit2=blocking
        public NetworkVariable<byte> ResourcePct = new NetworkVariable<byte>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> IsLeader = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> IsInCombat = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<uint> ReadyCheckId = new NetworkVariable<uint>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<byte> SocialRole = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // 0=Auto, 1=Tank, 2=Healer, 3=DPS
        public NetworkVariable<int> SpawnSlot = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> QuestIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> QuestAccepted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> QuestKills = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<byte> ReadyStatus = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // 0=None, 1=Ready, 2=NotReady

        PlayerController localPlayer;
        GameObject remoteVisualRoot;
        GameObject remoteMountVisual;
        MountVisual remoteMountComp;
        Transform remoteRiderHips;
        Renderer remoteWeaponRenderer;
        bool remoteWeaponWasEnabled;
        string builtMountId = "";
        Vector3 remoteBaseVisualPos = Vector3.zero;
        Animator remoteAnimator;
        string currentRemoteAnim = "";
        string builtRace = "";
        string builtClass = "";
        GroundedVisual remoteGrounding;
        bool remoteObserverBound;
        float syncTimer;
        float lastServerMoveTime;
        bool hostTeleportAuthorized;
        Vector3 authorizedTeleportTarget;
        float actionAnimUntil;
        uint localActionSeq;
        uint lastClientActionSeq;
        uint lastBroadcastSeq;
        float lastClientActionTime = -10f;
        uint lastObservedActionSeq;
        uint serverReadyCheckSeq;
        static uint activeGlobalReadyCheckSeq;
        static float activeGlobalReadyCheckExpires;
        float lastServerReadyCheckTime = -10f;
        static readonly Dictionary<string, MountDefinition> cachedMounts = new Dictionary<string, MountDefinition>(System.StringComparer.OrdinalIgnoreCase);

        public GameObject RemoteVisualRoot => remoteVisualRoot;
        public Animator RemoteAnimator => remoteAnimator;
        public GroundedVisual RemoteGroundedVisual => remoteGrounding;
        public bool IsObserverContextBound =>
            remoteObserverBound
            && remoteGrounding != null
            && remoteAnimator != null
            && remoteGrounding.animator == remoteAnimator
            && GetComponents<GroundedVisual>().Length == 1;
        public bool IsRemoteObserverMounted => IsMounted.Value;
        public bool IsRemoteObserverAlive => Health.Value > 0;
        public bool IsRemoteObserverAirborne => (LocomotionFlags.Value & 1) != 0;
        public MountVisual RemoteMountComponent => remoteMountComp;
        public bool HasRemoteCapsuleFallback => remoteMountVisual != null && remoteMountComp == null;
        public float RemoteSmoothedSpeed => remoteSmoothedSpeed;
        public string CurrentRemoteAnim => currentRemoteAnim;
        Vector3 lastLocalPos;
        bool hasLastLocalPos;
        Vector3 lastRemoteWorldPos;
        bool hasLastRemotePos;
        float remoteSmoothedSpeed;
        float remoteZeroSpeedTimer;
        Quaternion remoteBaseVisualRot = Quaternion.identity;
        float remoteVisualYawOffset;
        bool spawnPositioned;
        sealed class ServerPendingCorpse
        {
            public string enemyKey;
            public Vector3 position;
            public int gold;
            public int boneShards;
            public List<string> items = new List<string>();
        }

        readonly Dictionary<string, float> serverAbilityCooldowns = new Dictionary<string, float>();
        readonly HashSet<string> serverDefeatedEnemyKeys = new HashSet<string>();
        readonly HashSet<string> serverCompletedEvents = new HashSet<string>();
        readonly HashSet<string> serverDiscoveredLandmarks = new HashSet<string>();
        readonly HashSet<string> serverCompletedFactionQuests = new HashSet<string>();
        readonly List<string> serverOwnedItems = new List<string> { "starter-axe" };
        readonly HashSet<string> serverClaimedChests = new HashSet<string>(StringComparer.Ordinal);
        readonly Dictionary<string, ServerPendingCorpse> serverPendingCorpseLoot = new Dictionary<string, ServerPendingCorpse>(StringComparer.Ordinal);
        int serverPotions = 3;
        bool serverTemperedWeapon;
        int serverBoneShards;
        int serverAlchemySkill = ProfessionSystem.DefaultAlchemySkill;
        int serverBlacksmithingSkill = ProfessionSystem.DefaultBlacksmithingSkill;
        int serverEngineeringSkill = ProfessionSystem.DefaultEngineeringSkill;
        readonly Dictionary<string, int> serverReagents = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        string serverFaction = "Alliance";
        string serverActiveTitle = "";
        bool serverIdentityResolved;
        bool serverHydrated;
        uint serverHydrationSeq;

        bool hasPendingHydrationHeader;
        RealmHydrationHeader pendingHydrationHeader;
        readonly List<string> pendingHydrationOwnedItems = new List<string>();
        readonly List<string> pendingHydrationClaimedChests = new List<string>();
        readonly List<string> pendingHydrationClaimedQuests = new List<string>();
        readonly List<string> pendingHydrationDefeatedEnemies = new List<string>();
        readonly List<string> pendingHydrationLandmarks = new List<string>();
        readonly List<string> pendingHydrationBossKills = new List<string>();
        readonly List<string> pendingHydrationReagents = new List<string>();
        bool hasReadyClientHydrationSnapshot;
        float lastServerPotionTime = -10f;
        float lastServerRespawnTime = -10f;
        bool serverBlocking;
        float serverBlockUntil;
        float serverDodgeUntil;
        float serverLastDodgeTime = -10f;
        float serverSprintUntil;
        float serverBlinkAllowanceMeters;
        float serverBlinkExpiresAt;
        float serverMoveBudget = 1.35f;
        float serverLastDamagedAt = -10f;

        bool serverCastActive;
        bool serverCastReleased;
        uint serverCastSeq;
        string serverCastAbility = string.Empty;
        string serverCastEnemyKey = string.Empty;
        float serverCastStartTime;
        double serverCastStartNetTime;
        float serverCastEndTime;
        float serverCastDuration;
        Vector3 serverCastStartPos;
        uint serverLastReleasedCastSeq;
        uint serverLastCanceledCastSeq;
        ServerCastCancelReason serverLastCancelReason;

        uint activeClientCastSeq;
        string activeClientCastAbility = string.Empty;
        string activeClientCastTargetKey = string.Empty;
        float activeClientCastDuration;
        float activeClientCastStartTime;
        double activeClientCastServerStartTime;
        ServerCastPhase activeClientCastPhase;
        ServerCastCancelReason lastClientCastCancelReason;
        uint lastClientCastEventSeq;
        ServerCastPhase lastClientCastEventPhase;
        uint lastReleasedCastSeq;
        uint lastCanceledCastSeq;
        static GUIStyle nameplateStyle;

        bool IsServerBlocking => serverBlocking && Time.time <= serverBlockUntil;
        public bool IsServerSprinting => Time.time <= serverSprintUntil && Health.Value > 0 && !IsMounted.Value;

        public override void OnNetworkSpawn()
        {
            if (!AllPlayers.Contains(this)) AllPlayers.Add(this);
            lastServerMoveTime = Time.time;
            serverMoveBudget = 1.35f;
            serverSprintUntil = -10f;
            serverBlinkAllowanceMeters = 0f;
            serverBlinkExpiresAt = 0f;
            serverCastActive = false;
            serverCastReleased = false;
            activeClientCastSeq = 0;
            activeClientCastPhase = ServerCastPhase.None;

            if (IsServer)
            {
                serverIdentityResolved = false;
                serverHydrated = false;
                serverAssignedAccountId = "";
                serverOwnedItems.Clear();
                serverOwnedItems.Add("starter-axe");
                serverClaimedChests.Clear();
                serverDefeatedEnemyKeys.Clear();
                serverDiscoveredLandmarks.Clear();
                serverCompletedFactionQuests.Clear();
                serverPendingCorpseLoot.Clear();
                serverPotions = 3;
                serverTemperedWeapon = false;
                serverBoneShards = 0;
                serverAlchemySkill = ProfessionSystem.DefaultAlchemySkill;
                serverBlacksmithingSkill = ProfessionSystem.DefaultBlacksmithingSkill;
                serverEngineeringSkill = ProfessionSystem.DefaultEngineeringSkill;
                ProfessionSystem.PopulateDefaultReagents(serverReagents);
                serverFaction = "Alliance";
                serverActiveTitle = "";

                IsSprinting.Value = false;
                SpawnSlot.Value = CoopWorldManager.AllocateSpawnSlot(this);
                IsLeader.Value = !CoopSessionConfig.IsDedicatedServer && OwnerClientId == NetworkManager.ServerClientId;
                MaxHealth.Value = 100;
                Health.Value = 100;
                Level.Value = 1;
                EquippedWeaponId.Value = new FixedString64Bytes("starter-axe");
                EquippedArmorId.Value = new FixedString64Bytes("");
                EquippedTrinketId.Value = new FixedString64Bytes("");

                if (CoopWorldManager.TryConsumePendingRealmIdentity(OwnerClientId, out var pending))
                {
                    serverAssignedAccountId = pending.AccountId;
                    PlayerName.Value = new FixedString64Bytes(string.IsNullOrWhiteSpace(pending.CharacterName) ? $"Player {OwnerClientId}" : pending.CharacterName);
                    Race.Value = new FixedString32Bytes(pending.Race);
                    ClassName.Value = new FixedString32Bytes(pending.ClassName);
                    serverFaction = pending.Faction;
                    serverIdentityResolved = true;
                }

                if ((CoopSessionConfig.IsDedicatedServer || OwnerClientId != NetworkManager.ServerClientId) && transform.position.sqrMagnitude < 0.1f && !serverIdentityResolved)
                {
                    Vector3 basePos = CoopWorldManager.GetRealmSpawnAnchor("Human");
                    Vector3 slotPos = ComputeSpawnPosition(basePos, SpawnSlot.Value);
                    AuthorizeHostTeleport(slotPos);
                }

                if (serverIdentityResolved)
                {
                    if (RealmPersistenceService.IsInitialized)
                        HydrateServerRealmCharacterIfInitialized();
                    else
                    {
                        RecalculateServerVitals(preserveCurrentHp: false);
                        serverHydrated = true;
                    }
                }
                else if (!RealmPersistenceService.IsInitialized)
                {
                    RecalculateServerVitals(preserveCurrentHp: false);
                }
            }

            if (IsOwner)
            {
                LocalOwner = this;
                Gold.OnValueChanged += OnAuthoritativeGoldChanged;
                EquippedWeaponId.OnValueChanged += OnEquippedWeaponChanged;
                EquippedArmorId.OnValueChanged += OnEquippedArmorChanged;
                EquippedTrinketId.OnValueChanged += OnEquippedTrinketChanged;
                Level.OnValueChanged += OnAuthoritativeLevelOrXpChanged;
                Experience.OnValueChanged += OnAuthoritativeLevelOrXpChanged;
                Health.OnValueChanged += OnAuthoritativeHealthChanged;
                MaxHealth.OnValueChanged += OnAuthoritativeMaxHealthChanged;
                BindToLocalPlayer();
            }
            else
            {
                Race.OnValueChanged += OnIdentityChanged;
                ClassName.OnValueChanged += OnIdentityChanged;
                IsMounted.OnValueChanged += OnMountChanged;
                MountId.OnValueChanged += OnMountIdChanged;
                EquippedWeaponId.OnValueChanged += OnEquippedWeaponChanged;
                EquippedArmorId.OnValueChanged += OnEquippedArmorChanged;
                EquippedTrinketId.OnValueChanged += OnEquippedTrinketChanged;
                Health.OnValueChanged += OnRemoteHealthChanged;
                RebuildRemoteVisual();
            }
        }

        public override void OnNetworkDespawn()
        {
            serverCastActive = false;
            serverCastReleased = false;
            serverSprintUntil = -10f;
            if (IsServer)
            {
                if (serverIdentityResolved || serverHydrated)
                    PersistServerRealmCharacterIfInitialized();
                if (IsSprinting.Value) IsSprinting.Value = false;
                if (IsLeader.Value && OwnerClientId != NetworkManager.ServerClientId)
                {
                    var hostPeer = FindByClientId(NetworkManager.ServerClientId);
                    if (hostPeer && hostPeer != this) hostPeer.IsLeader.Value = true;
                }
            }
            AllPlayers.Remove(this);
            if (LocalOwner == this) LocalOwner = null;
            Gold.OnValueChanged -= OnAuthoritativeGoldChanged;
            Race.OnValueChanged -= OnIdentityChanged;
            ClassName.OnValueChanged -= OnIdentityChanged;
            IsMounted.OnValueChanged -= OnMountChanged;
            MountId.OnValueChanged -= OnMountIdChanged;
            EquippedWeaponId.OnValueChanged -= OnEquippedWeaponChanged;
            EquippedArmorId.OnValueChanged -= OnEquippedArmorChanged;
            EquippedTrinketId.OnValueChanged -= OnEquippedTrinketChanged;
            Level.OnValueChanged -= OnAuthoritativeLevelOrXpChanged;
            Experience.OnValueChanged -= OnAuthoritativeLevelOrXpChanged;
            Health.OnValueChanged -= OnAuthoritativeHealthChanged;
            MaxHealth.OnValueChanged -= OnAuthoritativeMaxHealthChanged;
            Health.OnValueChanged -= OnRemoteHealthChanged;
            ClearRemoteMountVisual();
            ReleaseRemoteGroundingObserver();
            if (remoteVisualRoot)
            {
                remoteVisualRoot.SetActive(false);
                Destroy(remoteVisualRoot);
                remoteVisualRoot = null;
                remoteAnimator = null;
            }
        }

        void OnAuthoritativeGoldChanged(int prev, int next)
        {
            if (IsOwner && localPlayer && (!CoopSessionConfig.IsRealmClient || localPlayer.IsRealmHydrated))
                localPlayer.SetAuthoritativeGold(next);
        }

        void OnAuthoritativeHealthChanged(int prev, int next)
        {
            if (IsOwner && localPlayer && localPlayer.Stats && (!CoopSessionConfig.IsRealmClient || localPlayer.IsRealmHydrated))
                localPlayer.Stats.SetReplicatedHealth(next, MaxHealth.Value);
        }

        void OnAuthoritativeMaxHealthChanged(int prev, int next)
        {
            if (IsOwner && localPlayer && localPlayer.Stats && (!CoopSessionConfig.IsRealmClient || localPlayer.IsRealmHydrated))
                localPlayer.Stats.SetReplicatedHealth(Health.Value, next);
        }

        void OnAuthoritativeLevelOrXpChanged(int prev, int next)
        {
            if (IsOwner && CoopSessionConfig.IsRealmClient && localPlayer && localPlayer.IsRealmHydrated)
                localPlayer.SetAuthoritativeServerProgress(Level.Value, Experience.Value);
        }

        void OnIdentityChanged(FixedString32Bytes prev, FixedString32Bytes next)
        {
            if (!IsOwner) RebuildRemoteVisual();
        }

        void OnMountChanged(bool prev, bool next)
        {
            if (!IsOwner) UpdateRemoteMountVisual();
        }

        void OnMountIdChanged(FixedString32Bytes prev, FixedString32Bytes next)
        {
            if (!IsOwner) UpdateRemoteMountVisual();
        }

        void OnEquippedWeaponChanged(FixedString64Bytes prev, FixedString64Bytes next)
        {
            if (!IsOwner) ApplyRemoteWeaponVisual();
            else if (localPlayer && (!CoopSessionConfig.IsRealmClient || localPlayer.IsRealmHydrated))
                localPlayer.ApplyAuthorizedEquippedSlot(GearSlot.Weapon, next.ToString(), Health.Value, MaxHealth.Value);
        }

        void OnEquippedArmorChanged(FixedString64Bytes prev, FixedString64Bytes next)
        {
            if (IsOwner && localPlayer && (!CoopSessionConfig.IsRealmClient || localPlayer.IsRealmHydrated))
                localPlayer.ApplyAuthorizedEquippedSlot(GearSlot.Armor, next.ToString(), Health.Value, MaxHealth.Value);
        }

        void OnEquippedTrinketChanged(FixedString64Bytes prev, FixedString64Bytes next)
        {
            if (IsOwner && localPlayer && (!CoopSessionConfig.IsRealmClient || localPlayer.IsRealmHydrated))
                localPlayer.ApplyAuthorizedEquippedSlot(GearSlot.Trinket, next.ToString(), Health.Value, MaxHealth.Value);
        }

        void ApplyRemoteWeaponVisual()
        {
            if (!remoteVisualRoot) return;
            string wid = EquippedWeaponId.Value.ToString();
            if (string.IsNullOrWhiteSpace(wid)) wid = "starter-axe";
            var item = ItemCatalog.Get(wid);
            string off = builtClass == "Rogue" && wid.IndexOf("dagger", System.StringComparison.OrdinalIgnoreCase) >= 0
                ? "copper-dagger" : null;
            foreach (var pav in remoteVisualRoot.GetComponentsInChildren<ProceduralActorVisual>(true))
            {
                pav.EquipWeaponVisual(wid, off, item?.rarity);
                pav.SetWeaponVisualVisible(!IsMounted.Value);
            }
        }

        void OnRemoteHealthChanged(int prev, int next)
        {
            if (IsOwner) return;
            if (next <= 0)
            {
                actionAnimUntil = 0f;
                ClearRemoteMountVisual();
            }
            else if (prev <= 0 && next > 0)
            {
                currentRemoteAnim = "";
                UpdateRemoteMountVisual();
            }
        }

        void BindToLocalPlayer()
        {
            if (CoopSessionConfig.IsDedicatedServer) return;
            localPlayer = FindFirstObjectByType<PlayerController>();
            if (!localPlayer) return;

            if (CoopSessionConfig.IsRealmClient)
            {
                var rt = RealmConfig.Runtime;
                if (!string.IsNullOrWhiteSpace(rt.clientCharName))
                {
                    localPlayer.CharacterName = rt.clientCharName;
                    if (localPlayer.Stats) localPlayer.Stats.displayName = rt.clientCharName;
                }
                if (!string.IsNullOrWhiteSpace(rt.clientRace)) localPlayer.CharacterRace = rt.clientRace;
                if (!string.IsNullOrWhiteSpace(rt.clientClass)) localPlayer.CharacterClass = rt.clientClass;
                if (!string.IsNullOrWhiteSpace(rt.clientFaction)) localPlayer.CharacterFaction = rt.clientFaction;
            }

            if (!spawnPositioned)
            {
                spawnPositioned = true;
                if (CoopSessionConfig.IsRealmClient || OwnerClientId != NetworkManager.ServerClientId)
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
            if (hasReadyClientHydrationSnapshot && localPlayer && !localPlayer.IsRealmHydrated)
                ApplyPendingHydrationToLocalPlayer();
        }

        public static Vector3 ComputeSpawnPosition(Vector3 basePos, int slot)
        {
            if (slot <= 0) return basePos;
            float angle = (slot - 1) * 65f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * 2.6f, 0f, Mathf.Sin(angle) * 2.6f);
            Vector3 candidate = basePos + offset;
            var terrain = WorldRegions.TerrainAt(candidate);
            if (terrain)
            {
                float h = terrain.SampleHeight(candidate) + terrain.transform.position.y;
                candidate.y = Mathf.Max(h + 0.05f, basePos.y);
            }
            return candidate;
        }

        public static MountDefinition ResolveMountDefinition(string mountId)
        {
            string key = string.IsNullOrEmpty(mountId) ? "horse" : mountId.Trim().ToLowerInvariant();
            if (cachedMounts.TryGetValue(key, out var cached) && cached) return cached;

            var lp = LocalOwner ? LocalOwner.localPlayer : null;
            if (!lp) lp = FindFirstObjectByType<PlayerController>();
            if (lp && lp.mounts != null)
            {
                foreach (var m in lp.mounts)
                {
                    if (m && !string.IsNullOrEmpty(m.mountId))
                        cachedMounts[m.mountId.Trim().ToLowerInvariant()] = m;
                }
            }

            var resMounts = Resources.LoadAll<MountDefinition>("Mounts");
            if (resMounts != null)
            {
                foreach (var m in resMounts)
                {
                    if (m && !string.IsNullOrEmpty(m.mountId) && !cachedMounts.ContainsKey(m.mountId.Trim().ToLowerInvariant()))
                        cachedMounts[m.mountId.Trim().ToLowerInvariant()] = m;
                }
            }

            if (cachedMounts.TryGetValue(key, out var resolved) && resolved) return resolved;
            if (cachedMounts.TryGetValue("horse", out var horse) && horse) return horse;
            return null;
        }

        void BindRemoteGroundingObserver()
        {
            if (IsOwner || !remoteAnimator) return;
            var grounding = GroundedVisual.EnsureFor(gameObject, remoteAnimator);
            if (grounding)
            {
                grounding.BindObserver(
                    remoteAnimator,
                    () => IsMounted.Value,
                    () => Health.Value > 0,
                    () => (LocomotionFlags.Value & 1) != 0);
                remoteGrounding = grounding;
                remoteObserverBound = true;
            }
        }

        void ReleaseRemoteGroundingObserver()
        {
            remoteObserverBound = false;
            if (remoteGrounding)
            {
                remoteGrounding.BindAnimator(null);
                remoteGrounding = null;
            }
        }

        void RebuildRemoteVisual()
        {
            string r = Race.Value.ToString();
            string c = ClassName.Value.ToString();
            if (string.IsNullOrWhiteSpace(r)) r = "Human";
            if (string.IsNullOrWhiteSpace(c)) c = "Warrior";
            if (remoteVisualRoot && builtRace == r && builtClass == c) return;

            ClearRemoteMountVisual();
            ReleaseRemoteGroundingObserver();
            if (remoteVisualRoot)
            {
                remoteVisualRoot.SetActive(false);
                Destroy(remoteVisualRoot);
                remoteVisualRoot = null;
                remoteAnimator = null;
            }
            builtRace = r;
            builtClass = c;
            remoteVisualRoot = CharacterVisualFactory.CreateVisualInstance(r, c, transform);
            if (remoteVisualRoot)
            {
                remoteVisualRoot.name = $"CoopVisual_{r}_{c}";
                remoteVisualRoot.transform.localPosition = Vector3.zero;
                remoteBaseVisualPos = remoteVisualRoot.transform.localPosition;
                remoteBaseVisualRot = remoteVisualRoot.transform.localRotation;
                remoteVisualYawOffset = 0f;
                foreach (var cam in remoteVisualRoot.GetComponentsInChildren<Camera>(true)) Destroy(cam);
                foreach (var al in remoteVisualRoot.GetComponentsInChildren<AudioListener>(true)) Destroy(al);
                remoteAnimator = remoteVisualRoot.GetComponentInChildren<Animator>(true);
                currentRemoteAnim = "";
                BindRemoteGroundingObserver();
                ApplyRemoteWeaponVisual();
            }
            UpdateRemoteMountVisual();
        }

        static Transform FindRiderHips(Animator anim)
        {
            if (!anim) return null;
            if (anim.isHuman)
            {
                var hips = anim.GetBoneTransform(HumanBodyBones.Hips);
                if (hips) return hips;
            }
            foreach (var bone in anim.GetComponentsInChildren<Transform>(true))
            {
                string bName = bone.name.ToLowerInvariant();
                if (bone.name.EndsWith(":Hips") || bName == "hips" || bName.Contains("hips") || bName.Contains("pelvis"))
                    return bone;
            }
            return null;
        }

        void ClearRemoteMountVisual()
        {
            bool hadMount = remoteMountVisual != null || remoteMountComp != null || !string.IsNullOrEmpty(builtMountId);
            if (remoteMountVisual)
            {
                Destroy(remoteMountVisual);
                remoteMountVisual = null;
            }
            remoteMountComp = null;
            builtMountId = "";
            if (remoteWeaponRenderer)
            {
                remoteWeaponRenderer.enabled = remoteWeaponWasEnabled;
                remoteWeaponRenderer = null;
            }
            if (remoteVisualRoot)
                foreach (var pav in remoteVisualRoot.GetComponentsInChildren<ProceduralActorVisual>(true))
                    pav.SetWeaponVisualVisible(true);
            remoteRiderHips = null;
            if (remoteVisualRoot)
            {
                bool separateAnimatorChild = !remoteAnimator || remoteAnimator.transform != remoteVisualRoot.transform;
                if (hadMount || separateAnimatorChild)
                    remoteVisualRoot.transform.localPosition = remoteBaseVisualPos;
                if (Health.Value <= 0)
                {
                    remoteVisualYawOffset = 0f;
                    remoteVisualRoot.transform.localRotation = remoteBaseVisualRot;
                }
            }
        }

        void UpdateRemoteMountVisual()
        {
            if (IsMounted.Value && Health.Value > 0)
            {
                string targetId = MountId.Value.ToString();
                if (string.IsNullOrWhiteSpace(targetId)) targetId = "horse";
                targetId = targetId.Trim().ToLowerInvariant();

                if (remoteMountVisual && remoteMountComp && builtMountId == targetId)
                {
                    PositionRemoteRider();
                    return;
                }

                ClearRemoteMountVisual();
                var def = ResolveMountDefinition(targetId);
                if (def && def.prefab)
                {
                    var inst = Instantiate(def.prefab, transform);
                    inst.name = $"CoopMount_{targetId}";
                    inst.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    foreach (var col in inst.GetComponentsInChildren<Collider>(true)) Destroy(col);
                    remoteMountComp = inst;
                    remoteMountVisual = inst.gameObject;
                    builtMountId = targetId;
                }

                if (remoteAnimator)
                {
                    remoteRiderHips = FindRiderHips(remoteAnimator);
                    if (remoteVisualRoot)
                        foreach (var pav in remoteVisualRoot.GetComponentsInChildren<ProceduralActorVisual>(true))
                            pav.SetWeaponVisualVisible(false);
                    foreach (var r in remoteAnimator.GetComponentsInChildren<Renderer>(true))
                    {
                        if (r.name.ToLowerInvariant().Contains("axe"))
                        {
                            remoteWeaponRenderer = r;
                            remoteWeaponWasEnabled = r.enabled;
                            r.enabled = false;
                            break;
                        }
                    }
                    if (remoteAnimator.HasState(0, Animator.StringToHash("Ride")))
                    {
                        remoteAnimator.CrossFadeInFixedTime("Ride", 0.08f, 0, 0f);
                        currentRemoteAnim = "Ride";
                    }
                }
                PositionRemoteRider();
            }
            else
            {
                ClearRemoteMountVisual();
                if (remoteAnimator && currentRemoteAnim == "Ride" && Health.Value > 0)
                    currentRemoteAnim = "";
            }
        }

        public static byte SanitizeSocialRole(int role) => (byte)Mathf.Clamp(role, 0, 3);

        public static string GetSocialRoleName(byte role) => role switch
        {
            1 => "Tank",
            2 => "Healer",
            3 => "DPS",
            _ => "Auto"
        };

        public static string GetSocialRoleTag(byte role) => role switch
        {
            1 => "[T]",
            2 => "[H]",
            3 => "[D]",
            _ => ""
        };

        public static float ComputeRemoteMountGroundOffset(bool airborne, bool hitFound, float hitY, float rootY)
        {
            if (airborne || !hitFound) return 0f;
            return Mathf.Clamp(hitY - rootY + 0.02f, -0.35f, 0.25f);
        }

        void PositionRemoteRider()
        {
            if (!IsMounted.Value || Health.Value <= 0 || !remoteVisualRoot || !remoteMountComp || !remoteMountComp.saddle || !remoteRiderHips)
                return;

            bool airborne = (LocomotionFlags.Value & 1) != 0;
            bool groundHitFound = false;
            float groundHitY = transform.position.y;
            if (!airborne && Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out var ground, 1.2f, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                if (ground.collider && !ground.collider.transform.IsChildOf(transform))
                {
                    groundHitFound = true;
                    groundHitY = ground.point.y;
                }
            }
            float yOff = ComputeRemoteMountGroundOffset(airborne, groundHitFound, groundHitY, transform.position.y);
            remoteMountComp.transform.localPosition = Vector3.up * yOff;

            Vector3 hipWorldOffset = remoteRiderHips.position - remoteVisualRoot.transform.position;
            remoteVisualRoot.transform.position = remoteMountComp.saddle.position - hipWorldOffset;
        }

        void LateUpdate()
        {
            if (!IsSpawned || IsOwner) return;
            PositionRemoteRider();
        }

        void Update()
        {
            if (!IsSpawned) return;

            if (IsServer)
            {
                if (Health.Value <= 0 && IsMounted.Value)
                    IsMounted.Value = false;
                TickServerSprintState();
                TickServerActiveCast();
            }

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
                    var motor = localPlayer.GetComponent<CharacterController>();
                    bool airborne = motor && !motor.isGrounded;
                    byte flags = (byte)((airborne ? 1 : 0) | (localPlayer.Walking ? 2 : 0) | (localPlayer.Blocking ? 4 : 0));
                    byte resPct = (byte)Mathf.RoundToInt(Mathf.Clamp01(localPlayer.ClassResource / Mathf.Max(1f, localPlayer.MaxClassResource)) * 100f);
                    bool inCombat = localPlayer.InCombat();
                    SubmitMoveStateServerRpc(curPos, localPlayer.transform.eulerAngles.y, spd, flags, resPct, inCombat);
                }
            }
            else
            {
                UpdateRemoteAnimation();
            }
        }

        void UpdateRemoteAnimation()
        {
            if (!remoteAnimator) return;
            Vector3 curWorldPos = transform.position;
            Vector3 worldVel = hasLastRemotePos && Time.deltaTime > 0.0001f
                ? (curWorldPos - lastRemoteWorldPos) / Time.deltaTime
                : Vector3.zero;
            lastRemoteWorldPos = curWorldPos;
            hasLastRemotePos = true;
            worldVel.y = 0f;

            float netSpd = MoveSpeed.Value;
            if (netSpd > 0.2f) remoteZeroSpeedTimer = 0f;
            else remoteZeroSpeedTimer += Time.deltaTime;
            float effectiveSpeed = netSpd > 0.2f ? netSpd : (remoteZeroSpeedTimer < 0.12f ? remoteSmoothedSpeed : 0f);
            remoteSmoothedSpeed = Mathf.Lerp(remoteSmoothedSpeed, effectiveSpeed, 1f - Mathf.Exp(-14f * Time.deltaTime));

            byte flags = LocomotionFlags.Value;
            bool airborne = (flags & 1) != 0;
            bool walking = (flags & 2) != 0;
            bool blocking = (flags & 4) != 0;

            float targetYaw = 0f;
            Vector3 localVel = transform.InverseTransformDirection(worldVel);
            bool movingBack = localVel.z < -0.25f && remoteSmoothedSpeed > 0.25f;
            if (Health.Value > 0 && !IsMounted.Value && !blocking && Time.time >= actionAnimUntil && worldVel.sqrMagnitude > 0.09f)
            {
                float signedAngle = Vector3.SignedAngle(transform.forward, worldVel.normalized, Vector3.up);
                if (Mathf.Abs(signedAngle) <= 105f) targetYaw = Mathf.Clamp(signedAngle, -52f, 52f);
                else targetYaw = Mathf.Clamp(Vector3.SignedAngle(-transform.forward, worldVel.normalized, Vector3.up), -30f, 30f);
            }
            remoteVisualYawOffset = Mathf.Lerp(remoteVisualYawOffset, targetYaw, 1f - Mathf.Exp(-14f * Time.deltaTime));
            if (remoteVisualRoot)
                remoteVisualRoot.transform.localRotation = remoteBaseVisualRot * Quaternion.Euler(0f, remoteVisualYawOffset, 0f);

            // Strict priority order: Dead > Mounted > Blocking > Sequenced One-Shot Action > Airborne > Locomotion
            string desired;
            if (Health.Value <= 0)
            {
                actionAnimUntil = 0f;
                ClearRemoteMountVisual();
                desired = remoteAnimator.HasState(0, Animator.StringToHash("Death")) ? "Death" : "Hit";
            }
            else if (IsMounted.Value)
            {
                actionAnimUntil = 0f;
                if (!remoteMountVisual) UpdateRemoteMountVisual();
                if (remoteMountComp && remoteMountComp.animator)
                    remoteMountComp.Animate(remoteSmoothedSpeed, airborne, walking);
                desired = "Ride";
            }
            else if (blocking)
            {
                actionAnimUntil = 0f;
                desired = remoteAnimator.HasState(0, Animator.StringToHash("Block")) ? "Block" : "Idle";
            }
            else if (Time.time < actionAnimUntil)
            {
                remoteAnimator.speed = 1f;
                return;
            }
            else if (airborne && remoteAnimator.HasState(0, Animator.StringToHash("Jump")))
            {
                desired = "Jump";
            }
            else if (remoteSmoothedSpeed > 3.6f && !walking)
                desired = movingBack && remoteAnimator.HasState(0, Animator.StringToHash("RunBack")) ? "RunBack" : "Run";
            else if (remoteSmoothedSpeed > 0.25f)
                desired = movingBack && remoteAnimator.HasState(0, Animator.StringToHash("WalkBack")) ? "WalkBack" : "Walk";
            else
                desired = "Idle";

            CharacterVisualFactory.GetCalibratedStrides(builtRace, builtClass, out float walkStride, out float runStride);
            if (desired == "Run" || desired == "RunBack")
                remoteAnimator.speed = Mathf.Clamp(remoteSmoothedSpeed / Mathf.Max(0.1f, desired == "RunBack" ? runStride * 0.85f : runStride), 0.75f, 2.25f);
            else if (desired == "Walk" || desired == "WalkBack")
                remoteAnimator.speed = Mathf.Clamp(remoteSmoothedSpeed / Mathf.Max(0.1f, desired == "WalkBack" ? walkStride * 0.92f : walkStride), 0.65f, 2.25f);
            else
                remoteAnimator.speed = 1f;

            if (desired == currentRemoteAnim) return;
            if (remoteAnimator.HasState(0, Animator.StringToHash(desired)))
            {
                remoteAnimator.CrossFadeInFixedTime(desired, desired == "Death" || desired == "Ride" ? 0.08f : 0.14f, 0, 0f);
                currentRemoteAnim = desired;
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void SubmitCharacterIdentityServerRpc(string pName, string race, string cls)
        {
            string cleanName = string.IsNullOrWhiteSpace(pName) ? $"Player {OwnerClientId}" : pName.Trim();
            string cleanRace = string.IsNullOrWhiteSpace(race) ? "Human" : race.Trim();
            string cleanClass = string.IsNullOrWhiteSpace(cls) ? "Warrior" : cls.Trim();

            if (!serverIdentityResolved)
            {
                PlayerName.Value = new FixedString64Bytes(cleanName);
                Race.Value = new FixedString32Bytes(cleanRace);
                ClassName.Value = new FixedString32Bytes(cleanClass);
                serverFaction = (cleanRace == "Orc" || cleanRace == "Brute" || cleanRace == "Undead" || cleanRace == "Goblin") ? "Horde" : "Alliance";
                serverIdentityResolved = true;
            }

            if (!serverHydrated)
            {
                if (RealmPersistenceService.IsInitialized)
                    HydrateServerRealmCharacterIfInitialized();
                else
                {
                    RecalculateServerVitals(preserveCurrentHp: false);
                    serverHydrated = true;
                }
            }

            SendCharacterHydrationSnapshotToOwner();
        }

        void SendCharacterHydrationSnapshotToOwner()
        {
            if (!IsServer || !IsSpawned) return;

            var claimedChests = new List<string>(serverClaimedChests);
            var claimedQuests = new List<string>(serverCompletedFactionQuests);
            var defeatedEnemies = new List<string>(serverDefeatedEnemyKeys);
            var landmarks = new List<string>(serverDiscoveredLandmarks);
            var bossKills = new List<string>();

            if (RealmPersistenceService.IsInitialized)
            {
                string charKey = ResolveCharacterRewardKey();
                var save = RealmPersistenceService.LoadOrCreateCharacter(ResolveServerAccountId(), charKey, Race.Value.ToString(), ClassName.Value.ToString(), serverFaction);
                if (save != null)
                {
                    if (save.claimedChests != null)
                    {
                        for (int i = 0; i < save.claimedChests.Count; i++)
                            if (!string.IsNullOrEmpty(save.claimedChests[i]) && !claimedChests.Contains(save.claimedChests[i]))
                                claimedChests.Add(save.claimedChests[i]);
                    }
                    if (save.claimedQuests != null)
                    {
                        for (int i = 0; i < save.claimedQuests.Count; i++)
                            if (!string.IsNullOrEmpty(save.claimedQuests[i]) && !claimedQuests.Contains(save.claimedQuests[i]))
                                claimedQuests.Add(save.claimedQuests[i]);
                    }
                    if (save.defeatedEnemies != null)
                    {
                        for (int i = 0; i < save.defeatedEnemies.Count; i++)
                            if (!string.IsNullOrEmpty(save.defeatedEnemies[i]) && !defeatedEnemies.Contains(save.defeatedEnemies[i]))
                                defeatedEnemies.Add(save.defeatedEnemies[i]);
                    }
                    if (save.discoveredLandmarks != null)
                    {
                        for (int i = 0; i < save.discoveredLandmarks.Count; i++)
                            if (!string.IsNullOrEmpty(save.discoveredLandmarks[i]) && !landmarks.Contains(save.discoveredLandmarks[i]))
                                landmarks.Add(save.discoveredLandmarks[i]);
                    }
                    if (save.claimedBossKills != null)
                    {
                        for (int i = 0; i < save.claimedBossKills.Count; i++)
                            if (!string.IsNullOrEmpty(save.claimedBossKills[i]) && !bossKills.Contains(save.claimedBossKills[i]))
                                bossKills.Add(save.claimedBossKills[i]);
                    }
                }
            }

            var chunks = new List<(byte kind, FixedString512Bytes csv)>();
            PackCsvChunks(1, serverOwnedItems, chunks);
            PackCsvChunks(2, claimedChests, chunks);
            PackCsvChunks(3, claimedQuests, chunks);
            PackCsvChunks(4, defeatedEnemies, chunks);
            PackCsvChunks(5, landmarks, chunks);
            PackCsvChunks(6, bossKills, chunks);
            var reagentEntries = BuildServerReagentEntries();
            PackCsvChunks(7, reagentEntries, chunks);

            uint seq = ++serverHydrationSeq;
            var header = new RealmHydrationHeader
            {
                snapshotSeq = seq,
                accountId = new FixedString64Bytes(TruncateUtf8(ResolveServerAccountId(), 60)),
                charName = PlayerName.Value,
                activeTitle = new FixedString64Bytes(TruncateUtf8(serverActiveTitle, 60)),
                race = Race.Value,
                className = ClassName.Value,
                faction = new FixedString32Bytes(TruncateUtf8(serverFaction, 28)),
                equippedWeaponId = EquippedWeaponId.Value,
                equippedArmorId = EquippedArmorId.Value,
                equippedTrinketId = EquippedTrinketId.Value,
                mountId = new FixedString64Bytes(MountId.Value.ToString()),
                level = Mathf.Max(1, Level.Value),
                remainderXp = Mathf.Max(0, Experience.Value),
                gold = Mathf.Max(0, Gold.Value),
                health = Mathf.Clamp(Health.Value, 0, Mathf.Max(1, MaxHealth.Value)),
                maxHealth = Mathf.Max(1, MaxHealth.Value),
                potions = Mathf.Clamp(serverPotions, 0, 10),
                boneShards = Mathf.Max(0, serverBoneShards),
                questIndex = Mathf.Clamp(QuestIndex.Value, 0, 3),
                questKills = Mathf.Max(0, QuestKills.Value),
                alchemySkill = Mathf.Clamp(serverAlchemySkill, 0, ProfessionSystem.MaxSkill),
                blacksmithingSkill = Mathf.Clamp(serverBlacksmithingSkill, 0, ProfessionSystem.MaxSkill),
                engineeringSkill = Mathf.Clamp(serverEngineeringSkill, 0, ProfessionSystem.MaxSkill),
                temperedWeapon = serverTemperedWeapon,
                questAccepted = QuestAccepted.Value,
                expectChunks = chunks.Count > 0
            };

            if (IsOwner)
            {
                BeginPendingHydration(header);
                for (int i = 0; i < chunks.Count; i++)
                    AppendPendingHydrationChunk(seq, chunks[i].kind, chunks[i].csv, i == chunks.Count - 1);
                if (!header.expectChunks)
                    FinalizePendingHydration();
                return;
            }

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                var rpcTarget = RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp);
                HydrateCharacterSnapshotClientRpc(header, rpcTarget);
                for (int i = 0; i < chunks.Count; i++)
                    HydrateCharacterListChunkClientRpc(seq, chunks[i].kind, chunks[i].csv, i == chunks.Count - 1, rpcTarget);
            }
        }

        static string TruncateUtf8(string value, int maxBytes)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (Encoding.UTF8.GetByteCount(value) <= maxBytes) return value;
            var sb = new StringBuilder(value.Length);
            int bytes = 0;
            foreach (char c in value)
            {
                int cb = Encoding.UTF8.GetByteCount(new[] { c });
                if (bytes + cb > maxBytes) break;
                sb.Append(c);
                bytes += cb;
            }
            return sb.ToString();
        }

        static void PackCsvChunks(byte listKind, IReadOnlyList<string> items, List<(byte kind, FixedString512Bytes csv)> chunks)
        {
            if (items == null || items.Count == 0) return;
            var sb = new StringBuilder(440);
            int curBytes = 0;
            for (int i = 0; i < items.Count; i++)
            {
                string raw = items[i];
                if (string.IsNullOrWhiteSpace(raw)) continue;
                string clean = raw.Trim().Replace(",", "");
                if (string.IsNullOrEmpty(clean)) continue;
                int addBytes = Encoding.UTF8.GetByteCount(clean) + (sb.Length > 0 ? 1 : 0);
                if (curBytes + addBytes > 440 && sb.Length > 0)
                {
                    chunks.Add((listKind, new FixedString512Bytes(sb.ToString())));
                    sb.Clear();
                    curBytes = 0;
                    addBytes = Encoding.UTF8.GetByteCount(clean);
                }
                if (sb.Length > 0)
                {
                    sb.Append(',');
                    curBytes++;
                }
                sb.Append(clean);
                curBytes += Encoding.UTF8.GetByteCount(clean);
            }
            if (sb.Length > 0)
                chunks.Add((listKind, new FixedString512Bytes(sb.ToString())));
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void HydrateCharacterSnapshotClientRpc(RealmHydrationHeader header, RpcParams rpcParams = default)
        {
            if (!IsOwner) return;
            BeginPendingHydration(header);
            if (!header.expectChunks)
                FinalizePendingHydration();
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void HydrateCharacterListChunkClientRpc(uint snapshotSeq, byte listKind, FixedString512Bytes packedCsv, bool isFinalChunk, RpcParams rpcParams = default)
        {
            if (!IsOwner) return;
            AppendPendingHydrationChunk(snapshotSeq, listKind, packedCsv, isFinalChunk);
        }

        void BeginPendingHydration(RealmHydrationHeader header)
        {
            hasPendingHydrationHeader = true;
            hasReadyClientHydrationSnapshot = false;
            pendingHydrationHeader = header;
            pendingHydrationOwnedItems.Clear();
            pendingHydrationClaimedChests.Clear();
            pendingHydrationClaimedQuests.Clear();
            pendingHydrationDefeatedEnemies.Clear();
            pendingHydrationLandmarks.Clear();
            pendingHydrationBossKills.Clear();
            pendingHydrationReagents.Clear();
        }

        void AppendPendingHydrationChunk(uint snapshotSeq, byte listKind, FixedString512Bytes packedCsv, bool isFinalChunk)
        {
            if (!hasPendingHydrationHeader || pendingHydrationHeader.snapshotSeq != snapshotSeq) return;
            string csv = packedCsv.ToString();
            if (!string.IsNullOrEmpty(csv))
            {
                var targetList = listKind switch
                {
                    1 => pendingHydrationOwnedItems,
                    2 => pendingHydrationClaimedChests,
                    3 => pendingHydrationClaimedQuests,
                    4 => pendingHydrationDefeatedEnemies,
                    5 => pendingHydrationLandmarks,
                    6 => pendingHydrationBossKills,
                    7 => pendingHydrationReagents,
                    _ => null
                };
                if (targetList != null)
                {
                    string[] parts = csv.Split(',');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(parts[i]))
                            targetList.Add(parts[i].Trim());
                    }
                }
            }
            if (isFinalChunk)
                FinalizePendingHydration();
        }

        void FinalizePendingHydration()
        {
            if (!hasPendingHydrationHeader) return;
            hasReadyClientHydrationSnapshot = true;
            ApplyPendingHydrationToLocalPlayer();
        }

        void ApplyPendingHydrationToLocalPlayer()
        {
            if (!hasReadyClientHydrationSnapshot) return;
            if (!localPlayer) localPlayer = FindFirstObjectByType<PlayerController>();
            if (!localPlayer) return;

            hasReadyClientHydrationSnapshot = false;
            hasPendingHydrationHeader = false;
            var h = pendingHydrationHeader;
            localPlayer.ApplyServerRealmHydration(
                h.charName.ToString(),
                h.race.ToString(),
                h.className.ToString(),
                h.faction.ToString(),
                h.level,
                h.remainderXp,
                h.gold,
                h.health,
                h.maxHealth,
                h.potions,
                h.temperedWeapon,
                h.boneShards,
                h.questIndex,
                h.questAccepted,
                h.questKills,
                pendingHydrationOwnedItems,
                h.equippedWeaponId.ToString(),
                h.equippedArmorId.ToString(),
                h.equippedTrinketId.ToString(),
                pendingHydrationClaimedChests,
                pendingHydrationBossKills,
                pendingHydrationClaimedQuests,
                pendingHydrationDefeatedEnemies,
                pendingHydrationLandmarks,
                h.activeTitle.ToString(),
                h.alchemySkill,
                h.blacksmithingSkill,
                h.engineeringSkill,
                pendingHydrationReagents);
        }

        // ── O-01 Authoritative Movement ─────────────────────────────────────────────────

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void SubmitMoveStateServerRpc(Vector3 pos, float yaw, float speed, byte flags, byte resourcePct, bool inCombat)
        {
            float dt = Mathf.Clamp(Time.time - lastServerMoveTime, 0.02f, 0.5f);
            lastServerMoveTime = Time.time;

            if (Health.Value <= 0)
            {
                if (IsMounted.Value) IsMounted.Value = false;
                if (IsSprinting.Value) IsSprinting.Value = false;
                serverSprintUntil = -10f;
                return;
            }

            TickServerSprintState();
            float maxSpeed = IsMounted.Value ? 15.5f : (IsServerSprinting ? 14.5f : 9.5f);
            float maxBudget = Mathf.Max(0.95f, maxSpeed * 0.14f);
            serverMoveBudget = Mathf.Min(maxBudget, serverMoveBudget + maxSpeed * dt);

            float horizDist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(pos.x, pos.z));
            float dist = Mathf.Max(horizDist, Mathf.Abs(pos.y - transform.position.y) * 0.5f);

            if (hostTeleportAuthorized)
            {
                if (Vector3.Distance(pos, authorizedTeleportTarget) <= 3.5f)
                {
                    hostTeleportAuthorized = false;
                    serverMoveBudget = maxBudget;
                    transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
                }
                else
                {
                    return; // Ignore stale pre-teleport movement packets until client acknowledges teleport
                }
            }
            else if (dist <= serverMoveBudget)
            {
                serverMoveBudget = Mathf.Max(0f, serverMoveBudget - dist);
                transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            }
            else if (serverBlinkAllowanceMeters > 0f && Time.time <= serverBlinkExpiresAt && dist <= serverMoveBudget + serverBlinkAllowanceMeters)
            {
                // One-shot Blink displacement allowance granted only on server-validated Lv 5+ Arcane Ward
                serverBlinkAllowanceMeters = 0f;
                serverBlinkExpiresAt = 0f;
                serverMoveBudget = 0f;
                transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            }
            else
            {
                float ratio = dist > 0.0001f ? Mathf.Clamp01(serverMoveBudget / dist) : 0f;
                Vector3 clamped = Vector3.Lerp(transform.position, pos, ratio);
                serverMoveBudget = 0f;
                transform.SetPositionAndRotation(clamped, Quaternion.Euler(0f, yaw, 0f));
                CorrectPositionClientRpc(clamped, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }

            if (serverCastActive && !serverCastReleased && Vector3.Distance(transform.position, serverCastStartPos) > 0.25f)
                CancelActiveServerCast(ServerCastCancelReason.Moved);

            MoveSpeed.Value = Mathf.Clamp(speed, 0f, maxSpeed);
            LocomotionFlags.Value = (byte)(flags & 0x07);
            ResourcePct.Value = (byte)Mathf.Min(100, (int)resourcePct);
            IsInCombat.Value = inCombat;
        }

        public void AuthorizeHostTeleport(Vector3 destination)
        {
            if (!IsServer) return;
            hostTeleportAuthorized = true;
            authorizedTeleportTarget = destination;
            serverMoveBudget = 1.5f;
            transform.position = destination;
            if (serverCastActive && !serverCastReleased && Vector3.Distance(destination, serverCastStartPos) > 0.25f)
                CancelActiveServerCast(ServerCastCancelReason.Moved);
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
            SubmitMoveStateServerRpc(targetPos, transform.eulerAngles.y, 10f, 0, 100, false);
        }

        public void RequestSetMount(bool mounted, string mountId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            RequestSetMountServerRpc(mounted, string.IsNullOrEmpty(mountId) ? "horse" : mountId);
        }

        public void RequestSetEquippedWeapon(string weaponId) => RequestEquipItem(weaponId);

        public void RequestEquipItem(string itemId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || !IsOwner) return;
            string clean = string.IsNullOrWhiteSpace(itemId) ? "" : itemId.Trim();
            if (string.IsNullOrEmpty(clean) || clean.Length > 48) return;
            if (IsServer)
                TryEquipItemOnServer(clean);
            else
                RequestEquipItemServerRpc(new FixedString64Bytes(clean));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestEquipItemServerRpc(FixedString64Bytes itemId) =>
            TryEquipItemOnServer(itemId.ToString());

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void SubmitEquippedWeaponServerRpc(FixedString64Bytes weaponId) =>
            TryEquipItemOnServer(weaponId.ToString());

        bool TryEquipItemOnServer(string itemId)
        {
            if (!IsServer || !IsSpawned || Health.Value <= 0) return false;
            string clean = string.IsNullOrWhiteSpace(itemId) ? "" : itemId.Trim();
            if (string.IsNullOrEmpty(clean) || clean.Length > 48) return false;
            var item = ItemCatalog.Get(clean);
            if (item == null || !serverOwnedItems.Contains(item.id)) return false;

            if ((item.slot == GearSlot.Armor || item.slot == GearSlot.Trinket) &&
                (IsInCombat.Value || Time.time - serverLastDamagedAt < 5f))
                return false;

            switch (item.slot)
            {
                case GearSlot.Weapon:
                    EquippedWeaponId.Value = new FixedString64Bytes(item.id);
                    break;
                case GearSlot.Armor:
                    EquippedArmorId.Value = new FixedString64Bytes(item.id);
                    break;
                case GearSlot.Trinket:
                    EquippedTrinketId.Value = new FixedString64Bytes(item.id);
                    break;
            }

            RecalculateServerVitals(preserveCurrentHp: true);
            PersistServerRealmCharacterIfInitialized();
            return true;
        }

        int GetServerArmorRating()
        {
            var armor = ItemCatalog.Get(EquippedArmorId.Value.ToString());
            var trinket = ItemCatalog.Get(EquippedTrinketId.Value.ToString());
            int raceArmor = Race.Value.ToString() == "Undead" ? 3 : 0;
            return (armor != null ? armor.armor : 0) + (trinket != null ? trinket.armor : 0) + raceArmor;
        }

        int GetServerWeaponDamageBonus()
        {
            var weapon = ItemCatalog.Get(EquippedWeaponId.Value.ToString());
            var trinket = ItemCatalog.Get(EquippedTrinketId.Value.ToString());
            return (weapon != null ? weapon.damage : 0) + (trinket != null ? trinket.damage : 0) + (serverTemperedWeapon ? 4 : 0);
        }

        void RecalculateServerVitals(bool preserveCurrentHp)
        {
            if (!IsServer) return;
            string cls = ClassName.Value.ToString();
            int baseHp = cls == "Warrior" || cls == "Paladin" ? 130 : (cls == "Mage" ? 90 : 105);
            int levelHp = (Mathf.Max(1, Level.Value) - 1) * 15;
            int raceHp = Race.Value.ToString() == "Brute" ? 25 : 0;
            var armor = ItemCatalog.Get(EquippedArmorId.Value.ToString());
            var trinket = ItemCatalog.Get(EquippedTrinketId.Value.ToString());
            int gearHp = (armor != null ? armor.health : 0) + (trinket != null ? trinket.health : 0);
            int newMax = Mathf.Max(1, baseHp + levelHp + raceHp + gearHp);
            MaxHealth.Value = newMax;
            if (preserveCurrentHp)
                Health.Value = Mathf.Clamp(Health.Value, 0, newMax);
            else
                Health.Value = newMax;
        }

        int ServerGreenhollowCount() =>
            (serverDiscoveredLandmarks.Contains("willow-crossing") ? 1 : 0) +
            (serverDiscoveredLandmarks.Contains("greenhollow-post") ? 1 : 0) +
            (serverDiscoveredLandmarks.Contains("ancient-grove") ? 1 : 0);

        public static bool IsSupportedMountId(string normalized) =>
            normalized == "horse" || normalized == "zebra" || normalized == "gryphon";

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestSetMountServerRpc(bool mounted, string mountId)
        {
            string normalized = string.IsNullOrEmpty(mountId) ? "horse" : mountId.Trim().ToLowerInvariant();
            if (!mounted)
            {
                IsMounted.Value = false;
                if (IsSupportedMountId(normalized)) MountId.Value = new FixedString32Bytes(normalized);
                return;
            }
            if (Health.Value <= 0) { IsMounted.Value = false; return; }
            if (!IsSupportedMountId(normalized)) return;
            if (normalized == "zebra" && ServerGreenhollowCount() < 3) return;
            if (serverCastActive && !serverCastReleased)
                CancelActiveServerCast(ServerCastCancelReason.Mounted);
            if (IsSprinting.Value) IsSprinting.Value = false;
            serverSprintUntil = -10f;
            MountId.Value = new FixedString32Bytes(normalized);
            IsMounted.Value = true;
        }

        public static bool IsSupportedActionAnim(string actionId) =>
            actionId == "Attack" || actionId == "Cleave" || actionId == "Cast" ||
            actionId == "Block" || actionId == "Dodge" || actionId == "Hit" || actionId == "Cancel";

        public void TriggerActionAnim(string animName, float duration = 0.55f)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || !IsOwner) return;
            if (!IsSupportedActionAnim(animName)) return;
            uint seq = ++localActionSeq;
            float clampedDur = animName == "Cancel" ? 0f : Mathf.Clamp(duration, 0.12f, 2.2f);
            if (IsServer) ApplyAndBroadcastActionAnim(animName, clampedDur, seq);
            else RequestActionAnimServerRpc(animName, clampedDur, seq);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestActionAnimServerRpc(string animName, float duration, uint seq) =>
            ApplyAndBroadcastActionAnim(animName, duration, seq);

        void ApplyAndBroadcastActionAnim(string animName, float duration, uint seq)
        {
            if (!IsServer || !IsSupportedActionAnim(animName)) return;
            if (seq <= lastClientActionSeq) return;
            if (animName != "Cancel" && (Health.Value <= 0 || IsMounted.Value)) return;
            lastClientActionSeq = seq;
            lastClientActionTime = Time.time;
            float clampedDur = animName == "Cancel" ? 0f : Mathf.Clamp(duration, 0.12f, 2.2f);
            BroadcastActionAnimClientRpc(animName, clampedDur, ++lastBroadcastSeq);
        }

        void BroadcastServerFallbackActionAnim(string animName, float duration)
        {
            if (!IsServer || !IsSupportedActionAnim(animName)) return;
            if (Time.time < lastClientActionTime + 0.35f) return;
            if (Health.Value <= 0 || IsMounted.Value) return;
            BroadcastActionAnimClientRpc(animName, Mathf.Clamp(duration, 0.12f, 2.2f), ++lastBroadcastSeq);
        }

        [Rpc(SendTo.Everyone)]
        void BroadcastActionAnimClientRpc(string animName, float duration, uint seq)
        {
            if (IsOwner || !IsSupportedActionAnim(animName)) return;
            if (seq <= lastObservedActionSeq) return;
            lastObservedActionSeq = seq;

            if (animName == "Cancel")
            {
                actionAnimUntil = 0f;
                return;
            }
            if (Health.Value <= 0 || IsMounted.Value) return;

            if (animName == "Dodge")
            {
                SpellFX.CreateArcaneBurst(transform.position);
                if (remoteAnimator)
                {
                    string dodgeAnim = remoteAnimator.HasState(0, Animator.StringToHash("Jump")) ? "Jump" : "Run";
                    if (remoteAnimator.HasState(0, Animator.StringToHash(dodgeAnim)))
                    {
                        remoteAnimator.CrossFadeInFixedTime(dodgeAnim, 0.06f, 0, 0f);
                        actionAnimUntil = Time.time + Mathf.Clamp(duration, 0.2f, 0.45f);
                        currentRemoteAnim = dodgeAnim;
                    }
                }
                return;
            }

            if (!remoteAnimator) return;
            string resolved = animName;
            if (animName == "Cast" && !remoteAnimator.HasState(0, Animator.StringToHash("Cast")))
                resolved = remoteAnimator.HasState(0, Animator.StringToHash("Cleave")) ? "Cleave" : "Attack";

            if (remoteAnimator.HasState(0, Animator.StringToHash(resolved)))
            {
                remoteAnimator.CrossFadeInFixedTime(resolved, 0.08f, 0, 0f);
                actionAnimUntil = Time.time + Mathf.Clamp(duration, 0.12f, 2.2f);
                currentRemoteAnim = resolved;
            }
        }

        // ── O-01 Host-Authoritative Combat & Server-Timed Casts ────────────────────────

        public const int Slot1UnlockLevel = 1;
        public const int Slot2UnlockLevel = 3;
        public const int Slot3UnlockLevel = 5;

        public const float FireballCastDuration = 1.20f;
        public const float HolyLightCastDuration = 1.50f;
        public const float CastMoveInterruptThreshold = 0.25f;

        public static event System.Action<string, int, int> OnAbilityRejectedLocked;
        public static event System.Action<ServerCastEvent> OnServerCastStarted;
        public static event System.Action<ServerCastEvent> OnServerCastCanceled;
        public static event System.Action<ServerCastEvent> OnServerCastReleased;
        public static event System.Action<ServerCastEvent> OnServerCastEvent;

        public bool HasActiveServerCast => IsServer && serverCastActive && !serverCastReleased;
        public uint ServerLastReleasedCastSeq => serverLastReleasedCastSeq;
        public uint ServerLastCanceledCastSeq => serverLastCanceledCastSeq;
        public ServerCastCancelReason ServerLastCancelReason => serverLastCancelReason;

        public bool HasActiveClientCast => activeClientCastSeq != 0 && activeClientCastPhase == ServerCastPhase.Started;
        public uint ActiveClientCastSeq => activeClientCastSeq;
        public string ActiveClientCastAbilityId => activeClientCastAbility;
        public string ActiveClientCastTargetEnemyKey => activeClientCastTargetKey;
        public float ActiveClientCastDuration => activeClientCastDuration;
        public float ActiveClientCastStartTime => activeClientCastStartTime;
        public double ActiveClientCastServerStartTime => activeClientCastServerStartTime;
        public float ActiveClientCastProgress => HasActiveClientCast ? Mathf.Clamp01((Time.time - activeClientCastStartTime) / Mathf.Max(0.05f, activeClientCastDuration)) : 0f;
        public float ActiveClientCastRemaining => HasActiveClientCast ? Mathf.Max(0f, activeClientCastDuration - (Time.time - activeClientCastStartTime)) : 0f;
        public ServerCastPhase LastClientCastPhase => activeClientCastPhase;
        public ServerCastCancelReason LastClientCastCancelReason => lastClientCastCancelReason;
        public uint LastReleasedCastSeq => lastReleasedCastSeq;
        public uint LastCanceledCastSeq => lastCanceledCastSeq;

        public static bool IsCastTimedAbility(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId)) return false;
            string norm = abilityId.Trim().ToLowerInvariant();
            return norm == "fireball" || norm == "holy_light";
        }

        public static float GetServerCastDuration(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId)) return 0f;
            switch (abilityId.Trim().ToLowerInvariant())
            {
                case "fireball": return FireballCastDuration;
                case "holy_light": return HolyLightCastDuration;
                default: return 0f;
            }
        }

        public string LastLockedAbilityRejection { get; private set; } = string.Empty;
        public int LastLockedRequiredLevel { get; private set; }
        public int LastLockedServerLevel { get; private set; }

        public static bool TryGetAbilitySlotRequirement(string abilityId, out int slot, out int requiredLevel)
        {
            slot = 0;
            requiredLevel = int.MaxValue;
            if (string.IsNullOrWhiteSpace(abilityId)) return false;
            switch (abilityId.Trim().ToLowerInvariant())
            {
                case "melee":
                case "fireball":
                case "interrupt":
                    slot = 1;
                    requiredLevel = Slot1UnlockLevel;
                    return true;
                case "frostnova":
                case "eviscerate":
                case "consecration":
                case "whirlwind":
                    slot = 2;
                    requiredLevel = Slot2UnlockLevel;
                    return true;
                case "arcane_ward":
                case "sprint":
                case "holy_light":
                case "second_wind":
                    slot = 3;
                    requiredLevel = Slot3UnlockLevel;
                    return true;
                default:
                    return false;
            }
        }

        public static int GetRequiredLevelForAbility(string abilityId) =>
            TryGetAbilitySlotRequirement(abilityId, out _, out int req) ? req : int.MaxValue;

        public static bool IsAbilityUnlockedForLevel(string abilityId, int characterLevel) =>
            TryGetAbilitySlotRequirement(abilityId, out _, out int req) && characterLevel >= req;

        public float GetServerCooldownRemainingForTest(string cdKey)
        {
            if (string.IsNullOrEmpty(cdKey)) return 0f;
            return serverAbilityCooldowns.TryGetValue(cdKey, out float readyAt) ? Mathf.Max(0f, readyAt - Time.time) : 0f;
        }

        public void SetServerLevelForTest(int level)
        {
            if (!IsServer) return;
            Level.Value = Mathf.Max(1, level);
        }

        public void ClearLastLockedRejectionForTest()
        {
            LastLockedAbilityRejection = string.Empty;
            LastLockedRequiredLevel = 0;
            LastLockedServerLevel = 0;
        }

        void RejectLockedAbilityOnServer(string key, int requiredLevel, int serverLevel)
        {
            RecordLockedAbilityRejection(key, requiredLevel, serverLevel);
            if (!IsOwner && NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                NotifyAbilityLockedClientRpc(key, requiredLevel, serverLevel, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void NotifyAbilityLockedClientRpc(string abilityId, int requiredLevel, int serverLevel, RpcParams rpcParams = default)
        {
            if (!IsOwner) return;
            RecordLockedAbilityRejection(abilityId, requiredLevel, serverLevel);
        }

        void RecordLockedAbilityRejection(string abilityId, int requiredLevel, int serverLevel)
        {
            LastLockedAbilityRejection = abilityId ?? string.Empty;
            LastLockedRequiredLevel = requiredLevel;
            LastLockedServerLevel = serverLevel;
            OnAbilityRejectedLocked?.Invoke(LastLockedAbilityRejection, requiredLevel, serverLevel);
        }

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
            bool knownId = key == "melee" || key == "fireball" || key == "frostnova" || key == "eviscerate" ||
                           key == "consecration" || key == "whirlwind" || key == "interrupt";
            if (!knownId) return;

            if ((key == "fireball" || key == "frostnova") && cls != "Mage") return;
            if (key == "eviscerate" && cls != "Rogue") return;
            if (key == "consecration" && cls != "Paladin") return;
            if (key == "whirlwind" && cls != "Warrior") return;

            int serverLevel = Mathf.Max(1, Level.Value);
            if (!TryGetAbilitySlotRequirement(key, out _, out int requiredLevel) || serverLevel < requiredLevel)
            {
                RejectLockedAbilityOnServer(key, requiredLevel, serverLevel);
                return;
            }

            bool isAoe = key == "frostnova" || key == "consecration" || key == "whirlwind";
            string cdKey = isAoe ? $"{key}:{enemy.NetworkKey}" : key;
            float cd = key switch
            {
                "melee" => 0.35f,
                "fireball" => 1.1f,
                "whirlwind" => 1.45f,
                "eviscerate" => 3.5f,
                "frostnova" => 5.5f,
                "consecration" => 5.5f,
                "interrupt" => 11.5f,
                _ => 0.8f
            };
            if (serverAbilityCooldowns.TryGetValue(cdKey, out float readyAt) && Time.time < readyAt) return;

            float maxRange = key == "fireball" ? 30f :
                             key == "interrupt" ? (cls == "Mage" ? 25f : 5.0f) :
                             (isAoe ? 8.5f : 5.0f);
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
                if (!CastBarSystem.IsTargetCasting(stats)) return;
                bool interrupted = CastBarSystem.InterruptTarget(stats, "Interrupt");
                if (interrupted) serverAbilityCooldowns[cdKey] = Time.time + cd;
                BroadcastServerFallbackActionAnim("Attack", 0.35f);
                return;
            }

            if (key == "fireball")
            {
                TryBeginServerTimedCast("fireball", enemy.NetworkKey);
                return;
            }

            serverAbilityCooldowns[cdKey] = Time.time + cd;

            float baseDmg = key switch
            {
                "fireball" => 34f,
                "frostnova" => 24f,
                "consecration" => 24f,
                "whirlwind" => 28f,
                "eviscerate" => 46f,
                _ => 18f
            };
            float computedDamage = baseDmg + (Level.Value - 1) * 4f + GetServerWeaponDamageBonus();
            if (Race.Value.ToString() == "Brute") computedDamage *= 1.15f;

            float hpBeforeHit = stats.Health;
            stats.Damage(computedDamage);
            float actualApplied = Mathf.Max(0f, hpBeforeHit - stats.Health);
            if (key == "melee" && actualApplied > 0f && Race.Value.ToString() == "Undead" && Health.Value > 0)
                Health.Value = Mathf.Min(MaxHealth.Value, Health.Value + 6);

            if (actualApplied > 0f)
                ConfirmAbilityHitClientRpc(enemy.NetworkKey, Mathf.RoundToInt(actualApplied), RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            BroadcastServerFallbackActionAnim(key == "frostnova" || key == "consecration" ? "Cast" : "Attack", 0.50f);
            BroadcastEnemyState(enemy);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ConfirmAbilityHitClientRpc(string enemyKey, int serverDamage, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            var enemyCombatant = enemy ? enemy.GetComponent<Combatant>() : null;
            if (enemyCombatant) localPlayer.ApplyConfirmedHitPresentation(enemyCombatant, serverDamage, false);
        }

        public void RequestStartCast(string abilityId, Combatant targetCombatant = null)
        {
            string enemyKey = string.Empty;
            if (targetCombatant)
            {
                var ec = targetCombatant.GetComponent<EnemyController>();
                if (ec) enemyKey = ec.NetworkKey ?? string.Empty;
            }
            RequestStartCast(abilityId, enemyKey);
        }

        public void RequestStartCast(string abilityId, string targetEnemyKey)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return;
            if (IsServer)
                TryBeginServerTimedCast(abilityId, targetEnemyKey);
            else if (IsOwner)
                RequestStartCastServerRpc(abilityId ?? string.Empty, targetEnemyKey ?? string.Empty);
        }

        public void RequestCancelCast()
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return;
            if (IsServer)
                CancelActiveServerCast(ServerCastCancelReason.ManualCancel);
            else if (IsOwner)
                RequestCancelCastServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestStartCastServerRpc(string abilityId, string targetEnemyKey) =>
            TryBeginServerTimedCast(abilityId, targetEnemyKey);

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestCancelCastServerRpc() =>
            CancelActiveServerCast(ServerCastCancelReason.ManualCancel);

        bool TryBeginServerTimedCast(string abilityId, string enemyKey)
        {
            if (!IsServer || Health.Value <= 0 || IsMounted.Value) return false;
            string key = string.IsNullOrWhiteSpace(abilityId) ? string.Empty : abilityId.Trim().ToLowerInvariant();
            if (!IsCastTimedAbility(key)) return false;

            string cls = ClassName.Value.ToString();
            if (key == "fireball" && cls != "Mage") return false;
            if (key == "holy_light" && cls != "Paladin") return false;

            int serverLevel = Mathf.Max(1, Level.Value);
            if (!TryGetAbilitySlotRequirement(key, out _, out int requiredLevel) || serverLevel < requiredLevel)
            {
                RejectLockedAbilityOnServer(key, requiredLevel, serverLevel);
                return false;
            }

            if (serverCastActive && !serverCastReleased && serverCastAbility == key)
                return false;

            if (key == "fireball")
            {
                if (serverAbilityCooldowns.TryGetValue("fireball", out float fbReady) && Time.time < fbReady) return false;
                var enemy = EnemyController.FindByKey(enemyKey);
                if (!HasValidFireballTarget(enemy, 30f)) return false;
            }
            else if (key == "holy_light")
            {
                if (Health.Value >= MaxHealth.Value) return false;
                if (serverAbilityCooldowns.TryGetValue("heal:holy_light", out float hlReady) && Time.time < hlReady) return false;
            }

            if (serverCastActive && !serverCastReleased)
                CancelActiveServerCast(ServerCastCancelReason.Replaced);

            float duration = GetServerCastDuration(key);
            uint seq = ++serverCastSeq;
            serverCastActive = true;
            serverCastReleased = false;
            serverCastAbility = key;
            serverCastEnemyKey = enemyKey ?? string.Empty;
            serverCastDuration = duration;
            serverCastStartTime = Time.time;
            serverCastStartNetTime = NetworkManager.Singleton != null ? NetworkManager.Singleton.ServerTime.Time : Time.timeAsDouble;
            serverCastEndTime = serverCastStartTime + duration;
            serverCastStartPos = transform.position;

            BroadcastServerCastEventClientRpc(
                seq,
                key,
                serverCastEnemyKey,
                (byte)ServerCastPhase.Started,
                (byte)ServerCastCancelReason.None,
                duration,
                serverCastStartNetTime);
            return true;
        }

        bool HasValidFireballTarget(EnemyController enemy, float rangeLimit)
        {
            if (!enemy) return false;
            var stats = enemy.GetComponent<Combatant>();
            if (!stats || !stats.Alive) return false;
            if (Vector3.Distance(transform.position, enemy.transform.position) > rangeLimit) return false;

            Vector3 eye = transform.position + Vector3.up * 1.2f;
            Vector3 tgt = enemy.transform.position + Vector3.up * 1.1f;
            if (Physics.Linecast(eye, tgt, out RaycastHit hit, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                if (hit.collider && !hit.collider.transform.IsChildOf(enemy.transform) && !hit.collider.transform.IsChildOf(transform))
                    return false;
            }
            return true;
        }

        void TickServerActiveCast()
        {
            if (!IsServer || !serverCastActive || serverCastReleased) return;

            if (Health.Value <= 0)
            {
                CancelActiveServerCast(ServerCastCancelReason.Died);
                return;
            }
            if (IsMounted.Value)
            {
                CancelActiveServerCast(ServerCastCancelReason.Mounted);
                return;
            }
            if (Vector3.Distance(transform.position, serverCastStartPos) > CastMoveInterruptThreshold)
            {
                CancelActiveServerCast(ServerCastCancelReason.Moved);
                return;
            }
            if (serverLastDamagedAt > serverCastStartTime)
            {
                CancelActiveServerCast(ServerCastCancelReason.Damaged);
                return;
            }

            if (serverCastAbility == "fireball")
            {
                var enemy = EnemyController.FindByKey(serverCastEnemyKey);
                var stats = enemy ? enemy.GetComponent<Combatant>() : null;
                if (!enemy || !stats || !stats.Alive)
                {
                    CancelActiveServerCast(ServerCastCancelReason.InvalidTarget);
                    return;
                }
                if (Vector3.Distance(transform.position, enemy.transform.position) > 32f)
                {
                    CancelActiveServerCast(ServerCastCancelReason.OutOfRange);
                    return;
                }
            }

            if (Time.time >= serverCastEndTime)
                CompleteServerTimedCast();
        }

        void CancelActiveServerCast(ServerCastCancelReason reason)
        {
            if (!IsServer || !serverCastActive || serverCastReleased) return;
            uint seq = serverCastSeq;
            string ability = serverCastAbility;
            string enemyKey = serverCastEnemyKey;
            float dur = serverCastDuration;

            serverCastActive = false;
            serverLastCanceledCastSeq = seq;
            serverLastCancelReason = reason;

            BroadcastServerCastEventClientRpc(
                seq,
                ability,
                enemyKey,
                (byte)ServerCastPhase.Canceled,
                (byte)reason,
                dur,
                serverCastStartNetTime);
        }

        void CompleteServerTimedCast()
        {
            if (!IsServer || !serverCastActive || serverCastReleased) return;

            uint seq = serverCastSeq;
            string ability = serverCastAbility;
            string enemyKey = serverCastEnemyKey;
            float dur = serverCastDuration;
            double startNet = serverCastStartNetTime;

            // Lock one-time release BEFORE applying side effects so re-entrancy cannot duplicate effects.
            serverCastReleased = true;
            serverCastActive = false;

            if (ability == "fireball")
            {
                var enemy = EnemyController.FindByKey(enemyKey);
                var stats = enemy ? enemy.GetComponent<Combatant>() : null;
                if (!enemy || !stats || !stats.Alive)
                {
                    serverLastCanceledCastSeq = seq;
                    serverLastCancelReason = ServerCastCancelReason.InvalidTarget;
                    BroadcastServerCastEventClientRpc(seq, ability, enemyKey, (byte)ServerCastPhase.Canceled, (byte)ServerCastCancelReason.InvalidTarget, dur, startNet);
                    return;
                }
                if (Vector3.Distance(transform.position, enemy.transform.position) > 32f)
                {
                    serverLastCanceledCastSeq = seq;
                    serverLastCancelReason = ServerCastCancelReason.OutOfRange;
                    BroadcastServerCastEventClientRpc(seq, ability, enemyKey, (byte)ServerCastPhase.Canceled, (byte)ServerCastCancelReason.OutOfRange, dur, startNet);
                    return;
                }
                if (!HasValidFireballTarget(enemy, 32f))
                {
                    serverLastCanceledCastSeq = seq;
                    serverLastCancelReason = ServerCastCancelReason.LineOfSight;
                    BroadcastServerCastEventClientRpc(seq, ability, enemyKey, (byte)ServerCastPhase.Canceled, (byte)ServerCastCancelReason.LineOfSight, dur, startNet);
                    return;
                }

                serverAbilityCooldowns["fireball"] = Time.time + 1.1f;
                float computedDamage = 34f + (Level.Value - 1) * 4f + GetServerWeaponDamageBonus();
                if (Race.Value.ToString() == "Brute") computedDamage *= 1.15f;

                float hpBeforeHit = stats.Health;
                stats.Damage(computedDamage);
                float actualApplied = Mathf.Max(0f, hpBeforeHit - stats.Health);
                if (actualApplied > 0f)
                    ConfirmAbilityHitClientRpc(enemy.NetworkKey, Mathf.RoundToInt(actualApplied), RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
                BroadcastEnemyState(enemy);
            }
            else if (ability == "holy_light")
            {
                if (Health.Value <= 0)
                {
                    serverLastCanceledCastSeq = seq;
                    serverLastCancelReason = ServerCastCancelReason.Died;
                    BroadcastServerCastEventClientRpc(seq, ability, enemyKey, (byte)ServerCastPhase.Canceled, (byte)ServerCastCancelReason.Died, dur, startNet);
                    return;
                }
                serverAbilityCooldowns["heal:holy_light"] = Time.time + 8.0f;
                if (Health.Value < MaxHealth.Value)
                    Health.Value = Mathf.Min(MaxHealth.Value, Health.Value + 60);
            }

            serverLastReleasedCastSeq = seq;
            BroadcastServerCastEventClientRpc(
                seq,
                ability,
                enemyKey,
                (byte)ServerCastPhase.Released,
                (byte)ServerCastCancelReason.None,
                dur,
                startNet);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        void BroadcastServerCastEventClientRpc(
            uint castSeq,
            string abilityId,
            string targetEnemyKey,
            byte phaseByte,
            byte cancelReasonByte,
            float duration,
            double serverStartTime)
        {
            if (castSeq == 0 || !System.Enum.IsDefined(typeof(ServerCastPhase), phaseByte)) return;
            var phase = (ServerCastPhase)phaseByte;
            var cancelReason = System.Enum.IsDefined(typeof(ServerCastCancelReason), cancelReasonByte)
                ? (ServerCastCancelReason)cancelReasonByte
                : ServerCastCancelReason.None;

            if (castSeq < lastClientCastEventSeq) return;
            if (castSeq == lastClientCastEventSeq)
            {
                if (phase == ServerCastPhase.Started || lastClientCastEventPhase != ServerCastPhase.Started)
                    return;
            }

            lastClientCastEventSeq = castSeq;
            lastClientCastEventPhase = phase;
            activeClientCastServerStartTime = serverStartTime;

            if (phase == ServerCastPhase.Started)
            {
                activeClientCastSeq = castSeq;
                activeClientCastAbility = abilityId ?? string.Empty;
                activeClientCastTargetKey = targetEnemyKey ?? string.Empty;
                activeClientCastDuration = Mathf.Max(0.1f, duration);
                activeClientCastStartTime = Time.time;
                activeClientCastPhase = ServerCastPhase.Started;
                lastClientCastCancelReason = ServerCastCancelReason.None;

                if (!IsOwner) PlayObserverCastVisual(Mathf.Clamp(duration, 0.25f, 2.2f), false);
            }
            else if (phase == ServerCastPhase.Canceled)
            {
                activeClientCastSeq = 0;
                activeClientCastAbility = string.Empty;
                activeClientCastTargetKey = string.Empty;
                activeClientCastDuration = 0f;
                activeClientCastPhase = ServerCastPhase.Canceled;
                lastClientCastCancelReason = cancelReason;
                lastCanceledCastSeq = castSeq;

                if (!IsOwner) actionAnimUntil = 0f;
            }
            else if (phase == ServerCastPhase.Released)
            {
                activeClientCastSeq = 0;
                activeClientCastAbility = string.Empty;
                activeClientCastTargetKey = string.Empty;
                activeClientCastDuration = 0f;
                activeClientCastPhase = ServerCastPhase.Released;
                lastClientCastCancelReason = ServerCastCancelReason.None;
                lastReleasedCastSeq = castSeq;

                if (!IsOwner) PlayObserverCastVisual(0.35f, true);
            }

            var ev = new ServerCastEvent(
                this,
                IsOwner,
                castSeq,
                abilityId,
                targetEnemyKey,
                phase,
                cancelReason,
                duration,
                serverStartTime);

            OnServerCastEvent?.Invoke(ev);
            if (phase == ServerCastPhase.Started) OnServerCastStarted?.Invoke(ev);
            else if (phase == ServerCastPhase.Canceled) OnServerCastCanceled?.Invoke(ev);
            else if (phase == ServerCastPhase.Released) OnServerCastReleased?.Invoke(ev);
        }

        void PlayObserverCastVisual(float duration, bool releaseFlash)
        {
            if (Health.Value <= 0 || IsMounted.Value || !remoteAnimator) return;
            string preferred = releaseFlash ? "Attack" : "Cast";
            string resolved = preferred;
            if (!remoteAnimator.HasState(0, Animator.StringToHash(resolved)))
                resolved = remoteAnimator.HasState(0, Animator.StringToHash("Cleave")) ? "Cleave" : "Attack";
            if (remoteAnimator.HasState(0, Animator.StringToHash(resolved)))
            {
                remoteAnimator.CrossFadeInFixedTime(resolved, 0.08f, 0, 0f);
                actionAnimUntil = Time.time + duration;
                currentRemoteAnim = resolved;
            }
        }

        public void RequestSprint() => RequestMovementAbility("sprint");

        public void RequestMovementAbility(string abilityId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return;
            if (IsServer)
                ApplyServerMovementAbility(abilityId);
            else if (IsOwner)
                RequestMovementAbilityServerRpc(abilityId ?? string.Empty);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestMovementAbilityServerRpc(string abilityId) => ApplyServerMovementAbility(abilityId);

        void ApplyServerMovementAbility(string abilityId)
        {
            if (!IsServer || Health.Value <= 0) return;
            string key = string.IsNullOrWhiteSpace(abilityId) ? string.Empty : abilityId.Trim().ToLowerInvariant();
            string cls = ClassName.Value.ToString();

            if (key == "sprint")
            {
                if (cls != "Rogue") return;
                int serverLevel = Mathf.Max(1, Level.Value);
                if (!TryGetAbilitySlotRequirement(key, out _, out int requiredLevel) || serverLevel < requiredLevel)
                {
                    RejectLockedAbilityOnServer(key, requiredLevel, serverLevel);
                    return;
                }
                if (IsMounted.Value) return;
                if (serverAbilityCooldowns.TryGetValue("move:sprint", out float readyAt) && Time.time < readyAt) return;
                serverAbilityCooldowns["move:sprint"] = Time.time + 10.0f;
                serverSprintUntil = Time.time + 5.0f;
                IsSprinting.Value = true;
            }
            else if (key == "arcane_ward")
            {
                ApplyServerAbilityHeal(40, "arcane_ward");
            }
        }

        void TickServerSprintState()
        {
            if (!IsServer) return;
            bool shouldSprint = Time.time <= serverSprintUntil && Health.Value > 0 && !IsMounted.Value;
            if (IsSprinting.Value != shouldSprint)
                IsSprinting.Value = shouldSprint;
        }

        public void RequestAbilityHeal(int amount, string abilityId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            string key = string.IsNullOrWhiteSpace(abilityId) ? string.Empty : abilityId.Trim().ToLowerInvariant();
            if (key == "sprint")
            {
                RequestMovementAbility("sprint");
                return;
            }
            if (amount <= 0 && key != "arcane_ward") return;
            if (IsServer) ApplyServerAbilityHeal(amount, abilityId);
            else RequestAbilityHealServerRpc(amount, abilityId);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestAbilityHealServerRpc(int amount, string abilityId) => ApplyServerAbilityHeal(amount, abilityId);

        void ApplyServerAbilityHeal(int amount, string abilityId)
        {
            if (!IsServer || Health.Value <= 0) return;
            string key = string.IsNullOrEmpty(abilityId) ? "" : abilityId.ToLowerInvariant();
            string cls = ClassName.Value.ToString();

            if (key == "sprint")
            {
                ApplyServerMovementAbility("sprint");
                return;
            }
            if (key == "holy_light")
            {
                if (cls != "Paladin") return;
                TryBeginServerTimedCast("holy_light", string.Empty);
                return;
            }

            int serverHeal = 0;
            if (key == "arcane_ward" && cls == "Mage") serverHeal = 40;
            else if (key == "second_wind" && cls == "Warrior") serverHeal = 45;
            else return;

            int serverLevel = Mathf.Max(1, Level.Value);
            if (!TryGetAbilitySlotRequirement(key, out _, out int requiredLevel) || serverLevel < requiredLevel)
            {
                RejectLockedAbilityOnServer(key, requiredLevel, serverLevel);
                return;
            }

            if (key != "arcane_ward" && Health.Value >= MaxHealth.Value) return;
            if (serverAbilityCooldowns.TryGetValue("heal:" + key, out float readyAt) && Time.time < readyAt) return;
            serverAbilityCooldowns["heal:" + key] = Time.time + 8.0f;
            if (key == "arcane_ward")
            {
                serverBlinkAllowanceMeters = 8.5f;
                serverBlinkExpiresAt = Time.time + 1.0f;
            }

            if (Health.Value < MaxHealth.Value)
                Health.Value = Mathf.Min(MaxHealth.Value, Health.Value + serverHeal);
        }

        public void SyncDefenseState(bool blocking, bool dodgeTriggered)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || !IsOwner) return;
            if (IsServer) ApplyServerDefenseState(blocking, dodgeTriggered);
            else SubmitDefenseStateServerRpc(blocking, dodgeTriggered);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void SubmitDefenseStateServerRpc(bool blocking, bool dodgeTriggered) => ApplyServerDefenseState(blocking, dodgeTriggered);

        void ApplyServerDefenseState(bool blocking, bool dodgeTriggered)
        {
            if (!IsServer || Health.Value <= 0) return;
            if (blocking)
            {
                if (!serverBlocking) serverBlockUntil = Time.time + 3.2f;
                serverBlocking = true;
            }
            else
            {
                serverBlocking = false;
            }

            if (dodgeTriggered && Time.time >= serverLastDodgeTime + 3.1f)
            {
                serverLastDodgeTime = Time.time;
                serverDodgeUntil = Time.time + 0.42f;
            }
        }

        public static RiftboundCoopPlayer ResolveCanonicalHostChannel(RiftboundCoopPlayer candidate = null)
        {
            if (NetworkManager.Singleton == null) return null;
            ulong serverId = NetworkManager.ServerClientId;
            if (LocalOwner && LocalOwner.IsSpawned && LocalOwner.IsServer && LocalOwner.OwnerClientId == serverId)
                return LocalOwner;
            var byServerId = FindByClientId(serverId);
            if (byServerId && byServerId.IsSpawned && byServerId.IsServer && byServerId.OwnerClientId == serverId)
                return byServerId;
            if (candidate && candidate.IsSpawned && candidate.IsServer && candidate.OwnerClientId == serverId)
                return candidate;
            return null;
        }

        public static bool IsAuthorizedEnemySenderForIds(ulong ownerClientId, ulong senderClientId, ulong serverClientId) =>
            ownerClientId == serverClientId && senderClientId == serverClientId;

        bool IsAuthorizedServerEnemySender(ulong senderClientId) =>
            NetworkManager.Singleton != null
            && IsAuthorizedEnemySenderForIds(OwnerClientId, senderClientId, NetworkManager.ServerClientId);

        public void BroadcastVisualEnemyProjectile(Vector3 origin, Vector3 direction)
        {
            if (RealmWorldChannel.Instance && RealmWorldChannel.Instance.IsSpawned && RealmWorldChannel.Instance.IsServer)
            {
                RealmWorldChannel.DispatchVisualEnemyProjectile(origin, direction);
                return;
            }
            var hostChannel = ResolveCanonicalHostChannel(this);
            if (!hostChannel) return;
            hostChannel.BroadcastVisualEnemyProjectileClientRpc(origin, direction);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        void BroadcastVisualEnemyProjectileClientRpc(Vector3 origin, Vector3 direction, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerEnemySender(rpcParams.Receive.SenderClientId)) return;
            EnemyProjectile.LaunchVisualOnly(origin, direction);
        }

        public void BroadcastEnemyState(EnemyController enemy)
        {
            if (!enemy) return;
            if (RealmWorldChannel.Instance && RealmWorldChannel.Instance.IsSpawned && RealmWorldChannel.Instance.IsServer)
            {
                RealmWorldChannel.DispatchEnemyState(enemy);
                return;
            }
            var hostChannel = ResolveCanonicalHostChannel(this);
            if (!hostChannel) return;
            var c = enemy.GetComponent<Combatant>();
            if (!c) return;
            hostChannel.BroadcastEnemyStateClientRpc(enemy.NetworkKey, c.Health, c.maxHealth, c.Alive, enemy.transform.position, enemy.transform.eulerAngles.y);
        }

        public void BroadcastEnemyAction(EnemyController enemy, byte actionKind, float duration, Vector3 origin, string spellName, string icon, uint actionSeq, float castDuration = -1f)
        {
            if (!enemy) return;
            if (RealmWorldChannel.Instance && RealmWorldChannel.Instance.IsSpawned && RealmWorldChannel.Instance.IsServer)
            {
                RealmWorldChannel.DispatchEnemyAction(enemy, actionKind, duration, origin, spellName, icon, actionSeq, castDuration);
                return;
            }
            var hostChannel = ResolveCanonicalHostChannel(this);
            if (!hostChannel) return;
            hostChannel.BroadcastEnemyActionClientRpc(enemy.NetworkKey ?? "", actionKind, Mathf.Clamp(duration, 0f, 3.5f), origin, spellName ?? "", icon ?? "", actionSeq, castDuration);
        }

        public bool AttemptSpoofedEnemyActionFromClientForTest(string enemyKey)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || IsServer) return false;
            bool actionRpcBlocked = false;
            bool snapRpcBlocked = false;
            try
            {
                BroadcastEnemyActionClientRpc(enemyKey, EnemyController.ActionHeavyWindup, 2.5f, transform.position, "Spoofed", "!", 999999u, 2.5f);
            }
            catch (RpcException)
            {
                actionRpcBlocked = true;
            }
            try
            {
                SendEnemyActionSnapshotClientRpc(enemyKey, EnemyController.ActionHeavyWindup, 2.5f, transform.position, "Spoofed", "!", 999999u, 2.5f, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
            catch (RpcException)
            {
                snapRpcBlocked = true;
            }
            return actionRpcBlocked && snapRpcBlocked;
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        void BroadcastEnemyActionClientRpc(string enemyKey, byte actionKind, float duration, Vector3 origin, string spellName, string icon, uint actionSeq, float castDuration, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerEnemySender(rpcParams.Receive.SenderClientId)) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedAction(actionSeq, actionKind, duration, origin, spellName, icon, castDuration);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        void BroadcastEnemyStateClientRpc(string enemyKey, float hp, float maxHp, bool alive, Vector3 pos, float yaw, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerEnemySender(rpcParams.Receive.SenderClientId)) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedState(hp, maxHp, alive, pos, yaw);
        }

        public static void DispatchEnemyHitToPlayer(ulong targetClientId, float damage, string enemyKey)
        {
            var avatar = FindByClientId(targetClientId);
            if (!avatar || !avatar.IsServer || avatar.Health.Value <= 0) return;

            if (Time.time < avatar.serverDodgeUntil)
            {
                avatar.ReceiveEnemyHitClientRpc(0f, enemyKey, true, avatar.RpcTarget.Single(targetClientId, RpcTargetUse.Temp));
                return;
            }

            float mitigated = Mathf.Max(1f, damage - avatar.GetServerArmorRating());
            if (avatar.IsServerBlocking) mitigated *= 0.5f;
            int dmgInt = Mathf.Max(1, Mathf.RoundToInt(mitigated));
            avatar.serverLastDamagedAt = Time.time;
            avatar.Health.Value = Mathf.Max(0, avatar.Health.Value - dmgInt);
            if (avatar.serverCastActive && !avatar.serverCastReleased)
                avatar.CancelActiveServerCast(avatar.Health.Value <= 0 ? ServerCastCancelReason.Died : ServerCastCancelReason.Damaged);
            if (avatar.Health.Value <= 0 && avatar.IsMounted.Value)
                avatar.IsMounted.Value = false;
            avatar.ReceiveEnemyHitClientRpc(dmgInt, enemyKey, false, avatar.RpcTarget.Single(targetClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveEnemyHitClientRpc(float damage, string enemyKey, bool dodged, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer || !localPlayer.Stats) return;
            localPlayer.Stats.ApplyAuthoritativeNetHit(damage, dodged);
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
            Vector3 baseSpawn = CoopWorldManager.GetRealmSpawnAnchor(Race.Value.ToString());
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

        static readonly Dictionary<string, float> ServerNodeCooldowns = new Dictionary<string, float>(StringComparer.Ordinal);

        List<string> BuildServerReagentEntries()
        {
            var entries = new List<string>(serverReagents.Count);
            foreach (var kv in serverReagents)
            {
                string clean = ProfessionSystem.NormalizeReagentName(kv.Key);
                if (!string.IsNullOrEmpty(clean) && kv.Value > 0)
                    entries.Add($"{clean}={Mathf.Clamp(kv.Value, 0, 9999)}");
            }
            return entries;
        }

        int GetServerProfessionSkill(CraftCategory category) => category switch
        {
            CraftCategory.Alchemy => serverAlchemySkill,
            CraftCategory.Blacksmithing => serverBlacksmithingSkill,
            CraftCategory.Engineering => serverEngineeringSkill,
            _ => 0
        };

        void SetServerProfessionSkill(CraftCategory category, int value)
        {
            int clamped = Mathf.Clamp(value, 0, ProfessionSystem.MaxSkill);
            switch (category)
            {
                case CraftCategory.Alchemy:
                    serverAlchemySkill = clamped;
                    break;
                case CraftCategory.Blacksmithing:
                    serverBlacksmithingSkill = clamped;
                    break;
                case CraftCategory.Engineering:
                    serverEngineeringSkill = clamped;
                    break;
            }
        }

        int GetServerReagentCount(string reagentName)
        {
            string clean = ProfessionSystem.NormalizeReagentName(reagentName);
            return !string.IsNullOrEmpty(clean) && serverReagents.TryGetValue(clean, out int count) ? count : 0;
        }

        public void RequestCraftRecipe(string recipeId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || !IsOwner || string.IsNullOrEmpty(recipeId)) return;
            if (IsServer)
            {
                if (ExecuteServerCraftRecipe(recipeId, out string outputItemId) && localPlayer)
                {
                    localPlayer.ApplyAuthorizedCraftRecipe(
                        recipeId.Trim().ToLowerInvariant(),
                        outputItemId,
                        serverPotions,
                        serverAlchemySkill,
                        serverBlacksmithingSkill,
                        serverEngineeringSkill,
                        ProfessionSystem.EncodeReagentsCsv(serverReagents));
                }
            }
            else
            {
                RequestCraftRecipeServerRpc(recipeId);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestCraftRecipeServerRpc(string recipeId)
        {
            if (ExecuteServerCraftRecipe(recipeId, out string outputItemId))
            {
                ApproveCraftRecipeClientRpc(
                    recipeId.Trim().ToLowerInvariant(),
                    outputItemId,
                    serverPotions,
                    serverAlchemySkill,
                    serverBlacksmithingSkill,
                    serverEngineeringSkill,
                    ProfessionSystem.EncodeReagentsCsv(serverReagents),
                    RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        bool ExecuteServerCraftRecipe(string recipeId, out string outputItemId)
        {
            outputItemId = "";
            if (!IsServer || Health.Value <= 0 || string.IsNullOrWhiteSpace(recipeId)) return false;
            string clean = recipeId.Trim().ToLowerInvariant();
            if (!ProfessionSystem.TryGetRecipeSpec(clean, out var recipe) || recipe == null) return false;

            int curSkill = GetServerProfessionSkill(recipe.category);
            if (curSkill < recipe.requiredSkill) return false;

            if (recipe.reagents != null)
            {
                for (int i = 0; i < recipe.reagents.Length; i++)
                {
                    var r = recipe.reagents[i];
                    if (GetServerReagentCount(r.matId) < r.count)
                        return false;
                }
                for (int i = 0; i < recipe.reagents.Length; i++)
                {
                    var r = recipe.reagents[i];
                    string cleanMat = ProfessionSystem.NormalizeReagentName(r.matId);
                    int nextCount = Mathf.Max(0, GetServerReagentCount(cleanMat) - r.count);
                    serverReagents[cleanMat] = nextCount;
                }
            }

            if (curSkill < ProfessionSystem.MaxSkill)
            {
                int gain = (recipe.requiredSkill + 15 >= curSkill) ? 2 : 1;
                SetServerProfessionSkill(recipe.category, Mathf.Min(ProfessionSystem.MaxSkill, curSkill + gain));
            }

            outputItemId = recipe.outputItemId ?? "";
            if (outputItemId == "potion")
            {
                serverPotions = Mathf.Min(10, serverPotions + Mathf.Max(1, recipe.outputCount));
            }
            else if (outputItemId == "copper-dagger" ||
                     outputItemId == "bronze-broadsword" ||
                     outputItemId == "iron-buckler" ||
                     outputItemId == "truesilver-blade")
            {
                GrantServerItem(outputItemId, equipIfBetter: false, persist: false);
            }

            PersistServerRealmCharacterIfInitialized();
            return true;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveCraftRecipeClientRpc(string recipeId, string outputItemId, int newPotions, int alchemySkill, int blacksmithingSkill, int engineeringSkill, string reagentsCsv, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.ApplyAuthorizedCraftRecipe(recipeId, outputItemId, newPotions, alchemySkill, blacksmithingSkill, engineeringSkill, reagentsCsv);
        }

        public void RequestGatherNode(string nodeName, Vector3 nodePos)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || !IsOwner || string.IsNullOrWhiteSpace(nodeName)) return;
            if (IsServer)
            {
                if (ExecuteServerGatherNode(nodeName, nodePos, out string reagentName, out int count, out int xpGranted, out int goldGranted, out Vector3 authoritativeNodePos) && localPlayer)
                {
                    localPlayer.ApplyAuthorizedGatherNode(
                        reagentName,
                        count,
                        Gold.Value,
                        Level.Value,
                        Experience.Value,
                        xpGranted,
                        goldGranted,
                        ProfessionSystem.EncodeReagentsCsv(serverReagents),
                        authoritativeNodePos);
                }
            }
            else
            {
                RequestGatherNodeServerRpc(nodeName, nodePos);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestGatherNodeServerRpc(string nodeName, Vector3 nodePos)
        {
            if (ExecuteServerGatherNode(nodeName, nodePos, out string reagentName, out int count, out int xpGranted, out int goldGranted, out Vector3 authoritativeNodePos))
            {
                ApproveGatherNodeClientRpc(
                    reagentName,
                    count,
                    Gold.Value,
                    Level.Value,
                    Experience.Value,
                    xpGranted,
                    goldGranted,
                    ProfessionSystem.EncodeReagentsCsv(serverReagents),
                    authoritativeNodePos,
                    RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        bool ExecuteServerGatherNode(string nodeName, Vector3 nodePos, out string reagentName, out int count, out int xpGranted, out int goldGranted, out Vector3 authoritativeNodePos)
        {
            reagentName = "";
            count = 0;
            xpGranted = 0;
            goldGranted = 0;
            authoritativeNodePos = nodePos;
            if (!IsServer || Health.Value <= 0 || string.IsNullOrWhiteSpace(nodeName)) return false;

            GatheringNode matchedNode = null;
            float bestDist = 3.5f;
            foreach (var node in FindObjectsByType<GatheringNode>(FindObjectsSortMode.None))
            {
                if (!node || !node.gameObject.activeInHierarchy) continue;
                if (!string.Equals(node.nodeName, nodeName.Trim(), StringComparison.OrdinalIgnoreCase)) continue;
                float dToHint = Vector3.Distance(node.transform.position, nodePos);
                if (dToHint <= bestDist && Vector3.Distance(transform.position, node.transform.position) <= 6.0f)
                {
                    bestDist = dToHint;
                    matchedNode = node;
                }
            }
            if (matchedNode == null) return false;
            authoritativeNodePos = matchedNode.transform.position;

            string nodeKey = $"{matchedNode.nodeName}@{matchedNode.transform.position.x:F1},{matchedNode.transform.position.z:F1}";
            if (ServerNodeCooldowns.TryGetValue(nodeKey, out float readyAt) && Time.time < readyAt)
                return false;
            ServerNodeCooldowns[nodeKey] = Time.time + Mathf.Max(5f, matchedNode.respawnDelay * 0.5f);

            reagentName = ProfessionSystem.NormalizeReagentName(matchedNode.nodeName);
            if (string.IsNullOrEmpty(reagentName)) return false;
            count = 1;
            int cur = GetServerReagentCount(reagentName);
            serverReagents[reagentName] = Mathf.Clamp(cur + count, 0, 9999);

            xpGranted = Mathf.Max(0, matchedNode.rewardXp);
            goldGranted = Mathf.Max(0, matchedNode.rewardGold);
            GrantAuthoritativeProgress(xpGranted, goldGranted);
            PersistServerRealmCharacterIfInitialized();
            return true;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveGatherNodeClientRpc(string reagentName, int count, int newGold, int newLevel, int newRemainderXp, int rewardXp, int rewardGold, string reagentsCsv, Vector3 nodePos, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.ApplyAuthorizedGatherNode(reagentName, count, newGold, newLevel, newRemainderXp, rewardXp, rewardGold, reagentsCsv, nodePos);
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
                PersistServerRealmCharacterIfInitialized();
                return true;
            }
            if (txId == "temper_weapon")
            {
                if (Gold.Value < 45 || serverTemperedWeapon) return false;
                serverTemperedWeapon = true;
                Gold.Value -= 45;
                PersistServerRealmCharacterIfInitialized();
                return true;
            }
            if (txId == "rest")
            {
                if (Gold.Value < 5 || Health.Value >= MaxHealth.Value) return false;
                Gold.Value -= 5;
                Health.Value = MaxHealth.Value;
                PersistServerRealmCharacterIfInitialized();
                return true;
            }
            if (txId == "sell_shards")
            {
                if (serverBoneShards <= 0) return false;
                Gold.Value += serverBoneShards * 2;
                serverBoneShards = 0;
                PersistServerRealmCharacterIfInitialized();
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
            PersistServerRealmCharacterIfInitialized();
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

                        int corpseGold = enemy.NetworkKey == "boss-abbot-vael" ? 45
                            : enemy.NetworkKey == "boss-malakar" ? 35
                            : enemy.NetworkKey == "vermithrax-boss" ? 85
                            : (enemy.isBoss ? 20 : enemy.campGuard ? 5 : 3);
                        int corpseShards = enemy.NetworkKey == "boss-abbot-vael" ? 8
                            : enemy.NetworkKey == "boss-malakar" ? 6
                            : enemy.NetworkKey == "vermithrax-boss" ? 25
                            : (enemy.isBoss ? 5 : enemy.campGuard ? 2 : 1);
                        var drops = SharedRewardCatalog.GetEnemyDropItemIds(enemy.NetworkKey);
                        var validDrops = new List<string>();
                        if (drops != null)
                        {
                            for (int i = 0; i < drops.Count; i++)
                            {
                                string d = drops[i];
                                if (!string.IsNullOrEmpty(d) && ItemCatalog.Get(d) != null && !validDrops.Contains(d))
                                    validDrops.Add(d);
                            }
                        }
                        cp.serverPendingCorpseLoot[enemy.NetworkKey] = new ServerPendingCorpse
                        {
                            enemyKey = enemy.NetworkKey,
                            position = enemy.transform.position,
                            gold = corpseGold,
                            boneShards = corpseShards,
                            items = validDrops
                        };
                        cp.PersistServerRealmCharacterIfInitialized();
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
            bool leveled = false;
            while (Experience.Value >= xpNeeded)
            {
                Experience.Value -= xpNeeded;
                Level.Value++;
                leveled = true;
                xpNeeded = Level.Value * 100;
            }
            if (leveled)
                RecalculateServerVitals(preserveCurrentHp: false);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveEnemyDefeatedClientRpc(string enemyKey, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy)
            {
                localPlayer.RecordEnemyDefeat(enemy);
                if (!IsServer &&
                    (enemy.enemyId == "boss-abbot-vael" || enemy.enemyId == "boss-malakar" || enemy.enemyId == "vermithrax-boss") &&
                    (LootDropSystem.Instance == null || LootDropSystem.Instance.FindCorpseByEnemyKey(enemy.NetworkKey) == null))
                {
                    int g = enemy.enemyId == "boss-abbot-vael" ? 45 : enemy.enemyId == "boss-malakar" ? 35 : 85;
                    int s = enemy.enemyId == "boss-abbot-vael" ? 8 : enemy.enemyId == "boss-malakar" ? 6 : 25;
                    string title = enemy.enemyId == "boss-abbot-vael" ? "Dread Abbot Vael"
                        : enemy.enemyId == "boss-malakar" ? "High Inquisitor Malakar"
                        : "Vermithrax the Ember Sovereign";
                    LootDropSystem.SpawnLootItems(enemy.transform.position, title, g, s, SharedRewardCatalog.GetEnemyDropItemIds(enemy.enemyId), enemy.NetworkKey);
                }
            }
        }

        public void RequestLootCorpse(string enemyKey, string itemId, bool lootAll, bool lootCoins)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || !IsOwner || string.IsNullOrEmpty(enemyKey)) return;
            if (IsServer)
            {
                if (ExecuteServerLootCorpse(enemyKey, itemId, lootAll, lootCoins, out string[] granted, out int goldDelta, out int shardDelta, out bool cleared))
                    LootDropSystem.Instance?.ApplyAuthorizedLootCorpse(enemyKey, granted, goldDelta, shardDelta, Gold.Value, serverBoneShards, cleared);
            }
            else
            {
                RequestLootCorpseServerRpc(enemyKey, itemId ?? "", lootAll, lootCoins);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestLootCorpseServerRpc(string enemyKey, string itemId, bool lootAll, bool lootCoins)
        {
            if (ExecuteServerLootCorpse(enemyKey, itemId, lootAll, lootCoins, out string[] granted, out int goldDelta, out int shardDelta, out bool cleared))
            {
                string csv = granted != null && granted.Length > 0 ? string.Join(",", granted) : "";
                ApproveLootCorpseClientRpc(enemyKey, csv, goldDelta, shardDelta, Gold.Value, serverBoneShards, cleared, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        bool ExecuteServerLootCorpse(string enemyKey, string itemId, bool lootAll, bool lootCoins, out string[] grantedItems, out int goldDelta, out int shardDelta, out bool corpseCleared)
        {
            grantedItems = Array.Empty<string>();
            goldDelta = 0;
            shardDelta = 0;
            corpseCleared = false;
            if (!IsServer || Health.Value <= 0 || string.IsNullOrEmpty(enemyKey)) return false;
            if (!serverPendingCorpseLoot.TryGetValue(enemyKey, out var pending)) return false;
            if (Vector3.Distance(transform.position, pending.position) > 6.5f) return false;

            if ((lootAll || lootCoins) && pending.gold > 0)
            {
                int g = Race.Value.ToString() == "Human" ? Mathf.RoundToInt(pending.gold * 1.25f) : pending.gold;
                Gold.Value += g;
                goldDelta = g;
                pending.gold = 0;
            }
            if (lootAll && pending.boneShards > 0)
            {
                shardDelta = pending.boneShards;
                serverBoneShards += pending.boneShards;
                pending.boneShards = 0;
            }

            var grantedList = new List<string>();
            if (lootAll)
            {
                for (int i = 0; i < pending.items.Count; i++)
                {
                    string id = pending.items[i];
                    if (ItemCatalog.Get(id) != null)
                    {
                        GrantServerItem(id, equipIfBetter: false, persist: false);
                        grantedList.Add(id);
                    }
                }
                pending.items.Clear();
            }
            else if (!string.IsNullOrWhiteSpace(itemId))
            {
                string clean = itemId.Trim();
                if (pending.items.Remove(clean) && ItemCatalog.Get(clean) != null)
                {
                    GrantServerItem(clean, equipIfBetter: false, persist: false);
                    grantedList.Add(clean);
                }
            }

            corpseCleared = pending.gold <= 0 && pending.boneShards <= 0 && pending.items.Count == 0;
            if (corpseCleared)
                serverPendingCorpseLoot.Remove(enemyKey);

            grantedItems = grantedList.ToArray();
            PersistServerRealmCharacterIfInitialized();
            return goldDelta > 0 || shardDelta > 0 || grantedItems.Length > 0 || corpseCleared;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveLootCorpseClientRpc(string enemyKey, string grantedCsv, int goldDelta, int shardDelta, int newGold, int newBoneShards, bool corpseCleared, RpcParams rpcParams = default)
        {
            if (!IsOwner) return;
            string[] granted = string.IsNullOrEmpty(grantedCsv) ? Array.Empty<string>() : grantedCsv.Split(',');
            LootDropSystem.Instance?.ApplyAuthorizedLootCorpse(enemyKey, granted, goldDelta, shardDelta, newGold, newBoneShards, corpseCleared);
        }

        public bool RequestOpenChestServerValidated(LootChest chest)
        {
            if (!chest || string.IsNullOrEmpty(chest.chestId)) return false;
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return true;

            if (IsServer)
            {
                if (!ValidateChestClaimOnServer(chest, out string[] grantedItems)) return false;
                if (IsOwner)
                {
                    if (!localPlayer) localPlayer = FindFirstObjectByType<PlayerController>();
                    if (localPlayer)
                    {
                        localPlayer.ApplyAuthorizedChestOpen(chest, grantedItems, Gold.Value, Level.Value, Experience.Value, serverPotions, fromServer: true);
                        chest.Refresh();
                    }
                }
                return true;
            }

            RequestOpenChestServerRpc(chest.chestId);
            return false;
        }

        static readonly HashSet<string> ServerClaimedWorldBossKills = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        static readonly HashSet<string> ServerClaimedWorldBossChests = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetWorldBossServerClaims()
        {
            ServerClaimedWorldBossKills.Clear();
            ServerClaimedWorldBossChests.Clear();
            ServerNodeCooldowns.Clear();
        }

        public static void ResetServerWorldBossClaimsForTest()
        {
            ServerClaimedWorldBossKills.Clear();
            ServerClaimedWorldBossChests.Clear();
            ServerNodeCooldowns.Clear();
        }

        string ResolveCharacterRewardKey()
        {
            string n = PlayerName.Value.ToString();
            return !string.IsNullOrWhiteSpace(n) ? n.Trim() : $"client:{OwnerClientId}";
        }

        string ResolveWorldBossClaimKey()
        {
            string acct = ResolveServerAccountId().Trim().ToLowerInvariant();
            string ch = ResolveCharacterRewardKey().Trim().ToLowerInvariant();
            return $"{acct}__{ch}";
        }

        public string ResolveNextGorgonaxKillItem()
        {
            return SharedRewardCatalog.ResolveGorgonaxKillItem(id => serverOwnedItems.Contains(id));
        }

        static int ServerGearValue(GearItem item) => item.damage * 10 + item.armor * 10 + item.health;

        bool GrantServerItem(string itemId, bool equipIfBetter = false, bool persist = true)
        {
            if (!IsServer || string.IsNullOrWhiteSpace(itemId)) return false;
            string clean = itemId.Trim();
            var item = ItemCatalog.Get(clean);
            if (item == null) return false;

            bool added = !serverOwnedItems.Contains(clean);
            if (added) serverOwnedItems.Add(clean);

            if (equipIfBetter)
            {
                string curId = item.slot == GearSlot.Weapon ? EquippedWeaponId.Value.ToString()
                    : item.slot == GearSlot.Armor ? EquippedArmorId.Value.ToString()
                    : EquippedTrinketId.Value.ToString();
                var curItem = ItemCatalog.Get(curId);
                if (curItem == null || ServerGearValue(item) > ServerGearValue(curItem))
                {
                    if (item.slot == GearSlot.Weapon) EquippedWeaponId.Value = new FixedString64Bytes(clean);
                    else if (item.slot == GearSlot.Armor) EquippedArmorId.Value = new FixedString64Bytes(clean);
                    else EquippedTrinketId.Value = new FixedString64Bytes(clean);
                    RecalculateServerVitals(preserveCurrentHp: true);
                }
            }

            if (persist)
                PersistServerRealmCharacterIfInitialized();
            return added;
        }

        public bool GrantServerWorldBossKillReward(int xp, int gold, string itemId, string title)
        {
            if (!IsServer || !IsSpawned || Health.Value <= 0) return false;
            string charKey = ResolveCharacterRewardKey();
            string claimKey = ResolveWorldBossClaimKey();
            if (ServerClaimedWorldBossKills.Contains(claimKey)) return false;
            if (RealmPersistenceService.IsInitialized)
            {
                if (!RealmPersistenceService.TryClaimBossKillOnce(ResolveServerAccountId(), charKey, "worldboss-gorgonax", Mathf.Max(0, gold), Mathf.Max(0, xp)))
                    return false;
            }
            ServerClaimedWorldBossKills.Add(claimKey);

            string resolvedItem = !string.IsNullOrEmpty(itemId) && !serverOwnedItems.Contains(itemId)
                ? itemId
                : ResolveNextGorgonaxKillItem();
            if (!string.IsNullOrEmpty(resolvedItem))
                GrantServerItem(resolvedItem, equipIfBetter: false, persist: false);
            if (!string.IsNullOrWhiteSpace(title))
                serverActiveTitle = title.Trim();

            GrantAuthoritativeProgress(Mathf.Max(0, xp), Mathf.Max(0, gold));
            PersistServerRealmCharacterIfInitialized();
            if (IsOwner && localPlayer)
                ApplyAuthorizedWorldBossKillReward(resolvedItem, title);
            else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                ReceiveWorldBossKillRewardClientRpc(resolvedItem ?? string.Empty, title ?? string.Empty, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            return true;
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void ReceiveWorldBossKillRewardClientRpc(string itemId, string title, RpcParams rpcParams = default)
        {
            if (!IsOwner) return;
            ApplyAuthorizedWorldBossKillReward(itemId, title);
        }

        void ApplyAuthorizedWorldBossKillReward(string itemId, string title)
        {
            if (!localPlayer) localPlayer = FindFirstObjectByType<PlayerController>();
            if (!localPlayer) return;
            if (!string.IsNullOrEmpty(itemId))
            {
                localPlayer.GrantItem(itemId);
                var item = ItemCatalog.Get(itemId);
                if (item != null)
                {
                    ChatSystem.LogLoot($"World Boss Loot: Received [{item.name}] ({item.rarity} {item.slot})!");
                    CombatTextManager.Spawn(localPlayer.transform.position + Vector3.up * 3.0f, $"Looted: {item.name}!", item.color, true);
                }
            }
            if (!string.IsNullOrEmpty(title))
            {
                PlayerTitleSystem.SetActiveTitle(title);
                ChatSystem.LogSystem($"<color=#e0b034>★ Title Unlocked: '{title}'!</color>");
            }
        }

        bool ValidateChestClaimOnServer(LootChest chest, out string[] grantedItems)
        {
            grantedItems = Array.Empty<string>();
            if (Health.Value <= 0 || !chest || string.IsNullOrEmpty(chest.chestId)) return false;
            if (Vector3.Distance(transform.position, chest.transform.position) > 4.5f) return false;
            if (chest.chestId == RiverReaverCamp.CacheId && !serverDefeatedEnemyKeys.Contains(RiverReaverCamp.CaptainId)) return false;
            if (chest.chestId == PinewatchWoodsZone.BossChestId && !serverDefeatedEnemyKeys.Contains(PinewatchWoodsZone.BossId)) return false;
            if (chest.chestId == AshenmawCavernZone.BossChestId && !serverDefeatedEnemyKeys.Contains(AshenmawCavernZone.BossId)) return false;
            if (chest.chestId == SunstoneAbbeyZone.BossChestId && !serverDefeatedEnemyKeys.Contains(SunstoneAbbeyZone.BossId)) return false;
            if (chest.chestId == IronmawQuarryZone.BossChestId && !serverDefeatedEnemyKeys.Contains(IronmawQuarryZone.BossId)) return false;
            if (chest.chestId == WorldBossEncounter.HoardChestId && !WorldBossEncounter.IsBossDefeatedAuthoritative) return false;
            if (chest.requiresCaptainDefeated && !serverDefeatedEnemyKeys.Contains("ashwood-captain")) return false;
            if (chest.requiresWardenDefeated && !serverDefeatedEnemyKeys.Contains("crypt-warden")) return false;

            string charKey = ResolveCharacterRewardKey();
            string claimKey = ResolveWorldBossClaimKey();
            if (serverClaimedChests.Contains(chest.chestId)) return false;
            if (chest.chestId == WorldBossEncounter.HoardChestId && ServerClaimedWorldBossChests.Contains(claimKey))
                return false;
            if (CoopWorldManager.Instance != null && CoopWorldManager.Instance.HasClaimedChestForClient(OwnerClientId, chest.chestId))
                return false;

            if (RealmPersistenceService.IsInitialized)
            {
                if (!RealmPersistenceService.TryClaimChestOnce(ResolveServerAccountId(), charKey, chest.chestId, chest.gold, chest.experience))
                    return false;
            }

            if (CoopWorldManager.Instance != null)
                CoopWorldManager.Instance.TryClaimChestForClient(OwnerClientId, chest.chestId);
            if (chest.chestId == WorldBossEncounter.HoardChestId)
                ServerClaimedWorldBossChests.Add(claimKey);
            serverClaimedChests.Add(chest.chestId);

            IReadOnlyList<string> toGrant = chest.chestId == WorldBossEncounter.HoardChestId
                ? (IReadOnlyList<string>)SharedRewardCatalog.ResolveGorgonaxChestItems(id => serverOwnedItems.Contains(id))
                : SharedRewardCatalog.GetChestItemIds(chest.chestId);

            var grantedList = new List<string>();
            if (toGrant != null)
            {
                for (int i = 0; i < toGrant.Count; i++)
                {
                    string id = toGrant[i];
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    string clean = id.Trim();
                    if (ItemCatalog.Get(clean) != null)
                    {
                        GrantServerItem(clean, equipIfBetter: false, persist: false);
                        grantedList.Add(clean);
                    }
                }
            }
            grantedItems = grantedList.ToArray();

            serverPotions = Mathf.Min(10, serverPotions + 1);
            GrantAuthoritativeProgress(chest.experience, chest.gold);
            PersistServerRealmCharacterIfInitialized();
            return true;
        }

        string serverAssignedAccountId;

        public void SetServerAccountIdForSession(string accountId)
        {
            if (!IsServer) return;
            serverAssignedAccountId = string.IsNullOrWhiteSpace(accountId) ? "" : accountId.Trim();
            serverIdentityResolved = true;
            serverHydrated = false;
            HydrateServerRealmCharacterIfInitialized();
        }

        string ResolveServerAccountId()
        {
            if (!string.IsNullOrWhiteSpace(serverAssignedAccountId)) return serverAssignedAccountId;
            return $"realm_c{OwnerClientId}";
        }

        void HydrateServerRealmCharacterIfInitialized()
        {
            if (!IsServer || !RealmPersistenceService.IsInitialized) return;
            string charKey = ResolveCharacterRewardKey();
            string accountId = ResolveServerAccountId();
            var save = RealmPersistenceService.LoadOrCreateCharacter(accountId, charKey, Race.Value.ToString(), ClassName.Value.ToString(), serverFaction);

            if (!string.IsNullOrWhiteSpace(save.faction)) serverFaction = save.faction.Trim();
            serverActiveTitle = save.activeTitle ?? "";

            serverOwnedItems.Clear();
            serverOwnedItems.Add("starter-axe");
            if (save.ownedItems != null)
            {
                for (int i = 0; i < save.ownedItems.Count; i++)
                {
                    string id = save.ownedItems[i];
                    if (!string.IsNullOrWhiteSpace(id) && ItemCatalog.Get(id.Trim()) != null && !serverOwnedItems.Contains(id.Trim()))
                        serverOwnedItems.Add(id.Trim());
                }
            }

            string wId = !string.IsNullOrWhiteSpace(save.equippedWeaponId) && serverOwnedItems.Contains(save.equippedWeaponId) && ItemCatalog.Get(save.equippedWeaponId)?.slot == GearSlot.Weapon
                ? save.equippedWeaponId : "starter-axe";
            string aId = !string.IsNullOrWhiteSpace(save.equippedArmorId) && serverOwnedItems.Contains(save.equippedArmorId) && ItemCatalog.Get(save.equippedArmorId)?.slot == GearSlot.Armor
                ? save.equippedArmorId : "";
            string tId = !string.IsNullOrWhiteSpace(save.equippedTrinketId) && serverOwnedItems.Contains(save.equippedTrinketId) && ItemCatalog.Get(save.equippedTrinketId)?.slot == GearSlot.Trinket
                ? save.equippedTrinketId : "";

            EquippedWeaponId.Value = new FixedString64Bytes(wId);
            EquippedArmorId.Value = new FixedString64Bytes(aId);
            EquippedTrinketId.Value = new FixedString64Bytes(tId);

            Level.Value = Mathf.Max(1, save.level);
            Experience.Value = Mathf.Max(0, save.experience);
            Gold.Value = Mathf.Max(0, save.gold);
            serverPotions = Mathf.Clamp(save.potions, 0, 10);
            serverTemperedWeapon = save.temperedWeapon;
            serverBoneShards = Mathf.Max(0, save.boneShards);
            serverAlchemySkill = Mathf.Clamp(save.alchemySkill, 0, ProfessionSystem.MaxSkill);
            serverBlacksmithingSkill = Mathf.Clamp(save.blacksmithingSkill, 0, ProfessionSystem.MaxSkill);
            serverEngineeringSkill = Mathf.Clamp(save.engineeringSkill, 0, ProfessionSystem.MaxSkill);
            serverReagents.Clear();
            if (save.reagents != null)
            {
                for (int i = 0; i < save.reagents.Count; i++)
                {
                    if (ProfessionSystem.TryParseReagentEntry(save.reagents[i], out string rName, out int rCount) && rCount > 0)
                        serverReagents[rName] = rCount;
                }
            }
            QuestIndex.Value = Mathf.Clamp(save.questIndex, 0, 3);
            QuestAccepted.Value = QuestIndex.Value < 3 && save.questAccepted;
            QuestKills.Value = Mathf.Max(0, save.questKills);

            serverClaimedChests.Clear();
            if (save.claimedChests != null)
            {
                for (int i = 0; i < save.claimedChests.Count; i++)
                    if (!string.IsNullOrWhiteSpace(save.claimedChests[i])) serverClaimedChests.Add(save.claimedChests[i].Trim());
            }

            serverDiscoveredLandmarks.Clear();
            if (save.discoveredLandmarks != null)
            {
                for (int i = 0; i < save.discoveredLandmarks.Count; i++)
                    if (!string.IsNullOrWhiteSpace(save.discoveredLandmarks[i])) serverDiscoveredLandmarks.Add(save.discoveredLandmarks[i].Trim());
            }

            serverDefeatedEnemyKeys.Clear();
            if (save.defeatedEnemies != null)
            {
                for (int i = 0; i < save.defeatedEnemies.Count; i++)
                    if (!string.IsNullOrWhiteSpace(save.defeatedEnemies[i])) serverDefeatedEnemyKeys.Add(save.defeatedEnemies[i].Trim());
            }

            serverCompletedFactionQuests.Clear();
            if (save.claimedQuests != null)
            {
                for (int i = 0; i < save.claimedQuests.Count; i++)
                {
                    string q = save.claimedQuests[i];
                    if (string.IsNullOrWhiteSpace(q)) continue;
                    string cleanQ = q.Trim();
                    if (cleanQ.StartsWith("faction_quest_", StringComparison.Ordinal))
                        serverCompletedFactionQuests.Add(cleanQ.Substring("faction_quest_".Length));
                }
            }

            RecalculateServerVitals(preserveCurrentHp: false);
            if (save.health > 0)
                Health.Value = Mathf.Clamp(save.health, 1, MaxHealth.Value);

            string claimKey = ResolveWorldBossClaimKey();
            if (save.claimedBossKills != null && save.claimedBossKills.Contains("worldboss-gorgonax"))
                ServerClaimedWorldBossKills.Add(claimKey);
            if (serverClaimedChests.Contains(WorldBossEncounter.HoardChestId))
                ServerClaimedWorldBossChests.Add(claimKey);

            serverHydrated = true;
        }

        void PersistServerRealmCharacterIfInitialized()
        {
            if (!IsServer || !RealmPersistenceService.IsInitialized) return;
            string charKey = ResolveCharacterRewardKey();
            var save = RealmPersistenceService.LoadOrCreateCharacter(ResolveServerAccountId(), charKey, Race.Value.ToString(), ClassName.Value.ToString(), serverFaction);
            save.race = Race.Value.ToString();
            save.className = ClassName.Value.ToString();
            save.faction = serverFaction;
            save.level = Mathf.Max(1, Level.Value);
            save.experience = Mathf.Max(0, Experience.Value);
            save.gold = Mathf.Max(0, Gold.Value);
            save.maxHealth = Mathf.Max(100, MaxHealth.Value);
            save.health = Mathf.Clamp(Health.Value, 0, save.maxHealth);
            save.posX = transform.position.x;
            save.posY = transform.position.y;
            save.posZ = transform.position.z;

            save.ownedItems = new List<string>(serverOwnedItems);
            if (!save.ownedItems.Contains("starter-axe")) save.ownedItems.Insert(0, "starter-axe");
            save.equippedWeaponId = EquippedWeaponId.Value.ToString();
            save.equippedArmorId = EquippedArmorId.Value.ToString();
            save.equippedTrinketId = EquippedTrinketId.Value.ToString();
            save.potions = Mathf.Clamp(serverPotions, 0, 10);
            save.temperedWeapon = serverTemperedWeapon;
            save.boneShards = Mathf.Max(0, serverBoneShards);
            save.alchemySkill = Mathf.Clamp(serverAlchemySkill, 0, ProfessionSystem.MaxSkill);
            save.blacksmithingSkill = Mathf.Clamp(serverBlacksmithingSkill, 0, ProfessionSystem.MaxSkill);
            save.engineeringSkill = Mathf.Clamp(serverEngineeringSkill, 0, ProfessionSystem.MaxSkill);
            save.professionInitialized = true;
            save.reagents = BuildServerReagentEntries();
            save.questIndex = Mathf.Clamp(QuestIndex.Value, 0, 3);
            save.questAccepted = save.questIndex < 3 && QuestAccepted.Value;
            save.questKills = Mathf.Max(0, QuestKills.Value);
            save.activeTitle = serverActiveTitle ?? "";
            save.discoveredLandmarks = new List<string>(serverDiscoveredLandmarks);
            save.defeatedEnemies = new List<string>(serverDefeatedEnemyKeys);

            save.claimedChests ??= new List<string>();
            foreach (var c in serverClaimedChests)
                if (!string.IsNullOrEmpty(c) && !save.claimedChests.Contains(c)) save.claimedChests.Add(c);

            save.claimedQuests ??= new List<string>();
            foreach (var fq in serverCompletedFactionQuests)
            {
                string fullId = $"faction_quest_{fq}";
                if (!string.IsNullOrEmpty(fq) && !save.claimedQuests.Contains(fullId) && !save.claimedQuests.Contains(fq))
                    save.claimedQuests.Add(fullId);
            }

            RealmPersistenceService.SaveCharacter(save);
        }

        public bool LastStaffCommandAllowed { get; private set; }
        public string LastStaffCommandMessage { get; private set; } = "";

        public void RequestStaffAction(RealmStaffAction action, string target = "", string detail = "")
        {
            if (!IsOwner || !IsSpawned) return;
            RequestStaffActionServerRpc((byte)action, target ?? "", detail ?? "");
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestStaffActionServerRpc(byte actionByte, string target, string detail)
        {
            var action = (RealmStaffAction)actionByte;
            string actor = ResolveServerAccountId();
            bool allowed = RealmPersistenceService.TryAuthorizeStaffAction(actor, action, target, detail, out string reason);
            if (allowed && action == RealmStaffAction.ResetBoss && WorldBossEncounter.Instance != null)
            {
                WorldBossEncounter.Instance.ResetEncounterState();
                reason = "Gorgonax encounter reset by staff.";
            }
            ReceiveStaffActionResultClientRpc(allowed, reason ?? "", RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void ReceiveStaffActionResultClientRpc(bool allowed, string message, RpcParams rpcParams = default)
        {
            if (!IsOwner) return;
            LastStaffCommandAllowed = allowed;
            LastStaffCommandMessage = message ?? "";
            ChatSystem.LogSystem(allowed ? $"<color=#6ee7b7>[Staff] {message}</color>" : $"<color=#f87171>[Staff] {message}</color>");
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestOpenChestServerRpc(string chestId)
        {
            LootChest targetChest = null;
            foreach (var c in FindObjectsByType<LootChest>(FindObjectsSortMode.None))
                if (c && c.chestId == chestId) { targetChest = c; break; }

            if (ValidateChestClaimOnServer(targetChest, out string[] grantedItems))
            {
                string csv = grantedItems != null && grantedItems.Length > 0 ? string.Join(",", grantedItems) : "";
                ApproveOpenChestClientRpc(chestId, csv, Gold.Value, Level.Value, Experience.Value, serverPotions, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveOpenChestClientRpc(string chestId, string grantedCsv, int newGold, int newLevel, int newRemainderXp, int newPotions, RpcParams rpcParams = default)
        {
            if (!IsOwner) return;
            if (!localPlayer) localPlayer = FindFirstObjectByType<PlayerController>();
            if (!localPlayer) return;
            string[] granted = string.IsNullOrEmpty(grantedCsv) ? Array.Empty<string>() : grantedCsv.Split(',');
            foreach (var c in FindObjectsByType<LootChest>(FindObjectsSortMode.None))
            {
                if (c && c.chestId == chestId)
                {
                    localPlayer.ApplyAuthorizedChestOpen(c, granted, newGold, newLevel, newRemainderXp, newPotions, fromServer: true);
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
            PersistServerRealmCharacterIfInitialized();
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
            {
                if (!ValidateCompleteQuestOnServer(questIndex)) return false;
                if (IsOwner)
                {
                    if (!localPlayer) localPlayer = FindFirstObjectByType<PlayerController>();
                    localPlayer?.ApplyAuthorizedCompleteQuest(questIndex, Gold.Value, Level.Value, Experience.Value, serverPotions, fromServer: true);
                }
                return true;
            }

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

            int baseGold = questIndex == 0 ? 25 : (questIndex == 1 ? 35 : 60);
            int gold = Race.Value.ToString() == "Human" ? Mathf.RoundToInt(baseGold * 1.25f) : baseGold;
            int xp = questIndex == 0 ? 80 : (questIndex == 1 ? 100 : 180);

            if (CoopWorldManager.Instance != null && CoopWorldManager.Instance.HasClaimedQuestForClient(OwnerClientId, questIndex))
                return false;

            if (RealmPersistenceService.IsInitialized)
            {
                if (!RealmPersistenceService.TryClaimQuestOnce(ResolveServerAccountId(), ResolveCharacterRewardKey(), $"main_quest_{questIndex}", gold, xp))
                    return false;
            }

            if (CoopWorldManager.Instance != null)
                CoopWorldManager.Instance.TryClaimQuestForClient(OwnerClientId, questIndex);

            GrantAuthoritativeProgress(xp, gold);
            serverPotions = Mathf.Min(10, serverPotions + 1);
            QuestIndex.Value++;
            QuestAccepted.Value = false;
            QuestKills.Value = 0;
            if (questIndex == 2)
            {
                GrantServerItem("emberwatch-axe", equipIfBetter: true, persist: false);
                GrantServerItem("emberwatch-sigil", equipIfBetter: true, persist: false);
                RecalculateServerVitals(preserveCurrentHp: false);
            }
            Health.Value = MaxHealth.Value;
            PersistServerRealmCharacterIfInitialized();
            return true;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestCompleteQuestServerRpc(int questIndex)
        {
            if (ValidateCompleteQuestOnServer(questIndex))
            {
                ApproveCompleteQuestClientRpc(questIndex, Gold.Value, Level.Value, Experience.Value, serverPotions, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveCompleteQuestClientRpc(int questIndex, int newGold, int newLevel, int newRemainderXp, int newPotions, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer) return;
            localPlayer.ApplyAuthorizedCompleteQuest(questIndex, newGold, newLevel, newRemainderXp, newPotions, fromServer: true);
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
                bool exactBoss = q.targetId == RiverReaverCamp.CaptainId
                    || q.targetId == "redridge-outcast-boss"
                    || q.targetId == "vermithrax-boss"
                    || q.targetId == PinewatchWoodsZone.BossId
                    || q.targetId == AshenmawCavernZone.BossId
                    || q.targetId == SunstoneAbbeyZone.BossId
                    || q.targetId == IronmawQuarryZone.BossId;
                if (exactBoss ? !serverDefeatedEnemyKeys.Contains(q.targetId) : serverDefeatedEnemyKeys.Count < q.requiredCount)
                    return false;
            }

            goldGain = Race.Value.ToString() == "Human" ? Mathf.RoundToInt(q.rewardGold * 1.25f) : q.rewardGold;
            xpGain = q.rewardXP;

            if (RealmPersistenceService.IsInitialized)
            {
                if (!RealmPersistenceService.TryClaimQuestOnce(ResolveServerAccountId(), ResolveCharacterRewardKey(), $"faction_quest_{questId}", goldGain, xpGain))
                    return false;
            }

            if (!serverCompletedFactionQuests.Add(questId)) return false;

            if (!string.IsNullOrEmpty(q.rewardItem))
            {
                GrantServerItem(q.rewardItem, equipIfBetter: true, persist: false);
                RecalculateServerVitals(preserveCurrentHp: false);
            }

            GrantAuthoritativeProgress(xpGain, goldGain);
            Health.Value = MaxHealth.Value;
            PersistServerRealmCharacterIfInitialized();
            return true;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ApproveCompleteFactionQuestClientRpc(string questId, int newServerGold, int goldGain, int xpGain, RpcParams rpcParams = default)
        {
            if (!IsOwner || !localPlayer || !FactionQuestSystem.Instance) return;
            FactionQuestSystem.Instance.ApplyAuthorizedComplete(localPlayer, questId, newServerGold, goldGain, xpGain);
        }

        // ── O-01 Shared World Events & Late-Join Snapshot ───────────────────────────────

        internal static uint ActiveReadyCheckSeq => activeGlobalReadyCheckSeq;
        internal void SetServerReadyCheckSeqForChannel(uint checkId) => serverReadyCheckSeq = checkId;

        public void SendWorldSnapshotToClient(ulong clientId)
        {
            if (RealmWorldChannel.Instance && RealmWorldChannel.Instance.IsSpawned && RealmWorldChannel.Instance.IsServer)
            {
                RealmWorldChannel.DispatchWorldSnapshotToClient(clientId);
                return;
            }
            var hostChannel = ResolveCanonicalHostChannel(this);
            if (!hostChannel) return;
            var rpcParams = hostChannel.RpcTarget.Single(clientId, RpcTargetUse.Temp);
            foreach (var kv in EnemyController.AllByKey)
            {
                var enemy = kv.Value;
                if (!enemy || enemy.trainingDummy) continue;
                var c = enemy.GetComponent<Combatant>();
                if (!c) continue;
                bool hasAction = enemy.TryGetActiveActionSnapshot(out byte ak, out float rem, out float castRem, out Vector3 orig, out string sp, out string ic, out uint aseq);
                if (!c.Alive || c.Health < c.maxHealth || !enemy.gameObject.activeInHierarchy || hasAction)
                {
                    hostChannel.SendEnemySnapshotClientRpc(enemy.NetworkKey, c.Health, c.maxHealth, c.Alive, enemy.transform.position, enemy.transform.eulerAngles.y, rpcParams);
                    if (hasAction)
                        hostChannel.SendEnemyActionSnapshotClientRpc(enemy.NetworkKey, ak, rem, orig, sp, ic, aseq, castRem, rpcParams);
                }
            }
            if (CoopWorldManager.Instance != null && CoopWorldManager.Instance.TryGetActiveReadyCheck(out string rcInitiator))
            {
                var joiningPeer = FindByClientId(clientId);
                if (joiningPeer) joiningPeer.ReadyCheckId.Value = activeGlobalReadyCheckSeq;
                hostChannel.SendReadyCheckSnapshotClientRpc(rcInitiator, activeGlobalReadyCheckSeq, rpcParams);
            }
            if (LivingWorldEvents.Instance != null)
            {
                LivingWorldEvents.Instance.SendEventSnapshotToClient(hostChannel, rpcParams);
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void SendReadyCheckSnapshotClientRpc(string initiatorName, uint checkId, RpcParams rpcParams = default)
        {
            serverReadyCheckSeq = checkId;
            if (LocalOwner) LocalOwner.serverReadyCheckSeq = checkId;
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

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void SendEnemySnapshotClientRpc(string enemyKey, float hp, float maxHp, bool alive, Vector3 pos, float yaw, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerEnemySender(rpcParams.Receive.SenderClientId)) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedState(hp, maxHp, alive, pos, yaw);
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void SendEnemyActionSnapshotClientRpc(string enemyKey, byte actionKind, float duration, Vector3 origin, string spellName, string icon, uint actionSeq, float castDuration, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerEnemySender(rpcParams.Receive.SenderClientId)) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedAction(actionSeq, actionKind, duration, origin, spellName, icon, castDuration);
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
            if (!CoopSessionConfig.IsRealmClient)
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
            if (Time.time < lastServerReadyCheckTime + 1.2f) return;
            lastServerReadyCheckTime = Time.time;
            activeGlobalReadyCheckSeq++;
            activeGlobalReadyCheckExpires = Time.time + 25f;
            foreach (var p in AllPlayers)
            {
                if (!p) continue;
                p.ReadyStatus.Value = 0;
                p.ReadyCheckId.Value = activeGlobalReadyCheckSeq;
            }
            ReadyStatus.Value = 1;
            BroadcastReadyCheckClientRpc(PlayerName.Value.ToString(), activeGlobalReadyCheckSeq);
        }

        [Rpc(SendTo.Everyone)]
        void BroadcastReadyCheckClientRpc(string initiatorName, uint checkId)
        {
            serverReadyCheckSeq = checkId;
            if (LocalOwner) LocalOwner.serverReadyCheckSeq = checkId;
            if (CoopWorldManager.Instance) CoopWorldManager.Instance.OpenReadyCheck(initiatorName);
        }

        public void RespondReadyCheck(bool ready)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            uint cid = ReadyCheckId.Value != 0 ? ReadyCheckId.Value : serverReadyCheckSeq;
            RespondReadyCheckServerRpc(ready, cid);
        }

        public void RespondReadyCheckForTest(bool ready, uint checkId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            RespondReadyCheckServerRpc(ready, checkId);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RespondReadyCheckServerRpc(bool ready, uint checkId)
        {
            if (activeGlobalReadyCheckSeq == 0 || checkId != activeGlobalReadyCheckSeq) return;
            if (Time.time > activeGlobalReadyCheckExpires) return;
            ReadyStatus.Value = (byte)(ready ? 1 : 2);
        }

        public void RequestPromoteLeader(ulong targetClientId)
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned) return;
            RequestPromoteLeaderServerRpc(targetClientId);
        }

        public void CycleSocialRole()
        {
            if (!CoopSessionConfig.IsMultiplayer || !IsSpawned || !IsOwner) return;
            byte next = (byte)((SocialRole.Value + 1) % 4);
            RequestSetSocialRoleServerRpc(next);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestSetSocialRoleServerRpc(byte role)
        {
            // Presentation-only party badge; never modifies Combatant stats or permissions.
            SocialRole.Value = SanitizeSocialRole(role);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        void RequestPromoteLeaderServerRpc(ulong targetClientId)
        {
            if (!IsLeader.Value) return;
            var target = FindByClientId(targetClientId);
            if (!target || !target.IsSpawned) return;
            foreach (var p in AllPlayers)
                if (p && p.IsSpawned) p.IsLeader.Value = (p.OwnerClientId == targetClientId);
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
            if (CoopSessionConfig.IsDedicatedServer || !IsSpawned || IsOwner || FrontendMenu.IsOpen) return;
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
