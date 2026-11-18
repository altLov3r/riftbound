# Codex presentation handoff —18 November2026

## Source and scope

- Branch: **visuals/codex-presentation-20261118**.
- Checkout: C:/Users/skama/Documents/wow=rpg-diplomna/Production/codex-presentation-20261118.
- Actual base: **033e15408795c25204959028d706f35a5c1d7f05**, published combat/antigravity-refinement-20261117. Plan copied exactly fromf7ef4ee, not old story runtime. Existing integration/main/finished branches/builds and Antigravity checkout untouched.
- Approximate execution04:52–05:23 Europe/Sofia. No eight-hour timer. Scene RELEASED/no scene lock or save.

**Delivered: audited assets/grounding evidence, three compiled read-only editor tools, and the user's specifically approved nine-state Undead animation correction. This is a partial visual workstream, not a fully grounded/replicated gameplay build.**

## Applied change

Only existing Player Undead Warrior/Mage/Rogue controllers changed: WalkBack now selects existing Walking_Backwards with positive1 speed; Jump selects existing Jump_Idle speed1; Ride selects existing Sit_Chair_Idle speed1. Paladin intentionally shares Warrior's controller. Exactly3controllers/9state records/15fields. All state fileIDs, controller GUIDs, transitions, other fields and metadata are preserved. No source FBX/importer, prefab, scene or terrain edits.

User approval: selected **Approve these nine state changes** after exact proposal and candidate renders. See [review and patch identity](Presentation/UNDEAD_CONTROLLER_REVIEW.md), JSON receipt and minimal zero-context patch. Block, RunBack and Humanoid backward animations are NOT corrected in this slice. A seated pose alone is not proof of saddle fit.

## Actual verification

| Check | Actual result |
| --- | --- |
| Unity6000.6.2f1 C# compilation | PASS for3new editor diagnostic scripts; post-approval methods executed normally, no compiler errors |
| Native prefab/rig/clip/pose audit |27models inspected,0execution errors;24characters+3mounts |
| Approved loaded-controller checks |12/12 across Undead Warrior/Paladin/Mage/Rogue: expected real clip, looping, speed+1, sampled pose data |
| Pre-application static rendering |31images produced on a real graphics device,0execution errors; selected normalization/pose comparisons inspected |
| Post-approval actual controller rendering |12images produced,0execution errors; selected Warrior backward-walk, Rogue seated and Paladin airborne poses inspected |
| Saved-scene grounding audit |90selected actors inspected,0execution errors;87review candidates/1within screen/1missing support/1noAnimator. NOT90grounding passes |
| Patch/source preservation |Candidate controller hashes match; reverse zero-context patch check PASS; state/controller identities preserved |
| Original source/terrain |3reserved visual scripts, original scene and10tracked protected terrain assets match033e154 raw/LFS identities; receipt included |

Geometry estimates suggest both visual pivot offsets and saved-root clearance contribute to levitation. Read [GROUNDING_REPAIR_PROPOSAL.md](Presentation/GROUNDING_REPAIR_PROPOSAL.md), not a global Y decrement. Native sole/contact estimates and cadence numbers are diagnostic heuristics, not final per-rig constants. Runtime-only actors/companions/peers were not instantiated into the saved scene.

Raw local receipts/captures: .code-check/presentation/. Portable summaries/proof images are in [Presentation/](Presentation/README.md). Proof/HumanIdleSoleCandidateNotApplied.png is a temporary visual-normalization candidate, explicitly NOT a production grounding fix. UndeadRideAfter.png is rendered from the actually modified controller. Tools do not save/reimport assets; startup/import normalization side effects were diagnosed and narrowly restored in this initially clean isolated checkout.

## Ownership gate / exact next action

No actual Antigravity release was found for **GroundedVisual.cs, MountVisual.cs or CharacterVisualFactory.cs** in its latest journal/handoff. They remain byte-identical to base. The user acknowledged asking AG; that is not a release. See [PRESENTATION_COORDINATION.md](Presentation/PRESENTATION_COORDINATION.md) for exact source hashes and caller/API requests.

Antigravity must record RELEASED TO CODEX or RESERVED with source commit for these three files. It retains Network/**, PlayerController/partials including PlayerMounts, EnemyController, combat, rewards and party/HUD callers. Agree explicit observer alive/grounded/mounted state and retain existing BindAnimator/Animate signatures. Then Codex resumes bounded sole/support/rebind and mount-stride/state-safety work; AG connects its own remote callers and replaces the capsule placeholder. Do not edit each other's checkout or infer release from silence.

The approved controllers can be integrated after normal branch review while that exchange continues. New runtime work must start from the latest agreed successor, preserving AG's033e154 authority fixes. No main merge performed/authorized here.

## NOT RUN / test instructions

- No actual normal-input animation playback, jump/land transition acceptance, mount seating on every combination or rendered host/guest test. Reproduce in the new combined candidate; static pose capture is insufficient.
- No foot/mounted corridor traversal, bridge/stair/slope/seam/camera acceptance, NPC/enemy/companion grounding fix or network-state agreement yet.
- No new standalone built; existing Combat01/Combined02 remain original. Broad145quest/41gameplay/32spawn/19combat and Relay suites were not rerun because no gameplay authority/progression code changed; historical results must not be transferred to this slice.
- After reviewed integration: select each Undead class, walk backward, jump/land, mount/dismount; compare both players' views and verify seats, cancellation/death/respawn. Then execute the full presentation plan's affected regression/route/online gates on the exact combined source/build.

No blind retries or weakened gameplay assertions. One preservation assertion caught and diagnosed material float roundoff; it was restored before delivery. Reserved code remains gated; no token-consuming ownership wait is running.
