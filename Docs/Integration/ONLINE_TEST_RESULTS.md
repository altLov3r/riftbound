# Combined02 standalone online verification — partial user-stop handoff

Headless automated Sessions/Relay checks on the exact combined executable. Not manual GUI, rendered performance or two-PC cross-network acceptance. Raw logs remain local; join codes/authentication details are not published.

Run: 20261116-180643

Build: Builds/RiftboundCombined02/Riftbound.exe
Source supplied by build owner: 76696a7bff9e86f40a5c7fb90747fac5a290be8b
Managed Assembly-CSharp SHA256: 428B40BA910C28F534804962BE8E49F1F3AB543D7655DAF2263F156FC9909CD6
Packaged level0 SHA256: 6DD2D5495E3C1E4BDEF783A8042B42DF5B8DDEE7304009AD2A9BB63220086B9D

**PARTIAL:4/10 scenarios completed and passed;0 completed scenario failures.** At approximately18:08 Europe/Sofia, the user stopped desktop automation and the parent requested immediate delivery/cleanup. The verifier interrupted its own runner withCtrl+C, confirmed finally cleanup, and checked the seven recorded owned headless PIDs:0 remaining. Parent/user visible game was not targeted. No additional test was started after the stop request.

| Scenario | Result | Exit codes | Sanitized assertions |
| --- | --- | --- | --- |
| Ui | PASS | Ui=0 | PASS UiFlow: menuOpen=True, state=Offline, mode=Offline, nm=0, cams=2, listeners=1 |
| InvalidCode | PASS | InvalidCode=0 | PASS InvalidCode: state=Failed, err=Join code is invalid or expired. |
| Resilience | PASS | Resilience=0 | PASS Resilience: cancelOk=True, code=[redacted], nmDuringHost=1, nmAfterLeave=0, finalState=Offline |
| Combat | PASS | Combat-H=0, Combat-C1=0 | PASS CoopHost: players=2, localOwner=True, cams=1, listeners=1, sep=2.60m (p0=(0.00, 0.05, -6.00), p1=(2.60, 0.05, -6.00)), enemy=road-raider-1 hp=36/70, clientDamaged=True; PASS CoopClient: players=2, localOwner=True, cams=1, listeners=1, sep=11.30m (p0=(0.00, 0.05, -6.00), p1=(2.18, 0.07, 5.09)), enemy=road-raider-1 hp=36/70, clientDamaged=True |

| Authority | INTERRUPTED / NOT VERIFIED | Host1900/client1796 stopped with runner | No completed outcome claimed |
| Progression | NOT RUN | Not started | User-stop handoff |
| LateJoin | NOT RUN | Not started | User-stop handoff |
| FullAcceptance | NOT RUN | Not started | User-stop handoff |
| ClientLeave | NOT RUN | Not started | User-stop handoff |
| Capacity | NOT RUN | Not started | User-stop handoff |

Grounding evidence from the completed Combat fixture: host receipt p0=(0.00,0.05,-6.00), remote initial p1=(2.60,0.05,-6.00), separation2.60m. This replaces candidate01's remote initialY9.45 at the same X/Z. The client's later p1=(2.18,0.07,5.09) follows its authorized combat relocation. This observation supports the containing-terrain spawn fix; it does not establish every spawn/walking/camera scenario.

Recorded own PIDs: UI23040; invalid14592; resilience10604; combat18352/11180; interrupted authority1900/1796. All exited after cleanup. No runtime/assertion edits or fixture retries by this verifier.

Full previous10/10 results remain historical candidate01 evidence in ONLINE_TEST_RESULTS_01.md; they are not Combined02 passes. **NOT RUN on Combined02:** completion of the six scenarios above, human two-PC cross-network acceptance, full manual gameplay and rendered performance profiling. The exact code/build identity above owns only the four completed results.

Final receipt and private extracted results.json checked for unredacted join-code fields:0. Raw local logs contain private diagnostic session details and must not be committed.

Private local evidence folder: C:\Users\skama\Documents\wow=rpg-diplomna\Integration\combined-20261116\.code-check\integration\relay-20261116-180643
