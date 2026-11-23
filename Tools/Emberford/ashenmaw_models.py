import math
import os
import sys
import bpy
import bmesh
from mathutils import Vector, Matrix

OUT_DIR = sys.argv[sys.argv.index("--") + 1]
os.makedirs(OUT_DIR, exist_ok=True)


def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def make_mat(name, rgba, metallic=0.0, roughness=0.75, emission=None):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = rgba
        bsdf.inputs["Metallic"].default_value = metallic
        bsdf.inputs["Roughness"].default_value = roughness
        if emission is not None and "Emission Color" in bsdf.inputs:
            bsdf.inputs["Emission Color"].default_value = emission
            bsdf.inputs["Emission Strength"].default_value = 2.4
    return mat


def add_uv_box(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    obj.select_set(False)


def smooth_mesh(obj):
    for p in obj.data.polygons:
        p.use_smooth = True


def build_bear_guardian(fbx_path):
    clear_scene()

    mat_fur_dark = make_mat("BearFurDark", (0.16, 0.12, 0.09, 1.0), roughness=0.88)
    mat_fur_light = make_mat("BearFurMane", (0.26, 0.20, 0.14, 1.0), roughness=0.85)
    mat_claw = make_mat("Bone", (0.82, 0.78, 0.68, 1.0), roughness=0.45)
    mat_iron = make_mat("Black Iron", (0.18, 0.19, 0.21, 1.0), metallic=0.85, roughness=0.35)
    mat_jade = make_mat("Jade", (0.16, 0.94, 0.64, 1.0), roughness=0.2, emission=(0.16, 0.94, 0.64, 1.0))

    parts = []

    def add_organic_part(name, loc, scale, rot=(0, 0, 0), segs=16, rings=12, mat=mat_fur_dark, subsurf=1):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=segs, ring_count=rings, radius=1.0, location=loc, rotation=rot)
        o = bpy.context.active_object
        o.name = name
        o.scale = scale
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        # Subtle organic displacement for natural fur/muscle volume
        for v in o.data.vertices:
            n = math.sin(v.co.x * 5.2 + v.co.y * 4.1) * math.cos(v.co.z * 4.7) * 0.035
            v.co += v.normal * n
        o.data.materials.append(mat)
        if subsurf > 0:
            mod = o.modifiers.new("Subsurf", "SUBSURF")
            mod.levels = subsurf
            mod.render_levels = subsurf
            bpy.ops.object.modifier_apply(modifier=mod.name)
        smooth_mesh(o)
        parts.append(o)
        return o

    # Blender Z-up, +Y forward (FBX exporter maps to Unity Y-up, +Z forward)
    # Massive bear ribcage, hindquarter, and iconic grizzly shoulder hump
    add_organic_part("Bear_Chest", (0.0, 0.35, 1.42), (0.88, 1.05, 0.78), mat=mat_fur_dark)
    add_organic_part("Bear_Hump", (0.0, 0.55, 1.92), (0.76, 0.72, 0.52), mat=mat_fur_light)
    add_organic_part("Bear_Hindquarters", (0.0, -0.78, 1.32), (0.78, 0.82, 0.70), mat=mat_fur_dark)
    add_organic_part("Bear_Tail", (0.0, -1.52, 1.25), (0.20, 0.22, 0.20), mat=mat_fur_dark, subsurf=0)

    # Thick neck, sculpted head, muzzle, jaw, nose, ears
    add_organic_part("Bear_Neck", (0.0, 1.12, 1.68), (0.58, 0.52, 0.55), mat=mat_fur_light)
    add_organic_part("Bear_Skull", (0.0, 1.62, 1.78), (0.52, 0.56, 0.48), mat=mat_fur_dark)
    add_organic_part("Bear_Snout", (0.0, 2.15, 1.64), (0.28, 0.38, 0.24), mat=mat_fur_light)
    add_organic_part("Bear_Jaw", (0.0, 2.04, 1.46), (0.24, 0.32, 0.14), mat=mat_fur_dark, subsurf=0)
    add_organic_part("Bear_Nose", (0.0, 2.50, 1.68), (0.12, 0.09, 0.09), mat=mat_iron, subsurf=0)
    add_organic_part("Bear_EarL", (-0.36, 1.52, 2.22), (0.14, 0.12, 0.18), mat=mat_fur_dark, subsurf=0)
    add_organic_part("Bear_EarR", (0.36, 1.52, 2.22), (0.14, 0.12, 0.18), mat=mat_fur_dark, subsurf=0)

    # Glowing ancient spirit eyes & jade back spirit-runes + iron spirit collar
    add_organic_part("Bear_EyeL", (-0.24, 2.02, 1.88), (0.07, 0.06, 0.06), mat=mat_jade, subsurf=0)
    add_organic_part("Bear_EyeR", (0.24, 2.02, 1.88), (0.07, 0.06, 0.06), mat=mat_jade, subsurf=0)
    add_organic_part("Bear_SpiritRune1", (0.0, 0.55, 2.40), (0.20, 0.38, 0.08), mat=mat_jade, subsurf=0)
    add_organic_part("Bear_SpiritRune2", (0.0, -0.15, 2.18), (0.16, 0.34, 0.07), mat=mat_jade, subsurf=0)

    bpy.ops.mesh.primitive_torus_add(major_radius=0.62, minor_radius=0.10, location=(0.0, 1.10, 1.68), rotation=(math.radians(72), 0, 0))
    collar = bpy.context.active_object
    collar.name = "Bear_SpiritCollar"
    collar.data.materials.append(mat_iron)
    smooth_mesh(collar)
    parts.append(collar)

    # Four muscular legs (upper limb + forearm + paw + 3 curved claws each)
    legs = [
        ("FL", -0.62, 0.72),
        ("FR", 0.62, 0.72),
        ("BL", -0.58, -0.82),
        ("BR", 0.58, -0.82),
    ]
    for tag, lx, ly in legs:
        add_organic_part(f"LegUpper_{tag}", (lx, ly, 0.92), (0.30, 0.34, 0.52), mat=mat_fur_dark, subsurf=0)
        add_organic_part(f"LegLower_{tag}", (lx, ly + 0.06, 0.42), (0.24, 0.26, 0.42), mat=mat_fur_dark, subsurf=0)
        add_organic_part(f"Paw_{tag}", (lx, ly + 0.22, 0.14), (0.28, 0.36, 0.14), mat=mat_fur_light, subsurf=0)
        for ci in (-1, 0, 1):
            bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=0.045, radius2=0.006, depth=0.22,
                                            location=(lx + ci * 0.09, ly + 0.54, 0.10),
                                            rotation=(math.radians(82), 0, 0))
            claw = bpy.context.active_object
            claw.name = f"Claw_{tag}_{ci}"
            claw.data.materials.append(mat_claw)
            smooth_mesh(claw)
            parts.append(claw)

    bpy.ops.object.select_all(action="DESELECT")
    for o in parts:
        o.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    bear = bpy.context.active_object
    bear.name = "Ursolak_BearGuardian"
    add_uv_box(bear)
    bear.select_set(True)
    bpy.context.view_layer.objects.active = bear

    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        mesh_smooth_type="FACE",
        bake_space_transform=True,
    )
    print("EXPORTED BEAR:", fbx_path)


