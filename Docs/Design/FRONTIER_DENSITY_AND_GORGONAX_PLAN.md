# Frontier density and Gorgonax arena

Planning baseline **18a33fa**, the combined Emberford client and dedicated server. The playable scene is `Assets/RPG/Scenes/Outpost-Expanded.unity`. Coordinates below are Unity world metres `(x, z)`. North is positive Z and west is negative X in the atlas. Heights, clearances and final placements are pending survey.

## Map

```text
                                NORTH ↑

  Pinewatch Relay ─ proposed ridge trail ─ Greenhollow Post          Wyrmcrest
         │                                         ╲
    Duskwood Cathedral                       North valley route
         │                                          ╲
    Deathknell              Whisperwood Exchange     Breaker Works ─ Camp Bloodrock
                                      │                                      │
    Highspire ─ Emberwatch ─ Ashwood / Redridge road ─────────────── Rustwater
                        ╲                                                 ╱
                        Emberford ───── proposed southern trail ─ Cinderroad
```

The routes are schematic. Each new site should give the player a recognizable silhouette from the approach, something people are doing on arrival, and a reason to leave by a different direction. Let the existing terrain dictate sightlines, elevation changes and the spaces between sites.

| Site | First survey window | What the player finds | Existing content to protect |
| --- | --- | --- | --- |
| **Pinewatch Relay** · northwest | Around `(-223, 400)`. Survey X −265 to −185, Z 355 to 455. | A mossy woodland watch post, rope hoist, lanterns, a ridge overlook and two short routes. | Duskwood Cathedral and Scarlet Crypts access, Deathknell approaches, Greenhollow arrival and existing vegetation. |
| **Whisperwood Exchange** · centre | Around `(35, 250)`. Survey X −45 to 55, Z 205 to 275. | A cart shelter and raised tally stall at a forest bend, with travelers and an optional short detour. | Ashwood quest corridor, Greenhollow road, existing Three-Chimes/Lantern Rest proposals, and clearance around the Z=224 terrain seam. |
| **Cinderroad Waystation** · southeast | Around `(262, -55)`. Survey X 205 to 325, Z −95 to 5. | A stepped freight yard with a crane, covered storehouse and an uphill beacon. | Emberford’s current route and spawn access, Rustwater’s existing salvage yard, the map edge and the Redridge road. |
| **Breaker Works** · Gorgonax | Existing Redridge Volcanic Crater around `(180, 300)`. Survey the floor, rim and both approaches. | A broken basalt arena, furnace arch and ruined pressure tower above the crater rim. | Existing boss, floor support, chase space, loot anchor, and travel between Bloodrock and the north valley. |

## Site briefs

### Pinewatch Relay

- **Approach:** Survey a woodland spur north of Duskwood Cathedral and a second, ridge-following path toward Greenhollow. Use one visible lantern at each real decision point. Reject a candidate route that sends players through the crypt entrance, crosses water without a verified crossing, or needs a cut into the hill.
- **Layout:** Put the shelter at the natural bench and the watch deck on a nearby rise. Use stepped timber supports, retaining stones and one short stair. Keep a mounted route around the site, with a foot detour to the overlook.
- **Activity:** A watchkeeper checks the signal, a messenger walks between the sheltered post and lantern, and a woodworker shifts supplies between the rack and repair bench. Start with two civilian routes that reuse current movement components. One observation about a late courier can point toward the next site.
- **Environmental story:** At the lower path, a snapped relay line points uphill. The repair bench holds its replacement. The overlook shows where the watchkeeper was trying to signal. Any proposed quest stays independent from the existing Lantern Rest patrol story.
- **First playable slice:** One shelter, one overlook, two walkers, a readable approach in both directions and one optional short conversation.

### Whisperwood Exchange

- **Approach:** Create a pullout beside the Greenhollow road and a short return toward Ashwood. Leave the main lane open for mounted traffic. Survey both sides of Z=224 even if the site itself fits on one tile.
- **Layout:** A tall canopy marks the stop. Put the loading shelf on the slope, with a low wall between traders and road traffic. Place the rest bench where the player can see both exits. Use the existing timber, slate and canvas palette.
- **Activity:** A porter unloads parcels, a traveler waits under the awning, and a scout takes a short route to the treeline. Make the detour optional with a worn side path and a readable return route.
- **Environmental story:** Show the ordinary difference between the freight tally and what the player sees being unloaded. Keep existing Western Road and Three-Chimes content independent and avoid adding a duplicate quartermaster, caravan quest or relay.
- **First playable slice:** One canopy, a loading bay, two or three walkers and a single safe point of interest off the through road.

### Cinderroad Waystation

- **Approach:** Evaluate a contour-following route from Emberford to the yard, then a return trail north onto the existing Redridge road toward Rustwater. Survey the southern boundary and potential blind turns. Start with a spur if the second connection needs a new bridge or earthworks.
- **Layout:** Use two natural benches. Freight and a short loading ramp go below, with a beacon and view over the road above. Use retaining walls and piers to fit the slope. Keep one broad route between benches and a stair as a shortcut.
- **Activity:** A wagon crew inspects cargo, a guard walks the near approach, and a salvage buyer uses the sheltered stall. Keep a distant optional salvage or encounter pocket off the main route. Protect traffic lanes from idle wagons and path endpoints.
- **Environmental story:** Build the yard around old quarry freight. Cracked stone crates and repaired wheel braces show the movement of heavy loads. Rustwater remains the industrial settlement and the new site serves the journey.
- **First playable slice:** Lower yard, crane silhouette, uphill light, two walkers, and the route toward Emberford. Add the Rustwater connection after both traversals check out.

## Breaker Works

