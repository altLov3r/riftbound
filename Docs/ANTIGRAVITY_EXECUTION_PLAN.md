# Antigravity execution plan — world polish and multiplayer

> NEW DETAILED FEATURE PLAN: **Docs/RIFTBOUND_FEATURE_BACKLOG.md**. Read with RIFTBOUND_MASTER_PLAN.md; execute gated work packets, preserve anti-loop/file-ownership rules and finish online acceptance before unbounded expansion.

> COMPLETE MASTER ROADMAP: **Docs/RIFTBOUND_MASTER_PLAN.md** maps the full game, dependencies, release layers and acceptance gates. Use it for overall scope; execute the online multiplayer work order first, then the master plan's remaining milestones. This older document remains a detailed historical backlog.

> LATEST USER DIRECTIVE: read **Docs/ANTIGRAVITY_ONLINE_MULTIPLAYER_WORK_ORDER.md** first. Implement internet co-op through Unity Multiplayer Services sessions + Relay join codes. That work order supersedes this document's LAN-first scope and older execution order. Preserve the anti-loop rules and remaining world/content backlog.

Prepared: 2026-11-15. Multiplayer-first backlog, revision 3. This file describes planned work, not completed implementation.

## ACTIVE DIRECTIVE — complete multiplayer first

The latest user instruction overrides the original phase order: complete usable multiplayer in the real RPG before adding more world content. The older phases remain a backlog, not the immediate execution sequence. Address offline/world defects now only when they block multiplayer or risk data loss.

Read this section before resuming an old network test. Existing Netcode packages, NetworkTest scene and capsule prefab are groundwork; do not recreate them each session. A host spawning its own capsule is not evidence of working co-op.

### Mandatory rule: stop repetitive work

- Before a test, state one hypothesis, the expected observable result and a timeout. Record attempts in Docs/IMPLEMENTATION_STATUS.md with timestamps.
- Run a failing test at most twice without new evidence. A third run requires a material change tied to a documented cause; restarting the editor, renaming the test or increasing its timeout alone does not count.
- If a focused network test has produced no new evidence or state change for 10 minutes, stop that attempt. Cap one unchanged blocker investigation at 20 minutes, then write a blocker report. These limits govern active work, not a claim that an already-running external agent can be interrupted by this file.
- Choose bounded timeouts before launching: initially 60 seconds for local host/client connection and 120 seconds for a focused Play Mode check. For import/build work use a separately recorded timeout justified by project size and actual progress. Poll in short intervals and report meaningful progress; never wait indefinitely.
- Stop only test processes launched by this attempt, using their recorded process IDs. Do not kill the user's Unity editor or unrelated Unity/build processes. If an existing editor prevents automation, record the condition and choose a safe alternative.
- On timeout, collect the relevant console errors, exception/call stack and last reached test stage. Do not dump or rescan generated log folders.
- After reaching the limit, mark the ticket BLOCKED with hypothesis, attempts, result, missing evidence and the smallest next diagnostic. Move to a genuinely independent task such as input ownership, disconnect cleanup, session UI or test documentation.
- Do not skip a failed gate, claim it passed or integrate unverified dependent systems. Stop the whole work session only when no safe independent work remains or user input is actually required; ask one precise question with the blocker evidence.
- Keep retry counts across context resets and handoffs. A new agent session does not reset the budget for the same unchanged failure.

### Immediate audit findings to resolve

These were observed during source/result-file review; recheck current files before changing them.

- LivingWorldEvents.IsAuthoritative always returns true. Replace the placeholder with the actual offline/host authority boundary before replicating events.
- RiftboundNetworkPlayer moves only its owner locally, while setup adds a plain NetworkTransform. Verify transform authority in the installed package and serialized prefab: select a consistent input-to-host/validated-movement design rather than assuming client transforms reach the host.
- LAN host binding uses 0.0.0.0 as connection data. Check the installed transport API's distinction between server address and listen address; verify localhost host/client and then LAN reachability separately.
- LAN UI says 2–4 players, but enforce capacity in connection approval and handle refusal clearly; button text is not a limit.
- V currently appears in both dodge and mount-cycle input. Resolve the binding conflict and update controls/tooltips.
- Event completion grants XP/gold after each cooldown. Specify repeatable-event rewards versus one-time character rewards and implement that policy explicitly.
- The inspected smoke result had 77 PASS lines followed by a Reaver terrain failure, despite the status note saying 82. Correct the evidence record; do not alter assertions simply to get a green result.
- The recorded package/scene setup results exist, but the expected host-verification result was absent at review time. Treat host/client runtime verification as pending.

### Multiplayer-first execution queue

