# Reaver repair preparation

> UPDATE 2026-11-16: Scene release was received and the scoped repair was applied using a serialized current-terrain copy. Live editor tooling is now under Assets/RPG/Editor. See the final section of CODEX_WORLD_AUDIT.md for verified results, the new test build and the buried-cache placement limitation. The preparation history below describes the earlier ownership wait.

This code is deliberately staged outside Assets while Antigravity's shared-scene ownership is unresolved. It is not imported into the live game and contains no scene/asset apply command.

User approval for the localized repair is recorded in the conversation. **Scene release is a separate prerequisite and has not been confirmed.** Absence of a Unity process is not a release.

`ReaverRepairPreparation.cs` was compiled/executed in the isolated validation project's Editor folder against hashed copies of the current and old protected terrain binaries. `VerifySnapshot` creates only an in-memory candidate. It does not create/save terrain assets, load/save the source scene, recreate actors or edit multiplayer code.

It checks:

- Compatible terrain grids and the reviewed 384 m tile footprint.
- Only the reviewed camp/trail height differences above 1 cm are transferred.
- Every height outside the footprint is unchanged.
- Tree/prototype counts, authored tree fields and prototype references/settings are preserved; vertical re-grounding is allowed.
- Every alphamap texel, terrain-layer reference, detail prototype/density and terrain-hole cell is preserved.
- An intentional horizontal tree mutation is rejected by the preservation gate.
- Affected tree positions and distance to trail centre are reported as review candidates, not collision verdicts.

Before eventual application:

1. Obtain an explicit dated scene release from Antigravity or the user; record it in the world audit. Do not infer it from silence or a completed build.
2. Refresh/check source hashes against the reviewed receipts. If changed, regenerate inputs/preview and review the differences.
3. Use the validated construction/preservation functions to create a **new** protected asset cloned from current tile 0–0. Refuse an existing destination; never overwrite either original terrain asset.
4. Assign the new asset to both the intended Terrain and TerrainCollider during the coordinated window. Back up the scene first; change no actors, quests or other terrain references.
5. Revalidate after serialization/reload, verify seam and route results, inspect the affected trees, and run existing quest/terrain plus actual foot/mount acceptance.
6. Report any NOT RUN checks and return scene ownership explicitly.

The preparation code is not a substitute for those application and gameplay gates. Receipt: `.code-check/reaver-preparation-verification.txt` in the validation checkout, copied to the live project's .code-check only after completion.
