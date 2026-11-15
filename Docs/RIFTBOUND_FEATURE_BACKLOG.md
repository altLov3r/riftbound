# Riftbound — expanded feature and content production backlog

Revision 1 · 2026-11-15 · Design targets and implementation work, not delivered features.

This is the detailed expansion of RIFTBOUND_MASTER_PLAN.md. It maps a much larger game while keeping implementation ordered. Read the master plan for architecture, the online work order for connection details, and current status/audit files for evidence. Do not start by adding more menus or treating existing prototype scripts as finished systems.

## 1. What the finished game should offer

An original third-person fantasy RPG where a player chooses a faction, race and class, enters a coherent inhabited world, completes regional stories, earns equipment and unlocks, and explores with up to three friends through online join codes. The game should work offline, feel responsive, preserve progress according to its disclosed save policy and provide replayable cooperative encounters.

The larger vision adds professions, class specializations, reputation, cosmetics, richer settlements, optional homes and more regions. Public persistent servers, competitive PvP and a global economy are separate future products of the architecture, not automatic consequences of Relay multiplayer.

### Proposed content envelope

These are planning targets for the expanded compact game, not current inventories, guaranteed deadlines or quotas to fill with repetitive content. Audit existing content, reuse what works and revise counts after playtesting.

| Area | First polished release | Expanded-game target |
| --- | --- | --- |
| Players | Solo and private 2–4-player online co-op | Same party size until measured redesign warrants more. |
| Identity | Two factions; review four-race launch subset without deleting existing prototypes | Additional reviewed races and cosmetic variants. |
| Classes | Four existing classes verified, fifth class introduced through one complete slice | Five distinct classes; two specializations each later. |
| World | Existing terrain with distinct connected adventure regions | Real new land only when content and performance justify it. |
| Story | Complete 20–30-minute reference adventure | Two faction introductions, three regional arcs and finale. |
| Quests | Preserve current verified quests; complete varied connective objectives | Approximately 25–35 meaningful quests including optional activities. |
| Enemies | Three distinct normal roles plus captain/boss | Approximately 10–12 reusable roles, regional variants, elites. |
| Dungeons | One complete short existing dungeon | Three distinct dungeons introduced one at a time. |
| Events | Caravan and rescue fully working | Five event templates with authored regional variants. |
| Professions | One gathering + one production loop | Two gathering and two production professions before broader growth. |
| Equipment | Useful starter, road and dungeon upgrades | Three coherent gear tiers across the compact level range. |
| Mounts | Existing horse/zebra fully verified | A few earned mounts using a proven animation pipeline. |
| Social | Party frames, pings, ready checks, revive only when verified | Party activity board, optional emotes and bounded chat. |

## 2. Priority, ownership and execution contract

- **P0:** data loss, crashes, startup/join blockers, broken authority, impossible mandatory traversal.
- **P1:** required to complete the advertised first adventure online/offline.
- **P2:** required for the expanded compact RPG after P1 gates pass.
- **P3:** optional expansions requiring their own feasibility gate.

Ticket states: NOT STARTED, INVESTIGATING, IMPLEMENTED, VERIFIED, BLOCKED. Record actual files, dependencies, expected behavior, host/client/save consequences, test result and evidence. A checkbox in this plan is not evidence.

Ownership while both agents are active:

| Workstream | Owner | Boundary |
| --- | --- | --- |
| Internet sessions, network authority, cooperative combat/progression | Antigravity | Separate online status note; coordinate shared runtime files. |
| World/traversal tooling, terrain comparison, tree/prop audit | Codex | CODEX_WORLD_AUDIT.md; do not alter networking/LivingWorldEvents during Antigravity ownership. |
| Shared scene, terrain/prefab replacement | Explicitly scheduled single owner | Approval requirements remain; never overwrite authored assets casually. |
| Final acceptance | Evidence plus user playtest where needed | Distinguish automated/synthetic checks from normal gameplay. |

All implementation retains the anti-loop rule: maximum two unchanged failed attempts, stop after ten minutes without new evidence, cap unchanged blocker investigation at twenty minutes and carry counts across handoffs. Record a blocker and do independent work instead of repeatedly rebuilding/restarting. Never hide a failed gate by weakening its assertion.

Preserve local work, saves, scene/prefab references and .meta GUIDs. No whole-world regeneration, destructive repository resets, account/billing changes, secrets or paid dependencies without the required authorization. Planning breadth does not override these rules.

## 3. Release train and dependencies

| Gate | Deliverable | Required before moving on |
| --- | --- | --- |
| G00 | Current inventory and stable baseline | Compile, reproducible defects, safe isolated test data. |
| G01 | Reliable online connection | Join codes, cross-network acceptance, cancellation and useful failure states. |
| G02 | Full cooperative gameplay slice | Combat both ways, individual quests/rewards, loot, mounts and respawn. |
| G03 | Session robustness | Four players, late join, disconnects and repeat session entry. |
| G04 | Traversal/world foundations | Local Reaver repair safely reviewed, bridge routes, tree grounding and camera checks. |
| G05 | Refined presentation | Locomotion, facing, readable combat, minimal UI and coherent audio/lighting. |
| G06 | Complete first adventure | A fresh player reaches the finale without debug help. |
| G07 | Final launch roster | Supported race/class combinations work across every core action. |
| G08 | Expanded regional content | Distinct settlements, faction introductions and three regional arcs. |
| G09 | Professions/economy/progression | Useful rewards, validated transactions and tested persistence policy. |
| G10 | Dungeon release | Solo and group entry, encounters, wipe/reset, rewards and exit. |
| G11 | Release candidate | Full regression, measured performance, licenses, credits and fresh build. |
| G12 | Competition/diploma package | Demonstration, architecture/test evidence, offline fallback and rehearsal. |
| G13 | Post-release expansions | Only content whose prerequisites and operating requirements are satisfied. |

Connection and authority must precede expanding online systems. Safe independent art/content planning may continue during blocked external acceptance, but a blocked test is not a pass.

## 4. Foundation and engineering epics

### FND — baseline and defect control [P0, G00]

Dependencies: current repository state and latest status. Deliverable: reproducible baseline, not a rewrite.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| FND-01 | Record source/build/package versions and dirty working state. | Test receipts identify what was actually tested. |
| FND-02 | Compile runtime/editor source and resolve blockers. | Zero compiler errors; warnings summarized honestly. |
| FND-03 | Smoke title → selection → world → quest → save/leave. | No invisible input blockers or missing local actor. |
| FND-04 | Create safe test characters and fixtures. | Real saves remain unchanged after verification. |
| FND-05 | Establish defect severity and reproduction template. | Each blocker has trigger, expected/actual behavior and next diagnostic. |
| FND-06 | Capture fixed route/settings performance baseline. | Hardware, settings and measurements are recorded. |
| FND-07 | Retire misleading completion claims in status notes. | Implemented, verified and NOT RUN remain distinct. |

### ARC — runtime architecture [P1, G00–G03]

Dependencies: FND. Extend current code incrementally.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| ARC-01 | Document bootstrap and shutdown ownership. | One network manager/session lifecycle. |
| ARC-02 | Separate input/camera/HUD from host simulation. | Remote avatars cannot operate local controls. |
| ARC-03 | Replace unsafe first-player assumptions where multiplayer needs it. | Enemies/interactions resolve the intended player. |
| ARC-04 | Map static registries and event subscriptions. | Re-entry leaves no stale actors or callbacks. |
| ARC-05 | Centralize durable entity/content IDs. | Renamed display text cannot corrupt claims. |
| ARC-06 | Separate immutable definitions and mutable session state. | Shared assets do not store one character's progress. |
| ARC-07 | Document migration boundaries before refactors. | Old serialized enums, IDs and references retain meaning. |

### DATA — definitions and validation [P1/P2, G02–G09]

Dependencies: ARC. Use existing catalogs/ScriptableObjects wherever suitable.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| DATA-01 | Define ability targeting, costs, cooldowns and effects. | UI and host use the same definition. |
| DATA-02 | Define items, stats, stack limits and restrictions. | Unknown/duplicate IDs produce useful validation errors. |
| DATA-03 | Define quests/objectives/prerequisites/rewards. | No missing targets or circular mandatory chains. |
| DATA-04 | Define encounter spawn IDs, home/leash and reward policy. | Enemy respawn/life identity is deliberate. |
| DATA-05 | Define regions, bounds, routes and arrival points. | World map and travel use consistent locations. |
| DATA-06 | Define event stages, participation and reset policy. | State can be reconstructed for late joiners. |
| DATA-07 | Add read-only content validation reports. | Reports identify actual asset paths without automatically modifying them. |

