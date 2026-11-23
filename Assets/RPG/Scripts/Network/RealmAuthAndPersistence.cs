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
                    loaded.claimedChests ??= new List<string>();
                    loaded.claimedBossKills ??= new List<string>();
                    loaded.claimedQuests ??= new List<string>();
                    CharacterCache[key] = loaded;
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
                lastSavedUtc = DateTime.UtcNow.ToString("O")
            };
            CharacterCache[key] = created;
            SaveCharacter(created);
            return created;
        }

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
