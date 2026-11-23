using UnityEngine;
using Riftbound.Network;

namespace Riftbound
{
    // Pose the driver after animation evaluation and before the visible actor copies its bones.
    [DefaultExecutionOrder(600)]
    public sealed class SpellCastPose : MonoBehaviour
    {
        struct Arm
        {
            internal Transform upper, forearm, hand;
            internal Quaternion upperRest, forearmRest;

            internal Arm(Animator animator, bool left) : this()
            {
                upper = animator.GetBoneTransform(left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm);
                forearm = animator.GetBoneTransform(left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm);
                hand = animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            }

            internal void Pose(Vector3 elbow, Vector3 wrist, float weight)
            {
                if (!upper || !forearm || !hand) return;
                upperRest = upper.localRotation;
                forearmRest = forearm.localRotation;
                Aim(upper, forearm.position - upper.position, elbow - upper.position, weight);
                Aim(forearm, hand.position - forearm.position, wrist - forearm.position, weight);
            }

            static void Aim(Transform bone, Vector3 from, Vector3 to, float weight)
            {
                if (from.sqrMagnitude < .0001f || to.sqrMagnitude < .0001f) return;
                bone.rotation = Quaternion.Slerp(bone.rotation, Quaternion.FromToRotation(from, to) * bone.rotation, weight);
            }

            internal void Restore()
            {
                if (!upper || !forearm || !hand) return;
                upper.localRotation = upperRest;
                forearm.localRotation = forearmRest;
            }
        }

        PlayerController player;
        CastBarSystem cast;
        RiftboundCoopPlayer serverCaster;
        Animator boundAnimator;
        Arm left, right;
        bool holy, applied;
        float releasedAt = float.NegativeInfinity;

        internal void Begin(PlayerController caster, CastBarSystem bar, bool healing)
        {
            Restore();
            player = caster;
            cast = bar;
            serverCaster = null;
            holy = healing;
            releasedAt = float.NegativeInfinity;
        }

        internal void Release() => releasedAt = Time.time;

        internal void BeginServer(PlayerController caster, RiftboundCoopPlayer owner, bool healing)
        {
            Begin(caster, null, healing);
            serverCaster = owner;
        }

        internal void CancelServer()
        {
            serverCaster = null;
            releasedAt = float.NegativeInfinity;
            Restore();
        }

        void Update() => Restore();
        void OnDisable() => Restore();

        void Restore()
        {
            if (!applied) return;
            left.Restore();
            right.Restore();
            applied = false;
        }

        void LateUpdate()
        {
            bool online = serverCaster && serverCaster.IsSpawned && serverCaster.HasActiveClientCast;
            bool casting = online || (cast && cast.IsOfflineCombatCasting);
            const float blendOut = .22f;
            float elapsedAfterRelease = Time.time - releasedAt;
            if ((!casting && elapsedAfterRelease >= blendOut) || !player || !player.animator || !player.animator.isHuman) return;
            if (boundAnimator != player.animator)
            {
                boundAnimator = player.animator;
                left = new Arm(boundAnimator, true);
                right = new Arm(boundAnimator, false);
            }
            var chest = boundAnimator.GetBoneTransform(HumanBodyBones.Chest);
            if (!chest) return;
            float progress = online
                ? Mathf.Clamp01(CastBarSystem.ServerCastElapsed(serverCaster) / serverCaster.ActiveClientCastDuration)
                : casting ? Mathf.Clamp01((Time.time - cast.CastStartTime) / cast.CastDuration) : 1f;
            float weight = casting ? Mathf.SmoothStep(0f, .95f, progress / .22f) :
                Mathf.SmoothStep(.95f, 0f, elapsedAfterRelease / blendOut);
            float release = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.65f, 1f, progress));
            Vector3 forward = transform.forward, up = transform.up, side = transform.right;
            Vector3 wrist = chest.position + forward * (holy ? .26f : .31f + .25f * release) +
                up * (holy ? .07f + .35f * release : .06f + .08f * release);
            Vector3 elbow = chest.position + forward * .19f - up * .12f;
            float spread = holy ? .10f + .20f * release : .10f;
            left.Pose(elbow - side * .32f, wrist - side * spread, weight);
            right.Pose(elbow + side * .32f, wrist + side * spread, weight);
            applied = true;
        }
    }
}
