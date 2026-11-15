# Riftbound — eight-hour AI development shift

Prepared 2026-11-16, Europe/Sofia. This is an execution plan, not a claim that an overnight run has started or that eight uninterrupted hours are guaranteed.

## 1. Mission and starting point

Deliver a visibly better world and a more reliable online adventure, with independently testable branches and clear morning handoffs. Complete small player-visible improvements rather than spending the night rebuilding plans, generating unnecessary frameworks or rerunning the same smoke test.

Published starting checkpoint: **ea744154c4b5bd9b037af2ca12a27342f312585e**, private repository https://github.com/altLov3r/riftbound. Fetch and start from the newest published commit containing this plan, not necessarily that older checkpoint. The checkpoint includes online integration, expanded world, current assets and the approved Reaver repair.

Read these before implementation:

1. Applicable AGENTS.md instructions.
2. RIFTBOUND_MASTER_PLAN.md and RIFTBOUND_FEATURE_BACKLOG.md; read the active sections, not the entire backlog on every continuation.
3. CODEX_WORLD_AUDIT.md final applied-repair section.
4. ONLINE_MULTIPLAYER_STATUS.md and ONLINE_SETUP.md.

Baseline evidence:

- QuestRouteSmokeTest: 145 passes, zero failures after Reaver repair.
- Protected Reaver terrain: 813 trees, 15 prototypes, paint/details/holes and outside-footprint heights preserved; three-lane maximum approach grade 27.92 degrees.
- Six actual scene CharacterController geometry probes reached camp. These are not normal input/mount-box/camera tests.
- Four sampled terrain seams match at 0.0000 m.
- Latest local build before this shift: Builds/CoopReaverRepair/Riftbound.exe, zero build errors, 553 warnings. Builds are local artifacts, not part of the source Git upload.
- Antigravity records verified Relay smoke scenarios, progression, late join and session recovery. Cross-network two-PC acceptance and ordinary-input end-to-end party play remain distinct checks.
- Existing placement issues: buried Willow Rest cache, bridge/bank discontinuities, fence near Emberwatch departure, Highspire road intersecting well/house/wall, Greenhollow road arriving at a marker collider.

## 2. Runtime and continuation rules

At the first execution prompt, record UTC start, Europe/Sofia display time and **one deadline eight hours after that start** in the agent's own run journal. The shift starts when actual execution begins, not when this file was written. Use current tool time; do not invent elapsed hours.

The eight hourly blocks below are targets. They are eight wall-clock hours per agent running concurrently, not a promise to deliver sixteen hours of serial work or every backlog feature.

- Work automatically through the next eligible packet until the deadline, completion of all approved work, unavailable credits/tools or a genuine blocker with no independent work remaining.
- A queued "continue" resumes this same shift and deadline. It does not create another eight-hour shift, reset attempts, redo setup or authorize additional scope.
- If the deadline has passed, finish the smallest safe operation, checkpoint and report. Do not begin another major change.
- Do not keep a process idle merely to consume the remaining time. Do not claim background work after the run has ended.
- Preserve adequate time for verification and delivery. At elapsed 6h30, freeze new systems; use the remaining 1h30 for regression, focused fixes and packaging.
- Local runs require the PC awake, applications open, repository available and sufficient usage allowance. Two-PC internet testing needs actual access to the second machine/network; mark NOT RUN if unavailable.

### Mandatory failure limits

Before a test record hypothesis, expected result, timeout, input/build identity and last reached stage.

1. At most two unchanged failed attempts. A different command name, editor restart or larger timeout is not a new hypothesis.
2. Stop a focused test after ten minutes without new evidence/state progress. Cap investigation of an unchanged blocker at twenty minutes, then document and move to independent work.
3. Build/import timeouts must reflect real measured progress and project size. Do not repeatedly rebuild to diagnose a dashboard permission error or repeat cloud calls for invalid input.
4. Stop only processes launched by your run, using recorded IDs. Never kill the other agent's editor/build.
5. Do not weaken assertions, bypass authority or hide broken features to get green results.
6. Maintain attempt counts across chat/context resets. A continuation message does not erase them.
7. If a test exposes unrelated defects, classify severity and ownership; do not expand across the other agent's files unilaterally.

## 3. Isolation setup and file ownership

### Required workspace setup

