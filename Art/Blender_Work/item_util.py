
# Wave 4 item helpers (2026-11-16): geometry, procedural materials and a multi-material bake. exec() this file.
import bpy, bmesh, math, random, os
from mathutils import Vector, noise, Euler, Matrix
D = bpy.data

def link(name, bm, smooth=True):
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    me = D.meshes.new(name); bm.to_mesh(me); bm.free()
    o = D.objects.new(name, me); bpy.context.collection.objects.link(o)
    for p in me.polygons: p.use_smooth = smooth
    return o

def ground(o):
    zmin = min(v.co.z for v in o.data.vertices)
    for v in o.data.vertices: v.co.z -= zmin
    o.data.update()

def stick(bm, p0, p1, r0, r1, segs=10, sides=6, wob=0.012, sd=0.0, mat=0):
    d = (p1 - p0); Ln = d.length; d.normalize()
    up = Vector((0, 0, 1)) if abs(d.z) < 0.9 else Vector((1, 0, 0))
    a = d.cross(up).normalized(); b = d.cross(a).normalized()
    rings = []
    for k in range(segs + 1):
        t = k / segs
        c = p0 + d * (Ln * t) + a * (wob * noise.noise(Vector((t * 3, sd, 0)))) + b * (wob * noise.noise(Vector((t * 3, sd, 5))))
        r = r0 + (r1 - r0) * t
        rings.append([bm.verts.new(c + (a * math.cos(2 * math.pi * i / sides) + b * math.sin(2 * math.pi * i / sides)) * r * (1 + 0.08 * noise.noise(Vector((t * 9, i, sd))))) for i in range(sides)])
    for ra, rb in zip(rings[:-1], rings[1:]):
        for i in range(sides):
            j = (i + 1) % sides
            f = bm.faces.new([ra[i], ra[j], rb[j], rb[i]]); f.material_index = mat
    for ring, cc in ((rings[0], p0 - d * 0.002), (rings[-1], p1 + d * 0.002)):
        c = bm.verts.new(cc)
        for i in range(sides):
            f = bm.faces.new([ring[i], ring[(i + 1) % sides], c]); f.material_index = mat
    return rings

def box(bm, c, size, rot=(0, 0, 0), mat=0, jitter=0.0, sd=0.0):
    R = Euler(rot).to_matrix(); h = Vector(size) / 2; C = Vector(c)
    vs = {}
    for sx in (-1, 1):
        for sy in (-1, 1):
            for sz in (-1, 1):
                p = Vector((sx * h.x, sy * h.y, sz * h.z))
                if jitter: p += Vector((noise.noise(p * 40 + Vector((sd, 0, 0))), noise.noise(p * 40 + Vector((0, sd, 0))), noise.noise(p * 40 + Vector((0, 0, sd))))) * jitter
                vs[(sx, sy, sz)] = bm.verts.new(C + R @ p)
    quads = [((-1,-1,-1),(-1,1,-1),(1,1,-1),(1,-1,-1)), ((-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)), ((-1,-1,-1),(1,-1,-1),(1,-1,1),(-1,-1,1)),
             ((-1,1,-1),(-1,1,1),(1,1,1),(1,1,-1)), ((-1,-1,-1),(-1,-1,1),(-1,1,1),(-1,1,-1)), ((1,-1,-1),(1,1,-1),(1,1,1),(1,-1,1))]
    for q in quads:
        f = bm.faces.new([vs[k] for k in q]); f.material_index = mat

def heap(bm, R=0.11, H=0.042, nr=12, ns=40, sd=2.0):
    cv = bm.verts.new((0, 0, H)); rings = []
    for i in range(1, nr + 1):
        t = i / nr; ring = []
        for j in range(ns):
            th = 2 * math.pi * j / ns
            Rr = R * (1 + 0.18 * noise.noise(Vector((math.cos(th) * 1.5, math.sin(th) * 1.5, sd))))
            r = Rr * t
            h = H * (1 - t * t) ** 1.6 + 0.004 * noise.noise(Vector((r * math.cos(th) * 40, r * math.sin(th) * 40, sd + 5))) * (1 - t)
            if i == nr: h = -0.002
            ring.append(bm.verts.new((r * math.cos(th), r * math.sin(th), h)))
        rings.append(ring)
    for j in range(ns): bm.faces.new([cv, rings[0][j], rings[0][(j + 1) % ns]])
    for a, b in zip(rings[:-1], rings[1:]):
        for j in range(ns):
            k = (j + 1) % ns; bm.faces.new([a[j], b[j], b[k], a[k]])

