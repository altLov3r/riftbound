# River Reaver update — 2026-11-10

## Content
- Optional River Reaver camp on the western riverbank at x=-283,z=78:Captain Selvek (260HP), two90HP guards, a locked supply cache and imported Quaternius camp props.
- A separate terrain variant adds a level clearing at15.46m and a painted trail from the Westbank road. Maximum measured grade across three walking lines is27.5 degrees. Original terrain assets remain available.
- Finish Charting the Frontier, then speak with Cartographer Elara at Greenhollow Post for Silence the River Reavers. Either faction can accept it.
- Defeat Selvek and return to Elara for60 base gold,140XP and Riverwarden's Signet (+30 maximum HP). The lockbox separately awards45 gold,75XP and a potion. Racial/XP bonuses use existing systems.
- Earlier captain defeats count when accepting the bounty. Guards and unrelated bosses do not satisfy it. Completed kill objectives place a return waypoint on the giver.

## Fixes
- Leaving the home leash cancels queued melee/projectile hits, heavy telegraphs and cast bars.
- Enemies finish returning home before reaggro and recover fully there.
- Enemy line of sight respects solid walls and accepts the intended player's collider.
- Loading restores undefeated actors to their authored position, health and Idle pose, including after an earlier character killed them. Defeated actors are immediately hidden on reload; normal death animations remain during combat.
- Legacy NPC auto-setup skips batch validation and existing expanded worlds, avoiding a Play Mode scene-switch attempt.

## Validation
130 passing headless gameplay assertions,0 failures. Includes actual damage-driven captain death, guard rejection, lockbox gating/persistence, one-time quest rewards, early defeats, leash/cast cancellation, wall occlusion, earlier-character restoration and the graded approach. Previous startup, terrain, companions, cartography and Abe campaign checks still pass. Combat defeats are simulated through the damage API; manual combat balance and traversal remain acceptance checks.

## Manual test
1. Finish Elara's survey and accept her follow-up bounty at Greenhollow Post.
2. Follow the waypoint to the western bank north of Willow Crossing. Fight the guards/captain and dodge the heavy attack circle.
3. Try opening the lockbox before and after defeating Selvek, then save/reload. Defeated enemies and claimed rewards should remain completed.
4. Return to Elara for the signet. Try drawing an enemy beyond its home boundary; attacks should stop while it returns and heals.

## Build
Windows standalone succeeded: Errors: 0, Warnings: 495; Time: 00:00:23.3276049. Output: `Builds/ReaverUpdate/Riftbound.exe`. Prior FrontierUpdate build retained. Copied executable SHA-256 matches; source/build C# mismatches:0. Camp/approach geometry was inspected in a GPU preview; full gameplay lighting and balance need manual acceptance. Previous scene backed up under `.code-check/ReaverBackup-20261110T092807`.

## Main files
- Assets/RPG/Scripts/RiverReaverCamp.cs:actors and lockbox creation.
- EnemyController.cs / PlayerProgress.cs:combat disengagement and reload restoration.
- FactionQuestSystem.cs / ItemCatalog.cs / PlayerServices.cs:bounty, reward and lockbox gate.
- Assets/RPG/Editor/QuestRouteSmokeTest.cs:gameplay regression.
- FacingContentPass.cs / WorldPresentationSetup.cs:additive camp dressing, preview and build.
- ReaverCampTerrainSetup.cs:separate terrain copy, clearing/trail painting and approach grade validation.
