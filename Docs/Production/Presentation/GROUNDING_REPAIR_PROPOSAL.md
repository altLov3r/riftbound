# CV04 — measured grounding repair and evidence

18 November2026. Initial proposal prepared at0fdc333, then implemented after actual Antigravity file release. Temporary normalized renders remain candidates, not proof of production behavior. The current implementation and native running-world results are in OWN_CODEX_PRESENTATION_HANDOFF.md and CV_RUNTIME_VALIDATION.json; remote caller/gameplay/traversal acceptance remains separate.

## Evidence

Native saved-scene audit inspected90 selected active saved actors:87 geometric contact-review candidates,1 within the6cm screening band,1 without accepted support and1 without Animator. These are classified candidates, not87 proven gameplay defects. Actors may settle after physics starts; runtime-only Elara/Selvek/companions/peers are outside this saved-scene inventory. Intentional fallen logs, floating magic and vegetation are not classified as actor defects.

| Saved actor | Estimated foot-region mesh minus selected collider support |
| --- | --- |
| Player at saved Emberwatch position |+0.514m |
| Abe |+0.184m |
| Quartermaster Rowan |+0.396m |
| Provisioner Mira |+0.347m |

The estimator bakes the Idle mesh, selects nearby foot-region vertices and compares the lowest candidate to an eligible support collider. This is not an authored sole marker or live contact measurement. Player's saved root/capsule bottom is0.05m while terrain collider is approximately−0.151m; saved clearance accounts for part of the measured difference. Idle mesh offset relative to root contributes separately. Removing server clearance globally would be the wrong repair.

Standalone Human_Warrior estimated Idle sole is0.317m above prefab root; Undead_Warrior0.160m. Existing GroundedVisual calibrates using foot/toe joints rather than baked sole geometry and clamps its one-time adjustment to±0.06m. This explains a plausible persistent remainder but must be reproduced during normal input before selecting final bounds.

Rendered disposable Human/Orc normalized candidates place only the visual mesh near a zero-height reference plane. They demonstrate a possible pivot correction without moving collision roots. They do not validate terrain support, gait, slope/bridge handling, jumping, remote state or a production fix.

## Reviewed implementation requirements

1. Retain GroundedVisual serialized fields/GUID/public BindAnimator signature. Capture an immutable unmounted visual baseline after model binding and valid Idle evaluation. Never apply correction to the actor/motor/NetworkTransform root; fail safely if the Animator lives on that root.
2. Derive a per-model geometric sole estimate once on bind, with validated foot/toe bindings. Cache it; do not follow the instantaneous lowest animated foot every frame. Preserve authored gait offsets. Missing rig/geometry is a classified fallback, not a guessed global offset.
3. Pick bounded physical support from the correct terrain/deck/stair collider. Exclude self/children, other actors, triggers, canopies/roofs above the support window and unrelated visual props. Missing support must not snap an actor to terrain beneath a bridge. Test the actual support helper against finite adversarial fixtures before use.
4. Agree one optional observer-state API with Antigravity: alive, grounded/airborne, mounted and valid context, supplied by its remote caller. BindAnimator alone cannot infer remote grounded state from a missing local CharacterController. Do not use the local singleton player.
5. Apply a bounded visual correction relative to the immutable baseline; reset/rebind on visual swap and restore the proper unmounted base on dismount. Preserve intentional jump/flight/death and saddle ownership. Prevent cumulative offset drift and repeated calibration from an already corrected pose.
6. Compare current vs proposed with normal input: idle/start/stop/back/strafe, jump/land, guard/dodge/attack/death, mount/dismount and respawn. Record actual sole gaps/jitter before selecting tolerances. Native geometric screening is not a final visual pass.

## Caller and route gates

- The initial file-release gate is now satisfied by the owner record in Multiplayer/antigravity-polish; provenance/API are in PRESENTATION_COORDINATION.md. Three released source files changed in this continuation; actor collision roots and all scene/terrain assets remain unchanged.
- AG owns PlayerController/PlayerMounts/EnemyController and remote callers. It must connect agreed grounding/mount state and maintain server-safe movement. Codex supplies exact method/field requirements instead of editing these callers.
- Validate Emberwatch→Highspire→Westbank→Reaver and Emberwatch→Greenhollow on foot and mounted, including bridge deck vs underlying terrain, stairs, seams, steep approaches, enclosed floors and camera clearance. These routes were NOT RUN in this packet.
- Preserve protected terrain/813trees/15prototypes and all authored scene/actor references. No scene/prefab/FBX regeneration or terrain lowering is proposed.

## Mount tuning after release

Native front-foot sampling gives approximate run contact strides6.064m/s horse/gryphon and5.979m/s zebra, consistent with their serialized run fields. Walk contact medians are approximately1.157/1.141m/s versus existing serialized upper-quartile estimates1.411/1.391m/s; the estimators differ. Do not auto-overwrite constants based on a different statistic. Current Animate ignores both fields for literal2.6/7.2 and clamps cadence tightly; using fields alone may still slide at walk speed3m/s. Evaluate model cadence and contact together before changing bounds or movement speeds. Retain current gameplay speeds/unlocks unless specifically agreed with AG.

Missing Animator/controller/state guards and rebind-safe state caching are focused code candidates. Preserve Animate(speed,airborne,walking). The flying gryphon ID currently uses horse art; no wing or flight clip exists in this inspected family. Remote model instantiation and state synchronization belong to AG; Codex's asset correction cannot replace the capsule caller by itself.

## Current implementation limits

- BindObserver implements explicit mounted/alive/airborne callbacks. No-context network roots fail safely. Saved active actors bind at sceneLoaded and once after existing Start methods; factory-created companions/models bind on creation. No polling world generator or actor recreation.
- Cached Idle foot-region mesh geometry replaces joint-height calibration. Correction is relative to immutable Animator baseline/parent, bounded to0.8m, smoothly applied to the visual only. Existing gait offsets remain. Legacy joint-clearance values are capped at0.03m for actual mesh soles, preserving serialized values/GUIDs.
- 32-hit nonallocating support buffer rejects saturation, actor/self/trigger surfaces and surfaces outside the normal/height window. Static service/patrol Idle capsules can bind without Move only while their root is vertically stable; players/normal enemy motors retain grounded-state gating. Jump/Ride/Death and observer gates take priority.
- Native reload probe with an explicitly synthetic idle keyboard and test-only title gate observed95attached components,91measurable contacts, all91within6cm screen. Actual range approximately−0.0037m to+0.0412m; this includes estimator/Idle-pose variation, not a zero-clipping guarantee. Four support-unavailable actors were left untouched: two Ember Hatchlings, Outrider Lyra, Vermithrax the Ember Sovereign. Intentional flight vs placement for those actors is NOT classified here.
- Real normal-input movement, per-mount saddle fit, remote callback hookup, host/guest visuals and both routes remain NOT RUN. No new standing/flight pose is invented for missing rig/support data.
