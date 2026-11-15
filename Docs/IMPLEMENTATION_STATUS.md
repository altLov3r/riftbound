# Riftbound — Execution Plan Implementation Status

> NEW WORK ORDER: **Docs/ANTIGRAVITY_ONLINE_MULTIPLAYER_WORK_ORDER.md** is the active queue. The user now requires internet co-op using Unity Multiplayer Services sessions + Relay join codes, with existing client damage/reward/authority defects repaired first. Internet connection setup and cross-network acceptance are pending; older COMPLETE labels below describe prior claims and do not establish this new delivery gate. Read the work order before resuming network tests.

Updated: 2026-11-15  
Reference Plan: `Docs/ANTIGRAVITY_EXECUTION_PLAN.md`

## ACTIVE PRIORITY — multiplayer first; stop unchanged retry loops

Follow revision 3's MP-01–MP-08 queue and mandatory retry rules in ANTIGRAVITY_EXECUTION_PLAN.md before resuming any old test. No more world-content expansion until usable co-op is delivered, except independent work while a documented network blocker is unresolved.

Review correction: the saved smoke result inspected by Codex contains 77 PASS lines and then a Reaver terrain failure; the 82-pass claim below is historical and not supported by that inspected file. Package installation and scene/prefab creation have result receipts. A host-verification receipt was absent at review; two-process gameplay has not been established. Source review also found placeholder event authority, repeatable event rewards and a V-key conflict. Recheck current source before acting.

### Next-attempt record (fill before each test)

- Ticket / start time:
- Hypothesis and last reached stage:
- Expected observable result / timeout:
- Attempt count for this unchanged failure (maximum 2):
- Material change or new evidence since previous attempt:
- Observed result and evidence path:
- On stop: blocker, safe cleanup, independent next task or precise user question:

Stop an unchanged focused test after 10 minutes without new evidence; cap unchanged blocker investigation at 20 minutes. Do not reset counts across handoffs, repeatedly restart Unity or mark the gate passed to escape the loop.

## Phase 0 — Playable Baseline
- **Compilation**: Verified via Unity 6000.6.2f1 batchmode (`EXIT:0`, 0 compiler errors).
- **Camera & AudioListener**: Verified in `Outpost-Expanded.unity` (`Active Cameras: 1, Active AudioListeners: 1`, `Missing/Error Materials: 0`).
- **Smoke Test (`Riftbound.Editor.QuestRouteSmokeTest.Run`)**:
  - **77 PASS lines** followed by known baseline defect `FAIL Camp uses a separate terrain asset` at `QuestRouteSmokeTest.cs:317` (`WorldRegions.TerrainAt(RiverReaverCamp.Position)` returned `World tile 0-0.asset` rather than `Assets/RPG/Terrain/ReaverCamp/Reaver approach.asset`). Preserved without overwriting scene terrain pending explicit terrain-asset decision.
- **Existing Living-World Inventory**: `Wildlife.cs`, `RoadPatrolGuard.cs`, `WeatherSystem.cs`, `ZoneAudioAmbience.cs`, `TownNPC.cs`.

## Phase 1 — Trees & Worldbuilding Defects
- **Root Cause Found**: All 13 `Assets/RPG/Prefabs/Nature/T_*Tree_*.prefab` and `T_Rock_*.prefab` assets had `localRotation = (-90, 0, 0)` and `localScale = (100, 100, 100)` directly on the root GameObject alongside `MeshFilter`/`MeshRenderer`. Unity Terrain's `TreeInstance` renderer ignores root prefab rotation, causing **4,501 terrain-painted trees and rocks** across the 4 tiles to render lying sideways.
- **Fix Applied (`Assets/RPG/Editor/Phase1TreeFix.cs`)**:
  - Baked the `-90°` X rotation and `100x` scale into upright mesh assets under `Assets/RPG/Prefabs/Nature/UprightMeshes/*_Upright.asset` and reset all 13 `T_*.prefab` roots to `Quaternion.identity` and `Vector3.one` (preserving all `.meta` GUIDs and materials).
  - Grounded 6 floating/buried `Expanded *Tree_*` GameObjects in `Outpost-Expanded.unity`.
  - Verified `0` floating/buried ground props (excluding wall-mounted `Lantern_Wall` fixtures).

