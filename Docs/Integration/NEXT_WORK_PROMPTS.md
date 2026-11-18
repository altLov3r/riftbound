# Lore and combat — next two workstreams

## Latest queue —18 November2026

For the user's reported remote horse placeholder, combat animation mismatch, hovering models and party polish, use **[MULTIPLAYER_PRESENTATION_PARTY_PLAN.md](../Design/MULTIPLAYER_PRESENTATION_PARTY_PLAN.md)**. Its sections11–12 contain the next Codex/Antigravity prompts and its ownership table supersedes this older queue only after explicit file release. This is a plan, not started implementation. Latest inspected AG runtime033e154 includes formal narrative schema acceptance; the prior pending observation on the story branch is historical. Preserve the accepted content and earlier deliverables.

## Previous queue —17 November2026

Prepared 17 November 2026. These start bounded implementation when the user submits them; writing this file has not started runtime changes. Both workers start from the same latest published integration/combined-20261116, including this plan. Main and the finished world/gameplay branches remain preserved.

## Chosen split

**Codex: story content, NPC voice/state data, world evidence/encounter staging and read-only narrative validation.** First deliverables SC00–SC03, then SC08/SC10 preparation.

**Antigravity: combat/AI/network correctness and feel, runtime NPC dialogue/state adapter and behavior.** First deliverables AG00 remaining online checks, AG01 demonstrated authority/parity corrections, AG02 lore adapter, AG03 one combat reference encounter. These labels name this work queue; detailed tasks are SC/CF in the plans.

Codex must not edit runtime NPC/combat files. Antigravity must not modify Codex's content files or world scenes/terrain/prefabs without agreement. FactionQuestSystem.cs contains both data and runtime: Antigravity owns the whole file; Codex supplies ID-keyed copy proposals instead of touching it. One agreed catalog schema, one adapter, one scene owner.

## Send this to Codex

```text
Start Riftbound's narrative-content workstream from the latest published integration/combined-20261116, including Docs/Integration/NEXT_WORK_PROMPTS.md.

Read applicable AGENTS.md, Docs/Integration/INTEGRATION_HANDOFF.md, Docs/Integration/POST_INTEGRATION_PLAN.md sections13–14, Docs/Lore/RIFTBOUND_LORE_BIBLE.md, Docs/Lore/NPC_STORY_CONTRACTS.md and Docs/Design/COMBAT_REFINEMENT_PLAN.md. Preserve current saves, IDs, serialized references, original assets and all finished branches/builds.

Use a new separate checkout/worktree on story/codex-content-20261117 from that integrated base, or inspect/resume the correct existing branch. Never reset dirty work, copy the older Codex Greenhollow WIP into it or edit Antigravity's checkout. Record the actual base/source and current ownership; no new eight-hour timer is implied.

Own SC00→SC01→SC02 agreement→SC03: verify the canon/actual actor and state inventory, publish the minimal catalog contract, then author the first Abe/Rowan/Mira/Elara/Selvek content pack under that schema. Give exact legacy-name mappings, line IDs, conditions/priority, personal/public/encounter knowledge and safe fallback text. Abe's actual PlayerProgress quest descriptions must be included in the content proposal. Selvek remains an Undead/Rogue enemy with current defeat/lockbox rules, not a new TownNPC or surrender branch.

I authorize the new narrative content data and read-only validators required by this slice after the common schema is agreed. Use Assets/RPG/Resources/Narrative/** for runtime content only after the adapter owner accepts the schema; preserve existing metadata/IDs. Do not introduce arbitrary-code dialogue expressions, a second quest/state framework, a paid provider or persistent flags disguised as text. Derive the first states from existing progress; version any genuinely new state through an explicit contract.

Write NPC copy and contracts; do not edit NPCGossipWindow.cs, TownNPC.cs, Merchant.cs, QuestGiver.cs, PrototypeHUD.cs, PlayerServices.cs, PlayerProgress.cs, FactionQuestSystem.cs, combat/runtime/network or LivingWorldEvents.cs while Antigravity owns them. Runtime adapter changes belong to Antigravity. If a schema/API gap blocks data, send a concrete field/example and continue independent profiles/read-only diagnostics.

Prepare SC08/SC10 and CF11 as exact existing-asset work/encounter/clue proposals along Emberwatch→Highspire→Westbank→Reaver and Emberwatch→Greenhollow. No unreviewed scene/terrain/prefab application, actor recreation, world regeneration, mass vegetation removal or new tile. Preserve813trees/15prototypes and current protected terrain. Use authored clues to support current gameplay, not new reward or collection flags.

Validate JSON/schema/identity uniqueness/condition references and contradiction cases. Compile any editor/tool C# change and run its relevant checks; docs-only checks are not gameplay. Record actual changes/results/NOTRUN in Docs/Production/OWN_CODEX_STORY_RUN.md and HANDOFF.md, along with exact content/schema version to hand Antigravity. Checkpoint/push only this branch; no main merge.

Maximum two unchanged failed attempts; after ten minutes without new evidence change hypothesis/independent task. No repeated green tests or unbounded waits. Deliver the usable content pack and handoff before expanding beyond the five-NPC slice.
```

