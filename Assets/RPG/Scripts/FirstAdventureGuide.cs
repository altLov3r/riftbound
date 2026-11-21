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
        float walked, turned, previousYaw, previousPitch;
        GUIStyle heading, copy;

        internal static FirstAdventureGuide Attach(PlayerController player)
        {
            if (!player || CoopSessionConfig.IsMultiplayer || !WorldRegions.Expanded ||
                player.CharacterFaction != "Alliance" || player.CharacterRace != "Human")
                return null;

            var guide = player.GetComponent<FirstAdventureGuide>();
            if (!guide) guide = player.gameObject.AddComponent<FirstAdventureGuide>();
            guide.player = player;
            guide.motor = player.GetComponent<CharacterController>();
            return guide;
        }

        FirstAdventureStep Step => player.TutorialStep;
        bool Active => player && !CoopSessionConfig.IsMultiplayer && !FrontendMenu.IsOpen;
        bool Engaged => Step > FirstAdventureStep.Unseen && Step < FirstAdventureStep.Complete;
        bool Suppressed => !player.Stats || !player.Stats.Alive || player.AnyBlockingWindowOpen() ||
            PrototypeHUD.MenuOpen || (player.orbit && player.orbit.Paused);
        bool Offer => Step == FirstAdventureStep.Unseen && player.QuestIndex == 0 && !player.QuestAccepted;

        internal void Begin()
        {
            if (!Active) return;
            walked = turned = 0;
            ChangeStep(FirstAdventureStep.Move);
        }

        void Skip() => ChangeStep(FirstAdventureStep.Skipped);

        void ChangeStep(FirstAdventureStep step)
        {
            if (Step == step) return;
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

            // Dialogue and the journal are legitimate tutorial actions, including when they block movement.
            switch (Step)
            {
                case FirstAdventureStep.Talk:
                    if (player.QuestIndex > 0 || player.QuestReady) ChangeStep(FirstAdventureStep.Journal);
                    else if (player.QuestAccepted) ChangeStep(FirstAdventureStep.Fight);
                    break;
                case FirstAdventureStep.Fight:
                    if (player.Kills > 0 || player.QuestIndex > 0) ChangeStep(FirstAdventureStep.Journal);
                    break;
                case FirstAdventureStep.Journal:
                    if (player.JournalOpen || player.MapOpen)
                        ChangeStep(player.QuestIndex > 0 ? FirstAdventureStep.Complete :
                            player.QuestReady ? FirstAdventureStep.Return : FirstAdventureStep.Raiders);
                    break;
                case FirstAdventureStep.Raiders:
                    if (player.QuestIndex > 0) ChangeStep(FirstAdventureStep.Complete);
                    else if (player.QuestReady) ChangeStep(FirstAdventureStep.Return);
                    break;
                case FirstAdventureStep.Return:
                    if (player.QuestIndex > 0) ChangeStep(FirstAdventureStep.Complete);
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
                    return player.CanTalk()
                        ? "Press E to talk to Abe, then accept The Forest Road."
                        : "Follow the quest marker to Abe at the village gate.";
                case FirstAdventureStep.Fight:
                    return "Follow the north road. Press Tab to target a raider and 1 to use your first ability.";
                case FirstAdventureStep.Journal:
                    return "Press J to see your quest in the Adventure Guide. M opens the map.";
                case FirstAdventureStep.Raiders:
                    return $"Defeat raiders along the north road ({Mathf.Min(player.Kills, 3)}/3).";
                case FirstAdventureStep.Return:
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

            float width = Mathf.Min(430f, Screen.width - 32f);
            float x = (Screen.width - width) * .5f;
            float y = 92f;
            bool offer = Offer;
            Color previous = GUI.color;
            GUI.color = new Color(.075f, .083f, .09f, .94f);
            GUI.DrawTexture(new Rect(x, y, width, offer ? 112 : 86), Texture2D.whiteTexture);
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
        }
    }
}
