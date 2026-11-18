# CV01 — bounded static character and mount inventory

18 November2026. Read-only source inspection at combat runtime033e15408795c25204959028d706f35a5c1d7f05 in the new visuals/codex-presentation-20261118 checkout. No original asset, metadata, controller, prefab, scene or runtime source was modified for this inventory. These findings are serialized/source facts, not native rig validation, measured stride, actual ground contact or gameplay passes.

## Character coverage

`Assets/RPG/Resources/Characters` contains24 prefabs: Human, Dwarf, Elf, Orc, Undead and Goblin × Warrior, Paladin, Mage and Rogue. This agrees with CharacterVisualFactory's actual public race/class arrays. A fifth class is not present in that runtime's supported set. All24 serialized Animators have `m_ApplyRootMotion: 0`.

| Race | Serialized avatar/model family | Controller family |
| --- | --- | --- |
| Human | Knight_Arms / Paladin_Guard / Arcanist_Mage / Ranger_Scout | Player Humanoid Melee / Melee / Caster / Rogue |
| Dwarf | Warrior uses Blacksmith_Man; Paladin/Mage/Rogue share the corresponding humanoid avatars above | Same three humanoid controllers |
| Elf | Same avatar identities as Human, separate scaled resource prefabs | Same three humanoid controllers |
| Orc | Crag_Orc for all four classes | Same three humanoid controllers; Humanoid import needs native validity/retarget check |
| Goblin | Forest_Goblin for all four classes | Same three humanoid controllers; Humanoid import needs native validity/retarget check |
| Undead | Skeleton_Warrior for Warrior/Paladin; Skeleton_Mage; Skeleton_Rogue | Player Undead Warrior / Warrior / Mage / Rogue |

Character factory loads `Resources/Characters/{race}_{class}` first. The24 existing resource prefabs supply a build-backed path; editor-only archetype fallback must not be mistaken for player-build coverage. Actual included dependencies and avatar validity require native inspection/build verification.

Relevant FBX importer declarations: NPCs_WoW humanoid source families and UniversalAnimations/UAL1_Standard use `animationType: 3`, `avatarSetup: 1`; Skeleton_Warrior/Mage/Rogue use `animationType: 2`, `avatarSetup: 1`. These are serialized Humanoid/Generic declarations, not proof the loaded Avatar is valid or each retarget looks correct.

## Player controller state audit

All six `Assets/RPG/Animations/Player*.controller` files contain12 states: Idle, Walk, Run, WalkBack, RunBack, Jump, Ride, Attack, Cleave, Block, Hit and Death. All inspected state records set `m_IKOnFeet: 0`. No dedicated dodge, strafe, turn, cast-start, fall or land state is authored in these six controllers; current runtime may render such actions using existing substitutes. Their absence is a coverage gap, not proof gameplay lacks those actions.

| State | Three Humanoid controllers | Three Undead controllers |
| --- | --- | --- |
| WalkBack | Same forward Walk_Loop motion as Walk, state speed−1 | Same Walking_A motion as Walk, state speed−1 |
| RunBack | Same Jog_Fwd_Loop as normal Melee/Caster Run, state speed−0.85 | Same Running_A motion as Run, state speed−0.85 |
| Run | Jog_Fwd_Loop speed1 for Melee/Caster; Sprint_Loop speed0.9 for Rogue | Running_A speed1 |
| Jump | Jump_Loop speed1 | Running_A speed0.75: grounded running is being used as an airborne substitute |
| Ride | Sitting_Idle_Loop speed1 | Same Idle motion as Idle: no actual seated rider pose selected |
| Block | Crouch_Idle_Loop speed1 | Same Idle motion as Idle: no actual guard pose selected |
| Attack | Sword_Attack speed1.3 Melee/1.45 Rogue; Spell_Simple_Shoot speed1.25 Caster | 2H_Melee_Attack_Slice speed1.25 Warrior; 1H_Melee_Attack_Slice_Horizontal speed1.25 Rogue; Spellcast_Shoot speed1.25 Mage |
| Cleave | Sword_Attack / Punch_Cross / Spell_Simple_Shoot speed1.1 | 2H_Melee_Attack_Spin speed1.1 |

Reverse playback is confirmed configuration; whether it causes the reported backward-foot appearance needs sampled/native poses. In particular reversed forward gait is not a purpose-authored backward gait. Do not globally reverse root orientation or change authoritative facing to compensate.

Existing compatible-family candidates are already present in each selected Skeleton FBX `.meta`: Walking_Backwards internalID−6411013857362534180; Jump_Idle5069180758234724613; Jump_Start−6402709644054655732; Jump_Land3282281275607961068; Sit_Chair_Idle7779023693546330386; Sit_Chair_Pose−6504270679737740119; Block−6801083763907307519; Blocking−6757986481916104768. Most are not selected by current Undead player controllers. Review native rig/state behavior and request exact controller-change approval before rebinding. `Blocking` is currently non-looping in the serialized import settings; do not assume a suitable hold loop just because of its name.

The UniversalAnimations metadata declares Walk_Loop0–40 frames, Jog_Fwd_Loop0–28, Sprint_Loop0–20 and Sitting_Idle_Loop0–50, all `loopTime: 1`; Jump_Loop0–75 loops. Sword_Attack0–46, Spell_Simple_Shoot0–15, Punch_Cross0–30, Hit_Chest0–10 and Death010–72 do not loop. Frame ranges do not establish seconds without the loaded clip frame rate. Actual state duration also depends on controller and runtime Animator speeds; do not derive a replacement network action duration from these frame ranges alone.

## Existing character calibration declarations

These values come from `CharacterVisualFactory.GetCalibratedStrides`; they are existing source constants, not fresh measurements.