## Send this to Antigravity

```text
Start Riftbound's combat-and-NPC-runtime refinement from the latest published integration/combined-20261116, including Docs/Integration/NEXT_WORK_PROMPTS.md. Use the integrated runtime/world, not your older production branch or a copied scene.

Read applicable AGENTS.md, Docs/Integration/INTEGRATION_HANDOFF.md, ONLINE_TEST_RESULTS.md, POST_INTEGRATION_PLAN.md sections13–14, Docs/Design/COMBAT_REFINEMENT_PLAN.md and Docs/Lore/RIFTBOUND_LORE_BIBLE.md plus NPC_STORY_CONTRACTS.md. Preserve exact faction/race/NPC/quest/enemy/chest keys, authority, rewards, offline saves and the session-only online policy. Source-risk observations are hypotheses to reproduce, not permission to rewrite working systems.

Create a separate checkout/worktree on combat/antigravity-refinement-20261117 from that latest integrated base, or inspect/resume the correct existing branch. Never reset dirty work or edit either old agent checkout. Preserve all finished builds/branches. Record base/source and actual start; no eight-hour timer is implied.

AG00: close the finalCombined02 incomplete coverage with the exact current executable/source: authority, progression, latejoin, fullacceptance, clientleave/rejoin and4player+capacity. Keep actual grounded entry/respawn/rejoin evidence and independent32spawn regression. Candidate01's10/10 is superseded; final02 completed onlyUI/invalidcode/resilience/combat before userstop. Use unique profiles/private logs, redact join codes/tokens and do not call same-machine Relay realtwoPC acceptance. If source changes, build a new unique candidate and rerun affected cases.

The known existing candidate is C:/Users/skama/Documents/wow=rpg-diplomna/Integration/combined-20261116/Builds/RiftboundCombined02/Riftbound.exe, runtime source76696a7. You may execute that artifact read-only for baseline checks while saving new logs/results in your own worktree. Resolve runner/build paths deliberately; do not assume the new checkout contains ignored build outputs or overwrite old evidence. Documentation-only commits do not require rebuilding unchanged runtime merely to finish this baseline.

AG01: work CF00–CF04 first. Reproduce exact class resource/cooldown/range discrepancies, Mage interrupt range/cooldown, local/remote enemy LOS/projectile differences, pre-accept hit feedback and proposed boss immunity/death edge cases from the source-risk table. Fix only demonstrated defects with focused rejection/parity tests. Preserve existing abilityIDs, legal damage scaling and one-hit/one-reward authority. Never weaken assertions, grant client HP/damage authority or change the global damage floor without testing intended immunity semantics.

AG02: coordinate SC02 schema with Codex, then implement SC04/SC05/SC07 and the first stationary behavior sliceSC09 using the approved five-NPC content. You own NPCGossipWindow/TownNPC/Merchant/QuestGiver/PrototypeHUD/PlayerServices/PlayerProgress/FactionQuestSystem and any new Scripts/Narrative adapter. Reuse existing windows and deterministicE selection; replace broad-name/role lore routing with exact profile lookup and safe fallback. Read player-owned state without spoilers, new rewards or shared save corruption. Codex owns external content files; ask for a precise field/content correction instead of editing its catalog concurrently. Keep services reachable and pause ambient work during interaction; no new NavMesh framework.

AG03: continue CF05–CF14 in one measured reference encounter before boss/class/content expansion. Match action windup/contact/recovery and supported animations, test proposed input buffer/cancel rules, readable raider/warden/hexer intent, dodge/block/interrupt fairness, spacing/LOS/leash and accepted-impact sound/VFX. Tune one dimension at a time from baseline; proposed numerical/latency targets are not measured facts. No global network timeScale hitstop, invisible success feedback or HUD clutter.

I authorize these focused runtime/NPC/combat corrections and existing-asset development fixtures in your isolated branch. Do not edit world scenes/terrain/road/nature/environment prefabs, source FBX/avatars/Animator assets or Codex content. Scene/asset requests need exact IDs/transforms/footprints and coordinated approval. Do not add the fifth class, PvP/backend or replace the roster during this slice.

Verify compilation, preserve145quest/41gameplay/32spawn assertions, run changed-behavior offline/host/client/latejoin checks and native visual/input tests when available. Distinguish source inspection, synthetic/native probes, queuedinput, headlessRelay and manualtwoPC. Finish a new uniquely named standalone with exact source/checks/remainingNOTRUN. Record Docs/Production/OWN_ANTIGRAVITY_COMBAT_RUN.md and HANDOFF.md; checkpoint/push only your branch, no main merge.

Maximum two unchanged failed attempts; ten minutes without new evidence requires a different hypothesis/independent task, twenty-minute unchanged-blocker investigation cap. No blind restarts or loops on one network scenario. Reserve time for regression/build/handoff; do not expand scope just to consume tokens.
```