Use **two new Git clones/checkouts based on the same latest published main snapshot**. Do not have two Unity editors or agents mutate the existing shared project directory. Avoid reusing the older .github-build-validation checkout as the working game; it contains intentional stale/synthetic snapshots.

Suggested new directories under the existing approved workspace:

- Overnight/codex-world
- Overnight/antigravity-online

Refuse to overwrite existing directories. If a suggested directory already exists, inspect and resume the correct run or choose a new unique directory. Do not reset or delete an existing checkout.

Each checkout has its own Assets, Library, Temp and Unity process. Use Git LFS pull before opening Unity and verify large files are real asset contents rather than pointer text. Do not claim a complete clone before LFS assets are available. Fresh import may take significant time; show actual progress, then revise targets without faking a completed hour.

Branch names:

- Codex: overnight/codex-world-20261116
- Antigravity: overnight/antigravity-online-20261116

If a branch already exists, inspect it and its run journal; do not overwrite it. Commit/push only your branch when the user starts the corresponding prompt. No force push, automatic main merge, destructive reset or clearing unrelated work.

### Ownership matrix

| Area | Codex world branch | Antigravity online branch |
| --- | --- | --- |
| Outpost-Expanded scene, world Terrain, nature/bridge/settlement prefabs | Own targeted changes | Treat as read-only; request Codex integration if required. |
| Phase1WorldAudit, Reaver/world authoring tools | Own | Read-only. |
| Scripts/Network and online bootstrap/configuration | Read-only | Own. |
| Combat/progression/class/quest/shop/mount runtime integration | Read-only | Own necessary corrections; preserve existing IDs. |
| LivingWorldEvents, PartySystem, UI/session/HUD runtime | Read-only | Own necessary bounded corrections. |
| Animation import/model family replacement | Audit/report; avoid roster replacement | Correct runtime playback only if clearly needed; coordinate asset changes. |
| Packages/manifest and lockfile | No dependency changes | Only compatibility/blocker changes; no speculative upgrades. |
| Main master/backlog/status historical docs | Read as context | Read as context. |
| Overnight journals and handoff | OWN_CODEX_RUN.md / OWN_CODEX_HANDOFF.md | OWN_ANTIGRAVITY_RUN.md / OWN_ANTIGRAVITY_HANDOFF.md |

These OWN_*.md files live under Docs/Overnight/. Each agent writes only its own pair. Do not claim that a note in one isolated clone automatically appears in the other; communicate through pushed branch commits/explicit handoffs. The user can relay a dependency if direct coordination is unavailable.

Codex is the sole shared-scene change owner for the final integration proposal. Antigravity must avoid generating/re-saving the scene or network prefab unless essential and recorded; separate runtime auto-bootstrap is preferred where existing architecture already supports it. Never run whole-world generation.

## 4. Scope and authorization boundary

Writing this plan is not itself authorization to perform new overnight asset edits. The start prompts accompanying this file explicitly request the scoped work; once the user submits one, its listed targeted edits are authorized. Preserve any narrower later user constraints.

Codex's proposed overnight asset scope, when started by its prompt:

- Ground the Willow Rest cache by its root, preserving ID, reward logic and lid hierarchy.
- Repair Westbank bridge deck/bank continuity through bounded bridge/approach geometry changes; preserve river, village and unrelated terrain.
- Create a deliberate usable opening/alignment at the identified Emberwatch fence route.
- Reconcile Highspire path/junction markers with the actual north gate, without removing major buildings/walls or redesigning the capital.
- Provide a safe Greenhollow arrival beside the scout marker, preserving marker IDs and all mount criteria.
- Make small purposeful decoration/vegetation improvements only around cleared reference routes and using existing licensed assets.

Any necessary new terrain work uses a new protected current-data clone and a documented local footprint. Keep original terrain assets unchanged. Preserve the verified Reaver terrain and vegetation; do not undo it or copy the old tree-less asset. No mass vegetation removal or major architectural replacement.

Antigravity's proposed overnight scope: finish/audit existing online gameplay, authority, progression, session recovery, party extras and user-facing reliability. New public servers, account systems, global economy, PvP, extensive class/quest expansion, paid services and asset downloads are outside this eight-hour shift.

## 5. Eight-hour schedule at a glance

Times are elapsed from each recorded start. If a prerequisite takes longer, shift lower-priority content out; never squeeze verification out.

