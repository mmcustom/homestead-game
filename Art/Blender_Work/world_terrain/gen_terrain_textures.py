# Tileable terrain textures for Homestead (Wave 6, 2026-11-19): grass turf, dirt, gravel.
# Every map wraps seamlessly (periodic noise by FFT, toroidal Voronoi, blades and stones wrapped round the edges).
# Outputs per layer: basecolor (sRGB), normal (OpenGL, Unity's convention), mask (R metallic 0, G AO, B height, A smoothness).
import numpy as np, sys
from PIL import Image
from scipy.spatial import cKDTree
from scipy.ndimage import gaussian_filter
N = int(sys.argv[2]) if len(sys.argv) > 2 else 1024
rng = np.random.default_rng(20261119)
OUT = "/home/claude/terrain/out/"

def fbm(beta=2.0, lo=2, hi=None, seed=None):
    """periodic fractal noise in 0..1: white noise shaped by 1/f^beta between frequencies lo and hi"""
    r = np.random.default_rng(seed) if seed is not None else rng
    w = r.normal(size=(N, N))
    F = np.fft.fft2(w)
    fy = np.fft.fftfreq(N)[:, None] * N; fx = np.fft.fftfreq(N)[None, :] * N
    f = np.sqrt(fx * fx + fy * fy); f[0, 0] = 1
    amp = f ** (-beta / 2)
    amp[f < lo] = 0
    if hi: amp *= np.exp(-(f / hi) ** 2)
    n = np.real(np.fft.ifft2(F * amp))
    n = (n - n.mean()) / (n.std() + 1e-9)
    return 0.5 + 0.18 * n          # roughly 0..1

def wrapblur(a, s): return gaussian_filter(a, s, mode='wrap')

def normal_from_height(h, strength):
    dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * 0.5 * strength
    dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * 0.5 * strength
    n = np.dstack([-dx, dy, np.ones_like(h)])     # image rows go down; OpenGL green up
    n /= np.linalg.norm(n, axis=2, keepdims=True)
    return n * 0.5 + 0.5

def ao_from_height(h, radius, k):
    return np.clip(1 - k * np.maximum(0, wrapblur(h, radius) - h), 0, 1)

def save(name, color, normal, ao, height, smooth):
    def to8(a): return (np.clip(a, 0, 1) * 255 + 0.5).astype(np.uint8)
    srgb = np.where(color <= 0.0031308, color * 12.92, 1.055 * np.power(np.clip(color, 0, 1), 1 / 2.4) - 0.055)
    Image.fromarray(to8(srgb), 'RGB').save(OUT + f"terrain_{name}_basecolor.png")
    Image.fromarray(to8(normal), 'RGB').save(OUT + f"terrain_{name}_normal.png")
    mask = np.dstack([np.zeros_like(ao), ao, height, smooth])
    Image.fromarray(to8(mask), 'RGBA').save(OUT + f"terrain_{name}_mask.png")

def lin(c): return np.array(c, dtype=np.float64)

