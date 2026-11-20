# Wave 7 UI direction mockups: one game moment (Inventory open over the woods, HUD showing) drawn in three looks.
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter

UP = "/mnt/user-data/uploads/Homestead Game/01_Unity_Project/Homestead/"
F = "/home/claude/ui/fonts/"
OUT = "/home/claude/ui/"
W, H = 1600, 900
rng = np.random.default_rng(4)

def font(name, size, wght=None):
    f = ImageFont.truetype(F + name, size)
    if wght is not None:
        try: f.set_variation_by_axes([wght])
        except Exception: pass
    return f

def noise(h, w, s, seed):
    from scipy.ndimage import gaussian_filter
    n = gaussian_filter(np.random.default_rng(seed).standard_normal((h, w)), s, mode="wrap"); return n / (n.std() + 1e-9)

def tex_parchment(w, h):
    n = noise(h, w, 30, 1) * 0.5 + noise(h, w, 4, 2) * 0.25 + noise(h, w, 1, 3) * 0.1
    base = np.array([226, 210, 176]); dark = np.array([196, 172, 128])
    t = np.clip(0.5 + n * 0.25, 0, 1)[..., None]
    yy, xx = np.mgrid[0:h, 0:w]
    edge = np.clip(np.minimum.reduce([xx, yy, w - 1 - xx, h - 1 - yy]) / 40.0, 0, 1)[..., None]
    c = (base * t + dark * (1 - t)) * (0.82 + 0.18 * edge)
    return Image.fromarray(np.clip(c, 0, 255).astype(np.uint8))

def tex_wood(w, h, horizontal=True):
    yy, xx = np.mgrid[0:h, 0:w].astype(float)
    a = yy if horizontal else xx
    n = noise(h, w, 2, 5)
    grain = np.sin(a * 0.35 + n * 2.5 + noise(h, w, 25, 6) * 3) * 0.5 + 0.5
    base = np.array([74, 50, 32]); light = np.array([112, 78, 50])
    c = base + (light - base) * (grain[..., None] * 0.7 + 0.15)
    return Image.fromarray(np.clip(c, 0, 255).astype(np.uint8))

def tex_canvas(w, h, colour):
    yy, xx = np.mgrid[0:h, 0:w].astype(float)
    weave = (np.sin(xx * 1.6) * np.sin(yy * 1.6)) * 0.04 + noise(h, w, 20, 7) * 0.05 + noise(h, w, 1, 8) * 0.03
    c = np.array(colour, float)[None, None] * (1 + weave[..., None])
    return Image.fromarray(np.clip(c, 0, 255).astype(np.uint8))

ICONS = [("item_logs", "Logs", 4, "48.0 kg"), ("item_firewood", "Firewood", 12, "18.0 kg"), ("item_stone", "Stone", 7, "14.0 kg"),
         ("item_clay", "Clay", 3, "6.0 kg"), ("item_wild_apples", "Wild Apples", 9, "1.4 kg"), ("item_catfish", "Catfish", 2, "2.6 kg")]

def icon(name, size):
    return Image.open(UP + f"Assets/Resources/Icons/{name}.png").convert("RGBA").resize((size, size), Image.LANCZOS)

def background():
    bg = Image.open(UP + "Temp/McpShots/woods_ground.png").convert("RGB").resize((W, H))
    return bg

