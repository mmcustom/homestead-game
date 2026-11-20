
# Wild turkey tom (Wave 5, 2026-11-19): walking, tail folded. About 0.95 m to the top of the head, 1.0 m beak to tail tip.
# Uses the shared animal_util.py. Front faces Blender -Y, ground at z = 0.
import bpy, bmesh, math
from mathutils import Vector, Euler
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
exec(open(ROOT + r"\animal_util.py", encoding="utf-8").read())
V = Vector

def skeleton():
    J = {}
    def j(n, p, r): J[n] = (V(p), r)
    j("tail0", (0, 0.24, 0.52), 0.14); j("rump", (0, 0.13, 0.515), 0.2); j("mid", (0, 0.0, 0.495), 0.23)
    j("breast", (0, -0.13, 0.465), 0.215); j("crop", (0, -0.215, 0.59), 0.115)
    j("neck0", (0, -0.24, 0.70), 0.062); j("neck1", (0, -0.255, 0.82), 0.042); j("head", (0, -0.27, 0.905), 0.042)
    j("beak", (0, -0.315, 0.9), 0.018); j("beaktip", (0, -0.345, 0.89), 0.007)
    for s, side in ((-1, "L"), (1, "R")):
        j("thigh" + side, (s * 0.075, 0.03, 0.33), 0.075); j("hock" + side, (s * 0.07, 0.05, 0.215), 0.022)
        j("foot" + side, (s * 0.07, 0.01, 0.022), 0.014)
    E = [("tail0", "rump"), ("rump", "mid"), ("mid", "breast"), ("breast", "crop"), ("crop", "neck0"), ("neck0", "neck1"),
         ("neck1", "head"), ("head", "beak"), ("beak", "beaktip")]
    for side in "LR":
        E += [("mid", "thigh" + side), ("thigh" + side, "hock" + side), ("hock" + side, "foot" + side)]
    return J, E