1. **MP-01: reproduce once.** Inspect current session bootstrap, prefab registration, transport settings and last reached verification stage. Compile, then make one bounded two-process connection attempt. Identify the concrete failure before changing architecture.
2. **MP-02: prove real client ownership.** Host and a separate client process both spawn; each owns exactly one avatar and controls only it. Observe movement in both directions. Check spawn separation, gravity, facing and disconnect cleanup. Capsule-only testing ends once this gate passes.
3. **MP-03: enter the RPG.** Wire Offline / Host / Join into FrontendMenu, transition into Outpost-Expanded safely, and use selected character visuals. Keep local camera, input, HUD and audio listener ownership explicit. Preserve the offline path.
4. **MP-04: authoritative combat.** Port one enemy and existing player abilities. Host validates action ownership, range, cooldowns, resources and damage; replicate health/death/animation. Test two players attacking the same enemy and either player disconnecting while targeted.
5. **MP-05: cooperative progression.** Port one Abe quest, one chest, inventory transactions and per-player reward claims. Then port existing content through the proven path. Prevent duplicate rewards from retries and simultaneous requests.
6. **MP-06: shared world.** Host owns enemies, NPC routines, events and interactables. Late joiners receive current state. Preserve mount unlocks, opt-in companions, dungeon travel and player-specific objectives. Add host-validated respawn.
7. **MP-07: session hardening.** Enforce four-player capacity, build compatibility, useful connection errors, host-exit behavior and repeatable return-to-menu/rehost. Test 2 players first, then 3–4, plus latency/disconnect cases.
8. **MP-08: delivery.** Produce a fresh standalone, exact host/join instructions, observed two-process results and a known-limitations list. Distinguish same-PC, LAN and internet tests. Do not call internet multiplayer complete without testing the chosen internet path.

Gate for saying multiplayer is complete: two separate processes play the real RPG with correct movement, combat, quests, rewards, mounts, death/respawn and session cleanup; four-player capacity and late join are tested; offline regression passes. If any part is missing, report the exact incomplete feature. Internet support and persistent cross-session client progression must be labeled explicitly according to what was actually delivered.

Do not require every world-polish ticket to pass before MP-01. Do not bypass a terrain failure when it affects spawn safety, travel or combat. Keep unrelated terrain repair tracked for the world-polish pass.

### Additional ideas after core co-op works

Implement in this order, reusing existing systems. Each item must use the same host authority and local UI ownership rules.

| Idea | Small first implementation | Completion check |
| --- | --- | --- |
| Party frames | Compact teammate name, HP and distance; hide when solo. | Only connected teammates appear; disconnect removes the row. |
| Team map pings | Temporary ping from a map click, rate-limited and host-relayed. | Everyone sees one ping; it expires and cannot flood messages. |
| Downed ally revival | Short interrupted revive interaction before normal respawn. | Two helpers cannot revive twice; damage/departure cancels safely. |
| Ready check | Host requests a ready check before entering a dungeon. | Clear timeout and cancellation; late join does not deadlock entry. |
| Shared quest offers | Offer eligible nearby teammates the quest without auto-accepting. | Decline works; completed/ineligible quests cannot duplicate rewards. |
| Cooperative objectives | Two switches or a defend-the-wagon objective with solo fallback. | Works with 1–4 players; disconnect cannot permanently lock it. |
| Encounter scaling | Bounded health/add count based on eligible participants. | No free healing or repeated rewards when players join/leave mid-fight. |
| Group dungeon | One existing dungeon using shared encounter/reset state. | Wipe, retry, rewards and exit all work for the whole party. |
| Session diagnostics | Development-only connection/ownership display. | Hidden in release HUD; no credentials or unnecessary personal data. |
| Internet join code | Evaluate a compatible service only after LAN delivery. | Account/quota terms documented; test from separate networks before claiming support. |

Defer trading, auction houses, ranked PvP, guilds, persistent public servers and host migration until session identity and progression are reliable. Never substitute these extras for finishing MP-01 through MP-08.

## Goal and scope

Improve Riftbound's existing world, repair incorrectly fallen trees, make settlements and roads feel inhabited, and add playable multiplayer.

Default multiplayer scope: 2–4 player cooperative host/client sessions, LAN first, with offline single-player retained. This is a proposed implementation scope, not a claim that multiplayer already exists. The latest user request includes multiplayer and supersedes the older exclusion in TASKS.md.

Prioritize a complete, stable gameplay route over adding more unfinished systems. Keep the existing stylized, WoW-inspired art direction. Use free, licensed assets; do not use ripped Warcraft assets. Character-model replacement remains paused.

## Rules for Antigravity