## 5. Complete online cooperative game

### ONL — internet session experience [P1, G01]

Dependencies: FND/ARC; active online work order. Reuse installed Sessions/Relay/NGO integration.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| ONL-01 | Host Online creates one private session. | Code appears only after successful startup. |
| ONL-02 | Join Online accepts a shared session code. | Two distinct identities connect without normal IP entry. |
| ONL-03 | Copy code and compact participant status. | Reliable clipboard action; no permanent debug panel. |
| ONL-04 | Initialization/authentication progress and cancellation. | Cancel cannot later enter an abandoned session. |
| ONL-05 | Specific invalid/full/incompatible/service errors. | Retry/Back restores usable UI. |
| ONL-06 | Cross-network standalone verification. | Two different internet connections pass; localhost is labeled separately. |
| ONL-07 | Offline mode bypasses online service dependency. | Offline startup works without internet. |

### AUTH — gameplay authority [P0/P1, G02]

Dependencies: ONL/ARC/DATA.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| AUTH-01 | Host validates actions rather than raw client damage. | Invented damage/cost/cooldown requests rejected. |
| AUTH-02 | Host owns health/resources/status/death. | Clients cannot heal or level themselves via regular state updates. |
| AUTH-03 | Bounded movement with authorized teleports. | Large arbitrary jumps rejected without breaking legitimate portals. |
| AUTH-04 | Targeted damage and kill credit delivery. | Non-host player loses HP and receives eligible credit. |
| AUTH-05 | Idempotent inventory/quest/loot requests. | Repeated/stale messages neither duplicate nor consume valid rewards. |
| AUTH-06 | Actor and session ownership checks. | A client cannot invoke another player's actions. |
| AUTH-07 | Host uses the same gameplay rules as remote players. | No accidental host-only cooldown/reward bypass. |

### SYNC — replicated world [P1, G02–G03]

Dependencies: AUTH.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| SYNC-01 | Player position/facing/locomotion interpolation. | Remote characters move smoothly in tested network conditions. |
| SYNC-02 | Attack/cast/hit/death action states. | Remote action and damage timing remain understandable. |
| SYNC-03 | Mount type/model/rider state. | Friends see the actual mount, not only a nameplate tag. |
| SYNC-04 | Host AI and NPC/event actors. | Clients do not simulate duplicate authoritative AI. |
| SYNC-05 | Snapshot current enemies/events/interactables. | Late join sees correct current state and no historical payouts. |
| SYNC-06 | Ready-to-receive snapshot lifecycle. | Slow loading cannot silently drop required initial state. |
| SYNC-07 | Relevance and update budgets after correctness. | Network traffic is measured and bounded. |

### FAIL — session failure/recovery [P1, G03]

Dependencies: ONL/SYNC.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| FAIL-01 | Enforce four players and reject a fifth. | Real session result, not merely menu text. |
| FAIL-02 | Client departure during combat/interaction. | Targets, channels and claims clean up safely. |
| FAIL-03 | Host departure ends gameplay with useful message. | Clients return safely; no host-migration claim. |
| FAIL-04 | Repeated host/join/leave cycles. | No duplicate managers, cameras or handlers. |
| FAIL-05 | Bad code, outage, quota and version mismatch. | Bounded errors; no endless authentication loop. |
| FAIL-06 | Latency/loss and delayed loading tests. | Corrections remain playable; missing state is handled. |
| FAIL-07 | Reconnect support decision. | Supported recovery is tested or explicitly unavailable. |

### TEAM — cooperative usability [P1/P2, G03–G06]

Dependencies: AUTH/SYNC. Audit already implemented extras before adding replacements.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| TEAM-01 | Compact teammate health/name/distance frames. | Accurate roster; hidden solo and cleaned on departure. |
| TEAM-02 | Rate-limited temporary map pings. | One ping per action, visible to party, expires correctly. |
| TEAM-03 | Ready check with timeout/decline. | Host departure/late join cannot deadlock it. |
| TEAM-04 | Ally revive with interruption. | Two helpers cannot complete twice. |
| TEAM-05 | Shared quest offers with explicit acceptance. | Ineligible/completed players cannot duplicate rewards. |
| TEAM-06 | Solo-friendly cooperative objective alternatives. | A disconnect cannot permanently lock a door/objective. |
| TEAM-07 | Minimal bounded party chat/emotes. | Text is escaped, rate-limited and does not steal gameplay input. |

## 6. Player identity and moment-to-moment play

### HERO — character creation [P1/P2, G05–G07]

Dependencies: ARC/DATA/UI. Current extra race prototypes must be preserved.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| HERO-01 | Resolve launch race/faction/class roster explicitly. | IDs preserved; supported combinations documented. |
| HERO-02 | Faction → race → class creation flow. | Back navigation retains valid choices. |
| HERO-03 | Name validation and preview. | Long/control-rich names cannot break UI/network payloads. |
| HERO-04 | Cozy faction preview scenes and equipment framing. | Correct grounding, scale, lighting and facing. |
| HERO-05 | Distinct appearance options supported by models. | Saved choices render identically after reload/join. |
| HERO-06 | Slot selection and explicit deletion confirmation. | Only the intended slot is affected. |
| HERO-07 | Missing-model/old-save fallback. | Useful recovery rather than invisible character or lost data. |

### FACTION — original factions and races [P2, G07–G08]

Dependencies: HERO/STORY.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| FACTION-01 | Original faction names, motivations and symbols proposal. | Consistent lore without changing technical IDs impulsively. |
| FACTION-02 | Two coherent starting-region experiences. | Different introductions lead to shared frontier content. |
| FACTION-03 | Small race passives with explicit formulas. | Bonuses apply once on host and match descriptions. |
| FACTION-04 | NPC hostility and safe-service rules. | Cross-faction friends can complete intended co-op. |
| FACTION-05 | Race-specific flavor dialogue and settlement props. | Identity appears beyond a menu description. |
| FACTION-06 | Balance current gold/speed/lifesteal bonuses. | No race trivializes travel, economy or combat. |
| FACTION-07 | Extra-race admission gate. | Full rig/animation/equipment test before release selection. |

### MOVE — responsive controls [P1, G04–G05]

Dependencies: AUTH for online movement.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| MOVE-01 | Run by default and deliberate slow-walk toggle. | Clear/rebindable controls with no conflicts. |
| MOVE-02 | Smooth start/stop, backpedal and strafe. | No unintended turning/sideways drifting. |
| MOVE-03 | Jump, falling and landing state. | Grounding and animation recover consistently. |
| MOVE-04 | Slope/step/door/bridge traversal. | Actual controller passes measured routes. |
| MOVE-05 | Dodge/guard/cast movement priorities. | No simultaneous conflicting action states. |
| MOVE-06 | UI focus, chat input and cursor behavior. | Menus never trigger attacks through UI. |
| MOVE-07 | Safe unstuck action. | Valid host-approved destination; no combat teleport exploit. |

### ANIM — animation quality [P1, G05]

Dependencies: HERO/MOVE; preserve current models until replacement review.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| ANIM-01 | Rig/axis/scale/forward-direction inventory. | Every model has a reproducible import profile. |
| ANIM-02 | Loop and root-drift corrections. | Run/walk repeat without floating or lateral root motion. |
| ANIM-03 | Stride-to-speed calibration. | No pronounced sliding at normal travel speeds. |
| ANIM-04 | Toe/knee/hip retargeting review. | Full motion cycle checked, not only idle screenshots. |
| ANIM-05 | Attack/cast/hit/death transitions and priorities. | Actions complete or cancel cleanly. |
| ANIM-06 | Mounted rider/animal synchronization. | Hips/feet/weapon and dismount restore correctly. |
| ANIM-07 | Remote animations and missing-clip fallback. | Network actor does not freeze or silently substitute invalid combat. |

### CAMERA — third-person camera [P1, G05]

Dependencies: MOVE/ANIM.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| CAMERA-01 | Orbit, zoom and stable following. | Useful default view at walking/running speeds. |
| CAMERA-02 | Wall/roof/terrain collision. | Camera never remains inside geometry. |
| CAMERA-03 | Mount and large-character framing. | Rider and destination remain visible. |
| CAMERA-04 | Narrow interior/door transitions. | No repeated zoom oscillation or loss of control. |
| CAMERA-05 | Sensitivity/invert/shake settings. | Settings persist and apply immediately. |
| CAMERA-06 | Target framing without forced disorientation. | Player can inspect surroundings during combat. |
| CAMERA-07 | Local ownership and focus-loss handling. | Exactly one intended camera/listener per player process. |

