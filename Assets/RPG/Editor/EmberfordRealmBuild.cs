using System;
using System.IO;
using Riftbound.Network;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riftbound.Editor
{
    public sealed class EmberfordRealmBuild : IProcessSceneWithReport
    {
        const string ChannelPrefabPath = "Assets/RPG/Resources/EmberfordRealmChannel.prefab";
        const string DefaultServerDir = "Builds/EmberfordServer01";
        const string DefaultClientDir = "Builds/EmberfordClient01";

        static bool buildingRealm;

        public int callbackOrder => 5;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (!buildingRealm || report == null || scene.path != WorldRegions.ScenePath) return;
            VillageRemodelPreviewBuild.StageRemodels(scene);
            PinewatchPreviewBuild.StagePinewatch(scene);
            FrontierSitesPreviewBuild.StageAll(scene);
        }

        [MenuItem("Riftbound/Emberford/Ensure Realm Prefabs")]
        public static void EnsureRealmPrefabs()
        {
            if (!File.Exists("Assets/RPG/Resources/CoopPlayerAvatar.prefab"))
                CoopPrefabSetup.BuildCoopAvatarPrefab();
            Directory.CreateDirectory("Assets/RPG/Resources");

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ChannelPrefabPath);
            if (existing && existing.GetComponent<NetworkObject>() && existing.GetComponent<RealmWorldChannel>())
            {
                Debug.Log("[EmberfordRealmBuild] Verified " + ChannelPrefabPath);
                return;
            }

            var go = new GameObject("EmberfordRealmChannel");
            go.AddComponent<NetworkObject>();
            go.AddComponent<RealmWorldChannel>();
            PrefabUtility.SaveAsPrefabAsset(go, ChannelPrefabPath);
            UnityEngine.Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EmberfordRealmBuild] Created " + ChannelPrefabPath);
        }

        [MenuItem("Riftbound/Emberford/Build Server (Windows x64 Dedicated)")]
        public static void BuildDedicatedServer()
        {
            int code = BuildServerInternal(GetArgValue("-emberfordServerOut", DefaultServerDir));
            if (Application.isBatchMode) EditorApplication.Exit(code);
        }

        [MenuItem("Riftbound/Emberford/Build Client (Windows x64)")]
        public static void BuildClient()
        {
            int code = BuildClientInternal(GetArgValue("-emberfordClientOut", DefaultClientDir));
            if (Application.isBatchMode) EditorApplication.Exit(code);
        }

        [MenuItem("Riftbound/Emberford/Build Both Server and Client (Windows x64)")]
        public static void BuildBoth()
        {
            int clientCode = BuildClientInternal(GetArgValue("-emberfordClientOut", DefaultClientDir));
            int serverCode = BuildServerInternal(GetArgValue("-emberfordServerOut", DefaultServerDir));
            if (Application.isBatchMode) EditorApplication.Exit(clientCode == 0 && serverCode == 0 ? 0 : 1);
        }

        public static bool IsWindowsDedicatedServerModuleInstalled()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                return false;
            string playbackRoot = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines");
            string winServerDir = Path.Combine(playbackRoot, "WindowsServer");
            if (Directory.Exists(winServerDir)) return true;
            string variationsDir = Path.Combine(playbackRoot, "windowsstandalonesupport", "Variations");
            if (Directory.Exists(variationsDir) && Directory.GetDirectories(variationsDir, "*server*").Length > 0)
                return true;
            return false;
        }

        static int BuildServerInternal(string outputDir)
        {
            try
            {
                EnsureRealmPrefabs();

                if (!IsWindowsDedicatedServerModuleInstalled())
                {
                    Debug.LogError("[EmberfordRealmBuild] Unity 'Windows Dedicated Server Build Support' (windows-server) is NOT installed for " + Application.unityVersion + ". Install it via Unity Hub or elevated 'unity editor module add " + Application.unityVersion + " -m windows-server --accept-eula' before building StandaloneBuildSubtarget.Server.");
                    return 2;
                }

                string safeDir = ValidateBuildDir(outputDir, DefaultServerDir);
                Directory.CreateDirectory(safeDir);
                string exePath = Path.Combine(safeDir, "EmberfordServer.exe");

                var prevTarget = EditorUserBuildSettings.activeBuildTarget;
                var prevGroup = BuildPipeline.GetBuildTargetGroup(prevTarget);
                var prevSubtarget = EditorUserBuildSettings.standaloneBuildSubtarget;

                buildingRealm = true;
                try
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, BuildTarget.StandaloneWindows64);
                    EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;

                    var opts = new BuildPlayerOptions
                    {
                        scenes = new[] { WorldRegions.ScenePath },
                        locationPathName = exePath,
                        target = BuildTarget.StandaloneWindows64,
                        subtarget = (int)StandaloneBuildSubtarget.Server,
                        options = BuildOptions.None
                    };

                    BuildReport report = BuildPipeline.BuildPlayer(opts);
                    Debug.Log($"[EmberfordRealmBuild] Server build result={report.summary.result}, errors={report.summary.totalErrors}, warnings={report.summary.totalWarnings}, path={exePath}, size={report.summary.totalSize}");
                    return report.summary.result == BuildResult.Succeeded ? 0 : 1;
                }
                finally
                {
                    buildingRealm = false;
                    EditorUserBuildSettings.standaloneBuildSubtarget = prevSubtarget;
                    if (prevGroup != BuildTargetGroup.Unknown && prevTarget != BuildTarget.NoTarget)
                        EditorUserBuildSettings.SwitchActiveBuildTarget(prevGroup, prevTarget);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return 1;
            }
        }

        static int BuildClientInternal(string outputDir)
        {
            try
            {
                EnsureRealmPrefabs();

                string safeDir = ValidateBuildDir(outputDir, DefaultClientDir);
                Directory.CreateDirectory(safeDir);
                string exePath = Path.Combine(safeDir, "Riftbound.exe");

                buildingRealm = true;
                try
                {
                    var opts = new BuildPlayerOptions
                    {
                        scenes = new[] { WorldRegions.ScenePath },
                        locationPathName = exePath,
                        target = BuildTarget.StandaloneWindows64,
                        subtarget = (int)StandaloneBuildSubtarget.Player,
                        options = BuildOptions.None
                    };

                    BuildReport report = BuildPipeline.BuildPlayer(opts);
                    Debug.Log($"[EmberfordRealmBuild] Client build result={report.summary.result}, errors={report.summary.totalErrors}, warnings={report.summary.totalWarnings}, path={exePath}, size={report.summary.totalSize}");
                    return report.summary.result == BuildResult.Succeeded ? 0 : 1;
                }
                finally
                {
                    buildingRealm = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return 1;
            }
        }

        static string GetArgValue(string flag, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == flag && !args[i + 1].StartsWith("-"))
                    return args[i + 1];
            }
            return fallback;
        }

        static string ValidateBuildDir(string candidate, string fallback)
        {
            if (string.IsNullOrWhiteSpace(candidate)) return fallback;
            return candidate.Trim().Replace('\\', '/');
        }
    }
}
