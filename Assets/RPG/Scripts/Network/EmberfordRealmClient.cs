using System;
using UnityEngine;

namespace Riftbound.Network
{
    public enum RealmConnectionState
    {
        Disconnected = 0,
        Selected = 1,
        Connecting = 2,
        InWorld = 3,
        Failed = 4
    }

    public sealed class RealmEntryInfo
    {
        public string Id;
        public string DisplayName;
        public string Endpoint;
        public ushort Port;
        public int MaxPlayers;
        public string ProtocolVersion;
        public string BuildVersion;
        public RealmConnectionState State;
        public string StatusSummary;
        public string LastError;
    }

    /// <summary>
    /// Minimal client-side realm integration API for Codex's FrontendMenu hook
    /// (Play Online -> Emberford -> Character Select -> Enter World).
    /// Keeps client UI decoupled from networking internals and preserves Offline/Relay modes.
    /// </summary>
    public static class EmberfordRealmClient
    {
        public static RealmConnectionState State { get; private set; } = RealmConnectionState.Disconnected;
        public static string StatusMessage { get; private set; } = "Ready";
        public static string LastError { get; private set; } = string.Empty;
        public static int ConnectedPlayerCount { get; private set; }

        public static bool IsRealmSelected => CoopSessionConfig.Mode == CoopSessionMode.RealmClient;
        public static bool IsConnectedOrConnecting =>
            State == RealmConnectionState.Connecting || State == RealmConnectionState.InWorld;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            State = RealmConnectionState.Disconnected;
            StatusMessage = "Ready";
            LastError = string.Empty;
            ConnectedPlayerCount = 0;
        }

        public static void ResetForTest() => ResetStatics();

        public static RealmEntryInfo GetEmberfordRealm()
        {
            var cfg = RealmConfig.Current;
            return new RealmEntryInfo
            {
                Id = cfg.realmId,
                DisplayName = cfg.displayName,
                Endpoint = cfg.endpoint,
                Port = cfg.port,
                MaxPlayers = RealmConfig.ClampCapacity(cfg.maxPlayers),
                ProtocolVersion = CoopSessionConfig.ProtocolVersion,
                BuildVersion = Application.version,
                State = State,
                StatusSummary = BuildStatusSummary(),
                LastError = LastError
            };
        }

        public static void SelectEmberfordRealm(string endpointOverride = null, ushort? portOverride = null)
        {
            if (!string.IsNullOrWhiteSpace(endpointOverride))
                RealmConfig.Current.endpoint = endpointOverride.Trim();
            if (portOverride.HasValue && portOverride.Value > 0)
                RealmConfig.Current.port = portOverride.Value;

            CoopSessionConfig.Mode = CoopSessionMode.RealmClient;
            CoopSessionConfig.LastDisconnectReason = string.Empty;
            LastError = string.Empty;
            State = RealmConnectionState.Selected;
            StatusMessage = $"Selected {RealmConfig.Current.displayName} ({RealmConfig.Current.endpoint}:{RealmConfig.Current.port})";
        }

        public static void CancelRealmSelection()
        {
            if (CoopSessionConfig.Mode == CoopSessionMode.RealmClient)
                CoopSessionConfig.ResetToOffline();
            State = RealmConnectionState.Disconnected;
            StatusMessage = "Ready";
            LastError = string.Empty;
            ConnectedPlayerCount = 0;
        }

        public static bool CanEnterRealm(out string reason)
        {
            var cfg = RealmConfig.Current;
            if (string.IsNullOrWhiteSpace(cfg.endpoint))
            {
                reason = "Emberford realm endpoint is empty.";
                return false;
            }
            if (cfg.port == 0)
            {
                reason = "Emberford realm port must be non-zero.";
                return false;
            }
            if (OnlineSessionService.IsBusy)
            {
                reason = "Another online attempt is currently in progress.";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public static void DisconnectToTitleMenu()
        {
            if (CoopWorldManager.Instance != null)
            {
                CoopWorldManager.Instance.DisconnectAndReturnToMenu();
                return;
            }
            CancelRealmSelection();
        }

        internal static void MarkConnecting(string endpoint, ushort port)
        {
            State = RealmConnectionState.Connecting;
            LastError = string.Empty;
            StatusMessage = $"Connecting to {RealmConfig.Current.displayName} ({endpoint}:{port})...";
        }

        internal static void MarkInWorld(int playerCount)
        {
            State = RealmConnectionState.InWorld;
            LastError = string.Empty;
            ConnectedPlayerCount = Mathf.Max(1, playerCount);
            StatusMessage = $"Connected to {RealmConfig.Current.displayName} ({ConnectedPlayerCount}/{RealmConfig.GetEffectiveMaxPlayers()})";
        }

        internal static void MarkDisconnectedOrFailed(string reason, bool failed)
        {
            LastError = string.IsNullOrEmpty(reason) ? "Disconnected from realm" : reason;
            State = failed ? RealmConnectionState.Failed : RealmConnectionState.Disconnected;
            ConnectedPlayerCount = 0;
            StatusMessage = LastError;
        }

        static string BuildStatusSummary()
        {
            var cfg = RealmConfig.Current;
            return State switch
            {
                RealmConnectionState.Connecting => $"Connecting ({cfg.endpoint}:{cfg.port})",
                RealmConnectionState.InWorld => $"Online ({ConnectedPlayerCount}/{RealmConfig.ClampCapacity(cfg.maxPlayers)})",
                RealmConnectionState.Failed => string.IsNullOrEmpty(LastError) ? "Unavailable" : $"Error: {LastError}",
                _ => $"{cfg.endpoint}:{cfg.port} (Cap {RealmConfig.ClampCapacity(cfg.maxPlayers)})"
            };
        }
    }
}
