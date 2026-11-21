# Wave 10 Steam store art (2026-11-20): capsules, library art, page background and screenshots, from in-game 4K captures and the logo.
# Layout: the homestead at autumn dusk (title_raw) with the cabin low in the frame and the logo in the open sky above it.
import os, sys
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter
from scipy.ndimage import gaussian_filter

CAP = sys.argv[1]      # folder with the captures (title_raw.png, loss_raw2.png, shot_*.png)
LOGO = sys.argv[2]     # folder with logo_emblem.png, logo_lockup.png
FONTS = sys.argv[3]
OUT = sys.argv[4]
os.makedirs(f"{OUT}/screenshots", exist_ok=True)
CREAM = (237, 227, 199); GOLD = (214, 186, 128)

def grade(img, lift=(0.012, 0.006, 0.0), gain=(1.08, 1.0, 0.86), sat=1.08, contrast=1.06):
    a = np.asarray(img.convert("RGB")).astype(float) / 255.0
    l = a.mean(axis=2, keepdims=True)
    a = l + (a - l) * sat
    a = (a - 0.5) * contrast + 0.5
    a = a * np.array(gain) + np.array(lift)
    h = a.shape[0]; yy = np.linspace(0, 1, h)[:, None, None]
    a = a * (1 - 0.25 * np.clip(0.35 - yy, 0, 1) / 0.35) + np.array([0.10, 0.05, 0.0]) * np.exp(-((yy - 0.48) / 0.12) ** 2)
    return Image.fromarray((np.clip(a, 0, 1) * 255).astype(np.uint8))

def vignette(img, strength=0.4, cx=0.5, cy=0.55):
    a = np.asarray(img).astype(float) / 255.0
    h, w = a.shape[:2]; yy, xx = np.mgrid[0:h, 0:w]
    r = np.sqrt(((xx / w - cx) / 0.75) ** 2 + ((yy / h - cy) / 0.62) ** 2)
    a = a * (1 - strength * np.clip(r - 0.35, 0, 1) ** 1.5)[..., None]
    return Image.fromarray((np.clip(a, 0, 1) * 255).astype(np.uint8))

KEY = grade(Image.open(f"{CAP}/title_raw.png"))           # 3840 x 2160
CABIN = (0.49, 0.57)                                        # cabin centre in the capture (x, y fractions)

def crop(aspect, width_frac, cabin_at=(0.5, 0.7), src=None):
    """A crop of the key art of the given aspect, width_frac of the capture wide, with the cabin at cabin_at within the crop."""
    src = src or KEY
    W, H = src.size
    w = W * width_frac; h = w / aspect
    if h > H:
        h = H; w = h * aspect
    x0 = CABIN[0] * W - cabin_at[0] * w; y0 = CABIN[1] * H - cabin_at[1] * h
    x0 = min(max(0, x0), W - w); y0 = min(max(0, y0), H - h)
    return src.crop((int(x0), int(y0), int(x0 + w), int(y0 + h)))

def shadowed(logo, radius, strength=0.85, offset=(0, 0)):
    a = np.asarray(logo)[..., 3].astype(float)
    pad = radius * 3
    big = np.zeros((a.shape[0] + 2 * pad, a.shape[1] + 2 * pad)); big[pad:-pad, pad:-pad] = a
    sh = gaussian_filter(big, radius) * strength
    out = np.zeros(big.shape + (4,), np.uint8); out[..., 3] = np.clip(sh, 0, 255)
    img = Image.fromarray(out, "RGBA")
    img.alpha_composite(logo, (pad + offset[0], pad + offset[1]))
    return img, pad

LOCKUP = Image.open(f"{LOGO}/logo_lockup.png").convert("RGBA")   # emblem over HOMESTEAD over the motto (has its own soft shadow)
EMBLEM = Image.open(f"{LOGO}/logo_emblem.png").convert("RGBA")


def font(name, size): return ImageFont.truetype(os.path.join(FONTS, name), size)

