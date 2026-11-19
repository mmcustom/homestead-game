
# Wave 4 batch 7 shapes: furniture, wheels, arrows, cordage, tripod (2026-11-19)
import bpy, bmesh, math, random
from mathutils import Vector, noise, Euler
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
exec(open(ROOT + r"\item_wave4f\geo.py", encoding="utf-8").read())
V = Vector

def wrap(bm, c, axis, R, turns, width, r=0.0025, mat=0, per=14, sides=5):
    axis = V(axis).normalized(); up = V((0, 0, 1)) if abs(axis.z) < 0.9 else V((1, 0, 0))
    a = axis.cross(up).normalized(); b = axis.cross(a).normalized(); n = int(turns * per)
    pts = [V(c) + axis * (width * (k / n - 0.5)) + (a * math.cos(2 * math.pi * turns * k / n) + b * math.sin(2 * math.pi * turns * k / n)) * R for k in range(n + 1)]
    tube(bm, pts, r, sides=sides, mat=mat)

def leg(bm, p0, p1, r0, r1, sd, mat):
    stick(bm, V(p0), V(p1), r0, r1, segs=6, sides=8, wob=0.006, sd=sd, mat=mat)

def planks(bm, c, size, n, along='X', gap=0.004, mat=0, sd=0.0):
    c = V(c)
    for i in range(n):
        if along == 'X':
            w = (size[1] - gap * (n - 1)) / n; y = -size[1] / 2 + w / 2 + i * (w + gap)
            box(bm, c + V((0.004 * noise.noise(V((i, sd, 1))), y, 0.002 * noise.noise(V((i, sd, 2))))), (size[0] * (1 - 0.01 * i % 2), w, size[2]), rot=(0.01 * noise.noise(V((i, sd, 3))), 0, 0.006 * noise.noise(V((i, sd, 4)))), mat=mat, jitter=0.002, sd=sd + i)
        else:
            w = (size[0] - gap * (n - 1)) / n; x = -size[0] / 2 + w / 2 + i * (w + gap)
            box(bm, c + V((x, 0, 0)), (w, size[1], size[2]), rot=(0, 0.01 * noise.noise(V((i, sd, 3))), 0), mat=mat, jitter=0.002, sd=sd + i)

def clear():
    for o in list(bpy.data.objects):
        if o.name.startswith("I_"): bpy.data.objects.remove(o)

# ---- rough_stool: split slab seat on three splayed branch legs (2 logs, 2 branches) ----
def make_stool():
    bm = bmesh.new()
    box(bm, (0, 0, 0.43), (0.38, 0.29, 0.055), mat=0, jitter=0.004, sd=1)
    for k, (x, y) in enumerate([(-0.11, -0.08), (0.11, -0.08), (0.0, 0.095)]):
        leg(bm, (x, y, 0.462), (x * 1.55, y * 1.55, 0.0), 0.022, 0.026, 10 + k, 1)
    return link("I_rough_stool", bm)

# ---- end_table: three-plank top, four branch legs, low shelf on stretchers ----
def make_end_table():
    bm = bmesh.new()
    planks(bm, (0, 0, 0.585), (0.46, 0.46, 0.03), 3, 'X', mat=0, sd=20)
    for x in (-0.15, 0.15): box(bm, (x, 0, 0.56), (0.04, 0.4, 0.022), mat=0, jitter=0.001, sd=25 + x)
    for k, (x, y) in enumerate([(-0.18, -0.18), (0.18, -0.18), (-0.18, 0.18), (0.18, 0.18)]):
        leg(bm, (x, y, 0.57), (x * 1.06, y * 1.06, 0.0), 0.022, 0.025, 30 + k, 1)
    for y in (-0.185, 0.185): leg(bm, (-0.21, y, 0.19), (0.21, y, 0.19), 0.014, 0.014, 40 + y, 1)
    planks(bm, (0, 0, 0.212), (0.40, 0.36, 0.022), 2, 'Y', mat=0, sd=45)
    return link("I_end_table", bm)

# ---- chair: plank seat, branch legs and back posts, two back slats, lashed with cordage ----
def make_chair():
    bm = bmesh.new()
    planks(bm, (0, 0, 0.445), (0.44, 0.40, 0.03), 3, 'X', mat=0, sd=50)
    for k, x in enumerate((-0.18, 0.18)):
        leg(bm, (x, -0.16, 0.44), (x * 1.08, -0.19, 0.0), 0.021, 0.024, 51 + k, 1)          # front legs
        leg(bm, (x, 0.19, 0.98), (x * 1.06, 0.20, 0.0), 0.021, 0.025, 53 + k, 1)          # back posts
        leg(bm, (x * 1.04, -0.18, 0.16), (x * 1.03, 0.20, 0.16), 0.013, 0.013, 55 + k, 1)  # side rungs
    leg(bm, (-0.2, -0.175, 0.2), (0.2, -0.175, 0.2), 0.013, 0.013, 57, 1)                   # front rung
    for z in (0.64, 0.84):
        box(bm, (0, 0.19 + (z - 0.44) * 0.0, z), (0.40, 0.022, 0.075), rot=(0.08, 0, 0), mat=0, jitter=0.0015, sd=60 + z)
    for x in (-0.18, 0.18):
        for z in (0.64, 0.84): wrap(bm, (x, 0.192, z), (0, 0, 1), 0.026, 4, 0.05, mat=2)
        wrap(bm, (x, 0.19, 0.42), (0, 0, 1), 0.026, 3, 0.035, mat=2)
    return link("I_chair", bm)

