using System;
using UnityEngine;

namespace Riftbound
{
    public partial class PrototypeHUD
    {
        int gossipMenuState = 0; // 0: Main, 1: Lore, 2: Threats, 3: Directions
        string gossipFeedback = "";
        Vector2 gossipScroll = Vector2.zero;

        void DrawTownNPCGossip(float width, float height)
        {
            var npc = player.ActiveTownNPC;
            if (!npc) return;

            float windowW = 460f, windowH = 500f;
            float x = 24f, y = 140f;

            // Outer dark gothic panel with gold border
            Fill(new Rect(x - 4f, y - 4f, windowW + 8f, windowH + 8f), new Color(0.85f, 0.70f, 0.28f, 0.95f));
            Fill(new Rect(x, y, windowW, windowH), new Color(0.06f, 0.08f, 0.11f, 0.98f));

            // Header Title Bar
            Fill(new Rect(x, y, windowW, 40f), new Color(0.12f, 0.15f, 0.22f, 0.98f));
            Fill(new Rect(x, y + 39f, windowW, 1.5f), new Color(0.85f, 0.70f, 0.28f, 0.95f));

            // Determine Crest Icon
            string crest = GetNPCCrest(npc);
            string npcTitle = $"{crest}  {npc.npcName.ToUpperInvariant()}";
            var headerStyle = new GUIStyle(label) { fontStyle = FontStyle.Bold, fontSize = 14 };
            headerStyle.normal.textColor = new Color(0.95f, 0.85f, 0.40f);
            GUI.Label(new Rect(x + 14f, y + 5f, windowW - 60f, 20f), npcTitle, headerStyle);

            var subStyle = new GUIStyle(small) { fontStyle = FontStyle.Italic };
            subStyle.normal.textColor = new Color(0.65f, 0.80f, 0.95f);
            GUI.Label(new Rect(x + 14f, y + 22f, windowW - 60f, 16f), $"<{npc.roleTag}>", subStyle);

            // Close button [✕]
            if (GUI.Button(new Rect(x + windowW - 32f, y + 8f, 24f, 24f), "✕", microBtnStyle))
            {
                player.CloseDialogue();
                gossipMenuState = 0;
                gossipFeedback = "";
                AudioEffects.PlayClick();
            }

            // Inner Aged Parchment Page
            float parchX = x + 12f;
            float parchY = y + 46f;
            float parchW = windowW - 24f;
            float parchH = windowH - 58f;

            Fill(new Rect(parchX - 2f, parchY - 2f, parchW + 4f, parchH + 4f), new Color(0.35f, 0.26f, 0.12f, 0.95f));
            Fill(new Rect(parchX, parchY, parchW, parchH), new Color(0.88f, 0.81f, 0.65f, 0.97f));

            // Parchment Typography Styles
            var quoteStyle = new GUIStyle(label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                wordWrap = true
            };
            quoteStyle.normal.textColor = new Color(0.25f, 0.16f, 0.08f);

            var bodyStyle = new GUIStyle(label)
            {
                fontSize = 12,
                wordWrap = true
            };
            bodyStyle.normal.textColor = new Color(0.12f, 0.08f, 0.04f);

            var optionStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 4, 4)
            };
            optionStyle.normal.textColor = new Color(0.10f, 0.06f, 0.02f);

            var feedbackStyle = new GUIStyle(small) { fontStyle = FontStyle.Bold };
            feedbackStyle.normal.textColor = new Color(0.05f, 0.45f, 0.15f);

            // Content Area
            GUILayout.BeginArea(new Rect(parchX + 14f, parchY + 12f, parchW - 28f, parchH - 24f));
            gossipScroll = GUILayout.BeginScrollView(gossipScroll, GUILayout.Width(parchW - 28f), GUILayout.Height(parchH - 24f));

