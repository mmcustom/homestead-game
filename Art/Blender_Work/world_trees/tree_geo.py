
# Homestead world_trees: procedural hardwood generator (skeleton -> 3 LOD meshes).
# Material slots: 0 bark, 1 leaves, 2 crown snow, 3 branch snow (SeasonalTrees).
# UV0: bark tiles / leaf atlas quadrant / foliage-mask texel for branch snow.
# UV1 ("FaceValue"): per-leaf-card random drop value in u (leaves + crown snow).
import bpy, bmesh, math, random
from mathutils import Vector, Matrix, noise

ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work\world_trees"
MASK = 64
BARK_TILE = 1.3   # metres of bark per texture repeat

def lerp(a, b, t): return a + (b - a) * t

class Br:
    def __init__(s, pts, rad, level):
        s.pts, s.rad, s.level = pts, rad, level
        s.len = [0.0]
        for i in range(1, len(pts)): s.len.append(s.len[-1] + (pts[i] - pts[i-1]).length)
    def at(s, t):
        L = s.len[-1] * max(0.0, min(1.0, t))
        for i in range(1, len(s.pts)):
            if s.len[i] >= L or i == len(s.pts) - 1:
                seg = s.len[i] - s.len[i-1]
                u = 0.0 if seg < 1e-6 else max(0.0, min(1.0, (L - s.len[i-1]) / seg))
                return s.pts[i-1].lerp(s.pts[i], u), lerp(s.rad[i-1], s.rad[i], u), (s.pts[i] - s.pts[i-1]).normalized()

def curve(start, d, length, r0, r1, segs, rng, tropism, wobble, taper=1.0, env=None):
    off = Vector((rng.uniform(0, 500), rng.uniform(0, 500), rng.uniform(0, 500)))
    pts = [start.copy()]; rad = [r0]; p = start.copy(); d = d.normalized(); step = length / segs
    for i in range(1, segs + 1):
        d = (d + noise.noise_vector(p * 0.6 + off) * wobble + Vector((0, 0, tropism))).normalized()
        p = p + d * step
        if env is not None and i > 1 and env(p) > 1.0: break
        t = i / segs
        pts.append(p.copy()); rad.append(r1 + (r0 - r1) * (1 - t) ** taper)
    if len(pts) < 2: pts.append(start + d * step); rad.append(r1)
    rad[-1] = r1
    return pts, rad

def branch_dir(parent_d, angle_deg, azimuth):
    perp = Matrix.Rotation(azimuth, 3, parent_d) @ parent_d.orthogonal().normalized()
    a = math.radians(angle_deg)
    return (parent_d * math.cos(a) + perp * math.sin(a)).normalized()

SPECIES = {
  "maple": dict(seed=11, trunk_h=3.2, trunk_r=0.34, leader_len=6.4, crown_c=Vector((0, 0, 8.1)), crown_r=Vector((4.5, 4.3, 4.4)),
                scaffolds=5, scaf_h=(2.8, 4.0), scaf_ang=(40, 58), scaf_len=(5.8, 7.0), scaf_r=(0.2, 0.035),
                l2_space=0.42, l2_len=2.5, l2_ang=(40, 65), l3_space=0.26, l3_len=(0.5, 0.85), card=(1.35, 1.8)),
  # tulip poplar (Hardwood Tall): long clear trunk, short limbs that shorten toward the top, a narrow high crown
  "poplar": dict(seed=23, trunk_h=6.8, trunk_r=0.34, leader_len=9.4, crown_c=Vector((0, 0, 11.8)), crown_r=Vector((3.5, 3.4, 5.2)),
                scaffolds=10, scaf_h=(6.4, 13.4), scaf_ang=(50, 70), scaf_len=(4.0, 4.8), scaf_r=(0.13, 0.03), scaf_taper=0.6,
                l2_space=0.32, l2_len=2.1, l2_ang=(45, 70), l3_space=0.24, l3_len=(0.45, 0.75), card=(1.4, 1.8)),
  # viburnum (Understory Shrub): several stems from the ground, a rounded 2.4 m mound, no trunk
  "viburnum": dict(seed=37, stems=7, trunk_h=0.0, crown_c=Vector((0, 0, 1.25)), crown_r=Vector((2.0, 1.7, 1.3)),
                scaf_ang=(12, 42), scaf_len=(1.9, 2.5), scaf_r=(0.035, 0.008),
                l2_space=0.22, l2_len=0.95, l2_ang=(40, 70), l3_space=0.16, l3_len=(0.25, 0.4), card=(0.7, 0.95), twig_r=0.005,
                sides={0: [5, 4, 3], 1: [5, 4, 3], 2: [4, 3, 0], 3: [3, 0, 0]}),
}