## 7. Combat and class identity

### FIGHT — core combat [P1, G02/G05]

Dependencies: AUTH/MOVE/DATA.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| FIGHT-01 | Shared targeting rules for melee/ranged/friendly/self. | Invalid/dead/out-of-range targets are rejected clearly. |
| FIGHT-02 | Cast/windup/impact/recovery stages. | Animation, effects and damage agree. |
| FIGHT-03 | Damage/heal/shield/armor/crit calculation order. | Modifiers are deterministic and applied once. |
| FIGHT-04 | Crowd-control stacking and interruption rules. | No permanent accidental freeze/stun loops. |
| FIGHT-05 | Dodge/guard/invulnerability windows. | Host validates timing; feedback reflects actual protection. |
| FIGHT-06 | Death/evade/projectile cancellation. | No stale queued hits after reset/death. |
| FIGHT-07 | Readable telegraphs and hit feedback. | Danger remains visible with four players casting. |

### CLASS — five-class roster [P1 existing/P2 fifth, G07]

Dependencies: FIGHT/ANIM/EQUIP. Fifth-class Ranger is a proposal until roster decision.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| CLASS-01 | Warrior: reliable strike, cleave and defensive recovery. | Distinct durable melee loop in solo/group play. |
| CLASS-02 | Mage: ranged damage, control and ward/blink. | Projectiles/LOS/control/teleport validated remotely. |
| CLASS-03 | Rogue: fast pressure, finisher and mobility. | Meaningful setup; no attack-speed exploit. |
| CLASS-04 | Paladin: support strike, area control and healing. | Party heal rules and resource costs work. |
| CLASS-05 | Ranger proposal: bow shot, charged attack and trap/utility. | Complete model/weapon/animation/projectile slice before selection. |
| CLASS-06 | Mixed and duplicate-class party balance. | No perfect-role composition required for launch content. |
| CLASS-07 | Consistent resources/tooltips/icons and starter gear. | Every supported class can finish the core arc. |

### BUILD — talents and specializations [P2/P3, G09/G13]

Dependencies: CLASS/SAVE/EQUIP.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| BUILD-01 | Audit current talents/glyphs/modifier order. | Switching heroes clears prior character state. |
| BUILD-02 | Meaningful small talent choices. | Each changes a supported gameplay behavior. |
| BUILD-03 | Respec/refund rules and UI preview. | No negative points or duplicated modifiers. |
| BUILD-04 | Warrior specialization proposals: guardian/skirmisher. | Distinct defense/mobility path using proven abilities. |
| BUILD-05 | Mage/Rogue/Paladin specialization prototypes. | One class at a time; network/save coverage included. |
| BUILD-06 | Ranger specialization after base-class approval. | Not a substitute for completing Ranger itself. |
| BUILD-07 | Talent-schema migration and balance iteration. | Old characters retain or deterministically refund valid points. |

### ENEMY — enemy behavior roster [P1/P2, G05–G10]

Dependencies: FIGHT/SYNC/WORLD.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| ENEMY-01 | Melee raider with pursuit/windup/recovery. | Spacing and dodge provide counterplay. |
| ENEMY-02 | Ranged attacker with bounded repositioning. | Uses LOS without endless kiting. |
| ENEMY-03 | Caster with interruptible threat. | Cast communicates timing and interruption works remotely. |
| ENEMY-04 | Defender and beast/skirmisher variants. | Behavior differs beyond health/color. |
| ENEMY-05 | Threat/target selection across connected players. | Disconnect/death changes target safely. |
| ENEMY-06 | Navigation failure, leash and home reset. | Bounded retries, no stuck invulnerable permanent encounter. |
| ENEMY-07 | Elite affix validation and visible identity. | Existing affixes work before adding more combinations. |

### BOSS — boss and encounter framework [P2, G08–G10]

Dependencies: ENEMY/FIGHT/LOOT.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| BOSS-01 | Start/participant/arena/wipe/reset states. | Repeated attempts do not leak adds or rewards. |
| BOSS-02 | Two learnable phases and limited mechanics. | Player can identify cause of failure. |
| BOSS-03 | Ground attack with honest area/timing. | Visual warning matches damage footprint. |
| BOSS-04 | Interruptible cast and add-wave variations. | All classes/party setups have viable responses. |
| BOSS-05 | Bounded solo-to-four-player scaling. | Joining/leaving does not reset/reheal for exploitation. |
| BOSS-06 | Per-player reward/lockout and exit. | Simultaneous completion grants correctly once. |
| BOSS-07 | Encounter journal with concise hints. | Explains real mechanics rather than misleading prototype text. |

## 8. World foundations and genuine expansion

### GROUND — terrain and traversal repairs [P0/P1, G04]

Dependencies: CODEX_WORLD_AUDIT findings; coordinated asset/scene ownership.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| GROUND-01 | Preserve current terrain data and authored changes. | Hash/assignment snapshot before targeted edits. |
| GROUND-02 | Local Reaver grading repair using current-tile copy. | Preserve 813 current tree instances; never swap in tree-less old asset blindly. |
| GROUND-03 | Match Terrain and TerrainCollider protected data. | Existing separate-asset regression passes without weakening it. |
| GROUND-04 | Bridge-aware Westbank road validation. | Actual deck/bank transitions tested, river not flattened from terrain-only warning. |
| GROUND-05 | Road grade, side width and camera clearance. | Foot/mount routes pass actual controller tests. |
| GROUND-06 | Terrain seam and boundary checks. | Heights, collisions and travel behave consistently. |
| GROUND-07 | Old-save safe spawn recovery. | New terrain changes do not strand characters underground. |

### NATURE — trees, vegetation and ecology [P1/P2, G04/G08]

Dependencies: GROUND/ART.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| NATURE-01 | Fresh tree/rock/bush rendered-orientation audit. | Root-transform evidence is complemented by actual visual checks. |
| NATURE-02 | Classify intentional logs versus accidental sideways trees. | Story props preserved; no blanket rotation fixes. |
| NATURE-03 | Trunk/root grounding and collision review. | No floating trunks or invisible path blockers. |
| NATURE-04 | Clusters based on water, slope and settlement use. | Biomes feel intentional rather than evenly scattered. |
| NATURE-05 | Forest edge/grass/rock transitions. | Roads/objectives remain visible and walkable. |
| NATURE-06 | LOD/wind/shadow consistency. | No distracting popping or excessive foliage cost. |
| NATURE-07 | Safe vegetation edits after terrain repair. | Only reviewed local instances moved/removed. |

### REGION — spatial world structure [P2, G08]

Dependencies: GROUND/NATURE/QUEST; detailed regional brief later in this file.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| REGION-01 | Zone bounds, landmarks and encounter separation. | Content occupies useful distinct space. |
| REGION-02 | Roads, junctions, bridge links and return routes. | Every mandatory destination has a continuous playable approach. |
| REGION-03 | Biome silhouettes and distant views. | Players recognize destination without relying only on markers. |
| REGION-04 | Safe hub → threat edge → optional discovery pattern. | Pacing includes quiet spaces and choices. |
| REGION-05 | Actual new terrain when expanding bounds. | Map extent and travel genuinely expand. |
| REGION-06 | Map/fast-travel/network relevance updates. | No new land outside UI/loading/authority assumptions. |
| REGION-07 | Full outbound/return acceptance for each region. | Both offline and party traversal verified. |

### TOWN — functional settlements [P2, G08]

Dependencies: REGION/LIFE/ART.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| TOWN-01 | Entrance, centre, work, storage and rest spaces. | Settlement has an understandable purpose and circulation. |
| TOWN-02 | Useful merchant/quest/travel locations. | Services do not overlap or block paths. |
| TOWN-03 | Forge/farm/dock/workshop vignettes. | Props support an activity rather than random clutter. |
| TOWN-04 | Guard routes and safe arrival areas. | Patrols never push players into mandatory combat. |
| TOWN-05 | Interiors worth entering. | At least one useful interaction/story; camera and exits work. |
| TOWN-06 | Day/night lamps and restrained ambience. | Night remains playable; helper meshes hidden. |
| TOWN-07 | Group-size interaction space. | Four players can gather without trapping NPCs/doors. |

### LIFE — NPC schedules and wildlife [P2, G08]

