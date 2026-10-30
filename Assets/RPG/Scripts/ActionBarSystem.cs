using System;
using UnityEngine;

namespace Riftbound
{
    public class ActionBarSystem : MonoBehaviour
    {
        public static ActionBarSystem Instance { get; private set; }

        GUIStyle slotStyle, keybindStyle, costStyle, cdStyle, labelStyle, tipTitleStyle, tipBodyStyle;
        Texture2D bgTex, borderTex, healthBgTex, healthFillTex, resourceFillTex, cdOverlayTex;

        void Awake()
        {
            if (Instance == null) Instance = this;
        }

        void InitStyles()
        {
            if (slotStyle != null) return;

            bgTex = MakeTex(1, 1, new Color(0.08f, 0.09f, 0.12f, 0.94f));
            borderTex = MakeTex(1, 1, new Color(0.25f, 0.28f, 0.35f, 1f));
            healthBgTex = MakeTex(1, 1, new Color(0.12f, 0.04f, 0.04f, 0.95f));
            healthFillTex = MakeTex(1, 1, new Color(0.16f, 0.72f, 0.22f, 1f));
            resourceFillTex = MakeTex(1, 1, new Color(0.85f, 0.22f, 0.18f, 1f));
            cdOverlayTex = MakeTex(1, 1, new Color(0.0f, 0.0f, 0.0f, 0.72f));

            slotStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            slotStyle.normal.textColor = Color.white;

            keybindStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
            keybindStyle.normal.textColor = new Color(1.0f, 0.88f, 0.32f);

            costStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight
            };
            costStyle.normal.textColor = new Color(0.75f, 0.85f, 1.0f);

            cdStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            cdStyle.normal.textColor = new Color(1.0f, 0.95f, 0.5f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = Color.white;

            tipTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
            tipTitleStyle.normal.textColor = new Color(1.0f, 0.85f, 0.25f);

            tipBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            tipBodyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        }

