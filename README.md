# Riftbound

A small 3D fantasy RPG built in Unity for a diploma project and competition.

## Open and play

1. Install Git and Git LFS, then run `git lfs install` once.
2. Clone this repository and run `git lfs pull` inside it.
3. Add the cloned folder in Unity Hub and open it with **Unity 6000.6.2f1**.
4. Let Unity import assets and install the packages from `Packages/`.
5. Open `Assets/RPG/Scenes/Outpost-Updated.unity`, press Play, select a character and choose **Enter World**.

Use the Game tab for playing. `Riftbound > Direct Play in Editor` bypasses character selection for quick testing.
While playing, `Riftbound > Validate Play Mode startup` checks the player, camera, HUD and keyboard connections.

## Controls

- WASD: move; mouse buttons: camera; mouse wheel: zoom; Space: jump.
- C: toggle slow walking; E: interact; Tab: select a target.
- Z: summon/dismiss mount; V: cycle unlocked mounts.
- I: inventory; J: quests; M: map; Escape: settings/pause.

In the quest journal, **Track Main Quest** creates a live minimap waypoint. It follows the next raider or supply crate and returns to Abe when the objective is ready. Placing a manual waypoint stops automatic tracking.

Enemies award XP on defeat. Press **E** near their remains to collect gold, bone shards and equipment. Unclaimed loot survives save/reload with its remaining two-minute gameplay timer; claimed rewards stay claimed.

More controls and project notes are in `CHARACTER_UPDATE.md`, `TASKS.md` and `HANDOFF.md`.

## Work with a teammate

- Each person clones into a separate local folder. Never share `Library/` or `Temp/`.
- Pull before working. Use a separate branch per task and merge through pull requests.
- Commit assets together with their `.meta` files; move or rename assets inside Unity.
- Agree who edits a scene or prefab. Avoid simultaneous edits to the same scene.
- Commit `Assets/`, `Packages/` and `ProjectSettings/`. Builds, caches, saves and Unity VC metadata stay local.
- For script changes, use small commits. Keep models and textures in Git LFS.

Unity VC metadata is retained locally as a fallback; new work is tracked with Git. This repository begins with the current project snapshot, not imported Unity VC history.

## Windows build

Exit Play Mode and choose **Riftbound > Build Windows demo**. The executable is written to `Builds/Windows/Riftbound.exe` and is intentionally ignored by Git.

## Assets

Keep the repository private while the source/license records for all imported character and prop assets are completed. Existing attribution and license files are preserved under `Assets/RPG/`; see `Assets/RPG/CHARACTER_ASSETS.md`.

## Quest regression check

Use an isolated Unity project copy containing `Assets/`, `Packages/` and `ProjectSettings/`. Start Unity in batch mode with `-executeMethod Riftbound.Editor.QuestRouteSmokeTest.Run` and without `-quit`; the check exits when finished. It creates a fresh test save under `.code-check/` and writes `QuestRouteSmokeResult.txt`.

The check covers startup, movement, Abe's three quests, enemy eligibility, loot collection, legacy save compatibility and live quest navigation. Defeats are simulated through the combat damage API; visual quality and combat balance still need manual testing.
