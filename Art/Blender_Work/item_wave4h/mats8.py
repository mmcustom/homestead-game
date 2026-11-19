
# Wave 4 batch 8 materials (2026-11-19)
import bpy, math
D = bpy.data

def M(t, L, op, a, b=None, c=None):
    n = t.nodes.new("ShaderNodeMath"); n.operation = op
    for i, v in enumerate((a, b, c)):
        if v is None: continue
        if isinstance(v, (int, float)): n.inputs[i].default_value = v
        else: L(v, n.inputs[i])
    return n.outputs[0]

def xyz(t, L, tc):
    s = t.nodes.new("ShaderNodeSeparateXYZ"); L(tc.outputs["Object"], s.inputs[0]); return s.outputs

def finish(t, L, b, col, rough, height, bumpd=0.001, strength=0.5):
    L(col, b.inputs["Base Color"]); L(rough, b.inputs["Roughness"])
    bp = t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value = strength; bp.inputs["Distance"].default_value = bumpd; L(height, bp.inputs["Height"]); L(bp.outputs[0], b.inputs["Normal"])

def noiseo(t, L, tc, sc, det=8):
    n = t.nodes.new("ShaderNodeTexNoise"); n.inputs["Scale"].default_value = sc; n.inputs["Detail"].default_value = det; L(tc.outputs["Object"], n.inputs["Vector"]); return n.outputs["Fac"]

def mixc(t, L, fac, a, b):
    m = t.nodes.new("ShaderNodeMix"); m.data_type = 'RGBA'
    if isinstance(fac, float): m.inputs[0].default_value = fac
    else: L(fac, m.inputs[0])
    for i, v in ((6, a), (7, b)):
        if isinstance(v, tuple): m.inputs[i].default_value = v
        else: L(v, m.inputs[i])
    return m.outputs[2]

def flat(name, col, rough):
    m, t, b, tc, L = base(name); b.inputs["Base Color"].default_value = col; b.inputs["Roughness"].default_value = rough; m.diffuse_color = col; return m

def glaze2(name, top, bot, split=0.066):
    m, t, b, tc, L = base(name); x, y, z = xyz(t, L, tc)
    n1 = noiseo(t, L, tc, 30, 4); n2 = noiseo(t, L, tc, 200, 8)
    edge = M(t, L, 'ADD', z, M(t, L, 'MULTIPLY', M(t, L, 'SUBTRACT', n1, 0.5), 0.02))
    f = ramp(t, L, edge, (0, 0, 0, 1), (1, 1, 1, 1), split - 0.003, split + 0.003)
    lo = ramp(t, L, n1, bot[0], bot[1], 0.3, 0.7); hi = ramp(t, L, n2, top[0], top[1], 0.3, 0.75)
    col = mixc(t, L, f.outputs[0], lo.outputs[0], hi.outputs[0])
    rough = M(t, L, 'MULTIPLY_ADD', f.outputs[0], -0.35, 0.62)
    finish(t, L, b, col, rough, n2, 0.0004, 0.35); m.diffuse_color = top[1]; return m

def spiral(name, c0, c1, R, pitch, line=(0.05, 0.045, 0.035, 1)):
    m, t, b, tc, L = base(name); x, y, z = xyz(t, L, tc)
    zz = M(t, L, 'SUBTRACT', z, R); r = M(t, L, 'SQRT', M(t, L, 'ADD', M(t, L, 'MULTIPLY', y, y), M(t, L, 'MULTIPLY', zz, zz)))
    a = M(t, L, 'DIVIDE', M(t, L, 'ARCTAN2', zz, y), 2 * math.pi)
    v = M(t, L, 'FRACT', M(t, L, 'ADD', a, M(t, L, 'DIVIDE', r, pitch)))
    ln = ramp(t, L, v, (1, 1, 1, 1), (0, 0, 0, 1), 0.0, 0.12)
    n2 = noiseo(t, L, tc, 260, 8); cloth = ramp(t, L, noiseo(t, L, tc, 25, 5), c0, c1, 0.3, 0.7)
    col = mixc(t, L, ln.outputs[0], cloth.outputs[0], line)
    h = M(t, L, 'MULTIPLY_ADD', ln.outputs[0], -1.0, 1.0); h = M(t, L, 'MULTIPLY_ADD', n2, 0.2, h)
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = 0.85; rr.inputs[4].default_value = 0.97; L(n2, rr.inputs[0])
    finish(t, L, b, col, rr.outputs[0], h, 0.002, 0.8); m.diffuse_color = c1; return m

