# Riftbound — post-integration production plan

Prepared16 November2026 for integration/combined-20261116. This is an actionable backlog, not a declaration that every system is absent or complete. It supersedes older immediate-work ordering while retaining the master/backlog as the full design reference.

## 1. Baseline and evidence rules

Integration merge56df479 combines Codexfc35bc8 and Gemini3935340 from their common published base1b32349. **Docs/Integration/INTEGRATION_HANDOFF.md owns the final source SHA, build identity and actual results.** Read it before choosing work; neither old agent standalone establishes combined compatibility.

Initial combined candidate01 compiled with0 C# errors and built with0 errors/844 warnings. Its combined-source checks recorded145 offline quest passes,41 gameplay passes,36 geometry reaches,4 queued departure passes and10/10 automated Relay scenarios. Native1280x720 title/character-selection views showed readable session labels. These results are real but did not establish grounded multi-tile world entry: a later native check exposed a spawn-position defect outside the older smoke coverage. Candidate01 is therefore not the final release candidate.

The integration owner added an independent TerrainCollider-based regression: the faulty baseline passed19/32 checks and failed13. A minimal ComputeSpawnPosition change selects the terrain through WorldRegions.TerrainAt; the new regression then passed32/32. This is native terrain/physics regression evidence, not a completed human adventure. Combined02 rebuild/final regressions are in progress at this update; its exact offline/online/build results and release decision belong in INTEGRATION_HANDOFF.md. Do not carry candidate01's results forward as if they tested a changed executable.

| Area | Recorded evidence | Work still required |
| --- | --- | --- |
| World/traversal | Protected Reaver/bridge repairs, cache support, three footways, corrected map arrays, Highspire paving, five supply/rest props; combined01 recorded36 geometry reaches/4 departure-input passes. New multi-tile spawn regression32/32 after the focused correction | Final changed-source/build checks per integration handoff; continuous ordinary-input routes/camera/AI and individual tree review |
| Online | Sessions/Relay code flow, authority, rewards, late join, events, revive and leave/rejoin; combined01 recorded10/10 headless scenarios including4 players/5th rejection | Verify final changed candidate and grounded entry/respawn/rejoin; two actual PCs on distinct networks |
| Gameplay | Four existing class kits, movement/facing/billboard/interaction fixes; combined01 recorded41 gameplay checks and145 offline assertions | Final changed-source checks, visual animation/boss/UI acceptance, measured balance |
| Animation assets | Runtime stride/orientation compensation; source Jog_Fwd_Loop/Sprint_Loop distortions diagnosed | Dedicated import/retargeting proposal and supported-rig matrix; source assets were not fixed |
| Presentation | Existing title/selection backgrounds, faction preview, ability UI, settings and audio systems | Visual/layout/listening/accessibility/performance review; do not build duplicate menus |
| Expansion | Extensive GP/CW/EX design backlog and region briefs | Audit prototypes before implementation; most added content below is still proposed |
| Greenhollow vignette | Three-prop read-only survey in the older Codex working copy | Not committed, integrated or shipped. Review/publish its exact source before adoption; never copy dirty files blindly |

Four terrain tiles currently cover x[-320,448],z[-160,608],768x768m. Stormglass Reach is a fifth-tile proposal, not existing land. The final roster remains a decision: original request2 factions/2 races each/5 classes; current four principal class kits and additional race prototypes are preserved. Do not delete saved enum IDs to force a design target.

Use these states in execution receipts: NOT STARTED, INVESTIGATING, IMPLEMENTED, VERIFIED, BLOCKED, NOT RUN. In the tables, **VERIFY** means exercise an existing implementation, **AUDIT** means establish its actual coverage, **FIX** means address a demonstrated defect, **ADD** means a proposed new slice and **DECIDE** means settle a contract before coding. A planning row is not a test result.

Priorities: P0 crash/data/authority/mandatory-route blocker; P1 polished first adventure and release; P2 expanded compact RPG; P3 separate optional product. Complete one vertical slice at a time. An item depends on its listed gates even if an independent design preview can proceed sooner.

## 2. Immediate integration and playable baseline — IG00–IG09

All P0/P1; integration owner executes these before accepting expansion. Existing tests must retain their assertions. Parent integration handoff owns actual status, not this table.

