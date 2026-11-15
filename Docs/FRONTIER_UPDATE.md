# Frontier update — 2026-11-10

## Changes
- Corrected reversed skeleton visuals, retaining the correct native axis for other models.
- Three exploration sites with persistent discoveries and supply caches:Westbank Overlook, Bloodrock Vista and Wyrmcrest Lookout.
- Cartographer Elara at Greenhollow Post offers Charting the Frontier to either faction. Survey all three sites, then return for75 base gold,150XP and Traveler's Charm. Existing discoveries count; waypoints follow the route.
- Fixed zebra unlock/counts, regional boss quest eligibility and cross-character faction progress leakage.
- Editor startup no longer regenerates the world automatically.

## Validation
107 passing headless gameplay assertions,0 failures. All24 playable prefabs checked; all four Undead variants retain their required yaw. GPU Idle/Run/Attack samples rendered for Warrior/Mage/Rogue, with selected samples inspected. Full animation feel and audio listening remain manual checks.

## Play route
1. Enter the expanded world. M opens the atlas; green Scout markers show discoveries.
2. Travel to Greenhollow Post and speak with Cartographer Elara, southwest of the post centre.
3. Accept Charting the Frontier. Follow its waypoint to each site; E discovers markers and collects nearby supply caches.
4. Return to Elara for the quest reward. Reload to check persistence.
5. Check Undead movement/combat and enemy orientation. Zebra still requires Willow Crossing, Greenhollow Post and Ancient Grove.

## Build
Windows standalone succeeded: Errors: 0, Warnings: 665; Time: 00:00:27.1288989. Output: `Builds\FrontierUpdate/Riftbound.exe`. Copied executable SHA-256 matches; source and validated C# files match. Existing imported shader/plugin warnings remain.
