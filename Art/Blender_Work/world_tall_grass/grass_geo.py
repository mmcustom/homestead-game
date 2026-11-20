
# Tall grass clump (Wave 6, 2026-11-19): GrassPlacer's "Tall Grass Clump", the meadow grass cut for Cordage.
# Same size as the generated clump it replaces (0.8-1.3 m tall, blades rising from a 0.25 m tuft and arching out).
# Thin curved ribbon blades, folded down the middle in the normal map, and a few seed-head stems. Drawn double-sided
# (the material), so each blade is one ribbon. The texture is painted here in code, not baked: a strip per blade colour
# (base green to straw tips; some blades greener, some drier) and a strip for the seed heads.
import bpy, bmesh, math, random
import numpy as np
from mathutils import Vector, noise
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
V = Vector
STRIPS = 14            # blade colour strips across u 0 .. 0.875; seed heads use 0.875 .. 1
SEED_U = (0.885, 0.99)

def blade(bm, uvl, base, ang, h, lean, w0, strip, segs=6, droop=0.0):
    out = V((math.cos(ang), math.sin(ang), 0)); side = V((-math.sin(ang), math.cos(ang), 0))
    twist = random.uniform(-0.5, 0.5)
    u0 = strip / STRIPS * 0.875; du = 0.875 / STRIPS
    rows = []
    for k in range(segs + 1):
        t = k / segs
        p = base + out * (lean * t ** 1.7) + V((0, 0, h * t - droop * t ** 3))
        w = w0 * (1 - t) ** 0.55 + 0.0008
        sd = (side * math.cos(twist * t) + out * math.sin(twist * t)) * w
        a = bm.verts.new(p - sd); b = bm.verts.new(p + sd)
        rows.append((a, b, t))
    for (a0, b0, t0), (a1, b1, t1) in zip(rows[:-1], rows[1:]):
        f = bm.faces.new([a0, b0, b1, a1])
        for loop in f.loops:
            v = loop.vert; t = t0 if v in (a0, b0) else t1
            loop[uvl].uv = (u0 + (du * 0.08 if v in (a0, a1) else du * 0.92), 0.02 + t * 0.96)
    return rows[-1]

def seed_head(bm, uvl, tip, direction, length):
    """a slim spindle of seeds at the top of a stem"""
    d = direction.normalized(); up = V((0, 0, 1)) if abs(d.z) < 0.9 else V((1, 0, 0))
    a = d.cross(up).normalized(); b = d.cross(a).normalized()
    n = 6; rings = []
    for k in range(5):
        t = k / 4; r = 0.011 * math.sin(math.pi * min(1, 0.15 + t * 0.85)) + 0.0015
        c = tip + d * (length * t)
        rings.append([bm.verts.new(c + (a * math.cos(2 * math.pi * i / n) + b * math.sin(2 * math.pi * i / n)) * r) for i in range(n)])
    for k, (ra, rb) in enumerate(zip(rings[:-1], rings[1:])):
        for i in range(n):
            j = (i + 1) % n
            f = bm.faces.new([ra[i], ra[j], rb[j], rb[i]])
            for loop in f.loops:
                kk = k if loop.vert in ra else k + 1; ii = ra.index(loop.vert) if loop.vert in ra else rb.index(loop.vert)
                if loop.vert in (ra[j], rb[j]) and j == 0: ii = n
                loop[uvl].uv = (SEED_U[0] + (SEED_U[1] - SEED_U[0]) * ii / n, 0.05 + 0.9 * kk / 4)

def build(seed=20261119):
    random.seed(seed)
    for ob in list(bpy.data.objects):
        if ob.name == "Tall_Grass_Clump": bpy.data.objects.remove(ob)
    bm = bmesh.new(); uvl = bm.loops.layers.uv.new("UVMap")
    for i in range(70):
        ang = random.uniform(0, 2 * math.pi); r = random.uniform(0, 0.2) ** 0.8 * 0.25 / 0.2 ** 0.8
        base = V((math.cos(ang) * r, math.sin(ang) * r, 0))
        h = random.uniform(0.75, 1.25); lean = random.uniform(0.12, 0.42) * (0.6 + r * 2)
        blade(bm, uvl, base, ang + random.uniform(-0.5, 0.5), h, lean, random.uniform(0.01, 0.016), random.randrange(STRIPS), droop=random.uniform(0.0, 0.18) * h)
    for i in range(9):   # seed-head stems: thin, straighter, a little taller
        ang = random.uniform(0, 2 * math.pi); r = random.uniform(0, 0.12)
        base = V((math.cos(ang) * r, math.sin(ang) * r, 0))
        h = random.uniform(1.05, 1.3); lean = random.uniform(0.08, 0.22)
        a, b, t = blade(bm, uvl, base, ang, h, lean, 0.003, random.randrange(STRIPS), segs=5, droop=0.02)
        tip = (a.co + b.co) * 0.5
        seed_head(bm, uvl, tip, V((math.cos(ang) * lean * 0.6, math.sin(ang) * lean * 0.6, 1.0)), random.uniform(0.08, 0.12))
    o = link("Tall_Grass_Clump", bm)
    zs = [v.co.z for v in o.data.vertices]; xs = [v.co.x for v in o.data.vertices]
    return {"tris": sum(len(p.vertices) - 2 for p in o.data.polygons), "z": [round(min(zs), 2), round(max(zs), 2)], "x": [round(min(xs), 2), round(max(xs), 2)]}

