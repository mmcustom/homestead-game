
# Mallard (Wave 5, 2026-11-19): the "waterfowl" species. Drake and hen, sitting on the water.
# Origin at the water line (the game keeps the duck's root on the water surface): the hull sits a little below z = 0.
# About 0.58 m bill to tail. Uses the shared animal_util.py. Front faces Blender -Y.
import bpy, bmesh, math
from mathutils import Vector, Euler
from mathutils.bvhtree import BVHTree
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
exec(open(ROOT + r"\animal_util.py", encoding="utf-8").read())
V = Vector

def skeleton():
    J = {}
    def j(n, p, r): J[n] = (V(p), r)
    # joints kept further apart than their radii: the skin modifier folds over itself when a joint sits inside its neighbour's radius
    j("tail", (0, 0.29, 0.125), 0.018); j("rump", (0, 0.15, 0.085), 0.095); j("chest", (0, -0.05, 0.072), 0.112)
    j("neck0", (0, -0.155, 0.165), 0.058); j("neck1", (0, -0.17, 0.215), 0.049); j("head", (0, -0.183, 0.264), 0.057)
    j("fore", (0, -0.222, 0.257), 0.03)
    E = [("tail", "rump"), ("rump", "chest"), ("chest", "neck0"), ("neck0", "neck1"), ("neck1", "head"), ("head", "fore")]
    return J, E

def squash(o, k=0.8, cz=0.07):
    """a duck's body is a low boat: flatten it top to bottom behind the neck, keeping the neck and head round"""
    for v in o.data.vertices:
        w = min(1.0, max(0.0, (v.co.y + 0.14) / 0.06))
        if v.co.z > 0.16: w *= max(0.0, 1 - (v.co.z - 0.16) / 0.03)
        v.co.z = cz + (v.co.z - cz) * (1 - (1 - k) * w)
    o.data.update()

