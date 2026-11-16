# Antigravity Combat & NPC Runtime Refinement Run Journal

- **Assignment:** `AG-COMBAT-NPC-20261117`
- **Agent:** Antigravity
- **Checkout / Worktree:** `c:\Users\skama\Documents\wow=rpg-diplomna\Combat\antigravity-refinement`
- **Branch:** `combat/antigravity-refinement-20261117`
- **Integrated Base Commit:** `8ad9071820c2fbed32fbb728122ffb2f02980888` (`integration/combined-20261116`; combined runtime source `76696a7bff9e86f40a5c7fb90747fac5a290be8b`, preserving `Integration/combined-20261116`, `Overnight/*`, `main`, and prior builds untouched)
- **Actual Start UTC:** `2026-11-16T22:09:49Z` (`2026-11-17T00:09:49+02:00` Europe/Sofia)
- **Timing Note:** No fixed 8-hour shift timer implied; bounded checkpointed execution (`AG00 -> AG01 (CF00–CF04) -> AG02/AG03`).
- **Authorized Scope:** Close `Combined02` incomplete online gates (`AG00`/`CF00`), reproduce and fix demonstrated class/authority/immunity/enemy-LOS parity defects (`AG01`/`CF01–CF04`), implement `SC02`-aligned NPC runtime adapter (`AG02`/`SC04,SC05,SC07,SC09`) using Codex's approved five-NPC content contract, and tune one reference combat encounter (`AG03`/`CF05–CF14`). Zero edits to world scenes, terrain, environment/road/nature prefabs, source FBX/Avatars/Animator assets, or Codex's external content files.
- **Failed Attempts Counter:** `0` unchanged failed attempts.

## AG00 / CF00 — Combined02 Baseline Coverage Closed (PASS)
- **Candidate Executed Read-Only:** `C:\Users\skama\Documents\wow=rpg-diplomna\Integration\combined-20261116\Builds\RiftboundCombined02\Riftbound.exe` (`source 76696a7`, `EXE_SHA256=AF88B840...`, `ASM_SHA256=428B40BA910C28F534804962BE8E49F1F3AB543D7655DAF2263F156FC9909CD6`).
- **Private Evidence Folder (Git-ignored):** `.code-check/ag00/relay-20261117-001232/` (all join codes redacted cleanly).
- **Retained Prior `4/10` Receipts on `Combined02`:** `Ui` (`EXIT=0`), `InvalidCode` (`EXIT=0`), `Resilience` (`EXIT=0`), `Combat` (`H=0, C1=0`, `p0=(0.00, 0.05, -6.00), p1=(2.60, 0.05, -6.00)`).
- **Newly Executed Remaining `6/10` Scenarios on `Combined02`:**

| Scenario | Exits | Grounded / Outcome Receipt |
| --- | --- | --- |
| `Authority` | `H=0, C1=0` | `PASS` — `sep=2.60m (p0=(0.00, 0.05, -6.00), p1=(2.60, 0.05, -6.00))`; `hostAuth=PASS(chest=True,zebra=True,farRevRej=True,nearRev=True)`, `clientAuth=PASS(clamp,rngRej,cdRej,aliveRespRej,restRej,dupLmRej,zEarly,zOk,temper,deadRej,rev,restOk,potOk,potCdRej)` |
| `Progression` | `H=0, C1=0` | `PASS` — initial `sep=2.60m (p0=(0.00, 0.05, -6.00))`, post-combat/respawn grounded `p1=(2.39, 0.06, -0.51)`; `hostProg=PASS(dead=True, chest=True)`, `clientProg=PASS(kill,qRej,c1,cDup,txRej,txBuy,resp,bounty)` |
| `LateJoin` | `H=0, C1=0` | `PASS` — `sep=2.60m (p0=(0.00, 0.05, -6.00), p1=(2.60, 0.05, -6.00))`; `lateJoin=PASS(40/70,evt=True,rcDecline=True/rcSnap=True)` |
| `FullAcceptance` | `H=0, C1=0` | `PASS` — `sep=2.60m`; `hostAccept=PASS(cid=1,ping=True,rc=True/True,revive=True,kills=0,qIdx=1)`, `clientAccept=PASS(spoofRej,abHit,abHeal,dismount,qAcc,qComp,rev,evt,noDblG,hostDisc)` |
| `ClientLeave` | `H=0, C1=0` | `PASS` — `sep=2.60m (p0=(0.00, 0.05, -6.00), p1=(2.60, 0.05, -6.00))`; Host `hostDisconnectRecovery=PASS(mounted,quest,playersAfter=1,rejoined=True,finalPlayers=1)`, Client `ClientLeave(questAccepted,mounted,rejoined,nmAfterLeave=0,finalState=Offline)` |
| `Capacity` | `H=0, C2=0, C3=0, C4=0, C1=1` | `PASS` — 4 admitted players (`sep=2.60m (p0=(0.00, 0.05, -6.00), p1=(2.60, 0.05, -6.00))`, Host `avgMs=6.67, p95Ms=10.56, enemies=54, netObjs=4`; Clients `avgMs=4.57–4.78, p95Ms=6.28–7.32`), 5th client (`C1`) cleanly rejected (`[OnlineSessionService] Join failed: session is full`) |

- **Independent Spawn Verification (`Riftbound.Editor.CombinedSpawnVerification.Run` in worktree):** `SPAWN_EXIT=0`, `TOTAL PASS=32 FAIL=0` (saved in `.code-check/integration/spawn-height-result.txt`).
- **Remaining Baseline Boundary:** Headless same-machine Relay only; manual windowed playthrough, GPU profiling, and real 2-PC cross-network play remain `NOT RUN`.

## AG01 / CF01–CF04 — Source-Risk Reproductions & Fixes
- Starting `CF01` PlayMode reproduction pass before runtime modifications.
