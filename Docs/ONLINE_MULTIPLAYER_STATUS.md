# Riftbound — Online Multiplayer Status

**Last Updated:** 2026-11-16 (`overnight/antigravity-online-20261116`, Run ID `AGY-OVERNIGHT-20261116`)  
**Shared Scene Ownership:** **SCENE RELEASED** (Zero scene, terrain, prefab, or world-gen edits on this branch)  
**Unity Version:** `6000.6.2f1`  
**Networking Stack:** Unity Netcode for GameObjects (`com.unity.netcode.gameobjects@2.14.0`) + Unity Multiplayer Services SDK (`com.unity.services.multiplayer@2.4.1`) + Unity Relay  
**Fresh Branch Build:** `Builds/OvernightAntigravityOnline/Riftbound.exe` (`BUILD_EXIT:0`, `228 files`, `542,430,960 bytes` / `517.30 MB`, `Assembly-CSharp.dll` `863,232 bytes`, `errors=0`)

---

## 1. Feature & Verification Matrix

Status definitions per `Docs/RIFTBOUND_MASTER_PLAN.md`: **IMPLEMENTED** | **VERIFIED** | **BLOCKED** | **NOT RUN**.

| Capability | Status | Evidence / Observed Result |
| --- | --- | --- |
| UGS Multiplayer Services + Relay Bootstrap & Offline Isolation | **VERIFIED** | `QuestRouteSmokeTest.Run` recorded `UGS_INIT_IN_OFFLINE_LOG=0`; `FactionQuestSystem.SaveData/LoadData` and `PlayerProgress.SaveAdventure` both skip disk I/O in co-op; Relay Host/Join passes up to 4 players. |
| Session Entry UI (`FrontendMenu`) & Debounce / Code Validation | **VERIFIED** (code + `-coopUiSmoke`) | Added `enteringWorld` debounce, `IsBusy` button disabling, `NormalizeJoinCode.Length >= 4`, updated Controls page, and `-coopUiSmoke` (`EXIT=0`, menu -> host -> leave cleanly). Manual interactive mouse/keyboard playthrough is `NOT RUN`. |
| Invalid Join Code Error Handling | **VERIFIED** | `-coopInvalidCodeSmoke` (`EXIT=0`): `state=Failed, err=Join code is invalid or expired.` |
| Class-Validated Abilities, AOE Cooldowns & Self-Heal RPC (`RequestAbilityHeal`) | **VERIFIED** | `-coopFullAcceptanceSmoke` (`H=0 C=0`): `spoofRej=True`, `abHit=True`, `abHeal=True`; `-coopAuthoritySmoke`: `clamp=True, rngRej=True, cdRej=True, deadRej=True`. |
| Kill Credit, Main & Optional Bounty Quests, Mount & Zebra Unlock Policy | **VERIFIED** | `-coopFullAcceptanceSmoke` (`H=0 C=0`): `qAcc=True`, 3 kills -> `qComp=True`, `dismount=True`; `-coopProgressionSmoke` (`H=0 C=0`): `bounty=True` (`frontier_cartography` premature reject -> 3 surveys -> server completion -> duplicate reject); `-coopAuthoritySmoke`: `dupLmRej=True, zEarly=True, zOk=True`. |
| Independent Chest Loot, Duplicate Chest Rejection, Potion/Temper/Rest Economy & Respawn | **VERIFIED** | `-coopProgressionSmoke` (`H=0 C=0`): `hostProg=PASS(dead=True, chest=True)`, `clientProg=PASS(kill=True,qRej=True,c1=True,cDup=True,txRej=True,txBuy=True,resp=True,bounty=True)`; `-coopAuthoritySmoke`: `temper=True, restOk=True[g=75->70,h=60->120/120], restRej=True`. |
| Party Extras (`[F6]` Ping rate-limit, `[F7]` Ready Check, Revive & HUD modal) | **VERIFIED** (RPC + authority checks; live manual input `NOT RUN`) | Moved off `[P]`/`[Y]` collisions, gated when dead/`uiBlocking`; `-coopFullAcceptanceSmoke` verified `ping=True, rc=True/True, revive=True, rev=True`; `-coopAuthoritySmoke` verified `farRevRej=True, nearRev=True`. |
| Living World Events (`Caravan` + `Ambush` sync, zero double gold) | **VERIFIED** | `-coopFullAcceptanceSmoke` (`H=0 C=0`): `evt=True, noDblG=True` (fixed double-gold client bug in `ReceiveWorldEventRewardClientRpc`). |
| Late Joining Mid-Combat + Active Event/ReadyCheck Snapshot Sync & Decline | **VERIFIED** | `-coopLateJoinSmoke` (`H=0 C=0`): `lateJoin=PASS(40/70,evt=True,rcDecline=True)` on Host, `lateJoin=PASS(40/70,evt=True,rcSnap=True)` on Client. |
| Async Cancel Recovery, Repeated Entry, Client Leave & Host Disconnect Notice | **VERIFIED** | `-coopResilienceSmoke` (`EXIT=0`, `cancelOk=True, nmAfterLeave=0, finalState=Offline`), `-coopClientLeaveSmoke` (`H=0 C=0`), `-coopFullAcceptanceSmoke` `hostDisc=True`. |
| 4-Player Relay Session + 5th-Player Rejection + Frame Telemetry | **VERIFIED** | `H=0 C1=0 C2=0 C3=0 C5=1`: `players=4`, Host `avgMs=5.11/p95Ms=7.68`, Clients `avgMs=4.42-5.67/p95Ms=5.44-9.39`, `enemies=52-54, netObjs=4`, C5 `Join failed: session is full`. |
| Offline Regression (`QuestRouteSmokeTest.Run`) | **VERIFIED** | `UNITY_EXIT=0`: `RESULT_PASS=145, RESULT_FAIL=0, TOTAL=150`. |
| Manual Interactive GUI Playthrough | **NOT RUN** | Requires interactive human testing in `Builds/OvernightAntigravityOnline/Riftbound.exe`. |
| Cross-Network 2-PC Internet Acceptance | **NOT RUN** | Same-machine Relay does not substitute for two distinct PCs/networks. |

---

## 2. Modified Files (Branch `overnight/antigravity-online-20261116`)

- `Assets/RPG/Editor/CoopPrefabSetup.cs`
- `Assets/RPG/Scripts/EnemyController.cs`
- `Assets/RPG/Scripts/FactionQuestSystem.cs`
- `Assets/RPG/Scripts/FrontendMenu.cs`
- `Assets/RPG/Scripts/LivingWorldEvents.cs`
- `Assets/RPG/Scripts/Network/CoopSessionConfig.cs`
- `Assets/RPG/Scripts/Network/CoopWorldManager.cs`
- `Assets/RPG/Scripts/Network/OnlineSessionService.cs`
- `Assets/RPG/Scripts/Network/RiftboundCoopPlayer.cs`
- `Assets/RPG/Scripts/PlayerController.cs`
- `Assets/RPG/Scripts/PlayerExploration.cs`
- `Docs/Overnight/OWN_ANTIGRAVITY_RUN.md`
- `Docs/Overnight/OWN_ANTIGRAVITY_HANDOFF.md`
- `Docs/ONLINE_MULTIPLAYER_STATUS.md`

See [Docs/Overnight/OWN_ANTIGRAVITY_HANDOFF.md](Docs/Overnight/OWN_ANTIGRAVITY_HANDOFF.md) for cross-network test steps and morning integration instructions.
