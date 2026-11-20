# Westbank ferry landing

The south bank gained a cedar landing, a shore ramp, a moored skiff, a roadside shelter and a cargo hoist. The ferry landing is a place to explore on foot. The skiff is scenery. Mounts can still use the existing bridge.

## Unity placement

Place **Westbank Ferry Landing** at **(-270, 0, 41)** with zero rotation and unit scale in a copy of Outpost-Expanded. The prefab was fitted to the existing 3.25 m river surface. From the west end of the bridge, go south just east of the hoist, then follow the bank around the spur to the ramp. The editor fixture checks the bridge in both directions using foot and mount collision proxies. It checks the road-to-dock spur on foot.

The combined village preview build stages the prefab. The Willow Rest cache, bridge collision, river mesh, and terrain are authored independently.

## Remodeled campaign access

The newer campaign reshapes the bank below the ramp. A short raised connector loads beside the existing ferry landing in the expanded scene. From the west bridgehead, go around the west side of the shelter and follow the lower bank east to the connector. This stays south of the bridge retaining wall. The final turn leads down the original ramp to the dock.

Re-export the connector against the existing Westbank atlas by running Blender 5.2 with `build_bank_connector.py -- ./WestbankBankConnectorExport Assets/RPG/Art/WestbankFerryLanding/westbank_atlas.png`.

## Editable source

Export into a new folder with Blender 5.2.

```powershell
& blender --background --factory-startup --python-exit-code 1 --python Art/WestbankFerryLanding/build_ferry_landing.py -- ./WestbankFerryLandingExport
```

The builder produces five original FBX and GLB models, a 768 × 768 atlas, a packed Blender scene and a display render.
