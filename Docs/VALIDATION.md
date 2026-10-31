# GitHub migration and playable build — 2026-10-31

For the newer QuestUpdate build and 57 quest/loot/navigation checks, see `Docs/QUEST_ROUTE_PASS.md`. This document preserves the migration pass results.

## Repository

- Private repository: https://github.com/altLov3r/riftbound
- Initial project snapshot: `66dbd55`.
- Models, textures, audio and native plugins use Git LFS. LFS integrity check passed, and the initial upload completed.
- Unity VC package removed; visible `.meta` files and text serialization retained. Original `.plastic` metadata remains local.
- Generated performance-test metadata, caches, diagnostic output and builds are ignored. Ignoring these files does not delete their local copies.

## Windows build

- Output: `Builds/GitHubPreview/Riftbound.exe`, including the accompanying player data folder.
- Result: **Succeeded — 0 errors, 680 warnings**; build size reported by Unity: 313,819,135 bytes.
- Warnings include AI inference shader variants and deprecated APIs in the imported Adobe plugin. They are not treated as resolved.
- Built from an isolated copy of the current project using Unity 6000.6.2f1 and `Riftbound.Editor.PrototypeBuild.BuildWindows`.
- The player, frontend, camera, scene and graphics settings matched the source snapshot. The copied executable's SHA-256 matched the built executable.
- Existing `Builds/Windows` output was preserved.

## Startup and movement smoke check

The isolated editor passed 17 assertions with a temporary test character and a separate save path:

- Character menu, player, health, animator, camera and HUD initialize successfully.
- Character handoff applies the test Human Warrior after reloading the scene.
- Entering the world closes the menu, unpauses the camera and restores time scale to 1.
- The selected character has a renderer; camera and HUD still reference the live player.
- Synthetic W input moved the character 10.30 metres.
- No RPG runtime exceptions were captured during this path.

The runner explicitly resumed the isolated editor when it paused. Earlier attempts stopped at frame 1 and were inconclusive. The passing run advanced beyond 1,000 frames. The temporary command-polling debug harness was replaced in the actual project with the manual `Riftbound > Validate Play Mode startup` command.

This was a headless control-flow check. It does not establish animation quality, rendering quality, combat balance, mount behaviour or a complete quest playthrough.

## Next game pass

Play the main route in the new executable: select a character, accept Abe's quest, fight enemies, claim the reward, summon a mount, save and reload. Prioritize blocking defects in that route. Review unused AI inference assets and the build warnings before adding further systems.
