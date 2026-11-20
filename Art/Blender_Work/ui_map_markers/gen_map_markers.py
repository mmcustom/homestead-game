# Homestead map markers (Wave 4 map icons, 2026-11-20): the minimap and world map discovery markers as art, matching
# MapIcons' colours and layout (a disc in the category colour, a dark rim from 84% of the radius, a cream glyph in the
# inner 62%), with shading and glyph detail. Drawn at 8x and reduced to 128 x 128 px.
import numpy as np, os
from PIL import Image, ImageDraw, ImageFilter

OUT = "/home/claude/trees/map/"
os.makedirs(OUT, exist_ok=True)
S = 1024                     # working size
FINAL = 128
OUTLINE = (23, 31, 23)
GLYPH = (242, 235, 209)
COLOURS = {                  # MapIcons.CategoryColor, as 0-255 sRGB
    "WaterSource": (77, 143, 194), "Plant": (133, 69, 128), "Wildlife": (140, 102, 66),
    "Fishing": (61, 140, 128), "PropertyFeature": (199, 143, 61),
}

def P(x, y):                 # glyph space -1..1 (y up) -> pixels, inside the inner 62% like MapIcons
    k = 0.62 * S / 2
    return (S / 2 + x * k, S / 2 - y * k)

def R(r): return r * 0.62 * S / 2

def ellipse(d, cx, cy, rx, ry, fill):
    x0, y0 = P(cx - rx, cy + ry); x1, y1 = P(cx + rx, cy - ry); d.ellipse([x0, y0, x1, y1], fill=fill)

def poly(d, pts, fill): d.polygon([P(x, y) for x, y in pts], fill=fill)

def line(d, pts, w, fill): d.line([P(x, y) for x, y in pts], fill=fill, width=int(R(w)), joint="curve")

CUT = 0                      # glyph mask: 255 = glyph, 0 = cut out (shows the disc colour)

def glyph_water(d):
    ellipse(d, 0, -0.25, 0.5, 0.5, 255)
    poly(d, [(0, 0.9), (-0.44, -0.08), (0.44, -0.08)], 255)
    ellipse(d, -0.2, -0.32, 0.1, 0.17, CUT)                      # a highlight
def glyph_berry(d):
    for cx, cy in ((-0.3, -0.32), (0.3, -0.32), (0, 0.14)):
        ellipse(d, cx, cy, 0.33, 0.33, 255)
        ellipse(d, cx - 0.12, cy + 0.1, 0.07, 0.07, CUT)
    line(d, [(0.0, 0.42), (0.08, 0.82)], 0.13, 255)
    poly(d, [(0.08, 0.7), (0.5, 0.92), (0.62, 0.62), (0.28, 0.56)], 255)   # a leaf
def glyph_track(d):
    # a deer's cloven hoof print: two rounded halves, pointed toward the top, a narrow gap between them
    for sx in (-1, 1):
        ellipse(d, sx * 0.27, -0.18, 0.24, 0.5, 255)
        poly(d, [(sx * 0.06, -0.1), (sx * 0.48, -0.1), (sx * 0.16, 0.78)], 255)
    poly(d, [(-0.05, -0.9), (0.05, -0.9), (0.05, 0.9), (-0.05, 0.9)], CUT)
def glyph_fish(d):
    ellipse(d, -0.15, 0, 0.6, 0.34, 255)
    poly(d, [(0.3, 0), (0.88, 0.45), (0.75, 0), (0.88, -0.45)], 255)
    poly(d, [(-0.35, 0.28), (-0.05, 0.5), (0.12, 0.28)], 255)     # dorsal fin
    ellipse(d, -0.5, 0.07, 0.08, 0.08, CUT)                      # eye
    line(d, [(-0.32, 0.25), (-0.26, -0.25)], 0.05, CUT)          # gill line
def glyph_cabin(d):
    poly(d, [(-0.5, -0.72), (0.5, -0.72), (0.5, 0.08), (-0.5, 0.08)], 255)
    poly(d, [(0, 0.82), (-0.8, 0.02), (0.8, 0.02)], 255)
    poly(d, [(0.36, 0.4), (0.36, 0.78), (0.54, 0.78), (0.54, 0.58)], 255)   # chimney
    poly(d, [(-0.14, -0.72), (0.14, -0.72), (0.14, -0.22), (-0.14, -0.22)], CUT)   # door
    poly(d, [(0.24, -0.2), (0.4, -0.2), (0.4, -0.04), (0.24, -0.04)], CUT)        # window
def glyph_apple(d):
    ellipse(d, -0.2, -0.14, 0.44, 0.48, 255); ellipse(d, 0.2, -0.14, 0.44, 0.48, 255)
    ellipse(d, 0, 0.26, 0.14, 0.08, CUT)
    line(d, [(0, 0.24), (0.06, 0.66)], 0.1, 255)
    poly(d, [(0.08, 0.55), (0.52, 0.72), (0.5, 0.46), (0.2, 0.42)], 255)
    ellipse(d, -0.3, -0.05, 0.08, 0.16, CUT)
