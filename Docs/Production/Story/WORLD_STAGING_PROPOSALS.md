# First-five world staging — SC08 / SC10 / CF11

Prepared17 November2026 on story/codex-content-20261117 from published base8ad9071820c2fbed32fbb728122ffb2f02980888. **Read-only proposal: no Unity import, scene/terrain/prefab/material/FBX save, actor move, runtime change or asset promotion.** No unfinished Greenhollow tool/layout was copied. Antigravity has not accepted the narrative schema; all new readable/work behavior remains outside Assets and pending that contract.

Canon: Docs/Lore/RIFTBOUND_LORE_BIBLE.md and NPC_STORY_CONTRACTS.md. Runtime/quest ownership stays with Antigravity; Codex owns this external staging specification and later specifically approved world edits. Text below proposes readable/gossip content, not new inventory, objectives, reward conditions or persistent flags.

## 1. Source evidence and preservation fence

Inspected the serialized Outpost-Expanded scene, relevant script/meta files, FantasyProps license/source metadata, current world layout/route sources and prior committed placement receipts. Scene parent transforms were composed for selected actors; actor positions are not guessed from labels or raw child-local coordinates.

- Scene: Assets/RPG/Scenes/Outpost-Expanded.unity, SHA256 **7C9A1244D6C63C6F383311EAA041405AADA73B7BD373493D2C3393CD9BA2AF63**.
- Active protected western terrain: Assets/RPG/Terrain/ReaverCamp/Westbank crossing protected.asset, GUID **9a025ac28e774a54781f605d50def2f8**, SHA256 **5CDA8E104878069D3FC9282B68611C33B2ABFBCC73EAD9453C70D69802C4C66D**.
- Existing preservation contract: **813 trees,15 prototypes**, paint/details/holes/outside-footprint heights and Terrain/TerrainCollider references. Those counts were verified in prior terrain/build work; this text-only pass did not load TerrainData or recount them natively. No vegetation/height/paint edit is proposed.
- Prior support/clearance receipts and images: Docs/Production/SETTLEMENT_SUPPLIES_CHANGESET.md; Evidence/Supplies02/placement-reload.txt and before/after PNGs; Docs/Integration/INTEGRATION_HANDOFF.md. Historical native passes are labelled as such, not rerun here.
- Source-only parsing establishes identities, references and authored transforms. It does **not** establish current rendered bases, clip availability, collider overlap, camera sightline, grounded runtime spawns or actual normal-input traversal. Those new checks are NOT RUN.

Every later world application must recheck current scene/model hashes, coordinate scene ownership and use a narrow reviewed change list. Never run WorldExpansionSetup.Generate, WorldDressingPass.Run, a terrain regenerator or RiverReaverCamp.Ensure as an authoring shortcut. Existing camp initialization must not be invoked to replace an authored instance.

## 2. Exact first-five anchors

World coordinates are X/Y/Z. G(x,z) means a future native WorldRegions.GroundHeight sample at that point, **not a measured Y in this pass**. Existing root positions remain unchanged; no routine may walk a service root away from its recorded position.