Dependencies: SYNC/TOWN; coordinate LivingWorldEvents ownership.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| LIFE-01 | Activity points/routes/waits for workers and residents. | Visible routine with bounded decisions. |
| LIFE-02 | Traveller/caravan follows verified paths. | Height sampling is not treated as collision/path clearance. |
| LIFE-03 | Quest NPC routine interruption. | Giver remains reachable and interaction resumes safely. |
| LIFE-04 | Grazing/wandering/fleeing wildlife. | Host-owned decisions; no duplicate client simulation. |
| LIFE-05 | Stuck/path-failure recovery. | Bounded retries with safe fallback. |
| LIFE-06 | Distance/culling/AI activity budgets. | Busy regions remain within measured performance target. |
| LIFE-07 | Late-join and scene cleanup of ambient actors. | Actor count/state consistent after repeated sessions. |

### EVENTS — dynamic world content [P1 existing/P2 expanded, G06/G08]

Dependencies: LIFE/SYNC/LOOT.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| EVENTS-01 | Complete caravan escort and replace cube visual. | Real route, ambush, arrival/failure and cleanup. |
| EVENTS-02 | Complete roadside rescue. | Scout state, failure and rewards are honest. |
| EVENTS-03 | Settlement-defense prototype. | Bounded waves; critical NPCs and main quests remain safe. |
| EVENTS-04 | Lost traveller/clue event. | Optional exploration with safe cancellation/recovery. |
| EVENTS-05 | Ruin-disturbance encounter. | One trigger cannot recursively spawn/farm rewards. |
| EVENTS-06 | Shared template states/eligibility/cooldowns. | All participants and late joiners see current stage. |
| EVENTS-07 | First-clear/repeat bounty policy. | Rates/claims persist only under the actually supported save policy. |

## 9. Story, questing and discovery

### STORY — narrative bible [P2, G06/G08]

Dependencies: faction/region decisions; preserve existing quest IDs.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| STORY-01 | One-page premise and frontier conflict. | Original coherent motivations connect regions. |
| STORY-02 | Faction viewpoints without blocking co-op. | Both introductions lead to shared goals plausibly. |
| STORY-03 | Named NPC roles and speaking style. | Givers feel distinct and have world purposes. |
| STORY-04 | Three regional arcs and finale outline. | Each resolves something and opens the next thread. |
| STORY-05 | Environmental clues and optional lore. | Discoveries reward attention without mandatory text walls. |
| STORY-06 | Naming/glossary/style consistency. | Original terms consistent across UI, dialogue and diploma. |
| STORY-07 | State-aware dialogue variants. | NPCs acknowledge acceptance/completion and current threats. |

### QUEST — reusable objectives and journal [P1/P2, G02/G06]

Dependencies: DATA/AUTH/STORY.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| QUEST-01 | Kill/collect/interact/discover objective contracts. | Host validates source IDs and counts. |
| QUEST-02 | Escort/defend objectives through event framework. | Failure/reset cannot permanently block main progress. |
| QUEST-03 | Prerequisites/accept/active/ready/complete/abandon states. | Valid transitions; no repeated payout. |
| QUEST-04 | Individual progress with eligible party credit. | One player cannot consume another's objective. |
| QUEST-05 | Prior-discovery and out-of-order rules. | Existing exploration credit remains intentional. |
| QUEST-06 | Reward preview, track/untrack and location hints. | Actual next objective/giver shown accurately. |
| QUEST-07 | Journal persistence and character isolation. | No cross-character or host-journal leakage. |

### TALK — NPC dialogue and service interactions [P1/P2, G06]

Dependencies: QUEST/UI/TOWN.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| TALK-01 | WoW-inspired readable quest dialogue layout with original art. | Clear accept/complete/back actions. |
| TALK-02 | Contextual gossip/service choices. | Only available services and relevant quests displayed. |
| TALK-03 | Range/LOS/life-state interaction rules. | Cannot turn in remotely through walls or while invalid. |
| TALK-04 | Interaction focus and input blocking. | No attack/cast triggered by dialogue clicks. |
| TALK-05 | Moving/interrupted NPC response. | Window closes/pauses safely and routine recovers. |
| TALK-06 | State-aware text and compact reward summaries. | Text matches actual quest and item grants. |
| TALK-07 | Text-only fallback with optional voiced flavor. | Missing audio never blocks understanding/progression. |

### DISCOVER — exploration and collectibles [P2, G06/G08]

Dependencies: REGION/QUEST/SAVE.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| DISCOVER-01 | Landmarks with stable discovery IDs. | Discovery is one-time under supported persistence. |
| DISCOVER-02 | Hidden caches with authored clues. | Findable through environment, not random pixel hunting. |
| DISCOVER-03 | Viewpoints, ruins and short lore interactions. | Each adds regional identity or useful reward. |
| DISCOVER-04 | Achievement/title cosmetic unlocks. | Criteria and claims match observed behavior. |
| DISCOVER-05 | Accurate map discovery/fog policy. | Does not expose every secret unintentionally. |
| DISCOVER-06 | Preserve exact zebra/runestone gates. | Unrelated markers cannot satisfy required unlocks. |
| DISCOVER-07 | Restrained notifications and progress display. | No oversized world captions or notification flood. |

## 10. Rewards, economy and long-term character growth

### LEVEL — progression pacing [P1/P2, G06/G09]

Dependencies: QUEST/FIGHT.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| LEVEL-01 | Audit current level cap and thresholds. | UI/enemy/reward tables use consistent limits. |
| LEVEL-02 | Reference-run XP budget. | New players progress without forced grinding. |
| LEVEL-03 | Optional content reward balance. | Exploration valuable without trivializing campaign. |
| LEVEL-04 | Rested/racial/party bonus order. | Each modifier applies once. |
| LEVEL-05 | Level-up stat/resource/talent handling. | No unintended full-heal loops or duplicate point grants. |
| LEVEL-06 | Gear/enemy scaling across regions. | Difficulty changes are understandable and bounded. |
| LEVEL-07 | Level-cap expansion gate. | New levels have actual abilities/items/encounters, not empty numbers. |

### EQUIP — inventory and equipment [P1, G02/G09]

Dependencies: DATA/AUTH/SAVE.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| EQUIP-01 | Stack, slot, rarity and restriction rules. | Invalid item/slot combinations rejected. |
| EQUIP-02 | Equip/unequip and stat recalculation. | Bonuses neither duplicate nor persist after removal. |
| EQUIP-03 | Tooltip comparisons and icons. | Descriptions match actual effects. |
| EQUIP-04 | Full inventory and partial-stack handling. | Failed grants preserve gold/materials/claims consistently. |
| EQUIP-05 | Weapon/armor appearance and sockets. | No wrong-facing or floating attachments. |
| EQUIP-06 | Starter/road/dungeon gear families. | Every class has useful obtainable upgrades. |
| EQUIP-07 | Safe old-item migration. | Retired/unknown IDs recover without losing the whole save. |

### LOOT — ownership and reward distribution [P0/P1, G02]

Dependencies: AUTH/EQUIP.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| LOOT-01 | Per-player enemy/chest claims. | One player does not steal another's eligibility. |
| LOOT-02 | Reward transaction identity and retry behavior. | Exactly one grant for repeated valid request. |
| LOOT-03 | Death/respawn-life reward identity. | Intended repeat kills work without duplicating old claims. |
| LOOT-04 | Unclaimed/partially claimed loot persistence policy. | Advertised recovery is tested. |
| LOOT-05 | Group-roll system audit. | Real participant/ownership rules, not simulated fairness. |
| LOOT-06 | Boss/quest/event claim separation. | One reward path cannot pay the same entitlement twice. |
| LOOT-07 | Clear loot feedback and failure reason. | Full inventory or ineligibility is understandable. |

### GOLD — shops and economy [P1/P2, G09]

Dependencies: EQUIP/LOOT/LEVEL.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| GOLD-01 | Source/sink table for quests, kills and events. | Repeatable rewards do not overwhelm main progression. |
| GOLD-02 | Atomic buy/sell transactions. | Duplicate clicks or failed item grant cannot lose/duplicate value. |
| GOLD-03 | Consumable usefulness and availability. | Supports recovery without mandatory grind. |
| GOLD-04 | Racial discounts/gold bonuses. | Apply once with validated values. |
| GOLD-05 | Vendor stock/restock policy. | Clients agree on shared versus personal stock. |
| GOLD-06 | Respec/travel/cosmetic sinks where useful. | Costs have a purpose and are explained. |
| GOLD-07 | Auction/trading exposure gate. | Incomplete/global-economy simulations hidden or labeled honestly. |

### CRAFT — gathering and professions [P2, G09]

