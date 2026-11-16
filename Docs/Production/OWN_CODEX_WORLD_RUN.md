# Codex world production run

- Assignment: CW01–CW03, then eligible independent preparation under CW04–CW10.
- Start observed: 2026-11-16 approximately 07:44 Europe/Sofia (05:44 UTC).
- Branch: production/codex-world-20261116.
- Base: bef0420f4117529647e22b8706ec6763d0c7199f, completed overnight delivery.
- Checkout: C:/Users/skama/Documents/wow=rpg-diplomna/Overnight/codex-world.
- Finished overnight branch/build/handoff preserved. This is a new assignment, not a reset/extension of the old eight-hour shift. No new timed window requested.
- Scope: owned world/audit/editor tools and existing narrowly approved targeted changes. Broader terrain painting, new tiles/prefabs/major placement require their specific review/approval. No networking, combat/progression, runtime UI/movement/camera or LivingWorldEvents edits. No main merge.
- Original shared checkout and Antigravity checkout read-only. Antigravity still at f415f351da708b065c3cc1b65d1c1769c48cf577; no new gameplay handoff found at start.
- No Unity MCP available in this session; use isolated batch tools and bounded captures. Never scan Library/Temp/Logs/build directories.

## Current work

CW01: compare corrected footways, current paint and old authoring routes; fix owned coordinate definitions where stale, produce paint/placement proposals without broad repaint.
CW02: expand diagnostics to complete north approach, bridge side lanes and camera collision; use actual queued input/runtime controller when possible, labelled separately from manual play.
CW03: classify the 26 tree candidates by source transforms/support and native scene evidence; preserve intentional logs/bush tilt. No mass fixes.

## Attempt ledger

| Probe | Hypothesis / expected result | Attempt / timeout | Result / next action |
| --- | --- | --- | --- |
| Setup | Clean delivered checkout can branch without overwriting overnight | 1 | PASS; new production branch created from bef0420 |
| Read-only diagnostics | Road definitions, full routes and tree data expose concrete remaining defects | 1 / 5 min | Preparing bounded tool |

Maximum two unchanged failed attempts; stop after ten minutes without new evidence; cap unchanged-blocker investigation at twenty minutes. Stop only own recorded processes. Every pass specifies whether geometry, synthetic, queued-input, rendered or manual evidence.

## CW01–CW03 checkpoint — 08:28 Sofia

- Owned authoring definitions and exactly two scene WorldMapRoutes arrays now follow reviewed footways. Scene diff: four route points added, one replaced; no other scene-field change. The serialized data is consumed by AdventurePanels; no Gemini runtime file needed editing.
- Baseline drift was Westbank 13.62m / Greenhollow 2.39m. Initial expanded geometry: 34 reached, two blocked because the newly written cache test incorrectly targeted the solid chest centre. Preserved that receipt; new interaction stand-off is within the unchanged 3m range. Real E/duplicate/reload check subsequently passed.
- Camera-shape survey: 5,696 samples, eleven compressed walking views near Highspire well, zero final overlap samples. No camera-runtime or well placement change; actual camera/visual interpretation remains separate.
- Per-instance tree table written in REAVER_TREE_REVIEW.md: 26 candidates, 23 trees / 3 shrubs, stored grounding offset zero. Native rendering/collision still not established for every instance. No vegetation changed.
- New native queued-input harness compiled. First GPU attempt (PID42108) failed before character readiness. Second GPU attempt (PID13024) stayed at frame1 and logged Unity editor search exception; stopped only own PID. Native window capture selected Game but showed the underlying Codex window, so no coordinates/input were used from it. No capture pass claimed.
- Headless attempt1 (PID29776) passed four bridge running/mounted directions, then failed Highspire. Isolated diagnostic PID42844 reproduced wrong path coordinates. New evidence: runtime static batching replaces live MeshFilter.sharedMesh with a Combined Mesh, mixing route buffers. Fixed owned SettlementRoutePass.ReadPath to load the original per-route source mesh and added endpoint/source checks; no world/runtime gameplay defect inferred from those invalid fixtures.
- Corrected full headless attempt PID32640: staticBatch=True, live combined vertices268, proper gate source75 points, correct (-200,-60) -> (-225,0) endpoints. **16 movement passes + 1 cache E/duplicate/save-load pass.** Covered bridge both run/mount directions, Highspire both run/mount directions, Greenhollow arrival both run/mount directions, bridge slow-walk toggle, full north road on-foot outbound/mounted return and cache approach.
- Strict mounted Reaver-centre case failed after reaching (-283.25,15.54,74.49), HP170 at last trace. Existing PlayerController.OnDamaged always Dismounts; hostile camp contact invalidates an expectation of uninterrupted riding through combat. Preserve the failed outcome; no health/AI/authority bypass or terrain change. Full combat/manual camp acceptance remains pending. Next harness failure now records exact final HP/mount state.
- New GPU capture-only hypothesis: existing error-pause was stopping early frames; corrected fixture clears it as the existing QuestRouteSmokeTest does. PID23712, 08:27:48, source mesh/readiness fixes retained, 120s internal bound. One capture-only attempt; do not repeat unchanged if it fails.
- Prepared WORLD_ROUTE_CONTRACT.md and WORLD_SETTLEMENT_REVIEW.md. No new props, paint or broader placement change made. Next: final diagnostics, existing quest regression, unique standalone and production handoff/push.

## Validated source checkpoint — approximately 08:48 Sofia

- GPU capture-only with pause handling succeeded: five original native frames, then six after the targeted material fix. Inspected all five first frames and Highspire/Highspire-gate/Reaver-upper recaptures. Before/after Highspire plaza and gate evidence copied unedited into Docs/Production/Evidence. Same nominal camera/settings, live weather/UI not frozen; no deterministic pixel/performance claim.
- Demonstrated dark Highspire footway strip replaced by a new separate material using the existing cobblestone terrain-layer diffuse/normal textures, five-metre tiling and layer surface settings. Changed only its renderer material reference; source Road.mat and terrain paint preserved. SaveAssets normalized one unused legacy _Color float in Road.mat; backed up/restored that automatic change, excluded from commit.
- Final read-only audit: **36 REACHED / 0 BLOCKED**, both route authoring deviations **0.00m**, serialized map arrays match definitions (Westbank7 / Greenhollow6). Camera shape totals unchanged:5,696 samples,11 near-well compressions,0 final overlaps. GPU foliage occlusion is a separate limitation and is not contradicted by those overlap counts.
- Seven terrain assets (including active protected bridge data) match completed-overnight Git/LFS SHA256 values. No terrain/prototype/paint/detail/height changes in this assignment.
- Existing QuestRouteSmokeTest completed after final scene/material/code changes: **145 PASS / 0 FAIL**, unchanged assertions. Runtime files, package versions and project settings unchanged.
- CW01 fixes validated; CW02 has native geometry plus16 queued movement passes and cache interaction pass, with strict mounted Reaver encounter outcome retained as a limitation. CW03 individual data classifications complete, sampled visual evidence available, full per-instance/camera acceptance pending. Broader settlement work remains a review brief, not applied content.
- Next: checkpoint/push source, new unique Builds/CodexWorldProduction01/Riftbound.exe and final own handoff. Old overnight branch/build remain preserved.