## Phase 2 & 3 — Coherent Zones & Living World
- **Combat & Traversal Polish**:
  - `OrbitCamera.Shake`, `ClassSpells.cs` procedural `ParticleSystem` trails/explosions/shockwaves/soul orbs, `TargetSelectionRing.cs`, `AmbientParticles.cs` (zone-aware), `CombatStreakSystem.cs`, `TargetFrameHUD.cs` (Target Frame + Boss Bar + `[Alt]` Dodge Pip), `WorldShrineSystem.cs` (4 interactive terrain-grounded shrines), `[Alt]` Dodge Roll with i-frames (`PlayerController.cs` + `Combatant.cs`, resolved `[V]` mount conflict), and Elite Enemy Affixes (`Molten`, `Vampiric`, `Frostbound`, `Arcane`).
- **Living World Events (`Assets/RPG/Scripts/LivingWorldEvents.cs`)**:
  - Host-authoritative (`IsAuthoritative`: checks `NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer`) boundary for multiplayer.
  - **Ambient Road Traveller** (`AmbientTraveller_RoadPorter`) patrolling between Emberwatch and Westbank.
  - **Event Prototype 1 (`supply_caravan_westbank`)**: Travelling Supply Caravan with mid-route Raider ambush, leave-area failure/cleanup, 120s cooldown, and first-clear (`+60 XP`, `+35 Gold`) vs 50% repeatable bounty (`completedEventIds`).
  - **Event Prototype 2 (`roadside_ambush_greenhollow`)**: Roadside Ambush/Rescue for a pinned-down Frontier Scout with cleanup, 120s cooldown, and first-clear (`+55 XP`, `+30 Gold`) vs 50% repeatable bounty.

## Phase 4 — Multiplayer Foundation (MP-01 & MP-02 VERIFIED)
- **Installed Package**: `com.unity.netcode.gameobjects@2.14.0` (with `com.unity.transport`) via UPM.
- **Additive Test Scene & Prefab**:
  - `Assets/RPG/Prefabs/NetworkPlayerAvatar.prefab` (`CharacterController`, `NetworkObject`, `NetworkTransform`, `RiftboundNetworkPlayer` with server-authoritative `SubmitMoveInputServerRpc`).
  - `Assets/RPG/Scenes/NetworkTest.unity` (`NetworkManager`, `UnityTransport`, `LanSessionManager`, `ArenaGround`).
- **LAN Session Controls (`LanSessionManager.cs`)**: `Host LAN`, `Join LAN`, `Disconnect`, `MaxPlayers = 4` `ConnectionApproval` on both host and client, `0.0.0.0` bind, `DisconnectReason` surfacing, and offline save isolation.
- **2-Process Verification (`Builds/NetworkTest/RiftboundNetTest.exe`, 2026-11-15)**:
  - `HOST_EXIT:0 CLIENT_EXIT:0` over `127.0.0.1:7777`
  - `PASS Host: clients=2, avatars=2, spawnSep=3.65m, p0Moved=0.00m, p1Moved=1.00m`
  - `PASS Client: clientId=1, avatars=2, ownedCount=1, moved=0.80m`

