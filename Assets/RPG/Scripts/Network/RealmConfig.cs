using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace Riftbound.Network
{
    [Serializable]
    public class RealmConfigData
    {
        public string realmId = "emberford";
        public string displayName = "Emberford";
        public string endpoint = "127.0.0.1";
        public string listenAddress = "127.0.0.1";
        public ushort port = 7777;
        public int maxPlayers = 4;
        public string dataDir = "";
        public string logPath = "";
    }

    public sealed class RealmRuntimeSettings
    {
        public string configFilePath = "";
        public bool autoEnterRealm;
        public bool autoEnterConsumed;
        public string clientCharName = "";
        public string clientRace = "Human";
        public string clientClass = "Warrior";
        public string clientFaction = "Alliance";
        public string smokeRole = "";
        public string smokeOutPath = "";
        public float smokeSeconds;
    }

    public static class RealmConfig
    {
        public const string DefaultRealmId = "emberford";
        public const string DefaultDisplayName = "Emberford";
        public const string DefaultLoopback = "127.0.0.1";
        public const ushort DefaultPort = 7777;
        public const int DefaultMaxPlayers = 4;
        public const int MinMaxPlayers = 1;
        public const int MaxSupportedPlayers = 64;

        static RealmConfigData current = new RealmConfigData();
        static RealmRuntimeSettings runtime = new RealmRuntimeSettings();
        static bool loaded;

        public static RealmConfigData Current
        {
            get
            {
                if (!loaded) LoadOnce();
                return current;
            }
        }

        public static RealmRuntimeSettings Runtime
        {
            get
            {
                if (!loaded) LoadOnce();
                return runtime;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            loaded = false;
            current = new RealmConfigData();
            runtime = new RealmRuntimeSettings();
        }

        public static void ResetForTest() => ResetStatics();

        public static void LoadOnce()
        {
            if (loaded) return;
            loaded = true;
            ApplyFromArgsAndEnv(Environment.GetCommandLineArgs(), Environment.GetEnvironmentVariable);
        }

        public static void LoadFromArgumentsForTest(string[] args, Func<string, string> envLookup = null)
        {
            loaded = true;
            current = new RealmConfigData();
            runtime = new RealmRuntimeSettings();
            ApplyFromArgsAndEnv(args ?? Array.Empty<string>(), envLookup);
        }

        static void ApplyFromArgsAndEnv(string[] args, Func<string, string> envLookup)
        {
            if (envLookup != null)
            {
                ApplyEnv(envLookup);
            }

            string configPath = FindArgValue(args, "-realmConfig");
            if (string.IsNullOrWhiteSpace(configPath) && envLookup != null)
                configPath = envLookup("RIFTBOUND_REALM_CONFIG");

            if (!string.IsNullOrWhiteSpace(configPath))
            {
                runtime.configFilePath = configPath.Trim();
                TryApplyJsonFile(runtime.configFilePath);
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-realmId" && TryNextValue(args, ref i, out string id))
                    current.realmId = SanitizeRealmId(id);
                else if (arg == "-realmName" && TryNextValue(args, ref i, out string name))
                    current.displayName = string.IsNullOrWhiteSpace(name) ? DefaultDisplayName : name.Trim();
                else if (arg == "-realmEndpoint" && TryNextValue(args, ref i, out string ep))
                    current.endpoint = string.IsNullOrWhiteSpace(ep) ? DefaultLoopback : ep.Trim();
                else if (arg == "-realmListen" && TryNextValue(args, ref i, out string listen))
                    current.listenAddress = string.IsNullOrWhiteSpace(listen) ? DefaultLoopback : listen.Trim();
                else if ((arg == "-realmPort" || arg == "-port") && TryNextValue(args, ref i, out string portStr) && ushort.TryParse(portStr, out ushort p) && p > 0)
                    current.port = p;
                else if ((arg == "-realmMaxPlayers" || arg == "-maxPlayers") && TryNextValue(args, ref i, out string capStr) && int.TryParse(capStr, out int cap))
                    current.maxPlayers = ClampCapacity(cap);
                else if (arg == "-realmDataDir" && TryNextValue(args, ref i, out string dir))
                    current.dataDir = dir.Trim();
                else if (arg == "-realmLog" && TryNextValue(args, ref i, out string log))
                    current.logPath = log.Trim();
                else if (arg == "-realmAutoEnter")
                    runtime.autoEnterRealm = true;
                else if (arg == "-realmCharName" && TryNextValue(args, ref i, out string charName))
                    runtime.clientCharName = charName.Trim();
                else if (arg == "-realmRace" && TryNextValue(args, ref i, out string race))
                    runtime.clientRace = string.IsNullOrWhiteSpace(race) ? "Human" : race.Trim();
                else if (arg == "-realmClass" && TryNextValue(args, ref i, out string cls))
                    runtime.clientClass = string.IsNullOrWhiteSpace(cls) ? "Warrior" : cls.Trim();
                else if (arg == "-realmFaction" && TryNextValue(args, ref i, out string fac))
                    runtime.clientFaction = string.IsNullOrWhiteSpace(fac) ? "Alliance" : fac.Trim();
                else if (arg == "-realmSmokeRole" && TryNextValue(args, ref i, out string role))
                    runtime.smokeRole = role.Trim().ToLowerInvariant();
                else if (arg == "-realmSmokeOut" && TryNextValue(args, ref i, out string smokeOut))
                    runtime.smokeOutPath = smokeOut.Trim();
                else if (arg == "-realmSmokeSeconds" && TryNextValue(args, ref i, out string secStr) && float.TryParse(secStr, out float sec))
                    runtime.smokeSeconds = Mathf.Max(2f, sec);
            }

            current.realmId = SanitizeRealmId(current.realmId);
            if (string.IsNullOrWhiteSpace(current.displayName)) current.displayName = DefaultDisplayName;
            if (string.IsNullOrWhiteSpace(current.endpoint)) current.endpoint = DefaultLoopback;
            if (string.IsNullOrWhiteSpace(current.listenAddress)) current.listenAddress = DefaultLoopback;
            if (current.port == 0) current.port = DefaultPort;
            current.maxPlayers = ClampCapacity(current.maxPlayers);
        }

        static void ApplyEnv(Func<string, string> env)
        {
            string id = env("RIFTBOUND_REALM_ID");
            if (!string.IsNullOrWhiteSpace(id)) current.realmId = SanitizeRealmId(id);
            string name = env("RIFTBOUND_REALM_NAME");
            if (!string.IsNullOrWhiteSpace(name)) current.displayName = name.Trim();
            string ep = env("RIFTBOUND_REALM_ENDPOINT");
            if (!string.IsNullOrWhiteSpace(ep)) current.endpoint = ep.Trim();
            string listen = env("RIFTBOUND_REALM_LISTEN");
            if (!string.IsNullOrWhiteSpace(listen)) current.listenAddress = listen.Trim();
            string portStr = env("RIFTBOUND_REALM_PORT");
            if (!string.IsNullOrWhiteSpace(portStr) && ushort.TryParse(portStr, out ushort p) && p > 0) current.port = p;
            string capStr = env("RIFTBOUND_REALM_MAX_PLAYERS");
            if (!string.IsNullOrWhiteSpace(capStr) && int.TryParse(capStr, out int cap)) current.maxPlayers = ClampCapacity(cap);
            string dir = env("RIFTBOUND_REALM_DATA_DIR");
            if (!string.IsNullOrWhiteSpace(dir)) current.dataDir = dir.Trim();
            string log = env("RIFTBOUND_REALM_LOG_PATH");
            if (!string.IsNullOrWhiteSpace(log)) current.logPath = log.Trim();
        }

        static void TryApplyJsonFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return;
                var parsed = JsonUtility.FromJson<RealmConfigData>(File.ReadAllText(path));
                if (parsed == null) return;
                if (!string.IsNullOrWhiteSpace(parsed.realmId)) current.realmId = parsed.realmId.Trim();
                if (!string.IsNullOrWhiteSpace(parsed.displayName)) current.displayName = parsed.displayName.Trim();
                if (!string.IsNullOrWhiteSpace(parsed.endpoint)) current.endpoint = parsed.endpoint.Trim();
                if (!string.IsNullOrWhiteSpace(parsed.listenAddress)) current.listenAddress = parsed.listenAddress.Trim();
                if (parsed.port > 0) current.port = parsed.port;
                if (parsed.maxPlayers > 0) current.maxPlayers = ClampCapacity(parsed.maxPlayers);
                if (!string.IsNullOrWhiteSpace(parsed.dataDir)) current.dataDir = parsed.dataDir.Trim();
                if (!string.IsNullOrWhiteSpace(parsed.logPath)) current.logPath = parsed.logPath.Trim();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RealmConfig] Failed reading config file '{path}': {ex.Message}");
            }
        }

        static string FindArgValue(string[] args, string flag)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == flag && !args[i + 1].StartsWith("-", StringComparison.Ordinal))
                    return args[i + 1];
            }
            return null;
        }

        static bool TryNextValue(string[] args, ref int index, out string value)
        {
            if (index + 1 < args.Length && !args[index + 1].StartsWith("-", StringComparison.Ordinal))
            {
                value = args[++index];
                return true;
            }
            value = "";
            return false;
        }

        public static string SanitizeRealmId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return DefaultRealmId;
            return raw.Trim().ToLowerInvariant().Replace(" ", "-");
        }

        public static int ClampCapacity(int cap) => Mathf.Clamp(cap <= 0 ? DefaultMaxPlayers : cap, MinMaxPlayers, MaxSupportedPlayers);

        public static int GetEffectiveMaxPlayers()
        {
            return CoopSessionConfig.IsRealmSession ? ClampCapacity(Current.maxPlayers) : CoopWorldManager.MaxPlayers;
        }

        public static bool IsLoopbackAddress(string addr)
        {
            if (string.IsNullOrWhiteSpace(addr)) return false;
            string trimmed = addr.Trim();
            if (string.Equals(trimmed, "localhost", StringComparison.OrdinalIgnoreCase)) return true;
            return IPAddress.TryParse(trimmed, out var ip) && IPAddress.IsLoopback(ip);
        }

        public static string ResolveServerDataDirectory()
        {
            if (!string.IsNullOrWhiteSpace(Current.dataDir))
                return Path.GetFullPath(Current.dataDir);
            string baseDir = Application.isEditor
                ? Path.GetFullPath(".code-check/realm-data")
                : Path.GetFullPath(Path.Combine(Application.dataPath, "..", "RealmData"));
            return Path.Combine(baseDir, Current.realmId);
        }

        public static string ResolveServerLogPath()
        {
            if (!string.IsNullOrWhiteSpace(Current.logPath))
                return Path.GetFullPath(Current.logPath);
            return Path.Combine(ResolveServerDataDirectory(), "logs", $"{Current.realmId}-server.log");
        }
    }
}
