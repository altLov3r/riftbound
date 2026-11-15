# Presentation update — 2026-11-10

## Changes
- New opening title page:Play,Settings,Controls,Quit and the selected character preview. Play/Enter leads into the existing character selection. This is a local single-player entry screen; no account/password backend is implied.
- Refined title/selection/create/settings styling:charcoal surfaces, restrained gold accents, quieter typography and flat buttons. Existing roster, delete confirmation, settings and Enter World flow retained.
- HUD keeps health/resource,target,action bar,context prompt,minimap and MENU. Quest tracking shows active work in compact rows. Utility hints are contextual; duplicate XP decoration and permanent zone-distance/campaign-complete blocks removed.
- Minimap frame is smaller; its coordinates and controls appear on hover. Existing visibility preferences remain respected.
- Original painted4x4 icon atlas replaces emoji ability art for12 class abilities plus potion,interrupt,guard and return-home. Keybind badges,cooldowns and tooltips remain. Ride/Map use quiet text utility slots.
- Legacy oversized uppercase world captions are hidden at runtime. Zone notifications are shorter/smaller; achievement trim and mail notices are restrained. Startup text no longer includes the garbled faction separator.

## Validation
145 passing GPU-backed Play Mode assertions,0 failures, including startup title,Play-to-character selection,texture loading and all16 icon mappings. Existing movement,companion,terrain,quest,loot,save,cartography and River Reaver regressions pass. Actual1280x720 title,character-selection and HUD screenshots captured and inspected.

The initial captures used an inherited narrow editor pane; the test helper now sets a fixed desktop GameView. Screenshot capture occurs before simulated quest/combat tests populate temporary feedback. Existing character models/animation poses are unchanged; model quality remains a separate task.

## Test
1. Launch the updated executable:check title,Play,Settings/Controls navigation and character selection.
2. Enter World. Hover ability icons for tooltips; test keys1–4,F,Q,H,Z and M.
3. Open MENU to reach secondary panels. Accept a quest to reveal the compact tracker. Hover the minimap to reveal its controls.
4. Check Mage/Rogue/Paladin characters for the corresponding icon set. Test cooldown overlays,window access and save/return to title.

## Files
FrontendMenu.cs;PrototypeHUD.cs;ActionBarSystem.cs;MinimapRadar.cs;AdventurePanels.cs;ZoneBanner.cs;AchievementSystem.cs;PlayerProgress.cs. New art:Assets/RPG/Resources/UI/AbilityIcons.png. Validation/import helpers:UIArtSetup.cs and QuestRouteSmokeTest.cs.

## Build
Windows standalone succeeded: Errors: 0, Warnings: 672; Time: 00:00:26.3644867. Run `Builds/PresentationUpdate/Riftbound.exe`. Earlier builds retained. Executable hash verified; source C# matches the validated build copy. Screenshots/results are under `.code-check/PresentationValidation`. Runtime tests preceded a final tooltip-text correction; final player compilation passed.