| ID / mode | Work | Dependencies / completion evidence |
| --- | --- | --- |
| IG00 / VERIFY | Record both source refs, merge base, changed-path overlaps and dirty work | Preserve original checkouts, branches, LFS and user saves. New integration branch/checkout only; record exactly what entered the merge. |
| IG01 / FIX | Compile combined runtime/editor source and fix demonstrated errors | IG00. Reproduce each compiler diagnostic, make the smallest compatible correction, record files and verify zero compiler errors. Do not hide errors by removing assemblies/features. |
| IG02 / VERIFY | Assert world identity and serialized references after merge/import | IG01. Correct scene, protected Terrain/Collider refs,813 trees/15 prototypes, original terrain/paint/details preserved, map arrays and persistent IDs retained. No prefab/world regeneration. |
| IG03 / VERIFY | Run existing offline quest/save and independent spawn regressions on combined source | IG01–02. Preserve all145 prior assertions plus the new32-case TerrainCollider regression; record actual counts, source and isolated save paths. Ground checks must independently sample real colliders across tiles/entry paths, not reuse the calculation under test. Compiler success is not quest acceptance. |
| IG04 / VERIFY | Run Gemini gameplay checks against the combined scene | IG01–03. Record the actual41-check result or new failures; ensure current interactions, mounted state, ability guards and modal ownership still agree. Never edit expected values merely to pass. |
| IG05 / VERIFY | Exercise repaired routes with combined movement/mount code | IG02–04. Both directions, walking/riding, departure/gate/bridge/cache/north arrival; distinguish geometry, queued keyboard and ordinary play. Preserve encounter-limited mounted Reaver failure until intended combat behavior is tested. |
| IG07 / VERIFY | Produce unique combined candidate standalone and startup smoke | IG01–05. Build before online executable tests; do not overwrite old builds. Record source SHA, result/warnings, exact files/hashes; title→selection→world with isolated character. Build folder includes data/DLLs. Seal delivery only afterIG06 passes against this exact candidate; a changed binary requires its dependent checks again. |
| IG06 / VERIFY | Re-run dependent headless online authority/session cases | IG03–05 plus the exactIG07 candidate executable. Host/join/leave, invalid code, combat/rewards, late join/events, revive/disconnect;4-player/5th rejection as relevant. Extend acceptance with independent TerrainCollider-based grounded host/client entry, respawn and rejoin assertions; old online smokes missed the multi-tile spawn defect. Record candidate identity and keep join codes/tokens out of committed logs. |
| IG08 / VERIFY | Run ordinary two-PC cross-network acceptance | IG06 andIG07. Same sealed candidate on both PCs, different network connections; follow section10. Record real results and exact blockers; same-machine Relay is not this gate. |
| IG09 / FIX | Triage user defects and freeze a known-good baseline | IG07–08. Each defect has build, reproduction, expected/actual, severity and next check. Fix P0 before expansion. Publish handoff/remaining NOT RUN; no automatic main merge. |

## 3. Polish the complete first adventure — PQ00–PQ11

Default P1. Build on current systems; this is verification and focused repair, not a framework rewrite.

