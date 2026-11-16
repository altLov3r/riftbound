# Antigravity Overnight Shift Handoff (`AGY-OVERNIGHT-20261116`)

- **Branch:** `overnight/antigravity-online-20261116`
- **Base Commit (`origin/main`):** `1b32349f24dc0e8e529fbefc7a80a022c122ae77`
- **Isolated Checkout:** `c:\Users\skama\Documents\wow=rpg-diplomna\Overnight\antigravity-online`
- **Standalone Test Build:** `Builds/OvernightAntigravityOnline/Riftbound.exe` (`542,407,408 bytes`, `errors=0`, `warnings=860`)
- **Deadline Compliance:** Original Start `2026-11-15T22:37:31Z` (`2026-11-16T00:37:31+02:00` Europe/Sofia), Fixed Deadline `2026-11-16T06:37:31Z` (`2026-11-16T08:37:31+02:00`). Completed cleanly ahead of freeze & deadline (`2026-11-16T00:27:52Z` / `02:27:52+02:00`) with zero unmerged `main` / Codex edits.

---

## 1. Changed Paths (Antigravity-Owned Only)

Zero changes to `Outpost-Expanded.unity`, terrain, prefabs, world-generation tools, `.gitignore`, `.meta` files, or Codex audit documents.

### Runtime & Editor C# Files Modified
- `Assets/RPG/Scripts/FrontendMenu.cs` — Added `enteringWorld` debounce, disabled world-entry action while `enteringWorld || OnlineSessionService.IsBusy`, enforced normalized Join Code length `>= 4` before starting `JoinOnline`, updated `DrawControlsPage()` with actual co-op keybinds (`[E]`, `[R]`, `[F6]`, `[F7]`), and added `-coopUiSmoke` menu-to-world-to-menu flow (`RunUiFlowSmokeRoutine`, skipping roster writes during smoke).
- `Assets/RPG/Scripts/PlayerController.cs` — Added `abilityId` parameter to `Hit(...)` (`"frostnova"`, `"eviscerate"`, `"consecration"`, `"whirlwind"`, `"melee"`); routed `CastAbility3()` self-heals (`"arcane_ward"`, `"holy_light"`, `"second_wind"`) through `RequestAbilityHeal(...)`.
- `Assets/RPG/Scripts/PlayerExploration.cs` — Routed multiplayer `DiscoverLandmark` through `RiftboundCoopPlayer.RequestDiscoverLandmark(...)` and added `ApplyAuthorizedDiscovery(...)` while keeping Offline mode intact.
- `Assets/RPG/Scripts/EnemyController.cs` — Updated co-op remote LOS targeting with `ClearSightForTarget(Transform)` while keeping 0-parameter `ClearSight()` intact for existing callers/reflection tests.
- `Assets/RPG/Scripts/LivingWorldEvents.cs` — Added multiplayer-aware proximity `DistanceToAnyActivePlayer`, Host-replicated state snapshots (`SendEventSnapshotToClient`, `ApplyClientEventState`, `BroadcastWorldEventState`), and synchronized Caravan & Roadside Ambush active/cooldown visual proxy states on clients.
- `Assets/RPG/Scripts/Network/RiftboundCoopPlayer.cs` — Added server-side class ownership checks and per-target AOE cooldown tracking (`ability:enemyKey`), `"eviscerate"`, server-validated `RequestAbilityHealServerRpc` (`8s` cooldown, `<= 65` clamp), `2.0s` ping rate-limiting, `SetAuthoritativeGold` fix in `ReceiveWorldEventRewardClientRpc`, late-join Ready Check & Event snapshots, server-authoritative potion count/cooldown (`serverPotions` start `3`, `buy_potion` cap `< 10`, `1.5s` cooldown), duplicate `"temper_weapon"` rejection, `1.5s` respawn cooldown, server-validated Greenhollow landmarks (`RequestDiscoverLandmarkServerRpc`, `<= 14m` range & dedup), Zebra unlock enforcement in `RequestSetMountServerRpc`, and `SendSpoofedPositionForTest`.
- `Assets/RPG/Scripts/Network/OnlineSessionService.cs` — Deduplicated in-flight `pendingInitTask` and `pendingSignInTask` in `EnsureServicesAndAuthAsync` so retrying immediately after cancelling sign-in/initialization never throws `"The player is already signing in."`, and guarded `if (token == attemptToken) busy = false;` in both async `finally` blocks.
- `Assets/RPG/Scripts/Network/CoopWorldManager.cs` — Moved Ping/Ready Check bindings off `[P]`/`[Y]` to `[F6]`/`[F7]` and gated `[F6]`/`[F7]`/`[E]` against `uiBlocking` & dead state; moved Ready Check timeout independent of `Keyboard.current`; added `Ping [F6]` / `Ready [F7]` HUD buttons; widened Dev LAN HUD height (`124f`); added `hostDisconnected` state & `"SESSION ENDED"` modal; switched smoke remote lookup to `GetFirstRemotePlayer()`; expanded `-coopResilienceSmoke` with mid-flight async cancel verification and `-coopLateJoinSmoke` with active Caravan Event + Ready Check snapshot & decline verification; added `-coopUiSmoke`, `-coopAuthoritySmoke`, `-coopFullAcceptanceSmoke`, `FindDeterministicTestChest(minGold, pickHighest)`, and 4-player telemetry.
- `Assets/RPG/Scripts/Network/CoopSessionConfig.cs` — Added `-coopFullAcceptanceSmoke`, `-coopUiSmoke`, and `-coopAuthoritySmoke` CLI flags.
- `Assets/RPG/Editor/CoopPrefabSetup.cs` — Added `BuildOvernightAntigravityOnline()` outputting cleanly to `Builds/OvernightAntigravityOnline/Riftbound.exe`.

