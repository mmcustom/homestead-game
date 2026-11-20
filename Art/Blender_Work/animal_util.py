
# Shared animal helpers (Wave 5, 2026-11-19), for every species: Mike will add animals for other regions and climates.
# A species script supplies its joint table (positions and thicknesses), its parts (ears, beak, antlers...) and its coat;
# these build the smooth body, seat parts on it, make the carcass pose and help write coat regions.
import bpy, bmesh, math
from mathutils import Vector, Matrix, noise
from mathutils.bvhtree import BVHTree
V = Vector

def skin_body(name, J, E, root, subsurf=2, flat=None):
    """J: {joint: (position, radius)} or (position, (radius_x, radius_y)); E: [(joint, joint)]; root: one joint name.
    flat: {joint: x_scale} squeezes the finished surface sideways near those joints (a folded tail, a narrow chest)."""
    names = list(J.keys())
    me = bpy.data.meshes.new(name)
    me.from_pydata([J[n][0] for n in names], [(names.index(a), names.index(b)) for a, b in E], [])
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    sk = o.modifiers.new("Skin", 'SKIN'); sk.branch_smoothing = 0.6; sk.use_smooth_shade = True
    for i, n in enumerate(names):
        r = J[n][1]; r = (r, r) if isinstance(r, (int, float)) else r
        me.skin_vertices[0].data[i].radius = r
        me.skin_vertices[0].data[i].use_root = (n == root)
    ss = o.modifiers.new("Sub", 'SUBSURF'); ss.levels = subsurf; ss.render_levels = subsurf
    bpy.context.view_layer.objects.active = o
    for o2 in bpy.context.scene.objects: o2.select_set(False)
    o.select_set(True)
    bpy.ops.object.modifier_apply(modifier="Skin"); bpy.ops.object.modifier_apply(modifier="Sub")
    if flat:
        for v in o.data.vertices:
            for jn, k in flat.items():
                p, r = J[jn][0], J[jn][1]; r = r if isinstance(r, (int, float)) else max(r)
                w = max(0.0, 1.0 - (v.co - p).length / (r * 2.2))
                if w > 0: v.co.x = p.x + (v.co.x - p.x) * (1 - (1 - k) * w)
        o.data.update()
    return o

def tris(o): return sum(len(p.vertices) - 2 for p in o.data.polygons)

def seat(body, part, base, up, sink):
    """move a part along `up` so its attachment point `base` sits `sink` into the body's real surface"""
    bvh = BVHTree.FromObject(body, bpy.context.evaluated_depsgraph_get())
    hit, nrm, idx, dist = bvh.ray_cast(base + up * 0.3, -up)
    if hit is None: return 0.0
    shift = (hit - base).dot(up) - sink
    for v in part.data.vertices: v.co += up * shift
    part.data.update(); return shift

def merge(name, objs):
    """join several finished meshes into one object (keeps their material slots)"""
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join(); objs[0].name = name; objs[0].data.name = name
    return objs[0]

def dead_point(P, c):
    """the carcass pose: roll -90 degrees about the long axis around the body's centre height (up becomes -X, the right side faces up),
    rest the body on the ground, sag the legs (below belly_z) down and lay the neck (above neck_z, ahead of front_y) down.
    c: centre_z, rest_r, belly_z, leg_span, neck_z, neck_span, front_y, head_drop; optional tail_y, tail_rest (lay a flat tail down)"""
    R = V((-(P.z - c["centre_z"]), P.y, P.x + c["rest_r"]))
    wl = min(1.0, max(0.0, (c["belly_z"] - P.z) / c["leg_span"])); wl = wl * wl * (3 - 2 * wl)
    R.z = R.z * (1 - 0.75 * wl) + 0.02 * wl
    if P.y < c["front_y"]:
        wh = min(1.0, max(0.0, (P.z - c["neck_z"]) / c["neck_span"])); wh = wh * wh * (3 - 2 * wh)
        R.z -= c["head_drop"] * wh
    if "tail_y" in c and P.y > c["tail_y"]:
        # a flat tail (a bird's) would stand up like a fan after the roll: lay it back down towards the ground
        w = min(1.0, (P.y - c["tail_y"]) / 0.06)
        R.z = R.z + (c["tail_rest"] + (R.z - c["tail_rest"]) * 0.28 - R.z) * w
    R.z = max(R.z, 0.004)
    return R

