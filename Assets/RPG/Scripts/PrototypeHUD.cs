using UnityEngine;

namespace Riftbound
{
    public partial class PrototypeHUD : MonoBehaviour
    {
        public PlayerController player;
        GUIStyle title, label, small, nameplate;
        GUIStyle questBody, questHeading, questButton;
        GUIStyle microBtnStyle, tooltipStyle, badgeStyle;
        static bool questTrackerCollapsed = false;
        static bool showHelpOverlay = false;
        string activeMicroTip = "";
        Rect activeMicroTipRect;
        Texture2D white;
        void Start()
        {
            if (!ActionBarSystem.Instance && !GetComponent<ActionBarSystem>()) gameObject.AddComponent<ActionBarSystem>();
            if (!CathedralBossEncounter.Instance && !GetComponent<CathedralBossEncounter>()) gameObject.AddComponent<CathedralBossEncounter>();
            if (!ApexBossEncounter.Instance && !GetComponent<ApexBossEncounter>()) gameObject.AddComponent<ApexBossEncounter>();
            if (!ZoneAudioAmbience.Instance && !GetComponent<ZoneAudioAmbience>()) gameObject.AddComponent<ZoneAudioAmbience>();
            if (!PetCompanionSystem.Instance && !GetComponent<PetCompanionSystem>()) gameObject.AddComponent<PetCompanionSystem>();
            if (!ProfessionSystem.Instance && !GetComponent<ProfessionSystem>()) gameObject.AddComponent<ProfessionSystem>();
            if (!AuctionHouseSystem.Instance && !GetComponent<AuctionHouseSystem>()) gameObject.AddComponent<AuctionHouseSystem>();
            if (!WorldBossEncounter.Instance && !GetComponent<WorldBossEncounter>()) gameObject.AddComponent<WorldBossEncounter>();
            if (!MacroSystem.Instance && !GetComponent<MacroSystem>()) gameObject.AddComponent<MacroSystem>();
            if (!SettingsSystem.Instance && !GetComponent<SettingsSystem>()) gameObject.AddComponent<SettingsSystem>();
            if (!RestedXPSystem.Instance && !GetComponent<RestedXPSystem>()) gameObject.AddComponent<RestedXPSystem>();
            if (!DungeonFinderSystem.Instance && !GetComponent<DungeonFinderSystem>()) gameObject.AddComponent<DungeonFinderSystem>();
            if (!ArchaeologySystem.Instance && !GetComponent<ArchaeologySystem>()) gameObject.AddComponent<ArchaeologySystem>();
            if (!TransmogSystem.Instance && !GetComponent<TransmogSystem>()) gameObject.AddComponent<TransmogSystem>();
            if (!FishingSystem.Instance && !GetComponent<FishingSystem>()) gameObject.AddComponent<FishingSystem>();
            if (!CalendarEventSystem.Instance && !GetComponent<CalendarEventSystem>()) gameObject.AddComponent<CalendarEventSystem>();
            if (!WeatherSystem.Instance && !GetComponent<WeatherSystem>()) gameObject.AddComponent<WeatherSystem>();
            if (!PetBattleSystem.Instance && !GetComponent<PetBattleSystem>()) gameObject.AddComponent<PetBattleSystem>();
            if (!BarberShopSystem.Instance && !GetComponent<BarberShopSystem>()) gameObject.AddComponent<BarberShopSystem>();
            if (!WorldPvPSystem.Instance && !GetComponent<WorldPvPSystem>()) gameObject.AddComponent<WorldPvPSystem>();
            if (!EquipmentSetSystem.Instance && !GetComponent<EquipmentSetSystem>()) gameObject.AddComponent<EquipmentSetSystem>();
            if (!DamageMeterSystem.Instance && !GetComponent<DamageMeterSystem>()) gameObject.AddComponent<DamageMeterSystem>();
            if (!GlyphSystem.Instance && !GetComponent<GlyphSystem>()) gameObject.AddComponent<GlyphSystem>();
            if (!GemSocketSystem.Instance && !GetComponent<GemSocketSystem>()) gameObject.AddComponent<GemSocketSystem>();
        }
        void OnGUI()
        {
            if (!player || !player.Stats || FrontendMenu.IsOpen) return;
            if (white == null)
            {
                white = Texture2D.whiteTexture;
                title = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold };
                title.normal.textColor = new Color(.95f, .79f, .45f);
                label = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
                label.normal.textColor = new Color(.92f, .94f, .91f);
                small = new GUIStyle(label) { fontSize = 13 };
                nameplate = new GUIStyle(small) { fontSize = 12, alignment = TextAnchor.MiddleCenter, wordWrap = false };
                questBody = new GUIStyle(label) { fontSize = 17 };
                questBody.normal.textColor = new Color(.19f, .12f, .065f);
                questHeading = new GUIStyle(questBody) { fontSize = 22, fontStyle = FontStyle.Bold };
                questButton = new GUIStyle(GUI.skin.button) { fontSize = 16, fontStyle = FontStyle.Bold };
                microBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                tooltipStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                tooltipStyle.normal.textColor = new Color(1f, 0.88f, 0.35f);
                badgeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 9,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                badgeStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
            }
            var oldMatrix = GUI.matrix;
            foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            {
                var stats = enemy.GetComponent<Combatant>();
                if (!stats.Alive || Vector3.Distance(player.transform.position, enemy.transform.position) > 16) continue;
                var point = Camera.main.WorldToScreenPoint(enemy.transform.position + Vector3.up * 2.6f);
                if (point.z <= 0) continue;
                var rect = new Rect(point.x - 45, Screen.height - point.y, 90, 7);
                Bar(rect, stats.Health / stats.maxHealth, new Color(.8f, .23f, .12f));
                if (!enemy.trainingDummy && (player.Target == stats || Vector3.Distance(player.transform.position, enemy.transform.position) < 10))
                {
                    int enemyLevel = enemy.isBoss ? 5 : enemy.heavyAttacker ? 4 : enemy.campGuard ? 3 : 2;
                    int levelDiff = enemyLevel - player.Level;
                    Color tagColor = levelDiff <= -3 ? new Color(0.65f, 0.65f, 0.65f) :
                                     levelDiff <= -1 ? new Color(0.2f, 0.85f, 0.25f) :
                                     levelDiff == 0 ? new Color(0.95f, 0.90f, 0.2f) :
                                     levelDiff <= 2 ? new Color(1f, 0.55f, 0.1f) :
                                     new Color(0.9f, 0.15f, 0.15f);
                    nameplate.normal.textColor = tagColor;
                    string tagText = enemy.isBoss ? $"[Lv {enemyLevel} Boss] {stats.displayName}" : $"[Lv {enemyLevel}] {stats.displayName}";
                    GUI.Label(new Rect(point.x - 100, rect.y - 25, 200, 23), tagText, nameplate);
                }
            }
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            float width = Screen.width / scale, height = Screen.height / scale;
            activeMicroTip = "";

