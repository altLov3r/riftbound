using UnityEngine;
namespace Riftbound
{
    public class Runestone : MonoBehaviour
    {
        public string runeId;
        public Renderer crystal;
        public TextMesh label;
        PlayerController player;
        MaterialPropertyBlock block;
        void Start() { player = FindFirstObjectByType<PlayerController>(); block = new MaterialPropertyBlock(); }
        void Update()
        {
            if (!player || !crystal) return;
            Color color = player.HasRune(runeId) ? new Color(.3f, 1, .8f) : new Color(.3f, .45f, .9f);
            block.SetColor("_BaseColor", color); block.SetColor("_EmissionColor", color * (1.1f + Mathf.Sin(Time.time * 2) * .2f)); crystal.SetPropertyBlock(block);
            if (label) { label.text = player.HasRune(runeId) ? "ACTIVATED" : "RUNESTONE"; if (Camera.main) label.transform.rotation = Camera.main.transform.rotation; }
        }
        public void Activate(PlayerController player)
        {
            if (player && player.Stats.Alive && Vector3.Distance(transform.position, player.transform.position) < 3) player.DiscoverRune(runeId);
        }
    }
}
