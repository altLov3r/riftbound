# Canon, knowledge and reveal matrix — SC00

Content-authoring evidence at base `8ad9071820c2fbed32fbb728122ffb2f02980888`,17 November2026. Canon sources: `Docs/Lore/RIFTBOUND_LORE_BIBLE.md` and `Docs/Lore/NPC_STORY_CONTRACTS.md`; gameplay-state source inventory: `ACTOR_STATE_INVENTORY.md`. **This document neither implements a resolver nor accepts a schema.** Actual Antigravity adapter-owner acceptance is still pending. No new flags, progression effects, scenes, rewards or dialogue have been applied.

## 1. Epistemic labels and safe thresholds

**Author truth** describes canon for writers; it does not make every NPC omniscient. **Official claim** is an institution's record and can be disputed. **Witness account** belongs to a named participant; true core facts still need an appropriate delivery context. **Rumour/suspicion** must sound uncertain and must not become a quest-completion predicate. These are prose review labels, not newly accepted machine schema fields.

Thresholds used below are conceptual guardrails:

- **Public now:** safe ambient/basic greeting information without private quest assumptions.
- **Existing personal state:** only an actual supplied player's main/faction/discovery/defeat/cache state, with source predicates listed explicitly. It establishes that gameplay event, not arbitrary additional historical knowledge.
- **Explicit optional topic:** deliberate player request can present a narrow character account where an agreed adapter later supports such a topic. It does not automatically create durable knowledge or grant an item/reward.
- **Future private testimony:** stronger disclosure needs a reviewed evidence/knowledge contract and perhaps new persistence scope. **Unavailable now**; omit the disclosure from a pack using only existing state.
- **Encounter public:** current hostile encounter/death/phase fact visible to eligible participants. Defeat flags are not proof of hearing a confession or choosing an ending.

Unknown, missing or wrong-owner state falls back to safe public copy. No “assume completed,” raw expression evaluation, inspecting another client's private save, invented `AbeConfessed` flag or testimony token is allowed. A player can skip optional lore and still follow all existing objectives.

## 2. Global canon and political claims

| Fact or statement | Classification / who may know | Safe reveal threshold and available source | Excluded inference |
| --- | --- | --- | --- |
| Veyr contains the March of Nine Hearths and its working roads/settlements | Author truth; ordinary local geography | Public directions using actual expanded region/actor transforms | An unseen new region, fifth tile or new teleport already exists |
| The First Binding occurred412 years ago | Author truth, also taught history; archivists may cite it as a record | Optional public history; no player age/timeline flag exists or is needed for a short account | Abe personally witnessed it, or every institution preserves an identical perfect account |
| Witness Lattice requires freely witnessed continuity and restrains overlapping unrealized histories | Author truth; custodians/engineers hold partial practical knowledge | Optional concise specialist account, later staged through evidence; not Abe's initial road order | Every NPC knows the entire mechanism; broken saves/obsolete map coordinates are “the Rift” |
| Consent produces a stable binding; coercion accumulates debt | Author truth; institutions interpret it self-servingly | Explain through freely given work/contract testimony first, deeper mechanism later | A magical loyalty test, forced player faction change or hidden reputation/cost mechanic |
| The Tithe of Names began103 years ago and erases testimony/memory to feed anchors | Author truth; concealed institutional practice, not public opening fact | Future private testimony/institutional evidence; no accepted current knowledge predicate | `QuestComplete`, a purchased potion or generic faction membership means the player knows the Tithe |
| Ashfall61 years ago made pressure/quarry work indispensable | Author truth; public industrial history plus private shortcuts | Optional public Rustwater/Frostpeak history, qualified source | Every catastrophe or current NPC secretly originates from the Seventh Wagon |
| Hollow Dusk3 weeks ago caused repeated notes/manifests and current shortages | Author truth; workers know symptoms, not universal cause | Public “recent discrepancies” line from Rowan/Mira/Elara | All already know Hollow Choir authorship or a complete alternative-history map |
| Roadward Truce permits mixed Compact/Pact adventuring parties | Author truth/public agreement | Public courtesy, existing cross-faction co-op policy | Removes historical conflict, changes legacy `Alliance`/`Horde` filters or creates PvP |
| Compact protection/central archives are necessary and can conceal coercive debt | Mixed public ideal and author truth about institutional failure | Public ideal from Ronald/officials; particular abuse needs named record/witness | Every Compact citizen approved coercion or every Highspire worker speaks as Ronald |
| Pact sovereignty/free contracts coexist with dangerous private reprisal | Mixed public ideal and author truth about failure | Public ideal from Mor'gok/clans; particular reprisal allegations identified as reports | Pact traders approve Selvek's extortion, or hostile crag raiders are automatically allies |
| Hollow Choir exploits forged witness records | Author truth, not first-greeting knowledge | Future private evidence/encounter-specific revelation; named witnesses qualify uncertainty | Elara can identify every Choir agent from three existing landscape discoveries alone |
| Returned are people; ancestry/model does not diagnose corruption | Author truth; basic social/ethical frame | Public respect and individual affiliation/action-based hostility | `CharacterRace==Undead` proves villainy, memory theft, guilt, death state or faction hostility |
| Nine great anchors occupy existing places; Emberwatch is a relay and Greenhollow a working post | Author truth; local partial knowledge | Optional specialist/local history tied to actual places | Nine newly spawned magical props or a tenth anchor have been implemented |