            // --- 1. Low-Profile Player Unit Frame (WoW Style: Portrait ring + Name/Title + HP + Resource + XP) ---
            float pfx = 24f, pfy = 20f, pfw = 224f, pfh = 52f;
            Fill(new Rect(pfx, pfy, pfw, pfh), new Color(0.06f, 0.08f, 0.11f, 0.90f));
            Fill(new Rect(pfx, pfy, pfw, 1.5f), new Color(0.85f, 0.70f, 0.28f, 0.95f)); // Gold top trim
            Fill(new Rect(pfx, pfy + pfh - 1f, pfw, 1f), new Color(0.35f, 0.30f, 0.15f, 0.8f));

            // Class Icon Portrait Badge
            float portSize = 36f;
            Fill(new Rect(pfx + 6f, pfy + 8f, portSize, portSize), new Color(0.12f, 0.15f, 0.20f, 0.95f));
            string classIcon = player.CharacterClass == "Warrior" ? "⚔" :
                               player.CharacterClass == "Paladin" ? "✝" :
                               player.CharacterClass == "Mage" ? "🔥" : "🗡";
            var portStyle = new GUIStyle(title) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(pfx + 6f, pfy + 7f, portSize, portSize), classIcon, portStyle);

            // Level Circle Badge (Overlapping bottom-right of portrait)
            Fill(new Rect(pfx + portSize - 4f, pfy + portSize - 4f, 16f, 15f), new Color(0.88f, 0.72f, 0.25f, 1f));
            GUI.Label(new Rect(pfx + portSize - 4f, pfy + portSize - 5f, 16f, 15f), $"{player.Level}", badgeStyle);

            // Formatted Name & Title
            float textX = pfx + portSize + 12f;
            float barW = pfw - portSize - 18f;
            string charName = PlayerTitleSystem.GetFormattedName(player);
            GUI.Label(new Rect(textX, pfy + 3f, barW, 16f), charName, label);

            // Heroic Difficulty Badge on Player Frame
            if (DungeonInstanceManager.Instance && DungeonInstanceManager.Instance.CurrentDifficulty == DungeonInstanceManager.DungeonDifficulty.Heroic)
            {
                Fill(new Rect(pfx + pfw - 62f, pfy - 14f, 62f, 13f), new Color(0.6f, 0.12f, 0.12f, 0.95f));
                GUI.Label(new Rect(pfx + pfw - 62f, pfy - 15f, 62f, 13f), "💀 HEROIC", badgeStyle);
            }

            // Health Bar (13px)
            float hpPct = player.Stats.Health / player.Stats.maxHealth;
            Bar(new Rect(textX, pfy + 21f, barW, 13f), hpPct, new Color(0.16f, 0.75f, 0.22f));
            GUI.Label(new Rect(textX, pfy + 19f, barW, 15f), $"{Mathf.CeilToInt(player.Stats.Health)} / {player.Stats.maxHealth}", small);

