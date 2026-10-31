# Execution checklist

## First playable milestone

- [x] Import supplied village, nature, Brute and animation files.
- [x] Configure humanoid animation clips and URP materials.
- [x] Camera-relative movement, sprint, jump, camera orbit and zoom.
- [x] Strike, area attack and healing with cooldowns.
- [x] Enemy chase, melee attacks, health and defeat.
- [x] Small exterior village and forest road.
- [x] Accept patrol, defeat three raiders, return for completion.
- [x] Health/target/cooldown/mission HUD, pause and recovery.
- [x] Input-driven Play Mode smoke test.
- [x] Build and launch standalone Windows demo on this machine.

## Evaluate together before expanding

- [ ] Choose final project title, two factions, four races and five classes.
- [ ] Decide whether combat should use nearest targeting, mouse targeting or both.
- [ ] Evaluate movement speed, camera distance, strike timing and enemy difficulty.
- [ ] Obtain original nature/Brute textures or choose a consistent flat-colour style.
- [ ] Choose distinct race models compatible with the animation workflow.
- [x] Add a proper death animation, hit effects and sound.

## Week 2: reusable RPG data and progress

- [ ] Move class/race/faction definitions into ScriptableObjects.
- [ ] Replace prototype ability constants with reusable ability definitions.
- [ ] Implement remaining four classes with two abilities each.
- [ ] Character creation and race restrictions by faction.
- [x] Experience and levels 1–5.
- [x] Basic equipment and inventory.
- [x] Save/load progress and safe outpost spawn.

## Week 3: world and content

- [ ] Separate starting zone for each faction.
- [ ] Shared adventure zone and scene transitions.
- [x] Replace beacon mission with an NPC and dialogue.
- [ ] Reusable quest definitions and objective tracking.
- [ ] Add collection/interaction quests and faction introductions.
- [ ] Additional enemy types and final boss with two telegraphed attacks.

## Week 4: competition build

- [ ] Freeze new systems and fix blocking issues.
- [ ] Profile performance on the presentation computer.
- [ ] Main menu, settings, sound and credits.
- [ ] Full playthrough for every class and save/load checks.
- [ ] Complete asset credits and source/license records.
- [ ] Diploma diagrams, implementation notes and test evidence.
- [ ] Final executable, backup, recorded demo and presentation rehearsal.

Subclasses and multiplayer are outside the competition milestone.

## Current demo additions (2026-10-29)

- [x] Western Crypt, three Runestones and optional Warden encounter.
- [x] Eleven gear items, four caches and saved side progress.
- [x] Guard, interruptible Hearthstone and Quest Journal.
- [x] Windows compilation and isolated data/logic checks.
- [ ] User playthrough: movement, Guard, crypt difficulty, return channel and journal.

## Greenhollow expansion

- [x] Extend ground, borders and connected north road to a 160x300m map.
- [x] Bridge, brook, riding post, provisioner, grove, scouts, wildlife and two new caches.
- [x] Native horse/zebra mounts, seated rider, summon/cancel/dismount and camera framing.
- [x] Three scouting landmarks, zebra unlock and saved mount selection.
- [x] Download free animal/prop models and retain author/license records.
- [ ] User riding playthrough and encounter balancing.

## Terrain and presentation

- [x] Correct Skeleton facing and attack target tracking.
- [x] Native heightfield terrain, elevated platforms and conformed paths.
- [x] Project existing scenery/gameplay objects onto terrain elevations.
- [x] Grass/dirt/rock layers, contact shading, soft shadows and color grading.
- [x] Geometry, grounding, route-grade and facing checks.
- [ ] User combat/riding acceptance test and frame-rate profiling.

## Quest-route stability and navigation — 2026-10-31

- [x] Restrict main-quest targets/credit to road raiders and the Ashwood Captain.
- [x] Keep optional bosses independent of Abe's quest gate.
- [x] Correct completion messages and completed quest count.
- [x] Collect enemy loot once, with a single Human gold bonus and valid item IDs.
- [x] Save unclaimed/partially claimed loot and retain legacy earned gear.
- [x] Add live main-quest navigation to the journal/minimap.
- [x] Add 57 passing headless startup, quest, loot and save/navigation assertions.
- [x] Build QuestUpdate for Windows: 0 errors, 653 existing shader/plugin warnings.
- [ ] Manual quest/combat/mount acceptance test.
- [ ] Safe malformed-save/backup recovery and cross-character talent reset.