def ellipsoid(bm, c, size, rot=(0, 0, 0), mat=0, seg=(16, 10)):
    t = bmesh.new(); bmesh.ops.create_uvsphere(t, u_segments=seg[0], v_segments=seg[1], radius=1.0)
    R = Euler(rot).to_matrix()
    for v in t.verts: v.co = V(c) + R @ V((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
    for f in t.faces: f.material_index = mat
    me = bpy.data.meshes.new("t"); t.to_mesh(me); t.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)

def tail_mesh(name="Turkey_Tail"):
    """the folded tail: a flat, slightly drooping wedge of feathers, wider and squared at the end"""
    bm = bmesh.new(); n_len, n_w = 10, 9
    rows = []
    for i in range(n_len + 1):
        t = i / n_len; y = 0.22 + 0.38 * t; z = 0.525 - 0.12 * t - 0.03 * t * t
        w = 0.09 + 0.09 * t ** 0.7; th = 0.03 * (1 - 0.35 * t) + 0.01
        ring = []
        for k in range(n_w):
            a = 2 * math.pi * k / n_w
            ring.append(bm.verts.new((math.cos(a) * w, y, z + math.sin(a) * th)))
        rows.append(ring)
    for a, b in zip(rows[:-1], rows[1:]):
        for k in range(n_w): bm.faces.new([a[k], a[(k + 1) % n_w], b[(k + 1) % n_w], b[k]])
    bm.faces.new(rows[-1]); bm.faces.new(list(reversed(rows[0])))
    return link(name, bm)

def parts(J, tom=True, name="Turkey_Parts"):
    """snood and wattle (red skin), the beard (black, toms only), eyes, toes"""
    bm = bmesh.new()
    if tom: tube(bm, [V((0, -0.305, 0.928)), V((0, -0.33, 0.915)), V((0.004, -0.343, 0.885)), V((0.008, -0.345, 0.855))], 0.0065, sides=8, mat=0)  # snood
    else: tube(bm, [V((0, -0.305, 0.93)), V((0, -0.318, 0.925))], 0.004, sides=6, mat=0)
    ellipsoid(bm, (0, -0.29, 0.865), (0.016, 0.022, 0.03), mat=0)                                                             # wattle
    if tom: tube_var(bm, [V((0, -0.255, 0.6)), V((0, -0.265, 0.55)), V((0.003, -0.262, 0.49)), V((0.006, -0.255, 0.45))], [0.012, 0.013, 0.011, 0.006], sides=8, mat=1)  # beard
    else: ellipsoid(bm, (0, -0.24, 0.56), (0.002, 0.002, 0.002), mat=1, seg=(4, 3))   # keeps the slot
    for s in (-1, 1): ellipsoid(bm, (s * 0.032, -0.282, 0.912), (0.008, 0.008, 0.008), mat=2, seg=(10, 8))
    for s in (-1, 1):
        f = V((s * 0.07, 0.01, 0.012))
        for ang in (-28, 0, 28):
            a = math.radians(ang); tip = f + V((math.sin(a) * 0.075 * s * 0 + math.sin(a) * 0.075, -math.cos(a) * 0.075, -0.004))
            tube_var(bm, [f, f.lerp(tip, 0.5) + V((0, 0, 0.004)), tip], [0.009, 0.007, 0.004], sides=6, mat=3)
        tube_var(bm, [f, f + V((0, 0.045, -0.002))], [0.008, 0.004], sides=6, mat=3)
    return link(name, bm)

def wings(body, name="Turkey_Wings"):
    """folded wings: smooth flattened shapes lying against the body's sides, from the shoulder back over the flank"""
    bm = bmesh.new()
    for s in (-1, 1):
        ellipsoid(bm, (s * 0.0, 0.07, 0.535), (0.045, 0.19, 0.105), rot=(math.radians(-8), 0, 0), mat=0, seg=(20, 12))
        # moved out to the body's side below
    o = link(name, bm)
    from mathutils.bvhtree import BVHTree
    bvh = BVHTree.FromObject(body, bpy.context.evaluated_depsgraph_get())
    half = len(o.data.vertices) // 2
    for i, v in enumerate(o.data.vertices):
        s = -1 if i < half else 1
        hit, n, idx, dist = bvh.ray_cast(V((s * 0.6, 0.07, 0.55)), V((-s, 0, 0)))
        side_x = hit.x if hit is not None else s * 0.2
        v.co.x += side_x - s * 0.02
    o.data.update()
    return o

FAN_C = V((0, 0.27, 0.6))   # the fan's hub, at the top of the rump

def fan_mesh(name="Turkey_Fan", R=0.42):
    """a tom's fan: a half-disc of tail feathers standing up behind him, tilted back, with scalloped feather tips"""
    bm = bmesh.new(); na, nr = 72, 8; N = 9
    rows = []
    for i in range(nr + 1):
        f = i / nr; row = []
        for k in range(na + 1):
            th = math.radians(-96 + 192 * k / na)          # 0 is straight up
            tip = 0.955 + 0.045 * abs(math.cos(th * N * 1.0))
            r = R * f * (tip if i == nr else 1.0) + 0.04
            p = V((math.sin(th) * r, 0, math.cos(th) * r))
            p = V((p.x, p.z * math.sin(math.radians(16)), p.z * math.cos(math.radians(16))))   # tilt back
            row.append(bm.verts.new(FAN_C + V((p.x, p.y, p.z)) + V((0, 0, -0.02))))
        rows.append(row)
    for a, b in zip(rows[:-1], rows[1:]):
        for k in range(na): bm.faces.new([a[k], a[k + 1], b[k + 1], b[k]])
    bmesh.ops.solidify(bm, geom=bm.faces[:], thickness=0.012)
    return link(name, bm)

def fan_mat(name):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    dx = x; dz = M(t, L, 'SUBTRACT', z, FAN_C.z - 0.02); dy = M(t, L, 'SUBTRACT', y, FAN_C.y)
    r = M(t, L, 'SQRT', M(t, L, 'ADD', M(t, L, 'ADD', M(t, L, 'MULTIPLY', dx, dx), M(t, L, 'MULTIPLY', dz, dz)), M(t, L, 'MULTIPLY', dy, dy)))
    ang = M(t, L, 'ARCTAN2', dx, dz)
    nzt = t.nodes.new("ShaderNodeTexNoise"); nzt.inputs["Scale"].default_value = 120; nzt.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], nzt.inputs["Vector"])
    base_c = ramp(t, L, nzt.outputs["Fac"], (0.07, 0.045, 0.025, 1), (0.2, 0.12, 0.06, 1), 0.3, 0.7).outputs[0]
    bars = M(t, L, 'GREATER_THAN', M(t, L, 'SINE', M(t, L, 'MULTIPLY', r, 300.0)), 0.5)
    col = mixc(t, L, M(t, L, 'MULTIPLY', bars, 0.35), base_c, (0.025, 0.018, 0.012, 1))
    col = mixc(t, L, band(t, L, r, 0.37, 0.405, 0.006), col, (0.02, 0.015, 0.01, 1))          # the dark band near the edge
    col = mixc(t, L, above(t, L, r, 0.408, 0.006), col, (0.5, 0.33, 0.17, 1))                 # chestnut and buff tips
    seams = ramp(t, L, M(t, L, 'ABSOLUTE', M(t, L, 'SINE', M(t, L, 'MULTIPLY', ang, 9.0 * 2))), (0, 0, 0, 1), (1, 1, 1, 1), 0.0, 0.08).outputs[0]
    col = mixc(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 1.0, seams), 0.7), col, (0.015, 0.012, 0.01, 1))   # dark lines between feathers
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.55
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.35; bp.inputs["Distance"].default_value = 0.001; L(seams, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.15, 0.09, 0.05, 1); return m

