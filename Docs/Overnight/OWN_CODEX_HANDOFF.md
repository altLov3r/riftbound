# Codex overnight world handoff

**Delivery complete — isolated world branch, with the acceptance limitations below.**

## Identity

- Checkout: C:/Users/skama/Documents/wow=rpg-diplomna/Overnight/codex-world
- Branch: overnight/codex-world-20261116; published base 1b32349f24dc0e8e529fbefc7a80a022c122ae77.
- Fixed shift: 2026-11-15T22:36:39.4470822Z to 2026-11-16T06:36:39.4470822Z (00:36:39–08:36:39 Sofia). Freeze respected; the intervening conversation gap is not claimed as continuous execution.
- Source checkpoint: **9c632c94dd2e97dc7f88ecad433483c0b248686b**, pushed. Original shared checkout and Antigravity checkout were not edited. No main merge.

## Delivered source

| Commit | Result |
| --- | --- |
| 5bf111696874c3c4b81087bd969a60f158903752 | Reproducible-clone bootstrap: restore 18 exact manifest-listed Adobe source/native/metadata files incorrectly excluded by broad Library ignore; narrow .gitignore exception. No package upgrade or recursive cache scan. |
| ad8e019 | Ground/support Willow Rest cache; bounded protected bridge-bank repair, six support adjustments and preservation/controller comparison tools. |
| 93d1423 | Expanded production plan and prompts across six Docs files; 32 owner packets, 72 feature slices and design-only Stormglass Reach proposal. |
| 9c632c9 | Emberwatch exit, Highspire north-gate and Greenhollow arrival footways, two retained inactive fence segments, route geometry verification and final audit/build entry point. |

World paths: Assets/RPG/Scenes/Outpost-Expanded.unity; Assets/RPG/Terrain/ReaverCamp/Westbank crossing protected.asset; Assets/RPG/Terrain/OvernightRoutes/. Owned tools: OvernightWorldPass.cs, SettlementRoutePass.cs, WorldTraversalVerification.cs and bounded-footprint extension to ReaverRepairPreparation.cs under Assets/RPG/Editor.

No networking, session UI, combat/progression, LivingWorldEvents, models/animation source, packages or major architecture were intentionally changed. No world regeneration. Small additions reuse existing materials; no downloaded/paid assets.

## Verification actually completed

- Fresh isolated QuestRouteSmokeTest: **145 PASS / 0 FAIL**, including unchanged separate-terrain assertion, queued W-input movement and existing quest/loot/save checks. No RPG runtime exceptions reported. Headless RenderTexture warnings are not a visual pass.
- Real saved-scene controller probes: **20 REACHED / 0 BLOCKED** for Emberwatch–Highspire, Highspire–Westbank, bridge, Reaver approach and Greenhollow arrival; walk/mount and both directions. Mounted cases include the existing predictive box sweep. Bridge baseline four blocked -> repaired four reached. These are geometry tests.
- Protected bridge data after save/reimport: **813 trees / 15 prototypes**, all eight terrain-layer references, every paint texel/detail density/hole and outside-footprint height preserved. Exactly 377 bank samples changed, no low riverbed changes. Reaver grade **27.92 degrees**.
- Terrain/Collider share protected bridge data. Six original terrain assets match published-base Git/LFS SHA256 bytes. Four seams sampled at 129 points each: **0.0000m** maximum delta.
- Tree review: 26 interpolated Reaver candidates, zero additional bridge candidates >0.1m, stored root-ground offset maximum 0.000m. No vegetation changed; visual/collider disposition remains unverified.
- Current renderer audit: 872 classified renderers, zero missing terrain, nine elevated component candidates (seven lids/two chest bands). Supported components are not nine floating root objects. Trees/unclassified props are outside renderer coverage.
- Existing scene-field replacements reviewed: six supports' positions/scales, cache root, Terrain/Collider data refs and two fence active flags. Giver/actor IDs, rewards, lid hierarchy, well/houses/walls preserved.
- Plan integrity: GP 16, CW 16, EX 72, SR 8 unique task rows; six files present; fenced blocks balanced. Source/docs whitespace check passed; generated Unity serialization retains its normal whitespace.

Local receipts: .code-check/overnight/{cache-repair,bridge-repair,bridge-controller-comparison,settlement-routes,traversal-geometry,final-world-checks}.txt, original-assets-verification.json, .code-check/phase1-world-audit.txt and QuestRouteSmokeResult.txt. Concise results are committed here; raw receipts/builds remain local.

## Build

