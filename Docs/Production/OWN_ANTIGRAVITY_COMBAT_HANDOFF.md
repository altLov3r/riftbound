# Antigravity Combat & NPC Runtime Refinement Handoff

- **Branch:** `combat/antigravity-refinement-20261117`
- **Worktree:** `c:\Users\skama\Documents\wow=rpg-diplomna\Combat\antigravity-refinement`
- **Base Published Commit:** `8ad9071820c2fbed32fbb728122ffb2f02980888` (`integration/combined-20261116`, runtime source `76696a7bff9e86f40a5c7fb90747fac5a290be8b`)
- **Build Output (Local Git-Ignored):** `Builds/RiftboundCombat01/Riftbound.exe`
  - `EXE_SHA256=AF88B840E599661AC63FC10AE27AC77DD97CD46B44DF8A7CE60EB0DCFACFC99E`
  - `Assembly-CSharp.dll SHA256=986D4723A8481F0FD4D8A8A7028DE0358FF12A02613A32C316E899C6A9DD661C`
  - `level0 SHA256=AAC618DF434B9CE9748A2DB08EFB4EDA9FFC03CEAE06DC831B09EC9F914E43E6`

## Summary of Delivered Scope

| Packet | Status | Highlights |
| --- | --- | --- |
| `AG00` / `CF00` | **COMPLETE** | Closed all 6 remaining Relay gates (`Authority`, `Progression`, `LateJoin`, `FullAcceptance`, `ClientLeave`, `Capacity`) + `32/32` spawn check on read-only `Combined02` before source changes |
| `AG01` / `CF01–CF04` | **COMPLETE** | Reproduced and fixed empty-swing resource mutation, dormant `incomingDamageMultiplier=0` chip dmg, remote enemy LOS bypass / projectile sync, closed server ability & heal allowlists, Mage `25f` interrupt parity, and server-confirmed hit FX/UI |
| `AG02` / `SC02,04,05,07,09` | **COMPLETE (Adapter Ready)** | Signed Codex `SC02` contract acceptance (`Docs/Production/NARRATIVE_SCHEMA_ACCEPTANCE.md`), implemented `Scripts/Narrative/NarrativeCatalog.cs` read-only JSON consumer with fail-closed `UNKNOWN` & per-viewer isolation, wired Abe/Rowan/Mira/Elara/Selvek (`"aggro"` topic) across HUD/Shop/Gossip/TownNPC/EnemyController with bark cooldowns and ambient modal pause |
| `AG03` / `CF05–CF14` | **INITIAL SLICE VERIFIED** | Added bounded 1-slot primary input buffer, LeftAlt dodge cancel epoch (`CF06`/`CF09`), synchronized ranged caster `damageAt` with `CastBarSystem` duration, Heavy Slam `SoundCue.Rune` audio tell, and `1.6s` Heavy Slam interrupt stagger (`CF07`/`CF08`) |

## Verification Matrix (Post-Final-Edit State)

| Check | Status | Notes / Source |
| --- | --- | --- |
| `Riftbound.Editor.CombatRefinementChecks.VerifyParity` | **PASS (`19/19`, `ERRORS=0`)** | PlayMode unit/integration suite (`CF01–CF04`, `SC02/04/05/09`, `CF06/08/09`) |
| `Riftbound.Editor.Gp01Diagnostics.RunPlayModeVerify` | **PASS (`41/41`, `ERRORS=0`)** | Full gameplay PlayMode diagnostics |
| `Riftbound.Editor.QuestRouteSmokeTest.Run` | **PASS (`145/145`, `0 FAIL`)** | All quest & route checks |
| `Riftbound.Editor.CombinedSpawnVerification.Run` | **PASS (`32/32`, `0 FAIL`)** | Terrain & settlement spawn grounding |
| Headless Relay `10`-Case Sweep (`RiftboundCombat01`) | **9/10 PASS + 1 Isolated Retry PASS** | `.code-check/integration/relay-20261117-075013/` passed `9/10` (`FullAcceptance` failed at `kills=2/3`); isolated re-run `.code-check/integration/relay-20261117-075638/` on the identical binary passed `FullAcceptance` (`H=0, C1=0`) |
| Promoted `Resources/Narrative/first-five.v1.json` load | **NOT RUN** | Pending Codex promotion of staged JSON into `Assets/RPG/Resources/Narrative/` after pulling `NARRATIVE_SCHEMA_ACCEPTANCE.md` |
| Manual Windowed Playthrough & Native Visual Soak | **NOT RUN** | Requires interactive desktop session |
| Real Two-PC Cross-Network Relay Acceptance | **NOT RUN** | Headless same-machine Relay is not a substitute |
| Remaining `AG03` Encounter/Boss Tuning (`CF10–CF23`) | **NOT RUN** | Deferred cleanly after reference encounter mechanics slice |

## Coordination Note for Codex (`story/codex-content-20261117`)
- [NARRATIVE_SCHEMA_ACCEPTANCE.md](file:///c:/Users/skama/Documents/wow=rpg-diplomna/Combat/antigravity-refinement/Docs/Production/NARRATIVE_SCHEMA_ACCEPTANCE.md) records formal `ACCEPTED` sign-off against commit `86051239bfb0ed9250b67955035c0cf6fa5c5b9d` / frozen LF SHA256s from `3fc0dcf4be005a6ef4e7d23f74f74555d0a1006f`.
- `NarrativeCatalog.cs` loads `Resources.Load<TextAsset>("Narrative/first-five.v1")` (`Assets/RPG/Resources/Narrative/first-five.v1.json`), validates `schemaVersion == 1`, `catalogId == "riftbound.first-five.v1"`, `profiles.Length == 5`, supports all 9 channels (including Selvek `encounter_bark` with `topicId == "aggro"`) and all 10 predicates, and enforces per-viewer `FactionQuestSystem` lookup without singleton spillover.