def rock(bm, sz, seed, sub=4, k1=0.16, k2=0.05, flat=0.72):
    tmp = bmesh.new(); bmesh.ops.create_icosphere(tmp, subdivisions=sub, radius=1.0)
    for v in tmp.verts:
        p = v.co.copy()
        k = 1 + k1 * noise.noise(p * 1.6 + Vector((seed, seed * 0.7, 0.4))) + k2 * noise.noise(p * 5 + Vector((9, seed, 1))) + 0.015 * noise.noise(p * 14)
        q = Vector((p.x * sz[0], p.y * sz[1], p.z * sz[2])) * k
        q.z = max(q.z, -sz[2] * flat)
        v.co = q
    me = D.meshes.new("tmp"); tmp.to_mesh(me); tmp.free(); bm.from_mesh(me); D.meshes.remove(me)

def base(name):
    m = D.materials.get(name) or D.materials.new(name)
    m.use_nodes = True; t = m.node_tree
    for n in list(t.nodes): t.nodes.remove(n)
    out = t.nodes.new("ShaderNodeOutputMaterial"); b = t.nodes.new("ShaderNodeBsdfPrincipled"); b.name = "Principled BSDF"
    t.links.new(b.outputs[0], out.inputs[0])
    tc = t.nodes.new("ShaderNodeTexCoord")
    return m, t, b, tc, t.links.new

def ramp(t, L, src, c0, c1, p0=0.0, p1=1.0):
    r = t.nodes.new("ShaderNodeValToRGB"); e = r.color_ramp.elements
    e[0].color = c0; e[1].color = c1; e[0].position = p0; e[1].position = p1; L(src, r.inputs[0]); return r

