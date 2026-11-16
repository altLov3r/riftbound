# First-five actor and state inventory — SC01

Read-only inspection on17 November2026, source `8ad9071820c2fbed32fbb728122ffb2f02980888`, branch `story/codex-content-20261117`. Scope is **five identities**, selected saved component categories and their directly used sources. It is not an all-world actor inventory or gameplay acceptance. No Unity process, parser/catalog/schema code, asset import, scene save or terrain mutation was performed for this document. No adapter-owner schema acceptance has been received or inferred.

## 1. Sources and counting limits

Scene: `Assets/RPG/Scenes/Outpost-Expanded.unity`, metadata GUID `d9531279b321b004fbcdf8961c6617f8`. Streaming `rg` searches matched exact script GUIDs, Unity record headers, component references and selected name/ID fields; bounded source reads checked the consumers. Counts below concern explicit records in this saved YAML, not active hierarchy totals, spawned actors, resolved prefab descendants or every `MonoBehaviour` in the project.

| Selected script/category | Metadata GUID | Saved records |
| --- | --- | ---: |
| `QuestGiver.cs` | `c41dab07fc4e11c4ebe64bc708047082` |1 |
| `Merchant.cs` | `cd973004245351b4aa7ceae909b2be54` |2 |
| `TownNPC.cs` | `18d5b6dbccaffc34bbaa7eaf595a1446` |37 |

Thus40 component records were counted across these three selected categories. They are **not40 total NPCs/actors**. For the five requested identities, exact saved `npcName`/enemy-ID matches are Abe1, Rowan1, Mira1, Elara0 and Selvek0. The last two have explicit runtime creation paths; absence from saved YAML is not a missing-actor defect.

`EnemyController.cs` metadata GUID is `76558e0a6bc4324489fa99aeafb15793`; it is relevant to Selvek's runtime-created component, not evidence of a saved Selvek record. Other source metadata: `FactionQuestSystem.cs`=`7a8e9f12bc434251a81239cd55910bf2`, `ExplorationSites.cs`=`dfba15a5c0474ce1b7bf7289395dff7d`, `RiverReaverCamp.cs`=`538ffdf346054197a6beff18ce1d91f8`, partial `PlayerProgress.cs`=`8e6c9b02248146c4ba3ab86ec240cf41`. A static class or partial source-file GUID is not automatically a serialized component script reference.

## 2. Saved actor presence, precise field paths and anchors

Unity record notation below identifies `classID &fileID`; field paths refer to the actual saved record. All three saved GameObjects have `m_IsActive:1`, and their relevant component `m_Enabled:1`. This is serialized evidence, not a Play Mode active-in-hierarchy check.

| Identity / exact lookup key | Saved presence and component fields | Transform/source anchor | Current surface boundary |
| --- | --- | --- | --- |
| Abe / `Abe` | GameObject `!u!1 &351378609`, `m_Name: Abe - Outpost Quest Giver`; components Transform351378610, CapsuleCollider351378612, QuestGiver351378611. `MonoBehaviour[351378611].m_Script.guid` matches QuestGiver; `.npcName=Abe`, `.questTitle=The Forest Road`, `.introduction`, `.progress`, `.completion`, `.rewardGold=25` | Transform351378610 local(-3,0,2), parent1754071920. Parent GO1754071919 is `04 - Gameplay`, transform identity/root, so saved world position(-3,0,2). Player component's `questGiver` ref points to Transform351378610 at scene line507584 | QuestGiver marker/nameplate; actual quest body/title comes from PlayerProgress, not the three serialized prose fields. No TownNPC in Abe's saved component list |
| Rowan / `Quartermaster Rowan` | GO2124646072, `m_Name: Quartermaster Rowan`; Merchant2124646073, CapsuleCollider2124646074, Transform2124646075. `MonoBehaviour[2124646073].npcName=Quartermaster Rowan`; Merchant GUID matches | Transform2124646075 is a root, local/world(-7,-0.05,-2), unit scale | Merchant nameplate and shop. No serialized TownNPC/greeting field on this actor |
| Mira / `Provisioner Mira` | GO1934746513, `m_Name: Provisioner Mira`; Transform1934746514, Merchant1934746515, CapsuleCollider1934746516. `MonoBehaviour[1934746515].npcName=Provisioner Mira`; Merchant GUID matches | Local(-25,5.400336,160), parent561147576. Parent GO561147575=`08 - Greenhollow Frontier`, position(-17,7.6496644,244), identity rotation/unit scale/root. Derived saved world(-42,13.0500004,404), approximately(-42,13.05,404) | Merchant nameplate and shop. No serialized TownNPC/greeting field on this actor |
| Elara / `Cartographer Elara` | No selected saved `npcName`, GameObject-name or runtime-group record found. Not categorized as defective | Runtime path in section3; expected X/Z(-46,397), Y sampled at creation | TownNPC greeting/gossip/faction quests, once Ensure prerequisites succeed |
| Selvek / `westbank-reaver-captain` (display `Captain Selvek`) | No selected saved enemy-ID, GameObject-name or camp-root record found. Not categorized as defective | Runtime path in section3; X/Z(-283,78), Y sampled at creation+.03 | Hostile EnemyController encounter, not a friendly TownNPC/gossip actor |

