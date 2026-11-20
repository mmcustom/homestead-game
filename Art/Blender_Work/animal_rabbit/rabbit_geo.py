
# Eastern cottontail (Wave 5, 2026-11-19): crouched, ready to hop. About 0.42 m nose to tail, 0.29 m to the ear tips.
# Bucks and does look alike, so one model (no Male/Female groups). Uses the shared animal_util.py. Front faces Blender -Y, ground at z = 0.
import bpy, bmesh, math
from mathutils import Vector, Euler
from mathutils.bvhtree import BVHTree
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
exec(open(ROOT + r"\animal_util.py", encoding="utf-8").read())
V = Vector

def skeleton():
    # joints kept further apart than their radii (the skin modifier folds over itself otherwise)
    J = {}
    def j(n, p, r): J[n] = (V(p), r)
    j("tailend", (0, 0.205, 0.095), 0.035); j("rump", (0, 0.11, 0.125), 0.1); j("mid", (0, 0.008, 0.124), 0.078)
    j("chest", (0, -0.078, 0.092), 0.064); j("head", (0, -0.148, 0.166), 0.056); j("nose", (0, -0.2, 0.15), 0.026)
    E = [("tailend", "rump"), ("rump", "mid"), ("mid", "chest"), ("chest", "head"), ("head", "nose")]
    return J, E