| ID / mode / owner | Work | Dependency and done condition |
| --- | --- | --- |
| PQ00 / VERIFY / Gameplay | Measure a fresh-player20–30-minute adventure | IG07. No debug help: title→Abe→combat/reward→Highspire/Westbank→Reaver/Greenhollow→return. Measure actual time/confusion/deaths; do not pad HP or travel to meet a duration. |
| PQ01 / FIX / Gameplay | Resolve first-adventure journal/marker inconsistencies | PQ00. Current objective, giver and turn-in agree on map/journal; accept/abandon/reload/complete cannot leave stale or duplicate rewards. |
| PQ02 / VERIFY / Gameplay | Test modal/input ownership and cursor recovery | IG04. Dialogue/shop/inventory/map/chat/settings/party: movement/casts do not leak through, Esc/Back works, cursor returns correctly after death and leave/rejoin. |
| PQ03 / VERIFY / Gameplay | Review grounded locomotion in real rendered play | IG05. Forward/back/strafe/diagonals/turns/jump/landing/attack; slow walk/run transitions local/remote. Record race/class/clip and footage before another speed tweak. |
| PQ04 / FIX / Gameplay+Art | Resolve the diagnosed run/foot twist through the approved asset pass | PQ03,AR00–03. Keep actor facing and clip root motion coherent, preserve feet/contact, validate local and remote actions; no broad avatar/import replacement. |
| PQ05 / VERIFY / Gameplay | Audit mount summon/travel/dismount/collision | IG05. Cancel/move/death/combat/dialogue/steep slope/bridge/respawn, horse and legitimately unlocked zebra; remote rider/mount attachments and camera acceptable. |
| PQ06 / FIX / Gameplay | Tune melee/caster combat readability | IG04,PQ03. Facing/range/windup/impact and resource/cooldown feedback agree. Distinguish miss/blocked/out-of-range; no damage duplication or unrestricted client casts. |
| PQ07 / VERIFY / Gameplay | Complete enemy reset/leash/LOS/death review | PQ06. Existing melee/ranged/caster roles chase intended players, return home, stop attacks when dead and recover from unreachable targets without blocking mandatory travel. |
| PQ08 / VERIFY / Gameplay | Play through existing bosses visually | IG04,PQ06–07. Telegraphs/phases/death-cast guards/wipe/reset/rewards local and remote; verify actual encounter, not component existence. |
| PQ09 / VERIFY / Gameplay | Exercise existing economy/equipment loop | IG03–04. Earn→inspect→buy/equip/use→save/reload; insufficient funds/full inventory/duplicate/rejected RPC preserve state and explain failure. |
| PQ10 / VERIFY / Gameplay | Listen to existing footsteps/combat/ambience | IG07. Walk/run/landing, melee/spell/hit/death/UI and town/forest/river at controlled volumes; identify duplicate/missing cues and loudness problems with exact actions. |
| PQ11 / FIX / UI | Refine startup/tutorial/HUD clutter from captured defects | PQ00–02.1280x720 plus common wider/higher resolution; no overlapping startup notifications, clipped session buttons or permanent debug panels. Reuse current title/selection/HUD. |

## 4. Art, rigs and asset production — AR00–AR07

P1 for current defects; P2 for replacement/expansion. Free licensed assets only. Asset-owner scope must be explicit before importing/replacing scenes, rigs or prefabs.

| ID / mode / owner | Work | Dependency and done condition |
| --- | --- | --- |
| AR00 / AUDIT / Art | Build supported character/rig/action inventory | IG07. Per saved race/class visual: source/license, rig/avatar, base facing, scale/materials, walk/run/back/attack/cast/hit/death/mounted coverage. Existing prototype count is not launch support. |
| AR01 / FIX / Art | Prepare isolated Jog/Sprint root-rotation tests | AR00,PQ03. Compare current import with one reviewed candidate in a protected copy; record affected clips, reference pose/root bake and foot trajectory. No silent global metadata change. |
| AR02 / VERIFY / Art | Validate retargeting and held-equipment alignment | AR01. Knees/feet/hips/weapon hands, scale and facing acceptable across target rigs; action loops and cast/attack completion remain correct. Preserve source GUIDs/references. |
| AR03 / FIX / Art+Gameplay | Apply only the accepted locomotion asset correction | AR01–02. Runtime stride/speed assumptions recalibrated if needed, local/remote regression and matched native captures; old saves still load correct visuals. |
| AR04 / ADD / Art | Publish environment/character style sheet and scale guide | AR00,WR00. Cohesive timber/stone/foliage/skin/palette, real reference sizes, texel density and roughness; four native reference views, not a broad remodel request. |
| AR05 / AUDIT / Art | Shortlist one coherent free model family before replacements | AR04,roster decisionCQ00. License/provenance, matching hands/faces/equipment, rig/action proof and performance budget. Reject inconsistent tiny/high-detail imports before admission. |
| AR06 / ADD / Art | Finish missing ability icons/VFX using current visual language | CQ02,UX02. Per-ability meaning, transparent/contrast-safe icons, atlas/import settings and license ledger; do not add effects that obscure telegraphs. |
| AR07 / VERIFY / Art | Gate any new model/mount/prop admission | AR04–05. Correct axes/metres, pivots/colliders/LOD/materials/textures, render cost, animation proof and receipt. No WoW-ripped assets or paid-provider jobs. |

## 5. World production and real expansion — WR00–WR11

P1 for safe current routes/presentation; P2 after baseline. World owner is sole scene/terrain/environment writer in a scheduled window.