Dependencies: GOLD/EQUIP/REGION/AUTH.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| CRAFT-01 | Gathering-node ecology and ownership. | Respawn and claim rules work for all players. |
| CRAFT-02 | Gathering channel/interrupt/range checks. | No remote or repeated instant harvesting. |
| CRAFT-03 | One complete production profession first. | Materials → useful item through validated recipe. |
| CRAFT-04 | Recipe discovery/training and skill progression. | Requirements/rewards explained in UI. |
| CRAFT-05 | Atomic materials/output transaction. | Full inventory/failure leaves consistent state. |
| CRAFT-06 | Second gather/production loop after balance. | Distinct purpose, not duplicate resource grind. |
| CRAFT-07 | Fishing/archaeology optional activities. | Complete input/feedback/reward loop before advertisement. |

### REPUTE — reputation and cosmetics [P2/P3, G09/G13]

Dependencies: QUEST/SAVE/GOLD.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| REPUTE-01 | Small regional/faction rank structure. | Sources and unlock requirements explicit. |
| REPUTE-02 | One useful rank reward per faction initially. | Actual service/recipe/cosmetic unlocked. |
| REPUTE-03 | Cosmetic transmog rules. | Appearance changes never alter unauthorized stats. |
| REPUTE-04 | Titles/emotes and achievement cosmetics. | Stable identity and correct ownership. |
| REPUTE-05 | Barber/appearance service audit. | Valid saved choices and no item/equipment corruption. |
| REPUTE-06 | Reputation branch dialogue. | Consequences visible without blocking friendly party play. |
| REPUTE-07 | Cosmetic collection UI. | Clear owned/unowned state without excessive permanent HUD. |

## 11. Travel, allies and recovery

### RIDE — mounts and travel [P1/P2, G02/G04]

Dependencies: MOVE/ANIM/SYNC/GROUND.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| RIDE-01 | Horse/zebra unlock and selection verification. | Exact existing discovery criteria preserved. |
| RIDE-02 | Summon/cancel/damage-dismount rules. | Host validates transitions and cooldowns. |
| RIDE-03 | Rider saddle/pose/weapon visibility. | No hovering rider or wrong dismount scale. |
| RIDE-04 | Mounted sweep/clearance and safe dismount. | Doors/bridges/trees cannot trap rider. |
| RIDE-05 | Hearth/flight/portal rules. | Cost, cancellation, scene readiness and destination validated. |
| RIDE-06 | New earned mount pipeline. | One complete licensed model/rig/animation before roster expansion. |
| RIDE-07 | World map travel presentation. | Destinations accurate and no unsupported instant travel. |

### ALLY — companions and pets [P2, G06/G09]

Dependencies: TEAM/ENEMY/SYNC.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| ALLY-01 | Solo default and explicit recruit/dismiss menu. | No automatic crew at spawn. |
| ALLY-02 | Host-owned follow/combat/recall logic. | No duplicate client companions. |
| ALLY-03 | Formation and doorway behavior. | Allies avoid blocking player/NPC interactions. |
| ALLY-04 | Follow/hold/passive commands if useful. | Commands have clear state and feedback. |
| ALLY-05 | Group companion count/performance budget. | Four-player parties cannot spawn uncontrolled armies. |
| ALLY-06 | Ownership/progression/reward rules. | Companions do not multiply rewards accidentally. |
| ALLY-07 | Cosmetic pets versus combat pets distinction. | UI and behavior match supported role. |

### RECOVER — death, respawn and safe recovery [P1, G02]

Dependencies: AUTH/GROUND/TEAM.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| RECOVER-01 | Death state and control/animation cleanup. | No actions after death; camera remains useful. |
| RECOVER-02 | Safe respawn location and host approval. | No collider/terrain/boss-arena trap. |
| RECOVER-03 | Revive versus respawn conflict handling. | Exactly one valid transition and reward/state cleanup. |
| RECOVER-04 | Group wipe behavior. | Encounters reset without corrupting personal progress. |
| RECOVER-05 | New-player penalty policy. | No surprise irreversible gear loss in compact release. |
| RECOVER-06 | Invalid position/save recovery. | Useful fallback and preserved original data. |
| RECOVER-07 | Repeated death/disconnect edge cases. | No duplicate actors or immortal stale state. |

## 12. Dungeons, endgame and replayability

### CRYPT — first finished dungeon [P2, G10]

Dependencies: BOSS/QUEST/LOOT/RECOVER/SYNC.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| CRYPT-01 | Scarlet Crypts entrance/staging and correct return. | Interior separation and portal destinations preserved. |
| CRYPT-02 | Two readable encounter spaces and boss arena. | Camera/navigation support actual combat. |
| CRYPT-03 | Group entry, late join and encounter readiness. | No partial world loading/duplicate managers. |
| CRYPT-04 | Boss phases and wipe/reset. | Learnable fight supports 1–4 players. |
| CRYPT-05 | Per-player boss/quest/reliquary rewards. | Repeated entry cannot duplicate entitlement. |
| CRYPT-06 | Useful route/story/reward purpose. | Dungeon is more than an empty portal room. |
| CRYPT-07 | Full run and failed-run acceptance. | Enter, fight, wipe/win, retry, loot and leave normally. |

### REPLAY — repeatable adventure content [P3, G13]

Dependencies: completed CRYPT/EVENTS/LOOT.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| REPLAY-01 | Elite hunt board using proven targets. | Exact kill IDs and reward cadence. |
| REPLAY-02 | Dungeon challenge modifiers. | Bounded meaningful difficulty, no unavoidable combinations. |
| REPLAY-03 | Rotating authored event variants. | Reset/seed/claim policy explicit. |
| REPLAY-04 | Second dungeon: quarry/forge proposal. | Different spatial/mechanical identity, full completion gate. |
| REPLAY-05 | Third dungeon: grove/ruins proposal. | Original content and tested group flow. |
| REPLAY-06 | Cosmetic/mastery goals. | Replayability without unlimited stat inflation. |
| REPLAY-07 | No forced daily obligation in compact game. | Optional activities remain optional. |

## 13. Presentation and content production

### ART — coherent model family [P1/P2, G05/G07]

Dependencies: licensed source assets and art review.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| ART-01 | Visual guide: proportions, palette, materials and silhouettes. | Representative character/environment screenshot looks coherent. |
| ART-02 | Free-asset provenance register. | Source, author, license and modifications recorded. |
| ART-03 | Character rig/deformation/equipment pipeline. | Shoulders/hips/knees/feet and sockets tested. |
| ART-04 | Scale/pivots/normals/UV/import validation. | Correct Unity orientation/materials and collision. |
| ART-05 | LOD/collision/material budget. | Visual quality and measured performance both accepted. |
| ART-06 | Model admission through one representative prefab. | No whole-roster replacement before validation. |
| ART-07 | Missing kit pieces: wagon, bridge parts, work props and signs. | Purposeful additions in the same visual family. |

### LIGHT — graphics and environmental mood [P1/P2, G05/G08]

Dependencies: WORLD/ART/PERF.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| LIGHT-01 | Reference day/night lighting palette. | Characters/roads/enemy warnings readable. |
| LIGHT-02 | Remove visible development helpers correctly. | Necessary illumination remains; debug meshes absent. |
| LIGHT-03 | Shadows/AA/fog/bloom/color grading. | Coherent preset, no indiscriminate effect stacking. |
| LIGHT-04 | Ground materials and biome blending. | No harsh texture seams or uniform tiled appearance. |
| LIGHT-05 | Water/foliage/transparency sorting. | Stable visuals at supported camera distances. |
| LIGHT-06 | Low/medium/high settings. | Measured meaningful performance/quality tradeoff. |
| LIGHT-07 | Interior and weather transitions. | No sudden unreadable exposure/fog state. |

### FX — ability and world effects [P1/P2, G05]

Dependencies: FIGHT/ART/SYNC.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| FX-01 | Distinct visual language by ability role. | Friendly/enemy danger distinguishable. |
| FX-02 | Telegraph projection and timing. | Matches authoritative area/impact. |
| FX-03 | Hit/heal/shield/status feedback. | Visible without excessive combat-number clutter. |
| FX-04 | Predicted/accepted effect deduplication. | One action does not play twice online. |
| FX-05 | Pooling/concurrency/particle-light budgets. | Four-player encounter remains within measured target. |
| FX-06 | Cancellation/death/scene cleanup. | No lingering damage decals/loops after reset. |
| FX-07 | Reduced-flash and effect-intensity options. | Gameplay cues remain useful with reductions. |

### SOUND — complete soundscape [P1/P2, G05]