            // Resource Bar (9px)
            Color resColor = player.CharacterClass == "Warrior" ? new Color(0.85f, 0.18f, 0.12f) :
                             player.CharacterClass == "Mage" ? new Color(0.18f, 0.48f, 0.95f) :
                             player.CharacterClass == "Rogue" ? new Color(0.95f, 0.85f, 0.15f) :
                             new Color(1f, 0.85f, 0.25f);
            float resRatio = Mathf.Clamp01(player.ClassResource / Mathf.Max(1f, player.MaxClassResource));
            Bar(new Rect(textX, pfy + 36f, barW, 9f), resRatio, resColor);

            // Ultra-thin XP Bar along bottom edge of Player Unit Frame (3px)
            float xpRatio = player.Level == 5 ? 1f : (float)(player.Experience - player.LevelFloor) / Mathf.Max(1, player.NextLevelExperience - player.LevelFloor);
            Bar(new Rect(pfx, pfy + pfh - 3f, pfw, 3f), xpRatio, new Color(0.55f, 0.25f, 0.90f));

            // Tooltip on Player Frame Hover (Shows Gold & XP Details)
            if (new Rect(pfx, pfy, pfw, pfh).Contains(Event.current.mousePosition))
            {
                activeMicroTip = $"<b>{charName}</b>\nLevel {player.Level} {player.CharacterRace} {player.CharacterClass}\nGold: {player.Gold} 💰  ·  XP: {player.Experience}/{player.NextLevelExperience}";
                activeMicroTipRect = new Rect(Event.current.mousePosition.x + 12f, Event.current.mousePosition.y + 12f, 210f, 48f);
            }

            // --- 2. Low-Profile Target Unit Frame (Directly adjacent to Player Frame) ---
            if (player.Target)
            {
                float tfx = pfx + pfw + 10f, tfy = pfy, tfw = 205f, tfh = 52f;
                Fill(new Rect(tfx, tfy, tfw, tfh), new Color(0.06f, 0.08f, 0.11f, 0.90f));
                Fill(new Rect(tfx, tfy, tfw, 1.5f), new Color(0.85f, 0.35f, 0.28f, 0.95f)); // Crimson top trim
                Fill(new Rect(tfx, tfy + tfh - 1f, tfw, 1f), new Color(0.35f, 0.15f, 0.15f, 0.8f));

                var enemyComp = player.Target.GetComponentInParent<EnemyController>();
                int targetLvl = enemyComp ? (enemyComp.isBoss ? 5 : enemyComp.heavyAttacker ? 4 : enemyComp.campGuard ? 3 : 2) : 1;
                string targetIcon = enemyComp && enemyComp.isBoss ? "💀" : "⚔";

                // Target Portrait Icon
                Fill(new Rect(tfx + 6f, tfy + 8f, portSize, portSize), new Color(0.22f, 0.10f, 0.10f, 0.95f));
                GUI.Label(new Rect(tfx + 6f, tfy + 7f, portSize, portSize), targetIcon, portStyle);

                // Target Level Badge
                Fill(new Rect(tfx + portSize - 4f, tfy + portSize - 4f, 16f, 15f), new Color(0.85f, 0.22f, 0.18f, 1f));
                GUI.Label(new Rect(tfx + portSize - 4f, tfy + portSize - 5f, 16f, 15f), $"{targetLvl}", badgeStyle);

                // Target Name & Raid Marker
                float ttextX = tfx + portSize + 12f;
                float tbarW = tfw - portSize - 18f;
                var tMarker = PartySystem.GetMarker(player.Target);
                string tNameText = tMarker != RaidTargetMarker.None ? $"{PartySystem.GetMarkerGlyph(tMarker)} {player.Target.displayName}" : player.Target.displayName;
                GUI.Label(new Rect(ttextX, tfy + 3f, tbarW - 24f, 16f), tNameText, label);

                // Target Health Bar
                float thpPct = player.Target.Health / player.Target.maxHealth;
                Bar(new Rect(ttextX, tfy + 21f, tbarW, 13f), thpPct, new Color(0.85f, 0.22f, 0.16f));
                GUI.Label(new Rect(ttextX, tfy + 19f, tbarW, 15f), $"{Mathf.CeilToInt(player.Target.Health)} / {player.Target.maxHealth}", small);

                // Target Debuffs (Auras)
                if (BuffSystem.Instance)
                {
                    float debuffY = (CastBarSystem.Instance && CastBarSystem.IsTargetCasting(player.Target)) ? tfy + tfh + 24f : tfy + tfh + 6f;
                    BuffSystem.Instance.DrawTargetDebuffsGUI(player.Target, tfx, debuffY);
                }

                // Target Duel Button
                var duelSys = DuelSystem.Instance ?? UnityEngine.Object.FindAnyObjectByType<DuelSystem>();
                if (duelSys && duelSys.State == DuelSystem.DuelState.Inactive && player.Target.Alive && player.Target != player.Stats)
                {
                    if (GUI.Button(new Rect(tfx + tfw + 4f, tfy + 12f, 54f, 26f), "⚔ DUEL", small))
                    {
                        duelSys.Challenge(player.Target, out _);
                    }
                }
            }

            // --- 3. Low-Profile Collapsible Quest Tracker (Beneath Minimap) ---
            float qtw = 236f;
            float qtx = width - qtw - 16f;
            float qty = 166f;

