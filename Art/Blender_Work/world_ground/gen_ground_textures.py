# Homestead runtime ground layers (Wave 6, 2026-11-19): snow, leaf litter, wet clay, creek sand.
# Tileable 1024 maps per layer: basecolor (sRGB), normal (OpenGL), mask (R metallic 0, G AO, B height, A smoothness).
# Height (mask B) drives HDRP's height blend on the terrain: snow is the highest, so a thin cover first shows in the hollows
# between the other layers' bumps... no: HDRP gives the higher layer the win, so thin snow first sits on the tops and fills down.
# Leaf litter reuses the tree leaf shapes (gen_tree_textures.py), so the leaves on the ground are the ones that fell.
import numpy as np, sys
from PIL import Image
from scipy.spatial import cKDTree
from scipy.ndimage import gaussian_filter

sys.argv = [sys.argv[0]] + sys.argv[1:]
exec(open("/home/claude/trees/gen_tree_textures.py").read().split("if __name__")[0])   # fbm, leaf_sprite, rotate_sprite, use_shape, srgb, to8

OUT = "/home/claude/trees/ground/"
N = 1024

def normal_from(h, s):
    gx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * s
    gy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * s
    n = np.dstack([-gx, gy, np.ones_like(gx)])
    return n / np.linalg.norm(n, axis=2, keepdims=True)

def save(name, col, nrm, ao, height, smooth):
    Image.fromarray(to8(srgb(col)), "RGB").save(OUT + f"ground_{name}_basecolor.png")
    Image.fromarray(to8(nrm * 0.5 + 0.5), "RGB").save(OUT + f"ground_{name}_normal.png")
    Image.fromarray(to8(np.dstack([np.zeros_like(ao), ao, height, smooth])), "RGBA").save(OUT + f"ground_{name}_mask.png")

def wrapped_voronoi(count, seed, sy=1.0):
    r = np.random.default_rng(seed)
    pts = r.uniform(0, 1, (count, 2)) * [N, N / sy]
    tree = cKDTree(pts, boxsize=[N, N / sy])
    yy, xx = np.mgrid[0:N, 0:N].astype(np.float64)
    d, idx = tree.query(np.dstack([xx, yy / sy]).reshape(-1, 2), k=2)
    return d[:, 0].reshape(N, N), d[:, 1].reshape(N, N), idx[:, 0].reshape(N, N)

def norm01(a): return (a - a.min()) / (a.max() - a.min() + 1e-9)

# ---------------------------------------------------------------- snow
def snow():
    drift = norm01(fbm(N, 2.6, 1, 10, seed=41))                 # soft wind-shaped swells
    lumps = norm01(fbm(N, 2.0, 8, 60, seed=42))
    fine = fbm(N, 1.2, 80, 500, seed=43)
    h = drift * 0.7 + lumps * 0.25 + fine * 0.01
    shade = 0.9 + 0.06 * (drift - 0.5) + 0.03 * (lumps - 0.5)
    col = np.dstack([shade * 0.93, shade * 0.95, shade * 1.0])  # a touch blue in the hollows
    col = np.clip(col, 0, 1)
    sparkle = (np.random.default_rng(44).random((N, N)) > 0.9965).astype(float)
    smooth = np.clip(0.35 + 0.5 * sparkle + fine * 0.03, 0, 1)
    ao = np.clip(0.85 + 0.15 * lumps, 0, 1)
    height = np.clip(0.55 + 0.4 * h, 0, 1)                    # the highest layer
    save("snow", col, normal_from(h, 1.5), ao, height, smooth)

# ---------------------------------------------------------------- leaf litter
def litter():
    r = np.random.default_rng(51)
    col = np.zeros((N, N, 3)); hgt = np.zeros((N, N)); nrm = np.zeros((N, N, 3)); nrm[..., 2] = 1; cov = np.zeros((N, N))
    # damp soil showing through
    soil = 0.6 + 0.4 * norm01(fbm(N, 2.0, 4, 80, seed=52))
    col[:] = np.dstack([soil * 0.075, soil * 0.05, soil * 0.03])
    # fallen leaves: brown, rust, dull orange, a few yellow; the newest on top are brighter
    palette = [((0.2, 0.12, 0.06), 0.4), ((0.28, 0.12, 0.05), 0.2), ((0.36, 0.19, 0.05), 0.14), ((0.4, 0.3, 0.08), 0.06), ((0.15, 0.1, 0.06), 0.2)]
    cols = np.array([p[0] for p in palette]); w = np.array([p[1] for p in palette]); w /= w.sum()
    shapes = ["maple"] * 3 + ["poplar"] * 2 + ["viburnum"]
    count = 900
    for i in range(count):
        layer = i / count
        use_shape(shapes[r.integers(len(shapes))])
        size = r.uniform(70, 120)
        cv, c, n = leaf_sprite(size, r.normal(0, 0.5, 2), 1.0, 0.0, r.random() < 0.3)
        cv, c, n = rotate_sprite(cv, c, n, r.uniform(0, 2 * np.pi))
        hue = cols[r.choice(len(cols), p=w)] * r.uniform(0.7, 1.1) * (0.7 + 0.4 * layer)
        lum = c.mean(axis=2, keepdims=True)
        leaf = hue * (0.6 + 0.4 * lum)
        hh, ww = cv.shape
        x0 = int(r.uniform(0, N)); y0 = int(r.uniform(0, N))
        ys = (np.arange(hh) + y0) % N; xs = (np.arange(ww) + x0) % N   # wrap round the tile
        A = cv[..., None]
        sub = np.ix_(ys, xs)
        col[sub] = col[sub] * (1 - A) + leaf * A
        nrm[sub] = nrm[sub] * (1 - A) + n * A
        hgt[sub] = np.maximum(hgt[sub] * (1 - cv), (0.3 + 0.7 * layer) * cv)
        cov[sub] = np.maximum(cov[sub], cv)
    nrm /= np.linalg.norm(nrm, axis=2, keepdims=True)
    occl = gaussian_filter(hgt, 4, mode="wrap")
    ao = np.clip(1 - 0.5 * np.clip(occl - hgt, 0, 1) * 3 - 0.3 * (1 - cov), 0, 1)
    col *= ao[..., None] ** 0.6
    smooth = np.clip(0.12 + 0.18 * cov * (1 - hgt), 0, 1)        # the lower, damper leaves shine a little
    height = np.clip(0.25 + 0.45 * hgt, 0, 1)
    save("leaf_litter", col, nrm, ao, height, smooth)