def canvas(name, c0, c1):
    m, t, b, tc, L = base(name)
    wv = t.nodes.new("ShaderNodeTexWave"); wv.inputs["Scale"].default_value = 900; wv.inputs["Distortion"].default_value = 0.4; L(tc.outputs["Object"], wv.inputs["Vector"])
    wv2 = t.nodes.new("ShaderNodeTexWave"); wv2.bands_direction = 'Y'; wv2.inputs["Scale"].default_value = 900; wv2.inputs["Distortion"].default_value = 0.4; L(tc.outputs["Object"], wv2.inputs["Vector"])
    weave = M(t, L, 'MULTIPLY', wv.outputs["Fac"], wv2.outputs["Fac"])
    n1 = noiseo(t, L, tc, 12, 6); n3 = noiseo(t, L, tc, 60, 8)
    col = ramp(t, L, M(t, L, 'MULTIPLY_ADD', n3, 0.3, M(t, L, 'MULTIPLY', n1, 0.8)), c0, c1, 0.25, 0.7)
    col2 = mixc(t, L, M(t, L, 'MULTIPLY', weave, 0.0), col.outputs[0], c0)
    rr = t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value = 0.85; rr.inputs[4].default_value = 0.97; L(n3, rr.inputs[0])
    finish(t, L, b, col2, rr.outputs[0], n3, 0.0008, 0.5); m.diffuse_color = c1; return m

def dial(name, cx, cy):
    m, t, b, tc, L = base(name); x, y, z = xyz(t, L, tc)
    dx = M(t, L, 'SUBTRACT', x, cx); dy = M(t, L, 'SUBTRACT', y, cy)
    r = M(t, L, 'SQRT', M(t, L, 'ADD', M(t, L, 'MULTIPLY', dx, dx), M(t, L, 'MULTIPLY', dy, dy)))
    a = M(t, L, 'DIVIDE', M(t, L, 'ARCTAN2', dy, dx), 2 * math.pi)
    tick = ramp(t, L, M(t, L, 'FRACT', M(t, L, 'MULTIPLY', a, 48)), (1, 1, 1, 1), (0, 0, 0, 1), 0.0, 0.14)
    big = ramp(t, L, M(t, L, 'FRACT', M(t, L, 'MULTIPLY', a, 8)), (1, 1, 1, 1), (0, 0, 0, 1), 0.0, 0.05)
    band1 = M(t, L, 'MULTIPLY', M(t, L, 'GREATER_THAN', r, 0.03), M(t, L, 'LESS_THAN', r, 0.037))
    band2 = M(t, L, 'MULTIPLY', M(t, L, 'GREATER_THAN', r, 0.024), M(t, L, 'LESS_THAN', r, 0.037))
    ring = M(t, L, 'MULTIPLY', M(t, L, 'GREATER_THAN', r, 0.0375), M(t, L, 'LESS_THAN', r, 0.0385))
    ring2 = M(t, L, 'MULTIPLY', M(t, L, 'GREATER_THAN', r, 0.0285), M(t, L, 'LESS_THAN', r, 0.0292))
    ink = M(t, L, 'MAXIMUM', M(t, L, 'MAXIMUM', M(t, L, 'MULTIPLY', tick.outputs[0], band1), M(t, L, 'MULTIPLY', big.outputs[0], band2)), M(t, L, 'MAXIMUM', ring, ring2))
    n2 = noiseo(t, L, tc, 300, 6)
    paper = ramp(t, L, n2, (0.74, 0.69, 0.57, 1), (0.84, 0.8, 0.69, 1), 0.3, 0.7)
    col = mixc(t, L, ink, paper.outputs[0], (0.03, 0.03, 0.03, 1))
    finish(t, L, b, col, M(t, L, 'MULTIPLY_ADD', n2, 0.1, 0.25), n2, 0.0002, 0.1); m.diffuse_color = (0.8, 0.76, 0.65, 1); return m

