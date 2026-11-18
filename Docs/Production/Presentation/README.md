# Presentation slice — tools and review entry point

- OWN_CODEX_PRESENTATION_RUN.md / OWN_CODEX_PRESENTATION_HANDOFF.md in the parent folder: actual scope, results and remaining gates.
- STATIC_ASSET_INVENTORY.md: before-change033e154 source inventory, not current final controller bindings.
- UNDEAD_CONTROLLER_REVIEW.md/.json/.patch: exact user-approved nine state changes; no other controller edits.
- NATIVE_AUDIT_SUMMARY.json: portable measured summary and local raw-report identities; native diagnostics are not gameplay.
- GROUNDING_REPAIR_PROPOSAL.md and PRESENTATION_COORDINATION.md: grounding/mount repair and API/file-release requests; not applied code fixes.
- Proof/: selected actual editor renders. Sole-normalized images are temporary candidates, not a production grounding fix.
- CV_RUNTIME_VALIDATION.json and new PlayerGroundedWorld/AbeGroundedWorld proof images: actual continuation results. The running-world observation uses isolated progress, an idle synthetic keyboard, a test-only title gate and a normal scene reload; it is not UI/input/gameplay acceptance.
- ANTIGRAVITY_MP_API_RECEIVED.md / OWNERSHIP_RELEASE_RECEIPT.json: actual owner release snapshot and implemented callback contract; older pending notes describe the first checkpoint.

## Reproduce in the isolated checkout

Use Unity6000.6.2f1 batch, not another worktree's editor. No Unity MCP is exposed in this session. On a fresh checkout create ignored local AnimationFixResult.txt containing an explicit "automatic repair suppressed, not performed" note before opening Unity: the old AnimationRepair.FirstRepair hook otherwise can reimport/save broadly. Do not invoke repair/generator menus. Existing texture-upgrade marker must also be present; inspect startup hooks before using another base.

Run each method sequentially with `-batchmode -projectPath <this checkout> -executeMethod <method> -logFile <unique ignored evidence path>`. Launch background Unity with PowerShell Start-Process -WindowStyle Hidden -PassThru and wait for its exit. Never start two editors on the same checkout.

| Method | Additional flags | Output / scope |
| --- | --- | --- |
| Riftbound.Editor.PresentationAssetAudit.Run | -nographics -presentationApproved | .code-check/presentation/asset-audit.json;27 prefab/clip/rig samples and12 approved Undead consumer checks |
| Riftbound.Editor.PresentationGroundingAudit.Run | -nographics | .code-check/presentation/grounding-audit.json; selected saved actors/support candidates; no Play Mode |
| Riftbound.Editor.PresentationPosePreview.Run | -presentationApproved; omit -nographics | .code-check/presentation/approved-previews/;12 actual controller poses, including Paladin |
| Riftbound.Editor.MountVisualChecks.Run | -nographics |16disposable native mount cadence/state/binding checks |
| Riftbound.Editor.GroundedVisualChecks.Run | -nographics |19native empty-scene PlayMode contact/root/gate/support checks |
| Riftbound.Editor.PresentationWorldProbe.Run | omit -nographics | Isolated running world, synthetic idle keyboard/title gate, scene reload, measured support and optional captures; NOT input or online acceptance |

The pose tool without -presentationApproved produces baseline/candidate benchmark images. Running it after applying the patch does not recreate the old controller baseline. Original pre-application captures remain separate in this delivery's local previews folder and selected Proof images.

Tools create disposable visuals/preview scenes and ignored receipts, never save/reimport game assets. Unity itself may upgrade six old Brute FBX metadata files and pipeline settings on import; this run also caught a single Paladin material color-channel float roundoff. Review any such delta, retain an ignored patch, and restore only known import-only changes in this initially clean isolated checkout. Never reset a user's dirty work or restore the approved controller changes.

After code changes compile and rerun the affected method; do not call prior green/historical reports new passes. Rendered static poses are not animation playback, normal input, mounted fitting or host/guest verification.