def ellipsoid(bm, c, size, rot=(0, 0, 0), mat=0, seg=(16, 10)):
    t = bmesh.new(); bmesh.ops.create_uvsphere(t, u_segments=seg[0], v_segments=seg[1], radius=1.0)
    R = Euler(rot).to_matrix()
    for v in t.verts: v.co = V(c) + R @ V((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
    for f in t.faces: f.material_index = mat
    me = bpy.data.meshes.new("t"); t.to_mesh(me); t.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)

def bill(bm, mat):
    """the flat, spoon-shaped bill: rings from the face to the rounded nail, wider than tall and a little wider at the tip"""
    rows = []; n = 14; sides = 16
    for i in range(n + 1):
        t = i / n; y = -0.214 - 0.07 * t; z = 0.258 - 0.026 * t
        end = math.sqrt(max(0.0, 1 - max(0.0, (t - 0.78) / 0.22) ** 2))      # rounded nail
        w = (0.022 + 0.003 * math.sin(t * math.pi * 0.8)) * end + 0.0015
        h = (0.014 * (1 - 0.6 * t)) * end + 0.0015
        ring = []
        for k in range(sides):
            a = 2 * math.pi * k / sides
            zz = math.sin(a) * h
            if zz < 0: zz *= 0.45            # flat underside
            ring.append(bm.verts.new((math.cos(a) * w, y, z + zz)))
        rows.append(ring)
    for a, b in zip(rows[:-1], rows[1:]):
        for k in range(sides):
            f = bm.faces.new([a[k], a[(k + 1) % sides], b[(k + 1) % sides], b[k]]); f.material_index = mat
    f = bm.faces.new(list(reversed(rows[-1]))); f.material_index = mat
    f = bm.faces.new(rows[0]); f.material_index = mat

def parts(drake=True, name="Drake_Parts"):
    """bill (0), eyes (1), feet (2: webbed, tucked under, paddling), and the drake's curled tail feathers (3)"""
    bm = bmesh.new()
    bill(bm, 0)
    for s in (-1, 1): ellipsoid(bm, (s * EYE_X, -0.2, 0.272), (0.0065, 0.0065, 0.0065), mat=1, seg=(10, 8))
    for s in (-1, 1):
        hip = V((s * 0.05, 0.08, -0.01)); ank = V((s * 0.06, 0.13, -0.05))
        tube_var(bm, [hip, ank], [0.012, 0.008], sides=6, mat=2)
        for ang in (-30, 0, 30):
            a = math.radians(ang); tip = ank + V((math.sin(a) * 0.045, 0.045 * math.cos(a), -0.012))
            tube_var(bm, [ank, tip], [0.006, 0.0035], sides=5, mat=2)
        web = bmesh.ops.create_circle(bm, cap_ends=True, segments=10, radius=1.0)
        for v in web["verts"]:
            v.co = ank + V((v.co.x * 0.03, 0.026 + v.co.y * 0.024, -0.008))
        for f in bm.faces:
            if all(v in web["verts"] for v in f.verts): f.material_index = 2
    if drake:
        for s in (-1, 1):
            p0 = V((s * 0.004, 0.262, 0.128))
            pts = [p0, p0 + V((0, 0.012, 0.016)), p0 + V((s * 0.002, 0.004, 0.032)), p0 + V((s * 0.003, -0.012, 0.034)), p0 + V((s * 0.003, -0.016, 0.022))]
            tube_var(bm, pts, [0.005, 0.0045, 0.004, 0.0035, 0.003], sides=6, mat=3)
    else:
        ellipsoid(bm, (0, 0.2, 0.08), (0.002, 0.002, 0.002), mat=3, seg=(4, 3))   # keeps the slot count equal
    return link(name, bm)

def wings(body, name="Drake_Wings"):
    """folded wings lying along the upper sides of the back, tips crossing over the rump"""
    bm = bmesh.new()
    for s in (-1, 1):
        ellipsoid(bm, (0, 0.085, 0.124), (0.045, 0.16, 0.024), rot=(math.radians(-7), math.radians(s * -24), math.radians(s * 9)), mat=0, seg=(20, 12))
    o = link(name, bm)
    bvh = BVHTree.FromObject(body, bpy.context.evaluated_depsgraph_get())
    half = len(o.data.vertices) // 2
    for i, v in enumerate(o.data.vertices):
        s = -1 if i < half else 1
        hit, n, idx, dist = bvh.ray_cast(V((s * 0.5, 0.09, 0.115)), V((-s, 0, 0)))
        side_x = hit.x if hit is not None else s * 0.1
        v.co.x += side_x - s * 0.036
    o.data.update()
    return o

EYE_X = 0.04
HEN = 0.93   # a hen is a little smaller

def scale_objs(objs, k):
    for o in objs:
        for v in o.data.vertices: v.co *= k
        o.data.update()

def build_standing():
    for ob in list(bpy.data.objects):
        if ob.name.startswith(("Drake_", "Hen_", "Dead_")): bpy.data.objects.remove(ob)
    J, E = skeleton()
    global EYE_X
    flat = {"chest": 1.1, "rump": 1.06}
    body = skin_body("Drake_Body", J, E, "chest", subsurf=3, flat=flat); squash(body)
    bvh = BVHTree.FromObject(body, bpy.context.evaluated_depsgraph_get())
    hit = bvh.ray_cast(V((0.3, -0.2, 0.272)), V((-1, 0, 0)))[0]; EYE_X = hit.x - 0.002
    pr = parts(True); wg = wings(body)
    hb = skin_body("Hen_Body", J, E, "chest", subsurf=3, flat=flat); squash(hb)
    hp = parts(False, "Hen_Parts"); hw = wings(hb, "Hen_Wings")
    scale_objs([hb, hp, hw], HEN)
    return {o.name: tris(o) for o in (body, pr, wg, hb, hp, hw)}

DEAD = {"centre_z": 0.07, "rest_r": 0.135, "belly_z": -0.02, "leg_span": 0.06, "neck_z": 0.13, "neck_span": 0.12, "front_y": -0.13, "head_drop": 0.09}
VITALS = (0, -0.05, 0.09)
DEAD_HEN = {k: v * HEN for k, v in DEAD.items()}


# ---- plumage ----
def feathers(t, L, tc, scale, base_dark, base_light, tip, tipw=0.12):
    """overlapping feathers: scale-shaped cells, each lighter in the middle with a contrasting edge"""
    vo = t.nodes.new("ShaderNodeTexVoronoi"); vo.feature = 'DISTANCE_TO_EDGE'; vo.inputs["Scale"].default_value = scale
    mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (1.0, 0.75, 1.3); L(tc.outputs["Object"], mp.inputs[0]); L(mp.outputs[0], vo.inputs["Vector"])
    nz = t.nodes.new("ShaderNodeTexNoise"); nz.inputs["Scale"].default_value = 140; nz.inputs["Detail"].default_value = 8; L(tc.outputs["Object"], nz.inputs["Vector"])
    edge = ramp(t, L, vo.outputs["Distance"], (0, 0, 0, 1), (1, 1, 1, 1), 0.0, tipw).outputs[0]
    body = ramp(t, L, M(t, L, 'MULTIPLY_ADD', nz.outputs["Fac"], 0.5, M(t, L, 'MULTIPLY', vo.outputs["Distance"], 1.5)), base_dark, base_light, 0.2, 0.8).outputs[0]
    col = mixc(t, L, edge, tip, body)
    return col, M(t, L, 'MULTIPLY_ADD', vo.outputs["Distance"], 2.0, M(t, L, 'MULTIPLY', nz.outputs["Fac"], 0.3))

def fine_noise(t, L, tc, scale=160):
    n = t.nodes.new("ShaderNodeTexNoise"); n.inputs["Scale"].default_value = scale; n.inputs["Detail"].default_value = 8; L(tc.outputs["Object"], n.inputs["Vector"])
    return n.outputs["Fac"]

def drake_coat(name, k=1.0):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    fn = fine_noise(t, L, tc)
    # grey flanks with fine wavy dark lines (vermiculation)
    wav = t.nodes.new("ShaderNodeTexWave"); wav.wave_type = 'BANDS'; wav.bands_direction = 'Z'; wav.inputs["Scale"].default_value = 90; wav.inputs["Distortion"].default_value = 6; wav.inputs["Detail"].default_value = 3
    L(tc.outputs["Object"], wav.inputs["Vector"])
    grey = mixc(t, L, M(t, L, 'MULTIPLY', ramp(t, L, wav.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.55, 0.75).outputs[0], 0.5), (0.5, 0.5, 0.48, 1), (0.22, 0.22, 0.21, 1))
    col = grey
    # brownish-grey back between the wings
    top = M(t, L, 'MULTIPLY', above(t, L, z, 0.135 * k, 0.015 * k), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.075 * k, M(t, L, 'ABSOLUTE', x)), 30.0 / k)))
    col = mixc(t, L, top, col, ramp(t, L, fn, (0.2, 0.18, 0.15, 1), (0.32, 0.29, 0.24, 1), 0.3, 0.7).outputs[0])
    # chestnut breast
    front = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.075 * k, y), 30.0 / k))
    col = mixc(t, L, front, col, ramp(t, L, fn, (0.2, 0.07, 0.035, 1), (0.32, 0.13, 0.06, 1), 0.3, 0.7).outputs[0])
    # black rump and under-tail, pale tail tip
    rear = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', y, 0.205 * k), 40.0 / k))
    col = mixc(t, L, rear, col, (0.015, 0.014, 0.014, 1))
    col = mixc(t, L, above(t, L, y, 0.272 * k, 0.006 * k), col, (0.62, 0.62, 0.6, 1))
    # paler belly below the water line
    col = mixc(t, L, clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.01 * k, z), 40.0 / k)), col, (0.55, 0.54, 0.5, 1))
    # the head: dark green with a sheen, and the white collar
    neck = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.115 * k, y), 40.0 / k))
    head = M(t, L, 'MULTIPLY', neck, above(t, L, z, 0.172 * k, 0.004 * k))
    green = mixc(t, L, M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'ABSOLUTE', nx)), 0.7), (0.012, 0.05, 0.02, 1), (0.02, 0.14, 0.06, 1))
    col = mixc(t, L, head, col, green)
    col = mixc(t, L, M(t, L, 'MULTIPLY', neck, band(t, L, z, 0.163 * k, 0.168 * k, 0.003 * k)), col, (0.7, 0.7, 0.68, 1))
    L(col, b.inputs["Base Color"])
    L(M(t, L, 'MULTIPLY_ADD', head, -0.15, 0.48), b.inputs["Roughness"])   # the green head is glossier
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.2; bp.inputs["Distance"].default_value = 0.0006; L(fn, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.4, 0.4, 0.38, 1); return m