| Identity / stable binding | Exact source anchor | State and limits |
| --- | --- | --- |
| Abe; QuestGiver.npcName=`Abe` | Scene GO351378609, transform351378610, name `Abe - Outpost Quest Giver`; world **(-3,0,2)**, yaw approximately180°, unit scale. Parent `04 - Gameplay` is identity transform. | Serialized QuestGiver, not a new TownNPC. Three index-based main missions plus separate `alliance_highspire_dispatch` remain intact. Actual PlayerProgress quest-window strings must use the later adapter; serialized prose alone is not the whole UI. |
| Quartermaster Rowan; Merchant.npcName=`Quartermaster Rowan` | Scene GO2124646072, transform2124646075; world **(-7,-0.05,-2)**, yaw approximately120°, root-level. | Existing merchant/service, not an observed new quest giver. Service state/purchases unchanged. Keep compatible name key; display alias must not alter identity matching. |
| Provisioner Mira; Merchant.npcName=`Provisioner Mira` | Scene GO1934746513, transform1934746514; world **(-42,13.05,404)**, yaw approximately180°. Local(-25,5.400336,160) plus Greenhollow parent(-17,7.6496644,244). | Existing merchant at Greenhollow, not another Emberwatch merchant. No quest credit, purchase or remembered-debt persistence simply from reading a greeting. |
| Cartographer Elara; ExplorationSites.CartographerName=`Cartographer Elara` | **Runtime source**, not serialized actor: `WorldRegions.Greenhollow + (-6,0,-3)` → **(-46,G(-46,397),397)**. EnsureCartographer creates a Human/Mage visual and CapsuleCollider height1.9/radius0.4/centerY0.95. | No serialized `npcName: Cartographer Elara` component found in this scene. Verify actual runtime existence/position before attaching anything. Preserve `frontier_cartography`, `river_reaver_bounty`, survey IDs and prerequisites. Do not add a second scene Elara to make the parser find her. |
| Captain Selvek; enemyId=`westbank-reaver-captain` | **Runtime source**, not serialized actor: RiverReaverCamp.Position(-283,15.46,78), then Ground() → **(-283,G(-283,78)+0.03,78)**; yaw180°, Undead/Rogue. | No serialized captain/lockbox identity components found. This is a current initialization contract, not a claim the object is absent in Play Mode. Preserve captain, guard, bounty and lockbox bindings; do not create a TownNPC quest-giver clone or change his ancestry. |

Current source interaction ranges differ by service: Abe's CanTalk uses4m; NearbyMerchant/CanTrade uses3m; TownNPC.FindNearby in the shared interaction resolver uses3.1m. Keep these rules and the existing nearby-ally-revive priority. Staging clearance goals below are more conservative placement requirements, not permission to change interaction distances.

**Identity caution:** scene object names, display titles and quest keys are not interchangeable. Existing NPC name comparisons remain compatibility keys. Selvek is a hostile EnemyController; dialogue/work proposals cannot grant him normal service interaction or override defeat state.

## 3. Existing supported props versus proposed anchors

### Existing supply/rest group — retained

All five below are active children of `Codex reference-route supplies`, whose world root is(0,0,0). Their wrapper/source links were rechecked in serialized PrefabInstance records. Prior native support/reload/clearance verification exists; this pass only confirmed bytes/references/transforms.

| Existing object / scene GO | Exact root position / yaw | Existing model | Historical support evidence |
| --- | --- | --- | --- |
| Emberwatch packed supply crate /200631426 | **(-13,-0.1137005,-5)** /10° | Crate_Wooden.fbx | Footprint height spread0.000m |
| Emberwatch outbound supply crate /846813285 | **(-11.6,-0.1137005,-4.85)** /90° | Crate_Wooden.fbx | Spread0.000m |
| Emberwatch provision barrel /1700066615 | **(-12.8,-0.1695689,-6.4)** /15° | Barrel.fbx | Spread0.000m |
| Willow Rest resting bench /204203575 | **(-287,6.035638,46)** /90° | Bench.fbx | Spread0.043m |
| Willow Rest provision barrel /2022193063 | **(-285.2,5.982034,45.8)** /0° | Barrel.fbx | Spread0.000m |

No new crate pile, barrel row or duplicate bench is needed for the first story slice. Existing renderer/collider budget remains five/five for this group. Do not turn its inert barrels into loot containers merely because a clue refers to supplies.

Other retained exact anchors:

- Rowan's nearest inspected cart: `Stall_Cart_Empty`, GO801222735, world **(-9.5,-0.15,1.5)**. Its root is about4.30m horizontally from Rowan. This is an inspection target, not a table within hand reach.
- Mira's nearby cart: `Stall_Cart_Empty`, GO1922398119, world **(-43,13.05,408)**, about4.12m from Mira. Existing `Table_Large`, GO25054085, world **(-50,13.05,406)** is about8.25m away. Do not animate Mira writing on that table from her fixed service root.
- Western cache: `Willow Rest river cache`, GO1510449846, chestId **willow-rest-cache**, root **(-291,7.192302,52.5)**; original footing retained. Current standing approach contract is approximately(-285,50)→(-288,51)→(-288.7,52) in X/Z. New props/readables may not occupy that approach or its lid sweep.
- Bridge deck reference: crossing center approximately **(-255.46,6,50)**; retained rails/supports/protected banks. No clue is placed on the deck or used to hide terrain seams.

