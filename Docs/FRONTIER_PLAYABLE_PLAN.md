# The Roads That Remember

Build a cohesive 20–30 minute adventure across the existing frontier. The player starts with Abe, follows a disputed shipment through Westbank, then heads east to the Brass Hollows to shut down a siphon. Make the adventure work from a fresh character and a reloaded save before expanding the map.

## First pass — make the new version playable

- Integrate the latest frontier source with the existing movement and combat fixes. Check camera steering, walk/run, jumps, queued abilities, cooldowns, and mount transitions together.
- Retain quest, encounter, faction and profile IDs. Follow existing solo and online save boundaries. A quest advertised for co-op must have a working completion path for host and guest.
- Walk the western loop from Rowan through Split-Seal Layby and Tallykeepers’ Rest. Show the current objective in the journal and on the map, then give the return trip a visible payoff.
- Make the Brass Hollows a short, readable dungeon. Give each room a clear purpose. Teach the steam tell in a safe first room, combine it with a fight in the next, then let Foreman Veyl test it.
- Give Veyl a readable windup, a committed strike, and a recovery window. Keep encounter outcomes authoritative and reward turn-in idempotent.

## Art and atmosphere

- Use the available CC0 medieval kit for a compact group of crates, grain sacks and lanterns along the western route. Keep the existing road clearance and terrain data. Preview fit before placement.
- Use warm lanterns and indigo cloth for western stops, then cold shale and hot brass for the Hollows. Players should recognize which area they are entering from silhouettes and lighting.
- Give the dungeon entrance, final chamber and return to the surface distinct audio beats. Reuse existing sound assets and preserve headroom for combat cues.
- Keep the HUD sparse. Show the active objective, target tell and action feedback when useful. Evaluate the atlas at 1280×720 and 1920×1080.

## Release checks

- Verify fresh start, quest acceptance, combat, both turn-ins, reload, and no duplicate reward.
- Verify on-foot and mounted travel along both western and eastern approaches. Include turning, camera clearance and stepping off ledges.
- Verify the dungeon’s entry, exit, death, retreat and reset paths. Test host and guest separately before promoting online dungeon support.
- Build a new Windows candidate and keep the prior playable build. Record what was tested on the exact source.
- Expand next into Lantern Rest, Coilbreak Yard and the summit lead-in after this route plays end to end.