# ---------------- dirt ----------------
def dirt():
    base = fbm(1.8, 4, seed=1); mid = fbm(1.6, 10, seed=2); fine = fbm(1.0, 60, seed=3)
    # colours in linear: a southern Ohio silt loam, brown, drier and paler on the high spots
    dark = lin([0.075, 0.05, 0.032]); light = lin([0.2, 0.145, 0.095]); dry = lin([0.27, 0.215, 0.16])
    t = np.clip((base - 0.5) * 2.2 + 0.5, 0, 1)[..., None]
    col = dark + (light - dark) * t
    # clods: periodic voronoi bumps
    pts = rng.uniform(0, N, (int(900 * (N / 1024) ** 2), 2)); tree = cKDTree(pts, boxsize=N)
    yy, xx = np.mgrid[0:N, 0:N]; P = np.dstack([yy, xx]).reshape(-1, 2) + 0.5
    d, idx = tree.query(P, k=2); d = d.reshape(N, N, 2)
    edge = (d[..., 1] - d[..., 0]); clod = np.clip(edge / (9.0 * N / 1024), 0, 1) ** 0.6
    h = 0.45 * base + 0.25 * mid + 0.22 * clod + 0.08 * fine
    hi = np.clip((h - wrapblur(h, 6)) * 6 + 0.5, 0, 1)[..., None]
    col = col + (dry - col) * np.clip(hi - 0.5, 0, 1) * 0.9
    col *= (0.85 + 0.3 * fine[..., None])
    # pebbles: small grey-tan stones
    npb = int(500 * (N / 1024) ** 2); pp = rng.uniform(0, N, (npb, 2)); pr = rng.uniform(1.5, 5.0, npb) * N / 1024
    ptree = cKDTree(pp, boxsize=N); dd, ii = ptree.query(P, k=1); dd = dd.reshape(N, N); ii = ii.reshape(N, N)
    inside = dd < pr[ii]; dome = np.where(inside, np.sqrt(np.clip(1 - (dd / pr[ii]) ** 2, 0, 1)), 0)
    pcol = rng.uniform(0.12, 0.26, npb)[:, None] * lin([1.0, 0.9, 0.76])[None, :]
    col = np.where(inside[..., None], pcol[ii] * (0.7 + 0.3 * dome[..., None]), col)
    h = h + dome * 0.18
    # dark organic specks
    sp = fbm(0.5, 150, seed=4); col *= np.where(sp[..., None] < 0.18, 0.55, 1.0)
    height = np.clip((h - h.min()) / (h.max() - h.min()), 0, 1)
    nrm = normal_from_height(height, 9.0)
    ao = ao_from_height(height, 4, 2.2)
    col *= (0.65 + 0.35 * ao[..., None])
    smooth = 0.12 + 0.12 * (1 - hi[..., 0]) + 0.15 * inside
    save("dirt", col, nrm, ao, height, smooth)

# ---------------- gravel ----------------
def gravel():
    """rounded pebbles lying in soil: each stone a dome round its centre (slightly oval and lumpy), the nearest one wins"""
    n_st = int(5200 * (N / 1024) ** 2)
    pts = rng.uniform(0, N, (n_st, 2)); tree = cKDTree(pts, boxsize=N)
    yy, xx = np.mgrid[0:N, 0:N]; P = np.dstack([yy, xx]).reshape(-1, 2) + 0.5
    k = 6
    d, idx = tree.query(P, k=k)
    rad = rng.uniform(5.5, 12.5, n_st) * N / 1024
    ang = rng.uniform(0, np.pi, n_st); ecc = rng.uniform(0.7, 1.0, n_st)
    best_h = np.zeros(P.shape[0]); best_i = np.full(P.shape[0], -1)
    lump = fbm(1.2, 40, seed=10).reshape(-1)
    for j in range(k):
        i = idx[:, j]
        dv = P - pts[i]; dv = (dv + N / 2) % N - N / 2                     # wrapped offset to the stone's centre
        ca, sa = np.cos(ang[i]), np.sin(ang[i])
        u = (dv[:, 0] * ca + dv[:, 1] * sa); v = (-dv[:, 0] * sa + dv[:, 1] * ca) / ecc[i]
        r = np.sqrt(u * u + v * v) / (rad[i] * (0.9 + 0.2 * lump))
        h = np.where(r < 1, np.sqrt(np.clip(1 - r * r, 0, 1)) * (0.6 + 0.4 * rad[i] / (12.5 * N / 1024)), 0)
        take = h > best_h
        best_h = np.where(take, h, best_h); best_i = np.where(take, i, best_i)
    best_h = best_h.reshape(N, N); best_i = best_i.reshape(N, N)
    on = best_i >= 0
    tone = rng.uniform(0, 1, n_st); pick = rng.uniform(0, 1, n_st)
    greys = lin([0.2, 0.195, 0.185]) * (0.7 + 0.6 * tone[:, None])
    buff = lin([0.27, 0.22, 0.16]) * (0.75 + 0.5 * tone[:, None])
    rust = lin([0.24, 0.14, 0.08]) * (0.75 + 0.5 * tone[:, None])
    sc = np.where(pick[:, None] < 0.7, greys, np.where(pick[:, None] < 0.95, buff, rust))
    fine = fbm(1.0, 80, seed=7); spk = fbm(0.6, 200, seed=8)
    soil = lin([0.06, 0.045, 0.032]) * (0.8 + 0.4 * fbm(1.8, 4, seed=9))[..., None]
    col = np.where(on[..., None], sc[np.maximum(best_i, 0)] * (0.8 + 0.4 * fine[..., None]) * (0.88 + 0.12 * spk[..., None]), soil)
    height = best_h + 0.04 * fine
    height = wrapblur(height, 0.6)
    height = (height - height.min()) / (height.max() - height.min())
    nrm = normal_from_height(height, 8.0)
    ao = ao_from_height(height, 3, 2.4)
    col *= (0.5 + 0.5 * ao[..., None])
    smooth = np.where(on, 0.2 + 0.15 * spk, 0.06)
    save("gravel", col, nrm, ao, height, smooth)

