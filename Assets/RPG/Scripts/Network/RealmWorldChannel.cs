using Unity.Netcode;
using UnityEngine;

namespace Riftbound.Network
{
    /// <summary>
    /// Server-owned world broadcast and snapshot channel for both Dedicated Server (no host avatar)
    /// and Listen Host sessions. Keeps all enemy/world-event replication independent of LocalOwner.
    /// </summary>
    public class RealmWorldChannel : NetworkBehaviour
    {
        public static RealmWorldChannel Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
        }

        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this)
            {
                if (IsServer) Debug.LogWarning("[RealmWorldChannel] Duplicate channel spawned; keeping latest server channel.");
            }
            Instance = this;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
        }

        public static bool IsAuthorizedServerSenderForIds(ulong ownerClientId, ulong senderClientId, ulong serverClientId) =>
            ownerClientId == serverClientId && senderClientId == serverClientId;

        bool IsAuthorizedServerSender(ulong senderClientId) =>
            NetworkManager.Singleton != null
            && IsAuthorizedServerSenderForIds(OwnerClientId, senderClientId, NetworkManager.ServerClientId);

        static RealmWorldChannel ResolveActiveServerChannel()
        {
            if (Instance && Instance.IsSpawned && Instance.IsServer &&
                NetworkManager.Singleton != null &&
                Instance.OwnerClientId == NetworkManager.ServerClientId)
                return Instance;
            return null;
        }

        public static void DispatchVisualEnemyProjectile(Vector3 origin, Vector3 direction)
        {
            var ch = ResolveActiveServerChannel();
            if (ch != null)
            {
                ch.BroadcastVisualEnemyProjectileClientRpc(origin, direction);
                return;
            }
            var fallback = RiftboundCoopPlayer.ResolveCanonicalHostChannel();
            if (fallback != null) fallback.BroadcastVisualEnemyProjectile(origin, direction);
        }

        public static void DispatchEnemyState(EnemyController enemy)
        {
            if (!enemy) return;
            var ch = ResolveActiveServerChannel();
            if (ch != null)
            {
                var c = enemy.GetComponent<Combatant>();
                if (!c) return;
                ch.BroadcastEnemyStateClientRpc(enemy.NetworkKey, c.Health, c.maxHealth, c.Alive, enemy.transform.position, enemy.transform.eulerAngles.y);
                return;
            }
            var fallback = RiftboundCoopPlayer.ResolveCanonicalHostChannel();
            if (fallback != null) fallback.BroadcastEnemyState(enemy);
        }

        public static void DispatchEnemyAction(EnemyController enemy, byte actionKind, float duration, Vector3 origin, string spellName, string icon, uint actionSeq, float castDuration = -1f)
        {
            if (!enemy) return;
            var ch = ResolveActiveServerChannel();
            if (ch != null)
            {
                ch.BroadcastEnemyActionClientRpc(enemy.NetworkKey ?? "", actionKind, Mathf.Clamp(duration, 0f, 3.5f), origin, spellName ?? "", icon ?? "", actionSeq, castDuration);
                return;
            }
            var fallback = RiftboundCoopPlayer.ResolveCanonicalHostChannel();
            if (fallback != null) fallback.BroadcastEnemyAction(enemy, actionKind, duration, origin, spellName, icon, actionSeq, castDuration);
        }

        public static void DispatchWorldEventState(string eventId, bool active, Vector3 pos)
        {
            var ch = ResolveActiveServerChannel();
            if (ch != null)
            {
                ch.BroadcastWorldEventStateClientRpc(eventId ?? "", active, pos);
                return;
            }
            RiftboundCoopPlayer.BroadcastWorldEventState(eventId, active, pos);
        }

        public static void DispatchWorldSnapshotToClient(ulong clientId)
        {
            var ch = ResolveActiveServerChannel();
            if (ch != null)
            {
                ch.SendWorldSnapshotToClient(clientId);
                return;
            }
            var fallback = RiftboundCoopPlayer.ResolveCanonicalHostChannel();
            if (fallback != null) fallback.SendWorldSnapshotToClient(clientId);
        }

        public void SendWorldSnapshotToClient(ulong clientId)
        {
            if (!IsServer || !IsSpawned || !IsAuthorizedServerSender(NetworkManager.ServerClientId)) return;
            var rpcParams = RpcTarget.Single(clientId, RpcTargetUse.Temp);
            foreach (var kv in EnemyController.AllByKey)
            {
                var enemy = kv.Value;
                if (!enemy || enemy.trainingDummy) continue;
                var c = enemy.GetComponent<Combatant>();
                if (!c) continue;
                bool hasAction = enemy.TryGetActiveActionSnapshot(out byte ak, out float rem, out float castRem, out Vector3 orig, out string sp, out string ic, out uint aseq);
                if (!c.Alive || c.Health < c.maxHealth || !enemy.gameObject.activeInHierarchy || hasAction)
                {
                    SendEnemySnapshotClientRpc(enemy.NetworkKey, c.Health, c.maxHealth, c.Alive, enemy.transform.position, enemy.transform.eulerAngles.y, rpcParams);
                    if (hasAction)
                        SendEnemyActionSnapshotClientRpc(enemy.NetworkKey, ak, rem, orig, sp, ic, aseq, castRem, rpcParams);
                }
            }
            if (CoopWorldManager.Instance != null && CoopWorldManager.Instance.TryGetActiveReadyCheck(out string rcInitiator))
            {
                var joiningPeer = RiftboundCoopPlayer.FindByClientId(clientId);
                uint rcSeq = CoopWorldManager.Instance.ActiveReadyCheckSequence;
                if (joiningPeer) joiningPeer.ReadyCheckId.Value = rcSeq;
                SendReadyCheckSnapshotClientRpc(rcInitiator, rcSeq, rpcParams);
            }
            if (LivingWorldEvents.Instance != null)
            {
                LivingWorldEvents.Instance.SendEventSnapshotViaChannel(this, rpcParams);
            }
        }

        public void SendEventSnapshotRpc(string eventId, bool active, Vector3 pos, RpcParams rpcParams)
        {
            if (!IsServer || !IsSpawned) return;
            SendEventSnapshotClientRpc(eventId, active, pos, rpcParams);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        void BroadcastVisualEnemyProjectileClientRpc(Vector3 origin, Vector3 direction, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerSender(rpcParams.Receive.SenderClientId)) return;
            EnemyProjectile.LaunchVisualOnly(origin, direction);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        void BroadcastEnemyActionClientRpc(string enemyKey, byte actionKind, float duration, Vector3 origin, string spellName, string icon, uint actionSeq, float castDuration, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerSender(rpcParams.Receive.SenderClientId)) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedAction(actionSeq, actionKind, duration, origin, spellName, icon, castDuration);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        void BroadcastEnemyStateClientRpc(string enemyKey, float hp, float maxHp, bool alive, Vector3 pos, float yaw, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerSender(rpcParams.Receive.SenderClientId)) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedState(hp, maxHp, alive, pos, yaw);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        void BroadcastWorldEventStateClientRpc(string eventId, bool active, Vector3 pos, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerSender(rpcParams.Receive.SenderClientId)) return;
            var lwe = LivingWorldEvents.Instance ? LivingWorldEvents.Instance : FindFirstObjectByType<LivingWorldEvents>();
            if (lwe) lwe.ApplyClientEventState(eventId, active, pos);
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void SendEnemySnapshotClientRpc(string enemyKey, float hp, float maxHp, bool alive, Vector3 pos, float yaw, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerSender(rpcParams.Receive.SenderClientId)) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedState(hp, maxHp, alive, pos, yaw);
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void SendEnemyActionSnapshotClientRpc(string enemyKey, byte actionKind, float duration, Vector3 origin, string spellName, string icon, uint actionSeq, float castDuration, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerSender(rpcParams.Receive.SenderClientId)) return;
            var enemy = EnemyController.FindByKey(enemyKey);
            if (enemy) enemy.ApplyReplicatedAction(actionSeq, actionKind, duration, origin, spellName, icon, castDuration);
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void SendEventSnapshotClientRpc(string eventId, bool active, Vector3 pos, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerSender(rpcParams.Receive.SenderClientId)) return;
            var lwe = LivingWorldEvents.Instance ? LivingWorldEvents.Instance : FindFirstObjectByType<LivingWorldEvents>();
            if (lwe) lwe.ApplyClientEventState(eventId, active, pos);
        }

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        void SendReadyCheckSnapshotClientRpc(string initiatorName, uint checkId, RpcParams rpcParams = default)
        {
            if (IsServer) return;
            if (!IsAuthorizedServerSender(rpcParams.Receive.SenderClientId)) return;
            if (RiftboundCoopPlayer.LocalOwner) RiftboundCoopPlayer.LocalOwner.SetServerReadyCheckSeqForChannel(checkId);
            if (CoopWorldManager.Instance) CoopWorldManager.Instance.OpenReadyCheck(initiatorName);
        }
    }
}
