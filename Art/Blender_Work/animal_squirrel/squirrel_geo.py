
# Eastern gray squirrel (Wave 5, 2026-11-19): on all fours, alert, the bushy tail curled up over its back.
# About 0.25 m nose to rump, the tail rising to 0.31 m. Males and females look alike: one model.
# Uses the shared animal_util.py. Front faces Blender -Y, ground at z = 0.
import bpy, bmesh, math
from mathutils import Vector, Euler, Matrix
from mathutils.bvhtree import BVHTree
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
exec(open(ROOT + r"\animal_util.py", encoding="utf-8").read())
V = Vector

def skeleton():
    # joints kept further apart than their radii (the skin modifier folds over itself otherwise)
    J = {}
    def j(n, p, r): J[n] = (V(p), r)
    j("rump", (0, 0.06, 0.08), 0.063); j("chest", (0, -0.04, 0.082), 0.045); j("head", (0, -0.1, 0.122), 0.04)
    j("nose", (0, -0.142, 0.112), 0.017)
    E = [("rump", "chest"), ("chest", "head"), ("head", "nose")]
    return J, E

def ellipsoid(bm, c, size, rot=(0, 0, 0), mat=0, seg=(16, 10)):
    t = bmesh.new(); bmesh.ops.create_uvsphere(t, u_segments=seg[0], v_segments=seg[1], radius=1.0)
    R = Euler(rot).to_matrix()
    for v in t.verts: v.co = V(c) + R @ V((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
    for f in t.faces: f.material_index = mat
    me = bpy.data.meshes.new("t"); t.to_mesh(me); t.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)

def legs(name="Squirrel_Legs"):
    bm = bmesh.new()
    for s in (-1, 1):
        ellipsoid(bm, (s * 0.034, 0.045, 0.055), (0.026, 0.044, 0.04), rot=(math.radians(-20), 0, 0), seg=(16, 10))       # haunch
        ellipsoid(bm, (s * 0.036, 0.035, 0.01), (0.012, 0.04, 0.009), rot=(math.radians(4), 0, math.radians(s * -6)), seg=(12, 8))   # hind foot
        tube_var(bm, [V((s * 0.024, -0.05, 0.055)), V((s * 0.027, -0.06, 0.03)), V((s * 0.028, -0.066, 0.009))], [0.013, 0.01, 0.007], sides=8)
        ellipsoid(bm, (s * 0.028, -0.071, 0.006), (0.009, 0.014, 0.006), seg=(10, 6))   # front paw
    return link(name, bm)

def tail(name="Squirrel_Tail"):
    """the bushy tail: a thick S-curve rising from the rump and curling forward over the back, flattened side to side"""
    pts = [V((0, 0.085, 0.095)), V((0, 0.125, 0.115)), V((0, 0.15, 0.158)), V((0, 0.158, 0.21)), V((0, 0.135, 0.255)), V((0, 0.095, 0.275)), V((0, 0.055, 0.262))]
    radii = [0.028, 0.04, 0.05, 0.053, 0.05, 0.042, 0.024]
    bm = bmesh.new(); tube_var(bm, pts, radii, sides=16)
    for v in bm.verts:
        v.co.x *= 0.7
        v.co += V((noise.noise(v.co * 45) * 0.007, noise.noise(v.co * 45 + V((3, 1, 2))) * 0.007, noise.noise(v.co * 45 + V((7, 5, 1))) * 0.007))   # tufty
    o = link(name, bm)
    ss = o.modifiers.new("Sub", 'SUBSURF'); ss.levels = 2
    bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier="Sub")
    return o

def parts(body, name="Squirrel_Parts"):
    """ears (0) and eyes (1)"""
    bm = bmesh.new()
    for s in (-1, 1):
        ellipsoid(bm, (s * 0.021, -0.093, 0.16), (0.008, 0.005, 0.014), rot=(math.radians(-12), math.radians(s * 18), 0), mat=0, seg=(12, 8))
    bvh = BVHTree.FromObject(body, bpy.context.evaluated_depsgraph_get())
    for s in (-1, 1):
        hit = bvh.ray_cast(V((s * 0.3, -0.113, 0.133)), V((-s, 0, 0)))[0]
        ellipsoid(bm, (hit.x - s * 0.002, -0.113, 0.133), (0.006, 0.008, 0.0075), mat=1, seg=(12, 8))
    return link(name, bm)

def build_standing():
    for ob in list(bpy.data.objects):
        if ob.name.startswith(("Squirrel_", "Dead_")): bpy.data.objects.remove(ob)
    J, E = skeleton()
    body = skin_body("Squirrel_Body", J, E, "chest", subsurf=3)
    lg = legs(); tl = tail(); pr = parts(body)
    return {o.name: tris(o) for o in (body, lg, tl, pr)}

DEAD = {"centre_z": 0.09, "rest_r": 0.05, "belly_z": 0.04, "leg_span": 0.03, "neck_z": 0.12, "neck_span": 0.04, "front_y": -0.07, "head_drop": 0.03}
VITALS = (0, -0.02, 0.09)