def thermo(name, tx):
    m, t, b, tc, L = base(name); x, y, z = xyz(t, L, tc)
    core = M(t, L, 'MULTIPLY', M(t, L, 'LESS_THAN', M(t, L, 'ABSOLUTE', M(t, L, 'SUBTRACT', x, tx)), 0.0013), M(t, L, 'LESS_THAN', y, 0.012))
    red = M(t, L, 'MAXIMUM', core, M(t, L, 'LESS_THAN', y, -0.0405))
    col = mixc(t, L, red, (0.72, 0.75, 0.76, 1), (0.45, 0.03, 0.02, 1))
    finish(t, L, b, col, M(t, L, 'MULTIPLY_ADD', red, 0.15, 0.08), red, 0.0002, 0.1); m.diffuse_color = (0.7, 0.72, 0.74, 1); return m

def tin(name):
    m, t, b, tc, L = base(name)
    n1 = noiseo(t, L, tc, 18, 8); n2 = noiseo(t, L, tc, 160, 10)
    wear = ramp(t, L, M(t, L, 'MULTIPLY_ADD', n2, 0.35, n1), (0, 0, 0, 1), (1, 1, 1, 1), 0.79, 0.83)
    paint = ramp(t, L, n1, (0.22, 0.04, 0.03, 1), (0.36, 0.07, 0.045, 1), 0.3, 0.7)
    rust = ramp(t, L, n2, (0.22, 0.12, 0.06, 1), (0.4, 0.33, 0.27, 1), 0.4, 0.6)
    col = mixc(t, L, wear.outputs[0], paint.outputs[0], rust.outputs[0])
    finish(t, L, b, col, M(t, L, 'MULTIPLY_ADD', wear.outputs[0], 0.25, 0.5), M(t, L, 'MULTIPLY_ADD', wear.outputs[0], 0.4, n2), 0.0005, 0.4); m.diffuse_color = (0.33, 0.06, 0.04, 1); return m

def leather(name, c0=(0.08, 0.04, 0.02, 1), c1=(0.22, 0.12, 0.06, 1)):
    return lump(name, c0, c1, rough=(0.5, 0.75), nscale=30, fine=300, bump=0.35, bumpd=0.0005)

def cordm(name):
    return lump(name, (0.33, 0.26, 0.13, 1), (0.56, 0.47, 0.27, 1), rough=(0.82, 0.97), nscale=60, fine=260, bump=0.6, grain_=True, bumpd=0.0006)

def metal(name, c0, c1, rough=(0.3, 0.5)):
    return lump(name, c0, c1, rough=rough, nscale=40, fine=250, bump=0.2, metal=1.0, bumpd=0.0003)

CLAY = lambda n: lump(n, (0.4, 0.28, 0.19, 1), (0.58, 0.44, 0.31, 1), rough=(0.78, 0.94), nscale=14, fine=180, bump=0.35, specks=(140, (0.28, 0.2, 0.14, 1), 0.72), bumpd=0.0006)
CORK = lambda n: lump(n, (0.36, 0.25, 0.14, 1), (0.6, 0.45, 0.28, 1), rough=(0.85, 0.98), nscale=50, fine=380, bump=0.8, specks=(220, (0.18, 0.12, 0.07, 1), 0.68), bumpd=0.0004)
SURF = {"excellent": ((0.012, 0.03, 0.04, 1), 0.04), "good": ((0.02, 0.04, 0.035, 1), 0.05), "questionable": ((0.07, 0.075, 0.04, 1), 0.12),
        "unsafe": ((0.12, 0.08, 0.035, 1), 0.25), "sap": ((0.07, 0.065, 0.04, 1), 0.06)}

