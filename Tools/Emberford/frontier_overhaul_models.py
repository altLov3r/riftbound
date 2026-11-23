"""
Frontier World Overhaul Models (Blender 5.2).
Generates:
  Assets/RPG/Resources/Ashenmaw/Ashenmaw_Cavern_Complete.fbx
  Assets/RPG/Resources/Ashenmaw/Ursolak_BearGuardian.fbx
  Assets/RPG/Art/FrontierOverhaul/BreakerWorks_MagmaArch.fbx
  Assets/RPG/Art/FrontierOverhaul/BreakerWorks_PressureTower.fbx
  Assets/RPG/Art/FrontierOverhaul/BreakerWorks_RimBastion.fbx
  Assets/RPG/Art/FrontierOverhaul/Frontier_CoastalLighthouse.fbx
Plus Workbench PNG previews in .code-check/frontier-models/.
"""
from math import cos, pi, sin, sqrt
from pathlib import Path
import random
import sys
import bpy
import bmesh
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
import meshkit

REPO_ROOT = Path(__file__).resolve().parents[2]
ASHEM_DIR = REPO_ROOT / "Assets" / "RPG" / "Resources" / "Ashenmaw"
FRONTIER_ART_DIR = REPO_ROOT / "Assets" / "RPG" / "Art" / "FrontierOverhaul"
PREVIEW_DIR = REPO_ROOT / ".code-check" / "frontier-models"


def reset_with_materials():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    meshkit.MATS.clear()
    meshkit.make_materials()
    # Keep fur/stone matte so Workbench & URP don't look plastic
    for mname in ("Dark Wood", "Oak", "Oak Edge", "Basalt", "Slate", "Pale Stone", "Limestone"):
        m = meshkit.MATS.get(mname)
        if m and m.node_tree:
            bsdf = m.node_tree.nodes.get("Principled BSDF")
            if bsdf:
                bsdf.inputs["Roughness"].default_value = 0.92
                if "Specular IOR Level" in bsdf.inputs:
                    bsdf.inputs["Specular IOR Level"].default_value = 0.18


def assign_box_uvs(obj):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")


def smooth_faces(obj):
    for p in obj.data.polygons:
        p.use_smooth = True


def export_fbx(path: Path, obj):
    path.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        axis_forward="-Z",
        axis_up="Y",
        apply_scale_options="FBX_SCALE_ALL",
        bake_space_transform=True,
        add_leaf_bones=False,
        bake_anim=False,
        mesh_smooth_type="FACE",
        path_mode="STRIP",
    )
    obj.data.calc_loop_triangles()
    print(f"ART {path.name} triangles={len(obj.data.loop_triangles)} dims={tuple(round(v, 2) for v in obj.dimensions)}", flush=True)


def render_preview(name: str, cam_loc, look_at, ortho_scale=18.0):
    PREVIEW_DIR.mkdir(parents=True, exist_ok=True)
    scene = bpy.context.scene
    bpy.ops.object.camera_add(location=cam_loc)
    cam = bpy.context.object
    cam.rotation_euler = (Vector(look_at) - cam.location).to_track_quat("-Z", "Y").to_euler()
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = ortho_scale
    scene.camera = cam
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.show_shadows = True
    scene.display.shading.show_cavity = True
    scene.display.shading.cavity_type = "BOTH"
    scene.display.shading.background_type = "WORLD"
    world = bpy.data.worlds.new("PreviewWorld_" + name)
    world.color = (0.14, 0.17, 0.20)
    scene.world = world
    scene.view_settings.view_transform = "Standard"
    scene.view_settings.exposure = 0.55
    scene.render.resolution_x = 1120
    scene.render.resolution_y = 760
    scene.render.resolution_percentage = 100
    out_png = PREVIEW_DIR / f"{name}.png"
    scene.render.filepath = str(out_png)
    bpy.ops.render.render(write_still=True)
    print(f"PREVIEW {out_png}", flush=True)


