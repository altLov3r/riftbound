# Riftbound Multiplayer Presentation API Contract & File Releases (`MP00`)

- **Branch:** `multiplayer/antigravity-polish-20261118`
- **Base Commit:** `033e15408795c25204959028d706f35a5c1d7f05` (`combat/antigravity-refinement-20261117`)
- **Plan Reference:** `Docs/Design/MULTIPLAYER_PRESENTATION_PARTY_PLAN.md` (`origin/story/codex-content-20261117` @ `f7ef4ee`)

## Explicit File Ownership & Releases (Zero Concurrent Edits)

Antigravity **explicitly releases** the following Codex visual presentation files (kept byte-identical to `033e154` and `a81b5e3`) and will NOT modify them on `multiplayer/antigravity-polish-20261118`:

| Released File | Status | Exact Verified Git Blob SHA1 |
| :--- | :--- | :--- |
| `Assets/RPG/Scripts/GroundedVisual.cs` | **RELEASED TO CODEX** | `600f6bc8c00f08e399bd14695d8de421b2659f10` |
| `Assets/RPG/Scripts/MountVisual.cs` | **RELEASED TO CODEX** | `5e1f956630ce5b8e70d010f14389e455eae33f33` |
| `Assets/RPG/Scripts/CharacterVisualFactory.cs` | **RELEASED TO CODEX** | `64b6f6e2af60ecd17c0324d984c6daeab89e053f` |

Codex coordination note (`origin/visuals/codex-presentation-20261118` @ `0fdc333`): Antigravity acknowledges Codex's 9 approved Undead Animator Controller state fixes (`Player Undead Warrior/Mage/Rogue.controller` adding `WalkBack`, `Jump`, and `Ride`) as directly compatible with `RiftboundCoopPlayer`'s priority table below; `.controller` files remain untouched on `multiplayer/antigravity-polish-20261118` for clean integration.

Antigravity owns:
- `Assets/RPG/Scripts/Network/RiftboundCoopPlayer.cs`
- `Assets/RPG/Scripts/Network/CoopWorldManager.cs`
- `Assets/RPG/Scripts/PlayerController.cs` (and `PlayerMounts.cs`, `PlayerUtilities.cs`, `PlayerServices.cs`)
- `Assets/RPG/Scripts/EnemyController.cs`
- `Assets/RPG/Scripts/PartySystem.cs`, `Assets/RPG/Scripts/PrototypeHUD.cs`, and session/group UI callers

## Agreed Presentation Contract v1 (Between Runtime & Visual Owners)

### 1. Mount Lookup & Presentation Instance
- **Runtime Lookup:** Antigravity resolves `MountDefinition` by exact lowercase `mountId` (`"horse"`, `"zebra"`, `"gryphon"`) from the local scene `PlayerController.mounts` serialized array plus `Resources.LoadAll<MountDefinition>("Mounts")` (zero `AssetDatabase` calls in runtime builds).
- **Instance Lifecycle:**
  - `RiftboundCoopPlayer` instantiates `MountDefinition.prefab` (`MountVisual` with `animator`, `saddle`, `walkStride`, `runStride`, `artVersion`) as a cosmetic child (`"CoopMount_{mountId}"`) without adding duplicate `NetworkObject`, `CharacterController`, or solid colliders.
  - On mount change (`MountId.OnValueChanged`), `IsMounted.OnValueChanged`, visual rebuild (`Race`/`ClassName` swap), `Health <= 0`, or `OnNetworkDespawn`, the previous mount child is cleanly destroyed, rider weapon visibility restored, and rider visual root reset to local origin (`Vector3.zero`).
- **Saddle Fitting (`LateUpdate`):**
  - When mounted & alive, rider `Animator` hips (`HumanBodyBones.Hips`, fallback `:Hips`/`hips`/`pelvis`) align to `MountVisual.saddle.position` identically to local `PlayerMounts.PositionRider()`.
- **Mount Animation Call:**
  - `remoteMountVisual.Animate(planarSpeed, isAirborne, isWalking)` is called once per frame while mounted (`Idle`, `Walk`, `Run`, `Jump`).
  - **Requested Codex Calibration (`CV02`):** Replace literal `2.6f` / `7.2f` stride divisors inside `MountVisual.Animate` with `walkStride` / `runStride` (or calibrated per-mount strides) and tune `Mount Horse` / `Mount Zebra` / `Mount gryphon` saddle transforms if hoof/seat offsets need adjustment.

### 2. Replicated Locomotion & Action Presentation
- **Durable Replicated States:**
  - `IsMounted` (`bool`) + `MountId` (`FixedString32Bytes` normalized `"horse"` / `"zebra"` / `"gryphon"`).
  - `MoveSpeed` (`float`), `LocomotionFlags` (`byte`: bit 0 `IsAirborne`, bit 1 `IsWalking`, bit 2 `IsBlocking`), `ResourcePct` (`byte` 0..100).
- **Sequenced One-Shot Actions (`actionId`, `duration`, `actionSeq`):**
  - Finite allowlisted IDs only: `"Attack"`, `"Cleave"`, `"Cast"`, `"Block"`, `"Dodge"`, `"Hit"`, `"Cancel"`.
  - Unknown action IDs or out-of-order/stale sequences (`seq <= lastObservedSeq`) are rejected server-side and ignored observer-side.
  - Observer priority order: `Dead (Health <= 0)` > `Mounted ("Ride" + MountVisual.Animate)` > `Blocking ("Block")` > `Active One-Shot (Time.time < actionAnimUntil)` > `Airborne ("Jump")` > `Grounded ("Run"/"RunBack"/"Walk"/"WalkBack"/"Idle")`.
  - **Agreed Codex Calibration (`CV03` / `CV04`):** Current `GroundedVisual` (`Start` / `LateUpdate`) binds `PlayerController` and `CharacterController` directly; for observer grounding on `RiftboundCoopPlayer`'s `remoteVisualRoot`, Antigravity agrees to `GroundedVisual.SetObserverState(bool alive, bool airborne, bool mounted)` (non-breaking opt-in observer hook that keeps `transform.position` authoritative on the network root and applies only local visual ground offset when `!mounted && alive && !airborne`). No concurrent edits by Antigravity.

### 3. Authoritative Constraints
- No cosmetic mount, animator state, social role badge (`SocialRole` 0..3 `Auto`/`Tank`/`Healer`/`DPS`), `ActionAnimRpc`, or party frame UI alters `Combatant` HP, damage, cooldowns, resources, quest state, chest unlocks, or transport host authority.
