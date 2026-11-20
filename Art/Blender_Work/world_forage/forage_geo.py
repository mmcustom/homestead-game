
# Homestead world_forage (Wave 6, 2026-11-19): one simple shape per forage crop. Each mesh fits the box of the Unity
# primitive ForagePlacer used before, so the placer's scales still apply: "sphere" shapes fit [-0.5, 0.5] on every axis,
# "cylinder" shapes fit x,z in [-0.5, 0.5] and height [-1, 1] (Blender Z, Unity Y).
import bpy, bmesh, math, random
from mathutils import Vector, Matrix, noise

def new_bm(): return bmesh.new()

def sphere(bm, center, r, segs=10, rings=7, sx=1, sy=1, sz=1):
    m = Matrix.Translation(center) @ Matrix.Diagonal((sx * r, sy * r, sz * r, 1))
    bmesh.ops.create_uvsphere(bm, u_segments=segs, v_segments=rings, radius=1.0, matrix=m)

def cone(bm, base, tip, r, segs=5):
    d = (tip - base); L = d.length
    rot = d.to_track_quat('Z', 'Y').to_matrix().to_4x4()
    m = Matrix.Translation(base + d * 0.5) @ rot
    bmesh.ops.create_cone(bm, cap_ends=False, segments=segs, radius1=r, radius2=0.0, depth=L, matrix=m)

def fit(bm, kind):
    """Scale and centre the mesh into the primitive's box (sphere: unit cube; cylinder: x,z unit, z height 2)."""
    vs = [v.co for v in bm.verts]
    lo = Vector((min(v.x for v in vs), min(v.y for v in vs), min(v.z for v in vs)))
    hi = Vector((max(v.x for v in vs), max(v.y for v in vs), max(v.z for v in vs)))
    c = (lo + hi) / 2; size = hi - lo
    target = Vector((1, 1, 1)) if kind == "sphere" else Vector((1, 1, 2))
    for v in bm.verts:
        q = v.co - c
        v.co = Vector((q.x / size.x * target.x, q.y / size.y * target.y, q.z / size.z * target.z))

def finish(name, bm, kind, smooth=True):
    fit(bm, kind)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = smooth
    ob = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(ob)
    return ob

def drupe_cluster(name, seed):
    r = random.Random(seed); bm = new_bm()
    sphere(bm, Vector((0, 0, 0)), 0.3, 8, 6)
    golden = math.pi * (3 - math.sqrt(5))
    n = 26
    for i in range(n):
        z = 1 - 2 * (i + 0.5) / n; rad = math.sqrt(1 - z * z); a = i * golden
        p = Vector((math.cos(a) * rad, math.sin(a) * rad, z * 1.1)) * 0.33
        sphere(bm, p, 0.11 * r.uniform(0.9, 1.1), 6, 4)
    return finish(name, bm, "sphere")

def apple():
    bm = new_bm(); sphere(bm, Vector(), 1.0, 18, 12)
    for v in bm.verts:
        x, y, z = v.co; rr = x * x + y * y
        v.co.z = z * 0.88 - 0.22 * math.exp(-rr / 0.06) * (1 if z > 0 else -0.6)    # dimples top and bottom
        v.co.x *= 1 + 0.04 * noise.noise(v.co * 2); v.co.y *= 1 + 0.04 * noise.noise(v.co * 2 + Vector((5, 0, 0)))
    cone(bm, Vector((0, 0, 0.55)), Vector((0.05, 0.02, 1.05)), 0.05, 4)
    return finish("wild_apples", bm, "sphere")

def pawpaw():
    bm = new_bm(); sphere(bm, Vector(), 1.0, 14, 10)
    for v in bm.verts:
        x, y, z = v.co
        v.co.x += 0.18 * (1 - z * z)                     # a gentle kidney curve
        v.co *= 1 + 0.05 * noise.noise(v.co * 1.5)
    return finish("pawpaw", bm, "sphere")

def hickory():
    bm = new_bm(); sphere(bm, Vector(), 1.0, 16, 10)
    for v in bm.verts:
        x, y, z = v.co; a = math.atan2(y, x)
        groove = 1 - 0.1 * max(0.0, math.cos(4 * a)) ** 8   # four husk seams
        tip = 1 + 0.18 * max(0.0, z) ** 3                   # a little point on top
        v.co.x *= groove; v.co.y *= groove; v.co.z *= tip
    return finish("hickory_nuts", bm, "sphere")