            if (gossipMenuState == 0)
            {
                // Main Greeting & Dialogue Options
                GUILayout.Label($"\"{npc.greeting}\"", quoteStyle);
                GUILayout.Space(10f);

                // Divider line
                DrawParchmentDivider();
                GUILayout.Space(8f);

                if (!string.IsNullOrEmpty(gossipFeedback))
                {
                    GUILayout.Label(gossipFeedback, feedbackStyle);
                    GUILayout.Space(8f);
                }

                GUILayout.Label("Choose a topic of inquiry:", bodyStyle);
                GUILayout.Space(6f);

                if (GUILayout.Button("💬 \"Tell me about this settlement.\"", optionStyle, GUILayout.Height(28f)))
                {
                    gossipMenuState = 1;
                    AudioEffects.PlayClick();
                }

                if (GUILayout.Button("📜 \"What threats lurk in this realm?\"", optionStyle, GUILayout.Height(28f)))
                {
                    gossipMenuState = 2;
                    AudioEffects.PlayClick();
                }

                if (GUILayout.Button("📍 \"Where can I find town services?\"", optionStyle, GUILayout.Height(28f)))
                {
                    gossipMenuState = 3;
                    AudioEffects.PlayClick();
                }

                // Contextual Option: Flight Master
                if (npc.roleTag.Contains("Flight Master") || npc.roleTag.Contains("Gryphon") || npc.roleTag.Contains("Wind Rider"))
                {
                    if (GUILayout.Button("🦅 \"I require flight transportation.\"", optionStyle, GUILayout.Height(28f)))
                    {
                        var fm = FlightMaster.GetNearby(player.transform.position, 6f);
                        if (fm) FlightMaster.OpenTaxiWindow(fm);
                        player.CloseDialogue();
                        AudioEffects.PlayClick();
                    }
                }

                // Contextual Option: Innkeeper Hearthstone Binding
                if (npc.roleTag.Contains("Innkeeper") || npc.npcName.Contains("Innkeeper"))
                {
                    if (GUILayout.Button("🍺 \"Make this inn my home. (Bind Hearthstone)\"", optionStyle, GUILayout.Height(28f)))
                    {
                        if (RestedXPSystem.Instance)
                        {
                            string curZone = ZoneManager.GetZoneName(player.transform.position);
                            RestedXPSystem.Instance.BindHearthstone(curZone + " Tavern", player.transform.position);
                        }
                        gossipFeedback = $"Your Hearthstone has been set to {npc.npcName}'s Inn!";
                        AudioEffects.PlayClick();
                    }

                    if (GUILayout.Button("✂ \"I'd like to get a haircut and makeover. (Barber Shop)\"", optionStyle, GUILayout.Height(28f)))
                    {
                        if (BarberShopSystem.Instance) BarberShopSystem.Instance.OpenBarber();
                        player.CloseDialogue();
                        AudioEffects.PlayClick();
                    }
                }

                // Contextual Option: Trainer
                if (npc.roleTag.Contains("Trainer"))
                {
                    if (GUILayout.Button("⚔ \"I wish to review my Class Talents [N].\"", optionStyle, GUILayout.Height(28f)))
                    {
                        player.TalentsOpen = true;
                        player.CloseDialogue();
                        AudioEffects.PlayClick();
                    }
                }

                GUILayout.Space(12f);
                DrawParchmentDivider();
                GUILayout.Space(6f);

                if (GUILayout.Button("👋 \"Farewell.\"", optionStyle, GUILayout.Height(26f)))
                {
                    player.CloseDialogue();
                    AudioEffects.PlayClick();
                }
            }
            else if (gossipMenuState == 1)
            {
                // Lore Sub-view
                GUILayout.Label($"<b>THE REALM & SETTLEMENT</b>", bodyStyle);
                GUILayout.Space(6f);
                string lore = GetNPCLore(npc);
                GUILayout.Label(lore, bodyStyle);

                GUILayout.Space(14f);
                DrawParchmentDivider();
                GUILayout.Space(8f);

                if (GUILayout.Button("◄ Back to Inquiries", optionStyle, GUILayout.Height(28f)))
                {
                    gossipMenuState = 0;
                    AudioEffects.PlayClick();
                }
            }
            else if (gossipMenuState == 2)
            {
                // Threats Sub-view
                GUILayout.Label($"<b>REGIONAL THREATS & BOUNTIES</b>", bodyStyle);
                GUILayout.Space(6f);
                string threats = GetNPCThreats(npc);
                GUILayout.Label(threats, bodyStyle);

                GUILayout.Space(14f);
                DrawParchmentDivider();
                GUILayout.Space(8f);

                if (GUILayout.Button("◄ Back to Inquiries", optionStyle, GUILayout.Height(28f)))
                {
                    gossipMenuState = 0;
                    AudioEffects.PlayClick();
                }
            }
            else if (gossipMenuState == 3)
            {
                // Directions & Waypoints Sub-view
                GUILayout.Label("<b>TOWN SERVICES & WAYPOINTS</b>", bodyStyle);
                GUILayout.Label("Select a service to drop a waypoint beacon on your Minimap Radar:", bodyStyle);
                GUILayout.Space(8f);

                if (GUILayout.Button("✈ Flight Master Taxi Post", optionStyle, GUILayout.Height(28f)))
                {
                    var fm = FlightMaster.GetNearby(player.transform.position, 120f);
                    if (fm)
                    {
                        MinimapRadar.SetWaypoint(fm.transform.position, $"Flight Master ({fm.nodeName})");
                        gossipFeedback = $"✔ Waypoint pinned: Flight Master ({fm.nodeName})";
                    }
                    else
                    {
                        MinimapRadar.SetWaypoint(new Vector3(2f, 0f, 10f), "Emberwatch Flight Master");
                        gossipFeedback = "✔ Waypoint pinned: Emberwatch Flight Master";
                    }
                    gossipMenuState = 0;
                }

                if (GUILayout.Button("🏛 Auction House & Mailbox", optionStyle, GUILayout.Height(28f)))
                {
                    var mb = Mailbox.GetNearby(player.transform.position, 120f);
                    Vector3 wp = mb ? mb.transform.position : new Vector3(8.5f, 0f, 6.5f);
                    MinimapRadar.SetWaypoint(wp, "Auction House & Mailbox");
                    gossipFeedback = "✔ Waypoint pinned: Auctioneer & Mailbox";
                    gossipMenuState = 0;
                }

                if (GUILayout.Button("⚔ The Scarlet Crypts Dungeon Portal", optionStyle, GUILayout.Height(28f)))
                {
                    MinimapRadar.SetWaypoint(new Vector3(-55f, 0f, 215f), "The Scarlet Crypts Portal");
                    gossipFeedback = "✔ Waypoint pinned: Scarlet Crypts Portal (-55, 215)";
                    gossipMenuState = 0;
                }

                if (GUILayout.Button("🌋 Mount Wyrmcrest Summit Dais", optionStyle, GUILayout.Height(28f)))
                {
                    MinimapRadar.SetWaypoint(new Vector3(0f, 48f, 225f), "Wyrmcrest Summit (Apex Dragon)");
                    gossipFeedback = "✔ Waypoint pinned: Mount Wyrmcrest (0, 48, 225)";
                    gossipMenuState = 0;
                }

                if (GUILayout.Button("🍺 Innkeeper & Traveling Merchant", optionStyle, GUILayout.Height(28f)))
                {
                    var merchant = player.NearbyMerchant();
                    Vector3 mPos = merchant ? merchant.transform.position : new Vector3(-8f, 0f, 10f);
                    MinimapRadar.SetWaypoint(mPos, "Town Inn & Provisions");
                    gossipFeedback = "✔ Waypoint pinned: Town Inn & Merchant";
                    gossipMenuState = 0;
                }

                GUILayout.Space(12f);
                DrawParchmentDivider();
                GUILayout.Space(8f);

                if (GUILayout.Button("◄ Back to Inquiries", optionStyle, GUILayout.Height(28f)))
                {
                    gossipMenuState = 0;
                    AudioEffects.PlayClick();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawParchmentDivider()
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(2f));
            Fill(rect, new Color(0.55f, 0.45f, 0.28f, 0.7f));
        }

