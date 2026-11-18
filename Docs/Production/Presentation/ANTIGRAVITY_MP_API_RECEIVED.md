# Riftbound Multiplayer Presentation API Contract & File Releases (`MP00`)

- **Branch:** `multiplayer/antigravity-polish-20261118`
- **Base Commit:** `033e15408795c25204959028d706f35a5c1d7f05` (`combat/antigravity-refinement-20261117`)
- **Plan Reference:** `Docs/Design/MULTIPLAYER_PRESENTATION_PARTY_PLAN.md` (`origin/story/codex-content-20261117` @ `f7ef4ee`)

## Explicit File Ownership & Releases (Zero Concurrent Edits)

Antigravity **explicitly releases** the following Codex visual presentation files and will NOT modify them on `multiplayer/antigravity-polish-20261118`:
- **RELEASED TO CODEX:** `Assets/RPG/Scripts/MountVisual.cs`
- **RELEASED TO CODEX:** `Assets/RPG/Scripts/GroundedVisual.cs`
- **RELEASED TO CODEX:** `Assets/RPG/Scripts/CharacterVisualFactory.cs`

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
  - **Requested Codex Calibration (`CV03` / `CV04`):** Current `GroundedVisual` (`Start` / `LateUpdate`) binds `PlayerController` and `CharacterController` directly; if `CV04` adds observer grounding on `RiftboundCoopPlayer`'s `remoteVisualRoot`, expose a clean non-breaking `BindObserver(Animator anim, Func<bool> isMounted, Func<bool> isAlive, Func<bool> isAirborne)` (or similar opt-in hook) without moving `transform.position` (server root stays authoritative). No concurrent edits by Antigravity.

### 3. Authoritative Constraints
- No cosmetic mount, animator state, `ActionAnimRpc`, or party frame UI alters `Combatant` HP, damage, cooldowns, resources, quest state, chest unlocks, or transport host authority.