def skeleton(S):
    rng = random.Random(S["seed"])
    c, cr = S["crown_c"], S["crown_r"]
    def env(p):
        q = p - c
        return (q.x / cr.x) ** 2 + (q.y / cr.y) ** 2 + (q.z / cr.z) ** 2
    up = Vector((0, 0, 1))
    golden = math.radians(137.5)
    if S.get("stems"):   # a shrub: stems straight from the ground, no trunk
        az0 = rng.uniform(0, 6.28)
        out = []
        for k in range(S["stems"]):
            base = Vector((rng.uniform(-0.15, 0.15), rng.uniform(-0.15, 0.15), -0.1))
            dd = branch_dir(up, rng.uniform(*S["scaf_ang"]), az0 + k * golden + rng.uniform(-0.3, 0.3))
            pts, rad = curve(base, dd, rng.uniform(*S["scaf_len"]), S["scaf_r"][0], S["scaf_r"][1], 7, rng, 0.06, 0.2)
            out.append(Br(pts, rad, 1))
        n = 0
    else:
        pts, rad = curve(Vector((0, 0, -0.3)), up, S["trunk_h"] + 0.3 + S["leader_len"], S["trunk_r"], 0.035, 16, rng, 0.0, 0.06, taper=1.6)
        trunk = Br(pts, rad, 0)
        out = [trunk]
        az0 = rng.uniform(0, 6.28)   # drawn after the trunk, as before (keeps the approved trees unchanged)
        n = S["scaffolds"]
    for k in range(n):
        h = lerp(S["scaf_h"][0], S["scaf_h"][1], k / (n - 1)) + rng.uniform(-0.15, 0.15)
        p, r, d = trunk.at((h + 0.3) / trunk.len[-1])
        dd = branch_dir(up, rng.uniform(*S["scaf_ang"]), az0 + k * golden + rng.uniform(-0.3, 0.3))
        L = rng.uniform(*S["scaf_len"]) * (1 - S.get("scaf_taper", 0.0) * k / (n - 1))
        pts, rad = curve(p - dd * r * 0.4, dd, L, min(r * 0.72, S["scaf_r"][0]), S["scaf_r"][1], 9, rng, 0.07, 0.14, taper=1.2)
        out.append(Br(pts, rad, 1))
    for parent in list(out):
        t0 = 0.3 if parent.level == 1 else (S["trunk_h"] + 0.9) / parent.len[-1]
        count = max(1, int(parent.len[-1] * (0.97 - t0) / S["l2_space"]))
        az = rng.uniform(0, 6.28)
        for i in range(count):
            t = t0 + (i + rng.uniform(0.2, 0.8)) / count * (0.97 - t0)
            p, r, d = parent.at(t)
            az += golden + rng.uniform(-0.4, 0.4)
            dd = branch_dir(d, rng.uniform(*S["l2_ang"]), az)
            L = S["l2_len"] * (1.1 - 0.6 * t) * rng.uniform(0.75, 1.15)
            pts, rad = curve(p - dd * r * 0.4, dd, L, min(r * 0.55, 0.06), S.get("twig_r", 0.012), 5, rng, 0.05, 0.28, env=env)
            out.append(Br(pts, rad, 2))
    for parent in [b for b in out if b.level == 2]:
        count = max(1, int(parent.len[-1] * 0.75 / S["l3_space"]))
        az = rng.uniform(0, 6.28)
        for i in range(count):
            t = 0.25 + (i + rng.uniform(0.2, 0.8)) / count * 0.75
            p, r, d = parent.at(t)
            az += golden
            dd = branch_dir(d, rng.uniform(35, 60), az)
            pts, rad = curve(p, dd, rng.uniform(*S["l3_len"]), min(r * 0.6, 0.018), 0.005, 2, rng, 0.03, 0.3)
            out.append(Br(pts, rad, 3))
    return out

