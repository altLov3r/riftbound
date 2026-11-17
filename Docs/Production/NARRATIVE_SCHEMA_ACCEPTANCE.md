# Narrative Catalog Contract v1 — Adapter Owner Acceptance (`SC02`)

- **Decision:** **ACCEPTED**
- **Adapter Owner:** Antigravity (`combat/antigravity-refinement-20261117`, worktree `Combat/antigravity-refinement`)
- **Reviewed Branch / Commit:** `origin/story/codex-content-20261117` (`86051239bfb0ed9250b67955035c0cf6fa5c5b9d`; frozen identity commit `3fc0dcf4be005a6ef4e7d23f74f74555d0a1006f`, based on `8ad9071820c2fbed32fbb728122ffb2f02980888`)
- **Runtime Load Key (authorized upon Codex promotion):** `Assets/RPG/Resources/Narrative/first-five.v1.json` loaded read-only via `Resources.Load<TextAsset>("Narrative/first-five.v1")`

## Reviewed Artifacts & SHA256 Identity

Exact UTF-8 LF SHA256 digests matching `Docs/Production/Story/schema-proposal-identity.json`:

| Artifact | SHA256 (UTF-8 LF bytes) |
| --- | --- |
| `Docs/Production/Story/CATALOG_CONTRACT_V1.md` | `8d88fd16f18042a793acf1d8d0f7e4b6b33f12a8f53b10080dc95410a65d971d` |
| `Docs/Production/Story/first-five.schema.json` | `a894d204bfd497acb2dd986fffa3d0804db2ea04bc5cae8ba7b46ea850b24e10` |
| `Docs/Production/Story/first-five.v1.proposed.json` | `9b7e38f26e1153a45c6ce0032ede3dd39acc536e0ba09594561de06e7c5b7f73` |
| `Docs/Production/Story/selection-fixtures.json` | `adf636845713368f289d43626fd1ef119834eb471db7d7b8d844dc50ef025c3e` |

> Also compatible with `first-five.v1.proposed.json` topics (`roadward-truce`, `road-work`, `supplies`, `receipts`, `road-crews`, `fair-trade`, `cartography`, and Selvek `encounter_bark` topic `"aggro"`). When the Resource JSON is absent before Codex promotes it, the adapter falls back safely to built-in v1 default five-NPC copy and operational game-state strings without modifying quest/economy state.

## Supported DTO, Validation & Selection Rules

- JsonUtility-compatible fixed DTO arrays/structs matching `first-five.schema.json` (`Catalog` -> `Profile` -> `Entry` -> `Condition`).
- Strict load validation: rejects non-`1` `schemaVersion`, mismatched `catalogId`, unknown `actorKind`/`channel`/`scope`/`field` values, and unclosed conditions.
- Deterministic entry resolution for `(profile, channel, topicId, viewer)`:
  1. Filter exact `channel`, `topicId` (or empty when non-topic channel), and `scope` eligibility (`viewer` requires non-null `viewer`, `host_encounter` requires active encounter context, `public` requires no viewer/quest condition).
  2. Evaluate every condition in `conditions` as logical `AND`. Any missing context resolves that predicate to `UNKNOWN` and fails the candidate closed (never coerced to `false`/`0`, including when `boolValue == false`).
  3. Order matching entries by highest `priority` (`99..0`), then `lineId` `StringComparison.Ordinal` ascending.
  4. On zero matching entries: return `profile.fallbackText` for `greeting`/`public_topic`, return `null` (preserving current operational UI text) for `main_title`/`main_body`/`quest_*` and `encounter_bark`.

## Supported Channels (v1)

| Channel | Consumer Surface | Read-Only Guard |
| --- | --- | --- |
| `greeting` | `TownNPC` greeting bark / `NPCGossipWindow` header, and `AdventurePanels` shop header (`Merchant`) | Never creates competing `[E]` prompts or blocks shop trade |
| `main_title` | `PrototypeHUD.DrawQuestWindow` (`Abe` / `QuestGiver`) | Presentation over live `player.QuestName`; does not mutate `QuestIndex` |
| `main_body` | `PrototypeHUD.DrawQuestWindow` (`Abe` / `QuestGiver`) | Presentation over live `player.QuestDescription` (`offer`/`accepted`/`ready`/`complete`) |
| `quest_offer` | `NPCGossipWindow` (`Cartographer Elara`, `topicId = quest.id`) | Preserves `FactionQuestSystem.GetAvailableQuestForNPC` eligibility |
| `quest_progress` | `NPCGossipWindow` (`topicId = quest.id`) | Active-not-ready FactionQuest state only |
| `quest_ready` | `NPCGossipWindow` (`topicId = quest.id`) | Turn-in buttons still invoke `FactionQuestSystem.CompleteActiveQuest` |
| `quest_after` | `NPCGossipWindow` (`topicId = quest.id`) | Personal aftermath where `quest.completed` is true |
| `public_topic` | `NPCGossipWindow` topic buttons (`topicId`) | Literal plain text only; no automatic private lore or persistent memory |
| `encounter_bark` | `EnemyController` hostile engage bark (`westbank-reaver-captain`) | Transient read-only combat bark only; Selvek remains hostile (`SC01`/`SC02`), no gossip UI or threat changes |

