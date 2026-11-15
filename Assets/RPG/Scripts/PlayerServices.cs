using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    public partial class PlayerController
    {
        public bool MapOpen { get; set; }
        public bool ShopOpen => ActiveMerchant;
        public Merchant ActiveMerchant { get; private set; }
        public bool TemperedWeapon { get; private set; }
        public int WeaponBonus
        {
            get
            {
                int bonus = (Weapon != null ? Weapon.damage : 0) + (TemperedWeapon ? 4 : 0);
                if (GemSocketSystem.Instance != null)
                {
                    GemSocketSystem.Instance.GetTotalEquippedGemStats(out int gemAp, out _, out _, out _, out _);
                    bonus += gemAp / 2;
                }
                return bonus;
            }
        }
        readonly HashSet<string> openedChests = new HashSet<string>();
        public Merchant NearbyMerchant()
        {
            if (!Stats || !Stats.Alive) return null;
            Merchant result = null; float nearest = 3;
            foreach (var npc in FindObjectsByType<Merchant>(FindObjectsSortMode.None))
            {
                float distance = Vector3.Distance(transform.position, npc.transform.position);
                if (distance < nearest) { result = npc; nearest = distance; }
            }
            return result;
        }
        public bool CanTrade() => ActiveMerchant && Stats.Alive && Vector3.Distance(transform.position, ActiveMerchant.transform.position) < 3;
        public void CloseShop() { ActiveMerchant = null; }
        public bool InCombat()
        {
            if (Stats && Time.time - Stats.LastHit < 5) return true;
            foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                if (!enemy.trainingDummy && enemy.GetComponent<Combatant>().Alive && Vector3.Distance(transform.position, enemy.transform.position) < 12) return true;
            return false;
        }
        void Interact()
        {
            if (Time.time < busyUntil || Blocking) return;
            if (LootDropSystem.Instance)
            {
                var corpse = LootDropSystem.Instance.GetNearbyCorpse(transform.position, 3.5f);
                if (corpse != null)
                {
                    if (LootDropSystem.Instance.CurrentLooting != null)
                        LootDropSystem.Instance.LootAll(LootDropSystem.Instance.CurrentLooting);
                    else
                        LootDropSystem.Instance.LootAll(corpse);
                    return;
                }
            }
            if (Mailbox.GetNearby(transform.position, 3.2f) is var mb && mb != null)
            {
                AuctionHouseSystem.MailboxOpen = !AuctionHouseSystem.MailboxOpen;
                AuctionHouseSystem.AuctionOpen = false;
                moveVelocity = Vector3.zero;
                return;
            }
            if (TownNPC.FindNearby(transform.position, 3.2f) is var npcNear && npcNear != null && npcNear.roleTag == "Auctioneer")
            {
                AuctionHouseSystem.AuctionOpen = !AuctionHouseSystem.AuctionOpen;
                AuctionHouseSystem.MailboxOpen = false;
                moveVelocity = Vector3.zero;
                return;
            }
            if (FlightMaster.GetNearby(transform.position, 3.5f) is var flightMaster && flightMaster != null)
            {
                if (InCombat()) { Message = "Cannot take flight while in combat."; return; }
                FlightMaster.OpenTaxiWindow(flightMaster);
                return;
            }
            if (GatheringNode.GetNearby(transform.position, 3.2f) is var gatherNode && gatherNode != null)
            {
                if (InCombat()) { Message = "Cannot gather while in combat."; return; }
                gatherNode.StartGather(this);
                return;
            }
            var merchant = NearbyMerchant();
            if (merchant && (!CanTalk() || Vector3.Distance(transform.position, merchant.transform.position) < Vector3.Distance(transform.position, questGiver.position)))
            {
                if (InCombat()) { Message = "Leave combat before trading."; return; }
                ActiveMerchant = merchant; InventoryOpen = false; moveVelocity = Vector3.zero; return;
            }
            if (CanTalk())
            {
                ActiveQuestGiver = questGiver.GetComponent<QuestGiver>();
                if (ActiveQuestGiver) { DialogueOpen = true; dialogueOpenedAt = Time.time; moveVelocity = Vector3.zero; }
                return;
            }
            var landmark = NearbyLandmark(); if (landmark) { landmark.Activate(this); return; }
            var rune = NearbyRune(); if (rune) { rune.Activate(this); return; }
            var supply = NearbySupply(); if (supply) { supply.Collect(this); return; }
            var chest = NearbyChest(); if (chest) { chest.Open(this); return; }
            var townNpc = TownNPC.FindNearby(transform.position, 3.2f);
            if (townNpc) { OpenTownNPCDialogue(townNpc); return; }
        }
        public LootChest NearbyChest()
        {
            LootChest result = null; float nearest = 3;
            foreach (var chest in FindObjectsByType<LootChest>(FindObjectsSortMode.None))
            {
                float distance = Vector3.Distance(transform.position, chest.transform.position);
                if (!HasOpenedChest(chest.chestId) && distance < nearest) { result = chest; nearest = distance; }
            }
            return result;
        }
        public bool HasOpenedChest(string id) => openedChests.Contains(id);
        public bool OpenChest(LootChest chest)
        {
            if (chest && chest.chestId == RiverReaverCamp.CacheId && !HasDefeated(RiverReaverCamp.CaptainId))
            { Message = "Defeat Captain Selvek at the River Reaver camp to unlock this lockbox."; return false; }
            if (chest && chest.requiresCaptainDefeated && !BossDefeated) { Message = "Defeat the captain to unlock this cache."; return false; }
            if (chest && chest.requiresWardenDefeated && !HasDefeated("crypt-warden")) { Message = "Defeat the Crypt Warden to unlock the reliquary."; return false; }
            if (!chest || !Stats.Alive || FrontendMenu.IsOpen || DialogueOpen || ShopOpen || MapOpen || JournalOpen || (orbit && orbit.Paused) || Vector3.Distance(transform.position, chest.transform.position) >= 3 || string.IsNullOrEmpty(chest.chestId) || HasOpenedChest(chest.chestId)) return false;
            if (Network.CoopSessionConfig.IsMultiplayer && Network.RiftboundCoopPlayer.LocalOwner != null)
            {
                if (!Network.RiftboundCoopPlayer.LocalOwner.RequestOpenChestServerValidated(chest)) return false;
            }
            return ApplyAuthorizedChestOpen(chest);
        }

        public bool ApplyAuthorizedChestOpen(LootChest chest)
        {
            if (!chest || string.IsNullOrEmpty(chest.chestId) || !openedChests.Add(chest.chestId)) return false;
            Gold += chest.gold; Potions = Mathf.Min(10, Potions + 1); AddExperience(chest.experience);
            string item = ChestItem(chest.chestId);
            Message = $"Cache recovered: {chest.gold} gold, {chest.experience} XP." + (item.Length > 0 ? " New item: " + item + " [I]" : ""); SaveAdventure(false); return true;
        }
        public void SetAuthoritativeGold(int value) => Gold = Mathf.Max(0, value);
        public void BuyPotion()
        {
            if (!CanTrade() || InCombat() || Gold < 10 || Potions >= 10) return;
            if (Network.CoopSessionConfig.IsMultiplayer && Network.RiftboundCoopPlayer.LocalOwner != null)
            {
                Network.RiftboundCoopPlayer.LocalOwner.RequestTransaction("buy_potion");
                return;
            }
            Gold -= 10; Potions++; Message = "Purchased a health potion."; SaveAdventure(false);
        }
        public void TemperWeapon()
        {
            if (!CanTrade() || InCombat() || Gold < 45 || TemperedWeapon) return;
            if (Network.CoopSessionConfig.IsMultiplayer && Network.RiftboundCoopPlayer.LocalOwner != null)
            {
                Network.RiftboundCoopPlayer.LocalOwner.RequestTransaction("temper_weapon");
                return;
            }
            Gold -= 45; TemperedWeapon = true; Message = "Weapon tempered: +4 damage."; SaveAdventure(false);
        }
        public void RestAtMerchant()
        {
            if (!CanTrade() || InCombat() || Gold < 5 || Stats.Health >= Stats.maxHealth) return;
            if (Network.CoopSessionConfig.IsMultiplayer && Network.RiftboundCoopPlayer.LocalOwner != null)
            {
                Network.RiftboundCoopPlayer.LocalOwner.RequestTransaction("rest");
                return;
            }
            Gold -= 5; Stats.Heal(Stats.maxHealth); Message = "Rested at Emberwatch."; SaveAdventure(false);
        }
        public void ApplyAuthorizedTransaction(string txId, int newGold)
        {
            Gold = Mathf.Max(0, newGold);
            if (txId == "buy_potion") { Potions = Mathf.Min(10, Potions + 1); Message = "Purchased a health potion."; }
            else if (txId == "temper_weapon") { TemperedWeapon = true; Message = "Weapon tempered: +4 damage."; }
            else if (txId == "rest") { Stats.Heal(Stats.maxHealth); Message = "Rested at Emberwatch."; }
        }
    }
}
