using UnityEngine;

namespace Riftbound
{
    public class MountVisual : MonoBehaviour
    {
        public Animator animator;
        public Transform saddle;
        public float walkStride = 1.5f, runStride = 4;
        [HideInInspector] public int artVersion;
        string state;
        public void Animate(float speed, bool airborne, bool walking = false)
        {
            string next = airborne ? "Jump" : speed < .15f ? "Idle" : walking ? "Walk" : "Run";
            if (state != next) { animator.CrossFadeInFixedTime(next, .15f, 0, 0); state = next; }
            float stride = next == "Run" ? 7.2f : 2.6f;
            animator.speed = airborne ? 1.0f : speed < .15f ? 1.0f : Mathf.Clamp(speed / stride, .85f, 1.25f);
        }
    }
}