            // Header Bar
            Fill(new Rect(qtx, qty, qtw, 20f), new Color(0.06f, 0.08f, 0.11f, 0.85f));
            Fill(new Rect(qtx, qty, qtw, 1.5f), new Color(0.85f, 0.70f, 0.28f, 0.95f));

            string toggleText = questTrackerCollapsed ? "[+] " : "[-] ";
            if (GUI.Button(new Rect(qtx + 2f, qty, 24f, 19f), toggleText, small))
            {
                questTrackerCollapsed = !questTrackerCollapsed;
                AudioEffects.PlayClick();
            }
            GUI.Label(new Rect(qtx + 26f, qty + 1f, qtw - 30f, 18f), $"QUESTS ({player.QuestIndex + 1}/3)", small);

            if (!questTrackerCollapsed)
            {
                float bodyH = 74f;
                Fill(new Rect(qtx, qty + 20f, qtw, bodyH), new Color(0.04f, 0.05f, 0.07f, 0.65f));

                // Quest Title in Gold
                GUI.Label(new Rect(qtx + 8f, qty + 22f, qtw - 16f, 18f), player.QuestName, label);

                // Objective with Checkbox
                string check = player.QuestReady ? "☑ " : "☐ ";
                GUI.Label(new Rect(qtx + 8f, qty + 40f, qtw - 16f, 32f), check + player.QuestObjective, small);

                // Distance
                float distance = Vector3.Distance(player.transform.position, player.ObjectivePosition);
                GUI.Label(new Rect(qtx + 8f, qty + 74f, qtw - 16f, 18f), player.QuestComplete ? "All 3 quests completed" : $"Objective: {Mathf.RoundToInt(distance)}m away", small);
            }