Scene evidence anchors: Abe GO/transform/component at lines99952–100006; Mira at541091–541138; Rowan at598708–598778. These line references describe the inspected hash; file IDs and script GUIDs are the stronger identity. Future insertions can move line numbers.

Do not substitute GameObject display names for exact `npcName` lookup values. In particular `Abe - Outpost Quest Giver`, prompt text `Elder Abe` and lookup `Abe` are different surfaces, not three actors. Keep the lookup key. Rowan and Mira cannot be assumed to enter `NPCGossipWindow.DrawTownNPCGossip` merely because they are NPC-shaped merchants.

## 3. Runtime-created anchors and dependencies

`PrototypeHUD.Start` calls `ExplorationSites.Ensure()` and `RiverReaverCamp.Ensure()` at source lines30–31. These are source-path findings, not proof this document ran them.

### Elara

`ExplorationSites.Ensure` returns unless `WorldRegions.Expanded`; in the current scene that property checks scene name `Outpost-Expanded`. If `Frontier exploration sites` already exists it calls `EnsureCartographer(existing.transform)` and returns. Otherwise it needs an existing `Landmark` with ID `ancient-grove`; if none exists it returns before creating the root or Elara.

`EnsureCartographer` guards `parent.Find(ExplorationSites.CartographerName)`, creates a root named `Cartographer Elara`, and places it at `WorldRegions.Greenhollow + (-6,0,-3)` before replacing Y with `WorldRegions.GroundHeight(position)`. `WorldRegions.Greenhollow=(-40,13,400)`, hence source X/Z(-46,397), **not** the marker centre(-40,400). It creates a Human/Mage visual, capsule(height1.9/radius.4/centreY.95) and `TownNPC` with exact `.npcName=Cartographer Elara`, `.roleTag=Cartographer`, and an explicit `.greeting` string.

The guard is under that parent; it is not a global guarantee against a separately authored duplicate elsewhere. Runtime hierarchy/duplicate validation remains parent-owned future evidence. Do not create a saved replacement just because the serialized name search returned0.

### Selvek

`RiverReaverCamp.Ensure` returns unless expanded or if `GameObject.Find("River Reaver camp")` already exists. It creates the root and calls `CreateEnemy` with `CaptainId="westbank-reaver-captain"`, display `Captain Selvek`, `race="Undead"`, `class="Rogue"`, raw `Position=(-283,15.46,78)`, boss=true. The actual root position comes from `Ground(point)`, which uses `WorldRegions.GroundHeight(point)+.03`; raw Position.Y is not a verified runtime foot height.

The captain has EnemyController flags `isBoss`, `heavyAttacker`, `campGuard`, `nativeDeathAnimation`; root rotationY180; source boss stats260HP/2armor, attack16/interval2.4 and movement multiplier.65. These are existing source settings, **not balance changes**. It also creates `westbank-reaver-1` Human/Rogue and `westbank-reaver-2` Undead/Warrior around the camp. Hostile Returned visuals are not proof all Returned are enemies.

`CreateEnemy` ends with active state `!player || !Application.isPlaying || !player.HasDefeated(id)`. Therefore missing/inactive runtime Selvek after a legitimate defeat is not a lost actor. Do not respawn him to make a narrative after-line visible.