| ID / mode | Work | Dependency and done condition |
| --- | --- | --- |
| WR00 / AUDIT | Reconcile current world inventory and regional purpose | IG02,IG05. Exact bounds/tiles/regions/services/routes/encounters; distinguish terrain paint, surface paths and runtime map. Keep authored architecture and IDs. |
| WR01 / VERIFY | Finish route/camera/obstacle acceptance | IG05. Emberwatch→Highspire→Westbank→Reaver and Emberwatch→Greenhollow, outbound/return, walking/mount, bridge edges/gates/approaches. Capture real camera foliage/wall/rail issues. |
| WR02 / AUDIT | Complete individual26-tree/prop disposition evidence | WR01. Identify each candidate, visible support/intended lean/log status/collision/sightline; inspect native views. No mass vertical rotation, removal or unsupported all-clear from stored root offsets. |
| WR03 / ADD | Finish the reviewed Greenhollow work vignette | WR00–02. Recover the separate three-prop survey only after hash review; native before/after, marker/mount/service aprons, support/collision and route regression before save. No new tile or copied dirty scene. |
| WR04 / ADD | Refine Emberwatch and Highspire purpose/readability | WR01,AR04. One bounded frontage/activity cluster at a time; homes/work/storage/services and departure visually coherent; well/gate architecture retained and useful empty travel space protected. |
| WR05 / ADD | Complete Willow/Westbank/Reaver staging | WR01–02. Coherent riverbank/rest/supply/camp-edge language; keep cache approach, bank grade, combat retreat and bridge support visible. New plants/rocks cannot hide known defects. |
| WR06 / ADD | Define biome/settlement transitions across existing regions | WR00,AR04. Sparse authored regional palettes and landmarks for Frostpeak/Silverglen/Deathknell/Rustwater/Bloodrock; inventory actual clearings/workflows before more props. |
| WR07 / FIX | Establish measured lighting/fog/shadow quality tiers | IG07,UX06. Same cameras/settings/time; sun and night readability, fog depth, shader/billboard warnings investigated. Separate editor gizmos from actual rendered light meshes; no stacked effects without cost evidence. |
| WR08 / ADD | Prepare three regional arc placement contracts | CQ05,WR00–06. Each location has approach, services, encounter pockets, safe rest/retreat, clue sightlines, return and owned actor/quest IDs agreed with gameplay. |
| WR09 / AUDIT | Preview Stormglass Reach as actual adjacent land | WR08,UX06. Design-only x[64,448],z[608,992] parcel; two existing-map approaches, edge preservation/seams/heights/vegetation/content/performance reviewed. Separate approval before any tile admission. |
| WR10 / ADD | Build one approved expansion destination end-to-end | WR09,CQ05,SE02. Landing→quarry problem→beacon climax→useful reward/return; late join/claim/reset rules and foot/mount/camera acceptance. No second empty expansion before first loop is complete. |
| WR11 / ADD | Improve read-only placement/content diagnostics | IG02,WR00–02. Uncapped accurate totals, bounded examples, stable IDs/targets/ground support/road/service margins and explicit coverage limits. Tools never automatically mutate the world. |

## 6. Character, combat, campaign and dungeon — CQ00–CQ11

P1 audit/balance for current advertised content; P2 new compact-RPG slices. Final creative roster decisions precede restricted creation or new identity systems.

