# Wave 8 effect textures (white with alpha; tinted in code): ripple ring, spark dot, droplet, boot print.
import numpy as np, os, sys
from PIL import Image
from scipy.ndimage import gaussian_filter
OUT = sys.argv[1] if len(sys.argv) > 1 else "."
def save(a, name):
    a = np.clip(a, 0, 1)
    rgba = np.dstack([np.ones_like(a), np.ones_like(a), np.ones_like(a), a])
    Image.fromarray((rgba * 255).round().astype(np.uint8), "RGBA").save(os.path.join(OUT, name + ".png"))

# ripple: a thin bright ring with a faint second ring inside, soft edges
S = 256; yy, xx = np.mgrid[0:S, 0:S]; r = np.hypot(xx - S / 2 + .5, yy - S / 2 + .5) / (S / 2)
ring = np.exp(-((r - 0.86) / 0.035) ** 2) + 0.45 * np.exp(-((r - 0.70) / 0.03) ** 2)
n = gaussian_filter(np.random.default_rng(1).standard_normal((S, S)), 6); n /= n.std()
save(ring * (0.85 + 0.15 * n), "fx_ripple")

# spark: a hot core with a soft halo
S = 64; yy, xx = np.mgrid[0:S, 0:S]; r = np.hypot(xx - S / 2 + .5, yy - S / 2 + .5) / (S / 2)
save(np.clip(1.2 * np.exp(-(r / 0.2) ** 2) + 0.3 * np.exp(-(r / 0.4) ** 2), 0, 1) * np.clip((0.95 - r) / 0.3, 0, 1), "fx_spark")

# droplet: a soft elongated drop (for stretched billboards)
S = 64; yy, xx = np.mgrid[0:S, 0:S]
d = np.hypot((xx - S / 2 + .5) / (S * 0.22), (yy - S / 2 + .5) / (S * 0.42))
save(np.clip(1 - d, 0, 1) ** 1.5, "fx_droplet")

# boot print (right foot, toe up): sole and heel with a lug pattern, soft edge; drawn at 4x then reduced
W, H, ss = 128, 320, 4
w, h = W * ss, H * ss
yy, xx = np.mgrid[0:h, 0:w].astype(float)
u = (xx / w - 0.5); v = yy / h          # v: 0 toe .. 1 heel
def ell(cx, cy, rx, ry): return ((u - cx) / rx) ** 2 + ((v - cy) / ry) ** 2 <= 1
sole = ell(0.03, 0.30, 0.36, 0.28) | (ell(0.0, 0.50, 0.30, 0.18))
heel = ell(-0.02, 0.84, 0.30, 0.14)
mask = (sole | heel).astype(float)
lugs = ((np.sin(v * 70) > -0.2) | (np.abs(u) < 0.03)).astype(float)
a = mask * (0.75 + 0.25 * lugs)
a = gaussian_filter(a, 2.5 * ss)
img = Image.fromarray((np.clip(a, 0, 1) * 255).astype(np.uint8)).resize((W, H), Image.LANCZOS)
save(np.asarray(img).astype(float) / 255, "fx_footprint")
print("ok")