- Inspect current files before editing; handoffs describe historical states and may be stale.
- Preserve all uncommitted work, scene references, prefab references and .meta GUIDs. Do not reset, pull over changes or regenerate the entire world.
- Do not delete or overwrite scenes, prefabs or assets without explicit approval. Prefer additive components and narrowly targeted edits; request approval if a scene/asset replacement is necessary.
- Do not scan Library, Temp, Logs or generated builds. Use concise Unity console errors and relevant source files.
- Work one phase at a time. Compile after code changes and validate each phase before expanding it.
- If the same fix fails twice, gather new evidence before changing it again.
- Do not install several networking stacks, add paid services or embed credentials. Verify package compatibility before proposing/installing networking dependencies.
- Keep HUD changes minimal. Do not add permanent panels for every new system.
- At each checkpoint, record files changed, evidence, remaining issues and the next exact action. Clearly separate verified behavior from assumptions.

## Read first and establish current truth

Unity project: this file's parent Docs directory belongs to `Unity Project/My project (1)`.

- Read AGENTS.md, Docs/ANTIGRAVITY_HANDOFF.md, Docs/WORLD_ZONES.md, Docs/VALIDATION.md and relevant sections of TASKS.md.
- Check Docs/PRESENTATION_UPDATE.md, Docs/FRONTIER_UPDATE.md and Docs/REAVER_UPDATE.md before touching those systems.
- Confirm the active scene and Unity version. Current reference: Assets/RPG/Scenes/Outpost-Expanded.unity, Unity 6000.6.2f1, URP.
- Inspect only the code needed for the active phase. Entry points include WorldExpansionSetup, WorldExpansionLayout, WorldArchitectureBuilder, TerrainPolishSetup, WorldRegions, PlayerController, PlayerProgress, EnemyController, FactionQuestSystem and FrontendMenu.
- World-generation startup protections must remain intact. Inspect LiveWorldSync and WoWNPCSetup before using any generation command.
- The latest compiler repair removed a premature namespace closing brace in LootDropSystem.cs. Runtime/editor C# project compilation passed with warnings. This does not prove a fresh Unity build or gameplay pass; older standalone builds may not contain newer changes.
- Earlier inspection found no multiplayer framework; the later Gemini pass installed Netcode for GameObjects 2.14.0. Recheck manifest/lockfile and reuse the installed stack rather than reinstalling it.

## Phase 0 — establish a playable baseline

- [ ] Inspect the working tree and identify current user/Antigravity changes; do not revert them.
- [ ] Refresh Unity, gather relevant compiler errors, and fix blockers with small edits.
- [ ] Verify title screen → character selection → world entry → movement → combat → quest interaction → save/reload.
- [ ] Confirm one active gameplay camera/audio listener and correct input focus.
- [ ] Capture a baseline along one repeatable route: Emberwatch → Westbank → Greenhollow. Record graphics settings, resolution, frame time, memory and visible defects.
- [ ] Inventory existing ambient NPCs, wildlife, weather, audio and events. Reuse working systems before creating replacements.

Gate: fresh Unity compilation succeeds and the baseline route is playable. Record unrelated bugs separately instead of silently expanding scope.

## Phase 1 — fix trees and worldbuilding defects

### Fallen trees

- [ ] Inspect representative bad instances and source prefabs: local up-axis, root/parent transforms, negative or nonuniform scale, scatter rotation, terrain alignment, colliders and Rigidbody gravity.
- [ ] Separate intentionally fallen logs from accidentally sideways living trees. Preserve deliberate environmental storytelling.
- [ ] Correct the actual cause. Do not apply a blanket 90-degree rotation to every tree or align trunks directly to steep terrain normals.
- [ ] For upright trees, keep a consistent vertical trunk, vary yaw/scale within sensible limits, and place the trunk base on terrain.
- [ ] Verify fixes persist after reopening the scene; document affected prefab/instance paths and before/after screenshots.

### Ground, roads and props

- [ ] Audit floating/buried props, backward-facing models, mismatched scale, missing materials, visible editor light gizmos and blocked roads.
- [ ] Inspect terrain joins, abrupt shelves, cliffs, water edges and road approaches. Smooth only problem areas; protect existing manual work.
- [ ] Check routes with the actual player controller and mount. Terrain height sampling alone does not prove accessibility.
- [ ] Check camera clipping, slopes, steps, collisions, NPC grounding and NavMesh continuity after terrain changes.
- [ ] Use consistent ground materials and gradual texture transitions. Keep settlements, paths, forest floors and rock slopes visually distinct.

Gate: no accidental fallen trees, obvious floating props or impassable mandatory routes on the baseline path. Capture the same viewpoints as Phase 0.

## Phase 2 — make zones coherent before expanding further

The documented world is already approximately 768 × 768 m across four terrain tiles, with bounds x[-320,448], z[-160,608]. Verify these against the scene. Improve the existing space before adding more territory.