| ID / mode / owner | Work | Dependency and done condition |
| --- | --- | --- |
| CQ00 / DECIDE / Design+Art | Finalize two original factions and supported launch races | AR00,IG09. Propose two races per faction from actual model support; preserve existing enum/save IDs and extra prototypes. Original names/lore; no copyrighted asset copying. |
| CQ01 / AUDIT / Gameplay | Publish four-class kit/balance matrix | PQ06–09. Warrior/Mage/Rogue/Paladin damage/heal/resource/range/cooldown/control roles measured offline/host/client. Record time-to-kill/survival and exact bugs before tuning. |
| CQ02 / FIX / Gameplay | Consolidate ability data where demonstrated drift exists | CQ01. Existing definitions shared by tooltip, action and authority where feasible; preserve IDs. Meaningful invalid/range/resource/cooldown tests, no new framework just for style. |
| CQ03 / ADD / Gameplay+Art | Deliver fifth class through one complete vertical slice | CQ00–02,AR07. Role/name decision first; creation/preview/action kit/gear/save/mount/death/remote presentation/authority tested before exposure. Ranger is a proposal, not already chosen or implemented. |
| CQ04 / ADD / Design+Gameplay | Produce comparable faction introductions | CQ00,PQ00,WR08. Each has a coherent start/problem/resolution/service/departure, solo completion, saved eligibility and cross-faction friendly co-op policy. Avoid duplicated filler quests. |
| CQ05 / ADD / Gameplay+World | Deliver three regional story arcs sequentially | PQ00,CQ04,WR08. Beginning→problem→climax→reward/return, one optional branch; stable objective/actor IDs and useful gear. Review counts after playtest instead of adding25 quests at once. |
| CQ06 / ADD / Gameplay | Add one missing objective type through full failure paths | CQ05. Investigate/interact/escort/defend chosen from existing framework audit; abandon/reaccept/full inventory/death/leave/late join/reset all specified. |
| CQ07 / ADD / Gameplay | Establish distinct enemy roles and one elite variant | PQ07,CQ01. Melee/ranged/caster counterplay, correct facing/telegraphs/leash/LOS; host authority and fair solo/group scaling. New models only throughAR07. |
| CQ08 / ADD / Gameplay+World | Complete one existing dungeon, not another portal shell | PQ08,WR08,SE03. Entrance/arrival/combat/rest/boss/reward/exit, solo/group admission, late join/wipe/reset/fall recovery and return route tested. Audit retained interior before replacing geometry. |
| CQ09 / ADD / Gameplay | Introduce one meaningful talent tier | CQ01–03,SE00. Existing framework first; clear choices, reset/refund policy, saved IDs/authority and balance. Full subclass trees deferred until stable base roster. |
| CQ10 / FIX / Design+Gameplay | Tune compact-level progression and rewards | PQ00,CQ01,CQ05,SE01–02. XP/gold/equipment sources/sinks, useful milestones and affordability measured. Do not raise cap without content/gear/enemy rebalance. |
| CQ11 / ADD / Design+Gameplay | Add one replayable activity only after campaign completion | CQ05,CQ08,CQ10. Explicit eligibility/reset/reward caps and solo/group fairness; modifiers add readable counterplay rather than HP inflation. |

## 7. Systems, economy, persistence and multiplayer extensions — SE00–SE07

P1 reliability/audit for existing systems; P2 professions/events; P3 backend/social expansion. A source file or menu does not establish a working feature.

| ID / mode / owner | Work | Dependency and done condition |
| --- | --- | --- |
| SE00 / VERIFY / Gameplay | Audit offline save recovery and migrations | IG03. Slot isolation, missing/corrupt/old-save behavior, recoverable writes and stable IDs tested with disposable copies; never modify real characters to test failure. |
| SE01 / AUDIT / Gameplay | Validate item, loot, vendor and equipment definitions | PQ09. Unique IDs, stack/stat/ownership rules, reward eligibility and inspect/equip/use feedback. Unknown item and inventory-full rejection are useful and atomic. |
| SE02 / ADD / Gameplay+World | Deliver one gathering→crafting→useful item loop | SE01,CQ10,WR08. Audit current profession prototypes; node ownership/depletion/reset, ingredient/currency validation, recipe unlock, interruption/full inventory/repeat requests and save policy. |
| SE03 / DECIDE / Multiplayer | Define group encounter/dungeon admission and rewards | IG08,PQ08. Shared-world versus instance contract, late-join/boss progress, wipe/reset/return, disconnected-player eligibility; no accidental duplicate scenes or payouts. |
| SE04 / ADD / Gameplay+World | Add one regional event beyond proven Caravan/Ambush | IG06,WR08,CQ06. Host stage/participants/clues/actors/success/failure/cooldown and late-join snapshot; cleanup/reset/rewards tested before adding more templates. |
| SE05 / VERIFY / Gameplay | Refine opt-in companion/pet scope | IG04,PQ02. Empty solo default, call/dismiss/recall/ownership/death/leave cleanup and collision/path budgets. Party menu stays minimal; do not claim networked companion AI if only local visuals exist. |
| SE06 / DECIDE / Architecture | Design durable online progress separately | IG08,SE00–01. Explicit trusted storage/auth/authority/migration/threat/cost/recovery proposal. Current online is session-only; no trusted client-file upload shortcut, new billing/backend or host migration without scope approval. |
| SE07 / DECIDE / Design+Architecture | Gate reputation/trade/guild/mail/auction/PvP ideas | CQ10,SE06. Audit prototypes; approve only a supportable feature with transaction/abuse/persistence policy. Public servers/global economy/competitive PvP are P3 product decisions, not implied by private co-op. |

