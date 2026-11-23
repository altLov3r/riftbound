"""Original Pinewatch Relay models. Run from Blender with -- <new output folder>."""
from math import atan2, cos, pi, sin
from pathlib import Path
import random
import sys

import bpy
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
from meshkit import Build, make_materials


def shelter():
    b = Build()
    rng = random.Random(31295)
    for x in (-3.05, 3.05):
        for y in (-2.15, 0, 2.1):
            b.box((x, y, .24), (.68, .62, .48), "Limestone", .08)
            b.box((x, y, 1.54), (.29, .30, 2.60), "Dark Wood", .037)
            b.beam((x, y, 2.08), (x * .76, y, 2.71), .105, "Oak Edge")
    for x in (-3.05, 3.05):
        b.box((x, 0, 2.74), (.30, 5.05, .33), "Dark Wood", .054)
    for i in range(24):
        x = -2.875 + i * .25
        b.box((x, 2.15, 1.56), (.237, .13, 2.34 + rng.uniform(-.09,.09)), rng.choice(("Oak", "Dark Wood", "Oak Edge")), .016)
    for y in (-2.23, -.80, .80, 2.23):
        b.beam((-3.32, y, 2.84), (0, y, 4.18), .12, "Dark Wood")
        b.beam((0, y, 4.18), (3.32, y, 2.84), .12, "Dark Wood")
    for side in (-1, 1):
        pitch = side * atan2(1.34, 3.32)
        b.box((side * 1.68, 0, 3.46), (3.64, 5.48, .14), "Dark Wood", .026, (0, pitch, 0))
        for row in range(8):
            x = .18 + row * .435
            height = 4.29 - x * .403
            for course in range(11):
                y = -2.55 + course * .51 + ((row % 2) - .5) * .12
                finish = rng.choices(("Slate", "Slate Edge", "Grass Dark", "Grass"), (40, 20, 30, 10))[0]
                b.box((side * x, y, height + rng.uniform(-.022,.022)), (.56, .494, .077),
                      finish, .021, (0, pitch, rng.uniform(-.022,.022)))
    b.beam((0,-2.81,4.25), (0,2.81,4.25), .137, "Dark Wood", .14, 8)
    for x in (-2.15,-.62):
        for y in (.62,1.46):
            b.box((x,y,.48),(.13,.13,.94),"Dark Wood",.020)
    for y in (.7,.96,1.22,1.48):
        b.box((-1.39,y,.97),(1.81,.248,.1),"Oak Edge",.022)
    for i in range(12):
        phase = i * pi / 6
        b.beam((-1.47 + .25 * cos(phase), 1.1 + .25 * sin(phase), 1.03),
               (-1.47 + .25 * cos(phase+pi/6), 1.1 + .25 * sin(phase+pi/6), 1.03),
               .036, "Linen", sides=5)
    b.box((.14,1.55,.34),(.66,.60,.68),"Oak",.042)
    for z in (.16,.51):
        b.box((.14,1.23,z),(.68,.069,.087),"Black Iron",.012)
    return b.finish("Pinewatch Shelter")


def overlook():
    b = Build()
    for i in range(24):
        x = -2.87 + i * .25
        b.box((x,0,.12),(.242,4.1,.23),("Oak Edge","Dark Wood","Oak")[i%3],.018)
    for x in (-2.88,2.88):
        for y in (-1.92,0,1.92):
            b.box((x,y,.76),(.20,.20,1.29),"Dark Wood",.032)
        b.box((x,0,1.21),(.17,4.08,.13),"Oak Edge",.026)
        b.box((x,0,.71),(.14,4.08,.12),"Oak",.018)
    for x in (-2.82,-1.4,0,1.4,2.82):
        b.box((x,1.92,.76),(.20,.20,1.29),"Dark Wood",.025)
    b.box((0,1.92,1.21),(5.92,.19,.13),"Oak Edge",.028)
    b.box((0,1.92,.71),(5.92,.14,.12),"Oak",.02)
    b.beam((-1.65,1.29,.2),(-1.6,1.14,7.15),.23,"Dark Wood",.13,10)
    b.beam((-2.43,1.20,5.64),(1.82,1.04,7.09),.145,"Oak Edge",.084,8)
    b.beam((-1.65,1.25,4.57),(.57,1.08,6.67),.115,"Oak",.076,8)
    for z in (1.1,2.4,4.7,5.35):
        b.box((-1.65,1.27,z),(.47,.47,.095),"Black Iron",.028)
    b.beam((1.74,1.03,7.07),(1.71,1.03,5.62),.027,"Linen",sides=7)
    b.beam((-1.63,1.07,2.05),(1.74,1.03,7.07),.019,"Linen",sides=6)
    lantern(b,1.71,1.03,5.58,.82)
    return b.finish("Pinewatch Overlook")


