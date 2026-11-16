# First-five narrative catalog — proposed consumer contract v1

SC02 proposal. **PENDING actual Antigravity adapter-owner acceptance.** This document and its example/catalog/schema are staged authoring artifacts outside Assets. An internal review, user starting the other worker or a valid JSON file is not adapter acceptance. Do not promote runtime content until the owner records this exact contract/schema hash as accepted or explicitly proposes a revised common version.

## Scope

One read-only catalog supplies replacement text to current surfaces. It does not select gameplay actions, pay rewards, change quest eligibility, add save flags, move actors or make Selvek friendly. Five profiles: Abe(main quest giver), Quartermaster Rowan(merchant), Provisioner Mira(merchant), Cartographer Elara(faction quest giver), Captain Selvek(hostile enemy).

Proposed runtime path after acceptance: Assets/RPG/Resources/Narrative/first-five.v1.json, loaded as TextAsset via Resources key Narrative/first-five.v1. Consumer code belongs to Antigravity; content/metadata belongs to Codex. Until accepted, authoritative candidate is Docs/Production/Story/first-five.v1.proposed.json. No runtime reader exists in this workstream.

## DTO shape

JSON objects use fixed fields and arrays, suitable for ordinary Unity serializable DTOs; no map/dictionary, polymorphic expression tree, executable condition or dependency is required.

The example below is a one-profile shape excerpt, not a complete passing fixture. The actual schema requires all five profiles; validate first-five.v1.proposed.json rather than this abbreviated excerpt.

```json
{
  "schemaVersion": 1,
  "catalogId": "riftbound.first-five.v1",
  "profiles": [{
    "profileId": "abe",
    "actorKind": "main_quest_giver",
    "legacyName": "Abe",
    "enemyId": "",
    "fallbackText": "Keep the road clear, and come back safely.",
    "entries": [{
      "lineId": "abe.main0.offer",
      "channel": "main_body",
      "scope": "viewer",
      "topicId": "",
      "priority": 50,
      "conditions": [
        {"field":"main.index","key":"","boolValue":false,"intValue":0},
        {"field":"main.accepted","key":"","boolValue":false,"intValue":0},
        {"field":"main.complete","key":"","boolValue":false,"intValue":0}
      ],
      "text": "The road carries our flour and medicine. Defeat three forest raiders along the north road, then return to me. I will see that the work is paid."
    }]
  }]
}
```

All fields shown are required; unused key is empty, unused boolValue is false and unused intValue is0. This avoids implicit false/zero values being mistaken for authored intent by a permissive deserializer. Import/content validation is strict. Catalog/condition/schema versions must match; unknown keys/fields/channels/kinds/predicates reject an authoring build, and the runtime consumer fails safely without changing progression. Game-state data absent from the viewer context is **UNKNOWN**, never automatic false/zero.

Profile IDs and line IDs are content identifiers only, not new NPC/quest/save IDs. legacyName is exact existing component identity. Matching may be OrdinalIgnoreCase, but no Contains/role/race guess. ActorKind disambiguates component roles; a Merchant's authoritative identity is its Merchant.npcName, a QuestGiver's its QuestGiver.npcName, Elara's TownNPC.npcName, Selvek's EnemyController.enemyId. The actor inventory must disclose conflicting companion-component names; do not silently bind a cloned merchant to the wrong profile.

## Channels and selection

| Channel | Existing consumer intent / guard |
| --- | --- |
| greeting | Current eligible greeting; never creates a competing E path |
| main_title | Replacement PlayerProgress.QuestName presentation for Abe's actual index; does not change the underlying index/name key |
| main_body | Replacement PlayerProgress.QuestDescription presentation, including offer/accepted/ready/complete branches; not just QuestGiver serialized fields |
| quest_offer | Current eligible FactionQuest description; topicId is its existing quest ID |
| quest_progress | Active-not-ready FactionQuest response; topicId exact quest ID |
| quest_ready | Already eligible ready-to-turn-in response; topicId exact quest ID; current authorized action alone grants completion/rewards |
| quest_after | Optional personal response after existing completion; topicId exact quest ID |
| public_topic | Explicitly selected optional public topic; no new heard/confession flag or automatic private lore |
| encounter_bark | Existing hostile encounter hook only; never a Selvek gossip menu or new threat/aggro rule |

Selection input is a resolved eligible actor, requested channel/topic and that interacting viewer's read-only context. Existing modal/dead/range/revive/hostile/action checks run first and stay outside content. Among entries matching all predicates, select highest priority, then lineId Ordinal ascending for deterministic safety. Authoring validation rejects equal-priority overlapping entries in the same profile/channel/topic; deterministic tie behavior is a fallback, not authoring practice. No cross-channel race where a greeting replaces a quest body.

No match: for greeting/public_topic use that profile's safe fallbackText; for main/quest surfaces leave the current operational text/title intact. For encounter_bark return no line and preserve existing combat behavior. Unknown actor/profile: safe neutral greeting and existing service options; do not re-enter copied broad-name/role lore branches. Catalog errors cannot enable interaction or hide a legal service.

