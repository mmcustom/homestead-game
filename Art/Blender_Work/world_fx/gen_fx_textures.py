# Homestead weather and fire sprites (Wave 6, 2026-11-19). All white/grey so the materials' colours tint them.
#   raindrop (32x128): a thin streak, soft across, tapered, the head at the bottom a little brighter.
#   snowflake (64x64): a soft clump of flakes with a fluffy edge.
#   lightning_bolt (128x32): the bolt's cross-section for the LineRenderer (u along the bolt, v across): hot core, glow.
#   smoke_puff (128x128): a billowy puff, lit from above, soft edge.
#   flame_sheet (1024x1024, 4x4 frames, premultiplied alpha): rising, flickering tongues of flame for the campfire.
import numpy as np, os
from PIL import Image
from scipy.ndimage import gaussian_filter, map_coordinates

OUT = "/home/claude/trees/fx/"
os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(90)

def to8(a): return (np.clip(a, 0, 1) * 255 + 0.5).astype(np.uint8)
def rgba(name, rgb, a):
    Image.fromarray(np.dstack([to8(rgb), to8(a)]), "RGBA").save(OUT + name + ".png")

def noise(h, w, sigma, seed):
    r = np.random.default_rng(seed)
    n = gaussian_filter(r.standard_normal((h, w)), sigma, mode="wrap")
    return n / (n.std() + 1e-9)

def raindrop():
    # As wide and as solid as the drop it replaced (16x64, alpha mean about 0.22): rain particles are only 2 cm across,
    # so a thin streak inside the sprite falls below a pixel and the rain disappears.
    h, w = 128, 32
    yy, xx = np.mgrid[0:h, 0:w].astype(float)
    t = yy / (h - 1)                                   # 0 top (tail) .. 1 bottom (head)
    across = np.exp(-((xx - w / 2 + 0.5) / (w * 0.2)) ** 2)
    along = np.clip(t * 4, 0, 1) * np.clip((1 - t) * 6, 0, 1)
    a = across * along * (0.75 + 0.25 * t)
    rgb = np.dstack([np.ones_like(a) * 0.92, np.ones_like(a) * 0.95, np.ones_like(a)])
    rgba("Raindrop", rgb, a)

def snowflake():
    s = 64
    yy, xx = np.mgrid[0:s, 0:s].astype(float)
    a = np.zeros((s, s))
    for i in range(7):                                  # a few overlapping crystals
        cx, cy = s / 2 + rng.normal(0, 5, 2)
        r = rng.uniform(5, 11)
        d = np.hypot(xx - cx, yy - cy) / r
        a = np.maximum(a, np.clip(1 - d, 0, 1) ** 1.3)
    a *= np.clip(0.75 + 0.35 * noise(s, s, 1.2, 91), 0, 1)
    a = gaussian_filter(a, 0.8)
    d = np.hypot(xx - s / 2, yy - s / 2) / (s / 2)
    a *= np.clip((1 - d) * 3, 0, 1)                     # nothing at the sprite's edge
    shade = 0.9 + 0.1 * a
    rgba("Snowflake", np.dstack([shade, shade, shade * 1.02]), np.clip(a * 1.3, 0, 1))

def bolt():
    h, w = 32, 128
    yy, xx = np.mgrid[0:h, 0:w].astype(float)
    v = (yy - h / 2 + 0.5) / (h / 2)                    # -1..1 across
    flick = 1 + 0.25 * noise(1, w, 2.0, 92)[0][None, :]
    core = np.exp(-(v / 0.12) ** 2)
    glow = np.exp(-(v / 0.5) ** 2) * 0.45 * flick
    a = np.clip(core + glow, 0, 1) * np.clip(1 - np.abs(v) ** 6, 0, 1)
    white = np.clip(core * 1.2, 0, 1)
    rgb = np.dstack([0.7 + 0.3 * white, 0.78 + 0.22 * white, np.ones_like(white)]) * a[..., None]   # premultiplied
    rgba("LightningBolt", rgb, a)

