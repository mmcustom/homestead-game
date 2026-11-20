# Homestead world_trees textures (Wave 6, 2026-11-19).
# 1) Leaf-cluster atlas, 2x2 clusters on a 2048 card: basecolor (sRGB, alpha = leaf coverage) + normal (OpenGL).
#    The colour is a light, near-neutral green so SeasonalFoliage's tint (_BaseColor) sets the season's colour.
# 2) Tileable bark (1024): basecolor, normal, mask (R metallic 0, G AO, B 0, A smoothness).
import numpy as np, sys
from PIL import Image, ImageDraw
from scipy.spatial import cKDTree
from scipy.ndimage import gaussian_filter

OUT = "/home/claude/trees/out/"
rng = np.random.default_rng(20261120)

def to8(a): return (np.clip(a, 0, 1) * 255 + 0.5).astype(np.uint8)
def srgb(c): return np.where(c <= 0.0031308, c * 12.92, 1.055 * np.power(np.clip(c, 0, 1), 1 / 2.4) - 0.055)

# ---------------------------------------------------------------- maple leaves
LEAF_SHAPES = {
    # base radius, lobes (angle, length, width), notches (angle, depth, width), teeth amplitude
    "maple":  dict(base=0.26, lobes=((0, 0.58, 0.30), (1.0, 0.54, 0.28), (-1.0, 0.54, 0.28), (2.05, 0.36, 0.26), (-2.05, 0.36, 0.26)),
                   notches=((3.14159, 0.08, 0.25),), teeth=0.09),
    # tulip poplar: four lobes and a broad, squared-off tip with a shallow notch
    "poplar": dict(base=0.36, lobes=((0.85, 0.5, 0.26), (-0.85, 0.5, 0.26), (1.95, 0.47, 0.26), (-1.95, 0.47, 0.26)),
                   notches=((0, 0.05, 0.35), (1.4, 0.06, 0.16), (-1.4, 0.06, 0.16), (3.14159, 0.06, 0.2)), teeth=0.0),
    # viburnum (Understory Shrub): an oval leaf with a short point and fine teeth all round
    "viburnum": dict(base=0.33, lobes=((0, 0.57, 0.5), (3.14159, 0.36, 0.8)), notches=(), teeth=0.0, serrate=0.035, veins=6),
}

# leaves per cluster and their size (fraction of the cluster cell)
CLUSTER = {"maple": (24, 0.2, 0.3), "poplar": (24, 0.2, 0.3), "viburnum": (36, 0.15, 0.21)}

def leaf_outline(shape, n=400):
    """A leaf in leaf space: petiole at (0,0), tip at about (0,1). Returns polygon points and the leaf centre."""
    c = np.array([0.0, 0.42])
    th = np.linspace(-np.pi, np.pi, n, endpoint=False)          # 0 = up the midrib
    r = np.full_like(th, shape["base"])
    for a, L, w in shape["lobes"]:
        d = np.angle(np.exp(1j * (th - a)))
        r += (L - shape["base"]) * np.exp(-(d / w) ** 2 * 1.6)
        for sgn in (-1, 1):
            d2 = np.angle(np.exp(1j * (th - a - sgn * w * 0.75)))
            r += shape["teeth"] * L * np.exp(-(d2 / 0.06) ** 2)
    if shape.get("serrate"):
        r *= 1 + shape["serrate"] * np.abs(np.sin(th * 26))
    for a, depth, w in shape["notches"]:
        d = np.angle(np.exp(1j * (th - a)))
        r -= depth * np.exp(-(d / w) ** 2)
    x = c[0] + r * np.sin(th); y = c[1] + r * np.cos(th)
    return np.stack([x, y], 1), c

LEAF, LEAF_C, LOBE_TIPS = None, None, None

def use_shape(name):
    global LEAF, LEAF_C, LOBE_TIPS
    LEAF, LEAF_C = leaf_outline(LEAF_SHAPES[name])
    if LEAF_SHAPES[name].get("veins"):   # pinnate: side veins angled up from the midrib
        LOBE_TIPS = [(0.0, 1.0)] + [(sgn * 0.8, 0.6) for sgn in (-1, 1)]
    else:
        LOBE_TIPS = [(np.sin(a), np.cos(a)) for a, L, w in LEAF_SHAPES[name]["lobes"]] + [(0.0, 1.0)]