def make_mats():
    jar = CLAY("item_water_jar_clay")
    S = {k: flat("item_water_surface_" + k, c, r) for k, (c, r) in SURF.items()}
    sets = {
        "I_water": [jar, CORK("item_water_cork")],
        "I_water_excellent": [jar, S["excellent"]], "I_water_good": [jar, S["good"]], "I_water_questionable": [jar, S["questionable"]],
        "I_water_unsafe": [jar, S["unsafe"]], "I_maple_sap": [jar, S["sap"]],
        "I_maple_syrup": [glaze2("item_maple_syrup_glaze", ((0.05, 0.028, 0.015, 1), (0.12, 0.065, 0.03, 1)), ((0.5, 0.38, 0.25, 1), (0.66, 0.53, 0.37, 1))), CORK("item_maple_syrup_cork")],
        "I_lye": [lump("item_lye_crock", (0.36, 0.35, 0.32, 1), (0.55, 0.53, 0.48, 1), rough=(0.35, 0.6), nscale=12, fine=420, bump=0.45, specks=(90, (0.3, 0.22, 0.14, 1), 0.74), bumpd=0.0004),
                  grain("item_lye_lid", (0.47, 0.37, 0.25, 1), (0.2, 0.14, 0.085, 1), stretch=(1.5, 70, 70), rough=(0.74, 0.92), lines=0.85), cordm("item_lye_cord")],
        "I_lamp_oil": [tin("item_lamp_oil_tin"), metal("item_lamp_oil_iron", (0.08, 0.075, 0.07, 1), (0.2, 0.19, 0.17, 1), rough=(0.45, 0.7))],
        "I_potash": [lump("item_potash_salt", (0.3, 0.3, 0.31, 1), (0.58, 0.57, 0.56, 1), rough=(0.7, 0.9), nscale=40, fine=300, bump=0.9, specks=(120, (0.42, 0.36, 0.33, 1), 0.66), bumpd=0.0008)],
        "I_rifle_rounds": [metal("item_rifle_rounds_brass", (0.55, 0.37, 0.13, 1), (0.8, 0.6, 0.28, 1), rough=(0.28, 0.45)), metal("item_rifle_rounds_copper", (0.55, 0.25, 0.14, 1), (0.75, 0.4, 0.25, 1), rough=(0.3, 0.45))],
        "I_sleeping_bag": [canvas("item_sleeping_bag_cloth", (0.12, 0.13, 0.07, 1), (0.25, 0.26, 0.15, 1)), spiral("item_sleeping_bag_end", (0.12, 0.13, 0.07, 1), (0.25, 0.26, 0.15, 1), 0.105, 0.022),
                           leather("item_sleeping_bag_leather"), metal("item_sleeping_bag_iron", (0.1, 0.095, 0.09, 1), (0.25, 0.24, 0.22, 1))],
        "I_tent": [canvas("item_tent_canvas", (0.38, 0.32, 0.22, 1), (0.6, 0.53, 0.4, 1)), spiral("item_tent_end", (0.38, 0.32, 0.22, 1), (0.6, 0.53, 0.4, 1), 0.08, 0.011),
                   cordm("item_tent_cord"), grain("item_tent_poles", (0.6, 0.48, 0.32, 1), (0.36, 0.26, 0.16, 1), stretch=(1.0, 90, 90), rough=(0.6, 0.8))],
        "I_weather_station": [grain("item_weather_station_wood", (0.27, 0.15, 0.075, 1), (0.1, 0.05, 0.025, 1), stretch=(1.5, 90, 90), rough=(0.32, 0.5), lines=0.9),
                              metal("item_weather_station_brass", (0.55, 0.38, 0.14, 1), (0.82, 0.62, 0.3, 1), rough=(0.25, 0.4)),
                              dial("item_weather_station_dial", -0.04, 0.0), thermo("item_weather_station_glass", 0.062), flat("item_weather_station_needle", (0.02, 0.02, 0.02, 1), 0.4)],
    }
    for on, mats in sets.items(): assign(D.objects[on], mats)
    return sorted({m.name for v in sets.values() for m in v})
