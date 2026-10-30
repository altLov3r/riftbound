using System.Collections;
using UnityEngine;

namespace Riftbound
{
    public class FireballSpell : MonoBehaviour
    {
        Combatant target;
        PlayerController caster;
        float damage;
        Vector3 targetLastPos;
        float speed = 22f;
        float life = 3.5f;

        public static void Cast(PlayerController player, Combatant enemy, float dmg)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Fireball";
            var col = go.GetComponent<Collider>();
            if (col) Destroy(col);

            go.transform.position = player.transform.position + Vector3.up * 1.4f + player.transform.forward * 0.8f;
            go.transform.localScale = Vector3.one * 0.45f;

            var r = go.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", new Color(1f, 0.45f, 0.08f));
            r.sharedMaterial = mat;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.15f);
            light.range = 5f;
            light.intensity = 2.5f;

            var spell = go.AddComponent<FireballSpell>();
            spell.caster = player;
            spell.target = enemy;
            spell.damage = dmg;
            spell.targetLastPos = enemy ? enemy.transform.position + Vector3.up : go.transform.position + player.transform.forward * 20f;
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0) { Destroy(gameObject); return; }

            Vector3 dest = (target && target.Alive) ? target.transform.position + Vector3.up * 1.2f : targetLastPos;
            Vector3 diff = dest - transform.position;
            float step = speed * Time.deltaTime;

            if (diff.magnitude <= step + 0.3f)
            {
                Impact();
                return;
            }

            transform.position += diff.normalized * step;
        }

        void Impact()
        {
            if (target && target.Alive)
            {
                target.Damage(damage);
            }
            SpawnExplosion(transform.position);
            Destroy(gameObject);
        }

        void SpawnExplosion(Vector3 pos)
        {
            var burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            burst.name = "FireballBurst";
            var c = burst.GetComponent<Collider>();
            if (c) Destroy(c);
            burst.transform.position = pos;
            burst.transform.localScale = Vector3.one * 1.2f;

            var r = burst.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", new Color(1f, 0.7f, 0.2f, 0.8f));
            r.sharedMaterial = mat;

            Destroy(burst, 0.18f);
        }
    }

    public static class SpellFX
    {
        public static void CreateFrostNova(Vector3 center, float radius)
        {
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "FrostNovaRing";
            var c = ring.GetComponent<Collider>();
            if (c) Object.Destroy(c);

            ring.transform.position = center + Vector3.up * 0.05f;
            ring.transform.localScale = new Vector3(radius * 2, 0.02f, radius * 2);

            var r = ring.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", new Color(0.25f, 0.85f, 1f, 0.65f));
            r.sharedMaterial = mat;

            Object.Destroy(ring, 0.6f);
        }

        public static void CreateConsecration(Vector3 center, float radius)
        {
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ConsecrationCircle";
            var c = ring.GetComponent<Collider>();
            if (c) Object.Destroy(c);

            ring.transform.position = center + Vector3.up * 0.05f;
            ring.transform.localScale = new Vector3(radius * 2, 0.03f, radius * 2);

            var r = ring.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", new Color(1f, 0.85f, 0.25f, 0.75f));
            r.sharedMaterial = mat;

            Object.Destroy(ring, 1.2f);
        }
    }
}
