# Riftbound — combined work report, 16 November 2026

Recorded approximately17:25 Europe/Sofia from current source, pushed refs and the two agents' handoffs. Covers the documented Reaver repair, overnight work and subsequent production updates. Earlier project features are baseline unless explicitly listed as changed here. Calendar gaps are not claimed as continuous execution.

## Current delivery state

| Workstream | Latest delivered source | Latest complete local build |
| --- | --- | --- |
| Codex world | production/codex-world-20261116, source397fe49095b4b3fca7172044f7c6faa592718a15; delivery docs1f1795c | Overnight/codex-world/Builds/CodexWorldProduction02/Riftbound.exe |
| Gemini/Antigravity gameplay and online | production/gemini-gameplay-20261116,3935340d3d756e2d85b8644e1636f8b210cca30c | Overnight/antigravity-online/Builds/ProductionGeminiGameplay/Riftbound.exe |

Both production refs verified on GitHub. Finished overnight branches preserved atbef0420f4117529647e22b8706ec6763d0c7199f (Codex) andf415f351da708b065c3cc1b65d1c1769c48cf577 (Antigravity). No main merge. **There is no delivered combined build containing both agents' latest production changes.** The old CoopReaverRepair build included that day's online baseline, not Gemini's later production fixes.

Build directories are local/ignored by Git. Share the entire build folder, including Riftbound_Data and DLLs; an EXE alone is insufficient. Current Codex editor-tool work-in-progress is listed separately below and is not in Production02.

## Codex — delivered world work

### Protected Reaver approach repair

- Diagnosed the terrain-reference regression. The older protected terrain had no trees/prototypes; assigning it directly would have discarded the current813 trees/15 prototypes.
- Waited for explicit shared-scene release, rechecked hashes and applied the specifically approved repair.
- Cloned current terrain with AssetDatabase.CopyAsset, preserving embedded paint data. Earlier Instantiate/CreateAsset serialization changed paint weights; that hypothesis was rejected after diagnosis.
- Applied only1,636 reviewed local height changes to a new protected terrain asset. Assigned the same new data to Terrain and TerrainCollider; exactly those two scene references changed in that packet.
- Verified all813 trees,15 prototypes,8 terrain-layer references, paint texels, details, holes and outside-footprint heights after save/reimport. Original assets retained.
- Maximum checked three-lane approach grade27.92 degrees, below the existing35-degree limit. Existing separate-terrain regression passed unchanged.
- Delivered Builds/CoopReaverRepair/Riftbound.exe with0 errors/553 warnings. This earlier build is preserved.

### Overnight bridge, cache and route work

- Grounded the Willow Rest cache by its root at approximately(-291,7.19,52.5), added a matching low stone footing, preserved chest ID/rewards/lid hierarchy.
- Fixed Westbank bank intrusions through the bridge deck using another protected terrain clone. Changed377 excessive bank-height samples within x[-274,-237],z[45,55]; low riverbed retained.
- Grounded/extended six existing bridge supports; retained deck and rails.
- Retained but disabled exactly two Emberwatch fence segments to open the west exit.
- Added three narrow terrain-conforming footways: Emberwatch departure, Highspire's actual north gate and Greenhollow arrival beside its marker.
- Preserved major buildings, well/walls, actors, quests, IDs, rewards and original terrain. No whole-world regeneration or mass tree removal.
- Fixed clean-clone Adobe bootstrap: restored18 exact manifest-listed source/native/metadata files excluded by an overly broad Library ignore rule; added the narrow exception. No package upgrade.
- Delivered Builds/OvernightCodexWorld/Riftbound.exe:0 errors/806 warnings. Title and character-selection preview rendered; no character/save changed by that smoke.

### Production01 — route guidance, paving and audit accuracy

- Synchronized two scene map-route arrays and owned authoring definitions with the actual Highspire gate and Greenhollow arrival. Measured drift13.62m/2.39m became0.00m.
- Replaced Highspire's visibly plain dark footway surface with a separate material using existing cobblestone textures; original material and terrain paint unchanged.
- Corrected the route-test reader: Play Mode static batching returned a combined mesh, rather than the individual route. Tests now load the original route source mesh and verify endpoints. Invalid early fixture results were retained rather than blamed on terrain.
- Expanded read-only controller/route/camera diagnostics. Corrected chest tests to target a reachable interaction stand-off instead of its solid centre.
- Classified26 affected Reaver tree candidates individually:23 trees/3 shrubs, all stored roots grounded. Preserved intentional shrub lean/log candidates; no blanket rotation/removal.
- Produced native Highspire before/after views and route/runtime integration contracts. Identified foliage occlusion despite zero camera-sphere overlaps; did not label geometry as visual clearance.
- Delivered Builds/CodexWorldProduction01/Riftbound.exe:0 errors/560 warnings.