def ellipsoid(bm, c, size, rot=(0, 0, 0), mat=0, seg=(16, 10)):
    t = bmesh.new(); bmesh.ops.create_uvsphere(t, u_segments=seg[0], v_segments=seg[1], radius=1.0)
    R = Euler(rot).to_matrix()
    for v in t.verts: v.co = V(c) + R @ V((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
    for f in t.faces: f.material_index = mat
    me = bpy.data.meshes.new("t"); t.to_mesh(me); t.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)

def legs(name="Rabbit_Legs"):
    """the big haunches folded under, long hind feet flat on the ground, short front legs and paws"""
    bm = bmesh.new()
    for s in (-1, 1):
        ellipsoid(bm, (s * 0.047, 0.085, 0.075), (0.045, 0.08, 0.066), rot=(math.radians(-15), 0, 0), seg=(18, 12))   # haunch
        ellipsoid(bm, (s * 0.062, 0.035, 0.016), (0.022, 0.068, 0.016), rot=(math.radians(3), 0, math.radians(s * -4)), seg=(14, 8))   # hind foot
        tube_var(bm, [V((s * 0.034, -0.088, 0.06)), V((s * 0.036, -0.105, 0.035)), V((s * 0.036, -0.116, 0.012))], [0.018, 0.014, 0.011], sides=10)
        ellipsoid(bm, (s * 0.036, -0.128, 0.01), (0.014, 0.022, 0.01), seg=(10, 6))   # front paw
    return link(name, bm)

def ear(bm, s, mat):
    """a long cupped ear: a flattened lofted leaf, hollow side forward and out, laid back and splayed"""
    base = V((s * 0.022, -0.138, 0.205)); n = 10; sides = 12; rows = []
    R = Euler((math.radians(-28), math.radians(s * 14), math.radians(s * -18))).to_matrix()
    for i in range(n + 1):
        t = i / n
        w = 0.019 * math.sin(math.pi * min(1.0, 0.18 + t * 0.95)) + 0.003
        th = 0.006 * (1 - 0.5 * t) + 0.0015
        ring = []
        for k in range(sides):
            a = 2 * math.pi * k / sides
            x = math.cos(a) * w; y = math.sin(a) * th - 0.006 * (math.cos(a) ** 2) * (1 - t)   # cupped
            ring.append(bm.verts.new(base + R @ V((x, y, 0.085 * t))))
        rows.append(ring)
    for a, b in zip(rows[:-1], rows[1:]):
        for k in range(sides):
            f = bm.faces.new([a[k], a[(k + 1) % sides], b[(k + 1) % sides], b[k]]); f.material_index = mat
    f = bm.faces.new(list(reversed(rows[-1]))); f.material_index = mat
    f = bm.faces.new(rows[0]); f.material_index = mat

def parts(body, name="Rabbit_Parts"):
    """ears (0), eyes (1), the cotton tail (2)"""
    bm = bmesh.new()
    for s in (-1, 1): ear(bm, s, 0)
    bvh = BVHTree.FromObject(body, bpy.context.evaluated_depsgraph_get())
    for s in (-1, 1):
        hit = bvh.ray_cast(V((s * 0.3, -0.162, 0.182)), V((-s, 0, 0)))[0]
        ellipsoid(bm, (hit.x - s * 0.003, -0.162, 0.182), (0.009, 0.011, 0.01), mat=1, seg=(12, 8))
    hit = bvh.ray_cast(V((0, 0.5, 0.115)), V((0, -1, 0)))[0]
    ellipsoid(bm, (0, hit.y + 0.012, 0.115), (0.03, 0.026, 0.03), mat=2, seg=(14, 10))   # the tail sits on the rump
    return link(name, bm)

def build_standing():
    for ob in list(bpy.data.objects):
        if ob.name.startswith(("Rabbit_", "Dead_")): bpy.data.objects.remove(ob)
    J, E = skeleton()
    body = skin_body("Rabbit_Body", J, E, "mid", subsurf=3, flat={"rump": 1.08})
    lg = legs(); pr = parts(body)
    return {o.name: tris(o) for o in (body, lg, pr)}

DEAD = {"centre_z": 0.1, "rest_r": 0.085, "belly_z": 0.035, "leg_span": 0.04, "neck_z": 0.15, "neck_span": 0.05, "front_y": -0.12, "head_drop": 0.04}
VITALS = (0, -0.05, 0.11)


# ---- fur ----
def fur(t, L, tc, dark=(0.1, 0.075, 0.05, 1), mid=(0.25, 0.19, 0.12, 1), tip=(0.33, 0.3, 0.25, 1)):
    """agouti fur: hairs banded dark, buff and grey, in fine streaks lying back along the body"""
    mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (1.0, 0.25, 1.0); L(tc.outputs["Object"], mp.inputs[0])
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = 420; n1.inputs["Detail"].default_value = 4; L(mp.outputs[0], n1.inputs["Vector"])
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = 35; n2.inputs["Detail"].default_value = 5; L(tc.outputs["Object"], n2.inputs["Vector"])
    hair = ramp(t, L, n1.outputs["Fac"], dark, mid, 0.35, 0.55).outputs[0]
    col = mixc(t, L, clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', n1.outputs["Fac"], 0.58), 6.0)), hair, tip)
    col = mixc(t, L, M(t, L, 'MULTIPLY', n2.outputs["Fac"], 0.35), col, dark)            # broad patches
    return col, n1.outputs["Fac"]

def coat(name, legs=False):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    col, h = fur(t, L, tc)
    pale = (0.62, 0.58, 0.5, 1)
    belly = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.15, nz), 3.0)), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.11, z), 25.0)))
    col = mixc(t, L, belly, col, pale)
    if legs:
        col = mixc(t, L, clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.03, z), 50.0)), col, (0.48, 0.41, 0.3, 1))    # pale feet
    else:
        # rusty nape behind the head, a pale ring round the eye, a pale chin
        nape = M(t, L, 'MULTIPLY', band(t, L, y, -0.125, -0.09, 0.012), above(t, L, z, 0.165, 0.012))
        col = mixc(t, L, M(t, L, 'MULTIPLY', nape, 0.85), col, (0.38, 0.19, 0.07, 1))
        dy = M(t, L, 'SUBTRACT', y, -0.162); dz = M(t, L, 'SUBTRACT', z, 0.182)
        r = M(t, L, 'SQRT', M(t, L, 'ADD', M(t, L, 'MULTIPLY', dy, dy), M(t, L, 'MULTIPLY', dz, dz)))
        ring = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.02, r), 120.0)), above(t, L, M(t, L, 'ABSOLUTE', x), 0.025, 0.006))
        col = mixc(t, L, M(t, L, 'MULTIPLY', ring, 0.8), col, (0.5, 0.42, 0.3, 1))
        chin = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.17, y), 30.0)), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.15, z), 40.0)))
        col = mixc(t, L, chin, col, pale)
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.8
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.4; bp.inputs["Distance"].default_value = 0.0006; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.25, 0.19, 0.13, 1); return m

