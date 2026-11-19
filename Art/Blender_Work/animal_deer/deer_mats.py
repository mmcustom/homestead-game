
# Deer coat and parts (2026-11-19). Coat regions come from where a point is on the body (object position) and which way the surface faces
# (geometry normal), so the same idea works for other species with their own region numbers.
import bpy
D = bpy.data

def M(t, L, op, a, b=None, c=None):
    n = t.nodes.new("ShaderNodeMath"); n.operation = op
    for i, v in enumerate((a, b, c)):
        if v is None: continue
        if isinstance(v, (int, float)): n.inputs[i].default_value = v
        else: L(v, n.inputs[i])
    return n.outputs[0]

def band(t, L, x, lo, hi, soft):
    """1 inside [lo, hi], fading over `soft` either side"""
    a = M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', x, lo - soft), 1.0 / soft)
    b = M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', hi + soft, x), 1.0 / soft)
    return M(t, L, 'MAXIMUM', M(t, L, 'MINIMUM', M(t, L, 'MINIMUM', a, b, None), 1.0), 0.0)   # clamped: a negative outside would turn white on when two regions multiply

def clamp01(t, L, x): return M(t, L, 'MAXIMUM', M(t, L, 'MINIMUM', x, 1.0), 0.0)

def mixc(t, L, fac, a, b):
    m = t.nodes.new("ShaderNodeMix"); m.data_type = 'RGBA'; L(fac, m.inputs[0])
    for i, v in ((6, a), (7, b)):
        if isinstance(v, tuple): m.inputs[i].default_value = v
        else: L(v, m.inputs[i])
    return m.outputs[2]

def coat(name):
    m, t, b, tc, L = base(name)
    sp = t.nodes.new("ShaderNodeSeparateXYZ"); L(tc.outputs["Object"], sp.inputs[0]); x, y, z = sp.outputs
    geo = t.nodes.new("ShaderNodeNewGeometry"); sn = t.nodes.new("ShaderNodeSeparateXYZ"); L(geo.outputs["Normal"], sn.inputs[0]); nx, ny, nz = sn.outputs
    ax = M(t, L, 'ABSOLUTE', x)
    # fur: fine streaks along the body, a coarser mottle
    mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (60, 18, 60); L(tc.outputs["Object"], mp.inputs[0])
    fur = t.nodes.new("ShaderNodeTexNoise"); fur.inputs["Scale"].default_value = 18; fur.inputs["Detail"].default_value = 10; L(mp.outputs[0], fur.inputs["Vector"])
    mot = t.nodes.new("ShaderNodeTexNoise"); mot.inputs["Scale"].default_value = 6; mot.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], mot.inputs["Vector"])
    edge = M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', mot.outputs["Fac"], 0.5), 0.12)   # breaks up region edges
    body = ramp(t, L, M(t, L, 'MULTIPLY_ADD', fur.outputs["Fac"], 0.6, M(t, L, 'MULTIPLY', mot.outputs["Fac"], 0.4)), (0.17, 0.115, 0.07, 1), (0.36, 0.26, 0.17, 1), 0.3, 0.75).outputs[0]
    # darker along the spine and on the forehead
    spine = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', nz, 0.55), 2.5)), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', z, 0.78), 6.0)))
    col = mixc(t, L, M(t, L, 'MULTIPLY', spine, 0.55), body, (0.1, 0.07, 0.045, 1))
    # lighter tan lower legs
    lower = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.42, M(t, L, 'ADD', z, edge)), 6.0))
    col = mixc(t, L, M(t, L, 'MULTIPLY', lower, 0.55), col, (0.46, 0.36, 0.25, 1))
    W = (0.74, 0.71, 0.65, 1)
    # white belly: the underside of the barrel
    under = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', M(t, L, 'MULTIPLY', nz, -1.0), M(t, L, 'ADD', 0.35, edge)), 4.0)), band(t, L, y, -0.42, 0.5, 0.08))
    under = M(t, L, 'MULTIPLY', under, clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.8, z), 8.0)))
    # white inner legs (the side facing the other leg), upper half
    inner = M(t, L, 'MULTIPLY', clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', M(t, L, 'MULTIPLY', M(t, L, 'MULTIPLY', nx, M(t, L, 'SIGN', x)), -1.0), 0.35), 3.0)), band(t, L, z, 0.3, 0.62, 0.06))
    # white throat patch and chin
    throat = M(t, L, 'MULTIPLY', M(t, L, 'MULTIPLY', band(t, L, y, -0.80, -0.68, 0.025), band(t, L, z, 1.06, 1.17, 0.025)), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'MULTIPLY', ny, -1.0), 3.0)))
    throat = M(t, L, 'MULTIPLY', throat, clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.55, M(t, L, 'ABSOLUTE', nx)), 4.0)))   # the front of the throat, not the neck's sides
    chin = M(t, L, 'MULTIPLY', band(t, L, y, -0.96, -0.86, 0.02), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', M(t, L, 'MULTIPLY', nz, -1.0), 0.2), 4.0)))
    # white under the tail and the rump patch either side of it
    tail = M(t, L, 'MULTIPLY', band(t, L, y, 0.66, 0.9, 0.03), clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', ny, 0.35), 3.0)))
    tail = M(t, L, 'MULTIPLY', tail, band(t, L, z, 0.62, 0.86, 0.04))
    white = M(t, L, 'MAXIMUM', M(t, L, 'MAXIMUM', under, inner), M(t, L, 'MAXIMUM', M(t, L, 'MAXIMUM', throat, chin), tail))
    col = mixc(t, L, clamp01(t, L, white), col, W)
    # black nose and hooves
    nose = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', -0.955, y), 40.0))
    col = mixc(t, L, nose, col, (0.025, 0.02, 0.018, 1))
    hoof = clamp01(t, L, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', 0.045, z), 60.0))
    col = mixc(t, L, hoof, col, (0.045, 0.04, 0.036, 1))
    L(col, b.inputs["Base Color"])
    rough = M(t, L, 'MULTIPLY_ADD', M(t, L, 'MAXIMUM', nose, hoof), -0.45, 0.88)
    L(rough, b.inputs["Roughness"])
    hgt = M(t, L, 'MULTIPLY_ADD', fur.outputs["Fac"], 1.0, 0.0)
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.5; bp.inputs["Distance"].default_value = 0.0015; L(hgt, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = (0.3, 0.22, 0.15, 1); return m

def make_mats():
    body = D.objects["Deer_Body"]; ears = D.objects["Deer_Ears"]; ant = D.objects["Deer_Antlers"]
    assign(body, [coat("animal_deer_coat")])
    ear = lump("animal_deer_ear", (0.16, 0.11, 0.075, 1), (0.32, 0.24, 0.17, 1), rough=(0.8, 0.95), nscale=40, fine=300, bump=0.4, bumpd=0.0006)
    eye = lump("animal_deer_eye", (0.005, 0.004, 0.004, 1), (0.02, 0.015, 0.012, 1), rough=(0.06, 0.12), nscale=20, fine=80, bump=0.05, bumpd=0.0001)
    assign(ears, [ear, eye])
    antler = lump("animal_deer_antler", (0.3, 0.23, 0.15, 1), (0.6, 0.53, 0.41, 1), rough=(0.55, 0.8), nscale=60, fine=320, bump=0.7, grain_=True, bumpd=0.0006)
    assign(ant, [antler])
    return [m.name for m in (body.data.materials[0], ear, eye, antler)]
