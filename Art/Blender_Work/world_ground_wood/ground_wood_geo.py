
# Ground wood (Wave 6, 2026-11-19): GroundWoodPlacer's three Stick Bundles and three Fallen Branches.
# Same sizes as the generated meshes they replace (chunkier than life so they read from a few metres away):
# a bundle is six sticks 3-4 cm thick, about 1 m across, crossed and stacked; a branch lies along X (the placer's box
# collider runs along local X), 1.6-2.2 m long, tapering, with 2-3 side twigs. Ground at z = 0.
# All six share one texture set (baked together), so the placer's single Ground Wood material still covers them all.
import bpy, bmesh, math, random
from mathutils import Vector, noise
ROOT = r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
exec(open(ROOT + r"\item_util.py", encoding="utf-8").read())
exec(open(ROOT + r"\animal_util.py", encoding="utf-8").read())
V = Vector
PIECES = ["Stick_Bundle_A", "Stick_Bundle_B", "Stick_Bundle_C", "Fallen_Branch_A", "Fallen_Branch_B", "Fallen_Branch_C"]

def bent(bm, p0, p1, r0, r1, sd, bend=0.04, segs=7, sides=6, wob=0.008):
    """a stick with a gentle bend and knobbly wobble, from p0 to p1"""
    mid = (p0 + p1) * 0.5 + V((noise.noise(V((sd, 1, 0))) * bend, noise.noise(V((sd, 2, 0))) * bend, abs(noise.noise(V((sd, 3, 0)))) * bend * 0.4))
    stick(bm, p0, mid, r0, (r0 + r1) * 0.5, segs=segs // 2 + 1, sides=sides, wob=wob, sd=sd)
    stick(bm, mid, p1, (r0 + r1) * 0.5, r1, segs=segs // 2 + 1, sides=sides, wob=wob, sd=sd + 0.5)

def bundle(name, seed):
    random.seed(seed); bm = bmesh.new()
    for i in range(6):
        yaw = random.uniform(0, math.pi); length = random.uniform(0.75, 1.1)
        r = random.uniform(0.028, 0.04)
        c = V((random.uniform(-0.15, 0.15), random.uniform(-0.15, 0.15), r + i * 0.032))
        along = V((math.cos(yaw), math.sin(yaw), random.uniform(-0.02, 0.02))) * (length * 0.5)
        bent(bm, c - along, c + along, r, r * 0.6, seed * 10 + i)
    return link(name, bm)

def branch(name, seed):
    random.seed(seed + 100); bm = bmesh.new()
    length = random.uniform(1.6, 2.2); thick = random.uniform(0.065, 0.085)
    a = V((-length * 0.5, 0, thick + 0.005)); b = V((length * 0.5, random.uniform(-0.1, 0.1), thick * 0.4 + 0.004))
    bent(bm, a, b, thick, thick * 0.4, seed * 7, bend=0.09, segs=12, sides=8, wob=0.012)
    for i in range(2 + random.randrange(2)):
        t = random.uniform(0.2, 0.8); f = a.lerp(b, t); side = random.choice((-1, 1))
        to = f + V((random.uniform(0.2, 0.4), side * random.uniform(0.22, 0.42), random.uniform(-0.02, 0.03)))
        to.z = max(to.z, thick * 0.3)
        bent(bm, f, to, thick * 0.45, thick * 0.18, seed * 7 + i + 1, bend=0.03, segs=5, sides=5, wob=0.006)
        if random.random() < 0.6:   # a smaller fork off the twig
            g = f.lerp(to, 0.6); g2 = g + V((random.uniform(0.08, 0.18), side * random.uniform(0.05, 0.15), 0.01))
            bent(bm, g, g2, thick * 0.2, thick * 0.1, seed * 7 + i + 9, bend=0.01, segs=3, sides=4, wob=0.003)
    return link(name, bm)

def build():
    for ob in list(bpy.data.objects):
        if ob.name in PIECES: bpy.data.objects.remove(ob)
    objs = [bundle(PIECES[i], i + 1) for i in range(3)] + [branch(PIECES[3 + i], i + 1) for i in range(3)]
    out = {}
    for o in objs:
        ground(o)
        out[o.name] = sum(len(p.vertices) - 2 for p in o.data.polygons)
    return out

def wood_mat(name="world_ground_wood"):
    """weathered fallen wood: grey-brown bark in rough furrows, peeled in places to pale grey silvered wood, pale broken ends"""
    m, t, b, tc, L = base(name)
    geo = t.nodes.new("ShaderNodeNewGeometry")
    mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (1.0, 1.0, 1.0); L(tc.outputs["Object"], mp.inputs[0])
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = 6; n1.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], n1.inputs["Vector"])
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = 90; n2.inputs["Detail"].default_value = 10; L(tc.outputs["Object"], n2.inputs["Vector"])
    fz = t.nodes.new("ShaderNodeTexNoise"); fz.inputs["Scale"].default_value = 55; fz.inputs["Detail"].default_value = 3; fz.inputs["Distortion"].default_value = 2.5; L(tc.outputs["Object"], fz.inputs["Vector"])
    bark = ramp(t, L, n2.outputs["Fac"], (0.15, 0.105, 0.07, 1), (0.34, 0.26, 0.18, 1), 0.3, 0.7).outputs[0]
    furrow = ramp(t, L, fz.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.4, 0.6).outputs[0]   # irregular ridges and furrows, no cells
    bark = mixc(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 1.0, furrow), 0.5), bark, (0.08, 0.055, 0.04, 1))
    bare = ramp(t, L, n2.outputs["Fac"], (0.32, 0.28, 0.23, 1), (0.46, 0.4, 0.32, 1), 0.3, 0.7).outputs[0]
    peel = ramp(t, L, n1.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.6, 0.64).outputs[0]
    col = mixc(t, L, peel, bark, bare)
    # broken ends: pale, where the surface faces along the stick (pointiness is high at the cap fans)
    pt = ramp(t, L, geo.outputs["Pointiness"], (0, 0, 0, 1), (1, 1, 1, 1), 0.56, 0.62).outputs[0]
    col = mixc(t, L, M(t, L, 'MULTIPLY', pt, 0.7), col, (0.62, 0.52, 0.38, 1))
    # a little moss on the upper side
    sn = t.nodes.new("ShaderNodeSeparateXYZ"); L(geo.outputs["Normal"], sn.inputs[0])
    nm = t.nodes.new("ShaderNodeTexNoise"); nm.inputs["Scale"].default_value = 14; nm.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], nm.inputs["Vector"])
    moss = M(t, L, 'MULTIPLY', ramp(t, L, nm.outputs["Fac"], (0, 0, 0, 1), (1, 1, 1, 1), 0.58, 0.64).outputs[0], clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', sn.outputs[2], 0.4), 2.5)))
    col = mixc(t, L, M(t, L, 'MULTIPLY', moss, 0.7), col, (0.17, 0.22, 0.08, 1))
    L(col, b.inputs["Base Color"])
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = 0.75; rr.inputs[4].default_value = 0.95; L(n2.outputs["Fac"], rr.inputs[0]); L(rr.outputs[0], b.inputs["Roughness"])
    h = M(t, L, 'ADD', M(t, L, 'MULTIPLY', furrow, M(t, L, 'SUBTRACT', 1.0, peel)), M(t, L, 'MULTIPLY', n2.outputs["Fac"], 0.3))
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.7; bp.inputs["Distance"].default_value = 0.003; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.3, 0.26, 0.21, 1); return m


def unwrap_together():
    """one UV layout across all six pieces, so one texture set serves the game's single Ground Wood material"""
    bpy.ops.object.select_all(action='DESELECT')
    for n in PIECES: bpy.data.objects[n].select_set(True)
    bpy.context.view_layer.objects.active = bpy.data.objects[PIECES[0]]
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(60), island_margin=0.003)
    bpy.ops.object.mode_set(mode='OBJECT')

def bake_together(kind):
    import time
    s = bpy.context.scene; t0 = time.time()
    s.render.engine = 'CYCLES'; s.cycles.device = 'CPU'; s.render.bake.margin = 6
    bpy.ops.object.select_all(action='DESELECT')
    for n in PIECES: bpy.data.objects[n].select_set(True)
    bpy.context.view_layer.objects.active = bpy.data.objects[PIECES[0]]
    mat = bpy.data.objects[PIECES[0]].material_slots[0].material; nt = mat.node_tree
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
