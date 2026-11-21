# Emberford kit in the original campaign

The playable map is `Assets/RPG/Scenes/Outpost-Expanded.unity`. Use **Riftbound → World → View the remodeled campaign** to frame Emberwatch in the editor. Play Mode uses the main campaign scene and atlas.

## Windows campaign playtest

Windows builds of the expanded campaign add the Emberwatch Service Court and its walking porter, Three-Chimes Relay at Lantern Rest, Coilbreak Yard, and the Gor'Kresh Trailhead Shelter. The sites are placed when Unity packages the scene. Their notices appear on the map and can be read with **E** in the world. The Adventure Guide lists the stops along its Explore routes.

For the Windows playtest with the existing campaign review saves and offline **F8** tour, run Unity from the project directory with a fresh output name:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.6.2f1/Editor/Unity.exe' -batchmode -projectPath (Get-Location).Path -executeMethod Riftbound.Editor.VillageRemodelPreviewBuild.RunCampaign -gameplayBuildName RiftboundEmberford01 -logFile 'emberford-build.log'
```

Open `Builds/RiftboundEmberford01/Riftbound.exe`. Enter the world offline and press **M** for settlement waypoints or **J** for the Adventure Guide. For scripted editor traversal of the four sites, use the batch entry point `Riftbound.Editor.WorldRoutePlayModeCheck.RunThreeChimesVerification`.

The remodel fits the Blender assets to existing building and prop transforms. Each settlement has a timber, stone, roof, cloth, and metal palette. The old visual meshes are disabled at rebuilt anchors. Where a quest, support surface, or bridge joint relies on the original collision, that collider stays at the authored position. The original NPCs, quests, portals, spawn anchors, and route records retain their scene references.

**Riftbound → World → Rebuild original locations with the Emberford kit** applies the layout to an original campaign scene. The separately authored showcase remains available through the Procedural Kit menu. The World Dressing Pass runs from its menu, so opening the campaign does not regenerate or save the scene.

The player and campaign actor models use the existing Animator as a driver. The replacement rig follows the driver's joint motion, with its weapon geometry weighted to the hands. The playable roster shares eight faction and class models across six races. Dragons, mounts, and other creatures use separate rigs.

To regenerate the Blender models, first make the base kit in a scratch directory, then run the architecture extension against that directory. The extension writes three LODs each for the watchtower, masonry tower, citadel, and war hall.

```powershell
& 'C:/blender/blender.exe' -b --python Tools/Emberford/generate.py -- --output ../emberford-source --no-render
& 'C:/blender/blender.exe' -b --python Tools/Emberford/architecture_upgrade.py -- ../emberford-source
& 'C:/blender/blender.exe' -b --python Tools/Emberford/emberford_textures.py -- ../emberford-source/Textures
```

Unity imports the checked-in FBX, prefabs, materials, and LODs directly.

## Terrain and river finish

**Riftbound → World → Refinish terrain and rivers** applies the authored surface pass to the main campaign. Use **View the restored Westbank river** to inspect the crossing in Scene view. Run the river finish after rebuilding original locations if you are regenerating both passes.

The four terrain tiles share eight biome textures with matching normal maps. River terrain adds a wet shoreline layer. Westbank flows from a high spring cascade through the existing crossing to the lower woodland pool. Willow Brook has its own tapered course and water material. Both bridges retain their original collision support and approach grade.

```powershell
& 'C:/blender/blender.exe' -b --python Tools/Emberford/paint_world_surfaces.py -- 'Assets/RPG/ProceduralKit/World Surfaces/Textures'
```
