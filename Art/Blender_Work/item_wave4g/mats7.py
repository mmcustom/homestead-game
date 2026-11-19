
# Wave 4 batch 7 materials (2026-11-19)
import bpy, math
D = bpy.data

def bark_thin(name, c0=(0.15, 0.105, 0.07, 1), c1=(0.30, 0.235, 0.17, 1)):
    """direction-free thin branch bark: mottled grey-brown with pale lenticel specks and fine roughness"""
    return lump(name, c0, c1, rough=(0.78, 0.95), nscale=22, fine=150, bump=0.7, specks=(160, (0.48, 0.42, 0.33, 1), 0.69), bumpd=0.0015)

def cord(name, c0=(0.33, 0.26, 0.13, 1), c1=(0.56, 0.47, 0.27, 1)):
    return lump(name, c0, c1, rough=(0.82, 0.97), nscale=60, fine=260, bump=0.6, grain_=True, bumpd=0.0006)

def endgrain(name, light=(0.56, 0.43, 0.28, 1), dark=(0.33, 0.22, 0.13, 1)):
    m, t, b, tc, L = base(name)
    wv = t.nodes.new("ShaderNodeTexWave"); wv.wave_type = 'RINGS'; wv.rings_direction = 'Z'; wv.inputs["Scale"].default_value = 22
    wv.inputs["Distortion"].default_value = 1.6; wv.inputs["Detail"].default_value = 2; L(tc.outputs["Object"], wv.inputs["Vector"])
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = 9; n1.inputs["Detail"].default_value = 6; L(tc.outputs["Object"], n1.inputs["Vector"])
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = 180; n2.inputs["Detail"].default_value = 10; L(tc.outputs["Object"], n2.inputs["Vector"])
    vo = t.nodes.new("ShaderNodeTexVoronoi"); vo.feature = 'DISTANCE_TO_EDGE'; vo.inputs["Scale"].default_value = 7; L(tc.outputs["Object"], vo.inputs["Vector"])
    ck = ramp(t, L, vo.outputs["Distance"], (0, 0, 0, 1), (1, 1, 1, 1), 0.0, 0.025)   # thin radial checks
    mx = t.nodes.new("ShaderNodeMath"); mx.operation = 'MULTIPLY_ADD'; mx.inputs[1].default_value = 0.45; L(wv.outputs["Fac"], mx.inputs[0]); L(n1.outputs["Fac"], mx.inputs[2])
    col = ramp(t, L, mx.outputs[0], dark, light, 0.45, 0.95)
    m1 = t.nodes.new("ShaderNodeMix"); m1.data_type = 'RGBA'; m1.blend_type = 'MULTIPLY'; m1.inputs[0].default_value = 1.0; L(col.outputs[0], m1.inputs[6])
    ckc = ramp(t, L, ck.outputs[0], (0.35, 0.3, 0.27, 1), (1, 1, 1, 1), 0.0, 1.0); L(ckc.outputs[0], m1.inputs[7])
    ov = t.nodes.new("ShaderNodeMix"); ov.data_type = 'RGBA'; ov.blend_type = 'OVERLAY'; ov.inputs[0].default_value = 0.3; L(m1.outputs[2], ov.inputs[6]); L(n2.outputs["Fac"], ov.inputs[7])
    L(ov.outputs[2], b.inputs["Base Color"])
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = 0.72; rr.inputs[4].default_value = 0.92; L(n2.outputs["Fac"], rr.inputs[0]); L(rr.outputs[0], b.inputs["Roughness"])
    h = t.nodes.new("ShaderNodeMath"); h.operation = 'MULTIPLY_ADD'; h.inputs[1].default_value = 0.2; L(n2.outputs["Fac"], h.inputs[0]); L(ck.outputs[0], h.inputs[2])
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.6; bp.inputs["Distance"].default_value = 0.0015; L(h.outputs[0], bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = light; return m

def rope(name, c0=(0.30, 0.25, 0.12, 1), c1=(0.58, 0.50, 0.29, 1), pitch=0.02):
    """two-ply twist from the 'rope' attribute: R,G = cos,sin of the angle round the cord (0..1), B = length along it"""
    m, t, b, tc, L = base(name)
    at = t.nodes.new("ShaderNodeAttribute"); at.attribute_name = "rope"
    sp = t.nodes.new("ShaderNodeSeparateColor"); L(at.outputs["Color"], sp.inputs[0])
    def M(op, a, bb=None, v=None):
        n = t.nodes.new("ShaderNodeMath"); n.operation = op
        if isinstance(a, float): n.inputs[0].default_value = a
        else: L(a, n.inputs[0])
        if bb is not None:
            if isinstance(bb, float): n.inputs[1].default_value = bb
            else: L(bb, n.inputs[1])
        return n.outputs[0]
    c = M('MULTIPLY_ADD', sp.outputs[0], 2.0); c.node.inputs[2].default_value = -1.0
    s = M('MULTIPLY_ADD', sp.outputs[1], 2.0); s.node.inputs[2].default_value = -1.0
    cos2 = M('SUBTRACT', M('MULTIPLY', c, c), M('MULTIPLY', s, s)); sin2 = M('MULTIPLY', M('MULTIPLY', c, s), 2.0)
    ks = M('MULTIPLY', sp.outputs[2], 4 * math.pi / pitch)
    stripe = M('ADD', M('MULTIPLY', cos2, M('COSINE', ks)), M('MULTIPLY', sin2, M('SINE', ks)))   # +1 on a ply's crown, -1 in the groove
    n2 = t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value = 320; n2.inputs["Detail"].default_value = 10; L(tc.outputs["Object"], n2.inputs["Vector"])
    n1 = t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value = 40; n1.inputs["Detail"].default_value = 4; L(tc.outputs["Object"], n1.inputs["Vector"])
    crown = M('MULTIPLY_ADD', stripe, 0.5); crown.node.inputs[2].default_value = 0.5
    tone = M('MULTIPLY_ADD', n1.node.outputs["Fac"] if False else n1.outputs["Fac"], 0.5); tone.node.inputs[2].default_value = 0.25
    shade = M('ADD', M('MULTIPLY', crown, 0.75), tone)
    col = ramp(t, L, shade, c0, c1, 0.15, 0.95)
    ov = t.nodes.new("ShaderNodeMix"); ov.data_type = 'RGBA'; ov.blend_type = 'OVERLAY'; ov.inputs[0].default_value = 0.35; L(col.outputs[0], ov.inputs[6]); L(n2.outputs["Fac"], ov.inputs[7])
    L(ov.outputs[2], b.inputs["Base Color"])
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = 0.8; rr.inputs[4].default_value = 0.97; L(n2.outputs["Fac"], rr.inputs[0]); L(rr.outputs[0], b.inputs["Roughness"])
    h = M('MULTIPLY_ADD', n2.outputs["Fac"], 0.15); L(crown, h.node.inputs[2])
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.9; bp.inputs["Distance"].default_value = 0.0012; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    m.diffuse_color = c1; return m

def make_mats():
    W = lambda n: grain(n, (0.47, 0.37, 0.25, 1), (0.2, 0.14, 0.085, 1), stretch=(1.5, 70, 70), rough=(0.74, 0.92), lines=0.85, bumpd=0.0016)
    sets = {
        "I_rough_stool": [W("item_rough_stool_wood"), bark_thin("item_rough_stool_bark")],
        "I_end_table": [W("item_end_table_wood"), bark_thin("item_end_table_bark")],
        "I_chair": [W("item_chair_wood"), bark_thin("item_chair_bark"), cord("item_chair_cord")],
        "I_table": [W("item_table_wood"), bark_thin("item_table_bark"), cord("item_table_cord")],
        "I_wooden_wheel": [endgrain("item_wooden_wheel_endgrain"), bark_thin("item_wooden_wheel_bark")],
        "I_arrows": [grain("item_arrows_shaft", (0.63, 0.51, 0.34, 1), (0.42, 0.32, 0.2, 1), stretch=(1.0, 90, 90), rough=(0.6, 0.8)),
                     lump("item_arrows_stone", (0.09, 0.09, 0.1, 1), (0.25, 0.24, 0.24, 1), rough=(0.35, 0.6), nscale=40, fine=200, bump=0.8, specks=(90, (0.45, 0.43, 0.4, 1), 0.7), bumpd=0.0004),
                     cord("item_arrows_cord")],
        "I_cordage": [rope("item_cordage_cord")],
        "I_tripod": [bark_thin("item_tripod_bark"), cord("item_tripod_cord")],
    }
    for on, mats in sets.items(): assign(D.objects[on], mats)
    return {k: [m.name for m in v] for k, v in sets.items()}
