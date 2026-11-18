# Antigravity Multiplayer Presentation & Party Polish Handoff

- **Branch:** `multiplayer/antigravity-polish-20261118`
- **Worktree:** `c:\Users\skama\Documents\wow=rpg-diplomna\Multiplayer\antigravity-polish`
- **Reviewed Base Commit:** `033e15408795c25204959028d706f35a5c1d7f05` (`combat/antigravity-refinement-20261117`, runtime derived from `integration/combined-20261116` `8ad9071` / `76696a7`)
- **Plan Reference:** `Docs/Design/MULTIPLAYER_PRESENTATION_PARTY_PLAN.md` (`origin/story/codex-content-20261117` @ `f7ef4ee`)
- **Unique Standalone Deliverable (Local Git-Ignored):** `Builds/RiftboundPresentation01/Riftbound.exe`
  - `EXE_SHA256=AF88B840E599661AC63FC10AE27AC77DD97CD46B44DF8A7CE60EB0DCFACFC99E`
  - `Assembly-CSharp.dll SHA256=AAA54B136DCF35AC23D658774867EA728B00D32CC8DB82C8CD2D3B407330854C`
  - `level0 SHA256=AAC618DF434B9CE9748A2DB08EFB4EDA9FFC03CEAE06DC831B09EC9F914E43E6`

## Summary of Completed Scope (`MP00–MP04`, `GP01`)

| Packet | Status | Summary |
| --- | --- | --- |
| `MP00` / Contract | **COMPLETE** | Reproduced capsule mount, missing `MountId` subscription, `actionAnimUntil` death/stale bugs, and HUD overlaps; published [MP_PRESENTATION_API_CONTRACT.md](file:///c:/Users/skama/Documents/wow=rpg-diplomna/Multiplayer/antigravity-polish/Docs/Production/MP_PRESENTATION_API_CONTRACT.md) and released `MountVisual.cs`, `GroundedVisual.cs`, and `CharacterVisualFactory.cs` |
| `MP01` Real Remote Mounts | **COMPLETE** | Replaced remote capsule placeholder with real `MountVisual` (`Mount Horse` / `Mount Zebra` / `Mount gryphon`), subscribed `MountId` + `IsMounted` + `Health`, fitted rider hips to `saddle`, hid axe while mounted, animated mount state, and enforced server death dismount & cleanup |
| `MP02` Action & Locomotion Timing | **COMPLETE** | Replicated `LocomotionFlags` (airborne/walking/blocking) and sequenced, server-allowlisted actions with actual clip durations and priority (`Dead > Mounted > Block > Action > Airborne > Locomotion`) across attack, cast, guard, dodge, interrupt, and cancellation |
| `MP03` Authoritative Group State | **COMPLETE** | Replicated `ResourcePct`, `IsLeader`, `IsInCombat`, and `ReadyCheckId` with late-join snapshot sync, `25s` server expiry, stale reply rejection, and non-leader promotion spoof rejection (`Host` still owns transport session; no fake host migration) |
| `MP04` MMO Party Roster & Group Menu | **COMPLETE** | Replaced permanent top-right debug/join-code panel with collapsed `Group X/4 [O]` pill, compact MMO peer frames with HP/resources/leader/status/left-click actions, `[O]` Group & Session modal (`••••••` invite code reveal/copy), and separate opt-in `[G]` AI crew |
| `GP01` Combat Feel Slice | **COMPLETE** | Added abort-aware swing windup with gentle non-steering melee target tracking and class-differentiated confirmed impact audio/shake without duplicate playback |

## Verification Matrix (Post-Final-Edit `AAA54B13...`)

| Check | Status | Details |
| --- | --- | --- |
| `Riftbound.Editor.CombatRefinementChecks.VerifyParity` | **PASS (`22/22`, `ERRORS=0`)** | `CF01–04`, `SC02/04/05/09`, `CF06/08/09`, and `MP01/02/04` mount registry, allowlists, & companion opt-in checks |
| `Riftbound.Editor.Gp01Diagnostics.RunPlayModeVerify` | **PASS (`41/41`, `ERRORS=0`)** | Core gameplay PlayMode verification |
| `Riftbound.Editor.QuestRouteSmokeTest.Run` | **PASS (`145/145`, `0 FAIL`)** | All quest & route checks |
| `Riftbound.Editor.CombinedSpawnVerification.Run` | **PASS (`32/32`, `0 FAIL`)** | Settlement & terrain spawn grounding |
| Rendered Host + Guest D3D11 Visual Check (`1280x720`) | **PASS (`H=0, C1=0`)** | `.code-check/visual-verify/`: verified real `Mount Horse` & `Mount gryphon`, teardown on dismount, clean Group Pill & Group Menu |
| Headless Relay `10`-Case Sweep | **9/10 PASS + 1 Isolated Retry PASS** | `.code-check/integration/relay-20261118-052649/` passed `9/10` (including `Authority` `realMnt=True, mntClr=True, ldrSpoofRej=True` & `LateJoin` `rcDecline=True`; `FullAcceptance` failed at `kills=2/3`); isolated re-run `.code-check/integration/relay-20261118-053125/` passed `FullAcceptance` (`H=0, C1=0`) |
| Codex Visual Grounding / Rig Clips (`CV01–CV04`) | **NOT RUN (Released to Codex)** | Rendered `host_horse_roster.png` confirmed `Human_Mage` (`Player Humanoid Caster.controller` `"Ride"`) tilts horizontally on saddle (`Human_Warrior` upright in `guest_horse_roster.png`); `Mount gryphon.prefab` visual parity & `MountVisual` stride/saddle calibration remain for Codex |
| Real Two-PC Cross-Network Play & Manual Soak | **NOT RUN** | Same-machine LAN/Relay checks are not two-PC WAN acceptance |
| Promoted `Resources/Narrative/first-five.v1.json` Load | **NOT RUN** | Awaits Codex promotion onto branch after `NARRATIVE_SCHEMA_ACCEPTANCE.md` |

## File Releases & Next Integration Steps for Codex
- **Released to Codex (zero Antigravity edits):** `Assets/RPG/Scripts/MountVisual.cs`, `Assets/RPG/Scripts/GroundedVisual.cs`, `Assets/RPG/Scripts/CharacterVisualFactory.cs` (see [MP_PRESENTATION_API_CONTRACT.md](file:///c:/Users/skama/Documents/wow=rpg-diplomna/Multiplayer/antigravity-polish/Docs/Production/MP_PRESENTATION_API_CONTRACT.md)).
- Codex `visuals/codex-presentation-20261118` can inspect `Human_Mage` `"Ride"` orientation, `MountVisual` stride divisors (`walkStride`/`runStride`), `Mount gryphon.prefab` wing/saddle distinction, and `GroundedVisual` observer hooking (`RemoteVisualRoot`).
