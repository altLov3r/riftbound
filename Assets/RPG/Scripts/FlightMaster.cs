using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    public class FlightMaster : MonoBehaviour
    {
        public static FlightMaster Instance { get; private set; }
        public static FlightMaster ActiveFlightMaster { get; private set; }

        public string nodeName = "Emberwatch Gryphon Master";
        public string faction = "Alliance"; // "Alliance", "Horde", "Neutral"
        public Vector3 flightStationPos;

        public struct FlightPathDestination
        {
            public string name;
            public string zone;
            public string faction;
            public Vector3 targetPos;
            public int costGold;
        }

        public static FlightPathDestination[] WorldFlightNodes => WorldRegions.Expanded ? ExpandedFlightNodes : LegacyFlightNodes;
        static readonly FlightPathDestination[] ExpandedFlightNodes = {
            new FlightPathDestination { name = "Emberwatch Outpost", zone = "Emberwatch Outpost", faction = "Alliance", targetPos = new Vector3(2.2f, 0f, 10.0f), costGold = 0 },
            new FlightPathDestination { name = "Highspire Keep", zone = "Highspire Keep", faction = "Alliance", targetPos = new Vector3(-169.0f, 12.1f, -31.0f), costGold = 5 },
            new FlightPathDestination { name = "Thornwatch Citadel", zone = "Thornwatch Citadel", faction = "Alliance", targetPos = new Vector3(-261.0f, 0f, -94.0f), costGold = 5 },
            new FlightPathDestination { name = "Cinderroad Ironworks", zone = "Cinderroad Ironworks", faction = "Neutral", targetPos = new Vector3(258.5f, 0f, -41.5f), costGold = 5 },
            new FlightPathDestination { name = "Whisperwood Exchange", zone = "Whisperwood Exchange", faction = "Neutral", targetPos = new Vector3(43.5f, 0f, 246.0f), costGold = 5 },
            new FlightPathDestination { name = "Duskwood Cathedral Grounds", zone = "Duskwood Cathedral", faction = "Neutral", targetPos = new Vector3(-208.0f, 18.1f, 246.0f), costGold = 5 },
            new FlightPathDestination { name = "Camp Bloodrock", zone = "Camp Bloodrock", faction = "Horde", targetPos = new Vector3(290.0f, 18.1f, 203.0f), costGold = 5 },
            new FlightPathDestination { name = "Gor'Kresh Stronghold", zone = "Gor'Kresh Stronghold", faction = "Horde", targetPos = new Vector3(348.0f, 27.8f, 314.0f), costGold = 5 },
            new FlightPathDestination { name = "Greenhollow Post", zone = "Greenhollow Valley", faction = "Neutral", targetPos = new Vector3(-35.0f, 13.0f, 392.0f), costGold = 5 },
            new FlightPathDestination { name = "Pinewatch Relay", zone = "Pinewatch Relay", faction = "Neutral", targetPos = new Vector3(-214.0f, 0f, 392.0f), costGold = 5 },
            new FlightPathDestination { name = "Wyrmcrest Summit", zone = "Wyrmcrest Summit", faction = "Neutral", targetPos = new Vector3(250.0f, 75.1f, 500.0f), costGold = 10 }
        };
        static readonly FlightPathDestination[] LegacyFlightNodes = new FlightPathDestination[]
        {
            new FlightPathDestination { name = "Highspire Keep (Alliance Capital)", zone = "Highspire Keep", faction = "Alliance", targetPos = new Vector3(-45f, 1.2f, -20f), costGold = 5 },
            new FlightPathDestination { name = "Emberwatch Outpost", zone = "Amber Vale", faction = "Alliance", targetPos = new Vector3(2f, 0.5f, 10f), costGold = 0 },
            new FlightPathDestination { name = "Duskwood Cathedral Grounds", zone = "Duskwood", faction = "Neutral", targetPos = new Vector3(-50f, 12.8f, 205f), costGold = 5 },
            new FlightPathDestination { name = "Camp Bloodrock", zone = "Redridge Canyons", faction = "Horde", targetPos = new Vector3(50f, 9.2f, 210f), costGold = 5 },
            new FlightPathDestination { name = "Gor'Kresh Stronghold (Horde Capital)", zone = "Gor'Kresh", faction = "Horde", targetPos = new Vector3(85f, 14.0f, 235f), costGold = 5 },
            new FlightPathDestination { name = "Wyrmcrest Summit (Mountain Peak)", zone = "Ironfang Peaks", faction = "Neutral", targetPos = new Vector3(0f, 49f, 215f), costGold = 10 }
        };

        public bool TaxiMapOpen { get; private set; }
        public bool InFlight { get; private set; }
        public static bool IsAnyTaxiInFlight { get; private set; }
        public static bool IsAnyTaxiMapOpen =>
            (ActiveFlightMaster != null && ActiveFlightMaster.TaxiMapOpen) ||
            (Instance != null && Instance.TaxiMapOpen);

        PlayerController player;
        GameObject perchedGryphon;
        Vector2 taxiScrollPos;
        GUIStyle winTitleStyle, nodeBtnStyle, subStyle, costStyle;
        Texture2D mapBgTex, borderTex;

        public GameObject PerchedGryphon => perchedGryphon;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
            ActiveFlightMaster = null;
            IsAnyTaxiInFlight = false;
        }

        void Awake()
        {
            flightStationPos = transform.position;
            if (!Instance) Instance = this;
            player = FindAnyObjectByType<PlayerController>();
        }

        void Start()
        {
            EnsurePerchedGryphon();
        }

        public GameObject EnsurePerchedGryphon()
        {
            if (perchedGryphon) return perchedGryphon;
            var existing = transform.Find("Perched Gryphon");
            if (existing) { perchedGryphon = existing.gameObject; return perchedGryphon; }

            Vector3 fwd = transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

            Vector3[] candidates =
            {
                right * 2.8f + fwd * 0.4f,
                -right * 2.8f + fwd * 0.4f,
                right * 2.6f - fwd * 1.4f,
                -right * 2.6f - fwd * 1.4f,
                fwd * 3.0f + right * 1.4f,
                fwd * 3.0f - right * 1.4f,
                -fwd * 2.9f + right * 1.0f,
                right * 3.6f
            };

            Vector3 bestPos = SampleStaticGround(transform.position + candidates[0]);
            Quaternion bestRot = Quaternion.LookRotation((fwd + right * 0.25f).normalized, Vector3.up);
            foreach (var offset in candidates)
            {
                Vector3 p = SampleStaticGround(transform.position + offset);
                if (Mathf.Abs(p.y - transform.position.y) > 2.2f) continue;
                bool blocked = Physics.CheckCapsule(
                    p + Vector3.up * 0.85f,
                    p + Vector3.up * 2.15f,
                    0.68f,
                    ~(1 << 2),
                    QueryTriggerInteraction.Ignore);
                if (!blocked)
                {
                    bestPos = p;
                    Vector3 outDir = (p - transform.position);
                    outDir.y = 0f;
                    Vector3 face = (fwd * 0.8f + outDir.normalized * 0.2f).normalized;
                    bestRot = Quaternion.LookRotation(face.sqrMagnitude > 0.01f ? face : fwd, Vector3.up);
                    break;
                }
            }

            perchedGryphon = GryphonPresentation.SpawnPerchedGryphon(transform, bestPos, bestRot, 1.05f);
            return perchedGryphon;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (ActiveFlightMaster == this) ActiveFlightMaster = null;
            if (InFlight) IsAnyTaxiInFlight = false;
            if (mapBgTex) Destroy(mapBgTex);
            if (borderTex) Destroy(borderTex);
        }

        public static void CloseActiveTaxiWindow()
        {
            if (ActiveFlightMaster != null) ActiveFlightMaster.CloseTaxiWindow();
            if (Instance != null) Instance.CloseTaxiWindow();
        }

        public static FlightMaster GetNearby(Vector3 pos, float radius = 3.5f)
        {
            FlightMaster nearest = null;
            float minDst = radius;
            foreach (var fm in FindObjectsByType<FlightMaster>(FindObjectsSortMode.None))
            {
                float d = Vector3.Distance(pos, fm.transform.position);
                if (d < minDst)
                {
                    minDst = d;
                    nearest = fm;
                }
            }
            return nearest;
        }

        public static void OpenTaxiWindow(FlightMaster fm)
        {
            if (!fm) return;
            ActiveFlightMaster = fm;
            Instance = fm;
            fm.TaxiMapOpen = true;
            AudioEffects.PlayClick();
            if (fm.player == null) fm.player = FindFirstObjectByType<PlayerController>();
            if (fm.player)
            {
                fm.player.ChatOpen = false;
                fm.player.Message = $"[{fm.nodeName}] Select a flight taxi destination.";
            }
        }

        public void CloseTaxiWindow()
        {
            TaxiMapOpen = false;
            if (ActiveFlightMaster == this) ActiveFlightMaster = null;
        }

        public void TakeFlight(FlightPathDestination dest)
        {
            if (!player) player = FindAnyObjectByType<PlayerController>();
            if (!player || InFlight || IsAnyTaxiInFlight) return;

            if (player.Gold < dest.costGold)
            {
                player.Message = "Not enough gold for flight taxi!";
                return;
            }

            if (dest.costGold > 0)
                if (!player.SpendGoldDirect(dest.costGold)) return;

            CloseTaxiWindow();
            StartCoroutine(FlightTaxiRoutine(dest));
        }

        public static Vector3 SampleStaticGround(Vector3 xz)
        {
            float terrainY = WorldRegions.GroundHeight(xz);
            float best = terrainY;
            var hits = Physics.RaycastAll(new Vector3(xz.x, terrainY + 24f, xz.z), Vector3.down, 48f, ~(1 << 2), QueryTriggerInteraction.Ignore);
            foreach (var h in hits)
            {
                if (!h.collider) continue;
                if (h.collider.GetComponentInParent<PlayerController>() ||
                    h.collider.GetComponentInParent<EnemyController>() ||
                    h.collider.GetComponentInParent<TownNPC>() ||
                    h.collider.GetComponentInParent<FlightMaster>())
                    continue;
                if (h.collider is TerrainCollider || h.point.y <= terrainY + 2.5f)
                {
                    if (h.point.y > best) best = h.point.y;
                }
            }
            return new Vector3(xz.x, best, xz.z);
        }

        public static float SampleObstacleCeiling(Vector3 xz)
        {
            float top = WorldRegions.GroundHeight(xz);
            var hits = Physics.RaycastAll(new Vector3(xz.x, 260f, xz.z), Vector3.down, 360f, ~(1 << 2), QueryTriggerInteraction.Ignore);
            foreach (var h in hits)
            {
                if (!h.collider) continue;
                if (h.collider.GetComponentInParent<PlayerController>() ||
                    h.collider.GetComponentInParent<EnemyController>() ||
                    h.collider.GetComponentInParent<TownNPC>() ||
                    h.collider.GetComponentInParent<FlightMaster>())
                    continue;
                if (h.point.y > top) top = h.point.y;
            }
            return top;
        }

        public static Vector3 ResolveLandingPos(FlightPathDestination dest)
        {
            Vector3 anchor = dest.targetPos;
            Vector3 forward = Vector3.forward;
            foreach (var fm in FindObjectsByType<FlightMaster>(FindObjectsSortMode.None))
            {
                if (!fm || string.IsNullOrEmpty(fm.nodeName)) continue;
                if (fm.nodeName.StartsWith(dest.name, StringComparison.OrdinalIgnoreCase) ||
                    dest.name.StartsWith(fm.nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    anchor = fm.transform.position;
                    forward = fm.transform.forward;
                    forward.y = 0f;
                    if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
                    forward.Normalize();
                    break;
                }
            }

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3[] offsets = { forward * 2.6f, -right * 2.6f, right * 2.6f, -forward * 2.6f, Vector3.zero };
            foreach (var off in offsets)
            {
                Vector3 cand = SampleStaticGround(anchor + off);
                if (!Physics.CheckCapsule(cand + Vector3.up * 0.65f, cand + Vector3.up * 1.95f, 0.42f, ~(1 << 2), QueryTriggerInteraction.Ignore))
                    return cand + Vector3.up * 0.15f;
            }
            return SampleStaticGround(anchor) + Vector3.up * 0.25f;
        }

        /// <summary>
        /// Builds a terrain- and obstacle-aware multi-waypoint soaring flight spline that climbs clear
        /// of nearby roofs, maintains >=24m clearance above every mountain/ridge/building along a 24m
        /// corridor, and smoothly descends onto the destination flight pad.
        /// </summary>
        public static List<Vector3> BuildSafeFlightPath(Vector3 startPos, Vector3 endPos, float clearanceMeters = 26f)
        {
            var points = new List<Vector3>();
            Vector3 flatDelta = new Vector3(endPos.x - startPos.x, 0f, endPos.z - startPos.z);
            float horizDist = flatDelta.magnitude;
            if (horizDist < 1f)
            {
                points.Add(startPos);
                points.Add(endPos);
                return points;
            }

            Vector3 flatDir = flatDelta / horizDist;
            Vector3 perp = new Vector3(-flatDir.z, 0f, flatDir.x);

            points.Add(startPos);

            // 1. Steep takeoff climb clear of roofs/trees near departure Flight Master
            float takeoffRun = Mathf.Min(14f, horizDist * 0.16f);
            Vector3 takeoffXZ = startPos + flatDir * takeoffRun;
            float takeoffCeil = Mathf.Max(SampleObstacleCeiling(startPos), SampleObstacleCeiling(takeoffXZ));
            float takeoffY = Mathf.Max(startPos.y + 18f, takeoffCeil + 18f);
            points.Add(new Vector3(takeoffXZ.x, takeoffY, takeoffXZ.z));

            // 2. Cruise corridor sampled every 12m across a +/-12m lateral swath
            float cruiseStart = takeoffRun;
            float approachRun = Mathf.Min(14f, horizDist * 0.16f);
            float cruiseEnd = Mathf.Max(cruiseStart + 1f, horizDist - approachRun);
            int cruiseSteps = Mathf.Max(3, Mathf.CeilToInt((cruiseEnd - cruiseStart) / 12f));
            var cruisePts = new Vector3[cruiseSteps + 1];
            var rawFloor = new float[cruiseSteps + 1];

            for (int i = 0; i <= cruiseSteps; i++)
            {
                float d = Mathf.Lerp(cruiseStart, cruiseEnd, i / (float)cruiseSteps);
                Vector3 center = startPos + flatDir * d;
                float maxY = SampleObstacleCeiling(center);
                maxY = Mathf.Max(maxY, SampleObstacleCeiling(center + perp * 6f));
                maxY = Mathf.Max(maxY, SampleObstacleCeiling(center - perp * 6f));
                maxY = Mathf.Max(maxY, SampleObstacleCeiling(center + perp * 12f));
                maxY = Mathf.Max(maxY, SampleObstacleCeiling(center - perp * 12f));
                rawFloor[i] = maxY + clearanceMeters;
                cruisePts[i] = new Vector3(center.x, rawFloor[i], center.z);
            }

            // Upward-safe smoothing: NEVER dips below rawFloor[i], smoothly arcs over mountain peaks
            for (int pass = 0; pass < 4; pass++)
            {
                float[] nextY = new float[cruisePts.Length];
                nextY[0] = Mathf.Max(rawFloor[0], takeoffY);
                nextY[cruisePts.Length - 1] = rawFloor[cruisePts.Length - 1];
                for (int i = 1; i < cruisePts.Length - 1; i++)
                {
                    float avg = 0.25f * cruisePts[i - 1].y + 0.50f * cruisePts[i].y + 0.25f * cruisePts[i + 1].y;
                    nextY[i] = Mathf.Max(rawFloor[i], avg);
                }
                for (int i = 0; i < cruisePts.Length; i++)
                    cruisePts[i].y = nextY[i];
            }

            // Skip index 0 if nearly coincident with takeoff point
            for (int i = 1; i < cruisePts.Length - 1; i++)
                points.Add(cruisePts[i]);

            // 3. Smooth landing approach point above destination
            Vector3 approachXZ = endPos - flatDir * approachRun;
            float approachCeil = Mathf.Max(SampleObstacleCeiling(endPos), SampleObstacleCeiling(approachXZ));
            float approachY = Mathf.Max(endPos.y + 18f, approachCeil + 18f);
            points.Add(new Vector3(approachXZ.x, approachY, approachXZ.z));
            points.Add(endPos);

            return points;
        }

        public static Vector3 EvaluateSafeSpline(IList<Vector3> pts, float normalizedT)
        {
            int count = pts.Count;
            if (count == 0) return Vector3.zero;
            if (count == 1) return pts[0];
            float scaled = Mathf.Clamp01(normalizedT) * (count - 1);
            int idx = Mathf.Min(count - 2, Mathf.FloorToInt(scaled));
            float u = scaled - idx;

            Vector3 p0 = pts[Mathf.Max(0, idx - 1)];
            Vector3 p1 = pts[idx];
            Vector3 p2 = pts[idx + 1];
            Vector3 p3 = pts[Mathf.Min(count - 1, idx + 2)];

            // Catmull-Rom spline with safety floor clamp so spline overshoot NEVER dips below segment line
            float u2 = u * u;
            float u3 = u2 * u;
            Vector3 cr = 0.5f * (
                (2f * p1) +
                (-p0 + p2) * u +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * u2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * u3);

            float linearY = Mathf.Lerp(p1.y, p2.y, u);
            if (cr.y < linearY) cr.y = linearY;
            return cr;
        }

        IEnumerator FlightTaxiRoutine(FlightPathDestination dest)
        {
            InFlight = true;
            IsAnyTaxiInFlight = true;
            ChatSystem.LogSystem($"Taking flight taxi to [{dest.name}]...");
            AudioEffects.PlayClick();

            bool wasAlreadyMounted = player != null && player.Mounted;
            if (player != null && !wasAlreadyMounted)
                player.MountForFlightTaxi();

            var cc = player ? player.GetComponent<CharacterController>() : null;
            if (cc) cc.enabled = false;

            Vector3 startPos = player ? player.transform.position : transform.position;
            Vector3 endPos = ResolveLandingPos(dest);
            var waypoints = BuildSafeFlightPath(startPos, endPos);

            float totalLen = 0f;
            for (int i = 1; i < waypoints.Count; i++)
                totalLen += Vector3.Distance(waypoints[i - 1], waypoints[i]);
            float duration = Mathf.Clamp(totalLen / 30f, 4.5f, 28f);
            float elapsed = 0f;

            SpellFX.SpawnBurst(startPos + Vector3.up * 0.8f, new Color(0.95f, 0.88f, 0.72f), new Color(0.65f, 0.45f, 0.25f), 14, 1.8f, 4.2f, 0.14f, 0.30f, 0.55f);
            CastBarSystem.StartCast($"Flying to {dest.name}...", duration, null, false, false);

            try
            {
                while (elapsed < duration)
                {
                    if (!player) yield break;

                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    Vector3 cur = EvaluateSafeSpline(waypoints, t);
                    Vector3 ahead = EvaluateSafeSpline(waypoints, Mathf.Min(1f, t + 0.02f));
                    Vector3 dir = ahead - cur;

                    if (dir.sqrMagnitude > 0.0001f)
                    {
                        Vector3 normDir = dir.normalized;
                        // Clamp pitch to +-28 deg so rider & gryphon look heroic and never flip
                        float flatLen = new Vector2(normDir.x, normDir.z).magnitude;
                        float clampedY = Mathf.Clamp(normDir.y, -0.47f, 0.47f);
                        Vector3 lookDir = new Vector3(normDir.x, clampedY * Mathf.Max(0.25f, flatLen), normDir.z).normalized;
                        Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRot, Time.deltaTime * 6.5f);
                    }

                    player.transform.position = cur;
                    player.AnimateFlightTaxi();
                    yield return null;
                }

                if (player)
                {
                    Vector3 flatFwd = endPos - startPos;
                    flatFwd.y = 0f;
                    if (flatFwd.sqrMagnitude > 0.01f)
                        player.transform.rotation = Quaternion.LookRotation(flatFwd.normalized);
                    else
                        player.transform.rotation = Quaternion.Euler(0f, player.transform.eulerAngles.y, 0f);

                    player.transform.position = endPos;
                    SpellFX.SpawnBurst(endPos + Vector3.up * 0.5f, new Color(0.92f, 0.86f, 0.72f), new Color(0.55f, 0.45f, 0.32f), 12, 1.5f, 3.6f, 0.12f, 0.26f, 0.45f);
                }
            }
            finally
            {
                if (cc) cc.enabled = true;
                if (player) player.EndFlightTaxiMount(wasAlreadyMounted);
                InFlight = false;
                IsAnyTaxiInFlight = false;
                CastBarSystem.CancelCast("");
            }

            if (player)
            {
                ChatSystem.LogSystem($"Arrived at [{dest.name}].");
                ZoneBanner.Show(dest.zone, dest.faction + " Territory");
                AudioEffects.PlayZoneDiscover();
            }
        }

        public void DrawTaxiMapGUI(float screenW, float screenH)
        {
            if (!TaxiMapOpen || FrontendMenu.IsOpen) return;

            InitStyles();

            float winW = 490f;
            float winH = Mathf.Min(500f, screenH - 80f);
            float x = screenW / 2f - winW / 2f;
            float y = screenH / 2f - winH / 2f;

            // Background & Border
            GUI.DrawTexture(new Rect(x - 2f, y - 2f, winW + 4f, winH + 4f), borderTex);
            GUI.DrawTexture(new Rect(x, y, winW, winH), mapBgTex);

            // Title
            GUI.Label(new Rect(x + 20f, y + 14f, winW - 80f, 28f), "FRONTIER GRYPHON FLIGHT NETWORK", winTitleStyle);
            GUI.Label(new Rect(x + 20f, y + 42f, winW - 80f, 20f), $"Select a destination across all {WorldFlightNodes.Length} flight hubs:", subStyle);

            if (GUI.Button(new Rect(x + winW - 42f, y + 14f, 26f, 26f), "X", nodeBtnStyle))
            {
                CloseTaxiWindow();
                return;
            }

            float listTop = y + 70f;
            float listHeight = winH - 122f;
            float contentHeight = WorldFlightNodes.Length * 54f + 6f;
            Rect viewRect = new Rect(x + 16f, listTop, winW - 32f, listHeight);
            Rect contentRect = new Rect(0f, 0f, winW - 54f, Mathf.Max(listHeight, contentHeight));

            taxiScrollPos = GUI.BeginScrollView(viewRect, taxiScrollPos, contentRect);
            Vector3 pPos = player ? player.transform.position : transform.position;
            for (int i = 0; i < WorldFlightNodes.Length; i++)
            {
                var node = WorldFlightNodes[i];
                float dist = Vector2.Distance(new Vector2(pPos.x, pPos.z), new Vector2(node.targetPos.x, node.targetPos.z));
                bool isCurrent = dist < 24f;
                int etaSec = Mathf.RoundToInt(Mathf.Clamp((dist + 40f) / 30f, 5f, 28f));

                Rect rowRect = new Rect(4f, i * 54f, contentRect.width - 8f, 48f);
                Color btnBg = isCurrent ? new Color(0.20f, 0.45f, 0.22f) :
                              node.faction == "Alliance" ? new Color(0.18f, 0.26f, 0.42f) :
                              node.faction == "Horde" ? new Color(0.40f, 0.18f, 0.18f) :
                              new Color(0.24f, 0.26f, 0.30f);

                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = btnBg;

                GUI.enabled = !isCurrent && !InFlight && !IsAnyTaxiInFlight;
                string label = isCurrent
                    ? $"[CURRENT HUB] {node.name} ({node.zone})"
                    : $"✈ {node.name}  [{node.faction}]\n   {node.zone}  •  {Mathf.RoundToInt(dist)}m (~{etaSec}s)  •  {node.costGold} Gold";
                if (GUI.Button(rowRect, label, nodeBtnStyle))
                {
                    GUI.enabled = true;
                    GUI.backgroundColor = oldBg;
                    GUI.EndScrollView();
                    TakeFlight(node);
                    return;
                }
                GUI.enabled = true;
                GUI.backgroundColor = oldBg;
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(x + 20f, y + winH - 42f, winW - 40f, 30f), "CLOSE [ESC]", nodeBtnStyle))
            {
                CloseTaxiWindow();
            }
        }

        void InitStyles()
        {
            if (winTitleStyle != null && subStyle != null && nodeBtnStyle != null && mapBgTex != null && borderTex != null) return;

            mapBgTex = new Texture2D(1, 1);
            mapBgTex.SetPixel(0, 0, new Color(0.06f, 0.08f, 0.12f, 0.96f));
            mapBgTex.Apply();

            borderTex = new Texture2D(1, 1);
            borderTex.SetPixel(0, 0, new Color(0.85f, 0.72f, 0.32f, 1f));
            borderTex.Apply();

            winTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            winTitleStyle.normal.textColor = new Color(1.0f, 0.85f, 0.35f);

            subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };
            subStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            nodeBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
        }
    }
}
