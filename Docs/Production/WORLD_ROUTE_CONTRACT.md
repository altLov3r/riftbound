# World route contract — Codex production 01

Owner: Codex scene/authoring. Branch production/codex-world-20261116, base bef0420. Read alongside OWN_CODEX_WORLD_RUN.md and final handoff. No runtime files owned by Gemini were changed.

## Applied data alignment

The scene's existing Expanded map route data / WorldMapRoutes component has two corrected point arrays. WorldExpansionLayout.Routes uses matching coordinates. AdventurePanels.cs reads that serialized component to draw roads, so adoption is a scene-data change; no runtime WorldMapRoutes/AdventurePanels rewrite is required.

| Existing route name | Corrected X/Y/Z points |
| --- | --- |
| Westbank approach | (-200,12,-60), (-202.5,12,-57.5), (-204,12.48,-15), (-225,9,0), (-240,6,50), (-260,6,50), (-285,6,50) |
| Greenhollow road | (0,3,100), (-15,9,235), (-17,11,349), (-17,11,368), (-27,11,386), (-36,13,395) |

Scene diff: four points added, one point replaced. Existing route names/container references, all other branches, markers/discovery IDs, NPCs, reward data, buildings and terrain unchanged. Existing north-valley branch is preserved; it has not been newly validated by this route contract.

The later narrow visual fix changes one additional scene material reference: Highspire north footway now uses its own material variant with the existing terrain cobblestone diffuse/normal textures and five-metre tiling. It removes the rendered plain dark strip. Terrain paint and original material assets remain unchanged; see Evidence/ before/after images.

The runtime map is a 2D road depiction, not an autopilot. Ground heights/bridge support must still be sampled by actual movement. No terrain generator, painter or network-prefab generator was run. Original paint remains; cobblestone within a town/plaza is not automatically a road defect.

## Evidence and interpretation

- Pre-correction authoring/footway drift: Westbank approach up to 13.62m, Greenhollow road up to 2.39m.
- Initial expanded physics probe reached 34 of 36 targets. Two targets incorrectly requested walking into the chest centre and were blocked by its solid body, as expected physically. New interaction approach is (-285,50) -> (-288,51) -> (-288.7,52) in X/Z, outside the chest, inside its unchanged 3m interaction range. Actual E/reload/duplicate-claim verification is a separate runtime check.
- Terrain-relative camera-shape survey: 5,696 samples at eight yaw angles/pitch23. Eleven walking views near Highspire's well compressed below1.5m; no final sphere-overlap samples. These are geometry candidates, not evidence to delete/move the well or a full actual-camera acceptance pass.
- Actual queued-input results, final authoring drift and regression/build identity are recorded in the final handoff when available. Do not promote pending checks to passes.

## Gemini integration

1. Read the pushed source/handoff, not only this coordinate table. Codex's completed overnight world branch and this production branch are separate from Gemini's finished online/runtime branch.
2. Preserve the scene route data when the user authorizes integration in a third checkout. Do not regenerate the world/map from older definitions.
3. If gameplay changes need another route/interaction location, provide stable ID, approach, range, fail/reset behavior and exact scene request. Codex remains scene owner.
4. Keep online session-only progress and no-host-migration disclosures. Current offline route tests do not establish online or combined-branch correctness.