def draw_mock(style):
    S = STYLES[style]
    img = background().convert("RGBA")
    # the game behind an open screen is dimmed a little
    img = Image.alpha_composite(img, Image.new("RGBA", (W, H), (0, 0, 0, S["dim"])))
    d = ImageDraw.Draw(img)
    # ---------- compass strip (top centre)
    cx = W // 2
    S["strip"](img, (cx - 260, 18, cx + 260, 62))
    d = ImageDraw.Draw(img)
    for i, lab in enumerate(["W", "NW", "N", "NE", "E"]):
        x = cx - 200 + i * 100
        f = S["display"](26 if len(lab) == 1 else 20)
        d.text((x, 40), lab, font=f, fill=S["accent"] if lab == "N" else S.get("striptext", S["text"]), anchor="mm")
    d.polygon([(cx - 8, 66), (cx + 8, 66), (cx, 76)], fill=S["accent"])
    # ---------- the Inventory panel (right)
    px0, py0, px1, py1 = 900, 100, 1560, 860
    S["panel"](img, (px0, py0, px1, py1))
    d = ImageDraw.Draw(img)
    d.text((px0 + 36, py0 + 34), "Inventory", font=S["display"](44), fill=S["title"], anchor="lm")
    d.text((px1 - 36, py0 + 36), "Load 90.0 / 120 kg", font=S["body"](24), fill=S["muted"], anchor="rm")
    # tabs
    tx = px0 + 30
    for i, tab in enumerate(["All", "Materials", "Food", "Tools"]):
        f = S["body"](24, bold=True)
        tw = d.textlength(tab, font=f) + 40
        S["tab"](img, (tx, py0 + 74, tx + tw, py0 + 118), i == 0)
        d = ImageDraw.Draw(img)
        d.text((tx + tw / 2, py0 + 96), tab, font=f, fill=S["tab_text"](i == 0), anchor="mm")
        tx += tw + 10
    # rows
    y = py0 + 136
    for i, (ic, name, qty, kg) in enumerate(ICONS):
        S["row"](img, (px0 + 28, y, px1 - 28, y + 72), i == 1)
        img.alpha_composite(icon(ic, 60), (px0 + 40, y + 6))
        d = ImageDraw.Draw(img)
        d.text((px0 + 118, y + 36), name, font=S["body"](30), fill=S["text"], anchor="lm")
        d.text((px1 - 190, y + 36), f"x{qty}", font=S["body"](30, bold=True), fill=S["text"], anchor="rm")
        d.text((px1 - 48, y + 36), kg, font=S["body"](24), fill=S["muted"], anchor="rm")
        y += 82
    # buttons
    bx = px0 + 28
    for lab, primary in (("Use", True), ("Drop", False), ("Close  (Tab)", False)):
        f = S["body"](26, bold=True)
        bw = d.textlength(lab, font=f) + 56
        S["button"](img, (bx, py1 - 82, bx + bw, py1 - 28), primary)
        d = ImageDraw.Draw(img)
        d.text((bx + bw / 2, py1 - 55), lab, font=f, fill=S["button_text"](primary), anchor="mm")
        bx += bw + 14
    # ---------- survival bars (bottom left)
    bars = [("Health", 0.82, (186, 60, 52)), ("Stamina", 0.55, (212, 170, 60)), ("Warmth", 0.7, (214, 120, 52)),
            ("Water", 0.4, (70, 140, 200)), ("Food", 0.63, (120, 160, 70))]
    S["hudbox"](img, (30, 560, 420, 860))
    d = ImageDraw.Draw(img)
    y = 588
    for lab, v, col in bars:
        d.text((52, y + 14), lab, font=S["body"](24, bold=True), fill=S["hudtext"], anchor="lm")
        S["bar"](img, (170, y + 2, 396, y + 26), v, col)
        d = ImageDraw.Draw(img)
        y += 52
    # ---------- crosshair and prompt (centre, left of the panel the screen is still the world)
    ccx, ccy = 450, 450
    S["crosshair"](d, ccx, ccy)
    S["prompt"](img, ccx, ccy + 60, "E", "Chop the Sugar Maple")
    # ---------- toast
    S["toast"](img, (40, 110, 520, 180), "Discovered: Black Walnut Trees")
    img.convert("RGB").save(OUT + f"mock_{style}.png")

# ---------------------------------------------------------------- the three looks
def rounded(img, box, r, fill, outline=None, width=0):
    d = ImageDraw.Draw(img); d.rounded_rectangle(box, r, fill=fill, outline=outline, width=width)

def paste_tex(img, box, tex, radius=0, alpha=255):
    x0, y0, x1, y1 = [int(v) for v in box]
    t = tex.resize((x1 - x0, y1 - y0)).convert("RGBA")
    m = Image.new("L", t.size, 0); ImageDraw.Draw(m).rounded_rectangle((0, 0, t.size[0] - 1, t.size[1] - 1), radius, fill=alpha)
    img.paste(t, (x0, y0), m)