Dependencies: FIGHT/LIFE/RIDE/UI; licensed audio.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| SOUND-01 | Master/music/ambience/SFX/UI controls. | Consistent persistent volume behavior. |
| SOUND-02 | Footstep surface and movement cadence. | No machine-gun loops while idle/sliding. |
| SOUND-03 | Weapon/cast/hit/heal/mount cues. | Timing and intensity match accepted action. |
| SOUND-04 | Spatial settlement/wildlife/water ambience. | Bounded range/rolloff and sensible layering. |
| SOUND-05 | Music transitions and quiet spaces. | Zones feel distinct without constant loud playback. |
| SOUND-06 | Online playback deduplication and priorities. | No every-client echo or unbounded simultaneous sounds. |
| SOUND-07 | Warning text/visual equivalents and credits. | Important information survives mute; sources recorded. |

### UI — minimal production-style interface [P1, G01/G05]

Dependencies: actual supported systems; do not expose unfinished features as complete.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| UI-01 | Rotating original title backgrounds and polished navigation. | No broken texture/loading fallback. |
| UI-02 | Character/session/error/loading flows. | Clear Back/Cancel and no invisible input capture. |
| UI-03 | Minimal default HUD and contextual hints. | Health, abilities and objective readable without crowded panels. |
| UI-04 | Original ability icons and accurate tooltips. | Costs/ranges/cooldowns match actual rules. |
| UI-05 | Intentional menu access for journal/map/inventory/companions. | No permanent panel per system. |
| UI-06 | Consistent typography, spacing, focus and notifications. | Supported resolutions have no clipping or spam. |
| UI-07 | Settings/keybinds/controls completeness. | Conflicts detectable; changed controls reflected in hints. |

### ACCESS — onboarding and accessibility [P1/P2, G05/G06]

Dependencies: UI/MOVE/FIGHT/SOUND.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| ACCESS-01 | First-ten-minute contextual tutorial. | Teaches interaction/combat/loot without modal combat interruptions. |
| ACCESS-02 | Text scale, contrast and non-color-only cues. | Objectives/status understandable across settings. |
| ACCESS-03 | Shake/flash/sensitivity options. | Actual effects honor settings. |
| ACCESS-04 | Skippable dialogue and readable controls help. | Players can recover forgotten instructions. |
| ACCESS-05 | Centralized stable player-facing strings. | One complete release language, no false localization claims. |
| ACCESS-06 | Unfamiliar-player observation pass. | Documented confusion leads to targeted fixes. |
| ACCESS-07 | Controller/platform support gate. | Advertised only after real device/flow testing. |

## 14. Data reliability, tools and release operations

### SAVE — persistence and identity [P0/P1/P2, G02/G09]

Dependencies: ARC/DATA/AUTH.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| SAVE-01 | Honest online session-only policy until durable storage exists. | Player sees what will/won't persist before playing. |
| SAVE-02 | Safe offline writes/backups/corrupt-file recovery. | Original damaged file retained and recoverable. |
| SAVE-03 | Character A → B → A isolation. | Talents/quests/companions/stats never leak. |
| SAVE-04 | Schema/legacy-position/item migration. | Old characters load with deterministic fallback. |
| SAVE-05 | Online persistent-identity design gate. | Anonymous service identity is not advertised as cross-device recovery. |
| SAVE-06 | Durable co-op claim/transaction recovery if implemented. | Crash/rejoin cannot duplicate or erase promised rewards. |
| SAVE-07 | Explicit offline/online import policy. | No silent overwrite or trusted-client global economy. |

### PERF — measured performance [P1/P2, G03/G11]

Dependencies: fixed scenarios and target hardware.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| PERF-01 | Hardware/settings/frame-time baseline. | CPU/GPU, spikes and memory recorded. |
| PERF-02 | Busy hub/forest/four-player fight/dungeon scenarios. | Worst cases measured, not only average FPS. |
| PERF-03 | AI/path/search/allocation budgets. | No unbounded retries or avoidable per-frame scene scans. |
| PERF-04 | LOD/culling/instancing/material/lighting budgets. | Before/after comparisons preserve gameplay. |
| PERF-05 | Network traffic/update/relevance budget. | More content does not replicate every actor globally forever. |
| PERF-06 | Loading and warmup behavior. | No false progress or deadlocked scene readiness. |
| PERF-07 | Streaming/Addressables feasibility only if needed. | Measured requirement before architecture expansion. |

### TOOL — authoring and diagnostics [P1/P2, G04/G11]

Dependencies: DATA/GROUND/ownership coordination.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| TOOL-01 | Read-only current-scene audit with accurate totals. | No automatic fixes/saves or truncated totals. |
| TOOL-02 | Bridge-aware terrain/road/clearance reports. | Candidate limits and NOT RUN areas explicit. |
| TOOL-03 | Asset/quest/reference validation. | Actionable IDs/paths rather than huge logs. |
| TOOL-04 | Previewable bounded world tools. | No startup whole-world regeneration. |
| TOOL-05 | Development-only encounter/event controls. | Hidden from normal release UI. |
| TOOL-06 | Structured receipts tied to source/build state. | Results distinguish fixtures, gameplay and visual checks. |
| TOOL-07 | Retry/bounded-test status tracking. | New agent session cannot reset an unchanged failing loop. |

### TRUST — service/security/privacy boundaries [P1, G01/G11]

Dependencies: ONL/AUTH/SAVE.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| TRUST-01 | Validate RPC input size/rate/ranges/ownership. | Invalid values rejected without damaging state. |
| TRUST-02 | Secrets and token handling. | No keys/tokens in source/build/handoff receipts. |
| TRUST-03 | Cloud ownership/environment/quota documentation. | No duplicate projects or unauthorized billing changes. |
| TRUST-04 | Join-code and diagnostic hygiene. | Invitation codes not unnecessarily published/logged. |
| TRUST-05 | Trusted-host limits explained. | Private co-op not misrepresented as cheat-proof competitive infrastructure. |
| TRUST-06 | Minimal optional diagnostics collection. | No unexplained external personal-data transmission. |
| TRUST-07 | Public community feature prerequisites. | Reporting/block/moderation/privacy designed before public exposure. |

### QA — systematic acceptance [P1, all gates]

Dependencies: reproducible source/build/test conditions.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| QA-01 | Fresh and existing character smoke paths. | Both offline and online results recorded. |
| QA-02 | Consequential rule/transaction tests. | Duplicate rewards, failed grants and authority rejections covered. |
| QA-03 | Full supported race/class action matrix. | Create/equip/fight/mount/die/reload works. |
| QA-04 | Normal-input two-player quest run. | No CLI-only setup masking real UI issues. |
| QA-05 | Four players, late join and failure matrix. | PASS/FAIL/NOT RUN with actual evidence. |
| QA-06 | Traversal/graphics/audio/resolution review. | Actual motion/listening/controller checks distinguished from fixtures. |
| QA-07 | Release regression and known-limitations inventory. | No unresolved blocker hidden behind old green tests. |

### SHIP — version control and delivery [P1, G11]

Dependencies: QA/TRUST/asset provenance.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| SHIP-01 | Git/LFS/source/meta hygiene. | Generated caches/secrets excluded; required assets tracked. |
| SHIP-02 | Separate collaborator checkouts and ownership. | No simultaneous edits to shared scenes/runtime files. |
| SHIP-03 | Source/version/build identification. | Executable matches documented code/configuration. |
| SHIP-04 | Clean-machine standalone launch test. | Required runtime data included. |
| SHIP-05 | Controls/setup/online/persistence documentation. | New tester can launch and invite a friend. |
| SHIP-06 | Asset/service credits and licenses. | All shipped sources documented. |
| SHIP-07 | Recoverable release archive and offline fallback. | Only working build is never replaced by an experiment. |

### DIPLOMA — demonstration and academic work [P1, G12]

Dependencies: actual implemented architecture and evidence.

| ID | Feature/work item | Completion check |
| --- | --- | --- |
| DIPLOMA-01 | Requirements, audience, scope and original contribution. | Distinguishes reused assets/tools from authored work. |
| DIPLOMA-02 | Architecture/data/authority diagrams. | Correspond to shipped implementation. |
| DIPLOMA-03 | Quest/event/save state-machine examples. | Explain real design decisions and failure handling. |
| DIPLOMA-04 | Test results/performance/before-after defects. | Evidence includes limitations rather than only success claims. |
| DIPLOMA-05 | Consistent Bulgarian explanation/English terminology. | Glossary matches code/UI/presentation. |
| DIPLOMA-06 | Short rehearsed playable demonstration. | Menu → online → quest/combat → world/mount → exit. |
| DIPLOMA-07 | Backup video/build/source/slides. | Presentation can continue if internet/services fail. |