def wordmark(height, motto=True):
    """HOMESTEAD (and the motto) on transparent, `height` px tall, no emblem."""
    S = 4
    f = font("Alegreya-Bold.ttf", int(height * (0.72 if motto else 1.0) * S))
    word = "HOMESTEAD"; spacing = int(f.size * 0.09)
    tmp = ImageDraw.Draw(Image.new("RGBA", (10, 10)))
    widths = [tmp.textlength(c, font=f) for c in word]
    tw = int(sum(widths) + spacing * (len(word) - 1))
    H = int(height * S); img = Image.new("RGBA", (tw + 20 * S, H), (0, 0, 0, 0)); d = ImageDraw.Draw(img)
    x = 10 * S; y = int(H * (0.36 if motto else 0.5))
    for c, w in zip(word, widths):
        d.text((x, y), c, font=f, fill=CREAM + (255,), anchor="lm"); x += w + spacing
    if motto:
        m = font("AlegreyaSans-Bold.ttf", int(height * 0.17 * S))
        d.text((img.width / 2, int(H * 0.86)), "Survive  ·  Prepare  ·  Build  ·  Prosper", font=m, fill=GOLD + (255,), anchor="mm")
    img = img.crop(img.getbbox())
    return img.resize((max(1, img.width // S), max(1, img.height // S)), Image.LANCZOS)

def horizontal_lockup(height):
    """The emblem on the left, HOMESTEAD on the right: for short, wide capsules."""
    e = EMBLEM.resize((height, height), Image.LANCZOS)
    w = wordmark(int(height * 0.62), motto=False)
    img = Image.new("RGBA", (height + int(height * 0.12) + w.width, height), (0, 0, 0, 0))
    img.alpha_composite(e, (0, 0)); img.alpha_composite(w, (height + int(height * 0.12), (height - w.height) // 2))
    return img

def stacked_lockup(width):
    """The emblem over HOMESTEAD, no motto: Steam capsules may carry only the game's name and art."""
    w = wordmark(int(width * 0.2), motto=False)
    w = w.resize((width, int(w.height * width / w.width)), Image.LANCZOS)
    e = EMBLEM.resize((int(width * 0.42), int(width * 0.42)), Image.LANCZOS)
    gap = int(width * 0.04)
    img = Image.new("RGBA", (width, e.height + gap + w.height), (0, 0, 0, 0))
    img.alpha_composite(e, ((width - e.width) // 2, 0)); img.alpha_composite(w, (0, e.height + gap))
    return img

CAPSULE_LOGO = None

def place(base, logo, cx, cy, width=None, height=None, shadow=12):
    if width:
        logo = logo.resize((int(width), int(logo.height * width / logo.width)), Image.LANCZOS)
    elif height:
        logo = logo.resize((int(logo.width * height / logo.height), int(height)), Image.LANCZOS)
    s, pad = shadowed(logo, shadow, 0.75, (0, max(1, shadow // 3)))
    base = base.convert("RGBA")
    base.alpha_composite(s, (int(cx - logo.width / 2 - pad), int(cy - logo.height / 2 - pad)))
    return base

def sky_shade(img, top=0.0, bottom=0.55, strength=0.45):
    a = np.asarray(img.convert("RGB")).astype(float) / 255.0
    h = a.shape[0]; yy = np.linspace(0, 1, h)[:, None, None]
    m = np.clip((bottom - yy) / max(1e-3, bottom - top), 0, 1) ** 1.3
    return Image.fromarray((np.clip(a * (1 - strength * m), 0, 1) * 255).astype(np.uint8))

def save(img, name, size):
    img = img.convert("RGB").resize(size, Image.LANCZOS) if img.size != size else img.convert("RGB")
    img.save(f"{OUT}/{name}.png"); print(name, img.size)

# --- capsules with the logo (Steam: the game's name must be readable on every capsule)
# Header capsule 920 x 430 (also the library header)
b = vignette(sky_shade(crop(920 / 430, 0.95, (0.5, 0.78)), 0, 0.6, 0.35)).resize((920, 430), Image.LANCZOS)
save(place(b, stacked_lockup(900), 460, 135, height=220, shadow=10), "header_capsule_920x430", (920, 430))

# Main capsule 1232 x 706
b = vignette(sky_shade(crop(1232 / 706, 0.95, (0.5, 0.84)), 0, 0.6, 0.35)).resize((1232, 706), Image.LANCZOS)
save(place(b, stacked_lockup(900), 616, 195, height=320, shadow=14), "main_capsule_1232x706", (1232, 706))

# Small capsule 462 x 174: the horizontal lockup big, the picture behind it darkened
b = sky_shade(crop(462 / 174, 0.9, (0.5, 0.7)), 0, 1.0, 0.55).resize((462, 174), Image.LANCZOS)
save(place(b, horizontal_lockup(400), 231, 87, width=420, shadow=6), "small_capsule_462x174", (462, 174))

# Vertical capsule 748 x 896
b = vignette(sky_shade(crop(748 / 896, 0.42, (0.5, 0.76)), 0, 0.55, 0.35)).resize((748, 896), Image.LANCZOS)
save(place(b, stacked_lockup(900), 374, 225, width=560, shadow=14), "vertical_capsule_748x896", (748, 896))

# Library capsule 600 x 900
b = vignette(sky_shade(crop(600 / 900, 0.37, (0.5, 0.78)), 0, 0.55, 0.35)).resize((600, 900), Image.LANCZOS)
save(place(b, stacked_lockup(900), 300, 225, width=470, shadow=12), "library_capsule_600x900", (600, 900))

# Library hero 3840 x 1240: no logo or text (Steam lays the library logo over it); the cabin in the middle
b = vignette(crop(3840 / 1240, 1.0, (0.5, 0.7)), 0.3).resize((3840, 1240), Image.LANCZOS)
save(b, "library_hero_3840x1240", (3840, 1240))

# Library logo 1280 x 720: the lockup on transparent, as large as fits
logo = stacked_lockup(1200); logo = logo.crop(logo.getbbox())
scale = min(1240 / logo.width, 680 / logo.height); logo = logo.resize((int(logo.width * scale), int(logo.height * scale)), Image.LANCZOS)
canvas = Image.new("RGBA", (1280, 720), (0, 0, 0, 0)); canvas.alpha_composite(logo, ((1280 - logo.width) // 2, (720 - logo.height) // 2))
canvas.save(f"{OUT}/library_logo_1280x720.png"); print("library_logo", canvas.size)

# Page background 1438 x 810: the key art, dark and soft so the store text reads over it
b = crop(1438 / 810, 1.0, (0.5, 0.6)).resize((1438, 810), Image.LANCZOS).filter(ImageFilter.GaussianBlur(3))
a = np.asarray(b).astype(float) * 0.45
save(Image.fromarray(a.astype(np.uint8)), "page_background_1438x810", (1438, 810))

# Community icon 184 x 184 and a 32 x 32 client icon (the emblem)
ic = Image.new("RGB", (184, 184), (40, 28, 18)); e = EMBLEM.resize((184, 184), Image.LANCZOS); ic.paste(e, (0, 0), e); ic.save(f"{OUT}/community_icon_184x184.jpg", quality=95)  # Steam wants a JPG
EMBLEM.resize((32, 32), Image.LANCZOS).save(f"{OUT}/client_icon_32x32.png")

# --- screenshots 1920 x 1080, straight from the game (no logo, no grading beyond the game's own)
shots = [("01_autumn_dusk_homestead", "title_raw.png"), ("02_spring_homestead", "shot_spring_homestead.png"),
         ("03_first_person_yard", "shot_firstperson_yard.png"), ("04_fall_woods", "shot_fall_woods.png"),
         ("05_crafting_screen", "shot_crafting.png"), ("06_winter_yard", "shot_winter_yard.png"),
         ("07_winter_dusk_snowfall", "loss_raw2.png")]
for name, f in shots:
    Image.open(f"{CAP}/{f}").convert("RGB").resize((1920, 1080), Image.LANCZOS).save(f"{OUT}/screenshots/{name}.png")
print("screenshots", len(shots))
