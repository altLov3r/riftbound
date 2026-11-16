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

## Delivery — approximately 09:00 Sofia

- Tested source **237039052ea015476418a4f7436f9fcb0521ef90** pushed to production/codex-world-20261116; upstream set. No main merge. Finished overnight ref still bef0420.
- Build attempt1 PID3828: **Succeeded / 0 errors / 560 warnings**,26.475s,542,389,611 reported bytes, completed08:51:31 Sofia. Protected data/grade and current read-only seams checked during build. Unique output Builds/CodexWorldProduction01/Riftbound.exe.
- Exact executable/DLL/global manager/level0/sharedassets0 files checked and hashed. New scene/sharedassets differ from overnight; new material name present in sharedassets0. Runtime DLL is unchanged, as expected for world/editor-only changes. Full identities in OWN_CODEX_WORLD_HANDOFF.md.
- Fresh standalone title rendered after explicit foreground activation; no character entered. Closed and confirmed own window gone. No background work/process remains.
- Source/docs whitespace check passed; all seven terrain hashes match overnight; original Road.mat restored after one automatic float normalization, not committed as an unrelated edit. Source working state clean before final documentation.
- Result boundaries preserved:145 existing regression passes;36 geometry reaches;17 queued-input pass lines and one retained strict Reaver encounter failure; GPU samples/foliage limits described honestly. No complete manual/online/performance/per-instance-tree acceptance claim.
- **SCENE RELEASED**, own production checkout only. Current assignment complete with reviewed source/build/handoff and remaining user/Gemini acceptance. No new eight-hour timer was created.

## Continuation 02 — CW04 / narrow CW05, started 09:03 Sofia

- Same production branch/checkpoint e746638, clean starting worktree. Completed overnight branch and Production01 build/handoff preserved. No new eight-hour deadline. No main/runtime integration.
- Read Gemini's new production handoff at3935340: it reports41 gameplay assertions,145 offline passes and10/10 Relay matrix on its new build; manual GUI/GPU FPS/two-PC acceptance still pending. Read-only inspection, no rerun or edit of its checkout.
- Scope: the previously approved C05 ordered Emberwatch loading area and small Westbank rest/supply vignette using existing licensed scenery. This is not a settlement redesign, terrain grading, new buildings or vegetation removal. Concrete previews precede saving placements. No new NPC logic/labels/HUD.
- New bounded supply survey creates/destroys temporary model instances only. Validates support over centre/corners, solid overlaps, road margins, service/giver aprons and terrain-tree footprint. Final maximum terrain height spread0.08m; no flattening/forced fallback.
- Legacy Prefab_Medieval_* candidates were rejected after rendered review: miniature scale beside the buildings. Initial asynchronous screenshot switching also mislabelled frames; fixed by keeping camera/scene unchanged until PNG and rendered-frame completion. Those temporary plans/frames were not applied.
- Reused the existing FantasyProps FBX family already used in the village, with the same existing material resolver. Local FantasyProps/License_Standard.txt identifies the standard free pack and CC0. No download, source model/import/material overwrite or roster change. Rigid model wrappers preserve imported axes; unused model animators disabled on instances only.
- Final read-only survey: three Emberwatch items and two Willow Rest items fit. Crates approximately0.9–1.0m, barrels0.9m tall; bench2.777m long/0.533m high. Bench faces along the crossing direction and its narrow footprint found one valid flat anchor among63. No terrain or tree edit needed. Plan bound to source scene SHA18B8CC23662D5FB38D0E9216304F623FD8736589C406502EE936F356D5D553A1 and source-model hashes.
- Attempt ledger: initial preview camera timing fixed; miniature prefab proposal rejected; one compile error from new guard declaration order fixed before any scene save. Final survey compiled/passed. Native six-view FBX preview launched PID27452 at09:57:54; review before apply. Performance samples are paused/UI-hidden editor frame timings, not standalone FPS. The batches counter reports0 and must not be interpreted as zero draw calls.

### Continuation02 validated placement — approximately10:14 Sofia

- Inspected all six final preview images. Correct family/scale, supported bases and open travel/service space. Editor p95 before/after: yard9.822/10.246ms, departure12.048/11.945ms, Willow10.050/10.546ms. This is a paused editor sample, not standalone/GPU acceptance.
- Applied exactly five inert scenery props under `Codex reference-route supplies`: Emberwatch two crates/one barrel; Willow Rest one bench/one barrel. Existing FantasyProps FBX links/materials preserved; no custom scripts, IDs or runtime changes. Actual source scene diff adds712 lines and removes0. Plan/source hashes rechecked and exact scene backed up before save.
- Apply PID17284: PASS after save/reload for root count, exact transforms, model references, footprint support and clearances. Maximum footprint height spread0.043m. Five active renderers/five solid box colliders.
- Focused actual-controller departure PID42088: four queued-input passes (foot/mount, outbound/return), isolated test save and scripted heading. Not continuous/manual gameplay. Headless log includes RenderTexture unsupported-device failures and one UnityEditor.Search.SearchDatabase ArgumentOutOfRangeException; no code compilation error. These are retained as tooling/graphics limitations, not a claim of a clean rendered run.
- Read-only full audit PID376,08:11:19Z:36 REACHED/0 BLOCKED. Westbank/Greenhollow alignment remains0.00m. All seven terrain files still match protected overnight SHA256 values.
- Existing QuestRouteSmokeTest PID40628, completed10:12:04 Sofia:145 PASS/0 FAIL, unchanged assertions including separate-terrain protection. No user saves touched. No repeat of the earlier strict mounted Reaver-centre scenario; its encounter-limited failure remains recorded.
- Native saved-scene capture PID40980 started after regression exited. Unique Production02 packaging follows; Production01/overnight deliveries remain intact. Four accepted native before/after frames and plan/reload/timing receipts published under Docs/Production/Evidence/Supplies02.
- Saved-scene capture completed and all three final images inspected: frames-20261116-081237. Correct scale/support/clear exit retained after reload; scene unchanged by capture. Source FBX hashes all match reviewed plan. Code/docs/meta whitespace check clean; Unity's new prefab override records contain ten conventional empty `value: ` lines, left as serialized by Unity. No pre-existing scene text removed.

### Production02 delivery — approximately10:18 Sofia

- Tested source397fe49095b4b3fca7172044f7c6faa592718a15 committed/pushed on the same production branch; five native PNGs uploaded throughGitLFS, fsckOK. No main merge or Gemini edit. Finished overnight ref unchanged.
- Build PID16920 succeeded10:15:40 Sofia:0 errors/560 warnings,17.936s,542,578,667 reported bytes. Unique Builds/CodexWorldProduction02/Riftbound.exe; previous outputs untouched. Protection813 trees/15 prototypes/8 layers, paint/detail/hole/outside-footprint heights, matching Terrain/Collider andgrade27.92 passed.
- Six exact packaged files hashed; level0/sharedassets changed and new scenery names present in level0. Runtime DLL unchanged from Production01. Receipts copied into Evidence/Supplies02. All seven terrain hashes reverified after build; no asset normalization required.
- Standalone first launched hidden (PID5764), initialized scene/D3D11 but had no targetable window. Stopped only that own process, then changed hypothesis to visible Computer Use launch. Actual title/background/model rendered; closed normally viaAlt+F4, verified no window/process. No character/save changed. Manual route/GPU-performance/combined-online checks remain NOT RUN.
- **SCENE RELEASED**, own checkout only. Final handoff OWN_CODEX_WORLD_02_HANDOFF.md; current source and final documentation pushed separately. No background work remains.
