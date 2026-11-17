# First-five adapter conformance — consumer handoff

17 November2026. Companion instructions for the frozen SC02 proposal at story/codex-content-20261117 checkpoint8605123. **This document is not adapter-owner acceptance, runtime implementation, a Unity test receipt or permission to promote content.** It changes none of the four frozen artifacts, the schema/catalog/contract, Assets, source code or existing quest/service actions.

Current state remains PENDING actual Antigravity decision. Portable70-case DTO export/check/result-comparison tooling is included: **62 frozen state-selection cases plus8 new transport-identity cases**. That tooling exercises authoring/reference contracts and validates submitted receipts; it does not execute or independently attest a Unity adapter.

## 1. Exact inputs and acceptance boundary

Read CATALOG_CONTRACT_V1.md, ACTOR_STATE_INVENTORY.md, SCHEMA_ACCEPTANCE_REQUEST.md and the current OWN_CODEX_STORY_HANDOFF.md before implementing. schema-proposal-identity.json owns frozen exact UTF-8 LF byte identities:

| Artifact | SHA256 |
| --- | --- |
| CATALOG_CONTRACT_V1.md |8d88fd16f18042a793acf1d8d0f7e4b6b33f12a8f53b10080dc95410a65d971d |
| first-five.schema.json |a894d204bfd497acb2dd986fffa3d0804db2ea04bc5cae8ba7b46ea850b24e10 |
| first-five.v1.proposed.json |9b7e38f26e1153a45c6ce0032ede3dd39acc536e0ba09594561de06e7c5b7f73 |
| selection-fixtures.json |adf636845713368f289d43626fd1ef119834eb471db7d7b8d844dc50ef025c3e |

If local bytes disagree, stop the compatibility claim and inspect branch/version/line endings. Do not change these files or their manifest to make an unsupported adapter appear compatible. A requested semantic change needs one agreed revised/versioned contract; a consumer fork is not a shared specification.

Actual adapter owner records ACCEPTED or REVISION REQUESTED in its own branch/journal or Docs/Production/NARRATIVE_SCHEMA_ACCEPTANCE.md, naming exact contract/schema hashes, supported channels/predicates, component identity rules, per-viewer context and concrete unsupported mappings. Codex cannot sign for that owner. Starting work, a user's “continue,” code presence, an internal review, export success,70 matching outputs or a producer label does not supply that decision.

Until that matching decision, keep authored content under Docs/Production/Story. No accepted runtime catalog, Resources loader, NPC copy replacement or promoted Assets/RPG/Resources/Narrative file is claimed by this handoff.

## 2. Consumer boundaries that must actually be implemented

| Actor / identity source | Correct text surface | Required preservation / common failure |
| --- | --- | --- |
| Abe: QuestGiver.npcName=`Abe` | Current PlayerProgress.QuestName and QuestDescription presentation, consumed by PrototypeHUD.DrawQuestWindow; scoped by the active actual QuestGiver | Changing only serialized QuestGiver.introduction/progress/completion leaves the real three-mission body/title unchanged. Retain exact title values, QuestIndex, objectives, rewards, accept/turn-in delegates, eligibility and current separate dispatch prerequisite. Avoid recursive fallback where a replacement QuestName getter calls itself through the adapter. |
| Rowan: Merchant.npcName=`Quartermaster Rowan` | Read-only copy in existing shop header/body or an explicitly accepted existing service view | No TownNPC/greeting field exists on the saved merchant. Do not cast a Merchant to TownNPC, add a duplicate actor, require a new conversation before trade, or replace price/action/cost text with lore. |
| Mira: Merchant.npcName=`Provisioner Mira` | Same agreed merchant shop-view surface, bound to the actual ActiveMerchant | No observed purchase-history/trust/debt predicate exists. No “thanks for buying” line based on guessed gold/potion changes or another customer's transaction. Keep merchant services and identity intact. |
| Elara: actual TownNPC.npcName=`Cartographer Elara` | Current greeting and existing faction-quest offer/progress/ready/after surfaces for exact frontier_cartography/river_reaver_bounty topics | Resolve against the intended interacting viewer. Runtime-created Elara is not a missing scene actor to duplicate. A singleton is usable only when proven to represent that viewer, not merely because its name is Instance. |
| Selvek: EnemyController.enemyId=`westbank-reaver-captain` | Existing host-owned hostile encounter hook; encounter_bark topic `aggro` only | No friendly greeting, gossip, parley, recruit/spare branch, new combat rule or reward. Missing/inactive enemy is UNKNOWN life state, not automatic proof of a kill; do not respawn him to obtain a line. |