def walnut():
    bm = new_bm(); sphere(bm, Vector(), 1.0, 14, 10)
    for v in bm.verts: v.co *= 1 + 0.06 * noise.noise(v.co * 3.0)
    return finish("walnuts", bm, "sphere")

def burr():
    r = random.Random(7); bm = new_bm(); sphere(bm, Vector(), 0.62, 10, 7)
    golden = math.pi * (3 - math.sqrt(5)); n = 70
    for i in range(n):
        z = 1 - 2 * (i + 0.5) / n; rad = math.sqrt(1 - z * z); a = i * golden
        d = Vector((math.cos(a) * rad, math.sin(a) * rad, z))
        cone(bm, d * 0.55, d * r.uniform(0.95, 1.08) + Vector((r.uniform(-.05, .05), r.uniform(-.05, .05), 0)), 0.045, 3)
    return finish("chestnuts", bm, "sphere")

def dandelion_head():
    bm = new_bm(); sphere(bm, Vector((0, 0, 0.05)), 0.22, 10, 5, sz=0.5)
    for ring, (count, rad, tilt) in enumerate(((22, 0.5, 0.25), (16, 0.36, 0.5), (10, 0.22, 0.8))):
        for i in range(count):
            a = (i + ring * 0.5) / count * 2 * math.pi
            d = Vector((math.cos(a), math.sin(a), 0))
            base = d * 0.12 + Vector((0, 0, 0.05))
            tip = d * rad + Vector((0, 0, 0.05 + tilt * rad * 0.6))
            side = Vector((-d.y, d.x, 0)) * 0.035
            v = [bm.verts.new(base - side), bm.verts.new(base + side), bm.verts.new(tip + side * 0.6), bm.verts.new(tip - side * 0.6)]
            bm.faces.new(v)
    return finish("dandelion", bm, "sphere", smooth=False)

def onion_umbel():
    r = random.Random(3); bm = new_bm()
    golden = math.pi * (3 - math.sqrt(5)); n = 14
    for i in range(n):
        z = 1 - (i + 0.5) / n; rad = math.sqrt(1 - z * z); a = i * golden
        p = Vector((math.cos(a) * rad, math.sin(a) * rad, z)) * 0.4
        sphere(bm, p, 0.12, 6, 4, sz=1.3)
    return finish("wild_onion", bm, "sphere")

def morel():
    bm = new_bm(); sphere(bm, Vector(), 1.0, 32, 24)
    for v in bm.verts:
        x, y, z = v.co
        taper = 0.55 + 0.45 * (1 - (z + 1) / 2) ** 0.7          # a cone: wide at the bottom, pointed on top
        d = noise.voronoi(Vector((x * 3.0, y * 3.0, z * 1.6)), distance_metric='DISTANCE')[0]
        pit = 0.8 + 0.3 * max(0.0, 1 - (d[1] - d[0]) * 4) ** 0.5       # raised ridges at the cell borders, pits between
        v.co.x *= taper * pit; v.co.y *= taper * pit
        v.co.z = z * 1.0
    return finish("morel", bm, "sphere")

def shelf(name, seed, wavy):
    r = random.Random(seed); bm = new_bm()
    segs, rings = 18, 5
    top, bot = [], []
    for j in range(rings + 1):
        rr = j / rings
        rowt, rowb = [], []
        for i in range(segs + 1):
            a = -math.pi * 0.85 + i / segs * math.pi * 1.7
            edge = 1 + wavy * 0.12 * math.sin(a * 5 + seed) * rr
            x = math.cos(a) * rr * edge; y = math.sin(a) * rr * edge * 0.8 + 0.5
            z = 0.25 * rr * rr - 0.05                         # cupped up toward the rim
            th = 0.18 * (1 - rr) + 0.02
            rowt.append(bm.verts.new((x, y, z + th))); rowb.append(bm.verts.new((x, y, z - th * 0.4)))
        top.append(rowt); bot.append(rowb)
    for j in range(rings):
        for i in range(segs):
            bm.faces.new([top[j][i], top[j][i + 1], top[j + 1][i + 1], top[j + 1][i]])
            bm.faces.new([bot[j][i], bot[j + 1][i], bot[j + 1][i + 1], bot[j][i + 1]])
    for i in range(segs):
        bm.faces.new([top[rings][i], top[rings][i + 1], bot[rings][i + 1], bot[rings][i]])
    return finish(name, bm, "sphere")

