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
    /// Stages all Frontier landmarks & strongholds in memory during build/verification (zero authored scene/terrain edits):
    /// 1. Breaker Works Arena (180, 300): Monumental multi-tier volcanic raid amphitheater for Gorgonax.
    /// 2. Whisperwood Exchange (35, 250): Deeply grounded roadside trade exchange + 3 civilian routes.
    /// 3. Cinderroad Ironworks Bastion (262, -55): Major SE Quarry & Smelter Fortress + 3 civilian routes.
    /// 4. Thornwatch Citadel (-275, -110): Major SW Frontier Lighthouse Keep & Garrison Courtyard + 3 patrols.
    /// </summary>
    public static class FrontierSitesPreviewBuild
    {
        const string Materials = "Assets/RPG/ProceduralKit/Materials/";

        public static readonly Vector3 BreakerWorksCenter = new Vector3(180f, 0f, 300f);
        public static readonly Vector3 WhisperwoodCenter = new Vector3(35f, 0f, 250f);
        public static readonly Vector3 CinderroadCenter = new Vector3(262f, 0f, -55f);
        public static readonly Vector3 ThornwatchCenter = new Vector3(-275f, 0f, -110f);

        public static GameObject StageAll(Scene scene, StringBuilder report = null)
        {
            report ??= new StringBuilder();
            var terrains = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>()).ToArray();
            if (terrains.Length == 0) throw new InvalidOperationException("No authored terrain in scene.");

            Physics.SyncTransforms();

            var root = new GameObject("Frontier Density Sites");
            SceneManager.MoveGameObjectToScene(root, scene);

            StageBreakerWorks(root.transform, terrains, report);
            StageWhisperwoodExchange(root.transform, terrains, report);
            StageCinderroadIronworks(root.transform, terrains, report);
            StageThornwatchCitadel(root.transform, terrains, report);

            return root;
        }

        // =========================================================================
        // 1. BREAKER WORKS — MONUMENTAL GORGONAX RAID AMPHITHEATER (180, 300)
        // =========================================================================
        static void StageBreakerWorks(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Breaker Works Arena");
            root.transform.SetParent(parent, false);

            // Inner combat floor (R <= 19.5m): 100% snag-free! Flush non-colliding obsidian ring & glowing lava fissures
            for (int i = 0; i < 20; i++)
            {
                float rad = i * Mathf.PI * 2f / 20f;
                Vector3 p = BreakerWorksCenter + new Vector3(Mathf.Cos(rad) * 19.6f, 0f, Mathf.Sin(rad) * 19.6f);
                var slab = Part(root.transform, $"ArenaFloorInlay_{i}", PhysicsSurface(p, terrains) + Vector3.up * 0.04f,
                    new Vector3(4.6f, 0.14f, 1.3f), "Basalt", collider: false);
                slab.transform.rotation = Quaternion.Euler(0f, -rad * Mathf.Rad2Deg + 90f, 0f);

                if (i % 5 == 0)
                {
                    Vector3 fissureMid = BreakerWorksCenter + new Vector3(Mathf.Cos(rad) * 12.5f, 0f, Mathf.Sin(rad) * 12.5f);
                    var fissure = Part(root.transform, $"LavaFissure_{i}", PhysicsSurface(fissureMid, terrains) + Vector3.up * 0.03f,
                        new Vector3(0.45f, 0.08f, 9.0f), "Ember Edge", collider: false);
                    fissure.transform.rotation = Quaternion.Euler(0f, -rad * Mathf.Rad2Deg + 90f, 0f);
                }
            }

            // Stepped Multi-Tier Obsidian Amphitheater Rim (R = 24.5m..27.5m), deeply skirted into crater slope
            for (int i = 0; i < 16; i++)
            {
                float deg = i * 22.5f;
                // Keep SE Furnace Gatehouse (~305..338 deg) and NW Viaduct Gate (~122..154 deg) wide open
                if ((deg >= 300f && deg <= 340f) || (deg >= 120f && deg <= 156f)) continue;
                float rad = deg * Mathf.Deg2Rad;
                Vector3 pInner = BreakerWorksCenter + new Vector3(Mathf.Cos(rad) * 24.2f, 0f, Mathf.Sin(rad) * 24.2f);
                Vector3 pOuter = BreakerWorksCenter + new Vector3(Mathf.Cos(rad) * 27.2f, 0f, Mathf.Sin(rad) * 27.2f);
                Vector3 gIn = PhysicsSurface(pInner, terrains);
                Vector3 gOut = PhysicsSurface(pOuter, terrains);
                Quaternion rot = Quaternion.Euler(0f, -deg + 90f, 0f);

                // Lower rim wall deeply anchored 3.5m below surface
                var lowWall = Part(root.transform, $"AmphitheaterTier1_{i}", gIn + Vector3.up * 0.1f,
                    new Vector3(7.2f, 6.4f, 1.8f), "Basalt", collider: true);
                lowWall.transform.rotation = rot;

                // Upper stepped amphitheater wall
                var highWall = Part(root.transform, $"AmphitheaterTier2_{i}", gOut + Vector3.up * 1.4f,
                    new Vector3(7.8f, 8.2f, 2.2f), "Slate", collider: true);
                highWall.transform.rotation = rot;

                // Towering Iron Forge Braziers & Chain Monoliths every 2nd segment
                if (i % 2 == 0)
                {
                    Part(root.transform, $"ForgeMonolith_{i}", gIn + Vector3.up * 2.4f,
                        new Vector3(1.5f, 8.4f, 1.5f), "Black Iron", collider: true);
                    Part(root.transform, $"ForgeBrazierBowl_{i}", gIn + Vector3.up * 6.8f,
                        new Vector3(2.1f, 0.7f, 2.1f), "Red Iron", collider: false);
                    Part(root.transform, $"ForgeBrazierFlame_{i}", gIn + Vector3.up * 7.6f,
                        new Vector3(1.3f, 1.1f, 1.3f), "Amber", collider: false);
                }
            }

            // Grand SE Furnace Gatehouse at (198, 279) — >= 6.4m wide x 5.8m tall mounted clearance
            Vector3 archCenter = new Vector3(198f, 0f, 279f);
            Quaternion archRot = Quaternion.Euler(0f, -42f, 0f);
            Vector3 gL = PhysicsSurface(archCenter + archRot * new Vector3(-4.2f, 0f, 0f), terrains);
            Vector3 gR = PhysicsSurface(archCenter + archRot * new Vector3(4.2f, 0f, 0f), terrains);
            float archTopY = Mathf.Max(gL.y, gR.y) + 6.4f;

            var pL = Part(root.transform, "FurnaceArch_LeftTower", new Vector3(gL.x, gL.y + 2.8f, gL.z), new Vector3(2.4f, 10.6f, 2.6f), "Basalt", collider: true);
            var pR = Part(root.transform, "FurnaceArch_RightTower", new Vector3(gR.x, gR.y + 2.8f, gR.z), new Vector3(2.4f, 10.6f, 2.6f), "Basalt", collider: true);
            pL.transform.rotation = archRot;
            pR.transform.rotation = archRot;
            Part(root.transform, "FurnaceArch_LeftFlame", new Vector3(gL.x, gL.y + 8.5f, gL.z), new Vector3(1.4f, 1.2f, 1.4f), "Amber", collider: false);
            Part(root.transform, "FurnaceArch_RightFlame", new Vector3(gR.x, gR.y + 8.5f, gR.z), new Vector3(1.4f, 1.2f, 1.4f), "Amber", collider: false);

            var lintel = Part(root.transform, "FurnaceArch_Lintel", new Vector3(archCenter.x, archTopY, archCenter.z), new Vector3(11.4f, 1.5f, 2.4f), "Black Iron", collider: true);
            lintel.transform.rotation = archRot;
            var crest = Part(root.transform, "FurnaceArch_EmberCrest", new Vector3(archCenter.x, archTopY + 1.0f, archCenter.z), new Vector3(4.8f, 0.7f, 1.0f), "Ember Edge", collider: false);
            crest.transform.rotation = archRot;

            // NW Broken Viaduct Gate Piers at (164, 316)
            Vector3 nwCenter = new Vector3(164f, 0f, 316f);
            Quaternion nwRot = Quaternion.Euler(0f, -45f, 0f);
            Vector3 nwL = PhysicsSurface(nwCenter + nwRot * new Vector3(-4.0f, 0f, 0f), terrains);
            Vector3 nwR = PhysicsSurface(nwCenter + nwRot * new Vector3(4.0f, 0f, 0f), terrains);
            Part(root.transform, "NWGate_PierL", nwL + Vector3.up * 2.2f, new Vector3(2.0f, 8.0f, 2.0f), "Basalt", collider: true);
            Part(root.transform, "NWGate_PierR", nwR + Vector3.up * 2.2f, new Vector3(2.0f, 8.0f, 2.0f), "Basalt", collider: true);

            // Monumental Ruined Pressure Tower on NE rim (204, 322), R ~ 32.5m (14m tall, deeply skirted)
            Vector3 towerPos = PhysicsSurface(new Vector3(204f, 0f, 322f), terrains);
            Part(root.transform, "PressureTower_Base", towerPos + Vector3.up * 1.4f, new Vector3(7.4f, 7.6f, 7.4f), "Basalt", collider: true);
            Part(root.transform, "PressureTower_Drum", towerPos + Vector3.up * 7.4f, new Vector3(5.2f, 5.8f, 5.2f), "Black Iron", collider: true);
            Part(root.transform, "PressureTower_Crown", towerPos + Vector3.up * 11.4f, new Vector3(6.2f, 2.4f, 6.2f), "Red Iron", collider: true);
            Part(root.transform, "PressureTower_EmberCore", towerPos + Vector3.up * 13.0f, new Vector3(2.8f, 1.8f, 2.8f), "Amber", collider: false);
            for (int b = 0; b < 4; b++)
            {
                float bRad = b * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                Vector3 bOff = new Vector3(Mathf.Cos(bRad) * 3.4f, 4.8f, Mathf.Sin(bRad) * 3.4f);
                var rib = Part(root.transform, $"PressureTower_Rib_{b}", towerPos + bOff, new Vector3(1.0f, 9.2f, 1.4f), "Steel", collider: false);
                rib.transform.rotation = Quaternion.Euler(0f, -bRad * Mathf.Rad2Deg, 0f);
            }

            // Quiet Rim Bypass Trail lanterns (>= 42m from Gorgonax (180, 302))
            Vector3[] bypassMarkers =
            {
                new Vector3(140f, 0f, 344f),
                new Vector3(160f, 0f, 347f),
                new Vector3(180f, 0f, 346f),
                new Vector3(205f, 0f, 340f),
                new Vector3(224f, 0f, 332f),
                new Vector3(230f, 0f, 260f)
            };
            for (int i = 0; i < bypassMarkers.Length; i++)
                AddTrailLantern(root.transform, $"BreakerBypass_Lantern_{i}", bypassMarkers[i], terrains);

            var pin = new GameObject("Breaker Works map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root.transform, false);
            pin.transform.position = PhysicsSurface(new Vector3(206f, 0f, 272f), terrains);
            pin.displayName = "Breaker Works";
            pin.notice = "Breaker Works Amphitheater. Gorgonax rules the molten basalt floor below. Enter through the SE Furnace Gatehouse or follow the outer rim lanterns to bypass.";

            report.AppendLine("Staged Monumental Breaker Works Amphitheater at (180,300).");
        }

        // =========================================================================
        // 2. WHISPERWOOD EXCHANGE (35, 250) — 100% GROUNDED WITH DEEP FOUNDATION
        // =========================================================================
        static void StageWhisperwoodExchange(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Whisperwood Exchange");
            root.transform.SetParent(parent, false);

            // Sample all 4 corners + center via active TerrainCollider raycast so NOTHING floats!
            float deckTopY = BuildGroundedFoundation(root.transform, "Exchange_Foundation", WhisperwoodCenter, 7.6f, 5.4f, 0.28f, "Limestone", terrains);
            Vector3 c = new Vector3(WhisperwoodCenter.x, deckTopY, WhisperwoodCenter.z);

            // Timber deck planks + canopy posts anchored straight into the solid deck
            Part(root.transform, "Exchange_Deck", c + Vector3.up * 0.08f, new Vector3(7.4f, 0.18f, 5.2f), "Oak", collider: true);
            Part(root.transform, "Exchange_PostNW", c + new Vector3(-3.2f, 2.3f, 2.2f), new Vector3(0.38f, 4.6f, 0.38f), "Dark Wood", collider: true);
            Part(root.transform, "Exchange_PostNE", c + new Vector3(3.2f, 2.3f, 2.2f), new Vector3(0.38f, 4.6f, 0.38f), "Dark Wood", collider: true);
            Part(root.transform, "Exchange_PostSW", c + new Vector3(-3.2f, 2.1f, -2.2f), new Vector3(0.38f, 4.2f, 0.38f), "Dark Wood", collider: true);
            Part(root.transform, "Exchange_PostSE", c + new Vector3(3.2f, 2.1f, -2.2f), new Vector3(0.38f, 4.2f, 0.38f), "Dark Wood", collider: true);
            var roofA = Part(root.transform, "Exchange_CanopyWest", c + new Vector3(-1.7f, 4.6f, 0f), new Vector3(4.2f, 0.16f, 5.8f), "Linen", collider: false);
            roofA.transform.rotation = Quaternion.Euler(0f, 0f, 14f);
            var roofB = Part(root.transform, "Exchange_CanopyEast", c + new Vector3(1.7f, 4.6f, 0f), new Vector3(4.2f, 0.16f, 5.8f), "Banner Teal", collider: false);
            roofB.transform.rotation = Quaternion.Euler(0f, 0f, -14f);
            Part(root.transform, "Exchange_Counter", c + new Vector3(0f, 0.65f, 1.3f), new Vector3(5.2f, 0.95f, 0.85f), "Oak Edge", collider: true);

            // Grounded cargo bundles & quarry ballast crates (each sunk slightly into terrain so zero gaps)
            for (int i = 0; i < 4; i++)
            {
                Vector3 bp = PhysicsSurface(new Vector3(39.4f, 0f, 248.4f + i * 0.9f), terrains);
                Part(root.transform, $"CedarBundle_{i}", bp + Vector3.up * 0.25f, new Vector3(1.5f, 0.65f, 0.65f), "Oak", collider: true);
            }
            Vector3 crateA = PhysicsSurface(new Vector3(30.4f, 0f, 247.0f), terrains);
            Vector3 crateB = PhysicsSurface(new Vector3(31.7f, 0f, 246.8f), terrains);
            Part(root.transform, "QuarryBallastCrateA", crateA + Vector3.up * 0.35f, new Vector3(1.1f, 1.0f, 1.1f), "Red Iron", collider: true);
            Part(root.transform, "QuarryBallastCrateB", crateB + Vector3.up * 0.32f, new Vector3(1.0f, 0.95f, 1.0f), "Slate", collider: true);

            // Deeply grounded 6m Stone & Timber Divider Wall
            float wallTopY = BuildGroundedFoundation(root.transform, "Exchange_DividerWall", new Vector3(26.5f, 0f, 249.5f), 0.70f, 6.2f, 1.25f, "Limestone", terrains);
            Part(root.transform, "Exchange_DividerCap", new Vector3(26.5f, wallTopY + 0.08f, 249.5f), new Vector3(0.84f, 0.16f, 6.4f), "Dark Wood", collider: false);

            // Pullout trail lanterns
            AddTrailLantern(root.transform, "Whisperwood_PulloutLanternA", new Vector3(4f, 0f, 247f), terrains);
            AddTrailLantern(root.transform, "Whisperwood_PulloutLanternB", new Vector3(19f, 0f, 248f), terrains);
            AddTrailLantern(root.transform, "Whisperwood_PulloutLanternC", new Vector3(29f, 0f, 255f), terrains);

            // Grounded Sightline Rest Bench
            Vector3 benchPos = PhysicsSurface(new Vector3(29.5f, 0f, 242.5f), terrains);
            Part(root.transform, "Exchange_RestBench", benchPos + Vector3.up * 0.22f, new Vector3(2.2f, 0.70f, 0.65f), "Oak", collider: true);

            // Quartermaster Brev standing squarely on the ground beside the stall counter
            AddTownNpc(root.transform, "Quartermaster Brev", "Whisperwood Exchange", "Human", "Warrior",
                new Vector3(34.5f, 0f, 253.8f),
                "Four cedar bundles arrived from Pinewatch, two short of the tally. Quarry ballast from Cinderroad is already stacked by the divider.",
                terrains);

            var pin = new GameObject("Whisperwood Exchange map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root.transform, false);
            pin.transform.position = PhysicsSurface(new Vector3(30.5f, 0f, 249.5f), terrains);
            pin.displayName = "Whisperwood Exchange";
            pin.notice = "Whisperwood Exchange Tally Board: 4 of 6 cedar bundles received from Pinewatch Relay; red-dust ballast crates cleared for Cinderroad return.";

            AddCivilianWalker(root.transform, "Whisperwood Exchange Porter", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(31.5f, 0f, 242.5f),
                new Vector3(37.5f, 0f, 242.5f),
                new Vector3(37.5f, 0f, 244.8f),
                new Vector3(31.5f, 0f, 244.8f)
            });
            AddCivilianWalker(root.transform, "Whisperwood Waiting Courier", "Human", "Rogue", 1.45f, 2.0f, terrains, new[]
            {
                new Vector3(30.0f, 0f, 255.0f),
                new Vector3(38.0f, 0f, 255.5f),
                new Vector3(39.0f, 0f, 257.5f),
                new Vector3(31.0f, 0f, 257.0f)
            });
            AddCivilianWalker(root.transform, "Whisperwood Treeline Scout", "Human", "Rogue", 1.40f, 2.4f, terrains, new[]
            {
                new Vector3(41.2f, 0f, 245.5f),
                new Vector3(43.2f, 0f, 247.5f),
                new Vector3(43.6f, 0f, 250.5f),
                new Vector3(43.2f, 0f, 253.5f),
                new Vector3(41.2f, 0f, 252.0f),
                new Vector3(41.0f, 0f, 248.5f)
            });

            report.AppendLine("Staged Grounded Whisperwood Exchange at (35,250).");
        }

        // =========================================================================
        // 3. SE CORNER MEGA-SITE — CINDERROAD IRONWORKS BASTION (262, -55)
        // =========================================================================
        static void StageCinderroadIronworks(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Cinderroad Waystation");
            root.transform.SetParent(parent, false);

            // Massive Stone Loading Quay + 14m Twin-Mast Gantry Crane at (262, -55)
            float quayTop = BuildGroundedFoundation(root.transform, "Ironworks_QuayDeck", CinderroadCenter, 10.5f, 8.2f, 0.65f, "Limestone", terrains);
            Vector3 craneBase = new Vector3(CinderroadCenter.x, quayTop, CinderroadCenter.z);
            Part(root.transform, "FreightCrane_MastA", craneBase + new Vector3(-1.4f, 6.8f, 0f), new Vector3(0.80f, 13.8f, 0.80f), "Dark Wood", collider: true);
            Part(root.transform, "FreightCrane_MastB", craneBase + new Vector3(1.4f, 6.8f, 0f), new Vector3(0.80f, 13.8f, 0.80f), "Dark Wood", collider: true);
            Part(root.transform, "FreightCrane_Crossbeam", craneBase + new Vector3(0f, 12.2f, 0f), new Vector3(4.2f, 0.75f, 1.1f), "Black Iron", collider: false);
            var boom = Part(root.transform, "FreightCrane_Boom", craneBase + new Vector3(2.8f, 12.8f, 1.2f), new Vector3(9.4f, 0.65f, 0.65f), "Black Iron", collider: false);
            boom.transform.rotation = Quaternion.Euler(0f, -18f, 14f);
            Part(root.transform, "FreightCrane_Counterweight", craneBase + new Vector3(-2.4f, 10.6f, -0.5f), new Vector3(1.8f, 1.8f, 1.8f), "Slate", collider: false);
            Part(root.transform, "FreightCrane_Cable", craneBase + new Vector3(6.2f, 8.6f, 2.1f), new Vector3(0.14f, 8.8f, 0.14f), "Iron", collider: false);
            Part(root.transform, "FreightCrane_QuarrySlab", craneBase + new Vector3(6.2f, 3.8f, 2.1f), new Vector3(2.2f, 1.6f, 2.2f), "Slate Edge", collider: false);

            // Grand Smelter Hall & 13m Foundry Chimney at (249, -54)
            float smelterTop = BuildGroundedFoundation(root.transform, "Storehouse_Base", new Vector3(249f, 0f, -54f), 9.6f, 7.2f, 0.45f, "Limestone", terrains);
            Vector3 sp = new Vector3(249f, smelterTop, -54f);
            Part(root.transform, "Storehouse_Walls", sp + Vector3.up * 2.4f, new Vector3(8.8f, 4.8f, 6.4f), "Basalt", collider: true);
            var sRoofA = Part(root.transform, "Storehouse_RoofNorth", sp + new Vector3(0f, 5.4f, 1.7f), new Vector3(9.8f, 0.30f, 4.4f), "Slate", collider: false);
            sRoofA.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
            var sRoofB = Part(root.transform, "Storehouse_RoofSouth", sp + new Vector3(0f, 5.4f, -1.7f), new Vector3(9.8f, 0.30f, 4.4f), "Slate", collider: false);
            sRoofB.transform.rotation = Quaternion.Euler(-22f, 0f, 0f);
            // 13m Foundry Chimney + glowing furnace mouth
            Part(root.transform, "Smelter_Chimney", sp + new Vector3(-3.2f, 6.6f, -1.8f), new Vector3(2.4f, 13.2f, 2.4f), "Black Iron", collider: true);
            Part(root.transform, "Smelter_ChimneyGlow", sp + new Vector3(-3.2f, 13.4f, -1.8f), new Vector3(1.5f, 0.7f, 1.5f), "Ember Edge", collider: false);
            Part(root.transform, "Smelter_FurnaceMouth", sp + new Vector3(0f, 1.2f, 3.25f), new Vector3(2.6f, 2.2f, 0.35f), "Amber", collider: false);

            // Fortified Gatehouse & Stone Palisade flanking the Emberford approach
            BuildGroundedFoundation(root.transform, "BastionGate_TowerL", new Vector3(250f, 0f, -42.5f), 2.8f, 2.8f, 5.6f, "Limestone", terrains);
            BuildGroundedFoundation(root.transform, "BastionGate_TowerR", new Vector3(258f, 0f, -42.5f), 2.8f, 2.8f, 5.6f, "Limestone", terrains);
            float gateH = Mathf.Max(PhysicsSurface(new Vector3(250f, 0f, -42.5f), terrains).y, PhysicsSurface(new Vector3(258f, 0f, -42.5f), terrains).y) + 5.2f;
            Part(root.transform, "BastionGate_Lintel", new Vector3(254f, gateH, -42.5f), new Vector3(10.8f, 1.2f, 2.2f), "Dark Wood", collider: true);
            Part(root.transform, "BastionGate_Banner", new Vector3(254f, gateH - 1.2f, -41.3f), new Vector3(2.2f, 2.4f, 0.14f), "Banner Red", collider: false);

            // 10m Uphill Signal Lighthouse Beacon at (274, -63)
            float beaconBaseTop = BuildGroundedFoundation(root.transform, "SignalBeacon_Base", new Vector3(274f, 0f, -63f), 4.6f, 4.6f, 2.8f, "Pale Stone", terrains);
            Vector3 bp = new Vector3(274f, beaconBaseTop, -63f);
            Part(root.transform, "SignalBeacon_Tower", bp + Vector3.up * 3.4f, new Vector3(2.8f, 6.8f, 2.8f), "Black Iron", collider: true);
            Part(root.transform, "SignalBeacon_Flame", bp + Vector3.up * 7.4f, new Vector3(1.9f, 1.5f, 1.9f), "Amber", collider: false);

            // Emberford Contour Spur trail lanterns
            Vector3[] spurLanterns =
            {
                new Vector3(86f, 0f, -36f),
                new Vector3(130f, 0f, -34f),
                new Vector3(175f, 0f, -36f),
                new Vector3(220f, 0f, -41f),
                new Vector3(254f, 0f, -46f)
            };
            for (int i = 0; i < spurLanterns.Length; i++)
                AddTrailLantern(root.transform, $"EmberfordSpur_Lantern_{i}", spurLanterns[i], terrains);

            AddTownNpc(root.transform, "Foreman Darek", "Cinderroad Ironworks", "Human", "Warrior",
                new Vector3(256.8f, 0f, -49.5f),
                "Welcome to Cinderroad Ironworks! Our smelter feeds the frontier, and the gantry hauls quarry stone straight onto the Emberford spur.",
                terrains);

            var pin = new GameObject("Cinderroad Waystation map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root.transform, false);
            pin.transform.position = PhysicsSurface(new Vector3(258f, 0f, -48f), terrains);
            pin.displayName = "Cinderroad Ironworks";
            pin.notice = "Cinderroad Ironworks Bastion: Quarry freight crane, grand smelter hall, and uphill signal lighthouse guarding the southeast frontier.";

            AddCivilianWalker(root.transform, "Cinderroad Wagon Inspector", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(258.5f, 0f, -45.5f),
                new Vector3(265.0f, 0f, -46.0f),
                new Vector3(265.5f, 0f, -49.0f),
                new Vector3(259.0f, 0f, -48.5f)
            });
            AddCivilianWalker(root.transform, "Cinderroad Approach Guard", "Human", "Warrior", 1.40f, 2.0f, terrains, new[]
            {
                new Vector3(243.5f, 0f, -45.5f),
                new Vector3(248.0f, 0f, -45.0f),
                new Vector3(248.0f, 0f, -48.5f),
                new Vector3(243.5f, 0f, -48.5f)
            });
            AddCivilianWalker(root.transform, "Cinderroad Salvage Buyer", "Human", "Rogue", 1.35f, 2.4f, terrains, new[]
            {
                new Vector3(269.0f, 0f, -50.5f),
                new Vector3(274.5f, 0f, -51.5f),
                new Vector3(274.0f, 0f, -55.5f),
                new Vector3(268.5f, 0f, -54.5f)
            });

            report.AppendLine("Staged SE Mega-Site: Cinderroad Ironworks Bastion at (262,-55).");
        }

        // =========================================================================
        // 4. SW CORNER MEGA-SITE — THORNWATCH CITADEL (-275, -110)
        // =========================================================================
        static void StageThornwatchCitadel(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Thornwatch Citadel");
            root.transform.SetParent(parent, false);

            // 15m Grand Lighthouse Watchtower at (-275, -110)
            float keepBase = BuildGroundedFoundation(root.transform, "Thornwatch_KeepBase", ThornwatchCenter, 8.6f, 8.6f, 3.2f, "Limestone", terrains);
            Vector3 tp = new Vector3(ThornwatchCenter.x, keepBase, ThornwatchCenter.z);
            Part(root.transform, "Thornwatch_KeepShaft", tp + Vector3.up * 4.8f, new Vector3(5.6f, 9.6f, 5.6f), "Pale Stone", collider: true);
            Part(root.transform, "Thornwatch_Battlement", tp + Vector3.up * 10.0f, new Vector3(6.8f, 1.4f, 6.8f), "Slate", collider: true);
            Part(root.transform, "Thornwatch_LanternCage", tp + Vector3.up * 12.2f, new Vector3(3.4f, 3.2f, 3.4f), "Black Iron", collider: false);
            Part(root.transform, "Thornwatch_BeaconFire", tp + Vector3.up * 12.2f, new Vector3(2.0f, 2.0f, 2.0f), "Amber", collider: false);
            Part(root.transform, "Thornwatch_RoofCap", tp + Vector3.up * 14.3f, new Vector3(4.4f, 1.4f, 4.4f), "Banner Teal", collider: false);

            // Garrison Barracks Hall at (-286, -104)
            float barrackBase = BuildGroundedFoundation(root.transform, "Thornwatch_BarracksBase", new Vector3(-286f, 0f, -104f), 8.2f, 6.0f, 0.45f, "Limestone", terrains);
            Vector3 bp = new Vector3(-286f, barrackBase, -104f);
            Part(root.transform, "Thornwatch_BarracksWalls", bp + Vector3.up * 2.1f, new Vector3(7.6f, 4.2f, 5.4f), "Plaster", collider: true);
            var bRoofA = Part(root.transform, "Thornwatch_BarracksRoofA", bp + new Vector3(0f, 4.7f, 1.4f), new Vector3(8.4f, 0.28f, 3.8f), "Banner Teal", collider: false);
            bRoofA.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
            var bRoofB = Part(root.transform, "Thornwatch_BarracksRoofB", bp + new Vector3(0f, 4.7f, -1.4f), new Vector3(8.4f, 0.28f, 3.8f), "Banner Teal", collider: false);
            bRoofB.transform.rotation = Quaternion.Euler(-22f, 0f, 0f);

            // Great Stone Gatehouse facing NE toward Highspire (-264, -102)
            BuildGroundedFoundation(root.transform, "Thornwatch_GateL", new Vector3(-264f, 0f, -99f), 2.8f, 2.8f, 6.0f, "Pale Stone", terrains);
            BuildGroundedFoundation(root.transform, "Thornwatch_GateR", new Vector3(-264f, 0f, -107f), 2.8f, 2.8f, 6.0f, "Pale Stone", terrains);
            float gY = Mathf.Max(PhysicsSurface(new Vector3(-264f, 0f, -99f), terrains).y, PhysicsSurface(new Vector3(-264f, 0f, -107f), terrains).y) + 5.6f;
            Part(root.transform, "Thornwatch_GateLintel", new Vector3(-264f, gY, -103f), new Vector3(2.4f, 1.3f, 10.6f), "Dark Wood", collider: true);
            Part(root.transform, "Thornwatch_Banner", new Vector3(-262.7f, gY - 1.3f, -103f), new Vector3(0.14f, 2.6f, 2.0f), "Banner Teal", collider: false);

            // Forge & Stables Pavilion at (-274, -122)
            float stabBase = BuildGroundedFoundation(root.transform, "Thornwatch_StablesBase", new Vector3(-274f, 0f, -122f), 8.0f, 5.2f, 0.35f, "Limestone", terrains);
            Vector3 stp = new Vector3(-274f, stabBase, -122f);
            Part(root.transform, "Thornwatch_StablesRoof", stp + Vector3.up * 3.5f, new Vector3(8.4f, 0.32f, 5.6f), "Oak", collider: false);
            Part(root.transform, "Thornwatch_StablesPostA", stp + new Vector3(-3.6f, 1.75f, -2.2f), new Vector3(0.4f, 3.5f, 0.4f), "Dark Wood", collider: true);
            Part(root.transform, "Thornwatch_StablesPostB", stp + new Vector3(3.6f, 1.75f, -2.2f), new Vector3(0.4f, 3.5f, 0.4f), "Dark Wood", collider: true);
            Part(root.transform, "Thornwatch_StablesPostC", stp + new Vector3(-3.6f, 1.75f, 2.2f), new Vector3(0.4f, 3.5f, 0.4f), "Dark Wood", collider: true);
            Part(root.transform, "Thornwatch_StablesPostD", stp + new Vector3(3.6f, 1.75f, 2.2f), new Vector3(0.4f, 3.5f, 0.4f), "Dark Wood", collider: true);

            // Approach lanterns linking Highspire road to Thornwatch Citadel
            AddTrailLantern(root.transform, "Thornwatch_ApproachLantern1", new Vector3(-235f, 0f, -84f), terrains);
            AddTrailLantern(root.transform, "Thornwatch_ApproachLantern2", new Vector3(-250f, 0f, -94f), terrains);
            AddTrailLantern(root.transform, "Thornwatch_ApproachLantern3", new Vector3(-261f, 0f, -103f), terrains);

            AddTownNpc(root.transform, "Captain Vaelen", "Thornwatch Citadel", "Human", "Warrior",
                new Vector3(-269.5f, 0f, -103.5f),
                "Thornwatch Citadel holds the southwest coast. Our lighthouse guides ships past the reef and watches every road below Highspire.",
                terrains);

            var pin = new GameObject("Thornwatch Citadel map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root.transform, false);
            pin.transform.position = PhysicsSurface(new Vector3(-267.5f, 0f, -103.5f), terrains);
            pin.displayName = "Thornwatch Citadel";
            pin.notice = "Thornwatch Citadel — Southwest frontier lighthouse fortress, barracks courtyard, and garrison stables.";

            AddCivilianWalker(root.transform, "Thornwatch Sentry", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(-271f, 0f, -97.5f),
                new Vector3(-273.5f, 0f, -97.5f),
                new Vector3(-276f, 0f, -97.5f),
                new Vector3(-276f, 0f, -100.5f),
                new Vector3(-273.5f, 0f, -100.5f),
                new Vector3(-271f, 0f, -100.5f)
            });
            AddCivilianWalker(root.transform, "Thornwatch Quartermaster Runner", "Human", "Rogue", 1.45f, 2.0f, terrains, new[]
            {
                new Vector3(-267.5f, 0f, -111.5f),
                new Vector3(-267.5f, 0f, -114.5f),
                new Vector3(-267.5f, 0f, -117.5f),
                new Vector3(-270.0f, 0f, -117.5f),
                new Vector3(-270.0f, 0f, -114.5f),
                new Vector3(-270.0f, 0f, -111.5f)
            });
            AddCivilianWalker(root.transform, "Thornwatch Rampart Guard", "Human", "Warrior", 1.35f, 2.4f, terrains, new[]
            {
                new Vector3(-260.5f, 0f, -98.5f),
                new Vector3(-258.5f, 0f, -101.0f),
                new Vector3(-258.5f, 0f, -104.0f),
                new Vector3(-258.5f, 0f, -107.0f),
                new Vector3(-260.5f, 0f, -106.5f),
                new Vector3(-260.5f, 0f, -102.5f)
            });

            report.AppendLine("Staged SW Mega-Site: Thornwatch Citadel at (-275,-110).");
        }

        // =========================================================================
        // SHARED ZERO-FLOATING GROUNDING HELPERS
        // =========================================================================
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
            float totalH = Mathf.Max(0.5f, topY - minY);
            Part(parent, name, new Vector3(centerXZ.x, minY + totalH * 0.5f, centerXZ.z),
                new Vector3(width, totalH, depth), materialName, collider: true);
            return topY;
        }

        static void AddTrailLantern(Transform parent, string name, Vector3 xzPos, Terrain[] terrains)
        {
            Vector3 g = PhysicsSurface(xzPos, terrains);
            Part(parent, name + "_Post", g + Vector3.up * 0.85f, new Vector3(0.24f, 3.1f, 0.24f), "Dark Wood", collider: false);
            Part(parent, name + "_Lamp", g + Vector3.up * 2.55f, new Vector3(0.40f, 0.44f, 0.40f), "Amber", collider: false);
        }

        static void AddTownNpc(Transform parent, string npcName, string roleTag, string race, string cls, Vector3 xzPos, string greeting, Terrain[] terrains)
        {
            var actor = new GameObject(npcName);
            actor.transform.SetParent(parent, false);
            actor.transform.position = PhysicsSurface(xzPos, terrains);
            CharacterVisualFactory.CreateVisualInstance(race, cls, actor.transform);
            var body = actor.AddComponent<CapsuleCollider>();
            body.height = 1.88f;
            body.radius = 0.36f;
            body.center = Vector3.up * 0.94f;
            var npc = actor.AddComponent<TownNPC>();
            npc.npcName = npcName;
            npc.roleTag = roleTag;
            npc.greeting = greeting;
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

        static GameObject Part(Transform parent, string name, Vector3 pos, Vector3 scale, string materialName, bool collider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (!collider)
            {
                var c = go.GetComponent<Collider>();
                if (c) Object.DestroyImmediate(c);
            }
            var mat = LoadMaterial(materialName);
            if (mat) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        static Material LoadMaterial(string name)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Materials + name.Replace(' ', '_') + ".mat");
            if (!mat) mat = AssetDatabase.LoadAssetAtPath<Material>(Materials + "Limestone.mat");
            return mat;
        }

        /// <summary>
        /// Raycasts active TerrainColliders first so overlapping legacy/expanded terrain tiles
        /// never place buildings or NPCs in the air.
        /// </summary>
        public static Vector3 PhysicsSurface(Vector3 point, Terrain[] terrains)
        {
            var hits = Physics.RaycastAll(new Vector3(point.x, 260f, point.z), Vector3.down, 420f);
            float best = float.MinValue;
            bool found = false;
            foreach (var h in hits)
            {
                if (h.collider is TerrainCollider && h.point.y > best)
                {
                    best = h.point.y;
                    found = true;
                }
            }
            point.y = found ? best : WorldRegions.GroundHeight(point, terrains);
            return point;
        }
    }
}