def paint_textures(W=256, H=1024):
    """basecolor, normal (the blade's centre fold) and mask, written straight to textures/"""
    import os
    d = os.path.join(os.path.dirname(bpy.data.filepath), "textures")
    rng = np.random.default_rng(7)
    u = (np.arange(W) + 0.5) / W; v = (np.arange(H) + 0.5) / H
    U, Vv = np.meshgrid(u, v)
    base_g = np.array([0.10, 0.17, 0.05]); mid = np.array([0.30, 0.34, 0.12]); straw = np.array([0.52, 0.45, 0.25])
    col = np.zeros((H, W, 3))
    t = Vv[..., None]
    grad = np.where(t < 0.55, base_g + (mid - base_g) * (t / 0.55), mid + (straw - mid) * ((t - 0.55) / 0.45))
    strip = np.minimum((U / 0.875 * STRIPS).astype(int), STRIPS - 1)
    tint = rng.uniform(-1, 1, (STRIPS, 3)) * np.array([0.05, 0.05, 0.03])
    dry = rng.uniform(0, 1, STRIPS) ** 2                     # some blades drier all the way down
    col = grad + tint[strip]
    col = col + (straw - col) * (dry[strip][..., None] * 0.55 * (0.3 + 0.7 * t))
    streak = rng.normal(0, 1, W); streak = np.convolve(streak, np.ones(3) / 3, mode="same") * 0.035
    col = col * (1 + streak[None, :, None])
    local = (U / (0.875 / STRIPS)) % 1.0
    col = col * (1 + 0.12 * np.exp(-((local - 0.5) / 0.08) ** 2))[..., None]   # a paler midrib
    seed = U >= 0.875
    sc = np.array([0.36, 0.27, 0.15]) + rng.normal(0, 0.04, (H, W, 1)) * np.array([1, 1, 0.6])
    col = np.where(seed[..., None], sc * (0.8 + 0.2 * np.sin(Vv * 140)[..., None]), col)
    col = np.clip(col, 0, 1)
    # normal: tilt across the blade either side of the fold
    nx = np.where(seed, 0.0, (local - 0.5) * 0.7); nz = np.sqrt(1 - nx ** 2)
    nrm = np.dstack([nx * 0.5 + 0.5, np.full_like(nx, 0.5), nz * 0.5 + 0.5])
    mask = np.dstack([np.zeros_like(nx), np.ones_like(nx), np.zeros_like(nx), np.where(seed, 0.12, 0.3)])
    out = {}
    for name, arr, cs in (("basecolor", col, 'sRGB'), ("normal", nrm, 'Non-Color'), ("mask", mask, 'Non-Color')):
        n = "world_tall_grass_" + name
        if n in bpy.data.images: bpy.data.images.remove(bpy.data.images[n])
        im = bpy.data.images.new(n, W, H, alpha=(name == "mask")); im.colorspace_settings.name = cs
        rgba = arr if arr.shape[2] == 4 else np.dstack([arr, np.ones((H, W))])
        if name == "basecolor": rgba = np.dstack([np.power(arr, 1 / 2.2), np.ones((H, W))])   # stored sRGB
        if name == "mask": im.alpha_mode = 'STRAIGHT'
        im.pixels.foreach_set(rgba.astype(np.float32).ravel())
        im.filepath_raw = os.path.join(d, n + ".png"); im.file_format = 'PNG'; im.save(); out[name] = im.filepath_raw
    return out

def review_mat():
    m = bpy.data.materials.get("world_tall_grass") or bpy.data.materials.new("world_tall_grass")
    m.use_nodes = True; t = m.node_tree
    for n in list(t.nodes): t.nodes.remove(n)
    out = t.nodes.new("ShaderNodeOutputMaterial"); b = t.nodes.new("ShaderNodeBsdfPrincipled"); t.links.new(b.outputs[0], out.inputs[0])
    tb = t.nodes.new("ShaderNodeTexImage"); tb.image = bpy.data.images["world_tall_grass_basecolor"]; t.links.new(tb.outputs[0], b.inputs["Base Color"])
    tn = t.nodes.new("ShaderNodeTexImage"); tn.image = bpy.data.images["world_tall_grass_normal"]
    nm = t.nodes.new("ShaderNodeNormalMap"); t.links.new(tn.outputs[0], nm.inputs["Color"]); t.links.new(nm.outputs[0], b.inputs["Normal"])
    b.inputs["Roughness"].default_value = 0.7
    b.inputs["Subsurface Weight"].default_value = 0.0
    m.use_backface_culling = False
    return m