## 15. Region production briefs

These add specificity to the master plan's existing coordinate map. Keep technical IDs and authored positions until a reviewed change is necessary. Content names below are proposals, not claims of implemented quests.

### Emberwatch and Ashwood — the introduction

- Purpose: teach movement, interaction, combat, loot and first party cooperation through Abe's existing chain.
- Layout: safe arrival, training corner, supply yard, village work area, road departure, raider area and captain approach with enough separation to read threats.
- Story: disrupted deliveries expose a broader frontier problem. Preserve current mission IDs and completion logic.
- Needed scenes/vignettes: crates being counted, blacksmith activity, returning patrol, visible road sign and quiet camp rest area.
- Enemy mix: simple melee first, one ranged threat after basic targeting is taught, captain introducing a telegraph.
- Optional discovery: a lookout or misplaced shipment with environmental clues.
- Acceptance: fresh character learns the loop and returns to Abe without map confusion or collision blockers.

### Highspire and Frostpeak — faction life and work

- Purpose: establish faction identity and introduce services/crafting without a giant empty capital.
- Layout: readable entrance, central square, forge/storehouse, inn/rest and roads to Westbank and foothills.
- Story: local workers need materials while patrols secure a trade route.
- Needed content: quarry disturbance, useful vendor, first profession introduction and one visible guard circuit.
- Optional discovery: old watch marker or mine-worker journal.
- Acceptance: each street/service has a purpose; foot/mount/camera routes remain clear.

### Silverglen — magic and ecology

- Purpose: quieter biome with ranged/caster threats and original magical-worldbuilding identity.
- Layout: grove approach, ritual/work circle, water or plant landmark and one contained corrupted clearing.
- Story: investigate disturbed ward markers through interaction and one fight.
- Needed content: clear magical material language, noncombat inhabitants, ambient wildlife and one class-neutral utility task.
- Optional discovery: a scenic cache/lore interaction that does not require Mage.
- Acceptance: every class can solve the arc; particle/fog effects do not hide the path.

### Westbank and Willow Rest — the moving frontier

- Purpose: connect trade, travel, caravan and Reaver threats through a believable river crossing.
- Layout: bridge/banks, rest stop, caravan drop-off, lookout and separate camp approach.
- Story: deliveries face ambushes; secure a crossing and expose the captain's camp.
- Needed content: actual wagon model, dock/rest props where appropriate, bridge signage, reeds/rocks below deck rather than on the road.
- Required repair: preserve current vegetation while restoring localized Reaver grading; never assign the old tree-less asset directly.
- Acceptance: bridge entry/exit and camp path work on foot/mount; caravan narration matches its real destination.

### Deathknell and Duskwood — ruins and the crypt approach

- Purpose: darker faction perspective and a readable dungeon foreshadowing route.
- Layout: inhabited safe pocket, ruined outer road, cathedral staging area and clear portal entrance.
- Story: investigate missing patrols/relic disturbances without copying established franchise lore.
- Needed content: state-aware residents, restrained night lighting, warnings before elite threats and a complete return route.
- Optional discovery: memorial/lore clues linking regional enemies to the dungeon.
- Acceptance: dark mood remains navigable; underground Crypts separation/portal return remain intact.

### Rustwater, Bloodrock and Gor'Kresh — industry and organized threats

- Purpose: distinct faction starting life, scrap/crafting activity and stronger cooperative encounters.
- Layout: functioning workshop, safe camp, resource edge, fortress approach and separate combat clearings.
- Story: damaged equipment/trade materials reveal an organized regional threat.
- Needed content: workers/guards, shared-material architecture, bounded smoke/fire and practical service placement.
- Enemy mix: ranged/caster/defender combinations introduced gradually.
- Acceptance: settlements feel different through use and silhouette; no overload of lights/effects or combat crammed into the hub.

### Greenhollow and Ancient Grove — exploration and restoration

- Purpose: forest rest, cartography, gathering and earned mount progression.
- Layout: clear arrival rise, cartographer, provisioner/riding post, farm/work area and trails to three preserved markers.
- Story: chart the frontier and discover signs of the larger threat.
- Needed content: forest-edge thinning, small domestic scenes, wildlife and quiet scenic rewards.
- Optional discovery: a hidden grove cache with visible clues.
- Acceptance: exact mount/marker criteria preserved; Elara remains reachable; camera/grounding near the measured approach rise reviewed.

### Crater and Wyrmcrest — optional danger and finale

- Purpose: distant anticipation, mountain traversal and a learnable apex encounter.
- Layout: readable switchbacks/rest points, safe lookout, boss warning/entry and retreat route.
- Story: collected regional clues explain why the threat gathers here.
- Needed content: progressively harsher ecology, clear landmark silhouettes and bounded weather effects.
- Encounter: two-phase boss combining familiar mechanics, not a sudden collection of new systems.
- Acceptance: continuous foot/mount approach and descent, honest solo/group scaling and one-time reward entitlement.

## 16. Proposed campaign and side-content manifest

Use this as a writing/content outline. Preserve existing IDs for already implemented equivalents; do not duplicate Abe/cartography/Reaver quests under new names. New IDs below are planning keys until admitted to the actual content catalog.

| Key | Proposed objective | Type | Dependency/purpose |
| --- | --- | --- | --- |
| Q01 | Report to the local quartermaster. | Dialogue | Faction introduction and first interaction. |
| Q02 | Practice attack, defense and one ability. | Training | Optional tutorial; dummy grants no farmable rewards. |
| Q03 | Secure the road patrol. | Kill | Existing Abe mission where applicable. |
| Q04 | Recover disrupted supplies. | Collect | Existing objectives and per-player ownership. |
| Q05 | Defeat the road captain. | Boss | Existing Abe gate; exact target identity. |
| Q06 | Deliver news to the faction hub. | Travel/dialogue | Connect starter and services. |
| Q07 | Gather materials for repairs. | Gathering | First useful profession loop. |
| Q08 | Craft or obtain a field provision. | Craft/vendor alternative | All classes/profession choices can proceed. |
| Q09 | Inspect the river crossing. | Discover | Establish Westbank landmarks. |
| Q10 | Escort a supply caravan. | Event | Uses complete caravan state machine. |
| Q11 | Rescue the roadside scout. | Event | Optional unless event availability cannot block story. |
| Q12 | Investigate the Reaver camp. | Explore/interact | Safe trail and visible clues. |
| Q13 | Defeat the Reaver captain. | Boss bounty | Existing exact captain target and locked chest. |
| Q14 | Chart the northern frontier. | Discover | Existing Elara/cartography chain. |
| Q15 | Find the three original riding landmarks. | Exploration | Existing zebra requirements stay separate from Q14. |
| Q16 | Restore disturbed grove markers. | Interact/fight | Silverglen local arc. |
| Q17 | Recover a quarry/workshop shipment. | Collect/encounter | Faction work/industry arc. |
| Q18 | Defend a settlement approach. | Defend | Optional reusable event after template verified. |
| Q19 | Follow traces of the missing patrol. | Clue trail | Duskwood story and dungeon foreshadowing. |
| Q20 | Investigate cathedral relics. | Interact | Dungeon admission through actual story rules. |
| Q21 | Enter the Crypts and defeat its guardian. | Dungeon | Full group/solo loop and distinct reward. |
| Q22 | Report the ruin's discovery. | Dialogue | Resolves regional arc and points to highlands. |
| Q23 | Survey the mountain approaches. | Travel/discover | Teaches actual switchback route. |
| Q24 | Defeat the optional crater threat. | World boss | Separate eligibility from story captain. |
| Q25 | Confront the summit threat. | Finale boss | Combines familiar mechanics. |
| Q26 | Return and receive a cosmetic recognition. | Turn-in | Clear story ending rather than abrupt stop. |
| Q27 | Find an abandoned traveller's cache. | Optional exploration | Environmental clues, bounded reward. |
| Q28 | Complete a profession commission. | Optional craft | Repeat policy and material economics. |
| Q29 | Discover a regional history site. | Optional lore | Original writing and exploration. |
| Q30 | Complete a later elite hunt. | Replay | Only after base adventure and reward framework. |

Do not force every proposed quest into the first 20–30 minutes. The first adventure is a compact subset; the rest belong to the expanded game. Each admitted quest needs a real giver, location, prerequisite, objective source, reward table and test row.

## 17. Encounter and asset manifest

### Encounter kits to produce

