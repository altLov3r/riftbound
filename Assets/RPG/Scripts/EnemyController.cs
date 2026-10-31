using UnityEngine;

namespace Riftbound
{
    [RequireComponent(typeof(CharacterController), typeof(Combatant))]
    public class EnemyController : MonoBehaviour
    {
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
        Combatant stats;
        CharacterController motor;
        Vector3 home;
        float nextAttack, deathTime, attackEnds, damageAt;
        bool pendingDamage, dying;
        string state;
        float heavyUntil, nextHeavy;
        Vector3 heavyCenter;
        LineRenderer warning;
        void Awake()
        {
            stats = GetComponent<Combatant>(); motor = GetComponent<CharacterController>(); home = transform.position;
            stats.Died += Defeated;
        }
        void Defeated(Combatant _) { if (!trainingDummy && player) player.RecordEnemyDefeat(this); if (warning) warning.enabled = false; }
        void OnDisable() { heavyUntil = 0; pendingDamage = false; if (warning) warning.enabled = false; }
        void OnDestroy() { if (stats) stats.Died -= Defeated; if (warning) { Destroy(warning.material); Destroy(warning.gameObject); } }
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
            warning.enabled = true;
            for (int i = 0; i < warning.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / 64;
                warning.SetPosition(i, heavyCenter + new Vector3(Mathf.Sin(angle) * 4, .1f, Mathf.Cos(angle) * 4));
            }
            Play("Idle"); if (animator) animator.speed = 1;
            string spell = heavyAttacker ? "Crushing Blow" : (ranged ? "Shadow Bolt" : "Savage Strike");
            string icon = heavyAttacker ? "🔨" : (ranged ? "💀" : "⚔");
            CastBarSystem.StartTargetCast(stats, spell, icon, 1.8f, null, true);
        }
        void Update()
        {
            if (FrontendMenu.IsOpen || Time.timeScale <= 0) return;
            if (trainingDummy)
            {
                if (!stats.Alive || Time.time - stats.LastHit > 3) stats.Restore();
                return;
            }
            if (!stats.Alive)
            {
                if (!dying) { dying = true; deathTime = Time.time; motor.enabled = false; if (animator) animator.speed = 1; Play(nativeDeathAnimation ? "Death" : "Hit", true); }
                if (!nativeDeathAnimation) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 82), Time.deltaTime * 3);
                if (Time.time > deathTime + 5) gameObject.SetActive(false);
                return;
            }
            if (!player) return;
            motor.Move(Vector3.down * 4 * Time.deltaTime);
            float range = Vector3.Distance(transform.position, player.transform.position);
            if (heavyUntil > 0)
            {
                if (CastBarSystem.GetTargetCast(stats) is var tCast && tCast != null && tCast.isInterrupted)
                {
                    heavyUntil = 0;
                    if (warning) warning.enabled = false;
                    return;
                }
                if (Time.time < heavyUntil) { FacePlayer(); return; }
                heavyUntil = 0; warning.enabled = false;
                if (player.Stats.Alive && Vector3.Distance(player.transform.position, heavyCenter) < 4) player.Stats.Damage(38);
                Play("Cleave", true); attackEnds = Time.time + cleaveDuration; return;
            }
            bool chasing = player.Stats.Alive && range < 12 && Vector3.Distance(home, player.transform.position) < 19;
            if (chasing && Time.time < attackEnds) FacePlayer();
            if (pendingDamage && Time.time >= damageAt)
            {
                pendingDamage = false;
                if (player.Stats.Alive && ranged && range < attackRange + 2)
                    EnemyProjectile.Launch(transform.position + Vector3.up * 1.25f, player, attackDamage);
                else if (player.Stats.Alive && !ranged && range < 2.8f && ClearSight()) player.Stats.Damage(attackDamage);
            }
            if (Time.time < attackEnds) return;
            if ((isBoss || heavyAttacker) && chasing && range < 7 && Time.time >= nextHeavy) { HeavyAttack(); return; }
            Vector3 destination = chasing ? player.transform.position : home;
            var direction = destination - transform.position; direction.y = 0;
            if (chasing && range < attackRange)
            {
                FacePlayer();
                if (Time.time >= nextAttack)
                {
                    nextAttack = Time.time + Mathf.Max(attackInterval, attackDuration + .1f);
                    attackEnds = Time.time + attackDuration; damageAt = Time.time + attackDuration * .45f;
                    if (animator) animator.speed = 1;
                    pendingDamage = true; Play("Attack", true);
                }
                else { Play("Idle"); if (animator) animator.speed = 1; }
            }
            else if (direction.magnitude > .6f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 7);
                float speed = (chasing ? player.runSpeed : player.walkSpeed) * movementMultiplier;
                var previousPosition = transform.position;
                motor.Move(direction.normalized * speed * Time.deltaTime);
                var travelled = transform.position - previousPosition; travelled.y = 0;
                Play(travelled.sqrMagnitude > .000001f ? (chasing ? "Run" : "Walk") : "Idle");
                float stride = chasing ? (runStrideSpeed > 0 ? runStrideSpeed : player.runStrideSpeed) : (walkStrideSpeed > 0 ? walkStrideSpeed : player.walkStrideSpeed);
                if (animator) animator.speed = Mathf.Clamp(travelled.magnitude / Mathf.Max(.001f, Time.deltaTime * stride), .6f, 2.0f);
            }
            else { Play("Idle"); if (animator) animator.speed = 1; }
        }
        void Play(string value, bool force = false)
        {
            if (!animator || (!force && state == value)) return;
            animator.CrossFadeInFixedTime(value, .18f, 0, 0); state = value;
        }
        bool ClearSight()
        {
            Vector3 origin = transform.position + Vector3.up * 1.1f;
            Vector3 delta = player.transform.position + Vector3.up - origin;
            Vector3 heading = Vector3.ProjectOnPlane(delta, Vector3.up);
            return (heading.sqrMagnitude < .01f || Vector3.Dot(transform.forward, heading.normalized) > .25f) && !Physics.Raycast(origin, delta.normalized, delta.magnitude, ~(1 << 2), QueryTriggerInteraction.Ignore);
        }
        void FacePlayer()
        {
            if (!player) return;
            Vector3 heading = player.transform.position - transform.position; heading.y = 0;
            if (heading.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(heading);
        }
    }
}