The optional resource chest uses `Resources.Load<GameObject>("Props/TreasureChest")`; if missing, captain/guards were already created but no lockbox is created. Cache ID=`river-reaver-lockbox`, base45gold/75XP, position sampled from camp+forward4. `PlayerServices` explicitly requires `HasDefeated(RiverReaverCamp.CaptainId)` to open it. Preserve that gate; it is not Abe's main-captain flag.

## 4. Actual display fields and API gaps

| Surface | Current source/read path | Available state / important gap |
| --- | --- | --- |
| Abe main title/body/objective/reward | `PrototypeHUD.DrawQuestWindow` reads `player.QuestName`, `.QuestDescription`, `.QuestObjective`, `.QuestGold`, `.QuestExperience`; `player.ActiveQuestGiver` establishes context | Serialized QuestGiver prose and rewardGold25 are not the actual3-stage body/reward calculation. Updating only those fields would leave the displayed quest text unchanged |
| Abe interaction/name | `PlayerServices.TryGetBestInteraction` uses `CanTalk()` then `questGiver.GetComponent<QuestGiver>()`; labels refer to Elder Abe; QuestGiver uses `.npcName` | `CanTalk`: alive, active questGiver and distance<4m. Keep existing modal/interaction rules. No serialized TownNPC was found to carry Abe's separate faction gossip |
| Rowan/Mira merchant header/body | `Merchant.Start` builds `npcName + "\n<Supplies & Repairs>"`; `AdventurePanels` shop header reads `player.ActiveMerchant.npcName` and supplies a generic inline shop body | Merchant has no public greeting/lore/progress/completion fields. Existing generic shop copy is a potential view-side adapter surface, not an assumed TownNPC callback or a serialized greeting |
| Rowan/Mira action acknowledgement | `PlayerServices.BuyPotion`, `TemperWeapon`, `RestAtMerchant`, `ApplyAuthorizedTransaction` | Public methods mutate state; they are **not read-only narrative predicates**. No public last-successful-purchase/receipt/story-trust history or event-context query was found. Gold/potion changes alone are ambiguous proof of a purchase |
| Elara greeting | `ExplorationSites.EnsureCartographer` sets `.greeting`; TownNPC has `.npcName/.roleTag/.greeting`, bark display, gossip role/name fallback | Current greeting is a single generic string, not a typed state-aware catalog. Exact runtime-created anchor must be covered by any later adapter |
| Elara quest text | `FactionQuestSystem.QuestDef.title/description/targetName`; `NPCGossipWindow` reads currently active/available quest | `FactionQuestSystem.Instance` is a singleton; use the intended local player's system/context. Public state APIs do not authorize reading a host's private progress for all clients |
| Selvek | `RiverReaverCamp` sets EnemyController.enemyId and Combatant.displayName; encounter attack/home/death paths | No TownNPC/QuestGiver is added; no friendly dialogue/parley surface or public “met Selvek”/spared outcome exists. Enemy display-name copy is not an implementation of a safe bark adapter |
| Optional memories/secrets | No first-five testimony flags in inspected sources | No `HeardMara`, `AbeConfessed`, `ReadSeventhWagon`, trust/reputation or chosen-ending API. Deep reveal lines remain excluded pending explicit contract; do not invent predicates |

Public read-only gameplay facts available for a future agreed adapter include `QuestIndex`, `QuestAccepted`, `QuestReady`, `QuestComplete`, `Kills`, `Supplies`, `BossDefeated`, `HasLandmark(id)`, `HasDefeated(id)`, `HasOpenedChest(id)`; faction `ActiveQuestId`, `CurrentProgress`, `GetActiveQuest()`, `GetQuest(id)`, `IsQuestCompleted(id)`, `IsActiveQuestReady()`, `HasAvailableQuest(npcName,player)`, `GetAvailableQuestForNPC(npcName,player)` and `HasActiveQuestToComplete(npcName,player)`. Availability checks can initialize the catalog; they are not a new testimony database. The lists do not constitute accepted schema tokens.

Do not use arbitrary reflection, raw expressions or progression mutations to manufacture unavailable knowledge. Exact acceptance for a content schema/adapter is still pending from actual Antigravity; historical lore documents and this inventory are not that acceptance.

## 5. Main quest description branch inventory

