# Riftbound — latest adventure update

Launch `Builds/Windows/Riftbound.exe`, or open `Assets/RPG/Scenes/Outpost-Updated.unity` in Unity.

Terrain / combat-facing update (2026-10-29):
- Skeleton models now face the same direction as AI movement/aiming. Enemies keep facing a moving target during melee attacks and heavy-strike preparation. Melee damage checks the forward arc as well as line of sight.
- Replaced the flat meadow with an editable Unity Terrain: rolling hills, ridges, grass/earth/rock layers and normal maps. The playable borders remain 160x300m; surrounding terrain provides the distant landscape.
- Villages and encounters sit on graded platforms: crypt +3.5m, watchtower +6m, Greenhollow +5.5m and Ancient Grove +10m. Roads and the watchtower courtyard conform to the ground; props, enemies, signs and rewards follow their new elevations.
- Added 4x MSAA, 2048px soft sun shadows, screen-space contact shading, ACES color grading, subtle bloom/vignette and atmospheric fog. Original PC rendering assets remain intact; the new preset/assets are in Assets/RPG/Terrain.
- Restored the village's textured stone paving, matched the sky's ground/horizon colors to the atmosphere, and replaced the rectangular brook with a curved, rippled surface.
- Wildlife follows ground height while wandering. Mounted collision sweeps allow the graded paths while still checking obstacles.
- TerrainPolishResult.txt records 36 new terrain/facing/render checks, with 63 previous progression/equipment/mount checks rerun. TerrainPolishBuildResult.txt is the latest build result. TerrainSurvey.txt reports road grades and actor grounding. No Play Mode gameplay tests were run.
- Terrain-*.png and Enemy-Fixed-*.png are edit-mode previews. Check the actual combat/riding feel yourself in the new build.
- Future asset priorities and search terms: ASSET_REQUESTS.md.

Greenhollow / mounts update (2026-10-29):
- Playable ground expanded from 160 x 180 to 160 x 300 metres, with the northern edge and distant scenery moved outward.
- Follow the connected north road to Willow Crossing, Greenhollow Riding Post and the Ancient Grove. Added a bridge, shallow brook, open lodges, market props, provisioner, wildlife, three frontier scouts and two caches.
- Z: summon the Emberwatch Horse after a 1.5-second cast outside combat; press Z again to cancel or dismount. Movement, damage and combat actions interrupt the summon. Combat actions/damage dismount the rider.
- Horse runs at 8 m/s; the Frontier Zebra runs at 9 m/s. C toggles slow travel. Mounted WASD turns the animal toward the chosen direction. The camera raises to frame the rider.
- V: cycle available mounts while on foot. Horse is available immediately. Discover all three Greenhollow scouting signs with E to unlock the zebra, earn 30 gold and 125 XP in total. M and J show the new route/progress.
- Mounts use their own native animations and measured hoof stride. A separate native Brute riding pose aligns the rider to the moving saddle, hides the axe, and restores the standing model/capsule when dismounting.
- Six persistent caches and two provisioners now exist. Frontier scouts do not advance Abe's road-raider/captain objectives.
- Twenty-eight free Quaternius models downloaded: seven animals plus twenty-one props; original license/source records are in Assets/RPG/CHARACTER_ASSETS.md.
- MountExpansionResult.txt records mount/scene/save checks. MountExpansionBuildResult.txt records that earlier pass's build. MountPreviewResult.txt and Mount-*.png show edit-mode rig previews, not gameplay captures.
- Gameplay riding, jump feel and encounter balance remain for your testing. Existing character saves are retained.

Test the new content: enter a character, stand in the open and press Z. Ride north over the bridge, follow the three green scouting markers on M, and press E at each. Press V after unlocking the zebra, then Z to ride it. Try combat/damage dismounting, Z cancellation, C slow travel and save/reload. In an already-open Unity editor, reopen the saved Updated scene to load the expanded map.