# =============================================================================
# 1. ASHENMAW SEAMLESS UNDERGROUND CAVERN (Double-Walled, Zero Spikes!)
# =============================================================================
def build_ashenmaw_cavern_fbx():
    reset_with_materials()
    mat_rock = meshkit.MATS["Basalt"]
    mat_floor = meshkit.MATS["Slate"]
    mat_jade = meshkit.MATS["Jade"]

    # Spine along +Y from y=0.0m (cave mouth flush with stone gate) -> y=74.0m (deep mountain Boss Sanctum)
    # Placed into a steeply rising mountain flank (+19m natural rise over 74m) while descending smoothly
    # to -4.20m (~0.145m per 1m), burying the tunnel & Sanctum dome deeply beneath natural mountain terrain!
    spine = [
        ( 0.0, 0.0,  0.04, 5.2, 6.4),   # Cave mouth archway (flush with stone gate)
        ( 6.0, 0.2, -0.80, 5.0, 6.0),
        (12.0, 0.5, -1.70, 5.0, 5.8),
        (18.0, 0.7, -2.60, 5.2, 5.8),
        (24.0, 0.4, -3.50, 5.8, 6.4),
        (29.0, 0.0, -4.20, 7.6, 7.4),
        (35.0,-0.5, -4.20, 8.8, 8.2),   # Ritual Hearth Chamber
        (41.0,-0.7, -4.20, 8.8, 8.2),
        (47.0,-0.3, -4.20, 6.2, 6.6),   # Sanctum Throat
        (53.0, 0.0, -4.20, 11.8, 9.8),  # Domed Bear Sanctum
        (60.0, 0.0, -4.20, 13.5, 11.2), # Ursolak Boss Arena
        (68.0, 0.0, -4.20, 11.2, 9.4),
        (74.0, 0.0, -4.20, 6.4, 6.4),
    ]

    bm = bmesh.new()
    # Explicitly construct INNER ring (walkable interior) AND OUTER ring (1.1m solid rock casing)
    # -> Zero Solidify spikes, closed manifold shell, valid inward + outward MeshCollider!
    inner_rings = []
    outer_rings = []
    segs = 20  # 0..5 flat floor left->right, 6..19 arched ceiling right->left

    for si, (y, xc, fz, hw, ch) in enumerate(spine):
        ring_in = []
        ring_out = []
        for k in range(segs):
            if k <= 5:
                u = (k / 5.0) * 2.0 - 1.0  # -1..+1 left to right
                ix = xc + u * hw * 0.88
                iz = fz
                ox = xc + u * (hw * 0.96 + 1.2)
                oz = fz - 1.2
            else:
                arc_t = (k - 5) / (segs - 5)  # 0..1 around arch from +1 to -1
                ang = arc_t * pi
                noise = (sin(y * 0.36 + ang * 3.0) * 0.42 + cos(y * 0.78 - ang * 4.0) * 0.22)
                if si <= 1:
                    noise *= 0.25
                ix = xc + cos(ang) * (hw + noise * 0.35) * 0.92
                iz = fz + 0.20 + sin(ang) * ch + (0.32 * noise if sin(ang) > 0.15 else 0.0)
                ox = xc + cos(ang) * (hw + 1.45 + noise * 0.45)
                oz = fz + 0.20 + sin(ang) * (ch + 1.45) + (0.40 * noise if sin(ang) > 0.15 else 0.0)
            ring_in.append(bm.verts.new((ix, y, iz)))
            ring_out.append(bm.verts.new((ox, y, oz)))
        inner_rings.append(ring_in)
        outer_rings.append(ring_out)

    # Connect inner rings (normals point INWARD into cave, floor is Slate mat 1)
    for si in range(len(spine) - 1):
        r0, r1 = inner_rings[si], inner_rings[si + 1]
        o0, o1 = outer_rings[si], outer_rings[si + 1]
        for k in range(segs):
            kn = (k + 1) % segs
            f_in = bm.faces.new((r0[k], r1[k], r1[kn], r0[kn]))
            f_in.smooth = (k >= 5)
            f_in.material_index = 1 if k < 5 else 0

            f_out = bm.faces.new((o0[k], o0[kn], o1[kn], o1[k]))
            f_out.smooth = True
            f_out.material_index = 0

    # Cap front rim (connect inner_rings[0] to outer_rings[0]) & back wall cleanly
    f0_in, f0_out = inner_rings[0], outer_rings[0]
    b_in, b_out = inner_rings[-1], outer_rings[-1]
    back_c_in = bm.verts.new((0.0, 75.5, -1.2))
    back_c_out = bm.verts.new((0.0, 76.8, -1.2))
    for k in range(segs):
        kn = (k + 1) % segs
        bm.faces.new((f0_out[k], f0_in[k], f0_in[kn], f0_out[kn])).material_index = 0
        f_bi = bm.faces.new((b_in[k], back_c_in, b_in[kn]))
        f_bi.smooth = True
        f_bi.material_index = 0
        f_bo = bm.faces.new((b_out[k], b_out[kn], back_c_out))
        f_bo.smooth = True
        f_bo.material_index = 0

    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)

    shell_mesh = bpy.data.meshes.new("Ashenmaw_CavernShell")
    bm.to_mesh(shell_mesh)
    bm.free()

    shell_obj = bpy.data.objects.new("Ashenmaw_Cavern_Complete", shell_mesh)
    bpy.context.collection.objects.link(shell_obj)
    shell_obj.data.materials.append(mat_rock)
    shell_obj.data.materials.append(mat_floor)

    # Sculpted exterior cliffside boulders framing the cave mouth & terrain-hole brow (leaving mouth & courtyard clean)
    extras = [shell_obj]
    rng = random.Random(42)
    for i in range(16):
        ang = 0.06 + (i / 15.0) * (pi - 0.12)
        bx = cos(ang) * 8.4 + rng.uniform(-0.3, 0.3)
        bz = sin(ang) * 7.8 + 0.4 + rng.uniform(-0.1, 0.4)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=rng.uniform(2.1, 2.8), location=(bx, rng.uniform(-0.3, 4.8), bz))
        b = bpy.context.active_object
        b.scale = (1.15, 1.85, 0.90)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        for v in b.data.vertices:
            v.co += v.normal * (sin(v.co.x * 2.1 + v.co.y * 1.9) * 0.25)
        b.data.materials.append(mat_rock)
        smooth_faces(b)
        extras.append(b)

    # Cliff-brow terrain-hole cover boulders along y = 6.5..17.5 (flanks + overhead brow cap)
    for side in (-1, 1):
        for by in (6.5, 12.0, 17.5):
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=2.6, location=(side * 8.6, by, 2.5 - by * 0.08))
            b = bpy.context.active_object
            b.scale = (1.15, 2.2, 1.1)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            for v in b.data.vertices:
                v.co += v.normal * (sin(v.co.x * 2.1 + v.co.y * 1.9) * 0.25)
            b.data.materials.append(mat_rock)
            smooth_faces(b)
            extras.append(b)

    for (bx, by, bz) in [(0.0, 7.5, 7.8), (-3.6, 11.5, 7.6), (3.6, 11.5, 7.6), (0.0, 16.0, 7.0)]:
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=2.5, location=(bx, by, bz))
        b = bpy.context.active_object
        b.scale = (1.45, 2.0, 0.85)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        for v in b.data.vertices:
            v.co += v.normal * (sin(v.co.x * 2.1 + v.co.y * 1.9) * 0.25)
        b.data.materials.append(mat_rock)
        smooth_faces(b)
        extras.append(b)

    # Open-air gently sloped stone entrance courtyard apron (y=-8.0 @ z=-2.60 up to y=0.0 @ z=+0.04, ZERO overhead roof)
    cbm = bmesh.new()

    def add_sloped_prism(x0, x1, y0, y1, z0_top, z1_top, z_bot, mat_idx):
        v = [
            cbm.verts.new((x0, y0, z_bot)),
            cbm.verts.new((x1, y0, z_bot)),
            cbm.verts.new((x1, y1, z_bot)),
            cbm.verts.new((x0, y1, z_bot)),
            cbm.verts.new((x0, y0, z0_top)),
            cbm.verts.new((x1, y0, z0_top)),
            cbm.verts.new((x1, y1, z1_top)),
            cbm.verts.new((x0, y1, z1_top)),
        ]
        for q in [(0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)]:
            f = cbm.faces.new([v[i] for i in q])
            f.material_index = mat_idx

    add_sloped_prism(-4.7, 4.7, -8.0, 0.0, -2.60, 0.04, -6.8, 1)
    for s in (-1, 1):
        kx0, kx1 = (4.35, 5.05) if s > 0 else (-5.05, -4.35)
        add_sloped_prism(kx0, kx1, -7.8, -0.5, -2.05, 0.45, -6.8, 0)
    bmesh.ops.recalc_face_normals(cbm, faces=cbm.faces)
    courtyard_mesh = bpy.data.meshes.new("Ashenmaw_CourtyardRamp")
    cbm.to_mesh(courtyard_mesh)
    cbm.free()
    courtyard_obj = bpy.data.objects.new("Ashenmaw_CourtyardRamp", courtyard_mesh)
    bpy.context.collection.objects.link(courtyard_obj)
    courtyard_obj.data.materials.append(mat_rock)
    courtyard_obj.data.materials.append(mat_floor)
    extras.append(courtyard_obj)

    # Carved gate + interior architecture
    kit = meshkit.Build()

    for s in (-1, 1):
        kit.box((s * 4.6, -0.3, 2.6), (1.3, 1.5, 5.3), "Pale Stone", 0.08)
        kit.box((s * 4.6, -1.1, 1.6), (0.6, 0.6, 0.7), "Amber", 0.05)
    kit.box((0.0, -0.3, 5.55), (10.6, 1.6, 0.85), "Basalt", 0.08)

    # Interior Ritual Hearth pillars (floor=-4.2, ceiling=+4.0)
    for px, py in ((-5.6, 35.0), (5.6, 35.0), (-5.6, 41.0), (5.6, 41.0)):
        kit.box((px, py, -1.3), (1.2, 1.2, 5.8), "Basalt", 0.07)
        kit.box((px, py, -2.6), (1.45, 1.45, 0.42), "Jade", 0.04)

    # Ursolak Spirit Altar at back of Boss Sanctum (floor=-4.2, chest sits cleanly in front at y=66.5)
    kit.box((0.0, 69.5, -3.8), (8.4, 4.2, 0.85), "Slate", 0.08)
    kit.box((0.0, 70.6, -1.2), (3.2, 1.3, 4.8), "Basalt", 0.09)
    kit.box((0.0, 69.9, -0.9), (1.1, 0.35, 2.5), "Jade", 0.04)
    kit_obj = kit.finish("Ashenmaw_InteriorKit")
    extras.append(kit_obj)

    # Hanging stalactites strictly inside interior ceiling & jade crystals inside floor
    for idx, (sx, sy, sz) in enumerate([
        (-2.8, 22.0, 1.0), (2.8, 25.0, 1.0), (-4.0, 34.0, 2.5), (4.0, 36.0, 2.5),
        (-5.2, 56.0, 4.2), (5.2, 58.0, 4.2), (-4.2, 65.0, 3.6), (4.2, 65.0, 3.6),
    ]):
        bpy.ops.mesh.primitive_cone_add(vertices=7, radius1=0.55, radius2=0.05, depth=2.0, location=(sx, sy, sz), rotation=(pi, 0, idx * 0.7))
        st = bpy.context.active_object
        st.data.materials.append(mat_rock)
        smooth_faces(st)
        extras.append(st)

    for idx, (cx, cy, cz) in enumerate([
        (-3.8, 12.0, -2.0), (3.8, 16.0, -2.6), (-7.2, 35.0, -4.1), (7.2, 38.0, -4.1),
        (-9.6, 56.0, -4.1), (9.6, 56.0, -4.1), (-8.2, 65.0, -4.1), (8.2, 65.0, -4.1),
    ]):
        bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.44, radius2=0.04, depth=2.4, location=(cx, cy, cz + 1.0), rotation=(0.15, -0.15 if cx < 0 else 0.15, idx))
        cr = bpy.context.active_object
        cr.data.materials.append(mat_jade)
        extras.append(cr)

    bpy.ops.object.select_all(action="DESELECT")
    for o in extras:
        o.select_set(True)
    bpy.context.view_layer.objects.active = shell_obj
    bpy.ops.object.join()
    shell_obj.name = "Ashenmaw_Cavern_Complete"
    assign_box_uvs(shell_obj)

    export_fbx(ASHEM_DIR / "Ashenmaw_Cavern_Complete.fbx", shell_obj)
    render_preview("cavern_preview", (15.0, -24.0, 12.0), (0.0, 10.0, 0.0), ortho_scale=30.0)