Resolve identity from the authoritative component and actor kind, not the GameObject label, role title, race, display alias or case name. `Abe - Outpost Quest Giver`, UI label `Elder Abe` and lookup `Abe` are different surfaces. Conflicting cloned companion names must be disclosed, not silently corrected with Contains matching. Hostile identity is the stable enemy key; a translated nameplate cannot redefine it. Follow the frozen bundle's exact binding rules and request a contract revision if a real component mapping cannot supply them.

Existing interaction/modal/dead/range/nearby-revive checks run before text display. Classifying an identity is not permission to open its UI or perform an action. No new E handler, input path, movement/cursor lock or service gating is part of the catalog. Cosmetic text failure must leave a legal original service usable.

## 3. Read-only typed context requirements

The context belongs to one resolved viewer and one sampling instant. Build an immutable snapshot for each resolution. Do not incrementally merge the last viewer's keys into a reusable static dictionary. Clear or replace context on actor/viewer switch, quest transition, close/reopen, load, respawn, scene exit, session leave/rejoin and host/client role change.

| Field | Existing source / meaning | Presence/type rule |
| --- | --- | --- |
| main.index | That viewer's QuestIndex0..3 | Integer only; unknown is absent, not0 |
| main.accepted / main.ready / main.complete | Same viewer's actual properties | Boolean only; use QuestReady rather than a new objective-count formula |
| quest.available:key | Existing GetAvailableQuestForNPC(actorLegacyName,viewer)?.id equals the requested existing key | Requires that viewer's valid context and existing prerequisite/faction/one-active rules; do not infer availability from line priority |
| quest.active:key | That viewer-context ActiveQuestId equals key | Exact quest ID, not title or any active quest |
| quest.ready:key | Exact active ID matches key AND that same context.IsActiveQuestReady() | Another ready quest cannot satisfy this topic. Unknown active context stays UNKNOWN |
| quest.completed:key | That viewer-context IsQuestCompleted(key) | No substitution from host, party member, unrelated completed quest or personal story assumption |
| enemy.alive:key | Actual resolved encounter Combatant state for the existing captain | If actor/state unavailable, omit the fact. Do not emit false to mean “object not found” |
| enemy.defeated:key | Intended viewer.HasDefeated(key), viewer scope only | Preserve session-only online vs offline save policy; never load another character's offline journal to fill a blank |

KNOWN FALSE is a real queried boolean value; UNKNOWN is missing/unavailable/untrusted state. A predicate asking for false fails when its key is UNKNOWN. int0 is not boolfalse; string"false" is not a boolean. Typed transport fields are not automatic facts: only main.index consumes intValue; other allowed fields consume boolValue. The inactive default value on a different field cannot synthesize a fact.

The closed condition set is AND-only. No purchase success, relationship trust, heard testimony, Mara revelation, expressions or extra state operator is available. Public lines disclose public facts only. Do not add a hidden persistent “heard” flag or custom predicate to make unavailable copy selectable.

Maintain contract consistency: ready main quest implies accepted/index0..2; complete chain implies index3/not accepted/not ready. A faction quest cannot be active/ready/available and completed together; ready requires its exact active ID. A transient contradictory snapshot safely preserves operational text/fallback instead of leaking a later story beat. Source reads must be atomic enough to avoid manufacturing a mixed old/new state during accept/turn-in.

## 4. Portable DTO bundle — independently resolve, never copy the oracle

Parent tool: Docs/Production/Story/adapter_conformance.py. Exact commands from repository root:

