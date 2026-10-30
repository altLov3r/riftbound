using UnityEngine;

namespace Riftbound
{
    public class EnemyProjectile : MonoBehaviour
    {
        PlayerController target;
        Vector3 direction;
        float damage, expires;
        Material material;
        public static void Launch(Vector3 origin, PlayerController player, float damage)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name = "Ashwood shadow bolt";
            go.GetComponent<Collider>().enabled = false; Destroy(go.GetComponent<Collider>());
            var bolt = go.AddComponent<EnemyProjectile>(); bolt.target = player; bolt.damage = damage;
            bolt.direction = (player.transform.position + Vector3.up - origin).normalized;
            go.transform.position = origin + bolt.direction * .8f; go.transform.localScale = Vector3.one * .3f;
            bolt.expires = Time.time + 3;
            bolt.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            bolt.material.SetColor("_BaseColor", new Color(.8f, .25f, 1)); go.GetComponent<Renderer>().sharedMaterial = bolt.material;
        }
        void Update()
        {
            if (!target || !target.Stats.Alive || Time.time >= expires) { Destroy(gameObject); return; }
            Vector3 from = transform.position, step = direction * (7 * Time.deltaTime);
            Vector3 to = from + step;
            if (step.sqrMagnitude > 0)
            {
                bool wall = Physics.SphereCast(from, .12f, direction, out var hit, step.magnitude, ~(1 << 2), QueryTriggerInteraction.Ignore);
                var bounds = target.GetComponent<CharacterController>().bounds;
                Vector3 center = bounds.center;
                float t = Mathf.Clamp01(Vector3.Dot(center - from, step) / step.sqrMagnitude);
                bool playerHit = bounds.SqrDistance(from + step * t) < .12f * .12f;
                if (playerHit && (!wall || hit.distance >= Vector3.Distance(from, from + step * t)))
                { target.Stats.Damage(damage); Destroy(gameObject); return; }
                if (wall) { Destroy(gameObject); return; }
            }
            transform.position = to;
        }
        void OnDestroy() { if (material) Destroy(material); }
    }
}
