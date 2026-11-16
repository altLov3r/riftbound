# Combined integration run — 16 November 2026

User authorized combining both workstreams, fixing compiler/integration errors, producing a testable build and planning remaining features. No authorization inferred for publishing/force-pushing main, replacing models, new backend or unrelated world generation.

- Started17:29:32 Europe/Sofia. This is a new integration assignment, not another eight-hour shift.
- Checkout: C:/Users/skama/Documents/wow=rpg-diplomna/Integration/combined-20261116.
- Branch: integration/combined-20261116, third separate Git worktree. Existing source branches/builds untouched. Codex's two uncommitted Greenhollow editor files remain only in its original checkout; no unfinished placements included.
- World inputfc35bc8 and runtime input3935340 share base1b32349. Zero overlapping changed paths. Clean explicit merge commit56df479daa8ce9e32ee4c791e3b35812376fb19e, no conflict resolution needed.
- Native Unity6000.6.2f1 batch compile/read-only audit startedPID11008. Fresh checkout has no generated import cache; actual first-import progress is checked before diagnosing a stall.
- Build entry point adds only a unique protected output, Builds/RiftboundCombined01, using the existing preservation/build guards. It does not regenerate network prefabs/world or overwrite previous builds.
- Scope ownership now includes necessary integration fixes in this checkout only. Keep serialized references, terrain/vegetation, gameplay IDs/rewards, offline saves and session-only policy intact.

## Verification ledger

| Stage | Status | Evidence / next action |
| --- | --- | --- |
| Git merge | PASS | Shared base,0 overlapping paths, explicit merge56df479 |
| Fresh import / compiler / world audit | RUNNING | PID11008, .code-check-integration-compile-audit.txt |
| Existing145 offline regression assertions | NOT RUN on combined source | Run after compile |
| Gemini gameplay41 checks | NOT RUN on combined source | Run existing fixture unchanged |
| Changed route/input/cache checks | NOT RUN on combined source | Focused actual-controller checks; distinguish from geometry |
| Combined standalone | NOT RUN | Unique output, preservation guard, no prefab regeneration |
| Combined Relay smoke cases | NOT RUN | Use the new combined executable and isolated profiles/saves |
| Native GUI/world inspection | NOT RUN | New executable, same-build acceptance boundaries |
| Human two-PC cross-network test | NOT RUN | User acceptance after delivery |

Maximum two unchanged failed attempts. Stop after ten minutes without new evidence; change hypothesis before retry. No blind editor restarts, repeated green checks, weakened assertions or claims based only on old branch results. Track/stop only own PIDs. Do not scan Library/Temp/Logs/build directories; inspect named receipts/artifacts only.

## Combined-source verification —17:37 Sofia

- Fresh import/compile/world audit PID11008 completed17:33:47:0 C# errors;36 REACHED/0 BLOCKED. Scene and all10 tracked expansion/Reaver terrain files retain baseline byte hashes. First import took roughly4m15, with continuing compilation/asset progress; no restart.
- QuestRouteSmokeTest PID15700:145 PASS/0 FAIL at17:34:45. Existing assertions unchanged, isolated saves.
- Gp01Diagnostics.RunPlayModeVerify PID10336:41 PASS/0 FAIL/ERRORS=0 at17:35:35. Existing fixture unchanged; errors count is its Riftbound-stack filter, not proof that headless graphics emitted no warnings.
- Focused actual-player/mount departure PID18652:four queued-input passes, both directions. Headless graphics warnings/error-pause clears recorded; not visual/manual acceptance.
- Native final-world capture PID6964 running after input checks. Three paused/UI-hidden fixed-camera frames will be inspected before source/build delivery; separate from ordinary player camera/animation acceptance.
- Unity fresh import normalized six known Brute FBX metadata skeleton records and the URP global runtime-settings cache list. These are automatic import diffs in this clean checkout, not authored animation/graphics changes. Preserve diagnostic copies and restore original tracked bytes after own editors/build exit; do not commit them as unrelated fixes.
- The post-integration plan contains80 prioritized packets; existing implementation/manual acceptance/missing features are distinguished. No expansion features were added during integration.
- Native capturePID6964 produced3 files; first yard image showed cyan placeholders, subsequent departure/Willow views rendered correctly. Shader compiler launches recorded in that fresh GPU run. Do not publish the first frame as visual acceptance. Added a capture-only ShaderUtil.anythingCompiling guard that resets warm-up/timing samples until compilation finishes; no runtime/material change. API checked against Unity's official ShaderUtil documentation. One evidence-based recapture follows; this is not a repeated unchanged attempt.
- Readiness-guard recapturePID6676 compiled/completed with0 C# errors. All3 final native PNGs inspected at17:40: supported existing props/clear departure and correctly facing NPC text, no cyan placeholders. Published unedited frames and unchanged test receipts under Docs/Integration/Evidence. No scene save or runtime change. No repeat of gameplay regressions is needed for this editor-only capture guard.

