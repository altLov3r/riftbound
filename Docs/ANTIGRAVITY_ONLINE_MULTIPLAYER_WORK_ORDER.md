# Antigravity work order — complete internet co-op

> Full game roadmap: **Docs/RIFTBOUND_MASTER_PLAN.md**. This online work order remains the first implementation priority; complete its acceptance gates before the broader content expansion. Read the latest IMPLEMENTATION_STATUS.md because some source/package work may already be implemented.

Date: 2026-11-15. Status: READY TO IMPLEMENT; no online implementation is claimed by this document.

## Authoritative task

The user has chosen internet multiplayer as the main deliverable. This work order supersedes LAN-first scope and any claim that MP-01–MP-08 are fully complete in older notes. Preserve existing source, assets, references, saves and offline play. Read current source before editing; do not recreate the working test arena or replace the networking stack.

Implement 2–4 player private online co-op in the actual RPG with a shareable join code. Finish existing gameplay replication defects before expanding the game. LAN may remain a development-only fallback; it is not the release multiplayer experience.

## Decision: Unity Multiplayer Services sessions + Relay

Keep Netcode for GameObjects and Unity Transport. Add a compatible Unity Multiplayer Services SDK and its required Authentication dependencies. Use anonymous UGS authentication and host-owned sessions configured for Relay networking. The host runs the authoritative game; Relay carries connections across networks. No dedicated game server, public matchmaking, router port forwarding, VPN, Steam dependency or custom password backend is required for this first release.

Use ONE session lifecycle. Prefer SDK sessions with Relay and the documented NGO integration; do not combine sessions-managed startup with the old direct-IP StartHost/StartClient path or a second manual Relay allocation flow. Verify the exact installed SDK APIs before coding and pin compatible versions in manifest/lockfile. Do not blindly upgrade Unity or NGO.

Unity sessions can coordinate services, but they do not automatically synchronize this game's combat, quests, AI or persistence. Those remain explicit work below. Host departure ends gameplay for this release; do not confuse session owner changes with implemented gameplay host migration.

## Online player flow

1. Open title screen, select/create character, choose Offline, Host Online or Join Online.
2. Host Online initializes services once, authenticates, creates a private four-player Relay-backed session and starts the existing world through one bootstrap path.
3. Display the session join code only after creation/network startup succeeds. Provide Copy Code, connected-player count and Leave Session. Treat the code as an invitation; do not publish it in telemetry.
4. Join Online accepts a pasted/typed code, normalizes whitespace/case according to SDK rules, authenticates, joins the session and enters the same world.
5. Show clear connecting/loading states and specific invalid/expired/full/incompatible/offline-service errors with Retry and Back.
6. Leave returns to a usable character/title flow. Host exit informs clients and ends the session cleanly. Offline remains usable without UGS initialization or internet access.

Keep UI minimal and use existing menu styling. No IP or port fields in the normal online flow. No fake email/password login. Do not show debug networking panels in the release HUD.

## O-00 — correct the evidence and reproduce current defects

- [ ] Read Docs/IMPLEMENTATION_STATUS.md and the existing network classes. Preserve successful test receipts; mark narrow tests as narrow evidence.
- [ ] Record current source revision/local changes, exact package versions and reproducible test setup. Compile before larger edits.
- [ ] Establish a short two-player test in the actual RPG, not only the capsule arena. Do not spend another session repeatedly proving package installation or host-only spawning.
- [ ] Track the known Reaver terrain failure separately; do not claim the full offline regression passes while it fails.

## O-01 — repair multiplayer correctness first

