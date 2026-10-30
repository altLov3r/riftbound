# Riftbound prototype

Open `Assets/RPG/Scenes/Outpost.unity` and press Play.
If first setup has not generated the scene yet, wait for imports/compilation, then choose **Riftbound > Create prototype scene**.

## Controls

- WASD / arrow keys: run relative to camera (5 m/s)
- C: toggle slow walking (1.5 m/s) / running
- S / down arrow: move backward while facing forward, using backward run/walk clips
- Space: jump
- Hold right mouse and move: orbit camera
- Mouse wheel: zoom
- Tab: target nearest living raider
- 1 / left click: melee strike (24 damage)
- 2: whirlwind (38 area damage, 5 second cooldown)
- 3: second wind (45 healing, 12 second cooldown)
- E near Abe: open his quest dialogue; use Accept Quest / Complete Quest buttons
- Escape or E while talking: close dialogue; movement and attacks are blocked while it is open
- Escape: pause/resume
- R when defeated: recover at the village

## Playthrough check

1. Move to Abe (yellow ! above his head), press E, then click Accept Quest.
2. Follow the road through the bannered gate.
3. Defeat three raiders, using healing between fights.
4. Return to Abe (yellow ? when ready), press E, then click Complete Quest for 25 gold and restored health.
5. Check camera orbit, zoom, building collision, pause, and recovery after defeat.

## Layout and code

- `Assets/RPG/Art`: copies of supplied FBX models and village textures; original downloads are untouched.
- `Assets/RPG/Scripts`: movement, camera, health, enemy AI, combat, mission and prototype HUD.
- `Assets/RPG/Editor`: asset import settings, scene generator and asset inspection report.
- `Assets/RPG/Materials`: generated URP materials.
- `Assets/RPG/Animations`: generated Brute animation controller.
- `Assets/RPG/Scenes`: generated prototype scene.

The generator only creates the Outpost scene if it does not already exist. Opening its menu item again opens the existing scene, preserving scene edits. **Rebuild generated prototype scene** replaces the generated layout; use it only before making manual scene changes or after backing them up.

## Developer checks

- **Riftbound > Run gameplay smoke test** runs an input-driven Play Mode check and writes `PrototypeSmokeResults.txt`. It moves the player and fights enemies, then exits Play Mode.
- **Riftbound > Build Windows demo** builds `Builds/Windows/Riftbound.exe` and writes `PrototypeBuildResult.txt`.
- `PrototypePreview.png` is a screenshot captured at the end of a successful smoke test.

Latest verification: 19 smoke checks passed. Windows release build succeeded with zero errors and launched successfully. Installed inference-package shaders emit build warnings; URP also logs stripped, unused post-processing effects at launch. The prototype renders and the missing target-ring shader exception was fixed. Those package warnings remain for later cleanup.

## Current scope

One playable warrior, six village buildings, forest road, four raiders, Abe and one patrol mission with dialogue and a gold reward. Progress lasts for the current Play session. Races, factions, character creation, equipment, saving and additional classes are later milestones.

Buildings are exterior set pieces; interiors and doors are not interactive yet. Defeated enemies use a temporary fall pose because no death clip was supplied. Nature uses temporary flat colours. Brute's embedded textures were extracted from the supplied FBX and assigned to generated URP materials. Humanoid animation compatibility and the gameplay loop were verified in Play Mode.

## Asset provenance

Village: Quaternius Medieval Village MegaKit Standard, CC0 (license included in original download).
Nature: supplied Quaternius Nature kit; preserve original license with final distribution.
Brute and animation FBX: supplied by project owner; record their source/license before distributing project source.
