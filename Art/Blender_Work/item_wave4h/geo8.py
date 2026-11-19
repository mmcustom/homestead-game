
# Wave 4 batch 8 shapes: water jars, sap, syrup jug, lye crock, lamp oil tin, potash, rifle rounds, rolls, weather station (2026-11-19)
import bpy, bmesh, math
from mathutils import Vector, noise, Euler, Matrix
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
V = Vector

def add(bm, tmp, rot=None, loc=(0, 0, 0)):
    if rot: bmesh.ops.rotate(tmp, verts=tmp.verts, cent=(0, 0, 0), matrix=Euler(rot).to_matrix())
    bmesh.ops.translate(tmp, verts=tmp.verts, vec=V(loc))
    me = bpy.data.meshes.new("tmp"); tmp.to_mesh(me); tmp.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)

def rev(prof, N=36, wob=0.0, sd=0.0, matf=None):
    t = bmesh.new(); revolve(t, prof, N=N, wob=wob, sd=sd); bmesh.ops.remove_doubles(t, verts=t.verts, dist=1e-5)
    if matf:
        for f in t.faces:
            c = f.calc_center_median(); f.material_index = matf(c, f)
    return t

def disc(r, z, N=36, mat=1):
    t = bmesh.new(); c = t.verts.new((0, 0, z)); ring = [t.verts.new((r * math.cos(2 * math.pi * i / N), r * math.sin(2 * math.pi * i / N), z)) for i in range(N)]
    for i in range(N): f = t.faces.new([c, ring[i], ring[(i + 1) % N]]); f.material_index = mat
    return t

def band(bm, x, R, w, th, mat, N=32):
    """a flat strap round a roll whose axis is X at height R"""
    t = bmesh.new(); rings = []
    for (dx, rr) in ((-w / 2, R), (-w / 2, R + th), (w / 2, R + th), (w / 2, R), (-w / 2, R)):
        rings.append([t.verts.new((x + dx, rr * math.cos(2 * math.pi * i / N), R + rr * math.sin(2 * math.pi * i / N))) for i in range(N)])
    for a, b in zip(rings[:-1], rings[1:]):
        for i in range(N):
            j = (i + 1) % N; f = t.faces.new([a[i], a[j], b[j], b[i]]); f.material_index = mat
    add(bm, t)

def wrap(bm, c, axis, R, turns, width, r=0.0025, mat=0, per=14, sides=5):
    axis = V(axis).normalized(); up = V((0, 0, 1)) if abs(axis.z) < 0.9 else V((1, 0, 0))
    a = axis.cross(up).normalized(); b = axis.cross(a).normalized(); n = int(turns * per)
    pts = [V(c) + axis * (width * (k / n - 0.5)) + (a * math.cos(2 * math.pi * turns * k / n) + b * math.sin(2 * math.pi * turns * k / n)) * R for k in range(n + 1)]
    tube(bm, pts, r, sides=sides, mat=mat)

JAR = [(0, 0), (0.045, 0), (0.052, 0.004), (0.063, 0.035), (0.066, 0.07), (0.059, 0.105), (0.043, 0.13), (0.037, 0.146), (0.041, 0.155), (0.04, 0.162),
       (0.034, 0.162), (0.031, 0.15), (0.034, 0.134), (0.051, 0.106), (0.058, 0.07), (0.056, 0.035), (0.04, 0.012), (0, 0.012)]
LEVEL = 0.124

def make_jar(name, corked=False, wet=True):
    bm = bmesh.new(); add(bm, rev(JAR, N=36, wob=0.012, sd=4.0))
    if wet:
        rr = 0.034 + (0.051 - 0.034) * (0.134 - LEVEL) / (0.134 - 0.106) + 0.0015
        add(bm, disc(rr, LEVEL, mat=1))
    if corked:
        add(bm, rev([(0, 0.142), (0.03, 0.142), (0.0325, 0.16), (0.034, 0.176), (0.031, 0.18), (0, 0.18)], N=24, wob=0.03, sd=7, matf=lambda c, f: 1))
    return link(name, bm)

def make_syrup():
    bm = bmesh.new()
    add(bm, rev([(0, 0), (0.036, 0), (0.042, 0.005), (0.047, 0.035), (0.045, 0.07), (0.033, 0.092), (0.017, 0.103), (0.015, 0.112), (0.018, 0.116), (0.017, 0.121), (0, 0.121)], N=32, wob=0.01, sd=2))
    pts = [V((0.015, 0, 0.108)), V((0.03, 0, 0.113)), V((0.046, 0, 0.104)), V((0.054, 0, 0.088)), V((0.052, 0, 0.07)), V((0.044, 0, 0.06))]
    tube(bm, pts, 0.0045, sides=8, mat=0)
    add(bm, rev([(0, 0.112), (0.0135, 0.112), (0.0145, 0.13), (0.0125, 0.134), (0, 0.134)], N=16, wob=0.03, sd=3, matf=lambda c, f: 1))
    return link("I_maple_syrup", bm)

