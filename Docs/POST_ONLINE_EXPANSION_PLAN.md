# Riftbound — post-online production plan

Revision 1, 2026-11-16. Planning targets, not a list of implemented features.

## 1. Next result

Turn the existing networked prototype into a coherent playable adventure: responsive characters, readable fights, useful rewards, believable settlements and a clear journey through the world. Expand land and systems only after that experience works.

This file operationalizes RIFTBOUND_MASTER_PLAN.md and RIFTBOUND_FEATURE_BACKLOG.md. Existing ticket IDs remain valid; the packets below group them into reviewable deliveries. It does not replace observed results in either agent's status/handoff.

### Inspected starting evidence

- Antigravity branch inspected locally: overnight/antigravity-online-20261116, HEAD f415f351da708b065c3cc1b65d1c1769c48cf577. Its handoff reports a successful standalone, 145 offline regression passes, authority/progression/recovery/event checks and four-player Relay with fifth-player rejection.
- Those are reported results from its handoff, not new reruns by Codex. Manual interactive GUI adventure and two-PC cross-network acceptance remain NOT RUN there. Session-only online progression and no host migration remain explicit product policies.
- Codex branch: overnight/codex-world-20261116. Cache grounding and protected bridge-bank repair are pushed in ad8e019; settlement path changes are locally applied and awaiting their delivery checkpoint.
- Codex current-scene geometry receipt: 20 REACHED, 0 BLOCKED for five route sections, walk/mount and both directions. This does not establish normal-input, camera, animation, AI or complete north-road acceptance.
- Codex fresh QuestRouteSmokeTest result at approximately 07:12 Sofia: 145 PASS, 0 FAIL. A new world-branch standalone is still required.
- The existing map is four 384 m tiles, 768 x 768 m. New objects inside those bounds are world enrichment, not geographic expansion.

### Authority and time

The current Codex shift retains start 2026-11-15T22:36:39.4470822Z and deadline 2026-11-16T06:36:39.4470822Z. Feature freeze has passed. Finish verification/build/handoff; do not start the new features below during that frozen shift.

The user's request to enlarge the plan authorizes documentation. It does not silently authorize every future terrain, scene, prefab, art replacement or backend change listed here. Submit the appropriate next-session prompt to start its bounded implementation scope. A new session is not automatically a new eight-hour authorization; record the actual work window explicitly when one is requested.

Antigravity's completed overnight record is immutable history. The next assignment uses a new gameplay journal and branch descended from its finished online branch. Do not overwrite its successful handoff or rerun A00–A07 merely to stay busy.

## 2. Two-agent ownership

| Area | Codex | Gemini / Antigravity |
| --- | --- | --- |
| Scene, terrain, road meshes, environment placement | Sole editor of approved targeted edits | Read-only; coordinate exact placement requests |
| World audit and authoring tools | Own | Read-only |
| Runtime movement, facing, animation playback, camera | Read-only unless handed over | Own bounded corrections; preserve online ownership |
| Runtime combat, quests, enemies, interactions, HUD/audio behavior | Placement/design contracts only | Own within activated packet |
| FBX import, avatars, animator assets, model replacements | Review/proposal until assigned | Diagnose; no silent reimport or roster swap |
| New quests/NPC placement/encounter definitions | Agree placement/content contract first | Agree runtime/IDs/rewards contract first |
| New terrain tiles, dungeon scenes, prefabs | Preview and separate scene approval | No direct edits |
| Network foundation | Do not touch | Preserve; regression after dependent gameplay edits |
| Durable online saves, backend, account/economy services | Design only | Design only until separate policy/authority approval |
| Journals | Docs/Production/OWN_CODEX_WORLD_RUN.md and HANDOFF.md | Docs/Production/OWN_GEMINI_GAMEPLAY_RUN.md and HANDOFF.md |

Do not edit each other's checkout. Read-only inspection is allowed. Stop before an actual file overlap, send the exact requested change, and continue independent work. The final integration uses a third clean checkout only when requested; neither worker merges main.

## 3. Production order and gates

| Milestone | Player outcome | Required exit evidence |
| --- | --- | --- |
| M0 — close overnight | Separate tested world and online branches | Fresh builds, exact commits, preserved originals, handoffs |
| M1 — feel and clarity | Movement, facing, interaction and first fight feel intentional | Normal-input locomotion/interaction matrix; online regression for changed behavior |
| M2 — first adventure | Fresh character completes a connected 20–30 minute target journey | Measured first run, rewards/reload/leave, no debug help; duration is a target, not a claim |
| M3 — inhabited frontier | Reference settlements and roads convey purpose | Grounded props, clear services/routes, authored visual review, coherent audio |
| M4 — regional arcs | Three distinct regional stories with useful optional exploration | Complete branchable objectives, encounters and rewards; all IDs validated |
| M5 — compact RPG | Stable roster, gathering/crafting and one finished dungeon | Solo/co-op loop, economy checks, save policy and reset rules |
| M6 — genuine expansion | One approved adjacent land parcel adds a complete destination | New bounds, protected seams, traversal, content and performance gates |
| M7 — release/diploma | Reproducible demonstration and defensible implementation report | Combined tested candidate, licenses, user instructions and rehearsed fallback |