| Area | Identity and work |
| --- | --- |
| Emberwatch / Ashwood | Readable starter outpost, functioning work areas, clear Abe quest route, safe arrival space. |
| Westbank / Willow Rest | River crossing, roadside rest, reeds/rocks/water details, clear Reaver camp approach. |
| Greenhollow / Ancient Grove | Quieter forest village, farms/gathering areas, wildlife and readable exploration landmarks. |
| Bloodrock / Gor'Kresh | Distinct faction settlement, rougher architecture and patrols, coherent roads and entrances. |
| Wyrmcrest / cathedral / other regions | Preserve access and identity; polish after the first route meets the quality gate. |

- [ ] Build one polished reference area around Emberwatch and its road to Westbank before repeating the approach elsewhere.
- [ ] Give each settlement an entrance, central landmark, practical work space, paths and a reason for its location.
- [ ] Use vegetation clusters based on terrain and water, with open sightlines at roads and quest objectives. Avoid uniform random scatter and excessive clutter.
- [ ] Add small stories through existing props: supplies being unloaded, a repaired bridge, a guarded camp, an abandoned cart.
- [ ] Preserve useful open space and distant views. If a region needs actual expansion, document its new bounds and road connection first; add terrain additively after approval for any affected assets.
- [ ] Preserve Abe's quests, cartography markers, faction hubs, dungeon entrances, mount unlocks and Reaver camp IDs.
- [ ] Preserve the separate Reaver clearing terrain and Scarlet Crypts interior/portal behavior.

Gate: a player can navigate the reference route using terrain, roads and landmarks, with no dense prop walls, obvious biome seams or mandatory jumps over road edges.

## Phase 3 — make the world feel alive

Implement bounded, reusable behavior rather than a large simulation. In offline mode the local simulation owns actors; in multiplayer the host will own them. Design that authority boundary now.

- [ ] Add/reuse a small data-driven activity system: destination points, activity type, wait duration and optional time-of-day window.
- [ ] Populate the reference area with a blacksmith/worker, market or inn visitors, a guard patrol, and a traveller between safe locations.
- [ ] Add wildlife behavior appropriate to each area: grazing, wandering, fleeing nearby players and predators where supported.
- [ ] Keep quest-critical NPCs available. Ambient routines must not strand them, block interactions or allow permanent accidental death.
- [ ] Add two complete event prototypes: a travelling supply caravan and a roadside ambush/rescue. Reuse existing NPC/enemy assets.
- [ ] Give each event explicit start conditions, active state, success/failure, cleanup and cooldown. Use stable IDs; prevent duplicate starts and reward farming.
- [ ] Define interruption behavior: player leaves, combat ends, actor gets stuck, scene reloads. Provide a debug trigger and reproducible setup.
- [ ] Add restrained spatial audio for work, wildlife, water and settlement activity; avoid overlapping loud loops and global playback.
- [ ] Tune lighting, fog and weather for readable combat and cohesive zone mood. Keep night playable and ability effects legible.
- [ ] Add distance budgets, pooling/culling where useful, and staggered AI decisions. Measure the same baseline route after additions.

Gate: activity is visible within a short visit, events can complete/reset without duplicate rewards, and the reference route remains responsive. Set an explicit performance target from the measured baseline and user hardware; do not invent a pass without measurements.

## Phase 4 — multiplayer design and dependency checkpoint

Proposed default: Unity Netcode for GameObjects with Unity Transport, subject to compatibility verification with the actual Unity version and existing project. Use one stack. LAN direct connection first; investigate internet sessions/Relay only after LAN works and account, quota and cost requirements are understood.

- [ ] Record an architecture decision with exact compatible package versions, license/service requirements and a minimal migration plan before changing dependencies.
- [ ] Start in an additive test scene with simple network players. Do not convert the entire production scene at once.
- [ ] Define stable IDs for player sessions, enemies, quest objects, chests and events. Do not use display names, transient instance IDs or client filesystem paths as identity.
- [ ] Audit singleton and FindFirst/FindAny player lookups. EnemyController's single player reference, PartySystem.Instance, PlayerProgress, FrontendMenu and faction/quest UI need explicit ownership.
- [ ] Separate offline/local services from authoritative simulation so offline play still works.

| System | Initial authority and behavior |
| --- | --- |
| Input, camera, HUD, menus, AudioListener | Local player only; remote avatars cannot create/control these. |
| Movement | Host validates movement/state; document prediction/interpolation decisions and test responsiveness. |
| Enemy targets, AI, combat, damage, death, drops | Host; validate range, cooldown, resources and target before applying actions. |
| NPC routines, caravan, events, world interactions | Host; clients observe replicated states. |
| Animation and effects | Driven by replicated movement/action state; avoid duplicate effects and per-frame RPC spam. |
| Character saves | Keep existing offline saves safe; client JSON cannot decide shared world deaths, rewards or loot. |

