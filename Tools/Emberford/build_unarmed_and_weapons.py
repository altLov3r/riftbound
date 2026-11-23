"""
Export 12 weaponless skinned combatant variants + 3 new bespoke GearWeapons FBXs & 256px icons.
Outputs:
  Assets/RPG/Resources/Actors/Unarmed/<Name>_Unarmed.fbx  (12 unarmed combatants, exact LOD0..2 & rig)
  Assets/RPG/Art/GearWeapons/sunstone-reliquary-staff.fbx
  Assets/RPG/Art/GearWeapons/ironmaw-cleaver.fbx
  Assets/RPG/Art/GearWeapons/mage-arcane-staff.fbx
  Assets/RPG/Resources/Gear/Icons/sunstone-reliquary-staff.png
  Assets/RPG/Resources/Gear/Icons/ironmaw-cleaver.png
  Assets/RPG/Resources/Gear/Icons/mage-arcane-staff.png
  .code-check/polish10/unarmed_and_weapons_preview.png
"""
from math import cos, pi, sin
from pathlib import Path
import sys

import bpy
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
from characters import character, rig, animate
from generate import ROSTER, export_fbx, slug, triangle_count
from meshkit import Build, make_materials
from pinewatch import export as export_static_fbx
import gear_art

REPO_ROOT = Path(__file__).resolve().parents[2]
UNARMED_DIR = REPO_ROOT / "Assets" / "RPG" / "Art" / "UnarmedActors"
WEAPONS_DIR = REPO_ROOT / "Assets" / "RPG" / "Art" / "GearWeapons"
ICONS_DIR = REPO_ROOT / "Assets" / "RPG" / "Resources" / "Gear" / "Icons"
PREVIEW_DIR = REPO_ROOT / ".code-check" / "polish10"


def build_unarmed_combatants():
    UNARMED_DIR.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    make_materials()
    sc = bpy.context.scene
    library = bpy.data.scenes.new("UnarmedLibrary")

    exported = 0
    for name, spec in ROSTER.items():
        kind, hostile, role = spec
        if role == "npc":
            continue
        col = bpy.data.collections.new(name + "_Unarmed")
        sc.collection.children.link(col)
        arm = rig(kind, name, col)
        meshes = []
        counts = []
        for level in range(3):
            b = Build(level)
            character(b, kind, hostile, include_weapons=False)
            # Preserve exact _LOD0..2 mesh naming for compatibility with existing checks
            obj = b.finish(slug(name) + "_LOD" + str(level), col)
            obj.parent = arm
            obj.modifiers.new("Skeleton", type="ARMATURE").object = arm
            meshes.append(obj)
            counts.append(triangle_count(obj))
        if not (counts[0] >= counts[1] >= counts[2] > 0):
            raise RuntimeError(f"Non-monotonic LODs for {name}: {counts}")
        animate(arm)
        out_path = UNARMED_DIR / (slug(name) + "_Unarmed.fbx")
        export_fbx(out_path, [arm] + meshes)
        sc.collection.children.unlink(col)
        library.collection.children.link(col)
        exported += 1
        print(f"UNARMED {out_path.name} tris={counts}", flush=True)

    print(f"Exported {exported} unarmed combatant FBXs.", flush=True)


def build_staff(b, shaft_mat, metal_mat, gem_mat, sun_halo=False):
    # Grip centered cleanly around z=0.0 so hand.R holds mid-shaft naturally (-0.70 .. +0.98)
    b.beam((0, 0, -0.70), (0, 0, 0.72), 0.036, shaft_mat, 0.032, 8)
    for z in (-0.12, -0.06, 0.0, 0.06, 0.12):
        b.beam((0, 0, z), (0, 0, z + 0.025), 0.043, "Leather", 0.041, 8)
    # Bottom ferrule & upper collar
    b.oval((0, 0, -0.71), (0.048, 0.048, 0.055), metal_mat, sides=8)
    b.box((0, 0, 0.68), (0.13, 0.13, 0.09), metal_mat, 0.025)
    if sun_halo:
        # Solar crescent & sun-rays cradling glowing Amber reliquary crystal
        gear_art.loop(b, 0, 0, 0.92, 0.19, 0.19, 0.026, metal_mat, segments=16)
        for i in range(8):
            ang = i * pi / 4.0
            r0, r1 = 0.19, 0.28
            b.beam((r0 * cos(ang), 0, 0.92 + r0 * sin(ang)),
                   (r1 * cos(ang), 0, 0.92 + r1 * sin(ang)), 0.022, metal_mat, 0.008, 5)
        b.oval((0, 0, 0.92), (0.105, 0.075, 0.135), gem_mat, sides=10)
    else:
        # Curved twin prongs cradling glowing Jade arcane crystal
        for s in (-1, 1):
            b.beam((s * 0.05, 0, 0.72), (s * 0.14, 0, 0.88), 0.028, metal_mat, 0.020, 6)
            b.beam((s * 0.14, 0, 0.88), (s * 0.07, 0, 1.03), 0.020, metal_mat, 0.011, 6)
        b.oval((0, 0, 0.90), (0.088, 0.070, 0.145), gem_mat, sides=8)


