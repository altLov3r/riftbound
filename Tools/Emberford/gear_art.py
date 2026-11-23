"""Render the Riftbound gear catalog icons and export inspectable weapons with Blender."""
from math import cos, pi, sin
from pathlib import Path
import sys

import bpy
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
from meshkit import Build, make_materials
from pinewatch import export


# Ids match ItemCatalog. Only the weapon entries are exported as game meshes.
ITEMS = {
    "starter-axe": ("axe", "Iron", "Oak", .89, .30, False),
    "raider-cleaver": ("axe", "Black Iron", "Leather", .70, .41, False),
    "captain-axe": ("axe", "Steel", "Gold", 1.30, .46, True),
    "emberwatch-axe": ("axe", "Steel", "Ember Tile", 1.12, .37, False),
    "warden-axe": ("axe", "Basalt", "Jade", 1.32, .55, True),
    "bloodrock-waraxe": ("axe", "Red Iron", "Bone", 1.26, .48, True),
    "wyrmtooth-edge": ("cleaver", "Black Iron", "Amber", 1.36, .35),
    "scarlet-blade": ("blade", "Steel", "Ember Tile", .97, .19),
    "duskwood-shadow-dagger": ("blade", "Black Iron", "Violet", .60, .102),
    "copper-dagger": ("blade", "Red Iron", "Gold", .49, .09),
    "bronze-broadsword": ("blade", "Gold", "Oak Edge", 1.16, .18),
    "truesilver-blade": ("blade", "Steel", "Jade", 1.34, .20),
    "worldbreaker-maul": ("maul", "Basalt", "Amber", 1.25, .31),
    "leather-vest": ("chest", "Leather", "Oak Edge", "Iron"),
    "boneplate": ("chest", "Basalt", "Bone", "Black Iron"),
    "watchtower-cuirass": ("chest", "Steel", "Gold", "Cloak Blue"),
    "scarlet-aegis": ("shield", "Banner Red", "Gold", False),
    "iron-buckler": ("shield", "Iron", "Steel", True),
    "crown-of-molten-ruin": ("crown", "Black Iron", "Amber"),
    "travel-charm": ("pendant", "Linen", "Jade"),
    "riverwarden-signet": ("ring", "Gold", "Jade"),
    "emberwatch-sigil": ("sigil", "Ember Tile", "Amber"),
    "crypt-signet": ("ring", "Iron", "Violet"),
    "cathedral-relic": ("censer", "Gold", "Amber"),
    "ember-drake-scale": ("heart", "Ember Tile", "Amber"),
    "horde-war-banner": ("banner", "Banner Red", "Bone"),
    "cinderfury-signet": ("ring", "Gold", "Ember Tile"),
}
WEAPONS = {"axe", "blade", "maul", "cleaver"}


def loop(b, x, y, z, rx, rz, thickness, material, segments=24):
    points = [(x + rx * cos(2 * pi * i / segments), y, z + rz * sin(2 * pi * i / segments))
              for i in range(segments)]
    for index, point in enumerate(points):
        b.beam(point, points[(index + 1) % segments], thickness, material, sides=6)


def inset(b, x, y, z, size, material):
    return b.plate([(x - size, z), (x, z + size), (x + size, z), (x, z - size)], y, size * .35, material)


def grip(b, length, accent, radius=.047):
    b.beam((0,0,-.16),(0,0,length),radius,"Dark Wood",radius * .76,8)
    for z in (-.145, -.106, -.067, -.028, .011, .050, .089):
        b.beam((0,0,z),(0,0,z+.019),radius * 1.1,"Leather",radius,sides=8)
    b.oval((0,0,-.167),(radius * 1.45,radius * 1.16,.029),accent,sides=9)


