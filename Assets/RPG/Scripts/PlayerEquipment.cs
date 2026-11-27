using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    public partial class PlayerController
    {
        readonly List<string> ownedItems = new List<string> { "starter-axe" };
        public IReadOnlyList<string> OwnedItems => ownedItems;
        public string EquippedWeaponId { get; private set; } = "starter-axe";
        public string EquippedArmorId { get; private set; } = "";
        public string EquippedTrinketId { get; private set; } = "";
        public int BoneShards { get; private set; }
        public void SetAuthoritativeBoneShards(int amount) => BoneShards = Mathf.Max(0, amount);
        public void AddGoldDirect(int amount) { Gold += Mathf.Max(0, amount); SaveAdventure(false); }
        public void AddPotionsDirect(int count) { Potions = Mathf.Clamp(Potions + count, 0, 10); SaveAdventure(false); }
        public bool SpendGoldDirect(int amount) { if (Gold < amount) return false; Gold -= amount; SaveAdventure(false); return true; }
        public void AddBoneShardsDirect(int amount) { BoneShards += Mathf.Max(0, amount); SaveAdventure(false); }
        public void RestoreClassResourceDirect(float amount) { ClassResource = Mathf.Min(MaxClassResource, ClassResource + Mathf.Max(0f, amount)); }
        public GearItem Weapon => ItemCatalog.Get(EquippedWeaponId);
        public GearItem Armor => ItemCatalog.Get(EquippedArmorId);
        public GearItem Trinket => ItemCatalog.Get(EquippedTrinketId);
        public int ArmorRating => Armor != null ? Armor.armor : 0;
        public bool OwnsItem(string id) => ownedItems.Contains(id);
        public bool IsEquipped(GearItem item) => item != null && (EquippedWeaponId == item.id || EquippedArmorId == item.id || EquippedTrinketId == item.id);
        public bool GrantItem(string id, bool equipIfBetter = false)
        {
            var item = ItemCatalog.Get(id); if (item == null) return false;
            bool added = !ownedItems.Contains(id); if (added) ownedItems.Add(id);
            if (!Network.CoopSessionConfig.IsMultiplayer)
            {
                var previous = item.slot == GearSlot.Weapon ? Weapon : item.slot == GearSlot.Armor ? Armor : Trinket;
                if (equipIfBetter && (previous == null || GearValue(item) > GearValue(previous))) EquipInternal(item);
            }
            return added;
        }
        static int GearValue(GearItem item) => item.damage * 10 + item.armor * 10 + item.health;
        public void EquipItem(string id)
        {
            var item = ItemCatalog.Get(id);
            if (item == null || !ownedItems.Contains(id) || (Stats && !Stats.Alive) || FrontendMenu.IsOpen || !InventoryOpen || ShopOpen || MapOpen || DialogueOpen || (orbit && orbit.Paused)) return;
            if (InCombat()) { Message = "Leave combat before changing equipment."; return; }
            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                if (Network.RiftboundCoopPlayer.LocalOwner == null) return;
                if (Network.CoopSessionConfig.IsRealmClient && !IsRealmHydrated) return;
                Network.RiftboundCoopPlayer.LocalOwner.RequestEquipItem(id);
                return;
            }
            EquipInternal(item); Message = "Equipped: " + item.name + "."; SaveAdventure(false);
        }
        public void EquipDirect(string id)
        {
            var item = ItemCatalog.Get(id);
            if (item == null) return;
            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                EquipIfOwned(id);
                return;
            }
            if (!ownedItems.Contains(id)) ownedItems.Add(id);
            EquipInternal(item);
            SaveAdventure(false);
        }
        public bool EquipIfOwned(string id)
        {
            var item = ItemCatalog.Get(id);
            if (item == null || !ownedItems.Contains(id) || (Stats && !Stats.Alive)) return false;
            if (Network.CoopSessionConfig.IsMultiplayer)
            {
                if (Network.RiftboundCoopPlayer.LocalOwner == null) return false;
                if (Network.CoopSessionConfig.IsRealmClient && !IsRealmHydrated) return false;
                Network.RiftboundCoopPlayer.LocalOwner.RequestEquipItem(id);
                return true;
            }
            EquipInternal(item);
            SaveAdventure(false);
            return true;
        }
        public void ApplyAuthoritativeEquipmentSlots(string weaponId, string armorId, string trinketId, int serverHp = -1, int serverMaxHp = -1)
        {
            EquippedWeaponId = ValidEquipment(weaponId, GearSlot.Weapon) ? weaponId : "starter-axe";
            EquippedArmorId = ValidEquipment(armorId, GearSlot.Armor) ? armorId : "";
            EquippedTrinketId = ValidEquipment(trinketId, GearSlot.Trinket) ? trinketId : "";
            RefreshEquipmentStats();
            if (Stats && serverMaxHp > 0 && serverHp >= 0)
                Stats.SetReplicatedHealth(serverHp, serverMaxHp);
        }
        public void ApplyAuthorizedEquippedSlot(GearSlot slot, string itemId, int serverHp = -1, int serverMaxHp = -1)
        {
            string clean = string.IsNullOrWhiteSpace(itemId) ? "" : itemId.Trim();
            if (!string.IsNullOrEmpty(clean) && ItemCatalog.Get(clean) != null && !ownedItems.Contains(clean))
                ownedItems.Add(clean);

            if (slot == GearSlot.Weapon)
                EquippedWeaponId = ValidEquipment(clean, GearSlot.Weapon) ? clean : "starter-axe";
            else if (slot == GearSlot.Armor)
                EquippedArmorId = ValidEquipment(clean, GearSlot.Armor) ? clean : "";
            else
                EquippedTrinketId = ValidEquipment(clean, GearSlot.Trinket) ? clean : "";

            RefreshEquipmentStats();
            if (Stats && serverMaxHp > 0 && serverHp >= 0)
                Stats.SetReplicatedHealth(serverHp, serverMaxHp);
            var item = ItemCatalog.Get(clean);
            if (item != null && !FrontendMenu.IsOpen)
                Message = "Equipped: " + item.name + ".";
        }
        void EquipInternal(GearItem item)
        {
            if (item.slot == GearSlot.Weapon) EquippedWeaponId = item.id;
            else if (item.slot == GearSlot.Armor) EquippedArmorId = item.id;
            else EquippedTrinketId = item.id;
            RefreshEquipmentStats();
        }
        public void RefreshEquipmentStats()
        {
            if (!Stats) return;
            int health = (Armor != null ? Armor.health : 0) + (Trinket != null ? Trinket.health : 0);
            int raceHpBonus = CharacterRace == "Brute" ? 25 : 0;
            int raceArmorBonus = CharacterRace == "Undead" ? 3 : 0;
            int gemHp = 0, gemArmor = 0;
            if (GemSocketSystem.Instance != null)
            {
                GemSocketSystem.Instance.GetTotalEquippedGemStats(out _, out _, out gemHp, out gemArmor, out _);
            }
            int levelHealth = (Mathf.Min(Level, 5) - 1) * 20 + Mathf.Max(0, Level - 5) * 5;
            var (specHealth, specArmor) = TalentSystem.GetSpecializationVitals(this);
            Stats.ConfigureVitals(150 + levelHealth + health + raceHpBonus + gemHp + MilestoneHealth + specHealth,
                ArmorRating + raceArmorBonus + gemArmor + MilestoneArmor + specArmor);
            if (Network.CoopSessionConfig.IsMultiplayer &&
                Network.RiftboundCoopPlayer.LocalOwner != null &&
                Network.RiftboundCoopPlayer.LocalOwner.MaxHealth.Value > 0)
            {
                Stats.SetReplicatedHealth(
                    Network.RiftboundCoopPlayer.LocalOwner.Health.Value,
                    Network.RiftboundCoopPlayer.LocalOwner.MaxHealth.Value);
            }
            RefreshHeldWeaponVisual();
            if (!animator || Weapon == null) return;
            var block = new MaterialPropertyBlock();
            Color tint = Weapon.rarity == "Legendary" ? new Color(1.0f, 0.65f, 0.2f) :
                         Weapon.rarity == "Epic" ? new Color(0.85f, 0.45f, 1.0f) :
                         Weapon.rarity == "Rare" ? new Color(0.45f, 0.75f, 1.0f) :
                         Weapon.rarity == "Uncommon" ? new Color(0.45f, 1.0f, 0.5f) : Color.white;

            Transform handSlot = null;
            if (animator.isHuman) handSlot = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (!handSlot)
            {
                foreach (var t in animator.GetComponentsInChildren<Transform>())
                {
                    string tName = t.name.ToLowerInvariant();
                    if (tName == "handslot.r" || tName.EndsWith(":righthand") || tName.Contains("hand.r"))
                    { handSlot = t; break; }
                }
            }

            if (handSlot)
            {
                foreach (var r in handSlot.GetComponentsInChildren<Renderer>())
                {
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        r.GetPropertyBlock(block, i);
                        block.SetColor("_BaseColor", tint);
                        if (Weapon.rarity == "Epic" || Weapon.rarity == "Legendary")
                            block.SetColor("_EmissionColor", tint * 0.45f);
                        r.SetPropertyBlock(block, i);
                    }
                }
            }

            foreach (var renderer in animator.GetComponentsInChildren<Renderer>())
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null) continue;
                    string mName = materials[i].name.ToLowerInvariant();
                    if (mName.Contains("blinn") || mName.Contains("axe") || mName.Contains("weapon") || mName.Contains("blade") || mName.Contains("staff"))
                    {
                        renderer.GetPropertyBlock(block, i);
                        block.SetColor("_BaseColor", tint);
                        if (Weapon.rarity == "Epic" || Weapon.rarity == "Legendary")
                            block.SetColor("_EmissionColor", tint * 0.45f);
                        renderer.SetPropertyBlock(block, i);
                    }
                }
            }
        }

        public void RefreshHeldWeaponVisual()
        {
            string weaponId = !string.IsNullOrEmpty(EquippedWeaponId) ? EquippedWeaponId : "starter-axe";
            var item = ItemCatalog.Get(weaponId);
            string offhand = CharacterClass == "Rogue" && weaponId.IndexOf("dagger", System.StringComparison.OrdinalIgnoreCase) >= 0
                ? "copper-dagger"
                : null;
            foreach (var pav in GetComponentsInChildren<ProceduralActorVisual>(true))
            {
                if (!pav) continue;
                pav.EquipWeaponVisual(weaponId, offhand, item?.rarity);
                pav.SetWeaponVisualVisible(!Mounted);
            }
        }
        string EnemyItem(EnemyController enemy) =>
            enemy == null ? "" : SharedRewardCatalog.GetEnemyDropItemId(enemy.enemyId);

        string ChestItem(string chestId)
        {
            IReadOnlyList<string> itemIds = chestId == WorldBossEncounter.HoardChestId
                ? SharedRewardCatalog.ResolveGorgonaxChestItems(OwnsItem)
                : SharedRewardCatalog.GetChestItemIds(chestId);
            if (itemIds == null || itemIds.Count == 0) return "";
            var grantedNames = new List<string>();
            for (int i = 0; i < itemIds.Count; i++)
            {
                string id = itemIds[i];
                if (GrantItem(id))
                {
                    var item = ItemCatalog.Get(id);
                    if (item != null) grantedNames.Add(item.name);
                }
            }
            return grantedNames.Count > 0 ? string.Join(", ", grantedNames) : "";
        }
        void RestoreEquipment(AdventureSave data)
        {
            ownedItems.Clear(); ownedItems.Add("starter-axe");
            if (data.items != null) foreach (string id in data.items) if (!string.IsNullOrEmpty(id) && !ownedItems.Contains(id)) ownedItems.Add(id);
            // Migration grants earned rewards without changing existing enemy/quest progress.
            if (!data.lootOnCorpses)
            {
                if (data.enemies != null && System.Array.IndexOf(data.enemies, "road-raider-2") >= 0) GrantItem("raider-cleaver");
                if (data.enemies != null && System.Array.IndexOf(data.enemies, "camp-guard-1") >= 0) GrantItem("boneplate");
                if (data.bossDefeated) GrantItem("captain-axe");
                if (data.enemies != null && System.Array.IndexOf(data.enemies, "crypt-warden") >= 0) GrantItem("warden-axe");
            }
            if (data.chests != null) foreach (string id in data.chests) ChestItem(id);
            if (data.weaponUpgraded) { GrantItem("emberwatch-axe"); GrantItem("emberwatch-sigil"); }
            EquippedWeaponId = ValidEquipment(data.weapon, GearSlot.Weapon) ? data.weapon : data.weaponUpgraded ? "emberwatch-axe" : "starter-axe";
            EquippedArmorId = ValidEquipment(data.armor, GearSlot.Armor) ? data.armor : "";
            EquippedTrinketId = ValidEquipment(data.trinket, GearSlot.Trinket) ? data.trinket : data.weaponUpgraded ? "emberwatch-sigil" : "";
            BoneShards = Mathf.Max(0, data.boneShards); RefreshEquipmentStats();
        }
        bool ValidEquipment(string id, GearSlot slot) => ownedItems.Contains(id ?? "") && ItemCatalog.Get(id) != null && ItemCatalog.Get(id).slot == slot;
        public void SellShards()
        {
            if (!CanTrade() || InCombat() || BoneShards <= 0) return;
            if (Network.CoopSessionConfig.IsMultiplayer && Network.RiftboundCoopPlayer.LocalOwner != null)
            {
                Network.RiftboundCoopPlayer.LocalOwner.RequestTransaction("sell_shards");
                return;
            }
            int gold = BoneShards * 2; BoneShards = 0; Gold += gold; Message = $"Sold bone shards for {gold} gold."; SaveAdventure(false);
        }
    }
}