def bark2(name, c0, c1, stretch=(2.5, 28, 28), furrow=0.035, depth=(0.05, 0.04, 0.03, 1), fine=110, bumpd=0.004):
    m, t, b, tc, L = base(name)
    def furrows(sc, loc, w):
        mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = sc; mp.inputs["Location"].default_value = loc; L(tc.outputs["Object"], mp.inputs[0])
        nf = t.nodes.new("ShaderNodeTexNoise"); nf.inputs["Scale"].default_value = 1.0; nf.inputs["Detail"].default_value = 6; nf.inputs["Distortion"].default_value = 0.4; L(mp.outputs[0], nf.inputs["Vector"])
        sub = t.nodes.new("ShaderNodeMath"); sub.operation = 'SUBTRACT'; sub.inputs[1].default_value = 0.5; L(nf.outputs["Fac"], sub.inputs[0])
        ab = t.nodes.new("ShaderNodeMath"); ab.operation = 'ABSOLUTE'; L(sub.outputs[0], ab.inputs[0])
        fr = t.nodes.new("ShaderNodeMapRange"); fr.inputs[2].default_value = w; L(ab.outputs[0], fr.inputs[0]); return fr
    f1 = furrows(stretch, (0, 0, 0), furrow); f2 = furrows((stretch[0] * 1.3, stretch[1] * 1.6, stretch[2] * 1.6), (3.1, 7.7, 1.9), furrow * 0.8)
    ridge = t.nodes.new("ShaderNodeMath"); ridge.operation = 'MINIMUM'; L(f1.outputs[0], ridge.inputs[0]); L(f2.outputs[0], ridge.inputs[1])
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = 12; n1.inputs["Detail"].default_value = 8; L(tc.outputs["Object"], n1.inputs["Vector"])
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = fine; n2.inputs["Detail"].default_value = 10; L(tc.outputs["Object"], n2.inputs["Vector"])
    col = ramp(t, L, n1.outputs["Fac"], c0, c1, 0.3, 0.75)
    ov = t.nodes.new("ShaderNodeMix"); ov.data_type = 'RGBA'; ov.blend_type = 'OVERLAY'; ov.inputs[0].default_value = 0.35; L(col.outputs[0], ov.inputs[6]); L(n2.outputs["Fac"], ov.inputs[7])
    mf = t.nodes.new("ShaderNodeMix"); mf.data_type = 'RGBA'; mf.inputs[6].default_value = depth; L(ridge.outputs[0], mf.inputs[0]); L(ov.outputs[2], mf.inputs[7])
    L(mf.outputs[2], b.inputs["Base Color"])
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = 0.8; rr.inputs[4].default_value = 0.95; L(n2.outputs["Fac"], rr.inputs[0]); L(rr.outputs[0], b.inputs["Roughness"])
    h = t.nodes.new("ShaderNodeMath"); h.operation = 'MULTIPLY_ADD'; h.inputs[1].default_value = 0.15; L(n2.outputs["Fac"], h.inputs[0]); L(ridge.outputs[0], h.inputs[2])
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.9; bp.inputs["Distance"].default_value = bumpd; L(h.outputs[0], bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = c1; return m

def grain(name, light, dark, stretch=(1.5, 60, 60), rough=(0.7, 0.88), bumpd=0.0012, lines=0.5):
    """sawn or split wood face: long grain streaks along X"""
    m, t, b, tc, L = base(name)
    mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = stretch; L(tc.outputs["Object"], mp.inputs[0])
    wv = t.nodes.new("ShaderNodeTexWave"); wv.wave_type = 'BANDS'; wv.bands_direction = 'Y'; wv.inputs["Scale"].default_value = 0.6; wv.inputs["Distortion"].default_value = 6; wv.inputs["Detail"].default_value = 3; L(mp.outputs[0], wv.inputs["Vector"])
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = 8; n1.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], n1.inputs["Vector"])
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = 140; n2.inputs["Detail"].default_value = 10; L(mp.outputs[0], n2.inputs["Vector"])
    mx = t.nodes.new("ShaderNodeMath"); mx.operation = 'MULTIPLY_ADD'; mx.inputs[1].default_value = lines; L(wv.outputs["Fac"], mx.inputs[0]); L(n1.outputs["Fac"], mx.inputs[2])
    col = ramp(t, L, mx.outputs[0], dark, light, 0.35, 0.95)
    ov = t.nodes.new("ShaderNodeMix"); ov.data_type = 'RGBA'; ov.blend_type = 'OVERLAY'; ov.inputs[0].default_value = 0.25; L(col.outputs[0], ov.inputs[6]); L(n2.outputs["Fac"], ov.inputs[7])
    L(ov.outputs[2], b.inputs["Base Color"])
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = rough[0]; rr.inputs[4].default_value = rough[1]; L(n2.outputs["Fac"], rr.inputs[0]); L(rr.outputs[0], b.inputs["Roughness"])
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.4; bp.inputs["Distance"].default_value = bumpd; L(n2.outputs["Fac"], bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = light; return m

def lump(name, c0, c1, rough=(0.85, 1.0), nscale=8, fine=70, bump=0.5, specks=None, grain_=False, metal=0.0, bumpd=0.003):
    m, t, b, tc, L = base(name)
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = nscale; n1.inputs["Detail"].default_value = 8; L(tc.outputs["Object"], n1.inputs["Vector"])
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = fine; n2.inputs["Detail"].default_value = 12; L(tc.outputs["Object"], n2.inputs["Vector"])
    col = ramp(t, L, n1.outputs["Fac"], c0, c1, 0.3, 0.7); last = col.outputs[0]
    if specks:  # (scale, colour, threshold)
        nz = t.nodes.new("ShaderNodeTexNoise"); nz.inputs["Scale"].default_value = specks[0]; nz.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], nz.inputs["Vector"])
        th = ramp(t, L, nz.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), specks[2], specks[2] + 0.08)
        mm = t.nodes.new("ShaderNodeMix"); mm.data_type = 'RGBA'; mm.inputs[7].default_value = specks[1]; L(th.outputs[0], mm.inputs[0]); L(last, mm.inputs[6]); last = mm.outputs[2]
    L(last, b.inputs["Base Color"]); b.inputs["Metallic"].default_value = metal
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = rough[0]; rr.inputs[4].default_value = rough[1]; L(n2.outputs["Fac"], rr.inputs[0]); L(rr.outputs[0], b.inputs["Roughness"])
    hsrc = n2.outputs["Fac"]
    if grain_:
        mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (2, 90, 90); L(tc.outputs["Object"], mp.inputs[0])
        vo = t.nodes.new("ShaderNodeTexVoronoi"); vo.feature = 'DISTANCE_TO_EDGE'; L(mp.outputs[0], vo.inputs["Vector"])
        gr = t.nodes.new("ShaderNodeMath"); gr.operation = 'MULTIPLY_ADD'; gr.inputs[1].default_value = 0.4; L(n2.outputs["Fac"], gr.inputs[0]); L(vo.outputs["Distance"], gr.inputs[2]); hsrc = gr.outputs[0]
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = bump; bp.inputs["Distance"].default_value = bumpd; L(hsrc, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = c1; return m

def assign(o, mats):
    """append materials without clearing (clearing resets every face to slot 0)"""
    while len(o.data.materials): o.data.materials.pop()
    for m in mats: o.data.materials.append(m)

def bake_all(objname, kind):
    import time
    s = bpy.context.scene; t0 = time.time()
    s.render.engine = 'CYCLES'; s.cycles.device = 'CPU'; s.render.bake.margin = 16
    ob = D.objects[objname]
    for o in s.objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active = ob
    tmp = []
    for sl in ob.material_slots:
        nt = sl.material.node_tree; node = nt.nodes["BAKE_TARGET"]; node.image = D.images[f"{sl.material.name}_{kind}"]
        for n in nt.nodes: n.select = False
        node.select = True; nt.nodes.active = node
        if kind in ("basecolor", "metallic"):
            b = nt.nodes["Principled BSDF"]; out = [n for n in nt.nodes if n.type == 'OUTPUT_MATERIAL'][0]
            em = nt.nodes.new('ShaderNodeEmission'); src = b.inputs['Base Color' if kind == "basecolor" else 'Metallic']
            if src.is_linked: nt.links.new(src.links[0].from_socket, em.inputs[0])
            elif kind == "basecolor": em.inputs[0].default_value = tuple(src.default_value)
            else: v = src.default_value; em.inputs[0].default_value = (v, v, v, 1)
            nt.links.new(em.outputs[0], out.inputs[0]); tmp.append((nt, em, b, out))
    if kind in ("basecolor", "metallic"): s.cycles.samples = 2; bpy.ops.object.bake(type='EMIT')
    elif kind == "roughness": s.cycles.samples = 2; bpy.ops.object.bake(type='ROUGHNESS')
    else: s.cycles.samples = 4; bpy.ops.object.bake(type='NORMAL', normal_space='TANGENT')
    for nt, em, b, out in tmp: nt.links.new(b.outputs[0], out.inputs[0]); nt.nodes.remove(em)
    for sl in ob.material_slots: D.images[f"{sl.material.name}_{kind}"].save()
    return round(time.time() - t0, 1)

def prep_images(objs, size=1024):
    tex = os.path.join(os.path.dirname(D.filepath), "textures")
    for o in objs:
        for s in D.objects[o].material_slots:
            mat = s.material.name; nt = s.material.node_tree
            node = nt.nodes.get("BAKE_TARGET") or nt.nodes.new("ShaderNodeTexImage"); node.name = "BAKE_TARGET"
            for kind in ["basecolor", "roughness", "normal"]:
                n = f"{mat}_{kind}"
                if n in D.images: D.images.remove(D.images[n])
                im = D.images.new(n, size, size); im.colorspace_settings.name = 'sRGB' if kind == "basecolor" else 'Non-Color'
                im.filepath_raw = os.path.join(tex, n + ".png"); im.file_format = 'PNG'

def masks(mats, metal=()):
    import numpy as np
    tex = os.path.join(os.path.dirname(D.filepath), "textures")
    for name in mats:
        ro = D.images[name + "_roughness"]; sz = ro.size[0]
        rough = np.array(ro.pixels[:], dtype=np.float32).reshape(-1, 4)
        out = np.zeros_like(rough); out[:, 0] = 1.0 if name in metal else 0.0; out[:, 1] = 1; out[:, 3] = 1 - rough[:, 0]
        n = name + "_mask"
        if n in D.images: D.images.remove(D.images[n])
        im = D.images.new(n, sz, sz, alpha=True); im.colorspace_settings.name = 'Non-Color'; im.alpha_mode = 'STRAIGHT'
        im.pixels.foreach_set(out.ravel()); im.filepath_raw = os.path.join(tex, n + ".png"); im.file_format = 'PNG'; im.save()

def export_items(items):
    """items: {item_id: object name}; each exported alone at the origin, footprint centred, ground at z=0"""
    d = os.path.dirname(D.filepath); out = {}
    for iid, on in items.items():
        o = D.objects[on]; keep = o.location.copy(); me = o.data
        xs = [v.co.x for v in me.vertices]; ys = [v.co.y for v in me.vertices]
        cx, cy = (min(xs) + max(xs)) / 2, (min(ys) + max(ys)) / 2
        for v in me.vertices: v.co.x -= cx; v.co.y -= cy
        o.location = (0, 0, 0)
        bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active = o
        fbx = os.path.join(d, f"item_{iid}_model.fbx")
        bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True, object_types={'EMPTY', 'MESH'}, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y', bake_space_transform=True, mesh_smooth_type='FACE', add_leaf_bones=False, path_mode='STRIP', embed_textures=False)
        o.location = keep; out[iid] = os.path.getsize(fbx)
    return out

def review_setup(sun=1.4, world=0.35):
    sc = bpy.context.scene
    sc.render.engine = 'CYCLES'; sc.cycles.device = 'CPU'; sc.cycles.samples = 32; sc.view_settings.view_transform = 'AgX'
    if not sc.world: sc.world = D.worlds.new("W")
    sc.world.use_nodes = True; bg = sc.world.node_tree.nodes["Background"]; bg.inputs[0].default_value = (0.55, 0.62, 0.72, 1); bg.inputs[1].default_value = world
    if "ReviewSun" not in D.objects:
        l = D.lights.new("ReviewSun", 'SUN'); l.energy = sun; l.angle = math.radians(3)
        so = D.objects.new("ReviewSun", l); sc.collection.objects.link(so); so.rotation_euler = (math.radians(50), math.radians(10), math.radians(-35))
    if "ReviewGround" not in D.objects:
        me = D.meshes.new("ReviewGround"); bm = bmesh.new(); bmesh.ops.create_grid(bm, x_segments=1, y_segments=1, size=2.0); bm.to_mesh(me); bm.free()
        g = D.objects.new("ReviewGround", me); sc.collection.objects.link(g); g.location = (0, 0, -0.0005)
        gm = D.materials.new("review_ground"); gm.use_nodes = True; gm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.2, 0.16, 0.11, 1)
        me.materials.append(gm)
    if "ReviewCam" not in D.objects:
        cam = D.cameras.new("cam"); cam.lens = 40; co = D.objects.new("ReviewCam", cam); sc.collection.objects.link(co)
    sc.camera = D.objects["ReviewCam"]