def leaf_sprite(px, tilt, bright, hue, under):
    """Rasterise one leaf (length px pixels) at 4x and return rgba coverage, colour and normal arrays."""
    S = 4; size = int(px * 1.25) + 4; big = size * S
    pts = (LEAF - [0, 0.5]) * px * S + big / 2       # centre the leaf in the sprite
    pts[:, 1] = big - pts[:, 1]                       # image rows go down
    im = Image.new("L", (big, big), 0); ImageDraw.Draw(im).polygon([tuple(p) for p in pts], fill=255)
    cov = np.asarray(im.resize((size, size), Image.LANCZOS), np.float64) / 255
    yy, xx = np.mgrid[0:size, 0:size]
    lx = (xx - size / 2) / px; ly = -(yy - size / 2) / px + 0.5   # back in leaf space
    # veins: distance to the lines from the leaf centre to each lobe tip
    vein = np.zeros_like(lx)
    for tx, ty in LOBE_TIPS:
        vx, vy = lx - LEAF_C[0], ly - LEAF_C[1]
        t = np.clip(vx * tx + vy * ty, 0, 0.6)
        dist = np.hypot(vx - t * tx, vy - t * ty)
        vein = np.maximum(vein, np.exp(-(dist / 0.012) ** 2) * (1 - t / 0.7))
    petiole = np.exp(-(np.abs(lx) / 0.012) ** 2) * (ly < LEAF_C[1]) * (ly > -0.25)
    cov = np.maximum(cov, np.clip(petiole * 1.4, 0, 1) * (ly > -0.25))
    rr = np.hypot(lx - LEAF_C[0], ly - LEAF_C[1])
    shade = bright * (0.9 + 0.1 * np.clip(1 - rr * 1.6, 0, 1)) * (1 - 0.18 * np.clip(rr - 0.35, 0, 1))
    base = np.array([1.0, 1.0, 0.86]) * np.array([1 + hue, 1, 1 - hue * 0.5])
    if under: base = base * 0.85 + 0.15
    col = shade[..., None] * base * (1 + 0.22 * vein[..., None])
    col = np.where(petiole[..., None] > 0.4, np.array([0.55, 0.42, 0.25]) * bright, col)
    # normal: the leaf's tilt plus a shallow cup toward its edges
    cup = 0.5
    nx = tilt[0] + cup * (lx - LEAF_C[0]); ny = tilt[1] + cup * (ly - LEAF_C[1])
    n = np.dstack([nx, ny, np.ones_like(nx)]); n /= np.linalg.norm(n, axis=2, keepdims=True)
    return cov, col, n

def rotate_sprite(cov, col, n, ang):
    """Rotate sprite arrays by ang (radians, counter-clockwise), rotating normals' xy with them."""
    def rot(a, mode):
        return np.asarray(Image.fromarray(a.astype(np.float32), "F").rotate(np.degrees(ang), resample=mode, expand=True), np.float64)
    c2 = rot(cov, Image.BILINEAR)
    col2 = np.dstack([rot(col[..., k], Image.BILINEAR) for k in range(3)])
    ca, sa = np.cos(ang), np.sin(ang)
    nx = n[..., 0] * ca - n[..., 1] * sa; ny = n[..., 0] * sa + n[..., 1] * ca
    n2 = np.dstack([rot(nx, Image.BILINEAR), rot(ny, Image.BILINEAR), rot(n[..., 2], Image.BILINEAR)])
    n2[c2 < 1e-3] = (0, 0, 1)
    return c2, col2, n2

