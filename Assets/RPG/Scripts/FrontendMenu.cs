using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Riftbound
{
    public class FrontendMenu : MonoBehaviour
    {
        [Serializable]
        public class Character
        {
            public string id = "";
            public string name = "Brute";
            public string faction = "Alliance";
            public string race = "Human";
            public string characterClass = "Warrior";
        }

        [Serializable]
        class Roster
        {
            public List<Character> characters = new List<Character>();
            public int selected;
        }

        enum Page { Characters, Create, Settings, Controls }
        public static bool IsOpen { get; private set; }
        static Character enteringCharacter;

        PlayerController player;
        Roster roster;
        Page page = Page.Characters;

        // Creation state
        string newName = "";
        string error = "";
        int factionIndex = 0; // 0 = Alliance, 1 = Horde
        int raceIndex = 0;
        int classIndex = 0;

        readonly string[] allianceRaces = new[] { "Human", "Elf", "Dwarf" };
        readonly string[] hordeRaces = new[] { "Brute", "Undead", "Goblin" };
        readonly string[] availableClasses = new[] { "Warrior", "Mage", "Rogue", "Paladin" };

        static readonly string[] RandomNames = new[]
        {
            "Garrick", "Valeria", "Morgrim", "Drakath", "Theron",
            "Alleria", "Vaelen", "Cheryl", "Torvald", "Halford",
            "Justin", "Aldous", "Sylvan", "Lyra", "Kaelen",
            "Grimjaw", "Brynn", "Cedric", "Morgath", "Zephyrus"
        };

        // Deletion modal state
        bool showDeleteConfirm = false;

        // UI Styles & Assets
        GUIStyle brand, heading, subHeading, body, small, button, enterButton, dangerButton, field, cardTitle, cardSub;
        RenderTexture portrait;
        Camera portraitCamera;
        GameObject preview, previewLight;
        float previewYaw = 180f;
        bool isDraggingPreview;
        Vector2 lastMousePos;

        float volume = 0.8f;
        float sensitivity = 1.0f;
        readonly Color gold = new Color(0.95f, 0.79f, 0.38f);
        readonly Color allianceBlue = new Color(0.18f, 0.42f, 0.88f);
        readonly Color hordeRed = new Color(0.85f, 0.18f, 0.12f);

        string RosterPath => Path.Combine(Application.persistentDataPath, "characters-v1.json");
        Character Selected => (roster != null && roster.characters.Count > 0)
            ? roster.characters[Mathf.Clamp(roster.selected, 0, roster.characters.Count - 1)]
            : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            IsOpen = false;
            enteringCharacter = null;
            PlayerController.SavePathOverride = null;
        }

        public void Initialize(PlayerController owner)
        {
            player = owner;
            volume = PlayerPrefs.GetFloat("Riftbound.Volume", 0.8f);
            sensitivity = PlayerPrefs.GetFloat("Riftbound.Sensitivity", 1.0f);
            ApplySettings();

#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetBool("Riftbound.DirectPlayInEditor", false) && enteringCharacter == null)
            {
                IsOpen = false;
                if (player.orbit) player.orbit.SetPaused(false);
                player.Stats.displayName = "Adventurer";
                player.LoadAdventure();
                player.RefreshAdventureWorld();
                return;
            }
#endif

            if (enteringCharacter != null)
            {
                var character = enteringCharacter;
                enteringCharacter = null;
                IsOpen = false;
                player.CharacterSaveId = character.id;
                player.CharacterName = character.name;
                player.CharacterFaction = character.faction;
                player.CharacterRace = character.race;
                player.CharacterClass = character.characterClass;
                player.Stats.displayName = character.name;
                player.LoadAdventure();
                player.RefreshAdventureWorld();
                if (player.orbit) player.orbit.SetPaused(false);
                return;
            }

            roster = new Roster();
            try
            {
                if (File.Exists(RosterPath))
                    roster = JsonUtility.FromJson<Roster>(File.ReadAllText(RosterPath)) ?? new Roster();
            }
            catch (Exception exception)
            {
                error = "Could not read character list. Existing saves are kept.";
                Debug.LogWarning(exception.Message);
            }

            if (roster.characters == null) roster.characters = new List<Character>();
            roster.characters.RemoveAll(c => c == null || !ValidId(c.id));

            if (roster.characters.Count == 0)
            {
                // Default starter character
                roster.characters.Add(new Character { name = "Garrick", faction = "Alliance", race = "Human", characterClass = "Warrior" });
            }

            roster.selected = Mathf.Clamp(roster.selected, 0, roster.characters.Count - 1);
            IsOpen = true;
            if (player.orbit) player.orbit.SetPaused(true);

            page = roster.characters.Count > 0 ? Page.Characters : Page.Create;
            SetupPortraitCamera();
            RefreshPreview();
        }

        static bool ValidId(string id) => id == "" || (id != null && Guid.TryParseExact(id, "N", out _));

        void ApplySettings()
        {
            AudioListener.volume = volume;
            if (player && player.orbit) player.orbit.mouseSensitivity = sensitivity;
        }

        void SaveSettings()
        {
            PlayerPrefs.SetFloat("Riftbound.Volume", volume);
            PlayerPrefs.SetFloat("Riftbound.Sensitivity", sensitivity);
            PlayerPrefs.Save();
        }

        bool SaveRoster()
        {
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                File.WriteAllText(RosterPath, JsonUtility.ToJson(roster, true));
                return true;
            }
            catch (Exception exception)
            {
                error = "Could not save character list. Check disk access.";
                Debug.LogWarning(exception.Message);
                return false;
            }
        }

        public void EnterWorld()
        {
            if (Selected == null) return;
            if (!SaveRoster()) return;

            enteringCharacter = Selected;
            IsOpen = false;
            Time.timeScale = 1;
            CleanupPreview();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMenu()
        {
            player.CloseDialogue();
            if (!player.SaveAdventure(false)) return;
            enteringCharacter = null;
            IsOpen = false;
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        string GetCurrentRace()
        {
            string[] pool = factionIndex == 0 ? allianceRaces : hordeRaces;
            return pool[Mathf.Clamp(raceIndex, 0, pool.Length - 1)];
        }

        void CreateCharacter()
        {
            string name = newName.Trim();
            if (name.Length < 2 || name.Length > 16) { error = "Choose a name between 2 and 16 characters."; return; }
            foreach (char c in name)
            {
                if (!char.IsLetter(c) && c != ' ' && c != '-' && c != '\'')
                {
                    error = "Name can only contain letters, spaces, apostrophes or hyphens.";
                    return;
                }
            }

            if (roster.characters.Count >= 8) { error = "All 8 character slots are occupied."; return; }
            if (roster.characters.Exists(c => string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase)))
            {
                error = "A character with that name already exists in your roster.";
                return;
            }

            var created = new Character
            {
                id = Guid.NewGuid().ToString("N"),
                name = name,
                faction = factionIndex == 0 ? "Alliance" : "Horde",
                race = GetCurrentRace(),
                characterClass = availableClasses[Mathf.Clamp(classIndex, 0, availableClasses.Length - 1)]
            };

            roster.characters.Add(created);
            roster.selected = roster.characters.Count - 1;

            if (!SaveRoster())
            {
                roster.characters.Remove(created);
                roster.selected = 0;
                return;
            }

            error = "";
            page = Page.Characters;
            RefreshPreview();
        }

        void DeleteSelectedCharacter()
        {
            if (roster.characters.Count == 0) return;
            int idx = Mathf.Clamp(roster.selected, 0, roster.characters.Count - 1);
            var character = roster.characters[idx];

            string savePath = Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(character.id) ? "adventure-v1.json" : "adventure-" + character.id + ".json");
            try
            {
                if (File.Exists(savePath)) File.Delete(savePath);
                if (File.Exists(savePath + ".bak")) File.Delete(savePath + ".bak");
                if (File.Exists(savePath + ".tmp")) File.Delete(savePath + ".tmp");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to delete character save: " + ex.Message);
            }

            roster.characters.RemoveAt(idx);
            if (roster.characters.Count == 0)
            {
                page = Page.Create;
                newName = "";
                error = "";
            }
            else
            {
                roster.selected = Mathf.Clamp(idx, 0, roster.characters.Count - 1);
                page = Page.Characters;
            }

            SaveRoster();
            RefreshPreview();
        }

        int GetCharacterLevel(Character c)
        {
            if (c == null) return 1;
            string savePath = Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(c.id) ? "adventure-v1.json" : "adventure-" + c.id + ".json");
            try
            {
                if (File.Exists(savePath))
                {
                    var data = JsonUtility.FromJson<PlayerController.AdventureSave>(File.ReadAllText(savePath));
                    if (data != null)
                    {
                        int xp = data.experience;
                        if (xp >= 750) return 5;
                        if (xp >= 400) return 4;
                        if (xp >= 200) return 3;
                        if (xp >= 75) return 2;
                        return 1;
                    }
                }
            }
            catch { }
            return 1;
        }

        string GetCharacterZone(Character c)
        {
            if (c == null) return "Emberwatch Outpost";
            string savePath = Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(c.id) ? "adventure-v1.json" : "adventure-" + c.id + ".json");
            try
            {
                if (File.Exists(savePath))
                {
                    var data = JsonUtility.FromJson<PlayerController.AdventureSave>(File.ReadAllText(savePath));
                    if (data != null)
                    {
                        if (data.hasPosition)
                            return ZoneManager.GetZoneName(new Vector3(data.posX, data.posY, data.posZ));
                        if (data.landmarks != null)
                        {
                            if (Array.IndexOf(data.landmarks, "scarlet-crypts") >= 0) return "Scarlet Crypts (Duskwood)";
                            if (Array.IndexOf(data.landmarks, "redridge-frontier") >= 0) return "Redridge Frontier (Horde)";
                            if (Array.IndexOf(data.landmarks, "greenhollow-post") >= 0) return "Greenhollow Valley";
                        }
                    }
                }
            }
            catch { }
            return PlayerController.GetStartingZoneName(c.race);
        }

        void SetupPortraitCamera()
        {
            if (portrait == null)
            {
                portrait = new RenderTexture(700, 850, 24) { name = "WoW Character Portrait RT" };
                portrait.Create();
            }

            if (!portraitCamera)
            {
                var camObj = new GameObject("WoW Character Portrait Camera");
                portraitCamera = camObj.AddComponent<Camera>();
                portraitCamera.targetTexture = portrait;
                portraitCamera.cullingMask = 1 << 31;
                portraitCamera.clearFlags = CameraClearFlags.SolidColor;
                portraitCamera.backgroundColor = new Color(0.02f, 0.035f, 0.045f, 1f);
                portraitCamera.fieldOfView = 30f;
                portraitCamera.nearClipPlane = 0.05f;
                portraitCamera.farClipPlane = 25f;
            }

            if (!previewLight)
            {
                previewLight = new GameObject("WoW Portrait Light");
                var light = previewLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.cullingMask = 1 << 31;
                light.color = new Color(1.0f, 0.90f, 0.75f);
                light.intensity = 2.4f;
                previewLight.transform.rotation = Quaternion.Euler(32f, -30f, 0f);
            }
        }

        void RefreshPreview()
        {
            if (!IsOpen) return;
            if (preview) Destroy(preview);

            string targetRace = page == Page.Create ? GetCurrentRace() : (Selected != null ? Selected.race : "Human");
            string targetClass = page == Page.Create ? availableClasses[classIndex] : (Selected != null ? Selected.characterClass : "Warrior");

            preview = CharacterVisualFactory.SpawnPreview(targetRace, targetClass, null, previewYaw);
            if (!preview) return;

            var renderers = preview.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                Vector3 center = bounds.center;

                if (portraitCamera)
                {
                    float dist = Mathf.Max(2.8f, bounds.size.y * 2.05f);
                    portraitCamera.transform.position = center + new Vector3(0f, 0.05f, -dist);
                    portraitCamera.transform.LookAt(center);
                }
            }
        }

        void CleanupPreview()
        {
            if (preview) Destroy(preview);
            if (previewLight) Destroy(previewLight);
            if (portraitCamera) Destroy(portraitCamera.gameObject);
            if (portrait) { portrait.Release(); Destroy(portrait); }
        }

        void Update()
        {
            if (!IsOpen || Keyboard.current == null) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (showDeleteConfirm) { showDeleteConfirm = false; return; }
                if (page == Page.Create) { page = Page.Characters; error = ""; RefreshPreview(); return; }
                if (page == Page.Settings || page == Page.Controls) { page = Page.Characters; SaveSettings(); return; }
            }

            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    // Check if clicking in preview portrait center zone
                    float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
                    float previewLeft = (Screen.width - 1280 * scale) * 0.5f + 320 * scale;
                    float previewRight = previewLeft + 540 * scale;
                    if (mousePos.x >= previewLeft && mousePos.x <= previewRight && !showDeleteConfirm)
                    {
                        isDraggingPreview = true;
                        lastMousePos = mousePos;
                    }
                }
                else if (Mouse.current.leftButton.isPressed && isDraggingPreview)
                {
                    float deltaX = mousePos.x - lastMousePos.x;
                    previewYaw -= deltaX * 0.65f;
                    lastMousePos = mousePos;
                    if (preview) preview.transform.rotation = Quaternion.Euler(0f, previewYaw, 0f);
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    isDraggingPreview = false;
                }
            }
        }

        void OnDestroy()
        {
            CleanupPreview();
        }

        void InitStyles()
        {
            brand = new GUIStyle(GUI.skin.label) { fontSize = 42, fontStyle = FontStyle.Bold };
            brand.normal.textColor = gold;

            heading = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold };
            heading.normal.textColor = gold;

            subHeading = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            subHeading.normal.textColor = new Color(0.92f, 0.88f, 0.76f);

            body = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
            body.normal.textColor = new Color(0.86f, 0.87f, 0.82f);

            small = new GUIStyle(body) { fontSize = 12 };
            small.normal.textColor = new Color(0.65f, 0.68f, 0.68f);

            button = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold };

            enterButton = new GUIStyle(GUI.skin.button) { fontSize = 21, fontStyle = FontStyle.Bold };
            enterButton.normal.textColor = new Color(1.0f, 0.95f, 0.80f);

            dangerButton = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold };
            dangerButton.normal.textColor = new Color(1.0f, 0.40f, 0.40f);

            field = new GUIStyle(GUI.skin.textField) { fontSize = 20, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(12, 12, 6, 6) };

            cardTitle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            cardSub = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = false };
            cardSub.normal.textColor = new Color(0.72f, 0.74f, 0.72f);
        }

        void OnGUI()
        {
            if (!IsOpen || roster == null) return;
            if (brand == null) InitStyles();

            var oldMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) * 0.5f, (Screen.height - 720 * scale) * 0.5f), Quaternion.identity, Vector3.one * scale);

            // Dark gothic background atmosphere
            Fill(new Rect(-1000, -1000, 3280, 2720), new Color(0.012f, 0.020f, 0.026f, 0.72f));

            // Top WoW Bar
            DrawTopBar();

            // Active Page
            if (page == Page.Characters) DrawCharacterSelectPage();
            else if (page == Page.Create) DrawCharacterCreatePage();
            else if (page == Page.Settings) DrawSettingsPage();
            else if (page == Page.Controls) DrawControlsPage();

            // Error display
            if (!string.IsNullOrEmpty(error))
            {
                var errStyle = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
                errStyle.normal.textColor = new Color(1.0f, 0.35f, 0.35f);
                GUI.Label(new Rect(340, 630, 600, 28), error, errStyle);
            }

            // Delete confirmation modal (if open)
            if (showDeleteConfirm)
            {
                DrawDeleteConfirmationModal();
            }

            GUI.matrix = oldMatrix;
        }

        void DrawTopBar()
        {
            // Golden filigree top border
            Fill(new Rect(30, 20, 1220, 2), gold * 0.7f);

            GUI.Label(new Rect(40, 26, 400, 48), "RIFTBOUND", brand);

            var realmStyle = new GUIStyle(small) { fontStyle = FontStyle.Bold };
            realmStyle.normal.textColor = new Color(0.85f, 0.75f, 0.45f);
            GUI.Label(new Rect(42, 68, 450, 20), "CHRONICLES OF EMBERWATCH  â€¢  REALM: EMBERWATCH (PvE)", realmStyle);

#if UNITY_EDITOR
            bool directPlay = UnityEditor.EditorPrefs.GetBool("Riftbound.DirectPlayInEditor", false);
            GUI.backgroundColor = directPlay ? new Color(0.35f, 0.8f, 0.45f) : new Color(0.45f, 0.45f, 0.5f);
            if (GUI.Button(new Rect(1050, 28, 190, 32), directPlay ? "âš¡ Direct Play: ON" : "âš¡ Direct Play: OFF", button))
            {
                UnityEditor.EditorPrefs.SetBool("Riftbound.DirectPlayInEditor", !directPlay);
            }
            GUI.backgroundColor = Color.white;
#endif
        }

        void DrawCharacterSelectPage()
        {
            // Center 3D Preview Turntable
            DrawCharacterPreviewArea(new Rect(330, 85, 530, 540));

            // Right Column: Character Selection List (WoW Classic)
            DrawCharacterListColumn(new Rect(880, 85, 360, 480));

            // Bottom Right Action Buttons: Enter World / Create / Delete
            DrawActionButtonsBottomRight(new Rect(880, 575, 360, 125));

            // Bottom Left Navigation Buttons: Settings / Controls / Quit
            DrawNavigationButtonsBottomLeft(new Rect(40, 645, 480, 48));
        }

        void DrawCharacterPreviewArea(Rect rect)
        {
            if (portrait)
            {
                GUI.DrawTexture(rect, portrait, ScaleMode.ScaleToFit);
            }

            // Rotation Turntable buttons
            if (GUI.Button(new Rect(rect.x + 80, rect.yMax - 36, 55, 32), "â—„", button))
            {
                previewYaw -= 30f;
                if (preview) preview.transform.rotation = Quaternion.Euler(0f, previewYaw, 0f);
            }

            if (GUI.Button(new Rect(rect.xMax - 135, rect.yMax - 36, 55, 32), "â–º", button))
            {
                previewYaw += 30f;
                if (preview) preview.transform.rotation = Quaternion.Euler(0f, previewYaw, 0f);
            }

            var tipStyle = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(rect.x + 140, rect.yMax - 32, rect.width - 280, 24), "Drag to rotate", tipStyle);

            // Character Pedestal Info under model
            if (Selected != null)
            {
                int lvl = GetCharacterLevel(Selected);
                string zone = GetCharacterZone(Selected);

                Fill(new Rect(rect.x + 60, rect.yMax + 6, rect.width - 120, 46), new Color(0.04f, 0.07f, 0.09f, 0.90f));
                Fill(new Rect(rect.x + 60, rect.yMax + 6, rect.width - 120, 1), gold * 0.6f);

                var nameStyle = new GUIStyle(heading) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
                nameStyle.normal.textColor = GetClassColor(Selected.characterClass);
                GUI.Label(new Rect(rect.x + 60, rect.yMax + 9, rect.width - 120, 24), Selected.name, nameStyle);

                var detailsStyle = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(rect.x + 60, rect.yMax + 29, rect.width - 120, 20), $"Level {lvl} {Selected.race} {Selected.characterClass}  â€¢  {zone}", detailsStyle);
            }
        }

        void DrawCharacterListColumn(Rect rect)
        {
            // Panel frame
            Fill(new Rect(rect.x - 4, rect.y - 4, rect.width + 8, rect.height + 8), new Color(0.02f, 0.035f, 0.05f, 0.92f));
            Fill(rect, new Color(0.05f, 0.07f, 0.09f, 0.88f));

            GUI.Label(new Rect(rect.x + 16, rect.y + 10, rect.width - 32, 28), "CHARACTERS", heading);

            float cardY = rect.y + 44;
            float cardH = 50f;
            float cardSpacing = 6f;

            for (int i = 0; i < roster.characters.Count && i < 8; i++)
            {
                var character = roster.characters[i];
                bool isSelected = roster.selected == i;
                Rect cardRect = new Rect(rect.x + 10, cardY + i * (cardH + cardSpacing), rect.width - 20, cardH);

                // Card background & selection border
                if (isSelected)
                {
                    Fill(new Rect(cardRect.x - 2, cardRect.y - 2, cardRect.width + 4, cardRect.height + 4), gold);
                    Fill(cardRect, new Color(0.18f, 0.16f, 0.10f, 0.98f));
                }
                else
                {
                    Fill(cardRect, new Color(0.08f, 0.10f, 0.12f, 0.85f));
                }

                // Invisible button to handle selection
                if (GUI.Button(cardRect, GUIContent.none, GUIStyle.none))
                {
                    if (roster.selected != i)
                    {
                        roster.selected = i;
                        error = "";
                        RefreshPreview();
                    }
                }

                int lvl = GetCharacterLevel(character);
                string zone = GetCharacterZone(character);

                cardTitle.normal.textColor = isSelected ? gold : GetClassColor(character.characterClass);
                GUI.Label(new Rect(cardRect.x + 14, cardRect.y + 6, cardRect.width - 28, 20), character.name, cardTitle);
                GUI.Label(new Rect(cardRect.x + 14, cardRect.y + 26, cardRect.width - 28, 18), $"Lv {lvl} {character.race} {character.characterClass} Â· {zone}", cardSub);
            }
        }

        void DrawActionButtonsBottomRight(Rect rect)
        {
            // Enter World (Giant Red/Gold WoW Button)
            GUI.backgroundColor = new Color(0.78f, 0.18f, 0.12f);
            if (GUI.Button(new Rect(rect.x, rect.y, rect.width, 52), "ENTER WORLD", enterButton))
            {
                EnterWorld();
            }
            GUI.backgroundColor = Color.white;

            // Create Character (Left)
            GUI.enabled = roster.characters.Count < 8;
            if (GUI.Button(new Rect(rect.x, rect.y + 58, 175, 42), "+ CREATE CHARACTER", button))
            {
                newName = "";
                error = "";
                page = Page.Create;
                RefreshPreview();
            }
            GUI.enabled = true;

            // Delete Character (Right - Red button)
            GUI.enabled = roster.characters.Count > 0;
            GUI.backgroundColor = new Color(0.65f, 0.15f, 0.15f);
            if (GUI.Button(new Rect(rect.x + 185, rect.y + 58, 175, 42), "DELETE CHARACTER", dangerButton))
            {
                showDeleteConfirm = true;
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        void DrawNavigationButtonsBottomLeft(Rect rect)
        {
            if (GUI.Button(new Rect(rect.x, rect.y, 140, 38), "SETTINGS", button))
            {
                page = Page.Settings;
            }

            if (GUI.Button(new Rect(rect.x + 152, rect.y, 140, 38), "CONTROLS", button))
            {
                page = Page.Controls;
            }

            if (GUI.Button(new Rect(rect.x + 304, rect.y, 140, 38), "QUIT GAME", button))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        void DrawCharacterCreatePage()
        {
            // Header
            GUI.Label(new Rect(40, 85, 450, 32), "CHARACTER CREATION", heading);
            GUI.Label(new Rect(42, 114, 500, 20), "Choose your allegiance, heritage, and calling in Aethelgard.", small);

            // Left Column: Faction & Race Selection (Alliance vs Horde)
            DrawFactionAndRaceSelection(new Rect(40, 142, 330, 480));

            // Center Column: 3D Character Preview
            DrawCharacterPreviewArea(new Rect(370, 85, 490, 500));

            // Right Column: Class Selection
            DrawClassSelection(new Rect(870, 142, 370, 480));

            // Bottom Bar: Name Input + Randomize + Create Button + Back
            DrawCreationBottomBar(new Rect(280, 595, 720, 105));
        }

        void DrawFactionAndRaceSelection(Rect rect)
        {
            Fill(new Rect(rect.x - 4, rect.y - 4, rect.width + 8, rect.height + 8), new Color(0.02f, 0.035f, 0.05f, 0.92f));
            Fill(rect, new Color(0.05f, 0.07f, 0.09f, 0.88f));

            GUI.Label(new Rect(rect.x + 14, rect.y + 12, rect.width - 28, 24), "1. SELECT FACTION", subHeading);

            // Faction Buttons (Alliance vs Horde)
            GUI.backgroundColor = factionIndex == 0 ? allianceBlue : new Color(0.35f, 0.4f, 0.5f);
            if (GUI.Button(new Rect(rect.x + 14, rect.y + 40, (rect.width - 36) * 0.5f, 42), "ALLIANCE", button))
            {
                if (factionIndex != 0) { factionIndex = 0; raceIndex = 0; RefreshPreview(); }
            }

            GUI.backgroundColor = factionIndex == 1 ? hordeRed : new Color(0.45f, 0.35f, 0.35f);
            if (GUI.Button(new Rect(rect.x + 22 + (rect.width - 36) * 0.5f, rect.y + 40, (rect.width - 36) * 0.5f, 42), "HORDE", button))
            {
                if (factionIndex != 1) { factionIndex = 1; raceIndex = 0; RefreshPreview(); }
            }
            GUI.backgroundColor = Color.white;

            // Race Selection
            GUI.Label(new Rect(rect.x + 14, rect.y + 95, rect.width - 28, 24), "2. SELECT RACE", subHeading);

            string[] currentRaces = factionIndex == 0 ? allianceRaces : hordeRaces;
            for (int r = 0; r < currentRaces.Length; r++)
            {
                string rName = currentRaces[r];
                bool isSel = raceIndex == r;
                GUI.backgroundColor = isSel ? gold : Color.white;
                if (GUI.Button(new Rect(rect.x + 14, rect.y + 124 + r * 46, rect.width - 28, 40), rName, button))
                {
                    if (raceIndex != r) { raceIndex = r; RefreshPreview(); }
                }
            }
            GUI.backgroundColor = Color.white;

            // Race Description Lore Box
            Fill(new Rect(rect.x + 14, rect.y + 275, rect.width - 28, 185), new Color(0.03f, 0.04f, 0.06f, 0.95f));
            Fill(new Rect(rect.x + 14, rect.y + 275, rect.width - 28, 1), gold * 0.5f);

            var raceLoreTitle = new GUIStyle(cardTitle);
            raceLoreTitle.normal.textColor = gold;
            GUI.Label(new Rect(rect.x + 22, rect.y + 283, rect.width - 44, 22), GetCurrentRace() + " Racial Passives", raceLoreTitle);
            GUI.Label(new Rect(rect.x + 22, rect.y + 308, rect.width - 44, 140), ClassSystem.GetRaceDescription(GetCurrentRace()), body);
        }

        void DrawClassSelection(Rect rect)
        {
            Fill(new Rect(rect.x - 4, rect.y - 4, rect.width + 8, rect.height + 8), new Color(0.02f, 0.035f, 0.05f, 0.92f));
            Fill(rect, new Color(0.05f, 0.07f, 0.09f, 0.88f));

            GUI.Label(new Rect(rect.x + 14, rect.y + 12, rect.width - 28, 24), "3. SELECT CLASS", subHeading);

            for (int c = 0; c < availableClasses.Length; c++)
            {
                string cName = availableClasses[c];
                bool isSel = classIndex == c;
                GUI.backgroundColor = isSel ? GetClassColor(cName) : Color.white;
                if (GUI.Button(new Rect(rect.x + 14, rect.y + 40 + c * 46, rect.width - 28, 40), cName, button))
                {
                    if (classIndex != c) { classIndex = c; RefreshPreview(); }
                }
            }
            GUI.backgroundColor = Color.white;

            // Class Description & Role Breakdown Box
            Fill(new Rect(rect.x + 14, rect.y + 235, rect.width - 28, 225), new Color(0.03f, 0.04f, 0.06f, 0.95f));
            Fill(new Rect(rect.x + 14, rect.y + 235, rect.width - 28, 1), gold * 0.5f);

            string chosenClass = availableClasses[classIndex];
            var classTitleStyle = new GUIStyle(cardTitle);
            classTitleStyle.normal.textColor = GetClassColor(chosenClass);
            GUI.Label(new Rect(rect.x + 22, rect.y + 243, rect.width - 44, 22), chosenClass, classTitleStyle);
            GUI.Label(new Rect(rect.x + 22, rect.y + 268, rect.width - 44, 180), ClassSystem.GetClassDescription(chosenClass), body);
        }

        void DrawCreationBottomBar(Rect rect)
        {
            GUI.Label(new Rect(rect.x, rect.y, 160, 24), "CHARACTER NAME", small);

            newName = GUI.TextField(new Rect(rect.x, rect.y + 22, 280, 40), newName, 16, field);

            if (GUI.Button(new Rect(rect.x + 290, rect.y + 22, 110, 40), "RANDOM", button))
            {
                newName = RandomNames[UnityEngine.Random.Range(0, RandomNames.Length)];
                error = "";
            }

            GUI.backgroundColor = new Color(0.85f, 0.65f, 0.25f);
            if (GUI.Button(new Rect(rect.x + 415, rect.y + 20, 180, 44), "ACCEPT & CREATE", enterButton))
            {
                CreateCharacter();
            }
            GUI.backgroundColor = Color.white;

            if (GUI.Button(new Rect(rect.x + 605, rect.y + 22, 110, 40), "CANCEL", button))
            {
                page = Page.Characters;
                error = "";
                RefreshPreview();
            }
        }

        void DrawDeleteConfirmationModal()
        {
            // Dim background
            Fill(new Rect(-1000, -1000, 3280, 2720), new Color(0f, 0f, 0f, 0.82f));

            float boxW = 520f;
            float boxH = 270f;
            Rect box = new Rect(1280 * 0.5f - boxW * 0.5f, 720 * 0.5f - boxH * 0.5f, boxW, boxH);

            Fill(new Rect(box.x - 4, box.y - 4, boxW + 8, boxH + 8), new Color(0.75f, 0.15f, 0.15f, 0.95f)); // Crimson Border
            Fill(box, new Color(0.06f, 0.07f, 0.09f, 0.98f));

            var redHeading = new GUIStyle(heading) { alignment = TextAnchor.MiddleCenter };
            redHeading.normal.textColor = new Color(1.0f, 0.30f, 0.30f);
            GUI.Label(new Rect(box.x + 20, box.y + 18, boxW - 40, 32), "DELETE CHARACTER", redHeading);

            var centerBody = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(box.x + 20, box.y + 56, boxW - 40, 24), "Are you sure you want to permanently delete:", centerBody);

            if (Selected != null)
            {
                var charStyle = new GUIStyle(heading) { fontSize = 21, alignment = TextAnchor.MiddleCenter };
                charStyle.normal.textColor = gold;
                GUI.Label(new Rect(box.x + 20, box.y + 86, boxW - 40, 28), $"{Selected.name}  (Level {GetCharacterLevel(Selected)} {Selected.race} {Selected.characterClass})", charStyle);
            }

            var warnStyle = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter };
            warnStyle.normal.textColor = new Color(0.92f, 0.48f, 0.48f);
            GUI.Label(new Rect(box.x + 20, box.y + 120, boxW - 40, 38), "This action cannot be undone. All adventure progress,\nitems, gold, and talent points will be erased permanently.", warnStyle);

            // Confirm Delete (Red)
            GUI.backgroundColor = new Color(0.85f, 0.18f, 0.15f);
            if (GUI.Button(new Rect(box.x + 40, box.y + 185, 200, 48), "CONFIRM DELETE", enterButton))
            {
                DeleteSelectedCharacter();
                showDeleteConfirm = false;
            }
            GUI.backgroundColor = Color.white;

            // Cancel (Gold/Neutral)
            if (GUI.Button(new Rect(box.x + boxW - 240, box.y + 185, 200, 48), "CANCEL", button))
            {
                showDeleteConfirm = false;
            }
        }

        void DrawSettingsPage()
        {
            GUI.Label(new Rect(60, 100, 500, 40), "SYSTEM SETTINGS", heading);

            GUI.Label(new Rect(60, 160, 500, 30), $"MASTER VOLUME  ({Mathf.RoundToInt(volume * 100)}%)", body);
            volume = GUI.HorizontalSlider(new Rect(60, 195, 450, 25), volume, 0f, 1f);

            GUI.Label(new Rect(60, 245, 500, 30), $"CAMERA ROTATION SENSITIVITY  ({sensitivity:F1}x)", body);
            sensitivity = GUI.HorizontalSlider(new Rect(60, 280, 450, 25), sensitivity, 0.3f, 2.5f);
            ApplySettings();

            GUI.Label(new Rect(580, 160, 500, 160), "All settings are saved locally to your device.\n\nCamera sensitivity controls RMB orbit rotation speed while exploring the open world.\n\nUse [V] to swap between Ground and Flying mounts.", body);

            if (GUI.Button(new Rect(60, 420, 220, 44), "SAVE & RETURN", button))
            {
                SaveSettings();
                page = Page.Characters;
            }
        }

        void DrawControlsPage()
        {
            GUI.Label(new Rect(60, 100, 600, 40), "GAME CONTROLS", heading);

            GUI.Label(new Rect(60, 155, 540, 340),
                "MOVEMENT & CAMERA:\n" +
                "  W / A / S / D       Run / Strafe (WoW Responsive Physics)\n" +
                "  C                   Toggle slow RP walk\n" +
                "  SPACE               Jump (or Ascend while on Flying Mount)\n" +
                "  X                   Descend (while on Flying Mount)\n" +
                "  LEFT SHIFT (Hold)   Flight Sprint Boost (24 m/s)\n" +
                "  RMB + Mouse         Turn character & orbit camera\n" +
                "  Mouse Wheel         Zoom camera in / out\n\n" +
                "INTERACTION & MOUNTS:\n" +
                "  TAB                 Target nearest enemy\n" +
                "  E                   Interact with NPCs, quests & caches\n" +
                "  Z                   Summon / Dismount active mount\n" +
                "  V                   Swap selected mount (Horse / Zebra / Gryphon)", body);

            GUI.Label(new Rect(640, 155, 560, 340),
                "COMBAT & TALENTS:\n" +
                "  1 / LMB             Primary Class Strike / Spell\n" +
                "  2                   Secondary AOE Cleave / Frost Nova / Finisher\n" +
                "  3                   Class Utility / Shield / Heal / Sprint\n" +
                "  4                   Healing Potion\n" +
                "  N                   Specialization & Talent Trees\n\n" +
                "PANELS & SHORTCUTS:\n" +
                "  I                   Character Inventory & Gear\n" +
                "  M                   World Map (Frontiers & Landmarks)\n" +
                "  J                   Quest Journal\n" +
                "  F5                  Manual Save Progress\n" +
                "  ESC                 Game Menu / Cancel Selection", body);

            if (GUI.Button(new Rect(60, 520, 220, 44), "BACK TO CHARACTERS", button))
            {
                page = Page.Characters;
            }
        }

        Color GetClassColor(string cls)
        {
            switch (cls)
            {
                case "Warrior": return new Color(0.85f, 0.22f, 0.16f); // Red
                case "Mage": return new Color(0.25f, 0.65f, 0.95f);    // Cyan
                case "Rogue": return new Color(0.98f, 0.90f, 0.25f);   // Yellow
                case "Paladin": return new Color(0.96f, 0.55f, 0.73f); // Pink
                default: return gold;
            }
        }

        static void Fill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