HEN = 0.86   # a hen is about 86% of a tom

def scale_objs(objs, k):
    for o in objs:
        for v in o.data.vertices: v.co *= k
        o.data.update()

def build_standing():
    for ob in list(bpy.data.objects):
        if ob.name.startswith(("Turkey_", "Hen_", "Dead_")): bpy.data.objects.remove(ob)
    J, E = skeleton()
    body = skin_body("Turkey_Body", J, E, "mid")
    fan = fan_mesh(); pr = parts(J, True); wg = wings(body)
    hb = skin_body("Hen_Body", J, E, "mid")
    ht = tail_mesh("Hen_Tail"); hp = parts(J, False, "Hen_Parts"); hw = wings(hb, "Hen_Wings")
    scale_objs([hb, ht, hp, hw], HEN)
    return {o.name: tris(o) for o in (body, fan, pr, wg, hb, ht, hp, hw)}

DEAD = {"centre_z": 0.495, "rest_r": 0.2, "belly_z": 0.40, "leg_span": 0.15, "neck_z": 0.66, "neck_span": 0.22, "front_y": -0.18, "head_drop": 0.12, "tail_y": 0.26, "tail_rest": 0.07}
VITALS = (0, -0.06, 0.51)
DEAD_HEN = {k: v * HEN for k, v in DEAD.items()}


# ---- plumage ----
def feathers(t, L, tc, scale, base_dark, base_light, tip, tipw=0.12):
    """overlapping feathers: scale-shaped cells, each lighter in the middle with a dark tip at its edge"""
    vo = t.nodes.new("ShaderNodeTexVoronoi"); vo.feature = 'DISTANCE_TO_EDGE'; vo.inputs["Scale"].default_value = scale
    mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (1.0, 0.75, 1.3); L(tc.outputs["Object"], mp.inputs[0]); L(mp.outputs[0], vo.inputs["Vector"])
    nz = t.nodes.new("ShaderNodeTexNoise"); nz.inputs["Scale"].default_value = 140; nz.inputs["Detail"].default_value = 8; L(tc.outputs["Object"], nz.inputs["Vector"])
    edge = ramp(t, L, vo.outputs["Distance"], (0, 0, 0, 1), (1, 1, 1, 1), 0.0, tipw).outputs[0]
    body = ramp(t, L, M(t, L, 'MULTIPLY_ADD', nz.outputs["Fac"], 0.5, M(t, L, 'MULTIPLY', vo.outputs["Distance"], 1.5)), base_dark, base_light, 0.2, 0.8).outputs[0]
    col = mixc(t, L, edge, tip, body)
    return col, M(t, L, 'MULTIPLY_ADD', vo.outputs["Distance"], 2.0, M(t, L, 'MULTIPLY', nz.outputs["Fac"], 0.3))