def cluster(N, seed, count=24, size_lo=0.2, size_hi=0.3):
    r = np.random.default_rng(seed)
    alpha = np.zeros((N, N)); col = np.zeros((N, N, 3)); nrm = np.zeros((N, N, 3)); nrm[..., 2] = 1
    stem = np.array([N * 0.5, N * 0.93])          # where the twig enters (pixel x, y)
    # twig: a few thin brown lines from the stem up into the cluster
    tw = Image.new("L", (N * 2, N * 2), 0); d = ImageDraw.Draw(tw)
    tips = []
    for k in range(4):
        a = -np.pi / 2 + r.uniform(-0.8, 0.8)
        L = r.uniform(0.25, 0.4) * N
        e = stem + L * np.array([np.cos(a), np.sin(a)])
        d.line([tuple(stem * 2), tuple(e * 2)], fill=255, width=int(N * 0.012))
        tips.append(e)
    tcov = np.asarray(tw.resize((N, N), Image.LANCZOS), np.float64) / 255
    alpha = tcov.copy(); col[:] = np.array([0.35, 0.27, 0.18])[None, None] * tcov[..., None]
    leaves = []
    for i in range(count):
        a = r.uniform(0, 2 * np.pi); rad = N * 0.36 * np.sqrt(r.uniform(0.05, 1))
        pos = np.array([N / 2 + rad * np.cos(a), N * 0.47 + rad * np.sin(a) * 0.9])
        size = r.uniform(size_lo, size_hi) * N
        depth = r.uniform(0, 1)
        leaves.append((depth, pos, size))
    leaves.sort(key=lambda t: t[0])
    for depth, pos, size in leaves:
        out = pos - stem; ang = np.arctan2(-out[1], out[0]) - np.pi / 2 + r.uniform(-0.5, 0.5)
        under = r.random() < 0.2
        cov, c, n = leaf_sprite(size, r.normal(0, 0.35, 2), 0.5 + 0.5 * depth * r.uniform(0.85, 1.0), r.uniform(-0.06, 0.06), under)
        cov, c, n = rotate_sprite(cov, c, n, ang)
        h, w = cov.shape
        x0 = int(pos[0] - w / 2); y0 = int(pos[1] - h / 2)
        # keep inside the cell with a margin
        m = 10
        sx0, sy0 = max(0, m - x0), max(0, m - y0); sx1, sy1 = min(w, N - m - x0), min(h, N - m - y0)
        if sx1 <= sx0 or sy1 <= sy0: continue
        A = cov[sy0:sy1, sx0:sx1][..., None]
        sl = (slice(y0 + sy0, y0 + sy1), slice(x0 + sx0, x0 + sx1))
        col[sl] = col[sl] * (1 - A) + c[sy0:sy1, sx0:sx1] * A
        nrm[sl] = nrm[sl] * (1 - A) + n[sy0:sy1, sx0:sx1] * A
        alpha[sl] = alpha[sl] * (1 - A[..., 0]) + A[..., 0]
    # contact darkening: leaves under others get a soft shadow from the coverage above (cheap AO)
    occl = gaussian_filter(alpha, N * 0.02)
    col *= (1 - 0.18 * np.clip(occl - alpha * 0.6, 0, 1))[..., None]
    nrm /= np.linalg.norm(nrm, axis=2, keepdims=True)
    return alpha, col, nrm

def bleed(col, alpha, sigma):
    """Fill transparent texels with nearby leaf colour so mip levels don't darken the edges."""
    w = gaussian_filter(alpha, sigma)[..., None] + 1e-6
    fill = np.dstack([gaussian_filter(col[..., k] * alpha, sigma) for k in range(3)]) / w
    a = np.clip(alpha * 4, 0, 1)[..., None]
    return col * a + fill * (1 - a)