def cards_for(S, branches, lod):
    rng = random.Random(S["seed"] + 700 + lod)
    c = S["crown_c"]
    res = []
    def add(pos, size):
        q = pos - c
        outward = Vector((q.x, q.y, q.z * 0.6)).normalized()
        rnd = Vector((rng.uniform(-1, 1), rng.uniform(-1, 1), rng.uniform(-1, 1)))
        nrm = (outward * 0.55 + Vector((0, 0, 0.35)) + rnd * 0.45).normalized()
        res.append((pos + outward * 0.1, nrm, size * rng.uniform(0.9, 1.1), rng.uniform(0, 6.28), rng.randrange(4), rng.uniform(0.02, 0.98)))
    lo, hi = S["card"]
    far = hi / 1.8   # the middle and far levels' big clusters scale with the species' leaf clusters
    for b in branches:
        if lod == 0:
            if b.level == 3:
                p, r, d = b.at(1.0); add(p + d * 0.15, rng.uniform(lo, hi))
                add(b.at(0.45)[0], rng.uniform(lo, hi) * 0.85)
            elif b.level == 2:
                p, r, d = b.at(1.0); add(p + d * 0.2, hi); add(b.at(0.8)[0], lo)
                add(b.at(0.45)[0], lo); add(b.at(0.2)[0] - d * 0.1, lo * 0.9)
        elif lod == 1 and b.level == 2:
            k = max(2, round(b.len[-1] / 0.55))
            for i in range(k): add(b.at(0.2 + 0.8 * (i + 0.5) / k)[0], rng.uniform(2.2, 2.7) * far)
        elif lod == 2 and b.level == 2:
            add(b.at(0.7)[0], rng.uniform(3.3, 3.9) * far); add(b.at(0.3)[0], rng.uniform(2.6, 3.0) * far)
    return res

