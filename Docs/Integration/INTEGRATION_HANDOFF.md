# Riftbound combined integration — delivery record

## Source and safety

- User authorized combining both workstreams, fixing compiler/integration errors and preparing the remaining feature plan.
- Third checkout: C:/Users/skama/Documents/wow=rpg-diplomna/Integration/combined-20261116.
- Branch: integration/combined-20261116. Merge56df479daa8ce9e32ee4c791e3b35812376fb19e combines worldfc35bc8 and gameplay3935340 from common base1b32349;0 overlapping changed paths/conflicts.
- First candidate source: **4af9e0e53b0316659936c05dccdbb0ecd6d26fbf**, pushed. Combined01 is superseded by the demonstrated spawn correction below; finalCombined02 source/build identity follows after execution.
- Original shared project, both agents' working copies, saved games, finished overnight branches and previous builds preserved. No main merge/force-push. The two uncommitted Greenhollow survey-tool files were not copied/integrated; no new feed-storage placements included.
- No C# compiler errors appeared. Integration changes beyond the clean merge: unique protected combined-build entry points, editor capture readiness guard, a minimal multi-tile spawn correction and its independent native regression. No runtime feature removed, assertion weakened, prefab/world regeneration or source material replacement.

## Checks actually run on the combined source

| Check | Actual result / boundary |
| --- | --- |
| Fresh import/compiler |0 C# errors. Native Unity6000.6.2f1; first import/audit roughly4m15 with continuing progress, no blind restart. |
| World route geometry |36 REACHED/0 BLOCKED, current saved scene/controller geometry; not human input or complete camera/AI acceptance. |
| Scene/terrain identity | Source scene and all10 tracked expansion/Reaver terrain files byte-identical to pre-test merged baseline. Later packaging identity must be rechecked below. |
| Existing offline quest regression |145 PASS/0 FAIL, unchanged assertions, isolated saves; includes separate-terrain regression. |
| Existing gameplay fixture |41 PASS/0 FAIL, ERRORS=0 for its Riftbound-stack error filter. Existing assertions unchanged. |
| Actual player/mount departure |4 queued-input passes, running/mounted, outbound/return; scripted heading/fixture teleports, isolated saves. Not continuous manual play. |
| Native merged-world views |3 final1280x720 paused/UI-hidden frames inspected; correct scenery/support/clear departure and readable NPC billboard text. Unedited evidence committed. Not live animation/FPS acceptance. |

First GPU yard frame contained cyan placeholders; subsequent cameras rendered correctly. Shader compiler launches supported a fresh-import warm-up hypothesis. Added ShaderUtil.anythingCompiling readiness guard to reset warm-up/sampling before screenshot; compiled recapture produced all3 correctly rendered views. This is an editor verification fix, not a shipped-material fix. Relevant API: https://docs.unity.com/en-us/engine/6000.0/script-reference/unityeditor/shaderutil/anythingcompiling.

Headless rendering warnings/editor Search startup exception remain tooling limitations. ERRORS=0 above is the fixture's documented filter, not a claim that no Unity graphics warning occurred. Prior strict mounted Reaver-centre scenario remains encounter-limited/failed in the historical world handoff; uninterrupted mounted combat is not passed by this integration.

## Build and online status

Candidate01 built17:52:34 Sofia from4af9e0e: **0 errors/844 warnings**,11m51.295s,542,648,803 reported bytes. Its full10-scenario Relay/UI suite passed, but it is **SUPERSEDED FOR SHIPPING** because initial client/respawn heights could use the wrong terrain tile. Historical receipt: ONLINE_TEST_RESULTS_01.md. Native title→character selection at1280x720 rendered readable Offline/Host Online/Join Online labels; no character entered, created or deleted. Closed normally. Its level0 exactly matches the delivered Codex world scene. This proves the source merge packaged both workstreams, not that the later spawn defect was covered by those old assertions.

The defect was noticed in actual Relay logs (client initial y9.454 versus host near0). A new independent native TerrainCollider-ray regression reproduced13 failures in32 checks, including Emberwatch and no-terrain/underground positioning. ComputeSpawnPosition now selects WorldRegions.TerrainAt(candidate), retaining existing X/Z spacing and elevated-host/no-terrain behavior. The unchanged32 checks then passed. After this runtime change, the existing145 quest and41 gameplay assertions were rerun and all passed with0 C# errors. Before/after receipts: Evidence/spawn-height-before.txt and spawn-height-after.txt.

Final **Builds/RiftboundCombined02/Riftbound.exe** is pending at this checkpoint. Protected build preflight on candidate01 passed813 trees,15 prototypes,8 layer references, every paint texel/detail/hole/outside-footprint height, matching Terrain/Collider and27.92-degree Reaver grade; the final build repeats that guard. No completion claim for02 until its results are appended below.

The exact final candidate is tested by Docs/Integration/Verify-CombinedRelay.ps1 with explicit build/source identity; sanitized final02 results belong in ONLINE_TEST_RESULTS.md. Prior Gemini/candidate01 passes are not substituted. Raw logs/auth/join codes remain local.

## User test and remaining work

Launch the complete combined standalone directory, or add this integration project folder in Unity Hub6000.6.2f1. The old shared editor project has not been overwritten. Detailed offline/online routes and controls: PLAYTEST.md.

Remaining NOT RUN until a player performs them: continuous ordinary-input first adventure; all26 individually identified tree bases/colliders; full OrbitCamera/foliage/AI/NavMesh review; complete animation/boss/audio/listening acceptance; rendered standalone GPU profiling; separate-PC cross-network play. Some imported jog/sprint torso/foot distortion remains an asset task. Online progression is session-only and host exit ends the session; no host migration.

The post-integration plan contains80 prioritized packets, with implemented/manual-acceptance/missing-system distinctions, dependencies, owners, acceptance checks and bounded next-agent prompts. See POST_INTEGRATION_PLAN.md and the superseding master-plan note. It is a roadmap, not permission to implement all expansions or a claim they are delivered.
