"""Westbank's short boardwalk from the rebuilt bank to the retained ferry ramp."""
import sys
from pathlib import Path

import bpy
from mathutils import Matrix

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "EmberwatchCivicSquare"))
from build_square import beam, box, export, image, material, mesh, parts

atlas = Path(sys.argv[sys.argv.index("--") + 2]).resolve()
image.image = bpy.data.images.load(str(atlas))
image.image.pack()
material.name = "Westbank cedar and willow"

# The FBX pivot is (-265.8, 5.70, 40.60) in Unity. Local forward is north.
width = 2.05
slope_end = 2.87
end = 5.93
stations = [(0, .30), (slope_end, 0), (end, 0)]
vertices = [(x, z, h) for x, h in stations for z in (-width, 0)]
mesh("Continuous walkable plank deck", vertices, [(0, 2, 3, 1), (2, 4, 5, 3)], "beam")

def height(x):
    return .30 * max(0, 1 - x / slope_end)

for x in (.34, .79, 1.24, 1.69, 2.14, 2.59, 3.07, 3.52, 3.97, 4.42, 4.87, 5.32, 5.77):
    box("Weathered plank join", (x, -width * .5, height(x) + .001), (.021, width - .07, .006), "ash", bevel=0)

for north in (-width + .07, -.07):
    for (a, h), (b, j) in zip(stations, stations[1:]):
        beam("Lower deck stringer", (a, north, h-.13), (b, north, j-.13), .20)
    for x in (1.10, 3.04, 5.40):
        bottom = -3.1 if x > 4 else -2.25 if x > 2 else -1.35
        box("Bank deck supporting pile", (x, north, (bottom+height(x)-.03)*.5), (.21, .21, height(x)-.03-bottom), "beam")
        beam("Joining knee brace", (x, north, height(x)-.75), (x-.50, north, height(x)-.17), .15)

def rail(a, b):
    for x, y in (a, b):
        h = height(x)
        box("Cedar rail post", (x, y, h+.47), (.16, .16, .96), "beam")
    for above, thickness in ((.86,.13), (.43,.10)):
        beam("Rounded handrail", (a[0],a[1],height(a[0])+above), (b[0],b[1],height(b[0])+above),
             thickness)

rail((.19,-width+.09), (2.87,-width+.09))
rail((2.87,-width+.09), (end-.10,-width+.09))
rail((end-.10,-width+.09), (end-.10,-.15))
rail((.22,-.08), (2.71,-.08))

# Unity's FBX import mirrors the Blender X axis. Bake the inverse into the
# mesh and preserve outward face winding, so the placed deck runs east.
bpy.context.view_layer.update()
mirror_x = Matrix.Diagonal((-1.0, 1.0, 1.0, 1.0))
for part in parts:
    part.data.transform(mirror_x @ part.matrix_world)
    part.matrix_world = Matrix.Identity(4)
    part.data.flip_normals()
export("BankConnector", "Westbank")