def hen_coat(name, k=HEN):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    col, h = feathers(t, L, tc, 130, (0.07, 0.045, 0.022, 1), (0.17, 0.11, 0.055, 1), (0.3, 0.21, 0.11, 1), tipw=0.045)
    # head and neck: finer, buff with a dark crown and a dark line through the eye
    fn = fine_noise(t, L, tc, 220)
    neck = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.115 * k, y), 40.0 / k))
    headz = above(t, L, z, 0.15 * k, 0.015 * k)
    buff = ramp(t, L, fn, (0.3, 0.21, 0.12, 1), (0.45, 0.34, 0.21, 1), 0.3, 0.7).outputs[0]
    col = mixc(t, L, M(t, L, 'MULTIPLY', neck, headz), col, buff)
    crown = M(t, L, 'MULTIPLY', neck, above(t, L, z, 0.3 * k, 0.008 * k))
    col = mixc(t, L, crown, col, (0.07, 0.045, 0.025, 1))
    stripe = M(t, L, 'MULTIPLY', M(t, L, 'MULTIPLY', neck, band(t, L, z, 0.268 * k, 0.274 * k, 0.004 * k)), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', M(t, L, 'ABSOLUTE', nx), 0.5), 4.0)))
    col = mixc(t, L, stripe, col, (0.08, 0.05, 0.03, 1))
    col = mixc(t, L, clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.01 * k, z), 40.0 / k)), col, (0.4, 0.32, 0.22, 1))
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.55
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.35; bp.inputs["Distance"].default_value = 0.0008; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.25, 0.17, 0.09, 1); return m