# =============================================================================
# 2. URSOLAK, ANCIENT BEAR GUARDIAN V3 (Heroic Sculpted Armor & Fur!)
# =============================================================================
def build_bear_guardian_fbx():
    reset_with_materials()
    # Combine meshkit chamfered armor/ruff planes + smooth sculpted bear anatomy facing +Y
    kit = meshkit.Build()

    # Heavy Bear Body & Chest Core + Armor Pauldrons & Bracers
    kit.oval((0.0, 0.35, 1.50), (0.94, 1.12, 0.84), "Dark Wood", sides=14)
    kit.oval((0.0, 0.58, 2.04), (0.82, 0.78, 0.58), "Oak", sides=12)       # Grizzly shoulder hump
    kit.oval((0.0, -0.82, 1.36), (0.80, 0.86, 0.74), "Dark Wood", sides=12)
    kit.oval((0.0, -1.62, 1.28), (0.20, 0.24, 0.20), "Dark Wood", sides=8)  # Short tail

    # Thick Shaggy Ruff Collar + Iron Spirit Harness
    kit.oval((0.0, 1.18, 1.76), (0.64, 0.60, 0.62), "Oak", sides=12)
    for side in (-1, 1):
        # Runed Black Iron War-Pauldrons on shoulders
        kit.box((side * 0.92, 0.55, 1.82), (0.42, 0.92, 0.62), "Black Iron", 0.08, (0, side * 0.32, 0))
        kit.box((side * 1.12, 0.55, 1.88), (0.14, 0.45, 0.26), "Jade", 0.03, (0, side * 0.32, 0))
    # Iron collar ring + chest medallion
    kit.box((0.0, 1.15, 1.75), (1.32, 0.24, 1.30), "Black Iron", 0.10)
    kit.box((0.0, 1.48, 1.36), (0.36, 0.18, 0.38), "Jade", 0.04)

    # Fierce Sculpted Bear Head, Heavy Brow Ridge, Wide Snout, Open Roaring Jaw, Ears & Fangs
    kit.oval((0.0, 1.66, 1.88), (0.58, 0.62, 0.54), "Dark Wood", sides=12)
    kit.box((0.0, 2.08, 2.04), (0.84, 0.36, 0.24), "Oak", 0.06, (0.22, 0, 0))      # Heavy brow ridge
    kit.oval((0.0, 2.26, 1.78), (0.32, 0.44, 0.24), "Oak", sides=10)                 # Upper muzzle
    kit.oval((0.0, 2.18, 1.44), (0.26, 0.36, 0.14), "Dark Wood", sides=10)           # Open lower jaw!
    kit.oval((0.0, 2.66, 1.82), (0.13, 0.10, 0.10), "Black Iron", sides=8)           # Nose
    for s in (-1, 1):
        kit.oval((s * 0.42, 1.56, 2.34), (0.16, 0.13, 0.20), "Dark Wood", sides=8)   # Ears
        kit.oval((s * 0.27, 2.12, 1.98), (0.08, 0.07, 0.07), "Jade", sides=8)        # Glowing jade eyes
        kit.beam((s * 0.18, 2.46, 1.68), (s * 0.18, 2.44, 1.48), 0.055, "Bone", 0.01, sides=6) # Upper fangs
        kit.beam((s * 0.15, 2.40, 1.46), (s * 0.15, 2.42, 1.64), 0.048, "Bone", 0.01, sides=6) # Lower fangs

    # Jade Spirit Runes along back spine
    for ry, rz in ((0.62, 2.58), (0.05, 2.35), (-0.48, 2.10)):
        kit.box((0.0, ry, rz), (0.24, 0.40, 0.10), "Jade", 0.03)

    # 4 Muscular Legs + Forearm Bracers + Paws + Bone Claws
    for lx, ly in ((-0.66, 0.74), (0.66, 0.74), (-0.60, -0.86), (0.60, -0.86)):
        kit.oval((lx, ly, 0.94), (0.34, 0.38, 0.56), "Dark Wood", sides=10)
        kit.oval((lx, ly + 0.06, 0.44), (0.27, 0.30, 0.46), "Dark Wood", sides=10)
        kit.box((lx, ly + 0.06, 0.48), (0.58, 0.62, 0.24), "Black Iron", 0.06)       # Iron leg cuffs!
        kit.oval((lx, ly + 0.24, 0.14), (0.32, 0.40, 0.16), "Oak", sides=10)
        for ci in (-1, 0, 1):
            kit.beam((lx + ci * 0.10, ly + 0.52, 0.12), (lx + ci * 0.10, ly + 0.76, 0.04), 0.05, "Bone", 0.008, sides=6)

    bear = kit.finish("Ursolak_BearGuardian")
    smooth_faces(bear)
    export_fbx(ASHEM_DIR / "Ursolak_BearGuardian.fbx", bear)
    # Render HEROIC FRONT 3/4 VIEW (+Y is forward, so camera at +Y,+X looks straight at his face & shoulder!)
    render_preview("bear_preview", (3.6, 4.2, 2.3), (0.0, 0.5, 1.3), ortho_scale=4.2)