        public void DrawActionBar(PlayerController player, float screenW, float screenH)
        {
            if (!player || !player.Stats || FrontendMenu.IsOpen) return;
            InitStyles();

            int slotCount = 9;
            float slotSize = 44f;
            float spacing = 4f;
            float totalBarW = slotCount * slotSize + (slotCount - 1) * spacing;
            float barX = screenW / 2f - totalBarW / 2f;
            float barY = screenH - 50f;

            // Sleek, Low-Profile Experience Bar (3.5px) resting on top of Action Slots
            float xpRatio = player.Level == 5 ? 1f : (float)(player.Experience - player.LevelFloor) / Mathf.Max(1, player.NextLevelExperience - player.LevelFloor);
            GUI.DrawTexture(new Rect(barX, barY - 5f, totalBarW, 3.5f), borderTex);
            var prevGuiCol = GUI.color;
            GUI.color = new Color(0.55f, 0.25f, 0.90f);
            GUI.DrawTexture(new Rect(barX, barY - 5f, totalBarW * xpRatio, 3.5f), healthFillTex);
            GUI.color = prevGuiCol;

            // --- Action Slots (1 to 8) ---
            var ab1 = ClassSystem.GetAbilityInfo(player.CharacterClass, 1);
            var ab2 = ClassSystem.GetAbilityInfo(player.CharacterClass, 2);
            var ab3 = ClassSystem.GetAbilityInfo(player.CharacterClass, 3);

            string hoverTitle = null;
            string hoverSub = null;
            string hoverDesc = null;

            // Slot 1: Primary Strike
            string s1Icon = player.CharacterClass == "Mage" ? "🔥" : player.CharacterClass == "Rogue" ? "🗡" : player.CharacterClass == "Paladin" ? "✝" : "⚔";
            string s1Cost = player.CharacterClass == "Mage" ? "15 MP" : player.CharacterClass == "Rogue" ? "40 NR" : "";
            if (DrawSlot(new Rect(barX, barY, slotSize, slotSize), "1", s1Icon, s1Cost, 0f, () => player.CastAbility1Public()))
            {
                hoverTitle = ab1.name;
                hoverSub = $"{s1Cost}  •  Melee / 25 yd  •  Instant";
                hoverDesc = ab1.desc + "\nBuilds primary class resource when attacking.";
            }

            // Slot 2: Cleave / Burst / Nova
            float cd2 = Mathf.Max(0f, player.CleaveReady - Time.time);
            string s2Icon = player.CharacterClass == "Mage" ? "❄" : player.CharacterClass == "Rogue" ? "🩸" : player.CharacterClass == "Paladin" ? "☼" : "🌪";
            string s2Cost = player.CharacterClass == "Mage" ? "25 MP" : player.CharacterClass == "Rogue" ? "35 NR" : player.CharacterClass == "Paladin" ? "1 HP" : "25 RG";
            if (DrawSlot(new Rect(barX + (slotSize + spacing) * 1, barY, slotSize, slotSize), "2", s2Icon, s2Cost, cd2, () => player.CastAbility2Public()))
            {
                hoverTitle = ab2.name;
                hoverSub = $"{s2Cost}  •  Instant  •  5.0s Cooldown";
                hoverDesc = ab2.desc + "\nDevastating multi-target burst ability.";
            }

            // Slot 3: Utility / Healing / Shield
            float cd3 = Mathf.Max(0f, player.HealReady - Time.time);
            string s3Icon = player.CharacterClass == "Mage" ? "✦" : player.CharacterClass == "Rogue" ? "⚡" : player.CharacterClass == "Paladin" ? "✚" : "♥";
            string s3Cost = player.CharacterClass == "Mage" ? "20 MP" : player.CharacterClass == "Rogue" ? "20 NR" : player.CharacterClass == "Paladin" ? "3 HP" : "";
            if (DrawSlot(new Rect(barX + (slotSize + spacing) * 2, barY, slotSize, slotSize), "3", s3Icon, s3Cost, cd3, () => player.CastAbility3Public()))
            {
                hoverTitle = ab3.name;
                hoverSub = $"{s3Cost}  •  Instant  •  10.0s Cooldown";
                hoverDesc = ab3.desc + "\nDefensive recovery or mobility ability.";
            }

            // Slot 4: Health Potion
            if (DrawSlot(new Rect(barX + (slotSize + spacing) * 3, barY, slotSize, slotSize), "4", "🧪", $"x{player.Potions}", 0f, () => player.UsePotionPublic()))
            {
                hoverTitle = "Health Potion";
                hoverSub = "Consumable  •  Instant";
                hoverDesc = $"Restores 65 Health points instantly.\nYou currently hold {player.Potions} potion(s). Purchase more from town vendors.";
            }

            // Slot 5: Spell Interrupt [F]
            float cdInterrupt = player.InterruptCooldownRemaining;
            string intName = player.InterruptSpellName;
            string intIcon = player.InterruptIcon;
            if (DrawSlot(new Rect(barX + (slotSize + spacing) * 4, barY, slotSize, slotSize), "F", intIcon, "", cdInterrupt, () => player.CastInterruptPublic()))
            {
                hoverTitle = $"{intName} (Interrupt)";
                hoverSub = $"Instant  •  12.0s Cooldown  •  {(player.CharacterClass == "Mage" ? "25m Range" : "Melee Range")}";
                hoverDesc = $"Interrupts the enemy's spellcast, locking out that school of magic for 4 seconds.\nPress [F] or click when target begins casting.";
            }

            // Slot 6: Guard / Shield Block [Q]
            float cdGuard = Mathf.Max(0f, player.GuardReady - Time.time);
            string guardLabel = player.Blocking ? "ACTIVE" : "";
            if (DrawSlot(new Rect(barX + (slotSize + spacing) * 5, barY, slotSize, slotSize), "Q", "🛡", guardLabel, cdGuard, () => player.ToggleGuardPublic()))
            {
                hoverTitle = "Guard (Shield Block)";
                hoverSub = "Stance  •  Instant";
                hoverDesc = "Raise your guard to absorb 50% of incoming damage for up to 3 seconds. Recharges every 6 seconds.";
            }

            // Slot 7: Mount [Z]
            string mountIcon = (player.SelectedMount && player.SelectedMount.isFlying) ? "🦅" : "🐎";
            string mountStatus = player.Mounted ? "DISMOUNT" : "SUMMON";
            if (DrawSlot(new Rect(barX + (slotSize + spacing) * 6, barY, slotSize, slotSize), "Z", mountIcon, mountStatus, 0f, () => player.ToggleMountPublic()))
            {
                hoverTitle = player.Mounted ? "Dismount" : (player.SelectedMount ? player.SelectedMount.displayName : "Celestial Winged Steed");
                hoverSub = (player.SelectedMount && player.SelectedMount.isFlying) ? "Flying Mount  •  1.5s Cast" : "Ground Mount  •  1.5s Cast";
                hoverDesc = "Summons your noble steed to increase travel speed. Flying mounts allow soaring high above the 6 zones.";
            }

            // Slot 8: Hearthstone [H]
            float cdHearth = Mathf.Max(0f, player.HearthReady - Time.time);
            if (DrawSlot(new Rect(barX + (slotSize + spacing) * 7, barY, slotSize, slotSize), "H", "🌀", "", cdHearth, () => player.TriggerHearthstonePublic()))
            {
                hoverTitle = "Hearthstone";
                hoverSub = "5.0s Cast  •  60s Cooldown";
                hoverDesc = "Communes with the ley leylines to teleport you safely back to Emberwatch Outpost. Cancelled by taking damage or moving.";
            }

            // Slot 9: World Atlas Map [M]
            if (DrawSlot(new Rect(barX + (slotSize + spacing) * 8, barY, slotSize, slotSize), "M", "🗺", "MAP", 0f, () => player.ToggleMapPublic()))
            {
                hoverTitle = "World Atlas of Azeboth";
                hoverSub = "Continental Map  •  [M]";
                hoverDesc = "Opens the full continental atlas detailing Highspire Keep, Amber Vale, Duskwood Cathedral, Redridge Canyons, Gor'Kresh Stronghold, and Mount Wyrmcrest.";
            }

            // --- 3. Mouseover Tooltip ---
            if (!string.IsNullOrEmpty(hoverTitle))
            {
                DrawTooltip(barX, barY - 105f, 290f, 96f, hoverTitle, hoverSub, hoverDesc);
            }
        }