def wing_mat(name, hen=False, k=1.0):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    if hen: col, h = feathers(t, L, tc, 90, (0.07, 0.045, 0.025, 1), (0.17, 0.11, 0.06, 1), (0.3, 0.21, 0.11, 1), tipw=0.045)
    else: col, h = feathers(t, L, tc, 80, (0.2, 0.19, 0.17, 1), (0.36, 0.34, 0.3, 1), (0.48, 0.46, 0.42, 1), tipw=0.05)
    # the speculum: a blue-violet patch edged in white on the trailing feathers, low on the folded wing
    low = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.136 * k, z), 60.0 / k))
    spec = M(t, L, 'MULTIPLY', low, band(t, L, y, 0.085 * k, 0.17 * k, 0.004 * k))
    edge = M(t, L, 'MULTIPLY', low, M(t, L, 'MAXIMUM', band(t, L, y, 0.077 * k, 0.083 * k, 0.002 * k), band(t, L, y, 0.172 * k, 0.178 * k, 0.002 * k)))
    col = mixc(t, L, spec, col, (0.05, 0.08, 0.38, 1))
    col = mixc(t, L, edge, col, (0.75, 0.75, 0.72, 1))
    L(col, b.inputs["Base Color"])
    L(M(t, L, 'MULTIPLY_ADD', spec, -0.2, 0.5), b.inputs["Roughness"])
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.35; bp.inputs["Distance"].default_value = 0.0008; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.3, 0.28, 0.25, 1); return m

def make_mats():
    D = bpy.data
    assign(D.objects["Drake_Body"], [drake_coat("animal_waterfowl_drake_coat")])
    assign(D.objects["Drake_Wings"], [wing_mat("animal_waterfowl_drake_wing")])
    assign(D.objects["Hen_Body"], [hen_coat("animal_waterfowl_hen_coat")])
    assign(D.objects["Hen_Wings"], [wing_mat("animal_waterfowl_hen_wing", hen=True, k=HEN)])
    def small(pre):   # eye, foot and curl: each sex bakes its own copy (their UV layouts differ)
        return [lump(pre + "eye", (0.01, 0.006, 0.004, 1), (0.03, 0.018, 0.01, 1), rough=(0.06, 0.12), nscale=20, fine=80, bump=0.05, bumpd=0.0001),
                lump(pre + "foot", (0.55, 0.2, 0.04, 1), (0.75, 0.33, 0.07, 1), rough=(0.45, 0.65), nscale=60, fine=300, bump=0.4, bumpd=0.0003),
                lump(pre + "curl", (0.01, 0.01, 0.01, 1), (0.03, 0.03, 0.035, 1), rough=(0.35, 0.5), nscale=60, fine=200, bump=0.2, bumpd=0.0002)]
    assign(D.objects["Drake_Parts"], [lump("animal_waterfowl_drake_bill", (0.45, 0.42, 0.08, 1), (0.62, 0.58, 0.15, 1), rough=(0.35, 0.55), nscale=40, fine=250, bump=0.3, bumpd=0.0003)] + small("animal_waterfowl_drake_"))
    assign(D.objects["Hen_Parts"], [lump("animal_waterfowl_hen_bill", (0.18, 0.1, 0.04, 1), (0.55, 0.28, 0.07, 1), rough=(0.4, 0.6), nscale=45, fine=250, bump=0.3, bumpd=0.0003)] + small("animal_waterfowl_hen_"))
    return [mm.name for o in D.objects if o.type == "MESH" and o.name.startswith(("Drake_", "Hen_")) for mm in o.data.materials]


STANDING = ["Drake_Body", "Drake_Wings", "Drake_Parts", "Hen_Body", "Hen_Wings", "Hen_Parts"]

def unwrap():
    """skin-modifier and bmesh parts have no UVs: Smart UV Project each standing mesh (before the carcass copies are made)"""
    for n in STANDING:
        o = bpy.data.objects[n]
        bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active = o
        bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.006)
        bpy.ops.object.mode_set(mode='OBJECT')

def build_all():
    build_standing(); unwrap(); mats = make_mats()
    build_dead([("Drake_Body", "Dead_Drake_Body"), ("Drake_Wings", "Dead_Drake_Wings"), ("Drake_Parts", "Dead_Drake_Parts")], DEAD)
    build_dead([("Hen_Body", "Dead_Hen_Body"), ("Hen_Wings", "Dead_Hen_Wings"), ("Hen_Parts", "Dead_Hen_Parts")], DEAD_HEN)
    slump()
    return sorted(set(mats))

def slump(k=0.78):
    """a dead duck on its side settles flatter than its live, buoyant width"""
    for o in bpy.data.objects:
        if o.type == 'MESH' and o.name.startswith("Dead_"):
            for v in o.data.vertices: v.co.z *= k
            o.data.update()
