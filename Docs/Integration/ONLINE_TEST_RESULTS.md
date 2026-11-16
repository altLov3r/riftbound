# Combined02 standalone online verification

PENDING. Parent is correcting a confirmed multi-terrain spawn-height defect and preparing a new unique Builds/RiftboundCombined02/Riftbound.exe. No Combined02 Relay fixture has run yet.

Historical candidate01 results are preserved in ONLINE_TEST_RESULTS_01.md:10 automated scenarios passed, but its high remote-spawn position was not covered by those network assertions. Candidate01 is superseded for shipping. A new native spawn regression plus combined02 verification must establish the actual fix.

Prepared Docs/Integration/Verify-CombinedRelay.ps1 with explicit -BuildDirectory and caller-supplied -SourceCommit. It records exact managed-DLL/packaged-scene hashes. Default build is Builds/RiftboundCombined02; wait for the parent's build-ready message before execution. No runtime assertion is changed by the runner.
