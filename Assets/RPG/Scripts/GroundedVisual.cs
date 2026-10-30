using UnityEngine;

namespace Riftbound
{
    [DefaultExecutionOrder(100)]
    public class GroundedVisual : MonoBehaviour
    {
        public Animator animator;
        public float soleHeight = .025f;
        public float walkOffset, runOffset, backWalkOffset, backRunOffset;
        CharacterController motor;
        Combatant stats;
        PlayerController player;
        Transform leftFoot, rightFoot;
        bool calibrated;
        Vector3 groundedPosition;
        float currentOffset;

        void Start()
        {
            motor = GetComponent<CharacterController>();
            stats = GetComponent<Combatant>();
            player = GetComponent<PlayerController>();
            if (!animator) return;
            if (animator.isHuman)
            {
                leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftToes) ?? animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                rightFoot = animator.GetBoneTransform(HumanBodyBones.RightToes) ?? animator.GetBoneTransform(HumanBodyBones.RightFoot);
            }
            else foreach (var bone in animator.GetComponentsInChildren<Transform>())
            {
                if (bone.name.EndsWith(":LeftToeBase") || bone.name == "toes.l") leftFoot = bone;
                if (bone.name.EndsWith(":RightToeBase") || bone.name == "toes.r") rightFoot = bone;
            }
            groundedPosition = animator.transform.localPosition;
            if (leftFoot && rightFoot) { animator.Update(0); Calibrate(); }
        }

        void LateUpdate()
        {
            if (Time.deltaTime <= 0 || (player && player.Mounted)) return;
            if (!animator || !stats || !stats.Alive) return;

            // When airborne, smoothly return to base grounded position
            if (!motor || !motor.isGrounded)
            {
                animator.transform.localPosition = Vector3.Lerp(animator.transform.localPosition, groundedPosition, 1 - Mathf.Exp(-12 * Time.deltaTime));
                return;
            }

            if (!calibrated && leftFoot && rightFoot)
            {
                Calibrate();
            }

            // Smooth gait offset without re-calculating raw foot jitter every single frame
            float targetOffset = 0;
            if (calibrated)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("Walk")) targetOffset = walkOffset;
                else if (state.IsName("Run")) targetOffset = runOffset;
                else if (state.IsName("WalkBack")) targetOffset = backWalkOffset;
                else if (state.IsName("RunBack")) targetOffset = backRunOffset;
            }

            currentOffset = Mathf.Lerp(currentOffset, targetOffset, 1 - Mathf.Exp(-16 * Time.deltaTime));
            animator.transform.localPosition = groundedPosition + Vector3.up * currentOffset;
        }

        void Calibrate()
        {
            if (!Physics.Raycast(transform.position + Vector3.up * .4f, Vector3.down, out var hit, 1, ~(1 << 2), QueryTriggerInteraction.Ignore)) return;
            float lowestFoot = Mathf.Min(leftFoot.position.y, rightFoot.position.y);
            float adjustment = (hit.point.y + soleHeight) - lowestFoot;
            animator.transform.position += Vector3.up * adjustment;
            groundedPosition = animator.transform.localPosition;
            calibrated = true;
        }
    }
}
