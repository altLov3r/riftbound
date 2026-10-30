# Riftbound — agent handoff

Updated: 2026-10-31. No Antigravity agent has been started or connected by this file.

## Project
- Unity 6000.6.2f1, URP, C#, Input System.
- Open this directory as the Unity project; Assets/, Packages/ and ProjectSettings/ are here.
- Current scene: Assets/RPG/Scenes/Outpost-Updated.unity.
- Latest validated executable: Builds/GitHubPreview/Riftbound.exe. Builds/Windows is the previous output; the normal build menu writes future builds there.
- Legacy Outpost.unity still contains the old rig. PrototypeEntry routes Play from it to Outpost-Updated.
- GitHub repository: https://github.com/altLov3r/riftbound (private). Use Git with Git LFS; preserve .meta GUIDs. The original .plastic metadata remains local as a fallback.

## Working features
- Editable heightfield terrain, conformed roads/courtyard, raised quest/encounter platforms and projected scenery. Source cube ground is retained as an inactive renderer/collider marker for compatibility; native Terrain supplies collision.
- Skeleton visual roots rotate 180 degrees to match AI +Z. EnemyController.FacePlayer tracks targets through attacks/windups, with a melee facing cone.
- Frontier rendering preset: 4x MSAA, 2048px soft shadows, SSAO, ACES/color/bloom/vignette volume and fog. Assets under Assets/RPG/Terrain; original Assets/Settings PC presets retained.
- Main menu, character selection/creation, per-character local saves.
- Three main quests: raiders, supplies, captain. Village, camp, watchtower, optional Western Crypt and Greenhollow frontier with three scouting landmarks.
- Four Skeleton enemy models with melee/ranged roles, boss telegraph.
- Two provisioners, potions, eleven gear items, equipment stats, six loot caches, expanded map, damage numbers.
- Ground is now 160x300m; northern route crosses a brook/bridge to Greenhollow Post and Ancient Grove. Three new scouts and five harmless wildlife actors.
- Z summons/dismounts; V cycles unlocked mounts. Horse is immediate, zebra unlocks from all three scouting markers. Summoning takes 1.5 seconds outside combat; damage/actions dismount. Selected mount and markers persist per character.
- Mounts use ScriptableObject definitions, native animal clips and a Brute riding pose. Moving saddle aligns rider hips, axe is hidden, standing capsule/model restored on dismount. Mount body uses clearance/sweep checks; camera frames the rider.
- Three persistent runestones unlock Crypt Warden; independent of the main captain quest. Warden drops axe, locked reliquary gives signet.
- Q held Guard: 50% damage after armor, max 3 seconds, 6-second recharge after release. Native Block animation.
- H: interruptible 5-second return to camp, only outside combat, 60-second recharge. J: quest journal.
- Generated short hit/heal/rune sounds use existing master volume. World labels face the camera.
- Training dummy: restores health, grants no quest/XP/gold credit.
- Native Generic Brute locomotion. Contact speed/phase/height calibration; slower backpedal.
- Quaternius-derived hit/death/air animations baked to native Brute curves.

## Evidence and limits
- TerrainPolishBuildResult.txt is now the latest build. TerrainPolishResult.txt has 36 new edit-mode geometry/facing/render checks, alongside 63 passing mount/demo/equipment regressions. No gameplay/Play Mode tests were run. TerrainSurvey.txt records slopes (all authored routes below 34 degrees) and root clearances.
- Terrain-*.png and Enemy-Fixed-*.png are edit-mode previews. TerrainPolishSetup.cs generates terrain, texture layers, road ribbons, lighting and checks. TerrainLayout.cs defines heights/platforms/routes. MountExpansionSetup avoids resetting the world geometry after terrain exists.
- Generated terrain diffuse/normal textures are produced by project code; existing village/nature/character source licenses remain unchanged. New asset search priorities are in ASSET_REQUESTS.md.
- MountExpansionBuildResult.txt records the earlier mount build. MountExpansionResult.txt contains its data/rig/scene/progression checks; existing DemoPassResult.txt and EquipmentResult.txt are rerun as regressions. No Play Mode gameplay tests were run.
- MountPreviewResult.txt, Mount-horse/zebra-Idle/Run.png and Greenhollow-*.png are edit-mode previews. Mounted travel, jumping, collision feel and balance require user feedback.
- Two downloaded packs: Quaternius's older LowPoly Animated Animals (7 FBXs) and Fantasy Props MegaKit Standard (21 FBXs + 4 base-color atlases). Source/license details in CHARACTER_ASSETS.md. The newer animal pack's Drive quota failed; no invalid HTML files remain in Assets.
- DemoPassBuildResult.txt records the earlier crypt build. DemoPassResult.txt: 16 edit-mode scene/progression/guard checks, including rune unlock/load and one-time Warden reward.
- Western-Crypt-Overview.png, Western-Crypt-Entrance.png and Native-Guard-Preview.png are edit-mode renders, not gameplay captures.
- AnimationAuditResult.txt: 11 native clips have intended loop flags and no actor-root drift. Toe heading statistics are bone diagnostics, not proof of foot orientation in gameplay. DemoPassValidation.txt verifies source, scene and player-assembly synchronization.
- MovementBuildResult.txt: succeeded, 0 errors, 34 warnings.
- MovementResult.txt: planted-foot measurements for four gaits, not a gameplay acceptance test.
- EquipmentResult.txt: 12 isolated edit-mode checks.
- PolishResult.txt: animation/save-data checks.
- User requested to do gameplay testing personally. Visual motion quality still needs their feedback.
- Do not claim the animations are perfect based on compilation or curve sampling.