def rosette():
    r = random.Random(11); bm = new_bm()
    for k in range(9):
        a = k / 9 * 2 * math.pi + r.uniform(-0.2, 0.2); d = Vector((math.cos(a), math.sin(a), 0)); side = Vector((-d.y, d.x, 0))
        L = r.uniform(0.75, 1.0); pts = 8; left, right = [], []
        for i in range(pts + 1):
            t = i / pts
            w = 0.16 * math.sin(math.pi * min(1, t * 1.1)) * (1 + 0.5 * (i % 2))     # toothed edge ("lion's teeth")
            c = d * (0.05 + t * L) + Vector((0, 0, 0.12 * t * (1 - t)))
            left.append(bm.verts.new(c + side * w)); right.append(bm.verts.new(c - side * w))
        for i in range(pts):
            bm.faces.new([left[i], left[i + 1], right[i + 1], right[i]])
    return finish("Dandelion Leaves", bm, "sphere", smooth=False)

def cattail_head():
    bm = new_bm(); bmesh.ops.create_cone(bm, cap_ends=True, segments=10, radius1=0.5, radius2=0.5, depth=1.6)
    sphere(bm, Vector((0, 0, 0.8)), 0.5, 10, 5, sz=0.4); sphere(bm, Vector((0, 0, -0.8)), 0.5, 10, 5, sz=0.4)
    bmesh.ops.create_cone(bm, cap_ends=False, segments=4, radius1=0.08, radius2=0.03, depth=0.6, matrix=Matrix.Translation((0, 0, 1.1)))
    return finish("cattail", bm, "cylinder")

def log():
    bm = new_bm(); segs, rings = 14, 10
    uv = bm.loops.layers.uv.new("UVMap")
    grid = []
    for j in range(rings + 1):
        z = -1 + 2 * j / rings; row = []
        for i in range(segs):
            a = i / segs * 2 * math.pi
            rr = 0.5 * (1 + 0.06 * noise.noise(Vector((math.cos(a), math.sin(a), z * 2))))
            row.append(bm.verts.new((math.cos(a) * rr, math.sin(a) * rr, z)))
        grid.append(row)
    for j in range(rings):
        for i in range(segs):
            k = (i + 1) % segs
            f = bm.faces.new([grid[j][i], grid[j][k], grid[j + 1][k], grid[j + 1][i]])
            u0, u1 = i / segs * 3, (i + 1) / segs * 3; v0, v1 = j / rings * 4, (j + 1) / rings * 4
            for l, c in zip(f.loops, ((u0, v0), (u1, v0), (u1, v1), (u0, v1))): l[uv].uv = c
    for row, sgn in ((grid[0], -1), (grid[-1], 1)):        # broken, slightly jagged ends
        c = bm.verts.new((0, 0, sgn * 0.97))
        for i in range(segs):
            row[i].co.z += sgn * 0.05 * noise.noise(row[i].co * 4)
            f = bm.faces.new([row[i], row[(i + 1) % segs], c])
            for l in f.loops: l[uv].uv = (0.5 + l.vert.co.x * 0.3, 0.5 + l.vert.co.y * 0.3)
    return finish("Fallen Log", bm, "cylinder")

def build_all():
    obs = [drupe_cluster("blackberries", 1), drupe_cluster("raspberries", 2), apple(), pawpaw(), hickory(), walnut(), burr(),
           dandelion_head(), onion_umbel(), morel(), shelf("dryads_saddle", 4, 1.0), shelf("oyster_mushroom", 5, 0.5),
           rosette(), cattail_head(), log()]
    for i, o in enumerate(obs): o.location = ((i % 5) * 1.6, (i // 5) * 2.5, 0)
    return obs