def ear_mat(name):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    col, h = fur(t, L, tc, tip=(0.32, 0.29, 0.25, 1))
    inner = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.1, ny), 3.0))           # the hollow faces forward
    col = mixc(t, L, M(t, L, 'MULTIPLY', M(t, L, 'MULTIPLY', inner, band(t, L, M(t, L, 'ABSOLUTE', x), 0.0, 0.05, 0.02)), 0.6), col, (0.42, 0.33, 0.28, 1))
    col = mixc(t, L, M(t, L, 'MULTIPLY', above(t, L, z, 0.27, 0.01), 0.7), col, (0.04, 0.03, 0.025, 1))   # dark tips
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.75
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.25; bp.inputs["Distance"].default_value = 0.0004; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.3, 0.24, 0.19, 1); return m

def tail_mat(name):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    n = t.nodes.new("ShaderNodeTexNoise"); n.inputs["Scale"].default_value = 300; n.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], n.inputs["Vector"])
    col = ramp(t, L, n.outputs["Fac"], (0.6, 0.57, 0.52, 1), (0.82, 0.8, 0.75, 1), 0.3, 0.7).outputs[0]
    col = mixc(t, L, M(t, L, 'MULTIPLY', above(t, L, nz, 0.55, 0.2), 0.75), col, (0.22, 0.17, 0.11, 1))   # brown on top, cotton white beneath
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.9
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.6; bp.inputs["Distance"].default_value = 0.001; L(n.outputs["Fac"], bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.7, 0.68, 0.63, 1); return m

def make_mats():
    D = bpy.data
    assign(D.objects["Rabbit_Body"], [coat("animal_rabbit_coat")])
    assign(D.objects["Rabbit_Legs"], [coat("animal_rabbit_legs", legs=True)])
    assign(D.objects["Rabbit_Parts"], [ear_mat("animal_rabbit_ear"),
                                       lump("animal_rabbit_eye", (0.01, 0.006, 0.004, 1), (0.04, 0.025, 0.015, 1), rough=(0.05, 0.1), nscale=20, fine=80, bump=0.05, bumpd=0.0001),
                                       tail_mat("animal_rabbit_tail")])
    return [mm.name for o in D.objects if o.type == "MESH" and o.name.startswith("Rabbit_") for mm in o.data.materials]

STANDING = ["Rabbit_Body", "Rabbit_Legs", "Rabbit_Parts"]

def unwrap():
    for n in STANDING:
        o = bpy.data.objects[n]
        bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active = o
        bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.006)
        bpy.ops.object.mode_set(mode='OBJECT')

def build_all():
    build_standing(); unwrap(); mats = make_mats()
    build_dead([("Rabbit_Body", "Dead_Body"), ("Rabbit_Legs", "Dead_Legs"), ("Rabbit_Parts", "Dead_Parts")], DEAD)
    lay_ears_back()
    return sorted(set(mats))

def lay_ears_back(angle=-40):
    """the carcass's ears: folded back along the neck before the roll, so they lie against the body instead of sticking out"""
    from mathutils import Matrix
    src = bpy.data.objects["Rabbit_Parts"].data; dst = bpy.data.objects["Dead_Parts"].data
    body = bpy.data.objects["Rabbit_Body"]
    zmin = min(dead_point(v.co.copy(), DEAD).z for v in body.data.vertices)
    ear = set(i for p in src.polygons if p.material_index == 0 for i in p.vertices)
    R = Matrix.Rotation(math.radians(angle), 3, 'X')
    for i in ear:
        P = src.vertices[i].co.copy(); s = 1 if P.x > 0 else -1
        base = V((s * 0.022, -0.138, 0.205))
        P = base + R @ (P - base)
        Q = dead_point(P, DEAD); Q.z = max(0.0, Q.z - zmin)
        bq = dead_point(base.copy(), DEAD); bq.z = max(0.0, bq.z - zmin)
        w = min(1.0, (Q - bq).length / 0.05)                      # away from the head the ear drapes down onto the ground
        Q.z = Q.z + (0.006 + (Q.z - bq.z) * 0.15 - Q.z) * w * w * (3 - 2 * w)
        dst.vertices[i].co = Q
    dst.update()
