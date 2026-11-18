# CV02 — mount cadence and binding safety

18 November2026. Source change authorized after Antigravity's explicit release in Multiplayer/antigravity-polish/Docs/Production/MP_PRESENTATION_API_CONTRACT.md, base033e154. Only MountVisual.cs and its new read-only native check tool are edited by this packet. No prefab/controller/source-FBX/scene/terrain/serialized-field change.

## Runtime change

- Retains `Animate(float speed, bool airborne, bool walking=false)` and all existing serialized fields/GUIDs. Uses the mount's existing `walkStride` / `runStride` instead of literal2.6/7.2.
- Cadence follows planar world speed with compatible0.65–2.25 bounds. Horse walk3m/s now requests approximately2.126x and run8m/s1.319x from the existing serialized fields, rather than the old tight1.25 upper bound. This is arithmetic, not a new hoof-contact/feel measurement.
- Idle and Jump remain authored speed1. Invalid/nonfinite movement input becomes Idle; invalid stride uses neutral cadence. No movement speed, unlock, summon, mount ID, collider or network authority is changed.
- Missing Animator/controller returns safely. Missing requested state falls back only to a supported Idle; a controller lacking Idle is left on its current state at neutral speed rather than receiving an invalid transition.
- Same state is not restarted each frame. Replacing the Animator or runtime-controller reference invalidates the cached state so the new binding receives its requested transition, including a newly assigned AnimatorOverrideController. In-place override clip changes continue through Unity's normal override evaluation; no forced per-frame restart is added.

## Native checks (authored by subagent, executed by parent)

Entrypoint: `Riftbound.Editor.MountVisualChecks.Run`. Launch once in this isolated checkout with Unity6000.6.2f1 `-batchmode -nographics -executeMethod` and a unique ignored log. Parent owns the only Unity process for this project; this agent launched none.

The tool uses disposable horse prefab instances in a preview scene, transient AnimatorOverrideController and in-memory single-state controllers. It never saves/imports game assets. Expected16 assertions cover configured walk/run cadence; lower/upper clamps; invalid stride; Idle/Jump speed; nonfinite input; repeated state phase; Animator replacement; override-controller replacement; missing Run/Jump fallback; unsupported controller; missing controller/Animator. Output: `.code-check/presentation/mount-visual-checks.txt`, plus concise console totals and batch exit0/1.

Parent execution completed in actual Unity6000.6.2f1: **16PASS/0FAIL**, exit0. Portable receipt: MOUNT_CHECK_RECEIPT.txt; source/report hashes: CV_RUNTIME_VALIDATION.json. No actual hoof-contact or online gameplay pass is inferred.

Agent validation: source diff/whitespace inspected. Unity compile/native execution is **NOT RUN by this agent**; parent must record the actual outcome. No gameplay, full-loop hoof contact, saddle fit, mount unlock, flight/landing or host/guest verification is claimed. Existing gryphon art is still horse art. Native front-foot estimates/upper-quartile serialized walking stride differ, so no new per-mount constants were substituted.

## Caller contract

Antigravity supplies actual planar speed, airborne and walking state once per frame for local/remote mount presentations. It owns creating/releasing the correct real mount, replicated identity/state convergence, rider-saddle positioning and restoration. This method alone does not replace the remote capsule caller or correct actor grounding. Shared runtime/network/player callers and all existing assets remain untouched by this packet.
