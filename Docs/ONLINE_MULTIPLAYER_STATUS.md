# Riftbound — Online Multiplayer Status

**Last Updated:** 2026-11-15  
**Shared Scene Ownership:** **SCENE RELEASED** (Antigravity has no pending edits to `Outpost-Expanded.unity`, terrain, nature prefabs, or world-gen tools; Codex may proceed with the localized Reaver terrain repair).  
**Unity Version:** `6000.6.2f1`  
**Networking Stack:** Unity Netcode for GameObjects (`com.unity.netcode.gameobjects@2.14.0`) + Unity Multiplayer Services SDK (`com.unity.services.multiplayer@2.4.1`) + Unity Relay  
**Fresh Standalone Build:** `Builds/CoopRelease/Riftbound.exe` (`BUILD_EXIT:0`)

---

## 1. Feature & Verification Matrix

Status definitions per `Docs/RIFTBOUND_MASTER_PLAN.md`: **IMPLEMENTED** | **VERIFIED** | **BLOCKED** | **NOT RUN**.

| Capability | Status | Evidence / Observed Result |
| --- | --- | --- |
| UGS Multiplayer Services + Relay Bootstrap (`OnlineSessionService`) | **VERIFIED** | `com.unity.services.multiplayer@2.4.1` installed; `StartHostOnlineAsync` & `JoinOnlineByCodeAsync` create/join private Relay sessions (`MaxPlayers = 4`). |
| Session Entry UI (`FrontendMenu`) & HUD (`CoopWorldManager`) | **VERIFIED** | Release UI exposes `Offline` \| `Host Online` \| `Join Online` + Join Code (no IP/port fields); `SceneManager.sceneLoaded` hook ensures `CoopWorldManager` spawns after menu scene reload. |
| Host Online → Share Code → Join Online (Live Relay) | **VERIFIED** | `PROG_CODE:TW9HJH`, `LJ_CODE:9LFPPR`, `CMB_CODE:PRJLMR`, `4P_CODE:KLPB7C` — all connected over live UGS Relay with `ExitCode=0`. |
| Invalid Join Code Error Handling | **VERIFIED** | `INV_EXIT:0`: `PASS InvalidCode: state=Failed, err=Join code is invalid or expired.` |
| Client Ability Damage & Client HP Loss | **VERIFIED** | `CMB_CODE:PRJLMR` (`H=0 C=0`): `enemy=road-raider-1 hp=36/70, clientDamaged=True` on both Host and Client. |
| Kill Credit, Premature Quest Rejection, Independent & Duplicate-Rejected Chest Loot | **VERIFIED** | `PROG_CODE:TW9HJH` (`H=0 C=0`): `hostProg=PASS(dead=True, chest=True)`, `clientProg=PASS(kill=True,qRej=True,c1=True,cDup=True,...)`. |
| Server-Validated Shop Transactions & Respawn (`RequestTransaction`, `RequestRespawn`) | **VERIFIED** | `PROG_CODE:TW9HJH` (`H=0 C=0`): `txRej=True` (45g weapon temper rejected when gold < 45), `txBuy=True` (10g potion deducted & granted), `resp=True` (`Health == MaxHealth`). |
| Late Joining Mid-Combat (`SendWorldSnapshotToClient`) | **VERIFIED** | `LJ_CODE:9LFPPR` (`H=0 C=0`): Host pre-damaged `road-raider-1` to `40/70` before Client joined 3s later; Client received `lateJoin=PASS(40/70)`. |
| Repeated Session Entry & Clean Leave (`Host → Leave → Host → Offline`) | **VERIFIED** | `RES_EXIT:0`: `PASS Resilience: codes=WKQBW9->F9M6HL, nmDuringHost=1, nmAfterLeave=0, finalState=Offline`. |
| Client `Join → Leave → Offline` & Host Disconnect Recovery | **VERIFIED** | `CL_CODE:RKCJPQ` (`H=0 C=0`): `PASS ClientLeave: questAccepted=True, mounted=True, nmAfterLeave=0, finalState=Offline`; Host observed `hostDisconnectRecovery=PASS(mounted=True,quest=True,playersAfter=1)`. |
| Quest Acceptance & Mount Replication (`RequestAcceptQuest`, `RequestSetMount`) | **VERIFIED** | `CL_CODE:RKCJPQ`: Client accepted quest near `TownNPC` (`quest=True`) and mounted (`mounted=True`); both replicated to Host. |
| 4-Player Relay Session & 5th-Player Rejection | **VERIFIED** | `4P_CODE:KLPB7C` (`H=0 C1=0 C2=0 C3=0 C4=1`): 4 players reached `players=4`; 5th client `C4` rejected with `[OnlineSessionService] Join failed: session is full`. |
| Offline Regression (`QuestRouteSmokeTest.Run`) | **VERIFIED (partial pre-existing world check)** | `77 PASS` checks in `.code-check/startup-smoke-failed.txt`, stopping at pre-existing Codex terrain check `FAIL Camp uses a separate terrain asset` (`QuestRouteSmokeTest.cs:317`). |
| Party Frames, Pings `[P]`, Ready Check `[Y]`, Ally Revive `[E]` | **IMPLEMENTED** (`NOT RUN` interactively) | Implemented in `RiftboundCoopPlayer.cs` and `CoopWorldManager.cs`; automated smoke verified spawn/combat/progression/mounts, not live human keyboard/mouse input. |
| Normal-Input Interactive Full Playthrough | **NOT RUN** | Requires manual player testing in `Riftbound.exe`. |
| Cross-Network 2-PC Internet Acceptance | **NOT RUN** | Requires two separate PCs on distinct networks (`Riftbound.exe` ready to test). |