**Do not conflate locations:** western Willow Rest cache/bench is distinct from the existing northern discovery ID `willow-crossing`, whose serialized marker is **(-12,10.7,377)**. Elara's `westbank-overlook` survey is a third binding, created near(-241,G,44). No marker move/rename, objective reassignment or new scouting credit follows from a ferryman story.

### Existing asset/provenance ledger

All model paths below start **Assets/RPG/Art/FantasyProps/FBX/**. Current local License_Standard.txt identifies the standard free Quaternius pack and **CC0 1.0**. Existing family material mapping is reused through WorldArchitectureBuilder.ResolveImportedMaterial and persisted Assets/RPG/Materials; no source import/material swap, new download, paid asset or provenance assumption from a filename.

| Model | GUID | Use and limit |
| --- | --- | --- |
| Crate_Wooden.fbx |59e1a2f0783af5140a1ffb7e5a94bb7e | Existing supply wrappers; no duplication requested |
| Barrel.fbx |90d2234944d96924fb7b168bdabe4f3d | Existing supply/rest wrappers; no new loot logic |
| Bench.fbx |43a68b156262ae64da862ab0e8b94a93 | Existing rest landmark; no sit interaction assumed |
| Stall_Cart_Empty.fbx |8aa624655a4ff5449b1ac038d4ba9cc9 | Existing Rowan/Mira carts; do not move root or infer collider/top bounds from pivot |
| Table_Large.fbx |6252350cd8326274184423c482b02391 | Existing Greenhollow work table; too far for stationary Mira hand work |
| Book_Stack_1.fbx |fb2c88258221ffd479fdbcb5eae06ac6 | Optional later readable visual; admitted asset exists, but no new instance/size/support/pose verified |

Snapshot model SHA256: crate1CEA14E2D937586186CC8F0C0CA01B33FD999030622BC4C33133C7D91C257916; barrelAB4FA392EB5122FCFFA9AED225E4136CBF2D34CEB8C53141CD951C1D69EBDB2D; benchCD55D1C21EE5055A2002739268014138F3CFE06C7A5CB9FCEE3F22C55196D6F3; book stack0E17CD5A69A74992070BFBF2D08D0DD7DE2D1066632AD3ED3CA11E7F992D7DF3; cart0D3F61B9F5D572B6F67ECDD3D0EE15CE2A618A43953F69BE5FC154D0BDB6D6AF; table64560001286E169879DEA986BD2E7295CCAF2269B0FBC1E7B2BC8B802F693689. Recheck before any later preview/application.

## 4. SC08 stationary work / attention contract

These are behavior specifications for Antigravity's later SC09 adapter, not installed routines or approved animation clips. Existing service/enemy roots never move. Use a validated Idle fallback first; do not play Attack as writing, packing or weighing. An animation state named Idle is not proof of every NPC rig/clip/bone. Per-identity rig, clip and socket inspection remains NOT RUN.

| Identity | Plausible stationary activity / exact attention anchor | Minimal supported fallback | What is not yet justified |
| --- | --- | --- | --- |
| Abe | Watches the relay approach; occasional attention toward the west departure **(-15,G,0)** and back to nearby speaker | Existing idle at(-3,0,2), preserved root yaw; a future visual-only bounded gaze if rig supports it | No patrol path, stick/hand attachment, new sitting pose or root teleport. Do not force a full-body turn toward a distant target while feet stay planted |
| Rowan | Visually checks existing cart at **(-9.5,-0.15,1.5)** and cargo cluster at **(-12.2,G,-5.1)**; thinks in received/unloaded quantities | Idle/attention plus context line in existing trade/gossip flow after schema agreement | No reaching4–7m, imaginary writing desk, moving cart or quest completion from looking at it |
| Mira | Checks stock on cart **(-43,13.05,408)**; speaks of fair supplies and remembered debts | Idle/attention at(-42,13.05,404); services stay useful after all story beats | No hand-sorting goods4m away, no navigation to the table8.25m away, no persistent debt flag created in prose |
| Elara | Compares a claimed crossing measurement with her route; attention to existing Greenhollow marker **(-40,13.05,400)** or currently selected survey explanation | Existing Human/Mage idle at runtime-created root; dialogue text can describe comparison without fake hand animation | No duplicated Elara, floating map attached to an unverified hand socket, or reveal of every anchor before relevant evidence |
| Selvek | In noncombat idle, watches the established southern approach **(-285,G,65)** as a former ferrymaster imposing violent tolls | Preserve existing EnemyController home/idle/hostile behavior; story bark only through an approved host-controlled state hook | No civilian service loop, negotiation/recruit/spare branch, mid-fight book reading or replacement captain model |

Pause/resume requirements for future implementation:

1. Pause work before accepted interaction/service/modal focus, nearby combat threat, root disable/death/despawn or session/scene transition. New idle behavior must never steal movement/cursor/input ownership from the existing interaction system.
2. Resume only after a short proposed1–2s clear interval and after the runtime owner validates that interval; restore the same root position/rotation and legitimate idle, not a fresh actor. This delay is a hypothesis, not measured current behavior.
3. Multiple players may use services independently. A cosmetic shared attention target cannot close another player's shop or expose the host's private quest state. Dialogue selection uses the interacting player's current state.
4. Online work/hostile intent timing is host-owned if shared; local look/UI is presentation only. Do not run combat or reward logic on every observing client. Existing initialization/replication is preserved; do not assume all current Ensure-created objects should be converted to new network prefabs.
5. No new persisted work timer, schedule, conversation memory or state counter until the accepted schema and runtime contract explicitly require it. No animation-event reward or quest progression.

## 5. SC10 localized clues — zero-addition first

First implementation should be text in the agreed existing conversation/readable surface, with no new geometry or collider. Book_Stack_1 is an optional later visual, not a reason to block content on an unverified tabletop. Proposed readable keys below are catalog proposal IDs only, not loot IDs, quest objectives or new saved flags. Parent canon/dialogue owner approves wording/state availability.

| Proposal key | Existing identity/anchor / proposed readable surface | Exact narrative purpose and safe implementation boundary |
| --- | --- | --- |
| ST01 / relay-watch | Abe root(-3,0,2); existing dialogue surface, no new prop | Abe notices a familiar seal and a bad patrol plan. He does not explain the Choir or Lattice origin at the opening. A Seventh Wagon reference is a state-aware personal clue, not a new tutorial gate |
| ST02 / received-three | Rowan; existing cargo crateGO200631426 at(-13,-.1137005,-5), or NPC conversation | Proposed receipt: “Received three. Unloaded two. Do not balance this column for me.” It establishes a discrepancy and Rowan's voice. No new supply count, inventory item, gold or quest credit |
| ST03 / fair-account | Mira; existing cartGO1922398119 at(-43,13.05,408), or current service conversation | Proposed stock note: “Paid in blankets. Keep the customer's name off the road copy.” Shows care and unusual supply demand without naming a frightened customer or inventing a tracked debt |
| ST04 / crossing-twice | Elara runtime root(-46,G,397); agreed gossip/readable surface | Proposed field note: “Same bank. Same mark. Two bearings. I will ask the ferryman before I correct either.” It distinguishes measurement from explanation; does not auto-complete frontier_cartography or turn frontier-vistas into a fourth marker |
| ST05 / western-rest | Existing bench(-287,6.035638,46) and barrel(-285.2,5.982034,45.8); no new prop required | Optional environmental readable about repairing a harness/setting a load down. It makes refuge useful without relabelling the cache as a quest object or changing the northern willow-crossing marker |
| ST06 / excluded-name | Selvek's existing defeat/bounty/lockbox context; river-reaver-lockbox source anchor(-283,G+0.03,82) | Proposed record: “Passenger nineteen: Selvek. Status: lost. Claim refused.” It connects denied recognition to forced tolls; not a new proof-item requirement, surrender ending or reason to erase any player name/save |

The Selvek record must remain morally clear: he was excluded and harmed; violence is still his choice. Do not turn a Returned visual into a corruption detector. Do not promise an interactable/readable attached to the lockbox before existing captain-defeat/loot availability and the adapter contract are respected.

### Optional physical readable transforms — PROPOSED, NOT VERIFIED

- ST02 book visual: parent to existing packed-crate wrapperGO200631426, **local X/Z(0,0), yaw relative0°, imported scale initially1**. Set localY from measured native crate top minus measured book bottom plus0.01m. ThatY and book dimensions are presently UNKNOWN; do not substitute “rootY+one metre” as a support pass. Candidate reading stand-off X/Z **(-11.4,-5.2)**, Y=G+controller clearance; collision/interaction/camera NOT RUN.
- ST03 book visual: parent to existing Mira cartGO1922398119, **local X/Z(0,0), yaw relative0°, imported scale initially1**. Native cart support surface/book footprint must be measured; cart pivot13.05 is not its countertop. Candidate stand-off X/Z **(-41.4,407.2)**; mesh/collider/support/apron/native view NOT RUN. No extra cart/sack/bucket cluster from the old Greenhollow WIP is admitted.
- ST04: **no physical map/book anchor selected**. Source proves an Elara root/capsule, not a nearby reachable desk or hand socket. Keep the first field note in the agreed UI until a current native survey supplies a supported surface and rig-compatible pose.
- ST05/ST06: no extra paper/book instance on sloped terrain, cache lid or bridge rail. Use an agreed readable text presentation with existing availability; do not glue a new collider to a moving lid.

If native bounds make a proposed book absurdly small/large, reject that candidate before saving. Do not silently change the original FBX import scale, duplicate materials or cover a support problem with clutter. Any new source/asset family requires its own provenance/admission review; the current free-assets-only constraint persists.

## 6. Reference routes, clearance and cameras

Retain actual paths rather than drawing through houses or signposts:

- Emberwatch departure X/Z **(-15,0),(-25,-5),(-34,-11)**.
- Highspire gate approach X/Z **(-200,-60),(-202.5,-57.5),(-204,-15),(-225,0)**. Existing highspire mesh asset Assets/RPG/Terrain/OvernightRoutes/Highspire north footway.asset, GUID93e511bd8ef3b374a8ea9ae029caf26b; cobble material Assets/RPG/Terrain/WorldProduction/Highspire footway cobblestone.mat, GUID4c2716c77ac177448a443bf22b69609a. No new throne/noticeboard/quest giver is placed in this leg.
- Westbank approach continues **(-240,50),(-260,50),(-285,50)**. Keep both bridge side lanes and cache standing approach; Reaver approach continues to existing camp(-283,78).
- Greenhollow arrival **(-17,368),(-27,386),(-36,395)** ends beside the marker(-40,400), not inside it. Elara/Mira remain nearby services at their distinct anchors.

Important serialization trap: all three authored footway GameObject roots are(0,0,0), with world-space geometry in their mesh assets. Their root transform is **not** the footway's physical location. Never attach a road clue at origin merely by parenting it to that root.

Later placement requirements, not newly executed checks:

- Keep at least2.2m horizontal clearance beyond a new prop's projected radius from reviewed travel centrelines; check mounted swept volumes and ordinary camera, not center distance alone.
- Protect a4m clear service/giver/marker apron beyond prop footprint and an unobstructed valid interaction stand-off. Explicitly include **Merchant** as well as TownNPC/QuestGiver. The inspected old supply Check() loops TownNPC/QuestGiver but not Merchant; do not advertise that source guard as universal merchant-apron proof. Existing Rowan supply distances are comfortably outside the reference4m zone, but future Mira proposals still need actual bounds.
- Preserve mount-display clearance at Greenhollow and retain turn/summon/dismount room. New readable interaction must not outrank a downed ally, hide the existing E hint or add a second permanent HUD panel.
- Sample support across an actual footprint, reject steep/unsupported locations, check solid overlap/tree footprint/lid sweep. Do not grade terrain or clear trees to force an anchor.
- Propose no new dynamic light. Reuse existing lighting; distinguish authoring gizmos from rendered markers before asking for changes.

| View | Exact camera/look-at proposal | Evidence class / unresolved limits |
| --- | --- | --- |
| Emberwatch loading | Camera(-6,6,2), target(-11.5,.4,-5.5) | Reuses a historically captured supply composition; no fresh story/clip/ordinary OrbitCamera acceptance |
| Emberwatch departure | Camera(-31,11,-5), target(-11,.5,-2) | Historically captured open departure; new readable scale/legibility still NOT RUN |
| Abe dialogue/work | Camera(-8,4,-7), target(-3,1.3,2) | New exact view proposal; obstruction, framing and foot support NOT RUN |
| Highspire gate | Camera(-191,29,9), target(-207,12,-28) | Previously used diagnostic angle; does not establish every near-well human camera sweep |
| Western rest | Camera(-279,12,39), target(-288,6.3,46) | Reuses prior native bench/barrel composition; readable/support additions not tested |
| Mira service/work | Camera(-34,20,412), target(-42,14.3,404) | New proposal only; check cart occlusion, service access and text in actual gameplay |
| Elara | Camera(-43,G(-46,397)+5,389), target(-46,G(-46,397)+1.3,397) | Source-derived variableY; runtime root/native scene needed before capture |
| Selvek approach | Camera(-276,20,67), target(-283,G(-283,78)+1.4,78) | New proposal only; known nearby foliage can occlude. No canopy/branch removal authorized |

Capture matched settings/camera before/after any approved local change. Frozen UI-hidden environment views are separate from live NPC gestures, interaction, input and performance. Reaver has known foliage occlusion despite zero final camera-sphere overlaps in the old audit; keep that limitation visible.

## 7. CF11 three-role reference encounter + Selvek

This is a content/runtime contract, not live placement approval. Reuse existing actor archetypes; do not pull authored actors out of their quest regions, duplicate their IDs, or regenerate the camp. Source current positions and class flags are recorded so the runtime owner can build an isolated development fixture without pretending these roles are already co-located.

| Role / exact existing source exemplar | Source position and observed behavior flag | Prototype requirement / preservation |
| --- | --- | --- |
| Desperate raider / `road-raider-1`, sceneGO1753818275 | **(-3,.23833,36)**, Forest raider1 under04-Gameplay | Existing melee archetype; one readable commitment/recovery. Keep real tutorial target and quest identity; a dev fixture must not award its live credit |
| Tollwarden / `westbank-reaver-2` source in RiverReaverCamp | **(-276,G(-276,74)+.03,74)**, Undead/Warrior guard; campGuard=true, attackDamage7/interval2s in source | Shield/hook behavior is a proposed adapter, not current verified capability. Keep actual guard/home/ID; no new shield attachment or pull through rails before art/collision checks |
| Bell-hexer / `crypt-guard-2`, sceneGO2142293494 | **(-42,3.500214,68)**, Crypt Hexer; ranged=true, attackRange9 | Existing ranged archetype supports a cast prototype. Bell prop/animation is not identified among inspected FantasyProps assets; do not claim a lantern is a bell. Actor remains in its crypt; an isolated fixture uses approved temporary identities |
| Ranged comparison / `road-raider-4`, sceneGO673876209 | **(3,1.649707,65)**, ranged=true, attackRange9 | Useful existing basic ranged comparison without borrowing a live crypt actor. It is not already a canon bell-hexer |
| Selvek champion / `westbank-reaver-captain` | **(-283,G+0.03,78)**, Undead/Rogue; captain sourceHP260/reduction2, attack16/interval2.4s, heavyAttacker=true | Reuse current captain, guards, home and lockbox linkage. A new champion behavior has to preserve death/claim/session authority and authored model identity |

Do not infer role from an ID/title alone: serialized `dungeon-hexer-1` is currently named Scarlet Blood Cleric with ranged=false/attackRange2.1, so it is **not** evidence of an existing ranged hexer. Display/canon aliases cannot become ability selectors.

### Fixture contract before any world change

1. Antigravity owns a temporary, bounded development fixture using the existing melee/ranged/camp archetypes and ephemeral test IDs isolated from quests/rewards/saves. It must not invoke an Ensure/installer to recreate the real camp. The exact fixture implementation and schema are agreed before coding; this file does not authorize an extra framework.
2. Learn each role alone, then a three-enemy composition: raider pursues cargo with a clear strike; warden braces/commits a bounded hook; hexer raises a readable interruptible disruption channel. VFX/castbar can prototype the missing bell cue, labelled temporary; no claim of a finished bell mesh/gesture.
3. Proposed paper layout for a future isolated arena: local origin(0,0,0), entry(0,0,-6), raider(-3,0,0), warden(2,0,2), hexer(0,0,6), retreat(0,0,-9); floor/support is supplied by the fixture, not unmeasured current terrain. These are **local fixture coordinates**, not a new world tile or proof of a safe live map pocket.
4. A later permanent pocket needs a specific off-road preview chosen from current native terrain/tree/camera/service evidence. No world coordinates are approved here: the existing road raiders, crypt caster and Reaver guards are in different places. Do not silently teleport them into one camp to satisfy the role count.
5. Selvek is a separate champion step at his current camp: southern approach/retreat stays(-285,50)→(-283,78), guard/lockbox anchors retained. Proposed shield/hook and heavy recovery must fit current terrain/trees and host-authority limits; no new height edits or arena rails. No optional surrender/recruit/nonlethal ending is implemented by this staging plan.

Online decisions—aggro, target selection, ability commit, damage/guard/dodge/interrupt, loot and defeat—remain host-owned. Client scene/cosmetic copies render the same intended warning; they cannot create rewards or independently decide a hit. Preserve the current session-only persistence/host-exit policy and ordinary offline saves. A readable text is zero reward and zero new flag until a later explicit design approves otherwise.

Acceptance must record all four classes, ordinary on-foot entry/retreat, mounts approaching/dismounting, cover for host and remote targets, camera/foliage, mixed-faction2–4player join/late join, defeat/reset and unique lockbox claims. Existing Combined02 authority/progression/late-join/full-acceptance/client-leave/capacity tests remain incomplete as disclosed in its handoff; this read-only proposal does not close them.

## 8. Review/application checklist and next action

Current result: source anchors, identities, model GUIDs/hashes/provenance and route contracts recorded; no new scene/readable/work/combat behavior applied.

**NOT RUN:** fresh Unity actor enumeration; actual Elara/Selvek runtime spawn/grounding; every current prop mesh/support/collider; new book surface/scale; NPC rig/clip/socket proof; work pause/resume/multiple-player behavior; reading interaction/quest/save behavior; foot/mount/camera traversal; tree/vegetation collision; matched story preview; live performance; online fixtures. Historical support and regression receipts remain labelled historical.

Required gate sequence:

1. Parent and Antigravity agree SC02 catalog/schema, identity mapping, existing state vocabulary and service/hostility boundaries. Keep drafts outside Assets; no presumed acceptance from document presence or silence.
2. Prefer ST01–ST04 in existing agreed text UI with zero world changes. Source content validator is not gameplay proof; Antigravity owns adapter and actual existing UI surfaces.
3. Obtain explicit scene ownership and narrowly reviewed approval for any optional physical readable or permanent encounter placement. Recheck current source hashes/actual bounds and reject bad anchors before saving. No source import/actor recreation/terrain cleanup.
4. Run native support/clearance/gesture/camera checks and relevant unchanged regression/online checks after approved implementation; record actual passes, failures and NOT RUN. Preserve original assets and all813trees/15prototypes data.
5. Release scene ownership and hand off exact transforms, affected source IDs, test/build identity and limitations. Do not present these proposals as installed NPC work, new quests or a completed combat encounter.

Next safe Codex action is a current actor/asset/route review preview after schema/ownership coordination; next Antigravity action remains its agreed final online/combat correctness packet before the narrative adapter and work state implementation. No tests/builds ran while creating this document.
