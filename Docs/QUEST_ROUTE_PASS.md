# Quest and loot update — 2026-10-31

## Changes

- Only `road-raider-*` enemies advance Abe's patrol, and only `ashwood-captain` advances his captain objective. Training dummies, racial enemies and optional bosses are excluded from main-quest targeting and credit.
- Only the captain is gated by acceptance of Abe's final mission. Optional world bosses keep their independent availability.
- Quest completion chat reports the title and XP of the quest just completed. The tracker stops at 3/3 after completion.
- Enemy XP is awarded on defeat; gold, bone shards and equipment are collected from the remains with E. The Human gold bonus applies once. Item drops use catalog IDs.
- Unclaimed loot and its remaining two-minute gameplay timer are saved. Partial claims and corpse removal persist, so reloading cannot replay already collected rewards.
- Legacy saves retain equipment earned under the previous automatic-reward system. New saves do not auto-grant equipment from an enemy's defeat record.
- Party loot-roll wins are saved immediately.
- The journal's **Track Main Quest** button starts a live minimap waypoint. It follows the next target, returns to Abe for hand-in and clears after campaign completion. A manual waypoint overrides it.

## Validation

- Unity 6000.6.2f1 Windows build: **Succeeded, 0 errors, 653 warnings**; reported size 313,821,151 bytes.
- Output: `Builds/QuestUpdate/Riftbound.exe`, with all accompanying player data. Previous builds were retained.
- **57 passing headless assertions** in `Assets/RPG/Editor/QuestRouteSmokeTest.cs`.
- Checks include startup, character handoff, virtual-keyboard movement, optional-boss availability, correct road targets, all three Abe quests, final equipment, repeat-hand-in protection, quest chat, loot claims, reloads, legacy saves and waypoint updates.
- Defeats are simulated using `Combatant.Damage`; this is a progression/control-flow regression, not a combat-balance or animation acceptance test.
- No RPG runtime exceptions were captured on the tested route. Changed runtime scripts matched the build copy, and the copied executable's SHA-256 matched the build output.
- Existing imported shader/plugin warnings remain unresolved. No source scenes or model assets were rewritten.

## Manual play route

1. Open the QuestUpdate executable and enter a character.
2. Accept Abe's quest; press J and choose Track Main Quest.
3. Defeat road raiders. Press E near remains and collect the loot.
4. Save/reload with some rewards unclaimed, and verify the remaining loot is available without repeating collected coins.
5. Complete the supplies and captain missions and claim the final rewards.
6. Check combat feel, animations, terrain traversal and mounted travel.

## Follow-up findings

The read-only audit also identified older issues outside this pass: malformed-save recovery does not consult the `.bak` file before falling back, and static talent ranks may leak between character selections. Address those with isolated recovery/character-switch tests next. Pending party-roll state itself is not persisted by this pass; completed winning rolls are saved.