```powershell
python Docs/Production/Story/adapter_conformance.py export --output Docs/Production/Story/adapter-fixtures.v1.json
python Docs/Production/Story/adapter_conformance.py check --bundle Docs/Production/Story/adapter-fixtures.v1.json
python Docs/Production/Story/adapter_conformance.py compare --bundle Docs/Production/Story/adapter-fixtures.v1.json --results PATH_TO_OWNER_RECEIPT.json
```

Export refuses to overwrite different existing bytes; inspect/version a mismatch instead of forcing replacement. Check validates the deterministic62-state+8-identity bundle read-only. Compare requires an actual independently produced receipt path. **No actual owner/runtime receipt was authored by Codex.** These commands are instructions; none was run while writing this companion document.

Bundle formatVersion1 contains catalogId, identities and cases. identities has exactly contractSha256/schemaSha256/catalogSha256/fixturesSha256. fixturesSha256 still identifies the unchanged original62-case file; the exact exported bundle bytes/bundleSha256 cover all70 cases, including the eight deterministic transport additions. These additions do not modify the four frozen artifacts or accept a schema revision. Each case has:

- caseId: reporting/correlation only; never a selector branch.
- actor: actorKind, legacyName, enemyId. **There is no profileId input.** Derive the profile from actual actor binding; do not trust a caller-supplied profile bypass.
- channel, topicId, eligible: the requested existing surface/topic and precomputed eligibility guard.
- knownValues: array of fixed field/key/boolValue/intValue records; absence means UNKNOWN. No dictionary/executable expression is required by the transport.
- expected: resolution, lineId, resolvedProfileId. This is test-oracle material only, not input to the runtime resolver.

The runtime adapter call must receive only actor/request/eligibility/context data. Test code can compare its returned result afterward; it must not pass expected, caseId or an expected profile into the production selection function. Deriving a result by copying expected or switching on recognizable case IDs is not conformance.

The eight identity probes cover ASCII name-case variants, wrong component kind, a Rowan name suffix that must not match by Contains, a renamed Selvek display with correct hostile enemy ID, wrong enemy ID despite a matching display name, unknown/ineligible actor and renamed-Selvek friendly denial. Use the exact case IDs/records exported by the parent tool. Hostile binding is actorKind+enemyId, independent of legacy/display name; other first-five component names use exact OrdinalIgnoreCase matching. Identity classification never bypasses eligible=false or the hostile channel guard.

Reject duplicate/unknown field+key entries, wrong scalar types, stale quest/enemy IDs, unknown schema/channel/scope and conflicting identity data according to the frozen validator/contract. A permissive DTO deserializer that silently defaults missing bool/int fields cannot establish strict import validation. Distinguish an invalid bundle from a valid bundle containing an intentionally unknown runtime fact.

Pure selection follows eligibility, actor kind, requested channel/topic, supported typed predicates, descending priority and Ordinal lineId tie order. No greeting replaces a quest body; no optional topic automatically appears as a private confession. Authoring validation forbids equal-priority overlapping entries; deterministic tie order is a safe fallback, not a license to create ambiguous content.

## 5. Exact result vocabulary and receipt honesty

| resolution | lineId | resolvedProfileId | Required consumer meaning |
| --- | --- | --- | --- |
| line | Exact chosen existing content line ID | Derived profile ID | Present that text only in the requested eligible surface; no actions or state mutation |
| fallback | Empty string | Derived profile ID, or empty for unknown actor | Greeting/public-topic safe fallback. Unknown actor uses neutral copy plus existing options; never broad role/name guessed lore |
| existing | Empty string | Derived profile ID, or empty for unknown actor | Preserve the current operational main/quest title/body; no blank quest, disappeared action or substituted later objective |
| none | Empty string | Derived profile ID, or empty for unknown actor | No encounter line; preserve existing enemy behavior |
| denied | Empty string | Derived profile ID when identity known, otherwise empty | Eligibility/hostile surface rejects display. Identity can still be classified; that does not grant interaction permission |

Known Abe/Selvek remains a resolved profile even if eligibility or a friendly-hostile surface returns denied. Unknown actor has no resolved profile. Do not emit `$fallback`/`$existing` as a literal lineId; those are the authoring reference's sentinel names, represented by the typed resolution values above. A lineId is nonempty only for resolution=line.