Gate: two processes can connect in the test scene, each controls only their avatar, and disconnecting does not corrupt offline data.

## Phase 5 — a complete cooperative gameplay slice

- [ ] Add minimal Offline / Host LAN / Join LAN controls and connection status/errors. Character selection remains local; do not imply real account login.
- [ ] Spawn players at safe distinct positions. Disable input/camera/UI/audio ownership on remote avatars.
- [ ] Replicate movement, facing, animation and mount state; verify remote characters do not slide, face backward or duplicate colliders.
- [ ] Convert one enemy type and one encounter first. Implement target selection across connected players and safe cleanup when a target disconnects.
- [ ] Preserve leash, evade, pending-hit cancellation, LOS and load-reset fixes. Host applies death and rewards exactly once.
- [ ] Convert one Abe quest and one chest before porting all content. Grant shared kill credit only to eligible nearby players; track quest progress per player.
- [ ] Use per-player stable claim records for quest objects/chests so the first player cannot consume another player's objective. Host validates each claim and reward.
- [ ] For the first prototype, allow friendly co-op across factions and disable PvP. Faction hostility/PvP requires a later explicit rules decision.
- [ ] Keep companion recruitment opt-in; verify crew does not spawn automatically or duplicate for every client.
- [ ] Define save policy explicitly: offline saves remain intact; isolate prototype multiplayer progression until host validation, persistence and rejoin identity are tested.

Gate: two players can enter the real world, travel, fight, complete the selected quest and claim rewards correctly without stealing each other's controls or objectives.

## Phase 6 — expand multiplayer and harden failure cases

- [ ] Port remaining enemies, quests, interactables, events, mounts and travel only after the cooperative slice passes.
- [ ] Replicate current authoritative world state for late joiners: alive/dead enemies, event stage, NPC activity, doors and relevant world flags.
- [ ] Test 3–4 players, duplicate names, repeated interactions, simultaneous kills/claims and joining during combat/events.
- [ ] Test client disconnect, host exit, failed address, full session and version mismatch. Return to a usable menu with a clear message.
- [ ] No host migration in the first release. On host exit, end the session cleanly. Only promise reconnect/session recovery after implementing and testing it.
- [ ] Check respawn, faction spawn points, zone travel/dungeons, mount transitions and returning to character selection.
- [ ] Test latency and packet loss with available tooling. Track bandwidth, frame time and AI cost; cap simulation and message rates deliberately.
- [ ] Consider internet connection services as a separate optional milestone. LAN success is not proof of internet connectivity.

Gate: a four-player session completes the selected route and survives the documented failure cases without hangs, duplicate actors or save corruption.

## Expanded delivery order and priorities

Revision 3 override: the active MP-01–MP-08 queue above takes priority over the historical milestone order below. M2/M3 are not blanket prerequisites for beginning multiplayer; migrate their required gameplay through the cooperative slice and defer unrelated expansion.

The sections below extend the phases above. They are implementation tickets, not permission to build everything simultaneously. Audit existing functionality first; fix or extend it rather than creating a competing system.

| Milestone | Dependencies | Deliverable |
| --- | --- | --- |
| M0: reliable baseline | Phase 0 | Offline gameplay and reproducible defect list. |
| M1: trustworthy world | M0, Phases 1–2 | Trees, terrain, movement, readable reference route. |
| M2: complete offline adventure | M1, tickets GAME/QUEST/ECON below | A polished 20–30 minute playable loop. |
| M3: living frontier | M1, Phase 3 | Schedules, wildlife, caravan and ambush, ready for host ownership. |
| M4: network foundation | M0, Phase 4 | Two connected processes with correct ownership. |
| M5: cooperative adventure | M2–M4, Phases 5–6 | The same gameplay loop working with 2–4 players. |
| M6: expansion and release | M5 | Distinct zone content, dungeon, polished build and handoff. |

P0 = broken gameplay/data loss/compile blockers. P1 = required for M1–M5. P2 = expansion after the cooperative loop passes. P3 = optional future work. Re-estimate remaining effort after each milestone; this backlog is larger than a guaranteed one-month delivery.

Each ticket must record: priority, dependency, actual affected files, implementation, test procedure, observed result and known limitation. Before a change, state the expected behavior; after it, verify that behavior. Do not mark a task done because the code exists.

## Detailed gameplay tickets

### GAME-01 — movement, animation and facing [P1, M1]

