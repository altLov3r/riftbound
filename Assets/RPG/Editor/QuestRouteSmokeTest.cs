using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    [InitializeOnLoad]
    public static class QuestRouteSmokeTest
    {
        const string Running = "Riftbound.QuestRouteSmokeTest";
        static readonly List<string> Results = new List<string>();
        static readonly List<string> Errors = new List<string>();
        static double due, timeout;
        static int stage;
        static Keyboard keyboard;
        static Vector3 start;

        static QuestRouteSmokeTest()
        {
            EditorApplication.playModeStateChanged += Changed;
        }

        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run this check in an isolated Unity batch process.");
            Directory.CreateDirectory(".code-check");
            SessionState.SetBool(Running, true);
            SessionState.SetBool(Running + ".Failed", false);
            SessionState.SetString(Running + ".SavePath", Path.GetFullPath(".code-check/quest-route-save-" + Guid.NewGuid().ToString("N") + ".json"));
            Application.runInBackground = true;
            PlayerSettings.runInBackground = true;
            EditorApplication.isPaused = false;
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSceneManager.OpenScene("Assets/RPG/Scenes/Outpost-Updated.unity");
            EditorApplication.isPlaying = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void IsolateSaves()
        {
            if (SessionState.GetBool(Running, false))
            {
                Application.runInBackground = true;
                PlayerController.SavePathOverride = SessionState.GetString(Running + ".SavePath", "");
            }
        }

        static void Changed(PlayModeStateChange change)
        {
            if (!SessionState.GetBool(Running, false)) return;
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                due = EditorApplication.timeSinceStartup + 3;
                timeout = due + 90;
                stage = 0;
                Application.logMessageReceived += Log;
                EditorApplication.update += Tick;
            }
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Running, false);
                EditorApplication.Exit(SessionState.GetBool(Running + ".Failed", false) ? 1 : 0);
            }
        }

        static void Log(string message, string stack, LogType type)
        {
            if ((type == LogType.Exception || type == LogType.Error) && stack.Contains("Riftbound"))
                Errors.Add(message + "\n" + stack);
        }

        static void Check(bool passed, string label)
        {
            Results.Add((passed ? "PASS " : "FAIL ") + label);
            if (!passed) throw new Exception(label);
        }

        static PlayerController Player()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            Check(player && player.Stats && player.animator, "Player, health and animator survive startup");
            Check(player.orbit && player.orbit.target == player.transform, "Camera follows the player");
            var hud = Object.FindFirstObjectByType<PrototypeHUD>();
            Check(hud && hud.player == player, "HUD references the live player");
            return player;
        }

        static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > timeout) throw new TimeoutException("Play Mode check timed out");
                if (EditorApplication.isPlaying && EditorApplication.isPaused)
                    EditorApplication.isPaused = false;
                EditorApplication.QueuePlayerLoopUpdate();
                if (EditorApplication.timeSinceStartup < due || !EditorApplication.isPlaying) return;
                if (stage == 0)
                {
                    var player = Player();
                    Results.Add($"Initial frame={Time.frameCount}, background={Application.runInBackground}, focused={Application.isFocused}, editorPaused={EditorApplication.isPaused}");
                    var menu = player.GetComponent<FrontendMenu>();
                    Check(menu, "Frontend exists");
                    Check(FrontendMenu.IsOpen || (!player.orbit.Paused && Time.timeScale > 0), "Menu or direct-play controls are available");
                    Results.Add("Initial character selection shown: " + FrontendMenu.IsOpen);
                    PlayerController.SavePathOverride = SessionState.GetString(Running + ".SavePath", "");
                    var entering = typeof(FrontendMenu).GetField("enteringCharacter", BindingFlags.NonPublic | BindingFlags.Static);
                    Check(entering != null, "Character-entry handoff is available");
                    entering.SetValue(null, new FrontendMenu.Character { id = Guid.NewGuid().ToString("N"), name = "BuildCheck", faction = "Alliance", race = "Human", characterClass = "Warrior" });
                    typeof(FrontendMenu).GetProperty("IsOpen").SetValue(null, false);
                    player.orbit.SetPaused(false);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    stage = 1;
                    due = EditorApplication.timeSinceStartup + 3;
                }
                else if (stage == 1)
                {
                    var player = Player();
                    Results.Add($"World state: menu={FrontendMenu.IsOpen}, paused={player.orbit.Paused}, timeScale={Time.timeScale}, name={player.CharacterName}, frames={Time.frameCount}");
                    Check(!FrontendMenu.IsOpen && !player.orbit.Paused && Time.timeScale > 0, "Entering the world restores gameplay time and camera control");
                    Check(player.CharacterName == "BuildCheck", "Selected character is applied after scene reload");
                    Check(player.CharacterRace == "Human", "Fresh test character retains its selected race");
                    Check(player.GetComponentsInChildren<Renderer>().Length > 0, "Character has a visible model");
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    Application.runInBackground = true;
                    keyboard = InputSystem.AddDevice<Keyboard>("QuestRouteSmokeTestKeyboard");
                    keyboard.MakeCurrent();
                    start = player.transform.position;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                    stage = 2;
                    due = EditorApplication.timeSinceStartup + 2;
                }
                else if (stage == 2)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    var player = Player();
                    var travel = player.transform.position - start;
                    travel.y = 0;
                    Check(travel.magnitude > .5f, $"W moves the player ({travel.magnitude:F2}m)");
                    Check(Errors.Count == 0, "No RPG runtime exceptions during startup and movement");
                    Time.timeScale = 0;
                    QuestRegression(player);
                    stage = 3;
                    due = EditorApplication.timeSinceStartup + .8;
                }
                else if (stage == 3)
                {
                    var player = Player();
                    var radar = MinimapRadar.Instance;
                    Check(radar.TrackingMainQuest && radar.customWaypoint.HasValue && Vector3.Distance(radar.customWaypoint.Value, player.questGiver.position) < .1f, "Live quest tracking returns to Abe when the objective is ready");
                    TurnIn(player);
                    Check(player.QuestIndex == 1 && !player.QuestAccepted, "Road quest turns in and advances exactly once");
                    OpenAbe(player); player.AcceptQuest();
                    foreach (var crate in Object.FindObjectsByType<SupplyCrate>(FindObjectsSortMode.None))
                    {
                        Move(player, crate.transform.position);
                        crate.Collect(player);
                    }
                    Check(player.QuestReady && player.Supplies >= 3, "Supply quest collects the three actual crates");
                    TurnIn(player);
                    Check(player.QuestIndex == 2, "Supply hand-in unlocks the captain mission");
                    var captain = Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(e => e.IsMainQuestCaptain);
                    Check(!captain.gameObject.activeSelf, "Captain remains locked until his quest is accepted");
                    OpenAbe(player); player.AcceptQuest();
                    Check(captain.gameObject.activeSelf, "Accepting the captain quest activates him");
                    captain.GetComponent<Combatant>().Damage(100000);
                    Check(player.QuestReady && player.BossDefeated, "Only the captain completes the captain objective");
                    TurnIn(player);
                    Check(player.QuestComplete && player.QuestIndex == 3 && player.OwnsItem("emberwatch-axe") && player.OwnsItem("emberwatch-sigil"), "Full campaign grants the final equipment");
                    int gold = player.Gold;
                    player.CompleteQuest();
                    Check(player.Gold == gold && player.QuestIndex == 3, "Repeated hand-in cannot duplicate rewards");
                    player.SaveAdventure(false); player.LoadAdventure();
                    Check(player.QuestComplete && player.OwnsItem("emberwatch-sigil"), "Campaign completion survives save/load");
                    Check(Errors.Count == 0, "Quest and loot route produces no RPG runtime exceptions");
                    stage = 4;
                    due = EditorApplication.timeSinceStartup + .8;
                }
                else if (stage == 4)
                {
                    Check(!MinimapRadar.Instance.TrackingMainQuest && !MinimapRadar.Instance.customWaypoint.HasValue, "Quest navigation clears after campaign completion");
                    Finish(false);
                }
            }
            catch (Exception exception)
            {
                Errors.Add(exception.ToString());
                Finish(true);
            }
        }

        static void Move(PlayerController player, Vector3 position)
        {
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = position;
            controller.enabled = true;
        }

        static void OpenAbe(PlayerController player)
        {
            Move(player, player.questGiver.position);
            typeof(PlayerController).GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null);
            Check(player.DialogueOpen && player.ActiveQuestGiver, $"Abe dialogue opens through interaction (current loot: {LootDropSystem.Instance.CurrentLooting?.name ?? "none"})");
        }

        static void TurnIn(PlayerController player)
        {
            OpenAbe(player);
            string title = player.QuestName;
            int experience = player.QuestExperience;
            player.CompleteQuest();
            var messages = (List<ChatSystem.ChatEntry>)typeof(ChatSystem).GetField("messages", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ChatSystem.Instance);
            Check(messages.Any(m => m.text.Contains("Quest Completed: " + title) && m.text.Contains(experience + " XP")), "Hand-in reports the completed quest and its reward");
        }

        static void DefeatTestEnemy(PlayerController player, string id, bool boss = false)
        {
            var go = new GameObject("Quest regression " + id);
            go.transform.position = player.transform.position + Vector3.right * 80;
            var enemy = go.AddComponent<EnemyController>();
            enemy.enabled = false;
            enemy.player = player;
            enemy.enemyId = id;
            enemy.isBoss = boss;
            go.GetComponent<Combatant>().Damage(100000);
        }

        static void QuestRegression(PlayerController player)
        {
            var all = Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var worldBoss = all.First(e => e.enemyId == "worldboss-gorgonax");
            Check(worldBoss.gameObject.activeSelf, "World boss is available independently of Abe's quest");
            OpenAbe(player); player.AcceptQuest();
            Check(player.QuestAccepted, "Abe's road mission is accepted");
            MinimapRadar.Instance.TrackMainQuest();
            Check(MinimapRadar.Instance.TrackingMainQuest, "Journal navigation can start live tracking");
            Check(all.Any(e => e.IsRoadRaider && e.transform.position == player.ObjectivePosition), "Road objective targets a real raider instead of a dummy or unrelated enemy");
            DefeatTestEnemy(player, "regression-side-boss", true);
            DefeatTestEnemy(player, "regression-rat");
            Check(player.Kills == 0 && !player.BossDefeated, "Unrelated mobs and bosses grant no main-quest credit");
            var raiders = all.Where(e => e.IsRoadRaider).OrderBy(e => e.enemyId).Take(3).ToArray();
            int gold = player.Gold, shards = player.BoneShards;
            foreach (var raider in raiders) raider.GetComponent<Combatant>().Damage(100000);
            Check(player.Kills == 3 && player.QuestReady, "Three road raiders complete the road objective");
            Check(player.Gold == gold && player.BoneShards == shards && !player.OwnsItem("raider-cleaver"), "Enemy loot is not awarded before collection");
            Check(player.SaveAdventure(false), "Unclaimed loot can be saved");
            player.LoadAdventure();
            Check(!player.OwnsItem("raider-cleaver"), "Reloading cannot bypass corpse looting");
            var loot = LootDropSystem.Instance;
            var corpse = loot.ActiveCorpses.First(c => c.items.Contains("raider-cleaver"));
            loot.LootCoins(corpse);
            int afterCoins = player.Gold;
            Check(afterCoins - gold == 4, $"Human gold bonus applies exactly once (received {afterCoins - gold}, race {player.CharacterRace})");
            player.LoadAdventure();
            corpse = loot.ActiveCorpses.First(c => c.items.Contains("raider-cleaver"));
            Check(corpse.gold == 0, "Claimed coins stay claimed after reload");
            loot.LootAll(corpse);
            loot.LootAll(corpse);
            Check(player.Gold == afterCoins && player.BoneShards == shards + 1 && player.OwnsItem("raider-cleaver"), "Looting grants valid gear and shards once");
            player.LoadAdventure();
            Check(player.OwnsItem("raider-cleaver") && !loot.ActiveCorpses.Any(c => c.items.Contains("raider-cleaver")), "Claimed item and corpse removal persist");
            var data = JsonUtility.FromJson<PlayerController.AdventureSave>(File.ReadAllText(player.SavePath));
            data.lootOnCorpses = false; data.items = new[] { "starter-axe" };
            File.WriteAllText(player.SavePath, JsonUtility.ToJson(data));
            player.LoadAdventure();
            Check(player.OwnsItem("raider-cleaver"), "Legacy automatic-loot saves retain earned equipment");
            data.lootOnCorpses = true;
            File.WriteAllText(player.SavePath, JsonUtility.ToJson(data));
            player.LoadAdventure();
            Check(!player.OwnsItem("raider-cleaver"), "New-format saves do not auto-grant enemy equipment");
        }

        static void Finish(bool failed)
        {
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= Log;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            string report = string.Join("\n", Results) + "\n" + string.Join("\n", Errors);
            File.WriteAllText("QuestRouteSmokeResult.txt", report);
            SessionState.SetBool(Running + ".Failed", failed);
            if (failed) File.WriteAllText(".code-check/startup-smoke-failed.txt", report);
            EditorApplication.isPlaying = false;
        }
    }
}