### Production02 — restrained supply/rest scenery

- Added exactly five existing free FantasyProps instances: two crates/one barrel beside Emberwatch's loading frontage; one bench/one barrel at Willow Rest.
- Rejected a miniature-scale prefab proposal after rendered inspection; it was never saved. Reused the correctly scaled existing FBX family and materials instead.
- Added bounded survey/preview/application tooling with source hashes, scene backup, duplicate protection, ground-footprint checks, travel margins, service aprons, vegetation/solid exclusion and saved-scene verification.
- Added five active renderers/five solid box colliders. Scene diff against Production01:712 additions/0 removals. No source FBX/import/material or original actor/terrain change.
- Inspected six native preview frames and three saved-scene recaptures. Published unedited before/after evidence and receipts in Docs/Production/Evidence/Supplies02/.
- Limited paused-editor p95 differences were roughly-0.1 to+0.5ms; batch counter unavailable. This is not standalone FPS/GPU acceptance.
- Delivered Builds/CodexWorldProduction02/Riftbound.exe, completed10:15:40 Sofia:0 errors/560 warnings,17.936s. Exact packaged files hashed; new scenery names verified in level0. Runtime DLL unchanged from Production01.
- New standalone title rendered and closed normally. All own test/build/preview processes closed; source scene released.

### Planning and handoffs

Expanded master/backlog/overnight/post-online instructions, ownership boundaries, anti-loop rules, source/test/build journals and integration contracts. The post-online plan contains16 GP packets,16 CW packets,72 feature slices and8 Stormglass Reach planning rows. Regional arcs, adjacent land, dungeon/class/crafting/backend expansion are proposals, not delivered features. Plan breadth is not permission to overwrite shared content.

## Antigravity/Gemini — reported delivered runtime work

The following is verified against its current checked-in handoffs/source identity. Codex did not independently rerun its full network/gameplay suite in this reporting turn.

### Internet co-op hardening

- Host/join by Sessions/Relay code for up to4 players;5th-player rejection. Normal play is not restricted to LAN.
- Prevented duplicate world-entry/session actions, validated join-code entry and handled invalid/expired/full sessions.
- Fixed async initialization/sign-in deduplication and cancellation/retry races; repeated Host/Leave/Host and client Leave/Rejoin cleanup tested.
- Preserved host-authoritative combat and reward paths: ability/class/range/cooldown checks, AOE deduplication, movement-spoof rejection, healing and client damage.
- Server-authorized potions/count/cooldown, rest/shop/upgrade transactions, gold/XP, main/faction quest completion and duplicate reward rejection.
- Server-validated landmark discovery and Zebra exploration unlock; mount synchronization and damage/action dismount paths exercised.
- Death/respawn/revive authorization, alive-respawn rejection, cooldown and stale-health guards.
- Synchronized Caravan/Ambush event state and rewards, including late joins; ready-check snapshots, pings and ally revive tested.
- Co-op PingF6/ReadyCheckF7 bindings and modal/dead-state gating; host-disconnect SESSION ENDED notice and return flow.
- Offline adventure/faction saves isolated from online. Online progression intentionally session-only. Host exit ends the session; no host migration.

### Gameplay production

- Calibrated movement animation strides and playback speeds per visual; preserved base visual orientation, smoothed strafe/backpedal alignment and added turn-in-place motion.
- Smoothed enemy facing/home-facing restoration and differentiated remote run/walk/backward/death states with stride-aware playback.
- Removed erroneous180-degree billboard flips from NPC names, barks, quest markers and floating combat numbers.
- Unified input blocking across menus/chat/dialogue/map and mounting; retained / slow-walk toggle and C character sheet. Moving interrupts mount summon.
- Made E interaction selection deterministic, reset out-of-range prompts and verified Abe dialogue movement lock/accept/unlock behavior.
- Fixed ability guards, Warrior rage cost, healing behavior, Blink's server move window, class AOE/debuff/double-scaling and dead-target issues. Added Eviscerate facing/SFX and co-op Fireball impact feedback.
- Added boss death/phase cast guards and offline-only direct boss gold to avoid unauthorized online rewards.
- Widened session-mode controls for1280x720; automated flow passed, final interactive visual sign-off remains pending.
- Verified settings/audio-volume persistence, empty-by-default opt-in companions and removal, shop purchases/equipment, main-quest tracking and event behavior.
- Built Builds/ProductionGeminiGameplay/Riftbound.exe after final edits. Scene/terrain/environment/FBX-import/Animator assets untouched by that workstream.

