# Antigravity Combat & NPC Runtime Refinement Run Journal

- **Assignment:** `AG-COMBAT-NPC-20261117`
- **Agent:** Antigravity
- **Checkout / Worktree:** `c:\Users\skama\Documents\wow=rpg-diplomna\Combat\antigravity-refinement`
- **Branch:** `combat/antigravity-refinement-20261117`
- **Integrated Base Commit:** `8ad9071820c2fbed32fbb728122ffb2f02980888` (`integration/combined-20261116`; combined runtime source `76696a7bff9e86f40a5c7fb90747fac5a290be8b`, preserving `Integration/combined-20261116`, `Overnight/*`, `main`, and prior builds untouched)
- **Actual Start UTC:** `2026-11-16T22:09:49Z` (`2026-11-17T00:09:49+02:00` Europe/Sofia)
- **Final Checkpoint Time:** `2026-11-17T07:58+02:00` Europe/Sofia (bounded checkpointed execution across `AG00 -> AG01 (CF01–CF04) -> AG02 (SC02/SC04/SC05/SC07/SC09) -> AG03 (CF05–CF14 initial reference slice)`)
- **Failed Attempts Counter:** `0` unchanged failed attempts (`CF01–CF04` and `AG02`/`AG03` compile/test iterations resolved with root-cause fixes; isolated `FullAcceptance` re-run verified transient sweep race).

## AG00 / CF00 — Combined02 Baseline Coverage Closed (PASS)
- **Candidate Executed Read-Only:** `C:\Users\skama\Documents\wow=rpg-diplomna\Integration\combined-20261116\Builds\RiftboundCombined02\Riftbound.exe` (`source 76696a7`, `EXE_SHA256=AF88B840E599661AC63FC10AE27AC77DD97CD46B44DF8A7CE60EB0DCFACFC99E`, `ASM_SHA256=428B40BA910C28F534804962BE8E49F1F3AB543D7655DAF2263F156FC9909CD6`).
- **Private Evidence Folder (Git-ignored):** `.code-check/ag00/relay-20261117-001232/` (join codes redacted).
- **Retained Prior `4/10` Receipts on `Combined02`:** `Ui` (`EXIT=0`), `InvalidCode` (`EXIT=0`), `Resilience` (`EXIT=0`), `Combat` (`H=0, C1=0`, `p0=(0.00, 0.05, -6.00), p1=(2.60, 0.05, -6.00)`).
- **Executed Remaining `6/10` Scenarios on `Combined02`:**

| Scenario | Exits | Grounded / Outcome Receipt |
| --- | --- | --- |
| `Authority` | `H=0, C1=0` | `PASS` — `sep=2.60m (p0=(0.00, 0.05, -6.00), p1=(2.60, 0.05, -6.00))`; `hostAuth=PASS`, `clientAuth=PASS` |
| `Progression` | `H=0, C1=0` | `PASS` — initial `sep=2.60m`, post-combat/respawn grounded `p1=(2.39, 0.06, -0.51)`; `hostProg=PASS`, `clientProg=PASS` |
| `LateJoin` | `H=0, C1=0` | `PASS` — `sep=2.60m`; `lateJoin=PASS(40/70,evt=True,rcDecline=True/rcSnap=True)` |
| `FullAcceptance` | `H=0, C1=0` | `PASS` — `sep=2.60m`; `hostAccept=PASS(cid=1,ping=True,rc=True/True,revive=True,kills=0,qIdx=1)`, `clientAccept=PASS` |
| `ClientLeave` | `H=0, C1=0` | `PASS` — `sep=2.60m`; `hostDisconnectRecovery=PASS`, `ClientLeave(questAccepted=True,mounted=True,rejoined=True,nmAfterLeave=0,finalState=Offline)` |
| `Capacity` | `H=0, C2=0, C3=0, C4=0, C1=1` | `PASS` — 4 admitted (`sep=2.60m`, Host `avgMs=6.67, p95Ms=10.56, enemies=54`), 5th client (`C1`) rejected (`[OnlineSessionService] Join failed: session is full`) |

- **Independent Spawn Verification (`Riftbound.Editor.CombinedSpawnVerification.Run`):** `SPAWN_EXIT=0`, `TOTAL PASS=32 FAIL=0` (`.code-check/integration/spawn-height-result.txt`).