# ---- fur ----
def fur(t, L, tc, dark=(0.07, 0.065, 0.06, 1), mid=(0.3, 0.28, 0.25, 1), tip=(0.55, 0.54, 0.51, 1), streak=(1.0, 0.25, 1.0), scale=420):
    """grizzled fur: hairs banded dark and pale, in fine streaks lying back along the body"""
    mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = streak; L(tc.outputs["Object"], mp.inputs[0])
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = scale; n1.inputs["Detail"].default_value = 4; L(mp.outputs[0], n1.inputs["Vector"])
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = 40; n2.inputs["Detail"].default_value = 5; L(tc.outputs["Object"], n2.inputs["Vector"])
    hair = ramp(t, L, n1.outputs["Fac"], dark, mid, 0.35, 0.55).outputs[0]
    col = mixc(t, L, clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', n1.outputs["Fac"], 0.56), 6.0)), hair, tip)
    col = mixc(t, L, M(t, L, 'MULTIPLY', n2.outputs["Fac"], 0.25), col, dark)
    return col, n1.outputs["Fac"]

def coat(name, legs=False):
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    col, h = fur(t, L, tc)
    # a brownish wash along the back, the flanks and the head
    col = mixc(t, L, M(t, L, 'MULTIPLY', above(t, L, nz, 0.1, 0.4), 0.45), col, (0.3, 0.2, 0.12, 1))
    white = (0.74, 0.72, 0.67, 1)
    belly = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.2, nz), 3.0)), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.085, z), 25.0)))
    col = mixc(t, L, belly, col, white)
    if not legs:
        # white ring round the eye, pale chin and throat
        dy = M(t, L, 'SUBTRACT', y, -0.113); dz = M(t, L, 'SUBTRACT', z, 0.133)
        r = M(t, L, 'SQRT', M(t, L, 'ADD', M(t, L, 'MULTIPLY', dy, dy), M(t, L, 'MULTIPLY', dz, dz)))
        ring = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.0135, r), 200.0)), above(t, L, M(t, L, 'ABSOLUTE', x), 0.02, 0.005))
        col = mixc(t, L, M(t, L, 'MULTIPLY', ring, 0.9), col, white)
        chin = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.09, y), 40.0)), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.11, z), 40.0)))
        col = mixc(t, L, chin, col, white)
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.8
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.4; bp.inputs["Distance"].default_value = 0.0005; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.33, 0.3, 0.27, 1); return m

def tail_mat(name):
    """long grizzled hairs, dark bands inside and a frosted white fringe at the edges seen from the side"""
    m, t, b, tc, L = base(name)
    (x, y, z), (nx, ny, nz) = pos_normal(t, L, tc)
    col, h = fur(t, L, tc, dark=(0.05, 0.045, 0.04, 1), mid=(0.28, 0.24, 0.2, 1), tip=(0.62, 0.6, 0.57, 1), streak=(1.0, 0.35, 0.35), scale=260)
    edge = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.4, M(t, L, 'ABSOLUTE', nx)), 4.0))   # where the flat tail's surface turns away
    col = mixc(t, L, M(t, L, 'MULTIPLY', edge, 0.55), col, (0.74, 0.73, 0.7, 1))
    L(col, b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.85
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.6; bp.inputs["Distance"].default_value = 0.001; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.4, 0.38, 0.35, 1); return m

def make_mats():
    D = bpy.data
    assign(D.objects["Squirrel_Body"], [coat("animal_squirrel_coat")])
    assign(D.objects["Squirrel_Legs"], [coat("animal_squirrel_legs", legs=True)])
    assign(D.objects["Squirrel_Tail"], [tail_mat("animal_squirrel_tail")])
    assign(D.objects["Squirrel_Parts"], [lump("animal_squirrel_ear", (0.22, 0.17, 0.13, 1), (0.42, 0.36, 0.3, 1), rough=(0.7, 0.85), nscale=80, fine=300, bump=0.3, bumpd=0.0003),
                                         lump("animal_squirrel_eye", (0.006, 0.004, 0.003, 1), (0.025, 0.018, 0.012, 1), rough=(0.05, 0.1), nscale=20, fine=80, bump=0.05, bumpd=0.0001)])
    return [mm.name for o in D.objects if o.type == "MESH" and o.name.startswith("Squirrel_") for mm in o.data.materials]

STANDING = ["Squirrel_Body", "Squirrel_Legs", "Squirrel_Tail", "Squirrel_Parts"]

def unwrap():
    for n in STANDING:
        o = bpy.data.objects[n]
        bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active = o
        bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.006)
        bpy.ops.object.mode_set(mode='OBJECT')

def lay_tail_back(angle=-80):
    """the carcass's tail: swung back from the rump to trail out behind before the roll, so it lies on the ground"""
    src = bpy.data.objects["Squirrel_Tail"].data; dst = bpy.data.objects["Dead_Tail"].data
    zmin = min(dead_point(v.co.copy(), DEAD).z for v in bpy.data.objects["Squirrel_Body"].data.vertices)
    base = V((0, 0.085, 0.095)); R = Matrix.Rotation(math.radians(angle), 3, 'X')
    for vs, vd in zip(src.vertices, dst.vertices):
        P = base + R @ (vs.co - base)
        Q = dead_point(P, DEAD); Q.z = max(0.0, Q.z - zmin); vd.co = Q
    zt = min(v.co.z for v in dst.vertices)
    for v in dst.vertices: v.co.z = max(0.0, v.co.z - zt * 0.85) * 0.75   # flattened a little where it rests
    dst.update()

def build_all():
    build_standing(); unwrap(); mats = make_mats()
    build_dead([("Squirrel_Body", "Dead_Body"), ("Squirrel_Legs", "Dead_Legs"), ("Squirrel_Tail", "Dead_Tail"), ("Squirrel_Parts", "Dead_Parts")], DEAD)
    lay_tail_back()
    return sorted(set(mats))