### Documentation Added / Updated on Branch
- `Docs/Overnight/OWN_ANTIGRAVITY_RUN.md`
- `Docs/Overnight/OWN_ANTIGRAVITY_HANDOFF.md`
- `Docs/ONLINE_MULTIPLAYER_STATUS.md`

---

## 2. Final Verification Outcomes (Post-Final-Edit on `Builds/OvernightAntigravityOnline/Riftbound.exe` & Source)

| Check | Status | Observed Result |
| --- | --- | --- |
| Standalone Build (`BuildOvernightAntigravityOnline`) | **PASS** | `BUILD_EXIT=0` — `542,407,408 bytes`, `warnings=860`, `errors=0` |
| Offline Regression (`QuestRouteSmokeTest.Run`, Play Mode) | **PASS** | `QUEST_ROUTE_EXIT=0` — `145 PASS, 0 FAIL`, `UGS_INIT_IN_OFFLINE_LOG=0` |
| UI Flow Menu -> Host -> Leave Smoke (`-coopUiSmoke`) | **PASS** | `EXIT=0` — `PASS UiFlow: menuOpen=True, state=Offline, mode=Offline, nm=0, cams=2, listeners=1` |
| Invalid Join Code (`-coopInvalidCodeSmoke`) | **PASS** | `EXIT=0` — `state=Failed, err=Join code is invalid or expired.` |
| Async Cancel + Repeated Entry (`-coopResilienceSmoke`) | **PASS** | `EXIT=0` — `cancelOk=True, Host -> Leave -> Host`, `nmDuringHost=1, nmAfterLeave=0, finalState=Offline` |
| Server Authority & Policy (`-coopAuthoritySmoke`, 2p) | **PASS** | `H=0 C=0` — `hostAuth=PASS(chest=True,zebra=True,farRevRej=True,nearRev=True)`, `clientAuth=PASS(clamp=True,rngRej=True,cdRej=True,restRej=True,dupLmRej=True,zEarly=True,zOk=True,temper=True,deadRej=True,rev=True,restOk=True[g=75->70,h=60->120/120])` |
| Full Acceptance Relay Smoke (`-coopFullAcceptanceSmoke`, 2p) | **PASS** | `H=0 C=0` — Host `cid=1, ping=True, rc=True/True, revive=True, kills=0, qIdx=1`; Client `spoofRej=True, abHit=True, abHeal=True, dismount=True, qAcc=True, qComp=True, rev=True, evt=True, noDblG=True, hostDisc=True` |
| Client Leave & Host Recovery (`-coopClientLeaveSmoke`, 2p) | **PASS** | `H=0 C=0` — Host `hostDisconnectRecovery=PASS(mounted=True,quest=True,playersAfter=1)`; Client `nmAfterLeave=0, finalState=Offline` |
| Progression & Server Economy (`-coopProgressionSmoke`, 2p) | **PASS** | `H=0 C=0` — `hostProg=PASS(dead=True, chest=True)`, `clientProg=PASS(kill=True,qRej=True,c1=True,cDup=True,txRej=True,txBuy=True,resp=True)` |
| Late Join Active Caravan + Ready Check (`-coopLateJoinSmoke`, 2p) | **PASS** | `H=0 C=0` — Host `lateJoin=PASS(40/70,evt=True,rcDecline=True)`; Client `lateJoin=PASS(40/70,evt=True,rcSnap=True)` |
| Basic Hit & HP Loss (`-coopCombatSmoke`, 2p) | **PASS** | `H=0 C=0` — `enemy=road-raider-1 hp=36/70, clientDamaged=True` |
| 4-Player Relay + 5th Rejection + Frame Telemetry | **PASS** | `H=0 C1=0 C2=0 C3=0 C4=1` — 4 players reached `players=4, cams=1, listeners=1`; Host `avgMs=6.23, p95Ms=11.13`, Clients `avgMs=5.84–7.45, p95Ms=7.53–13.00`, `enemies=52–54, netObjs=4`; C4 rejected (`Join failed: session is full`) |
| 2-PC Cross-Network Internet Acceptance | **NOT RUN** | Requires 2+ separate PCs on distinct networks; same-machine Relay is NOT cross-network acceptance |
| Manual Interactive GUI Playthrough | **NOT RUN** | All post-edit verification executed via headless batchmode Play Mode & standalone smokes |

