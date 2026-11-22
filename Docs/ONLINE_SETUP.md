# Online co-op setup

The project uses Unity Gaming Services multiplayer sessions/Relay and Netcode for GameObjects. Package versions are pinned in Packages/manifest.json. Use the Unity Cloud project linked in ProjectSettings and confirm anonymous authentication and the relevant multiplayer services are enabled for its environment.

## Emberford dedicated realm

From the title screen, choose **Play Online → Select Emberford → Character Select → Enter World**. Status appears while connecting. Use **Back to title** after a connection error, or **Leave Session & Return to Menu** in the Group menu while connected.

The client defaults to `127.0.0.1:7777` for local testing. To use a running realm at a different address, launch the client with `-realmEndpoint <address> -realmPort <port>`. The realm picker shows the configured address. Character selection uses the local roster and Emberford uses separate progress.

## Invite-code co-op

1. Build the same source/configuration for every participant and copy each complete standalone folder.
2. Open **Play Online → Invite-code co-op**. The host selects **Host Online**, chooses a character and creates the session.
3. Share its private invite code with the joining players; keep codes and authentication details out of public reports.
4. Joining players select **Join Online** and enter the code. Current capacity is four human players.

Offline mode does not require service initialization. Online progress is isolated to the session, and host departure ends it; persistent online characters and host migration are not implemented.

## Verify

For same-machine tests use separate authentication profiles, such as `-ugsProfile host_p0` and `-ugsProfile client_p1`, to avoid identity/token collisions. Same-machine checks do not establish two-PC cross-network acceptance.

The targeted runner is [Verify-CombinedRelay.ps1](Integration/Verify-CombinedRelay.ps1). Read its parameters and supply the exact candidate build; never mix binaries from different sources. Record actual role/exit/output evidence and distinguish old baseline checks from new candidate results.

Cloud project identifiers are configuration; service credentials, account keys, authentication tokens and live invite codes must stay out of Git. Monitor service usage in the linked dashboard and keep current project limits unchanged unless the owner approves a change.
