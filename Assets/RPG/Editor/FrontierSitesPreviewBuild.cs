using System;
using System.Linq;
using System.Text;
using Riftbound.WorldAmbient;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Riftbound.Editor
{
    /// <summary>
    /// Stages spacious, lived-in Frontier Small Towns & Strongholds using Riftbound's normal-mapped
    /// ProceduralKit hero buildings (Wayfarer_Inn, Timber_Cottage, Emberford_Smithy, Bell_Chapel,
    /// Highspire_Citadel_Hall, Briar_War_Hall, Briar_Camp_Hut) plus our custom Blender 5.2 FBX structures
    /// (BreakerWorks_MagmaArch, BreakerWorks_PressureTower, BreakerWorks_RimBastion, Frontier_CoastalLighthouse).
    /// Zero edits to authored scenes or terrains on disk.
    /// </summary>
    public static class FrontierSitesPreviewBuild
    {
        const string ArtRoot = "Assets/RPG/Art/";
        const string KitPrefabs = "Assets/RPG/ProceduralKit/Prefabs/";
        const string Materials = "Assets/RPG/ProceduralKit/Materials/";

        public static readonly Vector3 BreakerWorksCenter = new Vector3(180f, 0f, 300f);
        public static readonly Vector3 WhisperwoodCenter = new Vector3(35f, 0f, 250f);
        public static readonly Vector3 CinderroadCenter = new Vector3(262f, 0f, -55f);
        public static readonly Vector3 ThornwatchCenter = new Vector3(-275f, 0f, -110f);
        public static readonly Vector3 SkyreachCenter = new Vector3(355f, 0f, 420f);
        public static readonly Vector3 SunstoneCenter = new Vector3(115f, 0f, -115f);
        public static readonly Vector3 IronmawCenter = new Vector3(390f, 0f, 90f);
        public static readonly Vector3 FrostpineCenter = new Vector3(-270f, 0f, 520f);
        public static readonly Vector3 RedtideCenter = new Vector3(365f, 0f, -110f);
        public static readonly Vector3 WillowmereCenter = new Vector3(-270f, 0f, 125f);

        public static GameObject StageAll(Scene scene, StringBuilder report = null)
        {
            report ??= new StringBuilder();
            var terrains = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>()).ToArray();
            if (terrains.Length == 0) throw new InvalidOperationException("No authored terrain in scene.");

            Physics.SyncTransforms();

            var root = new GameObject("Frontier Density Sites");
            SceneManager.MoveGameObjectToScene(root, scene);

            StageBreakerWorks(root.transform, terrains, report);
            StageWhisperwoodTown(root.transform, terrains, report);
            StageCinderroadTown(root.transform, terrains, report);
            StageThornwatchTown(root.transform, terrains, report);
            StageSkyreachTown(root.transform, terrains, report);
            StageSunstoneAbbeyRuins(root.transform, terrains, report);
            StageIronmawQuarryTown(root.transform, terrains, report);
            StageFrostpineTown(root.transform, terrains, report);
            StageRedtideHarborTown(root.transform, terrains, report);
            StageWillowmereHamlet(root.transform, terrains, report);

            return root;
        }

        // =========================================================================
        // 1. BREAKER WORKS — BLENDER FBX AMPHITHEATER FOR GORGONAX (180, 300)
        // =========================================================================
        static void StageBreakerWorks(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Breaker Works Arena");
            root.transform.SetParent(parent, false);

            // Bespoke chamfered Blender Magma Arches at SE (198, 279) & NW (164, 316) — >=8.8m clear opening!
            PlaceFbx(root.transform, "FrontierOverhaul/BreakerWorks_MagmaArch.fbx", "FurnaceArch_Lintel", 198f, 279f, -42f, terrains, scale: 1.0f, footingSize: 0f);
            PlaceFbx(root.transform, "FrontierOverhaul/BreakerWorks_MagmaArch.fbx", "NWGate_Arch", 164f, 316f, 138f, terrains, scale: 0.92f, footingSize: 0f);

            // Bespoke Blender Ruined Pressure Tower on NE rim (204, 322, R ~ 32.5m)
            PlaceFbx(root.transform, "FrontierOverhaul/BreakerWorks_PressureTower.fbx", "PressureTower_Drum", 204f, 322f, -135f, terrains, scale: 1.05f, footingSize: 0f);

            // 10 Bespoke Blender Rim Bastions circling R=25.5m (keeps R<=19.5m combat floor & camera sweep 100% clear)
            for (int i = 0; i < 10; i++)
            {
                float deg = i * 36f;
                if ((deg >= 295f && deg <= 345f) || (deg >= 115f && deg <= 158f)) continue;
                float rad = deg * Mathf.Deg2Rad;
                float rx = 180f + Mathf.Cos(rad) * 25.5f;
                float rz = 300f + Mathf.Sin(rad) * 25.5f;
                PlaceFbx(root.transform, "FrontierOverhaul/BreakerWorks_RimBastion.fbx", $"RimBastion_{i}", rx, rz, -deg + 90f, terrains, scale: 1.0f, footingSize: 0f);
            }

            // Quiet Rim Bypass Trail lanterns (>= 42m from Gorgonax)
            Vector3[] bypass = { new(140, 0, 344), new(160, 0, 347), new(180, 0, 346), new(205, 0, 340), new(224, 0, 332), new(230, 0, 260) };
            for (int i = 0; i < bypass.Length; i++)
                PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_TrailLantern.fbx", $"BreakerBypass_Lantern_{i}", bypass[i].x, bypass[i].z, 0f, terrains, footingSize: 0f, addColliders: false);

            AddWaystop(root.transform, "Breaker Works", new Vector3(206f, 0f, 272f),
                "Breaker Works Crater. Gorgonax holds the open basalt floor below. Enter through the Magma Arch or follow the outer rim lanterns.", terrains);
            report.AppendLine("Staged Custom FBX Breaker Works Amphitheater.");
        }

        // =========================================================================
        // 2. WHISPERWOOD EXCHANGE SMALL TOWN (35, 250) — HERO BUILDINGS + PLAZA
        // =========================================================================
        static void StageWhisperwoodTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Whisperwood Exchange");
            root.transform.SetParent(parent, false);

            // Central Market Square well + spacious 15-18m layout of normal-mapped Hero Buildings
            PlaceKitPrefab(root.transform, "Wayfarer_Inn", 49f, 263f, -125f, terrains, 10.5f, 8.5f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", 24f, 264f, 35f, terrains, 7.6f, 7.0f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", 52f, 240f, -65f, terrains, 7.6f, 7.0f);
            PlaceKitPrefab(root.transform, "Emberford_Smithy", 23f, 236f, 60f, terrains, 8.2f, 6.8f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketShelter.fbx", "WW_MarketHall", 35f, 250f, 0f, terrains, footingSize: 6.8f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketWell.fbx", "WW_Well", 35f, 238f, 0f, terrains, footingSize: 2.8f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_SupplyWagon.fbx", "WW_Wagon", 24.5f, 248.5f, 20f, terrains, footingSize: 0f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_RoofedNoticeBoard.fbx", "WW_NoticeBoard", 30.5f, 249.5f, 90f, terrains, footingSize: 0f);
            PlaceFbx(root.transform, "Animals/Horse.fbx", "WW_Horse", 21.5f, 245.5f, 40f, terrains, footingSize: 0f, addColliders: false);

            PlaceKitPrefab(root.transform, "Street_Lantern", 4f, 247f, 0f, terrains, 0f, 0f, addColliders: false);
            PlaceKitPrefab(root.transform, "Street_Lantern", 19f, 248f, 0f, terrains, 0f, 0f, addColliders: false);
            PlaceKitPrefab(root.transform, "Street_Lantern", 42f, 255f, 0f, terrains, 0f, 0f, addColliders: false);

            AddTownNpc(root.transform, "Quartermaster Brev", "Whisperwood Exchange", "Human", "Warrior", new Vector3(34.5f, 0f, 253.8f),
                "Whisperwood Exchange grew into a full market town! Inn, cottages, smithy, and fresh well water right in the plaza.", terrains);
            AddTownNpc(root.transform, "Innkeeper Talia", "Wayfarer Inn", "Human", "Mage", new Vector3(43.5f, 0f, 259.5f),
                "Rooms and warm stew at the Wayfarer Inn, traveler.", terrains);
            AddTownNpc(root.transform, "Smith Garrett", "Exchange Smithy", "Human", "Warrior", new Vector3(28.0f, 0f, 238.0f),
                "Horseshoes and axle pins forged fresh for the north convoys.", terrains);

            AddWaystop(root.transform, "Whisperwood Exchange", new Vector3(30.5f, 0f, 249.5f),
                "Whisperwood Exchange — Bustling crossroads market town with Wayfarer Inn, cottages, smithy, and plaza well.", terrains);

            AddCivilianWalker(root.transform, "Whisperwood Exchange Porter", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(32.0f, 0f, 242.5f), new Vector3(38.0f, 0f, 242.5f), new Vector3(38.0f, 0f, 244.8f), new Vector3(32.0f, 0f, 244.8f)
            });
            AddCivilianWalker(root.transform, "Whisperwood Waiting Courier", "Human", "Rogue", 1.45f, 2.0f, terrains, new[]
            {
                new Vector3(31.0f, 0f, 255.2f), new Vector3(37.5f, 0f, 255.5f), new Vector3(38.0f, 0f, 257.5f), new Vector3(31.5f, 0f, 257.0f)
            });
            AddCivilianWalker(root.transform, "Whisperwood Treeline Scout", "Human", "Rogue", 1.40f, 2.4f, terrains, new[]
            {
                new Vector3(41.2f, 0f, 245.5f), new Vector3(43.2f, 0f, 247.5f), new Vector3(43.6f, 0f, 250.5f),
                new Vector3(43.2f, 0f, 253.5f), new Vector3(41.2f, 0f, 252.0f), new Vector3(41.0f, 0f, 248.5f)
            });

            report.AppendLine("Staged Whisperwood Exchange Hero-Building Town.");
        }

        // =========================================================================
        // 3. SE TOWN — CINDERROAD IRONWORKS (262, -55)
        // =========================================================================
        static void StageCinderroadTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Cinderroad Waystation");
            root.transform.SetParent(parent, false);

            // Hero foundry buildings + Wayfarer Inn + Cottages + Salvage Crane + Forge Arch
            PlaceKitPrefab(root.transform, "Emberford_Smithy", 244f, -55f, 75f, terrains, 8.2f, 6.8f);
            PlaceKitPrefab(root.transform, "Wayfarer_Inn", 275f, -40f, -145f, terrains, 10.5f, 8.5f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", 242f, -38f, 120f, terrains, 7.6f, 7.0f);
            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_SalvageCrane.fbx", "FreightCrane_Boom", 262f, -58f, -18f, terrains, scale: 1.25f, footingSize: 5.5f);
            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_SortingWorkshop.fbx", "Storehouse_Walls", 256f, -68f, 0f, terrains, footingSize: 7.5f);
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_KilnFurnace.fbx", "Smelter_Chimney", 245f, -68f, 25f, terrains, scale: 1.35f, footingSize: 5.2f);
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_ForgeArch.fbx", "BastionGate_Lintel", 254f, -39.5f, 0f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketWell.fbx", "CR_Well", 260f, -49f, 0f, terrains, footingSize: 2.8f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_SupplyWagon.fbx", "CR_Wagon", 253f, -52f, 70f, terrains, footingSize: 0f);

            Vector3[] spur = { new(86, 0, -36), new(130, 0, -34), new(175, 0, -36), new(220, 0, -41), new(254, 0, -43) };
            for (int i = 0; i < spur.Length; i++)
                PlaceKitPrefab(root.transform, "Street_Lantern", spur[i].x, spur[i].z, 0f, terrains, 0f, 0f, addColliders: false);

            AddTownNpc(root.transform, "Foreman Darek", "Cinderroad Ironworks", "Human", "Warrior", new Vector3(256.8f, 0f, -49.5f),
                "Welcome to Cinderroad Ironworks! Our grand smithy, inn, freight crane, and smelter keep the southeastern frontier supplied.", terrains);
            AddTownNpc(root.transform, "Blacksmith Torin", "Ironworks Smithy", "Dwarf", "Warrior", new Vector3(249.5f, 0f, -58.5f),
                "Fresh iron ingots straight out of the kiln!", terrains);

            AddWaystop(root.transform, "Cinderroad Ironworks", new Vector3(258f, 0f, -48f),
                "Cinderroad Ironworks — Southeast foundry town with Emberford Smithy, Wayfarer Inn, freight crane, and smelter yard.", terrains);

            AddCivilianWalker(root.transform, "Cinderroad Wagon Inspector", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(262.0f, 0f, -45.5f), new Vector3(266.5f, 0f, -46.0f), new Vector3(267.0f, 0f, -49.0f), new Vector3(262.5f, 0f, -48.5f)
            });
            AddCivilianWalker(root.transform, "Cinderroad Approach Guard", "Human", "Warrior", 1.40f, 2.0f, terrains, new[]
            {
                new Vector3(247.5f, 0f, -44.5f), new Vector3(251.0f, 0f, -44.5f), new Vector3(251.0f, 0f, -47.5f), new Vector3(247.5f, 0f, -47.5f)
            });
            AddCivilianWalker(root.transform, "Cinderroad Salvage Buyer", "Human", "Rogue", 1.35f, 2.4f, terrains, new[]
            {
                new Vector3(269.0f, 0f, -50.5f), new Vector3(274.5f, 0f, -51.5f), new Vector3(274.0f, 0f, -55.5f), new Vector3(268.5f, 0f, -54.5f)
            });

            report.AppendLine("Staged Cinderroad Ironworks Hero-Building Town.");
        }

        // =========================================================================
        // 4. SW TOWN — THORNWATCH CITADEL (-275, -110)
        // =========================================================================
        static void StageThornwatchTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Thornwatch Citadel");
            root.transform.SetParent(parent, false);

            // Citadel Keep + Gatehouse + Coastal Lighthouse + Inn + Cottage + Smithy + Fountain!
            PlaceKitPrefab(root.transform, "Highspire_Citadel_Hall", -280f, -112f, 90f, terrains, 11.5f, 11.5f);
            PlaceFbx(root.transform, "HighspireDistrict/Highspire_Gatehouse.fbx", "Thornwatch_GateLintel", -262f, -103f, 90f, terrains, footingSize: 6.5f);
            PlaceFbx(root.transform, "FrontierOverhaul/Frontier_CoastalLighthouse.fbx", "Thornwatch_KeepShaft", -294f, -118f, 0f, terrains, footingSize: 0f);
            PlaceKitPrefab(root.transform, "Wayfarer_Inn", -289f, -96f, 140f, terrains, 10.5f, 8.5f);
            PlaceKitPrefab(root.transform, "Emberford_Smithy", -274f, -126f, 0f, terrains, 8.2f, 6.8f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", -261f, -120f, -65f, terrains, 7.6f, 7.0f);
            PlaceFbx(root.transform, "HighspireDistrict/Highspire_Fountain.fbx", "Thornwatch_Fountain", -271f, -103f, 0f, terrains, footingSize: 3.2f);
            PlaceFbx(root.transform, "Animals/Horse.fbx", "TW_HorseA", -267f, -124f, -30f, terrains, footingSize: 0f, addColliders: false);
            PlaceKitPrefab(root.transform, "Street_Lantern", -235f, -84f, 0f, terrains, 0f, 0f, addColliders: false);
            PlaceKitPrefab(root.transform, "Street_Lantern", -250f, -94f, 0f, terrains, 0f, 0f, addColliders: false);

            AddTownNpc(root.transform, "Captain Vaelen", "Thornwatch Citadel", "Human", "Warrior", new Vector3(-267.5f, 0f, -103.5f),
                "Thornwatch Citadel stands watch over the southwest coast—Citadel Hall, coastal lighthouse, inn, smithy, and fountain square.", terrains);
            AddTownNpc(root.transform, "Quartermaster Sera", "Citadel Market", "Elf", "Rogue", new Vector3(-278.5f, 0f, -97.5f),
                "All sea lanes past the cape are marked by our lighthouse.", terrains);

            AddWaystop(root.transform, "Thornwatch Citadel", new Vector3(-266.0f, 0f, -103.5f),
                "Thornwatch Citadel — Southwest garrison fortress town with Citadel Hall, coastal lighthouse, inn, smithy, and plaza fountain.", terrains);

            AddCivilianWalker(root.transform, "Thornwatch Sentry", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(-271f, 0f, -96.5f), new Vector3(-273.5f, 0f, -96.5f), new Vector3(-276f, 0f, -96.5f),
                new Vector3(-276f, 0f, -98.5f), new Vector3(-273.5f, 0f, -98.5f), new Vector3(-271f, 0f, -98.5f)
            });
            AddCivilianWalker(root.transform, "Thornwatch Quartermaster Runner", "Human", "Rogue", 1.45f, 2.0f, terrains, new[]
            {
                new Vector3(-267.5f, 0f, -110.5f), new Vector3(-267.5f, 0f, -113.5f), new Vector3(-267.5f, 0f, -115.5f),
                new Vector3(-269.5f, 0f, -115.5f), new Vector3(-269.5f, 0f, -113.5f), new Vector3(-269.5f, 0f, -110.5f)
            });
            AddCivilianWalker(root.transform, "Thornwatch Rampart Guard", "Human", "Warrior", 1.35f, 2.4f, terrains, new[]
            {
                new Vector3(-254.0f, 0f, -101.0f), new Vector3(-252.5f, 0f, -101.0f), new Vector3(-252.5f, 0f, -103.0f),
                new Vector3(-252.5f, 0f, -105.0f), new Vector3(-254.0f, 0f, -105.0f), new Vector3(-254.0f, 0f, -103.0f)
            });

            report.AppendLine("Staged Thornwatch Citadel Hero-Building Fortress Town.");
        }

        // =========================================================================
        // 5. FAR NE MOUNTAIN TOWN — SKYREACH AERIE (355, 420)
        // =========================================================================
        static void StageSkyreachTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Skyreach Aerie");
            root.transform.SetParent(parent, false);

            PlaceKitPrefab(root.transform, "Bell_Chapel", 356f, 425f, 180f, terrains, 8.8f, 9.2f);
            PlaceKitPrefab(root.transform, "Highspire_Masonry_Tower", 344f, 420f, 45f, terrains, 6.2f, 6.2f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", 369f, 422f, -110f, terrains, 7.6f, 7.0f);
            PlaceFbx(root.transform, "SilverglenArrivalCourt/Silverglen_ReadingPavilion.fbx", "Skyreach_Pavilion", 364f, 408f, -45f, terrains, footingSize: 6.0f);
            PlaceFbx(root.transform, "FrostpeakTrailStation/Frostpeak_PassForge.fbx", "Skyreach_Forge", 347f, 407f, 20f, terrains, footingSize: 4.5f);
            PlaceFbx(root.transform, "SilverglenArrivalCourt/Silverglen_TrailBeacon.fbx", "Skyreach_Beacon1", 347f, 412f, 0f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Storm-Watcher Lyra", "Skyreach Aerie", "Elf", "Mage", new Vector3(353f, 0f, 413f),
                "Skyreach Aerie pairs its ancient bell chapel with our masonry spire and alpine pass-forge.", terrains);
            AddTownNpc(root.transform, "Scout Kael", "Ridge Watch", "Human", "Rogue", new Vector3(361f, 0f, 414f),
                "You can spot Breaker Works smoke from our tower!", terrains);

            AddWaystop(root.transform, "Skyreach Aerie", new Vector3(350f, 0f, 412f),
                "Skyreach Aerie — Northeast mountain sanctuary town with Bell Chapel, masonry watchtower, cottages, and alpine pass-forge.", terrains);

            AddCivilianWalker(root.transform, "Skyreach Wind-Scout", "Elf", "Rogue", 1.40f, 2.2f, terrains, new[]
            {
                new Vector3(358f, 0f, 411f), new Vector3(360.5f, 0f, 411f), new Vector3(360.5f, 0f, 414f), new Vector3(358f, 0f, 414f)
            });

            report.AppendLine("Staged Skyreach Aerie Hero-Building Mountain Town.");
        }

        // =========================================================================
        // 6. SOUTH RUINS — SUNSTONE ABBEY (115, -115)
        // =========================================================================
        static void StageSunstoneAbbeyRuins(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Sunstone Abbey Ruins");
            root.transform.SetParent(parent, false);

            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_WatchArch.fbx", "Abbey_EntryArch", 115f, -95f, 0f, terrains, scale: 1.4f, footingSize: 0f);
            PlaceKitPrefab(root.transform, "Bell_Chapel", 115f, -138f, 0f, terrains, 8.8f, 9.2f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_BrokenArcade.fbx", "Abbey_ArcadeL1", 104f, -106f, 90f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_BrokenArcade.fbx", "Abbey_ArcadeR1", 126f, -106f, -90f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_BrokenArcade.fbx", "Abbey_ArcadeL2", 104f, -120f, 90f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_BrokenArcade.fbx", "Abbey_ArcadeR2", 126f, -120f, -90f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceKitPrefab(root.transform, "Ruined_Wall", 107f, -132f, 20f, terrains, 0f, 0f);
            PlaceKitPrefab(root.transform, "Ruined_Wall", 123f, -132f, -20f, terrains, 0f, 0f);

            report.AppendLine("Staged Hero Bell_Chapel & Cloister Sunstone Abbey Ruins.");
        }

        // =========================================================================
        // 7. FAR EAST OUTLAW PIT — IRONMAW QUARRY (390, 90)
        // =========================================================================
        static void StageIronmawQuarryTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Ironmaw Quarry Architecture");
            root.transform.SetParent(parent, false);

            PlaceKitPrefab(root.transform, "Briar_War_Hall", 406f, 90f, -90f, terrains, 10.5f, 9.0f);
            PlaceKitPrefab(root.transform, "Briar_Camp_Hut", 398f, 105f, -45f, terrains, 6.2f, 6.2f);
            PlaceKitPrefab(root.transform, "Briar_Camp_Hut", 398f, 75f, 45f, terrains, 6.2f, 6.2f);
            PlaceKitPrefab(root.transform, "Briar_Watchtower", 374f, 104f, -135f, terrains, 4.5f, 4.5f);
            PlaceKitPrefab(root.transform, "Briar_Watchtower", 374f, 76f, -45f, terrains, 4.5f, 4.5f);
            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_SalvageCrane.fbx", "Ironmaw_Crane", 402f, 98f, -120f, terrains, scale: 1.25f, footingSize: 5.5f);

            report.AppendLine("Staged Briar_War_Hall & Watchtower Ironmaw Quarry.");
        }

        // =========================================================================
        // 8. FAR NW TIMBER VILLAGE — FROSTPINE WATCH (-270, 520)
        // =========================================================================
        static void StageFrostpineTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Frostpine Watch");
            root.transform.SetParent(parent, false);

            PlaceKitPrefab(root.transform, "Wayfarer_Inn", -254f, 523f, -120f, terrains, 10.5f, 8.5f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", -274f, 526f, 130f, terrains, 7.6f, 7.0f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", -276f, 510f, 65f, terrains, 7.6f, 7.0f);
            PlaceKitPrefab(root.transform, "Emberford_Smithy", -256f, 506f, -40f, terrains, 8.2f, 6.8f);
            PlaceFbx(root.transform, "BloodrockLookout/Bloodrock_VistaLookout.fbx", "Frostpine_Tower", -270f, 535f, 150f, terrains, scale: 1.3f, footingSize: 5.0f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketWell.fbx", "FP_Well", -265f, 518f, 0f, terrains, footingSize: 2.8f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_SupplyWagon.fbx", "FP_Wagon", -262f, 522f, 30f, terrains, footingSize: 0f);

            AddTownNpc(root.transform, "Forester Orin", "Frostpine Watch", "Human", "Warrior", new Vector3(-264f, 0f, 515f),
                "Frostpine Watch has grown into a real highland timber town—inn, cottages, smithy, well, and tower!", terrains);
            AddTownNpc(root.transform, "Sawyer Hale", "Timber Mill", "Human", "Rogue", new Vector3(-260f, 0f, 511f),
                "Fresh cedar cut and stacked for Whisperwood!", terrains);

            AddWaystop(root.transform, "Frostpine Watch", new Vector3(-265f, 0f, 517f),
                "Frostpine Watch — Far-northwest timber village with Wayfarer Inn, cottages, Emberford Smithy, well, and lookout tower.", terrains);

            AddCivilianWalker(root.transform, "Frostpine Sawyer", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(-265f, 0f, 512.5f), new Vector3(-261f, 0f, 512.5f), new Vector3(-261f, 0f, 514.5f), new Vector3(-265f, 0f, 514.5f)
            });

            report.AppendLine("Staged Frostpine Watch Hero-Building Village.");
        }

        // =========================================================================
        // 9. FAR SE PORT TOWN — REDTIDE HARBOR (365, -110)
        // =========================================================================
        static void StageRedtideHarborTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Redtide Harbor");
            root.transform.SetParent(parent, false);

            PlaceFbx(root.transform, "FrontierOverhaul/Frontier_CoastalLighthouse.fbx", "Redtide_Lighthouse", 365f, -110f, 0f, terrains, footingSize: 0f);
            PlaceKitPrefab(root.transform, "Wayfarer_Inn", 349f, -104f, 80f, terrains, 10.5f, 8.5f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", 352f, -119f, 40f, terrains, 7.6f, 7.0f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", 364f, -94f, -140f, terrains, 7.6f, 7.0f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketShelter.fbx", "Redtide_FishMarket", 359f, -100f, 180f, terrains, footingSize: 6.2f);
            PlaceFbx(root.transform, "WestbankFerryLanding/Westbank_CargoHoist.fbx", "Redtide_Hoist", 374f, -104f, -45f, terrains, footingSize: 4.0f);
            PlaceFbx(root.transform, "WestbankFerryLanding/Westbank_LandingDock.fbx", "Redtide_Dock", 376f, -114f, -90f, terrains, footingSize: 5.5f);
            PlaceFbx(root.transform, "WestbankFerryLanding/Westbank_WillowSkiff.fbx", "Redtide_Skiff", 379f, -117f, 20f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Harbormaster Vance", "Redtide Harbor", "Human", "Rogue", new Vector3(358f, 0f, -105f),
                "Redtide Harbor guides ships home with our 18-meter lighthouse, Wayfarer Inn, cottages, fish market, and cargo piers.", terrains);
            AddTownNpc(root.transform, "Merchant Cora", "Harbor Market", "Human", "Mage", new Vector3(359f, 0f, -96f),
                "Fresh catch and maritime supplies off the dock!", terrains);

            AddWaystop(root.transform, "Redtide Harbor", new Vector3(359f, 0f, -107f),
                "Redtide Harbor — Far-southeast port town with Coastal Lighthouse, Wayfarer Inn, cottages, market shelter, and piers.", terrains);

            AddCivilianWalker(root.transform, "Redtide Dockhand", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(355f, 0f, -102.5f), new Vector3(361f, 0f, -102.5f), new Vector3(361f, 0f, -104.5f), new Vector3(355f, 0f, -104.5f)
            });

            report.AppendLine("Staged Redtide Harbor Hero-Building Port Town.");
        }

        // =========================================================================
        // 10. WESTERN RIVERLANDS VILLAGE — WILLOWMERE MILL (-270, 125)
        // =========================================================================
        static void StageWillowmereHamlet(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Willowmere Mill");
            root.transform.SetParent(parent, false);

            PlaceKitPrefab(root.transform, "Wayfarer_Inn", -272f, 126f, 95f, terrains, 10.5f, 8.5f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", -255f, 132f, -135f, terrains, 7.6f, 7.0f);
            PlaceKitPrefab(root.transform, "Timber_Cottage", -273f, 112f, 45f, terrains, 7.6f, 7.0f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_LoadingShed.fbx", "Willowmere_Granary", -256f, 118f, -60f, terrains, footingSize: 5.2f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketWell.fbx", "WM_Well", -263f, 125f, 0f, terrains, footingSize: 2.8f);
            PlaceFbx(root.transform, "WestbankFerryLanding/Westbank_LandingDock.fbx", "Willowmere_Pier", -282f, 120f, 180f, terrains, footingSize: 5.0f);
            PlaceFbx(root.transform, "Animals/Cow.fbx", "Willowmere_Cow", -254f, 125f, -30f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "Animals/Sheep.fbx", "Willowmere_Sheep", -252f, 127f, 40f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Miller Bram", "Willowmere Mill", "Human", "Warrior", new Vector3(-264f, 0f, 123f),
                "Welcome to Willowmere Village! Inn, cottages, granary, well, and river ferry pier between Silverglen and Deathknell.", terrains);
            AddTownNpc(root.transform, "Baker Liora", "Willowmere Hearth", "Human", "Mage", new Vector3(-266f, 0f, 128f),
                "Fresh river-wheat loaves out of the inn oven!", terrains);

            AddWaystop(root.transform, "Willowmere Mill", new Vector3(-263f, 0f, 125f),
                "Willowmere Mill — Cozy riverside farming village with Wayfarer Inn, cottages, granary, well, livestock, and pier.", terrains);

            AddCivilianWalker(root.transform, "Willowmere Ferryhand", "Human", "Rogue", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(-264f, 0f, 117.5f), new Vector3(-259f, 0f, 117.5f), new Vector3(-259f, 0f, 119.5f), new Vector3(-264f, 0f, 119.5f)
            });

            report.AppendLine("Staged Willowmere Mill Hero-Building Village.");
        }

        // =========================================================================
        // SHARED NORMAL-MAPPED PREFAB & FBX PLACEMENT + PIVOT/SLOPE GROUNDING
        // =========================================================================
        public static GameObject PlaceKitPrefab(Transform parent, string prefabName, float x, float z, float yaw,
            Terrain[] terrains, float footingW, float footingD, bool addColliders = true)
        {
            string path = KitPrefabs + prefabName + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) throw new InvalidOperationException("Missing ProceduralKit prefab: " + path);

            float groundTopY = footingW > 0.5f && footingD > 0.5f
                ? BuildGroundedFoundation(parent, prefabName + "_Footing", new Vector3(x, 0f, z), footingW, footingD, 0.04f, "Limestone", terrains)
                : PhysicsSurface(new Vector3(x, 0f, z), terrains).y;

            // Preserve exact prefab name so EmberfordHouseSurfaces applies detailed normal maps automatically
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = prefabName;
            instance.transform.SetPositionAndRotation(new Vector3(x, groundTopY, z), Quaternion.Euler(0f, yaw, 0f));

            // Normalize vertical pivot using actual rendered MeshRenderer bounds so no prefab ever floats or sinks
            Physics.SyncTransforms();
            var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                float lift = groundTopY - b.min.y;
                if (Mathf.Abs(lift) > 0.02f)
                    instance.transform.position += Vector3.up * lift;
            }

            if (addColliders && !instance.GetComponentInChildren<Collider>(true))
            {
                foreach (var mf in instance.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh) mf.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
                }
            }
            return instance;
        }

        public static GameObject PlaceFbx(Transform parent, string relativeFbxPath, string name, float x, float z, float yaw,
            Terrain[] terrains, float scale = 1f, float footingSize = 4.5f, bool addColliders = true)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ArtRoot + relativeFbxPath);
            if (!asset)
                throw new InvalidOperationException("Missing FBX asset: " + ArtRoot + relativeFbxPath);

            float baseY = footingSize > 0.5f
                ? BuildGroundedFoundation(parent, name + "_Footing", new Vector3(x, 0f, z), footingSize, footingSize, 0.04f, "Limestone", terrains)
                : PhysicsSurface(new Vector3(x, 0f, z), terrains).y;

            var wrapper = new GameObject(name);
            wrapper.transform.SetParent(parent, false);
            wrapper.transform.SetPositionAndRotation(new Vector3(x, baseY, z), Quaternion.Euler(0f, yaw, 0f));
            wrapper.transform.localScale = Vector3.one * scale;

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(asset, wrapper.transform);
            if (inst)
            {
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;
            }

            foreach (var mf in wrapper.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = mf.GetComponent<MeshRenderer>();
                if (r && r.sharedMaterials != null)
                {
                    var mats = r.sharedMaterials;
                    var mapped = new Material[mats.Length];
                    for (int i = 0; i < mats.Length; i++)
                        mapped[i] = ResolveProjectMaterial(mats[i]);
                    r.sharedMaterials = mapped;
                }
                if (addColliders && mf.sharedMesh && !mf.GetComponent<Collider>())
                {
                    var mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
            }
            return wrapper;
        }

        static Material ResolveProjectMaterial(Material existing)
        {
            if (existing && existing.shader && existing.shader.name != "Hidden/InternalErrorShader")
            {
                var named = AssetDatabase.LoadAssetAtPath<Material>(Materials + existing.name.Replace(' ', '_') + ".mat");
                if (named) return named;
                return existing;
            }
            return LoadMaterial("Limestone");
        }

        static float BuildGroundedFoundation(Transform parent, string name, Vector3 centerXZ, float width, float depth, float heightAboveMaxGround, string materialName, Terrain[] terrains)
        {
            float hw = width * 0.5f;
            float hd = depth * 0.5f;
            Vector3[] samples =
            {
                PhysicsSurface(centerXZ, terrains),
                PhysicsSurface(centerXZ + new Vector3(-hw, 0f, -hd), terrains),
                PhysicsSurface(centerXZ + new Vector3(hw, 0f, -hd), terrains),
                PhysicsSurface(centerXZ + new Vector3(-hw, 0f, hd), terrains),
                PhysicsSurface(centerXZ + new Vector3(hw, 0f, hd), terrains)
            };
            float minY = samples.Min(s => s.y) - 2.6f;
            float topY = samples.Max(s => s.y) + heightAboveMaxGround;
            float totalH = Mathf.Max(0.4f, topY - minY);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(centerXZ.x, minY + totalH * 0.5f, centerXZ.z);
            go.transform.localScale = new Vector3(width, totalH, depth);
            var mat = LoadMaterial(materialName);
            if (mat) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return topY;
        }

        static void AddWaystop(Transform parent, string displayName, Vector3 xzPos, string noticeText, Terrain[] terrains)
        {
            var pin = new GameObject(displayName + " map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(parent, false);
            pin.transform.position = PhysicsSurface(xzPos, terrains);
            pin.displayName = displayName;
            pin.notice = noticeText;
        }

        static void AddTownNpc(Transform parent, string npcName, string roleTag, string race, string cls, Vector3 xzPos, string greeting, Terrain[] terrains)
        {
            var actor = new GameObject(npcName);
            actor.transform.SetParent(parent, false);
            actor.transform.position = PhysicsSurface(xzPos, terrains);
            CharacterVisualFactory.CreateVisualInstance(race, cls, actor.transform);
            var body = actor.AddComponent<CapsuleCollider>();
            body.height = 1.88f; body.radius = 0.36f; body.center = Vector3.up * 0.94f;
            var npc = actor.AddComponent<TownNPC>();
            npc.npcName = npcName; npc.roleTag = roleTag; npc.greeting = greeting;
        }

        static void AddCivilianWalker(Transform parent, string name, string race, string cls, float walkSpeed, float stopSeconds, Terrain[] terrains, Vector3[] rawPoints)
        {
            var routeRoot = new GameObject(name + " Route");
            routeRoot.transform.SetParent(parent, false);

            var wps = new Transform[rawPoints.Length];
            for (int i = 0; i < rawPoints.Length; i++)
            {
                var wp = new GameObject($"WP_{i}");
                wp.transform.SetParent(routeRoot.transform, false);
                wp.transform.position = PhysicsSurface(rawPoints[i], terrains);
                wps[i] = wp.transform;
            }

            var actor = new GameObject(name);
            actor.transform.SetParent(parent, false);
            actor.transform.position = wps[0].position;
            CharacterVisualFactory.CreateVisualInstance(race, cls, actor.transform);

            var walker = actor.AddComponent<CivilianRouteWalker>();
            var so = new SerializedObject(walker);
            var wpProp = so.FindProperty("waypoints");
            wpProp.arraySize = wps.Length;
            for (int i = 0; i < wps.Length; i++)
                wpProp.GetArrayElementAtIndex(i).objectReferenceValue = wps[i];
            so.FindProperty("closedLoop").boolValue = true;
            so.FindProperty("walkSpeed").floatValue = walkSpeed;
            so.FindProperty("stopSeconds").floatValue = stopSeconds;
            so.FindProperty("walkState").stringValue = "Walk";
            so.FindProperty("idleState").stringValue = "Idle";
            var anim = actor.GetComponentInChildren<Animator>();
            if (anim) so.FindProperty("animator").objectReferenceValue = anim;
            so.ApplyModifiedPropertiesWithoutUndo();
            walker.RebuildRoute();
        }

        static Material LoadMaterial(string name)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Materials + name.Replace(' ', '_') + ".mat");
            if (!mat) mat = AssetDatabase.LoadAssetAtPath<Material>(Materials + "Limestone.mat");
            return mat;
        }

        public static Vector3 PhysicsSurface(Vector3 point, Terrain[] terrains)
        {
            var hits = Physics.RaycastAll(new Vector3(point.x, 260f, point.z), Vector3.down, 420f);
            float best = float.MinValue;
            bool found = false;
            foreach (var h in hits)
            {
                if (h.collider is TerrainCollider && h.point.y > best) { best = h.point.y; found = true; }
            }
            point.y = found ? best : WorldRegions.GroundHeight(point, terrains);
            return point;
        }
    }
}
