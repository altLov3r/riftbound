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
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Ashwood shadow bolt";
            go.GetComponent<Collider>().enabled = false;
            Destroy(go.GetComponent<Collider>());

            var bolt = go.AddComponent<EnemyProjectile>();
            bolt.target = player;
            bolt.damage = damage;
            bolt.direction = (player.transform.position + Vector3.up - origin).normalized;
            go.transform.position = origin + bolt.direction * .8f;
            go.transform.localScale = Vector3.one * .3f;
            bolt.expires = Time.time + 3;
            bolt.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            bolt.material.SetColor("_BaseColor", new Color(.8f, .25f, 1));
            go.GetComponent<Renderer>().sharedMaterial = bolt.material;

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.24f;
            trail.startWidth = 0.22f;
            trail.endWidth = 0.02f;
            trail.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(0.85f, 0.3f, 1f, 0.9f);
            trail.endColor = new Color(0.4f, 0.08f, 0.65f, 0f);

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.75f, 0.25f, 1f);
            light.range = 4f;
            light.intensity = 1.8f;
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
                {
                    target.Stats.Damage(damage);
                    OrbitCamera.Shake(0.09f, 0.11f);
                    SpellFX.SpawnBurst(transform.position, new Color(0.85f, 0.3f, 1f), new Color(0.35f, 0.08f, 0.6f), 14, 2f, 4.5f, 0.09f, 0.18f, 0.32f);
                    Destroy(gameObject);
                    return;
                }
                if (wall)
                {
                    SpellFX.SpawnBurst(transform.position, new Color(0.8f, 0.3f, 1f), new Color(0.3f, 0.1f, 0.5f), 8, 1.5f, 3.5f, 0.07f, 0.14f, 0.25f);
                    Destroy(gameObject);
                    return;
                }
            }
            transform.position = to;
        }

        void OnDestroy() { if (material) Destroy(material); }
    }
}
