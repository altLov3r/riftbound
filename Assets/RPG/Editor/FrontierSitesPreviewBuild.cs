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
    /// Stages dense, lived-in Frontier Small Towns & Landmarks using Riftbound's authored Blender FBX kits
    /// (zero primitive-cube placeholder buildings, zero authored scene/terrain edits, zero floating).
    /// </summary>
    public static class FrontierSitesPreviewBuild
    {
        const string ArtRoot = "Assets/RPG/Art/";
        const string Materials = "Assets/RPG/ProceduralKit/Materials/";

        public static readonly Vector3 BreakerWorksCenter = new Vector3(180f, 0f, 300f);
        public static readonly Vector3 WhisperwoodCenter = new Vector3(35f, 0f, 250f);
        public static readonly Vector3 CinderroadCenter = new Vector3(262f, 0f, -55f);
        public static readonly Vector3 ThornwatchCenter = new Vector3(-275f, 0f, -110f);
        public static readonly Vector3 SkyreachCenter = new Vector3(355f, 0f, 420f);
        public static readonly Vector3 SunstoneCenter = new Vector3(115f, 0f, -115f);

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
            StageIronmawQuarry(root.transform, terrains, report);
            StageFrostpineTown(root.transform, terrains, report);
            StageRedtideHarborTown(root.transform, terrains, report);
            StageWillowmereHamlet(root.transform, terrains, report);

            return root;
        }

        // =========================================================================
        // 1. BREAKER WORKS — GORGONAX VOLCANIC RAID AMPHITHEATER (180, 300)
        // =========================================================================
        static void StageBreakerWorks(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Breaker Works Arena");
            root.transform.SetParent(parent, false);

            // Grand SE Furnace Archway at (198, 279) using real GorKresh_ForgeArch.fbx
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_ForgeArch.fbx", "FurnaceArch_Lintel", 198f, 279f, -42f, terrains, scale: 1.55f, footingSize: 0f);
            // NW Gate Archway at (164, 316)
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_ForgeArch.fbx", "NWGate_Arch", 164f, 316f, 135f, terrains, scale: 1.35f, footingSize: 0f);

            // Ruined Pressure Tower & Industrial Kilns on NE Rim (204, 322, R ~ 32.5m)
            PlaceFbx(root.transform, "BloodrockLookout/Bloodrock_VistaLookout.fbx", "PressureTower_Drum", 204f, 322f, -135f, terrains, scale: 1.65f, footingSize: 6.5f);
            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_DrumFrame.fbx", "PressureTower_Frame", 208f, 318f, -120f, terrains, scale: 1.35f, footingSize: 0f);
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_KilnFurnace.fbx", "RimKiln_NE", 198f, 324f, 20f, terrains, scale: 1.3f, footingSize: 4.5f);

            // Volcanic Forge Kilns & Banners ringing the outer rim (R = 25..27m, keeping R <= 19m floor 100% clear!)
            for (int i = 0; i < 10; i++)
            {
                float deg = i * 36f;
                if ((deg >= 295f && deg <= 340f) || (deg >= 120f && deg <= 155f)) continue;
                float rad = deg * Mathf.Deg2Rad;
                float rx = 180f + Mathf.Cos(rad) * 26.0f;
                float rz = 300f + Mathf.Sin(rad) * 26.0f;
                if (i % 2 == 0)
                    PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_KilnFurnace.fbx", $"RimKiln_{i}", rx, rz, -deg + 90f, terrains, scale: 1.15f, footingSize: 4.0f);
                else
                    PlaceFbx(root.transform, "BloodrockLookout/Bloodrock_OutpostBanner.fbx", $"RimBanner_{i}", rx, rz, -deg + 90f, terrains, scale: 1.35f, footingSize: 0f, addColliders: false);
            }

            // Quiet Rim Bypass Trail lanterns (>= 42m from Gorgonax)
            Vector3[] bypass = { new(140, 0, 344), new(160, 0, 347), new(180, 0, 346), new(205, 0, 340), new(224, 0, 332), new(230, 0, 260) };
            for (int i = 0; i < bypass.Length; i++)
                PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_TrailLantern.fbx", $"BreakerBypass_Lantern_{i}", bypass[i].x, bypass[i].z, 0f, terrains, footingSize: 0f, addColliders: false);

            AddWaystop(root.transform, "Breaker Works", new Vector3(206f, 0f, 272f),
                "Breaker Works Crater. Gorgonax holds the open basalt floor below. Enter through the SE Forge Arch or follow the outer rim lanterns.", terrains);
            report.AppendLine("Staged FBX Breaker Works Arena.");
        }

        // =========================================================================
        // 2. WHISPERWOOD EXCHANGE SMALL TOWN (35, 250)
        // =========================================================================
        static void StageWhisperwoodTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Whisperwood Exchange");
            root.transform.SetParent(parent, false);

            // Real FBX town square: Market Hall, Inn, Loading Shed, Town Well, Supply Wagon, Honey Stand, Herb Beds, Horse
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketShelter.fbx", "WW_MarketHall", 35f, 250f, 0f, terrains, footingSize: 6.8f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_HearthInn.fbx", "WW_HearthInn", 45f, 258f, -110f, terrains, footingSize: 7.5f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_LoadingShed.fbx", "WW_CargoShed", 26f, 256f, 75f, terrains, footingSize: 5.2f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketWell.fbx", "WW_TownWell", 35f, 240f, 0f, terrains, footingSize: 2.8f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_SupplyWagon.fbx", "WW_SupplyWagon", 27f, 245f, 25f, terrains, footingSize: 0f);
            PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_HoneyStand.fbx", "WW_HoneyStand", 43f, 242f, -40f, terrains, footingSize: 3.2f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_HerbBeds.fbx", "WW_HerbBeds", 39f, 237f, 0f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_RoofedNoticeBoard.fbx", "WW_NoticeBoard", 30.5f, 249.5f, 90f, terrains, footingSize: 0f);
            PlaceFbx(root.transform, "Animals/Horse.fbx", "WW_WagonHorse", 24.5f, 242.5f, 35f, terrains, scale: 1.0f, footingSize: 0f, addColliders: false);

            PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_TrailLantern.fbx", "WW_LanternA", 4f, 247f, 0f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_TrailLantern.fbx", "WW_LanternB", 19f, 248f, 0f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_TrailLantern.fbx", "WW_LanternC", 31f, 255f, 0f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Quartermaster Brev", "Whisperwood Exchange", "Human", "Warrior", new Vector3(34.5f, 0f, 253.8f),
                "Welcome to Whisperwood Exchange! Cedar from Pinewatch and stone from Cinderroad meet right here at our market square.", terrains);
            AddTownNpc(root.transform, "Innkeeper Talia", "Whisperwood Inn", "Human", "Mage", new Vector3(41.5f, 0f, 256.5f),
                "Warm cider and fresh bread inside the hearth inn, traveler. Plenty of couriers resting between Greenhollow and Emberford.", terrains);

            AddWaystop(root.transform, "Whisperwood Exchange", new Vector3(30.5f, 0f, 249.5f),
                "Whisperwood Exchange — Bustling crossroads market town linking Pinewatch Relay, Greenhollow Post, and Cinderroad Ironworks.", terrains);

            AddCivilianWalker(root.transform, "Whisperwood Exchange Porter", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(31.5f, 0f, 242.5f), new Vector3(37.5f, 0f, 242.5f), new Vector3(37.5f, 0f, 244.8f), new Vector3(31.5f, 0f, 244.8f)
            });
            AddCivilianWalker(root.transform, "Whisperwood Waiting Courier", "Human", "Rogue", 1.45f, 2.0f, terrains, new[]
            {
                new Vector3(30.0f, 0f, 255.0f), new Vector3(38.0f, 0f, 255.5f), new Vector3(39.0f, 0f, 257.5f), new Vector3(31.0f, 0f, 257.0f)
            });
            AddCivilianWalker(root.transform, "Whisperwood Treeline Scout", "Human", "Rogue", 1.40f, 2.4f, terrains, new[]
            {
                new Vector3(41.2f, 0f, 245.5f), new Vector3(43.2f, 0f, 247.5f), new Vector3(43.6f, 0f, 250.5f),
                new Vector3(43.2f, 0f, 253.5f), new Vector3(41.2f, 0f, 252.0f), new Vector3(41.0f, 0f, 248.5f)
            });

            report.AppendLine("Staged Whisperwood Exchange Small Town.");
        }

        // =========================================================================
        // 3. SE CORNER SMALL TOWN — CINDERROAD IRONWORKS (262, -55)
        // =========================================================================
        static void StageCinderroadTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Cinderroad Waystation");
            root.transform.SetParent(parent, false);

            // Real FBX industrial frontier town
            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_SalvageCrane.fbx", "FreightCrane_Boom", 262f, -55f, -18f, terrains, scale: 1.25f, footingSize: 5.5f);
            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_SortingWorkshop.fbx", "Storehouse_Walls", 248f, -55f, 15f, terrains, footingSize: 7.5f);
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_KilnFurnace.fbx", "Smelter_Chimney", 246f, -64f, 0f, terrains, scale: 1.35f, footingSize: 5.2f);
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_AnvilStation.fbx", "Ironworks_Anvil", 254f, -61f, 30f, terrains, footingSize: 0f);
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_ForgeArch.fbx", "BastionGate_Lintel", 254f, -41.5f, 0f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_HearthInn.fbx", "Ironworks_Inn", 271f, -45f, -150f, terrains, footingSize: 7.5f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketShelter.fbx", "Ironworks_Market", 264f, -64f, 180f, terrains, footingSize: 6.0f);
            PlaceFbx(root.transform, "BloodrockLookout/Bloodrock_VistaLookout.fbx", "SignalBeacon_Tower", 276f, -63f, -45f, terrains, scale: 1.3f, footingSize: 5.0f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_SupplyWagon.fbx", "Ironworks_Wagon", 256f, -52f, 70f, terrains, footingSize: 0f);
            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_SortedScrapBins.fbx", "Ironworks_Bins", 269f, -58f, 0f, terrains, footingSize: 0f);

            Vector3[] spur = { new(86, 0, -36), new(130, 0, -34), new(175, 0, -36), new(220, 0, -41), new(254, 0, -45) };
            for (int i = 0; i < spur.Length; i++)
                PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_TrailLantern.fbx", $"EmberfordSpur_Lantern_{i}", spur[i].x, spur[i].z, 0f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Foreman Darek", "Cinderroad Ironworks", "Human", "Warrior", new Vector3(256.8f, 0f, -49.5f),
                "Cinderroad Ironworks has grown into a full foundry town! Smelter, crane, tavern, and market—all feeding the Emberford spur.", terrains);
            AddTownNpc(root.transform, "Blacksmith Torin", "Ironworks Smithy", "Dwarf", "Warrior", new Vector3(253.0f, 0f, -59.5f),
                "Best tempered steel in the southeast! Pull up by the kiln if you need your blade checked.", terrains);

            AddWaystop(root.transform, "Cinderroad Ironworks", new Vector3(258f, 0f, -48f),
                "Cinderroad Ironworks — Southeast foundry town with smelter hall, freight crane, tavern, and market square.", terrains);

            AddCivilianWalker(root.transform, "Cinderroad Wagon Inspector", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(258.5f, 0f, -45.5f), new Vector3(265.0f, 0f, -46.0f), new Vector3(265.5f, 0f, -49.0f), new Vector3(259.0f, 0f, -48.5f)
            });
            AddCivilianWalker(root.transform, "Cinderroad Approach Guard", "Human", "Warrior", 1.40f, 2.0f, terrains, new[]
            {
                new Vector3(243.5f, 0f, -45.5f), new Vector3(248.0f, 0f, -45.0f), new Vector3(248.0f, 0f, -48.5f), new Vector3(243.5f, 0f, -48.5f)
            });
            AddCivilianWalker(root.transform, "Cinderroad Salvage Buyer", "Human", "Rogue", 1.35f, 2.4f, terrains, new[]
            {
                new Vector3(269.0f, 0f, -50.5f), new Vector3(274.5f, 0f, -51.5f), new Vector3(274.0f, 0f, -55.5f), new Vector3(268.5f, 0f, -54.5f)
            });

            report.AppendLine("Staged Cinderroad Ironworks Small Town.");
        }

        // =========================================================================
        // 4. SW CORNER SMALL TOWN — THORNWATCH CITADEL (-275, -110)
        // =========================================================================
        static void StageThornwatchTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Thornwatch Citadel");
            root.transform.SetParent(parent, false);

            // Real Highspire & Emberwatch FBX fortress town!
            PlaceFbx(root.transform, "HighspireDistrict/Highspire_GuildHall.fbx", "Thornwatch_KeepShaft", -276f, -110f, 90f, terrains, footingSize: 8.5f);
            PlaceFbx(root.transform, "HighspireDistrict/Highspire_Gatehouse.fbx", "Thornwatch_GateLintel", -262f, -103f, 90f, terrains, footingSize: 6.5f);
            PlaceFbx(root.transform, "HighspireDistrict/Highspire_MarketLoggia.fbx", "Thornwatch_Market", -286f, -103f, 0f, terrains, footingSize: 7.0f);
            PlaceFbx(root.transform, "HighspireDistrict/Highspire_Workshop.fbx", "Thornwatch_Smithy", -275f, -123f, 180f, terrains, footingSize: 6.8f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_HearthInn.fbx", "Thornwatch_Inn", -288f, -117f, 60f, terrains, footingSize: 7.5f);
            PlaceFbx(root.transform, "HighspireDistrict/Highspire_Fountain.fbx", "Thornwatch_Fountain", -272f, -103f, 0f, terrains, footingSize: 3.2f);
            PlaceFbx(root.transform, "Animals/Horse.fbx", "Thornwatch_HorseA", -267f, -121f, -35f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "Animals/Horse.fbx", "Thornwatch_HorseB", -265f, -120f, -20f, terrains, footingSize: 0f, addColliders: false);

            PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_TrailLantern.fbx", "TW_Lantern1", -235f, -84f, 0f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "VillageLandmarks/Greenhollow_TrailLantern.fbx", "TW_Lantern2", -250f, -94f, 0f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Captain Vaelen", "Thornwatch Citadel", "Human", "Warrior", new Vector3(-269.5f, 0f, -103.5f),
                "Thornwatch Citadel guards the southwest coast—guild hall, market loggia, smithy, and hearth inn all open to travelers.", terrains);
            AddTownNpc(root.transform, "Quartermaster Sera", "Citadel Market", "Elf", "Rogue", new Vector3(-282.5f, 0f, -103.0f),
                "Supplies arrive daily from Highspire Keep. Take a seat by the courtyard fountain.", terrains);

            AddWaystop(root.transform, "Thornwatch Citadel", new Vector3(-267.5f, 0f, -103.5f),
                "Thornwatch Citadel — Southwest fortified garrison town with guild hall, market loggia, smithy, tavern, and courtyard fountain.", terrains);

            AddCivilianWalker(root.transform, "Thornwatch Sentry", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(-271f, 0f, -97.5f), new Vector3(-273.5f, 0f, -97.5f), new Vector3(-276f, 0f, -97.5f),
                new Vector3(-276f, 0f, -99.5f), new Vector3(-273.5f, 0f, -99.5f), new Vector3(-271f, 0f, -99.5f)
            });
            AddCivilianWalker(root.transform, "Thornwatch Quartermaster Runner", "Human", "Rogue", 1.45f, 2.0f, terrains, new[]
            {
                new Vector3(-267.5f, 0f, -111.5f), new Vector3(-267.5f, 0f, -114.5f), new Vector3(-267.5f, 0f, -117.5f),
                new Vector3(-269.5f, 0f, -117.5f), new Vector3(-269.5f, 0f, -114.5f), new Vector3(-269.5f, 0f, -111.5f)
            });
            AddCivilianWalker(root.transform, "Thornwatch Rampart Guard", "Human", "Warrior", 1.35f, 2.4f, terrains, new[]
            {
                new Vector3(-254.0f, 0f, -101.0f), new Vector3(-252.5f, 0f, -101.0f), new Vector3(-252.5f, 0f, -103.0f),
                new Vector3(-252.5f, 0f, -105.0f), new Vector3(-254.0f, 0f, -105.0f), new Vector3(-254.0f, 0f, -103.0f)
            });

            report.AppendLine("Staged Thornwatch Citadel Small Town.");
        }

        // =========================================================================
        // 5. FAR NE MOUNTAIN TOWN — SKYREACH AERIE (355, 420)
        // =========================================================================
        static void StageSkyreachTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Skyreach Aerie");
            root.transform.SetParent(parent, false);

            PlaceFbx(root.transform, "BloodrockLookout/Bloodrock_VistaLookout.fbx", "Skyreach_WindspireShaft", 355f, 420f, -140f, terrains, scale: 1.55f, footingSize: 6.0f);
            PlaceFbx(root.transform, "SilverglenArrivalCourt/Silverglen_ReadingPavilion.fbx", "Skyreach_Pavilion", 365f, 418f, -45f, terrains, footingSize: 6.0f);
            PlaceFbx(root.transform, "FrostpeakTrailStation/Frostpeak_WindbreakShelter.fbx", "Skyreach_Lodge", 346f, 423f, 60f, terrains, footingSize: 5.5f);
            PlaceFbx(root.transform, "FrostpeakTrailStation/Frostpeak_PassForge.fbx", "Skyreach_Forge", 356f, 408f, 180f, terrains, footingSize: 4.5f);
            PlaceFbx(root.transform, "SilverglenArrivalCourt/Silverglen_TrailBeacon.fbx", "Skyreach_Beacon1", 347f, 411f, 0f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "SilverglenArrivalCourt/Silverglen_TrailBeacon.fbx", "Skyreach_Beacon2", 362f, 410f, 0f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Storm-Watcher Lyra", "Skyreach Aerie", "Elf", "Mage", new Vector3(352f, 0f, 413f),
                "Skyreach Aerie shelters travelers climbing the Wyrmcrest ridge. Rest by our pavilion and pass-forge.", terrains);
            AddTownNpc(root.transform, "Scout Kael", "Ridge Watch", "Human", "Rogue", new Vector3(361f, 0f, 414f),
                "Clear skies over the Redridge basin today. Watch the drake peaks to the north!", terrains);

            AddWaystop(root.transform, "Skyreach Aerie", new Vector3(350f, 0f, 412f),
                "Skyreach Aerie — Northeast mountain sanctuary with observatory lookout, reading pavilion, and alpine pass-forge.", terrains);

            AddCivilianWalker(root.transform, "Skyreach Wind-Scout", "Elf", "Rogue", 1.40f, 2.2f, terrains, new[]
            {
                new Vector3(358f, 0f, 411f), new Vector3(360.5f, 0f, 411f), new Vector3(360.5f, 0f, 414f), new Vector3(358f, 0f, 414f)
            });

            report.AppendLine("Staged Skyreach Aerie Mountain Town.");
        }

        // =========================================================================
        // 6. SOUTH-CENTRAL RUINS — SUNSTONE ABBEY (115, -115)
        // =========================================================================
        static void StageSunstoneAbbeyRuins(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Sunstone Abbey Ruins");
            root.transform.SetParent(parent, false);

            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_WatchArch.fbx", "Abbey_EntryArch", 115f, -95f, 0f, terrains, scale: 1.4f, footingSize: 0f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_BrokenArcade.fbx", "Abbey_ArcadeL1", 104f, -104f, 90f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_BrokenArcade.fbx", "Abbey_ArcadeR1", 126f, -104f, -90f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_BrokenArcade.fbx", "Abbey_ArcadeL2", 104f, -118f, 90f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_BrokenArcade.fbx", "Abbey_ArcadeR2", 126f, -118f, -90f, terrains, scale: 1.25f, footingSize: 0f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_Cenotaph.fbx", "Abbey_HighAltar", 115f, -134f, 0f, terrains, scale: 1.4f, footingSize: 4.5f);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_CloisterLantern.fbx", "Abbey_Lantern1", 109f, -94f, 0f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "DeathknellCloister/Deathknell_CloisterLantern.fbx", "Abbey_Lantern2", 121f, -94f, 0f, terrains, footingSize: 0f, addColliders: false);

            report.AppendLine("Staged FBX Sunstone Abbey Ruins.");
        }

        // =========================================================================
        // 7. FAR EAST OUTLAW QUARRY — IRONMAW QUARRY (385, 90)
        // =========================================================================
        static void StageIronmawQuarry(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Ironmaw Quarry Architecture");
            root.transform.SetParent(parent, false);

            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_SalvageCrane.fbx", "Ironmaw_Crane", 398f, 90f, -90f, terrains, scale: 1.3f, footingSize: 5.5f);
            PlaceFbx(root.transform, "RustwaterSalvageYard/Rustwater_SortingDrum.fbx", "Ironmaw_Drum", 394f, 100f, 30f, terrains, footingSize: 4.5f);
            PlaceFbx(root.transform, "GorKreshForgeCourt/GorKresh_QuartermasterShed.fbx", "Ironmaw_Shed", 395f, 80f, -45f, terrains, footingSize: 5.0f);
            PlaceFbx(root.transform, "BloodrockLookout/Bloodrock_VistaLookout.fbx", "Ironmaw_WatchN", 371f, 103f, -135f, terrains, footingSize: 4.5f);
            PlaceFbx(root.transform, "BloodrockLookout/Bloodrock_VistaLookout.fbx", "Ironmaw_WatchS", 371f, 77f, -45f, terrains, footingSize: 4.5f);

            report.AppendLine("Staged FBX Ironmaw Quarry.");
        }

        // =========================================================================
        // 8. FAR NW TIMBER TOWN — FROSTPINE WATCH (-270, 520)
        // =========================================================================
        static void StageFrostpineTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Frostpine Watch");
            root.transform.SetParent(parent, false);

            PlaceFbx(root.transform, "BloodrockLookout/Bloodrock_VistaLookout.fbx", "Frostpine_Tower", -272f, 522f, 120f, terrains, scale: 1.3f, footingSize: 5.0f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_HearthInn.fbx", "Frostpine_Inn", -256f, 520f, -90f, terrains, footingSize: 7.5f);
            PlaceFbx(root.transform, "FrostpeakTrailStation/Frostpeak_PassForge.fbx", "Frostpine_Forge", -268f, 509f, 20f, terrains, footingSize: 4.5f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_LoadingShed.fbx", "Frostpine_MillShed", -257f, 508f, 160f, terrains, footingSize: 5.2f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_SupplyWagon.fbx", "Frostpine_Wagon", -263f, 519f, 45f, terrains, footingSize: 0f);

            AddTownNpc(root.transform, "Forester Orin", "Frostpine Watch", "Human", "Warrior", new Vector3(-264f, 0f, 515f),
                "Frostpine Watch is the northernmost timber village in Riftbound. Warm up at the hearth inn before heading south to Pinewatch.", terrains);
            AddTownNpc(root.transform, "Sawyer Hale", "Timber Mill", "Human", "Rogue", new Vector3(-261f, 0f, 511f),
                "Fresh cedar beams stacked and ready for the wagons!", terrains);

            AddWaystop(root.transform, "Frostpine Watch", new Vector3(-263f, 0f, 517f),
                "Frostpine Watch — Far-northwest timber village with hearth inn, lookout tower, sawmill, and pass-forge.", terrains);

            AddCivilianWalker(root.transform, "Frostpine Sawyer", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(-264f, 0f, 512.5f), new Vector3(-260f, 0f, 512.5f), new Vector3(-260f, 0f, 514.0f), new Vector3(-264f, 0f, 514.0f)
            });

            report.AppendLine("Staged Frostpine Watch Timber Village.");
        }

        // =========================================================================
        // 9. FAR SE COASTAL TOWN — REDTIDE HARBOR (365, -110)
        // =========================================================================
        static void StageRedtideHarborTown(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Redtide Harbor");
            root.transform.SetParent(parent, false);

            PlaceFbx(root.transform, "HighspireDistrict/Highspire_Gatehouse.fbx", "Redtide_HarborGate", 365f, -110f, 0f, terrains, scale: 1.25f, footingSize: 6.5f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_HearthInn.fbx", "Redtide_Tavern", 351f, -104f, 70f, terrains, footingSize: 7.5f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketShelter.fbx", "Redtide_FishMarket", 362f, -97f, 180f, terrains, footingSize: 6.2f);
            PlaceFbx(root.transform, "WestbankFerryLanding/Westbank_CargoHoist.fbx", "Redtide_Hoist", 372f, -104f, -45f, terrains, footingSize: 4.0f);
            PlaceFbx(root.transform, "WestbankFerryLanding/Westbank_LandingDock.fbx", "Redtide_Dock", 374f, -114f, -90f, terrains, footingSize: 5.5f);
            PlaceFbx(root.transform, "WestbankFerryLanding/Westbank_WillowSkiff.fbx", "Redtide_Skiff", 377f, -117f, 20f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Harbormaster Vance", "Redtide Harbor", "Human", "Rogue", new Vector3(358f, 0f, -105f),
                "Redtide Harbor is the southeast port town! Tavern, fish market, cargo hoist, and coastal piers.", terrains);
            AddTownNpc(root.transform, "Merchant Cora", "Harbor Market", "Human", "Mage", new Vector3(361f, 0f, -100f),
                "Spices and iron ingots straight off the coastal skiffs!", terrains);

            AddWaystop(root.transform, "Redtide Harbor", new Vector3(359f, 0f, -107f),
                "Redtide Harbor — Far-southeast port town with harbor gatehouse, tavern, market shelter, cargo hoist, and docks.", terrains);

            AddCivilianWalker(root.transform, "Redtide Dockhand", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(356f, 0f, -101.5f), new Vector3(360f, 0f, -101.5f), new Vector3(360f, 0f, -103.0f), new Vector3(356f, 0f, -103.0f)
            });

            report.AppendLine("Staged Redtide Harbor Port Town.");
        }

        // =========================================================================
        // 10. WESTERN RIVERLANDS HAMLET — WILLOWMERE MILL (-270, 125)
        // =========================================================================
        static void StageWillowmereHamlet(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Willowmere Mill");
            root.transform.SetParent(parent, false);

            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_HearthInn.fbx", "Willowmere_MillHouse", -270f, 125f, 90f, terrains, footingSize: 7.5f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_LoadingShed.fbx", "Willowmere_Granary", -257f, 129f, -60f, terrains, footingSize: 5.2f);
            PlaceFbx(root.transform, "EmberwatchCivicSquare/Emberwatch_MarketWell.fbx", "Willowmere_Well", -263f, 125f, 0f, terrains, footingSize: 2.8f);
            PlaceFbx(root.transform, "VillageLandmarks/Emberwatch_HerbBeds.fbx", "Willowmere_Garden", -265f, 131f, 0f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "WestbankFerryLanding/Westbank_LandingDock.fbx", "Willowmere_Pier", -278f, 120f, 180f, terrains, footingSize: 5.0f);
            PlaceFbx(root.transform, "Animals/Cow.fbx", "Willowmere_Cow", -256f, 122f, -30f, terrains, footingSize: 0f, addColliders: false);
            PlaceFbx(root.transform, "Animals/Sheep.fbx", "Willowmere_Sheep", -254f, 124f, 40f, terrains, footingSize: 0f, addColliders: false);

            AddTownNpc(root.transform, "Miller Bram", "Willowmere Mill", "Human", "Warrior", new Vector3(-264f, 0f, 123f),
                "Welcome to Willowmere Hamlet! Fresh grain, garden herbs, and a quiet riverside inn between Silverglen and Deathknell.", terrains);
            AddTownNpc(root.transform, "Baker Liora", "Willowmere Hearth", "Human", "Mage", new Vector3(-266f, 0f, 127f),
                "Just pulled warm loaves from the hearth oven, traveler.", terrains);

            AddWaystop(root.transform, "Willowmere Mill", new Vector3(-263f, 0f, 125f),
                "Willowmere Mill — Cozy riverside farming hamlet with hearth inn, granary, herb gardens, livestock, and ferry pier.", terrains);

            AddCivilianWalker(root.transform, "Willowmere Ferryhand", "Human", "Rogue", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(-264f, 0f, 118.5f), new Vector3(-259f, 0f, 118.5f), new Vector3(-259f, 0f, 120.5f), new Vector3(-264f, 0f, 120.5f)
            });

            report.AppendLine("Staged Willowmere Mill Hamlet.");
        }

        // =========================================================================
        // SHARED FBX PLACEMENT & ZERO-FLOATING GROUNDING HELPERS
        // =========================================================================
        static GameObject PlaceFbx(Transform parent, string relativeFbxPath, string name, float x, float z, float yaw,
            Terrain[] terrains, float scale = 1f, float footingSize = 4.5f, bool addColliders = true)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ArtRoot + relativeFbxPath);
            if (!asset)
                throw new InvalidOperationException("Missing FBX asset: " + ArtRoot + relativeFbxPath);

            Vector3 pos = PhysicsSurface(new Vector3(x, 0f, z), terrains);
            float baseY = pos.y;
            if (footingSize > 0.5f)
            {
                baseY = BuildGroundedFoundation(parent, name + "_Footing", new Vector3(x, 0f, z), footingSize, footingSize, 0.04f, "Limestone", terrains);
            }

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
            float minY = samples.Min(s => s.y) - 2.2f;
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