# =============================================================================
# 3. BREAKER WORKS ARENA KIT & COASTAL LIGHTHOUSE
# =============================================================================
def build_frontier_structures_fbx():
    # Build all 3 Breaker Works pieces in one scene so we can render all 3 together, then reset for Lighthouse
    reset_with_materials()

    # 3A. BreakerWorks_MagmaArch.fbx
    b = meshkit.Build()
    for side in (-1, 1):
        px = side * 4.4
        b.box((px, 0.0, 1.4), (2.8, 3.0, 10.6), "Basalt", 0.14)
        b.box((px, 0.0, 7.0), (2.4, 2.6, 1.2), "Black Iron", 0.08)
        b.box((px, -1.4, 3.2), (1.3, 0.4, 5.2), "Ember Edge", 0.04)
        b.box((px, 0.0, 8.2), (1.5, 1.5, 1.4), "Amber", 0.06)
    b.box((0.0, 0.0, 6.2), (12.2, 2.5, 1.5), "Black Iron", 0.10)
    b.box((0.0, 0.0, 7.3), (5.4, 1.4, 0.9), "Ember Edge", 0.06)
    arch = b.finish("BreakerWorks_MagmaArch")
    export_fbx(FRONTIER_ART_DIR / "BreakerWorks_MagmaArch.fbx", arch)

    # 3B. BreakerWorks_PressureTower.fbx
    b = meshkit.Build()
    b.box((0.0, 0.0, 0.2), (7.6, 7.6, 8.4), "Basalt", 0.16)
    b.box((0.0, 0.0, 6.8), (5.2, 5.2, 5.4), "Black Iron", 0.12)
    b.box((0.0, 0.0, 10.4), (6.4, 6.4, 2.0), "Red Iron", 0.10)
    b.box((0.0, 0.0, 12.4), (2.8, 2.8, 2.2), "Amber", 0.08)
    for ang in (0.78, 2.35, 3.92, 5.49):
        cx, cy = cos(ang) * 3.5, sin(ang) * 3.5
        b.beam((cx, cy, 0.0), (cx * 0.72, cy * 0.72, 9.8), 0.55, "Steel", 0.38, sides=6)
    tower = b.finish("BreakerWorks_PressureTower")
    export_fbx(FRONTIER_ART_DIR / "BreakerWorks_PressureTower.fbx", tower)

    # 3C. BreakerWorks_RimBastion.fbx
    b = meshkit.Build()
    b.box((0.0, 0.0, -0.4), (7.4, 2.2, 6.8), "Basalt", 0.12)
    b.box((0.0, 0.0, 3.3), (7.8, 2.5, 0.7), "Slate", 0.08)
    b.box((-2.8, 0.0, 4.4), (1.3, 1.3, 1.8), "Black Iron", 0.06)
    b.box((-2.8, 0.0, 5.6), (0.9, 0.9, 0.9), "Amber", 0.05)
    b.box((2.8, 0.0, 4.4), (1.3, 1.3, 1.8), "Black Iron", 0.06)
    b.box((2.8, 0.0, 5.6), (0.9, 0.9, 0.9), "Amber", 0.05)
    bastion = b.finish("BreakerWorks_RimBastion")
    export_fbx(FRONTIER_ART_DIR / "BreakerWorks_RimBastion.fbx", bastion)

    tower.location = (-8.5, 0.0, 0.0)
    arch.location = (6.5, 0.0, 0.0)
    bastion.location = (0.0, -9.0, 0.0)
    render_preview("breaker_preview", (20.0, -24.0, 14.0), (0.0, -2.0, 4.5), ortho_scale=26.0)

    # 3D. Frontier_CoastalLighthouse.fbx
    reset_with_materials()
    b = meshkit.Build()
    b.box((0.0, 0.0, 0.2), (6.4, 6.4, 5.6), "Pale Stone", 0.14)
    b.beam((0.0, 0.0, 3.0), (0.0, 0.0, 7.2), 2.35, "Pale Stone", 2.05, sides=8)
    b.beam((0.0, 0.0, 7.2), (0.0, 0.0, 10.8), 2.05, "Red Iron", 1.85, sides=8)
    b.beam((0.0, 0.0, 10.8), (0.0, 0.0, 13.6), 1.85, "Pale Stone", 1.75, sides=8)
    b.beam((0.0, 0.0, 13.6), (0.0, 0.0, 14.2), 2.55, "Black Iron", 2.65, sides=8)
    b.beam((0.0, 0.0, 14.2), (0.0, 0.0, 16.4), 1.35, "Amber", 1.35, sides=8)
    b.beam((0.0, 0.0, 16.4), (0.0, 0.0, 18.6), 1.85, "Black Iron", 0.15, sides=8)
    lh = b.finish("Frontier_CoastalLighthouse")
    export_fbx(FRONTIER_ART_DIR / "Frontier_CoastalLighthouse.fbx", lh)
    render_preview("lighthouse_preview", (16.0, -18.0, 11.0), (0.0, 0.0, 8.5), ortho_scale=22.0)


def main():
    build_ashenmaw_cavern_fbx()
    build_bear_guardian_fbx()
    build_frontier_structures_fbx()


if __name__ == "__main__":
    main()