def shadow(img, box, r=12, spread=14, strength=150):
    x0, y0, x1, y1 = [int(v) for v in box]
    sh = Image.new("RGBA", img.size, (0, 0, 0, 0))
    ImageDraw.Draw(sh).rounded_rectangle((x0 + 6, y0 + 10, x1 + 6, y1 + 10), r, fill=(0, 0, 0, strength))
    img.alpha_composite(sh.filter(ImageFilter.GaussianBlur(spread)))

# A: Homestead Ledger: parchment panels in a dark wood frame, ink-brown text, serif display font
PARCH = tex_parchment(700, 700); WOOD = tex_wood(800, 800)
def A_panel(img, box):
    shadow(img, box)
    paste_tex(img, box, WOOD, 10)
    x0, y0, x1, y1 = box
    paste_tex(img, (x0 + 14, y0 + 14, x1 - 14, y1 - 14), PARCH, 4)
def A_strip(img, box): shadow(img, box, spread=8, strength=110); paste_tex(img, box, WOOD, 8)
def A_tab(img, box, sel): rounded(img, box, 6, (92, 62, 36) if sel else (205, 186, 148), outline=(92, 62, 36), width=2)
def A_row(img, box, sel): rounded(img, box, 6, (236, 222, 190) if not sel else (214, 192, 146), outline=(170, 146, 104), width=1)
def A_button(img, box, primary): rounded(img, box, 8, (92, 62, 36) if primary else (205, 186, 148), outline=(70, 46, 26), width=2)
def A_hudbox(img, box):
    lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ImageDraw.Draw(lay).rounded_rectangle(box, 12, fill=(30, 22, 14, 150)); img.alpha_composite(lay)
def A_bar(img, box, v, col):
    x0, y0, x1, y1 = box; rounded(img, box, 6, (40, 30, 20), outline=(150, 120, 80), width=2)
    rounded(img, (x0 + 3, y0 + 3, x0 + 3 + (x1 - x0 - 6) * v, y1 - 3), 4, col)
def A_cross(d, x, y): d.ellipse((x - 4, y - 4, x + 4, y + 4), fill=(242, 232, 205), outline=(40, 30, 20), width=2)
def A_prompt(img, x, y, key, text):
    d = ImageDraw.Draw(img); f = font("AlegreyaSans-Bold.ttf", 30)
    tw = d.textlength(text, font=f)
    lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ld = ImageDraw.Draw(lay)
    ld.rounded_rectangle((x - tw / 2 - 40, y - 26, x + tw / 2 + 40, y + 26), 10, fill=(30, 22, 14, 170)); img.alpha_composite(lay)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((x - tw / 2 - 30, y - 17, x - tw / 2 + 4, y + 17), 6, fill=(226, 210, 176)); d.text((x - tw / 2 - 13, y), key, font=f, fill=(70, 46, 26), anchor="mm")
    d.text((x + 20, y), text, font=f, fill=(242, 232, 205), anchor="mm")
def A_toast(img, box, text):
    paste_tex(img, box, PARCH, 6); d = ImageDraw.Draw(img); d.rectangle((box[0], box[1], box[0] + 8, box[3]), fill=(92, 62, 36))
    d.text((box[0] + 28, (box[1] + box[3]) / 2), text, font=font("Alegreya.ttf", 30, 700), fill=(60, 40, 22), anchor="lm")

# B: Field Kit: the current dark green, as canvas, with brass edges and the very readable Atkinson Hyperlegible
CANV = tex_canvas(800, 800, (30, 40, 30))
BRASS = (196, 160, 92)
def B_panel(img, box):
    shadow(img, box); paste_tex(img, box, CANV, 14, alpha=245)
    d = ImageDraw.Draw(img); x0, y0, x1, y1 = box
    d.rounded_rectangle((x0 + 6, y0 + 6, x1 - 6, y1 - 6), 10, outline=BRASS, width=2)
    d.line((x0 + 30, y0 + 62, x1 - 30, y0 + 62), fill=(196, 160, 92, 140), width=1)
def B_strip(img, box):
    shadow(img, box, spread=8, strength=110); paste_tex(img, box, CANV, 10, alpha=235)
    ImageDraw.Draw(img).rounded_rectangle(box, 10, outline=BRASS, width=2)
