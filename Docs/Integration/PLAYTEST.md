# Combined Riftbound playtest

Use the delivery status/source in INTEGRATION_HANDOFF.md before testing. Do not mix the old Codex/Gemini standalone versions in one session.

## Launch

- Standalone: C:/Users/skama/Documents/wow=rpg-diplomna/Integration/combined-20261116/Builds/RiftboundCombined01/Riftbound.exe.
- Unity project: C:/Users/skama/Documents/wow=rpg-diplomna/Integration/combined-20261116. Add/open this folder through Unity Hub using6000.6.2f1. The original shared project still contains its older source.
- This test build is on integration/combined-20261116. Main and both agents' original working copies remain separate. The unfinished Greenhollow feed-storage survey is not included.

## Offline first — approximately15–25 minutes

1. Open title/character selection at1280x720 or your normal resolution. Check readable Offline/Host Online/Join Online controls and character preview. Prefer a separate test character so progression on your main character is intentional.
2. In Offline mode, test forward run, / slow-walk toggle, backward/strafe/turn, attacks and casts. Check feet/torso and enemy facing. Some imported jog/sprint distortion is still an asset-quality issue; report exact race/class/action.
3. Talk to Abe with E; accept/close dialogue, move again and check nearest interaction prompts. Open/close menus/chat to check movement and mount blocking.
4. Inspect Emberwatch's supply frontage and west departure. Run through and back; summon with Z, ride through both ways and dismount. Keep camera near fences/buildings to expose clipping.
5. Follow the map to Highspire plaza/north gate, then Westbank. Check the matching cobblestone footway and both bridge lanes/approaches.
6. Visit Willow Rest bench/barrel and cache. Check support and access. Open the cache, leave/reload and confirm it does not award loot twice.
7. Approach Reaver camp on foot and mounted. Hostile damage can dismount; test fighting/retreat separately from whether the terrain is traversable. Check canopy/camera visibility.
8. Travel Emberwatch→Greenhollow and return; check arrival beside the marker, services/mount displays, three discovery criteria and Zebra unlock. The new proposed feed-storage objects are not present.
9. Test shop/equipment, potions, death/respawn, sound-volume persistence and optional companion recruitment/removal. Listen for clipping/repetition; automated sound-setting checks are not a listening test.

## Online — same complete build on both PCs

Copy the entire RiftboundCombined01 folder, including Riftbound_Data and DLLs. Keep internet access available. Distinct networks, such as home broadband and a phone hotspot, establish a stronger internet acceptance test than two local instances.

1. PC1 selects Host Online and creates a private session. Share the displayed join code privately.
2. PC2 selects Join Online and enters that code. Up to4 players are supported; a further participant should receive session-full feedback.
3. Both players move, fight, heal, use potions, loot, accept/complete quests, buy/equip items, mount/dismount and interact near the repaired routes.
4. Test F6 ping, F7 ready check and E ally revive. Check late join during an event and Leave→Rejoin.
5. Host exit ends the session; clients should receive SESSION ENDED and return cleanly. Host migration is not implemented.
6. Online progress is session-only; it should not overwrite offline adventure/faction saves. Do not expect durable online characters in this build.

## Defect report

Record build/source, Offline/Host/Client, race/class, location, exact action, expected/actual result and whether it repeats. For crashes/compiler errors provide the first relevant error and stack, not the entire log. Screenshots/video help for feet/facing/terrain/UI. Do not publish live join codes or authentication tokens.

Headless Relay passes, script-driven movement and geometry probes are listed separately in the handoff; this guide is for acceptance that still needs a player.
