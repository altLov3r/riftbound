using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Riftbound
{
    public partial class MinimapRadar
    {
        const float NavigationTop = 52f, NavigationWidth = 224f, NavigationHeight = 266f;
        const float NavigationRadius = 92f;
        internal const float NavigationTrackerTop = NavigationTop + NavigationHeight + 12f;

        static readonly Color FrameInk = new(.08f, .13f, .16f, .96f);
        static readonly Color MapWater = new(.08f, .14f, .17f);
        static readonly Color FrameLine = new(.56f, .64f, .60f, .65f);
        GUIStyle zoneTitle, actionLabel;
        Texture2D navigationArrow;
        Vector2 navigationPress;
        int navigationButton = -1;
        bool navigationDragged;
        float dungeonRange = 34f;

        bool NavigationVisible => IsVisible && !PrototypeHUD.MenuOpen &&
            !player.AnyBlockingWindowOpen() && (!orbitCam || !orbitCam.Paused);

        static Rect NavigationBounds(float width) =>
            new(width - NavigationWidth - 16, NavigationTop, NavigationWidth, NavigationHeight);

        internal static bool NavigationTakesMouse(PlayerController source, Vector2 screenPoint)
        {
            var radar = Instance;
            if (!radar || radar.player != source || !radar.NavigationVisible) return false;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            if (scale <= 0f) return false;
            var gui = new Vector2(screenPoint.x, Screen.height - screenPoint.y) / scale;
            var panel = NavigationBounds(Screen.width / scale);
            var mouse = Mouse.current;
            bool holdingMap = mouse != null && (radar.navigationButton == 0 && mouse.leftButton.isPressed ||
                radar.navigationButton == 1 && mouse.rightButton.isPressed);
            return holdingMap || panel.Contains(gui) ||
                radar.filtersOpen && FilterBounds(panel).Contains(gui);
        }

        internal void DrawNavigationHud(float width)
        {
            if (!NavigationVisible)
            {
                navigationButton = -1;
                filtersOpen = false;
                return;
            }

            PrepareNavigationStyle();
            Rect panel = NavigationBounds(width);
            Vector2 center = new(panel.center.x, panel.y + 119);
            Vector3 position = player.transform.position;
            int floor = MapFloor(position);
            float range = floor == 0 ? Mathf.Clamp(radarRange, 22, 180) : dungeonRange;
            float yaw = rotateWithCamera && orbitCam ? orbitCam.transform.eulerAngles.y : 0;
            float pixelsPerMeter = NavigationRadius / range;
            string zone = ZoneManager.GetZoneName(position);

            Fill(new Rect(panel.x + 3, panel.y + 4, panel.width, panel.height), new Color(0, 0, 0, .37f));
            Fill(panel, FrameInk);
            Outline(panel, FrameLine);
            Fill(new Rect(panel.x + 1, panel.y + 1, panel.width - 2, 23), Color.Lerp(FrameInk, ZoneColor(zone), .23f));
            Label(new Rect(panel.x + 10, panel.y + 2, panel.width - 20, 20), zone, zoneTitle, Paper);
            Dot(center, NavigationRadius * 2 + 6, FrameLine);
            Dot(center, NavigationRadius * 2 + 3, MapWater);

            if (Event.current.type == EventType.Repaint)
            {
                UpdateTerrainTexture(position, range, floor);
                var matrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(-yaw, center);
                GUI.DrawTexture(Centered(center, NavigationRadius * 2), terrainTex);
                GUI.matrix = matrix;
            }
            // The relief is cached between samples. Use the same origin for its marks and hit tests.
            Vector3 origin = terrainTex && lastFloor == floor && lastRange == range ? terrainOrigin : position;
            foreach (var marker in mapPoints)
            {
                if (MapFloor(marker.position) != floor) continue;
                Vector2 offset = Project(marker.position - origin, yaw) * pixelsPerMeter;
                if (offset.sqrMagnitude > (NavigationRadius - 12) * (NavigationRadius - 12)) continue;
                Dot(center + offset, marker.size + 2, Shadow);
                Dot(center + offset, marker.size, marker.color);
            }

            string hovered = DrawNavigationPins(center, origin, floor, yaw, pixelsPerMeter);
            Vector2 playerPoint = center + Project(position - origin, yaw) * pixelsPerMeter;
            string tracking = DrawNavigationTarget(center, playerPoint, position, origin, floor, yaw, pixelsPerMeter);
            DrawNavigationArrow(playerPoint, player.transform.eulerAngles.y - yaw, 20, PlayerColor);
            if (showCompass)
            {
                DrawCardinal(center, NavigationRadius, "N", -yaw);
                DrawCardinal(center, NavigationRadius, "E", 90 - yaw);
                DrawCardinal(center, NavigationRadius, "S", 180 - yaw);
                DrawCardinal(center, NavigationRadius, "W", 270 - yaw);
            }

            float footer = panel.y + 215;
            float captionWidth = panel.width - (customWaypoint.HasValue ? 42 : 20);
            Label(new Rect(panel.x + 10, footer, captionWidth, 19),
                hovered ?? tracking ?? (floor == 0 ? Coordinates(position) : "Underground"), actionLabel, Paper);
            if (customWaypoint.HasValue && GUI.Button(new Rect(panel.xMax - 29, footer, 20, 17),
                new GUIContent("×", "Clear waypoint"), button)) ClearWaypoint();
            Fill(new Rect(panel.x + 9, footer + 19, panel.width - 18, 1), FrameLine);
            float controls = footer + 23;
            if (GUI.Button(new Rect(panel.x + 9, controls, 24, 20), "−", button)) ZoomNavigation(floor, false);
            Label(new Rect(panel.x + 35, controls, 46, 20), $"{range:0} m", centered, Muted);
            if (GUI.Button(new Rect(panel.x + 82, controls, 24, 20), "+", button)) ZoomNavigation(floor, true);
            if (GUI.Button(new Rect(panel.x + 112, controls, 45, 20), "Map", button)) player.MapOpen = true;
            if (GUI.Button(new Rect(panel.x + 163, controls, 52, 20), "Filters", button)) filtersOpen = !filtersOpen;

            if (filtersOpen)
            {
                var popup = FilterBounds(panel);
                if (Event.current.type == EventType.MouseDown && !panel.Contains(Event.current.mousePosition) &&
                    !popup.Contains(Event.current.mousePosition)) filtersOpen = false;
                else
                {
                    bool otherChange = GUI.changed;
                    GUI.changed = false;
                    DrawFilters(popup);
                    if (GUI.changed) RefreshMapPoints();
                    GUI.changed |= otherChange;
                }
            }

            HandleNavigationGesture(center, origin, position, floor, yaw, range, hovered != null);
        }

        static string Coordinates(Vector3 position) =>
            $"{Mathf.Abs(position.x):0} {(position.x < 0 ? "W" : "E")}  /  {Mathf.Abs(position.z):0} {(position.z < 0 ? "S" : "N")}";

        string DrawNavigationTarget(Vector2 center, Vector2 playerPoint, Vector3 position, Vector3 origin,
            int floor, float yaw, float pixelsPerMeter)
        {
            if ((!trackQuests && !customWaypoint.HasValue) ||
                !TryGetNavigationTarget(out var destination, out var title)) return null;

            Vector3 next = LocalPosition(destination, floor);
            Vector2 offset = Project(next - origin, yaw) * pixelsPerMeter;
            Vector2 direction = center + offset - playerPoint;
            Color ink = customWaypoint.HasValue ? PlayerColor : PinColor;
            if (offset.sqrMagnitude > (NavigationRadius - 17) * (NavigationRadius - 17))
                DrawNavigationArrow(center + offset.normalized * (NavigationRadius - 17),
                    Vector2.SignedAngle(Vector2.up, -direction), 14, ink);
            else if (direction.sqrMagnitude > 49f)
            {
                Outline(Centered(center + offset, 22), ink);
            }

            float distance = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(next.x, next.z));
            return $"{Mathf.RoundToInt(distance)} m  ·  {title}";
        }

        string DrawNavigationPins(Vector2 center, Vector3 origin, int floor, float yaw, float pixelsPerMeter)
        {
            if (!trackQuests) return null;
            drawnPins.Clear();
            string hovered = null;
            int preferred = PreferredNavigationPin();
            if (preferred >= 0) Draw(questPins[preferred]);
            for (int i = 0; i < questPins.Count; i++)
                if (i != preferred) Draw(questPins[i]);
            return hovered;

            void Draw(QuestPin pin)
            {
                if (floor != 0 && MapFloor(pin.position) != floor) return;
                Vector2 offset = Project(LocalPosition(pin.position, floor) - origin, yaw) * pixelsPerMeter;
                if (offset.sqrMagnitude > (NavigationRadius - 15) * (NavigationRadius - 15)) return;
                Vector2 point = center + offset;
                foreach (var drawn in drawnPins)
                    if ((point - drawn).sqrMagnitude < 324) return;
                drawnPins.Add(point);
                Rect hit = Centered(point, 19f);
                if (hit.Contains(Event.current.mousePosition)) hovered = pin.caption;
                if (DrawQuestMapPin(hit, pin)) SelectQuestPin(pin);
            }
        }

        int PreferredNavigationPin()
        {
            for (int i = 0; i < questPins.Count; i++)
                if (questPins[i].id == selectedQuest) return i;
            if (!TryGetNavigationTarget(out var target, out _)) return -1;
            for (int i = 0; i < questPins.Count; i++)
                if ((target - questPins[i].position).sqrMagnitude < 4f) return i;
            return -1;
        }

        void HandleNavigationGesture(Vector2 center, Vector3 origin, Vector3 position,
            int floor, float yaw, float range, bool overQuest)
        {
            var input = Event.current;
            Vector2 offset = input.mousePosition - center;
            bool inside = offset.sqrMagnitude < NavigationRadius * NavigationRadius;
            if (input.type == EventType.ScrollWheel && inside && input.delta.y != 0)
            {
                ZoomNavigation(floor, input.delta.y < 0);
                input.Use();
            }
            else if (input.type == EventType.MouseDown && inside && input.button <= 1 && (!overQuest || input.button == 1))
            {
                navigationButton = input.button;
                navigationPress = input.mousePosition;
                navigationDragged = false;
                input.Use();
            }
            else if (input.type == EventType.MouseDrag && input.button == navigationButton)
            {
                navigationDragged |= (navigationPress - input.mousePosition).sqrMagnitude > 25f;
                input.Use();
            }
            else if (input.rawType == EventType.MouseUp && input.button == navigationButton)
            {
                navigationButton = -1;
                if (!inside || navigationDragged || (navigationPress - input.mousePosition).sqrMagnitude > 25f) return;
                if (input.button == 1) ClearWaypoint();
                else if (!overQuest)
                {
                    Vector2 world = Rotate(offset, yaw) * (range / NavigationRadius);
                    Vector3 target = new(origin.x + world.x, position.y, origin.z - world.y);
                    if (TrySnapNavigationPin(ref target, floor, Terrain.activeTerrains, dungeonFloors))
                        SetWaypoint(target);
                }
                if (input.type != EventType.Used) input.Use();
            }
        }

        internal static bool TrySnapNavigationPin(ref Vector3 target, int floor, Terrain[] terrains, IReadOnlyList<Bounds> rooms)
        {
            if (floor == 0)
            {
                var terrain = WorldRegions.TerrainAt(target, terrains);
                if (!terrain) return false;
                target.y = terrain.SampleHeight(target) + terrain.transform.position.y;
                return true;
            }

            foreach (var room in rooms)
            {
                if (target.x < room.min.x || target.x > room.max.x || target.z < room.min.z || target.z > room.max.z) continue;
                target.y = room.max.y;
                return true;
            }
            return false;
        }

        void ZoomNavigation(int floor, bool closer)
        {
            float multiplier = closer ? .78f : 1f / .78f;
            if (floor == 0) radarRange = Mathf.Clamp(radarRange * multiplier, 22f, 180f);
            else dungeonRange = Mathf.Clamp(dungeonRange * multiplier, 14f, 46f);
        }

        void PrepareNavigationStyle()
        {
            PrepareMapStyle();
            if (navigationArrow) return;
            zoneTitle = new GUIStyle(header) { fontSize = 14, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
            actionLabel = new GUIStyle(caption) { alignment = TextAnchor.MiddleLeft };
            const int size = 32;
            navigationArrow = new Texture2D(size, size, TextureFormat.RGBA32, false)
                { name = "Minimap direction arrow", wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float lateral = Mathf.Abs(x + .5f - size * .5f);
                bool inside = lateral < (29f - y) * .47f && y > 9f - lateral * .38f;
                pixels[y * size + x] = new Color32(255, 255, 255, inside ? (byte)255 : (byte)0);
            }
            navigationArrow.SetPixels32(pixels);
            navigationArrow.Apply(false, true);
        }

        void DrawNavigationArrow(Vector2 point, float heading, float size, Color ink)
        {
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(heading, point);
            Tint(Centered(point, size + 4), navigationArrow, Shadow);
            Tint(Centered(point, size), navigationArrow, ink);
            GUI.matrix = matrix;
        }
    }
}
