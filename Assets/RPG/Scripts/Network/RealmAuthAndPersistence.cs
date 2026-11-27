using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Riftbound.Network
{
    /// <summary>
    /// Server-enforced role hierarchy for Emberford Dedicated Realm (Milestone R2).
    /// Clients never self-assign roles; all checks happen server-side from accounts.json.
    /// </summary>
    public enum RealmRole
    {
        Player = 0,
        Guide = 1,
        Moderator = 2,
        GameMaster = 3,
        RealmAdmin = 4
    }

    public enum RealmStaffAction
    {
        AnnounceHelp = 10,      // Guide+
        InspectPlayer = 11,     // Guide+
        MutePlayer = 20,        // Moderator+
        KickPlayer = 21,        // Moderator+
        TeleportPlayer = 30,    // GameMaster+
        ResetBoss = 31,         // GameMaster+
        GrantReward = 32,       // GameMaster+
        SetRole = 40,           // RealmAdmin
        ShutdownRealm = 41      // RealmAdmin
    }

    [Serializable]
    public sealed class RealmAccountRecord
    {
        public string accountId = "";
        public string displayName = "";
        public string salt = "";
        public string tokenHash = "";
        public RealmRole role = RealmRole.Player;
        public bool isBanned;
        public string createdUtc = "";
    }

    [Serializable]
    sealed class RealmAccountStoreFile
    {
        public List<RealmAccountRecord> accounts = new List<RealmAccountRecord>();
    }

    [Serializable]
    public sealed class ServerCharacterSave
    {
        public string accountId = "";
        public string characterName = "";
        public string race = "Human";
        public string className = "Warrior";
        public string faction = "Alliance";
        public int level = 1;
        public int experience;
        public int gold;
        public int health = 100;
        public int maxHealth = 100;
        public float posX;
        public float posY;
        public float posZ;
        public List<string> claimedChests = new List<string>();
        public List<string> claimedBossKills = new List<string>();
        public List<string> claimedQuests = new List<string>();
        public string lastSavedUtc = "";
        public List<string> ownedItems = new List<string> { "starter-axe" };
        public string equippedWeaponId = "starter-axe";
        public string equippedArmorId = "";
        public string equippedTrinketId = "";
        public int potions = 3;
        public bool temperedWeapon;
        public int boneShards;
        public int questIndex;
        public bool questAccepted;
        public int questKills;
        public List<string> discoveredLandmarks = new List<string>();
        public List<string> defeatedEnemies = new List<string>();
        public string activeTitle = "";
        public int alchemySkill = ProfessionSystem.DefaultAlchemySkill;
        public int blacksmithingSkill = ProfessionSystem.DefaultBlacksmithingSkill;
        public int engineeringSkill = ProfessionSystem.DefaultEngineeringSkill;
        public bool professionInitialized;
        public List<string> reagents = new List<string>();
    }

    [Serializable]
    public sealed class RealmAuditEntry
    {
        public string timestampUtc = "";
        public string actorAccount = "";
        public string actorRole = "";
        public string action = "";
        public string target = "";
        public string result = "";
        public string detail = "";
    }

    /// <summary>
    /// Authoritative server-side authentication, durable character saves (surviving server restarts
    /// with zero duplicate rewards), 5-tier role enforcement, and append-only JSONL audit logging.
    /// Stores all files strictly inside the realm's configurable data directory, never touching offline saves.
    /// </summary>
    public static class RealmPersistenceService
    {
        static readonly Dictionary<string, RealmAccountRecord> Accounts = new Dictionary<string, RealmAccountRecord>(StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, ServerCharacterSave> CharacterCache = new Dictionary<string, ServerCharacterSave>(StringComparer.OrdinalIgnoreCase);

        public static bool IsInitialized { get; private set; }
        public static string DataDirectory { get; private set; } = "";
        public static string AccountsFilePath => string.IsNullOrEmpty(DataDirectory) ? "" : Path.Combine(DataDirectory, "accounts.json");
        public static string CharactersDirectory => string.IsNullOrEmpty(DataDirectory) ? "" : Path.Combine(DataDirectory, "characters");
        public static string AuditLogPath => string.IsNullOrEmpty(DataDirectory) ? "" : Path.Combine(DataDirectory, "audit.jsonl");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            ShutdownForTest();
        }

        public static void ShutdownForTest()
        {
            Accounts.Clear();
            CharacterCache.Clear();
            IsInitialized = false;
            DataDirectory = "";
        }

        public static void Initialize(string dataDirectory)
        {
            if (string.IsNullOrWhiteSpace(dataDirectory))
                throw new ArgumentException("Realm data directory cannot be empty.", nameof(dataDirectory));

            Accounts.Clear();
            CharacterCache.Clear();

            DataDirectory = Path.GetFullPath(dataDirectory.Trim());
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(CharactersDirectory);

            LoadAccountsFromDisk();
            IsInitialized = true;
            AppendAudit("SYSTEM", RealmRole.RealmAdmin, "SERVER_INIT", DataDirectory, "ALLOWED", $"accounts={Accounts.Count}");
        }

        // =========================================================================
        // 1. AUTHENTICATED REALM ENTRY
        // =========================================================================
        public static RealmAccountRecord RegisterOrUpdateAccount(string accountId, string rawToken, RealmRole role = RealmRole.Player, bool isBanned = false, string displayName = null)
        {
            EnsureInitialized();
            string cleanId = SanitizeKey(accountId);
            if (string.IsNullOrEmpty(cleanId)) throw new ArgumentException("Invalid accountId.", nameof(accountId));
            if (string.IsNullOrEmpty(rawToken)) throw new ArgumentException("Auth token cannot be empty.", nameof(rawToken));

            string salt = Guid.NewGuid().ToString("N");
            var record = new RealmAccountRecord
            {
                accountId = cleanId,
                displayName = string.IsNullOrWhiteSpace(displayName) ? cleanId : displayName.Trim(),
                salt = salt,
                tokenHash = HashSecret(salt, rawToken),
                role = role,
                isBanned = isBanned,
                createdUtc = DateTime.UtcNow.ToString("O")
            };

            Accounts[cleanId] = record;
            SaveAccountsToDisk();
            AppendAudit("SYSTEM", RealmRole.RealmAdmin, "ACCOUNT_REGISTER", cleanId, "ALLOWED", $"role={role}, banned={isBanned}");
            return record;
        }

        public static bool Authenticate(string accountId, string rawToken, bool requireExistingAccount, out RealmAccountRecord account, out string error)
        {
            EnsureInitialized();
            account = null;
            string cleanId = SanitizeKey(accountId);
            if (string.IsNullOrEmpty(cleanId) || string.IsNullOrEmpty(rawToken))
            {
                error = "Missing account ID or auth token.";
                AppendAudit(cleanId, RealmRole.Player, "AUTH_LOGIN", cleanId, "DENIED", error);
                return false;
            }

            if (!Accounts.TryGetValue(cleanId, out var existing))
            {
                if (requireExistingAccount)
                {
                    error = "Unknown realm account.";
                    AppendAudit(cleanId, RealmRole.Player, "AUTH_LOGIN", cleanId, "DENIED", error);
                    return false;
                }
                existing = RegisterOrUpdateAccount(cleanId, rawToken, RealmRole.Player);
            }

            if (existing.isBanned)
            {
                error = "Account is banned from this realm.";
                AppendAudit(cleanId, existing.role, "AUTH_LOGIN", cleanId, "DENIED", error);
                return false;
            }

            string computed = HashSecret(existing.salt, rawToken);
            if (!FixedTimeEquals(computed, existing.tokenHash))
            {
                error = "Invalid realm credentials.";
                AppendAudit(cleanId, existing.role, "AUTH_LOGIN", cleanId, "DENIED", error);
                return false;
            }

            account = existing;
            error = "";
            AppendAudit(cleanId, existing.role, "AUTH_LOGIN", cleanId, "ALLOWED", $"role={existing.role}");
            return true;
        }

        public static RealmRole GetRole(string accountId)
        {
            string cleanId = SanitizeKey(accountId);
            return Accounts.TryGetValue(cleanId, out var rec) ? rec.role : RealmRole.Player;
        }

        // =========================================================================
        // 2. SERVER-OWNED CHARACTER SAVES & ANTI-DUPLICATION REWARD GATES
        // =========================================================================
        public static ServerCharacterSave LoadOrCreateCharacter(string accountId, string characterName, string race = "Human", string className = "Warrior", string faction = "Alliance")
        {
            EnsureInitialized();
            string key = CharacterKey(accountId, characterName);
            if (CharacterCache.TryGetValue(key, out var cached))
                return cached;

            string path = CharacterFilePath(accountId, characterName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                var loaded = JsonUtility.FromJson<ServerCharacterSave>(json);
                if (loaded != null)
                {
                    bool migrated = NormalizeCharacterSave(loaded);
                    CharacterCache[key] = loaded;
                    if (migrated)
                        SaveCharacter(loaded);
                    return loaded;
                }
            }

            var created = new ServerCharacterSave
            {
                accountId = SanitizeKey(accountId),
                characterName = string.IsNullOrWhiteSpace(characterName) ? "Adventurer" : characterName.Trim(),
                race = string.IsNullOrWhiteSpace(race) ? "Human" : race.Trim(),
                className = string.IsNullOrWhiteSpace(className) ? "Warrior" : className.Trim(),
                faction = string.IsNullOrWhiteSpace(faction) ? "Alliance" : faction.Trim(),
                level = 1,
                experience = 0,
                gold = 0,
                health = 100,
                maxHealth = 100,
                alchemySkill = ProfessionSystem.DefaultAlchemySkill,
                blacksmithingSkill = ProfessionSystem.DefaultBlacksmithingSkill,
                engineeringSkill = ProfessionSystem.DefaultEngineeringSkill,
                professionInitialized = true,
                reagents = ProfessionSystem.BuildDefaultReagentSaveList(),
                lastSavedUtc = DateTime.UtcNow.ToString("O")
            };
            NormalizeCharacterSave(created);
            CharacterCache[key] = created;
            SaveCharacter(created);
            return created;
        }

        public static bool TryGetAccount(string accountId, out RealmAccountRecord account)
        {
            EnsureInitialized();
            string cleanId = SanitizeKey(accountId);
            if (string.IsNullOrEmpty(cleanId))
            {
                account = null;
                return false;
            }
            return Accounts.TryGetValue(cleanId, out account);
        }

        public static bool TryLoadExistingCharacter(string accountId, string characterName, out ServerCharacterSave save)
        {
            EnsureInitialized();
            string key = CharacterKey(accountId, characterName);
            if (CharacterCache.TryGetValue(key, out save) && save != null)
                return true;

            string path = CharacterFilePath(accountId, characterName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                var loaded = JsonUtility.FromJson<ServerCharacterSave>(json);
                if (loaded != null)
                {
                    bool migrated = NormalizeCharacterSave(loaded);
                    CharacterCache[key] = loaded;
                    if (migrated)
                        SaveCharacter(loaded);
                    save = loaded;
                    return true;
                }
            }
            save = null;
            return false;
        }

        public static bool GrantItem(string accountId, string characterName, string itemId, bool saveNow = true)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            string cleanId = itemId.Trim();
            if (ItemCatalog.Get(cleanId) == null) return false;

            var save = LoadOrCreateCharacter(accountId, characterName);
            NormalizeCharacterSave(save);
            if (save.ownedItems.Contains(cleanId)) return false;

            save.ownedItems.Add(cleanId);
            if (saveNow) SaveCharacter(save);
            return true;
        }

        public static bool TryEquipItem(string accountId, string characterName, string itemId, out GearSlot slot, out string error)
        {
            slot = GearSlot.Weapon;
            error = "";
            if (string.IsNullOrWhiteSpace(itemId))
            {
                error = "Empty item ID.";
                return false;
            }
            string cleanId = itemId.Trim();
            var item = ItemCatalog.Get(cleanId);
            if (item == null)
            {
                error = $"Unknown item '{cleanId}'.";
                return false;
            }
            slot = item.slot;

            var save = LoadOrCreateCharacter(accountId, characterName);
            NormalizeCharacterSave(save);
            if (!save.ownedItems.Contains(cleanId))
            {
                error = $"Character does not own '{cleanId}'.";
                return false;
            }

            switch (item.slot)
            {
                case GearSlot.Weapon:
                    save.equippedWeaponId = cleanId;
                    break;
                case GearSlot.Armor:
                    save.equippedArmorId = cleanId;
                    break;
                case GearSlot.Trinket:
                    save.equippedTrinketId = cleanId;
                    break;
            }
            SaveCharacter(save);
            return true;
        }

        public static bool NormalizeCharacterSave(ServerCharacterSave save)
        {
            if (save == null) return false;
            bool migrated = false;

            string prevAcct = save.accountId;
            string prevName = save.characterName;
            string prevRace = save.race;
            string prevCls = save.className;
            string prevFaction = save.faction;
            string prevWeapon = save.equippedWeaponId;
            string prevArmor = save.equippedArmorId;
            string prevTrinket = save.equippedTrinketId;

            save.accountId = SanitizeKey(save.accountId);
            save.characterName = string.IsNullOrWhiteSpace(save.characterName) ? "Adventurer" : save.characterName.Trim();
            save.race = string.IsNullOrWhiteSpace(save.race) ? "Human" : save.race.Trim();
            save.className = string.IsNullOrWhiteSpace(save.className) ? "Warrior" : save.className.Trim();
            save.faction = string.IsNullOrWhiteSpace(save.faction) ? "Alliance" : save.faction.Trim();
            save.lastSavedUtc = string.IsNullOrWhiteSpace(save.lastSavedUtc) ? "" : save.lastSavedUtc.Trim();
            save.activeTitle = string.IsNullOrWhiteSpace(save.activeTitle) ? "" : save.activeTitle.Trim();
            save.equippedWeaponId = string.IsNullOrWhiteSpace(save.equippedWeaponId) ? "starter-axe" : save.equippedWeaponId.Trim();
            save.equippedArmorId = string.IsNullOrWhiteSpace(save.equippedArmorId) ? "" : save.equippedArmorId.Trim();
            save.equippedTrinketId = string.IsNullOrWhiteSpace(save.equippedTrinketId) ? "" : save.equippedTrinketId.Trim();

            if (!string.Equals(prevAcct ?? "", save.accountId, StringComparison.Ordinal) ||
                !string.Equals(prevName ?? "", save.characterName, StringComparison.Ordinal) ||
                !string.Equals(prevRace ?? "", save.race, StringComparison.Ordinal) ||
                !string.Equals(prevCls ?? "", save.className, StringComparison.Ordinal) ||
                !string.Equals(prevFaction ?? "", save.faction, StringComparison.Ordinal) ||
                !string.Equals(prevWeapon ?? "", save.equippedWeaponId, StringComparison.Ordinal) ||
                !string.Equals(prevArmor ?? "", save.equippedArmorId, StringComparison.Ordinal) ||
                !string.Equals(prevTrinket ?? "", save.equippedTrinketId, StringComparison.Ordinal))
                migrated = true;

            save.claimedChests = DedupeList(save.claimedChests, false, ref migrated);
            save.claimedBossKills = DedupeList(save.claimedBossKills, false, ref migrated);
            save.claimedQuests = DedupeList(save.claimedQuests, false, ref migrated);
            save.ownedItems = DedupeList(save.ownedItems, true, ref migrated);
            save.discoveredLandmarks = DedupeList(save.discoveredLandmarks, false, ref migrated);
            save.defeatedEnemies = DedupeList(save.defeatedEnemies, false, ref migrated);

            if (!save.ownedItems.Contains("starter-axe"))
            {
                save.ownedItems.Insert(0, "starter-axe");
                migrated = true;
            }

            int clampedLevel = Mathf.Max(1, save.level);
            if (clampedLevel != save.level) { save.level = clampedLevel; migrated = true; }
            int clampedXp = Mathf.Max(0, save.experience);
            if (clampedXp != save.experience) { save.experience = clampedXp; migrated = true; }
            int clampedGold = Mathf.Max(0, save.gold);
            if (clampedGold != save.gold) { save.gold = clampedGold; migrated = true; }
            int clampedMaxHp = Mathf.Max(100, save.maxHealth);
            if (clampedMaxHp != save.maxHealth) { save.maxHealth = clampedMaxHp; migrated = true; }
            int clampedHp = Mathf.Clamp(save.health, 0, save.maxHealth);
            if (clampedHp != save.health) { save.health = clampedHp; migrated = true; }
            int clampedPotions = Mathf.Clamp(save.potions, 0, 10);
            if (clampedPotions != save.potions) { save.potions = clampedPotions; migrated = true; }
            int clampedShards = Mathf.Max(0, save.boneShards);
            if (clampedShards != save.boneShards) { save.boneShards = clampedShards; migrated = true; }
            int clampedQuestIdx = Mathf.Clamp(save.questIndex, 0, 3);
            if (clampedQuestIdx != save.questIndex) { save.questIndex = clampedQuestIdx; migrated = true; }
            int clampedQuestKills = Mathf.Max(0, save.questKills);
            if (clampedQuestKills != save.questKills) { save.questKills = clampedQuestKills; migrated = true; }
            if (save.questIndex >= 3 && save.questAccepted) { save.questAccepted = false; migrated = true; }

            if (!save.professionInitialized)
            {
                save.professionInitialized = true;
                if (save.alchemySkill <= 0 && save.blacksmithingSkill <= 0 && save.engineeringSkill <= 0)
                {
                    save.alchemySkill = ProfessionSystem.DefaultAlchemySkill;
                    save.blacksmithingSkill = ProfessionSystem.DefaultBlacksmithingSkill;
                    save.engineeringSkill = ProfessionSystem.DefaultEngineeringSkill;
                }
                if (save.reagents == null || save.reagents.Count == 0)
                    save.reagents = ProfessionSystem.BuildDefaultReagentSaveList();
                migrated = true;
            }

            int clampedAlchemy = Mathf.Clamp(save.alchemySkill, 0, ProfessionSystem.MaxSkill);
            if (clampedAlchemy != save.alchemySkill) { save.alchemySkill = clampedAlchemy; migrated = true; }
            int clampedBlacksmith = Mathf.Clamp(save.blacksmithingSkill, 0, ProfessionSystem.MaxSkill);
            if (clampedBlacksmith != save.blacksmithingSkill) { save.blacksmithingSkill = clampedBlacksmith; migrated = true; }
            int clampedEng = Mathf.Clamp(save.engineeringSkill, 0, ProfessionSystem.MaxSkill);
            if (clampedEng != save.engineeringSkill) { save.engineeringSkill = clampedEng; migrated = true; }

            var normalizedReagents = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            bool reagentsDirty = save.reagents == null;
            if (save.reagents != null)
            {
                for (int i = 0; i < save.reagents.Count; i++)
                {
                    string rawEntry = save.reagents[i];
                    if (!ProfessionSystem.TryParseReagentEntry(rawEntry, out string rName, out int rCount) || rCount <= 0)
                    {
                        reagentsDirty = true;
                        continue;
                    }
                    string canonical = $"{rName}={rCount}";
                    if (!string.Equals(rawEntry, canonical, StringComparison.Ordinal))
                        reagentsDirty = true;
                    if (normalizedReagents.ContainsKey(rName))
                    {
                        normalizedReagents[rName] = Mathf.Clamp(normalizedReagents[rName] + rCount, 0, 9999);
                        reagentsDirty = true;
                    }
                    else
                    {
                        normalizedReagents[rName] = rCount;
                    }
                }
            }
            if (reagentsDirty)
            {
                var rebuilt = new List<string>(normalizedReagents.Count);
                foreach (var kv in normalizedReagents)
                    rebuilt.Add($"{kv.Key}={kv.Value}");
                save.reagents = rebuilt;
                migrated = true;
            }

            if (float.IsNaN(save.posX) || float.IsInfinity(save.posX) ||
                float.IsNaN(save.posY) || float.IsInfinity(save.posY) ||
                float.IsNaN(save.posZ) || float.IsInfinity(save.posZ))
            {
                save.posX = 0f;
                save.posY = 0f;
                save.posZ = 0f;
                migrated = true;
            }

            if (save.ownedItems.Count == 1 && save.ownedItems[0] == "starter-axe" &&
                (save.claimedChests.Count > 0 || save.claimedBossKills.Count > 0 || save.claimedQuests.Count > 0))
            {
                void AddIfCatalog(string itemId)
                {
                    if (string.IsNullOrWhiteSpace(itemId)) return;
                    string clean = itemId.Trim();
                    if (ItemCatalog.Get(clean) != null && !save.ownedItems.Contains(clean))
                    {
                        save.ownedItems.Add(clean);
                        migrated = true;
                    }
                }

                for (int i = 0; i < save.claimedChests.Count; i++)
                {
                    string chestId = save.claimedChests[i];
                    var chestItems = SharedRewardCatalog.GetChestItemIds(chestId);
                    for (int j = 0; j < chestItems.Count; j++)
                        AddIfCatalog(chestItems[j]);
                    if (chestId == "worldboss-gorgonax-chest")
                        AddIfCatalog("worldbreaker-maul");
                }

                for (int i = 0; i < save.claimedBossKills.Count; i++)
                {
                    string bossId = save.claimedBossKills[i];
                    if (bossId == "worldboss-gorgonax")
                    {
                        AddIfCatalog("worldbreaker-maul");
                        if (string.IsNullOrEmpty(save.activeTitle))
                        {
                            save.activeTitle = "Titanbreaker";
                            migrated = true;
                        }
                    }
                    else
                    {
                        var dropItems = SharedRewardCatalog.GetEnemyDropItemIds(bossId);
                        for (int j = 0; j < dropItems.Count; j++)
                            AddIfCatalog(dropItems[j]);
                    }
                }

                for (int i = 0; i < save.claimedQuests.Count; i++)
                {
                    string qId = save.claimedQuests[i];
                    if (qId == "main_quest_2")
                    {
                        AddIfCatalog("emberwatch-axe");
                        AddIfCatalog("emberwatch-sigil");
                    }
                    else
                    {
                        string normQ = qId.StartsWith("faction_quest_", StringComparison.Ordinal)
                            ? qId.Substring("faction_quest_".Length)
                            : qId;
                        string factionReward = ResolveLegacyFactionQuestRewardItem(normQ);
                        if (!string.IsNullOrEmpty(factionReward))
                            AddIfCatalog(factionReward);
                    }
                }
            }

            if (string.IsNullOrEmpty(save.equippedWeaponId) ||
                !save.ownedItems.Contains(save.equippedWeaponId) ||
                ItemCatalog.Get(save.equippedWeaponId)?.slot != GearSlot.Weapon)
            {
                if (!string.Equals(save.equippedWeaponId, "starter-axe", StringComparison.Ordinal))
                    migrated = true;
                save.equippedWeaponId = "starter-axe";
            }

            if (!string.IsNullOrEmpty(save.equippedArmorId) &&
                (!save.ownedItems.Contains(save.equippedArmorId) || ItemCatalog.Get(save.equippedArmorId)?.slot != GearSlot.Armor))
            {
                save.equippedArmorId = "";
                migrated = true;
            }

            if (!string.IsNullOrEmpty(save.equippedTrinketId) &&
                (!save.ownedItems.Contains(save.equippedTrinketId) || ItemCatalog.Get(save.equippedTrinketId)?.slot != GearSlot.Trinket))
            {
                save.equippedTrinketId = "";
                migrated = true;
            }

            return migrated;
        }

        static List<string> DedupeList(List<string> source, bool requireCatalogItem, ref bool changed)
        {
            if (source == null)
            {
                changed = true;
                return new List<string>(0);
            }
            var result = new List<string>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                string raw = source[i];
                if (string.IsNullOrWhiteSpace(raw))
                {
                    changed = true;
                    continue;
                }
                string clean = raw.Trim();
                if (!string.Equals(raw, clean, StringComparison.Ordinal))
                    changed = true;
                if (requireCatalogItem && ItemCatalog.Get(clean) == null)
                {
                    changed = true;
                    continue;
                }
                if (!result.Contains(clean))
                    result.Add(clean);
                else
                    changed = true;
            }
            return result;
        }

        static string ResolveLegacyFactionQuestRewardItem(string questId) => questId switch
        {
            "sunstone_inquisitor_boss" => "sunstone-reliquary-staff",
            "gorkresh_war_forge" => "horde-war-banner",
            "highspire_royal_council" => "alliance-war-banner",
            _ => ""
        };

        public static void SaveCharacter(ServerCharacterSave save)
        {
            EnsureInitialized();
            if (save == null) return;
            save.lastSavedUtc = DateTime.UtcNow.ToString("O");
            string key = CharacterKey(save.accountId, save.characterName);
            CharacterCache[key] = save;

            string path = CharacterFilePath(save.accountId, save.characterName);
            WriteAtomicText(path, JsonUtility.ToJson(save, true));
        }

        public static bool TryClaimChestOnce(string accountId, string characterName, string chestId, int goldReward, int xpReward)
        {
            if (string.IsNullOrWhiteSpace(chestId)) return false;
            var save = LoadOrCreateCharacter(accountId, characterName);
            if (save.claimedChests.Contains(chestId)) return false;

            save.claimedChests.Add(chestId);
            save.gold += Mathf.Max(0, goldReward);
            ApplyExperience(save, Mathf.Max(0, xpReward));
            SaveCharacter(save);
            return true;
        }

        public static bool TryClaimBossKillOnce(string accountId, string characterName, string bossId, int goldReward, int xpReward)
        {
            if (string.IsNullOrWhiteSpace(bossId)) return false;
            var save = LoadOrCreateCharacter(accountId, characterName);
            if (save.claimedBossKills.Contains(bossId)) return false;

            save.claimedBossKills.Add(bossId);
            save.gold += Mathf.Max(0, goldReward);
            ApplyExperience(save, Mathf.Max(0, xpReward));
            SaveCharacter(save);
            return true;
        }

        public static bool TryClaimQuestOnce(string accountId, string characterName, string questId, int goldReward, int xpReward)
        {
            if (string.IsNullOrWhiteSpace(questId)) return false;
            var save = LoadOrCreateCharacter(accountId, characterName);
            if (save.claimedQuests.Contains(questId)) return false;

            save.claimedQuests.Add(questId);
            save.gold += Mathf.Max(0, goldReward);
            ApplyExperience(save, Mathf.Max(0, xpReward));
            SaveCharacter(save);
            return true;
        }

        static void ApplyExperience(ServerCharacterSave save, int xpDelta)
        {
            save.experience += xpDelta;
            while (save.experience >= 100 * save.level)
            {
                save.experience -= 100 * save.level;
                save.level++;
                save.maxHealth += 20;
                save.health = save.maxHealth;
            }
        }

        // =========================================================================
        // 3. SERVER-ENFORCED 5-TIER ROLES & APPEND-ONLY AUDIT LOG
        // =========================================================================
        public static RealmRole RequiredRoleFor(RealmStaffAction action) => action switch
        {
            RealmStaffAction.AnnounceHelp => RealmRole.Guide,
            RealmStaffAction.InspectPlayer => RealmRole.Guide,
            RealmStaffAction.MutePlayer => RealmRole.Moderator,
            RealmStaffAction.KickPlayer => RealmRole.Moderator,
            RealmStaffAction.TeleportPlayer => RealmRole.GameMaster,
            RealmStaffAction.ResetBoss => RealmRole.GameMaster,
            RealmStaffAction.GrantReward => RealmRole.GameMaster,
            RealmStaffAction.SetRole => RealmRole.RealmAdmin,
            RealmStaffAction.ShutdownRealm => RealmRole.RealmAdmin,
            _ => RealmRole.RealmAdmin
        };

        public static bool TryAuthorizeStaffAction(string actorAccountId, RealmStaffAction action, string target, string detail, out string reason)
        {
            EnsureInitialized();
            string cleanActor = SanitizeKey(actorAccountId);
            RealmRole actorRole = GetRole(cleanActor);
            RealmRole required = RequiredRoleFor(action);

            if (actorRole < required)
            {
                reason = $"Denied: {action} requires {required} (actor '{cleanActor}' has {actorRole}).";
                AppendAudit(cleanActor, actorRole, action.ToString(), target ?? "", "DENIED", reason);
                return false;
            }

            reason = "Allowed";
            AppendAudit(cleanActor, actorRole, action.ToString(), target ?? "", "ALLOWED", detail ?? "");
            return true;
        }

        public static bool TrySetAccountRole(string actorAccountId, string targetAccountId, RealmRole newRole, out string reason)
        {
            if (!TryAuthorizeStaffAction(actorAccountId, RealmStaffAction.SetRole, targetAccountId, $"newRole={newRole}", out reason))
                return false;

            string cleanTarget = SanitizeKey(targetAccountId);
            if (!Accounts.TryGetValue(cleanTarget, out var rec))
            {
                reason = $"Target account '{cleanTarget}' not found.";
                AppendAudit(SanitizeKey(actorAccountId), GetRole(actorAccountId), "SetRole", cleanTarget, "DENIED", reason);
                return false;
            }

            rec.role = newRole;
            SaveAccountsToDisk();
            reason = $"Role for '{cleanTarget}' updated to {newRole}.";
            return true;
        }

        public static void AppendAudit(string actorAccount, RealmRole actorRole, string action, string target, string result, string detail)
        {
            if (string.IsNullOrEmpty(AuditLogPath)) return;
            var entry = new RealmAuditEntry
            {
                timestampUtc = DateTime.UtcNow.ToString("O"),
                actorAccount = actorAccount ?? "",
                actorRole = actorRole.ToString(),
                action = action ?? "",
                target = target ?? "",
                result = result ?? "",
                detail = detail ?? ""
            };
            File.AppendAllText(AuditLogPath, JsonUtility.ToJson(entry) + "\n", Encoding.UTF8);
        }

        // =========================================================================
        // INTERNAL STORAGE HELPERS
        // =========================================================================
        static void EnsureInitialized()
        {
            if (!IsInitialized)
                Initialize(RealmConfig.ResolveServerDataDirectory());
        }

        static void LoadAccountsFromDisk()
        {
            if (!File.Exists(AccountsFilePath)) return;
            string json = File.ReadAllText(AccountsFilePath, Encoding.UTF8);
            var file = JsonUtility.FromJson<RealmAccountStoreFile>(json);
            if (file?.accounts == null) return;
            foreach (var rec in file.accounts)
            {
                if (rec != null && !string.IsNullOrEmpty(rec.accountId))
                    Accounts[rec.accountId] = rec;
            }
        }

        static void SaveAccountsToDisk()
        {
            var file = new RealmAccountStoreFile();
            foreach (var kv in Accounts) file.accounts.Add(kv.Value);
            WriteAtomicText(AccountsFilePath, JsonUtility.ToJson(file, true));
        }

        static void WriteAtomicText(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, content, Encoding.UTF8);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        static string CharacterKey(string accountId, string charName) => $"{SanitizeKey(accountId)}__{SanitizeKey(charName)}";
        static string CharacterFilePath(string accountId, string charName) => Path.Combine(CharactersDirectory, CharacterKey(accountId, charName) + ".json");

        static string SanitizeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            var sb = new StringBuilder(raw.Length);
            foreach (char c in raw.Trim())
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_') sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        static string HashSecret(string salt, string secret)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{salt}:{secret}"));
            var sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }

        static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