# ---- table: five-plank top on cleats, four log legs, branch stretchers lashed with cordage ----
def make_table():
    bm = bmesh.new()
    planks(bm, (0, 0, 0.73), (1.40, 0.80, 0.04), 5, 'X', mat=0, sd=70)
    for x in (-0.5, 0.5): box(bm, (x, 0, 0.695), (0.07, 0.72, 0.03), mat=0, jitter=0.002, sd=71 + x)
    L = [(-0.6, -0.31), (0.6, -0.31), (-0.6, 0.31), (0.6, 0.31)]
    for k, (x, y) in enumerate(L): leg(bm, (x, y, 0.71), (x * 1.01, y * 1.02, 0.0), 0.042, 0.047, 72 + k, 1)
    for y in (-0.31, 0.31):
        leg(bm, (-0.66, y + 0.05 * (1 if y > 0 else -1), 0.2), (0.66, y + 0.05 * (1 if y > 0 else -1), 0.2), 0.02, 0.018, 80 + y, 1)
    for x in (-0.6, 0.6):
        leg(bm, (x + 0.055 * (1 if x > 0 else -1), -0.37, 0.26), (x + 0.055 * (1 if x > 0 else -1), 0.37, 0.26), 0.018, 0.018, 85 + x, 1)
    for (x, y) in L:
        wrap(bm, (x, y, 0.2), (0, 0, 1), 0.052, 4, 0.05, r=0.003, mat=2)
        wrap(bm, (x, y, 0.26), (0, 0, 1), 0.052, 3, 0.035, r=0.003, mat=2)
    return link("I_table", bm)

# ---- wooden_wheel: two solid wheels sawn from one log, bark round the rim, an axle hole ----
def make_wheels():
    bm = bmesh.new(); R, T, h = 0.22, 0.065, 0.028
    for k, (ox, oy, rz) in enumerate([(0, 0, 0), (0.035, 0.02, 0.9)]):
        tmp = bmesh.new()
        prof = [(h, 0.0), (R - 0.006, 0.0), (R, 0.006), (R + 0.003, T * 0.5), (R, T - 0.006), (R - 0.006, T), (h, T), (h, 0.0005)]
        revolve(tmp, prof, N=40, wob=0.012, sd=3 + k)
        bmesh.ops.remove_doubles(tmp, verts=tmp.verts, dist=0.001)
        for f in tmp.faces:
            cc = f.calc_center_median(); rr = math.hypot(cc.x, cc.y)
            f.material_index = 1 if (abs(f.normal.z) < 0.6 and rr > R * 0.8) else 0
        bmesh.ops.rotate(tmp, verts=tmp.verts, cent=(0, 0, 0), matrix=Euler((0, 0, rz)).to_matrix())
        bmesh.ops.translate(tmp, verts=tmp.verts, vec=(ox, oy, k * (T + 0.0015)))
        me = bpy.data.meshes.new("tmp"); tmp.to_mesh(me); tmp.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)
    return link("I_wooden_wheel", bm)

# ---- arrows: three stick shafts with knapped stone points, bound with cord (1 stick, 1 stone) ----
def make_arrows():
    bm = bmesh.new(); r = 0.0045
    for k, (y, ang) in enumerate([(-0.012, -0.03), (0.0, 0.0), (0.014, 0.045)]):
        R = Euler((0, 0, ang)).to_matrix(); z = r + (0.0065 if k == 1 else 0.0)
        p0 = R @ V((-0.36, y, 0)) + V((0, 0, z)); p1 = R @ V((0.33, y, 0)) + V((0, 0, z))
        stick(bm, p0, p1, r, r * 0.92, segs=4, sides=6, wob=0.0015, sd=90 + k, mat=0)
        tip = bmesh.new(); rock(tip, (0.026, 0.0105, 0.0032), 91 + k, sub=2, k1=0.12, k2=0.08, flat=1.0)
        for v in tip.verts:
            if v.co.x > 0: v.co.y *= max(0.0, 1 - v.co.x / 0.03); v.co.z *= max(0.15, 1 - v.co.x / 0.03)
        me = bpy.data.meshes.new("t"); tip.to_mesh(me); tip.free()
        tb = bmesh.new(); tb.from_mesh(me); bpy.data.meshes.remove(me)
        for f in tb.faces: f.material_index = 1
        bmesh.ops.rotate(tb, verts=tb.verts, cent=(0, 0, 0), matrix=R)
        bmesh.ops.translate(tb, verts=tb.verts, vec=p1 + (R @ V((0.018, 0, 0))))
        me = bpy.data.meshes.new("t"); tb.to_mesh(me); tb.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)
        wrap(bm, p1 - (R @ V((0.004, 0, 0))), R @ V((1, 0, 0)), r + 0.0018, 5, 0.018, r=0.0012, mat=2, per=8, sides=4)
        wrap(bm, p0 + (R @ V((0.03, 0, 0))), R @ V((1, 0, 0)), r + 0.0016, 4, 0.014, r=0.0011, mat=2, per=8, sides=4)
    return link("I_arrows", bm)

