# CV03 — exact Undead controller change, APPROVED / APPLIED

18 November2026. Proposal `cv03.undead-state-bindings.v1`; **approved and applied after explicit user reply**. Baseline033e15408795c25204959028d706f35a5c1d7f05. This packet contains a review patch and serialized-source identity receipt, not a playback/gameplay pass. Approval was subsequently recorded from the user for exactly these nine state changes; no other asset edits are included.

## Exact scope

Only these three assets changed after approval:

| Controller | Existing controller GUID | Existing family FBX GUID |
| --- | --- | --- |
| Assets/RPG/Animations/Player Undead Warrior.controller |929c253787b093042bbaf380c6d448c5 |7d25d53b3a94a2d40947a5e01b34c24d |
| Assets/RPG/Animations/Player Undead Mage.controller |fa3c75763c7044d43aef7999bd9da312 |ef3b31bef3d3d1947a85c8184c14c05f |
| Assets/RPG/Animations/Player Undead Rogue.controller |78619923806380545b038b04dab8748b |b50f13423f75fd340a152667ec0b16b9 |

Each controller keeps its own family source, every state fileID, controller GUID, transition, parameter, default state and all other fields. No clip import, FBX, metadata, prefab, terrain, scene or gameplay/movement-speed edit is included. Undead Paladin currently shares the Warrior controller, so it is affected by any approved Warrior-controller application and needs its own validation.

## Exact state changes, identical intent for each family

| State | Existing motion internalID / state speed | Proposed existing-family motion internalID / state speed |
| --- | --- | --- |
| WalkBack | Walking_A7060811993321468294 /−1 | Walking_Backwards−6411013857362534180 /+1 |
| Jump | Running_A3543698089625107784 /0.75 | Jump_Idle5069180758234724613 /+1 |
| Ride | Idle−3100369314251171874 /1 | Sit_Chair_Idle7779023693546330386 /+1 |

The three target clips exist in **each** matching Skeleton_Warrior, Skeleton_Mage and Skeleton_Rogue `.fbx.meta` and declare `loopTime: 1`. Serialized ranges are Walking_Backwards0–32, Jump_Idle0–32 and Sit_Chair_Idle0–108 frames. Loaded duration/frame rate, movement contact and actual seating must be measured natively; a positive speed1 is a candidate state setting, not a claim of correct stride at every runtime movement speed.

- **WalkBack:** use a purpose-authored backwards walk rather than reverse forward playback. Validate actual travel direction, foot contact, knees and local/remote caller playback scaling.
- **Jump:** use the existing airborne idle loop rather than ground running. This does not create jump-start/landing transitions; confirm current caller exits the state correctly and does not pin a landed actor airborne.
- **Ride:** use an existing seated idle rather than standing idle. This does not prove a chair pose fits the horse saddle; verify hip/seat position, leg clearance, each current mount, scaling and dismount restoration.
- **Block deferred:** current pose is Idle, but selecting Block/Blocking requires reviewing nonlooping/hold behavior, guard duration and gameplay/caller timing. No guard-state change is included.
- **RunBack deferred:** no dedicated backwards-run clip is selected by this packet; reversing/changing the run gait without evidence is excluded. Keep current state/motion/speed unchanged.

## Artifacts and preparation validation (historical)

- `UNDEAD_CONTROLLER_REVIEW.patch`: unified patch, SHA256 **c77d1bda6d40d01777e755685edb763b0612f9baa3234323460b422f0b6a7d09** (minimal zero-context representation; original reviewed patch SHA256 10fb823b0a293bf96cb58462023bb0eba94191094a33c4cab0b5d15fc1ea475e). Source/candidate asset identities and the approved semantic scope are unchanged. Use `--unidiff-zero` when checking/applying this representation..
- `UNDEAD_CONTROLLER_REVIEW.json`: exact source/candidate canonical-LF SHA256 values, working-byte hashes, source FBX metadata hashes, all nine preserved state fileIDs and old/new motion/speed values. No source file is overwritten while creating this receipt.
- Three controllers × three states = nine reviewed state records. Fifteen actual serialized field changes: six fields per family are eligible, but Ride speed remains1, so only five fields change per controller.
- `git apply --check Docs/Production/Presentation/UNDEAD_CONTROLLER_REVIEW.patch` **PASS**. This checks applicability without applying the patch.
- Actual three source-controller working texts match their current committed source. `git diff --exit-code` over the three controllers **PASS** after preparation: no live asset changes.
- Preparation asserts every target internalID exists with looping enabled in its own family metadata, old state/motion/speed matches expectations, and stripping only m_Speed/m_Motion leaves each state block byte-equivalent in canonical LF form.

Native candidate previews, if supplied alongside this packet by the parent diagnostic run, are **static sampled poses**, not playback, transitions, real mounted/jump gameplay or host/guest verification. The receipt does not infer a visual pass from patch applicability.

## Preparation next action (historical)

Review the matching native source/candidate pose captures and confirm applicable approval for these exact three controller assets before any application. Recheck source/controller/FBX metadata hashes if work continues or inputs change; regenerate this proposal rather than applying stale context. After approved application, compile/import and run loaded-state/clip checks, actual forward/backward movement, jump/land and mount/dismount for Undead Warrior, Paladin, Mage and Rogue, then host/guest presentation checks through Antigravity's agreed caller contract. Preserve the original assets and all serialized identities.

At preparation time the review packet had not applied assets. Application is now complete as recorded below. Animation playback, transition checks, measured gameplay backward stride, per-mount seating, rendered gameplay and network replication remain NOT RUN. No gameplay completion claim.

## Application update

User explicitly selected "Approve these nine state changes" on18 November2026. Rechecked all controller/family-meta/source/patch hashes before application. Applied exactly3controllers/9states/15fields; candidate hashes match. State IDs, controller GUIDs and unrelated fields remain unchanged. Original review checks above describe preparation time; the live controllers now contain the approved candidate. Reverse applicability is checked with `git apply --reverse --unidiff-zero --check`; native loaded-state/rendered results are recorded in OWN_CODEX_PRESENTATION_HANDOFF.md. Block, RunBack and reserved visual C# files remain untouched. No scene/terrain/prefab/source-FBX edits.