- [ ] Audit all currently playable model/rig combinations and imported clips. Record model-forward direction, avatar mapping, root motion settings and feet grounding per model.
- [ ] Fix direction at a dedicated visual root where appropriate; do not rotate the controller/collider to compensate for a model's import orientation.
- [ ] Check run, slow walk, backward movement, strafe, idle, attack, cast, hit and death. Make locomotion clips loop; one-shot actions must complete and return safely to locomotion.
- [ ] Match controller speed to clip stride and blend parameters. Run is the normal travel mode, with a visible/rebindable slow-walk toggle. Prevent sideways root translation from overriding movement.
- [ ] Prioritize correct import/retargeting before adding procedural foot placement. If foot IK is needed, bound offsets and disable it during jumps, mounts and unsuitable actions.
- [ ] Test slopes, stopping/starting, diagonal movement, target-facing, camera-relative input and low/high frame rates. Verify enemy model direction separately from attack-facing logic.

Acceptance: a 60-second run/walk/combat sequence has no repeated foot flips, sideways drift, floating idle or stuck animation; remote playback must later pass the same sequence.

### GAME-02 — readable combat and varied enemies [P1, M2]

- [ ] Reuse existing classes and abilities. Record damage, resource costs, cooldowns, ranges and interruption rules in existing configuration; avoid a major class rewrite.
- [ ] Create distinct roles using existing compatible models: melee raider, ranged attacker and durable captain. Variation must affect behavior, not only color or health.
- [ ] Add readable windups, attack reach, hit feedback and clear recovery. Telegraphs must match the real damage area and respect terrain/LOS.
- [ ] Prevent attacks through walls, post-death damage, overlapping queued hits after evade, and duplicate death/loot callbacks.
- [ ] Bound pursuit distance and attack slots so several enemies cannot occupy the same point. Ensure ranged enemies reposition without permanently kiting out of reach.
- [ ] Give normal, elite and boss enemies distinct presentation without filling the HUD with labels. Tune affixes already present before adding more.

Acceptance: each role can be fought and escaped; interrupted/dead/evading actors cannot deal stale damage, and encounters remain understandable with four players.

### GAME-03 — death, respawn, mounts and recovery [P1, M2/M5]

- [ ] Define player death state, input restrictions, respawn location and penalty. Avoid item loss for the initial release.
- [ ] Provide a clear respawn action and an unstuck action to a validated safe point, with cooldown and combat restrictions.
- [ ] Validate mounted collision, ground clearance, dismount placement and transitions at doors, water, portals and combat. Keep existing unlock requirements intact.
- [ ] In multiplayer, host validates death/respawn/mount transitions. Only the dead player's controls change; another player cannot trigger their respawn.

Acceptance: dying or getting stuck never requires deleting a save or restarting the editor; no safe-point spawn occurs inside a collider.

## Quest, progression and economy tickets

### QUEST-01 — a connected 20–30 minute adventure [P1, M2]

- [ ] Preserve Abe's existing three-quest sequence and build connective dialogue around it instead of replacing it.
- [ ] Link the starter outpost, Westbank Reavers and Greenhollow exploration through one readable story: supplies are threatened, the road is secured, the frontier is charted.
- [ ] Include combat, exploration and interaction objectives. Show objective counts, map location, reward preview and return-to-giver state.
- [ ] Handle objectives collected before acceptance deliberately; retain existing exploration credit where intended. Stable quest IDs must survive wording changes.
- [ ] Add abandon/reaccept behavior only with explicit progress rules; it must not reset reward claims or create infinite rewards.
- [ ] For co-op, specify sharing eligibility, distance, acceptance requirements and turn-in ownership. A joining player receives their own progress, not a copy of the host's journal.

Acceptance: a new character can finish the route without debug tools; an existing character retains completed quests and cannot claim final rewards twice.

### ECON-01 — useful loot and a small sustainable economy [P1, M2]

- [ ] Inventory existing equipment, consumables, vendor and profession systems. Complete missing behavior before adding currencies or crafting frameworks.
- [ ] Define a small item table with stable IDs, readable icons, rarity, stack limits, equipment restrictions, buy/sell values and source.
- [ ] Provide a meaningful loop: fight/gather → useful drop/material → vendor or simple recipe → equipment/consumable improvement.
- [ ] Keep progression within the current level cap until existing XP/reward tables are balanced. Raising the cap requires updating affected systems together.
- [ ] Test full inventory, partial stacks, insufficient gold, duplicate clicks, cancelled interactions and invalid item IDs. Never remove currency unless the item grant succeeds.
- [ ] In co-op, host validates transactions and loot claims. Retry/repeated requests must not duplicate an item or charge twice; document the transaction identity used.

Acceptance: rewards remain valuable through the reference adventure, and failed transactions leave player inventory and gold consistent.

### ECON-02 — professions and faction reputation [P2, M6]

- [ ] Extend one existing gathering profession and one crafting loop with a few recipes before adding more professions.
- [ ] Place resource nodes according to region ecology; define respawn and per-player/shared claim rules.
- [ ] Add simple faction reputation only if existing systems support it cleanly: visible rank, deterministic sources and one useful unlock. Cross-faction co-op must remain compatible with the initial friendly rules.

