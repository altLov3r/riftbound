# Riftbound — Online Multiplayer Status

**Last Updated:** 2026-11-16 (`overnight/antigravity-online-20261116`, Run ID `AGY-OVERNIGHT-20261116`)  
**Shared Scene Ownership:** **SCENE RELEASED** (Zero scene, terrain, prefab, or world-gen edits on this branch)  
**Unity Version:** `6000.6.2f1`  
**Networking Stack:** Unity Netcode for GameObjects (`com.unity.netcode.gameobjects@2.14.0`) + Unity Multiplayer Services SDK (`com.unity.services.multiplayer@2.4.1`) + Unity Relay  
**Fresh Branch Build:** `Builds/OvernightAntigravityOnline/Riftbound.exe` (`BUILD_EXIT:0`, `542,395,728 bytes`, `warnings=845`, `errors=0`)

---

## 1. Feature & Verification Matrix

Status definitions per `Docs/RIFTBOUND_MASTER_PLAN.md`: **IMPLEMENTED** | **VERIFIED** | **BLOCKED** | **NOT RUN**.

| Capability | Status | Evidence / Observed Result |
| --- | --- | --- |
| UGS Multiplayer Services + Relay Bootstrap & Offline Isolation | **VERIFIED** | `QuestRouteSmokeTest.Run` recorded `UGS_INIT_IN_OFFLINE_LOG=0`; Relay Host/Join passes up to 4 players. |
| Session Entry UI (`FrontendMenu`) & Debounce / Code Validation | **VERIFIED** (in code + headless flow) | Added `enteringWorld` debounce, `IsBusy` button disabling, and `NormalizeJoinCode.Length >= 4` check. Manual mouse/keyboard click-through is `NOT RUN`. |
| Invalid Join Code Error Handling | **VERIFIED** | `-coopInvalidCodeSmoke` (`EXIT=0`): `state=Failed, err=Join code is invalid or expired.` |
| Class-Validated Abilities, AOE Cooldowns & Self-Heal RPC (`RequestAbilityHeal`) | **VERIFIED** | `-coopFullAcceptanceSmoke` (`H=0 C=0`): `spoofRej=True` (cross-class ability blocked), `abHit=True`, `abHeal=True`. |
| Kill Credit, Quest Accept/Complete, Duplicate Claim Rejection, Mount/Dismount | **VERIFIED** | `-coopFullAcceptanceSmoke` (`H=0 C=0`): `qAcc=True`, 3 kills -> `qComp=True` (duplicate blocked), `dismount=True`. |
| Independent Chest Loot, Duplicate Chest Rejection, Shop Transactions & Respawn | **VERIFIED** | `-coopProgressionSmoke` (`H=0 C=0`): `hostProg=PASS(dead=True, chest=True)`, `clientProg=PASS(kill=True,qRej=True,c1=True,cDup=True,txRej=True,txBuy=True,resp=True)`. |
| Party Extras (`[F6]` Ping rate-limit, `[F7]` Ready Check, Revive & HUD modal) | **VERIFIED** (RPC flow; live input `NOT RUN`) | Moved off `[P]`/`[Y]` collisions, gated when dead/`uiBlocking`; `-coopFullAcceptanceSmoke` verified `ping=True, rc=True/True, revive=True, rev=True`. |
| Living World Events (`Caravan` + `Ambush` sync, zero double gold) | **VERIFIED** | `-coopFullAcceptanceSmoke` (`H=0 C=0`): `evt=True, noDblG=True` (fixed double-gold client bug in `ReceiveWorldEventRewardClientRpc`). |
| Late Joining Mid-Combat + Event/ReadyCheck Snapshot Sync | **VERIFIED** | `-coopLateJoinSmoke` (`H=0 C=0`): `lateJoin=PASS(40/70)`; Host snapshot sends active Ready Check and Event states. |
| Repeated Session Entry, Client Leave & Host Disconnect Notice | **VERIFIED** | `-coopResilienceSmoke` (`EXIT=0`, `nmAfterLeave=0, finalState=Offline`), `-coopClientLeaveSmoke` (`H=0 C=0`), `-coopFullAcceptanceSmoke` `hostDisc=True`. |
| 4-Player Relay Session + 5th-Player Rejection + Frame Telemetry | **VERIFIED** | `H=0 C1=0 C2=0 C3=0 C4=1`: `players=4`, Host `avgMs=5.31/p95Ms=7.68`, Clients `avgMs=4.08-4.40/p95Ms=4.94-5.58`, C4 `Join failed: session is full`. |
| Offline Regression (`QuestRouteSmokeTest.Run`) | **VERIFIED** | `QUEST_ROUTE_EXIT:0`: `145 PASS, 0 FAIL` (`TOTAL=150`). |
| Manual Interactive GUI Playthrough | **NOT RUN** | Requires interactive human testing in `Builds/OvernightAntigravityOnline/Riftbound.exe`. |
| Cross-Network 2-PC Internet Acceptance | **NOT RUN** | Same-machine Relay does not substitute for two distinct PCs/networks. |

---

## 2. Modified Files (Branch `overnight/antigravity-online-20261116`)

- `Assets/RPG/Editor/CoopPrefabSetup.cs`
- `Assets/RPG/Scripts/EnemyController.cs`
- `Assets/RPG/Scripts/FrontendMenu.cs`
- `Assets/RPG/Scripts/LivingWorldEvents.cs`
- `Assets/RPG/Scripts/Network/CoopSessionConfig.cs`
- `Assets/RPG/Scripts/Network/CoopWorldManager.cs`
- `Assets/RPG/Scripts/Network/RiftboundCoopPlayer.cs`
- `Assets/RPG/Scripts/PlayerController.cs`
- `Docs/Overnight/OWN_ANTIGRAVITY_RUN.md`
- `Docs/Overnight/OWN_ANTIGRAVITY_HANDOFF.md`
- `Docs/ONLINE_MULTIPLAYER_STATUS.md`

See [Docs/Overnight/OWN_ANTIGRAVITY_HANDOFF.md](Docs/Overnight/OWN_ANTIGRAVITY_HANDOFF.md) for cross-network test steps and morning integration instructions.
