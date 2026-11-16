# Codex narrative content — staged delivery

## Identity and status

- Branch: **story/codex-content-20261117**.
- Checkout: C:/Users/skama/Documents/wow=rpg-diplomna/Production/codex-story-20261117.
- Exact published integrated base:8ad9071820c2fbed32fbb728122ffb2f02980888.
- Validated content/portable-identity checkpoint: **3fc0dcf4be005a6ef4e7d23f74f74555d0a1006f**. This handoff follows in a documentation-only commit.
- Start00:10:11, final handoff approximately00:38 Europe/Sofia,17 November2026. No eight-hour timer reset or new shift claim.
- No main merge, original checkout edit, old Greenhollow WIP copy, original asset overwrite, save mutation or Unity process.

**Delivered: usable authored/proposed content plus validation and precise implementation contracts. Not delivered: runtime admission or functioning NPC adapter.** Actual Antigravity schema acceptance is still pending. The user's rule requires owner acceptance before Assets placement, so no content was promoted into Assets/RPG/Resources/Narrative and no runtime file was edited.

## Packet outcome

| Packet | Actual state |
| --- | --- |
| SC00 | PREPARED: author truth/official claim/witness/rumour/reveal table with private knowledge withheld where no current predicate exists |
| SC01 | AUDITED: selected saved1QuestGiver/2Merchant/37TownNPC component records, precise first-five bindings and runtimeEnsure paths; not40 total NPCs |
| SC02 | PROPOSED / PENDING actual adapter-owner acceptance of exact contract/schema hashes |
| SC03 | AUTHORED / VALIDATED / STAGED outside Assets:5profiles,37lines; final admission depends onSC02 and the owner's consumer implementation |
| SC08/SC10/CF11 | PREPARED: exact existing assets/anchors/work/clue/encounter proposals; no placement/routine/encounter applied |

## Usable pack

Folder: **Docs/Production/Story/**; README.md is the entry point.

- first-five.v1.proposed.json: content versionriftbound.first-five.v1/schemaVersion1. Exact Abe/Quartermaster Rowan/Provisioner Mira/Cartographer Elara/Captain Selvek identities. Includes actual Abe PlayerProgress main_title/main_body offer/progress/ready/completed branches, merchant public service text, the existing Elara survey/bounty states and one hostile Selvek bark.
- first-five.schema.json + CATALOG_CONTRACT_V1.md: fixed arrays/typed fields, exact identities, AND-only supported existing predicates, UNKNOWN fails closed, priority/fallback/scope and strict no-side-effect rules. Plain literal text; no executable expressions, new state flags or new framework.
- schema-proposal-identity.json: exact UTF-8 LF SHA256 for contract/schema/catalog/scenarios, matching committed Git bytes. StatusPENDING, runtimePromotionAllowedfalse. Starting another prompt does not authorize toggling that flag.
- selection-fixtures.json + validate_story_pack.py + test_story_pack.py: portable Python-standard-library read-only authoring checks/reference selection/rejection tests, not installed consumer code.
- ACTOR_STATE_INVENTORY.md + CANON_REVEAL_MATRIX.md: exact source field/ID/API/knowledge boundaries. Main boss-defeat flag refers to the Ashwood Captain, not Selvek. No new Mara/heard-testimony/choice/purchase-history predicate invented.
- WORLD_STAGING_PROPOSALS.md: source anchors, five existing supply wrappers, six model GUID/hash/license records, six clue-surface proposals, stationary attention/work limits, route/camera/clearance requirements and existing-archetype three-role/Selvek encounter contract. Source certainty distinguished from native NOT RUN.

Copy remains operational: three road raiders, three supplies, existing main captain; current reward/eligibility paths untouched. Elara's survey uses the existing3 sites and bounty uses westbank-reaver-captain. Westbank bridge/Willow Rest are not the northern willow-crossing discovery marker. Selvek remains hostile Undead/Rogue; no conversation/recruit/surrender or duplicate cache outcome.

## Actual validation

| Check | Result |
| --- | --- |
| Proposed JSON structure/schema vocabulary/source references/identities/conditions | PASS:5profiles/37entries, existing35-quest source catalog checked; only existing in-slice references used |
| Reference-selection fixture matrix |62 PASS, including main states, exact faction states, unknown-not-false, unearned aftermath, invalid context, no friendly Selvek and safe fallback |
| Rejection/regression unit tests |24 PASS; unknown predicates/IDs, types, duplicates/ambiguous lines, contradictory state, reward side effects, private/public mismatch, withheld text, malformed/nonfinite JSON and title identity |
| Python byte compilation | PASS for validator/tests |
| Portable artifact identities |4/4 raw committed Git bytes and working bytes match proposed hashes; CRLF authoring identity corrected without changing parsed JSON |
| Whitespace | PASS |
| Protected source identity | Saved scene matches base Git blob; all10 tracked expansion/Reaver terrain SHA256 values match base Git LFS OIDs; receiptsource-protection.json |
| Runtime/asset delta | None under Assets/Packages/ProjectSettings relative to base |

The tests above are **authoring/reference-model checks**, not proof of a Unity predicate adapter, actual UI, gameplay, network isolation or rendered NPC behavior. Existing145/41/32 runtime passes belong to the earlier Combined02 delivery; they are not new passes for an unimplemented consumer. No C# changed, so no Unity compilation or new standalone was run for this content-only branch.

## Concrete Antigravity acceptance request

Read **Docs/Production/Story/SCHEMA_ACCEPTANCE_REQUEST.md** from this branch and compare the frozen hash manifest. Please record ACCEPTED or an exact revision request in the runtime owner's own branch/journal or Docs/Production/NARRATIVE_SCHEMA_ACCEPTANCE.md, with contract/schema hashes and supported channels/predicates.

Two real consumer boundaries to resolve: merchants have no TownNPC/greeting field and need an agreed read-only existing-shop surface; Abe's live text is PlayerProgress, so serialized QuestGiver prose alone does not implement the catalog. Confirm per-viewer FactionQuest context and component-role identity precedence. Do not use another player's singleton, treat unknown as false, revive hostile actors, grant rewards from displaying prose or quietly extend persistent flags.

After matching owner acceptance: promote exact accepted JSON to the agreed Resources path with metadata preserved; Antigravity implements/compiles its adapter and runs relevant145/41/32/reward/UI/host-client/late-join checks. If schema changes, revise/version and revalidate one common contract first. Any scene/readable/routine/encounter placement still needs its specific current review and scene ownership; no whole-world generation.

## NOT RUN / next action

NOT RUN: actual adapter/schema acceptance, runtime catalog loading, NPC text changes in game, merchant-header UI, hostile bark replication, private viewer-state isolation, fresh native actor enumeration, work clips/sockets, readables/support/collision/camera, new traversal/encounter/gameplay/online or standalone checks. These proposals do not resolve the six interrupted finalCombined02 online scenarios.

Next action is the exact runtime-owner schema decision above. All independent five-NPC content, read-only checks and scene-free staging proposals are ready; no token-consuming wait or speculative expansion is in progress. **SCENE RELEASED — own story worktree only; no scene lock/write or native process was taken.**