Acceptance: recipe costs, node ownership and unlock requirements are explained in-game and persist correctly.

## Zone expansion and encounter tickets [P2 unless stated]

### WORLD-01 — authoring contract [P1, M1]

- [ ] Give each zone a small content record: bounds, entry points, biome palette, landmark, road connections, safe area, encounter areas, audio/weather profile and content budget.
- [ ] Maintain a placement audit with persistent object/prefab references for known tree/prop defects. Any future generator must be previewable and must identify what it will modify.
- [ ] Define routes as connected playable paths. Validate walking, mount traversal, camera clearance and AI access independently; mark deliberately inaccessible scenery.
- [ ] Track asset provenance and texture/material conventions. Reuse atlases/materials where appropriate; avoid a unique high-resolution material for every small prop.

### WORLD-02 — three distinct adventure regions [M6]

1. Starter frontier: clear roads, safe work areas, simple raiders and tutorial interactions.
2. River/forest frontier: caravan, Reaver camp, crossing, gathering and exploration chain.
3. Highland/ruins frontier: stronger mixed enemies, environmental clues and a dungeon approach.

- [ ] Give each region at least one safe hub, one recognizable landmark, one traversal choice, one optional discovery and one complete encounter.
- [ ] Connect regions with physical travel and readable transitions, not only teleports. Add map bounds only when actual new terrain is authored.
- [ ] Distribute encounters using sightlines and approach space. Do not put unrelated settlements, boss arenas and quest objectives on top of one another.
- [ ] Keep ambient events separate from mandatory quest progression so a cooldown never blocks the main adventure.

### DUNGEON-01 — one finished short dungeon [M6]

- [ ] Improve an existing dungeon, preferably Scarlet Crypts, before adding another. Target a clear entrance, two encounter spaces, one boss arena and a reliable exit.
- [ ] Give the boss two readable phases and a limited set of mechanics: avoidable ground attack, interruptible cast and optional adds. Validate each independently.
- [ ] Define wipe/reset, door state, loot eligibility, group entry and late-join behavior. Preserve underground separation and portal destinations.
- [ ] Scale health and encounter population within explicit 1–4 player rules; do not increase every damage value blindly.

Acceptance: a solo player and a cooperative group can enter, win or wipe, retry, claim eligible rewards once and leave without losing control or progress.

## Presentation and usability tickets

### UI-01 — minimal, complete player flows [P1, M2/M5]

- [ ] Preserve rotating title backgrounds and faction character previews; fix framing, grounding and loading fallbacks where needed.
- [ ] Keep health/resource, abilities, compact quest tracker and contextual interaction readable. Put inventory, journal, settings and companions behind intentional menu actions.
- [ ] Support mouse/keyboard navigation, Escape/back behavior and correct cursor/input locking. Opening a menu must not trigger combat through UI clicks.
- [ ] Add tooltips for ability cost/cooldown/range and disabled-state explanations. Use the existing ability icon atlas and original art.
- [ ] Add volume categories, sensitivity, resolution/fullscreen, graphics presets and keybind presentation using existing settings infrastructure where possible.
- [ ] Validate UI at 1280×720 and 1920×1080, with readable text and no clipped dialogs. Avoid color-only objective or combat cues.
- [ ] Show connection/loading/error states with retry/back actions; session failure must not leave a permanent invisible input blocker.

### AV-01 — consistent visuals and audio [P1 reference route; P2 remaining regions]

- [ ] Establish one reference lighting setup and palette. Check terrain, characters, particles and props under both day and night conditions.
- [ ] Diagnose visible light shapes as gizmos, helper meshes, billboards or actual effect geometry before hiding anything. Hide editor helpers from gameplay without disabling necessary illumination.
- [ ] Use controlled shadow distances, light counts and particle budgets. Verify baked/static lighting assumptions before moving affected objects.
- [ ] Add consistent hit, cast, footsteps, UI, mount and ambience cues with sensible distance rolloff and concurrency limits. Footsteps should match movement/surface where practical.
- [ ] Offer audio fallback when a clip is absent. Log missing references concisely; do not spam every frame.

Acceptance: no missing materials, blinding effects, persistent debug visuals or repeated overlapping sounds on the reference route.

## Reliability and multiplayer engineering tickets

### DATA-01 — persistence and content identity [P1, before M5]

- [ ] Inventory all save files, schemas and static state; document which data belongs to character, session or settings.
- [ ] Add schema versions and tested migrations only when needed. Back up saves before migration; malformed files must produce a recoverable message and preserve the original.
- [ ] Use safe staged writes so interruption cannot silently replace the last good save with partial data. Test missing, old and corrupt save cases.
- [ ] Clear transient static state during session/character changes. Verify playing character A, then B, does not leak quests, companions, targets or rewards.
- [ ] For multiplayer, distinguish display name, session identity and persistent identity. Document prototype trust limits: host authority is not protection against a modified host.
- [ ] If durable cross-session client identity is unresolved, ship explicitly session-only multiplayer progression with isolated saves; do not pretend local names provide authentication.