Result receipt formatVersion1 contains bundleSha256 of the exact consumed bundle bytes; the same identities object; producer{kind,sourceCommit,entryPoint}; and results[{caseId,resolution,lineId,resolvedProfileId}]. Cases/results must form an exact bijection: no duplicate, missing, unknown or extra cases; no stale IDs, extra fields, mismatched hashes or invented resolution enum.

Producer rules:

- kind=`authoring_reference` means an authoring/reference implementation produced the data. It must never be relabelled unity_adapter because the answers match.
- kind=`unity_adapter` is a claim that the actual adapter implementation was invoked. Record its actual40-hex source commit and real invoked entryPoint. Do not invent an owner commit/runner, record a convenient unrelated HEAD, or label a Python script/native UI observation as execution of a Unity resolver.
- A40-hex shape, hash match and plausible symbol are not independent execution evidence. Retain separate native runner logs/source/build identity and state how the consumer code was executed. Do not add extra receipt fields to a closed DTO; place further evidence in the owner's own run/handoff.
- A comparator can validate only the submitted data/identities and expected answers. Its report always retains **independentRuntimeExecutionVerified=false** and **runtimePromotionAuthorized=false**. These are not toggled by producer.kind or a70/70 comparison.

A truthful matching Unity receipt can support a later runtime verification statement for its exact cases, source and runner. It still does not establish native UI surfaces, network viewer isolation, successful schema review, scene/readable/routine behavior, every quest edge case or the owner's acceptance/promotion decision. Keep those evidence layers separate.

## 6. State and sequence tests beyond the70 portable cases

The portable matrix is necessary authoring coverage, not a substitute for these native/manual adapter checks. All below are **NOT RUN in this task**. Antigravity owns implementation/runner/UI/network tests; Codex supplies unchanged accepted content after the owner decision.

| Sequence / setup | Expected behavior and evidence |
| --- | --- |
| Abe main0 offer→accepted→ready | Same intended viewer selects abe.main0.offer/progress/ready at the actual UI surface; old objective/accept/reward behavior unchanged. Source capture shows PlayerProgress hook, not only edited QuestGiver inspector fields |
| Abe ready turn-in→main1 offer, then main2 and main3 | A legitimate existing action alone advances state/rewards. New title stays one of the four legacy titles. No stale ready body or early complete/aftermath line after transition |
| Abe completed viewer→fresh viewer | Close/switch/open resets all transient context. Fresh index0 gets offer, not host's completed greeting; previously completed viewer stays complete when reopened |
| Transient/out-of-order accept/load snapshot | Inconsistent ready/not-accepted, completed/not-index3 or partially missing fields preserves safe operational copy. Repeated resolution cannot create a quest transition or repair game state by writing it |
| Elara survey offer→progress→ready→after | Uses frontier_cartography availability/active/readiness/completion for that viewer, exact topic. Showing ready/after text does not call CompleteQuest or grant anything |
| Survey complete→bounty offer→progress→ready→after | Existing river_reaver_bounty prerequisite remains actual gameplay rule; no invented main-chain requirement. Captain key is westbank-reaver-captain, not Ashwood BossDefeated |
| Unrelated active/ready quest | A different active quest cannot make either Elara topic ready. Missing per-viewer FactionQuest context remains UNKNOWN, not guessed false or a host singleton |
| Host advanced, guest new | Host has completed survey/bounty; guest has neither. Open same Elara concurrently and capture each intended viewer's selected line/context. Guest must not see earned aftermath or inherit host availability/completion |
| Two simultaneous conversations | Different players can resolve different Abe/Elara states without mutating shared npc.greeting/catalog entries/static context. Closing one UI must not close, advance or replace the other's; no global mutable “current viewer” cache |
| Close/reopen in different order | Abe→Mira→Elara→Rowan→Abe and the reverse produce the same result for the same current state. Cached profile, topic, eligibility and known facts do not bleed across actors |
| Merchant display without purchase history | Header/public topics remain public copy whether no purchase, rejected purchase, heal, loot or a legitimate buy occurred. No thanks-for-buying/debt/trust line is inferred; no purchase predicate is invented |
| Merchant actions while adapter succeeds/fails | Buy/temper/rest/close/inventory input routes, cost checks and authorized transactions stay unchanged. Missing/corrupt catalog still allows legal original trade; lore rendering never invokes those actions |
| Stale catalog/actor/quest IDs | Invalid import is diagnosed; live unknown actor retains safe neutral UI/existing services. No Contains-based alias guess, wrong-profile text, renamed quest key, new progression flag or null-reference crash |
| Selvek alive→dead/despawned/unavailable | Only eligible host encounter may choose encounter_bark/aggro. Friendly greeting stays denied; missing enemy life fact yields no bark. Never respawn him or open a service to obtain a line |
| Selected optional private/unknown topic | Unimplemented Mara/heard-memory/purchase topic stays safe fallback; a complete quest alone does not manufacture private knowledge. No new persistent flag is written just because the UI was viewed |
| Death/respawn/scene/session leave/rejoin | Work/context/modal caches clear safely; offline saves and session-only online policy preserved. No stale actor reference, friendly Selvek, repeated reward, delayed old text or another character's context |
| Repeated repaint / repeated resolver call | Same immutable snapshot gives same result without side effects. Instrument adapter-caused writes/calls; normal unrelated simulation/regen/autosave must not be misreported as a narrative mutation |
| State change during open UI | Refresh/re-resolve against a coherent current viewer snapshot without duplicating accept/turn-in button invocation, jumping cursor/focus, freezing movement beyond original modal behavior or appending lore every frame |