## Important files
- Assets/RPG/Scripts/PlayerController.cs: movement/combat; partials PlayerProgress, PlayerServices, PlayerEquipment.
- GroundedVisual.cs: ground offset, no procedural ankle rotation override.
- ItemCatalog.cs, EquipmentPanel.cs: gear data/UI.
- FrontendMenu.cs, PrototypeHUD.cs, AdventurePanels.cs: menus/HUD/map/vendor.
- Assets/RPG/Editor/MovementSetup.cs: contact calibration/training dummy and BatchBuild.
- EquipmentSetup.cs and PolishSetup.cs: isolated setup/check/build helpers.
- DemoPassSetup.cs: idempotent crypt setup, isolated progression checks, scene/guard preview and BatchBuild. Uses the existing Updated scene; preserves main quest roles and authored content.
- PlayerUtilities.cs, PlayerDiscovery.cs, Runestone.cs, QuestJournal.cs, WorldLabel.cs and GameSound.cs contain this pass's added features.
- PlayerMounts.cs, MountDefinition.cs, MountVisual.cs, PlayerExploration.cs, Landmark.cs and Wildlife.cs implement mounts/exploration. MountExpansionSetup.cs imports/converts assets, builds the new zone, validates saved progress and renders the mounted poses. Use -rebakeRider only when intentionally regenerating the rider clip.
- FlowingWater.cs scrolls the brook's rippled normal surface. TerrainPolishSetup restores village paving and horizon/sky colors, preserving the original rendering presets and old flat-scene backup.
- CHARACTER_UPDATE.md: controls and recent changes.
- Assets/RPG/CHARACTER_ASSETS.md: asset sources/licenses.

## Collaboration
- One agent owns a file or scene at a time. Agree scope with the human before parallel edits.
- Use separate Git clones/branches for parallel work; integrate through pull requests and compile afterward.
- Only one Unity editor may open a given project directory. Do not share Library/ or Temp/.
- Preserve user edits, saves, backups and .meta files. Do not rebuild the whole prototype scene.
- Never copy an older scene/controller over the current one without examining the differences.
- Do not run an agent from an application not explicitly started/authorized by the human.
- When handing back, update this file with changes, validation, unresolved problems and the next task.

## Latest animation overhaul (2026-10-29)
- Fixed GroundedVisual.cs per-frame jitter: stopped shifting animator root position every single frame on Idle foot height changes. Baseline position is calibrated stably on start, and gait offsets are smoothly interpolated without frame freezing or snapping.
- Fixed locomotion crossfading: replaced unpredictable normalizedTime offsets in PlayerController.Play() with smooth fixed-time crossfades (0.16s), eliminating frame skipping and desync popping.
- Fixed hit stun lockup: taking damage no longer zeroes player moveVelocity or freezes movement for 0.28s; hit animation plays while retaining fluid player locomotion.
- Fixed diagonal strafing snap: movingBack requires intentional backward direction (input.y < -0.35 and abs(input.x) < 0.6), preventing abrupt 180-degree rotation flipping during lateral movement.
- Fixed mount hyperactivity: tuned MountVisual.cs galloping stride from 4m to 7m, bringing horse/zebra playback from an unnatural 2.25x speed down to a natural, majestic cadence (~1.14x - 1.28x), with airborne jump speed normalized to 1.0x.
- Fixed enemy slow-motion dragging: raised EnemyController.cs playback speed floor from 0.1x to 0.6x and added smooth attack recovery transitions.
- Verified C# compilation of Assembly-CSharp and Assembly-CSharp-Editor with 0 errors.

## Suggested next task
Use the main quest route in the GitHubPreview build as the next stability target: character selection, Abe's quest, combat, reward, mount, save/reload. GitHub is private and synced; teammate clone/import instructions are in README.md. See Docs/VALIDATION.md for the successful Windows build (0 errors, 680 warnings) and 17 passing headless startup/movement assertions. Actual animation/rendering quality and a full quest/mount playthrough still need manual testing.

Test enemy orientation/tracking during combat, hills/roads, grounding, mounted travel and the new rendering preset in Builds/Windows/Riftbound.exe. Profile actual frame rate before increasing graphics further. Preserve terrain edits; do not run the original whole-scene rebuild.
Get the user's riding/Greenhollow feedback using Builds/Windows/Riftbound.exe: Z summon/cancel/dismount, C slow travel, movement/jumps/collisions, combat dismount, three markers, V zebra selection, save/load. On-foot animation feedback remains relevant.
Inspect the relevant runtime behavior before adding more animation workarounds. For independent
work, improve a bounded UI or content task without touching movement/controller/scene files owned
by another agent. Do not start concurrent editing until ownership is agreed.
