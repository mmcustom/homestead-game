# Tileable water textures (2026-11-19): ripple normal map and a foam pattern.
import numpy as np
from PIL import Image
from scipy.spatial import cKDTree
from scipy.ndimage import gaussian_filter
N = 512
def fbm(beta, lo, hi, seed):
    r = np.random.default_rng(seed); F = np.fft.fft2(r.normal(size=(N, N)))
    fy = np.fft.fftfreq(N)[:, None] * N; fx = np.fft.fftfreq(N)[None, :] * N
    f = np.sqrt(fx * fx + fy * fy); f[0, 0] = 1
    a = f ** (-beta / 2); a[f < lo] = 0; a *= np.exp(-(f / hi) ** 2)
    n = np.real(np.fft.ifft2(F * a)); return (n - n.mean()) / n.std()
# ripples: soft rounded wavelets, a few sizes
h = fbm(3.4, 2, 22, 1) * 0.75 + fbm(2.8, 6, 50, 2) * 0.25
h = np.tanh(h * 0.8)
s = 5.0
dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * 0.5 * s; dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * 0.5 * s
n = np.dstack([-dx, dy, np.ones_like(h)]); n /= np.linalg.norm(n, axis=2, keepdims=True)
Image.fromarray(((n * 0.5 + 0.5) * 255 + 0.5).astype(np.uint8), 'RGB').save("water_ripple_normal.png")
# foam: bubbly lace, the gaps between packed cells, broken by noise
r = np.random.default_rng(5); pts = r.uniform(0, N, (1800, 2)); tree = cKDTree(pts, boxsize=N)
yy, xx = np.mgrid[0:N, 0:N]; P = np.dstack([yy, xx]).reshape(-1, 2) + 0.5
d, _ = tree.query(P, k=2); d = d.reshape(N, N, 2); edge = d[..., 1] - d[..., 0]
lace = np.clip(1 - edge / 3.0, 0, 1) ** 1.5
blot = (fbm(2.0, 2, 30, 6) * 0.5 + 0.5)
foam = np.clip(lace * 0.8 + np.clip(blot - 0.2, 0, 1) * 0.6, 0, 1)
foam = gaussian_filter(foam, 0.6, mode='wrap')
Image.fromarray((foam * 255).astype(np.uint8), 'L').save("water_foam.png")
print("ok")