- [ ] **Damage/kill credit routing:** inspect EnemyController calls through RiftboundCoopPlayer.LocalOwner on the host. Their broadcast handlers currently gate on the receiving network object's IsOwner/OwnerClientId. Deliver targeted messages through the intended client's avatar or use a server-owned dispatcher that resolves the recipient's local player. Test real client HP loss and client kill credit; synchronized enemy HP alone is insufficient.
- [ ] **Combat authority:** clients submit an ability/action identifier and target, not trusted damage. Host computes damage from validated state and checks class, equipped ability, cooldown, resources, life state, distance and LOS. Apply the same rules to host actions, interrupts and area effects.
- [ ] **Movement authority:** remove the acceptance of arbitrary jumps over 30 m in SubmitStateServerRpc. Use elapsed-time/speed limits and explicit host-authorized spawn/teleport transitions, with collision/ground validation and correction behavior. Do not trust client-mounted state to bypass speed limits.
- [ ] **Vitals/progression authority:** do not accept arbitrary HP, maximum HP or level from regular client state messages. Separate local visual identity from host-validated combat stats and progression. Document any allowed imported-character trust boundary.
- [ ] **Quest/chest authority:** validate prerequisites, objective progress, target eligibility, interaction distance, life state and claim identity before approval. A valid quest index and an unclaimed hash-set entry do not prove quest completion. Reject duplicate/stale requests without consuming legitimate future claims.
- [ ] **Shared world:** host spawns/owns event actors and publishes event state, NPC/world interactions and rewards to eligible players. IsAuthoritative alone does not replicate the caravan or scout. Late join must reconstruct active events without replaying historical rewards.
- [ ] **Animation/mounts:** replicate attacks, casts, interrupts, hits, death and mount model/type, not only Idle/Walk/Run/Ride or a Mounted nameplate.
- [ ] **Bootstrap:** audit runtime initialization versus subsequent SceneManager loads. Ensure session manager creation works when entering from the menu, restarting a session and returning to the world, not only when launched via smoke-test CLI flags.

Gate: both host and client can damage and be damaged, receive correct quest credit and loot independently, respawn, and see the same actors/actions. Test through ordinary input as well as focused automation.

## O-02 — cloud/project setup without blocking all work

- [ ] Verify current Unity project/cloud association and the owner's existing UGS organization/project. Reuse the correct project; do not create duplicate cloud projects or change ownership casually.
- [ ] Link the editor project and select one documented UGS environment for development. Enable Authentication and the services required by the selected Sessions/Relay flow.
- [ ] Record required dashboard actions and SDK compatibility in Docs/ONLINE_SETUP.md. Public project/environment IDs may be configuration; never commit service-account keys, access tokens or other secrets.
- [ ] Check current quotas, billing requirements and usage monitoring. Do not promise unlimited free service or enable paid usage/change billing without explicit user approval.
- [ ] If account access, terms acceptance, billing or service permission is required, finish code/configuration that can be prepared safely, then ask for the one exact missing dashboard action. Keep offline play working. Do not fake a successful service connection or loop on rejected authentication.
- [ ] Use separate Authentication profiles for same-machine test processes so they do not share the same anonymous identity/token storage. Do not sign out/delete a real player's identity just to isolate a test.

## O-03 — implement the online session adapter

Likely integration points: CoopSessionConfig.cs, CoopWorldManager.cs, FrontendMenu.cs and a small online-session service. Follow existing project style and keep gameplay authority separate from cloud connection setup.

- [ ] Replace release modes HostLan/JoinLan with HostOnline/JoinOnline, retaining Offline and optional hidden direct-IP development support.
- [ ] Implement one state machine: Offline → Initializing → Authenticating → Creating/Joining → Connecting → InWorld → Leaving, plus recoverable Failed state.
- [ ] Serialize create/join operations; disable duplicate button presses. Reuse the single NetworkManager and register the required prefabs before network startup.
- [ ] Use the chosen SDK session join code consistently. Do not mix a Sessions code with a separately generated Relay allocation code.
- [ ] Enforce four players including host, build/protocol/content compatibility and authentication/session admission. A join code is not authorization to invoke another player's actions; enforce NGO ownership and server validation independently.
- [ ] Coordinate existing scene/bootstrap logic with sessions-managed network startup. Verify prefab readiness, spawn points, world snapshot readiness and exactly one local camera/HUD/listener.
- [ ] Use bounded async operations with user cancellation. If an SDK method cannot truly cancel, invalidate its attempt token and clean up any late-created session rather than letting it reopen a cancelled screen.
- [ ] Leave/delete sessions as appropriate through the SDK, shut down NGO once, unsubscribe callbacks and clear transient state. Handle partial initialization failures and repeated Host → Leave → Join cycles.
- [ ] Surface service quota/network/rate-limit errors. Use bounded backoff only for transient failures; never retry invalid codes, missing permissions or incompatible builds indefinitely.

## O-04 — progression policy and complete gameplay