        string GetNPCCrest(TownNPC npc)
        {
            string name = npc.npcName;
            string role = npc.roleTag;

            if (role.Contains("Flight Master") || role.Contains("Gryphon") || role.Contains("Wind Rider")) return "🦅";
            if (role.Contains("Mage") || role.Contains("Arcane") || role.Contains("Scholar") || role.Contains("Priest")) return "🔮";
            if (role.Contains("Smith") || role.Contains("Armor") || role.Contains("Engineer")) return "⚒";
            if (role.Contains("Innkeeper") || role.Contains("Tavern")) return "🍺";
            if (role.Contains("Auctioneer") || role.Contains("Cartel")) return "💰";
            if (name.Contains("Ronald") || name.Contains("Halford") || name.Contains("Justin") || name.Contains("Thalorien") || name.Contains("Ironbrow")) return "🦁";
            if (name.Contains("Mor'gok") || name.Contains("Gorgash") || name.Contains("Grimjaw") || name.Contains("Morzan") || name.Contains("Griznak")) return "🐺";
            return "🛡";
        }

        string GetNPCLore(TownNPC npc)
        {
            string name = npc.npcName;
            string role = npc.roleTag;

            if (name.Contains("Ronald") || role.Contains("Highspire"))
                return "Highspire Keep stands tall upon the northern crags of Aethelgard. For three hundred years, the Royal Knights and the Archmagi of Kirin Tor have guarded this bastion. The Lion crest of the Alliance flies proudly over our ramparts, reminding the realm that justice and honor shall endure.";

            if (name.Contains("Mor'gok") || role.Contains("Gor'Kresh") || role.Contains("Horde"))
                return "Gor'Kresh was forged from blood and dark timber in the heat of the red canyons. The Ashen Pact welcomes those whom the world discarded: the proud crag orcs, the resurrected Forsaken, and the cunning goblins. Strength and honor guide our blades!";

            if (name.Contains("Karen") || name.Contains("Aldous") || name.Contains("Kaelen"))
                return "The arcane currents running through this continent originate deep beneath the mountains. Ancient ley lines intersect at Mount Wyrmcrest and branch outward toward the Scarlet Crypts. Those who master the weave of Mana can turn the tides of any war.";

            if (name.Contains("Kelsey") || role.Contains("Shaman"))
                return "The elements here are restless. Fire spirits surge in the Redridge chasms, while stone and air whisper warnings of an ancient predator waking at the highest summit. Keep your spirit centered, young hero.";

            if (name.Contains("Ironbrow") || name.Contains("Torvald") || role.Contains("Smith"))
                return "A warrior is only as stout as their tempered steel! Deep beneath the peaks lie veins of Copper, Dark Iron, and Obsidian Core. Heat the forge, strike while the iron is red, and your armor will turn aside any raider's blade.";

            if (name.Contains("Thalorien") || role.Contains("Silverglen"))
                return "Silverglen Sanctum preserves the harmony of the eternal groves. Even in exile across these rugged lands, the descendants of the High Elves maintain the ancient vigilance over sacred fonts and runestones.";

            if (name.Contains("Morzan") || name.Contains("Alyssa"))
                return "Death is not an end — it is a crucible. The Forsaken hold no delusions about the cruelty of this realm. We walk the razor's edge between the Light that burned us and the Shadow that gave us second breath.";

            if (name.Contains("Griznak") || name.Contains("Fizzlebang"))
                return "Bilgewater Cartel policy is simple: if it doesn't turn a profit, replace it! If it explodes, sell tickets! Our turbo-copters and clockwork sentries are cutting-edge goblin engineering — ninety percent tested, ten percent thrilling mystery!";

            return "Emberwatch Outpost was founded by veterans of the border campaigns. We shelter farmers, prospectors, and traveling heroes who journey between the Alliance plains of Amber Vale and the contested frontier.";
        }

        string GetNPCThreats(TownNPC npc)
        {
            string name = npc.npcName;

            if (name.Contains("Ronald") || name.Contains("Lucian") || name.Contains("Brian"))
                return "The greatest immediate threat lies within <b>The Scarlet Crypts</b> (-55, 215). High Inquisitor Malakar has succumbed to fanatic corruption, turning his zealous followers into undead abominations. Only heroes clad in tempered gear should venture beyond the crimson gateway.";

            if (name.Contains("Mor'gok") || name.Contains("Gorgash") || name.Contains("Kelsey"))
                return "Beyond the crags of Camp Bloodrock (55, 215) looms <b>Mount Wyrmcrest</b> (0, 48, 225). At the snowy summit roosts <b>Vermithrax the Ember Sovereign</b>, an ancient dragon of terrifying fury. He incinerates any traveler who approaches without wings.";

            return "Watch your step on the northern highway. The Ashwood raiders have fortified a ruined watchtower with bone raiders, boneguards, and hexers. Further east in the Redridge canyons, crag orcs ambush any patrol caught separated from their mount.";
        }
    }
}
