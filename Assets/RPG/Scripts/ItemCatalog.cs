using UnityEngine;

namespace Riftbound
{
    public enum GearSlot { Weapon, Armor, Trinket }
    public class GearItem
    {
        public readonly string id, name, rarity;
        public readonly GearSlot slot;
        public readonly int damage, armor, health;
        public readonly Color color;
        public GearItem(string id, string name, GearSlot slot, string rarity, int damage, int armor, int health)
        {
            this.id = id; this.name = name; this.slot = slot; this.rarity = rarity;
            this.damage = damage; this.armor = armor; this.health = health;
            color = rarity == "Legendary" ? new Color(1.0f, 0.50f, 0.0f) :
                    rarity == "Epic" ? new Color(0.64f, 0.21f, 0.93f) :
                    rarity == "Rare" ? new Color(0.0f, 0.44f, 0.87f) :
                    rarity == "Uncommon" ? new Color(0.12f, 1.0f, 0.0f) :
                    new Color(0.85f, 0.86f, 0.82f);
        }
        public string Stats => slot == GearSlot.Weapon ? $"+{damage} weapon damage" : slot == GearSlot.Armor ? $"{armor} armor / +{health} maximum HP" : $"+{health} maximum HP";
    }
    public static class ItemCatalog
    {
        public static readonly GearItem[] All = {
            new GearItem("starter-axe", "Brute Axe", GearSlot.Weapon, "Common", 0, 0, 0),
            new GearItem("raider-cleaver", "Raider Cleaver", GearSlot.Weapon, "Uncommon", 5, 0, 0),
            new GearItem("captain-axe", "Captain's Greataxe", GearSlot.Weapon, "Rare", 10, 0, 0),
            new GearItem("emberwatch-axe", "Emberwatch Axe", GearSlot.Weapon, "Rare", 8, 0, 0),
            new GearItem("leather-vest", "Scout's Leather Vest", GearSlot.Armor, "Common", 0, 2, 10),
            new GearItem("boneplate", "Boneguard Plate", GearSlot.Armor, "Uncommon", 0, 4, 25),
            new GearItem("watchtower-cuirass", "Watchtower Cuirass", GearSlot.Armor, "Rare", 0, 5, 30),
            new GearItem("travel-charm", "Traveler's Charm", GearSlot.Trinket, "Uncommon", 0, 0, 20),
            new GearItem("emberwatch-sigil", "Emberwatch Sigil", GearSlot.Trinket, "Rare", 0, 0, 35),
            new GearItem("warden-axe", "Warden's Executioner", GearSlot.Weapon, "Rare", 12, 0, 0),
            new GearItem("crypt-signet", "Signet of the Crypt", GearSlot.Trinket, "Rare", 0, 0, 45),
            new GearItem("scarlet-blade", "Inquisitor's Sunblade", GearSlot.Weapon, "Epic", 16, 0, 20),
            new GearItem("scarlet-aegis", "Aegis of the Scarlet Dawn", GearSlot.Armor, "Epic", 0, 8, 50),
            new GearItem("duskwood-shadow-dagger", "Shadowfang Stiletto", GearSlot.Weapon, "Epic", 18, 0, 10),
            new GearItem("cathedral-relic", "Censer of the Pure Light", GearSlot.Trinket, "Epic", 4, 3, 60),
            new GearItem("bloodrock-waraxe", "Bloodrock Decapitator", GearSlot.Weapon, "Rare", 14, 0, 15),
            new GearItem("ember-drake-scale", "Heart of the Ember Drake", GearSlot.Trinket, "Legendary", 8, 6, 120),
            new GearItem("wyrmtooth-edge", "Wyrmtooth Cleaver", GearSlot.Weapon, "Legendary", 22, 0, 40),
            new GearItem("horde-war-banner", "Bloodrock War Standard", GearSlot.Trinket, "Epic", 5, 4, 75),
            // Blacksmithing Crafted Gear
            new GearItem("copper-dagger", "Sharpened Copper Dagger", GearSlot.Weapon, "Common", 8, 0, 0),
            new GearItem("bronze-broadsword", "Tempered Bronze Broadsword", GearSlot.Weapon, "Uncommon", 16, 0, 15),
            new GearItem("iron-buckler", "Heavy Iron Buckler", GearSlot.Armor, "Rare", 0, 8, 35),
            new GearItem("truesilver-blade", "Truesilver Executioner", GearSlot.Weapon, "Epic", 24, 0, 30),
            // World Boss Gorgonax Epic Drops
            new GearItem("worldbreaker-maul", "Worldbreaker's War-Maul", GearSlot.Weapon, "Epic", 26, 0, 50),
            new GearItem("crown-of-molten-ruin", "Crown of Molten Ruin", GearSlot.Armor, "Epic", 0, 12, 70),
            new GearItem("cinderfury-signet", "Cinderfury Signet", GearSlot.Trinket, "Epic", 6, 4, 65)
        };
        public static GearItem Get(string id)
        {
            foreach (var item in All) if (item.id == id) return item;
            return null;
        }
    }
}