## 8. UI, sound, accessibility and performance — UX00–UX07

P1 where needed for advertised play; P2 expanded settings/localization. Reuse existing systems and measure user-facing results.

| ID / mode / owner | Work | Dependency and done condition |
| --- | --- | --- |
| UX00 / VERIFY / UI | Test full title/selection/session/world flow visually | IG07,PQ02. Rotating backgrounds and faction preview scale/facing/grounding, retained choices, invalid names/missing models and1280x720 layout; preserve character slots. |
| UX01 / FIX / UI | Resolve concrete HUD/notification hierarchy defects | PQ11. One primary objective, one contextual interaction, temporary combat/party information; solo frames hidden, joined roster accurate, achievement/weather popups non-overlapping. |
| UX02 / ADD / UI+Art | Finish coherent action icons and ability explanations | AR06,CQ02. Correct key/cost/cooldown/target and disabled-state reason; readable at native sizes, contrast-safe, same local/server rule. |
| UX03 / ADD / UI | Complete unobtrusive onboarding and control reference | PQ00,PQ02,PQ05. Dismissible first-use hints for target/E/slow walk/mount/party; no infinite repeated tutorial or keyboard collision with character/chat windows. |
| UX04 / ADD / UI+Gameplay | Review rebinding, scale and accessibility | UX00–03. Existing input support first; reset conflicts, readable text/UI scale, color-independent danger, subtitles where needed and reduced motion/effect options with persistence. |
| UX05 / ADD / Audio+World | Improve regional soundscape with licensed cues | PQ10,WR07. Town/forest/river music/ambience priorities/crossfades, surface footsteps, voice/text fallback and no doubled combat events. Verify by listening; settings round-trip alone is insufficient. |
| UX06 / VERIFY / Performance | Measure rendered CPU/GPU/network/memory hotspots | IG07–08. Fixed route/camera/settings/hardware/resolution, hubs/foliage/combat/effects/four players, frame-time percentiles and allocations. Headless Relay timings are not rendered FPS. Optimize only demonstrated hotspots and compare same scenario. |
| UX07 / ADD / Content+UI | Prepare localization and content wording | CQ00,CQ04–05,UX00. English/Bulgarian strategy, fonts/layout/line lengths and consistent original terminology. Display text changes cannot rename saved/network IDs. |

## 9. Release operations, test coverage and diploma — RD00–RD09

P1 delivery gates, with repeated checks only after relevant changes or unresolved failures.

| ID / mode / owner | Work | Dependency and done condition |
| --- | --- | --- |
| RD00 / ADD / Integration | Maintain an honest implementation matrix | IG09. Per advertised feature: offline/host/client/late join/save/failure/UI-art evidence and NOT RUN; clear source/build identity. Old historical failure snapshots stay labelled. |
| RD01 / VERIFY / Integration | Test fresh clone/LFS/vendor reproducibility | IG07. Correct Unity version, package lock, admitted licenses/Adobe bootstrap, no missing meshes/textures/references. No Library/cache dependence or speculative package upgrade. |
| RD02 / ADD / QA | Maintain a small high-value regression matrix | IG03–06. Retain the independent32-case native spawn regression and extend grounded entry/respawn/rejoin checks into ongoing online acceptance. Compiler/quest/save/authority/route/input failure tests plus reproducible manual cases use isolated saves. Never replace actual collider support with the same helper being tested or synthetic constants. |
| RD03 / VERIFY / QA | Exercise latency/loading/re-entry/failure bounds | IG08. Bounded delay/loss/loading and several session cycles, no stale manager/camera/listener/subscriptions/rewards. No unbounded overnight retry/soak loop. |
| RD04 / ADD / Art+Integration | Complete license/credits/provenance ledger | AR07,UX05. Every admitted model/texture/icon/audio/font source and redistribution rights recorded; packaged credits accurate. Free-only rule maintained. |
| RD05 / ADD / Integration | Package a release candidate and testing guide | IG09,RD01–04. Unique complete build directory, source SHA/known limitations/controls/host-join/offline fallback; publish only approved artifacts, no credentials or live join codes. |
| RD06 / VERIFY / QA+User | Run advertised-feature acceptance on release candidate | PQ00,RD05. Full first adventure offline and actual two-PC session, save/reload, optional routes/dungeon as advertised. Record failures without declaring every backlog feature complete. |
| RD07 / ADD / Diploma | Produce architecture and original-contribution evidence | RD00–06. Explain Unity/URP/Input/NGO/Relay, host authority, state/IDs/saves, terrain preservation, asset workflow and AI-assisted development with attributable human decisions. Use actual diagrams/code/test captures. |
| RD08 / ADD / Diploma | Prepare demonstration and failure recovery | RD05–07. Rehearsed playable route, clear role split, offline fallback, backup known-good build, screenshots/video and concise limitations; no claim of MMO/durable online progression. |
| RD09 / DECIDE / User+Integration | Approve main merge/release and next milestone | RD06–08. Review code/data/build/results, retain old branches/tags/backups. Main merge/publication is a separate controlled final step, not an automatic consequence of this plan. |

