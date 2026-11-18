# Riftbound — multiplayer presentation, grounding and party polish

Prepared18 November2026. Planning deliverable; no gameplay, assets, scenes or networking changed while writing this plan.

## 1. Outcome and priority

Make the same playable character, animated mount and readable combat visible to both players. Ground characters correctly without breaking bridges, jumping, mounted seating, flight or network authority. Replace competing party displays with a compact, consistent group interface inspired by familiar MMO conventions and original Riftbound visuals.

Order: reproduce/build identity → real remote mounts → replicated action timing → local animation/grounding → party correctness → party layout → polish → exact-candidate two-player acceptance. Do not expand the world, class roster or content until this slice works.

The user's multiplayer test is valuable evidence of connection working, not proof of every replicated presentation state. A headless Relay test cannot establish that a horse looks right.

## 2. Inspected source and preservation

- Codex planning checkout: Production/codex-story-20261117; story/codex-content-20261117 at33def664bf6edfa1282e4baaad19c7c6c65066ea before this document. Its source is still integrated8ad9071; it does NOT yet contain Antigravity's new runtime work.
- Latest inspected Antigravity checkout: Combat/antigravity-refinement; combat/antigravity-refinement-20261117 at033e154. Read-only inspection, clean at inspection. This is newer than the shared integration branch and must be included in the next combined candidate.
- Integration/combined-20261116 remains8ad9071. Do not use it alone as the next runtime baseline or silently copy old files over033e154. The executable the user tested has not been identified; capture its source/build identity before attributing all symptoms to that exact candidate.
- Newly found Docs/Production/NARRATIVE_SCHEMA_ACCEPTANCE.md on033e154 records actual ACCEPTED status for the four existing frozen narrative hashes. This supersedes the earlier story handoff's pending observation. It does not prove adapter execution or require mixing narrative admission into animation repairs. Preserve the accepted artifacts and follow the separate content handoff when resumed.
- Preserve saves, IDs, metadata/GUIDs, serialized references, original assets, existing builds and all finished branches. No main merge, dirty reset, world regeneration, camp recreation or whole-project animation-import rewrite.
- Protected terrain, including813 trees/15 prototypes, is outside this repair scope. Do not lower terrain to hide hovering meshes. Scene/prefab/FBX/Animator changes need an exact reviewed list, current ownership and applicable user approval before application. Planning does not grant that approval.

## 3. Source evidence and hypotheses

Paths below are relative to the inspected Antigravity checkout. These are source findings, not fresh visual/gameplay passes.

| Finding | Source | Meaning / next investigation |
| --- | --- | --- |
| Remote mount is deliberately a primitive capsule | Assets/RPG/Scripts/Network/RiftboundCoopPlayer.cs:219, UpdateRemoteMountVisual | The reported brown blob has a concrete implementation explanation. Fix model instantiation; changing brown material cannot create a horse. |
| Local mount uses real MountDefinition.prefab, MountVisual and saddle/hips alignment | Scripts/PlayerMounts.cs, Scripts/MountDefinition.cs, Scripts/MountVisual.cs | Reuse the same presentation definitions for observers; do not create a second mount catalog or change unlock IDs. |
| Remote mount listens to IsMounted changes, not MountId changes | Network/RiftboundCoopPlayer.cs:119–148 | Audit initial snapshot/order and changing identity while mounted; stale/partial state must converge. |
| Remote rider uses fixed vertical offset rather than local saddle fitting | Network/RiftboundCoopPlayer.cs, UpdateRemoteMountVisual | Needs per-model seat alignment and restoration on dismount/death/despawn. |
| Remote locomotion and actions are separate from local animation selection | Network/RiftboundCoopPlayer.cs:277, UpdateRemoteAnimation; Scripts/PlayerController.cs | Remote motion chooses Idle/Walk/Run/Ride/Death; investigate missing airborne/guard/dodge/cast and backward/strafe parity. |
| Action RPC uses string state and fixed0.55s override | Network/RiftboundCoopPlayer.cs:445–468 | Action duration differs from real clip/windup. Investigate truncation, replay, stale actions, death priority and missing state handling. Do not treat a cosmetic RPC as damage authority. |
| Grounding calibrates once, clamps correction to±0.06m and requires local CharacterController/Combatant | Scripts/GroundedVisual.cs | May leave larger pivot errors and does not automatically cover remote presentation roots. This is a hypothesis for levitation, not proof every actor has this cause. |
| Existing recovery adds clearance to actor position | Scripts/PlayerController.cs, SampleSafeGroundY / EnsureAboveTerrain / Respawn | Collision/spawn safety and sole contact are different. Do not remove clearances globally or move server roots to fix a mesh offset. |
| MountVisual stride fields are not used by Animate's literal2.6/7.2 normalization | Scripts/MountVisual.cs | Calibrate actual clip stride and use one definition consistently; speed mismatch may produce sliding. |
| Companion and human-peer party systems are different | Scripts/PartySystem.cs; Network/CoopWorldManager.cs:466 | Preserve optional companions; build one presentation view of human peers without counting companions as network members. |

