# Riftbound — Online Multiplayer Setup (UGS Sessions + Relay)

Date: 2026-11-15  
Status: SDK installed (`com.unity.services.multiplayer@2.4.1`); ready for UGS Sessions + Relay runtime verification.

## 1. Linked Unity Cloud Project & Environment
- **Unity Version**: `6000.6.2f1`
- **Project Name**: `My project (1)`
- **Organization ID**: `skamaz3434`
- **Cloud Project ID**: `3138f0db-6d7e-4e5c-b527-395d23acf091` (stored in `ProjectSettings/ProjectSettings.asset`)
- **Environment**: `production` (default UGS environment; or specify a `development` environment in `InitializationOptions.SetEnvironmentName(...)` if configured in Dashboard).

## 2. Installed Packages (Pinned in `Packages/manifest.json`)
- `com.unity.services.multiplayer`: `2.4.1` (provides `Unity.Services.Core`, `Unity.Services.Authentication`, `Unity.Services.Multiplayer`, Relay & Session integration)
- `com.unity.netcode.gameobjects`: `2.14.0` (with `com.unity.transport`)

## 3. Required Unity Cloud Dashboard Actions (Owner-Only)
Before public or cross-network testing, verify in [Unity Cloud Dashboard](https://cloud.unity.com/) under project `3138f0db-6d7e-4e5c-b527-395d23acf091`:
1. **Authentication**:
   - Navigate to **Products > Authentication > Settings**.
   - Ensure **Anonymous** sign-in provider is **Enabled**.
2. **Multiplayer Services (Relay & Sessions)**:
   - Navigate to **Products > Multiplayer > Relay** and **Sessions / Lobby**.
   - Accept the UGS Terms of Service if prompted and confirm **Relay** is enabled for the active environment.
3. **No Secrets in Repo**:
   - Do **not** commit service-account keys, API secret keys, or auth tokens to the repository. Only the public `cloudProjectId` is stored in `ProjectSettings.asset`.

## 4. Quotas, Billing & Cost Notes
- Unity Gaming Services **Authentication**, **Sessions/Lobby**, and **Relay** include a free tier (typically metered by CCU / bandwidth / MAU), after which pay-as-you-go charges apply if a billing method is attached and limits are exceeded.
- Do not enable paid billing tiers or raise usage caps without explicit owner approval; monitor usage under **Administration > Usage & Billing** in Unity Cloud Dashboard.

## 5. Same-Machine Multi-Process Testing (Profiles)
- `AuthenticationService.Instance.SwitchProfile(profileName)` (or `InitializationOptions.SetProfile(...)`) is used so same-machine test processes (`-ugsProfile host_p0`, `-ugsProfile client_p1`) use **separate** anonymous identities and token storage without affecting any real player profile.

## 6. Progression & Persistence Policy (Current Release)
- **Offline Single-Player**: Uses local character saves as before (`Offline` mode works with zero internet or UGS initialization).
- **Online Co-op**: Uses **isolated session-only progression** for the current release (explicitly labeled in-game). `SaveAdventure` is bypassed in multiplayer so offline single-player saves are never overwritten. Host departure ends the session cleanly (no host migration in this release).
