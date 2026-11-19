
exec(open(r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work\item_util.py", encoding="utf-8").read())
def sepxyz(t, L, tc):
    s = t.nodes.new("ShaderNodeSeparateXYZ"); L(tc.outputs["Object"], s.inputs[0]); return s
def mrange(t, L, src, a, b, c=0.0, d=1.0, clamp=True):
    m = t.nodes.new("ShaderNodeMapRange"); m.inputs[1].default_value = a; m.inputs[2].default_value = b; m.inputs[3].default_value = c; m.inputs[4].default_value = d; m.clamp = clamp; L(src, m.inputs[0]); return m
def mix(t, L, fac, a, b, blend='MIX'):
    m = t.nodes.new("ShaderNodeMix"); m.data_type = 'RGBA'; m.blend_type = blend
    if isinstance(fac, (int, float)): m.inputs[0].default_value = fac
    else: L(fac, m.inputs[0])
    for sock, v in ((6, a), (7, b)):
        if isinstance(v, tuple): m.inputs[sock].default_value = v
        else: L(v, m.inputs[sock])
    return m.outputs[2]
def mth(t, L, op, a, b=None, c=None):
    m = t.nodes.new("ShaderNodeMath"); m.operation = op
    for i, v in enumerate((a, b, c)):
        if v is None: continue
        if isinstance(v, (int, float)): m.inputs[i].default_value = v
        else: L(v, m.inputs[i])
    return m.outputs[0]
def noiseN(t, L, tc, scale, detail=6, vec=None):
    n = t.nodes.new("ShaderNodeTexNoise"); n.inputs["Scale"].default_value = scale; n.inputs["Detail"].default_value = detail
    L(vec if vec is not None else tc.outputs["Object"], n.inputs["Vector"]); return n
SPECIES = {
    "Bass":     dict(back=(0.12, 0.15, 0.06, 1), side=(0.36, 0.38, 0.18, 1), belly=(0.72, 0.7, 0.56, 1), mark=(0.08, 0.1, 0.04, 1), kind="band"),
    "Bluegill": dict(back=(0.1, 0.16, 0.14, 1), side=(0.26, 0.34, 0.3, 1), belly=(0.78, 0.52, 0.18, 1), mark=(0.07, 0.1, 0.09, 1), kind="bars"),
    "Catfish":  dict(back=(0.12, 0.14, 0.16, 1), side=(0.32, 0.35, 0.38, 1), belly=(0.78, 0.78, 0.74, 1), mark=(0.07, 0.08, 0.09, 1), kind="speck"),
    "Crappie":  dict(back=(0.22, 0.26, 0.18, 1), side=(0.58, 0.6, 0.52, 1), belly=(0.8, 0.8, 0.74, 1), mark=(0.06, 0.07, 0.05, 1), kind="spots"),
}
def fish_mat(name, obj, sp, cooked):
    m, t, b, tc, L = base(name); s = sepxyz(t, L, tc)
    Lf = obj["L"]; xe, ye = obj["eye"]; P = SPECIES[sp]
    hy = mrange(t, L, s.outputs[1], -0.2 * Lf, -0.06 * Lf)
    upper = mrange(t, L, s.outputs[1], 0.03 * Lf, 0.16 * Lf)
    col = mix(t, L, hy.outputs[0], P["belly"], P["side"])
    col = mix(t, L, upper.outputs[0], col, P["back"])
    if P["kind"] == "band":
        nb = noiseN(t, L, tc, 30, 4)
        yb = mth(t, L, 'MULTIPLY_ADD', nb.outputs["Fac"], 0.02 * Lf / 0.34, s.outputs[1])
        band = mrange(t, L, mth(t, L, 'ABSOLUTE', mth(t, L, 'SUBTRACT', yb, 0.01 * Lf / 0.34)), 0.006, 0.016, 1.0, 0.0)
        blot = mrange(t, L, nb.outputs["Fac"], 0.42, 0.58)
        col = mix(t, L, mth(t, L, 'MULTIPLY', band.outputs[0], blot.outputs[0]), col, P["mark"])
    elif P["kind"] == "bars":
        bars = mth(t, L, 'SINE', mth(t, L, 'MULTIPLY', s.outputs[0], 2 * math.pi / 0.028))
        barm = mrange(t, L, bars, 0.3, 0.8)
        nb = noiseN(t, L, tc, 25, 3)
        col = mix(t, L, mth(t, L, 'MULTIPLY', mth(t, L, 'MULTIPLY', barm.outputs[0], 0.55), mrange(t, L, nb.outputs["Fac"], 0.35, 0.6).outputs[0]), col, P["mark"])
        dx = mth(t, L, 'SUBTRACT', s.outputs[0], Lf / 2 - 0.2 * Lf); dy = mth(t, L, 'SUBTRACT', s.outputs[1], 0.03 * Lf)
        dist = mth(t, L, 'SQRT', mth(t, L, 'ADD', mth(t, L, 'MULTIPLY', dx, dx), mth(t, L, 'MULTIPLY', dy, dy)))
        col = mix(t, L, mrange(t, L, dist, 0.012, 0.009).outputs[0], col, (0.03, 0.03, 0.05, 1))
    elif P["kind"] == "speck":
        sp_n = noiseN(t, L, tc, 160, 4)
        col = mix(t, L, mth(t, L, 'MULTIPLY', mrange(t, L, sp_n.outputs["Fac"], 0.62, 0.68).outputs[0], upper.outputs[0]), col, P["mark"])
    else:
        vo = t.nodes.new("ShaderNodeTexVoronoi"); vo.inputs["Scale"].default_value = 60; L(tc.outputs["Object"], vo.inputs["Vector"])
        nb = noiseN(t, L, tc, 18, 3)
        spots = mth(t, L, 'MULTIPLY', mrange(t, L, vo.outputs["Distance"], 0.22, 0.12).outputs[0], mrange(t, L, nb.outputs["Fac"], 0.45, 0.62).outputs[0])
        col = mix(t, L, mth(t, L, 'MULTIPLY', spots, mrange(t, L, s.outputs[1], -0.1 * Lf, 0.0).outputs[0]), col, P["mark"])
    gx = mth(t, L, 'SUBTRACT', s.outputs[0], mth(t, L, 'MULTIPLY_ADD', mth(t, L, 'MULTIPLY', s.outputs[1], s.outputs[1]), -3.0 / Lf, Lf / 2 - 0.17 * Lf))
    gill = mrange(t, L, mth(t, L, 'ABSOLUTE', gx), 0.0, 0.0025, 1.0, 0.0)
    col = mix(t, L, mth(t, L, 'MULTIPLY', gill.outputs[0], 0.7), col, (0.06, 0.04, 0.04, 1))
    if cooked:
        n1 = noiseN(t, L, tc, 14, 6)
        brown = mix(t, L, n1.outputs["Fac"], (0.14, 0.065, 0.025, 1), (0.36, 0.19, 0.07, 1))
        col = mix(t, L, 0.88, col, brown)
        g = mth(t, L, 'SINE', mth(t, L, 'MULTIPLY', mth(t, L, 'ADD', s.outputs[0], mth(t, L, 'MULTIPLY', s.outputs[1], 0.8)), 2 * math.pi / 0.05))
        grill = mrange(t, L, g, 0.93, 0.99)
        col = mix(t, L, mth(t, L, 'MULTIPLY', mth(t, L, 'MULTIPLY', grill.outputs[0], 0.75), mrange(t, L, noiseN(t, L, tc, 40, 3).outputs["Fac"], 0.4, 0.6).outputs[0]), col, (0.04, 0.025, 0.015, 1))
    ex = mth(t, L, 'SUBTRACT', s.outputs[0], xe); ey = mth(t, L, 'SUBTRACT', s.outputs[1], ye)
    ed = mth(t, L, 'SQRT', mth(t, L, 'ADD', mth(t, L, 'MULTIPLY', ex, ex), mth(t, L, 'MULTIPLY', ey, ey)))
    er = obj.get("eye_r", 0.022) * Lf
    ring = mrange(t, L, ed, er * 1.15, er * 1.0)
    pupil = mrange(t, L, ed, er * 0.62, er * 0.52)
    col = mix(t, L, ring.outputs[0], col, (0.55, 0.42, 0.12, 1) if not cooked else (0.82, 0.8, 0.72, 1))
    col = mix(t, L, pupil.outputs[0], col, (0.01, 0.01, 0.01, 1) if not cooked else (0.75, 0.72, 0.64, 1))
    L(col, b.inputs["Base Color"])
    k = Lf / 0.34
    mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (1.0 / (0.012 * k), 1.0 / (0.009 * k), 1.0 / (0.009 * k)); L(tc.outputs["Object"], mp.inputs[0])
    vs = t.nodes.new("ShaderNodeTexVoronoi"); vs.feature = 'DISTANCE_TO_EDGE'; vs.inputs["Scale"].default_value = 1.0; L(mp.outputs[0], vs.inputs["Vector"])
    nf = noiseN(t, L, tc, 200, 8)
    h = mth(t, L, 'MULTIPLY_ADD', vs.outputs["Distance"], 0.5, nf.outputs["Fac"])
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = 0.35 if not cooked else 0.6; bp.inputs["Distance"].default_value = 0.0006; L(h, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    L(mrange(t, L, nf.outputs["Fac"], 0.3, 0.7, 0.22 if not cooked else 0.45, 0.38 if not cooked else 0.65).outputs[0], b.inputs["Roughness"])
    m.diffuse_color = P["side"]; return m
def flesh(name, c0, c1, rough, nscale=10, goose=False, streak=None, topdark=None, specks=None, bumpd=0.0008, bstr=0.4):
    m, t, b, tc, L = base(name); s = sepxyz(t, L, tc)
    n1 = noiseN(t, L, tc, nscale, 8)
    col = mix(t, L, mrange(t, L, n1.outputs["Fac"], 0.3, 0.7).outputs[0], c0, c1)
    if streak:
        mp = t.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = streak[1]; L(tc.outputs["Object"], mp.inputs[0])
        ns = noiseN(t, L, tc, 1.0, 5, vec=mp.outputs[0])
        col = mix(t, L, mrange(t, L, ns.outputs["Fac"], 0.6, 0.72).outputs[0], col, streak[0])
    if topdark:
        col = mix(t, L, mrange(t, L, s.outputs[2], topdark[1], topdark[2]).outputs[0], col, topdark[0])
    if specks:
        nsp = noiseN(t, L, tc, specks[0], 4)
        col = mix(t, L, mrange(t, L, nsp.outputs["Fac"], specks[2], specks[2] + 0.06).outputs[0], col, specks[1])
    L(col, b.inputs["Base Color"])
    nf = noiseN(t, L, tc, 160, 10); hsrc = nf.outputs["Fac"]
    if goose:
        vo = t.nodes.new("ShaderNodeTexVoronoi"); vo.inputs["Scale"].default_value = 260; L(tc.outputs["Object"], vo.inputs["Vector"])
        hsrc = mth(t, L, 'MULTIPLY_ADD', vo.outputs["Distance"], -0.6, nf.outputs["Fac"])
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = bstr; bp.inputs["Distance"].default_value = bumpd; L(hsrc, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])
    L(mrange(t, L, nf.outputs["Fac"], 0.3, 0.7, rough[0], rough[1]).outputs[0], b.inputs["Roughness"])
    m.diffuse_color = c1; return m