def glyph_acorn(d):
    ellipse(d, 0, -0.22, 0.36, 0.52, 255)
    poly(d, [(0, -0.92), (-0.12, -0.62), (0.12, -0.62)], 255)
    ellipse(d, 0, 0.28, 0.58, 0.28, 255)
    poly(d, [(-0.7, 0.1), (0.7, 0.1), (0.7, 0.24), (-0.7, 0.24)], CUT)
    ellipse(d, 0, 0.18, 0.56, 0.06, 255)
    line(d, [(-0.3, 0.38), (0.3, 0.38)], 0.04, CUT)                # cap texture
    line(d, [(0, 0.5), (0.05, 0.78)], 0.1, 255)
def glyph_leaf(d):
    pts = []
    for t in np.linspace(0, 1, 40):
        w = 0.36 * np.sin(np.pi * t) ** 0.8
        pts.append((t, w))
    outline = [(-0.6 + 1.3 * t, -0.6 + 1.3 * t) for t, _ in pts]
    left = [(-0.6 + 1.3 * t - w * 0.7071, -0.6 + 1.3 * t + w * 0.7071) for t, w in pts]
    right = [(-0.6 + 1.3 * t + w * 0.7071, -0.6 + 1.3 * t - w * 0.7071) for t, w in pts]
    poly(d, left + right[::-1], 255)
    line(d, [(-0.85, -0.85), (0.62, 0.62)], 0.06, CUT)           # midrib (and a stem below the leaf)
    line(d, [(-0.85, -0.85), (-0.6, -0.6)], 0.09, 255)
    for t in (0.3, 0.5, 0.7):
        bx, by = -0.6 + 1.3 * t, -0.6 + 1.3 * t
        line(d, [(bx, by), (bx - 0.18, by + 0.28)], 0.04, CUT)
        line(d, [(bx, by), (bx + 0.28, by - 0.18)], 0.04, CUT)
def glyph_mushroom(d):
    ellipse(d, 0, 0.08, 0.76, 0.56, 255)
    poly(d, [(-0.8, 0.08), (0.8, 0.08), (0.8, -0.6), (-0.8, -0.6)], CUT)
    poly(d, [(-0.2, 0.12), (0.2, 0.12), (0.26, -0.74), (-0.26, -0.74)], 255)
    for cx, cy, r in ((-0.36, 0.3, 0.12), (0.18, 0.44, 0.1), (0.42, 0.22, 0.08)):
        ellipse(d, cx, cy, r, r, CUT)
    line(d, [(-0.55, 0.06), (0.55, 0.06)], 0.05, CUT)

MARKERS = {
    "WaterSource": ("WaterSource", glyph_water), "Plant": ("Plant", glyph_berry), "Wildlife": ("Wildlife", glyph_track),
    "Fishing": ("Fishing", glyph_fish), "PropertyFeature": ("PropertyFeature", glyph_cabin),
    "Plant_fruit": ("Plant", glyph_apple), "Plant_nut": ("Plant", glyph_acorn), "Plant_greens": ("Plant", glyph_leaf),
    "Plant_mushroom": ("Plant", glyph_mushroom),
}

def marker(name, category, glyph):
    yy, xx = np.mgrid[0:S, 0:S].astype(float)
    x = (xx + 0.5) / S * 2 - 1; y = 1 - (yy + 0.5) / S * 2
    r = np.hypot(x, y)
    base = np.array(COLOURS[category], float)
    # disc: lit from the top left, a little darker toward the rim
    light = 1.08 - 0.18 * np.clip((x * 0.55 - y * 0.85 + 1) / 2, 0, 1) - 0.1 * np.clip(r / 0.84, 0, 1) ** 3
    img = np.clip(base[None, None] * light[..., None], 0, 255)
    rim_hi = (r > 0.79) & (r <= 0.84) & (y > 0.1)                # a thin highlight inside the rim, on top
    img[rim_hi] = img[rim_hi] * 0.75 + 255 * 0.25
    img[r > 0.84] = OUTLINE
    alpha = (r <= 1.0).astype(float) * 255
    # glyph mask, with a soft shadow down and right
    m = Image.new("L", (S, S), 0); glyph(ImageDraw.Draw(m))
    mask = np.asarray(m, float) / 255
    sh = np.asarray(m.filter(ImageFilter.GaussianBlur(S * 0.012)), float) / 255
    sh = np.roll(np.roll(sh, int(S * 0.012), 0), int(S * 0.01), 1) * (r <= 0.84)
    img = img * (1 - 0.45 * sh[..., None])
    img = img * (1 - mask[..., None]) + np.array(GLYPH, float)[None, None] * mask[..., None]
    out = Image.fromarray(np.dstack([img, alpha]).astype(np.uint8), "RGBA").resize((FINAL, FINAL), Image.LANCZOS)
    out.save(OUT + f"map_marker_{name}.png")
    return out

if __name__ == "__main__":
    sheet = Image.new("RGBA", (FINAL * len(MARKERS), FINAL), (120, 130, 110, 255))
    for i, (name, (cat, g)) in enumerate(MARKERS.items()):
        sheet.alpha_composite(marker(name, cat, g), (i * FINAL, 0))
    sheet.convert("RGB").save(OUT + "preview.png")
