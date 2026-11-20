
# World rocks (Wave 6, 2026-11-19): three weathered limestone rocks for StonePlacer's Rock A, B and C meshes.
# The game scales one shape from a 0.25 m loose stone up to a 3.2 m outcrop boulder, so each rock is about 1 unit across,
# centred, with a flatter bottom near y = -0.2 (Blender z), like the generated meshes they replace. All three share one
# texture (baked together into one UV atlas), so StonePlacer's single Stone material still covers every stone.
import bpy, bmesh, math, random
from mathutils import Vector, noise
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
exec(open(ROOT + r"\animal_util.py", encoding="utf-8").read())   # the shader math helpers (M, mixc, clamp01)
V = Vector

def rock(name, seed, stretch=(1.0, 1.0, 0.72), cuts=7):
    random.seed(seed)
    bm = bmesh.new(); bmesh.ops.create_icosphere(bm, subdivisions=5, radius=0.5)
    off = V((seed * 3.1, seed * 1.7, seed * 2.3))
    for v in bm.verts:
        p = v.co.copy()
        n = noise.fractal(p * 2.2 + off, 0.6, 2.0, 4) * 0.11 + noise.noise(p * 7.0 + off) * 0.025
        v.co = p * (1 + n)
        v.co = V((v.co.x * stretch[0], v.co.y * stretch[1], v.co.z * stretch[2]))
    # limestone breaks along bedding and joints: cut a few flat faces, the bedding planes nearly level
    planes = []
    for i in range(cuts):
        if i < 2:
            nrm = V((random.uniform(-0.15, 0.15), random.uniform(-0.15, 0.15), 1 if i == 0 else -1)).normalized()
        else:
            a = random.uniform(0, 2 * math.pi); e = random.uniform(-0.35, 0.5)
            nrm = V((math.cos(a) * math.cos(e), math.sin(a) * math.cos(e), math.sin(e)))
        d = max((v.co.dot(nrm) for v in bm.verts)) * random.uniform(0.78, 0.9)
        planes.append((nrm, d))
    for v in bm.verts:
        for nrm, d in planes:
            h = v.co.dot(nrm)
            if h > d:
                v.co -= nrm * (h - d) * 0.985           # flattened, keeping a little irregularity
    # sit it: a flatter bottom near -0.2
    for v in bm.verts:
        if v.co.z < -0.17: v.co.z = -0.17 - (v.co.z + 0.17) * 0.25
    # weathering: soften the cut edges and add small pits
    bmesh.ops.smooth_vert(bm, verts=bm.verts, factor=0.25, use_axis_x=True, use_axis_y=True, use_axis_z=True)
    for v in bm.verts:
        v.co += v.normal * (noise.noise(v.co * 18 + off) * 0.006)
    o = link(name, bm)
    dm = o.modifiers.new("Dec", 'DECIMATE'); dm.ratio = 0.16
    bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier="Dec")
    bpy.ops.object.shade_smooth_by_angle(angle=math.radians(38))   # rounded faces, crisp broken edges
    return o

def build():
    for ob in list(bpy.data.objects):
        if ob.name.startswith("Rock"): bpy.data.objects.remove(ob)
    a = rock("Rock_A", 1, (1.0, 0.9, 0.7), 9)
    b = rock("Rock_B", 2, (1.05, 0.82, 0.62), 10)
    c = rock("Rock_C", 3, (0.92, 0.95, 0.78), 8)
    out = {}
    for o in (a, b, c):
        zs = [v.co.z for v in o.data.vertices]; xs = [v.co.x for v in o.data.vertices]; ys = [v.co.y for v in o.data.vertices]
        out[o.name] = {"tris": sum(len(p.vertices) - 2 for p in o.data.polygons), "x": [round(min(xs), 2), round(max(xs), 2)], "y": [round(min(ys), 2), round(max(ys), 2)], "z": [round(min(zs), 2), round(max(zs), 2)]}
    return out