def weapon(b, style, main, accent, length, width, mirrored=False):
    grip(b, length if style == "axe" else length * .76 if style == "maul" else .184, accent)
    if style == "axe":
        top = length
        sides = (-1, 1) if mirrored else (1,)
        b.box((0,0,top-.06),(.19,.18,.20),accent,.035)
        for sign in sides:
            blade = [(sign*.035,top+.008),(sign*width*.79,top+.15),(sign*width,top+.06),
                     (sign*width*.91,top-.22),(sign*width*.51,top-.31),(sign*.074,top-.16)]
            b.plate(blade,0,.097,main)
            trim = [(sign*width*.79,top+.15),(sign*width,top+.06),(sign*width*.91,top-.22),
                    (sign*width*.51,top-.31),(sign*width*.64,top-.28),(sign*width*.78,top-.20),
                    (sign*width*.86,top+.05)]
            b.plate(trim,0,.11,"Steel" if main != "Steel" else "Slate Edge")
            for y in (-.055,.055):
                inset(b,sign*width*.46,y,top-.02,.052,accent)
    elif style == "maul":
        for z in (.32,.69,.91):
            b.beam((0,0,z),(0,0,z+.054),.069,"Black Iron",sides=8)
        b.box((0,0,length-.12),(.66,.37,.36),main,.074)
        for x in (-.29,.29):
            b.box((x,0,length-.12),(.20,.40,.39),"Black Iron",.057)
            b.box((x,-.217,length-.12),(.11,.06,.26),accent,.024)
        for y in (-.217,.217):
            inset(b,0,y,length-.12,.104,accent)
            inset(b,0,y*1.12,length-.12,.045,"Linen")
    else:
        guard = width * 2.9
        b.beam((-guard,0,.186),(guard,0,.186),.034,accent,.030,8)
        inset(b,0,-.065,.204,width*.45,accent)
        if style == "cleaver":
            profile = [(-width*.38,.214),(-width*.71,.54),(-width*.39,length+.02),
                       (.043,length+.13),(width*.84,length-.045),(width*.68,.72),
                       (width*.92,.65),(width*.64,.45),(width*.4,.214)]
        else:
            profile = [(-width*.76,.214),(-width,.36),(-width*.7,length*.84),
                       (0,length),(width*.7,length*.84),(width,.36),(width*.76,.214)]
        b.plate(profile,0,.062,main)
        b.plate([(x * .59,z) for x,z in profile], -.039,.028, "Slate" if main == "Steel" else accent)
        for y in (-.061,.061):
            inset(b,0,y,length*.65,width*.32,"Amber" if style == "cleaver" else accent)


def chest(b, leather, trim, badge):
    profile = [(-.46,.98),(-.28,1.08),(-.17,.94),(.17,.94),(.28,1.08),(.46,.98),
               (.33,.75),(.32,.27),(.18,.14),(-.18,.14),(-.32,.27),(-.33,.75)]
    b.plate(profile,0,.18,leather)
    for side in (-1,1):
        b.oval((side*.31,-.053,.91),(.18,.11,.116),trim,sides=10)
        b.beam((side*.29,-.12,.83),(side*.22,-.14,.38),.045,trim,sides=6)
        if trim == "Bone":
            for row in range(5):
                z = .33 + row * .09
                b.beam((side*.055,-.137,z),(side*.258,-.152,z+.045),.038,"Bone",.026,7)
    b.box((0,-.117,.34),(.60,.078,.093),trim,.022)
    inset(b,0,-.184,.66,.13,badge)
    inset(b,0,-.199,.66,.058,trim)


def shield(b, main, trim, round_shape):
    if round_shape:
        b.beam((0,.093,.55),(0,-.086,.55),.42,trim,.40,20)
        b.beam((0,-.09,.55),(0,-.138,.55),.34,main,.31,20)
    else:
        crest = [(-.42,.88),(-.31,1.0),(0,1.10),(.31,1.0),(.42,.88),
                 (.35,.48),(.25,.32),(0,.185),(-.25,.32),(-.35,.48)]
        b.plate(crest,0,.14,trim)
        b.plate([(x*.87,.64 + (z-.64)*.86) for x,z in crest],-.09,.083,main)
    b.oval((0,-.18,.60),(.15,.105,.15),trim,sides=13)
    inset(b,0,-.274,.60,.071,"Amber" if not round_shape else "Black Iron")