def B_tab(img, box, sel): rounded(img, box, 8, (78, 92, 62) if sel else (44, 56, 42), outline=BRASS if sel else (70, 84, 60), width=2)
def B_row(img, box, sel): rounded(img, box, 8, (52, 66, 48) if sel else (40, 52, 38), outline=BRASS if sel else None, width=2)
def B_button(img, box, primary): rounded(img, box, 10, BRASS if primary else (52, 66, 48), outline=BRASS, width=2)
def B_hudbox(img, box):
    lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ImageDraw.Draw(lay).rounded_rectangle(box, 14, fill=(20, 28, 20, 170)); img.alpha_composite(lay)
def B_bar(img, box, v, col):
    x0, y0, x1, y1 = box; rounded(img, box, 12, (12, 16, 12), outline=(90, 104, 78), width=2)
    rounded(img, (x0 + 3, y0 + 3, x0 + 3 + (x1 - x0 - 6) * v, y1 - 3), 10, col)
def B_cross(d, x, y):
    for dx, dy in ((0, -1), (0, 1), (-1, 0), (1, 0)):
        d.line((x + dx * 6, y + dy * 6, x + dx * 14, y + dy * 14), fill=(20, 20, 20), width=5)
        d.line((x + dx * 6, y + dy * 6, x + dx * 14, y + dy * 14), fill=(240, 234, 214), width=3)
def B_prompt(img, x, y, key, text):
    d = ImageDraw.Draw(img); f = font("AtkinsonHyperlegible-Bold.ttf", 30)
    tw = d.textlength(text, font=f)
    lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ImageDraw.Draw(lay).rounded_rectangle((x - tw / 2 - 44, y - 28, x + tw / 2 + 44, y + 28), 14, fill=(20, 28, 20, 190)); img.alpha_composite(lay)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((x - tw / 2 - 34, y - 18, x - tw / 2 + 2, y + 18), 8, fill=BRASS); d.text((x - tw / 2 - 16, y), key, font=f, fill=(24, 30, 22), anchor="mm")
    d.text((x + 22, y), text, font=f, fill=(240, 234, 214), anchor="mm")
def B_toast(img, box, text):
    paste_tex(img, box, CANV, 10, alpha=240); d = ImageDraw.Draw(img); d.rounded_rectangle(box, 10, outline=BRASS, width=2)
    d.text((box[0] + 26, (box[1] + box[3]) / 2), text, font=font("AtkinsonHyperlegible-Bold.ttf", 28), fill=(240, 234, 214), anchor="lm")

# C: Woodsmoke: smoky charcoal glass, no frames, big clean type, an ember-orange accent
EMBER = (232, 136, 64)
def glass(img, box, r=18, a=200):
    x0, y0, x1, y1 = [int(v) for v in box]
    blur = img.crop((x0, y0, x1, y1)).filter(ImageFilter.GaussianBlur(14))
    img.paste(blur, (x0, y0))
    lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ImageDraw.Draw(lay).rounded_rectangle(box, r, fill=(22, 22, 24, a)); img.alpha_composite(lay)
def C_panel(img, box): shadow(img, box, r=18, strength=110); glass(img, box)
def C_strip(img, box): glass(img, box, r=22, a=170)
def C_tab(img, box, sel):
    if sel:
        d = ImageDraw.Draw(img); d.rounded_rectangle(box, 22, fill=EMBER)
    else:
        lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ImageDraw.Draw(lay).rounded_rectangle(box, 22, fill=(255, 255, 255, 26)); img.alpha_composite(lay)
def C_row(img, box, sel):
    lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ld = ImageDraw.Draw(lay)
    ld.rounded_rectangle(box, 12, fill=(255, 255, 255, 46 if sel else 16)); img.alpha_composite(lay)
    if sel: ImageDraw.Draw(img).rounded_rectangle((box[0], box[1] + 12, box[0] + 5, box[3] - 12), 2, fill=EMBER)
def C_button(img, box, primary):
    if primary: rounded(img, box, 27, EMBER)
    else:
        lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ImageDraw.Draw(lay).rounded_rectangle(box, 27, fill=(255, 255, 255, 30), outline=(255, 255, 255, 90), width=2); img.alpha_composite(lay)