## Candidate01 and demonstrated multi-tile spawn defect

- Candidate01 source4af9e0e built17:52:34:0 errors/844 warnings,11m51.295s,542,648,803 reported bytes. Native title→character selection inspected at1280x720; all3 session-mode labels readable. Closed normally without entering/creating/deleting a character. Packaged level0 byte-identical to Production02 world; current gameplay symbols present, runtime source matches Gemini. Exact file hashes in Evidence/build-artifacts.json.
- Existing full10-scenario Relay/UI matrix passed on candidate01. Sanitized receipts preserved in ONLINE_TEST_RESULTS_01.md. Those assertions did not test spawn grounding; this candidate is SUPERSEDED FOR SHIPPING, not retrospectively relabelled a failed network suite.
- Agent noticed remote initial y~9.454 versus host near0.0. Source ComputeSpawnPosition used global Terrain.activeTerrain, selecting Expanded terrain1-1 while Emberwatch is on another tile. This was not its later combat-fixture teleport.
- New independent native saved-scene regression compares production spawn positions with downward TerrainCollider rays:32 checks across six surface origins/slots, raised platform and no-terrain/underground preservation. BaselinePID2464:19 PASS/13 FAIL; Emberwatch clients at9.454m versus expected-.051m. Existing tests unchanged.
- Minimal runtime fix selects WorldRegions.TerrainAt(candidate), retains slot offsets/base-platform height, and preserves Y when no terrain covers a candidate. No scene, prefab, authority or reward change. Compile/regression/new uniqueCombined02 follow; candidate01 output preserved.
- Post-fix native regressionPID5200,18:01:10 Sofia:32 PASS/0 FAIL, same assertions/tolerance, wrong global active tile still1-1. Emberwatch slots now-.051m rather than9.454m. No C# errors. Before/after receipts committed with test source. Final quest/gameplay and new-candidate online checks rerun because production spawn behavior changed.
- Post-spawn-fix final questPID23000:145 PASS/0 FAIL,18:02:26. Final gameplayPID12868:41 PASS/0 FAIL/ERRORS=0,18:03:41. Test receipts updated with these actual final-source runs. New uniqueCombined02 uses warm build/shader caches; previous candidate preserved.
- Final source76696a7 pushed. BuildPID13916 completed18:05:44:0 errors/815 warnings,30.765s,542,645,635 reported bytes, uniqueBuilds/RiftboundCombined02. All10 terrain/source-scene hashes unchanged; six exact packaged files verified. Protection813 trees/15 prototypes/8 layers/paint/details/holes/outside heights/grade27.92 passed again.
- Final02 online rerun started with explicit build/source. User then stopped Computer Use with physicalEscape during visible02 launch; no further desktop input. Parent stops at a safe handoff and instructs agent to stop only its own headless tests. Completed02cases/remainingNOTRUN in ONLINE_TEST_RESULTS.md; no10/10finalclaim. Final02 startup not visually verified because interrupted. No user character entered or edited by parent. Both previousworkingcopies/branches remain preserved; source scene stays released.
- Cleanup confirmed: runner68634 interrupted,7 owned headless PIDs all exited; visible user game untouched. Final02 UI/invalid-code/resilience/combat allPASS,4/10 scenarios complete. Authority interrupted,5 later scenarios notstarted. Actual Combat initial host/clientYboth.05 with2.60m separation confirms the previous9.45m spawn is gone in the actual Relay session. Final receipts sanitized; no new test starts after user stop.