def small_item(b, style, main, accent):
    if style == "ring":
        loop(b,0,0,.49,.31,.31,.081,main)
        b.oval((0,-.066,.803),(.176,.132,.14),"Black Iron",sides=10)
        b.oval((0,-.163,.839),(.137,.103,.119),accent,sides=8)
    elif style == "crown":
        loop(b,0,0,.55,.36,.19,.085,main)
        for i in range(7):
            t = (i - 3) * .29
            x, y = .34*sin(t),-.17*cos(t)
            b.beam((x,y,.57),(x*1.10,y,.99 - abs(x)*.73),.073,main,.012,6)
            inset(b,x,y-.046,.62,.058,accent)
    elif style == "heart":
        b.oval((0,0,.57),(.39,.22,.43),main,sides=9)
        for i in range(5):
            t = (i - 2)*.46
            inset(b,.255*sin(t),-.21*cos(t),.57+.15*cos(t),.11,accent)
    elif style == "banner":
        b.beam((-.36,0,.16),(-.36,0,1.12),.042,"Dark Wood",.036)
        b.beam((-.43,0,1.01),(.36,0,1.01),.032,"Gold")
        b.plate([(-.33,1),(.3,1),(.26,.36),(0,.47),(-.27,.36)],0,.046,main)
        inset(b,-.02,-.043,.74,.15,accent)
    elif style == "censer":
        b.oval((0,0,.37),(.31,.25,.21),main,sides=14)
        b.oval((0,-.08,.47),(.254,.206,.148),accent,sides=13)
        for x in (-.24,.24):
            b.beam((x,-.04,.52),(0,-.04,.96),.027,"Dark Wood",sides=6)
        loop(b,0,0,1,.077,.084,.028,main,12)
    else:
        loop(b,0,0,.67,.27,.31,.038,main)
        b.oval((0,-.08,.52),(.281,.125,.32),main,sides=10)
        inset(b,0,-.208,.56,.21,accent)
        inset(b,0,-.246,.56,.083,"Gold")


def icon(scene, camera, obj, path, armed):
    bpy.context.view_layer.update()
    vertices = [obj.matrix_world @ Vector(p) for p in obj.bound_box]
    centre = sum(vertices, Vector()) / 8
    camera.location = centre + Vector((2.9,-9,2.1))
    camera.rotation_euler = (centre-camera.location).to_track_quat("-Z","Y").to_euler()
    if armed:
        camera.rotation_euler.rotate_axis("Z", pi/5.5)
    inv = camera.rotation_euler.to_matrix().inverted()
    points = [inv @ (p-centre) for p in vertices]
    camera.data.ortho_scale = max(2*abs(v[i]) for v in points for i in (0,1)) * 1.21
    scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)


def main():
    output = Path(sys.argv[sys.argv.index("--") + 1]).resolve()
    output.mkdir(parents=True, exist_ok=False)
    for folder in ("Icons","Weapons"):
        (output / folder).mkdir()
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
    scene.view_settings.exposure = .72
    bpy.ops.object.camera_add()
    camera = bpy.context.object
    camera.data.type = "ORTHO"
    scene.camera = camera
    for name, spec in ITEMS.items():
        style, *parts = spec
        b = Build()
        if style in WEAPONS:
            weapon(b,style,*parts)
        elif style == "chest":
            chest(b,*parts)
        elif style == "shield":
            shield(b,*parts)
        else:
            small_item(b,style,*parts)
        obj = b.finish(name)
        if style in WEAPONS:
            export(output / "Weapons" / (name + ".fbx"),obj)
        icon(scene,camera,obj,output / "Icons" / (name + ".png"),style in WEAPONS)
        mesh = obj.data
        bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.meshes.remove(mesh)


if __name__ == "__main__":
    main()