## Closed Predicate Set & Per-Viewer Mapping

| Field | Key / Value | Consumer Resolution |
| --- | --- | --- |
| `main.index` | `""`, `intValue` (`0..3`) | `viewer != null` -> `viewer.QuestIndex == intValue` (`UNKNOWN` if `viewer == null`) |
| `main.accepted` | `""`, `boolValue` | `viewer != null` -> `viewer.QuestAccepted == boolValue` |
| `main.ready` | `""`, `boolValue` | `viewer != null` -> `viewer.QuestReady == boolValue` |
| `main.complete` | `""`, `boolValue` | `viewer != null` -> `viewer.QuestComplete == boolValue` |
| `quest.available` | quest ID, `boolValue` | Requires `viewer != null` and viewer-owned `FactionQuestSystem`; compares `(fqs.GetAvailableQuestForNPC(profile.legacyName, viewer)?.id == key)` |
| `quest.active` | quest ID, `boolValue` | Requires viewer-owned `FactionQuestSystem`; compares `(fqs.ActiveQuestId == key)` |
| `quest.ready` | quest ID, `boolValue` | Requires viewer-owned `FactionQuestSystem`; compares `(fqs.ActiveQuestId == key && fqs.IsActiveQuestReady())` |
| `quest.completed` | quest ID, `boolValue` | Requires viewer-owned `FactionQuestSystem`; compares `fqs.IsQuestCompleted(key)` |
| `enemy.alive` | `"westbank-reaver-captain"`, `boolValue` | Requires resolved `EnemyController` matching exact `enemyId` (via `EnemyController.AllByKey` / scene lookup); evaluates `enemy.GetComponent<Combatant>().Alive == boolValue`. Returns `UNKNOWN` if actor reference is unavailable (never assumes `false`). |
| `enemy.defeated` | `"westbank-reaver-captain"`, `boolValue` | `viewer` scope only; `viewer != null` -> `viewer.HasDefeated(key) == boolValue` |

### Per-Viewer Host / Client Guarantee
- UI and NPC surfaces pass the local interacting `PlayerController` (`viewer`) directly (`PrototypeHUD`, `AdventurePanels`, `NPCGossipWindow`).
- `FactionQuestSystem` is resolved exclusively from `viewer.GetComponent<FactionQuestSystem>()` (matching `FactionQuestSystem.Ensure(player)` ownership on both host and remote clients). It never substitutes `FactionQuestSystem.Instance` across different players; missing `viewer` or `FactionQuestSystem` yields `UNKNOWN` for every `viewer`-scoped predicate.

## Actor-Identity Precedence (Exact `OrdinalIgnoreCase`, Zero `Contains`)

1. **Shop surface (`Merchant`):** Exact `merchant.npcName` (`Quartermaster Rowan`, `Provisioner Mira`) -> `actorKind == "merchant"`. Never casts to `TownNPC` or adds pre-trade dialogue gates.
2. **Main quest surface (`QuestGiver`):** Exact `questGiver.npcName` (`Abe`) -> `actorKind == "main_quest_giver"`. Preserves precedence over companion `TownNPC` if co-located.
3. **Gossip / Faction NPC surface (`TownNPC`):** Disallowed if GameObject also hosts `QuestGiver` on quest window path; matches exact `townNpc.npcName` (`Cartographer Elara`) -> `actorKind == "faction_quest_giver"`. Unknown TownNPCs fall back cleanly to safe neutral non-spoilery lines using `townNpc.roleTag` / `townNpc.greeting`, never copied Amber Vale / Bilgewater / Kirin Tor / Forsaken strings.
4. **Hostile encounter surface (`EnemyController`):** Exact `enemy.enemyId` (`westbank-reaver-captain`) -> `actorKind == "hostile_enemy"`. `encounter_bark` channel only.