Parallelism: Codex M3 preparation may run beside Gemini M1/M2. M4 content requires the location/ID contract. M5 and M6 do not all start together. Complete one active implementation packet per worker; a blocked external test may have one independent replacement packet.

## 4. Immediate Gemini assignment — gameplay feel and first adventure

Start with GP01, then GP02 and GP03. Inspect before changing anything; an already-correct behavior should be recorded and skipped. Reuse current models, controls and assets.

| Packet / backlog links | Concrete work | Acceptance and boundary |
| --- | --- | --- |
| GP00 / FND, QA | Preserve finished online branch, record gameplay branch/base, dirty state, known controls and test identities. Read both handoffs. | Existing build/results retained; no world edits; no new clone over existing files. |
| GP01 / MOVE, ANIM, ENEMY | Diagnose the reported sliding/sideways locomotion and wrong enemy facing. Separate actor forward, visual forward, clip root motion, state looping, turn blending and network interpolation. Record which source actually fails. | Compare idle/run/walk/back/strafe/turn/attack/death on local actor, remote actor and an enemy. Small runtime fix only when demonstrated. If cause is imported rig/clip, prepare exact asset proposal instead of flipping every model. |
| GP02 / MOVE, CAMERA, ACCESS | Refine existing run-default/walk-toggle behavior, start/stop response, diagonal speed and camera steering. Inspect mount/dismount control restoration. | Normal W/A/S/D, walk toggle, reverse, menu-open input, summon/cancel/dismount and post-respawn movement; no remote avatar input, camera or audio listener. Keep control scheme stable unless a change is documented. |
| GP03 / UI, QUEST, TALK | Make one clear contextual interaction prompt. Pick nearest eligible target deterministically; report out-of-range/busy/locked state briefly. Give dialogue useful accept/turn-in/cancel behavior. | Abe, vendor, chest, landmark, revive and mount interaction cannot compete for the same press. Dialogue blocks combat/movement as intended and closes without a stuck cursor. No permanent additional HUD panel. |
| GP04 / FIGHT, FX, SOUND | Make selected target, ability range, cooldown, resource and hit outcome readable. Audit repeated input and animation completion; effects follow accepted gameplay state. | Valid/invalid cast, target death mid-cast, out-of-range, global/cast cooldown, moving cancellation where applicable. Client prediction never grants unauthorized damage/reward. |
| GP05 / QUEST, STORY, ACCESS | Walk the existing first adventure through ordinary UI. Repair missing objective text, stale tracking, unusable turn-in, reward clarity and recovery from interruption. | New isolated character accepts, progresses, turns in, equips reward, leaves/reloads according to policy. No forced teleport/debug-complete counts as a normal playthrough. Do not add a second quest framework. |
| GP06 / ENEMY, BOSS | Stabilize existing melee/ranged/caster behavior: face target, sensible windup/recovery, obstacle/LOS handling and return-home cleanup. | Each chosen archetype has observable counterplay; evade resets correctly; late join sees consistent vitals. No new scene spawns or inflated HP to simulate difficulty. |
| GP07 / UI, TEAM | Refine current HUD hierarchy: essentials visible, secondary panels opt-in, brief feedback bounded, co-op panel compact. Preserve settings and keyboard navigation. | Check 1280x720 and 1920x1080; ordinary gameplay, target, dialogue, bag, death and party. No overlapping hotkey/modal ownership. Mark unrendered sizes NOT RUN. |
| GP08 / AUDIO, FX | Remove duplicate loops/listeners, cap repeated impact sounds, tune near/far attenuation and prioritize important cues using existing audio. | Actual listening pass at normal volume where available; missing device is NOT RUN. Audio volumes/settings survive promised local persistence. No generated placeholder noise claimed as final sound design. |
| GP09 / ALLY, TEAM | Verify companions start dismissed and remain an explicit opt-in. Call/dismiss states and availability are readable. | Fresh offline character has no default crew; session ownership/host authority respected; no duplicate companions after reload/leave. Coordinate actor/prefab needs rather than editing scene. |
| GP10 / CLASS, DATA | Inventory Warrior/Mage/Rogue/Paladin abilities and find duplicate/unused actions. Improve existing identity one class at a time. | Class action matrix records role, cooldown, damage/heal/control and cost. Every changed ability works offline and as client; fifth class remains separately gated. |
| GP11 / ECON, ITEM | Improve existing item comparison, equip feedback, full-bag failure and vendor affordability/transaction clarity. | Unknown items rejected; repeated purchase/claim cannot duplicate; rejected transaction leaves money/items unchanged. Preserve session-only online policy. |
| GP12 / EVENTS, LIFE | Review existing Caravan/Ambush as playable content rather than repeating replication smoke. Make stage, participation and success/failure understandable. | Encounter can start, resolve/fail and clean up; late join and reward exactly-once remain intact. Physical route/prop changes requested from Codex. |
| GP13 / SAVE, RECOVER | Test existing offline identity isolation, reload/death/settings and co-op session-only disclosure. | No offline save writes from online play, no test overwrite of user characters, no silent online persistence promise. |
| GP14 / PERF, QA | Profile a repeatable gameplay route/fight at fixed settings. Correct one measured runtime bottleneck only if meaningful. | Before/after frame-time evidence with hardware/settings and same actors; headless service telemetry is not rendered FPS. |
| GP15 / SHIP, QA | Package gameplay branch, run dependent regressions, deliver an interaction/first-adventure build and handoff. | Exact commit/build/artifact identity, tests and NOT RUN; no main merge or world overwrite. |

