using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    public sealed class BrassHollows : MonoBehaviour
    {
        public const string QuestTitle = "Cut the Siphon";
        public const string Engineer = "Tinker Fizzlebang";
        static readonly Vector3 EntranceSite = new(135f, 0f, 183f);
        static readonly Vector3 InteriorOrigin = new(642f, 36f, 163f);
        static readonly string[] EnemyIds = { "hollows-sentry", "hollows-lookout", "hollows-bruiser", "hollows-channeler", "hollows-veyl" };
        static readonly string[] EnemyNames = { "Choir sentry", "Choir lookout", "Choir bruiser", "Choir channeler", "Foreman Veyl" };
        static readonly Vector3[] EnemySpawns = {
            new(-3f, 0f, 8f), new(2.4f, 0f, 13f), new(3.4f, 0f, 27f), new(-2.8f, 0f, 32f), new(0f, 0f, 49f)
        };
        static BrassHollows current;
        [SerializeField] List<Material> materials = new();
        Material shale, slate, oak, bronze, amber, cooled, danger;
        Transform chamber, pump;
        Renderer pumpCore, ventCore;
        Light pumpLight;
        Mesh rockMesh;
        Transform serviceDoor;
        PlayerController entrant;
        float lastVentCycle = -1;
        public static BrassHollows Current => current;
        [field: SerializeField] public Vector3 Entrance { get; private set; }

        void OnEnable()
        {
            if (Application.isPlaying && transform.Find("Mine mouth"))
                current = this;
        }
        public static bool Inside(Vector3 point) =>
            point.x > InteriorOrigin.x - 13f && point.x < InteriorOrigin.x + 13f &&
            point.z > InteriorOrigin.z - 3f && point.z < InteriorOrigin.z + 61f &&
            point.y > InteriorOrigin.y - 2f && point.y < InteriorOrigin.y + 10f;

        public static void Ensure()
        {
            if (current || !WorldRegions.Expanded || Terrain.activeTerrains.Length == 0) return;
            current = new GameObject("The Brass Hollows").AddComponent<BrassHollows>();
            current.BuildEntrance();
        }

#if UNITY_EDITOR
        public static GameObject CreateScenePreview()
        {
            if (!WorldRegions.Expanded || Terrain.activeTerrains.Length == 0) return null;
            var preview = new GameObject("Frontier preview — Brass Hollows") { hideFlags = HideFlags.DontSave };
            preview.AddComponent<BrassHollows>().BuildEntrance();
            return preview;
        }
#endif

        void BuildEntrance()
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (!lit) lit = Shader.Find("Standard");
            shale = Paint(lit, "Charcoal shale", new Color(.16f, .13f, .11f));
            slate = Paint(lit, "Copper-streaked slate", new Color(.27f, .21f, .17f));
            oak = Paint(lit, "Mine timber", new Color(.33f, .17f, .087f));
            bronze = Paint(lit, "Weathered brass", new Color(.61f, .36f, .13f));
            amber = Paint(lit, "Lantern amber", new Color(.95f, .58f, .16f), new Color(1f, .44f, .055f));
            cooled = Paint(lit, "Cold pump glass", new Color(.13f, .30f, .29f));
            danger = Paint(lit, "Steam warning", new Color(.8f, .20f, .055f), new Color(1f, .13f, .015f));

            Entrance = Surface(EntranceSite);
            var mouth = new GameObject("Mine mouth").transform;
            mouth.SetParent(transform);
            mouth.SetPositionAndRotation(Entrance + Vector3.forward * 1.5f, Quaternion.Euler(0, -14f, 0));
            Part(mouth, "Dark shaft", new Vector3(0, 1.5f, 1.1f), new Vector3(3.2f, 3.3f, .65f), shale);
            PortalFrame(mouth, 0f, 1.72f, 3.0f);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int tier = 0; tier < 3; tier++)
                {
                    var rock = Rock(mouth, "Fractured rock face", new Vector3(side * (2.5f - tier * .34f), .4f + tier * .83f, .54f),
                        new Vector3(1.95f, 1.36f, 1.79f), tier % 2 == 0 ? slate : shale);
                    rock.localRotation = Quaternion.Euler(side * (12 + tier * 8), tier * 19, side * (18 - tier * 7));
                }
            }
            Rock(mouth, "Overhanging shale", new Vector3(0, 3.36f, .43f), new Vector3(4.15f, .80f, 1.72f), slate)
                .localRotation = Quaternion.Euler(5, 7, -7);
            Lantern(mouth, new Vector3(1.4f, 2.3f, -.42f));
            Vector3 marker = Surface(Vector3.Lerp(new Vector3(134, 0, 157), Entrance, .48f));
            Part(transform, "Mine waymark", marker + Vector3.up * .83f, new Vector3(.15f,1.66f,.15f), oak);
            Part(transform, "Brass trail flag", marker + new Vector3(.23f,1.30f,0), new Vector3(.42f,.18f,.06f), bronze);
            var cart = new GameObject("Broken ore cart").transform;
            cart.SetParent(transform);
            cart.SetPositionAndRotation(Surface(Entrance + new Vector3(-3.2f,0,-2.3f)), Quaternion.Euler(0,29,0));
            Part(cart, "Ore cart bed", new Vector3(0,.42f,0), new Vector3(1.12f,.15f,1.55f), oak);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(cart, "Cart trough", new Vector3(side*.53f,.64f,0), new Vector3(.15f,.43f,1.55f), oak);
                Part(cart, "Crushed wheel", new Vector3(side*.56f,.21f,-.47f), new Vector3(.19f,.40f,.43f), bronze);
            }
            for (int i = 0; i < 4; i++)
                Part(cart, "Spilled ore", new Vector3(-.35f+i*.22f,.59f,-.23f+i*.14f), new Vector3(.21f,.22f,.24f), slate)
                    .localRotation = Quaternion.Euler(28*i, 21*i, 12*i);
        }

        void BuildInterior(PlayerController player)
        {
            if (chamber) return;
            chamber = new GameObject("Brass Hollows underground passage").transform;
            chamber.SetParent(transform);
            chamber.position = InteriorOrigin;
            entrant = player;

            Section("Ore Landing", 0f, 17f, 5.5f, 5.0f);
            Section("Lower drive", 17f, 22f, 2.4f, 3.6f);
            Section("Pump Gallery", 22f, 36f, 6.5f, 5.5f);
            Section("Siphon tunnel", 36f, 42f, 2.6f, 3.6f);
            Section("Bell Chamber", 42f, 59f, 8f, 6.4f);
            Part(chamber, "Entrance rock cap", new Vector3(0,2.5f,-.55f), new Vector3(12.2f,5.5f,1.1f), shale, true);
            Part(chamber, "Far rock cap", new Vector3(0,3.2f,59.55f), new Vector3(17.2f,7f,1.1f), shale, true);
            foreach (float z in new[] { 1f, 17f, 22f, 36f, 42f, 56f }) PortalFrame(chamber, z, 2.25f, 3.25f);
            Lantern(chamber, new Vector3(-2.15f,2.8f,3.2f));
            Lantern(chamber, new Vector3(4.2f,2.65f,12.1f));
            Lantern(chamber, new Vector3(-5.3f,2.8f,27.8f));
            Lantern(chamber, new Vector3(6.1f,3.15f,51f));
            for (int i = 0; i < 12; i++)
                Part(chamber, "Trestle sleeper", new Vector3(1.9f,.022f,2.9f+i*.8f), new Vector3(1.4f,.045f,.17f), oak);
            foreach (float x in new[] { 1.46f, 2.34f })
                Part(chamber, "Ore cart rail", new Vector3(x,.05f,7.3f), new Vector3(.045f,.055f,9.3f), bronze);

            // The vent’s flashing plate warns players before its damage pulse.
            ventCore = Part(chamber, "Pressure vent", new Vector3(0,.025f,29.5f), new Vector3(2.6f,.05f,2.6f), bronze).GetComponent<Renderer>();
            pump = new GameObject("Choir siphon shutoff").transform;
            pump.SetParent(chamber, false);
            pump.localPosition = new Vector3(0,0,55.5f);
            Part(pump, "Stone foundation", new Vector3(0,.20f,0), new Vector3(3.25f,.4f,1.9f), slate, true);
            Part(pump, "Siphon housing", new Vector3(0,1.20f,.15f), new Vector3(1.4f,2f,1.1f), bronze, true);
            pumpCore = Part(pump, "Resonance glass", new Vector3(0,1.43f,-.43f), new Vector3(.57f,1.13f,.18f), amber).GetComponent<Renderer>();
            for (int side = -1; side <= 1; side += 2)
            {
                Part(pump, "Pressure return riser", new Vector3(side*1.22f,1.08f,.34f), new Vector3(.24f,2.15f,.24f), bronze);
                Part(pump, "Pressure return header", new Vector3(side*.63f,2.10f,.34f), new Vector3(1.29f,.24f,.24f), bronze);
            }
            pumpLight = Lantern(pump, new Vector3(0,3.7f,-.8f));
            pumpLight.range = 11f;
            serviceDoor = Part(chamber, "Service hatch", new Vector3(3f,1.32f,58.96f), new Vector3(1.5f,2.65f,.18f), oak);
            Part(chamber, "Service hatch lintel", new Vector3(3f,2.79f,58.81f), new Vector3(1.8f,.20f,.24f), bronze);
            Lantern(chamber, new Vector3(3.85f,2.68f,58.6f));

            for (int i = 0; i < EnemyIds.Length; i++)
            {
                if (player.HasDefeated(EnemyIds[i])) continue;
                RiverReaverCamp.CreateEnemy(chamber, player, EnemyIds[i], EnemyNames[i],
                    i == 3 ? "Undead" : "Human", i == 3 ? "Mage" : i <= 1 ? "Rogue" : "Warrior",
                    InteriorOrigin + EnemySpawns[i] + Vector3.up * .06f, i == 4, i == 3);
            }
            RefreshPump(player.BrassHollowsStage >= 2);
        }

        void Section(string label, float from, float to, float halfWidth, float ceiling)
        {
            float center = (from + to) / 2f, length = to - from;
            Part(chamber, label + " floor", new Vector3(0,-.45f,center), new Vector3(halfWidth*2+1,.9f,length), slate, true);
            Part(chamber, label + " ceiling", new Vector3(0,ceiling+.42f,center), new Vector3(halfWidth*2+1,.84f,length), shale, true);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(chamber, label + " wall", new Vector3(side*(halfWidth+.36f),ceiling*.5f,center),
                    new Vector3(.84f,ceiling+.5f,length+.03f), shale, true);
                for (int i = 0; i < Mathf.FloorToInt(length / 3f); i++)
                {
                    var slab = Rock(chamber, "Banded rock", new Vector3(side*(halfWidth-.18f),.58f+(i%3)*.61f,from+1.65f+i*3f),
                        new Vector3(.68f,1.05f,2.3f), i%2 == 0 ? slate : shale);
                    slab.localRotation = Quaternion.Euler(side*12, side*(i%3)*13, side*15);
                }
            }
            if (halfWidth > 4f)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float span = halfWidth - 2.2f;
                    foreach (float z in new[] { from, to })
                        Part(chamber, "Chamber shoulder", new Vector3(side*(2.2f+span*.5f),ceiling*.5f,z),
                            new Vector3(span,ceiling,.65f), slate, true);
                }
            }
        }

        void PortalFrame(Transform parent, float z, float halfWidth, float height)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part(parent, "Braced timber upright", new Vector3(side*halfWidth,height*.5f,z),
                    new Vector3(.28f,height,.34f), oak).localRotation = Quaternion.Euler(0,0,-side*6f);
                Part(parent, "Riveted binding", new Vector3(side*(halfWidth-.13f),height-.44f,z),
                    new Vector3(.32f,.22f,.38f), bronze);
            }
            Part(parent, "Cut timber lintel", new Vector3(0,height,z), new Vector3(halfWidth*2+.24f,.29f,.43f), oak);
        }

        Light Lantern(Transform parent, Vector3 position)
        {
            Part(parent, "Lantern cage", position, new Vector3(.29f,.43f,.29f), bronze);
            Part(parent, "Lantern glow", position + Vector3.forward * .03f, new Vector3(.20f,.28f,.30f), amber);
            var lamp = new GameObject("Lantern light").AddComponent<Light>();
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = position + new Vector3(0,-.22f,.3f);
            lamp.type = LightType.Point;
            lamp.color = new Color(1f,.62f,.31f);
            lamp.range = 8f;
            lamp.intensity = 3.4f;
            return lamp;
        }

        static Vector3 Surface(Vector3 point)
        {
            point.y = WorldRegions.GroundHeight(point) + .03f;
            return point;
        }

        Material Paint(Shader lit, string name, Color color, Color emission = default)
        {
            var material = new Material(lit) { color = color, name = name };
            material.SetFloat("_Smoothness", name.Contains("brass") ? .35f : .12f);
            if (emission != default)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            materials.Add(material);
            return material;
        }

        static Transform Part(Transform parent, string name, Vector3 point, Vector3 scale, Material color, bool solid = false)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = point;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = color;
            part.GetComponent<Collider>().enabled = solid;
            return part.transform;
        }

        Transform Rock(Transform parent, string label, Vector3 point, Vector3 scale, Material material)
        {
            if (!rockMesh)
            {
                Vector3[] corners = {
                    new(-.49f,-.22f,-.32f), new(-.39f,.25f,-.40f), new(.28f,.46f,-.27f),
                    new(.48f,.08f,-.34f), new(.38f,-.44f,-.30f),
                    new(-.39f,-.40f,.38f), new(-.48f,.15f,.41f), new(.25f,.36f,.46f),
                    new(.44f,.10f,.39f), new(.24f,-.35f,.46f)
                };
                int[] faces = { 0,1,2, 0,2,4, 2,3,4, 5,7,6, 5,9,7, 7,9,8,
                    0,6,1, 0,5,6, 1,6,7, 1,7,2, 2,7,8, 2,8,3, 3,8,9, 3,9,4, 4,9,5, 4,5,0 };
                var vertices = new Vector3[faces.Length];
                var indices = new int[faces.Length];
                for (int i = 0; i < faces.Length; i += 3)
                {
                    Vector3 a = corners[faces[i]], b = corners[faces[i+1]], c = corners[faces[i+2]];
                    if (Vector3.Dot(Vector3.Cross(b-a, c-a), a+b+c) < 0f) (b, c) = (c, b);
                    vertices[i] = a; vertices[i+1] = b; vertices[i+2] = c;
                    indices[i] = i; indices[i+1] = i+1; indices[i+2] = i+2;
                }
                rockMesh = new Mesh { name = "Brass Hollows faceted shale", vertices = vertices, triangles = indices };
                rockMesh.RecalculateNormals();
            }
            var rock = new GameObject(label);
            rock.transform.SetParent(parent, false);
            rock.transform.localPosition = point;
            rock.transform.localScale = scale;
            rock.AddComponent<MeshFilter>().sharedMesh = rockMesh;
            rock.AddComponent<MeshRenderer>().sharedMaterial = material;
            return rock.transform;
        }

        void Update()
        {
            if (!chamber || !ventCore || FrontendMenu.IsOpen) return;
            var player = entrant;
            if (!player || !Inside(player.transform.position) || player.BrassHollowsStage != 1) return;
            float cycle = Time.time % 6f;
            ventCore.sharedMaterial = cycle >= 4f ? danger : bronze;
            if (cycle < 5.4f || cycle >= 5.7f || Mathf.Floor(Time.time / 6f) == lastVentCycle) return;
            lastVentCycle = Mathf.Floor(Time.time / 6f);
            Vector3 vent = InteriorOrigin + new Vector3(0,0,29.5f);
            SpellFX.CreateArcaneBurst(vent);
            Vector3 distance = player.transform.position - vent;
            if (Mathf.Abs(distance.x) < 1.3f && Mathf.Abs(distance.z) < 1.3f) player.Stats.Damage(15f);
        }

        public int DefeatedCount(PlayerController player)
        {
            int count = 0;
            foreach (string id in EnemyIds) if (player.HasDefeated(id)) count++;
            return count;
        }

        public bool TryDestination(PlayerController player, out Vector3 position, out string caption)
        {
            position = Entrance; caption = "Enter the Brass Hollows";
            if (player.BrassHollowsStage == 1 && chamber && Inside(player.transform.position))
            {
                for (int i = 0; i < EnemyIds.Length; i++)
                {
                    if (player.HasDefeated(EnemyIds[i])) continue;
                    var enemy = EnemyController.FindByKey(EnemyIds[i]);
                    position = enemy ? enemy.transform.position : InteriorOrigin + EnemySpawns[i];
                    caption = "Defeat " + EnemyNames[i];
                    return true;
                }
                position = pump.position;
                caption = "Shut down the siphon";
            }
            else if (player.BrassHollowsStage == 2)
            {
                if (Inside(player.transform.position))
                {
                    position = InteriorOrigin + new Vector3(3f, 0, 57.5f);
                    caption = "Take the service exit";
                    return true;
                }
                foreach (var npc in FindObjectsByType<TownNPC>(FindObjectsSortMode.None))
                {
                    if (npc.npcName != Engineer) continue;
                    position = npc.transform.position;
                    caption = "Return to Fizzlebang";
                    return true;
                }
            }
            return player.BrassHollowsStage is 1 or 2;
        }

        public bool TryInteraction(PlayerController player, out PlayerController.InteractionCandidate candidate)
        {
            candidate = default;
            if (!player.CanUseBrassHollowsQuest) return false;
            Vector3 point = player.transform.position;
            if (!Inside(point))
            {
                if ((point - Entrance).sqrMagnitude > 3.1f * 3.1f) return false;
                return Near(point, Entrance, "Enter the Brass Hollows", () =>
                {
                    if (player.BrassHollowsStage == 0) { player.Message = "Speak to Tinker Fizzlebang in Rustwater about the sealed mine."; return; }
                    BuildInterior(player);
                    if (player.TravelBrassHollows(InteriorOrigin + new Vector3(0,.12f,2f), Quaternion.identity))
                        ZoneBanner.Show("The Brass Hollows", "Ore Landing");
                }, out candidate);
            }

            if (Near(point, InteriorOrigin + new Vector3(0,0,1.1f), "Leave the Brass Hollows",
                () => Leave(player), out candidate)) return true;
            if (player.BrassHollowsStage >= 2)
                return Near(point, InteriorOrigin + new Vector3(3f,0,57.5f), "Take the service exit", () => Leave(player), out candidate);
            if (!pump) return false;
            return Near(point, pump.position, "Shut down the siphon", () =>
            {
                if (DefeatedCount(player) < EnemyIds.Length) { player.Message = "Clear all five Choir enemies before touching the siphon."; return; }
                player.ShutDownBrassSiphon();
                RefreshPump(true);
            }, out candidate);
        }

        void RefreshPump(bool stopped)
        {
            pumpCore.sharedMaterial = stopped ? cooled : amber;
            ventCore.sharedMaterial = stopped ? cooled : bronze;
            pumpLight.intensity = stopped ? 1f : 3.4f;
            serviceDoor.localPosition = new Vector3(stopped ? 4.48f : 3f, 1.32f, 58.96f);
        }

        void Leave(PlayerController player)
        {
            if (player.TravelBrassHollows(Surface(Entrance - Vector3.forward * 2f) + Vector3.up * .15f,
                Quaternion.Euler(0,180f,0))) ZoneBanner.Show("Redridge Pass", "Brass Hollows approach");
        }

        static bool Near(Vector3 point, Vector3 goal, string name, System.Action action, out PlayerController.InteractionCandidate candidate)
        {
            float distance = Vector3.Distance(point, goal);
            candidate = new PlayerController.InteractionCandidate { text = name, execute = action, distance = distance };
            return distance <= 3.1f;
        }

        void OnDestroy()
        {
            if (current == this) current = null;
            foreach (var material in materials)
            {
                if (!material) continue;
                if (Application.isPlaying) Destroy(material);
                else DestroyImmediate(material);
            }
            if (rockMesh)
            {
                if (Application.isPlaying) Destroy(rockMesh);
                else DestroyImmediate(rockMesh);
            }
        }
    }

    public partial class PlayerController
    {
        public int BrassHollowsStage { get; private set; }
        public bool CanUseBrassHollowsQuest => WorldRegions.Expanded && !Network.CoopSessionConfig.IsMultiplayer;
        public bool BrassHollowsQuestActive => CanUseBrassHollowsQuest && BrassHollowsStage is 1 or 2;

        public string BrassHollowsObjective => BrassHollowsStage switch
        {
            1 => $"Defeat the Choir in the Brass Hollows ({(BrassHollows.Current ? BrassHollows.Current.DefeatedCount(this) : 0)}/5), then shut down the siphon.",
            2 => "Take the service exit and return the regulator to Tinker Fizzlebang in Rustwater.",
            3 => "The siphon is silent and the regulator has been returned.",
            _ => "Speak to Tinker Fizzlebang at Rustwater Scrapworks about the missing regulator."
        };

        public void HandleBrassHollowsQuest()
        {
            if (!CanUseBrassHollowsQuest || !ActiveTownNPC || ActiveTownNPC.npcName != BrassHollows.Engineer) return;
            if (BrassHollowsStage == 0)
            {
                BrassHollowsStage = 1;
                BrassHollows.Ensure();
                MinimapRadar.Instance?.FollowBrassHollows();
                PostBrassQuest("Cut the Siphon accepted. Find the Brass Hollows off Redridge Pass.");
            }
            else if (BrassHollowsStage == 2)
            {
                BrassHollowsStage = 3;
                AddGold(55);
                AddExperience(240);
                PostBrassQuest("Cut the Siphon complete. Fizzlebang paid 55 gold for the regulator.");
            }
        }

        public void ShutDownBrassSiphon()
        {
            if (!BrassHollowsQuestActive || BrassHollowsStage != 1) return;
            BrassHollowsStage = 2;
            PostBrassQuest("The siphon falls silent. Take the service exit and bring the regulator to Fizzlebang.");
        }

        void PostBrassQuest(string message)
        {
            AudioEffects.PlayClick();
            ChatSystem.LogQuest(message);
            if (SaveAdventure(false)) Message = message;
        }

        public bool TravelBrassHollows(Vector3 destination, Quaternion facing, bool savePosition = true)
        {
            if (Stats && Time.time - Stats.LastHit < 3f) { Message = "Get clear before using the mine passage."; return false; }
            if (Mounted) { Message = "Dismount before using the mine passage."; return false; }
            if (!motor || !motor.enabled) return false;
            ClearCombatInputBuffer();
            SetTarget(null);
            vertical = 0f;
            dodgeEnds = 0f;
            moveVelocity = Vector3.zero;
            motor.enabled = false;
            transform.SetPositionAndRotation(destination, facing);
            motor.enabled = true;
            if (orbit) orbit.FaceTravel(facing.eulerAngles.y);
            if (savePosition) SaveAdventure(false);
            return true;
        }
    }
}