First-slice content channels are deliberately limited: Abe=greeting/main_title/main_body/public_topic; Rowan/Mira=greeting/public_topic; Elara=greeting/quest_offer/quest_progress/quest_ready/quest_after/public_topic for frontier_cartography and river_reaver_bounty; Selvek=encounter_bark only. The source's separate Abe faction dispatch remains unchanged rather than acquiring a new main-chain prerequisite. Main title text retains the four actual PlayerProgress.QuestName values. Any broader profile/channel combination needs a reviewed scope/version, not an authoring workaround.

## Closed condition set and existing source mapping

Conditions are conjunctions(AND). There are no operators, variable interpolation, OR expressions, actions or persistent writes. Alternatives use separate entries. Text is literal plain text, with newline allowed; no rich-text tags, dynamic script fragments or unimplemented coordinate substitution in v1.

| field | key/value | Required read-only mapping |
| --- | --- | --- |
| main.index | key empty, intValue0..3 | Interacting PlayerController.QuestIndex |
| main.accepted | key empty, boolValue | Same viewer.QuestAccepted |
| main.ready | key empty, boolValue | Same viewer.QuestReady; do not recalculate counts differently |
| main.complete | key empty, boolValue | Same viewer.QuestComplete |
| quest.available | existing FactionQuest ID, boolValue | GetAvailableQuestForNPC(legacyName,viewer)?.id equals key, preserving its existing faction/prerequisite/one-active rules |
| quest.active | existing FactionQuest ID, boolValue | Viewer-context FactionQuestSystem.ActiveQuestId equals key |
| quest.ready | existing FactionQuest ID, boolValue | Exact active ID AND that context.IsActiveQuestReady(); never any other player's singleton |
| quest.completed | existing FactionQuest ID, boolValue | That viewer-context IsQuestCompleted(key) |
| enemy.alive | westbank-reaver-captain, boolValue | Resolved actual encounter EnemyController/Combatant life state; false is UNKNOWN if the actor is unavailable, not proof of defeat |
| enemy.defeated | westbank-reaver-captain, boolValue | Viewer.HasDefeated(key) for personal knowledge; only viewer-scope entries may consume it in this slice |

main.* and quest.* conditions require scope viewer; enemy.defeated also requires viewer. Public lines are unconditional public facts. host_encounter may use enemy.alive only and must come from the existing host-owned encounter; it does not synchronize personal journals. No new Mara/Seventh Wagon/relationship-trust predicate exists. Those private revelations remain held outside runtime content.

Consistency invariant: a completed main chain is index3/not accepted/not ready; ready implies accepted and index0..2. A faction quest cannot be completed and active/available/ready at once; ready implies its exact active ID. Missing context fails the candidate, including a predicate asking for false. Reject contradictory predicates, wrong-value types and unknown referenced IDs before integration.

## API/behavior decision requested from Antigravity

Existing APIs suffice for the first quest predicates, but the adapter owner must confirm a per-viewer context on host/client and exact Merchant-vs-TownNPC identity precedence. Parent's singleton may represent the host; blindly passing it for a remote viewer is not accepted. If the consumer cannot expose a predicate truthfully, return UNKNOWN and request a concrete contract revision rather than guessing or advancing state.

SC01 also found an actual surface gap: Rowan and Mira are saved Merchant components without TownNPC, and Merchant currently has no greeting field/purchase history. Their v1 greeting needs an accepted read-only surface in the existing shop header, not a cast to TownNPC or a new mandatory dialogue before trade. No purchase-success predicate or line is included. Abe's main_body/main_title need a presentation hook over actual PlayerProgress properties; changing only serialized QuestGiver strings does not implement them. Please accept these consumer hooks explicitly or propose the supported channel revision.

Purchase-success lines, actor relocation, routine animation triggers, private testimony memory and newly persistent flags are deliberately excluded from v1 data. They can be proposed later with an existing accepted-event hook/schema revision. Selvek only receives an encounter bark; Elara's personal aftermath uses the existing completed bounty. No speculative friendly meeting or capture outcome is implied.

## Acceptance and promotion

1. Codex publishes this contract, JSON schema, complete staged pack and read-only authoring scenarios with source identity.
2. Actual adapter owner records ACCEPTED or REVISION REQUESTED in its own runtime journal or Docs/Production/NARRATIVE_SCHEMA_ACCEPTANCE.md, with exact contract/schema SHA256, supported channels/predicates, per-viewer mapping and actor-identity precedence. Codex cannot sign on its behalf.
3. Any requested schema change is resolved once and version/hash updated; contradictory local forks are not a shared contract.
4. After matching acceptance, Codex may promote exact accepted content to the authorized Resources path, preserving metadata and validating byte identity. Antigravity implements the single adapter in its own files. No consumer acceptance yet means staged delivery only, not a fake installed feature.
