
# Fishing bobber (Wave 5, 2026-11-19): the classic round float, red over white, with a spring-button clip on top and a wire hook below.
# Origin at the water line (the rod puts the bobber's root on the water surface): the white half sits mostly under water.
# 6.6 cm across. One mesh, materials: 0 red paint, 1 white paint, 2 wire.
import bpy, bmesh, math
from mathutils import Vector
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
V = Vector
C = 0.008; R = 0.033      # ball centre above the water line, radius

def piece(bm, prof, mat, N=40, dz=0.0):
    before = set(bm.faces)
    revolve(bm, [(r, z + dz) for r, z in prof], N=N)
    for f in bm.faces:
        if f not in before: f.material_index = mat

def build():
    for ob in list(bpy.data.objects):
        if ob.name.startswith("Bobber"): bpy.data.objects.remove(ob)
    bm = bmesh.new()
    n = 14
    top = [(R * math.cos(a), C + R * math.sin(a)) for a in [math.pi / 2 * i / n for i in range(n + 1)]]          # equator up to the pole
    top = [(r * (1.0 if i else 1.012), z) for i, (r, z) in enumerate(top)]                                        # a slight lip at the seam
    piece(bm, [(0.0, top[-1][1])] + list(reversed(top[:-1])), 0)                                                 # red upper half (pole down to the equator)
    bot = [(R * math.cos(a), C - R * math.sin(a)) for a in [math.pi / 2 * i / n for i in range(n + 1)]]
    piece(bm, [(0.0, bot[-1][1])] + list(reversed(bot[:-1])), 1)                                                 # white lower half
    # the spring button on top: a white plunger with a wire clip beside it
    piece(bm, [(0.0, C + R + 0.013), (0.0045, C + R + 0.0128), (0.0055, C + R + 0.011), (0.0055, C + R - 0.004), (0.0, C + R - 0.004)], 1, N=16)
    tube(bm, [V((0.0035, 0, C + R + 0.003)), V((0.006, 0, C + R + 0.009)), V((0.002, 0, C + R + 0.0145)), V((-0.003, 0, C + R + 0.0135))], 0.0007, sides=6, mat=2)
    # the hook underneath that grips the line
    tube(bm, [V((0, 0, C - R + 0.004)), V((0, 0, C - R - 0.006)), V((0.0035, 0, C - R - 0.0095)), V((0.0065, 0, C - R - 0.0065)), V((0.006, 0, C - R - 0.003))], 0.0007, sides=6, mat=2)
    o = link("Bobber", bm)
    return {"tris": sum(len(p.vertices) - 2 for p in o.data.polygons), "z": [round(min(v.co.z for v in o.data.vertices), 4), round(max(v.co.z for v in o.data.vertices), 4)]}

def make_mats():
    o = bpy.data.objects["Bobber"]
    red = lump("fishing_bobber_red", (0.5, 0.04, 0.03, 1), (0.62, 0.07, 0.05, 1), rough=(0.18, 0.3), nscale=30, fine=200, bump=0.08, bumpd=0.0002)
    white = lump("fishing_bobber_white", (0.78, 0.77, 0.73, 1), (0.88, 0.87, 0.84, 1), rough=(0.2, 0.32), nscale=30, fine=200, bump=0.08, bumpd=0.0002)
    wire = lump("fishing_bobber_wire", (0.5, 0.5, 0.48, 1), (0.66, 0.65, 0.62, 1), rough=(0.25, 0.4), nscale=50, fine=300, bump=0.1, metal=1.0, bumpd=0.0001)
    assign(o, [red, white, wire])