Western Crypt update (2026-10-29):
- New western trail and ruined courtyard with three sentinels and the optional Crypt Warden.
- Activate three blue Runestones with E. They are marked on the map (M); the third awakens the Warden.
- The Warden uses a heavy strike warning. Move out of the red circle before it lands.
- Warden's Executioner (+12 weapon damage) and Signet of the Crypt (+45 max HP) expand the catalog to eleven items. The signet is in the sealed reliquary behind the Warden.
- Q: hold Guard, rooted, for up to 3 seconds. Incoming damage is halved after armor. Recharge: 6 seconds after release.
- H: channel a Hearthstone for 5 seconds to return to Emberwatch. Requires safe ground outside combat. Movement, damage, attacks or opening a window interrupt it. Recharge: 60 seconds after success.
- J: Quest Journal with all main chapters, side objectives and a shortcut to the map.
- New damage/heal/rune sounds respect the existing volume setting. Skeleton toe bones now participate in ground alignment.
- Runestone progress and crypt rewards persist per character, with older saves supported. The optional Warden does not count as Abe's captain.
- DemoPassResult.txt: 16 scene/progression/utility checks. EquipmentResult.txt: 12 equipment checks. DemoPassBuildResult.txt: current build result. No Play Mode gameplay tests were run for this update.
- AnimationAuditResult.txt: 11 native states checked for intended looping and stationary actor root; pose renders are diagnostic. Native run/walk visual quality still needs your gameplay feedback. DemoPassValidation.txt records final source/build synchronization.
- Nearby enemies now display their names; casters and heavy attackers have different name colors.

Quick test route: enter a character, try Q near an enemy, inspect J, follow the cyan map markers, activate the three stones, defeat the Warden, open his chest, equip the rewards with I, then leave combat and hold still after pressing H. Reload the character to check saved progress.

Latest movement update:
- Play from legacy Outpost automatically opens the current adventure, avoiding its old Humanoid controller.
- Movement clips follow actual velocity when reversing direction or slowing into a walk.
- Gait phases align across forward/backward walk/run. Cadence uses planted-foot samples and ramps smoothly.
- Backpedal speed is 65% of forward travel. Ground offsets are calibrated separately for each gait.
- Training dummy in the village resets health and gives no quest, XP or gold rewards. Use TAB and attack keys to practice.
- MovementResult.txt records contact speed, phase and vertical offset measurements. Gameplay testing remains with you.

Latest equipment update:
- Eleven gear items across weapon, armor and trinket slots. Open inventory with I, select an item, then Equip outside combat.
- Gear changes weapon damage, armor mitigation and maximum health. Equipping health bonuses does not heal.
- Raider Cleaver drops from the road Stalker, Boneguard Plate from the camp Boneguard, Captain's Greataxe from the boss.
- Village/camp/tower caches contain Leather Vest, Traveler's Charm and Watchtower Cuirass. The tower cache requires captain defeat.
- Abe's final quest grants Emberwatch Axe and Sigil. Existing saves receive their earned items when loaded.
- Rowan buys bone shards for 2 gold each and sells a Leather Vest for 25 gold.
- Inventory and equipped slots persist per character.
- EquipmentResult.txt records 12 isolated edit-mode checks; EquipmentBuildResult.txt records the Windows build.

Changes:
- Walk/run changes preserve gait phase. Feet calibrate before movement with a smaller ground offset.
- Jump uses an airborne pose with height supplied by physics. Native hit and death clips are baked for Brute.
- Strikes keep their original target when TAB changes selection. TAB cycles enemies.
- Damage/healing numbers show combat results.
- Quartermaster Rowan in the village sells potions (10 gold), tempers the axe (+4 damage, 45 gold) and offers rest (5 gold).
- Six loot caches give gold, XP and a potion once per character, including the Western Crypt reliquary and two Greenhollow caches.
- M opens the world map with player, enemies, caches and current quest objective.
- Upgrades and opened caches persist. Saves keep a previous-version backup.

Controls: WASD run, C toggle walk, SPACE jump, TAB cycle targets, 1/LMB strike, 2 whirlwind, 3 heal, 4 potion, Q hold guard, H return to camp, Z summon/dismount, V change mount, E interact, I inventory, J journal, M map, F5 save, ESC close/pause, R recover after defeat.

Validation: animation data, real death body collapse, single-source jump height, scene references, old-save compatibility, save roundtrip, repeated atomic writes and Windows compilation. Gameplay testing remains with you.

Original Outpost and the previous executable remain available. The pre-update scene is in Assets/RPG/Scenes/Backups/Outpost-Updated-before-polish.unity.