## AG01 / CF01–CF04 — Reproductions & Minimal Authority/Parity Fixes (PASS)
- **Baseline Reproduction (`Riftbound.Editor.CombatRefinementChecks.ReproduceBaseline`, `REPRO_EXIT=0`, `.code-check/ag01/cf01_repro.txt`):**
  - `CF01`: Empty Warrior swing raised Rage `0->15`; empty Paladin swing raised Holy Power `0->1`; empty Rogue swing spent Energy `100->60`.
  - `CF03`: Dormant target (`incomingDamageMultiplier=0`) took `1` chip damage (`999/1000`).
  - `CF04`: Solid cover blocked `ClearSightForTarget` (`blocked=True`) while `EnemyController` previously bypassed LOS when `targetIsRemote` was true.
- **Implemented Fixes:**
  - `PlayerController.cs`: Gated `CastAbility1()` melee resource gain/spend (`Warrior`, `Paladin`, `Rogue`) on valid living target `<= 3.2f`; deferred multiplayer `Hit()` damage/crit/DoT/Undead-heal presentation until server confirmation (`ConfirmAbilityHitClientRpc`).
  - `Combatant.cs`: Added dormant `incomingDamageMultiplier <= 0f` `"IMMUNE"` gate while preserving positive-multiplier `>= 1` armor floor; added `ApplyAuthoritativeNetHit`; guarded `if (previous > 0f && !Alive)`.
  - `BuffSystem.cs` & `ClassSpells.cs`: Guarded periodic DoT ticks to authoritative/offline side; removed unconfirmed multiplayer Fireball impact sparks/audio.
  - `PlayerUtilities.cs`: Synced Q Guard start/stop to server (`SyncDefenseState`).
  - `EnemyController.cs` & `EnemyProjectile.cs`: Enforced `ClearSightForTarget` for both local and remote players; routed remote ranged casts through `EnemyProjectile.LaunchForRemotePlayer` with wall/capsule collision; synced host `LocalOwner.Health.Value` on projectile hits.
  - `RiftboundCoopPlayer.cs` & `CoopWorldManager.cs`: Closed server ability allowlist (`melee`, `fireball`, `frostnova`, `eviscerate`, `consecration`, `whirlwind`, `interrupt`), Mage `25f` interrupt range + `IsTargetCasting` cooldown gate, closed heal table (`arcane_ward=40`, `holy_light=60`, `second_wind=45`) rejecting full-HP calls, server Undead `+6` HP on confirmed melee hit, server Q Guard & LeftAlt Dodge defense mitigation, `BroadcastVisualEnemyProjectileClientRpc`, and `Authority` smoke assertions for `unkAtkRej` (`"spoof_unknown_ab"`) and `"spoof_unknown_heal"`.