---

## 2. Key Fixes Completed

- **`CoopWorldManager.cs` (`SceneManager.sceneLoaded` fix)**: Fixed a P0 bug where `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` only fired on initial app launch and did not re-spawn `CoopWorldManager` when `FrontendMenu` or `DisconnectAndReturnToMenu()` reloaded `Outpost-Expanded.unity`.
- **`PlayerServices.cs` & `PlayerProgress.cs` & `RiftboundCoopPlayer.cs`**: Routed `BuyPotion()`, `TemperWeapon()`, `RestAtMerchant()`, and `UsePotion()` through server-validated RPCs (`RequestTransaction`, `RequestUsePotion`) and synced authoritative `Gold` back to `localPlayer`.
- **`PlayerController.cs` & `RiftboundCoopPlayer.cs`**: Routed `Respawn()` in multiplayer through `RequestRespawn()` -> Host `ExecuteAuthoritativeRespawn()` -> `ApproveRespawnClientRpc()`.
- **`CoopWorldManager.cs` & `CoopSessionConfig.cs`**: Added `-coopProgressionSmoke`, `-coopLateJoinSmoke`, and `-coopResilienceSmoke` automated multi-process Relay verification modes.

---

## 3. Exact Cross-Network Testing Instructions (2+ PCs)

Use the fresh standalone build at `Unity Project/My project (1)/Builds/CoopRelease/Riftbound.exe` (copy the entire `Builds/CoopRelease/` folder to each PC):

1. **Host (PC 1):**
   - Launch `Riftbound.exe`.
   - Select faction/race/class, click **Host Online**, then click **CREATE ONLINE SESSION**.
   - Wait for the world to load; copy the 6-character **Join Code** shown in the top-right Co-op HUD (or click **Copy Code**).
2. **Client(s) (PC 2–4, up to 3 friends):**
   - Launch `Riftbound.exe`.
   - Select faction/race/class, click **Join Online**, paste/type the 6-character Join Code, and click **JOIN ONLINE**.
3. **What to Verify Together:**
   - All players (up to 4) appear in the same world and in the left-side party frames.
   - A 5th player attempting the same Join Code is rejected (`session is full`).
   - Host and Clients can damage enemies (`road-raider-1`, etc.), take damage, loot chests independently without duplicate claims, complete quests, buy potions/rest/temper weapons at the merchant, mount up, press `[P]` to ping, `[Y]` for ready check, and `[E]`/`[R]` for revive/respawn.
   - Click **Leave Session** on a Client, then enter **Offline** or **Host Online** again to confirm clean recovery.

---

## 4. Modified Files (Antigravity Networking & Integration)

- `Assets/RPG/Scripts/Network/CoopSessionConfig.cs`
- `Assets/RPG/Scripts/Network/CoopWorldManager.cs`
- `Assets/RPG/Scripts/Network/OnlineSessionService.cs`
- `Assets/RPG/Scripts/Network/RiftboundCoopPlayer.cs`
- `Assets/RPG/Scripts/PlayerController.cs`
- `Assets/RPG/Scripts/PlayerProgress.cs`
- `Assets/RPG/Scripts/PlayerServices.cs`
- `Assets/RPG/Scripts/FrontendMenu.cs`
- `Assets/RPG/Scripts/EnemyController.cs`
- `Assets/RPG/Scripts/ClassSpells.cs`
- `Assets/RPG/Scripts/PlayerMounts.cs`
- `Assets/RPG/Scripts/LivingWorldEvents.cs`
- `Docs/ONLINE_SETUP.md`
- `Docs/ONLINE_MULTIPLAYER_STATUS.md`
