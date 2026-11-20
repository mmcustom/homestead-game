# Homestead night sky (Wave 6, 2026-11-19): a star field (equirectangular, imported as a cubemap for HDRP's Physically
# Based Sky "space emission") and a moon surface texture for the moon light's celestial body.
import numpy as np
from PIL import Image
from scipy.ndimage import gaussian_filter
OUT = "/home/claude/trees/sky/"
import os; os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(2026)

def stars(W=4096, H=2048):
    img = np.zeros((H, W, 3))
    # uniform on the sphere: z = sin(latitude) uniform
    n = 7000
    z = rng.uniform(-1, 1, n); lon = rng.uniform(0, 2 * np.pi, n)
    lat = np.arcsin(z)
    x = (lon / (2 * np.pi) * W).astype(int) % W
    y = ((0.5 - lat / np.pi) * H).astype(int).clip(0, H - 1)
    # magnitudes: many faint, few bright (power law)
    b = np.clip(rng.pareto(2.2, n) * 0.08 + 0.02, 0, 3.0)
    tint = rng.choice([0, 1, 2, 3], n, p=[0.6, 0.2, 0.12, 0.08])
    cols = np.array([[1, 1, 1], [0.8, 0.88, 1.0], [1.0, 0.9, 0.75], [1.0, 0.75, 0.6]])
    for i in range(n):
        img[y[i], x[i]] += cols[tint[i]] * b[i]
    img = gaussian_filter(img, (0.55, 0.55, 0))
    # stars near the poles are stretched in an equirect map: that's fine, the importer's cubemap undoes it
    # a faint Milky Way: a soft band along a tilted great circle, broken up by noise and dark lanes
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    lat2 = (0.5 - yy / H) * np.pi; lon2 = xx / W * 2 * np.pi
    p = np.stack([np.cos(lat2) * np.cos(lon2), np.cos(lat2) * np.sin(lon2), np.sin(lat2)], -1)
    normal = np.array([0.0, np.sin(1.05), np.cos(1.05)])
    d = np.abs(p @ normal)
    def fbm(s, seed):
        r = np.random.default_rng(seed); f = gaussian_filter(r.standard_normal((H // 4, W // 4)), s, mode="wrap")
        f = np.kron(f, np.ones((4, 4))); f = gaussian_filter(f, 3, mode='wrap'); return f / f.std()
    band = np.exp(-(d / 0.13) ** 2) * np.clip(0.6 + 0.3 * fbm(10, 3), 0, 1.2)   # no dark lanes: at this faintness they read as blobs
    img += band[..., None] * np.array([0.012, 0.012, 0.014])
    # dust of very faint stars in the band
    dust = (rng.random((H, W)) > 0.99) * band * 0.18
    img += gaussian_filter(dust, 0.5)[..., None]
    out = np.clip(img, 0, 1) ** (1 / 2.2)
    Image.fromarray((out * 255 + 0.5).astype(np.uint8), "RGB").save(OUT + "sky_stars.png")

def moon(S=512):
    yy, xx = np.mgrid[0:S, 0:S].astype(float)
    u = (xx - S / 2 + 0.5) / (S / 2); v = (yy - S / 2 + 0.5) / (S / 2)
    r = np.hypot(u, v)
    def fbm(s, seed):
        g = np.random.default_rng(seed); f = gaussian_filter(g.standard_normal((S, S)), s); return f / f.std()
    maria = np.clip(fbm(28, 11) * 0.8 + fbm(12, 12) * 0.3 - 0.35, 0, 1)
    shade = 0.78 - 0.32 * np.clip(maria * 1.5, 0, 1) + 0.04 * fbm(2, 13)
    for i in range(70):                           # craters: dark floor, bright rim
        cx, cy = rng.uniform(-0.85, 0.85, 2); cr = rng.uniform(0.015, 0.09) * (1 if i > 6 else 1.6)
        d = np.hypot(u - cx, v - cy) / cr
        shade += 0.08 * np.exp(-((d - 1) / 0.18) ** 2) - 0.06 * np.exp(-(d / 0.7) ** 4)
    for i in range(3):                            # a few bright ray craters
        cx, cy = rng.uniform(-0.6, 0.6, 2); d = np.hypot(u - cx, v - cy)
        ang = np.arctan2(v - cy, u - cx)
        rays = np.clip(np.cos(ang * rng.integers(9, 15)) , 0, 1) ** 6 * np.exp(-d / 0.35)
        shade += 0.12 * rays + 0.15 * np.exp(-(d / 0.03) ** 2)
    shade = np.clip(shade, 0, 1)
    a = np.clip((1 - r) * S / 3, 0, 1)
    rgb = np.dstack([shade * 0.98, shade * 0.97, shade * 0.95])
    Image.fromarray(np.dstack([(rgb * 255).astype(np.uint8), (a * 255).astype(np.uint8)]), "RGBA").save(OUT + "sky_moon_surface.png")

stars(); moon()