**Build an open arena with a tower on the rim.** The tower gives the crater a skyline landmark while Gorgonax fights on the existing floor. Treat the ruin as a quarry pressure station whose guardian still mistakes visitors for extraction crews.

1. **Approach from Bloodrock:** Reveal a furnace arch and broken pressure tower ahead of the crater. Use a short service ramp or stair that follows the existing grade. Keep an unobstructed retreat toward the Volcanic Trail.
2. **Approach from the north valley:** Give the player a clear view of the arena and a rim path toward the southeast exit. Survey a through route outside the existing encounter wake distance. Start the safe-route survey beyond **35 m** from the boss, with extra margin, and validate actual pulls and leash behavior in play.
3. **Fight floor:** Keep the existing floor collider and boss anchor. Low broken walls and battered pylons should frame the ring without trapping the boss or blocking camera sweeps. Keep the centre open and spacing sufficient to move away from the existing meteor impact radius. Place tall geometry, stairs, decorative vents and overhead pipes outside the main dodge space.
4. **Defeat and exit:** Preserve `worldboss-gorgonax`, `worldboss-gorgonax-chest`, the crater landmark and the reward flow. Confirm the hoard is visible, grounded and reachable after the kill. Test reset after retreat and re-entry through both approaches.
5. **Later mechanics:** The current encounter triggers near a local player and directly applies stomp and meteor damage to that player. Review authority before changing the fight on a dedicated server. Plan server-owned start, target snapshots, damage, resets and rewards, with matching telegraph timing for all clients. Visual steam and warning lamps are scenery until that contract exists.

Keep the first playable arena focused on geometry and readability. Existing combat ranges and timings provide a test baseline, including the reported 14 m stomp radius and 6 m meteor impact radius after a 2.4 s warning. These are source readings, so measure effective clearance, pull behavior and actual playable timing before accepting an arena layout.

## Terrain and placement

- Keep the four authored terrain tiles, protected Reaver terrain and existing trees. Preserve the previously recorded 813-tree / 15-prototype baseline on the protected tile and recheck it in the new preview.
- Sample actual ground under foundations and path edges. Record elevations, side slopes, abrupt steps, seam crossings, tree and rock conflicts, and candidate approaches. Put walks, piers, stairs and ramps on the terrain or move a module to a better bench.
- Preserve recognizable ridges, depressions and uneven ground. Use local retaining geometry and short bends. Any proposal that needs a specific tree relocation, terrain paint edit or collision change gets a separate exact review.
- Keep current biome, faction and spawn resolution in the first slice. `WorldRegions.ZoneName` currently labels much of the northwest window Greenhollow Valley and the southeast window Emberwatch Outpost. Use local landmark names on signs and waypoints. Review zone changes and audio transitions separately if they become useful.
- Prefer existing materials and verified asset variants. Author a small original set where the existing kit has a gap, especially the quarry tower, broken retaining segments and load platforms. Give each camp a distinct silhouette and local colors.
- Base functional routes on each existing assembly and collider. Thin décor and background trees can frame paths. Every post, wagon wheel and stair collider near a travel lane gets an actual clearance check.

## Production order

| Phase | Deliverable | Close the phase when |
| --- | --- | --- |
| Survey | Per-site snapshot with candidate coordinates, terrain and obstacle measurements, existing assets or prefab references, road ties, and one alternate. | Establish the NW corridor and boss constraints first. Record exclusions and verify the protected source hashes. |
| Greybox | Pinewatch and Breaker Works preview using temporary placement and current terrain. | Check both approaches, camera, footing, mounted travel, through route, retreat, boss path and chest access. Record blockers at exact positions. |
| First art pass | Pinewatch original shelter and overlook. Breaker Works basalt ring, rim tower and primary arch. | World silhouettes read from the current routes. Texture, light and ground contact look consistent in rendered daylight and dusk. |
| Density pass | Whisperwood and Cinderroad layouts, civilian routes, signs and small environmental stories. | Map labels and signs agree, civilians loop without blocking the road, and each site offers one action or reason to explore. |
| Gameplay integration | Reviewed placement and a combined offline/realm playtest. | Existing quests, IDs, loot and saves survive. Movement and terrain checks pass. Boss multiplayer work has its own release gate. |

## Verification checklist

- Record checked source revision, scene and terrain hashes, candidate prefab authoring paths, and build receipt. Distinguish a scene scan, edit-mode preview and actual play.
- Run the approach and return on foot and mounted. Check collision, camera, facing and readable signage at each fork, including Z=224 near the centre route.
- Test the quiet route past the crater, intentional pull, dodge paths, rider dismount lane, retreat/reset and post-fight chest. If encounter behavior changes later, exercise two clients, reconnect and a single reward per eligible player on the matching dedicated server.
- Check the new layouts in the packaged level. Confirm frontier and existing starter quest routes still work. Compare scenery cost against the matching baseline at fixed camera positions.
- Keep the current combined playtest available while prototype evidence is reviewed. Scene, terrain and existing prefab application are separate from this plan.

## Source references

- `Docs/WORLD_ZONES.md` and `Assets/RPG/Scripts/WorldRegions.cs` supply the map bounds and existing zone anchors.
- `Assets/RPG/Editor/WorldExpansionLayout.cs` supplies established road candidates and terrain layout. `WorldMapRoutes` in the active scene should be checked before placement.
- `Docs/Design/FRONTIER_MAP_PLAN.md` covers related sites and earlier quest proposals. Confirm which preview sites the selected build stages.
- `Docs/Lore/RIFTBOUND_LORE_BIBLE.md` and `Assets/RPG/Scripts/WorldBossEncounter.cs` supply the quarry guardian story and current boss behavior.