## 4. Chosen ownership

**Antigravity: network/action correctness and real-player party behavior/UI. Codex: visual assets, model grounding and locomotion calibration.** This is the proposed next allocation. Confirm explicit file ownership before either worker edits overlapping runtime files; the earlier combat reservation still applies until released.

| Owner | Owned implementation after recorded handoff | Exclusions / exchange |
| --- | --- | --- |
| Codex | MountVisual.cs; GroundedVisual.cs; CharacterVisualFactory.cs; bounded read-only visual diagnostics; approved per-model animation/visual profiles and approved art/controller adjustments | No Network/**, combat/health/reward changes, PartySystem or HUD editing. Source FBX/meta/controller/prefab edits need specific approval first. |
| Antigravity | Network/RiftboundCoopPlayer.cs; Network/CoopWorldManager.cs; session/real-party state, peer frames and appropriate existing HUD entry points | Does not edit Codex visual files/assets. Calls agreed presentation functions. No terrain/scene rewrite. |
| Antigravity, shared-file gate | PlayerController.cs and partials including PlayerMounts.cs, EnemyController.cs, PrototypeHUD.cs, AdventurePanels.cs and current session UI | Single editor: Codex supplies exact calibration/caller requests; AG applies them. No parallel edits even if changes affect different methods. |
| Integration owner, after releases | Combine reviewed branches in a new isolated integration checkout, resolve references and produce a new unique build | Neither worker merges main or edits the other's checkout. Record SCENE RELEASED separately; a code release does not authorize scene writes. |

Proposed new isolated branches after agreement: visuals/codex-presentation-20261118 and multiplayer/antigravity-polish-20261118. Base both on the agreed latest reviewed runtime033e154 or its explicitly recorded successor. Do not branch visuals from old story runtime and later overwrite newer combat files. Keep the story branch and accepted content frozen.

### Required presentation contract before shared implementation

- One local resource-backed mount lookup by existing mountId. Runtime build must include model, materials, clips and controller; editor-only AssetDatabase fallback cannot be the player-build solution.
- Create/bind/release a presentation instance: mount animator, saddle reference and rider-base transform. It contains no unlock, movement, reward or health authority. AG owns any network-facing caller and runtime mount-definition registry; Codex supplies validated model/seat/stride requirements.
- AG supplies accepted mounted state/identity, planar speed, walking/airborne state and agreed action identity/start/duration/cancellation. Codex reports exact supported state names, clip lengths, stride measurements and per-model visual baseline. Version genuinely new replicated state explicitly.
- No second NetworkObject horse unless a demonstrated gameplay need exists. A cosmetic child must not add duplicate collisions, extra network authority or a second player motor.
- Both owners record exact fields/example calls, resource keys and ownership in their journals before overlapping work. If an API gap blocks one worker, exchange the concrete missing field and continue independent audit/calibration.

## 5. Execution packets

| ID / owner | Work | Required completion evidence |
| --- | --- | --- |
| MP00 / both | Identify current tested EXE/source; capture host and guest mount/combat/hover clips; record race/class/mount/zone and actual state. Establish exact branch/file releases and baseline build. | Repro matrix with CONFIRMED, NOT REPRODUCED and NOT RUN; no guessed measured values. |
| CV01 / Codex | Audit existing character/mount rig, avatar, scale, pivot, sole/hoof, seat, materials, controller/state/clip references and build inclusion. Inventory all supported combinations rather than assuming four races/five classes. | Per-model table; deliberate fallen logs/floating magical art excluded from defect list; missing assets separated from miscalibration. |
| MP01 / AG | Replace remote capsule path with existing real mount presentation; handle MountId plus mounted state idempotently; bind full initial snapshot and teardown. | Both perspectives, non-default mount, late join already mounted, rapid mount/dismount, death and disconnect show one correct instance; no capsule in normal success path. |
| CV02 / Codex | Tune mount presentation: idle/walk/run/jump where supported, actual stride data, per-model saddle fitting, shadow/material correctness, safe missing-clip fallback. | Approved model inspection and local+observer visual comparison; each existing mount tested, not only horse. Unsupported flight/clip states documented. |
| MP02 / AG | Close accepted action/locomotion state contract. Reject unsupported action requests; sequence events, apply duration/start/cancel rules, clear stale state, separate display from accepted damage. | Attack/cast/guard/dodge/jump/hit/death/respawn order tests; host/guest see corresponding readable action; spam or bad IDs never cause damage or stuck pose. |
| CV03 / Codex | Calibrate sole/hoof baseline and gait per supported race/model; fix demonstrated avatar, root orientation, loop/transition and stride defects using existing assets first. | No lateral root sway/backward feet/full-loop truncation on reference set. Measurements and approved asset diffs; no whole-world importer menu. |
| CV04 / Codex + AG caller | Implement bounded presentation grounding for local/remote/NPC variants; bind after visual swap, reuse current support sampling, preserve valid jump/flight/death poses. AG connects network/enemy callers if needed. | Model-specific sole/support evidence; bridges remain on deck, slopes and stairs stable, no root drift/terrain snapping through floors. |
| MP03 / AG | Real-player group state: authoritative roster, identity/leader/ready status and disconnect cleanup; retain existing session limit and rewards. | Full roster and leader rules converge after join/leave/rejoin; client cannot spoof leader controls or another member's state. |
| MP04 / AG | Compact group frames and one group menu; remove duplicate peer/companion displays. Maintain keyboard/mouse access and modal input rules. | Screenshots at target resolutions; roles/status readable; no HUD overlap or actions firing beneath menus. |
| GP01 / both, AG runtime | Reference combat-feel pass after parity: contact timing, recovery, movement/target facing, accepted hit audio/VFX, target/cast feedback. Tune one dimension at a time. | Each current class offline/host/guest; no resource/reward regression or false successful-hit feedback. |
| QA01 / integration owner + both | Build the combined reviewed candidate; run targeted native/online/visual regressions, record exact hashes and deliver test checklist. | Distinguish fresh results from historical baseline and headless from actual rendered two-PC play. |

## 6. Grounding rules and coverage

Audit local player, remote player, enemies, named/service NPCs, companions, mounts and riders independently. Then classify nearby static model hover defects separately. Do not apply one global Y decrement to all models.

For each failure record actor ID/name, prefab/model/rig, controller bottom, visual baseline, current lowest valid sole/hoof and supporting surface. Determine whether the defect is actor/root clearance, prefab pivot, scale, avatar/clip root transform, saddle fit, replicated position or incorrect support sampling before changing it. Recheck after model/race swaps and respawn.

- Keep server collision roots and safe spawn/respawn placement authoritative. A cosmetic offset cannot alter range, capsule collision, hit checks or recorded server positions.
- Sample the correct terrain tile and real deck/stair colliders. Exclude self/child colliders, triggers, canopies, VFX and unrelated characters. Resolve multiple hits by a bounded support rule; never choose the highest arbitrary roof or always use terrain under a bridge.
- Evaluate supported idle/contact pose, not instantaneous lowest moving foot every frame. Bounded smooth correction must not produce bobbing or cumulative drift. Rebind/recalibrate on visual replacement; explicitly bound recovery from missing support.
- Preserve intentional airborne motion, death/ragdoll placement, seated poses, flight and non-grounded VFX. Use small slope foot alignment/IK only after root/pivot/avatar issues are understood; unsupported rigs fall back safely.
- Target hypothesis for grounded idle is a visible sole/support gap no larger than roughly0–3cm at normal character scale, subject to measured rig geometry and no clipping. Walking foot lift is intentional; don't fail it as levitation. Record actual worst gap/jitter before accepting a tolerance.
- Terrain route sample: Emberwatch flat/spawn → Highspire approaches → Westbank bridge/deck → Reaver approach; Emberwatch → Greenhollow. Include stairs, slopes, seam crossings, sheltered floors, steep camera approaches, mounted clearance and valid airborne transitions. Preserve the reviewed Reaver repair and vegetation.

## 7. Animation and combat-feel requirements

Inventory Idle, Walk, Run, WalkBack, RunBack, supported strafe/turn, Jump/fall/land, Ride, primary/secondary/class cast, guard/dodge, Hit and Death. Mark unsupported states rather than faking successful coverage. Prioritize the current playable combinations; do not create another animation framework.

- Locomotion loops continuously; attacks play their intended windup/contact/recovery and return cleanly. State transitions are not restarted each render frame. Controller/clip names and avatar compatibility validated before binding.
- Match planar movement speed to measured stride per model. Derive backward calibration rather than multiplying all models by arbitrary constants. Idle turning, backpedal and strafe cannot reverse knees or rotate the authoritative hit direction incorrectly.
- Decide existing in-place/root-motion policy explicitly. Root bone translation must not fight CharacterController/NetworkTransform. Keep rendering interpolation separate from motor authority; reproduce remote start/stop jitter before adding more smoothing.
- Network action presentation uses finite supported IDs, sequencing and authoritative timing. Delayed/duplicate packets cannot replay a stale swing after death or restore an old mount after dismount. Late join receives durable current state; do not replay every historical one-shot.
- Priority proposal: death/despawn and authoritative cancellation override old actions; mounted state has a consistent transition policy; locomotion resumes after action recovery. Owner prediction must reconcile without a second hit animation or duplicate accepted-impact sound.
- Damage, cooldown, resource costs, defense, LOS and reward acceptance remain AG-owned and authoritative. Cosmetic animation events must not deal a second hit. Preserve033e154 empty-swing/parity/LOS/immunity/confirmed-feedback fixes.
- Readable weapon contact, impact and cast release should line up on host and guest within an agreed measured latency budget. Record environment and packet simulation; do not invent a universal frame-accurate Internet guarantee.
- Improve existing sound/VFX only for verified accepted events: distinct swing/cast/impact/interrupt cues, spatial falloff, no duplicate owner+RPC playback or endless combat loops. No new provider/package is needed for this first pass.

## 8. Party system — familiar MMO layout, Riftbound implementation

### First release: polish current four-player online co-op

- One compact peer roster, consistent order and labels on every client; portraits/class tint, name, health, class resource where supported, alive/dead/disconnected and leader indicator. Use symbols/text alongside color. Unknown resource/range is shown honestly rather than guessed.
- Keep the self player frame and party list coordinated to avoid a second large self frame. No permanent connection/debug panels while playing; place session controls/join code behind the group menu. Join codes are private and excluded from shared screenshots/logs.
- Left-click selects a valid peer; context menu contains only implemented actions. Ready check/ping reuse existing paths. Role preference is presentation, not automatic class assignment or combat permission.
- Party leader initially follows session host. Explicitly state whether promotion is only social; it must not pretend to transfer transport authority. No host migration promise; preserve current host-disconnect exit/recovery policy until a separately designed migration feature exists.
- Server validates leader-only actions, roster membership, ready-check identifiers/timeouts and request rate limits. Handle decline/no response/death/disconnect correctly; no stale replies satisfying a new check. Existing ability/target/loot authority unchanged.
- Ready results shown briefly in frames; one small actionable prompt, not repeated overlapping modals. Revive/help state and range use actual supported data. No unsupported inspect, summon, trading or follow buttons that silently do nothing.
- Optional NPC crew remains opt-in through its companion menu. Human peers and companions are distinct types; opening co-op does not spawn a default crew, count a companion against network capacity, or display duplicate ready checks/frames.
- Local and mixed-faction party behavior preserves existing supported policy. Shared combat does not silently share personal quest state, unlocks, chest claims or saved rewards.

### Later group features, after the first release passes

1. Separate social group membership from transport/session membership if inviting/kicking without an immediate connection change becomes a real requirement. Existing Relay join-code flow remains supported initially; do not advertise an account/friends backend that does not exist.
2. Invite/accept/decline/expiry, leader promotion, leave/kick and reconnect grace. Specify session-vs-group consequences, authorization and UX before implementing. A kicked peer cannot remain an invisible damaging participant.
3. World target markers, quick pings and clear distance/zone indicators from supported replicated data. Scope persistence and anti-spam rules; do not replicate arbitrary transforms/commands from clients.
4. Party chat tabs/mute, optional frame scale/position/accessibility, compact shared-objective summary clearly distinguished from personal progression. Default loot remains current rules until an explicit alternative is approved and tested.
5. Five-player parties, raids, matchmaking, guilds, cross-session friends, host migration and dungeon queues are separate expansion projects. Do not change the current four-player capacity assertion to imitate a five-player MMO party.

## 9. Test matrix and acceptance gate

| Layer | Minimum checks | Evidence |
| --- | --- | --- |
| Source/assets | State/clip/avatar/resource/mount IDs, GUID preservation, dependency inclusion and owner file diff | Read-only audit and reviewed changes; no gameplay label |
| Focused native tests | Mount initial state/order/switch/teardown; action duplicate/stale/cancel/death; grounding bind/swap/support; roster/ready-check permissions | Real Unity compile + focused checks for changed behavior; no weakened assertions |
| Rendered local | Race/class reference set, all existing mounts; grounded idle, start/stop/run/back/strafe/jump, attacks/guard/dodge/cast/death/respawn | Captures annotated with source/actor/state; measure gaps/stride and distinguish airborne frames |
| Rendered two-player | Host mounts while guest observes, then reverse; both fight, turn and jump; late-join mounted/in combat; dismount/die/rejoin | Both actual views, correct horse/material/seat and action playback; separate headless receipts |
| Group | Host+1, four admitted/fifth rejected, mixed roles, ready yes/no/timeout, peer disconnect/rejoin, host stop, optional crew toggles, spoofed leader requests | Consistent roster and frame cleanup, no stale UI/authority leak |
| Routes | Both reference corridors, deck vs terrain, stairs/seams/slopes/interior floor, mounted camera clearance | Actual foot/mount route evidence; static scene checks are insufficient |
| Regression/build | Existing quest/gameplay/spawn/changed combat suites; affected Relay cases; unique standalone, source/config/hash receipt | Preserve145/41/32 and current combat assertions as existing baselines, not claimed new passes. Inspect unexpected skips, do not rewrite expected totals merely to turn green. |

First milestone is accepted only when neither perspective sees a placeholder for a valid existing horse, both see readable correct action states, reference actors stand on their actual support, party frames are compact/coherent, and authoritative gameplay/rewards still pass. Missing visual/two-PC evidence is NOT RUN and remains a gate to a visual-completion claim. User testing confirms feel; it does not replace compile/regression checks.

Capture initial presentation frame time/allocations with a stable scene and two/four visible peers if available. Fix obvious per-frame instantiation/lookup leaks introduced by this slice; defer broad performance work without a reproducible measurement.

## 10. Delivery and stop rules

- Each worker records actual base, owned/released files, changes, tests actually run, failures and NOT RUN checks in its own production journal and handoff. Link this plan and attach exact supported presentation schema/clip/profile version.
- At most two unchanged failed attempts. After ten minutes without new evidence, change hypothesis or independent task; cap investigation of an unchanged blocker at twenty minutes. No repeated green sweeps, blind Unity restarts or unbounded network waits.
- Before integration: checkpoint/push only the worker branch, release shared files explicitly, review diffs against the same runtime base. Preserve independent story/world work and current source. If either checkout is dirty, inspect and coordinate; do not reset or copy whole directories.
- Integration produces a new uniquely named candidate such as RiftboundPresentation01; never overwrite Combined02 or Combat01. Record exact source, Unity/config, executable/assembly hashes, passing checks and unresolved limitations. No main merge without its own instruction.
- This document starts no implementation shift. Next action is assign/confirm file ownership and execute MP00/CV01; seek specific approval only for concrete scene/prefab/FBX/controller changes that require it.

## 11. Prompt for Codex

```text
Start Riftbound's visual presentation/grounding workstream from Docs/Design/MULTIPLAYER_PRESENTATION_PARTY_PLAN.md. Inspect current AGENTS.md, latest Antigravity combat handoff and exact current source before editing. Use an isolated visuals/codex-presentation-20261118 checkout from the agreed reviewed runtime033e154 or its recorded successor, not old story runtime. Preserve all earlier branches, builds, saves, IDs, accepted narrative hashes and metadata.

Start MP00/CV01 read-only. Coordinate explicit release of GroundedVisual.cs, MountVisual.cs and CharacterVisualFactory.cs before runtime edits. Own CV01–CV04 model/rig/sole/hoof/seat/stride/clip diagnostics and focused approved presentation fixes. Antigravity owns Network/**, PlayerController/PlayerMounts/EnemyController, combat/state/rewards and all party/HUD callers. Send exact caller/API requests; do not edit its files.

Reuse existing real mount/character assets. Separate collision roots from cosmetic grounding; preserve jump/flight/death, bridge decks and correct terrain tile sampling. No global Y shift, world/actor regeneration or whole-world import menu. Source FBX/meta/controller/prefab/scene changes require their exact reviewed list and applicable approval first; do independent diagnostics while blocked. Agree one mount/action presentation contract with Antigravity.

Compile C# changes and run focused native and rendered checks when available. Synthetic/source tests are not gameplay. Journal actual changes/measurements/results/NOT RUN, checkpoint and push only your branch, deliver exact handoff and file releases. Maximum two unchanged failures; change hypothesis after ten minutes without evidence. Do not merge main or edit another checkout.
```

## 12. Prompt for Antigravity

```text
Start Riftbound multiplayer presentation and party polish from Docs/Design/MULTIPLAYER_PRESENTATION_PARTY_PLAN.md on Codex's story branch. Read current AGENTS.md and your latest combat handoff; preserve033e154's authority/parity fixes and accepted narrative contract. Use your isolated branch multiplayer/antigravity-polish-20261118 from the agreed latest reviewed combat runtime, or record a deliberate successor checkout. Never reset dirty work or edit Codex's checkout.

Own MP00–MP04 and GP01 runtime integration: replace the remote brown primitive capsule with the same resource-backed animated mount family used locally; converge mounted+mountId snapshot/order/switch state, rider saddle fitting and teardown. Agree a read-only presentation contract with Codex. Own Network/**, PlayerController/PlayerMounts/EnemyController, session/real-peer group state and HUD callers. Release Codex's GroundedVisual/MountVisual/CharacterVisualFactory files explicitly before it edits them; do not edit those assets/files concurrently.

Correct action timing/sequence/cancellation and locomotion state parity for observers without giving clients hit/health/reward authority. Replace fixed action duration where demonstrated, reject bad/stale action IDs, keep death/dismount/respawn priority. Refine current four-player party into one compact roster/menu with actual health/status/leader/ready/ping behavior; keep optional NPC crew separate and opt-in. Do not add five-player capacity, account/friends backend or host migration implicitly. Preserve personal progress and existing loot rules.

Compile, run focused native/authority/party tests, affected existing regressions and rendered host/guest mount/action checks. Record exact candidate source and distinguish headless Relay from actual two-PC visual checks. No scene/terrain/FBX/controller/prefab changes without their concrete review/approval. Preserve source assets and all builds; deliver a unique new standalone plus truthful results/NOT RUN, journal and handoff, checkpoint/push only your branch, no main merge. Maximum two unchanged failures; change hypothesis after ten minutes without new evidence. No unbounded test loops.
```