GP01 diagnostic receipt must include actor/model/clip names, current forward axes, root-motion setting, active state/transition and a short reproduction. Quantify foot sliding against ground/stride where captures permit. Do not claim that a state name or loop flag proves animation quality.

**Concrete GP07 repro, 2026-11-16 07:39 Sofia:** Codex world standalone from 9c632c9 launched and rendered title/character selection at 1280x720. Session-mode labels are visibly clipped in their narrow buttons. Reproduce on Gemini's newer runtime branch before fixing; the observation does not establish that its latest UI has the same defect. Character preview rendered; no character was entered or save edited during this check.

## 5. Codex current finish and next world assignment

The current overnight run finishes CW00 only. CW01 onward is the next production queue, subject to the scope/scene gates below.

| Packet / backlog links | Concrete work | Acceptance and boundary |
| --- | --- | --- |
| CW00 / GROUND, SHIP | Close current cache/bridge/settlement repairs: saved-scene preservation, seams, regression, new standalone, push and handoff. | Original terrain unchanged; 813 trees/15 prototypes preserved; 145 regression assertions remain intact; distinguish geometry from input/visual acceptance. |
| CW01 / TOOL, GROUND | Reconcile route samples, visible path meshes, current terrain paint and runtime map requests. Identify any obsolete road still visually directing through scenery. | Record exact offending segment. Change only approved local mesh/placement; terrain paint needs current-data clone/footprint approval. No whole-map repaint. |
| CW02 / GROUND, TRAVEL | Validate full Emberwatch–Highspire–Westbank–Reaver and Emberwatch–Greenhollow outbound/return. Include departure/arrival and side lanes. | Foot/mount, summon/dismount, camera, bridges/rails, NPC/chest approach and intended jump/landing. Each evidence type separately labelled. |
| CW03 / NATURE | Review the 26 interpolated Reaver tree candidates and any new bridge-footprint candidates. Classify logs, roots, rocks and bushes by actual support/intended orientation. | Per candidate: prototype/location, visible base, lean/intentional status, collision relationship and disposition. No mass vertical rotation or deletion; unresolved visuals stay unresolved. |
| CW04 / TOWN | Prepare Emberwatch reference settlement: clear arrival, social/service centre, homes/work/storage and departure. | Existing buildings/services retained, no repeated prop grids, grounded small clusters and readable route. Publish placements/captures before broader edits. |
| CW05 / TOWN, STORY | Complete Willow Rest as a small supply/rest story: cache access, bridge structure, riverbank ecology and trail readability. | Chest reachable/off-route; support bases coherent; waterline intact; route and combat clearance unchanged. |
| CW06 / TOWN, REGION | Make Highspire streets connect well/plaza, gate, services and departure without clipping through houses/walls. | Major architecture preserved; actual gate approach has walking/mounted/camera clearance; signs agree with route coordinates. |
| CW07 / NATURE, TOWN | Frame Greenhollow as forest-edge riding/work post: empty service apron, marker access, farm/work details and grove transition. | Three discovery IDs/zebra criteria preserved; useful open space and arrival path; no new collider obstruction. |
| CW08 / LIGHT, ART | Establish existing-asset environment style sheet: proportions, material palette, vegetation density, stone/timber language and weathering. | Four reference views at matched settings, reuse/provenance ledger and approved candidate list. No new character family/model swap. |
| CW09 / LIGHT, SOUND | Prepare bounded lighting/ambience proposals: sun/shadow readability, fog depth, river/forest/town acoustic zones, hidden authoring gizmos. | Distinguish editor gizmos from rendered light meshes. Changes need repeatable rendered comparison and performance evidence; no effect stacking without measurement. |
| CW10 / REGION, STORY | Produce location briefs and authored staging for the first three regional arcs. | Route, services, landmarks, encounter pockets, safe resting points and asset needs agreed with Gemini before runtime content. |
| CW11 / TOOL, QA | Extend read-only validation for missing support, intersecting travel lanes, bridge transitions, service approach and persistent-ID placement contracts. | Accurate uncapped totals with bounded examples and known coverage. Synthetic fixtures remain labelled; no automatic scene mutation. |
| CW12 / PERF | Measure world hotspots at identical camera/settings and address a demonstrated environmental cost. | Report renderers/draw calls/shadows/trees/frame-time as available; no global LOD/culling change based only on guesses. |
| CW13 / REGION | Prepare one adjacent terrain parcel as a design/preview, with new bounds and two entry routes. | Extension lies outside current 768x768 bounds; seams, heights, rivers, terrain references, vegetation and budget reviewed. No generated tile admitted until separately approved. |
| CW14 / CRYPT | Plan one complete dungeon space using existing licensed kit: entrance, combat rooms, rest/readability, boss arena, reset/exit and fall recovery. | Solo/mount restrictions and group arrival agreed; no portal destination changed before runtime contract. |
| CW15 / SHIP | Deliver world production checkpoint with screenshots, route evidence, new build and exact integration request. | Scene released, no runtime-network files edited, no main merge, original assets preserved. |