def coat(name, hen=False):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    if hen: col, h = feathers(t, L, tc, 70, (0.06, 0.04, 0.025, 1), (0.2, 0.13, 0.07, 1), (0.24, 0.18, 0.11, 1), tipw=0.045)   # duller, pale tips
    else: col, h = feathers(t, L, tc, 70, (0.03, 0.018, 0.01, 1), (0.19, 0.095, 0.035, 1), (0.012, 0.01, 0.008, 1), tipw=0.07)
    # bare head and upper neck: red, with a pale blue face
    head = above(t, L, z, 0.79, 0.03)
    face = M(t, L, 'MULTIPLY', above(t, L, z, 0.875, 0.015), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', M(t, L, 'ABSOLUTE', nx), 0.4), 3.0)))
    skin = mixc(t, L, face, (0.42, 0.05, 0.04, 1), (0.42, 0.52, 0.7, 1)) if not hen else mixc(t, L, face, (0.33, 0.3, 0.32, 1), (0.4, 0.45, 0.55, 1))
    col = mixc(t, L, M(t, L, 'MULTIPLY', head, 0.6) if hen else head, col, skin)   # a hen's head is partly feathered and grey
    beak = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.305, y), 60.0))
    col = mixc(t, L, M(t, L, 'MULTIPLY', beak, above(t, L, z, 0.85, 0.01)), col, (0.55, 0.5, 0.4, 1))
    legs = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.27, z), 25.0))
    col = mixc(t, L, legs, col, (0.42, 0.3, 0.27, 1))
    L(col, b.inputs["Base Color"])
    plain = M(t, L, 'MAXIMUM', head, legs)
    L(M(t, L, 'MULTIPLY_ADD', plain, 0.2, 0.42), b.inputs["Roughness"])    # feathers have a sheen; skin and legs are duller
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.45; bp.inputs["Distance"].default_value = 0.0015; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.18, 0.1, 0.05, 1); return m

def wing_mat(name, hen=False):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    col, h = feathers(t, L, tc, 60, (0.06, 0.04, 0.025, 1), (0.21, 0.14, 0.08, 1), (0.24, 0.18, 0.11, 1), tipw=0.045) if hen else feathers(t, L, tc, 60, (0.04, 0.025, 0.014, 1), (0.2, 0.11, 0.05, 1), (0.015, 0.012, 0.01, 1), tipw=0.07)
    # the flight feathers at the back and bottom of the folded wing: white bars across dark
    bars = M(t, L, 'GREATER_THAN', M(t, L, 'SINE', M(t, L, 'MULTIPLY', M(t, L, 'ADD', y, M(t, L, 'MULTIPLY', z, 0.6)), 190.0)), 0.62)
    lower = M(t, L, 'MULTIPLY', M(t, L, 'MAXIMUM', above(t, L, y, 0.1, 0.03), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.49, z), 12.0))), 0.85)
    barred = mixc(t, L, bars, (0.04, 0.032, 0.026, 1), (0.62, 0.59, 0.53, 1))
    col = mixc(t, L, lower, col, barred)
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.5
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.4; bp.inputs["Distance"].default_value = 0.0015; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.2, 0.12, 0.06, 1); return m

