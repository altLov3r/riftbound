using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    // Original griffin silhouette in mount-root metres (+Z forward). The legacy
    // rig still drives the rider's saddle. This geometry is presentation only.
    internal sealed class GryphonPresentation
    {
        const float SaddleHeight = 1.55f;
        const int Tawny = 0, Umber = 1, Ivory = 2, Gold = 3, Dark = 4, Teal = 5;
        static readonly Color[] Palette =
        {
            new Color(.65f, .40f, .21f), new Color(.30f, .17f, .105f),
            new Color(.89f, .80f, .65f), new Color(.84f, .54f, .18f),
            new Color(.13f, .10f, .09f), new Color(.14f, .32f, .28f)
        };

        static Material[] materials;
        static Mesh bodyMesh, leftWingMesh, rightWingMesh, forelegMesh, hindlegMesh;
        readonly Transform leftWing, rightWing;
        readonly Transform[] legs;
        float phase, flightBlend, pace;

        GryphonPresentation(Transform root)
        {
            Part(root, "Feathered body", bodyMesh, Vector3.zero);
            leftWing = Part(root, "Left wing", leftWingMesh, new Vector3(-.35f, 1.41f, .32f));
            rightWing = Part(root, "Right wing", rightWingMesh, new Vector3(.35f, 1.41f, .32f));
            legs = new[]
            {
                Part(root, "Left talon", forelegMesh, new Vector3(-.31f, .97f, .67f)),
                Part(root, "Right talon", forelegMesh, new Vector3(.31f, .97f, .67f)),
                Part(root, "Left hind paw", hindlegMesh, new Vector3(-.32f, .94f, -.71f)),
                Part(root, "Right hind paw", hindlegMesh, new Vector3(.32f, .94f, -.71f))
            };
            Animate(0f, false, 0f);
        }

        internal static GryphonPresentation TryAttach(Transform mount, Transform saddle)
        {
            if (!saddle || (mount.name != "Mount gryphon" && mount.name != "Mount gryphon(Clone)")) return null;
            var sourceRenderers = mount.GetComponentsInChildren<Renderer>(true);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) return null;
            if (!bodyMesh || materials == null || !materials[0]) BuildMeshes(shader);

            Vector3 seat = mount.InverseTransformPoint(saddle.position);
            if (seat.y < .65f || seat.y > 2.5f) return null;

            var root = new GameObject("Emberford gryphon visual").transform;
            root.gameObject.layer = mount.gameObject.layer;
            root.SetParent(mount, false);
            float scale = seat.y / SaddleHeight;
            root.localScale = Vector3.one * scale;
            root.localPosition = new Vector3(seat.x, 0f, seat.z);
            var presentation = new GryphonPresentation(root);
            foreach (var renderer in sourceRenderers) renderer.enabled = false;
            return presentation;
        }

        internal void Animate(float speed, bool airborne, float delta)
        {
            flightBlend = Mathf.MoveTowards(flightBlend, airborne ? 1f : 0f, delta * 3.5f);
            pace = Mathf.MoveTowards(pace, Mathf.Clamp01(speed / 6f), delta * 5f);
            phase = Mathf.Repeat(phase + delta * Mathf.Lerp(9f, 13f, flightBlend), Mathf.PI * 2f);
            float wingBeat = Mathf.Sin(phase) * 32f;
            float wingYaw = Mathf.Lerp(78f, 5f, flightBlend);
            float wingRoll = Mathf.Lerp(22f, 10f + wingBeat, flightBlend);
            leftWing.localRotation = Quaternion.Euler(0f, -wingYaw, -wingRoll);
            rightWing.localRotation = Quaternion.Euler(0f, wingYaw, wingRoll);
            for (int i = 0; i < legs.Length; i++)
            {
                float swing = Mathf.Sin(phase + ((i == 0 || i == 3) ? 0f : Mathf.PI)) * (19f * pace);
                legs[i].localRotation = Quaternion.Euler(Mathf.Lerp(swing, i < 2 ? 32f : -22f, flightBlend), 0f, 0f);
            }
        }

        static Transform Part(Transform parent, string name, Mesh mesh, Vector3 position)
        {
            var go = new GameObject(name);
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = materials;
            return go.transform;
        }

        static void BuildMeshes(Shader shader)
        {
            materials = new Material[Palette.Length];
            for (int i = 0; i < Palette.Length; i++)
            {
                var mat = new Material(shader) { name = "Gryphon palette " + i, color = Palette[i] };
                mat.SetFloat("_Metallic", i == Gold ? .12f : 0f);
                mat.SetFloat("_Smoothness", i == Dark ? .27f : .12f);
                materials[i] = mat;
            }

            var body = new Form();
            body.Ellipsoid(new Vector3(0, 1.10f, -.22f), new Vector3(.47f, .40f, .99f), Tawny);
            body.Ellipsoid(new Vector3(0, 1.21f, .58f), new Vector3(.43f, .46f, .59f), Ivory);
            body.Ellipsoid(new Vector3(0, 1.53f, .79f), new Vector3(.29f, .54f, .33f), Ivory);
            body.Ellipsoid(new Vector3(0, 1.88f, 1.02f), new Vector3(.31f, .30f, .40f), Ivory);
            body.Taper(new Vector3(0, 1.86f, 1.27f), new Vector3(0, 1.82f, 1.59f), .19f, .075f, Gold);
            body.Taper(new Vector3(0, 1.84f, 1.52f), new Vector3(0, 1.69f, 1.60f), .087f, .012f, Gold);
            body.Ellipsoid(new Vector3(0, 1.75f, 1.39f), new Vector3(.145f, .045f, .18f), Umber);
            foreach (int side in new[] { -1, 1 })
            {
                body.Ellipsoid(new Vector3(side * .277f, 1.94f, 1.13f), new Vector3(.070f, .075f, .079f), Gold);
                body.Ellipsoid(new Vector3(side * .322f, 1.96f, 1.151f), new Vector3(.029f, .040f, .046f), Dark);
                body.Taper(new Vector3(side * .19f, 2.03f, .89f), new Vector3(side * .26f, 2.29f, .63f), .115f, .012f, Ivory);
                for (int feather = 0; feather < 5; feather++)
                {
                    Vector3 start = new Vector3(side * (.29f - feather * .017f), 1.67f - feather * .105f, .74f);
                    body.Taper(start, start + new Vector3(side * .12f, -.19f, -.28f), .109f, .015f, feather % 2 == 0 ? Ivory : Tawny);
                }
                body.Taper(new Vector3(side * .32f, 1.44f, .11f), new Vector3(side * .42f, .94f, .17f), .05f, .038f, Dark);
                body.Ellipsoid(new Vector3(side * .43f, 1.06f, .18f), new Vector3(.058f, .088f, .045f), Gold);
            }
            body.Ellipsoid(new Vector3(0, 1.46f, -.05f), new Vector3(.39f, .075f, .43f), Teal);
            body.Ellipsoid(new Vector3(0, 1.51f, -.10f), new Vector3(.31f, .07f, .29f), Dark);
            body.Ellipsoid(new Vector3(0, 1.58f, -.33f), new Vector3(.27f, .07f, .07f), Dark);
            body.Taper(new Vector3(0, 1.27f, -1.00f), new Vector3(.01f, 1.08f, -1.56f), .12f, .079f, Tawny);
            body.Taper(new Vector3(.01f, 1.08f, -1.56f), new Vector3(.13f, 1.18f, -1.93f), .084f, .053f, Tawny);
            body.Ellipsoid(new Vector3(.16f, 1.23f, -1.99f), new Vector3(.13f, .12f, .22f), Umber);
            bodyMesh = body.Finish("Emberford gryphon body");
            leftWingMesh = BuildWing(-1);
            rightWingMesh = BuildWing(1);
            forelegMesh = BuildLeg(true);
            hindlegMesh = BuildLeg(false);
        }

        static Mesh BuildWing(int side)
        {
            var wing = new Form();
            wing.FromToEllipsoid(new Vector3(side * .10f, 0, 0), new Vector3(side * .91f, .08f, -.20f), .12f, .24f, Umber);
            wing.FromToEllipsoid(new Vector3(side * .56f, .04f, -.13f), new Vector3(side * 1.34f, .09f, -.46f), .092f, .19f, Tawny);
            for (int i = 0; i < 11; i++)
            {
                float t = i / 10f;
                Vector3 root = new Vector3(side * (.15f + t * 1.14f), .03f + t * .07f, -.12f - t * .31f);
                Vector3 tip = new Vector3(side * (.30f + t * 1.89f), -.08f + t * .22f, -.83f - t * .48f);
                wing.FromToEllipsoid(root, tip, .043f, .115f - t * .024f, i % 3 == 0 ? Tawny : Umber);
                wing.FromToEllipsoid(root + Vector3.up * .055f, Vector3.Lerp(root, tip, .51f) + Vector3.up * .04f,
                    .037f, .092f, i % 2 == 0 ? Ivory : Tawny);
                wing.FromToEllipsoid(Vector3.Lerp(root, tip, .77f), tip, .029f, .071f, Dark);
            }
            return wing.Finish(side == -1 ? "Emberford left flight feathers" : "Emberford right flight feathers");
        }

        static Mesh BuildLeg(bool front)
        {
            var leg = new Form();
            int color = front ? Ivory : Tawny;
            leg.Ellipsoid(new Vector3(0, -.20f, -.02f), new Vector3(.16f, .34f, .19f), color);
            leg.Taper(new Vector3(0, -.35f, -.02f), new Vector3(0, -.80f, front ? .08f : -.06f),
                .115f, .071f, front ? Gold : color);
            leg.Ellipsoid(new Vector3(0, -.86f, .11f), new Vector3(.127f, .085f, .21f), front ? Gold : color);
            for (int i = -1; i <= 1; i++)
            {
                if (front)
                {
                    leg.Taper(new Vector3(i * .072f, -.86f, .18f), new Vector3(i * .104f, -.87f, .34f), .048f, .018f, Gold);
                    leg.Taper(new Vector3(i * .104f, -.87f, .31f), new Vector3(i * .111f, -.929f, .40f), .034f, .006f, Dark);
                }
                else leg.Ellipsoid(new Vector3(i * .064f, -.86f, .25f), new Vector3(.044f, .059f, .086f), Tawny);
            }
            return leg.Finish(front ? "Emberford griffin foretalons" : "Emberford griffin hind paws");
        }

        sealed class Form
        {
            readonly List<Vector3> vertices = new List<Vector3>();
            readonly List<int>[] faces = { new List<int>(), new List<int>(), new List<int>(),
                new List<int>(), new List<int>(), new List<int>() };
            const int Sides = 10, Bands = 6;

            internal void Ellipsoid(Vector3 center, Vector3 radius, int material)
                => Ellipsoid(center, radius, Quaternion.identity, material);

            internal void FromToEllipsoid(Vector3 from, Vector3 to, float thickness, float width, int material)
            {
                Vector3 axis = to - from;
                Quaternion rotate = Quaternion.LookRotation(axis, Vector3.up);
                Ellipsoid((from + to) * .5f, new Vector3(width, thickness, axis.magnitude * .58f), rotate, material);
            }

            void Ellipsoid(Vector3 center, Vector3 radius, Quaternion rotate, int material)
            {
                int first = vertices.Count;
                for (int ring = 0; ring <= Bands; ring++)
                {
                    float latitude = Mathf.PI * (ring / (float)Bands - .5f);
                    float width = Mathf.Max(.001f, Mathf.Cos(latitude));
                    Ring(center, rotate, radius.x * width, radius.z * width, radius.y * Mathf.Sin(latitude));
                }
                Shell(first, Bands, material);
            }

            internal void Taper(Vector3 from, Vector3 to, float baseRadius, float tipRadius, int material)
            {
                Vector3 direction = to - from;
                if (direction.sqrMagnitude < .0001f) return;
                Quaternion rotate = Quaternion.FromToRotation(Vector3.up, direction);
                int first = vertices.Count;
                Ring(from, rotate, baseRadius, baseRadius, 0f);
                Ring(to, rotate, tipRadius, tipRadius, 0f);
                Shell(first, 1, material);
            }

            void Ring(Vector3 center, Quaternion rotate, float xRadius, float zRadius, float height)
            {
                for (int i = 0; i < Sides; i++)
                {
                    float angle = Mathf.PI * 2f * i / Sides;
                    vertices.Add(center + rotate * new Vector3(Mathf.Cos(angle) * xRadius, height, Mathf.Sin(angle) * zRadius));
                }
            }

            void Shell(int first, int rings, int material)
            {
                List<int> index = faces[material];
                for (int ring = 0; ring < rings; ring++)
                {
                    for (int i = 0; i < Sides; i++)
                    {
                        int a = first + ring * Sides + i;
                        int b = first + ring * Sides + (i + 1) % Sides;
                        index.Add(a); index.Add(a + Sides); index.Add(b);
                        index.Add(b); index.Add(a + Sides); index.Add(b + Sides);
                    }
                }
                int top = first + rings * Sides;
                for (int i = 1; i < Sides - 1; i++)
                {
                    index.Add(first); index.Add(first + i); index.Add(first + i + 1);
                    index.Add(top); index.Add(top + i + 1); index.Add(top + i);
                }
            }

            internal Mesh Finish(string name)
            {
                var mesh = new Mesh { name = name, subMeshCount = faces.Length };
                mesh.SetVertices(vertices);
                for (int i = 0; i < faces.Length; i++) mesh.SetTriangles(faces[i], i);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