## 6. Reference-route contracts

Current corrected local samples, not a demand to overwrite Antigravity runtime files:

| Segment | X/Z samples | Integration note |
| --- | --- | --- |
| Emberwatch departure | (-15,0), (-25,-5), (-34,-11) | Two original west fence segments retained inactive; check connection to highway. |
| Highspire north exit | (-200,-60), (-202.5,-57.5), (-204,-15), (-225,0) | Avoid well at approximately (-200,-56); continue to Westbank road. Ground-sample Y. |
| Westbank crossing | Road toward (-240,50), bridge centre near (-255.46,50), then (-285,50) | Deck y=6.0; protected bank clone; do not flatten the river. |
| Reaver approach | (-285,50) to (-283,78) | Existing protected grade below 35 degrees; preserve actors/IDs. |
| Willow Rest cache | Root approximately (-291,52.5), y=7.19 | ID willow-rest-cache, root/hinge/rewards preserved; approach interaction acceptance pending. |
| Greenhollow arrival | (-17,368), (-27,386), (-36,395) | Stop beside marker, not at its centre (-40,400); retain discovery radius/ID. |

Before sharing coordinates, confirm them against current branch scene. Runtime navigation/map adoption is a Gemini-owned change and must preserve all unrelated route branches. A new footway does not automatically erase older terrain paint; report that separately.

## 7. Expanded regional content specification

These are proposed targets. Reuse existing names/IDs for compatibility; final original faction display names and lore are a separate content decision. Do not add all quests at once.

| Region / order | Purpose and physical story | Candidate playable content | Exit evidence |
| --- | --- | --- | --- |
| Emberwatch / 1 | Safe working frontier village; supplies visibly leave for outposts. | Existing Abe introduction refined; optional delivery/recovery; first useful equipment choice. | Fresh player finds giver/service/departure without debug help; no forced default crew. |
| Ashwood / 1 | Gradual transition from maintained road to contested woodland. | Patrol lesson, optional hidden cache, recoverable roadside event. | Sightlines teach combat; optional danger does not block mandatory road. |
| Highspire / 2 | Compact administrative/military hub with believable storage/work. | Report delivery, training or supply request, lead toward Westbank. | Services usable through streets; no quest line through well/wall. |
| Frostpeak / 3 | Highland quarry/work camp with shelter and resource logic. | Rescue/recover tools, short gathering-to-crafting tutorial, sturdy melee enemy. | Steep routes support advertised travel; shelter and work props grounded. |
| Silverglen / 3 | Arcane grove with intentional clearings and ecological disturbance. | Restore focal point, inspect clues, caster/interrupt lesson. | Readable magic hazard; no mandatory pixel-hunt interaction. |
| Westbank / 2 | River supply crossing with a useful rest landmark. | Existing cache, Reaver bounty, optional damaged-supply story. | Bridge and cache work both directions; no duplicated rewards. |
| Reaver camp / 2 | Threatening camp built around an accessible approach. | Existing captain/guards, optional rescue/recovery variant later. | Enemies face targets; arena/leash supports fair combat; late join state valid. |
| Greenhollow / 2 | Riding frontier post, provisioner, forest/farm boundary. | Existing three discoveries and mount unlock; grove repair arc. | Arrival beside marker, unlock legitimate once, all service aprons clear. |
| Ancient Grove / 3 | Quieter sacred/exploration space with selective environmental damage. | Clue sequence, restoration or optional rare encounter. | Distinct landmark silhouette; clear return route; no dense random props. |
| Deathknell / 4 | Reclaimed ruin/starting area with shelter-to-danger gradient. | Investigate remains, protect a worker, reveal crypt lead. | Faction intro can be completed alone; stable persistent IDs. |
| Duskwood exterior / 4 | Pilgrim/expedition staging before underground danger. | Entrance preparation, missing expedition story, dungeon unlock. | Portal/group arrival/exit is unambiguous and recoverable. |
| Rustwater / 4 | Small industrial salvage site: sorting, repair, water and access. | Salvage/craft loop, malfunction or hostile patrol. | Resource quantities/economy bounded; work detail does not obstruct NPCs. |
| Bloodrock / 4 | Organized military camp linking industry and the east road. | Supply protection, commander objective, distinct melee/ranged pack. | Group pull/leash is fair; reserve safe road width and escape space. |
| Gor'Kresh / 5 | Second faction centre with its own silhouettes and civic purpose. | Faction report, class/service introduction, eastern adventure lead. | Same service quality as Highspire without copying its prop layout. |
| Crater / 5 | Optional dangerous basin with advance warning and evacuation space. | Existing world boss refined, participation/reward gate. | Readable telegraphs, optional access, no unavoidable road kill zone. |
| Wyrmcrest / 5 | Mountain journey with staging overlooks and final landmark. | Switchback traversal, summit encounter, return travel reward. | No cliff shortcut required; mount/camera/fall recovery verified. |
| New adjacent parcel / 6 | First genuine land extension, proposed after existing core works. | One hub, one environmental problem, two encounters and one optional landmark. | Coordinates outside current bounds, approved seams/tile, complete outward/return loop. |