def smoke_puff():
    s = 128
    yy, xx = np.mgrid[0:s, 0:s].astype(float)
    a = np.zeros((s, s))
    for i in range(9):                                   # lumps that make a billow
        cx, cy = s / 2 + rng.normal(0, 13, 2)
        r = rng.uniform(18, 32)
        d = np.hypot(xx - cx, yy - cy) / r
        a = np.maximum(a, np.clip(1 - d, 0, 1))
    a = a ** 1.2 * np.clip(0.85 + 0.2 * noise(s, s, 3.0, 93) + 0.08 * noise(s, s, 1.2, 94), 0.5, 1.2)
    d = np.hypot(xx - s / 2, yy - s / 2) / (s / 2)
    a = gaussian_filter(np.clip(a, 0, 1) * np.clip((1 - d) * 2.5, 0, 1), 1.0)
    # lit from above: the top of each lump lighter, the underside grey
    lit = np.clip(0.75 + 0.25 * (gaussian_filter(a, 4) - np.roll(gaussian_filter(a, 4), 6, 0)) * 4 + 0.1 * (1 - yy / s), 0.55, 1)
    rgba("fx_smoke_puff", np.dstack([lit, lit, lit * 0.98]), np.clip(a, 0, 1))

def flame_sheet():
    F, cols = 256, 4
    frames = cols * cols
    sheet_rgb = np.zeros((F * cols, F * cols, 3)); sheet_a = np.zeros((F * cols, F * cols))
    # a looping turbulence field: two noise layers scrolled upward, cross-faded so frame 16 meets frame 1
    big = [noise(F * 2, F, s, 95 + k) for k, s in enumerate((10, 5, 2.5))]
    yy, xx = np.mgrid[0:F, 0:F].astype(float)
    u = (xx - F / 2) / (F / 2)          # -1..1 across
    v = 1 - yy / (F - 1)                # 0 bottom .. 1 top
    for f in range(frames):
        ph = f / frames
        def field(ofs):
            out = np.zeros((F, F))
            for k, (n, wgt) in enumerate(zip(big, (0.6, 0.3, 0.15))):
                shift = int(((ph + ofs) * F * 2 * (k + 1) * 0.5)) % (F * 2)
                rows = (np.arange(F) + shift) % (F * 2)
                out += n[rows] * wgt
            return out
        turb = field(0) * (1 - ph) + field(-1) * ph
        turb2 = np.roll(field(0.37), F // 3, 1) * (1 - ph) + np.roll(field(-0.63), F // 3, 1) * ph
        turb = turb - turb.mean(); turb2 = turb2 - turb2.mean()
        # sample a teardrop flame through coordinates bent by the turbulence (more bending higher up)
        xd = u + 0.22 * v * turb
        yd = v + 0.12 * v * turb2
        width = 0.5 * np.clip(1 - yd, 0, 1) ** 0.65 + 0.02
        body = np.clip(1 - np.abs(xd) / width, 0, 1) ** 0.8
        tip = np.clip((0.95 - yd) * 3, 0, 1)
        a = body * tip * np.clip(v * 10, 0, 1)
        a *= np.clip(1.15 + 0.35 * turb2 * v, 0, 1)        # tongues break off near the top, never holes low down
        a = gaussian_filter(np.clip(a * 1.5, 0, 1), 1.0)
        heat = np.clip(body * 1.2 - yd * 0.5, 0, 1)            # hottest low and in the middle
        lum = 0.55 + 0.45 * heat
        rgb = np.dstack([lum, lum * (0.85 + 0.15 * heat), lum * (0.6 + 0.4 * heat)]) * a[..., None]   # premultiplied
        r, c = divmod(f, cols)
        sheet_rgb[r * F:(r + 1) * F, c * F:(c + 1) * F] = rgb
        sheet_a[r * F:(r + 1) * F, c * F:(c + 1) * F] = a * 0.85
    rgba("fx_flame_sheet", sheet_rgb, sheet_a)

if __name__ == "__main__":
    raindrop(); snowflake(); bolt(); smoke_puff(); flame_sheet()
