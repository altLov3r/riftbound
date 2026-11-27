"""
Render bespoke 256x256 transparent PNG icons for the 2 new Build 11 ItemCatalog entries:
  Assets/RPG/Resources/Gear/Icons/alliance-war-banner.png
  Assets/RPG/Resources/Gear/Icons/sunforged-breastplate.png
"""
from pathlib import Path
import sys
import bpy

sys.path.insert(0, str(Path(__file__).resolve().parent))
from meshkit import Build, make_materials
import gear_art

REPO_ROOT = Path(__file__).resolve().parents[2]
ICONS_DIR = REPO_ROOT / "Assets" / "RPG" / "Resources" / "Gear" / "Icons"


def main():
    ICONS_DIR.mkdir(parents=True, exist_ok=True)
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
        ("alliance-war-banner", lambda b: gear_art.small_item(b, "banner", "Cloak Blue", "Gold")),
        ("sunforged-breastplate", lambda b: gear_art.chest(b, "Steel", "Gold", "Amber")),
    ]
    for name, fn in specs:
        b = Build()
        fn(b)
        obj = b.finish(name)
        out_path = ICONS_DIR / (name + ".png")
        gear_art.icon(scene, camera, obj, out_path, armed=False)
        print(f"ICON {out_path}", flush=True)
        mesh = obj.data
        bpy.data.objects.remove(obj, do_unlink=True)
        bpy.data.meshes.remove(mesh)


if __name__ == "__main__":
    main()