# ---------------- grass turf ----------------
def grass():
    soil = lin([0.06, 0.045, 0.03])
    under = fbm(2.0, 2, seed=11)
    patch = fbm(2.4, 1, seed=12)          # greener and drier patches across the tile
    col = np.zeros((N, N, 3)) + soil * (0.7 + 0.6 * under[..., None])
    hbuf = np.zeros((N, N)) + 0.02 * under
    nb = int(N * N * 0.11)
    # each blade: a short stroke seen from above, tip lighter; colours vary with the patches
    x0 = rng.uniform(0, N, nb); y0 = rng.uniform(0, N, nb)
    ang = rng.uniform(0, 2 * np.pi, nb); L = rng.uniform(5, 16, nb) * N / 1024
    tall = rng.uniform(0.4, 1.0, nb)
    pv = patch[(y0.astype(int)) % N, (x0.astype(int)) % N]
    greens = [lin([0.045, 0.09, 0.022]), lin([0.08, 0.14, 0.035]), lin([0.13, 0.17, 0.05]), lin([0.2, 0.19, 0.08])]
    g = np.clip((pv - 0.5) * 2.5 + 1.5 + rng.normal(0, 0.7, nb), 0, 2.999)
    gi = g.astype(int); gf = (g - gi)[:, None]
    G = np.array(greens); bc = G[gi] * (1 - gf) + G[np.minimum(gi + 1, 3)] * gf
    bc *= rng.uniform(0.75, 1.25, nb)[:, None]
    steps = 8
    for k in range(steps + 1):
        t = k / steps
        px = ((x0 + np.cos(ang) * L * t).astype(int)) % N
        py = ((y0 + np.sin(ang) * L * t).astype(int)) % N
        h = tall * (0.15 + 0.85 * t)
        c = bc * (0.55 + 0.6 * t)[..., None] if np.ndim(t) else bc * (0.55 + 0.6 * t)
        order = np.argsort(h)
        px, py, h, c = px[order], py[order], h[order], c[order]
        cur = hbuf[py, px]
        win = h > cur
        hbuf[py[win], px[win]] = h[win]
        col[py[win], px[win]] = c[win]
        # widen a little: the neighbour across the blade
        qx = (px + 1) % N
        win2 = h > hbuf[py, qx]
        hbuf[py[win2], qx[win2]] = h[win2] * 0.97
        col[py[win2], qx[win2]] = c[win2] * 0.85
    # a few dead brown blades and clover-dark spots
    height = wrapblur(hbuf, 0.6)
    height = (height - height.min()) / (height.max() - height.min())
    nrm = normal_from_height(height, 5.0)
    ao = ao_from_height(height, 3, 2.0)
    col *= (0.55 + 0.45 * ao[..., None])
    smooth = 0.18 + 0.1 * height
    save("grass", col, nrm, ao, height, smooth)

which = sys.argv[1]
{"dirt": dirt, "gravel": gravel, "grass": grass}[which]()
print("done", which)