Each region brief must supply an overhead layout, playable footprint, route widths, focal views, asset list, NPC/service/encounter points, quest IDs/rewards, online state and a test route. Numbers of quests/enemies are content targets chosen during that brief, not automatic implementation quotas.

### First true expansion proposal — Stormglass Reach

Working original region name, design only. Proposed first added tile: **x=64..448, z=608..992**, 384 x 384 m north of the current northeastern tile. This adds 147,456 square metres of terrain, 25% of the existing four-tile area; the occupied map becomes five tiles in an irregular footprint, not a complete larger rectangle. Coordinates/elevations are unverified proposals and must survive preview review.

- Southern entry A: an approved Wyrmcrest northern saddle, proposed crossing near (250,608), connecting the summit journey to a safer descending trail.
- Southern entry B: a valley route, proposed crossing near (110,608), with its existing-map approach surveyed separately. Two entries must not become two untested straight lines over cliffs.
- Glasswater Landing, proposed hub near (190,730): sheltered work/storage/service cluster, safe arrival, return travel and the region's main giver.
- Broken quarry near (320,790): visible resource industry and one melee/ranged encounter pocket, with room to disengage.
- Stormglass Beacon near (240,920): distant navigational silhouette and a small finale arena; lighting must remain readable without expensive permanent effects.
- Optional quiet landmark and cache placed after sightline/route review, outside mandatory combat/arrival clearances.

| Contract | Complete content slice | World/runtime handoff |
| --- | --- | --- |
| SR-00 | Tile preview, existing-edge preservation and two candidate arrival routes | Codex produces bounds/heights/seams/vegetation/performance preview; approval before scene admission. |
| SR-01 | Arrive and report at Landing | Codex service/approach layout; Gemini ID, dialogue, current-objective and return-state handling. |
| SR-02 | Discover what interrupted quarry supplies | Environmental clues plus a valid objective chain, useful map guidance and no required random drop grind. |
| SR-03 | Recover material or defend a short work action | One existing gathering/defend pattern; full bag, interrupt, fail/reset and co-op eligibility defined. |
| SR-04 | Restore the beacon through a readable encounter | Existing enemy/interaction kits first; host stages, late join snapshot, fair telegraphs and exactly-once reward. |
| SR-05 | Return, select a useful reward and unlock return travel | Existing item/travel system reused; duplicate turn-in rejected; local/online persistence follows disclosed policy. |
| SR-06 | Optional exploration branch | One landmark/cache with an authored view and safe way back; optional means not blocking main completion. |
| SR-07 | Release the region | Full approach/return foot/mount/camera route, seams/trees/original assets, ordinary-input adventure and integrated regression. |

Do not start a second expansion tile while SR-07 is incomplete. The region needs a playable problem, resolution and return loop before more terrain or additional boss systems.

## 8. Larger feature queue — implement complete loops

All entries below are planned. Existing prototypes must be audited/reused before introducing parallel systems. P1 follows M1/M2; P2 follows the regional/gameplay gates; P3 requires a separate feasibility decision.