| Elapsed | Codex: world/traversal | Antigravity: online/gameplay |
| --- | --- | --- |
| 00:00–01:00 | New checkout, baseline, cache repair and reference-route survey | New checkout, baseline, normal UI/session entry and ownership audit |
| 01:00–02:00 | Westbank bridge transitions and collision | Client combat, HP, abilities and exactly-once rewards |
| 02:00–03:00 | Emberwatch/Highspire path obstructions and gate alignment | Quest/shop/loot/mount/respawn cooperative progression |
| 03:00–04:00 | Greenhollow arrival and affected tree/prop grounding | Late join/disconnect/repeated session recovery |
| 04:00–05:00 | Coherent reference-route environmental polish | Party frames/pings/ready/revive quality and UI input conflicts |
| 05:00–06:00 | Foot/mount/camera/nav checks and bounded geometry fixes | World-event replication/eligibility and state cleanup |
| 06:00–07:00 | Visual/performance comparison; freeze new systems at 06:30 | Four-player/failure matrix; freeze new systems at 06:30 |
| 07:00–08:00 | Regression, fresh branch build, handoff and pushed checkpoint | Regression, fresh branch build, handoff and pushed checkpoint |

## 6. Codex detailed work packets

### C00 — setup and baseline [00:00–00:25]

- Record base SHA, branch, project path, editor version, start/deadline and clean Git status.
- Confirm latest main includes the repaired final Terrain/TerrainCollider assignment and all required LFS assets.
- Inspect CODEX_WORLD_AUDIT final checkpoint and do a concise current-scene audit. Do not rerun every historical audit or recreate the protected terrain.
- Capture matched Westbank/Reaver/Greenhollow views and record baseline settings. Verify original asset hashes before edits.
- Write a list of the exact objects/assets to change, preserving persistent IDs.

Gate: current world opens/compiles, baseline is understood and own scene scope is recorded.

### C01 — restore the buried Willow Rest cache [00:25–01:00]

- Locate the existing cache near x=-285,z=60 by persistent ID and hierarchy. Inspect full root/renderer/collider footprint; do not move its individual lid/bands.
- Choose the smallest safe root adjustment to a grounded, reachable, off-path position. Keep it within its intended rest/crossing area and preserve loot eligibility.
- Sample ground over the footprint; check slope, collider access, interaction range and occlusion, not only the renderer centre.
- Ensure the adjusted cache does not re-block the Reaver route or clip into the bridge/river.
- Capture before/after and verify that opening/reloading/claiming still follows existing rules. Do not change loot runtime code to mask placement.

Gate: cache visibly grounded, reachable and off the clear route; unchanged ID/rewards/hierarchy.

### C02 — continuous Westbank crossing [01:00–02:00]

- Inspect the known grade discontinuity near (-248,6.03,50) using actual deck and bank colliders. Keep the bridge deck footprint and river geometry visible in the review.
- Verify both ends, rail openings, support alignment and approach slope. Identify whether the fault is gap, step, terrain clipping or collision mismatch.
- Fix through the smallest bounded deck/approach adjustment. Prefer preserving the river over flattening its trench; no broad world regeneration.
- Test centre and side lanes in both directions with walking/mounted controller shapes and actual player/mount behavior where available.
- Check overhead/lateral camera clearance, rails, falls and dismount locations. A flat terrain grade alone does not pass the bridge.
- Confirm surrounding water and props remain coherent and original terrain/vegetation outside any reviewed footprint remains intact.

Gate: a continuous bridge route can be travelled in both directions without jumping a geometry gap or falling through it.

### C03 — align roads with usable settlement circulation [02:00–03:00]

- Emberwatch: inspect the fence around (-21,-0.15,-2); produce one intentional path opening or narrow alignment correction while retaining the fence's purpose.
- Highspire: preserve well, western townhouse and curtain walls. Test a street route through the actual north gate around (-204,-20), instead of a line through scenery.
- Review nearby benches/lamps/carts before choosing waypoints; do not relocate one blocker into another route.
- Update only owned road/path/map authoring definitions if needed. If runtime navigation/map code belongs to Antigravity, provide an exact coordinate/ID integration request rather than editing it.
- If visible road meshes require scene changes, limit them to the corrected segment; preserve world bounds and other road branches.

