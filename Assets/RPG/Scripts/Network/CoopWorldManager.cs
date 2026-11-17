using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Riftbound.Network
{
    public class CoopWorldManager : MonoBehaviour
    {
        public const int MaxPlayers = 4;
        public static CoopWorldManager Instance { get; private set; }

        NetworkManager nm;
        UnityTransport utp;
        string statusText = "";
        bool shuttingDown;

        // Deliverables #5: Party Frames, Map Ping, Ready Check
        bool readyCheckOpen;
        string readyCheckInitiator = "";
        float readyCheckExpires;
        string activePingSender = "";
        Vector3 activePingWorldPos;
        float activePingExpires;
        bool hostDisconnected;
        string hostDisconnectMessage = "";

        public bool HasActivePingForTest => Time.time < activePingExpires;
        public bool IsReadyCheckOpenForTest => readyCheckOpen && Time.time < readyCheckExpires;
        public bool IsHostDisconnectedForTest => hostDisconnected;

        public bool TryGetActiveReadyCheck(out string initiator)
        {
            if (readyCheckOpen && Time.time < readyCheckExpires && !string.IsNullOrEmpty(readyCheckInitiator))
            {
                initiator = readyCheckInitiator;
                return true;
            }
            initiator = "";
            return false;
        }

        public void ShowMapPing(string sender, Vector3 pos)
        {
            activePingSender = sender;
            activePingWorldPos = pos;
            activePingExpires = Time.time + 8f;
            SpellFX.CreateArcaneBurst(pos + Vector3.up * 0.4f);
        }

        public void OpenReadyCheck(string initiator)
        {
            readyCheckInitiator = initiator;
            readyCheckOpen = true;
            readyCheckExpires = Time.time + 20f;
        }

        void Update()
        {
            if (!CoopSessionConfig.IsMultiplayer || FrontendMenu.IsOpen || RiftboundCoopPlayer.LocalOwner == null) return;
            if (readyCheckOpen && Time.time > readyCheckExpires) readyCheckOpen = false;
            var keys = Keyboard.current;
            if (keys == null) return;

            var localPc = FindFirstObjectByType<PlayerController>();
            bool uiBlocking = localPc != null && (localPc.ChatOpen || localPc.DialogueOpen || localPc.ShopOpen || localPc.InventoryOpen || localPc.MapOpen || localPc.JournalOpen || localPc.TalentsOpen || localPc.SpellbookOpen || localPc.AchievementOpen || localPc.ReputationOpen);
            if (uiBlocking || RiftboundCoopPlayer.LocalOwner.Health.Value <= 0) return;

            if (keys.f6Key.wasPressedThisFrame)
            {
                RiftboundCoopPlayer.LocalOwner.RequestMapPing(RiftboundCoopPlayer.LocalOwner.transform.position);
            }
            if (keys.f7Key.wasPressedThisFrame)
            {
                RiftboundCoopPlayer.LocalOwner.RequestReadyCheck();
            }
            // Note: [E] Ally Revive is handled cleanly through PlayerServices.TryGetBestInteraction / Interact() so [E] never double-fires alongside quests/NPCs/chests.
        }

        readonly Dictionary<ulong, HashSet<string>> claimedChestsByClient = new Dictionary<ulong, HashSet<string>>();
        readonly Dictionary<ulong, HashSet<int>> claimedQuestsByClient = new Dictionary<ulong, HashSet<int>>();
        readonly Dictionary<ulong, HashSet<string>> creditedKillsByClient = new Dictionary<ulong, HashSet<string>>();

        public bool TryClaimChestForClient(ulong clientId, string chestId)
        {
            if (string.IsNullOrEmpty(chestId)) return false;
            if (!claimedChestsByClient.TryGetValue(clientId, out var set))
                claimedChestsByClient[clientId] = set = new HashSet<string>();
            return set.Add(chestId);
        }

        public bool TryClaimQuestForClient(ulong clientId, int questIndex)
        {
            if (!claimedQuestsByClient.TryGetValue(clientId, out var set))
                claimedQuestsByClient[clientId] = set = new HashSet<int>();
            return set.Add(questIndex);
        }

        public bool TryCreditKillForClient(ulong clientId, string enemyKey)
        {
            if (string.IsNullOrEmpty(enemyKey)) return false;
            if (!creditedKillsByClient.TryGetValue(clientId, out var set))
                creditedKillsByClient[clientId] = set = new HashSet<string>();
            return set.Add(enemyKey);
        }

        static bool sceneHookRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneLoaded()
        {
            if (!sceneHookRegistered)
            {
                sceneHookRegistered = true;
                SceneManager.sceneLoaded += (_, _) => EnsureManagerForActiveScene();
            }
            EnsureManagerForActiveScene();
        }

        static int clientLeaveStage = 0;
        static bool clientLeaveQuestAccepted = false;
        static bool clientLeaveMounted = false;
        static string clientLeaveRejoinCode = "";
        static bool clientLeaveRejoinOk = false;

        static void EnsureManagerForActiveScene()
        {
            CoopSessionConfig.ParseCommandLineOnce();
            string sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.Contains("Outpost")) return;
            bool resilienceActive = CoopSessionConfig.ResilienceSmoke && (resilienceStage == -2 || resilienceStage == 1 || resilienceStage == 3);
            bool clientLeaveActive = CoopSessionConfig.ClientLeaveSmoke && (clientLeaveStage == 1 || clientLeaveStage == 3);
            bool uiSmokeActive = CoopSessionConfig.UiSmoke && UiSmokeStage == 2;
            if (!CoopSessionConfig.IsMultiplayer && !resilienceActive && !clientLeaveActive && !uiSmokeActive) return;
            if (Instance != null) return;

            var go = new GameObject("CoopWorldManager");
            go.AddComponent<CoopWorldManager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public static int UiSmokeStage = 0;

        static int resilienceStage = -1;
        static string resilienceCode1 = "";
        static int resilienceNmCount = 0;
        static bool resilienceCancelOk = false;

        void Start()
        {
            if (CoopSessionConfig.UiSmoke)
            {
                if (UiSmokeStage == 1)
                {
                    Time.timeScale = 1f;
                    StartSelectedSessionMode();
                    StartCoroutine(RunUiSmokeWorldRoutine());
                    return;
                }
                if (UiSmokeStage == 2)
                {
                    StartCoroutine(FinishUiSmokeRoutine());
                    return;
                }
            }

            if (CoopSessionConfig.ClientLeaveSmoke && (clientLeaveStage == 1 || clientLeaveStage == 3))
            {
                StartCoroutine(FinishClientLeaveSmokeRoutine());
                return;
            }

            if (CoopSessionConfig.ResilienceSmoke)
            {
                if (resilienceStage == -2)
                {
                    resilienceCancelOk = OnlineSessionService.State == OnlineSessionState.Offline &&
                                         !OnlineSessionService.IsBusy &&
                                         FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length == 0;
                    resilienceStage = 0;
                    CoopSessionConfig.Mode = CoopSessionMode.HostOnline;
                }
                else if (resilienceStage == 1)
                {
                    resilienceStage = 2;
                    CoopSessionConfig.Mode = CoopSessionMode.HostOnline;
                }
                else if (resilienceStage == 3)
                {
                    StartCoroutine(FinishResilienceSmokeRoutine());
                    return;
                }
            }

            if (!CoopSessionConfig.IsMultiplayer)
            {
                Destroy(gameObject);
                return;
            }

            if (CoopSessionConfig.AutoSmokeExitSeconds > 0f)
            {
                Time.timeScale = 1f;
            }

            StartSelectedSessionMode();

            if (CoopSessionConfig.AutoSmokeExitSeconds > 0f)
            {
                StartCoroutine(RunCoopSmokeRoutine(CoopSessionConfig.AutoSmokeExitSeconds));
            }
        }

        void StartSelectedSessionMode()
        {
            if (CoopSessionConfig.IsOnlineMode)
            {
                StartOnlineFlow();
            }
            else if (CoopSessionConfig.IsLanDevMode)
            {
                StartLanDevFlow();
            }
        }

        void EnsureNetworkManagerPrepared()
        {
            GameObject avatarPrefab = Resources.Load<GameObject>("CoopPlayerAvatar");
            if (!avatarPrefab)
            {
                statusText = "Missing Resources/CoopPlayerAvatar prefab!";
                Debug.LogError("[CoopWorldManager] " + statusText);
                return;
            }

            nm = NetworkManager.Singleton;
            if (nm == null)
            {
                var nmGo = new GameObject("CoopNetworkManager");
                nm = nmGo.AddComponent<NetworkManager>();
                utp = nmGo.AddComponent<UnityTransport>();
                nm.NetworkConfig = new NetworkConfig
                {
                    NetworkTransport = utp,
                    PlayerPrefab = avatarPrefab,
                    ConnectionApproval = CoopSessionConfig.IsLanDevMode
                };
                nm.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = avatarPrefab });
            }
            else
            {
                utp = nm.GetComponent<UnityTransport>() ?? nm.gameObject.AddComponent<UnityTransport>();
                if (nm.NetworkConfig == null) nm.NetworkConfig = new NetworkConfig();
                nm.NetworkConfig.NetworkTransport = utp;
                nm.NetworkConfig.PlayerPrefab = avatarPrefab;
                nm.NetworkConfig.ConnectionApproval = CoopSessionConfig.IsLanDevMode;
            }

            nm.OnClientConnectedCallback -= OnClientConnected;
            nm.OnClientDisconnectCallback -= OnClientDisconnected;
            nm.OnClientConnectedCallback += OnClientConnected;
            nm.OnClientDisconnectCallback += OnClientDisconnected;
        }

        async void StartOnlineFlow()
        {
            if (CoopSessionConfig.Mode == CoopSessionMode.HostOnline)
            {
                bool ok = await OnlineSessionService.StartHostOnlineAsync(EnsureNetworkManagerPrepared);
                statusText = ok ? $"Online Host ({OnlineSessionService.ActiveJoinCode})" : OnlineSessionService.LastError;
            }
            else if (CoopSessionConfig.Mode == CoopSessionMode.JoinOnline)
            {
                bool ok = await OnlineSessionService.JoinOnlineByCodeAsync(CoopSessionConfig.JoinCode, EnsureNetworkManagerPrepared);
                statusText = ok ? $"Online Co-op ({OnlineSessionService.ActiveJoinCode})" : OnlineSessionService.LastError;
            }
        }

        void StartLanDevFlow()
        {
            EnsureNetworkManagerPrepared();
            if (nm == null) return;

            if (CoopSessionConfig.Mode == CoopSessionMode.HostLan)
            {
                nm.ConnectionApprovalCallback = ApprovalCheck;
                utp.SetConnectionData("127.0.0.1", CoopSessionConfig.Port, "0.0.0.0");
                bool ok = nm.StartHost();
                statusText = ok ? $"Dev LAN Host :{CoopSessionConfig.Port}" : "Failed to start Host";
                Debug.Log($"[CoopWorldManager] {statusText}");
            }
            else if (CoopSessionConfig.Mode == CoopSessionMode.JoinLan)
            {
                string ip = string.IsNullOrWhiteSpace(CoopSessionConfig.JoinIp) ? "127.0.0.1" : CoopSessionConfig.JoinIp.Trim();
                utp.SetConnectionData(ip, CoopSessionConfig.Port);
                bool ok = nm.StartClient();
                statusText = ok ? $"Connecting to {ip}:{CoopSessionConfig.Port}..." : "Failed to start Client";
                Debug.Log($"[CoopWorldManager] {statusText}");
            }
        }

        void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (nm.ConnectedClientsIds.Count >= MaxPlayers)
            {
                response.Approved = false;
                response.Reason = $"Server full ({MaxPlayers}/{MaxPlayers})";
                return;
            }

            response.Approved = true;
            response.CreatePlayerObject = true;
            int slot = nm.ConnectedClientsIds.Count;
            Vector3 basePos = new Vector3(0f, 1.0f, 10f);
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc) basePos = pc.transform.position;
            response.Position = RiftboundCoopPlayer.ComputeSpawnPosition(basePos, slot);
            response.Rotation = Quaternion.identity;
            response.Pending = false;
        }

        void OnClientConnected(ulong clientId)
        {
            if (nm != null && nm.IsServer && clientId != nm.LocalClientId && RiftboundCoopPlayer.LocalOwner != null)
            {
                RiftboundCoopPlayer.LocalOwner.SendWorldSnapshotToClient(clientId);
            }
            if (nm != null && clientId == nm.LocalClientId)
            {
                if (CoopSessionConfig.IsOnlineMode)
                    statusText = nm.IsHost ? $"Online Host ({OnlineSessionService.ActiveJoinCode})" : $"Online Co-op ({OnlineSessionService.ActiveJoinCode})";
                else
                    statusText = nm.IsHost ? $"Dev LAN Host :{CoopSessionConfig.Port}" : $"Connected to {CoopSessionConfig.JoinIp}:{CoopSessionConfig.Port}";
            }
        }

        void OnClientDisconnected(ulong clientId)
        {
            if (shuttingDown || nm == null) return;

            if (clientId == nm.LocalClientId && !nm.IsServer)
            {
                string reason = string.IsNullOrEmpty(nm.DisconnectReason) ? "Host closed the session" : nm.DisconnectReason;
                CoopSessionConfig.LastDisconnectReason = reason;
                statusText = $"Disconnected: {reason}";
                hostDisconnected = true;
                hostDisconnectMessage = $"{reason}. Host exit ends the session (no host migration).";
                readyCheckOpen = false;
                activePingExpires = 0f;
                Debug.LogWarning($"[CoopWorldManager] {statusText}");
            }
        }

        public async void DisconnectAndReturnToMenu()
        {
            if (shuttingDown) return;
            shuttingDown = true;

            OnlineSessionService.CancelPendingAttempt();
            if (OnlineSessionService.HasActiveSession)
            {
                await OnlineSessionService.LeaveSessionAsync();
            }

            if (nm != null)
            {
                nm.OnClientConnectedCallback -= OnClientConnected;
                nm.OnClientDisconnectCallback -= OnClientDisconnected;
                if (nm.IsListening) nm.Shutdown();
                Destroy(nm.gameObject);
                nm = null;
            }

            CoopSessionConfig.ResetToOffline();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (nm != null)
            {
                nm.OnClientConnectedCallback -= OnClientConnected;
                nm.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        void OnGUI()
        {
            if (!CoopSessionConfig.IsMultiplayer || FrontendMenu.IsOpen) return;

            if (CoopSessionConfig.IsOnlineMode && OnlineSessionService.IsBusy)
            {
                DrawCenteredStatusBox("CONNECTING TO ONLINE CO-OP", OnlineSessionService.StatusMessage, showRetry: false);
                return;
            }

            if (CoopSessionConfig.IsOnlineMode && OnlineSessionService.State == OnlineSessionState.Failed)
            {
                DrawCenteredStatusBox("ONLINE CO-OP ERROR", OnlineSessionService.LastError, showRetry: true);
                return;
            }

            if (hostDisconnected)
            {
                DrawCenteredStatusBox("SESSION ENDED", hostDisconnectMessage, showRetry: false, cancelLabel: "Back to Menu");
                return;
            }

            int count = RiftboundCoopPlayer.AllPlayers.Count;
            float w = 265f;
            float h = CoopSessionConfig.IsOnlineMode && !string.IsNullOrEmpty(OnlineSessionService.ActiveJoinCode) ? 132f : 124f;
            float x = Screen.width - w - 16f;
            float y = 16f;

            GUI.color = new Color(0.06f, 0.08f, 0.11f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(x + 10f, y + 6f, w - 20f, 20f), $"{statusText} ({count}/{MaxPlayers})");
            GUI.Label(new Rect(x + 10f, y + 24f, w - 20f, 18f), "Session-only progress (offline saves safe)");

            float btnY = y + 46f;
            if (CoopSessionConfig.IsOnlineMode && !string.IsNullOrEmpty(OnlineSessionService.ActiveJoinCode))
            {
                GUI.Label(new Rect(x + 10f, btnY, 145f, 22f), $"Code: {OnlineSessionService.ActiveJoinCode}");
                if (GUI.Button(new Rect(x + 155f, btnY, w - 165f, 22f), "Copy Code"))
                {
                    GUIUtility.systemCopyBuffer = OnlineSessionService.ActiveJoinCode;
                }
                btnY += 26f;
            }

            float halfW = (w - 26f) * 0.5f;
            if (GUI.Button(new Rect(x + 10f, btnY, halfW, 22f), "Ping [F6]"))
            {
                RiftboundCoopPlayer.LocalOwner?.RequestMapPing(RiftboundCoopPlayer.LocalOwner.transform.position);
            }
            if (GUI.Button(new Rect(x + 16f + halfW, btnY, halfW, 22f), "Ready [F7]"))
            {
                RiftboundCoopPlayer.LocalOwner?.RequestReadyCheck();
            }
            btnY += 26f;

            if (GUI.Button(new Rect(x + 10f, btnY, w - 20f, 24f), "Leave Session"))
            {
                DisconnectAndReturnToMenu();
            }

            DrawPartyFramesAndCoopExtras();
        }

        void DrawPartyFramesAndCoopExtras()
        {
            if (RiftboundCoopPlayer.AllPlayers.Count > 1)
            {
                float py = 180f;
                foreach (var p in RiftboundCoopPlayer.AllPlayers)
                {
                    if (!p || !p.IsSpawned || p.IsOwner) continue;
                    GUI.color = new Color(0.05f, 0.07f, 0.1f, 0.84f);
                    GUI.DrawTexture(new Rect(16f, py, 215f, 44f), Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    string readyMark = p.ReadyStatus.Value == 1 ? " [OK]" : (p.ReadyStatus.Value == 2 ? " [NO]" : "");
                    string downMark = p.Health.Value <= 0 ? " [DOWN - E Revive]" : (p.IsMounted.Value ? " [Mount]" : "");
                    GUI.Label(new Rect(24f, py + 4f, 200f, 18f), $"{p.PlayerName.Value} Lv{p.Level.Value} {p.ClassName.Value}{readyMark}{downMark}");

                    float pct = p.MaxHealth.Value > 0 ? Mathf.Clamp01((float)p.Health.Value / p.MaxHealth.Value) : 1f;
                    GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
                    GUI.DrawTexture(new Rect(24f, py + 26f, 198f, 8f), Texture2D.whiteTexture);
                    GUI.color = p.Health.Value <= 0 ? new Color(0.55f, 0.2f, 0.2f) : new Color(0.25f, 0.85f, 0.35f);
                    GUI.DrawTexture(new Rect(25f, py + 27f, 196f * pct, 6f), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    py += 50f;
                }
            }

            if (Time.time < activePingExpires && Camera.main)
            {
                Vector3 sp = Camera.main.WorldToScreenPoint(activePingWorldPos + Vector3.up * 2.5f);
                if (sp.z > 0.1f)
                {
                    GUI.color = new Color(1f, 0.82f, 0.2f, 0.95f);
                    GUI.Label(new Rect(sp.x - 80f, Screen.height - sp.y - 20f, 160f, 24f), $"★ PING ({activePingSender})");
                    GUI.color = Color.white;
                }
            }

            if (readyCheckOpen)
            {
                float rw = 300f;
                float rx = (Screen.width - rw) * 0.5f;
                float ry = 90f;
                GUI.color = new Color(0.06f, 0.08f, 0.12f, 0.92f);
                GUI.DrawTexture(new Rect(rx, ry, rw, 84f), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(rx + 14f, ry + 8f, rw - 28f, 22f), $"Ready Check from {readyCheckInitiator}");
                if (GUI.Button(new Rect(rx + 14f, ry + 38f, 128f, 30f), "Ready"))
                {
                    RiftboundCoopPlayer.LocalOwner?.RespondReadyCheck(true);
                    readyCheckOpen = false;
                }
                if (GUI.Button(new Rect(rx + 158f, ry + 38f, 128f, 30f), "Not Ready"))
                {
                    RiftboundCoopPlayer.LocalOwner?.RespondReadyCheck(false);
                    readyCheckOpen = false;
                }
            }
        }

        void DrawCenteredStatusBox(string title, string message, bool showRetry, string cancelLabel = "Cancel")
        {
            float w = 440f;
            float h = 150f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.42f;

            GUI.color = new Color(0.05f, 0.06f, 0.09f, 0.94f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(x + 18f, y + 14f, w - 36f, 24f), title);
            GUI.Label(new Rect(x + 18f, y + 44f, w - 36f, 48f), message);

            if (showRetry)
            {
                if (GUI.Button(new Rect(x + 18f, y + 104f, 180f, 30f), "Retry"))
                {
                    OnlineSessionService.ClearError();
                    StartSelectedSessionMode();
                }
                if (GUI.Button(new Rect(x + 242f, y + 104f, 180f, 30f), "Back to Menu"))
                {
                    DisconnectAndReturnToMenu();
                }
            }
            else
            {
                if (GUI.Button(new Rect(x + 130f, y + 104f, 180f, 30f), cancelLabel))
                {
                    DisconnectAndReturnToMenu();
                }
            }
        }

        static RiftboundCoopPlayer GetFirstRemotePlayer()
        {
            foreach (var p in RiftboundCoopPlayer.AllPlayers)
                if (p && p.IsSpawned && p != RiftboundCoopPlayer.LocalOwner)
                    return p;
            return null;
        }

        IEnumerator RunUiSmokeWorldRoutine()
        {
            float t = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t < 25f &&
                   (OnlineSessionService.State != OnlineSessionState.InWorld || string.IsNullOrEmpty(OnlineSessionService.ActiveJoinCode) || RiftboundCoopPlayer.LocalOwner == null))
                yield return new WaitForSeconds(0.25f);

            bool worldEntered = !FrontendMenu.IsOpen &&
                                OnlineSessionService.State == OnlineSessionState.InWorld &&
                                !string.IsNullOrEmpty(OnlineSessionService.ActiveJoinCode) &&
                                RiftboundCoopPlayer.LocalOwner != null;
            if (!worldEntered)
            {
                string failMsg = $"FAIL UiFlow stage1: menu={FrontendMenu.IsOpen}, state={OnlineSessionService.State}, codeEmpty={string.IsNullOrEmpty(OnlineSessionService.ActiveJoinCode)}";
                Debug.Log(failMsg);
                System.Console.WriteLine(failMsg);
                Application.Quit(1);
                yield break;
            }
            UiSmokeStage = 2;
            DisconnectAndReturnToMenu();
        }

        IEnumerator FinishUiSmokeRoutine()
        {
            yield return new WaitForSecondsRealtime(0.6f);
            int remainingNm = FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length;
            int cams = 0;
            foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if (c.enabled && c.gameObject.activeInHierarchy) cams++;
            int listeners = 0;
            foreach (var al in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                if (al.enabled && al.gameObject.activeInHierarchy) listeners++;
            bool pass = FrontendMenu.IsOpen &&
                        OnlineSessionService.State == OnlineSessionState.Offline &&
                        CoopSessionConfig.Mode == CoopSessionMode.Offline &&
                        remainingNm == 0 &&
                        cams >= 1 &&
                        listeners == 1;
            string msg = $"{(pass ? "PASS" : "FAIL")} UiFlow: menuOpen={FrontendMenu.IsOpen}, state={OnlineSessionService.State}, mode={CoopSessionConfig.Mode}, nm={remainingNm}, cams={cams}, listeners={listeners}";
            Debug.Log(msg);
            System.Console.WriteLine(msg);
            Time.timeScale = 1f;
            yield return new WaitForSecondsRealtime(0.2f);
            Application.Quit(pass ? 0 : 1);
        }

        IEnumerator FinishClientLeaveSmokeRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            int remainingNm = FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length;
            bool offlineClean = OnlineSessionService.State == OnlineSessionState.Offline &&
                                CoopSessionConfig.Mode == CoopSessionMode.Offline &&
                                remainingNm == 0;

            if (clientLeaveStage == 1)
            {
                if (!offlineClean || string.IsNullOrEmpty(clientLeaveRejoinCode))
                {
                    string failMsg = $"FAIL ClientLeave(stage1): questAccepted={clientLeaveQuestAccepted}, mounted={clientLeaveMounted}, nmAfterLeave={remainingNm}, finalState={OnlineSessionService.State}";
                    Debug.Log(failMsg);
                    System.Console.WriteLine(failMsg);
                    yield return new WaitForSeconds(0.2f);
                    Application.Quit(1);
                    yield break;
                }

                clientLeaveStage = 2;
                CoopSessionConfig.Mode = CoopSessionMode.JoinOnline;
                CoopSessionConfig.JoinCode = clientLeaveRejoinCode;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                yield break;
            }

            bool pass = offlineClean &&
                        clientLeaveQuestAccepted &&
                        clientLeaveMounted &&
                        clientLeaveRejoinOk;
            string msg = $"{(pass ? "PASS" : "FAIL")} ClientLeave: questAccepted={clientLeaveQuestAccepted}, mounted={clientLeaveMounted}, rejoined={clientLeaveRejoinOk}, nmAfterLeave={remainingNm}, finalState={OnlineSessionService.State}";
            Debug.Log(msg);
            System.Console.WriteLine(msg);
            yield return new WaitForSeconds(0.2f);
            Application.Quit(pass ? 0 : 1);
        }

        IEnumerator FinishResilienceSmokeRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            int remainingNm = FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length;
            bool pass = OnlineSessionService.State == OnlineSessionState.Offline &&
                        CoopSessionConfig.Mode == CoopSessionMode.Offline &&
                        remainingNm == 0 &&
                        resilienceCancelOk;
            string msg = $"{(pass ? "PASS" : "FAIL")} Resilience: cancelOk={resilienceCancelOk}, codes={resilienceCode1}, nmDuringHost={resilienceNmCount}, nmAfterLeave={remainingNm}, finalState={OnlineSessionService.State}";
            Debug.Log(msg);
            System.Console.WriteLine(msg);
            yield return new WaitForSeconds(0.2f);
            Application.Quit(pass ? 0 : 1);
        }

        IEnumerator RunCoopSmokeRoutine(float timeoutSeconds)
        {
            float start = Time.realtimeSinceStartup;
            if (CoopSessionConfig.InvalidCodeSmoke)
            {
                while (Time.realtimeSinceStartup - start < timeoutSeconds && OnlineSessionService.State != OnlineSessionState.Failed)
                    yield return new WaitForSeconds(0.2f);

                bool invPass = OnlineSessionService.State == OnlineSessionState.Failed && !string.IsNullOrEmpty(OnlineSessionService.LastError);
                string invMsg = $"{(invPass ? "PASS" : "FAIL")} InvalidCode: state={OnlineSessionService.State}, err={OnlineSessionService.LastError}";
                Debug.Log(invMsg);
                System.Console.WriteLine(invMsg);
                yield return new WaitForSeconds(0.2f);
                Application.Quit(invPass ? 0 : 1);
                yield break;
            }

            if (CoopSessionConfig.ResilienceSmoke)
            {
                if (resilienceStage == -1)
                {
                    float waitBusy = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - waitBusy < 5f && !OnlineSessionService.IsBusy)
                        yield return null;
                    yield return new WaitForSeconds(0.05f);
                    resilienceStage = -2;
                    DisconnectAndReturnToMenu();
                    yield break;
                }
                else if (resilienceStage == 0)
                {
                    while (Time.realtimeSinceStartup - start < timeoutSeconds * 0.45f &&
                           (OnlineSessionService.State != OnlineSessionState.InWorld || string.IsNullOrEmpty(OnlineSessionService.ActiveJoinCode)))
                        yield return new WaitForSeconds(0.25f);
                    resilienceCode1 = OnlineSessionService.ActiveJoinCode;
                    resilienceStage = 1;
                    DisconnectAndReturnToMenu();
                    yield break;
                }
                else if (resilienceStage == 2)
                {
                    while (Time.realtimeSinceStartup - start < timeoutSeconds * 0.45f &&
                           (OnlineSessionService.State != OnlineSessionState.InWorld || string.IsNullOrEmpty(OnlineSessionService.ActiveJoinCode)))
                        yield return new WaitForSeconds(0.25f);
                    string code2 = OnlineSessionService.ActiveJoinCode;
                    resilienceNmCount = FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length;
                    bool stage2Ok = !string.IsNullOrEmpty(resilienceCode1) && !string.IsNullOrEmpty(code2) &&
                                    code2 != resilienceCode1 && resilienceNmCount == 1;
                    if (!stage2Ok)
                    {
                        string failMsg = $"FAIL Resilience stage2: code1={resilienceCode1}, code2={code2}, nmCount={resilienceNmCount}";
                        Debug.Log(failMsg);
                        System.Console.WriteLine(failMsg);
                        Application.Quit(1);
                        yield break;
                    }
                    resilienceCode1 = $"{resilienceCode1}->{code2}";
                    resilienceStage = 3;
                    DisconnectAndReturnToMenu();
                    yield break;
                }
            }

            bool isHost = CoopSessionConfig.Mode == CoopSessionMode.HostLan || CoopSessionConfig.Mode == CoopSessionMode.HostOnline;
            string role = isHost ? "CoopHost" : "CoopClient";
            int minPlayers = Mathf.Max(2, CoopSessionConfig.ExpectedMinPlayers);

            bool hostPreDamaged = false;
            if (CoopSessionConfig.LateJoinSmoke && isHost)
            {
                while (Time.realtimeSinceStartup - start < timeoutSeconds * 0.5f && (nm == null || !nm.IsListening || RiftboundCoopPlayer.LocalOwner == null))
                    yield return new WaitForSeconds(0.2f);
                var preEnemy = EnemyController.FindByKey("road-raider-1");
                if (preEnemy)
                {
                    var preStats = preEnemy.GetComponent<Combatant>();
                    if (preStats && preStats.Alive)
                    {
                        preStats.Damage(30f);
                        hostPreDamaged = preStats.Health < preStats.maxHealth;
                        Debug.Log($"[LateJoinSmoke] Host pre-damaged {preEnemy.NetworkKey} to {preStats.Health:F0}/{preStats.maxHealth:F0}");
                    }
                }
                if (LivingWorldEvents.Instance != null) LivingWorldEvents.Instance.TriggerCaravanEvent();
                if (RiftboundCoopPlayer.LocalOwner != null) RiftboundCoopPlayer.LocalOwner.RequestReadyCheck();
            }

            while (Time.realtimeSinceStartup - start < timeoutSeconds)
            {
                if (nm != null && nm.IsListening && RiftboundCoopPlayer.AllPlayers.Count >= minPlayers && RiftboundCoopPlayer.LocalOwner != null)
                {
                    break;
                }
                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(0.8f);

            int players = RiftboundCoopPlayer.AllPlayers.Count;
            int activeCams = 0;
            foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if (c.enabled && c.gameObject.activeInHierarchy) activeCams++;
            int activeListeners = 0;
            foreach (var al in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                if (al.enabled && al.gameObject.activeInHierarchy) activeListeners++;

            float sep = players >= 2
                ? Vector3.Distance(RiftboundCoopPlayer.AllPlayers[0].transform.position, RiftboundCoopPlayer.AllPlayers[1].transform.position)
                : 0f;
            string posInfo = players >= 2
                ? $"p0={RiftboundCoopPlayer.AllPlayers[0].transform.position}, p1={RiftboundCoopPlayer.AllPlayers[1].transform.position}"
                : "p<2";

            bool combatPass = true;
            string combatInfo = "";
            if (minPlayers >= 4 && players >= 4)
            {
                var frameSamples = new List<float>(30);
                for (int s = 0; s < 30; s++)
                {
                    yield return null;
                    frameSamples.Add(Time.unscaledDeltaTime * 1000f);
                }
                frameSamples.Sort();
                float sumMs = 0f;
                for (int s = 0; s < frameSamples.Count; s++) sumMs += frameSamples[s];
                float avgMs = frameSamples.Count > 0 ? sumMs / frameSamples.Count : 0f;
                float p95Ms = frameSamples.Count > 0 ? frameSamples[Mathf.Clamp((int)(frameSamples.Count * 0.95f), 0, frameSamples.Count - 1)] : 0f;
                int netCount = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None).Length;
                combatInfo += $", telemetry(avgMs={avgMs:F2}, p95Ms={p95Ms:F2}, enemies={EnemyController.AllByKey.Count}, netObjs={netCount})";
            }
            if (CoopSessionConfig.CombatSmoke && players >= 2 && RiftboundCoopPlayer.LocalOwner != null)
            {
                EnemyController targetEnemy = null;
                float bestDist = float.MaxValue;
                Vector3 refPos = RiftboundCoopPlayer.AllPlayers[0].transform.position;
                foreach (var kv in EnemyController.AllByKey)
                {
                    if (kv.Value && !kv.Value.trainingDummy)
                    {
                        var st = kv.Value.GetComponent<Combatant>();
                        if (st && st.Alive)
                        {
                            float d = Vector3.Distance(refPos, kv.Value.transform.position);
                            if (d < bestDist) { bestDist = d; targetEnemy = kv.Value; }
                        }
                    }
                }

                if (targetEnemy)
                {
                    var enemyStats = targetEnemy.GetComponent<Combatant>();
                    float initialHp = enemyStats.Health;
                    Vector3 combatSpot = targetEnemy.transform.position + new Vector3(4f, 0f, 0f);
                    if (isHost)
                    {
                        var clientAvatar = RiftboundCoopPlayer.FindByClientId(1);
                        if (clientAvatar != null)
                        {
                            clientAvatar.AuthorizeHostTeleport(combatSpot);
                            RiftboundCoopPlayer.DispatchEnemyHitToPlayer(1, 10f, targetEnemy.NetworkKey);
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.6f);
                        string ab = RiftboundCoopPlayer.LocalOwner.ClassName.Value.ToString() == "Mage" ? "fireball" : "melee";
                        RiftboundCoopPlayer.LocalOwner.RequestAbilityAttack(enemyStats, ab);
                    }

                    float waitStart = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - waitStart < 7f && enemyStats.Health >= initialHp)
                        yield return new WaitForSeconds(0.15f);

                    bool clientTookDamage = true;
                    if (!isHost)
                    {
                        var lp = FindFirstObjectByType<PlayerController>();
                        float waitDmg = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitDmg < 3f && lp && lp.Stats && lp.Stats.Health >= lp.Stats.maxHealth)
                            yield return new WaitForSeconds(0.15f);
                        clientTookDamage = lp && lp.Stats && lp.Stats.Health < lp.Stats.maxHealth;
                    }

                    combatPass = enemyStats.Health < initialHp && clientTookDamage;
                    combatInfo = $", enemy={targetEnemy.NetworkKey} hp={enemyStats.Health:F0}/{initialHp:F0}, clientDamaged={clientTookDamage}";
                }
                else
                {
                    combatPass = false;
                    combatInfo = ", enemy=none";
                }
            }

            bool lateJoinPass = true;
            if (CoopSessionConfig.LateJoinSmoke && players >= 2)
            {
                yield return new WaitForSeconds(1.8f);
                var ljEnemy = EnemyController.FindByKey("road-raider-1");
                var ljStats = ljEnemy ? ljEnemy.GetComponent<Combatant>() : null;
                bool evtActive = LivingWorldEvents.Instance != null && LivingWorldEvents.Instance.CaravanState == LivingWorldEvents.EventState.Active;
                if (isHost)
                {
                    var clientAvatar = GetFirstRemotePlayer();
                    float wDecline = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - wDecline < 4f && (clientAvatar == null || clientAvatar.ReadyStatus.Value != 2))
                        yield return new WaitForSeconds(0.15f);
                    bool rcDeclineOk = clientAvatar != null && clientAvatar.ReadyStatus.Value == 2;
                    lateJoinPass = ljStats != null && ljStats.Health < ljStats.maxHealth && hostPreDamaged && evtActive && rcDeclineOk;
                    combatInfo += $", lateJoin={(lateJoinPass ? "PASS" : "FAIL")}({(ljStats ? $"{ljStats.Health:F0}/{ljStats.maxHealth:F0}" : "none")},evt={evtActive},rcDecline={rcDeclineOk})";
                }
                else
                {
                    bool rcOpen = IsReadyCheckOpenForTest;
                    if (RiftboundCoopPlayer.LocalOwner != null)
                        RiftboundCoopPlayer.LocalOwner.RespondReadyCheck(false);
                    yield return new WaitForSeconds(0.6f);
                    lateJoinPass = ljStats != null && ljStats.Health < ljStats.maxHealth && evtActive && rcOpen;
                    combatInfo += $", lateJoin={(lateJoinPass ? "PASS" : "FAIL")}({(ljStats ? $"{ljStats.Health:F0}/{ljStats.maxHealth:F0}" : "none")},evt={evtActive},rcSnap={rcOpen})";
                }
            }

            bool progPass = true;
            if (CoopSessionConfig.ProgressionSmoke && players >= 2 && RiftboundCoopPlayer.LocalOwner != null)
            {
                var targetEnemy = EnemyController.FindByKey("road-raider-1");
                LootChest targetChest = FindDeterministicTestChest(15, false);

                var enemyStats = targetEnemy ? targetEnemy.GetComponent<Combatant>() : null;
                var lp = FindFirstObjectByType<PlayerController>();
                TownNPC elara = null;
                foreach (var n in FindObjectsByType<TownNPC>(FindObjectsSortMode.None))
                    if (n && n.npcName == ExplorationSites.CartographerName) { elara = n; break; }
                Landmark sv0 = null, sv1 = null, sv2 = null;
                foreach (var lm in FindObjectsByType<Landmark>(FindObjectsSortMode.None))
                {
                    if (!lm) continue;
                    if (lm.landmarkId == "westbank-overlook") sv0 = lm;
                    else if (lm.landmarkId == "bloodrock-vista") sv1 = lm;
                    else if (lm.landmarkId == "wyrmcrest-lookout") sv2 = lm;
                }
                if (targetEnemy && enemyStats && targetChest && lp && elara && sv0 && sv1 && sv2)
                {
                    if (isHost)
                    {
                        var clientAvatar = GetFirstRemotePlayer();
                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(targetEnemy.transform.position + new Vector3(4f, 0f, 0f));

                        float waitKill = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitKill < 15f && enemyStats.Alive)
                            yield return new WaitForSeconds(0.2f);

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(targetChest.transform.position + new Vector3(1.5f, 0f, 0f));

                        float waitLoot = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitLoot < 8f && (clientAvatar == null || clientAvatar.Gold.Value < 18))
                            yield return new WaitForSeconds(0.25f);
                        yield return new WaitForSeconds(1.8f);

                        if (clientAvatar != null)
                        {
                            int peakGold = clientAvatar.Gold.Value;
                            float waitBuy = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - waitBuy < 4f && clientAvatar.Gold.Value > peakGold - 10)
                                yield return new WaitForSeconds(0.15f);

                            RiftboundCoopPlayer.DispatchEnemyHitToPlayer(clientAvatar.OwnerClientId, 999f, "smoke-test");
                            yield return new WaitForSeconds(0.2f);
                            float waitRespH = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - waitRespH < 5f && clientAvatar.Health.Value < clientAvatar.MaxHealth.Value)
                                yield return new WaitForSeconds(0.15f);
                        }

                        // Teleport Client to Cartographer Elara -> 3 survey sites -> Elara for optional bounty test
                        Landmark[] svs = { sv0, sv1, sv2 };
                        if (clientAvatar != null)
                        {
                            clientAvatar.AuthorizeHostTeleport(elara.transform.position + new Vector3(1.5f, 0f, 0f));
                            yield return new WaitForSeconds(1.2f);
                            for (int s = 0; s < 3; s++)
                            {
                                int gBefore = clientAvatar.Gold.Value;
                                clientAvatar.AuthorizeHostTeleport(svs[s].transform.position + new Vector3(1.5f, 0f, 0f));
                                float wSv = Time.realtimeSinceStartup;
                                while (Time.realtimeSinceStartup - wSv < 6f && clientAvatar.Gold.Value < gBefore + 15)
                                    yield return new WaitForSeconds(0.2f);
                            }
                            int gBeforeTurnIn = clientAvatar.Gold.Value;
                            clientAvatar.AuthorizeHostTeleport(elara.transform.position + new Vector3(1.5f, 0f, 0f));
                            float wBounty = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - wBounty < 8f && clientAvatar.Gold.Value < gBeforeTurnIn + 75)
                                yield return new WaitForSeconds(0.2f);
                            yield return new WaitForSeconds(0.8f);
                        }

                        Vector3 hostChestSpot = targetChest.transform.position + new Vector3(1.5f, 0f, 0f);
                        RiftboundCoopPlayer.LocalOwner.AuthorizeHostTeleport(hostChestSpot);
                        yield return new WaitForSeconds(0.25f);
                        bool hostChestOk = lp.OpenChest(targetChest);
                        progPass = !enemyStats.Alive && hostChestOk;
                        combatInfo += $", hostProg={(progPass ? "PASS" : "FAIL")}(dead={!enemyStats.Alive}, chest={hostChestOk})";
                    }
                    else
                    {
                        float waitTp = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitTp < 8f && Vector3.Distance(lp.transform.position, targetEnemy.transform.position) > 9f)
                            yield return new WaitForSeconds(0.2f);

                        float waitKill = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitKill < 14f && enemyStats.Alive && RiftboundCoopPlayer.LocalOwner != null)
                        {
                            string ab = RiftboundCoopPlayer.LocalOwner.ClassName.Value.ToString() == "Mage" ? "fireball" : "melee";
                            RiftboundCoopPlayer.LocalOwner.RequestAbilityAttack(enemyStats, ab);
                            yield return new WaitForSeconds(0.55f);
                        }
                        yield return new WaitForSeconds(0.8f);
                        if (RiftboundCoopPlayer.LocalOwner == null)
                        {
                            progPass = false;
                            combatInfo += ", clientProg=FAIL(disconnected)";
                        }
                        else
                        {
                            bool killCreditOk = !enemyStats.Alive && RiftboundCoopPlayer.LocalOwner.Gold.Value > 0 &&
                                                (RiftboundCoopPlayer.LocalOwner.Experience.Value > 0 || RiftboundCoopPlayer.LocalOwner.Level.Value > 1);

                            RiftboundCoopPlayer.LocalOwner.RequestCompleteQuestServerValidated(0);
                            yield return new WaitForSeconds(0.8f);
                            bool questRejectOk = RiftboundCoopPlayer.LocalOwner.QuestIndex.Value == 0;

                            float waitChestTp = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - waitChestTp < 8f && Vector3.Distance(lp.transform.position, targetChest.transform.position) > 4f)
                                yield return new WaitForSeconds(0.2f);

                            int goldBeforeChest = RiftboundCoopPlayer.LocalOwner.Gold.Value;
                            lp.OpenChest(targetChest);
                            yield return new WaitForSeconds(1.2f);
                            bool chest1Ok = lp.HasOpenedChest(targetChest.chestId) && RiftboundCoopPlayer.LocalOwner.Gold.Value > goldBeforeChest;

                            int goldAfterChest1 = RiftboundCoopPlayer.LocalOwner.Gold.Value;
                            RiftboundCoopPlayer.LocalOwner.RequestOpenChestServerValidated(targetChest);
                            yield return new WaitForSeconds(0.8f);
                            bool chestDupRejectOk = RiftboundCoopPlayer.LocalOwner.Gold.Value == goldAfterChest1;

                            RiftboundCoopPlayer.LocalOwner.RequestTransaction("temper_weapon");
                            yield return new WaitForSeconds(0.6f);
                            bool txRejectOk = !lp.TemperedWeapon && RiftboundCoopPlayer.LocalOwner.Gold.Value == goldAfterChest1;

                            int potsBefore = lp.Potions;
                            RiftboundCoopPlayer.LocalOwner.RequestTransaction("buy_potion");
                            yield return new WaitForSeconds(0.8f);
                            bool txBuyOk = RiftboundCoopPlayer.LocalOwner.Gold.Value == goldAfterChest1 - 10 && lp.Potions == Mathf.Min(10, potsBefore + 1);

                            float waitDie = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - waitDie < 6f && RiftboundCoopPlayer.LocalOwner.Health.Value > 0)
                                yield return new WaitForSeconds(0.15f);
                            bool didDie = RiftboundCoopPlayer.LocalOwner.Health.Value <= 0;
                            if (didDie)
                                RiftboundCoopPlayer.LocalOwner.RequestRespawn();
                            float waitResp = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - waitResp < 3f &&
                                   (RiftboundCoopPlayer.LocalOwner.Health.Value < RiftboundCoopPlayer.LocalOwner.MaxHealth.Value ||
                                    Vector3.Distance(lp.transform.position, targetChest.transform.position) <= 5f))
                                yield return new WaitForSeconds(0.15f);
                            bool respawnOk = didDie &&
                                             RiftboundCoopPlayer.LocalOwner.Health.Value == RiftboundCoopPlayer.LocalOwner.MaxHealth.Value &&
                                             Vector3.Distance(lp.transform.position, targetChest.transform.position) > 5f;

                            // Optional Bounty (Cartographer frontier_cartography): premature reject -> 3 surveys -> turn in once -> duplicate reject
                            bool bountyOk = false;
                            var fq = FactionQuestSystem.Instance;
                            if (fq != null)
                            {
                                float wElara = Time.realtimeSinceStartup;
                                while (Time.realtimeSinceStartup - wElara < 10f && Vector3.Distance(lp.transform.position, elara.transform.position) > 5f)
                                    yield return new WaitForSeconds(0.15f);
                                var qDef = fq.GetAvailableQuestForNPC(ExplorationSites.CartographerName, lp);
                                if (qDef != null && qDef.id == "frontier_cartography")
                                {
                                    fq.AcceptQuest(qDef, lp);
                                    int gPre = RiftboundCoopPlayer.LocalOwner.Gold.Value;
                                    RiftboundCoopPlayer.LocalOwner.RequestCompleteFactionQuestServerValidated("frontier_cartography");
                                    yield return new WaitForSeconds(0.55f);
                                    bool preRej = !fq.IsQuestCompleted("frontier_cartography") && RiftboundCoopPlayer.LocalOwner.Gold.Value == gPre;

                                    Landmark[] svs = { sv0, sv1, sv2 };
                                    for (int s = 0; s < 3; s++)
                                    {
                                        float wS = Time.realtimeSinceStartup;
                                        while (Time.realtimeSinceStartup - wS < 8f && Vector3.Distance(lp.transform.position, svs[s].transform.position) > 5f)
                                            yield return new WaitForSeconds(0.15f);
                                        int gS = RiftboundCoopPlayer.LocalOwner.Gold.Value;
                                        lp.DiscoverLandmark(svs[s].landmarkId);
                                        float wGs = Time.realtimeSinceStartup;
                                        while (Time.realtimeSinceStartup - wGs < 5f && RiftboundCoopPlayer.LocalOwner.Gold.Value < gS + 15)
                                            yield return new WaitForSeconds(0.15f);
                                    }

                                    float wRet = Time.realtimeSinceStartup;
                                    while (Time.realtimeSinceStartup - wRet < 8f && Vector3.Distance(lp.transform.position, elara.transform.position) > 5f)
                                        yield return new WaitForSeconds(0.15f);
                                    int gBeforeTurn = RiftboundCoopPlayer.LocalOwner.Gold.Value;
                                    fq.CompleteQuest(lp);
                                    float wDone = Time.realtimeSinceStartup;
                                    while (Time.realtimeSinceStartup - wDone < 5f && !fq.IsQuestCompleted("frontier_cartography"))
                                        yield return new WaitForSeconds(0.15f);
                                    int gAfterTurn = RiftboundCoopPlayer.LocalOwner.Gold.Value;
                                    RiftboundCoopPlayer.LocalOwner.RequestCompleteFactionQuestServerValidated("frontier_cartography");
                                    yield return new WaitForSeconds(0.55f);
                                    bool dupBountyRej = RiftboundCoopPlayer.LocalOwner.Gold.Value == gAfterTurn;
                                    bountyOk = preRej && fq.IsQuestCompleted("frontier_cartography") && gAfterTurn >= gBeforeTurn + 75 && dupBountyRej;
                                }
                            }

                            progPass = killCreditOk && questRejectOk && chest1Ok && chestDupRejectOk && txRejectOk && txBuyOk && respawnOk && bountyOk;
                            combatInfo += $", clientProg={(progPass ? "PASS" : "FAIL")}(kill={killCreditOk},qRej={questRejectOk},c1={chest1Ok},cDup={chestDupRejectOk},txRej={txRejectOk},txBuy={txBuyOk},resp={respawnOk},bounty={bountyOk})";
                        }
                    }
                }
                else
                {
                    progPass = false;
                    combatInfo += ", prog=FAIL(missingEntity)";
                }
            }

            bool clientLeavePass = true;
            if (CoopSessionConfig.ClientLeaveSmoke && players >= 2 && RiftboundCoopPlayer.LocalOwner != null)
            {
                if (!isHost && clientLeaveStage == 2)
                {
                    yield return new WaitForSeconds(0.4f);
                    int rCams = 0;
                    foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None)) if (c.enabled) rCams++;
                    int rListeners = 0;
                    foreach (var l in FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) if (l.enabled) rListeners++;
                    clientLeaveRejoinOk = RiftboundCoopPlayer.AllPlayers.Count >= 2 &&
                                          RiftboundCoopPlayer.LocalOwner != null &&
                                          rCams == 1 &&
                                          rListeners == 1 &&
                                          !FrontendMenu.IsOpen;
                    clientLeaveStage = 3;
                    DisconnectAndReturnToMenu();
                    yield break;
                }

                var giver = FindFirstObjectByType<TownNPC>();
                if (isHost)
                {
                    var clientAvatar = GetFirstRemotePlayer();
                    if (clientAvatar != null && giver != null)
                        clientAvatar.AuthorizeHostTeleport(giver.transform.position + new Vector3(1.5f, 0f, 0f));

                    float waitMount = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - waitMount < 10f && (clientAvatar == null || !clientAvatar.IsMounted.Value))
                        yield return new WaitForSeconds(0.2f);
                    bool sawMounted = clientAvatar != null && clientAvatar.IsMounted.Value;
                    bool sawQuest = clientAvatar != null && clientAvatar.QuestAccepted.Value;

                    float waitDisc = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - waitDisc < 12f && RiftboundCoopPlayer.AllPlayers.Count > 1)
                        yield return new WaitForSeconds(0.2f);

                    int afterPlayers = RiftboundCoopPlayer.AllPlayers.Count;

                    float waitRejoin = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - waitRejoin < 18f && RiftboundCoopPlayer.AllPlayers.Count < 2)
                        yield return new WaitForSeconds(0.2f);
                    bool rejoined = RiftboundCoopPlayer.AllPlayers.Count >= 2;

                    float waitDisc2 = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - waitDisc2 < 12f && RiftboundCoopPlayer.AllPlayers.Count > 1)
                        yield return new WaitForSeconds(0.2f);
                    int finalPlayers = RiftboundCoopPlayer.AllPlayers.Count;

                    clientLeavePass = sawMounted && sawQuest && afterPlayers == 1 && rejoined && finalPlayers == 1 && nm != null && nm.IsListening;
                    combatInfo += $", hostDisconnectRecovery={(clientLeavePass ? "PASS" : "FAIL")}(mounted={sawMounted},quest={sawQuest},playersAfter={afterPlayers},rejoined={rejoined},finalPlayers={finalPlayers})";
                }
                else
                {
                    if (giver != null)
                    {
                        float waitGiver = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitGiver < 8f &&
                               Vector3.Distance(RiftboundCoopPlayer.LocalOwner.transform.position, giver.transform.position) > 5.5f)
                            yield return new WaitForSeconds(0.2f);
                    }
                    RiftboundCoopPlayer.LocalOwner.RequestAcceptQuest();
                    RiftboundCoopPlayer.LocalOwner.RequestSetMount(true, "horse");
                    float waitSync = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - waitSync < 6f &&
                           (!RiftboundCoopPlayer.LocalOwner.QuestAccepted.Value || !RiftboundCoopPlayer.LocalOwner.IsMounted.Value))
                        yield return new WaitForSeconds(0.2f);

                    clientLeaveQuestAccepted = RiftboundCoopPlayer.LocalOwner.QuestAccepted.Value;
                    clientLeaveMounted = RiftboundCoopPlayer.LocalOwner.IsMounted.Value;
                    clientLeaveRejoinCode = OnlineSessionService.ActiveJoinCode;
                    clientLeaveStage = 1;
                    DisconnectAndReturnToMenu();
                    yield break;
                }
            }

            bool fullAcceptPass = true;
            if (CoopSessionConfig.FullAcceptanceSmoke && players >= 2 && RiftboundCoopPlayer.LocalOwner != null)
            {
                var giver = FindFirstObjectByType<TownNPC>();
                var lp = FindFirstObjectByType<PlayerController>();
                var enemies = new List<EnemyController>();
                foreach (var kv in EnemyController.AllByKey)
                {
                    if (kv.Value && !kv.Value.trainingDummy)
                    {
                        var st = kv.Value.GetComponent<Combatant>();
                        if (st && st.Alive && st.maxHealth <= 110f && kv.Value.gameObject.activeInHierarchy) enemies.Add(kv.Value);
                    }
                }
                enemies.Sort((a, b) => Vector3.Distance(giver.transform.position, a.transform.position).CompareTo(Vector3.Distance(giver.transform.position, b.transform.position)));

                if (giver && enemies.Count >= 3 && lp)
                {
                    if (isHost)
                    {
                        var clientAvatar = GetFirstRemotePlayer();
                        ulong remoteCid = clientAvatar != null ? clientAvatar.OwnerClientId : 1UL;
                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(giver.transform.position + new Vector3(1.5f, 0f, 0f));

                        float waitAcc = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitAcc < 10f && (clientAvatar == null || !clientAvatar.QuestAccepted.Value))
                            yield return new WaitForSeconds(0.2f);

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(enemies[0].transform.position + new Vector3(2.2f, 0f, 0f));

                        yield return new WaitForSeconds(2.2f);
                        RiftboundCoopPlayer.DispatchEnemyHitToPlayer(remoteCid, 30f, enemies[0].NetworkKey);
                        yield return new WaitForSeconds(1.2f);

                        float wait3Kills = Time.realtimeSinceStartup;
                        int lastIdx = 0;
                        while (Time.realtimeSinceStartup - wait3Kills < 26f && clientAvatar != null && clientAvatar.QuestKills.Value < 3)
                        {
                            if (lastIdx < 3)
                            {
                                var e = enemies[lastIdx];
                                var est = e ? e.GetComponent<Combatant>() : null;
                                if (est && !est.Alive)
                                {
                                    lastIdx++;
                                    if (lastIdx < 3)
                                        clientAvatar.AuthorizeHostTeleport(enemies[lastIdx].transform.position + new Vector3(2.2f, 0f, 0f));
                                }
                            }
                            yield return new WaitForSeconds(0.2f);
                        }

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(giver.transform.position + new Vector3(1.5f, 0f, 0f));

                        // Wait for QuestIndex == 1 + Ping + ReadyCheck (give generous synchronized window)
                        float waitSocial = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitSocial < 16f && (clientAvatar == null || clientAvatar.QuestIndex.Value < 1 || !HasActivePingForTest || !IsReadyCheckOpenForTest))
                            yield return new WaitForSeconds(0.2f);

                        bool pingOk = HasActivePingForTest;
                        bool rcOpen = IsReadyCheckOpenForTest;
                        RiftboundCoopPlayer.LocalOwner.RespondReadyCheck(true);
                        yield return new WaitForSeconds(0.4f);
                        bool rcResponded = RiftboundCoopPlayer.LocalOwner.ReadyStatus.Value == 1;

                        // Wait briefly so client reaches waitDown before downing
                        yield return new WaitForSeconds(0.8f);
                        RiftboundCoopPlayer.DispatchEnemyHitToPlayer(remoteCid, 500f, enemies[0].NetworkKey);
                        float waitDown = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitDown < 4f && clientAvatar != null && clientAvatar.Health.Value > 0)
                            yield return new WaitForSeconds(0.15f);

                        if (clientAvatar != null)
                        {
                            RiftboundCoopPlayer.LocalOwner.AuthorizeHostTeleport(clientAvatar.transform.position + new Vector3(1.5f, 0f, 0f));
                            yield return new WaitForSeconds(0.4f);
                            RiftboundCoopPlayer.LocalOwner.RequestReviveAlly(remoteCid);
                        }
                        float waitRev = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitRev < 4f && clientAvatar != null && clientAvatar.Health.Value <= 0)
                            yield return new WaitForSeconds(0.15f);
                        bool reviveOk = clientAvatar != null && clientAvatar.Health.Value > 0;

                        yield return new WaitForSeconds(0.6f);
                        var lwe = LivingWorldEvents.Instance ? LivingWorldEvents.Instance : FindFirstObjectByType<LivingWorldEvents>();
                        if (lwe)
                        {
                            lwe.TriggerCaravanEvent();
                            lwe.TriggerAmbushEvent();
                        }
                        yield return new WaitForSeconds(0.8f);
                        if (clientAvatar != null)
                            RiftboundCoopPlayer.BroadcastWorldEventReward("smoke_event_test", clientAvatar.transform.position, 40, 20);
                        yield return new WaitForSeconds(2.5f);

                        fullAcceptPass = pingOk && rcOpen && rcResponded && reviveOk && clientAvatar != null && clientAvatar.QuestIndex.Value == 1;
                        combatInfo += $", hostAccept={(fullAcceptPass ? "PASS" : "FAIL")}(cid={(clientAvatar ? (long)remoteCid : -1)},ping={pingOk},rc={rcOpen}/{rcResponded},revive={reviveOk},kills={(clientAvatar ? clientAvatar.QuestKills.Value : -1)},qIdx={(clientAvatar ? clientAvatar.QuestIndex.Value : -1)})";
                        string hostMsg = $"{(fullAcceptPass ? "PASS" : "FAIL")} {role}: players={players}, localOwner=True, cams={activeCams}, listeners={activeListeners}, sep={sep:F2}m{combatInfo}";
                        Debug.Log(hostMsg);
                        System.Console.WriteLine(hostMsg);

                        yield return new WaitForSeconds(0.6f);
                        if (OnlineSessionService.HasActiveSession)
                        {
                            var leaveTask = OnlineSessionService.LeaveSessionAsync();
                            float lw = Time.realtimeSinceStartup;
                            while (!leaveTask.IsCompleted && Time.realtimeSinceStartup - lw < 5f)
                                yield return new WaitForSeconds(0.1f);
                        }
                        if (nm != null && nm.IsListening) nm.Shutdown();
                        yield return new WaitForSeconds(0.3f);
                        Application.Quit(fullAcceptPass ? 0 : 1);
                        yield break;
                    }
                    else
                    {
                        var me = RiftboundCoopPlayer.LocalOwner;
                        string myClass = me.ClassName.Value.ToString();
                        string wrongAb = myClass == "Mage" ? "consecration" : "fireball";
                        string rightAb = myClass == "Mage" ? "fireball" : (myClass == "Warrior" ? "whirlwind" : (myClass == "Rogue" ? "eviscerate" : "consecration"));
                        string healAb = myClass == "Mage" ? "arcane_ward" : (myClass == "Paladin" ? "holy_light" : "second_wind");

                        float waitGiver1 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitGiver1 < 10f && Vector3.Distance(lp.transform.position, giver.transform.position) > 5.5f)
                            yield return new WaitForSeconds(0.2f);
                        me.RequestAcceptQuest();
                        float waitAcc = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitAcc < 5f && !me.QuestAccepted.Value)
                            yield return new WaitForSeconds(0.15f);
                        bool questAccepted = me.QuestAccepted.Value;

                        var e0Stats = enemies[0].GetComponent<Combatant>();
                        float waitTp0 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitTp0 < 10f && Vector3.Distance(lp.transform.position, enemies[0].transform.position) > 6f)
                            yield return new WaitForSeconds(0.2f);

                        float hpBeforeSpoof = e0Stats.Health;
                        me.RequestAbilityAttack(e0Stats, wrongAb);
                        yield return new WaitForSeconds(0.55f);
                        bool spoofBlocked = Mathf.Approximately(e0Stats.Health, hpBeforeSpoof);

                        me.RequestAbilityAttack(e0Stats, rightAb);
                        float waitHit = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitHit < 3f && e0Stats.Health >= hpBeforeSpoof)
                            yield return new WaitForSeconds(0.15f);
                        bool validAbHit = e0Stats.Health < hpBeforeSpoof;

                        float waitDmg = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitDmg < 8f && me.Health.Value >= me.MaxHealth.Value)
                            yield return new WaitForSeconds(0.15f);
                        int hpDamaged = me.Health.Value;
                        me.RequestAbilityHeal(40, healAb);
                        float waitHeal = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitHeal < 3f && me.Health.Value <= hpDamaged)
                            yield return new WaitForSeconds(0.15f);
                        bool abHealOk = me.Health.Value > hpDamaged;

                        me.RequestSetMount(true, "horse");
                        float waitMnt = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitMnt < 3f && !me.IsMounted.Value)
                            yield return new WaitForSeconds(0.15f);
                        bool mountedOk = me.IsMounted.Value;
                        me.RequestSetMount(false, "");
                        float waitDism = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitDism < 3f && me.IsMounted.Value)
                            yield return new WaitForSeconds(0.15f);
                        bool dismountOk = !me.IsMounted.Value;

                        for (int k = 0; k < 3; k++)
                        {
                            var ek = enemies[k];
                            var eks = ek.GetComponent<Combatant>();
                            float wt = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - wt < 10f && Vector3.Distance(lp.transform.position, ek.transform.position) > 6f)
                                yield return new WaitForSeconds(0.15f);
                            float wk = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - wk < 10f && eks.Alive && RiftboundCoopPlayer.LocalOwner != null && me.Health.Value > 0)
                            {
                                if (me.Health.Value < 35 && lp.Potions > 0)
                                    me.RequestUsePotion();
                                me.RequestAbilityAttack(eks, "melee");
                                yield return new WaitForSeconds(0.42f);
                            }
                        }

                        float waitGiver2 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitGiver2 < 10f && Vector3.Distance(lp.transform.position, giver.transform.position) > 5.5f)
                            yield return new WaitForSeconds(0.2f);
                        me.RequestCompleteQuestServerValidated(0);
                        float waitQ1 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitQ1 < 5f && me.QuestIndex.Value < 1)
                            yield return new WaitForSeconds(0.15f);
                        int goldAfterQuest = me.Gold.Value;
                        me.RequestCompleteQuestServerValidated(0); // Repeat claim must fail
                        yield return new WaitForSeconds(0.5f);
                        bool questCompletedOnce = me.QuestIndex.Value == 1 && me.Gold.Value == goldAfterQuest && goldAfterQuest >= 25;

                        me.RequestMapPing(lp.transform.position);
                        me.RequestReadyCheck();

                        float waitDownCli = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitDownCli < 10f && me.Health.Value > 0)
                            yield return new WaitForSeconds(0.15f);
                        float waitRevCli = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitRevCli < 8f && me.Health.Value <= 0)
                            yield return new WaitForSeconds(0.15f);
                        bool clientRevived = me.Health.Value > 0;

                        int goldBeforeEvt = me.Gold.Value;
                        float waitEvt = Time.realtimeSinceStartup;
                        var lweCli = LivingWorldEvents.Instance ? LivingWorldEvents.Instance : FindFirstObjectByType<LivingWorldEvents>();
                        while (Time.realtimeSinceStartup - waitEvt < 10f && (lweCli == null || lweCli.CaravanState != LivingWorldEvents.EventState.Active || lweCli.AmbushState != LivingWorldEvents.EventState.Active || lp.Gold <= goldBeforeEvt))
                        {
                            lweCli = LivingWorldEvents.Instance ? LivingWorldEvents.Instance : FindFirstObjectByType<LivingWorldEvents>();
                            yield return new WaitForSeconds(0.2f);
                        }
                        bool eventSeen = lweCli != null && lweCli.CaravanState == LivingWorldEvents.EventState.Active && lweCli.AmbushState == LivingWorldEvents.EventState.Active;
                        bool noDoubleGold = lp.Gold == goldBeforeEvt + 20;

                        float waitHostDisc = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitHostDisc < 12f && !hostDisconnected)
                            yield return new WaitForSeconds(0.2f);
                        bool hostExitDetected = hostDisconnected;

                        fullAcceptPass = spoofBlocked && validAbHit && abHealOk && mountedOk && dismountOk && questAccepted && questCompletedOnce && clientRevived && eventSeen && noDoubleGold && hostExitDetected;
                        string cliMsg = $"{(fullAcceptPass ? "PASS" : "FAIL")} {role}: clientAccept={(fullAcceptPass ? "PASS" : "FAIL")}(spoofRej={spoofBlocked},abHit={validAbHit},abHeal={abHealOk},dismount={dismountOk},qAcc={questAccepted},qComp={questCompletedOnce},rev={clientRevived},evt={eventSeen},noDblG={noDoubleGold},hostDisc={hostExitDetected})";
                        Debug.Log(cliMsg);
                        System.Console.WriteLine(cliMsg);
                        yield return new WaitForSeconds(0.2f);
                        Application.Quit(fullAcceptPass ? 0 : 1);
                        yield break;
                    }
                }
                else
                {
                    fullAcceptPass = false;
                    combatInfo += ", fullAccept=FAIL(missingEntity)";
                }
            }

            bool authorityPass = true;
            if (CoopSessionConfig.AuthoritySmoke && players >= 2 && RiftboundCoopPlayer.LocalOwner != null)
            {
                var lp = FindFirstObjectByType<PlayerController>();
                var targetEnemy = EnemyController.FindByKey("road-raider-1");
                LootChest targetChest = FindDeterministicTestChest(20, true);
                Landmark lm0 = null, lm1 = null, lm2 = null;
                foreach (var lm in FindObjectsByType<Landmark>(FindObjectsSortMode.None))
                {
                    if (!lm) continue;
                    if (lm.landmarkId == "willow-crossing") lm0 = lm;
                    else if (lm.landmarkId == "greenhollow-post") lm1 = lm;
                    else if (lm.landmarkId == "ancient-grove") lm2 = lm;
                }

                if (lp && targetEnemy && targetChest && lm0 && lm1 && lm2)
                {
                    var eStats = targetEnemy.GetComponent<Combatant>();
                    if (isHost)
                    {
                        var clientAvatar = GetFirstRemotePlayer();
                        ulong remoteCid = clientAvatar != null ? clientAvatar.OwnerClientId : 1UL;
                        yield return new WaitForSeconds(1.2f);

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(targetEnemy.transform.position + new Vector3(14f, 0f, 0f));
                        yield return new WaitForSeconds(2.0f);

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(targetEnemy.transform.position + new Vector3(2.2f, 0f, 0f));
                        yield return new WaitForSeconds(2.2f);

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(targetChest.transform.position + new Vector3(1.5f, 0f, 0f));
                        float waitChestCli = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitChestCli < 10f && (clientAvatar == null || clientAvatar.Gold.Value <= 0))
                            yield return new WaitForSeconds(0.2f);
                        int hostGoldBefore = RiftboundCoopPlayer.LocalOwner.Gold.Value;
                        RiftboundCoopPlayer.LocalOwner.AuthorizeHostTeleport(targetChest.transform.position + new Vector3(1.5f, 0f, 0f));
                        yield return new WaitForSeconds(0.35f);
                        bool hostChestOk = lp.OpenChest(targetChest) && RiftboundCoopPlayer.LocalOwner.Gold.Value > hostGoldBefore;

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(lm0.transform.position + new Vector3(1.5f, 0f, 0f));
                        int gBeforeLm0 = clientAvatar != null ? clientAvatar.Gold.Value : 0;
                        float waitLm0 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitLm0 < 10f && (clientAvatar == null || clientAvatar.Gold.Value < gBeforeLm0 + 15))
                            yield return new WaitForSeconds(0.2f);
                        yield return new WaitForSeconds(1.2f); // allow early zebra check + dup check

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(lm1.transform.position + new Vector3(1.5f, 0f, 0f));
                        int gBeforeLm1 = clientAvatar != null ? clientAvatar.Gold.Value : 0;
                        float waitLm1 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitLm1 < 10f && (clientAvatar == null || clientAvatar.Gold.Value < gBeforeLm1 + 15))
                            yield return new WaitForSeconds(0.2f);

                        if (clientAvatar != null)
                            clientAvatar.AuthorizeHostTeleport(lm2.transform.position + new Vector3(1.5f, 0f, 0f));
                        float waitLm2 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitLm2 < 10f && (clientAvatar == null || !clientAvatar.IsMounted.Value))
                            yield return new WaitForSeconds(0.2f);
                        bool zebraSeenOnHost = clientAvatar != null && clientAvatar.IsMounted.Value && clientAvatar.MountId.Value.ToString() == "zebra";

                        int gBeforeTemper = clientAvatar != null ? clientAvatar.Gold.Value : 0;
                        float waitTemperH = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitTemperH < 4f && (clientAvatar == null || clientAvatar.Gold.Value > gBeforeTemper - 45))
                            yield return new WaitForSeconds(0.15f);
                        yield return new WaitForSeconds(0.7f);

                        if (clientAvatar != null)
                            RiftboundCoopPlayer.LocalOwner.AuthorizeHostTeleport(clientAvatar.transform.position + new Vector3(22f, 0f, 0f));
                        yield return new WaitForSeconds(0.4f);
                        RiftboundCoopPlayer.DispatchEnemyHitToPlayer(remoteCid, 500f, targetEnemy.NetworkKey);
                        float waitDownH = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitDownH < 6f && clientAvatar != null && clientAvatar.Health.Value > 0)
                            yield return new WaitForSeconds(0.15f);
                        yield return new WaitForSeconds(1.8f); // Let dead client fail invalid RPCs
                        RiftboundCoopPlayer.LocalOwner.RequestReviveAlly(remoteCid); // Must fail > 5m
                        yield return new WaitForSeconds(0.6f);
                        bool farRevBlocked = clientAvatar != null && clientAvatar.Health.Value <= 0;

                        if (clientAvatar != null)
                            RiftboundCoopPlayer.LocalOwner.AuthorizeHostTeleport(clientAvatar.transform.position + new Vector3(1.5f, 0f, 0f));
                        yield return new WaitForSeconds(0.5f);
                        RiftboundCoopPlayer.LocalOwner.RequestReviveAlly(remoteCid);
                        float waitRevH = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitRevH < 5f && clientAvatar != null && clientAvatar.Health.Value <= 0)
                            yield return new WaitForSeconds(0.15f);
                        int revHp = clientAvatar != null ? clientAvatar.Health.Value : -1;
                        RiftboundCoopPlayer.LocalOwner.RequestReviveAlly(remoteCid); // Duplicate revive must not overwrite
                        yield return new WaitForSeconds(0.5f);
                        bool nearRevOk = revHp > 0 && clientAvatar != null && clientAvatar.Health.Value == revHp;

                        // Client is at ~50% HP after revive; wait for alive-respawn check + rest + repeat-rest check, then deal 90 dmg for potion test
                        float waitRestH = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - waitRestH < 8f && clientAvatar != null && clientAvatar.Health.Value < clientAvatar.MaxHealth.Value)
                            yield return new WaitForSeconds(0.2f);
                        yield return new WaitForSeconds(0.9f);
                        if (clientAvatar != null)
                        {
                            RiftboundCoopPlayer.DispatchEnemyHitToPlayer(clientAvatar.OwnerClientId, 90f, "smoke-test");
                            float waitPotH = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - waitPotH < 6f && clientAvatar.Health.Value < 100)
                                yield return new WaitForSeconds(0.2f);
                            yield return new WaitForSeconds(0.8f);
                        }

                        authorityPass = hostChestOk && zebraSeenOnHost && farRevBlocked && nearRevOk;
                        combatInfo += $", hostAuth={(authorityPass ? "PASS" : "FAIL")}(chest={hostChestOk},zebra={zebraSeenOnHost},farRevRej={farRevBlocked},nearRev={nearRevOk})";
                    }
                    else
                    {
                        var me = RiftboundCoopPlayer.LocalOwner;
                        string rightAb = me.ClassName.Value.ToString() == "Mage" ? "fireball" : "whirlwind";

                        Vector3 prePos = lp.transform.position;
                        me.SendSpoofedPositionForTest(prePos + new Vector3(200f, 0f, 0f));
                        yield return new WaitForSeconds(0.75f);
                        bool posClampOk = Vector3.Distance(lp.transform.position, prePos) < 25f;

                        float wtFar = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wtFar < 8f && Vector3.Distance(lp.transform.position, targetEnemy.transform.position) < 10f)
                            yield return new WaitForSeconds(0.15f);
                        float hpBeforeFar = eStats.Health;
                        me.RequestAbilityAttack(eStats, "melee");
                        yield return new WaitForSeconds(0.6f);
                        bool outRangeRej = Mathf.Approximately(eStats.Health, hpBeforeFar);

                        float wtNear = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wtNear < 8f && Vector3.Distance(lp.transform.position, targetEnemy.transform.position) > 5f)
                            yield return new WaitForSeconds(0.15f);
                        float hpBeforeUnk = eStats.Health;
                        me.RequestAbilityAttack(eStats, "spoof_unknown_ab");
                        yield return new WaitForSeconds(0.45f);
                        bool unknownAtkRej = Mathf.Approximately(eStats.Health, hpBeforeUnk);

                        float hpBeforeCd = eStats.Health;
                        me.RequestAbilityAttack(eStats, rightAb);
                        yield return new WaitForSeconds(0.05f);
                        me.RequestAbilityAttack(eStats, rightAb);
                        yield return new WaitForSeconds(0.9f);
                        float dmgDealt = hpBeforeCd - eStats.Health;
                        bool cooldownRej = dmgDealt > 0f && dmgDealt <= 42f;

                        float wtChest = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wtChest < 8f && Vector3.Distance(lp.transform.position, targetChest.transform.position) > 4f)
                            yield return new WaitForSeconds(0.15f);
                        lp.OpenChest(targetChest);
                        float wtGold1 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wtGold1 < 5f && me.Gold.Value <= 0)
                            yield return new WaitForSeconds(0.15f);
                        int goldAfterChest = me.Gold.Value;

                        // Discover 3 Greenhollow landmarks server-authoritatively & check Zebra policy
                        Landmark[] lms = { lm0, lm1, lm2 };
                        bool zebraBlockedEarly = false;
                        bool dupLmRej = false;
                        for (int i = 0; i < 3; i++)
                        {
                            float wlm = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - wlm < 8f && Vector3.Distance(lp.transform.position, lms[i].transform.position) > 5f)
                                yield return new WaitForSeconds(0.15f);
                            int gBefore = me.Gold.Value;
                            lp.DiscoverLandmark(lms[i].landmarkId);
                            float wg = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - wg < 4f && me.Gold.Value <= gBefore)
                                yield return new WaitForSeconds(0.15f);
                            if (i == 0)
                            {
                                int gDupBefore = me.Gold.Value;
                                me.RequestDiscoverLandmark(lms[0].landmarkId);
                                yield return new WaitForSeconds(0.45f);
                                dupLmRej = me.Gold.Value == gDupBefore;

                                me.RequestSetMount(true, "zebra");
                                yield return new WaitForSeconds(0.45f);
                                zebraBlockedEarly = !me.IsMounted.Value;
                            }
                        }

                        me.RequestSetMount(true, "zebra");
                        float wmZebra = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wmZebra < 4f && !me.IsMounted.Value)
                            yield return new WaitForSeconds(0.15f);
                        bool zebraUnlockedOk = me.IsMounted.Value && me.MountId.Value.ToString() == "zebra";

                        // Temper weapon once + duplicate temper rejection
                        bool temperOk = false;
                        if (me.Gold.Value >= 45)
                        {
                            me.RequestTransaction("temper_weapon");
                            float wTemp = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - wTemp < 3f && !lp.TemperedWeapon)
                                yield return new WaitForSeconds(0.15f);
                            int goldAfterT1 = me.Gold.Value;
                            me.RequestTransaction("temper_weapon");
                            yield return new WaitForSeconds(0.45f);
                            temperOk = lp.TemperedWeapon && me.Gold.Value == goldAfterT1;
                        }

                        // Wait down -> check dead rejections -> wait revive
                        float wtDownC = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wtDownC < 12f && me.Health.Value > 0)
                            yield return new WaitForSeconds(0.15f);
                        float enemyHpDead = eStats.Health;
                        int goldDead = me.Gold.Value;
                        me.RequestAbilityAttack(eStats, rightAb);
                        me.RequestSetMount(true, "horse");
                        me.RequestTransaction("buy_potion");
                        yield return new WaitForSeconds(0.8f);
                        bool deadRejOk = Mathf.Approximately(eStats.Health, enemyHpDead) && !me.IsMounted.Value && me.Gold.Value == goldDead;

                        float wtRevC = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wtRevC < 12f && me.Health.Value <= 0)
                            yield return new WaitForSeconds(0.15f);
                        bool revBackOk = me.Health.Value > 0;
                        yield return new WaitForSeconds(1.2f); // Let host verify duplicate revive check before healing via rest!

                        // Wait for -25 dmg -> test rest heals to MaxHealth and costs 5g
                        float wtDmgRest = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wtDmgRest < 8f && me.Health.Value >= me.MaxHealth.Value)
                            yield return new WaitForSeconds(0.15f);
                        bool restDmgOk = false;
                        bool restFullRej = false;
                        bool aliveRespRej = false;
                        bool potOk = false;
                        bool potCdRej = false;
                        int restG0 = me.Gold.Value, restH0 = me.Health.Value;
                        if (me.Gold.Value >= 5 && me.Health.Value < me.MaxHealth.Value)
                        {
                            Vector3 preRespPos = lp.transform.position;
                            int preRespHp = me.Health.Value;
                            me.RequestRespawn();
                            yield return new WaitForSeconds(0.5f);
                            aliveRespRej = me.Health.Value == preRespHp && me.Health.Value < me.MaxHealth.Value && Vector3.Distance(lp.transform.position, preRespPos) < 5f;

                            int gRest = me.Gold.Value;
                            me.RequestTransaction("rest");
                            float wtR = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - wtR < 4f && (me.Health.Value < me.MaxHealth.Value || me.Gold.Value > gRest - 5))
                                yield return new WaitForSeconds(0.15f);
                            restDmgOk = me.Health.Value == me.MaxHealth.Value && me.Gold.Value == gRest - 5;

                            // Now Client is guaranteed full HP -> repeat rest must be rejected
                            int gAtFull = me.Gold.Value;
                            me.RequestTransaction("rest");
                            yield return new WaitForSeconds(0.45f);
                            restFullRej = me.Gold.Value == gAtFull;
                        }

                        // Wait for Host's -90 dmg hit -> verify server potion heal & cooldown reject without client desync
                        float wtPotDmg = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - wtPotDmg < 6f && me.Health.Value > 50)
                            yield return new WaitForSeconds(0.15f);
                        if (me.Health.Value <= 50 && lp.Potions > 0)
                        {
                            int potsBeforeUse = lp.Potions;
                            int hpLow = me.Health.Value;
                            me.RequestUsePotion();
                            float wtPotUse = Time.realtimeSinceStartup;
                            while (Time.realtimeSinceStartup - wtPotUse < 3f && (me.Health.Value < hpLow + 70 || lp.Potions != potsBeforeUse - 1))
                                yield return new WaitForSeconds(0.1f);
                            potOk = me.Health.Value >= hpLow + 70 && lp.Potions == potsBeforeUse - 1;

                            int hpAfterPot = me.Health.Value;
                            me.RequestUsePotion(); // Within 1.5s server cooldown -> must reject with zero local potion/HP desync
                            me.RequestAbilityHeal(60, "spoof_unknown_heal"); // Unknown heal ID must reject cleanly
                            yield return new WaitForSeconds(0.5f);
                            potCdRej = lp.Potions == potsBeforeUse - 1 && me.Health.Value == hpAfterPot;
                        }

                        authorityPass = posClampOk && outRangeRej && unknownAtkRej && cooldownRej && goldAfterChest > 0 && aliveRespRej && restFullRej && dupLmRej && zebraBlockedEarly && zebraUnlockedOk && temperOk && deadRejOk && revBackOk && restDmgOk && potOk && potCdRej;
                        combatInfo += $", clientAuth={(authorityPass ? "PASS" : "FAIL")}(clamp={posClampOk},rngRej={outRangeRej},unkAtkRej={unknownAtkRej},cdRej={cooldownRej},aliveRespRej={aliveRespRej},restRej={restFullRej},dupLmRej={dupLmRej},zEarly={zebraBlockedEarly},zOk={zebraUnlockedOk},temper={temperOk},deadRej={deadRejOk},rev={revBackOk},restOk={restDmgOk}[g={restG0}->{me.Gold.Value},h={restH0}->{me.Health.Value}/{me.MaxHealth.Value}],potOk={potOk},potCdRej={potCdRej})";
                    }
                }
                else
                {
                    authorityPass = false;
                    combatInfo += ", auth=FAIL(missingEntity)";
                }
            }

            bool pass = players >= minPlayers && RiftboundCoopPlayer.LocalOwner != null && activeCams == 1 && activeListeners == 1 && sep >= 1.0f && combatPass && lateJoinPass && progPass && clientLeavePass && fullAcceptPass && authorityPass;
            string msg = $"{(pass ? "PASS" : "FAIL")} {role}: players={players}, localOwner={(RiftboundCoopPlayer.LocalOwner != null)}, cams={activeCams}, listeners={activeListeners}, sep={sep:F2}m ({posInfo}){combatInfo}";
            Debug.Log(msg);
            System.Console.WriteLine(msg);

            yield return new WaitForSeconds(0.5f);
            Application.Quit(pass ? 0 : 1);
        }

        /// <summary>Picks an un-gated LootChest deterministically across Host and Client.</summary>
        static LootChest FindDeterministicTestChest(int minGold, bool pickHighest)
        {
            LootChest best = null;
            foreach (var c in FindObjectsByType<LootChest>(FindObjectsSortMode.None))
            {
                if (!c || string.IsNullOrEmpty(c.chestId) || c.gold < minGold || c.requiresCaptainDefeated || c.requiresWardenDefeated || c.chestId == RiverReaverCamp.CacheId)
                    continue;
                bool better = best == null ||
                    (pickHighest ? c.gold > best.gold : c.gold < best.gold) ||
                    (c.gold == best.gold && string.CompareOrdinal(c.chestId, best.chestId) < 0);
                if (better) best = c;
            }
            return best;
        }
    }
}