            if (!player.DialogueOpen)
            {
                int unspent = TalentSystem.GetUnspentTalentPoints(player);
                if (unspent > 0)
                {
                    float pulse = 0.8f + Mathf.PingPong(Time.time * 2f, 0.4f);
                    var oldColor = GUI.color;
                    GUI.color = new Color(1f, 0.85f * pulse, 0.25f * pulse);
                    if (GUI.Button(new Rect(width / 2 - 140, height - 90, 280, 26), $"★ {unspent} UNSPENT TALENT POINT{(unspent > 1 ? "S" : "")} [N]", questButton))
                    {
                        player.TalentsOpen = !player.TalentsOpen;
                    }
                    GUI.color = oldColor;
                }
                else if (player.Mounted && player.SelectedMount && player.SelectedMount.isFlying)
                {
                    Panel(new Rect(width / 2 - 200, height - 85, 400, 24));
                    GUI.Label(new Rect(width / 2 - 190, height - 85, 380, 22), "★ FLYING: [Space] Ascend  ·  [X] Descend  ·  [Shift] Boost", small);
                }

                if (ActionBarSystem.Instance)
                {
                    ActionBarSystem.Instance.DrawActionBar(player, width, height);
                }
                else
                {
                    Panel(new Rect(width / 2 - 300, height - 105, 600, 85));
                    var ab1 = ClassSystem.GetAbilityInfo(player.CharacterClass, 1);
                    var ab2 = ClassSystem.GetAbilityInfo(player.CharacterClass, 2);
                    var ab3 = ClassSystem.GetAbilityInfo(player.CharacterClass, 3);
                    GUI.Label(new Rect(width / 2 - 280, height - 98, 560, 24), $"[1 / LMB] {ab1.name}   [2] {ab2.name}   [3] {ab3.name}", label);
                    GUI.Label(new Rect(width / 2 - 280, height - 70, 560, 25), $"[2] {Cooldown(player.CleaveReady)}   [3] {Cooldown(player.HealReady)}   [4] Potion x{player.Potions}   ·   {player.ClassResourceName}: {Mathf.RoundToInt(player.ClassResource)}", small);
                }

                // Centered Notification Toast
                if (!string.IsNullOrEmpty(player.Message))
                {
                    float msgW = Mathf.Min(480f, width - 120f);
                    float msgX = width / 2f - msgW / 2f;
                    float msgY = height - 110f;
                    Fill(new Rect(msgX - 6f, msgY - 3f, msgW + 12f, 26f), new Color(0.04f, 0.06f, 0.08f, 0.85f));
                    var centerMsg = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
                    GUI.Label(new Rect(msgX, msgY, msgW, 22f), player.Message, centerMsg);
                }

                // --- 4. WoW-Style Low-Profile Micro-Menu Icons Bar (Bottom Right) ---
                float btnSize = 25f;
                float btnSpacing = 3f;
                float totalBarW = 16 * (btnSize + btnSpacing);
                float mmX = width - totalBarW - 14f;
                float mmY = height - 29f;

                void DrawMicroIcon(int index, string icon, string tipText, System.Action onClick, bool pulse = false)
                {
                    Rect btnRect = new Rect(mmX + index * (btnSize + btnSpacing), mmY, btnSize, btnSize);
                    if (pulse)
                    {
                        float p = 0.8f + Mathf.PingPong(Time.time * 3f, 0.35f);
                        GUI.color = new Color(1f, 0.85f * p, 0.25f * p);
                    }
                    if (GUI.Button(btnRect, icon, microBtnStyle))
                    {
                        onClick?.Invoke();
                        AudioEffects.PlayClick();
                    }
                    GUI.color = Color.white;

                    if (btnRect.Contains(Event.current.mousePosition))
                    {
                        activeMicroTip = tipText;
                        activeMicroTipRect = new Rect(btnRect.x - 45f, btnRect.y - 28f, 125f, 24f);
                    }
                }

                int unspentPts = TalentSystem.GetUnspentTalentPoints(player);
                bool hasMail = MailboxSystem.Instance && MailboxSystem.Instance.HasUnreadMail();
                DrawMicroIcon(0, "👤", "Character [C]", () => { if (CharacterSheetSystem.Instance) CharacterSheetSystem.Instance.ToggleSheet(); });
                DrawMicroIcon(1, "📖", "Spellbook [P]", () => { player.SpellbookOpen = !player.SpellbookOpen; });
                DrawMicroIcon(2, "⚡", unspentPts > 0 ? $"Talents ({unspentPts}) [N]" : "Talents [N]", () => { player.TalentsOpen = !player.TalentsOpen; }, unspentPts > 0);
                DrawMicroIcon(3, "📜", "Quest Log [J]", () => { player.JournalOpen = !player.JournalOpen; });
                DrawMicroIcon(4, "🏰", "Guild & Vault [G]", () => { if (GuildSystem.Instance) GuildSystem.Instance.IsOpen = !GuildSystem.Instance.IsOpen; });
                DrawMicroIcon(5, "⚒", "Professions [K]", () => { ProfessionSystem.IsOpen = !ProfessionSystem.IsOpen; });
                DrawMicroIcon(6, "🏛", "Auction House", () => { AuctionHouseSystem.AuctionOpen = !AuctionHouseSystem.AuctionOpen; });
                DrawMicroIcon(7, "🏆", "Achievements [Y]", () => { player.AchievementOpen = !player.AchievementOpen; });
                DrawMicroIcon(8, "🎖", "Reputation [U]", () => { player.ReputationOpen = !player.ReputationOpen; });
                DrawMicroIcon(9, "⚔", "PvP & Arena [Shift+H]", () => { if (BattlegroundSystem.Instance) BattlegroundSystem.Instance.IsWindowOpen = !BattlegroundSystem.Instance.IsWindowOpen; });
                DrawMicroIcon(10, "👁", "Dungeon Finder [I/LFG]", () => { if (DungeonFinderSystem.Instance) DungeonFinderSystem.Instance.ToggleWindow(); }, DungeonFinderSystem.Instance && DungeonFinderSystem.Instance.State == DungeonFinderSystem.QueueState.Queued);
                DrawMicroIcon(11, "🐾", "Pet Journal & Battles [Shift+P]", () => { if (PetBattleSystem.Instance) PetBattleSystem.Instance.ToggleJournal(); });
                DrawMicroIcon(12, "✉", hasMail ? "Mailbox (New Mail!)" : "Mailbox", () => { if (MailboxSystem.Instance) MailboxSystem.Instance.IsOpen = !MailboxSystem.Instance.IsOpen; }, hasMail);
                DrawMicroIcon(13, "🎒", "Bags [B]", () => { player.InventoryOpen = !player.InventoryOpen; });
                DrawMicroIcon(14, "🗺", "World Map [M]", () => { player.MapOpen = !player.MapOpen; });
                DrawMicroIcon(15, "❓", "Help & Keybinds", () => { showHelpOverlay = !showHelpOverlay; });

                // Micro-Menu Tooltip
                if (!string.IsNullOrEmpty(activeMicroTip))
                {
                    GUI.Box(activeMicroTipRect, activeMicroTip, tooltipStyle);
                }

                // Help & Controls Popup Overlay (Only when requested via ❓ button)
                if (showHelpOverlay)
                {
                    float hw = 340f, hh = 145f;
                    float hx = width - hw - 14f, hy = height - hh - 36f;
                    Fill(new Rect(hx - 2f, hy - 2f, hw + 4f, hh + 4f), new Color(0.85f, 0.70f, 0.28f, 0.95f));
                    Fill(new Rect(hx, hy, hw, hh), new Color(0.06f, 0.08f, 0.11f, 0.96f));

                    GUI.Label(new Rect(hx + 10f, hy + 6f, hw - 50f, 20f), "KEYBOARD & CONTROLS", label);
                    if (GUI.Button(new Rect(hx + hw - 26f, hy + 5f, 20f, 20f), "✕", microBtnStyle))
                    {
                        showHelpOverlay = false;
                        AudioEffects.PlayClick();
                    }
                    string helpText = "<b>Movement:</b> WASD Run  ·  C Walk  ·  Space Jump\n" +
                                      "<b>Camera:</b> RMB + Mouse Look  ·  Wheel Zoom\n" +
                                      "<b>Combat:</b> 1-3 Abilities  ·  4 Potion  ·  Q Shield  ·  Z Mount\n" +
                                      "<b>UI Windows:</b> C/I Bags  ·  N Talents  ·  J Quest  ·  M Map  ·  K Craft\n" +
                                      "<b>Chat:</b> Enter to Chat  ·  Tab Target";
                    GUI.Label(new Rect(hx + 10f, hy + 30f, hw - 20f, hh - 36f), helpText, small);
                }
            }
            if (!player.DialogueOpen && !player.ShopOpen && LootDropSystem.Instance && LootDropSystem.Instance.GetNearbyCorpse(player.transform.position, 3.2f) is var corpse && corpse != null)
                GUI.Label(new Rect(width / 2 - 140, height / 2 + 70, 350, 35), $"[E] Loot {corpse.name}", label);
            else if (!player.DialogueOpen && !player.ShopOpen && Mailbox.GetNearby(player.transform.position) is var mbNear && mbNear != null)
                GUI.Label(new Rect(width / 2 - 140, height / 2 + 70, 350, 35), "[E] Open Mailbox", label);
            else if (!player.DialogueOpen && !player.ShopOpen && TownNPC.FindNearby(player.transform.position) is TownNPC townNpcAuc && townNpcAuc.roleTag == "Auctioneer")
                GUI.Label(new Rect(width / 2 - 140, height / 2 + 70, 360, 35), "[E] Browse Auction House", label);
            else if (!player.DialogueOpen && !player.ShopOpen && FlightMaster.GetNearby(player.transform.position) is var fm && fm != null)
                GUI.Label(new Rect(width / 2 - 140, height / 2 + 70, 360, 35), "[E] Flight Master: " + fm.nodeName, label);
            else if (!player.DialogueOpen && !player.ShopOpen && GatheringNode.GetNearby(player.transform.position, 3.2f) is var gn && gn != null)
                GUI.Label(new Rect(width / 2 - 140, height / 2 + 70, 360, 35), "[E] " + gn.ActionPrompt, label);
            else if (!player.DialogueOpen && !player.ShopOpen && player.NearbyMerchant())
                GUI.Label(new Rect(width / 2 - 140, height / 2 + 70, 350, 35), "[E] " + player.NearbyMerchant().npcName, label);
            else if (!player.DialogueOpen && player.CanTalk())
                GUI.Label(new Rect(width / 2 - 100, height / 2 + 70, 260, 35), "[E] Talk to Abe", label);
            else if (!player.DialogueOpen && player.NearbyLandmark())
                GUI.Label(new Rect(width / 2 - 150, height / 2 + 70, 380, 35), "[E] Discover " + player.NearbyLandmark().displayName, label);
            else if (!player.DialogueOpen && player.NearbyRune())
                GUI.Label(new Rect(width / 2 - 120, height / 2 + 70, 300, 35), "[E] Activate runestone", label);
            else if (!player.DialogueOpen && player.NearbySupply())
                GUI.Label(new Rect(width / 2 - 120, height / 2 + 70, 300, 35), "[E] Recover supplies", label);
            else if (!player.DialogueOpen && player.NearbyChest())
                GUI.Label(new Rect(width / 2 - 120, height / 2 + 70, 300, 35), "[E] Open loot cache", label);
            else if (!player.DialogueOpen && !player.ShopOpen && TownNPC.FindNearby(player.transform.position) is TownNPC townNpc)
                GUI.Label(new Rect(width / 2 - 140, height / 2 + 70, 360, 35), "[E] Speak with " + townNpc.npcName, label);
            if (player.DialogueOpen)
            {
                if (player.ActiveQuestGiver) DrawQuestWindow(height);
                else if (player.ActiveTownNPC) DrawTownNPCGossip(width, height);
            }
            if (player.InventoryOpen && !player.DialogueOpen && !(player.orbit && player.orbit.Paused))
            {
                DrawEquipment(width, height);
            }
            if (player.TalentsOpen && !player.DialogueOpen && !(player.orbit && player.orbit.Paused))
            {
                DrawTalentWindow(width, height);
            }
            if (player.SpellbookOpen && !player.DialogueOpen && !(player.orbit && player.orbit.Paused))
            {
                DrawSpellbook(width, height);
            }
            if (AchievementSystem.Instance)
            {
                AchievementSystem.Instance.DrawToastGUI(width, height);
                if (player.AchievementOpen && !player.DialogueOpen && !(player.orbit && player.orbit.Paused))
                {
                    AchievementSystem.Instance.DrawAchievementsWindow(width, height);
                }
            }
            if (ReputationSystem.Instance && player.ReputationOpen && !player.DialogueOpen && !(player.orbit && player.orbit.Paused))
            {
                ReputationSystem.Instance.DrawReputationWindow(width, height);
            }
            if (EncounterJournal.Instance && player.EncounterJournalOpen && !player.DialogueOpen && !(player.orbit && player.orbit.Paused))
            {
                EncounterJournal.Instance.DrawEncounterJournalGUI(width, height);
            }
            if (ProfessionSystem.Instance && ProfessionSystem.IsOpen && !player.DialogueOpen && !(player.orbit && player.orbit.Paused))
            {
                ProfessionSystem.Instance.DrawProfessionsGUI(width, height);
            }
            if (AuctionHouseSystem.Instance && !(player.orbit && player.orbit.Paused))
            {
                AuctionHouseSystem.Instance.DrawAuctionGUI(width, height);
                AuctionHouseSystem.Instance.DrawMailboxGUI(width, height);
            }
            if (WorldBossEncounter.Instance)
            {
                WorldBossEncounter.Instance.DrawBossHealthBar(width, height);
            }
            if (AuctionHouseSystem.Instance && AuctionHouseSystem.Instance.HasUnreadMail())
            {
                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1.0f, 0.85f, 0.2f, 0.95f);
                if (GUI.Button(new Rect(width - 165, 8, 115, 26), "✉ NEW MAIL", questButton))
                {
                    AuctionHouseSystem.MailboxOpen = !AuctionHouseSystem.MailboxOpen;
                }
                GUI.backgroundColor = prevColor;
            }
            DrawAdventurePanels(width, height);
            if (!player.Stats.Alive || (player.orbit && player.orbit.Paused))
            {
                Panel(new Rect(width / 2 - 230, height / 2 - 70, 460, 240));
                GUI.Label(new Rect(width / 2 - 205, height / 2 - 50, 410, 40), player.Stats.Alive ? "PAUSED" : "YOU HAVE FALLEN", title);
                GUI.Label(new Rect(width / 2 - 205, height / 2, 410, 50), player.Stats.Alive ? "Press ESC to resume." : "Press R to recover at the outpost.", label);
                if (player.Stats.Alive && GUI.Button(new Rect(width / 2 - 205, height / 2 + 53, 195, 35), "Save Progress", questButton)) player.SaveAdventure();
                if (player.Stats.Alive && GUI.Button(new Rect(width / 2 + 10, height / 2 + 53, 195, 35), "Resume", questButton)) player.orbit.SetPaused(false);
                if (player.Stats.Alive && GUI.Button(new Rect(width / 2 - 205, height / 2 + 100, 410, 35), "Save & Return to Main Menu", questButton)) player.GetComponent<FrontendMenu>().ReturnToMenu();
            }

            if (MinimapRadar.Instance) MinimapRadar.Instance.DrawRadarGUI(width, height);
            if (ZoneBanner.Instance) ZoneBanner.Instance.DrawBannerGUI(width, height);
            if (ChatSystem.Instance) ChatSystem.Instance.DrawChatGUI(width, height);
            if (LootDropSystem.Instance) LootDropSystem.Instance.DrawLootGUI(width, height);
            if (CombatTextManager.Instance) CombatTextManager.Instance.DrawCombatTextGUI();
            if (PartySystem.Instance) PartySystem.Instance.DrawPartyGUI(width, height);
            if (BuffSystem.Instance) BuffSystem.Instance.DrawBuffsGUI(width, height);
            if (CastBarSystem.Instance) CastBarSystem.Instance.DrawCastBarGUI(width, height);
            if (DuelSystem.Instance) DuelSystem.Instance.DrawDuelGUI(width, height);
            if (LootRollSystem.Instance) LootRollSystem.Instance.DrawLootRollGUI(width, height);
            if (MailboxSystem.Instance) MailboxSystem.Instance.DrawMailboxGUI(width, height);
            if (BattlegroundSystem.Instance) BattlegroundSystem.Instance.DrawBattlegroundGUI(width, height);
            if (GuildSystem.Instance) GuildSystem.Instance.DrawGuildGUI(width, height);
            if (CharacterSheetSystem.Instance) CharacterSheetSystem.Instance.DrawCharacterSheetGUI(width, height);
            if (MacroSystem.Instance) MacroSystem.Instance.DrawMacroGUI(width, height);
            if (SettingsSystem.Instance) SettingsSystem.Instance.DrawSettingsGUI(width, height);
            if (DungeonFinderSystem.Instance) DungeonFinderSystem.Instance.DrawDungeonFinderGUI(width, height);
            if (ArchaeologySystem.Instance) ArchaeologySystem.Instance.DrawArchaeologyGUI(width, height);
            if (TransmogSystem.Instance) TransmogSystem.Instance.DrawTransmogGUI(width, height);
            if (FishingSystem.Instance) FishingSystem.Instance.DrawFishingGUI(width, height);
            if (CalendarEventSystem.Instance) CalendarEventSystem.Instance.DrawCalendarGUI(width, height);
            if (PetBattleSystem.Instance) PetBattleSystem.Instance.DrawPetSystemGUI(width, height);
            if (BarberShopSystem.Instance) BarberShopSystem.Instance.DrawBarberShopGUI(width, height);
            if (WorldPvPSystem.Instance) WorldPvPSystem.Instance.DrawWorldPvPGUI(width, height);
            if (EquipmentSetSystem.Instance) EquipmentSetSystem.Instance.DrawEquipmentSetGUI(width, height);
            if (DamageMeterSystem.Instance) DamageMeterSystem.Instance.DrawDamageMeterGUI(width, height);
            if (GlyphSystem.Instance) GlyphSystem.Instance.DrawGlyphGUI(width, height);
            if (GemSocketSystem.Instance) GemSocketSystem.Instance.DrawGemSocketGUI(width, height);

            // Nearby Physical Mailbox interaction prompt
            var nearMailbox = Mailbox.GetNearby(player.transform.position, 3.5f);
            if (nearMailbox && !FrontendMenu.IsOpen && (!MailboxSystem.Instance || !MailboxSystem.Instance.IsOpen))
            {
                float mw = 220f, mh = 32f;
                float mx = (width - mw) * 0.5f, my = height * 0.65f;
                Fill(new Rect(mx, my, mw, mh), new Color(0.06f, 0.08f, 0.12f, 0.85f));
                GUI.Label(new Rect(mx, my + 6f, mw, 20f), $"[E] Open {nearMailbox.mailboxName}", label);
            }
            if (FlightMaster.ActiveFlightMaster != null && FlightMaster.ActiveFlightMaster.TaxiMapOpen)
                FlightMaster.ActiveFlightMaster.DrawTaxiMapGUI(width, height);
            else if (FlightMaster.Instance != null && FlightMaster.Instance.TaxiMapOpen)
                FlightMaster.Instance.DrawTaxiMapGUI(width, height);
            if (CathedralBossEncounter.Instance) CathedralBossEncounter.Instance.DrawBossFrameGUI(width, height);
            if (ApexBossEncounter.Instance) ApexBossEncounter.Instance.DrawBossFrameGUI(width, height);
            if (PetCompanionSystem.Instance) PetCompanionSystem.Instance.DrawPetFrameGUI(width, height);

            GUI.matrix = oldMatrix;
        }
        void DrawQuestWindow(float height)
        {
            var npc = player.ActiveQuestGiver;
            float x = 24, y = 150, w = 450, h = Mathf.Min(530, height - 174);
            Fill(new Rect(x, y, w, h), new Color(.45f, .31f, .13f));
            Fill(new Rect(x + 3, y + 3, w - 6, h - 6), new Color(.86f, .79f, .62f));
            Fill(new Rect(x + 3, y + 3, w - 6, 45), new Color(.12f, .14f, .11f));
            GUI.Label(new Rect(x + 19, y + 8, w - 70, 34), npc.npcName.ToUpperInvariant() + "  ·  QUESTS", title);
            if (GUI.Button(new Rect(x + w - 37, y + 11, 24, 25), "X", questButton)) player.CloseDialogue();
            GUI.Label(new Rect(x + 22, y + 64, w - 44, 38), player.QuestName, questHeading);
            GUI.Label(new Rect(x + 22, y + 110, w - 44, 166), player.QuestDescription, questBody);
            Fill(new Rect(x + 22, y + 286, w - 44, 1), new Color(.55f, .43f, .25f));
            GUI.Label(new Rect(x + 22, y + 301, w - 44, 30), "Quest Objectives", questHeading);
            GUI.Label(new Rect(x + 22, y + 337, w - 44, 52), player.QuestObjective, questBody);
            GUI.Label(new Rect(x + 22, y + 397, w - 44, 30), "Rewards", questHeading);
            GUI.Label(new Rect(x + 22, y + 433, w - 44, 30), player.QuestComplete ? "Rewards: Emberwatch Axe & Sigil" : $"{player.QuestGold} gold / {player.QuestExperience} XP / 1 potion", questBody);
            float bottom = y + h - 47;
            if (!player.QuestAccepted && !player.QuestComplete)
            {
                if (GUI.Button(new Rect(x + 22, bottom, 190, 32), "Accept Quest", questButton)) player.AcceptQuest();
                if (GUI.Button(new Rect(x + 235, bottom, 190, 32), "Not Now", questButton)) player.CloseDialogue();
            }
            else if (!player.QuestComplete && player.QuestReady)
            {
                if (GUI.Button(new Rect(x + 22, bottom, 240, 32), "Complete Quest", questButton)) player.CompleteQuest();
                if (GUI.Button(new Rect(x + 280, bottom, 145, 32), "Close", questButton)) player.CloseDialogue();
            }
            else if (GUI.Button(new Rect(x + 22, bottom, w - 44, 32), player.QuestComplete ? "Farewell" : "I'll return", questButton)) player.CloseDialogue();
        }
        void Fill(Rect rect, Color color) { GUI.color = color; GUI.DrawTexture(rect, white); GUI.color = Color.white; }
        string Cooldown(float end) => end <= Time.time ? "READY" : $"{Mathf.CeilToInt(end - Time.time)}s";
        void Panel(Rect rect) { GUI.color = new Color(.035f, .065f, .065f, .9f); GUI.DrawTexture(rect, white); GUI.color = Color.white; }
        void Bar(Rect rect, float fill, Color color)
        {
            GUI.color = new Color(.1f, .12f, .12f); GUI.DrawTexture(rect, white);
            rect.width *= Mathf.Clamp01(fill); GUI.color = color; GUI.DrawTexture(rect, white); GUI.color = Color.white;
        }
    }
}