Gate: player can leave Emberwatch, reach Highspire and exit toward Westbank through a believable opening/gate; route samples and player behavior agree.

### C04 — Greenhollow arrival and tree/prop review [03:00–04:00]

- Terminate the arrival path at usable space beside the scout marker, not inside its collider. Preserve all discovery IDs and zebra requirements.
- Inspect the 26 interpolated-height tree-review candidates from the Reaver receipt; verify bases, trunk direction and nearby collision without mass removal.
- Review sampled bush/rock orientation and intentional logs. A nonidentity root or rock partly in ground is not automatically a defect.
- Correct only demonstrated local grounding/rotation errors and document each affected root/prefab. Never rotate every nature object blindly.
- Check provider/NPC/giver approach spaces and mounted camera framing.

Gate: clear Greenhollow arrival, preserved discoveries, and a documented disposition for affected tree/prop candidates.

### C05 — add restrained environmental life through scenery [04:00–05:00]

Use existing licensed props; no new asset-family shopping or AI model-generation detour.

- Emberwatch: ordered loading/work area and readable departure sign, placed off the travel corridor.
- Westbank: rest/supply vignette, believable bridge supports and vegetation clustered along banks below the deck.
- Reaver approach: readable dirt/rock transition and camp-edge framing; keep combat and route sightlines open.
- Greenhollow: forest-edge thinning, farm/work details and landmark framing, with useful empty space around services.
- Preserve consistent scale/material palette. Avoid repeated identical prop grids or piles hiding the terrain problems.
- Do not add NPC logic, duplicate events, runtime labels or permanent HUD panels; those belong to the other workstream.

Gate: matched captures demonstrate a coherent improvement without new blockers or meaningful measured regressions.

### C06 — traversal, collision, camera and performance [05:00–06:30]

- Test Emberwatch → Highspire → Westbank → Reaver camp and Emberwatch → Greenhollow, outbound and return.
- Include run/walk, mount summon/travel/dismount, jump/landing where intended, camera rotation near walls/trees/rails and interaction access.
- Exercise actual input when tools permit; label controller probes separately. Check NavMesh/AI traversal where relevant, without rebuilding unrelated actors.
- Rerun seam, outside-footprint and vegetation-preservation checks after terrain changes.
- Compare fixed route/frame-time captures at identical settings; address a demonstrated bottleneck instead of adding more graphics effects.
- Review all intended edits in the serialized scene and ensure no runtime/quest/other terrain churn slipped in.

Gate: known route failures resolved or explicitly identified; no claim of full traversal from sample overlap counts alone.

### C07 — world branch delivery [06:30–08:00]

- Freeze additions. Fix only blockers/regressions within scope.
- Run the relevant existing regressions, including separate-terrain protection, progression and discoveries. Preserve tests and real save data.
- Build to a new unique directory, e.g. Builds/OvernightCodexWorld/Riftbound.exe, with required data. Never rebuild multiplayer prefabs just to produce a world build.
- Capture exact source SHA/local changes, build result, warnings, before/after screenshots, route acceptance and any NOT RUN checks.
- Commit/push validated changes only to the Codex overnight branch. Do not merge Antigravity or publish main automatically.
- Write OWN_CODEX_HANDOFF.md with changed paths, overlap risks, integration order, user test route and SCENE RELEASED for this checkout.

## 7. Antigravity detailed work packets

### A00 — setup, existing evidence and normal UI entry [00:00–01:00]

- Record base SHA, branch, own project path and shared eight-hour shift metadata in your journal.
- Read the latest online handoff before repeating tests already verified. Preserve successful receipts and focus on remaining gaps.
- Launch through title/selection/Host Online/Join Online UI, not exclusively CLI shortcuts. Verify scene-loaded bootstrap, pending states, button debouncing and cancel/back.
- Check offline entry without UGS initialization and exactly one camera/audio listener/local controller per process.
- Inspect credentials/profile/environment handling without printing tokens or publishing invitation codes in Git docs.
- Establish one short normal-input cooperative route and isolated test identities/saves.

Gate: the UI users actually click enters/leaves a playable session correctly, or a precise blocker is recorded.

### A01 — combat and authority [01:00–02:00]