# ---------------------------------------------------------------- wet clay
def clay():
    base = norm01(fbm(N, 2.0, 3, 40, seed=61))
    mid = fbm(N, 1.8, 15, 120, seed=62)
    fine = fbm(N, 1.2, 120, 500, seed=63)
    # slick, smeared surface: shallow ridges along one direction, puddles in the low spots
    smear = fbm(N, 2.0, 3, 60, stretch=0.35, seed=64)
    h = base * 0.5 + smear * 0.08 + mid * 0.05 + fine * 0.01
    puddle = np.clip((0.25 - base) / 0.2, 0, 1) * 0.6
    # small stones pressed in
    d0, d1, idx = wrapped_voronoi(500, 65)
    stone_r = np.random.default_rng(66).uniform(2.5, 7, 500)[idx]
    keep = np.random.default_rng(67).random(500)[idx] < 0.35
    stone = np.clip(1 - d0 / stone_r, 0, 1) ** 0.5 * keep
    h = h + stone * 0.25
    red = np.array([0.34, 0.16, 0.085]); grey = np.array([0.2, 0.16, 0.13]); dark = np.array([0.17, 0.09, 0.055])
    t = np.clip(0.5 + mid * 0.35 + (base - 0.5) * 1.1 + smear * 0.15 + fine * 0.08, 0, 1)[..., None]
    col = grey + (red - grey) * t
    col = col * (1 - puddle[..., None] * 0.45) + dark * puddle[..., None] * 0.45
    pebble = np.array([0.36, 0.33, 0.29]) * (0.8 + 0.4 * np.random.default_rng(68).random(500)[idx])[..., None]
    col = col * (1 - stone[..., None] * 0.9) + pebble * stone[..., None] * 0.9
    smooth = np.clip(0.45 + 0.4 * puddle + fine * 0.04 - stone * 0.25, 0, 1)
    ao = np.clip(0.8 + 0.2 * base - 0.2 * np.clip(stone - 0.05, 0, 0.2) * 5 * 0 , 0, 1)
    height = np.clip(0.2 + 0.4 * h, 0, 1)
    save("wet_clay", col, normal_from(h, 2.2), ao, height, smooth)

# ---------------------------------------------------------------- creek sand
def sand():
    ripple_warp = fbm(N, 2.6, 1, 4, seed=71) * 18
    yy, xx = np.mgrid[0:N, 0:N].astype(np.float64)
    # water-laid ripples, wrapped: 16 crests across the tile, bent by the warp
    ripple = 0.5 + 0.5 * np.sin(2 * np.pi * (yy + ripple_warp) / N * 20 + fbm(N, 2.4, 1, 6, seed=72) * 0.5)
    ripple = ripple ** 1.6
    grain = np.random.default_rng(73).random((N, N))
    grain = gaussian_filter(grain, 0.7, mode="wrap")
    patches = norm01(fbm(N, 2.2, 2, 24, seed=74))
    d0, d1, idx = wrapped_voronoi(260, 75)
    rr = np.random.default_rng(76)
    stone_r = rr.uniform(3, 10, 260)[idx]
    keep = rr.random(260)[idx] < 0.3
    stone = np.clip(1 - d0 / stone_r, 0, 1) ** 0.5 * keep
    h = ripple * 0.35 * (0.5 + 0.5 * patches) + (grain - 0.5) * 0.12 + stone * 0.4
    tan = np.array([0.5, 0.42, 0.28]); pale = np.array([0.62, 0.55, 0.4]); damp = np.array([0.36, 0.29, 0.18])
    col = tan + (pale - tan) * (ripple * 0.25 + (grain - 0.5) * 1.6 + (patches - 0.5) * 0.6)[..., None] * 0.6
    wet = np.clip((0.3 - patches) / 0.3, 0, 1)
    col = col * (1 - wet[..., None] * 0.25) + damp * wet[..., None] * 0.25
    speck = (np.random.default_rng(77).random((N, N)) > 0.985)[..., None]
    col = np.where(speck, col * 0.45, col)                     # dark mineral grains
    pebble = np.array([0.3, 0.28, 0.25]) * (0.7 + 0.6 * rr.random(260)[idx])[..., None]
    col = col * (1 - stone[..., None]) + pebble * stone[..., None]
    smooth = np.clip(0.08 + 0.3 * wet + stone * 0.15, 0, 1)
    ao = np.clip(0.85 + 0.15 * ripple - 0.1 * (1 - stone) * 0, 0, 1)
    height = np.clip(0.2 + 0.4 * h, 0, 1)
    save("creek_sand", col, normal_from(h, 1.6), ao, height, smooth)

if __name__ == "__main__":
    import os; os.makedirs(OUT, exist_ok=True)
    what = sys.argv[1] if len(sys.argv) > 1 else "all"
    for name, fn in (("snow", snow), ("litter", litter), ("clay", clay), ("sand", sand)):
        if what in ("all", name): fn()