def stone_mat(name="world_rock_stone"):
    m, t, b, tc, L = base(name)
    geo = t.nodes.new("ShaderNodeNewGeometry")
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = 3.5; n1.inputs["Detail"].default_value = 10; n1.inputs["Roughness"].default_value = 0.6; L(tc.outputs["Object"], n1.inputs["Vector"])
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = 28; n2.inputs["Detail"].default_value = 12; L(tc.outputs["Object"], n2.inputs["Vector"])
    wv = t.nodes.new("ShaderNodeTexWave"); wv.wave_type = 'BANDS'; wv.bands_direction = 'Z'; wv.inputs["Scale"].default_value = 2.2; wv.inputs["Distortion"].default_value = 3.0; wv.inputs["Detail"].default_value = 4
    L(tc.outputs["Object"], wv.inputs["Vector"])
    # weathered grey limestone, warmer where it's fresher, darker in the hollows
    col = ramp(t, L, n1.outputs["Fac"], (0.27, 0.26, 0.24, 1), (0.52, 0.5, 0.45, 1), 0.35, 0.68).outputs[0]
    col = mixc(t, L, M(t, L, 'MULTIPLY', ramp(t, L, n2.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.45, 0.62).outputs[0], 0.3), col, (0.58, 0.54, 0.46, 1))
    pt = ramp(t, L, geo.outputs["Pointiness"], (0, 0, 0, 1), (1, 1, 1, 1), 0.44, 0.52).outputs[0]
    col = mixc(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 1.0, pt), 0.45), col, (0.16, 0.15, 0.13, 1))
    # lichen: grey-green patches on top, pale crusty spots
    nl = t.nodes.new("ShaderNodeTexNoise"); nl.inputs["Scale"].default_value = 9; nl.inputs["Detail"].default_value = 8; L(tc.outputs["Object"], nl.inputs["Vector"])
    sn = t.nodes.new("ShaderNodeSeparateXYZ"); L(geo.outputs["Normal"], sn.inputs[0])
    up = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', sn.outputs[2], 0.15), 2.0))
    lich = M(t, L, 'MULTIPLY', ramp(t, L, nl.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.55, 0.62).outputs[0], up)
    col = mixc(t, L, M(t, L, 'MULTIPLY', lich, 0.75), col, (0.34, 0.37, 0.27, 1))
    spots = ramp(t, L, n2.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.66, 0.7).outputs[0]
    col = mixc(t, L, M(t, L, 'MULTIPLY', spots, 0.6), col, (0.72, 0.71, 0.64, 1))
    # thin dark lines along the bedding, broken up
    crack = M(t, L, 'MULTIPLY', ramp(t, L, wv.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.955, 0.98).outputs[0], ramp(t, L, n1.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.4, 0.55).outputs[0])
    col = mixc(t, L, M(t, L, 'MULTIPLY', crack, 0.45), col, (0.16, 0.15, 0.14, 1))
    # a warmer, buff tint over parts of it
    col = mixc(t, L, M(t, L, 'MULTIPLY', ramp(t, L, nl.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.3, 0.45).outputs[0], 0.25), col, (0.5, 0.44, 0.34, 1))
    L(col, b.inputs["Base Color"])
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = 0.72; rr.inputs[4].default_value = 0.95; L(n2.outputs["Fac"], rr.inputs[0]); L(rr.outputs[0], b.inputs["Roughness"])
    h = M(t, L, 'ADD', M(t, L, 'MULTIPLY', n2.outputs["Fac"], 0.6), M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 1.0, crack), 0.25))
    h = M(t, L, 'ADD', h, M(t, L, 'MULTIPLY', n1.outputs["Fac"], 0.4))
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.6; bp.inputs["Distance"].default_value = 0.01; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.42, 0.4, 0.36, 1); return m


ROCKS = ["Rock_A", "Rock_B", "Rock_C"]

def unwrap_together():
    """one UV layout across all three rocks, so one texture set serves the game's single Stone material"""
    bpy.ops.object.select_all(action='DESELECT')
    for n in ROCKS: bpy.data.objects[n].select_set(True)
    bpy.context.view_layer.objects.active = bpy.data.objects[ROCKS[0]]
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(60), island_margin=0.004)
    bpy.ops.object.mode_set(mode='OBJECT')

def bake_together(kind):
    """bake_all for all three rocks at once into the shared images"""
    import time
    s = bpy.context.scene; t0 = time.time()
    s.render.engine = 'CYCLES'; s.cycles.device = 'CPU'; s.render.bake.margin = 8
    bpy.ops.object.select_all(action='DESELECT')
    for n in ROCKS: bpy.data.objects[n].select_set(True)
    bpy.context.view_layer.objects.active = bpy.data.objects[ROCKS[0]]
    mat = bpy.data.objects[ROCKS[0]].material_slots[0].material; nt = mat.node_tree
    node = nt.nodes["BAKE_TARGET"]; node.image = bpy.data.images[f"{mat.name}_{kind}"]
    for n in nt.nodes: n.select = False
    node.select = True; nt.nodes.active = node
    tmp = None
    if kind == "basecolor":
        b = nt.nodes["Principled BSDF"]; out = [n for n in nt.nodes if n.type == 'OUTPUT_MATERIAL'][0]
        em = nt.nodes.new('ShaderNodeEmission'); nt.links.new(b.inputs['Base Color'].links[0].from_socket, em.inputs[0]); nt.links.new(em.outputs[0], out.inputs[0]); tmp = (em, b, out)
        s.cycles.samples = 2; bpy.ops.object.bake(type='EMIT')
    elif kind == "roughness": s.cycles.samples = 2; bpy.ops.object.bake(type='ROUGHNESS')
    else: s.cycles.samples = 4; bpy.ops.object.bake(type='NORMAL', normal_space='TANGENT')
    if tmp: nt.links.new(tmp[1].outputs[0], tmp[2].inputs[0]); nt.nodes.remove(tmp[0])
    bpy.data.images[f"{mat.name}_{kind}"].save()
    return round(time.time() - t0, 1)


def raise_to_game_height(k=1.4):
    """the game sinks boulders 30% of their size and squashes stones to 70% height, so the rocks keep the old meshes'
    height (top near 0.5); scaled about the flat bottom after the bake, which keeps the shared UV layout"""
    for n in ROCKS:
        me = bpy.data.objects[n].data
        for v in me.vertices: v.co.z = -0.17 + (v.co.z + 0.17) * k
        me.update()