- Test both host and client attacks, resource/cooldown rejection, real client HP loss, healing, control and death.
- Verify host computes accepted damage and validates life state, range, LOS, class/ability and resources; all clients cannot dictate arbitrary stats.
- Ensure targeted RPCs reach the intended local owner and effects/audio play once. Audit host-only shortcuts separately from client behavior.
- Recheck movement limits, authorized teleports, dodge/mount state and impossible-position rejection.
- Exercise invalid/stale/repeated requests with a bounded automated check; do not build an elaborate exploit framework.
- Preserve leash/reset/pending-hit cancellation and existing native animation behavior unless evidence requires a focused correction.

Gate: a non-host player can fight, take damage and receive correct credit under the same supported rules as host.

### A02 — complete cooperative progression [02:00–03:00]

- Play one Abe quest and one optional bounty through accept/objective/return/reward with independent players.
- Verify per-player kills/discoveries/chests and repeated-claim rejection. One player cannot consume another's objective permanently.
- Test potion purchase/use, insufficient-gold transaction, full inventory, rest/temper where supported and atomic grant failure.
- Mount and dismount using normal controls; verify remote actual model/state, unlock policy and return to movement.
- Test respawn/revive and simultaneous helpers; prevent stale health overwrite or duplicate transitions.
- Clearly disclose online session-only progress unless durable persistence is genuinely implemented and tested. Do not attempt an unplanned backend overnight.

Gate: complete short adventure works for both players without manual debug grants, duplicated rewards or misleading save claims.

### A03 — late join, departure and repeated sessions [03:00–04:00]

- Late join during combat and during an active event; verify snapshot readiness, enemy health/death, actor count and personal progression.
- Test client leave while targeted/interacting/casting/mounted, then return to Offline or another session.
- Test host departure and return-to-menu message; no gameplay host migration promise.
- Repeat host → leave → host → offline and join → leave cycles. Count managers, player objects, listeners, event handlers and UI locks.
- Test bad/expired/full/version-mismatched join conditions and user cancellation during asynchronous service calls.
- Use bounded timeouts and collect the exact last stage; do not rerun a stalled network test all night.

Gate: failure returns a usable client, and a fresh session can start without restarting the application.

### A04 — polish already implemented party extras [04:00–05:00]

- Party frames: accurate ownership, health, roster removal, solo hide and readable minimal footprint.
- Pings: correct recipient set, rate limit, expiry, text/input conflict and map-coordinate accuracy.
- Ready check: acceptance/decline/timeout, late join and host-exit cleanup.
- Revive: distance, interruptions, dead/alive state, two-helper race and respawn conflict.
- Resolve key collisions among interaction, revive, mount, dodge, chat and menus. Update tooltips/Controls only for actual bindings.
- Keep menus/context hints restrained; do not add permanent panels or unrelated social systems.

Gate: extras work through real input or are marked unavailable/NOT RUN honestly; no recurring overlay crowds the HUD.

### A05 — existing world events and state hygiene [05:00–06:00]

- Verify caravan/rescue actor creation is host-owned and replicated, including current stage for late joiners.
- Test success, failure, abandon/leave, cooldown and cleanup without duplicate actors or XP/gold.
- Check first-clear versus repeat bounty policy under session-only persistence; no unearned durable-claim promises.
- Ensure quest-critical actors remain accessible and event cooldowns do not block main progression.
- Correct only runtime behavior, not caravan scenery/routes or Codex's scene. Supply exact runtime/authoring integration requests if necessary.

Gate: two existing event templates are either verified end-to-end or their exact remaining limitations are documented. Do not replace them with five unfinished new events.

### A06 — party-size/failure/performance matrix [06:00–06:30]

- Verify four concurrent players, fifth-player refusal, disconnect/rejoin eligibility and capacity accounting.
- Recheck the highest-risk case introduced by your changes, not every old scenario blindly.
- Measure bandwidth, frame time and actor/effect count in one repeatable encounter; fix demonstrated pathological behavior.
- If a real second PC/network is unavailable, provide the exact external acceptance steps and mark that cell NOT RUN. Same-machine Relay is not cross-network proof.

Gate: honest test matrix with no hidden P0/P1 failures.

### A07 — online branch delivery [06:30–08:00]