def build_ashenmaw_cavern(fbx_path):
    """
    Organic sculpted subterranean cavern mesh (ZERO boxes!):
    - Local origin (0, 0, 0) is the cave mouth at terrain level.
    - Descends smoothly underground along +Y (which maps to Unity +Z) from Z=0 down to Z=-7.5m,
      curves gently through a stalactite gallery, widens into a ritual hearth chamber,
      and opens into a 24m-wide domed Boss Sanctum at Y=58..74m.
    - Normals face INWARD for the walkable cavern interior + organic rocky exterior mouth lip.
    """
    clear_scene()
    mat_rock = make_mat("Basalt", (0.16, 0.17, 0.19, 1.0), roughness=0.92)
    mat_floor = make_mat("Slate", (0.20, 0.21, 0.23, 1.0), roughness=0.88)
    mat_jade = make_mat("Jade", (0.16, 0.90, 0.62, 1.0), roughness=0.25, emission=(0.16, 0.90, 0.62, 1.0))

    bm = bmesh.new()

    # Cross-section profile along the cave spine (from y=0 at terrain hole to y=74m deep underground)
    # Each tuple: (y_dist, x_center, floor_z, half_width, ceil_height)
    spine = [
        (0.0,  0.0,  0.2,  5.2, 5.4),
        (5.0,  0.4, -1.2,  5.0, 5.4),
        (11.0, 1.2, -3.2,  4.8, 5.5),
        (17.0, 1.8, -5.4,  5.2, 5.8),
        (23.0, 1.0, -7.2,  5.8, 6.2),
        (30.0, -0.5, -7.5, 8.2, 7.6),   # Ritual Hearth Chamber
        (37.0, -1.2, -7.5, 8.6, 8.0),
        (44.0, -0.6, -7.5, 6.0, 6.4),   # Narrow Sanctum Throat
        (52.0,  0.0, -7.5, 11.5, 9.8),  # Bear Guardian Sanctum
        (60.0,  0.0, -7.5, 13.0, 11.0),
        (68.0,  0.0, -7.5, 11.0, 9.4),
        (74.0,  0.0, -7.5, 5.5, 5.8),
    ]

    ring_verts = []
    segs = 20  # 20-vertex cross-section around the tunnel (flat walkable floor + organic domed ceiling)

    for si, (y, xc, fz, hw, ch) in enumerate(spine):
        ring = []
        for k in range(segs):
            # Bottom 5 segments form a flat, walkable stone floor; upper 15 form an organic vaulted arch
            t = k / segs
            ang = t * math.pi * 2.0
            # Map angle so bottom is flat
            cx = math.cos(ang)
            sz = math.sin(ang)
            if sz < -0.35:
                # Flat walkable stone floor with subtle micro-unevenness (< 6cm)
                fx = xc + cx * hw * 0.92
                fz_pt = fz + math.sin(y * 0.25 + fx * 0.4) * 0.04
            else:
                # Organic arched rock wall & ceiling with multi-frequency rock noise
                norm_h = (sz + 0.35) / 1.35  # 0 at floor edge, 1 at crown
                noise = (math.sin(y * 0.42 + ang * 3.0) * 0.55 +
                         math.cos(y * 0.85 - ang * 5.0) * 0.32)
                if si == 0:
                    noise *= 0.35
                fx = xc + cx * (hw + noise * 0.85)
                fz_pt = fz + norm_h * ch + noise * 0.65
            v = bm.verts.new((fx, y, fz_pt))
            ring.append(v)
        ring_verts.append(ring)

    # Connect rings with inward-facing faces
    for si in range(len(ring_verts) - 1):
        r0 = ring_verts[si]
        r1 = ring_verts[si + 1]
        for k in range(segs):
            kn = (k + 1) % segs
            # Winding so normal points INWARD into the cave
            f = bm.faces.new((r0[k], r1[k], r1[kn], r0[kn]))
            f.smooth = True

    # Cap the far back wall of the Boss Sanctum cleanly
    back_ring = ring_verts[-1]
    back_center = bm.verts.new((0.0, 76.2, -7.5 + 2.8))
    for k in range(segs):
        kn = (k + 1) % segs
        f = bm.faces.new((back_ring[k], back_center, back_ring[kn]))
        f.smooth = True

    mesh = bpy.data.meshes.new("Ashenmaw_CavernShell")
    bm.to_mesh(mesh)
    bm.free()

    cave_obj = bpy.data.objects.new("Ashenmaw_CavernShell", mesh)
    bpy.context.collection.objects.link(cave_obj)
    cave_obj.data.materials.append(mat_rock)

    # Add sculpted rocky entrance arch boulders framing the terrain hole & organic stalactites inside
    extras = [cave_obj]
    for i in range(14):
        ang = (i / 13.0) * math.pi
        bx = math.cos(ang) * 5.8
        bz = math.sin(ang) * 5.4 + 0.3
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1.9 + (i % 3) * 0.45, location=(bx, -0.5 + (i % 2) * 0.8, bz))
        b = bpy.context.active_object
        b.scale = (1.15, 1.35, 0.95)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        for v in b.data.vertices:
            v.co += v.normal * (math.sin(v.co.x * 2.4 + v.co.y * 2.1) * 0.28)
        b.data.materials.append(mat_rock)
        smooth_mesh(b)
        extras.append(b)

    # Hanging organic stalactites & glowing jade crystal spires inside the cavern
    stalactite_coords = [
        (-2.2, 16.0, -0.2), (2.4, 21.0, -1.5), (-3.8, 32.0, -0.1), (4.2, 35.0, -0.2),
        (-5.5, 55.0, 1.8), (5.8, 57.0, 1.9), (-4.2, 64.0, 1.5), (4.5, 65.0, 1.4),
    ]
    for idx, (sx, sy, sz) in enumerate(stalactite_coords):
        bpy.ops.mesh.primitive_cone_add(vertices=7, radius1=0.65, radius2=0.06, depth=2.8, location=(sx, sy, sz), rotation=(math.pi, 0, idx * 0.7))
        st = bpy.context.active_object
        st.data.materials.append(mat_rock)
        smooth_mesh(st)
        extras.append(st)

    crystal_coords = [
        (-4.0, 14.0, -4.2), (4.2, 24.0, -7.2), (-7.0, 34.0, -7.3), (7.2, 36.0, -7.3),
        (-9.5, 56.0, -7.3), (9.5, 56.0, -7.3), (-8.0, 66.0, -7.3), (8.0, 66.0, -7.3),
        (-3.2, 70.0, -7.3), (3.2, 70.0, -7.3),
    ]
    for idx, (cx, cy, cz) in enumerate(crystal_coords):
        bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.45, radius2=0.05, depth=2.4, location=(cx, cy, cz + 1.0), rotation=(0.18, -0.15 if cx < 0 else 0.15, idx))
        cr = bpy.context.active_object
        cr.data.materials.append(mat_jade)
        extras.append(cr)

    bpy.ops.object.select_all(action="DESELECT")
    for o in extras:
        o.select_set(True)
    bpy.context.view_layer.objects.active = cave_obj
    bpy.ops.object.join()
    add_uv_box(cave_obj)
    cave_obj.select_set(True)
    bpy.context.view_layer.objects.active = cave_obj

    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        mesh_smooth_type="FACE",
        bake_space_transform=True,
    )
    print("EXPORTED CAVERN:", fbx_path)


build_bear_guardian(os.path.join(OUT_DIR, "Ursolak_BearGuardian.fbx"))
build_ashenmaw_cavern(os.path.join(OUT_DIR, "Ashenmaw_CavernShell.fbx"))
