# Combined standalone online verification

**Historical candidate01 — SUPERSEDED FOR SHIPPING.** The parent reproduced its multi-terrain spawn-height defect after these automated checks. Preserve these results as evidence for this exact candidate; do not ship it or transfer the passes to Combined02.

Headless automated Sessions/Relay checks on the exact combined executable. Not manual GUI, rendered performance or two-PC cross-network acceptance. Raw logs remain local; join codes/authentication details are not published.

Run: 20261116-175339

Candidate identity: source4af9e0e53b0316659936c05dccdbb0ecd6d26fbf, Builds/RiftboundCombined01/Riftbound.exe built17:52:34 Europe/Sofia. Managed Assembly-CSharp SHA25668E5AD9AF7A9954DF9A5C9CCBA5D276557C2597FCEA274A91A8561CC90E44871; level0 SHA2566DD2D5495E3C1E4BDEF783A8042B42DF5B8DDEE7304009AD2A9BB63220086B9D.

**Result:10 scenarios passed/0 failed.** One invocation, no fixture retries or runtime/assertion edits. Twenty named process logs checked for NullReferenceException, MissingReferenceException, IndexOutOfRangeException and MissingMethodException:0 matches. Expected invalid-code/full-session service errors are not a clean-server-log claim.

| Scenario | Result | Exit codes | Sanitized assertions |
| --- | --- | --- | --- |
| Ui | PASS | Ui=0 | PASS UiFlow: menuOpen=True, state=Offline, mode=Offline, nm=0, cams=2, listeners=1 |
| InvalidCode | PASS | InvalidCode=0 | PASS InvalidCode: state=Failed, err=Join code is invalid or expired. |
| Resilience | PASS | Resilience=0 | PASS Resilience: cancelOk=True, code=[redacted], nmDuringHost=1, nmAfterLeave=0, finalState=Offline |
| Combat | PASS | Combat-H=0, Combat-C1=0 | PASS CoopHost: players=2, localOwner=True, cams=1, listeners=1, sep=9.76m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), enemy=road-raider-1 hp=36/70, clientDamaged=True; PASS CoopClient: players=2, localOwner=True, cams=1, listeners=1, sep=9.52m (p0=(0.00, 0.05, -6.00), p1=(2.52, 9.00, -3.95)), enemy=road-raider-1 hp=36/70, clientDamaged=True |
| Authority | PASS | Authority-H=0, Authority-C1=0 | PASS CoopHost: players=2, localOwner=True, cams=1, listeners=1, sep=9.76m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), hostAuth=PASS(chest=True,zebra=True,farRevRej=True,nearRev=True); PASS CoopClient: players=2, localOwner=True, cams=1, listeners=1, sep=9.75m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), clientAuth=PASS(clamp=True,rngRej=True,cdRej=True,aliveRespRej=True,restRej=True,dupLmRej=True,zEarly=True,zOk=True,temper=True,deadRej=True,rev=True,restOk=True[g=75->70,h=60->100/120],potOk=True,potCdRej=True) |
| Progression | PASS | Progression-H=0, Progression-C1=0 | PASS CoopHost: players=2, localOwner=True, cams=1, listeners=1, sep=9.76m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), hostProg=PASS(dead=True, chest=True); PASS CoopClient: players=2, localOwner=True, cams=1, listeners=1, sep=9.75m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), clientProg=PASS(kill=True,qRej=True,c1=True,cDup=True,txRej=True,txBuy=True,resp=True,bounty=True) |
| LateJoin | PASS | LateJoin-H=0, LateJoin-C1=0 | PASS CoopHost: players=2, localOwner=True, cams=1, listeners=1, sep=9.76m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), lateJoin=PASS(40/70,evt=True,rcDecline=True); PASS CoopClient: players=2, localOwner=True, cams=1, listeners=1, sep=9.75m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), lateJoin=PASS(40/70,evt=True,rcSnap=True) |
| FullAcceptance | PASS | FullAcceptance-H=0, FullAcceptance-C1=0 | PASS CoopHost: players=2, localOwner=True, cams=1, listeners=1, sep=9.76m, hostAccept=PASS(cid=1,ping=True,rc=True/True,revive=True,kills=0,qIdx=1); PASS CoopClient: clientAccept=PASS(spoofRej=True,abHit=True,abHeal=True,dismount=True,qAcc=True,qComp=True,rev=True,evt=True,noDblG=True,hostDisc=True) |
| ClientLeave | PASS | ClientLeave-H=0, ClientLeave-C1=0 | PASS CoopHost: players=2, localOwner=True, cams=1, listeners=1, sep=9.76m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), hostDisconnectRecovery=PASS(mounted=True,quest=True,playersAfter=1,rejoined=True,finalPlayers=1); PASS ClientLeave: questAccepted=True, mounted=True, rejoined=True, nmAfterLeave=0, finalState=Offline |
| Capacity | PASS | Capacity-H=0, Capacity-C1=0, Capacity-C2=0, Capacity-C3=1, Capacity-C4=0 | PASS CoopHost: players=4, localOwner=True, cams=1, listeners=1, sep=9.76m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), telemetry(avgMs=4.92, p95Ms=5.89, enemies=52, netObjs=4); PASS CoopClient: players=4, localOwner=True, cams=1, listeners=1, sep=9.73m (p0=(0.00, 0.05, -6.00), p1=(2.59, 9.43, -5.98)), telemetry(avgMs=4.34, p95Ms=4.97, enemies=52, netObjs=4); PASS CoopClient: players=4, localOwner=True, cams=1, listeners=1, sep=9.76m (p0=(0.00, 0.05, -6.00), p1=(2.60, 9.45, -6.00)), telemetry(avgMs=4.30, p95Ms=6.20, enemies=52, netObjs=4); FAIL CoopClient: players=0, localOwner=False, cams=1, listeners=1, sep=0.00m (p<2); PASS CoopClient: players=4, localOwner=True, cams=1, listeners=1, sep=9.73m (p0=(0.00, 0.05, -6.00), p1=(2.59, 9.43, -5.98)), telemetry(avgMs=4.60, p95Ms=7.27, enemies=52, netObjs=4); successfulParticipants=4; extraRejectedForFullSession=1 (client admission order may vary) |

