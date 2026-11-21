using System;
using Riftbound.Network;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Riftbound
{
    // Values are persisted in firstAdventureV1. Keep existing values stable.
    internal enum FirstAdventureStep
    {
        Unseen = 0, Move = 1, Look = 2, Talk = 3, Fight = 4, Journal = 5,
        Raiders = 6, Return = 7, Complete = 8, Skipped = 9
    }

    public sealed class FirstAdventureGuide : MonoBehaviour
    {
        PlayerController player;
        CharacterController motor;
        FactionQuestSystem factionQuests;
        FactionQuestSystem.QuestDef starterQuest;
        TownNPC starterGiver;
        Vector3? guidePin;
        float walked, turned, previousYaw, previousPitch;
        GUIStyle heading, copy;

        internal static FirstAdventureGuide Attach(PlayerController player)
        {
            if (!player || CoopSessionConfig.IsMultiplayer || !WorldRegions.Expanded)
                return null;

            var questId = player.CharacterRace switch
            {
                "Dwarf" => "frostpeak_troggs",
                "Elf" or "High Elf" => "silverglen_sprites",
                "Orc" or "Brute" => "bloodrock_cull",
                "Undead" => "deathknell_ghouls",
                "Goblin" => "rustwater_scavengers",
                _ => null
            };
            var localQuest = questId != null ? FactionQuestSystem.GetQuest(questId) : null;
            if (localQuest == null && !(player.CharacterRace == "Human" && player.CharacterFaction == "Alliance"))
                return null;

            var guide = player.GetComponent<FirstAdventureGuide>();
            if (!guide) guide = player.gameObject.AddComponent<FirstAdventureGuide>();
            guide.player = player;
            guide.motor = player.GetComponent<CharacterController>();
            guide.factionQuests = player.GetComponent<FactionQuestSystem>();
            guide.starterQuest = localQuest;
            guide.starterGiver = null;
            return guide;
        }

        FirstAdventureStep Step => player.TutorialStep;
        bool Active => player && !CoopSessionConfig.IsMultiplayer && !FrontendMenu.IsOpen;
        bool Engaged => Step > FirstAdventureStep.Unseen && Step < FirstAdventureStep.Complete;
        bool Suppressed => !player.Stats || !player.Stats.Alive || player.AnyBlockingWindowOpen() ||
            PrototypeHUD.MenuOpen || (player.orbit && player.orbit.Paused);
        bool LocalStarter => starterQuest != null;
        bool StarterComplete => LocalStarter ? factionQuests && factionQuests.IsQuestCompleted(starterQuest.id) : player.QuestIndex > 0;
        bool StarterAccepted => LocalStarter ? factionQuests && factionQuests.ActiveQuestId == starterQuest.id : player.QuestAccepted;
        bool StarterReady => LocalStarter ? StarterAccepted && factionQuests.IsActiveQuestReady() : player.QuestReady;
        int StarterProgress => LocalStarter ? StarterAccepted ? factionQuests.CurrentProgress : 0 : player.Kills;
        bool HasDifferentQuest => LocalStarter && factionQuests &&
            !string.IsNullOrEmpty(factionQuests.ActiveQuestId) && !StarterAccepted;
        bool Offer => Step == FirstAdventureStep.Unseen && !StarterComplete && !StarterAccepted && !HasDifferentQuest;

        internal void Begin()
        {
            if (!Active) return;
            ClearGuidePin();
            walked = turned = 0;
            ChangeStep(FirstAdventureStep.Move);
        }

        void Skip() => ChangeStep(FirstAdventureStep.Skipped);

        void ChangeStep(FirstAdventureStep step)
        {
            if (Step == step) return;
            ClearGuidePin();
            player.TutorialStep = step;
            player.SaveAdventure(false);
        }

        void LateUpdate()
        {
            if (!Active || !Engaged) return;

            var camera = player.orbit;
            float yaw = camera ? camera.Yaw : 0;
            float pitch = camera ? camera.Pitch : 0;
            float lookDelta = Mathf.Abs(Mathf.DeltaAngle(previousYaw, yaw)) + Mathf.Abs(pitch - previousPitch);
            previousYaw = yaw;
            previousPitch = pitch;

            if (HasDifferentQuest && Step >= FirstAdventureStep.Talk && Step != FirstAdventureStep.Journal)
                ChangeStep(FirstAdventureStep.Journal);
            if (LocalStarter && !StarterAccepted && !StarterComplete && !HasDifferentQuest &&
                (Step == FirstAdventureStep.Fight || Step == FirstAdventureStep.Raiders || Step == FirstAdventureStep.Return))
                ChangeStep(FirstAdventureStep.Talk);

            // Dialogue and the journal are legitimate tutorial actions, including when they block movement.
            switch (Step)
            {
                case FirstAdventureStep.Talk:
                    if (StarterComplete || StarterReady) ChangeStep(FirstAdventureStep.Journal);
                    else if (StarterAccepted) ChangeStep(FirstAdventureStep.Fight);
                    break;
                case FirstAdventureStep.Fight:
                    if (StarterProgress > 0 || StarterComplete) ChangeStep(FirstAdventureStep.Journal);
                    break;
                case FirstAdventureStep.Journal:
                    if (player.MapOpen || (!LocalStarter && player.JournalOpen))
                    {
                        if (StarterComplete || HasDifferentQuest) ChangeStep(FirstAdventureStep.Complete);
                        else if (StarterReady) ChangeStep(FirstAdventureStep.Return);
                        else if (StarterAccepted || !LocalStarter) ChangeStep(FirstAdventureStep.Raiders);
                        else ChangeStep(FirstAdventureStep.Talk);
                    }
                    break;
                case FirstAdventureStep.Raiders:
                    if (StarterComplete) ChangeStep(FirstAdventureStep.Complete);
                    else if (StarterReady) ChangeStep(FirstAdventureStep.Return);
                    break;
                case FirstAdventureStep.Return:
                    if (StarterComplete) ChangeStep(FirstAdventureStep.Complete);
                    break;
            }

            if (Suppressed || player.Mounted) return;
            if (Step == FirstAdventureStep.Move && motor && motor.isGrounded && HasMovementInput())
            {
                walked += Vector3.ProjectOnPlane(motor.velocity, Vector3.up).magnitude * Time.deltaTime;
                if (walked >= 1.2f) ChangeStep(FirstAdventureStep.Look);
            }
            else if (Step == FirstAdventureStep.Look && camera && (camera.IsFreeLooking || camera.IsSteering))
            {
                turned += lookDelta;
                if (turned >= 18f) ChangeStep(FirstAdventureStep.Talk);
            }
        }

        static bool HasMovementInput()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return (keyboard != null && (keyboard.wKey.isPressed || keyboard.sKey.isPressed ||
                keyboard.aKey.isPressed || keyboard.dKey.isPressed || keyboard.upArrowKey.isPressed ||
                keyboard.downArrowKey.isPressed)) ||
                (mouse != null && mouse.leftButton.isPressed && mouse.rightButton.isPressed);
        }

        TownNPC FindStarterGiver()
        {
            if (starterGiver && starterGiver.gameObject.activeInHierarchy) return starterGiver;
            starterGiver = null;
            foreach (var npc in FindObjectsByType<TownNPC>(FindObjectsSortMode.None))
            {
                if (!string.Equals(npc.npcName, starterQuest.giverNpc, StringComparison.Ordinal)) continue;
                starterGiver = npc;
                break;
            }
            return starterGiver;
        }

        bool CanPinGuide => LocalStarter && (Step == FirstAdventureStep.Talk || Step == FirstAdventureStep.Return) &&
            MinimapRadar.Instance && !MinimapRadar.Instance.customWaypoint.HasValue && FindStarterGiver();

        void PinGuide()
        {
            if (!CanPinGuide) return;
            guidePin = starterGiver.transform.position;
            MinimapRadar.SetWaypoint(guidePin.Value, "First Adventure: " + starterQuest.giverNpc);
        }

        void ClearGuidePin()
        {
            var radar = MinimapRadar.Instance;
            if (guidePin.HasValue && radar && radar.customWaypoint == guidePin &&
                radar.TryGetNavigationTarget(out _, out string caption) &&
                caption == "First Adventure: " + starterQuest?.giverNpc)
                MinimapRadar.ClearWaypoint();
            guidePin = null;
        }

        string Instruction()
        {
            switch (Step)
            {
                case FirstAdventureStep.Move:
                    return SettingsSystem.ClassicMovement
                        ? "Press W to move forward. A and D turn your character."
                        : "Press W to move forward. A and D move you sideways.";
                case FirstAdventureStep.Look:
                    return "Hold the left mouse button and move the mouse to look around.";
                case FirstAdventureStep.Talk:
                    if (LocalStarter)
                        return $"Find {starterQuest.giverNpc} in {starterQuest.zone}. Press E to accept {starterQuest.title}.";
                    return player.CanTalk()
                        ? "Press E to talk to Abe, then accept The Forest Road."
                        : "Follow the quest marker to Abe at the village gate.";
                case FirstAdventureStep.Fight:
                    if (LocalStarter)
                        return $"Look for {starterQuest.targetName} outside the settlement. Press Tab to target and 1 to attack.";
                    return "Follow the north road. Press Tab to target a raider and 1 to use your first ability.";
                case FirstAdventureStep.Journal:
                    if (HasDifferentQuest)
                        return $"Press M to find your current quest, {factionQuests.GetActiveQuest()?.title}, on the map.";
                    if (LocalStarter)
                        return "Press M to open the map and see your local quest objective.";
                    return "Press J to see your quest in the Adventure Guide. M opens the map.";
                case FirstAdventureStep.Raiders:
                    if (LocalStarter)
                        return $"Defeat {starterQuest.targetName} ({StarterProgress}/{starterQuest.requiredCount}).";
                    return $"Defeat raiders along the north road ({Mathf.Min(player.Kills, 3)}/3).";
                case FirstAdventureStep.Return:
                    if (LocalStarter)
                        return $"Return to {starterQuest.giverNpc} and press E to turn in {starterQuest.title}.";
                    return player.CanTalk() ? "Press E and turn in The Forest Road to Abe."
                        : "Follow the quest marker back to Abe to turn in The Forest Road.";
                default:
                    return "Learn how to move, fight, and finish your first quest.";
            }
        }

        void OnGUI()
        {
            if (!Active || Suppressed || !(Offer || Engaged)) return;
            heading ??= new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            heading.normal.textColor = new Color(.97f, .81f, .50f);
            copy ??= new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            copy.normal.textColor = new Color(.95f, .94f, .90f);

            var previousMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = 430f;
            float x = (Screen.width / scale - width) * .5f;
            float y = 195f;
            bool offer = Offer;
            bool canPin = CanPinGuide;
            Color previous = GUI.color;
            GUI.color = new Color(.075f, .083f, .09f, .94f);
            GUI.DrawTexture(new Rect(x, y, width, offer || canPin ? 112 : 86), Texture2D.whiteTexture);
            GUI.color = previous;
            GUI.Label(new Rect(x + 12, y + 6, width - 96, 24), "First Adventure", heading);
            GUI.Label(new Rect(x + 12, y + 32, width - 24, 44), Instruction(), copy);
            if (offer)
            {
                if (GUI.Button(new Rect(x + 12, y + 73, 122, 27), "Learn the basics")) Begin();
                if (GUI.Button(new Rect(x + 142, y + 73, 104, 27), "Skip tutorial")) Skip();
            }
            else if (GUI.Button(new Rect(x + width - 64, y + 7, 52, 22), "Skip"))
                Skip();
            if (canPin && GUI.Button(new Rect(x + 12, y + 73, 132, 27), "Pin quest giver"))
                PinGuide();
            GUI.matrix = previousMatrix;
        }
    }
}
