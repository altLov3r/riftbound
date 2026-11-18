# CV04 native grounding safety fixtures

Entrypoint: `Riftbound.Editor.GroundedVisualChecks.Run`, Unity batch with the presentation checkout and an explicit ignored log path. Parent owns the Unity process; this packet has not launched Unity or claimed a pass.

The harness opens an empty **unsaved** scene, enters actual Play Mode, uses frozen native Idle visual prefabs only and creates disposable physical floors/decks/obstructors. It does not instantiate a PlayerController, network session or world actor and sets an isolated ignored save override defensively. All support assertions require at least three actual game frames and a bounded settling interval; whole run times out after45seconds.

The intended19 checks cover:

- Geometric foot-region contact at `.025±.025m` above a flat collider, actor root and outer visual wrapper unchanged.
- Rebinding the same visual does not accumulate offset.
- Airborne movement preserves the local visual baseline and the raised actor root.
- Mounted seating and dead visual transforms remain caller-owned.
- Grounding resumes, and legacy serialized `.08m` soleHeight does not preserve a large hover gap.
- Self and another actor's CharacterController are excluded from support.
- An elevated bridge deck wins over a lower floor; actor root is unchanged.
- A different model binding recalibrates without double correction.
- Stationary no-motor/no-Combatant presentation via existing BindAnimator works.
- Missing support and128colliders overflowing the planned32hit buffer fail safely rather than choosing arbitrary support.
- An Animator on the GroundedVisual owner itself never moves its authoritative transform.
- A stationary TownNPC with a CharacterController whose Move method was never called still grounds its visual over valid support while `motor.isGrounded` remains false and the actor root stays fixed. This covers the service-only fallback; it does not authorize a player airborne bypass.

The oracle measures disposable baked visual geometry independently at assertion time. It does not call a runtime calibration helper or assume a copied internal correction value. Native mesh-region contact is still an estimate of the sole, not rendered contact/gameplay acceptance. No scene, prefab, metadata, controller or source asset is saved. Reports go to `.code-check/presentation/grounded-visual-checks.txt` with actual pass/error totals and native-only scope. Existing gameplay assertions are unchanged.

Required runtime API: existing `BindAnimator(Animator)` plus agreed opt-in `BindObserver(Animator, Func<bool> isMounted, Func<bool> isAlive, Func<bool> isAirborne)`. Binding before Start must retain observer state. Actual compile/run outcome belongs in the parent's run journal/handoff after execution.

## Fixture correction after first native run

The first run reached7PASS before resume-contact failed. Investigation found positional assertions/sentinels were reading or moving the outer resource wrapper while GroundedVisual owns the nested Animator transform. Corrected all baseline/rebind/airborne/mounted/dead/missing-support/saturation position references to `animator.transform.localPosition`; the outer wrapper remains only the visual lifetime/mesh-enumeration container. Added its explicit unchanged assertion. Tolerances and geometric contact/root assertions are unchanged; earlier wrapper-based position passes are superseded and must not be counted as valid coverage. Parent owns the new execution and runtime-parent-space cache correction.

The later live-world probe exposed a stationary service motor that had not yet called Move. Added one final disposable TownNPC+CharacterController fixture and one combined observable assertion: actual sole contact, unchanged root, and false `motor.isGrounded` after real settling frames. All preceding18 assertions and their tolerances are unchanged. No Unity launched by the fixture author for this addition; parent records actual execution.

Parent executed the final19-check method in actual Unity6000.6.2f1: **19PASS/0ERROR**, exit0. Portable receipt: GROUNDING_CHECK_RECEIPT.txt. Separate running-world reload observation is in CV_RUNTIME_VALIDATION.json; neither constitutes normal-input or online acceptance.