`PlayerProgress.QuestDescription` currently has nine semantic branch surfaces below. Rows describe **existing code**, not accepted data IDs or a proposed replacement parser. The existing body precedence is QuestComplete→unaccepted indexed offer→ready indexed reply→QuestObjective progress.

| Surface | Actual branch condition | Current body / objective wording | Preserve gameplay |
| --- | --- | --- | --- |
| Main complete | `QuestComplete` | “You broke the raiders' hold on the forest. The village will remember your name.” | Completion body wins even if a stale accepted flag remains |
| Offer0 | `!QuestComplete && !QuestAccepted && QuestIndex==0` | Raiders cut the forest road; defeat3; return; currently recommends Second Wind | No extra quota; class-specific advice is a copy issue, not permission to change ability availability |
| Offer1 | Unaccepted index1 | Winter supplies; north road then east camp;3 marked crates;2guards | No renamed crate IDs or extra recovery requirement |
| Offer2 | Unaccepted remaining normal index2 | Crates carried a map; ruined watchtower northeast; heavy strike/red circle | Existing `ashwood-captain` objective, not Selvek |
| Ready0 | Accepted, QuestReady, index0 | Shared current reply: “Well done! These supplies and a safe road will keep our village alive. Rest here before your next task.” | Same literal currently serves0 and1; semantic copy can be refined later without adding rewards |
| Ready1 | Accepted, QuestReady, index1 | Same shared reply as Ready0 | Keep ready/turn-in semantics separate from “already completed” |
| Ready2 | Accepted, QuestReady, index2 | “The captain has fallen! Take this axe as a symbol of Emberwatch's gratitude. You have earned it.” | Existing authorized final reward only; reading text never grants axe |
| Progress0 | Accepted, not ready, index0 | `QuestObjective`: defeat forest raiders `{min(Kills,3)}/3`, follow north road | Live count stays current; do not freeze it as a static catalog sentence |
| Progress1 | Accepted, not ready, index1 | `QuestObjective`: recover crates `{Supplies}/3`, camp east of road `[E]` | Actual supply IDs and selection unchanged |
| Progress2 | Accepted, not ready, index2 | `QuestObjective`: defeat Ashwood Captain0/1; ruined watchtower northeast | Actual boss state unchanged |

Counting clarification: source has **nine body outcomes if the shared Ready0/Ready1 text is counted once**, or **ten index-specific rows** as shown. There are3 indexed offers,3 indexed progress states,2distinct ready replies and1complete reply. This is a description of conditional branches, not a claim of nine stored dialogue records.

`QuestReady = QuestAccepted && (index0? Kills>=3 : index1? Supplies>=3 : index2 && BossDefeated)`. `EnemyController.IsMainQuestCaptain` is exactly `enemyId=="ashwood-captain"`; `IsRoadRaider` requires non-training enemy ID starting `road-raider-`. **`BossDefeated` is not the Reaver captain defeat key.** `HasDefeated("westbank-reaver-captain")` is the separate relevant public fact.

| Existing main quest index/name | Required existing progress | Base reward unchanged |
| --- | --- | --- |
|0 / The Forest Road |3road raiders |25gold/80XP |
|1 / Stolen Supplies |3supply crates |35gold/100XP |
|2 / The Ashwood Captain |Ashwood captain defeated |60gold/180XP |

`ApplyAuthorizedCompleteQuest` applies the existing Human1.25 gold multiplier, adds1potion capped10, heals, increments index, clears accepted state and at index>=3 marks complete/weapon upgraded and grants `emberwatch-axe` and `emberwatch-sigil`. Values and side effects are inventoried, not changed. The quest window displays rewards from those computed properties, not the saved QuestGiver.rewardGold field.

## 6. Selected faction quests and reward invariants

The source catalog has35 `Add(new QuestDef)` definitions. This section inventories only the3 definitions directly given by Abe/Elara among the five selected identities; it is not the full quest graph. No selected giver/target definition was found for Rowan, Mira or friendly Selvek.