def leaves(species, N=2048):
    use_shape(species)
    H = N // 2
    A = np.zeros((N, N)); C = np.zeros((N, N, 3)); Nn = np.zeros((N, N, 3))
    for q in range(4):
        a, c, n = cluster(H, 100 + q, *CLUSTER[species])
        # quadrant q: UV (q%2*0.5, q//2*0.5) -> image columns q%2, rows from the bottom
        cx = (q % 2) * H; cy = (1 - q // 2) * H
        A[cy:cy + H, cx:cx + H] = a; C[cy:cy + H, cx:cx + H] = c; Nn[cy:cy + H, cx:cx + H] = n
    C = bleed(C, A, 6)
    C = bleed(C, np.clip(A + 0.0, 0, 1), 30)
    rgba = np.dstack([to8(srgb(C)), to8(A)])
    Image.fromarray(rgba, "RGBA").save(OUT + f"world_tree_{species}_leaves_basecolor.png")
    # OpenGL normal: +y = up the image. Our sprite normals' y already points up the image.
    Image.fromarray(to8(Nn * 0.5 + 0.5), "RGB").save(OUT + f"world_tree_{species}_leaves_normal.png")

# ---------------------------------------------------------------- bark
def fbm(N, beta, lo, hi, stretch=1.0, seed=0):
    r = np.random.default_rng(seed)
    kx = np.fft.fftfreq(N) * N; ky = np.fft.fftfreq(N) * N
    KX, KY = np.meshgrid(kx, ky)
    k = np.hypot(KX, KY * stretch)
    amp = np.where((k >= lo) & (k <= hi), 1 / np.maximum(k, 1) ** (beta / 2), 0)
    f = np.fft.ifft2(amp * np.exp(2j * np.pi * r.random((N, N)))).real
    f = (f - f.mean()) / (f.std() + 1e-9)
    return f

BARK = {
    "maple":  dict(seed=0, cells=260, sy=5.0, stretch=6, offset=0.3, width=0.55, grey=(0.20, 0.19, 0.175), pale=(0.30, 0.29, 0.27), furrow=(0.055, 0.042, 0.032), diag=0.0),
    # tulip poplar: narrow, interlacing ridges (a loose diamond net) over darker grey-brown furrows
    # viburnum stems: smooth grey-brown with faint lengthwise streaks and a few small cracks
    "viburnum": dict(seed=2, cells=180, sy=6.0, stretch=8, offset=1.3, width=0.8, grey=(0.15, 0.13, 0.115), pale=(0.22, 0.2, 0.18), furrow=(0.07, 0.055, 0.045), diag=0.0),
    "poplar": dict(seed=1, cells=420, sy=4.0, stretch=7, offset=0.05, width=0.35, grey=(0.17, 0.16, 0.145), pale=(0.27, 0.26, 0.24), furrow=(0.05, 0.04, 0.032), diag=0.55),
}

def bark(species, N=1024):
    B = BARK[species]
    global rng
    rng = np.random.default_rng(20261120 + B["seed"])
    # elongated plates: toroidal Voronoi with cells about 3x taller than wide
    Sy = 3.2
    Sy = B["sy"]
    pts = rng.uniform(0, 1, (B["cells"], 2)) * [N, N / Sy]
    tree = cKDTree(pts, boxsize=[N, N / Sy])
    yy, xx = np.mgrid[0:N, 0:N].astype(np.float64)
    warp = fbm(N, 2.0, 1, 6, stretch=3, seed=5) * 14
    P = np.dstack([(xx + warp) % N, (yy / Sy) % (N / Sy)]).reshape(-1, 2)
    d, idx = tree.query(P, k=2)
    edge = (d[:, 1] - d[:, 0]).reshape(N, N)
    cell = idx[:, 0].reshape(N, N)
    cell_v = rng.uniform(-1, 1, B["cells"])[cell]
    streak = fbm(N, 1.8, 2, 48, stretch=B["stretch"], seed=11 + B["seed"])   # long vertical ridges
    if B["diag"] > 0:   # interlacing: the ridges wander left and right so neighbours meet and part
        yy2, xx2 = np.mgrid[0:N, 0:N]
        wob = fbm(N, 2.0, 1, 6, stretch=2, seed=31) * 18
        streak = np.roll(streak, 0, 1)
        idx = ((xx2 + wob * B["diag"]).astype(int)) % N
        streak = np.take_along_axis(streak, idx, 1)
    ridge = np.clip((streak + cell_v * 0.35 + B["offset"]) / B["width"], 0, 1)
    ridge = ridge * ridge * (3 - 2 * ridge)
    breaks = np.exp(-(edge / 3.0) ** 2) * ridge                          # cross-breaks between plates
    grain = fbm(N, 2.2, 6, 160, stretch=4, seed=7)                       # fibres
    fine = fbm(N, 1.5, 60, 400, seed=8)
    crack = np.clip((1 - ridge) * 0.85 + breaks * 0.6, 0, 1)            # 1 in the furrows
    height = ridge * 0.75 + grain * 0.05 + fine * 0.03 - breaks * 0.3
    # colours (linear): grey plates, warm brown furrows, a little lichen
    grey = np.array(B["grey"]); pale = np.array(B["pale"]); furrow = np.array(B["furrow"])
    t = np.clip(0.5 + grain * 0.18 + cell_v * 0.25 + fine * 0.1, 0, 1)[..., None]
    col = grey + (pale - grey) * t
    lichen = np.clip(fbm(N, 2.0, 3, 30, seed=9) - 1.3, 0, 1)[..., None] * (1 - crack[..., None])
    col = col * (1 - lichen * 0.6) + np.array([0.26, 0.29, 0.2]) * lichen * 0.6
    col = col * (1 - crack[..., None] * 0.85) + furrow * crack[..., None] * 0.85
    # normal from the periodic height gradient
    s = 6.0
    gx = (np.roll(height, -1, 1) - np.roll(height, 1, 1)) * s
    gy = (np.roll(height, -1, 0) - np.roll(height, 1, 0)) * s
    n = np.dstack([-gx, gy, np.ones_like(gx)]); n /= np.linalg.norm(n, axis=2, keepdims=True)
    ao = np.clip(1 - crack * 0.55 - np.clip(-height, 0, 1) * 0.3, 0, 1)
    smooth = np.clip(0.2 - crack * 0.12 + fine * 0.02, 0, 1)
    Image.fromarray(to8(srgb(col)), "RGB").save(OUT + f"world_tree_{species}_bark_basecolor.png")
    Image.fromarray(to8(n * 0.5 + 0.5), "RGB").save(OUT + f"world_tree_{species}_bark_normal.png")
    Image.fromarray(to8(np.dstack([np.zeros_like(ao), ao, np.zeros_like(ao), smooth])), "RGBA").save(OUT + f"world_tree_{species}_bark_mask.png")

if __name__ == "__main__":
    species = sys.argv[1]
    what = sys.argv[2] if len(sys.argv) > 2 else "all"
    if what in ("all", "leaves"): leaves(species)
    if what in ("all", "bark"): bark(species)
