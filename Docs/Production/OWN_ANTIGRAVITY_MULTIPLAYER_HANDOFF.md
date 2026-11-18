# Antigravity Multiplayer Presentation & Party Polish Handoff

- **Branch:** `multiplayer/antigravity-polish-20261118`
- **Worktree:** `c:\Users\skama\Documents\wow=rpg-diplomna\Multiplayer\antigravity-polish`
- **Reviewed Base Commit:** `033e15408795c25204959028d706f35a5c1d7f05` (`combat/antigravity-refinement-20261117`, runtime derived from `integration/combined-20261116` `8ad9071` / `76696a7`)
- **Plan Reference:** `Docs/Design/MULTIPLAYER_PRESENTATION_PARTY_PLAN.md` (`origin/story/codex-content-20261117` @ `f7ef4ee`)
- **Codex Branch Coordinated:** `origin/visuals/codex-presentation-20261118` @ `0fdc333` (9 Undead controller state fixes; released visual scripts untouched)
- **Unique Standalone Deliverable (Local Git-Ignored):** `Builds/RiftboundPresentation02/Riftbound.exe`
  - `EXE_SHA256=AF88B840E599661AC63FC10AE27AC77DD97CD46B44DF8A7CE60EB0DCFACFC99E`
  - `Assembly-CSharp.dll SHA256=247640286ECDC03DE8FB85C67FE33C2E0E8919EE58D9D33C986A812AB4893D72`
  - `level0 SHA256=AAC618DF434B9CE9748A2DB08EFB4EDA9FFC03CEAE06DC831B09EC9F914E43E6`

## Summary of Completed Scope (`MP00–MP04`, `GP01`)

| Packet | Status | Summary |
| --- | --- | --- |
| `MP00` / Contract | **COMPLETE** | Reproduced capsule mount, missing `MountId` subscription, `actionAnimUntil` death/stale bugs, and HUD overlaps; published [MP_PRESENTATION_API_CONTRACT.md](file:///c:/Users/skama/Documents/wow=rpg-diplomna/Multiplayer/antigravity-polish/Docs/Production/MP_PRESENTATION_API_CONTRACT.md) with exact released Git blob hashes and `GroundedVisual.SetObserverState(bool alive, bool airborne, bool mounted)` observer hook agreement |
| `MP01` Real Remote Mounts | **COMPLETE** | Replaced remote capsule placeholder with real `MountVisual` (`Mount Horse` / `Mount Zebra` / `Mount gryphon`), subscribed `MountId` + `IsMounted` + `Health`, fitted rider hips to `saddle` with terrain-raycast mount grounding (`ComputeRemoteMountGroundOffset`), hid axe while mounted, animated mount state, and enforced server death dismount & cleanup |
| `MP02` Action & Locomotion Timing | **COMPLETE** | Replicated `LocomotionFlags` (airborne/walking/blocking) and sequenced, server-allowlisted actions with actual clip durations and priority (`Dead > Mounted > Block > Action > Airborne > Locomotion`) across attack, cast, guard, dodge, interrupt, and cancellation |
| `MP03` Authoritative Group State | **COMPLETE** | Replicated `ResourcePct`, `IsLeader`, `IsInCombat`, `ReadyCheckId`, and presentation-only `SocialRole` (`Auto`/`Tank`/`Healer`/`DPS`) with late-join snapshot sync, `25s` server expiry, stale reply rejection, and non-leader promotion spoof rejection (`Host` retains transport authority) |
| `MP04` MMO Party Roster & Group Menu | **COMPLETE** | Replaced permanent top-right debug/join-code panel with collapsed `Group X/4 [O]` pill, compact MMO peer frames cleared below `[Alt] DODGE READY` (`py=134f`) with clean label padding, `[T]`/`[H]`/`[D]` badges, left-click actions, `[O]` Group & Session modal (join code scrubbed from header & masked until `Reveal`, `Role` cycle button), and opt-in `[G]` AI crew |
| `GP01` Combat Feel Slice | **COMPLETE** | Added abort-aware swing windup with gentle non-steering melee target tracking and class-differentiated confirmed impact audio/shake without duplicate playback |

## Verification Matrix (Post-Final-Edit `Presentation02` `24764028...`)

| Check | Status | Details |
| --- | --- | --- |
| `Riftbound.Editor.CombatRefinementChecks.VerifyParity` | **PASS (`23/23`, `ERRORS=0`)** | `CF01–04`, `SC02/04/05/09`, `CF06/08/09`, and `MP01–04` mount registry, allowlists, `SocialRole` & ground-offset bounds, & Group Menu modal checks |
| Rendered Host + Guest D3D11 Visual Check (`1280x720`) | **PASS (`VIS_HOST_EXIT=0, VIS_GUEST_EXIT=0`)** | `.code-check/visual-verify-02b/`: `horseReal=True, gryphonReal=True, dismountClear=True`; inspected `guest_horse_roster.png` & `guest_group_menu.png` (zero Dodge-pip overlap, masked invite code, `Role: Auto` toggle) |
| Headless Relay Targeted Suite (`4` Cases) | **PASS (`4/4`)** | `.code-check/integration/relay-20261118-054752/`: `Authority`, `LateJoin`, `FullAcceptance`, and `Capacity` (`4` admitted + `1` extra rejected; `Host avgMs=6.98, p95Ms=9.99`, `Clients avgMs=5.97–6.09`) |
| Unchanged Regression Baseline (`a81b5e3` / `Presentation01`) | **NOT RE-RUN ON `Presentation02`** | `Gp01Diagnostics` (`41/41`), `QuestRouteSmokeTest` (`145/145`), `CombinedSpawnVerification` (`32/32`), and Relay `Ui`, `InvalidCode`, `Resilience`, `Combat`, `Progression`, `ClientLeave` passed on prior `Presentation01` (`AAA54B13...`) |
| Codex Visual Grounding / Rig Clips (`CV01–CV04`) | **NOT RUN (Released to Codex)** | `Human_Mage` `"Ride"` orientation tilt, `MountVisual` stride divisors (`walkStride`/`runStride`), `Mount gryphon.prefab` wing/mesh visuals, & `GroundedVisual.SetObserverState` remain for Codex (`visuals/codex-presentation-20261118`) |
| Real Two-PC Cross-Network Play & Manual Soak | **NOT RUN** | Same-machine LAN/Relay checks are not two-PC WAN acceptance |
| Promoted `Resources/Narrative/first-five.v1.json` Load | **NOT RUN** | Awaits Codex promotion onto branch after `NARRATIVE_SCHEMA_ACCEPTANCE.md` |

## File Releases & Next Integration Steps for Codex
- **Released to Codex (zero Antigravity edits; exact SHA1 blobs recorded):** `Assets/RPG/Scripts/MountVisual.cs` (`5e1f9566...`), `Assets/RPG/Scripts/GroundedVisual.cs` (`600f6bc8...`), `Assets/RPG/Scripts/CharacterVisualFactory.cs` (`64b6f6e2...`) (see [MP_PRESENTATION_API_CONTRACT.md](file:///c:/Users/skama/Documents/wow=rpg-diplomna/Multiplayer/antigravity-polish/Docs/Production/MP_PRESENTATION_API_CONTRACT.md)).
- Codex `visuals/codex-presentation-20261118` can proceed on `CV01–CV04` (`Human_Mage` `"Ride"` orientation, `MountVisual` stride divisors, `Mount gryphon.prefab` distinction, and `GroundedVisual.SetObserverState` observer hooking) while keeping `Player Undead *.controller` (`0fdc333`) ready for final branch integration.