def make_lye():
    bm = bmesh.new()
    add(bm, rev([(0, 0), (0.055, 0), (0.06, 0.005), (0.068, 0.04), (0.067, 0.09), (0.061, 0.118), (0.064, 0.124), (0.062, 0.13), (0, 0.13)], N=40, wob=0.01, sd=5))
    add(bm, rev([(0, 0.128), (0.069, 0.128), (0.071, 0.133), (0.069, 0.142), (0.02, 0.143), (0.018, 0.155), (0.012, 0.158), (0, 0.158)], N=40, wob=0.006, sd=6, matf=lambda c, f: 1))
    wrap(bm, (0, 0, 0.116), (0, 0, 1), 0.0625, 3, 0.008, r=0.0022, mat=2, per=20)
    for ang in (0.0, math.pi / 2):
        R = Euler((0, 0, ang)).to_matrix()
        pts = [R @ V((x, 0, 0)) for x in (-0.064, -0.071, -0.06, -0.03, 0.0, 0.03, 0.06, 0.071, 0.064)]
        zs = [0.117, 0.134, 0.1445, 0.146, 0.1465, 0.146, 0.1445, 0.134, 0.117]
        tube(bm, [p + V((0, 0, z)) for p, z in zip(pts, zs)], 0.0022, sides=5, mat=2)
    return link("I_lye", bm)

def make_lamp_oil():
    bm = bmesh.new(); R = 0.055
    add(bm, rev([(0, 0), (R - 0.004, 0), (R + 0.0015, 0.003), (R, 0.008), (R, 0.15), (R + 0.0015, 0.154), (R - 0.004, 0.157), (0.02, 0.163), (0, 0.164)], N=44))
    add(bm, rev([(0, 0.155), (0.011, 0.155), (0.011, 0.176), (0.0135, 0.177), (0.0135, 0.19), (0.012, 0.192), (0, 0.192)], N=20, matf=lambda c, f: 1), loc=(0.032, 0, 0))
    for y in (-0.05, 0.05): box(bm, (0, y, 0.158), (0.012, 0.004, 0.012), mat=1)
    pts = [V((0, -0.05, 0.162)), V((0, -0.045, 0.19)), V((0, -0.025, 0.21)), V((0, 0, 0.215)), V((0, 0.025, 0.21)), V((0, 0.045, 0.19)), V((0, 0.05, 0.162))]
    tube(bm, pts, 0.0017, sides=6, mat=1)
    return link("I_lamp_oil", bm)

def make_potash():
    bm = bmesh.new()
    spots = [((0, 0, 0.022), (0.05, 0.04, 0.03), 1), ((0.05, 0.025, 0.016), (0.033, 0.028, 0.02), 2), ((-0.048, 0.018, 0.017), (0.035, 0.03, 0.021), 3),
             ((0.012, -0.045, 0.015), (0.03, 0.026, 0.019), 4), ((-0.02, 0.048, 0.014), (0.028, 0.022, 0.017), 5), ((0.035, -0.02, 0.04), (0.025, 0.02, 0.016), 6),
             ((-0.03, -0.03, 0.012), (0.022, 0.018, 0.013), 7), ((0.06, -0.03, 0.01), (0.016, 0.013, 0.01), 8), ((-0.004, 0.012, 0.05), (0.026, 0.021, 0.015), 9)]
    for c, sz, sd in spots:
        t = bmesh.new(); rock(t, sz, sd * 3.1, sub=1, k1=0.35, k2=0.15, flat=0.6); add(t if False else bm, t, rot=(0.3 * sd, 0.2 * sd, 0.7 * sd), loc=c)
    return link("I_potash", bm)

def make_rounds():
    bm = bmesh.new(); r = 0.006
    prof = [(0, 0), (0.0057, 0), (0.0059, 0.0012), (0.005, 0.0017), (0.0053, 0.003), (0.0058, 0.0045), (0.0057, 0.05), (0.0049, 0.056), (0.0043, 0.0585), (0.0043, 0.0652),
            (0.0042, 0.072), (0.0031, 0.08), (0.0014, 0.0843), (0, 0.085)]
    for k, (y, ang, dx) in enumerate([(-0.022, 0.08, 0.0), (-0.006, -0.05, 0.012), (0.01, 0.12, -0.006), (0.026, -0.02, 0.008)]):
        t = rev(prof, N=20, matf=lambda c, f: 1 if c.z > 0.0648 else 0)
        add(bm, t, rot=(0, math.pi / 2, ang), loc=(dx - 0.042, y, r))
    return link("I_rifle_rounds", bm)