## Active Priority — Online Co-op Work Order (`Docs/ANTIGRAVITY_ONLINE_MULTIPLAYER_WORK_ORDER.md`)
- **Superseded**: LAN-first `MP-01`–`MP-08` completion claims are superseded by `ANTIGRAVITY_ONLINE_MULTIPLAYER_WORK_ORDER.md` (`O-00`–`O-05`).
- **Environment & Packages**: Unity `6000.6.2f1`, `com.unity.netcode.gameobjects@2.14.0`, cloud project `3138f0db-6d7e-4e5c-b527-395d23acf091` (`organizationId: skamaz3434`).
- **Narrow Receipts Preserved**:
  - `Builds/NetworkTest/RiftboundNetTest.exe` (capsule arena): 2-process localhost spawn + movement (`HOST_EXIT:0 CLIENT_EXIT:0`).
  - `Builds/CoopRelease/Riftbound.exe` (`Outpost-Expanded.unity` narrow smoke): 2-process localhost spawn (`sep=9.76m`), single camera/AudioListener, and enemy HP reduction (`silverglen-mana-sprite-4 hp=35/70`).
  - **Known Limitations Identified in O-00 Audit**: Client HP loss & kill credit RPCs were dispatched through `RiftboundCoopPlayer.LocalOwner` (Host's avatar) while gating on `IsOwner`, preventing client receipt; client combat submitted raw damage numbers rather than ability/target validation; `SubmitStateServerRpc` bypassed speed checks when `dist > 30f` and trusted client HP/level; `LivingWorldEvents` actors were not replicated to clients; UGS Sessions + Relay is not yet installed/wired.
- **Offline Regression Baseline**: `QuestRouteSmokeTest.Run` reaches `77 PASS` lines and stops at known baseline failure `FAIL Camp uses a separate terrain asset` (`QuestRouteSmokeTest.cs:317`); full offline suite is not claimed as fully passing while that failure remains.

## Active Work Queue (`O-00` – `O-05`)
- **O-00 (Verified)**: Evidence corrected; cloud linkage `3138f0db-6d7e-4e5c-b527-395d23acf091` (`skamaz3434`) & `com.unity.netcode.gameobjects@2.14.0` recorded.
- **O-01 (Source Implemented + Narrow 2-Process Verified)**:
  - Fixed targeted client HP/kill credit routing (`RiftboundCoopPlayer.FindByClientId` + `RpcTarget.Single`), server-computed ability damage + cooldown/range/LOS checks (`RequestAbilityAttackServerRpc`), velocity-bounded movement (`SubmitMoveStateServerRpc` + `AuthorizeHostTeleport`), host-owned vitals/progression, chest/quest prerequisite validation, mount/action animation sync, and `LivingWorldEvents` reward dispatch.
- **O-02 (Verified)**: Installed `com.unity.services.multiplayer@2.4.1`; documented dashboard/quota/profile/persistence policy in `Docs/ONLINE_SETUP.md`.
- **O-03 & O-04 (Implemented + Build Verified)**: `OnlineSessionService.cs` (state machine + Relay-backed `CreateSessionAsync` / `JoinSessionByCodeAsync` + cancellation + friendly errors), `FrontendMenu.cs` (`Offline` / `Host Online` / `Join Online`), `CoopWorldManager.cs` (Join Code + Copy Code + Leave Session + explicit session-only progression label; offline saves untouched).
- **O-05 Verification Matrix**:
  - **2 standalone processes, separate UGS profiles, Relay enabled (`Builds/CoopRelease/Riftbound.exe`)**: **PASS** (`FOUND_CODE:KF86QG`, `HOST_EXIT:0 CLIENT_EXIT:0`, `players=2`, `localOwner=True`, `cams=1`, `listeners=1`, `enemy=road-raider-1 hp=36/70`, `clientDamaged=True`).
  - **3/4 players and 5th join attempt over live Relay**: **PASS** (`FOUND_CODE:TKG7H8`, `EXITS H=0 C1=0 C2=0 C3=0 C4=1`, all 4 reached `players=4`; 5th client `C4` rejected with `[OnlineSessionService] Join failed: session is full`).
  - **Invalid/expired join code handling**: **PASS** (`INVALID_CODE_EXIT:0`, `PASS InvalidCode: state=Failed, err=Join code is invalid or expired.`).
  - **Offline regression (`QuestRouteSmokeTest.Run`)**: **77 PASS** lines followed by pre-existing `FAIL Camp uses a separate terrain asset` (`QuestRouteSmokeTest.cs:317`).
  - **2 PCs on different internet connections (e.g. home + mobile hotspot)**: **NOT RUN** (single-machine environment; ready for user cross-network test).
  - **Interactive UI Host/Leave/Rejoin cycles & ordinary-input full quest/chest/mount/respawn/party-extras walkthrough**: **NOT RUN** in interactive UI (implemented in source: compact party frames, `[P]` map pings, `[Y]` ready checks, `[E]` ally revive).

## Standalone Online Co-op Instructions (`Builds/CoopRelease/Riftbound.exe`)
1. **Host**: Run `Unity Project/My project (1)/Builds/CoopRelease/Riftbound.exe` → **Play** → select/create character → select **Host Online** → **HOST ONLINE**. Share the generated 6-character Join Code shown in the top-right HUD (use **Copy Code**).
2. **Client(s)** (up to 4 total): Run `Riftbound.exe` → **Play** → select/create character → select **Join Online** → paste the **Session Join Code** → **JOIN ONLINE**.
3. **Offline Single-Player**: Keep **Offline** selected and click **ENTER WORLD** (no internet or UGS initialization required; offline saves preserved).
4. **Known Limitations**: Online co-op uses session-only progression (not saved to disk), and Host departure ends the session (no host migration in this release).