## 10. Two-PC manual acceptance guide

Use IG07's complete combined build folder on both PCs. Confirm the same source/build identity; do not mix CodexWorldProduction02 and ProductionGeminiGameplay. Use disposable test characters. Preserve normal offline saves. If possible, use a mobile hotspot for the second PC so the connections are genuinely distinct.

1. Host selects **Host Online**, creates the session and shares the displayed code privately. Client selects **Join Online** and enters it. Check actual grounded/usable entry on both PCs, with no hovering, falling through terrain or spawn inside geometry. Record time to usable spawn and any exact error; do not commit the code/token.
2. Check two visible players, correct names/race/class/facing, compact party HP, one controllable local camera and one audible listener per client. Open/close menus, dialogue and chat; no movement/input leakage.
3. Accept/complete Abe's initial objective as eligible players. Both fight: verify client damage, class costs/cooldowns, melee/cast visuals and enemy targeting. Try an invalid/out-of-range action and check useful rejection without lost valid resources.
4. Earn and claim loot/rewards once each. Buy/use a potion and equip one item; repeat a claimed chest/turn-in. Verify no duplicate reward and no client-only gold or inventory desynchronization.
5. Run and mount through Emberwatch→Highspire north gate→Westbank both ways, approach/open Willow Rest cache, then Reaver trail. Check bridge edges/banks/supports, camera/foliage, body/feet/facing and retreat. Combat-triggered dismount may be intended; distinguish it from a terrain block.
6. Travel to Greenhollow, find the preserved discoveries and legitimately unlock the zebra. Verify remote mount/rider model, summon cancellation and service/marker access. Do not use debug-unlock as acceptance.
7. ExerciseF6 ping,F7 ready/decline/timeout,E nearby revive andR death respawn. Late join during active Caravan/Ambush sees current stage, not historical rewards. Use exact current controls if the integrated build changes them; report the mismatch.
8. Client leaves, returns to menu and rejoins; host remains usable. Host then leaves: client receives session-ended state and can return safely. Online progress is session-only. Confirm ordinary offline character data is still intact afterward.

If four testers are available, verify four players and a fifth rejection on this exact build. Otherwise mark four-PC acceptance NOT RUN even if the headless four-process scenario passed. Record each case as PASS/FAIL/NOT RUN with build, PC/network distinction, action, expected/actual and screenshot/video where useful. Do not call this test complete solely because joining worked.

## 11. Near-term ownership and queue

| Order | Primary owner | Next bounded work | Hold until |
| --- | --- | --- | --- |
| 1 | Integration owner | IG00–05→IG07 candidate→IG06 online checks→sealed delivery | Actual compilation, regression and exact-candidate evidence; do not rebuild unchanged green tests for activity |
| 2 | User+QA | IG08/PQ00 ordinary play with defect list | Same complete build, two PCs where available |
| 3 | Gameplay owner | PQ01–03/PQ05–11 based on reproducible defects | Explicit runtime ownership; preserve world/source rigs |
| 4 | Art owner | AR00–03 locomotion source fix | Captured defect, protected candidate import and accepted scope |
| 5 | World owner | WR01–03 and one reviewed settlement cluster | Scene ownership; concrete preview/clearances and necessary approval |
| 6 | Design+Gameplay+World | CQ00–05,SE02 andCQ08 one slice at a time | Polished baseline; roster/quest/placement/persistence contracts |
| 7 | Integration+QA | RD00–09 release/diploma | Actual advertised feature acceptance, licenses and reproducibility |