def make_roll(name, L, R, cloth_ends=True, sd=1.0):
    bm = bmesh.new()
    rings = stick(bm, V((-L / 2, 0, R)), V((L / 2, 0, R)), R, R * 0.97, segs=14, sides=32, wob=0.004, sd=sd, mat=0)
    for f in bm.faces:
        if abs(f.normal.x) > 0.8 or abs(f.calc_center_median().x) > L / 2 - 1e-4: f.material_index = 1
    return bm

def make_sleeping_bag():
    L, R = 0.46, 0.105; bm = make_roll("I_sleeping_bag", L, R, sd=2)
    for x in (-0.13, 0.13):
        band(bm, x, R - 0.001, 0.026, 0.004, 2)
        box(bm, (x, -0.012, 2 * R + 0.003), (0.03, 0.028, 0.006), mat=3)
    return link("I_sleeping_bag", bm)

def make_tent():
    L, R = 0.64, 0.08; bm = make_roll("I_tent", L, R, sd=5)
    for k, y in enumerate((-0.045, 0.045)):
        stick(bm, V((-0.4, y, 2 * R - 0.012)), V((0.38, y * 0.9, 2 * R - 0.008)), 0.011, 0.01, segs=8, sides=8, wob=0.003, sd=20 + k, mat=3)
    for x in (-0.2, 0.2):
        wrap(bm, (x, 0, R), (1, 0, 0), R + 0.004, 4, 0.02, r=0.003, mat=2, per=26, sides=5)
    return link("I_tent", bm)

def make_weather_station():
    bm = bmesh.new()
    box(bm, (0, 0, 0.011), (0.21, 0.13, 0.022), mat=0, jitter=0.0008, sd=3)
    cx = -0.04
    add(bm, rev([(0.036, 0.022), (0.047, 0.022), (0.048, 0.026), (0.047, 0.034), (0.043, 0.036), (0.04, 0.032), (0.036, 0.032)], N=48, matf=lambda c, f: 1), loc=(cx, 0, 0))
    add(bm, disc(0.0405, 0.029, N=48, mat=2), loc=(cx, 0, 0))
    box(bm, (cx + 0.012, 0.006, 0.0305), (0.032, 0.0018, 0.0012), rot=(0, 0, 0.45), mat=4)
    add(bm, rev([(0, 0.029), (0.004, 0.029), (0.004, 0.0325), (0, 0.0325)], N=12, matf=lambda c, f: 1), loc=(cx, 0, 0))
    box(bm, (0.062, 0, 0.0235), (0.026, 0.11, 0.003), mat=1)
    tube(bm, [V((0.062, -0.042, 0.0285)), V((0.062, 0.046, 0.0285))], 0.0032, sides=10, mat=3)
    t = bmesh.new(); bmesh.ops.create_uvsphere(t, u_segments=12, v_segments=8, radius=0.0055)
    for f in t.faces: f.material_index = 3
    add(bm, t, loc=(0.062, -0.046, 0.0285))
    for y in (-0.05, 0.05): add(bm, rev([(0, 0.025), (0.0028, 0.025), (0.0028, 0.0265), (0, 0.0265)], N=10, matf=lambda c, f: 1), loc=(0.062, y, 0))
    return link("I_weather_station", bm)

def clear():
    for o in list(bpy.data.objects):
        if o.name.startswith("I_"): bpy.data.objects.remove(o)

def build_all():
    clear()
    objs = [make_jar("I_water", corked=True, wet=False), make_jar("I_water_excellent"), make_jar("I_water_good"), make_jar("I_water_questionable"),
            make_jar("I_water_unsafe"), make_jar("I_maple_sap"), make_syrup(), make_lye(), make_lamp_oil(), make_potash(), make_rounds(),
            make_sleeping_bag(), make_tent(), make_weather_station()]
    for k, o in enumerate(objs):
        o.location = ((k % 7) * 0.3 - 0.9, (k // 7) * 0.6, 0)
    objs[11].location = (-0.6, 1.3, 0); objs[12].location = (0.3, 1.3, 0)
    for o in objs:
        for f in o.data.polygons:
            if f.material_index in () : pass
    return {o.name: sum(len(p.vertices) - 2 for p in o.data.polygons) for o in objs}