Anchor reference list: Highspire/Crown Bell, Frostpeak/Deep Anvil, Silverglen/Silver Root, Deathknell/Namewell, Rustwater/Pressure Wheel, Gor'Kresh/Ash Hearth, Duskwood/Witness Vault, Ancient Grove/Living Witness, Wyrmcrest/Summit Crown. Functional names are canon descriptions, not new serialized object/quest IDs.

## 3. First-five character and event matrix

| Character/fact | Truth, claim, account or suspicion / knowledge holder | Reveal guardrail and actual state boundary | Safe line intent / forbidden leap |
| --- | --- | --- | --- |
| Abe needs the current forest road cleared | Current gameplay fact; Abe/local workers | Main `QuestIndex==0`, unaccepted/accepted/ready branches from PlayerProgress | Clear3 road raiders and return. Do not replace quota or direct to the Reaver camp |
| Supply crates were stolen and3 marked crates must be recovered | Current gameplay fact; Abe knows current objective | Main index1; `Supplies` read-only count and actual crate interactions | North road/east camp directions. Does not prove a new manifest evidence item was collected |
| Ashwood Captain threatens the old watchtower | Current gameplay fact; Abe knows immediate threat | Main index2; `BossDefeated` for exact `ashwood-captain` | Heavy blow/return advice. Never confuse this with Selvek, Malakar or all world bosses |
| Main quest complete means the player's local road tasks were rewarded | Personal gameplay fact | `QuestComplete` after existing authorized index progression | Thanks/limited allusion to Abe's responsibility. Does not imply every camp is clear or any private memory was heard |
| Abe obeyed the order that held the Seventh Wagon | Author truth; Abe first-hand account; not everyone's public certainty | **Private optional future**. An explicit account may later be authored through an accepted topic; no current confession/heard flag | Practical guilt can be hinted without naming details. Never gate the current quest on a new confession or assert a party heard it |
| Mara was Abe's sister and died after the held wagon | Author truth; Abe's private family knowledge | **Private optional future**, with deliberate reveal/evidence contract. No `HasHeardMara`, journal collectible or family-link API exists | Named disclosure reserved. No invented daughter, player-relative, rescue actor or implied resurrection branch |
| Nineteen people were officially declared lost at Broken Toll18 years ago | Official record with author-confirmed core; individual fates differ | Public historical record if clearly attributed; detailed names/connection to Abe optional | “The toll record lists nineteen lost.” Do not say all19 share the same present state or are all Returned |
| Selvek was among those declared lost and later Returned | Author truth; Selvek's first-hand account and disputed institutional record | Basic present grievance may appear as a **character claim** in a later accepted hostile bark; corroborated identity/history needs explicit evidence/topic | Keep Undead/Rogue. A visual `Undead` model is not proof of his identity or every passenger's fate |
| Official ledger refuses to recognize Selvek's return/wages/testimony | Author truth about a particular institution; Selvek knows the harm | Optional public dispute attributed to Selvek/Elara, deeper evidence later; no current “read ledger” flag | Explain grievance without declaring that the player recovered an actual document or must pay a new toll |
| Selvek extorts and attacks travellers, including Pact traders | Author truth plus existing hostile encounter | Encounter public; EnemyController/actual current actor, existing authoritative captain defeat | Humanize without excusing violence. No new friendly gossip, spare/recruit/surrender or nonlethal quest result |
| Selvek defeated for this player's claim | Personal gameplay fact, potentially shared encounter | `HasDefeated("westbank-reaver-captain")`; faction quest ready/completed is a separate phase | Aftermath/eligibility can acknowledge defeat; cannot say the player chose mercy, heard last words or collected a witness item |
| Reaver lockbox opened | Personal gameplay fact only | `HasOpenedChest("river-reaver-lockbox")`; existing gate requires captain defeat | Acknowledge opened loot. No actual contents include Mara testimony by this plan; do not invent evidence from an opened-cache flag |
| Rowan's shelves and receipts disagree | Authored local observation; Rowan knows stock; cause not established | Public merchant-adjacent copy once a merchant adapter is agreed | Shortage/receipt theme. Not proof he runs every caravan, caused sabotage or knows the Choir/Tithe mechanism |
| A purchase/rest/temper transaction succeeded for this buyer | Current gameplay action result, but no read-only narrative history API | **Unavailable as persistent/current predicate**. Requires a specific accepted authorized transaction context; public service greeting remains safe | Never infer success from merely opening shop, gold amount, potion count or request dispatch; no success line after rejection |
| Mira knows unusual orders and frightened workers | Authored local knowledge with a privacy duty | Public general trend may be told; customer identity remains private | Fair supplies/workers' trust. No trust meter, food buff, unpaid-debt quest or customer-spoiler gossip implied |
| Elara needs independent observations at3survey sites | Current gameplay fact with authored method | Active/available `frontier_cartography`; `HasLandmark` and `CurrentProgress` are existing state | Site list stays Westbank Overlook/Bloodrock Vista/Wyrmcrest Lookout; earlier discoveries count |
| Existing survey completed | Personal gameplay fact | `IsQuestCompleted("frontier_cartography")`; or ready predicate while active, clearly distinguished | Elara can compare marks and offer existing next bounty. No perfect knowledge of who forged an institution's record |
| Elara suspects incompatible manifests are more than copying mistakes | Authored professional suspicion, not a full villain diagnosis | Optional public qualified account; each claim attributed to what she compared | “These marks disagree with the copied route.” Do not claim a hidden conspirator/Seventh Wagon proof was unlocked by landscape marker state |
| Reaver bounty completed | Personal quest fact | `IsQuestCompleted("river_reaver_bounty")`; not merely `HasDefeated` if turn-in has not happened | Appropriate personal aftermath/reward already authorized; no additional narrative reward or confession knowledge |