def render(name, tgt, loc, res=(1200, 800), samples=32):
    sc = bpy.context.scene; cam = D.objects["ReviewCam"]
    cam.location = Vector(loc); cam.rotation_euler = (Vector(tgt) - Vector(loc)).to_track_quat('-Z', 'Y').to_euler()
    sc.render.resolution_x, sc.render.resolution_y = res; sc.cycles.samples = samples
    sc.render.filepath = os.path.join(os.path.dirname(D.filepath), "review", name); bpy.ops.render.render(write_still=True)
    return sc.render.filepath

def tube(bm, pts, r, sides=10, mat=0, caps=True):
    """one continuous tube along pts (no seams between segments)"""
    rings = []; prev_a = None
    for k, p in enumerate(pts):
        d = (pts[min(k + 1, len(pts) - 1)] - pts[max(k - 1, 0)]).normalized()
        if prev_a is None:
            up = Vector((0, 0, 1)) if abs(d.z) < 0.9 else Vector((1, 0, 0)); a = d.cross(up).normalized()
        else:
            a = (prev_a - d * prev_a.dot(d)).normalized()
        b = d.cross(a).normalized(); prev_a = a
        rings.append([bm.verts.new(p + (a * math.cos(2 * math.pi * i / sides) + b * math.sin(2 * math.pi * i / sides)) * r) for i in range(sides)])
    for ra, rb in zip(rings[:-1], rings[1:]):
        for i in range(sides):
            j = (i + 1) % sides; f = bm.faces.new([ra[i], ra[j], rb[j], rb[i]]); f.material_index = mat
    if caps:
        for ring, cc, flip in ((rings[0], pts[0], True), (rings[-1], pts[-1], False)):
            c = bm.verts.new(cc)
            for i in range(sides):
                vs = [ring[i], ring[(i + 1) % sides], c]
                f = bm.faces.new(vs[::-1] if flip else vs); f.material_index = mat

