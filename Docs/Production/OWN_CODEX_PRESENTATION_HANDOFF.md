# Codex presentation handoff —18 November2026

## Current delivery

- Branch: **visuals/codex-presentation-20261118**; checkout Production/codex-presentation-20261118.
- Reviewed runtime base **033e15408795c25204959028d706f35a5c1d7f05**; continues published checkpoint0fdc333, preserving its approved nine Undead controller changes. No main merge or edits to Antigravity's checkout.
- Actual owner release found in **Multiplayer/antigravity-polish/Docs/Production/MP_PRESENTATION_API_CONTRACT.md**, with matching three base source blobs. Snapshot/hash/uncommitted-at-observation provenance in Presentation/OWNERSHIP_RELEASE_RECEIPT.json. Earlier pending-release notes describe checkpoint0fdc333 and are superseded.

**Delivered: mount cadence/state safety, per-model visual grounding and observer API, compiled native checks and running-world contact evidence. Remote caller integration and normal-input/online acceptance remain separate.**

## Changes

- **MountVisual.cs:** uses existing serialized walk/run stride fields instead of literal divisors, bounded0.65–2.25cadence, authored-speed Idle/Jump, invalid-input/controller/state safety and rebind-aware transition cache. Gameplay speed, unlocks, mount IDs, prefabs and saddles unchanged.
- **GroundedVisual.cs:** caches Idle mesh-foot-region geometry once per model binding. Smooth bounded correction affects only the descendant Animator, relative to its immutable authored baseline/parent. Same binding does not accumulate offsets. Legacy toe-clearance values are capped at3cm for actual soles, without rewriting serialized fields.
- Physical support rejects self/other actors/triggers, steep/high candidates and a saturated32-hit query. Missing support leaves the visual untouched. Explicit airborne/mounted/dead states retain caller pose ownership; default players/enemies retain motor grounding. Static service/patrol Idle capsules without Move are supported only with vertically stable roots and valid nearby support.
- Actors bind on **each scene load**, with one bounded follow-up after existing Start methods; factory-created companions/models bind on creation. No world regeneration, actor recreation or scene save. **CharacterVisualFactory.cs** connects these visual helpers without changing motor dimensions, stats or movement policy.
- Added three editor-only methods: MountVisualChecks.Run, GroundedVisualChecks.Run and PresentationWorldProbe.Run. Existing diagnostics/assertions were not weakened.

## Actual verification

| Check | Result |
| --- | --- |
| Unity6000.6.2f1 C# compilation | PASS for released visual source and new editor diagnostics |
| Native mount checks | **16PASS/0FAIL**, configured cadence, Idle/Jump, missing states/controllers/Animator and binding behavior |
| Native grounding PlayMode fixtures | **19PASS/0ERROR**, contact, root/wrapper invariants, rebinding, caller gates, deck vs lower floor, self/other actors, missing/saturated support, root-Animator guard and static NPC capsule |
| Running-world probe after scene reload | **95attached components**,277advancing frames;91measurable contacts, all91within6cm screening band |
| Measured gap range | Approximately−0.0037m..+0.0412m; geometric/Idle-pose estimates, not a zero-clipping guarantee |
| Actual world renders inspected | Player and Abe; retained in Presentation/Proof/PlayerGroundedWorld.png and AbeGroundedWorld.png |
| Previous approved controller slice | Nine state changes retained; no additional controller/FBX/prefab edits |

The world probe uses isolated progress, an explicitly synthetic idle keyboard, a test-only title gate and actual scene reload. It never calls EnterWorld/SaveRoster, drives the desktop or claims normal input/gameplay acceptance. Four actors have no accepted support nearby and are untouched: two Ember Hatchlings, Outrider Lyra, Vermithrax the Ember Sovereign. Intentional flight vs placement for those actors is NOT classified here.

Native receipts and source/report hashes: [CV_RUNTIME_VALIDATION.json](Presentation/CV_RUNTIME_VALIDATION.json), MOUNT_CHECK_RECEIPT.txt and GROUNDING_CHECK_RECEIPT.txt. The initial failed grounding fixture tracked the outer wrapper instead of the nested owned Animator; corrected targets and added wrapper preservation without relaxing tolerances. Startup probe failures were investigated using actual paused/frame state, then an explicit test fixture gate; no unchanged retry loop.

## Required Antigravity integration

The released exact API is implemented. After creating/replacing the remote visual, AG must bind its own peer context:

```csharp
var grounding = GroundedVisual.EnsureFor(gameObject, remoteAnimator);
grounding?.BindObserver(remoteAnimator,
    () => IsMounted.Value,
    () => Health.Value > 0,
    () => (LocomotionFlags.Value & 1) != 0);
```

Network roots **intentionally skip correction without these explicit callbacks**. This call is not added to Codex's old baseline networking; Network/**, PlayerController/PlayerMounts/EnemyController and party/HUD remain AG-owned. Do not use the local player's singleton. Rebind on actual model replacement and restore the authored outer visual wrapper on dismount without overwriting the nested Animator correction or moving the server root. Details: [PRESENTATION_COORDINATION.md](Presentation/PRESENTATION_COORDINATION.md).

This pinned visual implementation is released for reviewed integration. Coordinate further edits to these files before concurrent implementation; do not reset dirty AG work. Preserve its newer authority/party changes and existing branches/builds. **SCENE RELEASED**; no scene lock/write taken.

## NOT RUN / next test

- Normal physical-input locomotion/combat/jump/landing, per-mount seat/hoof contact, optional companion call/dismiss, all-race gait feel, both route corridors and camera clearance.
- Integrated observer callbacks, rendered host/guest/latejoin, disconnect/death/mount transitions and actual two-PC testing.
- Standalone build; old Combat01/Combined02 remain original. Unchanged quest/gameplay/spawn/combat/Relay suites were not repeated or transferred as new passes.
- Additional Humanoid backward clips, Undead RunBack/Block and missing flight art remain separate reviewed tasks. No unsupported clip/model invented.

Next: integrate the pinned visual branch with AG's multiplayer changes in a separate reviewed candidate, connect BindObserver, compile and run affected normal-input/route/host-client checks. This owned code slice is delivered; no background ownership wait, world expansion or repeated green tests are running.