# ---- cordage: a coiled hank of two-ply twisted grass cord with a loose tail ----
def make_cordage():
    bm = bmesh.new(); rc = 0.0058; S = 8; turns = 5.3; n = 230
    path = []
    for k in range(n + 1):
        th = 2 * math.pi * turns * k / n; t = k / n
        Rr = 0.066 + 0.011 * math.sin(th * 0.43 + 1.0) + 0.006 * noise.noise(V((t * 6, 1, 2)))
        cx = 0.016 * math.sin(t * 7.0); cy = 0.012 * math.cos(t * 5.0)
        path.append(V((cx + Rr * math.cos(th), cy + Rr * math.sin(th), rc + 0.009 * (0.5 + 0.5 * math.sin(th * 0.9 + t * 4)) + 0.002 * noise.noise(V((t * 9, 3, 4))))))
    end = path[-1]; dirv = (path[-1] - path[-2]).normalized()
    for k in range(1, 9):
        path.append(end + dirv * (0.012 * k) + V((0.0, 0.004 * k * k * 0.12, -0.0006 * k)))
    for p in path: p.z = max(p.z, rc)
    rings = []; prev_a = None; s = 0.0; cl = bm.loops.layers.color.new("rope") if False else None
    col = []; 
    for k, p in enumerate(path):
        if k: s += (path[k] - path[k - 1]).length
        d = (path[min(k + 1, len(path) - 1)] - path[max(k - 1, 0)]).normalized()
        if prev_a is None:
            a = d.cross(V((0, 0, 1))).normalized()
        else:
            a = (prev_a - d * prev_a.dot(d)).normalized()
        b = d.cross(a).normalized(); prev_a = a; ring = []
        for i in range(S):
            ph = 2 * math.pi * i / S
            v = bm.verts.new(p + (a * math.cos(ph) + b * math.sin(ph)) * rc); ring.append(v); col.append((math.cos(ph), math.sin(ph), s))
        rings.append(ring)
    for ra, rb in zip(rings[:-1], rings[1:]):
        for i in range(S):
            j = (i + 1) % S; bm.faces.new([ra[i], ra[j], rb[j], rb[i]])
    for ring, cc, flip in ((rings[0], path[0], True), (rings[-1], path[-1], False)):
        c = bm.verts.new(cc); col.append((0.0, 0.0, 0.0 if flip else s))
        for i in range(S):
            vs = [ring[i], ring[(i + 1) % S], c]; bm.faces.new(vs[::-1] if flip else vs)
    o = link("I_cordage", bm)
    at = o.data.color_attributes.new("rope", 'FLOAT_COLOR', 'POINT')
    for i, v in enumerate(o.data.vertices): at.data[i].color = (col[i][0] * 0.5 + 0.5, col[i][1] * 0.5 + 0.5, col[i][2], 1.0)
    return o

# ---- tripod: three branch poles bound near the top with cord, carried folded (3 branches, 1 cordage) ----
def make_tripod():
    bm = bmesh.new(); r0, r1 = 0.032, 0.022; Ln = 1.6
    poles = [(-0.034, r0, 0.03), (0.034, r0, -0.025), (0.0, r0 + 0.055, 0.0)]
    for k, (y, z, spread) in enumerate(poles):
        p0 = V((-Ln / 2, y + spread * 3.0, z)); p1 = V((Ln / 2, y * 0.6, z - 0.006 if k < 2 else z - 0.012))
        stick(bm, p0, p1, r0, r1, segs=12, sides=8, wob=0.01, sd=100 + k, mat=0)
    wrap(bm, (0.6, 0.0, 0.05), (1, 0, 0), 0.058, 8, 0.06, r=0.004, mat=1, per=12, sides=5)
    return link("I_tripod", bm)

def build_all():
    clear()
    objs = [make_stool(), make_end_table(), make_chair(), make_table(), make_wheels(), make_arrows(), make_cordage(), make_tripod()]
    xs = [-2.2, -1.5, -0.8, 0.4, 1.7, 2.6, 3.4, 1.0]; ys = [0, 0, 0, 0, 0, 0, 0, -1.2]
    for o, x, y in zip(objs, xs, ys): o.location = (x, y, 0)
    flat_planks()
    return {o.name: len(o.data.polygons) for o in objs}

def flat_planks():
    """boxes (planks, slabs, slats) shade flat; sticks, poles and cord stay smooth; wheel faces flat, rims smooth"""
    for on in ("I_rough_stool", "I_end_table", "I_chair", "I_table"):
        for f in bpy.data.objects[on].data.polygons:
            if f.material_index == 0: f.use_smooth = False
    for f in bpy.data.objects["I_wooden_wheel"].data.polygons:
        if abs(f.normal.z) > 0.85: f.use_smooth = False
