using UnityEngine;

namespace Riftbound
{
    /// <summary>
    /// Spawns Expedition Herald Veyra at the Emberwatch north road junction (46, -24)
    /// so every player immediately has a gold quest breadcrumb leading out across
    /// Whisperwood, Pinewatch, Ashenmaw Cavern, Sunstone Abbey, Cinderroad, Ironmaw, and Thornwatch.
    /// </summary>
    public static class FrontierExpeditionHerald
    {
        public const string RootName = "Frontier Expedition Herald";
        public const string NpcName = "Expedition Herald Veyra";
        public static readonly Vector3 Position = new Vector3(46f, 0f, -24f);

        public static void Ensure()
        {
            if (!WorldRegions.Expanded || GameObject.Find(RootName)) return;

            var root = new GameObject(RootName).transform;
            Vector3 g = Ground(Position);

            var npcGo = new GameObject(NpcName);
            npcGo.transform.SetParent(root, false);
            npcGo.transform.position = g;
            npcGo.transform.rotation = Quaternion.Euler(0f, 195f, 0f);
            CharacterVisualFactory.CreateVisualInstance("Elf", "Mage", npcGo.transform);
            var col = npcGo.AddComponent<CapsuleCollider>();
            col.height = 1.90f;
            col.radius = 0.36f;
            col.center = Vector3.up * 0.95f;
            var npc = npcGo.AddComponent<TownNPC>();
            npc.npcName = NpcName;
            npc.roleTag = "Frontier Expedition";
            npc.greeting = "Reports from the outer settlements are pouring in! Take my dispatch north along the Greenhollow road to Whisperwood Exchange, and help every frontier town along the circuit.";
        }

        static Vector3 Ground(Vector3 xz)
        {
            var hits = Physics.RaycastAll(new Vector3(xz.x, 260f, xz.z), Vector3.down, 420f);
            float best = float.MinValue;
            bool found = false;
            foreach (var h in hits)
            {
                if (h.collider is TerrainCollider && h.point.y > best) { best = h.point.y; found = true; }
            }
            xz.y = (found ? best : WorldRegions.GroundHeight(xz)) + 0.03f;
            return xz;
        }
    }
}