### Character, combat and enemies

| ID / priority | Feature | Dependency and completion check |
| --- | --- | --- |
| EX-C01 / P1 | Locomotion state quality | GP01; planted-feet review across actions, no sideways actor oscillation. |
| EX-C02 / P1 | Melee readability | GP04; facing/range/windup/impact agree on local and remote actor. |
| EX-C03 / P1 | Caster feedback | Interrupt/cancel/success cues distinct; UI and host costs agree. |
| EX-C04 / P1 | Enemy archetypes | One coherent melee, ranged and caster kit before variants; LOS/leash/death/reset verified. |
| EX-C05 / P2 | Four-class tuning sheet | Existing class action matrix and measured fights; preserve IDs and saved equipment. |
| EX-C06 / P2 | Fifth-class vertical slice | Roster/role approval; full creation-to-combat-to-save/client support before exposure. |
| EX-C07 / P2 | Equipment identity | Useful choices and visible/stat changes where current assets support them; no reward inflation. |
| EX-C08 / P2 | Talent choices | Existing framework audit; one meaningful tier and reset policy before a large tree. |
| EX-C09 / P2 | Elite encounter modifiers | Readable telegraph/counterplay and bounded combinations; authority and loot eligibility. |
| EX-C10 / P2 | First multi-phase boss | Thresholds/stages, wipe/reset, late join, simultaneous defeat/kill and rewards. |
| EX-C11 / P3 | Subclasses | Stable base roster/XP/gear; complete one subclass without invalidating old saves. |
| EX-C12 / P3 | Companion roles/pets | Opt-in ownership and collision/path budget; no extra permanent UI or forced party. |

### Questing, story and world activity

| ID / priority | Feature | Dependency and completion check |
| --- | --- | --- |
| EX-Q01 / P1 | Connected first adventure | GP05 plus route contracts; launch-to-reward without debug action. |
| EX-Q02 / P1 | Clear journal/tracking | Correct current giver/objective/turn-in, optional untrack, no stale marker after completion. |
| EX-Q03 / P2 | Investigate/interact objectives | Existing objective framework, stable targets, interruption/reload and client validation. |
| EX-Q04 / P2 | Escort/defend content | Stuck recovery, abandon/fail/reset and late join; host simulates actors. |
| EX-Q05 / P2 | Three regional story arcs | Separate beginning/problem/climax/reward, useful optional branch, no mandatory circular prerequisite. |
| EX-Q06 / P2 | Original faction introductions | Final display/lore decision; saved faction keys kept stable, comparable starter quality. |
| EX-Q07 / P2 | Environmental clue chain | Diegetic props and accessible fallback clues; no required unreadable tiny text. |
| EX-Q08 / P2 | Civilian work routines | Small bounded schedules near hubs; no road/service obstruction, correct pause/cleanup. |
| EX-Q09 / P2 | Wildlife ecology | Clear harmless/hostile species behavior and bounded population; no uncontrolled spawning. |
| EX-Q10 / P2 | Expanded event library | Existing Caravan/Ambush complete first; one new event with eligible participants and reset policy. |
| EX-Q11 / P3 | Reputation and faction response | Define sources/sinks/rewards/persistence; no session-only reward misrepresentation. |
| EX-Q12 / P3 | Repeatable contracts | Endgame gate, anti-farm/reward caps and explicit reset rules; no live-service scheduling requirement. |

### World, art, graphics and sound

| ID / priority | Feature | Dependency and completion check |
| --- | --- | --- |
| EX-W01 / P1 | Corrected reference road network | CW01/CW02; visual road, geometry and runtime guidance agree. |
| EX-W02 / P1 | Tree/prop disposition report | CW03; candidates classified by instance, logs preserved intentionally. |
| EX-W03 / P1 | Settlement visual grammar | CW04–CW08; workplaces/storage/homes/services have purpose and clear circulation. |
| EX-W04 / P1 | Bridge/river coherence | Supports, shoreline and bank vegetation grounded; route/camera remain safe. |
| EX-W05 / P2 | Region ecology palettes | Licensed existing asset family, biome transitions, landmark silhouette and density budgets. |
| EX-W06 / P2 | Lighting quality tiers | Same route at agreed settings; readable gameplay and measured cost. |
| EX-W07 / P2 | Day/night design | Define gameplay impact first; synchronized phase if used online, service/quest readability at night. |
| EX-W08 / P2 | Weather variation | Bounded VFX/audio, visibility/accessibility and performance; no unexplained gameplay penalties. |
| EX-W09 / P2 | Surface footsteps | Existing surface/material mapping, no duplicate events, actual listening and landing cases. |
| EX-W10 / P2 | Music/ambience transitions | Licensed content, controlled crossfades/priorities, pause/death/loading and volume settings. |
| EX-W11 / P2 | Consistent character asset family | Reviewed free-license shortlist, rig/animation/material proof before roster replacement. |
| EX-W12 / P2 | Genuine adjacent land extension | CW13 approval, new bounds/seams, complete destination and return loop. |
| EX-W13 / P3 | Further region production | First extension verified; one location contract at a time, no empty terrain as completion. |
| EX-W14 / P3 | Vertical exploration | Safe climbing/overlooks/caves only with traversal, camera and recovery rules. |

