using System;
using System.Collections.Generic;

namespace Riftbound
{
    /// <summary>
    /// Headless-server-safe authoritative mapping from chest IDs and enemy IDs to gear item IDs.
    /// Shared across offline solo, co-op clients, and dedicated server (Assembly-CSharp).
    /// </summary>
    public static class SharedRewardCatalog
    {
        static readonly string[] Empty = Array.Empty<string>();

        static readonly Dictionary<string, string[]> ChestRewards = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            // 6 R3 Target Chests
            { "pinewatch-boss-chest", new[] { "pinewatch-warden-crest" } },
            { "ashenmaw-guardian-chest", new[] { "ursolak-spirit-pelt" } },
            { "sunstone-abbey-chest", new[] { "sunforged-breastplate" } },
            { "ironmaw-vault-chest", new[] { "ironmaw-cleaver" } },
            { "river-reaver-lockbox", new[] { "riverwarden-signet" } },
            { "worldboss-gorgonax-chest", new[] { "crown-of-molten-ruin", "cinderfury-signet" } },
            // Scene / Progression Chests
            { "cathedral-altar-chest", new[] { "cathedral-relic" } },
            { "wyrmcrest-hoard-chest", new[] { "ember-drake-scale" } },
            { "crypt-reliquary", new[] { "crypt-signet" } },
            { "village-cache", new[] { "leather-vest" } },
            { "camp-cache", new[] { "travel-charm" } },
            { "tower-cache", new[] { "watchtower-cuirass" } },
        };

        static readonly Dictionary<string, string[]> EnemyDrops = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { "vermithrax-boss", new[] { "wyrmtooth-edge" } },
            { "redridge-outcast-boss", new[] { "bloodrock-waraxe" } },
            { "boss-abbot-vael", new[] { "scarlet-aegis", "duskwood-shadow-dagger" } },
            { "boss-malakar", new[] { "scarlet-blade" } },
            { "crypt-warden", new[] { "warden-axe" } },
            { "ashwood-captain", new[] { "captain-axe" } },
            { "road-raider-2", new[] { "raider-cleaver" } },
            { "camp-guard-1", new[] { "boneplate" } },
        };

        public static IReadOnlyList<string> GetChestItemIds(string chestId)
        {
            if (string.IsNullOrEmpty(chestId)) return Empty;
            return ChestRewards.TryGetValue(chestId, out var ids) ? ids : Empty;
        }

        public static string GetEnemyDropItemId(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return "";
            return EnemyDrops.TryGetValue(enemyId, out var ids) && ids.Length > 0 ? ids[0] : "";
        }

        public static IReadOnlyList<string> GetEnemyDropItemIds(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return Empty;
            return EnemyDrops.TryGetValue(enemyId, out var ids) ? ids : Empty;
        }

        public static readonly string[] GorgonaxEpicIds =
        {
            "worldbreaker-maul",
            "crown-of-molten-ruin",
            "cinderfury-signet"
        };

        public static string ResolveGorgonaxKillItem(Func<string, bool> ownsItem)
        {
            for (int i = 0; i < GorgonaxEpicIds.Length; i++)
            {
                string id = GorgonaxEpicIds[i];
                if (ownsItem == null || !ownsItem(id))
                    return id;
            }
            return GorgonaxEpicIds[0];
        }

        public static List<string> ResolveGorgonaxChestItems(Func<string, bool> ownsItem)
        {
            var result = new List<string>(GorgonaxEpicIds.Length);
            for (int i = 0; i < GorgonaxEpicIds.Length; i++)
            {
                string id = GorgonaxEpicIds[i];
                if (ownsItem == null || !ownsItem(id))
                    result.Add(id);
            }
            return result;
        }
    }
}