def build_dead(pairs, c):
    """pairs: [(standing object name, carcass object name)]; the carcass copies keep the standing UVs and materials"""
    for src, dst in pairs:
        if dst in bpy.data.objects: bpy.data.objects.remove(bpy.data.objects[dst])
    made = []
    for src, dst in pairs:
        me = bpy.data.objects[src].data.copy(); me.name = dst
        o = bpy.data.objects.new(dst, me); bpy.context.collection.objects.link(o)
        for v in me.vertices: v.co = dead_point(v.co.copy(), c)
        me.update(); made.append(o)
    zmin = min(v.co.z for v in made[0].data.vertices)
    for o in made:
        for v in o.data.vertices: v.co.z = max(0.0, v.co.z - zmin)
        o.data.update()
    return made

# ---- coat region helpers (shader math) ----
def M(t, L, op, a, b=None, c=None):
    n = t.nodes.new("ShaderNodeMath"); n.operation = op
    for i, v in enumerate((a, b, c)):
        if v is None: continue
        if isinstance(v, (int, float)): n.inputs[i].default_value = v
        else: L(v, n.inputs[i])
    return n.outputs[0]

def clamp01(t, L, x): return M(t, L, 'MAXIMUM', M(t, L, 'MINIMUM', x, 1.0), 0.0)

def band(t, L, x, lo, hi, soft):
    """1 inside [lo, hi], fading over `soft` either side, 0 outside (clamped: two regions multiplied never turn on outside)"""
    a = M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', x, lo - soft), 1.0 / soft)
    b = M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', hi + soft, x), 1.0 / soft)
    return clamp01(t, L, M(t, L, 'MINIMUM', a, b))

def above(t, L, x, edge, soft): return clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', x, edge), 1.0 / soft))

def mixc(t, L, fac, a, b):
    m = t.nodes.new("ShaderNodeMix"); m.data_type = 'RGBA'
    if isinstance(fac, (int, float)): m.inputs[0].default_value = fac
    else: L(fac, m.inputs[0])
    for i, v in ((6, a), (7, b)):
        if isinstance(v, tuple): m.inputs[i].default_value = v
        else: L(v, m.inputs[i])
    return m.outputs[2]

def pos_normal(t, L, tc):
    sp = t.nodes.new("ShaderNodeSeparateXYZ"); L(tc.outputs["Object"], sp.inputs[0])
    geo = t.nodes.new("ShaderNodeNewGeometry"); sn = t.nodes.new("ShaderNodeSeparateXYZ"); L(geo.outputs["Normal"], sn.inputs[0])
    return sp.outputs, sn.outputs

def export_animal(species, alive, dead, vitals_at, d):
    """alive / dead: {export name: object}, or {group: {export name: object}} for a species drawn by sex (groups "Male" and "Female";
    the carcass groups become Dead_Male and Dead_Female). Writes animal_<species>_model.fbx with Pose_Alive, Pose_Dead and Vitals."""
    import os
    def empty(name, loc=(0, 0, 0), parent=None):
        e = bpy.data.objects.get(name) or bpy.data.objects.new(name, None)
        if e.name not in bpy.context.collection.objects: bpy.context.collection.objects.link(e)
        e.location = loc; e.empty_display_size = 0.1; e.parent = parent; return e
    pa, pd, vt = empty("Pose_Alive"), empty("Pose_Dead"), empty("Vitals", vitals_at)
    keep = {}; empties = [pa, pd, vt]
    for group, pose, prefix in ((alive, pa, ""), (dead, pd, "Dead_")):
        nested = all(isinstance(v, dict) for v in group.values())
        sets = [(empty(prefix + g, parent=pose), parts) for g, parts in group.items()] if nested else [(pose, group)]
        for holder, parts in sets:
            if holder is not pose: empties.append(holder)
            for export_name, o in parts.items():
                o.parent = holder; o.location = (0, 0, 0); keep[o] = o.name; o.name = export_name
    bpy.ops.object.select_all(action='DESELECT')
    for o in empties + list(keep.keys()): o.select_set(True)
    fbx = os.path.join(d, f"animal_{species}_model.fbx")
    bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True, object_types={'EMPTY', 'MESH'}, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
                             bake_space_transform=True, mesh_smooth_type='FACE', add_leaf_bones=False, path_mode='STRIP', embed_textures=False)
    for o, n in keep.items(): o.name = n
    return os.path.getsize(fbx)