def revolve(bm, prof, N=40, wob=0.0, sd=0.0):
    """prof: (r, z) from the outer base centre, up the outside, over the rim, down the inside; r == 0 closes with a fan"""
    rings = []
    for (r, z) in prof:
        if r < 1e-6: rings.append([bm.verts.new((0, 0, z))]); continue
        ring = []
        for i in range(N):
            th = 2 * math.pi * i / N; k = 1 + wob * noise.noise(Vector((math.cos(th) * 1.3, math.sin(th) * 1.3, z * 6 + sd)))
            ring.append(bm.verts.new((r * k * math.cos(th), r * k * math.sin(th), z)))
        rings.append(ring)
    for a, b in zip(rings[:-1], rings[1:]):
        if len(a) == 1:
            for i in range(N): bm.faces.new([a[0], b[(i + 1) % N], b[i]])
        elif len(b) == 1:
            for i in range(N): bm.faces.new([a[i], a[(i + 1) % N], b[0]])
        else:
            for i in range(N): bm.faces.new([a[i], a[(i + 1) % N], b[(i + 1) % N], b[i]])

def tube_var(bm, pts, radii, sides=10, mat=0, cap0=True, cap1=True):
    """a tube along pts with a radius per point; caps optional (so two tubes can join seamlessly)"""
    rings = []; prev_a = None
    for k, p in enumerate(pts):
        d = (pts[min(k + 1, len(pts) - 1)] - pts[max(k - 1, 0)]).normalized()
        if prev_a is None:
            up = Vector((0, 0, 1)) if abs(d.z) < 0.9 else Vector((1, 0, 0)); a = d.cross(up).normalized()
        else:
            a = (prev_a - d * prev_a.dot(d)).normalized()
        b = d.cross(a).normalized(); prev_a = a
        rings.append([bm.verts.new(p + (a * math.cos(2 * math.pi * i / sides) + b * math.sin(2 * math.pi * i / sides)) * radii[k]) for i in range(sides)])
    for ra, rb in zip(rings[:-1], rings[1:]):
        for i in range(sides):
            j = (i + 1) % sides; f = bm.faces.new([ra[i], ra[j], rb[j], rb[i]]); f.material_index = mat
    for ring, cc, flip, on in ((rings[0], pts[0], True, cap0), (rings[-1], pts[-1], False, cap1)):
        if not on: continue
        c = bm.verts.new(cc)
        for i in range(sides):
            vs = [ring[i], ring[(i + 1) % sides], c]; f = bm.faces.new(vs[::-1] if flip else vs); f.material_index = mat
    return rings
