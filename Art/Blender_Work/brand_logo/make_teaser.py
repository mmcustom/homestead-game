# Wave 10 teaser (2026-11-20): five in-game camera moves recorded by MarketingCamera (Temp/Marketing/teaser/*.mjpeg), each with one
# line of the game's motto, then the logo; the Main Menu theme under it. Needs ffmpeg.
import os, subprocess, sys
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import numpy as np

SHOTS = sys.argv[1]; FONTS = sys.argv[2]; LOGO = sys.argv[3]; MUSIC = sys.argv[4]; OUT = sys.argv[5]
W, H = 1920, 1080
CREAM = (237, 227, 199, 255); GOLD = (214, 186, 128, 255)
work = os.path.join(OUT, "work"); os.makedirs(work, exist_ok=True)

def font(n, s): return ImageFont.truetype(os.path.join(FONTS, n), s)

def card(text, sub=None, where="lower"):
    """A transparent 1920x1080 overlay: a big line in Alegreya Bold with a soft shadow, optional smaller line under it."""
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(img)
    y = int(H * (0.78 if where == "lower" else 0.5))
    f = font("Alegreya-Bold.ttf", 96)
    d.text((W // 2, y), text, font=f, fill=CREAM, anchor="mm")
    if sub:
        d.text((W // 2, y + 82), sub, font=font("AlegreyaSans-Bold.ttf", 40), fill=GOLD, anchor="mm")
    a = np.asarray(img)[..., 3].astype(float)
    from scipy.ndimage import gaussian_filter
    sh = np.zeros((H, W, 4), np.uint8); sh[..., 3] = np.clip(gaussian_filter(a, 10) * 0.9, 0, 255).astype(np.uint8)
    out = Image.fromarray(sh, "RGBA"); out = out.transform(out.size, Image.AFFINE, (1, 0, -3, 0, 1, -5))
    # A soft dark band behind the words so they read on snow as well as on grass.
    yy = np.linspace(0, 1, H)[:, None]
    band = np.clip(1 - np.abs(yy - (y + (40 if sub else 0)) / H) / 0.14, 0, 1) ** 1.5 * 150
    shade = np.zeros((H, W, 4), np.uint8); shade[..., 3] = np.broadcast_to(band, (H, W)).astype(np.uint8)
    base = Image.fromarray(shade, "RGBA"); base.alpha_composite(out); base.alpha_composite(img)
    return base

# (shot file, seconds used, the line)
plan = [("01_spring", 4.6, "Survive.", "Find water. Make fire. Get through the first night."),
        ("02_fall", 4.6, "Prepare.", "Learn the land, season by season."),
        ("04_winter", 4.6, "Build.", "A homestead, log by log."),
        ("05_storm", 4.6, "Endure.", "Winter tests everything you put by."),
        ("06_dusk", 5.6, "Prosper.", None)]
FADE = 0.6

clips = []
for i, (name, secs, line, sub) in enumerate(plan):
    overlay = os.path.join(work, f"text_{i}.png"); card(line, sub).save(overlay)
    clip = os.path.join(work, f"clip_{i}.mp4")
    subprocess.run(["ffmpeg", "-loglevel", "error", "-y", "-f", "mjpeg", "-framerate", "30", "-i", os.path.join(SHOTS, name + ".mjpeg"),
                    "-loop", "1", "-framerate", "30", "-i", overlay, "-filter_complex",
                    f"[1]format=rgba,fade=in:st=0.5:d=0.7:alpha=1,fade=out:st={secs - 1.2}:d=0.6:alpha=1[t];[0][t]overlay=shortest=1,format=yuv420p",
                    "-t", str(secs), "-r", "30", "-c:v", "libx264", "-crf", "16", "-preset", "medium", clip], check=True)
    clips.append((clip, secs))

# End card: the key art behind the logo lockup and a line, 5.5 s.
bg = Image.open(os.path.join(LOGO, "title_background.png")).convert("RGB").resize((W, H))
a = np.asarray(bg).astype(float) * 0.68
end = Image.fromarray(a.astype(np.uint8)).convert("RGBA")
lock = Image.open(os.path.join(LOGO, "logo_lockup.png")).convert("RGBA")
lock = lock.resize((1100, int(lock.height * 1100 / lock.width)), Image.LANCZOS)
end.alpha_composite(lock, ((W - lock.width) // 2, 150))
d = ImageDraw.Draw(end)
d.text((W // 2, 150 + lock.height + 70), "In development for Windows PC", font=font("AlegreyaSans-Bold.ttf", 44), fill=CREAM, anchor="mm")
d.text((W // 2, H - 70), "MM Custom", font=font("AlegreyaSans-Regular.ttf", 34), fill=GOLD, anchor="mm")
endpng = os.path.join(work, "end.png"); end.convert("RGB").save(endpng)
endclip = os.path.join(work, "clip_end.mp4")
subprocess.run(["ffmpeg", "-loglevel", "error", "-y", "-loop", "1", "-framerate", "30", "-i", endpng, "-t", "5.5",
                "-vf", "format=yuv420p", "-r", "30", "-c:v", "libx264", "-crf", "16", endclip], check=True)
clips.append((endclip, 5.5))

# Cross-fade them all together, black fade at both ends, the theme under it.
inputs, chain, offset = [], "", 0.0
for c, _ in clips:
    inputs += ["-i", c]
prev = "[0:v]"
for i in range(1, len(clips)):
    offset += clips[i - 1][1] - FADE
    out = f"[v{i}]"
    chain += f"{prev}[{i}:v]xfade=transition=fade:duration={FADE}:offset={offset:.3f}{out};"
    prev = out
total = sum(s for _, s in clips) - FADE * (len(clips) - 1)
chain += f"{prev}fade=in:st=0:d=0.8,fade=out:st={total - 1.2:.3f}:d=1.2[v]"
audio_idx = len(clips)
chain += f";[{audio_idx}:a]atrim=0:{total:.3f},afade=in:st=0:d=1.5,afade=out:st={total - 2.5:.3f}:d=2.5,volume=0.9[a]"
final = os.path.join(OUT, "homestead_teaser_1080p.mp4")
subprocess.run(["ffmpeg", "-loglevel", "error", "-y", *inputs, "-i", MUSIC, "-filter_complex", chain, "-map", "[v]", "-map", "[a]",
                "-c:v", "libx264", "-crf", "18", "-preset", "slow", "-pix_fmt", "yuv420p", "-c:a", "aac", "-b:a", "192k",
                "-movflags", "+faststart", final], check=True)
print(final, f"{total:.1f}s")
