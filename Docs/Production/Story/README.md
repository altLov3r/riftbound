# First-five narrative content pack — staged proposal

Base: integrated8ad9071. Branch: story/codex-content-20261117. Content version: riftbound.first-five.v1, schemaVersion1. **Not installed:** actual Antigravity schema acceptance and its runtime adapter are pending. Nothing has been placed in Assets/RPG/Resources/Narrative and no game code/scene/save/reward was changed.

## Files

- CATALOG_CONTRACT_V1.md: exact DTO/channels/predicates/identity/unknown-context/promotion contract.
- first-five.schema.json: strict structural JSON schema; validator supports its explicitly bounded vocabulary without dependencies.
- first-five.v1.proposed.json:5 profiles/37 authored lines, including actual Abe PlayerProgress title/body branches, merchant text, Elara's two existing quests and Selvek's hostile bark.
- selection-fixtures.json:62 reference-selection cases. These are authoring scenarios, not an installed Unity adapter.
- validate_story_pack.py / test_story_pack.py: read-only format/source/identity/condition/contradiction checks and24 rejection/regression tests.
- ACTOR_STATE_INVENTORY.md: exact saved/runtime identity evidence, current UI surfaces, APIs/counts/rewards and limits.
- CANON_REVEAL_MATRIX.md: author truth versus claims/rumour, viewer knowledge and withheld revelations.
- WORLD_STAGING_PROPOSALS.md: exact existing props/anchors/IDs/provenance and proposed work/clue/encounter contracts; no scene application or new traversal/visual pass.
- SCHEMA_ACCEPTANCE_REQUEST.md and schema-proposal-identity.json: adapter-owner handoff and frozen artifact hashes; neither is an acceptance signature.

## Run from this repository root

```text
python Docs/Production/Story/validate_story_pack.py --fixtures
python -m unittest discover -s Docs/Production/Story -p test_story_pack.py
```

Python standard library only. Commands read the staged data/schema and named source files. Unit tests mutate in-memory copies and use their own temporary invalid-JSON file; they do not write game assets or saves. No Unity/C# compilation is required because no C# was changed. Schema/reference checks do not prove a runtime predicate, native NPC gesture, eligibility/range check or co-op implementation.

## Consumer work required

Abe's live body/title come from PlayerProgress, not merely QuestGiver serialized prose. Rowan/Mira are merchants without TownNPC/greeting fields: use an agreed read-only existing shop surface. Elara is runtime-created and has35-catalog-backed existing APIs; her quest predicates use this viewer's actual state. Selvek is runtime-created hostile Undead/Rogue; there is no gossip/surrender branch. Refer to the inventory rather than assuming saved scene presence or a component from the name.

Actual adapter owner must accept the exact contract/schema hashes in its own runtime journal or NARRATIVE_SCHEMA_ACCEPTANCE.md. Starting the other prompt, silence or an internal Codex review does not count. If unsupported, name the field/channel and proposed replacement. Codex can then version/amend the draft and rerun authoring checks. Do not copy a mutually inconsistent parser/data pair into the game.

Proposal hashes use exact UTF-8 LF bytes matching Git attributes. The manifest is portable across Windows/fresh checkouts; do not hash an editor-transformed CRLF copy and call it the accepted artifact. Predicate/source validation remains independent of this byte-identity gate.

After acceptance, promote the exact accepted JSON to the agreed Resources path with metadata preserved; Antigravity alone implements adapter/runtime changes. Existing145/41/32 and relevant final-candidate Relay checks remain runtime integration gates; no historical pass is transferred to unimplemented dialogue.
