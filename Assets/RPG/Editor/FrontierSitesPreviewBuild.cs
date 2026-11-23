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
    /// Stages the remaining Frontier Density & Gorgonax Arena sites in memory during build/verification
    /// (zero authored scene or terrain edits):
    /// 1. Breaker Works Arena (Gorgonax crater at (180, 300): open basalt floor, rim walls, SE Furnace Arch,
    ///    NE Ruined Pressure Tower, and >=42m quiet rim bypass trail).
    /// 2. Whisperwood Exchange (Centre at (35, 250): twin-post canvas canopy, divider wall, cedar & ballast cargo,
    ///    Quartermaster Brev, 3 CivilianRouteWalker loops, and Greenhollow pullout markers).
    /// 3. Cinderroad Waystation (SE at (262, -55): freight crane, covered storehouse, uphill signal beacon,
    ///    Emberford contour spur lanterns, Foreman Darek, and 3 CivilianRouteWalker loops).
    /// </summary>
    public static class FrontierSitesPreviewBuild
    {
        const string Materials = "Assets/RPG/ProceduralKit/Materials/";

        public static readonly Vector3 BreakerWorksCenter = new Vector3(180f, 0f, 300f);
        public static readonly Vector3 WhisperwoodCenter = new Vector3(35f, 0f, 250f);
        public static readonly Vector3 CinderroadCenter = new Vector3(262f, 0f, -55f);

        public static GameObject StageAll(Scene scene, StringBuilder report = null)
        {
            report ??= new StringBuilder();
            var terrains = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>()).ToArray();
            if (terrains.Length == 0) throw new InvalidOperationException("No authored terrain in scene.");

            var root = new GameObject("Frontier Density Sites");
            SceneManager.MoveGameObjectToScene(root, scene);

            StageBreakerWorks(root.transform, terrains, report);
            StageWhisperwoodExchange(root.transform, terrains, report);
            StageCinderroadWaystation(root.transform, terrains, report);

            return root;
        }

        // =========================================================================
        // 1. BREAKER WORKS ARENA (180, 300)
        // =========================================================================
        static void StageBreakerWorks(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Breaker Works Arena");
            root.transform.SetParent(parent, false);

            // Flush, non-colliding basalt arena ring collar at R=19.5m (keeps R<=19m combat floor & hoard pad 100% clear)
            for (int i = 0; i < 16; i++)
            {
                float rad = i * Mathf.PI * 2f / 16f;
                Vector3 p = BreakerWorksCenter + new Vector3(Mathf.Cos(rad) * 19.5f, 0f, Mathf.Sin(rad) * 19.5f);
                var slab = Part(root.transform, $"ArenaFloorInlay_{i}", Surface(p, terrains) + Vector3.up * 0.04f,
                    new Vector3(4.2f, 0.12f, 1.1f), "Basalt", collider: false);
                slab.transform.rotation = Quaternion.Euler(0f, -rad * Mathf.Rad2Deg + 90f, 0f);
            }

            // Broken basalt rim walls & iron-capped forge pylons at R=24m (leaving SE Arch & NW Gate wide open)
            for (int i = 0; i < 12; i++)
            {
                float deg = i * 30f;
                // Keep SE Furnace Arch (~310 deg / -50 deg) and NW Gate (~135 deg) open
                if ((deg >= 295f && deg <= 335f) || (deg >= 120f && deg <= 155f)) continue;
                float rad = deg * Mathf.Deg2Rad;
                Vector3 p = BreakerWorksCenter + new Vector3(Mathf.Cos(rad) * 24.5f, 0f, Mathf.Sin(rad) * 24.5f);
                Vector3 g = Surface(p, terrains);

                var wall = Part(root.transform, $"BasaltRimWall_{i}", g + Vector3.up * 0.95f,
                    new Vector3(5.6f, 2.2f, 1.3f), "Basalt", collider: true);
                wall.transform.rotation = Quaternion.Euler(0f, -deg + 90f, 0f);

                if (i % 2 == 0)
                {
                    Part(root.transform, $"ForgePylon_{i}", g + Vector3.up * 2.4f,
                        new Vector3(1.1f, 4.8f, 1.1f), "Black Iron", collider: true);
                    Part(root.transform, $"PylonEmber_{i}", g + Vector3.up * 4.95f,
                        new Vector3(0.65f, 0.45f, 0.65f), "Ember Edge", collider: false);
                }
            }

            // SE Furnace Arch at (198, 279) — >= 6.0m wide x 5.2m tall mounted clearance
            Vector3 archCenter = new Vector3(198f, 0f, 279f);
            Quaternion archRot = Quaternion.Euler(0f, -42f, 0f);
            Vector3 leftPillar = archCenter + archRot * new Vector3(-3.6f, 0f, 0f);
            Vector3 rightPillar = archCenter + archRot * new Vector3(3.6f, 0f, 0f);
            Vector3 gL = Surface(leftPillar, terrains);
            Vector3 gR = Surface(rightPillar, terrains);
            float archTopY = Mathf.Max(gL.y, gR.y) + 5.6f;

            var pL = Part(root.transform, "FurnaceArch_LeftPillar", new Vector3(gL.x, gL.y + 3.0f, gL.z), new Vector3(1.4f, 6.2f, 1.6f), "Basalt", collider: true);
            var pR = Part(root.transform, "FurnaceArch_RightPillar", new Vector3(gR.x, gR.y + 3.0f, gR.z), new Vector3(1.4f, 6.2f, 1.6f), "Basalt", collider: true);
            pL.transform.rotation = archRot;
            pR.transform.rotation = archRot;
            var lintel = Part(root.transform, "FurnaceArch_Lintel", new Vector3(archCenter.x, archTopY, archCenter.z), new Vector3(9.4f, 1.1f, 1.8f), "Black Iron", collider: true);
            lintel.transform.rotation = archRot;
            var crest = Part(root.transform, "FurnaceArch_EmberCrest", new Vector3(archCenter.x, archTopY + 0.7f, archCenter.z), new Vector3(3.2f, 0.45f, 0.8f), "Ember Edge", collider: false);
            crest.transform.rotation = archRot;

            // Ruined Pressure Tower on NE rim (204, 322), R ~ 32.5m
            Vector3 towerPos = Surface(new Vector3(204f, 0f, 322f), terrains);
            Part(root.transform, "PressureTower_Base", towerPos + Vector3.up * 2.0f, new Vector3(5.6f, 4.4f, 5.6f), "Basalt", collider: true);
            Part(root.transform, "PressureTower_Drum", towerPos + Vector3.up * 6.2f, new Vector3(4.2f, 4.6f, 4.2f), "Black Iron", collider: true);
            Part(root.transform, "PressureTower_Crown", towerPos + Vector3.up * 9.4f, new Vector3(4.8f, 1.8f, 4.8f), "Red Iron", collider: true);
            Part(root.transform, "PressureTower_EmberCore", towerPos + Vector3.up * 10.6f, new Vector3(2.0f, 1.2f, 2.0f), "Amber", collider: false);
            var pipe1 = Part(root.transform, "PressureTower_PipeA", towerPos + new Vector3(-2.6f, 5.2f, -1.2f), new Vector3(0.65f, 6.2f, 0.65f), "Steel", collider: false);
            pipe1.transform.rotation = Quaternion.Euler(14f, 0f, 18f);
            var pipe2 = Part(root.transform, "PressureTower_PipeB", towerPos + new Vector3(2.4f, 4.5f, 1.4f), new Vector3(0.65f, 5.4f, 0.65f), "Red Iron", collider: false);
            pipe2.transform.rotation = Quaternion.Euler(-12f, 0f, -16f);

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

            // Atlas Waypoint outside wake/aggro range
            var pin = new GameObject("Breaker Works map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root.transform, false);
            pin.transform.position = Surface(new Vector3(206f, 0f, 272f), terrains);
            pin.displayName = "Breaker Works";
            pin.notice = "Breaker Works Crater. Gorgonax holds the basalt floor below. Use the outer rim lanterns to bypass quietly, or enter through the SE Furnace Arch.";

            report.AppendLine("Staged Breaker Works Arena at (180,300) with Pressure Tower at (204,322), SE Furnace Arch at (198,279), and 6 quiet-bypass lanterns.");
        }

        // =========================================================================
        // 2. WHISPERWOOD EXCHANGE (35, 250)
        // =========================================================================
        static void StageWhisperwoodExchange(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Whisperwood Exchange");
            root.transform.SetParent(parent, false);

            Vector3 c = Surface(WhisperwoodCenter, terrains);

            // Tall Twin-Post Canvas Canopy Stall (single-tile Z=247..253, X=31..39)
            Part(root.transform, "Exchange_Deck", c + Vector3.up * 0.14f, new Vector3(7.4f, 0.36f, 5.2f), "Oak", collider: true);
            Part(root.transform, "Exchange_PostNW", c + new Vector3(-3.2f, 2.3f, 2.2f), new Vector3(0.38f, 4.6f, 0.38f), "Dark Wood", collider: true);
            Part(root.transform, "Exchange_PostNE", c + new Vector3(3.2f, 2.3f, 2.2f), new Vector3(0.38f, 4.6f, 0.38f), "Dark Wood", collider: true);
            Part(root.transform, "Exchange_PostSW", c + new Vector3(-3.2f, 2.1f, -2.2f), new Vector3(0.38f, 4.2f, 0.38f), "Dark Wood", collider: true);
            Part(root.transform, "Exchange_PostSE", c + new Vector3(3.2f, 2.1f, -2.2f), new Vector3(0.38f, 4.2f, 0.38f), "Dark Wood", collider: true);
            var roofA = Part(root.transform, "Exchange_CanopyWest", c + new Vector3(-1.7f, 4.6f, 0f), new Vector3(4.2f, 0.16f, 5.8f), "Linen", collider: false);
            roofA.transform.rotation = Quaternion.Euler(0f, 0f, 14f);
            var roofB = Part(root.transform, "Exchange_CanopyEast", c + new Vector3(1.7f, 4.6f, 0f), new Vector3(4.2f, 0.16f, 5.8f), "Banner Teal", collider: false);
            roofB.transform.rotation = Quaternion.Euler(0f, 0f, -14f);
            Part(root.transform, "Exchange_Counter", c + new Vector3(0f, 0.75f, 1.3f), new Vector3(5.2f, 0.95f, 0.85f), "Oak Edge", collider: true);

            // Shared cosmetic world thread: 4 of 6 cedar bundles (from Pinewatch) + red-dust quarry ballast crates (from Cinderroad)
            for (int i = 0; i < 4; i++)
                Part(root.transform, $"CedarBundle_{i}", Surface(new Vector3(38.8f, 0f, 248.4f + i * 0.9f), terrains) + Vector3.up * 0.35f,
                    new Vector3(1.5f, 0.55f, 0.65f), "Oak", collider: true);
            Part(root.transform, "QuarryBallastCrateA", Surface(new Vector3(31.2f, 0f, 247.2f), terrains) + Vector3.up * 0.45f, new Vector3(1.1f, 0.9f, 1.1f), "Red Iron", collider: true);
            Part(root.transform, "QuarryBallastCrateB", Surface(new Vector3(32.5f, 0f, 247.0f), terrains) + Vector3.up * 0.40f, new Vector3(1.0f, 0.8f, 1.0f), "Slate", collider: true);

            // 6m Stone & Timber Divider Wall protecting civilian foot traffic from the roadside pullout
            Vector3 wallPos = Surface(new Vector3(26.5f, 0f, 249.5f), terrains);
            Part(root.transform, "Exchange_DividerWall", wallPos + Vector3.up * 0.62f, new Vector3(0.65f, 1.25f, 6.2f), "Limestone", collider: true);
            Part(root.transform, "Exchange_DividerCap", wallPos + Vector3.up * 1.30f, new Vector3(0.80f, 0.16f, 6.4f), "Dark Wood", collider: false);

            // Pullout trail lanterns leading off Greenhollow road (keeps X=-15 highway >= 6m clear)
            AddTrailLantern(root.transform, "Whisperwood_PulloutLanternA", new Vector3(4f, 0f, 247f), terrains);
            AddTrailLantern(root.transform, "Whisperwood_PulloutLanternB", new Vector3(19f, 0f, 248f), terrains);
            AddTrailLantern(root.transform, "Whisperwood_PulloutLanternC", new Vector3(29f, 0f, 255f), terrains);

            // Sightline Rest Bench
            Vector3 benchPos = Surface(new Vector3(29.5f, 0f, 242.5f), terrains);
            Part(root.transform, "Exchange_RestBench", benchPos + Vector3.up * 0.35f, new Vector3(2.2f, 0.50f, 0.65f), "Oak", collider: true);

            // Quartermaster Brev (read-only NPC)
            AddTownNpc(root.transform, "Quartermaster Brev", "Whisperwood Exchange", "Human", "Warrior",
                new Vector3(34.5f, 0f, 252.4f),
                "Four cedar bundles arrived from Pinewatch, two short of the tally. Quarry ballast from Cinderroad is already stacked by the divider.",
                terrains);

            // Map Pin & Tally Board Waystop
            var pin = new GameObject("Whisperwood Exchange map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root.transform, false);
            pin.transform.position = Surface(new Vector3(30.5f, 0f, 249.5f), terrains);
            pin.displayName = "Whisperwood Exchange";
            pin.notice = "Whisperwood Exchange Tally Board: 4 of 6 cedar bundles received from Pinewatch Relay; red-dust ballast crates cleared for Cinderroad return.";

            // 3 CivilianRouteWalker loops strictly east of divider wall (X >= 28.5)
            AddCivilianWalker(root.transform, "Whisperwood Exchange Porter", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(29.5f, 0f, 244.5f),
                new Vector3(36.5f, 0f, 244.5f),
                new Vector3(37.5f, 0f, 246.2f),
                new Vector3(29.8f, 0f, 246.2f)
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

            report.AppendLine("Staged Whisperwood Exchange at (35,250) with canopy stall, divider wall, Quartermaster Brev, and 3 civilian walkers.");
        }

        // =========================================================================
        // 3. CINDERROAD WAYSTATION (262, -55)
        // =========================================================================
        static void StageCinderroadWaystation(Transform parent, Terrain[] terrains, StringBuilder report)
        {
            var root = new GameObject("Cinderroad Waystation");
            root.transform.SetParent(parent, false);

            // Cinderroad Freight Crane at (262, -55)
            Vector3 cranePos = Surface(CinderroadCenter, terrains);
            Part(root.transform, "FreightCrane_Foot", cranePos + Vector3.up * 0.55f, new Vector3(3.4f, 1.3f, 3.4f), "Limestone", collider: true);
            Part(root.transform, "FreightCrane_Mast", cranePos + Vector3.up * 5.0f, new Vector3(0.65f, 9.2f, 0.65f), "Dark Wood", collider: true);
            var boom = Part(root.transform, "FreightCrane_Boom", cranePos + new Vector3(2.2f, 8.6f, 0.8f), new Vector3(6.8f, 0.45f, 0.45f), "Black Iron", collider: false);
            boom.transform.rotation = Quaternion.Euler(0f, -18f, 16f);
            Part(root.transform, "FreightCrane_Counterweight", cranePos + new Vector3(-1.4f, 7.2f, -0.4f), new Vector3(1.2f, 1.3f, 1.2f), "Slate", collider: false);
            Part(root.transform, "FreightCrane_Cable", cranePos + new Vector3(4.5f, 6.2f, 1.4f), new Vector3(0.10f, 5.6f, 0.10f), "Iron", collider: false);
            Part(root.transform, "FreightCrane_QuarrySlab", cranePos + new Vector3(4.5f, 3.1f, 1.4f), new Vector3(1.5f, 1.1f, 1.5f), "Slate Edge", collider: false);

            // Covered Storehouse on lower bench at (251, -52)
            Vector3 storePos = Surface(new Vector3(251f, 0f, -52f), terrains);
            Part(root.transform, "Storehouse_Base", storePos + Vector3.up * 0.25f, new Vector3(6.4f, 0.7f, 4.8f), "Limestone", collider: true);
            Part(root.transform, "Storehouse_Walls", storePos + Vector3.up * 1.9f, new Vector3(5.8f, 2.8f, 4.2f), "Dark Wood", collider: true);
            var sRoofA = Part(root.transform, "Storehouse_RoofNorth", storePos + new Vector3(0f, 3.7f, 1.2f), new Vector3(6.8f, 0.24f, 3.2f), "Slate", collider: false);
            sRoofA.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
            var sRoofB = Part(root.transform, "Storehouse_RoofSouth", storePos + new Vector3(0f, 3.7f, -1.2f), new Vector3(6.8f, 0.24f, 3.2f), "Slate", collider: false);
            sRoofB.transform.rotation = Quaternion.Euler(-20f, 0f, 0f);

            // Uphill Signal Beacon at (273, -62)
            Vector3 beaconPos = Surface(new Vector3(273f, 0f, -62f), terrains);
            Part(root.transform, "SignalBeacon_Base", beaconPos + Vector3.up * 1.1f, new Vector3(3.2f, 2.4f, 3.2f), "Pale Stone", collider: true);
            Part(root.transform, "SignalBeacon_Tower", beaconPos + Vector3.up * 4.0f, new Vector3(1.8f, 3.8f, 1.8f), "Black Iron", collider: true);
            Part(root.transform, "SignalBeacon_Flame", beaconPos + Vector3.up * 6.3f, new Vector3(1.3f, 1.0f, 1.3f), "Amber", collider: false);

            // Quarry cargo stacks beside crane
            Part(root.transform, "QuarryCargo_A", Surface(new Vector3(260f, 0f, -59f), terrains) + Vector3.up * 0.50f, new Vector3(1.6f, 1.0f, 1.4f), "Slate", collider: true);
            Part(root.transform, "QuarryCargo_B", Surface(new Vector3(262.4f, 0f, -59.5f), terrains) + Vector3.up * 0.45f, new Vector3(1.4f, 0.9f, 1.3f), "Red Iron", collider: true);

            // Emberford Contour Spur trail lanterns ((86,-36) -> (254,-47))
            Vector3[] spurLanterns =
            {
                new Vector3(86f, 0f, -36f),
                new Vector3(130f, 0f, -34f),
                new Vector3(175f, 0f, -36f),
                new Vector3(220f, 0f, -41f),
                new Vector3(254f, 0f, -47f)
            };
            for (int i = 0; i < spurLanterns.Length; i++)
                AddTrailLantern(root.transform, $"EmberfordSpur_Lantern_{i}", spurLanterns[i], terrains);

            // Foreman Darek (read-only NPC)
            AddTownNpc(root.transform, "Foreman Darek", "Cinderroad Waystation", "Human", "Warrior",
                new Vector3(256.8f, 0f, -52.0f),
                "The Emberford spur holds steady for mounts. We ship quarry stone north to Whisperwood as fast as the crane can swing it.",
                terrains);

            // Map Pin & Freight Slate Waystop
            var pin = new GameObject("Cinderroad Waystation map pin").AddComponent<SettlementWaystop>();
            pin.transform.SetParent(root.transform, false);
            pin.transform.position = Surface(new Vector3(258f, 0f, -49f), terrains);
            pin.displayName = "Cinderroad Waystation";
            pin.notice = "Cinderroad Freight Slate: Quarry stone ballast dispatched along the Emberford contour spur toward Whisperwood Exchange.";

            // 3 CivilianRouteWalker loops clear of crane, storehouse, and beacon colliders
            AddCivilianWalker(root.transform, "Cinderroad Wagon Inspector", "Human", "Warrior", 1.35f, 2.2f, terrains, new[]
            {
                new Vector3(256.5f, 0f, -46.0f),
                new Vector3(264.5f, 0f, -46.5f),
                new Vector3(265.0f, 0f, -50.2f),
                new Vector3(257.0f, 0f, -49.8f)
            });
            AddCivilianWalker(root.transform, "Cinderroad Approach Guard", "Human", "Warrior", 1.40f, 2.0f, terrains, new[]
            {
                new Vector3(244.0f, 0f, -45.0f),
                new Vector3(251.5f, 0f, -44.5f),
                new Vector3(252.0f, 0f, -47.5f),
                new Vector3(244.5f, 0f, -48.0f)
            });
            AddCivilianWalker(root.transform, "Cinderroad Salvage Buyer", "Human", "Rogue", 1.35f, 2.4f, terrains, new[]
            {
                new Vector3(268.0f, 0f, -50.0f),
                new Vector3(274.0f, 0f, -51.0f),
                new Vector3(273.5f, 0f, -55.5f),
                new Vector3(267.5f, 0f, -54.5f)
            });

            report.AppendLine("Staged Cinderroad Waystation at (262,-55) with freight crane, storehouse, uphill beacon, 5 Emberford spur lanterns, Foreman Darek, and 3 civilian walkers.");
        }

        // =========================================================================
        // SHARED HELPERS
        // =========================================================================
        static void AddTrailLantern(Transform parent, string name, Vector3 xzPos, Terrain[] terrains)
        {
            Vector3 g = Surface(xzPos, terrains);
            Part(parent, name + "_Post", g + Vector3.up * 1.25f, new Vector3(0.24f, 2.5f, 0.24f), "Dark Wood", collider: false);
            Part(parent, name + "_Lamp", g + Vector3.up * 2.55f, new Vector3(0.38f, 0.42f, 0.38f), "Amber", collider: false);
        }

        static void AddTownNpc(Transform parent, string npcName, string roleTag, string race, string cls, Vector3 xzPos, string greeting, Terrain[] terrains)
        {
            var actor = new GameObject(npcName);
            actor.transform.SetParent(parent, false);
            actor.transform.position = Surface(xzPos, terrains);
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
                wp.transform.position = Surface(rawPoints[i], terrains);
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

        static Vector3 Surface(Vector3 point, Terrain[] terrains)
        {
            point.y = WorldRegions.GroundHeight(point, terrains);
            return point;
        }
    }
}