## AG02 / SC02 + SC04, SC05, SC07, SC09 — Narrative Schema Acceptance & Consumer Adapter (PASS)
- **Schema Coordination (`SC02`):** Audited Codex `origin/story/codex-content-20261117` (`86051239bfb0ed9250b67955035c0cf6fa5c5b9d` / frozen `3fc0dcf4be005a6ef4e7d23f74f74555d0a1006f`) and recorded formal `ACCEPTED` contract sign-off with exact UTF-8 LF hashes in [NARRATIVE_SCHEMA_ACCEPTANCE.md](file:///c:/Users/skama/Documents/wow=rpg-diplomna/Combat/antigravity-refinement/Docs/Production/NARRATIVE_SCHEMA_ACCEPTANCE.md).
- **Adapter & NPC Runtime (`Assets/RPG/Scripts/Narrative/NarrativeCatalog.cs`):**
  - Read-only `Resources.Load<TextAsset>("Narrative/first-five.v1")` JSON adapter with strict `schemaVersion == 1`, `catalogId == "riftbound.first-five.v1"`, `profiles.Length == 5` validation, fail-closed `UNKNOWN` predicate evaluation, per-viewer `player.GetComponent<FactionQuestSystem>()` ownership, exact `OrdinalIgnoreCase` profile matching (no `Contains`/role traps), and built-in safe defaults until Codex promotes external JSON into `Assets/RPG/Resources/Narrative/`.
  - Wired `PrototypeHUD.cs` (`Abe` `main_title`/`main_body`), `AdventurePanels.cs` (`Quartermaster Rowan` & `Provisioner Mira` shop header `greeting`), `NPCGossipWindow.cs` (`Cartographer Elara` state-aware greeting, `quest_offer`/`quest_progress`/`quest_ready`/`quest_after`/`public_topic` with Codex topic IDs `roadward-truce`, `road-work`, `supplies`, `receipts`, `road-crews`, `fair-trade`, `cartography`, clean Riftbound lore/threats replacing copied WoW strings, honest nearby waypoint scan), `TownNPC.cs` + `PlayerServices.cs` (`SC07` 10s bark chat/SFX cooldown, `SC09` ambient pause during modals/combat/death while keeping services reachable), and `EnemyController.cs` (`Captain Selvek` `encounter_bark` topic `"aggro"` + 25s cooldown, hostile state intact).

## AG03 / CF05–CF14 — Reference Encounter Slice (Initial Verified Slice)
- `PlayerController.cs`: Added bounded 1-slot primary input buffer (`160ms` window during last `350ms` of `attackReady`, cleared on death/dodge/guard/modal/target loss) and `combatActionEpoch` so LeftAlt dodge cancels swing windup damage cleanly (`CF06`, `CF09`).
- `EnemyController.cs`: Synchronized ranged caster (`Hexer`/`Sorcerer`) `damageAt` to `attackDuration * 0.85f` matching the visible `CastBarSystem` bar (`CF07`/`CF08`), added `SoundCue.Rune` audio telegraph on Heavy Slam windup, and added a `1.6s` stagger recovery window on Heavy Slam interrupt.

## Final Build & Post-Edit Verification Receipts
- **Executable (`Riftbound.Editor.CoopPrefabSetup.BuildCombatRefinement01`):** `Builds/RiftboundCombat01/Riftbound.exe` (`BUILD_EXIT=0`, git-ignored build output)
  - `EXE_SHA256=AF88B840E599661AC63FC10AE27AC77DD97CD46B44DF8A7CE60EB0DCFACFC99E`
  - `Assembly-CSharp.dll SHA256=986D4723A8481F0FD4D8A8A7028DE0358FF12A02613A32C316E899C6A9DD661C`
  - `level0 SHA256=AAC618DF434B9CE9748A2DB08EFB4EDA9FFC03CEAE06DC831B09EC9F914E43E6`
- **Post-Final-Edit Unity PlayMode & Editor Suites:**
  - `Riftbound.Editor.CombatRefinementChecks.VerifyParity`: `CF_EXIT=0`, `ERRORS=0` (`19/19 PASS`)
  - `Riftbound.Editor.Gp01Diagnostics.RunPlayModeVerify`: `GP_PLAY_EXIT=0`, `ERRORS=0` (`41/41 PASS`)
  - `Riftbound.Editor.QuestRouteSmokeTest.Run`: `QR_EXIT=0`, `QR_PASS=145 QR_FAIL=0 QR_TOTAL=150`
  - `Riftbound.Editor.CombinedSpawnVerification.Run`: `SPAWN_EXIT=0`, `TOTAL PASS=32 FAIL=0`
- **Post-Final-Edit Headless Relay Matrix on `Builds/RiftboundCombat01/Riftbound.exe`:**
  - Full 10-case sweep (`.code-check/integration/relay-20261117-075013/`): `9/10 PASS` (`Ui=0`, `InvalidCode=0`, `Resilience=0`, `Combat H=0/C1=0`, `Authority H=0/C1=0` with `unkAtkRej=True`, `Progression H=0/C1=0`, `LateJoin H=0/C1=0`, `ClientLeave H=0/C1=0`, `Capacity H=0/C2=0/C3=0/C4=0 + C1=1 rejected`; `FullAcceptance` failed with `kills=2/3, H=1/C1=1`).
  - Isolated re-run on exact same build (`.code-check/integration/relay-20261117-075638/`): `FullAcceptance PASS` (`FullAcceptance-H=0, FullAcceptance-C1=0`, `hostAccept=PASS(cid=1,ping=True,rc=True/True,revive=True,kills=0,qIdx=1)`, `clientAccept=PASS(spoofRej=True,abHit=True,abHeal=True,dismount=True,qAcc=True,qComp=True,rev=True,evt=True,noDblG=True,hostDisc=True)`).
- **Clean Scene/Asset State:** Reverted Unity-touched `.fbx.meta` and `UniversalRenderPipelineGlobalSettings.asset`; restored baseline `Docs/Integration/ONLINE_TEST_RESULTS.md`; `Outpost-Expanded.unity`, terrain, environment/road prefabs, and `Assets/RPG/Resources/Narrative/` remain completely untouched.