def C_hudbox(img, box): pass
def C_bar(img, box, v, col):
    x0, y0, x1, y1 = box
    lay = Image.new("RGBA", img.size, (0, 0, 0, 0)); ImageDraw.Draw(lay).rounded_rectangle((x0, y0 + 6, x1, y1 - 6), 7, fill=(0, 0, 0, 120)); img.alpha_composite(lay)
    rounded(img, (x0, y0 + 6, x0 + (x1 - x0) * v, y1 - 6), 7, col)
def C_cross(d, x, y): d.ellipse((x - 5, y - 5, x + 5, y + 5), fill=(250, 250, 250), outline=(0, 0, 0), width=2)
def C_prompt(img, x, y, key, text):
    d = ImageDraw.Draw(img); f = font("SourceSans3.ttf", 32, 600)
    tw = d.textlength(text, font=f)
    glass(img, (x - tw / 2 - 46, y - 28, x + tw / 2 + 46, y + 28), r=28, a=170)
    d = ImageDraw.Draw(img)
    d.ellipse((x - tw / 2 - 36, y - 18, x - tw / 2, y + 18), fill=(250, 250, 250)); d.text((x - tw / 2 - 18, y), key, font=f, fill=(22, 22, 24), anchor="mm")
    d.text((x + 22, y), text, font=f, fill=(250, 250, 250), anchor="mm")
def C_toast(img, box, text):
    glass(img, box, r=16, a=190); d = ImageDraw.Draw(img); d.rounded_rectangle((box[0], box[1] + 14, box[0] + 6, box[3] - 14), 3, fill=EMBER)
    d.text((box[0] + 26, (box[1] + box[3]) / 2), text, font=font("SourceSans3.ttf", 30, 600), fill=(250, 250, 250), anchor="lm")

def fam(regular, boldfile, var=False):
    def f(size, bold=False):
        if var: return font(regular, size, 700 if bold else 450)
        return font(boldfile if bold else regular, size)
    return f

STYLES = {
    "A_ledger": dict(dim=40, panel=A_panel, strip=A_strip, tab=A_tab, row=A_row, button=A_button, hudbox=A_hudbox, bar=A_bar,
                     crosshair=A_cross, prompt=A_prompt, toast=A_toast,
                     display=lambda s: font("Alegreya.ttf", s, 700), body=fam("AlegreyaSans-Regular.ttf", "AlegreyaSans-Bold.ttf"),
                     title=(60, 40, 22), text=(48, 34, 20), muted=(110, 88, 60), accent=(232, 110, 70), hudtext=(242, 232, 205), striptext=(242, 232, 205),
                     tab_text=lambda sel: (242, 232, 205) if sel else (70, 46, 26), button_text=lambda p: (242, 232, 205) if p else (70, 46, 26)),
    "B_fieldkit": dict(dim=40, panel=B_panel, strip=B_strip, tab=B_tab, row=B_row, button=B_button, hudbox=B_hudbox, bar=B_bar,
                       crosshair=B_cross, prompt=B_prompt, toast=B_toast,
                       display=lambda s: font("Bitter.ttf", s, 700), body=fam("AtkinsonHyperlegible-Regular.ttf", "AtkinsonHyperlegible-Bold.ttf"),
                       title=(232, 212, 160), text=(240, 234, 214), muted=(176, 180, 156), accent=BRASS, hudtext=(240, 234, 214),
                       tab_text=lambda sel: (240, 234, 214), button_text=lambda p: (24, 30, 22) if p else (240, 234, 214)),
    "C_woodsmoke": dict(dim=30, panel=C_panel, strip=C_strip, tab=C_tab, row=C_row, button=C_button, hudbox=C_hudbox, bar=C_bar,
                        crosshair=C_cross, prompt=C_prompt, toast=C_toast,
                        display=lambda s: font("Oswald.ttf", s, 500), body=fam("SourceSans3.ttf", None, var=True),
                        title=(250, 250, 250), text=(250, 250, 250), muted=(190, 190, 192), accent=EMBER, hudtext=(250, 250, 250),
                        tab_text=lambda sel: (22, 22, 24) if sel else (250, 250, 250), button_text=lambda p: (22, 22, 24) if p else (250, 250, 250)),
}

if __name__ == "__main__":
    for s in STYLES: draw_mock(s)
