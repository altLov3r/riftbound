using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    [RequireComponent(typeof(CharacterController), typeof(Combatant))]
    public class EnemyController : MonoBehaviour
    {
        public static readonly Dictionary<string, EnemyController> AllByKey = new Dictionary<string, EnemyController>();
        public string NetworkKey => !string.IsNullOrEmpty(enemyId) ? enemyId : gameObject.name;
        public static EnemyController FindByKey(string key) => !string.IsNullOrEmpty(key) && AllByKey.TryGetValue(key, out var e) ? e : null;
        public Animator animator;
        public float attackDuration = 1.2f;
        public PlayerController player;
        public string enemyId;
        public bool campGuard, isBoss;
        public bool trainingDummy;
        public bool IsMainQuestCaptain => enemyId == "ashwood-captain";
        public bool IsRoadRaider => !trainingDummy && enemyId != null && enemyId.StartsWith("road-raider-", System.StringComparison.Ordinal);
        public bool heavyAttacker, requiresRunes;
        public float attackDamage = 10;
        public float movementMultiplier = 1;
        public bool ranged, nativeDeathAnimation;
        public float attackRange = 2.1f;
        public float attackInterval = 1.6f;
        public float walkStrideSpeed, runStrideSpeed;
        public float cleaveDuration = 1.22f;
        public enum EnemyAffix { None, Molten, Vampiric, Frostbound, Arcane }
        public EnemyAffix Affix { get; private set; }
        Combatant stats;
        CharacterController motor;
        Vector3 home;
        Quaternion homeRotation;
        float nextAttack, deathTime, attackEnds, damageAt;
        bool pendingDamage, dying, evading, initialized, resetPoseOnEnable, wasChasing;
        float nextEncounterBarkAt;
        string state;
        float heavyUntil, nextHeavy;
        Vector3 heavyCenter;
        LineRenderer warning;
        bool hasReplicatedState;
        Vector3 replicatedPos;
        float replicatedYaw;
        float netBroadcastTimer;
        void Awake() { Initialize(); }
        void Initialize()
        {
            if (initialized) return;
            stats = GetComponent<Combatant>(); motor = GetComponent<CharacterController>();
            home = transform.position; homeRotation = transform.rotation;
            stats.Died += Defeated;
            AllByKey[NetworkKey] = this;
            if (!trainingDummy)
            {
                int hash = Mathf.Abs((string.IsNullOrEmpty(enemyId) ? gameObject.name : enemyId).GetHashCode());
                if (isBoss || heavyAttacker || (hash % 4 == 0))
                    Affix = (EnemyAffix)((hash % 4) + 1);
            }
            initialized = true;
        }
        void OnEnable()
        {
            wasChasing = false;
            if (resetPoseOnEnable) ResetIdlePose();
        }
        public void ResetForLoad()
        {
            Initialize();
            CancelAttack(); dying = false; evading = false;
            nextAttack = nextHeavy = deathTime = damageAt = 0;
            motor.enabled = false;
            transform.SetPositionAndRotation(home, homeRotation);
            motor.enabled = true;
            stats.Restore();
            resetPoseOnEnable = true;
            ResetIdlePose();
        }
        void ResetIdlePose()
        {
            if (!animator) animator = GetComponentInChildren<Animator>(true);
            if (!animator) { resetPoseOnEnable = false; return; }
            animator.speed = 1;
            if (!animator.isActiveAndEnabled) return;
            animator.Rebind();
            animator.Play("Idle", 0, 0);
            animator.Update(0);
            state = "Idle"; resetPoseOnEnable = false;
        }
        void Defeated(Combatant _)
        {
            CancelAttack();
            if (!trainingDummy && player)
            {
                if (Network.CoopSessionConfig.IsMultiplayer && Unity.Netcode.NetworkManager.Singleton && Unity.Netcode.NetworkManager.Singleton.IsListening)
                {
                    if (Unity.Netcode.NetworkManager.Singleton.IsServer)
                        Network.RiftboundCoopPlayer.DispatchEnemyDefeatedToEligiblePlayers(this);
                }
                else
                {
                    player.RecordEnemyDefeat(this);
                }
                CombatStreakSystem.RegisterKill(transform.position);
                if (Affix == EnemyAffix.Molten)
                    SpellFX.CreateHeavySlam(transform.position, 3.2f);
                else if (Affix == EnemyAffix.Arcane)
                {
                    player.AddExperience(40);
                    CombatTextManager.Spawn(transform.position + Vector3.up * 2.1f, "+40 ARCANE XP", new Color(0.85f, 0.65f, 1f), true);
                }
                SpellFX.SpawnBurst(transform.position + Vector3.up * 0.9f, new Color(1f, 0.8f, 0.3f), new Color(0.6f, 0.25f, 0.95f), 18, 2f, 5f, 0.1f, 0.22f, 0.55f);
                SpellFX.CreateSoulOrb(transform.position + Vector3.up * 1.1f, player.transform);
            }
        }
        void OnDisable() { CancelAttack(); evading = false; }
        void CancelAttack()
        {
            heavyUntil = 0; pendingDamage = false; attackEnds = 0;
            if (warning) warning.enabled = false;
            CastBarSystem.CancelTargetCast(stats);
        }
        void OnDestroy()
        {
            if (AllByKey.TryGetValue(NetworkKey, out var existing) && existing == this) AllByKey.Remove(NetworkKey);
            if (stats) stats.Died -= Defeated;
            if (warning) { Destroy(warning.material); Destroy(warning.gameObject); }
        }

        public void ApplyReplicatedState(float hp, float maxHp, bool alive, Vector3 pos, float yaw)
        {
            if (!stats) Initialize();
            replicatedPos = pos;
            replicatedYaw = yaw;
            hasReplicatedState = true;
            stats.SetReplicatedHealth(hp, maxHp);
            if (alive && !gameObject.activeSelf) ResetForLoad();
        }
        void HeavyAttack()
        {
            if (!warning)
            {
                warning = new GameObject("Captain heavy strike warning").AddComponent<LineRenderer>();
                warning.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                warning.material.SetColor("_BaseColor", new Color(1, .15f, .04f));
                warning.positionCount = 65; warning.startWidth = warning.endWidth = .16f;
            }
            heavyCenter = transform.position; heavyUntil = Time.time + 1.8f; nextHeavy = Time.time + 9;
            CombatTextManager.Spawn(transform.position + Vector3.up * 2.5f, "⚠ HEAVY SLAM!", new Color(1f, 0.45f, 0.1f), true);
            warning.enabled = true;
            for (int i = 0; i < warning.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / 64;
                warning.SetPosition(i, heavyCenter + new Vector3(Mathf.Sin(angle) * 4, .1f, Mathf.Cos(angle) * 4));
            }
            Play("Idle"); if (animator) animator.speed = 1;
            string spell = heavyAttacker ? "Crushing Blow" : (ranged ? "Shadow Bolt" : "Savage Strike");
            string icon = heavyAttacker ? "🔨" : (ranged ? "💀" : "⚔");
            GameSound.Play(SoundCue.Rune, transform.position);
            CastBarSystem.StartTargetCast(stats, spell, icon, 1.8f, null, true);
        }
        void Update()
        {
            if (FrontendMenu.IsOpen || Time.timeScale <= 0) return;

            bool isNetClient = Network.CoopSessionConfig.IsMultiplayer
                && Unity.Netcode.NetworkManager.Singleton
                && Unity.Netcode.NetworkManager.Singleton.IsListening
                && !Unity.Netcode.NetworkManager.Singleton.IsServer;

            if (isNetClient)
            {
                if (!stats.Alive)
                {
                    if (!dying) { dying = true; deathTime = Time.time; motor.enabled = false; if (animator) animator.speed = 1; Play(nativeDeathAnimation ? "Death" : "Hit", true); }
                    if (!nativeDeathAnimation) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 82), Time.deltaTime * 3);
                    if (Time.time > deathTime + 5) gameObject.SetActive(false);
                    return;
                }
                if (hasReplicatedState)
                {
                    float d = Vector3.Distance(transform.position, replicatedPos);
                    if (d > 0.08f)
                    {
                        Vector3 prevPos = transform.position;
                        transform.position = Vector3.Lerp(transform.position, replicatedPos, Time.deltaTime * 12f);
                        float approxSpeed = Time.deltaTime > 0.0001f ? Vector3.Distance(prevPos, transform.position) / Time.deltaTime : d * 12f;
                        Play(approxSpeed > 2.4f ? "Run" : "Walk", false);
                    }
                    else
                    {
                        Play("Idle", false);
                    }
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, replicatedYaw, 0f), Time.deltaTime * 12f);
                }
                return;
            }

            if (trainingDummy)
            {
                if (!stats.Alive || Time.time - stats.LastHit > 3) stats.Restore();
                return;
            }
            if (!stats.Alive)
            {
                if (!dying)
                {
                    dying = true; deathTime = Time.time; motor.enabled = false; if (animator) animator.speed = 1; Play(nativeDeathAnimation ? "Death" : "Hit", true);
                    if (Network.CoopSessionConfig.IsMultiplayer && Network.RiftboundCoopPlayer.LocalOwner)
                        Network.RiftboundCoopPlayer.LocalOwner.BroadcastEnemyState(this);
                }
                if (!nativeDeathAnimation) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 82), Time.deltaTime * 3);
                if (Time.time > deathTime + 5) gameObject.SetActive(false);
                return;
            }

            Transform targetTransform = player ? player.transform : null;
            bool targetAlive = player && player.Stats && player.Stats.Alive;
            ulong targetClientId = 0;
            bool targetIsRemote = false;

            if (Network.CoopSessionConfig.IsMultiplayer && Unity.Netcode.NetworkManager.Singleton && Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                float bestDist = float.MaxValue;
                foreach (var cp in Network.RiftboundCoopPlayer.AllPlayers)
                {
                    if (!cp || !cp.IsSpawned || cp.Health.Value <= 0) continue;
                    float dist = Vector3.Distance(transform.position, cp.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        targetTransform = cp.transform;
                        targetAlive = true;
                        targetClientId = cp.OwnerClientId;
                        targetIsRemote = !cp.IsOwner;
                    }
                }
            }

            if (!targetTransform) return;
            motor.Move(Vector3.down * 4 * Time.deltaTime);
            float range = Vector3.Distance(transform.position, targetTransform.position);
            float homeDistance = Vector3.Distance(transform.position, home);
            bool inPursuitRange = targetAlive && range < 14 && Vector3.Distance(home, targetTransform.position) < 25;
            if (!inPursuitRange && homeDistance > 1.2f) evading = true;
            if (evading && homeDistance <= 1.2f) { evading = false; stats.Restore(); }
            bool chasing = inPursuitRange && !evading;
            if (chasing && !wasChasing)
            {
                CombatTextManager.Spawn(transform.position + Vector3.up * 2.2f, "!", new Color(1f, 0.25f, 0.15f), true);
                if (Time.time >= nextEncounterBarkAt)
                {
                    var enemyProfile = Narrative.NarrativeCatalog.ResolveHostileEnemy(this);
                    if (enemyProfile != null)
                    {
                        var barkSel = Narrative.NarrativeCatalog.SelectText(enemyProfile, "encounter_bark", "aggro", player, this);
                        if (!barkSel.matched)
                            barkSel = Narrative.NarrativeCatalog.SelectText(enemyProfile, "encounter_bark", "", player, this);
                        if (barkSel.matched && !string.IsNullOrEmpty(barkSel.text))
                        {
                            nextEncounterBarkAt = Time.time + 25f;
                            CombatTextManager.Spawn(transform.position + Vector3.up * 2.65f, $"\"{barkSel.text}\"", new Color(1f, 0.78f, 0.35f), false);
                            if (ChatSystem.Instance)
                                ChatSystem.Instance.AddMessage(ChatSystem.Channel.Say, stats ? stats.displayName : name, barkSel.text, new Color(1f, 0.45f, 0.35f));
                        }
                    }
                }
            }
            wasChasing = chasing;
            if (!chasing) CancelAttack();
            if (heavyUntil > 0)
            {
                if (CastBarSystem.GetTargetCast(stats) is var tCast && tCast != null && tCast.isInterrupted)
                {
                    heavyUntil = 0;
                    attackEnds = Time.time + 1.6f;
                    if (warning) warning.enabled = false;
                    Play("Idle", true);
                    return;
                }
                if (Time.time < heavyUntil) { FacePoint(targetTransform.position); return; }
                heavyUntil = 0; warning.enabled = false;
                SpellFX.CreateHeavySlam(heavyCenter, 4f);
                if (targetAlive && Vector3.Distance(targetTransform.position, heavyCenter) < 4)
                {
                    if (targetIsRemote)
                        Network.RiftboundCoopPlayer.DispatchEnemyHitToPlayer(targetClientId, 38f, NetworkKey);
                    else if (player && player.Stats)
                    {
                        player.Stats.Damage(38);
                        if (Network.CoopSessionConfig.IsMultiplayer && Network.RiftboundCoopPlayer.LocalOwner)
                            Network.RiftboundCoopPlayer.LocalOwner.Health.Value = Mathf.Max(0, Mathf.RoundToInt(player.Stats.Health));
                    }
                }
                Play("Cleave", true); attackEnds = Time.time + cleaveDuration; return;
            }
            if (chasing && Time.time < attackEnds) FacePoint(targetTransform.position);
            if (pendingDamage)
            {
                if (CastBarSystem.GetTargetCast(stats) is var tCast && tCast != null && tCast.isInterrupted)
                {
                    pendingDamage = false;
                    attackEnds = Time.time + 2.0f;
                    Play("Idle", true);
                    return;
                }
            }
            if (pendingDamage && Time.time >= damageAt)
            {
                pendingDamage = false;
                CastBarSystem.CancelTargetCast(stats);
                if (targetAlive && ranged && range < attackRange + 2 && ClearSightForTarget(targetTransform))
                {
                    if (targetIsRemote)
                        EnemyProjectile.LaunchForRemotePlayer(transform.position + Vector3.up * 1.25f, targetClientId, targetTransform, attackDamage, NetworkKey);
                    else if (player)
                    {
                        EnemyProjectile.Launch(transform.position + Vector3.up * 1.25f, player, attackDamage);
                    }
                }
                else if (targetAlive && !ranged && range < 2.8f && ClearSightForTarget(targetTransform))
                {
                    if (targetIsRemote)
                    {
                        Network.RiftboundCoopPlayer.DispatchEnemyHitToPlayer(targetClientId, attackDamage, NetworkKey);
                    }
                    else if (player && player.Stats)
                    {
                        player.Stats.Damage(attackDamage);
                        if (Network.CoopSessionConfig.IsMultiplayer && Network.RiftboundCoopPlayer.LocalOwner)
                            Network.RiftboundCoopPlayer.LocalOwner.Health.Value = Mathf.Max(0, Mathf.RoundToInt(player.Stats.Health));
                        if (Affix == EnemyAffix.Vampiric) stats.Heal(attackDamage * 0.35f);
                        else if (Affix == EnemyAffix.Frostbound)
                            BuffSystem.ApplyTargetDebuff(player.Stats, "frost_chill", "Chilled", "Slowed by frostbound foe.", "❄", new Color(0.4f, 0.85f, 1f), 2.5f, 0f);
                    }
                }
            }
            if (Time.time < attackEnds) return;
            if ((isBoss || heavyAttacker) && chasing && range < 7 && Time.time >= nextHeavy) { HeavyAttack(); return; }
            Vector3 destination = chasing ? targetTransform.position : home;
            if (!chasing && Vector3.Distance(transform.position, home) > 1.2f)
            {
                // Evading / Leashing back to home camp: rapidly regenerate health
                if (stats && stats.Health < stats.maxHealth)
                    stats.Heal(stats.maxHealth * Time.deltaTime * 0.45f);
            }
            var direction = destination - transform.position; direction.y = 0;
            if (chasing && range < attackRange)
            {
                FacePoint(targetTransform.position);
                if (Time.time >= nextAttack)
                {
                    nextAttack = Time.time + Mathf.Max(attackInterval, attackDuration + .1f);
                    attackEnds = Time.time + attackDuration;
                    damageAt = Time.time + (ranged ? attackDuration * 0.85f : attackDuration * .45f);
                    if (animator) animator.speed = 1;
                    pendingDamage = true; Play("Attack", true);
                    if (ranged)
                    {
                        string spellName = gameObject.name.Contains("Hexer") ? "Shadow Bolt" : gameObject.name.Contains("Sorcerer") ? "Chaos Fire" : "Frost Bolt";
                        string icon = gameObject.name.Contains("Hexer") ? "💀" : gameObject.name.Contains("Sorcerer") ? "🔥" : "❄";
                        CastBarSystem.StartTargetCast(stats, spellName, icon, attackDuration * 0.85f, null, true);
                    }
                }
                else { Play("Idle"); if (animator) animator.speed = 1; }
            }
            else if (direction.magnitude > .6f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 7);
                float speed = (chasing ? (player ? player.runSpeed : 6f) : (player ? player.walkSpeed : 3.2f)) * movementMultiplier;
                var previousPosition = transform.position;
                motor.Move(direction.normalized * speed * Time.deltaTime);
                var travelled = transform.position - previousPosition; travelled.y = 0;
                Play(travelled.sqrMagnitude > .000001f ? (chasing ? "Run" : "Walk") : "Idle");
                float stride = chasing
                    ? (runStrideSpeed > 0 ? runStrideSpeed : (player ? player.runStrideSpeed : 4.2f))
                    : (walkStrideSpeed > 0 ? walkStrideSpeed : (player ? player.walkStrideSpeed : 0.9f));
                if (animator) animator.speed = Mathf.Clamp(travelled.magnitude / Mathf.Max(.001f, Time.deltaTime * stride), .65f, 2.25f);
            }
            else
            {
                Play("Idle");
                if (animator) animator.speed = 1;
                if (!chasing && Vector3.Distance(transform.position, home) <= 1.2f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, homeRotation, Time.deltaTime * 5f);
            }

            if (Network.CoopSessionConfig.IsMultiplayer && Unity.Netcode.NetworkManager.Singleton && Unity.Netcode.NetworkManager.Singleton.IsServer && (chasing || evading))
            {
                netBroadcastTimer += Time.deltaTime;
                if (netBroadcastTimer >= 0.1f)
                {
                    netBroadcastTimer = 0f;
                    if (Network.RiftboundCoopPlayer.LocalOwner)
                        Network.RiftboundCoopPlayer.LocalOwner.BroadcastEnemyState(this);
                }
            }
        }

        void FacePoint(Vector3 point)
        {
            var look = point - transform.position; look.y = 0;
            if (look.sqrMagnitude > .01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(look), 540f * Time.deltaTime);
        }
        void Play(string value, bool force = false)
        {
            if (!animator || (!force && state == value)) return;
            animator.CrossFadeInFixedTime(value, .18f, 0, 0); state = value;
        }
        bool ClearSight() => ClearSightForTarget(player ? player.transform : null);
        bool ClearSightForTarget(Transform target)
        {
            Transform t = target ? target : (player ? player.transform : null);
            if (!t) return false;
            Vector3 origin = transform.position + Vector3.up * 1.1f;
            Vector3 delta = t.position + Vector3.up - origin;
            Vector3 heading = Vector3.ProjectOnPlane(delta, Vector3.up);
            if (heading.sqrMagnitude >= .01f && Vector3.Dot(transform.forward, heading.normalized) <= .25f) return false;
            return !Physics.Raycast(origin, delta.normalized, out var hit, delta.magnitude, ~(1 << 2), QueryTriggerInteraction.Ignore) ||
                hit.collider.GetComponentInParent<PlayerController>() != null ||
                hit.collider.GetComponentInParent<Network.RiftboundCoopPlayer>() != null ||
                hit.collider.transform.IsChildOf(t);
        }
        void FacePlayer()
        {
            if (!player) return;
            Vector3 heading = player.transform.position - transform.position; heading.y = 0;
            if (heading.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(heading);
        }
    }
}