def build_ironmaw_cleaver(b):
    # Heavy industrial pig-iron quarry cleaver-axe (grip z=-0.16..+0.12, head up to +1.22)
    gear_art.grip(b, 1.16, "Red Iron", radius=0.050)
    b.box((0, 0, 1.08), (0.21, 0.20, 0.24), "Red Iron", 0.035)
    # Broad single-sided notched cleaver blade + spiked counterweight
    blade = [
        (0.05, 1.18), (0.38, 1.28), (0.50, 1.12), (0.46, 0.80),
        (0.32, 0.68), (0.08, 0.84)
    ]
    b.plate(blade, 0, 0.105, "Black Iron")
    edge = [
        (0.38, 1.28), (0.50, 1.12), (0.46, 0.80), (0.32, 0.68),
        (0.39, 0.76), (0.42, 1.09)
    ]
    b.plate(edge, 0, 0.116, "Steel")
    # Back quarry pick spike
    b.beam((-0.08, 0, 1.08), (-0.30, 0, 1.14), 0.052, "Black Iron", 0.012, 6)
    for y in (-0.058, 0.058):
        gear_art.inset(b, 0.24, y, 1.02, 0.055, "Amber")


def build_new_weapons_and_icons():
    WEAPONS_DIR.mkdir(parents=True, exist_ok=True)
    ICONS_DIR.mkdir(parents=True, exist_ok=True)
    PREVIEW_DIR.mkdir(parents=True, exist_ok=True)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    make_materials()
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.render.film_transparent = True
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.resolution_x = scene.render.resolution_y = 256
    scene.render.resolution_percentage = 100
    shading = scene.display.shading
    shading.light = "STUDIO"
    shading.color_type = "MATERIAL"
    shading.show_shadows = True
    shading.show_cavity = True
    shading.cavity_type = "BOTH"
    scene.view_settings.view_transform = "Standard"
    scene.view_settings.exposure = 0.72
    bpy.ops.object.camera_add()
    camera = bpy.context.object
    camera.data.type = "ORTHO"
    scene.camera = camera

    specs = [
        ("sunstone-reliquary-staff", lambda b: build_staff(b, "Oak", "Gold", "Amber", sun_halo=True)),
        ("ironmaw-cleaver", build_ironmaw_cleaver),
        ("mage-arcane-staff", lambda b: build_staff(b, "Dark Wood", "Steel", "Jade", sun_halo=False)),
    ]

    preview_objs = []
    for idx, (name, fn) in enumerate(specs):
        b = Build()
        fn(b)
        obj = b.finish(name)
        fbx_path = WEAPONS_DIR / (name + ".fbx")
        icon_path = ICONS_DIR / (name + ".png")
        export_static_fbx(fbx_path, obj)
        gear_art.icon(scene, camera, obj, icon_path, armed=True)
        print(f"WEAPON {fbx_path.name} tris={triangle_count(obj)} icon={icon_path.name}", flush=True)
        obj.location = ((idx - 1) * 1.15, 0.0, 0.0)
        preview_objs.append(obj)

    # Render combined preview sheet with an unarmed knight beside the 3 new weapons
    kb = Build(0)
    character(kb, "knight", False, include_weapons=False)
    k_obj = kb.finish("Unarmed_Knight_Check")
    k_obj.location = (-2.4, 0.0, -0.7)
    preview_objs.append(k_obj)

    scene.render.film_transparent = False
    world = bpy.data.worlds.new("PolishPreviewWorld")
    world.color = (0.12, 0.15, 0.19)
    scene.world = world
    scene.render.resolution_x = 1200
    scene.render.resolution_y = 640
    camera.location = Vector((-0.6, -7.5, 0.5))
    camera.rotation_euler = (Vector((-0.6, 0.0, 0.25)) - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.ortho_scale = 5.4
    out_preview = PREVIEW_DIR / "unarmed_and_weapons_preview.png"
    scene.render.filepath = str(out_preview)
    bpy.ops.render.render(write_still=True)
    print(f"PREVIEW {out_preview}", flush=True)


def main():
    build_unarmed_combatants()
    build_new_weapons_and_icons()


if __name__ == "__main__":
    main()