All requested automated scenarios above executed. Expected session-full rejection is checked separately from successful participants. Capacity's extra client was C3 in this run: admission order varies, but exactly4 participants succeeded and1 was rejected for a full session. No runtime assertion is modified by this runner.

Recorded PIDs: UI12972; invalid21496; resilience10720; combat924/15220; authority2480/19388; progression10480/11984; late join8660/10332; full acceptance17640/6616; client leave18608/22164; capacity23480/19868/11936/3488/3720. All owned smoke processes exited. Native parent-owned preview was untouched.

**Open integration finding:** early Combat telemetry reports hostY0.05/clientY9.45. posInfo is captured before the later smoke combatSpot teleport. RiftboundCoopPlayer.ComputeSpawnPosition currently samples Terrain.activeTerrain rather than selecting the containing tile, which can produce a wrong height in the four-tile scene. This is a source-derived hypothesis pending parent reproduction/fix; these passing network assertions do not establish grounded-spawn acceptance. No runtime modification or fix claim by this verifier.

**NOT RUN:** manual GUI/gameplay, renderedGPU profiling, actual separate-PC cross-network play, and any later rebuilt candidate. If the parent fixes spawn/rebuilds, these identities/results remain historical evidence for this candidate and must not be relabelled as tests of the new executable.

Reporting correction: the first in-progress runner redacted singular code fields but missed the Resilience plural rotation field. The runner now redacts both; this final document and private extracted results.json were sanitized and verified before handoff. Raw private logs intentionally retain diagnostic details and must not be committed.

Private local evidence folder: C:\Users\skama\Documents\wow=rpg-diplomna\Integration\combined-20261116\.code-check\integration\relay-20261116-175339