        bool DrawSlot(Rect rect, string keybind, string icon, string cost, float cd, Action onClick)
        {
            // Slot background & border
            GUI.DrawTexture(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), borderTex);
            GUI.DrawTexture(rect, bgTex);

            // Icon symbol
            GUI.Label(new Rect(rect.x, rect.y + 4, rect.width, rect.height - 8), icon, slotStyle);

            // Cooldown overlay
            if (cd > 0.05f)
            {
                GUI.DrawTexture(rect, cdOverlayTex);
                GUI.Label(rect, $"{cd:F1}s", cdStyle);
            }

            // Keybind badge (Top-Left)
            GUI.Label(new Rect(rect.x + 3, rect.y + 2, 28, 16), keybind, keybindStyle);

            // Cost or sub-label (Bottom-Right)
            if (!string.IsNullOrEmpty(cost))
            {
                GUI.Label(new Rect(rect.x, rect.yMax - 15, rect.width - 3, 14), cost, costStyle);
            }

            // Click handling
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                onClick?.Invoke();
                Event.current.Use();
            }

            return rect.Contains(Event.current.mousePosition);
        }

        void DrawStatusBar(float x, float y, float width, float height, float current, float max, Color fillColor, string title, string text)
        {
            GUI.DrawTexture(new Rect(x - 1, y - 1, width + 2, height + 2), borderTex);
            GUI.DrawTexture(new Rect(x, y, width, height), healthBgTex);

            float pct = Mathf.Clamp01(max > 0 ? current / max : 0f);
            if (pct > 0.001f)
            {
                var oldBg = GUI.color;
                GUI.color = fillColor;
                GUI.DrawTexture(new Rect(x, y, width * pct, height), healthFillTex);
                GUI.color = oldBg;
            }

            GUI.Label(new Rect(x, y - 1, width, height), text, labelStyle);
        }

        void DrawTooltip(float x, float y, float width, float height, string title, string sub, string desc)
        {
            // Tooltip dark panel with golden trim
            GUI.DrawTexture(new Rect(x - 2, y - 2, width + 4, height + 4), borderTex);
            GUI.DrawTexture(new Rect(x, y, width, height), bgTex);

            GUI.Label(new Rect(x + 10, y + 8, width - 20, 20), title, tipTitleStyle);
            if (!string.IsNullOrEmpty(sub))
            {
                GUI.Label(new Rect(x + 10, y + 26, width - 20, 18), sub, keybindStyle);
            }
            GUI.Label(new Rect(x + 10, y + 46, width - 20, 44), desc, tipBodyStyle);
        }

        Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var t = new Texture2D(w, h);
            t.SetPixels(pix);
            t.Apply();
            return t;
        }
    }
}
