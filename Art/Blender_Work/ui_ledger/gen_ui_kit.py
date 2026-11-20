# Wave 7 "Ledger" UI kit: 9-slice sprites for UiKit. Panel = parchment in a dark wood frame (final colours);
# card and button = near-white paper with a darker rim, tinted in Unity by the Image / Button colours.
import numpy as np
from PIL import Image, ImageDraw
from scipy.ndimage import gaussian_filter
import json, sys

OUT = sys.argv[1] if len(sys.argv) > 1 else "/home/claude/ui/kit"
import os; os.makedirs(OUT, exist_ok=True)

def noise(h, w, s, seed):
    n = gaussian_filter(np.random.default_rng(seed).standard_normal((h, w)), s, mode="wrap")
    return n / (n.std() + 1e-9)

def rr_mask(w, h, r, inset=0, ss=4):
    m = Image.new("L", (w * ss, h * ss), 0)
    ImageDraw.Draw(m).rounded_rectangle((inset * ss, inset * ss, (w - inset) * ss - 1, (h - inset) * ss - 1), r * ss, fill=255)
    return np.asarray(m.resize((w, h), Image.LANCZOS), float) / 255.0

def save(rgb, alpha, name):
    a = np.clip(alpha, 0, 1)
    img = np.dstack([np.clip(rgb, 0, 1), a]) * 255
    Image.fromarray(img.round().astype(np.uint8), "RGBA").save(f"{OUT}/{name}.png")

# ---------------- panel: wood frame + parchment
S, FRAME, R = 1024, 26, 16
yy, xx = np.mgrid[0:S, 0:S].astype(float)
# wood: grain runs along each side (horizontal on top/bottom, vertical on the sides)
def streaks(seed, along_x):
    r = np.random.default_rng(seed).standard_normal((S, S))
    sig = (0.8, 45) if along_x else (45, 0.8)
    n = gaussian_filter(r, sig, mode="wrap"); n /= n.std()
    w = gaussian_filter(np.random.default_rng(seed + 1).standard_normal((S, S)), (6, 120) if along_x else (120, 6), mode="wrap"); w /= w.std()
    return np.clip(0.5 + 0.28 * n + 0.15 * w, 0, 1)
gh, gv = streaks(5, True), streaks(7, False)
dist_side = np.minimum(xx, S - 1 - xx); dist_tb = np.minimum(yy, S - 1 - yy)
g = np.where(dist_tb <= dist_side, gh, gv)
wood_dark, wood_light = np.array([0.24, 0.155, 0.09]), np.array([0.40, 0.27, 0.16])
wood = wood_dark + (wood_light - wood_dark) * (g[..., None] * 0.75 + 0.1)
# bevel: lighter outer lip, darker inner lip
d = np.minimum(dist_side, dist_tb)
wood *= (1.0 + 0.18 * np.clip(1 - d / 5, 0, 1) - 0.25 * np.clip(1 - (FRAME - d) / 5, 0, 1))[..., None]
# parchment: low-frequency mottling (survives stretching) plus faint fibres
n = noise(S, S, 60, 1) * 0.55 + noise(S, S, 12, 2) * 0.3 + noise(S, S, 1.2, 3) * 0.12
base, dark = np.array([0.905, 0.845, 0.715]), np.array([0.80, 0.70, 0.53])
t = np.clip(0.62 + n * 0.2, 0, 1)[..., None]
paper = base * t + dark * (1 - t)
inner = np.clip((d - FRAME) / 46.0, 0, 1)            # darkened edge of the page under the frame
paper *= (0.80 + 0.20 * inner ** 0.7)[..., None]
inside = rr_mask(S, S, 6, inset=FRAME)
rgb = wood * (1 - inside[..., None]) + paper * inside[..., None]
save(rgb, rr_mask(S, S, R), "ui_panel")

# ---------------- card (list rows, boxes): tintable paper
def tintable(w, h, r, rim, rim_value, seed, shade=0.0):
    yy, xx = np.mgrid[0:h, 0:w].astype(float)
    outer = rr_mask(w, h, r)
    fill = rr_mask(w, h, max(1, r - rim), inset=rim)
    paper = np.full((h, w), 0.975)
    paper = paper - shade * (yy / (h - 1))               # buttons: a little darker toward the bottom
    paper[: rim + 2] += shade * 0.5                     # and a highlight under the top rim
    v = rim_value * (1 - fill) + paper * fill
    return np.dstack([v, v, v]), outer

rgb, a = tintable(96, 96, 8, 2, 0.55, 11)
save(rgb, a, "ui_card")
rgb, a = tintable(96, 64, 10, 2, 0.45, 13, shade=0.08)
save(rgb, a, "ui_button")
# pill for bar tracks / small badges
rgb, a = tintable(64, 32, 8, 2, 0.5, 17)
save(rgb, a, "ui_track")

json.dump({"ui_panel": 72, "ui_card": 14, "ui_button": 16, "ui_track": 10}, open(f"{OUT}/borders.json", "w"))
print("ok")

# ---------------- wood strip (compass, HUD plates): horizontal planks, final colours, translucent-free
W2, H2, R2 = 512, 64, 10
yy, xx = np.mgrid[0:H2, 0:W2].astype(float)
r = np.random.default_rng(21).standard_normal((H2, W2))
n = gaussian_filter(r, (0.7, 40), mode="wrap"); n /= n.std()
w = gaussian_filter(np.random.default_rng(22).standard_normal((H2, W2)), (4, 90), mode="wrap"); w /= w.std()
g = np.clip(0.5 + 0.28 * n + 0.15 * w, 0, 1)
dark, light = np.array([0.20, 0.13, 0.075]), np.array([0.36, 0.24, 0.14])
wood = dark + (light - dark) * (g[..., None] * 0.75 + 0.1)
d = np.minimum.reduce([xx, yy, W2 - 1 - xx, H2 - 1 - yy])
wood *= (1.0 + 0.22 * np.clip(1 - d / 3, 0, 1) * (yy < H2 / 2)[..., None].squeeze(-1) - 0.30 * np.clip(1 - d / 3, 0, 1) * (yy >= H2 / 2))[..., None]
save(wood, rr_mask(W2, H2, R2), "ui_wood")
b = json.load(open(f"{OUT}/borders.json")); b["ui_wood"] = 16; json.dump(b, open(f"{OUT}/borders.json", "w"))
print("wood ok")
