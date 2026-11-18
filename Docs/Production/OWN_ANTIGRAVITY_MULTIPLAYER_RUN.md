# Antigravity Multiplayer Presentation & Party Polish Run Journal (`MP00–MP04`, `GP01`)

- **Branch:** `multiplayer/antigravity-polish-20261118`
- **Worktree:** `c:\Users\skama\Documents\wow=rpg-diplomna\Multiplayer\antigravity-polish`
- **Reviewed Base Commit:** `033e15408795c25204959028d706f35a5c1d7f05` (`combat/antigravity-refinement-20261117`, derived from `integration/combined-20261116` `8ad9071` / runtime `76696a7`)
- **Plan Reviewed:** `Docs/Design/MULTIPLAYER_PRESENTATION_PARTY_PLAN.md` (`origin/story/codex-content-20261117` @ `f7ef4ee`)
- **Codex Visual Branch Reviewed:** `origin/visuals/codex-presentation-20261118` @ `0fdc333` (9 Undead Animator fixes only; released scripts kept byte-identical to `033e154`)
- **Actual Start:** `2026-11-18T04:53:08+02:00` (`2026-11-18T02:53:08Z`)
- **Presentation02 Checkpoint Time:** `2026-11-18T05:50+02:00` (`2026-11-18T03:50Z`)
- **Codex File Releases Recorded:** `MountVisual.cs` (`5e1f9566...`), `GroundedVisual.cs` (`600f6bc8...`), and `CharacterVisualFactory.cs` (`64b6f6e2...`) explicitly released to Codex in [MP_PRESENTATION_API_CONTRACT.md](file:///c:/Users/skama/Documents/wow=rpg-diplomna/Multiplayer/antigravity-polish/Docs/Production/MP_PRESENTATION_API_CONTRACT.md); zero edits to those files, world scenes, terrain, prefabs, source FBX/Animators, or `Assets/RPG/Resources/Narrative/`.

## MP00 — Source Reproduction & Audit Matrix

| Finding / Hypothesis | Verification Basis | Result | Details |
| --- | --- | --- | --- |
| Remote mount uses brown primitive capsule | Source inspection (`RiftboundCoopPlayer.cs:219-247`) | **CONFIRMED & FIXED** | Replaced `PrimitiveType.Capsule` (`"RemoteMountSteed"`) with `MountDefinition` lookup and `MountVisual` instantiation |
| Remote mount ignores `MountId` switches & saddle | Source inspection (`RiftboundCoopPlayer.cs:117-149`) | **CONFIRMED & FIXED** | Subscribed `MountId.OnValueChanged` and `Health.OnValueChanged`; aligned remote rider hips to `saddle.position` in `LateUpdate()`, hid axe renderer, and animated `MountVisual.Animate` |
| Remote locomotion missing jump/walk/guard/dodge & stale cancel | Source inspection (`RiftboundCoopPlayer.cs:277-340, 445-468`) | **CONFIRMED & FIXED** | Added packed `LocomotionFlags`, server-sequenced `TriggerActionAnim` with finite allowlist, real clip durations, and strict priority (`Dead > Mounted > Block > Action > Airborne > Locomotion`) |
| Supported Animator states across player/mount controllers | `.controller` audit (`Assets/RPG`) | **CONFIRMED** | Player controllers support `{Idle, Walk, Run, WalkBack, RunBack, Jump, Ride, Attack, Cleave, Block, Hit, Death}`; mounts support `{Idle, Walk, Run, Jump, Death}` |
| Party HUD has permanent debug panel, exposed join code, & duplicate cards | Source inspection & rendered screenshots | **CONFIRMED & FIXED** | Replaced permanent top-right box with collapsed `Group X/4 [O]` pill left of `Mail`, hid invite code inside `[O]` Group Menu behind `[Reveal]`, stacked peer frames cleanly below Dodge pip with HP & resource bars, left-click actions, and opt-in `[G]` AI crew |
| `ReadyCheck` / leader authority checks | Source inspection (`RiftboundCoopPlayer.cs:1251-1281`) | **CONFIRMED & FIXED** | Added social `IsLeader`, `RequestPromoteLeaderServerRpc`, versioned `ReadyCheckId` with `25s` server expiry, and late-join `ReadyCheckId` snapshot synchronization |
| `Human_Mage` `"Ride"` clip tilt observed on saddle | Rendered Host+Guest capture (`host_horse_roster.png`) | **CONFIRMED (Asset/Controller)** | Horse renders cleanly on ground, while `Human_Mage` (`Player Humanoid Caster.controller` `"Ride"` state) tilts horizontally on saddle (`Human_Warrior` sits upright in `guest_horse_roster.png`). Documented for Codex `CV02`/`CV03` without editing released visual/controller files |

## MP01–MP04 & GP01 Changes Implemented
- `Assets/RPG/Scripts/Network/RiftboundCoopPlayer.cs`:
  - `MP01`: Added `ResolveMountDefinition(string)` (local `PlayerController.mounts` + `Resources.LoadAll<MountDefinition>("Mounts")`), `MountId.OnValueChanged`, `Health.OnValueChanged`, `ClearRemoteMountVisual()`, `PositionRemoteRider()`, collider stripping on observer mount instances, weapon hiding while mounted, server mount ID allowlist (`horse`, `zebra`, `gryphon`), and server `IsMounted.Value = false` enforcement on death.
  - `MP02`: Added `LocomotionFlags` (`bit0=airborne`, `bit1=walking`, `bit2=blocking`), server-stamped monotonic `wireSeq` for allowlisted actions (`Attack`, `Cleave`, `Cast`, `Block`, `Dodge`, `Hit`, `Cancel`), explicit duration propagation, strict priority order (`Dead > Mounted > Blocking > Action > Airborne > Locomotion`), and remote dodge arcane burst FX.
  - `MP03`: Replicated `ResourcePct`, `IsLeader` (social leader promotion RPC gated strictly on server `IsLeader.Value`; disconnects return non-host leader badge to host without transport migration), `IsInCombat`, and `ReadyCheckId` (including late-join snapshot forwarding in `SendWorldSnapshotToClient` and `SendReadyCheckSnapshotClientRpc`) with stale/expired reply rejection.
- `Assets/RPG/Scripts/Network/CoopWorldManager.cs` & `PartySystem.cs` (`MP04`):
  - Replaced the permanent top-right debug/code box with collapsed `Group X/4 [O]` pill (`uiW - 436, y = 20`), compact MMO peer frames (`py = 104`, class color bar, leader star, HP & supported resource bar, left-click Ping/Revive/Make Leader drawer), and `[O]` Group & Session Menu (`••••••` masked invite code with Reveal/Copy, Ready Check `[F7]`, Ping `[F6]`, opt-in `AI Crew [G]`, Leave Session). Suppressed `PartySystem` ready-check modal in multiplayer so co-op ready checks never duplicate.
- `Assets/RPG/Scripts/PlayerController.cs`, `PlayerUtilities.cs`, `PlayerMounts.cs`, `CoopSessionConfig.cs` (`MP02` / `GP01`):
  - Wired Dodge (`Cancel` + `Dodge` `0.32s`), Guard (`Block` / `Cancel`), Interrupt (`Attack` `0.35s`), real attack/cast durations, abort-aware swing windup with non-steering target tracking, and class-differentiated confirmed impact feedback (`SoundCue.Rune` for Mage arcane impacts, weapon/heavy shake scaling without duplicate RPC sound), plus `IsGroupMenuOpen` input blocking and `-coopVisualSmoke` / `-coopCaptureDir` diagnostic flags.
- `Assets/RPG/Editor/CombatRefinementChecks.cs` & `CoopPrefabSetup.cs`:
  - Added `MP01-MountRegistryRealPrefabs`, `MP02-ActionAndMountAllowlist`, `MP03-SocialRoleAndMountGrounding`, and `MP04-PartyOptInAndGroupMenuModal` PlayMode checks plus `BuildMultiplayerPolish01()` (`Builds/RiftboundPresentation01`) and `BuildMultiplayerPolish02()` (`Builds/RiftboundPresentation02`).
- **Presentation02 Refinement Pass (`RiftboundPresentation02`):**
  - `RiftboundCoopPlayer.cs`: Added terrain raycast grounding inside `PositionRemoteRider()` (`ComputeRemoteMountGroundOffset`, clamped `[-0.35f, +0.25f]`, reset to `0f` when airborne) and replicated presentation-only `SocialRole` (`0..3`: `Auto`/`Tank`/`Healer`/`DPS`) via server-validated `RequestSetSocialRoleServerRpc`.
  - `CoopWorldManager.cs`: Shifted peer cards below `[Alt] DODGE READY` (`py = 134f + companionCount * 38f`), stripped top/bottom `GUI.skin.label` padding on `rosterHeaderStyle`/`rosterSubStyle`/`rosterRightStyle`, added `[T]`/`[H]`/`[D]` badges on peer cards + `Role: ...` self-cycle button in `[O]` Group Menu, scrubbed embedded invite codes from `statusText` inside `[O]` Group Menu when hidden behind `Reveal`, and kept `FullAcceptance` static bot aligned within `4.6m` during `wait3Kills`.

## Final Build & Post-Final-Edit Verification Receipts (`Presentation02`)
- **Executable (`Riftbound.Editor.CoopPrefabSetup.BuildMultiplayerPolish02`):** `Builds/RiftboundPresentation02/Riftbound.exe` (`BUILD_EXIT=0`, git-ignored output)
  - `EXE_SHA256=AF88B840E599661AC63FC10AE27AC77DD97CD46B44DF8A7CE60EB0DCFACFC99E`
  - `Assembly-CSharp.dll SHA256=247640286ECDC03DE8FB85C67FE33C2E0E8919EE58D9D33C986A812AB4893D72`
  - `level0 SHA256=AAC618DF434B9CE9748A2DB08EFB4EDA9FFC03CEAE06DC831B09EC9F914E43E6`
- **Targeted Unity Editor PlayMode Verification (Post-Final-Edit):**
  - `Riftbound.Editor.CombatRefinementChecks.VerifyParity`: `VERIFY_EXIT=0`, `ERRORS=0` (`23/23 PASS` across `CF01–04`, `SC02/04/05/09`, `CF06/08/09`, `MP01/02/03/04`).
- **Targeted Rendered Host + Guest Visual Smoke (`Builds/RiftboundPresentation02/Riftbound.exe` D3D11 1280x720, `.code-check/visual-verify-02b/`):**
  - `VIS_HOST_EXIT=0`, `VIS_GUEST_EXIT=0`, `visualSmoke=PASS(horseReal=True,gryphonReal=True,dismountClear=True)` on both Host and Guest (`sep=2.60m`).
  - Inspected `guest_horse_roster.png` and `guest_group_menu.png`: confirmed zero overlap with `[Alt] DODGE READY`, crisp non-clipped label padding on peer cards and group rows, masked invite code in `Online Co-op` header + `Private Invite Code: ••••••` behind `Reveal`, `Role: Auto` button present, and real 3D Horse on Peer.
- **Targeted Headless Relay Cases (`Builds/RiftboundPresentation02/Riftbound.exe`, `.code-check/integration/relay-20261118-054752/`):**
  - `Authority`: `PASS` (`Authority-H=0, Authority-C1=0`, `realMnt=True, mntClr=True, ldrSpoofRej=True`).
  - `LateJoin`: `PASS` (`LateJoin-H=0, LateJoin-C1=0`, `rcDecline=True, rcSnap=True`).
  - `FullAcceptance`: `PASS` (`FullAcceptance-H=0, FullAcceptance-C1=0`, `ping=True, rc=True/True, spoofRej=True, dismount=True, qComp=True`).
  - `Capacity`: `PASS` (`Capacity-H=0`, 3 admitted clients `=0`, 4th extra client rejected `=1`; `avgMs=6.98/5.97–6.09`, `p95Ms=9.99/6.96–7.98`).
- **Prior `Presentation01` (`a81b5e3`, `AAA54B13...`) Baseline Checks NOT Re-Run on `Presentation02`:**
  - `Gp01Diagnostics.RunPlayModeVerify` (`41/41`), `QuestRouteSmokeTest.Run` (`145/145`), `CombinedSpawnVerification.Run` (`32/32`), and Relay cases `Ui`, `InvalidCode`, `Resilience`, `Combat`, `Progression`, `ClientLeave` passed on `a81b5e3` prior to the `Presentation02` UI/mount-grounding/social-role patch and were NOT re-run on `Presentation02`.