### Crafting, loot and economy

| ID / priority | Feature | Dependency and completion check |
| --- | --- | --- |
| EX-E01 / P1 | Reliable existing item loop | Receive, inspect, equip/use, reject invalid action and reload per policy. |
| EX-E02 / P2 | Gathering loop | One resource family with shared/personal eligibility, depletion/reset and inventory-full behavior. |
| EX-E03 / P2 | One crafting profession | Gather -> recipe -> valid ingredients -> useful item; cancellation and exactly-once transaction. |
| EX-E04 / P2 | Recipe discovery | Requirements and source clear; unknown/duplicate recipe safely handled. |
| EX-E05 / P2 | Equipment rarity/stat budgets | Measured time-to-kill/survivability and reward pacing; no arbitrary exponential numbers. |
| EX-E06 / P2 | Vendor inventory/buyback proposal | Bounded stock/prices, accidental-sale recovery and authority; existing system first. |
| EX-E07 / P2 | Resource/economy balancing | Sources/sinks, affordability on first adventure and duplication checks. |
| EX-E08 / P3 | Direct cooperative trading | Explicit trust/transaction/persistence policy, atomic accept/cancel and disconnect recovery. |
| EX-E09 / P3 | Online durable characters | Separate storage/auth/threat/cost/migration design; no trusted client-file upload shortcut. |
| EX-E10 / P3 | Guild/mail/auction systems | Backend and abuse/ownership feasibility; hidden until operationally supportable. |

### Travel, dungeon and replay

| ID / priority | Feature | Dependency and completion check |
| --- | --- | --- |
| EX-D01 / P1 | Existing mounts polished | Summon/cancel/travel/dismount/combat/death/reset with valid unlocks and remote view. |
| EX-D02 / P2 | Travel network clarity | Existing flight/portal nodes, destination preview, arrival support and return route. |
| EX-D03 / P2 | First dungeon complete | CW14 plus runtime contract; entrance, encounter sequence, boss, reward, exit and wipe/reset. |
| EX-D04 / P2 | Group admission/late join | Agree session/instance model before implementation; no accidental shared-world duplication. |
| EX-D05 / P2 | Encounter journal | Useful discovered mechanics and optional spoiler disclosure, minimal HUD impact. |
| EX-D06 / P2 | Exploration collection loop | Distinct visible landmarks and useful reward; save IDs retained and duplicate claims safe. |
| EX-D07 / P3 | Difficulty variants | Completed base dungeon, scalable mechanics rather than HP-only inflation. |
| EX-D08 / P3 | Additional mounts/cosmetics | Coherent licensed assets and unlock loop; collision/camera/remote attachment proof. |
| EX-D09 / P3 | Optional challenge contracts | Completion/reset/reward contract and solo/group fairness. |
| EX-D10 / P3 | Dedicated servers/PvP | Separate architecture and operating-cost decision; not part of private co-op polish. |

### UX, reliability and delivery

| ID / priority | Feature | Dependency and completion check |
| --- | --- | --- |
| EX-U01 / P1 | Minimal HUD and modal ownership | GP03/GP07; no competing interactions, stuck cursor or input leaking behind windows. |
| EX-U02 / P1 | Title/selection quality | Existing rotating backgrounds and faction preview: correct scale/lighting/centering, selection retained as promised. |
| EX-U03 / P1 | Onboarding | Contextual movement/target/interact/mount guidance; dismissible and not repeated forever. |
| EX-U04 / P2 | Rebinding/accessibility | Existing input system, readable scale, contrast and non-color-only cues; no binding collisions. |
| EX-U05 / P2 | Localization structure | English/Bulgarian strings where planned; layouts and fonts tested, no broken saved IDs. |
| EX-U06 / P1 | Offline save recovery | Character isolation, atomic/recoverable writes where existing design permits, corrupted/missing input behavior. |
| EX-U07 / P1 | Content validation | Duplicate/missing IDs/targets, impossible prerequisites, placement contracts and useful paths in report. |
| EX-U08 / P1 | Reproducible builds | Fresh clone plus LFS/vendor source, unique artifact directory, source SHA and exact test results. |
| EX-U09 / P1 | Rendered performance matrix | Fixed route, combat, hubs, effects, shadows and terrain at controlled settings/hardware. |
| EX-U10 / P2 | Load/re-entry stability | Several bounded scene/session cycles with subscription/object/save cleanup; no unbounded soak loop. |
| EX-U11 / P1 | Cross-PC user acceptance | Two actual PCs/networks; GUI connection, adventure, leave/rejoin, exact outcome recorded. |
| EX-U12 / P1 | Diploma evidence pack | Original contribution, architecture, multiplayer authority, world pipeline, test limitations and license ledger. |
| EX-U13 / P1 | Competition demo | Rehearsed playable path, known-good offline fallback and honest feature list. |
| EX-U14 / P2 | Release support package | Controls/setup, changelog, known limitations, source/build hashes, backups and rollback instructions. |

