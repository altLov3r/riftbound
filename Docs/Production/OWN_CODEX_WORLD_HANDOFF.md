# Codex world production 01 — handoff

## Identity and delivery

- Branch: **production/codex-world-20261116**. Tested source: **237039052ea015476418a4f7436f9fcb0521ef90**, pushed. Final documentation follows in its own commit.
- Checkout: C:/Users/skama/Documents/wow=rpg-diplomna/Overnight/codex-world.
- This new assignment started approximately 07:44 Sofia, after the previous delivery. It did not extend/reset the finished overnight shift. Overnight branch remains exactly **bef0420f4117529647e22b8706ec6763d0c7199f**; its build/handoff preserved.
- No main merge, Antigravity checkout edit, runtime/network/combat/UI/camera file change, package upgrade or world regeneration.

## Player-visible changes

1. Corrected the world-map roads to follow Highspire's real north gate and Greenhollow's clear arrival, rather than the old obstruction/marker line. Matching owned authoring definitions retained. Other routes, markers, buildings, actor IDs and rewards preserved.
2. Highspire north footway now matches the existing cobbled plaza and continues visibly through the gate. A new separate material reuses current terrain cobblestone diffuse/normal textures at five-metre tiling. Original materials and terrain paint unchanged.
3. Fixed the owned route-reader diagnostic: Play Mode static batching replaces live sharedMesh with a combined multi-route buffer. Tests now read the actual per-route source asset; verified combined268 vertices versus gate75 source points and correct endpoints. This is an editor/tool fix, not a runtime movement replacement.

Intentional scene diff against overnight: **four route points added, one replaced, one renderer material reference changed**. No transform, actor, terrain-reference or vegetation change in this assignment.

Changed gameplay assets: Assets/RPG/Scenes/Outpost-Expanded.unity; new Assets/RPG/Terrain/WorldProduction/Highspire footway cobblestone.mat plus metadata. Owned editor files: SettlementRoutePass.cs, WorldExpansionLayout.cs, WorldTraversalVerification.cs, WorldProductionAudit.cs and WorldRoutePlayModeCheck.cs. Review evidence/contracts/tree table/preparation are under Docs/Production/.

## Verified results

| Check | Actual result |
| --- | --- |
| Compilation and standalone | **Succeeded; 0 errors, 560 warnings**. Warnings remain, including existing deprecated Unity API usage; no broad cleanup attempted. |
| Existing QuestRouteSmokeTest after final source/scene/material | **145 PASS / 0 FAIL**, assertions unchanged. Includes existing separate-terrain, progression/reward/save and queued W checks. |
| Saved-scene controller geometry | **36 REACHED / 0 BLOCKED**, walk/mount, forward/reverse; includes full north road, bridge side lanes and cache standing approach. Native CharacterController plus mounted predictive box sweep; not manual acceptance. |
| Actual controller/mount via queued keyboard | **16 movement passes + 1 combined cache interaction/reload/duplicate-claim pass**. Scripted heading, fixture teleports between cases, isolated test-adventure files; not a continuous human playthrough. |
| Strict mounted Reaver-centre scenario | **FAILED / encounter-limited**, retained below; not silently converted to a pass. |
| Authoring/map alignment | Westbank and Greenhollow deviation **0.00m**; serialized data matches authoring, point counts7/6. |
| Camera-shape survey | **5,696 samples**,11 near-well walking compressions below1.5m,0 final overlaps. Eight yaw samples/pitch23; actual visual occlusion remains separate. |
| Protected terrain | **813 trees,15 prototypes**, Terrain/Collider match; every paint texel/detail density/hole and outside bridge-footprint height preserved; Reaver grade27.92 degrees. |
| Original/current terrain identity | All seven terrain files, including active protected bridge data, byte-identical to completed overnight Git/LFS SHA256 values. Four sampled seams remain0.0000m. |
| GPU world views | Five before frames and six recaptures produced through actual rendered Play Mode frames. Inspected all five before and three targeted recaptures. Native unedited Highspire before/after/gate images committed under Evidence/. Live actors/weather/UI not frozen; no deterministic pixel/FPS claim. |
| Fresh standalone launch | Exact new executable launched; unobstructed title/background/model rendered after foreground activation. Closed and confirmed exit. No character entered or roster/save edited in this smoke. |

