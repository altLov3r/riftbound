# Codex world production02 — handoff

## Scope and identity

Branch production/codex-world-20261116, continuing from e746638d7c8ccc024388995007a456841c951130. Production01 and the completed overnight branch/build/handoff are preserved. This is a user-activated continuation, not a new eight-hour timer. No main merge or edit to the shared/Gemini checkout.

CW04/narrow CW05: five existing, free FantasyProps scenery instances under the earlier C05 loading/rest authorization. Exact transforms, source hashes, support/clearance limits and native evidence: SETTLEMENT_SUPPLIES_CHANGESET.md and Evidence/Supplies02/.

- Emberwatch: two human-scale wooden crates and one provision barrel at the existing supply frontage, outside the departure/service corridor.
- Willow Rest: one bench and one provision barrel on a surveyed flat patch, preserving the bridge/cache/standing interaction approach.
- Five active renderers and five solid box colliders. Imported axes and prefab links retained; unused model animators disabled on instances only. No new gameplay interaction, persistent ID or runtime logic.

Scene diff against Production01:712 additions/0 removals. No existing scene fields removed/replaced. Source FBX/import/materials, original buildings, actors, roads, all terrain/vegetation/paint/details and saves preserved. No new assets downloaded.

## Checks actually run

| Check | Result |
| --- | --- |
| Apply and saved-scene reload | PASS: five roots, exact transforms, original model links, grounding/clearance. Maximum footprint height spread0.043m; no terrain flattening. |
| Native before/after preview | Six fixed-camera native frames inspected. Four representative unedited PNGs committed; matching scale/materials and supported bases. UI hidden/simulation paused. |
| Editor timing sample | Yard p95 9.822→10.246ms; departure12.048→11.945ms; Willow10.050→10.546ms. Warm20/sample90; no >25%+2ms increase in this limited sample. Batch counter unavailable. Not standalone FPS. |
| Focused actual controller/mount | Four queued keyboard passes: Emberwatch departure foot/mount, both directions. Scripted heading and isolated saves; fixture teleports between cases. |
| Full read-only controller geometry | 36 REACHED/0 BLOCKED after scene save; walking/mounted/reverse, bridge side lanes, full north road and cache approach. Not manual play. |
| Existing quest regression | 145 PASS/0 FAIL at10:12:04 Sofia; unchanged assertions including separate-terrain regression. |
| Seven protected terrain files | Byte hashes match the finished overnight versions; no terrain/paint/tree edit. |

Headless input/regression runs cannot validate rendering: their logs include unsupported RenderTexture/constant-buffer warnings. The focused departure log also contains one UnityEditor.Search.SearchDatabase startup exception; editor error-pause handling is recorded. No runtime or test assertion was bypassed to turn those into graphics passes.

Earlier strict mounted Reaver-centre test remains FAILED/encounter-limited in Production01: existing damage-dismount behavior prevents uninterrupted riding through hostile contact. It was not repeated or relabelled for this off-route scenery change.

## Packaging and next acceptance

Saved-scene native recapture completed: frames-20261116-081237, three1280x720 images inspected after reload. Props retain their intended scale, orientation, support and open exit; no source scene saved during capture. Editor p95 yard10.130ms/departure12.613ms/Willow10.625ms, batch counter unavailable. This is a paused native-editor verification, not a standalone benchmark. Seven terrain and all three source FBX hashes match their reviewed baselines.

Unique Production02 build is pending at this source checkpoint; final identities/results will be appended after execution. Do not infer build completion from this draft status.

NOT RUN: continuous manual adventure, complete camera/foliage/animation/audio/NavMesh review, all26 individually identified tree colliders, standalone GPU profiling, complete mounted Reaver combat, real two-PC online or combined-branch compatibility. The world branch does not include Gemini's newer gameplay/runtime fixes.

Once packaged, test this world build in Offline mode: inspect Emberwatch loading/departure, run and ride through the exit, then visit Willow Rest/cache and cross Westbank both ways. Broader building/fence/terrain/vegetation edits still require their specific approved scope. Integration with Gemini belongs in a third clean checkout when authorized, preserving this scene and rerunning dependent offline/online checks.