| Existing ID | Giver / target / count | Eligibility and exact base reward |
| --- | --- | --- |
| `alliance_highspire_dispatch` | giver `Abe`; `talk_npc`, target `Grand Duke Ronald`, required1; location(-200,12,-60) | faction `Alliance`; empty prerequisite; next `highspire_royal_council`;45gold/110XP/`alliance-war-banner`. Do not silently gate on main QuestComplete |
| `frontier_cartography` | giver `ExplorationSites.CartographerName`=`Cartographer Elara`; `discover_landmark`, target group `frontier-vistas`, required3; first target(-241,6,44) | faction `Any`; no explicit prerequisite; next `river_reaver_bounty`;75gold/150XP/`travel-charm` |
| `river_reaver_bounty` | giver Elara; `defeat_enemy`, target `RiverReaverCamp.CaptainId`=`westbank-reaver-captain`, required1; targetLocation raw camp Position | faction `Any`; prerequisite `frontier_cartography`;60gold/140XP/`riverwarden-signet`; existing displayed title **Silence the River Reavers** |

Survey membership is exactly `westbank-overlook`, `bloodrock-vista`, `wyrmcrest-lookout`; **not** the separate zebra-unlock trio `willow-crossing`, `greenhollow-post`, `ancient-grove`. Existing discovery caches are `${surveyId}-cache`, each20gold/40XP in ExplorationSites. A charting line cannot rename the IDs, substitute Ancient Grove or claim a fourth evidence object exists.

`AcceptQuest` refreshes survey progress from existing discoveries; for eligible exact1boss targets including Selvek it also credits a pre-existing `HasDefeated(targetId)`. Preserve that behavior: the player may complete the bounty after already defeating Selvek, without a forced re-fight. Faction turn-in uses target NPC for `talk_npc`, original giver otherwise; completion readiness is `CurrentProgress>=requiredCount`.

Faction availability currently determines legacy side from race keys(Brute/Orc/Goblin/Undead considered Horde). `CharacterFaction` display identity and the lore names Hearthward Compact/Ashen Pact are not interchangeable filters. Keep rules intact; any future roster/faction-eligibility correction is an explicit gameplay contract, not a string update.

Offline faction data lives in the existing `FactionQuestSaveData`(activeQuestId/currentProgress/completedQuests). Its `SaveData/LoadData` early-return in multiplayer. Existing online authority/session-only policy remains; optional dialogue cannot silently persist or replicate new lore knowledge.

## 7. Read-only preservation receipt and remaining evidence

The following exact bytes were hashed during inspection. They are **six selected terrain assets plus the scene**, not every source/terrain file. No file listed was written by this work.

| File | SHA256 |
| --- | --- |
| `Assets/RPG/Scenes/Outpost-Expanded.unity` | `7C9A1244D6C63C6F383311EAA041405AADA73B7BD373493D2C3393CD9BA2AF63` |
| `Assets/RPG/Terrain/WorldExpansion/World tile 0-0.asset` | `B9E57EF716768EBC4F0579B3FECB43AB6928D0CE4D1A8D1D810C586AAD659B65` |
| `Assets/RPG/Terrain/WorldExpansion/World tile 0-1.asset` | `E8E71AFE76879A20AD39DB49B5557042B6D1E384B5886D4B24442CA06326E1FD` |
| `Assets/RPG/Terrain/WorldExpansion/World tile 1-0.asset` | `8A65CCDD945654F00958A237FC69B9582C4D7ADE831B128894FCD189ADE88289` |
| `Assets/RPG/Terrain/WorldExpansion/World tile 1-1.asset` | `9A794173BD01C1DBFF75DFDE7C1EC763222A822AC3EA7E07BC7AA4E8D9060FE6` |
| `Assets/RPG/Terrain/ReaverCamp/Reaver approach preserved final.asset` | `0D9B4991D077E49B5B3C96CA91B64A1D477F76EB3EC9C811B8BFFA41556A4397` |
| `Assets/RPG/Terrain/ReaverCamp/Westbank crossing protected.asset` | `5CDA8E104878069D3FC9282B68611C33B2ABFBCC73EAD9453C70D69802C4C66D` |

NOT RUN for these documentation changes: Unity compilation, native runtime hierarchy/actor creation, dialogue adapter execution, player-specific host/client knowledge tests, screenshots, physics/traversal or gameplay regression. Existing145/41/32 integration results remain historical baseline, not fresh passes for a new story implementation. Parent owns later read-only validator/schema/catalog work and exact adapter acceptance.

After both owned documents were written, the seven listed scene/terrain hashes were rechecked:7/7 match the inspection values. Source catalog count rechecked:35 definitions. No scene/terrain dirty path was reported by the scoped Git check.