For native evidence, record exact source/adapter entryPoint, actor component identity, intended viewer/session role, relevant known facts and selected resolution/line ID. Use anonymized test identities; do not publish actual player personal data/auth tokens/join codes. Explain whether state was set synthetically, loaded, earned by normal input or received through server-authorized progression. Do not call direct field setup a natural quest playthrough.

Existing145 quest/41 gameplay/32 spawn passes belong to the earlier Combined02 source. Rerun relevant dependent checks after actual adapter integration rather than transferring them to new code. Combined02's six interrupted/unstarted online cases remain incomplete until genuinely executed on the intended new build. Same-machine Relay cannot establish separate-PC internet acceptance.

## 7. Minimal function responsibilities for the runtime owner

These are implementation responsibilities, not installed symbols or a request for a new framework:

1. **Resolve actor binding:** one exact role/component identity → catalog profile or unknown; no caller profile/case-name shortcut. Expose the derived profile for diagnostics independently of interaction permission.
2. **Capture viewer snapshot:** a typed, read-only per-viewer provider for the allowed facts. Missing mapping returns UNKNOWN; no progression/service/action method is called to “query” state.
3. **Select replacement:** pure actor/request/eligible/context → resolution/lineId/profile, using the agreed frozen catalog. Scope/channel/topic guard and deterministic selection remain explicit.
4. **Apply presentation:** replace only supported text surfaces, preserve operational fallback/actions and avoid shared mutable actor text for viewer-private lines. No recursive original-property fallback.
5. **Emit conformance results:** invoke actual selector for each bundle case, write truthful producer/source/hash metadata and one actual result per case. Keep the expected oracle out of runtime inputs.
6. **Record owner decision separately:** actual acceptance/revision request and per-viewer/merchant/main-surface support are not generated automatically by a test.

## 8. Current status / next handoff

At this bounded read-only check, named acceptance/run paths were absent in the known old Antigravity checkout(3935340) and original shared checkout(1b32349). Only the Codex story checkout was listed under the inspected Production directory. This does not prove no new adapter exists elsewhere or that the other worker has not progressed; request its actual branch/journal rather than infer acceptance from silence or old source.

Next Antigravity step: read the four frozen artifacts/manifest, record an exact supported-schema decision or revision in its own workstream, then independently implement/run the supported consumer and native sequence checks. Next Codex step: preserve this staged candidate, validate any agreed revision once, and only after matching acceptance promote the exact accepted content at the authorized runtime path. No acceptance receipt, runtime result receipt, tests/builds or asset promotion was produced while writing this document.