---

## 3. Known Limitations & Morning Integration Notes

1. **Clean-Clone Environment Dependency (`.gitignore`):** `.gitignore` contains `**/[Ll]ibrary/`, which excludes `Assets/Adobe/Substance3DForUnity/Connector/Library/` from Git. Fresh clones require copying that Adobe Connector `Library/` folder (or narrowing `.gitignore` explicitly with approval) before opening Unity.
2. **Online Session-Only Persistence:** Online co-op progress (gold, XP, quest index, chests, defeated state) intentionally remains session-only; offline saves are preserved untouched.
3. **Host Exit Policy:** No host migration is implemented; if the Host leaves, clients display the `"SESSION ENDED"` notice (`"Back to Menu"`) and must start or join a new session.
4. **Combined Merge Check Needed:** Codex (`Outpost-Expanded.unity` & Reaver terrain repairs) and Antigravity (`overnight/antigravity-online-20261116`) developed independently from `1b32349`. Per Section 9 of `Docs/OVERNIGHT_8_HOUR_PLAN.md`, review and integrate both branches in a third clean checkout, re-run `QuestRouteSmokeTest.Run` and online smokes, and build one combined executable before merging to `main`.

---

## 4. Exact User Host / Join Test Steps (`Builds/OvernightAntigravityOnline/`)

Copy the complete `Builds/OvernightAntigravityOnline/` folder (preserving `Riftbound_Data` and Unity plugin DLLs) to PC 1 and PC 2–4:

1. **Host (PC 1):**
   - Launch `Riftbound.exe`.
   - Choose race/class, select **Host Online**, and click **CREATE ONLINE SESSION**.
   - Share the 6-character **Join Code** displayed in the top-right Co-op HUD (**Copy Code** copies to clipboard).
2. **Clients (PC 2–4):**
   - Launch `Riftbound.exe`, pick race/class, select **Join Online**, enter the 6-character code, and click **JOIN ONLINE**.
3. **What to Verify Interactively:**
   - All up to 4 players spawn and appear in the party list; a 5th client receives `"session is full"`.
   - Combat damage, abilities, healing, chest loot (unique per player, no duplicates), quest completion, shop transactions, mounting/dismounting, Living World Events (Supply Caravan & Roadside Ambush), `[F6]` Ping, `[F7]` Ready Check, `[E]` Ally Revive, `[R]` Respawn, and **Leave Session** / Host disconnect `"SESSION ENDED"` modal all behave cleanly.