Queued-input routes passed: bridge run and mount both ways; Highspire gate run and mount both ways; Greenhollow arrival run and mount both ways; bridge slow-walk toggle; full Emberwatch-to-Greenhollow running outbound/mounted return; Willow Rest standing approach, E interaction, duplicate rejection and isolated save/load retention.

## Fresh build and artifact identity

**Builds/CodexWorldProduction01/Riftbound.exe** — copy the entire directory. Built **2026-11-16T06:51:31.0800570Z / 08:51:31 Sofia**, **26.475s**, reported **542,389,611 bytes**. No network-prefab regeneration. Builds/raw receipts remain local, not uploaded as release artifacts.

The EXE/runtime assembly are unchanged generic/runtime binaries because this work changes world data and editor tools. Verify scene/shared data rather than identifying the build by EXE alone:

- Source scene SHA256: **18b8cc23662d5fb38d0e9216304f623fd8736589c406502ee936f356d5d553a1**.
- Riftbound_Data/level0: **2accd8dc13d7dd05fe2b168940a68103e9f935f6b4373d6671debdea435c9e52**, 5,506,272 bytes; differs from overnight.
- Riftbound_Data/sharedassets0.assets: **bf43d36673c6e5d4a616e320a21acef32caaaa5dd36a9d0fe0762b2ad6c60f9e**, 33,004,124 bytes; differs from overnight and contains the new Highspire footway material.
- Assembly-CSharp.dll: **6db7bc6bb19fceca41304fafbbcfbe060d9af0a519f7f6fef6d1f9e1e49546db**, unchanged; confirms no gameplay-runtime replacement.

## Limitations and ownership follow-up

- Strict Reaver mounted case reached last trace (-283.25,15.54,74.49), grounded, HP170, then failed the alive/mounted assertion before camp centre. That fixture version did not separately record final HP/mount state. Existing PlayerController.OnDamaged always dismounts; hostile contact is consistent with the result. This does not prove a terrain failure, a kill or complete mounted combat acceptance. No AI/health/collision bypass used. See GEMINI_RUNTIME_FINDINGS.md for reproduction/design review.
- Tree disposition table covers all26 data candidates:23 trees/3 shrubs, zero stored base offset. Sampled visible trunks/bases look upright/supported. **NOT RUN:** individually identified rendered bases/branches/colliders for every instance. No mass rotation/removal.
- GPU Reaver upper views show foliage occlusion despite zero camera-shape overlaps. **NOT RUN:** complete ordinary OrbitCamera rotation/foliage sightline acceptance, full animation review, live AI/NavMesh acceptance and audio listening. No camera runtime change made.
- **NOT RUN:** rendered frame-time comparison, continuous manual full adventure and two-PC online/combined acceptance. This world build does not include Gemini's later runtime branch; do not mix versions for online testing.
- CW04–CW10 preparation is a settlement/art-direction review brief, not applied new props, terrain paint or new land. Broader changes still need their specific reviewed scope/approval. No fifth tile generated.

## Integration and next action

No merge authorized/executed. Use a third clean checkout when the user requests integration with Gemini, preserve the world scene/data, rerun dependent offline/online checks and produce one combined build. AdventurePanels already reads the corrected serialized WorldMapRoutes; no duplicate runtime coordinate rewrite is required solely for these roads.

User test: launch the complete new build, open M to inspect roads, travel Emberwatch -> Highspire north gate -> Westbank/cache -> Reaver camp, and separately Greenhollow. Use / for slow walk, Z for mount and E for cache. Check camera foliage and combat-triggered dismount; these remain acceptance items.

**SCENE RELEASED — 2026-11-16 approximately 09:00 Sofia, this isolated production checkout only.** Own Unity build/test and standalone preview processes closed. Original shared checkout, completed overnight branch and Antigravity files preserved.
