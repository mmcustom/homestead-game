
# White-tailed deer (Wave 5, 2026-11-19): a skin-modifier body over a joint skeleton (one smooth piece), ears, eyes, and buck antlers.
# Real size: about 1.0 m at the shoulder, 1.75 m nose to tail. Front faces Blender -Y (Unity +Z). Ground at z = 0.
import bpy, bmesh, math
from mathutils import Vector, Matrix, Euler, noise
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
V = Vector

def skeleton():
    J = {}  # name: (position, radius)
    def j(n, p, r): J[n] = (V(p), r)
    j("tailtip", (0, 0.755, 0.69), 0.028); j("tailbase", (0, 0.69, 0.84), 0.08)
    j("rump", (0, 0.555, 0.875), 0.215); j("loin", (0, 0.26, 0.85), 0.205); j("mid", (0, -0.04, 0.83), 0.215)
    j("chest", (0, -0.30, 0.80), 0.215); j("withers", (0, -0.45, 0.86), 0.17)
    j("brisket", (0, -0.42, 0.63), 0.12); j("belly", (0, 0.02, 0.66), 0.14)
    j("neck0", (0, -0.56, 0.97), 0.13); j("neck1", (0, -0.635, 1.08), 0.11); j("neck2", (0, -0.70, 1.18), 0.092)
    j("skull", (0, -0.765, 1.255), 0.095); j("muzzle", (0, -0.885, 1.205), 0.062); j("nose", (0, -0.965, 1.17), 0.042)
    for s, side in ((-1, "L"), (1, "R")):
        j("shoulder" + side, (s * 0.12, -0.39, 0.70), 0.092); j("elbow" + side, (s * 0.12, -0.35, 0.52), 0.056)
        j("carpus" + side, (s * 0.11, -0.37, 0.30), 0.031); j("ffet" + side, (s * 0.11, -0.39, 0.09), 0.022); j("fhoof" + side, (s * 0.11, -0.42, 0.016), 0.021)
        j("hip" + side, (s * 0.115, 0.50, 0.76), 0.17); j("stifle" + side, (s * 0.125, 0.38, 0.56), 0.115)
        j("hock" + side, (s * 0.11, 0.61, 0.37), 0.036); j("hfet" + side, (s * 0.11, 0.57, 0.10), 0.022); j("hhoof" + side, (s * 0.11, 0.545, 0.016), 0.021)
    E = [("tailtip", "tailbase"), ("tailbase", "rump"), ("rump", "loin"), ("loin", "mid"), ("mid", "chest"), ("chest", "withers"),
         ("chest", "brisket"), ("mid", "belly"), ("withers", "neck0"), ("neck0", "neck1"), ("neck1", "neck2"), ("neck2", "skull"),
         ("skull", "muzzle"), ("muzzle", "nose")]
    for side in "LR":
        E += [("chest", "shoulder" + side), ("shoulder" + side, "elbow" + side), ("elbow" + side, "carpus" + side), ("carpus" + side, "ffet" + side),
              ("ffet" + side, "fhoof" + side), ("rump", "hip" + side), ("hip" + side, "stifle" + side), ("stifle" + side, "hock" + side),
              ("hock" + side, "hfet" + side), ("hfet" + side, "hhoof" + side)]
    return J, E

def skin_body(name, J, E, subsurf=2):
    names = list(J.keys())
    me = bpy.data.meshes.new(name)
    me.from_pydata([J[n][0] for n in names], [(names.index(a), names.index(b)) for a, b in E], [])
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    sk = o.modifiers.new("Skin", 'SKIN'); sk.branch_smoothing = 0.6; sk.use_smooth_shade = True
    for i, n in enumerate(names):
        r = J[n][1]; me.skin_vertices[0].data[i].radius = (r, r)
        me.skin_vertices[0].data[i].use_root = (n == "mid")
    ss = o.modifiers.new("Sub", 'SUBSURF'); ss.levels = subsurf; ss.render_levels = subsurf
    bpy.context.view_layer.objects.active = o
    for o2 in bpy.context.scene.objects: o2.select_set(False)
    o.select_set(True)
    bpy.ops.object.modifier_apply(modifier="Skin"); bpy.ops.object.modifier_apply(modifier="Sub")
    return o