Attempt 1: PID 26548, FinalChecksAndBuild, unique output **Builds/OvernightCodexWorld/Riftbound.exe**. **Succeeded, 0 errors, 806 warnings**, duration **13m30.571s**, reported size **542,392,715 bytes**, completed **2026-11-16T05:37:35.4593853Z / 07:37:35 Sofia**. No multiplayer prefab regeneration. Compiler output includes deprecated Unity object-lookup API warnings; warnings are not cleared or waived as a visual/runtime quality pass. Editor shutdown also reported JobTempAlloc allocations; runtime impact was not profiled.

Known executable/data checks passed for Riftbound.exe, UnityPlayer.dll, Riftbound_Data/globalgamemanagers and Riftbound_Data/Managed/Assembly-CSharp.dll. EXE SHA256 **af88b840e599661ac63fc10ae27ac77dd97cd46b44df8a7ce60eb0dcfacfc99e**; gameplay assembly SHA256 **6db7bc6bb19fceca41304fafbbcfbe060d9af0a519f7f6fef6d1f9e1e49546db**. Exact local receipt: .code-check/overnight/build-artifacts.json. Copy the complete build directory, not just the EXE; build artifacts are local and ignored by Git.

Fresh standalone launched through Computer Use: title rendered, Play opened character selection and the existing character/background preview rendered at 1280x720. Closed via Alt+F4, then confirmed the own window exited. No character entry, roster changes or gameplay save writes were performed. Screenshots were observed in the tool/chat, not saved as matched world captures. **Observed UI issue:** session-mode labels clip in their narrow buttons; assigned as GP07 reproduction on Gemini's newer runtime branch, not fixed across ownership here.

## Limitations / next work

- **NOT RUN:** full ordinary-input walking/riding route, camera sweep on all routes, normal cache open/reload/duplicate claim at the new position, complete north-road coverage, individual rendered tree/prop bases/colliders, live AI/NavMesh and actual audio listening.
- **NOT RUN:** matched GPU before/after/performance comparison. First GPU Camera.Render stalled; bounded headless fallback completed independent checks. Existing prototype bush tilts are candidates, not demonstrated defects.
- Corrected footways can coexist with original terrain paint; reconcile visible old road traces in CW01 after rendered review. Do not repaint/regenerate the whole map to hide them.
- C05 decorative expansion deferred at freeze. New regional/class/crafting/dungeon/tile content is planning only, not advertised as delivered.
- Antigravity's f415f351da708b065c3cc1b65d1c1769c48cf577 handoff reports extensive online passes and a separate build. Manual GUI/two-PC tests remain NOT RUN there; those passes were read, not rerun here. This world build does not contain that branch's later runtime changes.

## Integration request

No merge authorized/executed. Preserve both branches and integrate only in a third checkout when requested, then rerun dependent regression and online/route acceptance and produce one combined build.

1. Adopt the isolated Adobe bootstrap commit if the integration checkout lacks those files. Antigravity's handoff still describes the missing-source prerequisite.
2. Review Codex scene/data additions and Antigravity runtime changes independently. Avoid copying an old scene over the new one.
3. Runtime map/navigation route adoption belongs to Gemini: Highspire X/Z (-200,-60),(-202.5,-57.5),(-204,-15),(-225,0); Greenhollow arrival (-17,368),(-27,386),(-36,395). Ground-sample Y and retain unrelated branches/IDs. Details in POST_ONLINE_EXPANSION_PLAN.md section 6.
4. Keep online session-only persistence/no host migration disclosed. No combined-compatibility claim from separate branch builds.

## Suggested player test once packaging completes

Use the complete new build directory. Offline title/character entry -> exit Emberwatch west -> Highspire plaza/north gate -> cross Westbank both ways -> inspect/open Willow Rest cache -> walk/ride to Reaver camp -> return. Separately travel north to Greenhollow and stop beside its scout marker. Check summon/dismount, rails/banks, camera and service interactions; report location/action/character for defects.

Six known fresh-import Brute walk FBX metadata normalizations were backed up locally and restored to committed bytes after the editor/preview closed. No animation import changes are included in this branch. The build used Unity's fresh-imported assets; animation quality was not reviewed by the title/selection check. No unrelated dirty files remain at delivery.

**SCENE RELEASED — 2026-11-16, approximately 07:40 Europe/Sofia, this isolated Codex checkout only.** Own Unity build process and standalone preview are closed. No pending scene/terrain/prefab write is in progress. Next implementation is a separately activated CW production packet; external gameplay/visual acceptance remains pending above.