### NET-01 — requests, snapshots and scene lifecycle [P1, M4/M5]

- [ ] Define validated requests for attacks, abilities, interaction, loot, inventory and respawn. Reject invalid ownership, range, cooldown, quantities and stale targets.
- [ ] Separate durable replicated state from transient feedback. A late joiner needs current health/event state, not a replay of every historical particle or sound.
- [ ] Set rate limits and collection bounds on client requests; record concise rejection diagnostics for development.
- [ ] Define one session bootstrap and shutdown path. Returning to the title must release handlers, network objects, player references and UI locks.
- [ ] Define scene readiness before spawning actors; prevent duplicate scene managers, world systems and players after reconnect/re-entry.
- [ ] Add a content/protocol compatibility check. A client with incompatible quest/item IDs or build version receives a useful rejection.
- [ ] Plan relevancy by zone/distance after correctness is proven; do not send every ambient actor update to everyone indefinitely.

### QA-01 — reusable verification [P1, all milestones]

- [ ] Keep a small repeatable smoke route with a fresh character, an existing save and a two-player session.
- [ ] Add focused automated checks for consequential rules: duplicate reward prevention, transaction failure, save migration, authority validation and event reset. Avoid tests that only repeat trivial implementation details.
- [ ] Add editor validation for missing references, duplicate persistent IDs, invalid quest targets and broken content definitions when recurring defects justify it. Validation should report before mutating.
- [ ] Provide development-only controls for spawning the selected encounter, setting event state and showing navigation/ownership diagnostics. Keep them out of the release HUD.
- [ ] Capture performance in the same standalone route/settings. Set frame-time, memory, active AI, visible-light and bandwidth budgets from measurements; record regressions and causes.
- [ ] Repeat gameplay at low and high frame rates, with pause/menu transitions and repeated scene/session entry.

## Suggested implementation sessions

Historical world-first schedule below is superseded by the multiplayer-first queue at the top. Resume MP-01–MP-08 now; use this schedule for remaining polish/content after multiplayer delivery.

Use these as bounded work packets, not deadlines. Split a packet if its gate cannot be reached safely.

1. Baseline, inventory, defect reproduction and P0 compiler/input fixes.
2. Tree diagnosis, terrain/road repairs and world placement audit.
3. Animation/facing and player recovery; prove the reference traversal route.
4. Settlement polish, coherent materials, restrained lighting/audio and HUD flow checks.
5. Connect quests, balance the small item economy and finish the offline adventure.
6. NPC schedules and wildlife, then one complete caravan event.
7. Ambush event, cleanup/cooldown checks and measured performance pass.
8. Networking decision, additive test scene and two-process ownership proof.
9. Player movement/combat and one host-owned enemy encounter.
10. Quest/loot transactions, persistence boundary and multiplayer menu/error handling.
11. Mounts, companions, late join, disconnect and four-player testing.
12. Expand the proven content pattern, finish one dungeon, package and document the release.

Do not begin P2 expansion while P0 defects or the current P1 gate remain unresolved. Keep P3 ideas—large persistent servers, PvP, auction house, guilds, host migration, procedural continents and broad subclass systems—in a future backlog until the core release works.

## Final acceptance and handoff

| Check | Required evidence |
| --- | --- |
| Compilation | Fresh Unity import/compile, zero compiler errors; summarize material warnings. |
| World | Matched before/after images, tree-instance audit, walked road/mount route and collision checks. |
| Living world | Completed and failed event runs, reset/cooldown checks, NPC availability, measured performance comparison. |
| Offline regression | Existing character loads, quests/rewards/mounts work, save/reload remains correct. |
| Multiplayer | Separate host/client executables, preferably two PCs; 2-player slice and 4-player session evidence. |
| Ownership | One local camera/HUD/listener per process; no client AI or remote input control. |
| Persistence | No duplicate rewards, no cross-character leakage, offline save compatibility preserved. |
| Delivery | Fresh standalone build with source revision/date and exact startup/join instructions. |

Update a separate implementation status note after each phase: completed checklist items, changed files, tests actually run, known limitations and the next step. Do not describe planned features as delivered or use an old executable as proof of new code.

Active order: bounded baseline check → existing network failure diagnosis → separate host/client ownership → real RPG integration → authoritative combat/progression → shared world and session hardening → fresh cooperative build → remaining world/content polish. Apply the retry limits above throughout; report remaining work honestly rather than looping indefinitely or shipping an unverified gate.