def shape_body(o):
    """barrel narrower than deep; a slight muscle ripple"""
    for v in o.data.vertices:
        p = v.co
        if -0.55 < p.y < 0.72 and p.z > 0.5:
            k = 0.86 if abs(p.x) < 0.3 else 1.0
            p.x *= k
        p += p.normalized() * 0.0 + V((0, 0, 0))
        n = noise.noise(p * 9.0) * 0.004
        p.x += n * (1 if p.x >= 0 else -1)
    o.data.update()

def ears_eyes(head, fwd, up, side_axis):
    bm = bmesh.new()
    for s in (-1, 1):
        base = head + side_axis * (0.055 * s) + up * 0.062 - fwd * 0.005
        tip = base + side_axis * (0.085 * s) + up * 0.065 - fwd * 0.035
        mid = (base + tip) / 2
        d = (tip - base); L = d.length; d.normalize()
        a = side_axis * s; b = d.cross(a).normalized()
        t = bmesh.new(); bmesh.ops.create_uvsphere(t, u_segments=14, v_segments=10, radius=1.0)
        for v in t.verts:
            q = v.co
            cup = 0.6 * (q.x ** 2) if q.y < 0 else 0
            v.co = mid + d * (q.z * L * 0.55) + b * (q.x * 0.033 * (1 - 0.35 * max(0, q.z))) + a * ((q.y * 0.010) + cup * 0.012)
        me = bpy.data.meshes.new("t"); t.to_mesh(me); t.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)
    nb = len(bm.faces)
    for s in (-1, 1):
        t = bmesh.new(); bmesh.ops.create_uvsphere(t, u_segments=10, v_segments=8, radius=0.013)
        c = head + side_axis * (0.058 * s) + fwd * 0.045 + up * 0.012
        bmesh.ops.translate(t, verts=t.verts, vec=c)
        me = bpy.data.meshes.new("t"); t.to_mesh(me); t.free(); bm.from_mesh(me); bpy.data.meshes.remove(me)
    for i, f in enumerate(bm.faces): f.material_index = 1 if i >= nb else 0
    return bm

def antlers(skull, fwd, up, side_axis, name="Deer_Antlers"):
    bm = bmesh.new()
    for s in (-1, 1):
        A = lambda f, u, sd: skull + (fwd * f + up * u + side_axis * (sd * s)) * 0.82 + up * 0.012
        beam = [A(-0.03, 0.065, 0.035), A(-0.06, 0.13, 0.09), A(-0.07, 0.21, 0.165), A(-0.01, 0.27, 0.205), A(0.08, 0.30, 0.19), A(0.16, 0.30, 0.14), A(0.21, 0.28, 0.09)]
        radii = [0.017, 0.015, 0.013, 0.011, 0.009, 0.007, 0.004]
        tube_var(bm, beam, radii, sides=8)
        for (i0, h, lean, r0) in ((1, 0.07, 0.01, 0.007), (3, 0.15, -0.02, 0.008), (4, 0.16, 0.0, 0.007), (5, 0.11, 0.01, 0.006)):
            b0 = beam[i0]; tip = b0 + (up * h + fwd * lean + side_axis * (0.01 * s)) * 0.82
            mid = b0.lerp(tip, 0.5) + fwd * 0.012
            tube_var(bm, [b0, mid, tip], [r0, r0 * 0.7, r0 * 0.25], sides=6)
    o = link(name, bm); return o