- Freeze features, run source/Play Mode/standalone checks appropriate to changed systems and resolve new blockers.
- Build to a new unique directory, e.g. Builds/OvernightAntigravityOnline/Riftbound.exe. Preserve baseline builds and Codex's assets.
- Commit/push only your branch. Do not merge main or Codex automatically and do not regenerate the shared world.
- Write OWN_ANTIGRAVITY_HANDOFF.md with changed paths, tested source/build, raw outcomes summarized, known limitations, setup needs and exact user host/join steps.
- Explicitly state any unavoidable scene/prefab integration request instead of silently taking ownership of Codex files.

## 8. Shared checkpoints and communication

At approximately 01:00, 02:00, 04:00 and 06:30 elapsed, update the agent's journal with:

```text
Run ID / agent / project / branch / base SHA:
Original start UTC / fixed deadline UTC / current elapsed:
Current ticket / actual files touched:
Completed player behavior:
Tests, observed results and evidence paths:
Failures / attempts / last new evidence:
Blocked dependencies and independent next task:
Current commit / build path / uncommitted work:
```

Keep secrets, service tokens and join codes out of published handoffs. Full local logs remain local; commit concise test results and repro steps under Docs/Overnight so GitHub readers can understand the evidence without .code-check artifacts.

If a small validated packet is ready, checkpoint it rather than keeping every change uncommitted for eight hours. Never commit a knowingly broken halfway state as a finished feature. No force pushing or deletion of unrelated changes.

## 9. Morning integration proposal

Neither agent auto-merges. The user reviews two branches first.

1. Confirm both branches descend from the same published base and identify any overlaps.
2. Review Codex scene/terrain/prop changes and Antigravity runtime changes independently.
3. Integrate in a **third clean checkout/branch** only when authorized, never by copying an older scene/controller over newer work.
4. Merge/review runtime changes and Codex-owned scene changes with explicit ownership. Do not resolve conflicts by accepting every side blindly.
5. Recompile; rerun quest/discovery/loot/save and the changed online/route cases on the combined state.
6. Produce one combined build with source SHA, player instructions and known limitations. Separate branch builds do not establish combined compatibility.
7. Merge to main/publish the combined state only after this integration gate and authorization.

## 10. Morning acceptance checklist

- Fresh launch, title/character selection, Offline/Host Online/Join Online and usable Leave/Back.
- All supported local controls and exactly one local camera/HUD/listener.
- Two players can fight, take damage, receive their own quest/loot rewards, use a shop, mount and respawn.
- Late join and host/client departure behave as documented.
- Willow Rest cache is visible, grounded, reachable, uniquely claimable and off the path.
- Bridge crossing works outbound/return on foot/mount, including camera and bank transitions.
- Emberwatch exit, Highspire north-gate approach and Greenhollow marker arrival are clear.
- Reaver protection, 813-tree baseline or individually documented approved vegetation edits, unchanged original terrain and matching Terrain/Collider data.
- Existing regressions pass or new failures are reported with exact scope; no weakened assertions.
- Matched visual/performance comparison and precise NOT RUN list.
- Fresh branch builds and pushed commits; no silent merge to main or claim of full combined acceptance.

## 11. Fallback work if a packet blocks

| Blocker | Independent useful work | Forbidden shortcut |
| --- | --- | --- |
| Cloud/account/second-network access | Offline authority/cleanup tests, UI errors, documentation | Infinite login/retry loop or fake network pass. |
| Expensive initial import/build | Inspect assigned source/content, write bounded tests/repro notes | Starting competing editors in one project or killing other processes. |
| Shared runtime file belongs to other agent | Prepare coordinate/ID integration note, continue own files | Editing the other branch's live files. |
| Scene/terrain repair needs broader scope | Preserve state, prepare exact proposal/preview, do approved local work | Whole-world generation, deleting architecture or clearing trees. |
| A feature is already verified | Work on the next unverified acceptance gap | Repeating it for hours to appear active. |
| Credits/deadline reached | Safe checkpoint and final handoff | Promising continued invisible background work. |

## 12. Completion standard

Eight hours is a work window, not a feature-completion guarantee. Report delivered player behavior and evidence, not hours supposedly consumed. If all approved useful work is done sooner, finish cleanly. If time/credits end, leave a precise restart point with the same recorded deadline and unresolved checks.

The best outcome is two small coherent verified branches, fresh test builds and an integration plan. An enormous untested diff or endless network loop is a failed shift even if the agents stayed busy.