- [ ] Preserve offline saves and explicitly label multiplayer progression policy in-game. Current SaveAdventure skips multiplayer; do not silently imply online rewards are saved.
- [ ] Initial delivery may use isolated session-only progression, clearly disclosed. If adding durable co-op saves, version their format, identify players consistently, validate rewards on host and test duplicate claims/rejoin without overwriting offline files.
- [ ] Anonymous UGS identity supports service access; it is not automatically a recoverable cross-device account or cloud save. Do not advertise either without implementation.
- [ ] Verify Abe quests, faction/cartography objectives, enemies, chests, shops, abilities, mounts, companions, death/respawn, portals/dungeons and event rewards through the same validated paths. Mark unsupported systems explicitly and keep them inaccessible rather than letting clients corrupt state.
- [ ] Handle late join snapshots and content readiness, client departure during combat/interaction, host departure and session re-entry. No host migration in this release.

## O-05 — release verification

Required checks; record PASS/FAIL/NOT RUN and evidence rather than a blanket COMPLETE:

| Scenario | Required outcome |
| --- | --- |
| Two standalone processes, separate test identities, Relay enabled | Host creates a code, client joins, both see each other and own only their controls. |
| Two PCs on different internet connections, e.g. home network and mobile hotspot | Join code works without direct IP, port forwarding or VPN; verify the active Relay configuration. |
| Two-player ordinary gameplay | Combat both ways, client HP loss, kill credit, quests, independent chests, mounts, respawn and events work. |
| Three/four players and fifth join attempt | Four play together; fifth gets a useful full-session rejection. |
| Late join during combat/event | Correct current actors, HP, event stage and player-specific progression; no historical reward replay. |
| Cancel, wrong/expired code, services unavailable, incompatible build | No infinite spinner, duplicate session, input lock or offline dependency. |
| Client disconnect and host exit | Cleanup and useful menu recovery; no ghost actors or saves corrupted. |
| Host → leave → host; join → leave → offline | No duplicate managers, callbacks, cameras or stale UI/player state. |
| Invalid actions and repeated requests | Host rejects impossible movement, invented damage, premature quest completion and duplicate claims. |
| Offline regression | Existing characters still load and play; report the terrain failure honestly until fixed. |

Two local processes are an intermediate check, not proof of cross-network internet play. If a second network/device is unavailable, deliver a reviewable build and a short exact test request; leave that check NOT RUN. Avoid replacing runtime tests with assertions that only exercise the happy-path smoke CLI.

## Mandatory anti-loop rule

Retain revision 3 limits: at most two unchanged failed attempts; stop after 10 minutes with no new evidence; cap an unchanged blocker investigation at 20 minutes. Every test has a recorded hypothesis, expected result, timeout, last reached stage and attempt count across handoffs.

Do not repeatedly rebuild a large standalone for a dashboard permission failure. Do not repeatedly reinstall packages for an RPC ownership bug. Choose the smallest diagnostic that distinguishes causes. After the limit, document the blocker and continue safe independent tasks. Never mark a failed gate passed to escape it.

## Deliverables

1. Focused implementation preserving .meta references and existing authored world.
2. Docs/ONLINE_SETUP.md with exact cloud prerequisites, environment/package versions, cost/quota notes and owner-only steps still required.
3. Updated Docs/IMPLEMENTATION_STATUS.md separating source implementation, compilation, local Relay tests and actual cross-network acceptance. Replace unsupported COMPLETE labels with precise status.
4. A fresh standalone build, exact startup path, Host/Join-code instructions, test matrix results and remaining limitations, especially persistence/host exit.
5. Only after these gates: compact party frames, map pings, ready checks, ally revive and a shared dungeon. Do not add extras while core client damage/rewards or internet joining are broken.

## Official references checked for this decision

Recheck the documentation for the selected package version before implementation:

- [Multiplayer Services SDK](https://docs.unity.com/en-us/mps-sdk)
- [UGS initialization and anonymous authentication](https://docs.unity.com/en-us/mps-sdk/init-ugs)
- [Relay-backed sessions](https://docs.unity.com/en-us/mps-sdk/networking/relay-servers)
- [Session networking and SDK-managed startup](https://docs.unity.com/en-us/mps-sdk/session-networking)
- [Joining by session code](https://docs.unity.com/en-us/mps-sdk/join-session)
- [NGO session tutorial and underlying service billing](https://docs.unity.com/en-us/mps-sdk/tutorials/build-your-first-session)
