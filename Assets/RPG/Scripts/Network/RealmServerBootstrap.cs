using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riftbound.Network
{
    /// <summary>
    /// Handles Dedicated Server headless initialization (suppressing local avatar, camera, audio,
    /// FrontendMenu, HUD, and offline save bootstrap) and Realm Client test CLI auto-entry.
    /// </summary>
    public static class RealmServerBootstrap
    {
        public static Vector3 SceneDefaultSpawnAnchor { get; private set; } = new Vector3(0f, 1.0f, 10f);
        public static bool LocalAvatarSuppressed { get; private set; }
        public static bool OfflineSavesSuppressed { get; private set; }

        static bool sceneHooked;
        static bool logInitialized;
        static bool autoEnterCompleted;
        static string activeLogPath = "";
        static string previousSavePath;
        static string isolatedSavePath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            SceneDefaultSpawnAnchor = new Vector3(0f, 1.0f, 10f);
            LocalAvatarSuppressed = false;
            OfflineSavesSuppressed = false;
            sceneHooked = false;
            logInitialized = false;
            autoEnterCompleted = false;
            activeLogPath = "";
            previousSavePath = null;
            isolatedSavePath = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeBeforeScene()
        {
            CoopSessionConfig.ParseCommandLineOnce();
            RealmConfig.LoadOnce();

            if (!CoopSessionConfig.IsRealmSession) return;

            IsolateOfflineSaves();

            if (CoopSessionConfig.IsDedicatedServer)
            {
                Application.runInBackground = true;
                Time.timeScale = 1f;
                Application.targetFrameRate = 30;
                InitializeServerLogging();
            }

            if (!sceneHooked)
            {
                sceneHooked = true;
                SceneManager.sceneLoaded += (_, _) => OnSceneReady();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InitializeAfterScene()
        {
            CoopSessionConfig.ParseCommandLineOnce();
            RealmConfig.LoadOnce();

            if (CoopSessionConfig.IsRealmSession)
                IsolateOfflineSaves();

            OnSceneReady();
        }

        internal static void IsolateOfflineSaves()
        {
            if (OfflineSavesSuppressed) return;
            previousSavePath = PlayerController.SavePathOverride;
            isolatedSavePath = Path.Combine(Path.GetTempPath(), $"rb_realm_isolated_{Guid.NewGuid():N}.json");
            PlayerController.SavePathOverride = isolatedSavePath;
            OfflineSavesSuppressed = true;
        }

        internal static void RestoreOfflineSaves()
        {
            if (!OfflineSavesSuppressed) return;
            if (PlayerController.SavePathOverride == isolatedSavePath)
                PlayerController.SavePathOverride = previousSavePath;
            previousSavePath = null;
            isolatedSavePath = null;
            OfflineSavesSuppressed = false;
        }

        static void OnSceneReady()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.Contains("Outpost")) return;

            if (CoopSessionConfig.IsDedicatedServer)
            {
                PrepareDedicatedServerScene();
            }
            else if (CoopSessionConfig.IsRealmClient)
            {
                IsolateOfflineSaves();
                if (RealmConfig.Runtime.autoEnterRealm && !autoEnterCompleted && CoopSessionConfig.AutoSmokeExitSeconds <= 0f)
                {
                    var runner = new GameObject("RealmClientAutoEntry").AddComponent<RealmAutoEntryRunner>();
                    runner.Begin();
                }
            }
        }

        static void PrepareDedicatedServerScene()
        {
            Application.runInBackground = true;
            Time.timeScale = 1f;

            var localPlayer = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (localPlayer != null)
            {
                SceneDefaultSpawnAnchor = localPlayer.transform.position;
                localPlayer.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(localPlayer.gameObject);
                LocalAvatarSuppressed = true;
            }

            foreach (var orbit in UnityEngine.Object.FindObjectsByType<OrbitCamera>(FindObjectsSortMode.None))
            {
                if (!orbit) continue;
                orbit.enabled = false;
                if (orbit.GetComponent<Camera>() is var cam && cam) cam.enabled = false;
                if (orbit.GetComponent<AudioListener>() is var al && al) al.enabled = false;
                orbit.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(orbit.gameObject);
            }

            foreach (var listener in UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            {
                if (listener) listener.enabled = false;
            }

            foreach (var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (cam) cam.enabled = false;
            }

            var servicesGo = GameObject.Find("EmberfordServerServices");
            if (!servicesGo)
            {
                servicesGo = new GameObject("EmberfordServerServices");
                servicesGo.AddComponent<CastBarSystem>();
                servicesGo.AddComponent<FactionQuestSystem>();
                servicesGo.AddComponent<WorldBossEncounter>();
            }
            else if (!servicesGo.GetComponent<WorldBossEncounter>())
            {
                servicesGo.AddComponent<WorldBossEncounter>();
            }

            RealmPersistenceService.Initialize(RealmConfig.ResolveServerDataDirectory());

            ExplorationSites.Ensure();
            RiverReaverCamp.Ensure();
            PinewatchWoodsZone.Ensure();
            AshenmawCavernZone.Ensure();
            SunstoneAbbeyZone.Ensure();
            RoadPatrolGuard.Ensure();

            LogServer($"Scene '{SceneManager.GetActiveScene().name}' prepared headless: avatarSuppressed={LocalAvatarSuppressed}, offlineSavesSuppressed={OfflineSavesSuppressed}, spawnAnchor={SceneDefaultSpawnAnchor}");
        }

        static void InitializeServerLogging()
        {
            if (logInitialized) return;
            logInitialized = true;
            try
            {
                string dataDir = RealmConfig.ResolveServerDataDirectory();
                Directory.CreateDirectory(dataDir);
                activeLogPath = RealmConfig.ResolveServerLogPath();
                string logDir = Path.GetDirectoryName(activeLogPath);
                if (!string.IsNullOrEmpty(logDir)) Directory.CreateDirectory(logDir);
                LogServer($"Booting dedicated realm server: realmId={RealmConfig.Current.realmId}, name={RealmConfig.Current.displayName}, listen={RealmConfig.Current.listenAddress}:{RealmConfig.Current.port}, cap={RealmConfig.ClampCapacity(RealmConfig.Current.maxPlayers)}, dataDir={dataDir}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RealmServerBootstrap] Failed initializing server log directory: {ex.Message}");
            }
        }

        public static void LogServer(string message)
        {
            string line = $"[{DateTime.UtcNow:O}] [EmberfordServer] {message}";
            Debug.Log(line);
            if (!string.IsNullOrEmpty(activeLogPath))
            {
                try { File.AppendAllText(activeLogPath, line + Environment.NewLine); }
                catch { }
            }
        }

        sealed class RealmAutoEntryRunner : MonoBehaviour
        {
            public void Begin() => StartCoroutine(EnterRealmWhenMenuReady());

            IEnumerator EnterRealmWhenMenuReady()
            {
                yield return null;
                yield return null;
                if (autoEnterCompleted)
                {
                    Destroy(gameObject);
                    yield break;
                }

                EmberfordRealmClient.SelectEmberfordRealm(RealmConfig.Current.endpoint, RealmConfig.Current.port);
                var frontend = UnityEngine.Object.FindFirstObjectByType<FrontendMenu>();
                if (frontend != null && FrontendMenu.IsOpen)
                {
                    autoEnterCompleted = true;
                    frontend.OpenCharacterSelect();
                    yield return null;
                    frontend.EnterWorld();
                }
                else
                {
                    autoEnterCompleted = true;
                }
                Destroy(gameObject);
            }
        }
    }
}