def lantern(b,x,y,z,s):
    b.box((x,y,z-s*.48),(s*.62,s*.62,s*.09),"Black Iron",.029)
    b.box((x,y,z-s*.12),(s*.64,s*.64,s*.09),"Black Iron",.029)
    b.box((x,y,z-s*.30),(s*.42,s*.42,s*.29),"Amber",.033)
    for dx in (-.26,.26):
        for dy in (-.26,.26):
            b.box((x+s*dx,y+s*dy,z-s*.30),(.066,.066,s*.40),"Black Iron",.014)


def marker():
    b = Build()
    b.box((0,0,.22),(.65,.60,.44),"Limestone",.075)
    b.beam((0,0,.04),(.034,.016,2.65),.127,"Dark Wood",.104,8)
    b.box((.39,0,2.38),(1.05,.22,.15),"Oak Edge",.03)
    b.box((.0,-.10,1.73),(1.35,.16,.37),"Oak",.045)
    b.box((.0,-.19,1.73),(1.14,.075,.25),"Dark Wood",.019)
    lantern(b,.82,-.024,2.33,.59)
    return b.finish("Pinewatch Trail Lantern")


def export(path, obj):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=True,
                             axis_forward="-Z", axis_up="Y", apply_unit_scale=True,
                             apply_scale_options="FBX_SCALE_UNITS", add_leaf_bones=False,
                             bake_anim=False, mesh_smooth_type="FACE", path_mode="STRIP")
    obj.data.calc_loop_triangles()
    print(f"ART {path.name} triangles={len(obj.data.loop_triangles)}", flush=True)


def preview(out, assets):
    scene = bpy.context.scene
    assets[0].location = (0,0,0)
    assets[1].location = (8,1,1.4)
    assets[2].location = (-4.1,-4.8,0)
    b = Build(lod=2)
    b.box((1.7,0,-.2),(21,13,.4),"Grass Dark",.0)
    for x in (5.3,10.6):
        for y in (-.9,2.9):
            b.box((x,y,.71),(.33,.34,1.42),"Dark Wood",.0)
    b.finish("Preview ground and overlook supports")
    bpy.ops.object.camera_add(location=(14,-20,12))
    camera = bpy.context.object
    camera.rotation_euler = (Vector((2.8,0,2.8)) - camera.location).to_track_quat("-Z","Y").to_euler()
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 20
    scene.camera = camera
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.show_shadows = True
    scene.display.shading.show_cavity = True
    scene.display.shading.cavity_type = "BOTH"
    scene.display.shading.background_type = "WORLD"
    scene.world = bpy.data.worlds.new("Pinewatch preview backdrop")
    scene.world.color = (.15, .19, .21)
    scene.view_settings.view_transform = "Standard"
    scene.view_settings.exposure = .55
    scene.render.resolution_x = 1120
    scene.render.resolution_y = 780
    scene.render.resolution_percentage = 100
    scene.render.filepath = str(out / "Pinewatch-model-preview.png")
    bpy.ops.wm.save_as_mainfile(filepath=str(out / "Pinewatch.blend"))
    bpy.ops.render.render(write_still=True)


def main():
    out = Path(sys.argv[sys.argv.index("--") + 1]).resolve()
    out.mkdir(parents=True, exist_ok=False)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    make_materials()
    assets = [shelter(), overlook(), marker()]
    for obj in assets:
        export(out / (obj.name + ".fbx"), obj)
    preview(out, assets)


if __name__ == "__main__":
    main()