## Required exchange before implementation overlaps

| Handoff | Sender → receiver | Must contain |
| --- | --- | --- |
| Catalogv1 contract | Codex ↔ Antigravity | Example profile/line, exact field types, supported existing-state predicates, priority/fallback, identity aliases, runtime load path and schema version. Both explicitly agree before data/parser implementation. |
| First content pack | Codex → Antigravity | Five profiles, actual legacy keys and state fixtures, authored lines, invalid/missing-profile cases, no hidden quest/reward changes. |
| State gap | Antigravity → Codex | Exact missing read-only getter/condition, viewer/public ownership, proposed default and example; no guessed flags interpreted as true. |
| Encounter request | Antigravity → Codex | Existing actor/archetype IDs, required arena/approach/retreat/LOS, collision and camera envelope; no silent scene change. |
| Ready for integration | Each → integrator | SourceSHA, changed paths, schema/content version, compile/tests actually run, failed cases/NOTRUN, unique build and explicit scene release when applicable. |

Integrate completed packets in a clean branch, preserve the two workers' files and run dependent checks on that exact combined candidate. Planning completion does not pass gameplay. Human playtest and real two-PC acceptance remain their own gates.

## Authoring receipt

Validated 17 November 2026:25 NPC dossiers; all35 existing FactionQuest definition IDs represented in the identity/state contracts;16 unique SC packets,8 unique WL packets and24 unique CF packets; required files present and fenced blocks balanced. Whitespace checks passed. These are document/source-reference checks. No Assets/Packages/ProjectSettings file changed and no Unity compilation/gameplay test was run for this documentation-only update. Combined02 remains the prior playable build with its disclosed remaining checks.
