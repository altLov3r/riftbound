# Settlement supplies — production checkpoint02

Small scenery update under the existing C05 loading/rest authorization, on production/codex-world-20261116. This is not a settlement redesign or new terrain work. Source scene before edit: SHA25618B8CC23662D5FB38D0E9216304F623FD8736589C406502EE936F356D5D553A1.

## Actual placements

| New scenery root | World position | Yaw | Source asset | Footprint height spread |
| --- | --- | --- | --- | --- |
| Emberwatch packed supply crate | (-13.000,-0.114,-5.000) | 10 | FantasyProps/FBX/Crate_Wooden.fbx | 0.000m |
| Emberwatch outbound supply crate | (-11.600,-0.114,-4.850) | 90 | FantasyProps/FBX/Crate_Wooden.fbx | 0.000m |
| Emberwatch provision barrel | (-12.800,-0.170,-6.400) | 15 | FantasyProps/FBX/Barrel.fbx | 0.000m |
| Willow Rest resting bench | (-287.000,6.036,46.000) | 90 | FantasyProps/FBX/Bench.fbx | 0.043m |
| Willow Rest provision barrel | (-285.200,5.982,45.800) | 0 | FantasyProps/FBX/Barrel.fbx | 0.000m |

Assets are under Assets/RPG/Art/. They come from the already-admitted free Fantasy Props family used elsewhere in Emberwatch; Assets/RPG/Art/FantasyProps/License_Standard.txt identifies CC0. Existing family material mapping is reused. No download, paid asset, source import change or original prefab/material overwrite.

The crates/barrels are roughly one-metre props; bench is2.777m long and0.533m high. Wrappers retain imported model orientation; unused model animators disabled on these instances. Five active renderers and five solid box colliders added. No loot/quest/NPC script, persistent ID, runtime label or HUD element added.

## Grounding and clearance

The bounded survey rejected unsuitable positions instead of flattening terrain. It sampled centre/corners, required <=0.08m terrain height spread, kept conservative2.2m clearance beyond prop radius from reference travel centrelines, protected4m service/giver aprons and rejected solid overlaps/terrain-tree footprints. Bench was oriented toward the crossing to fit its narrow footprint on the flat patch. All five source-model hashes and the original scene hash were rechecked before applying.

After save/reload: exact root count/transforms, original source-model links, grounding and clearance passed. Serialized scene edit adds712 lines with no existing line removed. No original building, road, NPC, actor, terrain/paint/tree or serialized reference removed/replaced.

## Preview evidence

Six native1280x720 before/after frames at .code-check/world-production/frames-20261116-075808/. All reviewed: matching scale/material palette, visible supports and clear departure/rest access. UI hidden and simulation paused; these are environment previews, not normal gameplay or animation acceptance. Camera/scene held until asynchronous screenshots completed.

| View | Editor p95 before | Editor p95 after |
| --- | --- | --- |
| Emberwatch loading area | 9.822ms | 10.246ms |
| Emberwatch departure | 12.048ms | 11.945ms |
| Willow Rest | 10.050ms | 10.546ms |

Warm20 rendered frames, sample90 per view. No increase above the predefined25%+2ms threshold in this limited paused editor sample. This is not standalone FPS/GPU acceptance. The batches counter returned0 and is treated as unavailable, not zero draw calls. Live rendered-game performance remains a separate check.

The first legacy decorative-prefab proposal was rejected because it rendered as miniatures. No such instances were saved. Native FBX models plus existing material mapping resolved that family/scale mismatch without altering original assets.

## Verification status

Source placement/reload, six-view preview and three saved-scene recaptures passed. Focused actual-controller departure:four queued-input passes. Full geometry:36 REACHED/0 BLOCKED. Existing regression:145 PASS/0 FAIL. Seven terrain/three source-model hashes unchanged. Production02 standalone succeeded with0 errors/560 warnings, and its title launched/rendered/closed normally. Exact identities, receipts, headless-tooling warnings and NOT RUN checks are in OWN_CODEX_WORLD_02_HANDOFF.md. No continuous manual, standalone-performance, combined-runtime or two-PC acceptance claimed. Broader settlement/lighting/vegetation changes remain outside this update.