Suggested next gameplay prompt:

```text
Continue Riftbound on the named combined integration baseline, reading AGENTS.md, the latest integration handoff and Docs/Integration/POST_INTEGRATION_PLAN.md. Preserve finished branches/builds. Take the highest-severity reproduced PQ defect first, then one eligible gameplay packet. Own runtime only; coordinate scene/terrain/prefab and source-animation changes before editing them. Do not rebuild online foundations or advertise pending GUI/two-PC checks as passed. Use isolated saves, retain authority/IDs/rewards, compile and run changed-behavior regression, then hand off a concrete checkpoint. Maximum two unchanged failures; ten minutes without evidence changes hypothesis. No main merge or paid service.
```

Suggested next world prompt:

```text
Continue Riftbound's world work from the verified combined baseline. Read AGENTS.md, integration handoff, CODEX_WORLD_AUDIT.md and Docs/Integration/POST_INTEGRATION_PLAN.md. Start WR01–WR03: actual route/camera and individual vegetation evidence, then the small Greenhollow feed-storage preview if its separate WIP source is reviewed. Preserve813 trees/15 prototypes, paint/details, original terrain/assets, persistent IDs and repaired bridge/cache. No mass cleanup or regeneration. Sole scene owner for an explicitly coordinated window; publish exact placements/native views before broader changes. Do not edit gameplay/networking or silently import dirty files. Verify focused changes and record actual results/NOT RUN/release.
```

Suggested asset-animation prompt:

```text
Read the current integration handoff, AR00–AR03 and Gemini's GP01 diagnostic evidence. Audit the supported rig/action matrix and reproduce the actual run/foot-twist defect. Prepare a protected import/retargeting candidate for the affected Jog_Fwd_Loop/Sprint_Loop clips; compare native captures and root/foot motion against the current baseline. Do not globally reimport/replace avatars, delete prototype races or overwrite source assets without the specific approved candidate scope. Validate local/remote movement, attacks/casts/death/mount attachment and saved visual identity, then hand off exact asset/settings/source hashes and remaining limits.
```

These are future task prompts, not running jobs or an eight-hour timer. Continue eligible work within current scope, but do not spend tokens by rerunning green tests, expanding menus, generating empty terrain or repeatedly restarting Unity. Keep final-quarter time for validation/build/handoff when a timed shift is explicitly requested.

## 12. Scope, contracts and exit conditions

- Integration is authorized now; broader world/asset/backend changes still require their own concrete scope. One scene/prefab owner at a time. Never overwrite original terrain, scenes, model sources or user saves casually.
- Preserve GUIDs, serialized enum/content IDs, actor/chest/quest ownership and reward rules. New IDs are definitions, not renamed display strings. No force-push/reset of other workers or auto-merge main.
- Any quest/event/dungeon packet shared between world/gameplay must state location/approach/clearance, stable IDs/stages, eligibility, fail/reset/reward, offline persistence, online session state/late join and exact files/owner before implementation.
- Audit existing systems before adding alternatives. Guild/auction/profession/dungeon scripts can be prototypes; a source file or UI panel is not proof of a complete feature.
- Stop after two unchanged failed attempts; ten minutes without new evidence requires another hypothesis/independent work. Carry failed-attempt counts across sessions and cap unchanged-blocker investigation at twenty minutes. Never weaken tests or fabricate graphical/gameplay passes.
- Success for the next delivery is a reproducible combined build and a complete playable reference adventure, not completion of all80 packets at once. P2/P3 are the larger roadmap and have explicit dependencies.

Cross-reference the existing master/backlog epics and GP/CW/EX IDs rather than deleting them: IG→FND/ARC/AUTH/QA/SHIP; PQ→MOVE/ANIM/CAMERA/FIGHT/QUEST; AR→ART/ANIM/FX; WR→GROUND/NATURE/REGION/TOWN/TOOL; CQ→HERO/FACTION/CLASS/STORY/ENEMY/BOSS/CRYPT; SE→SAVE/EQUIP/LOOT/GOLD/CRAFT/EVENTS/ALLY/TRUST; UX→UI/ACCESS/LIGHT/SOUND/PERF; RD→QA/SHIP/DIPLOMA.