Distinguish two safe variants for the first pack: **claim-only wording** (“Selvek says the ledger refused him”) can be optional attributed speech if the adapter surface supports it; **corroborated private history** requires an explicit later evidence/reveal contract and is excluded from a current-state-only pack. This avoids treating authored truth as automatically known to the player or all NPCs.

## 4. Reveal ordering without inventing gameplay

1. **Immediate need:** Abe gives the actual road/supply/captain directions; Rowan makes shortages tangible. A new player can accept without reading a history book.
2. **Independent observation:** Elara's existing3-site survey establishes why first-hand marks matter. Mira makes worker trust concrete without new prices, items or favour currency.
3. **Public dispute:** Existing Selvek bounty/hostility can be contextualized as an unjustly excluded person's chosen extortion. Keep the current defeat/lockbox outcome and do not pretend a friendly conversation occurred.
4. **Optional private history, future:** Abe's specific held-wagon order, Mara's relationship/fate and the complete19-person account need a deliberate topic/evidence and accepted knowledge scope. Current `QuestComplete` alone is insufficient for named-confession memory.
5. **Institutional interpretation, later campaign:** Ronald/Mor'gok/Namewell/Duskwood accounts compare partial truths. Tithe/Choir/Perfect Witness facts are revealed through actual authored sources, not generic role fallback.
6. **Future climax:** Consent-based public oversight versus distributed custody is a proposal requiring outcome/save/co-op design. No existing boss defeat means a new political ending or final phasing already happened.

If schema acceptance limits the first implementation to existing typed states and public text, stop at layers1–3 and omit unsupported private lines. Do not introduce arbitrary conditions to force a complete lore bible into the first slice.

## 5. Scope and co-op checks for later adapter review

| Check | Required outcome |
| --- | --- |
| Unknown or wrong-player context | Safe public fallback; never use the host's private main/faction progress for the client |
| Main quest offer/progress/ready/complete | Correct actual branch; live counts preserved; no narrative line mutates quest state |
| Abe faction dispatch versus main chain | Existing independent eligibility retained; completion of either does not fabricate the other |
| Elara earlier discoveries / pre-defeated Selvek | Existing accepted-quest refresh/credit behavior retained; no new mandatory detour or respawn |
| Mixed-stage party | Each player sees eligible personal objective/reward text; shared combat can be public without sharing private confessions |
| Late join | Current replicated world state only; no replay of another player's private topic, historical reward or assumed memory |
| Merchant request rejected | No purchase-success line or lost valid resource; service options remain useful after story copy |
| Hostile Selvek | No talk lock/input modal added through enemy proximity; death does not resurrect an NPC conversation |
| Rumour/official claim | Attributed and uncertain where necessary; not recorded as proven fact simply because a line displayed |
| Narrative erasure | No deletion/mutation of names, characters, quest/save IDs, inventory or real progress |
| Schema agreement | Actual adapter-owner response names accepted version/fields/surfaces; authored docs are not a signature |

Verification for this matrix: source/canon cross-check and documentation whitespace review only. NOT RUN: schema parser/catalog execution, Unity compilation, adapter routing, UI, player-input or host/client gameplay tests. Parent owns later validation and records exact results; existing integration passes are not reused as fresh narrative acceptance.
