using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Riftbound.Network;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Riftbound.Editor
{
    public static class EmberfordRealmChecks
    {
        const string ReportPath = ".code-check/emberford-r1/realm-unit-checks.txt";

        [MenuItem("Riftbound/Emberford/Run Realm Unit Checks")]
        public static void Run()
        {
            var lines = new List<string>();
            int passed = 0;
            int failed = 0;

            void Check(string name, bool ok, string detail)
            {
                lines.Add($"{(ok ? "PASS" : "FAIL")} [{name}] {detail}");
                if (ok) passed++;
                else failed++;
            }

            try
            {
                EmberfordRealmBuild.EnsureRealmPrefabs();

                // 1. Default RealmConfig guarantees Emberford loopback + 4 player cap
                RealmConfig.LoadFromArgumentsForTest(new string[0]);
                var def = RealmConfig.Current;
                Check("realm_config_defaults",
                    def.realmId == "emberford" &&
                    def.displayName == "Emberford" &&
                    def.endpoint == "127.0.0.1" &&
                    def.listenAddress == "127.0.0.1" &&
                    def.port == 7777 &&
                    def.maxPlayers == 4 &&
                    RealmConfig.IsLoopbackAddress(def.listenAddress) &&
                    !RealmConfig.IsLoopbackAddress("0.0.0.0"),
                    $"id={def.realmId}, listen={def.listenAddress}:{def.port}, cap={def.maxPlayers}");

                // 2. CLI + JSON overrides for endpoint, port, cap, dataDir, logPath
                string tempDir = Path.Combine(Path.GetTempPath(), "rb_realm_cfg_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                string cfgJson = Path.Combine(tempDir, "realm.json");
                File.WriteAllText(cfgJson, "{\"realmId\":\"emberford\",\"displayName\":\"Emberford\",\"endpoint\":\"10.0.0.5\",\"listenAddress\":\"0.0.0.0\",\"port\":8888,\"maxPlayers\":8}");
                RealmConfig.LoadFromArgumentsForTest(new[]
                {
                    "-realmConfig", cfgJson,
                    "-realmPort", "7900",
                    "-realmDataDir", tempDir,
                    "-realmLog", Path.Combine(tempDir, "srv.log"),
                    "-realmCharName", "Alden",
                    "-realmClass", "Mage"
                });
                Check("realm_config_json_and_cli_override",
                    RealmConfig.Current.endpoint == "10.0.0.5" &&
                    RealmConfig.Current.listenAddress == "0.0.0.0" &&
                    RealmConfig.Current.port == 7900 &&
                    RealmConfig.Current.maxPlayers == 8 &&
                    RealmConfig.ResolveServerDataDirectory() == Path.GetFullPath(tempDir) &&
                    RealmConfig.Runtime.clientCharName == "Alden" &&
                    RealmConfig.Runtime.clientClass == "Mage",
                    $"ep={RealmConfig.Current.endpoint}, listen={RealmConfig.Current.listenAddress}, port={RealmConfig.Current.port}, cap={RealmConfig.Current.maxPlayers}");
                Directory.Delete(tempDir, true);

                // Reset back to defaults
                RealmConfig.LoadFromArgumentsForTest(new string[0]);

                // 3. Handshake payload validation rejects mismatch/wrong protocol/wrong realm
                byte[] goodPayload = Encoding.UTF8.GetBytes($"{CoopSessionConfig.ProtocolVersion}|emberford|Elf");
                byte[] badProto = Encoding.UTF8.GetBytes("bad-proto|emberford|Human");
                byte[] badRealm = Encoding.UTF8.GetBytes($"{CoopSessionConfig.ProtocolVersion}|other-realm|Human");
                bool okGood = CoopWorldManager.ValidateRealmApprovalPayload(goodPayload, "emberford", out string raceOut, out _);
                bool rejProto = !CoopWorldManager.ValidateRealmApprovalPayload(badProto, "emberford", out _, out _);
                bool rejRealm = !CoopWorldManager.ValidateRealmApprovalPayload(badRealm, "emberford", out _, out _);
                bool rejEmpty = !CoopWorldManager.ValidateRealmApprovalPayload(null, "emberford", out _, out _);
                Check("realm_handshake_payload_validation",
                    okGood && raceOut == "Elf" && rejProto && rejRealm && rejEmpty,
                    $"good={okGood}({raceOut}), rejProto={rejProto}, rejRealm={rejRealm}, rejEmpty={rejEmpty}");

                // 4. Server-channel sender validation invariant
                bool channelAuthOk =
                    RealmWorldChannel.IsAuthorizedServerSenderForIds(0UL, 0UL, 0UL) &&
                    !RealmWorldChannel.IsAuthorizedServerSenderForIds(1UL, 0UL, 0UL) &&
                    !RealmWorldChannel.IsAuthorizedServerSenderForIds(0UL, 1UL, 0UL) &&
                    !RealmWorldChannel.IsAuthorizedServerSenderForIds(1UL, 1UL, 0UL);
                Check("realm_channel_sender_and_owner_validation", channelAuthOk, $"authOk={channelAuthOk}");

                // 5. Client Realm API entry & clean cancel/offline restoration
                EmberfordRealmClient.ResetForTest();
                CoopSessionConfig.ResetToOffline();
                EmberfordRealmClient.SelectEmberfordRealm("127.0.0.1", 7777);
                var info = EmberfordRealmClient.GetEmberfordRealm();
                bool canEnter = EmberfordRealmClient.CanEnterRealm(out string reason);
                bool selectedOk = CoopSessionConfig.IsRealmClient &&
                                  info.Id == "emberford" &&
                                  info.DisplayName == "Emberford" &&
                                  info.MaxPlayers == 4 &&
                                  canEnter &&
                                  string.IsNullOrEmpty(reason);
                EmberfordRealmClient.CancelRealmSelection();
                bool cancelClean = !CoopSessionConfig.IsMultiplayer && EmberfordRealmClient.State == RealmConnectionState.Disconnected;
                Check("emberford_realm_client_api", selectedOk && cancelClean, $"selectedOk={selectedOk}, cancelClean={cancelClean}");

                // 6. Prefab parity check
                var avatarPrefab = Resources.Load<GameObject>("CoopPlayerAvatar");
                var channelPrefab = Resources.Load<GameObject>("EmberfordRealmChannel");
                Check("realm_network_prefabs_loadable",
                    avatarPrefab != null && avatarPrefab.GetComponent<NetworkObject>() != null && avatarPrefab.GetComponent<RiftboundCoopPlayer>() != null &&
                    channelPrefab != null && channelPrefab.GetComponent<NetworkObject>() != null && channelPrefab.GetComponent<RealmWorldChannel>() != null,
                    $"avatar={(avatarPrefab != null)}, channel={(channelPrefab != null)}");

                // 7. Spawn anchor works headless without PlayerController
                Vector3 humanAnchor = CoopWorldManager.GetRealmSpawnAnchor("Human");
                Vector3 bruteAnchor = CoopWorldManager.GetRealmSpawnAnchor("Brute");
                Check("headless_spawn_anchors_valid",
                    humanAnchor != bruteAnchor && !float.IsNaN(humanAnchor.y) && !float.IsNaN(bruteAnchor.y),
                    $"human={humanAnchor}, brute={bruteAnchor}");

                bool serverSubtargetSupported = EmberfordRealmBuild.IsWindowsDedicatedServerModuleInstalled();
                lines.Add($"INFO [unity_windows_dedicated_server_module_installed] supported={serverSubtargetSupported}");
            }
            catch (Exception ex)
            {
                lines.Add($"FAIL [exception] {ex}");
                failed++;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            lines.Add($"TOTAL PASS={passed} FAIL={failed} RESULT={(failed == 0 ? "PASS" : "FAIL")}");
            File.WriteAllText(ReportPath, string.Join("\n", lines) + "\n");
            Debug.Log($"[EmberfordRealmChecks] PASS={passed} FAIL={failed}");

            if (Application.isBatchMode)
                EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
    }
}