## 9. Content handoff contract

Before one agent implements content needing the other agent's files, write:

```text
Contract ID / packet / branch / source SHA:
Stable IDs (existing or proposed; no rename of saved keys):
World location, approach, useful empty space and route clearance:
Actor/prefab ownership and exact scene request:
Quest stages, eligibility, failure/reset and rewards:
Offline save behavior / online session state / late join behavior:
UI text / audio/VFX asset references and licenses:
Tests: normal input, invalid action, repeat claim, reload/leave:
Approved scope / unresolved decisions / integration order:
```

Default no new scene writes by Gemini. A Codex placeholder location is not permission to spawn a new runtime event; a runtime quest definition is not permission to move a persistent giver.

## 10. Testing and budgets

- Maintain implemented / verified / blocked / NOT RUN separately. Tie every receipt to source and build identity.
- Geometry probes, queued Input System tests, recorded/manual gameplay, synthetic fixtures, headless Relay and actual two-PC testing are different evidence. Never substitute their labels.
- Preserve 145 existing regression assertions and all terrain/ID/authority tests. Add meaningful tests for changed failure behavior; avoid tests merely restating constants.
- Before a test: hypothesis, expected state, attempt count, timeout, last successful stage. At most two unchanged failures; ten minutes with no new evidence stops the attempt; twenty minutes caps unchanged-blocker investigation. Carry counts across sessions.
- New builds use unique directories. Do not overwrite overnight artifacts, user saves or original terrain. Only stop process IDs launched by that worker.
- Performance targets require a measured baseline and agreed settings. Same-machine headless Relay frame telemetry is not evidence of rendered-world performance. Do not invent FPS targets already achieved.
- Keep at least the final quarter of any newly agreed work window for regression/build/handoff. Stop additions when remaining time cannot cover validation.
- No paid dependencies, speculative package upgrades, backend/billing changes or ripped game assets.

## 11. Integration and branch policy

1. Finish/push both overnight branches; maintain their build/handoff records.
2. Publish this documentation update on Codex's branch. Gemini fetches/reads POST_ONLINE_EXPANSION_PLAN.md, RIFTBOUND_MASTER_PLAN.md, RIFTBOUND_FEATURE_BACKLOG.md, OVERNIGHT_8_HOUR_PLAN.md, OVERNIGHT_PROMPTS.md and WORLD_ZONES.md from that ref; it must not cherry-pick a mixed world commit or copy a scene to obtain instructions.
3. A new Gemini gameplay session starts from its finished online HEAD on production/gemini-gameplay-20261116. Preserve the old branch. Refuse to overwrite an existing branch; inspect its journal and resume if correct.
4. Codex next world session starts only after its current handoff, on production/codex-world-20261116, preserving the finished overnight branch. If current checkout is dirty, finish/checkpoint it first; never reset it to switch branches.
5. Read opposite branch handoffs before any cross-system assumption. Use exact content contracts and coordinate files explicitly.
6. When the user requests integration, create a third clean integration checkout/branch, review overlaps, combine both streams and rerun dependent checks. Do not merge main automatically.
7. A combined build must include integrated runtime route guidance and world geometry before claiming one coherent release. Separate branch successes do not establish combined compatibility.

## 12. What each worker should do next

**Gemini now:** GP00 -> GP01 -> GP02 -> GP03, then the next eligible GP packet. Deliver actual player-visible fixes and a fresh gameplay build. Preserve online results; run only relevant dependent online checks instead of repeating the full suite for unchanged documentation.

**Codex current shift:** CW00 only through the existing 08:36:39 Sofia deadline: final preservation/seam review, fresh world build, branch push and scene-release handoff. Planning breadth does not undo feature freeze.

**Codex next session:** CW01 -> CW02 -> CW03; prepare CW04–CW10 as coherent reviewed placements/art direction. Apply only already approved edits; present the next specific scene/terrain/prefab change set when broader authorization is needed. Keep independent diagnostics moving.

**User acceptance request when builds are ready:** one normal-input offline adventure and one two-PC online adventure, with the exact branch/build version. Report interaction, animation, camera and route defects by location/action. External tests remain pending until actually performed.