def build(name, S, branches, lod):
    sides = {0: [10, 7, 5][lod], 1: [8, 6, 4][lod], 2: [5, 4, 0][lod], 3: [3, 0, 0][lod]}
    if "sides" in S: sides = {k: v[lod] for k, v in S["sides"].items()}
    rng = random.Random(S["seed"] + 50 + lod)
    bm = bmesh.new()
    uv0 = bm.loops.layers.uv.new("UVMap"); uv1 = bm.loops.layers.uv.new("FaceValue")
    loopn = []
    def face(vs, mat, uvs, fv, ns):
        f = bm.faces.new([bm.verts.new(v) for v in vs]); f.material_index = mat
        for l, uv, nn in zip(f.loops, uvs, ns): l[uv0].uv = uv; l[uv1].uv = (fv, fv); loopn.append(nn)
        return f
    def mask_uv():
        u = ((rng.randrange(MASK) + 0.5) / MASK, (rng.randrange(MASK) + 0.5) / MASK); return [u] * 4
    # Unity orders the submeshes by each material's first face, so faces go in slot order:
    # bark, then leaves (each followed by its crown snow), then the branch snow last.
    branch_snow = []
    for b in branches:
        n = sides[b.level]
        if n == 0: continue
        if lod >= 1 and b.level >= 1 and len(b.pts) > 3:
            # far away, a limb needs half the rings to keep its curve
            keep = list(range(0, len(b.pts) - 1, 2)) + [len(b.pts) - 1]
            b = Br([b.pts[i] for i in keep], [b.rad[i] for i in keep], b.level)
        rings = []; ns = []
        tan = [(b.pts[min(i + 1, len(b.pts) - 1)] - b.pts[max(i - 1, 0)]).normalized() for i in range(len(b.pts))]
        x = tan[0].orthogonal().normalized()
        for i, p in enumerate(b.pts):
            t = tan[i]; x = (x - t * x.dot(t)).normalized(); y = t.cross(x)
            ring = []; rn = []
            for j in range(n):
                a = 6.28318 * j / n; dirv = x * math.cos(a) + y * math.sin(a)
                r = b.rad[i]
                if b.level == 0:
                    flare = max(0.0, 1.0 - (p.z + 0.3) / 1.0) ** 2
                    r *= 1.0 + flare * (0.45 + 0.25 * math.cos(3 * a + 0.7))
                r *= 1.0 + 0.06 * noise.noise(p * 3.0 + dirv)
                ring.append(p + dirv * r); rn.append(dirv)
            rings.append(ring); ns.append(rn)
        U = max(1, round(6.28318 * b.rad[0] / BARK_TILE))
        for i in range(len(rings) - 1):
            v0, v1 = b.len[i] / BARK_TILE, b.len[i + 1] / BARK_TILE
            for j in range(n):
                k = (j + 1) % n
                vs = [rings[i][j], rings[i][k], rings[i + 1][k], rings[i + 1][j]]
                uvs = [(U * j / n, v0), (U * (j + 1) / n, v0), (U * (j + 1) / n, v1), (U * j / n, v1)]
                nn = [ns[i][j], ns[i][k], ns[i + 1][k], ns[i + 1][j]]
                fn = (vs[1] - vs[0]).cross(vs[3] - vs[0])
                if fn.dot(sum(nn, Vector()) ) < 0: vs.reverse(); uvs.reverse(); nn.reverse(); fn = -fn
                face(vs, 0, uvs, 0.0, nn)
                fn.normalize()
                if b.level <= 2 and fn.z > 0.45:
                    lift = fn * 0.03
                    branch_snow.append(([v + lift for v in vs], mask_uv()))
    c, cr = S["crown_c"], S["crown_r"]
    for pos, nrm, size, rot, q, fv in cards_for(S, branches, lod):
        a = Matrix.Rotation(rot, 3, nrm) @ nrm.orthogonal().normalized(); bb = nrm.cross(a)
        h = size / 2
        vs = [pos - a * h - bb * h, pos + a * h - bb * h, pos + a * h + bb * h, pos - a * h + bb * h]
        ox, oy = (q % 2) * 0.5, (q // 2) * 0.5
        uvs = [(ox, oy), (ox + 0.5, oy), (ox + 0.5, oy + 0.5), (ox, oy + 0.5)]
        def soft(v):
            qv = v - c; rad = Vector((qv.x / cr.x, qv.y / cr.y, qv.z / cr.z)).normalized()
            return (rad * 0.7 + nrm * 0.3).normalized()
        nn = [soft(v) for v in vs]
        face(vs, 1, uvs, fv, nn)
        if nrm.z > 0.3:
            lift = Vector((0, 0, 0.05))
            face([v + lift for v in vs], 2, uvs, fv, [(x + Vector((0, 0, 0.6))).normalized() for x in nn])
    for vs, uvs in branch_snow:
        face(vs, 3, uvs, 0.0, [Vector((0, 0, 1))] * 4)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    me.normals_split_custom_set(loopn)
    for m in ("bark", "leaves", "snow_leaves", "snow"):
        mn = "world_tree_" + m if m.startswith("snow") else "world_tree_%s_%s" % (S["id"], m)
        me.materials.append(bpy.data.materials.get(mn) or bpy.data.materials.new(mn))
    ob = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(ob)
    return ob

def make(species):
    S = dict(SPECIES[species]); S["id"] = species
    br = skeleton(S)
    obs = [build("world_tree_%s_LOD%d" % (species, l), S, br, l) for l in range(3)]
    for i, ob in enumerate(obs): ob.location.x = i * 10.0 if False else 0
    return obs, br

def stats(ob):
    me = ob.data; per = [0, 0, 0, 0]
    for p in me.polygons: per[p.material_index] += len(p.vertices) - 2
    return per