def seat(body, part, base, up, sink):
    """move a part along `up` so its attachment point `base` sits `sink` into the body's real surface (cast down onto the smoothed mesh)"""
    from mathutils.bvhtree import BVHTree
    bvh = BVHTree.FromObject(body, bpy.context.evaluated_depsgraph_get())
    hit, nrm, idx, dist = bvh.ray_cast(base + up * 0.3, -up)
    if hit is None:
        return 0.0
    shift = (hit - base).dot(up) - sink
    for v in part.data.vertices: v.co += up * shift
    part.data.update()
    return shift

def build_standing():
    for ob in list(bpy.data.objects):
        if ob.name.startswith("Deer_"): bpy.data.objects.remove(ob)
    J, E = skeleton()
    body = skin_body("Deer_Body", J, E); shape_body(body)
    skull = J["skull"][0]; nose = J["nose"][0]
    # the smoothed head is smaller than its joint radius: put the skull point just under the real top of the head
    top = max(v.co.z for v in body.data.vertices if abs(v.co.y - skull.y) < 0.03 and abs(v.co.x) < 0.03)
    skull = V((0, skull.y, top - 0.055))
    fwd = (nose - skull).normalized(); side_axis = V((1, 0, 0)); up = side_axis.cross(fwd).normalized()
    if up.z < 0: up = -up
    eo = link("Deer_Ears", ears_eyes(skull, fwd, up, side_axis))
    ao = antlers(skull, fwd, up, side_axis)
    seat(body, eo, skull + side_axis * 0.055 + up * 0.062, up, 0.012)
    seat(body, ao, skull + (fwd * -0.03 + up * 0.065 + side_axis * 0.035) * 0.82 + up * 0.012, up, 0.008)
    return {o.name: sum(len(p.vertices) - 2 for p in o.data.polygons) for o in (body, eo, ao)}


# ---- Dead pose (the carcass), the reusable way: deform the finished standing animal. Roll it onto its side, let the legs sag to the
# ground and lay the neck and head down. Every point moves by where it was on the standing animal, so body, ears and antlers stay together
# and nothing can tear. Per species: the belly height (where legs start), the neck-base height, the body's resting radius.
DEAD = {"belly_z": 0.62, "leg_span": 0.25, "neck_z": 0.98, "neck_span": 0.3, "front_y": -0.45, "rest_r": 0.2, "head_drop": 0.11}

def dead_point(P, c=DEAD):
    # roll -90 degrees about the long axis: up becomes -X, the animal's right side faces up
    R = V((-(P.z - 0.8), P.y, P.x + c["rest_r"]))
    wl = min(1.0, max(0.0, (c["belly_z"] - P.z) / c["leg_span"]))
    wl = wl * wl * (3 - 2 * wl)
    R.z = R.z * (1 - 0.75 * wl) + 0.02 * wl                     # legs sag until their tips rest on (or on top of) the ground
    if P.y < c["front_y"]:
        wh = min(1.0, max(0.0, (P.z - c["neck_z"]) / c["neck_span"]))
        wh = wh * wh * (3 - 2 * wh)
        R.z -= c["head_drop"] * wh                               # the neck bends down so the head lies on the ground
    R.z = max(R.z, 0.004)
    return R

def build_dead():
    for ob in list(bpy.data.objects):
        if ob.name.startswith("Dead_"): bpy.data.objects.remove(ob)
    made = []
    for src, dst in (("Deer_Body", "Dead_Body"), ("Deer_Ears", "Dead_Ears"), ("Deer_Antlers", "Dead_Antlers")):
        s = bpy.data.objects[src]
        me = s.data.copy(); me.name = dst
        o = bpy.data.objects.new(dst, me); bpy.context.collection.objects.link(o)
        for v in me.vertices: v.co = dead_point(v.co.copy())
        me.update(); made.append(o)
    zmin = min(v.co.z for v in made[0].data.vertices)
    for o in made:
        for v in o.data.vertices: v.co.z = max(0.0, v.co.z - zmin)
        o.data.update()
    return {o.name: sum(len(p.vertices) - 2 for p in o.data.polygons) for o in made}
