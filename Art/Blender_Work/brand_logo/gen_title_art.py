# Wave 7 / Wave 10 title art: the emblem, the logo lockup, the app icon, and the Main Menu / Loading and Loss backgrounds
# graded from in-game 4K captures (the player's own homestead: the Small Cabin, Potter's Shed, kiln, tripod).
import math, os, sys
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont
from scipy.ndimage import gaussian_filter

SRC = sys.argv[1]            # folder with title_raw.png and loss_raw2.png
FONTS = sys.argv[2]          # folder with the Unity font files
OUT = sys.argv[3]
os.makedirs(OUT, exist_ok=True)

INK = (48, 33, 18); CREAM = (237, 227, 199); PAPER = (233, 218, 182); WOOD = (74, 48, 28); WOOD_L = (112, 78, 48)
EMBER = (196, 102, 44)

def font(name, size): return ImageFont.truetype(os.path.join(FONTS, name), size)

# ------------------------------------------------------------------ emblem (drawn at 4x, then reduced)
def emblem(size=1024, ss=4):
    S = size * ss
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c = S / 2
    def circ(r, **kw): d.ellipse((c - r, c - r, c + r, c + r), **kw)
    R = S * 0.48
    circ(R, fill=WOOD + (255,))
    circ(R * 0.955, fill=WOOD_L + (255,))
    circ(R * 0.93, fill=WOOD + (255,))
    circ(R * 0.86, fill=CREAM + (255,))
    circ(R * 0.835, fill=PAPER + (255,))
    # paper mottling inside
    arr = np.asarray(img).astype(float)
    yy, xx = np.mgrid[0:S, 0:S]
    inside = (xx - c) ** 2 + (yy - c) ** 2 < (R * 0.835) ** 2
    n = gaussian_filter(np.random.default_rng(3).standard_normal((S // 8, S // 8)), 6, mode="wrap")
    n = np.kron(n / n.std(), np.ones((8, 8)))[:S, :S]
    for k in range(3): arr[..., k] = np.where(inside, arr[..., k] * (1 + 0.012 * n), arr[..., k])
    img = Image.fromarray(np.clip(arr, 0, 255).astype(np.uint8)); d = ImageDraw.Draw(img)
    u = S / 100.0  # drawing unit
    # sun low behind the cabin with rays
    sx, sy = c, c + 6 * u
    for i in range(13):
        a = math.radians(180 + i * 15)
        r0, r1 = 17 * u, (31 if i % 2 == 0 else 26) * u
        w = 1.6 * u
        p = [(sx + math.cos(a - 0.04) * r0, sy + math.sin(a - 0.04) * r0), (sx + math.cos(a) * r1, sy + math.sin(a) * r1),
             (sx + math.cos(a + 0.04) * r0, sy + math.sin(a + 0.04) * r0)]
        d.polygon(p, fill=EMBER + (255,))
    d.pieslice((sx - 14 * u, sy - 14 * u, sx + 14 * u, sy + 14 * u), 180, 360, fill=EMBER + (255,))
    # two pines either side
    def pine(x, base, h, w):
        d.rectangle((x - 0.8 * u, base - 3 * u, x + 0.8 * u, base), fill=INK + (255,))
        for k in range(4):
            t = k / 4
            y0 = base - 2.5 * u - h * t * 0.85
            ww = w * (1 - t * 0.7)
            d.polygon([(x - ww, y0), (x + ww, y0), (x, y0 - h * 0.38)], fill=INK + (255,))
    pine(c - 27 * u, c + 20 * u, 30 * u, 8 * u)
    pine(c - 19 * u, c + 20 * u, 22 * u, 6 * u)
    pine(c + 27 * u, c + 20 * u, 33 * u, 8.5 * u)
    # cabin: logs, gable roof, door, window, chimney with smoke
    gx0, gx1, gy = c - 15 * u, c + 15 * u, c + 20 * u
    wall_top = gy - 15 * u
    d.rectangle((gx0, wall_top, gx1, gy), fill=INK + (255,))
    for k in range(1, 6):  # log lines in paper colour
        y = wall_top + k * (gy - wall_top) / 6
        d.line((gx0 + 0.8 * u, y, gx1 - 0.8 * u, y), fill=PAPER + (255,), width=int(0.55 * u))
    d.polygon([(gx0 - 4 * u, wall_top + 0.6 * u), (c, wall_top - 13 * u), (gx1 + 4 * u, wall_top + 0.6 * u)], fill=INK + (255,))
    d.polygon([(gx0 + 1.2 * u, wall_top - 0.4 * u), (c, wall_top - 10.6 * u), (gx1 - 1.2 * u, wall_top - 0.4 * u)], fill=EMBER + (255,))
    d.polygon([(gx0 + 3.2 * u, wall_top - 0.8 * u), (c, wall_top - 8.8 * u), (gx1 - 3.2 * u, wall_top - 0.8 * u)], fill=INK + (255,))
    d.rectangle((c - 3 * u, gy - 9 * u, c + 3 * u, gy), fill=PAPER + (255,))
    d.rectangle((c - 2 * u, gy - 8 * u, c + 2 * u, gy), fill=EMBER + (255,))
    d.rectangle((gx0 + 3 * u, wall_top + 4 * u, gx0 + 8 * u, wall_top + 8.5 * u), fill=EMBER + (255,))
    d.rectangle((gx1 - 8 * u, wall_top + 4 * u, gx1 - 3 * u, wall_top + 8.5 * u), fill=EMBER + (255,))
    chx = gx1 - 6 * u
    d.rectangle((chx - 2 * u, wall_top - 12 * u, chx + 2 * u, wall_top - 3 * u), fill=INK + (255,))
    # smoke: a soft wavy ribbon of filled puffs, lighter as it rises
    for k in range(14):
        t = k / 13
        x = chx + (math.sin(t * 5.0) * 2.2 + t * 4) * u
        y = wall_top - (13 + t * 13) * u
        r = (1.4 + t * 1.8) * u
        g = int(110 + t * 90)
        d.ellipse((x - r, y - r, x + r, y + r), fill=(g - 30, g - 40, g - 50, 255))
    # ground
    d.rounded_rectangle((c - 36 * u, gy - 0.2 * u, c + 36 * u, gy + 2.2 * u), int(1.2 * u), fill=INK + (255,))
    d.line((c - 26 * u, gy + 6 * u, c + 26 * u, gy + 6 * u), fill=INK + (255,), width=int(1.0 * u))
    d.line((c - 16 * u, gy + 10 * u, c + 16 * u, gy + 10 * u), fill=INK + (255,), width=int(1.0 * u))
    # clip drawing to the inner disc
    mask = Image.new("L", (S, S), 0); ImageDraw.Draw(mask).ellipse((c - R * 0.835, c - R * 0.835, c + R * 0.835, c + R * 0.835), fill=255)
    ring = Image.new("L", (S, S), 255); ImageDraw.Draw(ring).ellipse((c - R * 0.835, c - R * 0.835, c + R * 0.835, c + R * 0.835), fill=0)
    base = Image.new("RGBA", (S, S), (0, 0, 0, 0)); bd = ImageDraw.Draw(base)
    bd.ellipse((c - R, c - R, c + R, c + R), fill=WOOD + (255,)); bd.ellipse((c - R * 0.955, c - R * 0.955, c + R * 0.955, c + R * 0.955), fill=WOOD_L + (255,))
    bd.ellipse((c - R * 0.93, c - R * 0.93, c + R * 0.93, c + R * 0.93), fill=WOOD + (255,)); bd.ellipse((c - R * 0.86, c - R * 0.86, c + R * 0.86, c + R * 0.86), fill=CREAM + (255,))
    base.paste(img, (0, 0), mask)
    # nail heads on the ring
    bd = ImageDraw.Draw(base)
    for k in range(8):
        a = math.radians(22.5 + k * 45); r = R * 0.893
        x, y = c + math.cos(a) * r, c + math.sin(a) * r
        bd.ellipse((x - 1.2 * u, y - 1.2 * u, x + 1.2 * u, y + 1.2 * u), fill=CREAM + (255,))
    return base.resize((size, size), Image.LANCZOS)

em = emblem(1024)
em.save(f"{OUT}/logo_emblem.png")

# ------------------------------------------------------------------ app icon (.ico, 16..256) and a 1024 png for PlayerSettings
em.save(f"{OUT}/homestead_icon.ico", sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])
em.save(f"{OUT}/app_icon.png")

# ------------------------------------------------------------------ logo lockup: emblem + wordmark + motto (cream, with a soft shadow)
def lockup():
    W, H = 2000, 760
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    e = em.resize((520, 520), Image.LANCZOS)
    img.alpha_composite(e, ((W - 520) // 2, 0))
    d = ImageDraw.Draw(img)
    f = font("Alegreya-Bold.ttf", 190)
    word = "HOMESTEAD"
    # letter-spaced wordmark
    spacing = 18
    widths = [d.textlength(ch, font=f) for ch in word]
    total = sum(widths) + spacing * (len(word) - 1)
    x = (W - total) / 2; y = 540
    for ch, w in zip(word, widths):
        d.text((x, y), ch, font=f, fill=CREAM + (255,), anchor="lm"); x += w + spacing
    m = font("AlegreyaSans-Bold.ttf", 52)
    motto = "Survive  ·  Prepare  ·  Build  ·  Prosper"
    d.text((W / 2, 690), motto, font=m, fill=(214, 186, 128, 255), anchor="mm")
    # rules either side of the motto
    tw = d.textlength(motto, font=m)
    for sgn in (-1, 1):
        x0 = W / 2 + sgn * (tw / 2 + 30); x1 = W / 2 + sgn * (tw / 2 + 170)
        d.line((x0, 690, x1, 690), fill=(214, 186, 128, 255), width=4)
    a = np.asarray(img)[..., 3].astype(float)
    sh = gaussian_filter(a, 10) * 0.85
    shadow = np.zeros((H, W, 4), np.uint8); shadow[..., 3] = np.clip(sh, 0, 255).astype(np.uint8)
    sh_img = Image.fromarray(shadow, "RGBA")
    out = Image.new("RGBA", (W, H + 20), (0, 0, 0, 0))
    out.alpha_composite(sh_img, (6, 14)); out.alpha_composite(img, (0, 0))
    return out
lockup().save(f"{OUT}/logo_lockup.png")

# ------------------------------------------------------------------ backgrounds
def grade(img, lift=(0, 0, 0), gain=(1, 1, 1), sat=1.0, contrast=1.0):
    a = np.asarray(img.convert("RGB")).astype(float) / 255.0
    l = a.mean(axis=2, keepdims=True)
    a = l + (a - l) * sat
    a = (a - 0.5) * contrast + 0.5
    a = a * np.array(gain) + np.array(lift)
    return np.clip(a, 0, 1)

def crop_scale(path, scale, x0f, y0f):
    im = Image.open(path).convert("RGB")
    W, H = im.size
    big = im.resize((int(W * scale), int(H * scale)), Image.LANCZOS)
    x0 = int((big.width - W) * x0f); y0 = int((big.height - H) * y0f)
    return big.crop((x0, y0, x0 + W, y0 + H)).resize((1920, 1080), Image.LANCZOS)

def vignette(a, strength=0.45, cx=0.5, cy=0.5):
    h, w = a.shape[:2]; yy, xx = np.mgrid[0:h, 0:w]
    r = np.sqrt(((xx / w - cx) / 0.75) ** 2 + ((yy / h - cy) / 0.62) ** 2)
    return a * (1 - strength * np.clip(r - 0.35, 0, 1) ** 1.5)[..., None]

def grain(a, amount=0.018, seed=1):
    n = np.random.default_rng(seed).standard_normal(a.shape[:2])[..., None]
    return np.clip(a + n * amount, 0, 1)

def side_shade(a, side="left", width=0.48, strength=0.72):
    h, w = a.shape[:2]; xx = np.linspace(0, 1, w)[None, :]
    t = (1 - xx) if side == "left" else xx
    m = np.clip((t - (1 - width)) / width, 0, 1) ** 1.4
    return a * (1 - strength * m)[..., None]

def save(a, name):
    Image.fromarray((np.clip(a, 0, 1) * 255).round().astype(np.uint8)).save(f"{OUT}/{name}.png")

# Main Menu / Loading: the cabin at autumn dusk, pushed to the right third, a warm late-light grade, a dark side for the menu.
t = crop_scale(f"{SRC}/title_raw.png", 1.45, 0.0, 0.42)
a = grade(t, lift=(0.012, 0.006, 0.0), gain=(1.08, 1.0, 0.86), sat=1.08, contrast=1.06)
# warm the sky near the horizon, deepen the top
h = a.shape[0]; yy = np.linspace(0, 1, h)[:, None, None]
a = a * (1 - 0.25 * np.clip(0.35 - yy, 0, 1) / 0.35) + np.array([0.10, 0.05, 0.0]) * np.exp(-((yy - 0.48) / 0.12) ** 2)
a = vignette(a, 0.5, cx=0.62)
title_clean = grain(a, 0.012, 2)
save(title_clean, "title_background")          # loading screen uses it as is
save(side_shade(title_clean, "left", 0.62, 0.86), "menu_background")

# Loss: the same homestead in a winter dusk under heavy cloud, cold and dim, snow falling, cabin on the left (panel on the right).
L = crop_scale(f"{SRC}/loss_raw2.png", 1.55, 1.0, 0.42)
a = grade(L, lift=(0.01, 0.014, 0.024), gain=(0.95, 1.0, 1.1), sat=0.55, contrast=1.08)
a = vignette(a, 0.55, cx=0.18)
rng = np.random.default_rng(7)
snow = np.zeros(a.shape[:2])
for layer, (count, size, blur, alpha) in enumerate([(650, 1, 0.8, 0.45), (260, 2, 1.3, 0.5), (60, 3, 2.6, 0.42)]):
    s = np.zeros(a.shape[:2])
    ys = rng.integers(0, a.shape[0], count); xs = rng.integers(0, a.shape[1], count)
    for y, x in zip(ys, xs):
        s[max(0, y - size):y + size + 1, max(0, x - size):x + size + 1] = 1.0
    snow = np.maximum(snow, gaussian_filter(s, blur) * alpha / max(gaussian_filter(s, blur).max(), 1e-6))
a = a * (1 - snow[..., None]) + np.array([0.86, 0.88, 0.92]) * snow[..., None]
a = side_shade(a, "right", 0.58, 0.55)
save(grain(a, 0.014, 3), "loss_background")
print("done")