| Race | Walk | Run, other classes | Run, Rogue |
| --- | --- | --- | --- |
| Human |0.93 |5.00 |4.73 |
| Dwarf |1.02; Rogue1.20 |5.50 |6.19 |
| Elf |0.80 |4.25 |4.05 |
| Orc |0.77 |4.10 |4.39 |
| Goblin |0.65 |3.30 |3.76 |
| Undead |0.72 |1.00 |1.00 |

RacialProfile has a second set of walk/run values used as profile defaults. Trace actual callers and sampled scale before replacing either set. Controller body-height declarations range1.40m Goblin to2.20m Orc and visual target-height declarations1.32m to2.08m; these are authored targets, not measured rendered geometry or evidence that a root should be moved down.

## Mount coverage and references

Three `Assets/RPG/Data/Mounts/*.asset` definitions are present. The saved player `mounts` array in Outpost-Expanded references all three by their existing GUIDs. There is no Resources/Mount* definition or prefab lookup entry in the inspected resource root. Scene reachability provides a dependency path, but actual build inclusion must be verified natively; the remote implementation must use an agreed runtime-visible registry/scene reference rather than an editor AssetDatabase fallback.

| mountId / data asset | Existing MountVisual prefab | Run/walk/fly speed | Unlock / actual art |
| --- | --- | --- | --- |
| horse / Horse.asset | Prefabs/Mount Horse.prefab |8 /3 /8 | No exploration gate; Animals/Horse.fbx + Animal Horse.controller |
| zebra / Zebra.asset | Prefabs/Mount Zebra.prefab |9 /3 /8 | requiresExploration1; Animals/Zebra.fbx + Animal Zebra.controller |
| gryphon / gryphon.asset | Prefabs/Mount gryphon.prefab |8 /3 /16 | isFlying1; currently the same Horse.fbx and Animal Horse.controller, not a separate winged model/flight controller |

The gryphon ID/displayName must be preserved even if its art is upgraded later. This audit does not claim wings exist because the display name says Celestial Winged Steed.

| Prefab | artVersion | Serialized walkStride | Serialized runStride | Saddle local position under animated parent |
| --- | --- | --- | --- | --- |
| Mount Horse |2 |1.410795 |6.0645556 |(0.00069961796,0.035730693,0.002349862) |
| Mount gryphon |2 |1.410795 |6.0645556 |Same as Horse |
| Mount Zebra |2 |1.3907937 |5.978579 |(−0.00048979383,0.035960473,0.0023665102) |

Saddle values are **bone-parent local coordinates**, not world metres or a direct rider Y offset. A static source audit cannot infer seat/hips contact from these numbers. Existing MountExpansionSetup creates seats on `Body`, scales animals to a2.4m renderer-bounds target and applies180° model yaw, then measures stride. This historical editor recipe is evidence of intent, not permission to rerun it: Setup also imports models, writes assets and changes/saves world content.

MountVisual currently ignores both serialized stride fields in Animate and divides by hardcoded2.6/7.2. The fields above were already serialized by the setup's stride sampler, but are not newly verified. Native sampling should assess whether using those fields gives acceptable local and remote hoof contact before broad tuning. Missing Animator/controller/state guards are also absent in current Animate.

Horse and Zebra controllers both contain Idle, Walk, Run, Jump and Death, all at state speed1 and mapped to the corresponding FBX clips. Their `.fbx.meta` files use Generic `animationType: 2`, create-own-avatar setup1, `loopTime/loopBlend: 1` for Idle, Walk, Run and WalkSlow; Jump and Death do not loop. Selected frames are Idle0–150, Walk0–64, Run0–20, Jump0–36, Death0–26. All selected clip records declare `heightFromFeet: 1`, `keepOriginalPositionXZ: 0`. No distinct flight/landing state exists in these two animal controllers.

## Grounding, presentation and material limitations

- All24 resource character Animators disable root motion. That does not establish absence of bone/root sway in the clips, model-child transform offsets or interpolation error.
- Native rig validation, loaded clip seconds, sampled soles/hooves, controller-bottom distance, actual material/shader/texture support, visual bounds, seats, camera clearance, collision and bridge support are **NOT RUN** in this source-only inventory. Unresolved external GUIDs from a bounded lookup are not labelled missing assets.
- No whole-world model count or universal hover claim is made. Authored magical floating objects, particle/trail art, airborne poses and intentional fallen logs must be classified separately in any later scene audit.
- Remote primitive horse and fixed seat offset are documented by the parent MP00 audit. Correct native art here does not fix that network caller automatically.

## Proposed exact next reviews

1. Native inspect all24 character prefabs and three mount definitions, selected states/clip durations/rig validity, per-model idle sole offset and mounted seat/hips gap. Do not mutate persistent assets in diagnostics.
2. Compare actual forward/backward contact trajectories against travel; prioritize existing Skeleton Walking_Backwards and supported airborne/seat/guard clips. Produce exact controller/state mapping proposals before application.
3. After explicit file release, correct MountVisual stride-field use and robust state binding, preserving gameplay speeds/unlocks and missing-state fallback.
4. Ground mesh children using stable support/pose evidence. Preserve player/server motor roots, collision, jumping, flight and deck-vs-terrain support. Do not solve model hover by globally reducing spawn clearance or terrain height.
5. Antigravity owns remote registry/caller/action/network changes. Hand off mount IDs, prefab/Animator/saddle dependencies, state support and measured/native follow-up evidence instead of editing its files.

Validation performed: read serialized text and source at the exact baseline; cross-checked24 prefab names, Animator root-motion declarations, selected controller/GUID references, controller state speed/motion records, imported clip internal IDs/loop flags, three mount definitions/prefab fields and scene's mount-definition GUID references. No C# changed; no compilation, Unity execution, rendered/native or gameplay pass is claimed by this inventory.