| Kit | Unique behavior | Art/animation requirement | Validation focus |
| --- | --- | --- | --- |
| Road raider | Basic melee pressure | Existing compatible weapon/rig | Facing, windup, leash. |
| Highway archer | Ranged LOS | Bow/projectile clips or reviewed compatible substitute | Cover, projectile ownership. |
| Frontier caster | Interruptible spell | Cast pose and distinct warning | Interrupt/cost/cooldown. |
| Camp defender | Slow heavy strike | Shield/heavy weapon and recovery | Attack cone and space. |
| Forest beast | Pounce/flee/return | Licensed compatible animal clips | Collision and motion quality. |
| Grove sentinel | Area denial/control | Distinct original model/material variant | No permanent CC. |
| Ruin watcher | Patrol/alert combination | Ruin-family model and clear silhouette | Detection and path reset. |
| Worksite saboteur | Short burst/reposition | Light equipment variant | No endless kiting. |
| Road captain | First telegraphed elite | Readable captain appearance | Main quest exact target. |
| Reaver captain | Camp-specific encounter | Current rig retained until verified replacement | Trail/arena/reward lock. |
| Crypt guardian | Two-phase dungeon fight | Arena-scale effects and death state | Wipe/late join/loot. |
| Summit threat | Finale pattern combination | Visible large silhouette and bounded VFX | Solo/group balance and exit. |

Do not download one unrelated visual family per kit. Reuse compatible models and distinguish behavior first; character art replacement remains a reviewed pipeline.

### Art/audio production queue

1. Consistent character/weapon sockets and locomotion clips.
2. Real caravan wagon and small escort kit.
3. Bridge transition/support/rail review pieces.
4. Settlement work props: tools, storage, forge/farm elements and road signs.
5. Coherent ground/grass/rock/water material set.
6. Original quest/ability/item/status icon families.
7. Readable enemy telegraph and class effect families.
8. Footsteps, hits, casts, mounts, UI and restrained settlement ambience.
9. Dungeon modular kit using the existing art language.
10. Cosmetic/mount expansions only after the base admission pipeline passes.

Every package records source/license, import settings, modifications, geometry/material/texture costs, collision and rig compatibility. AI-generated meshes are review inputs until topology, deformation, scale and runtime performance are accepted.

## 18. Optional large-game features and prerequisite gates

These remain P3 even though the backlog is ambitious. Existing prototype scripts do not bypass the requirements.

| Feature | Needed before implementation | First complete slice |
| --- | --- | --- |
| Housing/inn room | Stable object/save IDs and decoration ownership | One cosmetic room, no shared economic inventory. |
| Guilds | Persistent identity/storage and permission model | Small trusted group roster with tested roles. |
| Mail | Durable inventory and idempotent transfer | One authenticated attachment/message flow. |
| Player trade | Atomic bilateral item/gold transfer | Confirm/cancel/disconnect-safe two-player exchange. |
| Auction house | Trusted persistent economy and abuse policy | Small bounded listing/buy/expire model on backend. |
| Public group finder | Reliable sessions, player identity and moderation | One activity queue with real players and useful failure handling. |
| PvP duels | Explicit consent, authority and balance rules | Opt-in duel sandbox, no permanent loss. |
| Battleground | Duel rules, trusted match state and team lifecycle | One small objective map. |
| Dedicated servers | Demand, operating budget and headless authority design | Measured server prototype, not a hosting promise. |
| Persistent shared world | Trusted server/storage, backups and operations | One persistent region with migration/recovery tests. |
| Seamless large-world streaming | Demonstrated load/memory requirement | Two-region loading with stable network/save identity. |
| Modding | Validated content schemas and security model | Read-only data pack without remote code execution. |
| Seasons | Reset/claim rules, content tooling and communication | Optional authored event, no forced daily grind. |
| Full voice acting | Stable script, licensed recordings and budget | One scene with text fallback. |
| Other platforms | Stable desktop release and input/UI support | One verified device/build target at a time. |

## 19. Feature-completeness matrix

Each released feature must address all applicable columns; use N/A with a reason rather than skipping silently.

| Feature family | Offline | Host/client | Late join | Saves | Failure cleanup | UI/audio/art |
| --- | --- | --- | --- | --- | --- | --- |
| Character creation | Slot ownership | Valid identity/visuals | Appearance snapshot | Migration | Invalid/missing data | Accurate preview |
| Movement/mounts | Controller feel | Validation/interpolation | Current pose/mount | Unlock/selection | Dismount/teleport | Grounded motion |
| Abilities | Responsive kit | Valid accepted action | Current status | Build configuration | Cancel/death | Readable effects |
| Enemy encounters | Full AI loop | Host target/damage | Current state | Life/reward policy | Evade/wipe | Facing/telegraphs |
| Quests | Complete chain | Individual eligibility | Own journal | Stable IDs | Abandon/reaccept | Dialogue/tracker |
| Loot/inventory | Useful rewards | Atomic claims | Correct ownership | Claim recovery | Full inventory/retry | Clear feedback |
| Events | Start/succeed/fail | Host actors/participants | Current stage | First-clear policy | Leave/reset | Coherent scene |
| Professions | Useful recipe loop | Valid transaction | Node/actor state | Skill/recipes | Interrupted action | Requirements shown |
| Dungeons | Solo completion | Group state | Admission policy | Rewards/unlocks | Wipe/exit | Navigable interior |
| Companions | Opt-in control | Host ownership | Correct actors | Selection if promised | Recall/dismiss | Minimal menu |
| Social extras | Hidden if irrelevant | Correct recipients | Roster state | Explicit scope | Timeout/departure | Bounded notifications |

## 20. Work packets, risk and actual completion

### First twelve implementation packets

1. Reconcile latest source/status and finish the remaining external online acceptance requests.
2. Run a normal-input two-player quest/loot/respawn/mount session, repairing correctness rather than adding features.
3. Verify late join, host/client leave and repeated UI session cycles; record four-player tests distinctly from ordinary playthrough.
4. Obtain the shared-scene window and prepare the reviewed local Reaver repair preserving current trees.
5. Validate the full bridge/camp/Greenhollow foot-and-mount routes, then address demonstrated obstacles/grounding.
6. Fix animation/facing/stride/camera defects using actual motion review.
7. Polish Emberwatch departure and Westbank arrival as the visual/worldbuilding reference.
8. Complete caravan/rescue visuals, routes, failure states, replication and rewards.
9. Connect the compact first story arc with accurate UI and meaningful equipment rewards.
10. Finish roster decision and class/race action matrix; admit fifth class only through a complete slice.
11. Expand regional quests/professions and finish one dungeon, then balance the full loop.
12. Freeze systems, profile, run release acceptance and package the diploma/demo assets.

### Risk register

| Risk | Early signal | Response |
| --- | --- | --- |
| Endless network test | Same logs/last stage repeated | Apply two-attempt/time limits; new hypothesis or independent work. |
| Feature sprawl | More exposed menus than completed loops | Freeze additions; complete the active vertical slice. |
| Silent asset loss | Old terrain/prefab swaps remove new content | Compare data/GUIDs; preserve current content; targeted new variant. |
| Multiplayer illusion | Host works, client rewards/damage don't | Test actual client behavior and ownership separately. |
| Save disappointment | Online rewards disappear unexpectedly | Disclose session-only policy before play or implement/test persistence. |
| Art mismatch | New model has unrelated proportions/materials | Review family consistency before import/replacement. |
| Poor traversal | Terrain samples look valid but controller fails | Walk/mount/camera tests on actual geometry. |
| Unplayable performance | Hub/fight spikes despite average FPS | Fixed worst-case profiling and bounded effects/AI. |
| Collaboration conflict | Both agents edit same scene/runtime file | Explicit file ownership and integration window. |
| False confidence | Old green receipts used for new build | Tie evidence to current source/configuration and mark NOT RUN. |

### Done means

The feature works through normal player input; valid and invalid paths behave correctly; host/client/late-join ownership is consistent; promised persistence is tested; art/audio/UI are coherent; cleanup and performance are bounded; documentation matches the delivered build. A code file, screenshot, compile pass or single narrow smoke result is only part of that evidence.

### Required per-packet report

```text
Packet / ticket IDs:
Files actually changed and ownership:
Implemented player-visible behavior:
Tests actually run, source/build identity and observed results:
NOT RUN / known limitations:
Failed-attempt count and blocker evidence:
Next smallest exact action:
```

Keep the game's long-term scope ambitious and the next change concrete. Complete connected, tested experiences before adding another unfinished system.