### Animation issue still open

Gemini found torso/foot-direction distortion in the imported Humanoid Jog_Fwd_Loop/Sprint_Loop clips, plus a slow generic Undead run cycle relative to travel speed. Runtime compensation is implemented; the source clips/import offsets were not changed. A dedicated asset-owner import/retargeting pass and ordinary visual playtest are still needed before saying animations are perfected.

## Verification boundaries

| Evidence | Latest documented outcome | What it does not prove |
| --- | --- | --- |
| Codex existing offline regression |145 PASS/0 FAIL after Production02 | Full human adventure or combined build |
| Codex saved-scene geometry |36 REACHED/0 BLOCKED | Ordinary input, rendering, all camera/AI behavior |
| Codex queued real-player/mount input | Production01:16 movement passes plus cache E/save/reload/dedup; Production02:4 departure passes | Continuous human play; repeated test runs are not added into inflated totals |
| Codex terrain protection |813 trees/15 prototypes preserved; seven terrain files unchanged after later production | Each rendered tree/collider individually reviewed |
| Codex sampled seams | Four edges,129 points each, max0.0000m | Every possible terrain/nav boundary |
| Gemini gameplay verification | Reported41/41 Play Mode checks and145 offline assertions | Complete animation/boss/UI visual acceptance |
| Gemini final Relay suite | Reported10/10 automated headless scenarios, including4-player/5th rejection | Separate-PC cross-network acceptance or rendered GPU FPS |

Known retained failure: the strict mounted Reaver-centre input case failed near hostile contact. Existing damage dismount is consistent with that result; uninterrupted mounted combat is not passed. No AI/health bypass, camp recreation, terrain flattening or weakened assertion was used to hide it.

Headless runs also produced unsupported graphics warnings and an editor Search startup exception. Those are recorded as tooling limits, not clean-render claims. Build warnings remain. Tree-prototype shader/billboard warnings and foliage occlusion need actual asset/render review rather than speculative mass changes.

## Newest work — prepared, not shipped

At10:24:10 Sofia, a new read-only Greenhollow feed-storage survey compiled/completed. It examined209 anchors:125 accepted,84 rejected, choosing an existing carrot crate, sack and bucket around(-42,411), away from the marker/mount displays. Temporary instances were removed; no scene/terrain/asset saved.

Two owned editor files contain uncommitted continuation changes: SettlementSupplyPass.cs and WorldRoutePlayModeCheck.cs. They add a Greenhollow profile, marker/mount apron guards and focused capture/input entry points. Native before/after review, application, gameplay/regression and a new build are **NOT RUN**. Production02 does not contain these placements or tool changes. Work stopped at this safe reporting boundary; source scene remains released.

## Next actions

1. When authorized, integrate both production branches in a third clean checkout. Preserve Codex's scene/terrain and Gemini's runtime; review bootstrap/dependencies. Do not overwrite main or either finished branch.
2. Run offline/gameplay/online regression on that actual combined source, then produce one unique combined build for both PCs.
3. Human acceptance: first adventure, Abe/services/cache, both reference routes, mounts, camera/foliage, boss fights, UI/audio and imported animation quality.
4. Two different PCs/networks: join by code, fight/quest/loot/shop, mount, late join, revive, leave/rejoin and host exit. Use the same combined build version.
5. Resume the small Greenhollow preview only after this handoff if desired; do not regenerate the world or treat the broader backlog as shipped content.

Primary receipts: Docs/CODEX_WORLD_AUDIT.md; Docs/Overnight/OWN_CODEX_HANDOFF.md; Docs/Production/OWN_CODEX_WORLD_HANDOFF.md; Docs/Production/OWN_CODEX_WORLD_02_HANDOFF.md in Codex checkout; Docs/Overnight/OWN_ANTIGRAVITY_HANDOFF.md and Docs/Production/OWN_GEMINI_GAMEPLAY_HANDOFF.md in Antigravity checkout.