def tail_mat(name):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    nzt = t.nodes.new("ShaderNodeTexNoise"); nzt.inputs["Scale"].default_value = 90; nzt.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], nzt.inputs["Vector"])
    bars = M(t, L, 'GREATER_THAN', M(t, L, 'SINE', M(t, L, 'MULTIPLY', y, 90.0)), 0.3)
    col = mixc(t, L, M(t, L, 'MULTIPLY', bars, 0.6), ramp(t, L, nzt.outputs["Fac"], (0.08, 0.05, 0.03, 1), (0.22, 0.13, 0.06, 1), 0.3, 0.7).outputs[0], (0.03, 0.02, 0.015, 1))
    col = mixc(t, L, above(t, L, y, 0.545, 0.012), col, (0.42, 0.24, 0.1, 1))   # the chestnut band at the tip
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.55
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.3; bp.inputs["Distance"].default_value = 0.001; L(nzt.outputs["Fac"], bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.18, 0.1, 0.05, 1); return m

def make_mats():
    D = bpy.data
    assign(D.objects["Turkey_Body"], [coat("animal_turkey_coat")])
    assign(D.objects["Turkey_Wings"], [wing_mat("animal_turkey_wing")])
    assign(D.objects["Turkey_Fan"], [fan_mat("animal_turkey_fan")])
    assign(D.objects["Hen_Body"], [coat("animal_turkey_hen_coat", hen=True)])
    assign(D.objects["Hen_Wings"], [wing_mat("animal_turkey_hen_wing", hen=True)])
    assign(D.objects["Hen_Tail"], [tail_mat("animal_turkey_tail")])
    assign(D.objects["Hen_Parts"], [lump("animal_turkey_hen_skin", (0.3, 0.06, 0.05, 1), (0.45, 0.12, 0.1, 1), rough=(0.4, 0.6), nscale=60, fine=300, bump=0.5, bumpd=0.0004),
                                    lump("animal_turkey_hen_beard", (0.015, 0.012, 0.01, 1), (0.06, 0.045, 0.03, 1), rough=(0.6, 0.85), nscale=80, fine=400, bump=0.8, bumpd=0.0005),
                                    lump("animal_turkey_hen_eye", (0.005, 0.004, 0.004, 1), (0.02, 0.015, 0.012, 1), rough=(0.06, 0.12), nscale=20, fine=80, bump=0.05, bumpd=0.0001),
                                    lump("animal_turkey_hen_leg", (0.33, 0.23, 0.21, 1), (0.48, 0.36, 0.32, 1), rough=(0.5, 0.7), nscale=50, fine=300, bump=0.6, bumpd=0.0004)])
    assign(D.objects["Turkey_Parts"], [lump("animal_turkey_skin", (0.32, 0.03, 0.03, 1), (0.5, 0.07, 0.06, 1), rough=(0.4, 0.6), nscale=60, fine=300, bump=0.5, bumpd=0.0004),
                                       lump("animal_turkey_beard", (0.015, 0.012, 0.01, 1), (0.06, 0.045, 0.03, 1), rough=(0.6, 0.85), nscale=80, fine=400, bump=0.8, grain_=True, bumpd=0.0005),
                                       lump("animal_turkey_eye", (0.005, 0.004, 0.004, 1), (0.02, 0.015, 0.012, 1), rough=(0.06, 0.12), nscale=20, fine=80, bump=0.05, bumpd=0.0001),
                                       lump("animal_turkey_leg", (0.33, 0.23, 0.21, 1), (0.48, 0.36, 0.32, 1), rough=(0.5, 0.7), nscale=50, fine=300, bump=0.6, bumpd=0.0004)])
    return [mm.name for o in D.objects if o.type == "MESH" and o.name.startswith(("Turkey_", "Hen_")) for mm in o.data.materials]


def lay_fan_flat():
    """the tom's carcass: the whole fan lies down (the generic tail rule fades in near the hub and left its corners standing)"""
    body = bpy.data.objects["Turkey_Body"]
    zmin = min(dead_point(v.co.copy(), DEAD).z for v in body.data.vertices)
    c = dict(DEAD); c["tail_y"] = -10.0
    src = bpy.data.objects["Turkey_Fan"]; dst = bpy.data.objects["Dead_Fan"]
    for vs, vd in zip(src.data.vertices, dst.data.vertices):
        p = dead_point(vs.co.copy(), c); p.z = max(0.0, p.z - zmin); vd.co = p
    dst.data.update()
