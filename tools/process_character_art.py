"""Turn the generated character art (art_generated_v2/character/, see gen_character_art.py) into the
mod's character images.

  fullbody.png       -> art_reference/fullbody_flat.png and art/charui/fullbody.png (combat / shop rig)
  rest_sitting.png   -> art_reference/rest_site_sitting_flat.png (rest-site rig, 1086x1448)
  char_select_bg.png -> charui/char_select_bg.png (through make_char_select_bg.py)
  bust.png           -> charui/char_select.png, char_select_locked.png (132x195),
                        character_icon.png + _outline (128, top panel)
  map_marker.png     -> charui/map_marker.png (128)
  hand_*.png         -> charui/hands/hand_{point,rock,paper,scissors}.png (422x1200, the treasure-room hands,
                        slimmed to the base game's proportions with the sleeve stretched to the bottom)

Green-screen backgrounds (#00FF00) are keyed out. Run make_spine_rig.py afterwards.
Usage: python process_character_art.py
"""

import shutil
import subprocess
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageEnhance

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "art_generated_v2" / "character"
REF = ROOT / "art_reference"
CHARUI = ROOT / "DeepSeekWhale" / "DeepSeekWhale" / "images" / "charui"
ART = ROOT / "art" / "charui"

RING = (27, 42, 107, 255)       # navy ring, the character's outline colour
HAND_SIZE = (422, 1200)         # base-game multiplayer_hand_*.png
HAND_FADE = 160                 # the base-game arms fade out over the bottom ~160 px


def key_green(im: Image.Image) -> Image.Image:
    """Soft chroma key for a flat #00FF00 background, with green spill removed from the edges."""
    a = np.asarray(im.convert("RGB")).astype(np.int32)
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    greenness = g - np.maximum(r, b)
    alpha = np.clip((90 - greenness) / 60, 0, 1)          # >=90 fully keyed, <=30 fully kept
    spill = greenness > 0
    g = np.where(spill, np.maximum(r, b), g)
    out = np.dstack([r, g, b, (alpha * 255).round()]).astype(np.uint8)
    return Image.fromarray(out, "RGBA")


def fit(im: Image.Image, size: tuple[int, int], anchor_bottom: bool = True) -> Image.Image:
    """Scale to fit inside size (keeping the aspect ratio) on a transparent canvas."""
    s = min(size[0] / im.width, size[1] / im.height)
    im = im.resize((round(im.width * s), round(im.height * s)), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    y = size[1] - im.height if anchor_bottom else (size[1] - im.height) // 2
    canvas.paste(im, ((size[0] - im.width) // 2, y), im)
    return canvas


def cover(im: Image.Image, size: tuple[int, int], top: float = 0.0) -> Image.Image:
    s = max(size[0] / im.width, size[1] / im.height)
    im = im.resize((round(im.width * s), round(im.height * s)), Image.Resampling.LANCZOS)
    left = (im.width - size[0]) // 2
    y = round((im.height - size[1]) * top)
    return im.crop((left, y, left + size[0], y + size[1]))


def token(face: Image.Image, size: int = 128, ring: int = 6) -> tuple[Image.Image, Image.Image]:
    big = size * 4
    t = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    d = ImageDraw.Draw(t)
    d.ellipse((4, 4, big - 4, big - 4), fill=RING)
    inner = big - 2 * (4 + ring * 4)
    f = face.convert("RGBA").resize((inner, inner), Image.Resampling.LANCZOS)
    mask = Image.new("L", (inner, inner), 0)
    ImageDraw.Draw(mask).ellipse((0, 0, inner - 1, inner - 1), fill=255)
    t.paste(f, (4 + ring * 4,) * 2, mask)
    sil = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    ImageDraw.Draw(sil).ellipse((4, 4, big - 4, big - 4), fill=(255, 255, 255, 255))
    small = lambda im: im.resize((size, size), Image.Resampling.LANCZOS)  # noqa: E731
    return small(t), small(sil)


HAND_SCALE = 0.6     # the base-game hands are slim: about half the width our generated arm comes out at
SLEEVE_SHARE = 0.25  # bottom share of the arm (plain sleeve) that is stretched down to the bottom edge


def hand(src: Path, out: Path, scale: float) -> None:
    im = key_green(Image.open(src))
    box = im.getchannel("A").point(lambda v: 255 if v > 40 else 0).getbbox()
    arm = im.crop(box)
    arm = arm.resize((round(arm.width * scale), round(arm.height * scale)), Image.Resampling.LANCZOS)
    top = 24
    cut = round(arm.height * (1 - SLEEVE_SHARE))
    upper, sleeve = arm.crop((0, 0, arm.width, cut)), arm.crop((0, cut, arm.width, arm.height))
    sleeve = sleeve.resize((arm.width, HAND_SIZE[1] - top - cut + 20), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", HAND_SIZE, (0, 0, 0, 0))
    x = (HAND_SIZE[0] - arm.width) // 2
    canvas.paste(sleeve, (x, top + cut), sleeve)
    canvas.paste(upper, (x, top), upper)
    a = np.asarray(canvas).copy()
    fade = np.clip((HAND_SIZE[1] - np.arange(HAND_SIZE[1])) / HAND_FADE, 0, 1)[:, None]
    a[..., 3] = (a[..., 3] * fade).astype(np.uint8)
    out.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(a, "RGBA").save(out)


def main() -> None:
    ART.mkdir(parents=True, exist_ok=True)

    full = fit(key_green(Image.open(SRC / "fullbody.png")), (1024, 1536))
    full.save(REF / "fullbody_flat.png")
    full.save(ART / "fullbody.png")

    rest = fit(key_green(Image.open(SRC / "rest_sitting.png")), (1086, 1448))
    rest.save(REF / "rest_site_sitting_flat.png")

    shutil.copy2(SRC / "char_select_bg.png", REF / "char_select_bg_flat.png")
    subprocess.run([sys.executable, str(ROOT / "tools" / "make_char_select_bg.py"),
                    str(REF / "char_select_bg_flat.png"), str(CHARUI / "char_select_bg.png")], check=True)
    shutil.copy2(CHARUI / "char_select_bg.png", ART / "char_select_bg.png")

    bust = Image.open(SRC / "bust.png").convert("RGBA")
    button = cover(bust, (132, 195), top=0.0)
    button.save(CHARUI / "char_select.png")
    ImageEnhance.Brightness(ImageEnhance.Color(button).enhance(0.15)).enhance(0.45).save(CHARUI / "char_select_locked.png")
    w, h = bust.size
    side = round(w * 0.74)
    face = bust.crop(((w - side) // 2, round(h * 0.10), (w - side) // 2 + side, round(h * 0.10) + side))
    icon, outline = token(face)
    icon.save(CHARUI / "character_icon.png")
    outline.save(CHARUI / "character_icon_outline.png")

    marker = key_green(Image.open(SRC / "map_marker.png"))
    marker = marker.crop(marker.getchannel("A").point(lambda v: 255 if v > 40 else 0).getbbox())
    fit(marker, (116, 116), anchor_bottom=False).resize((116, 116)).save(CHARUI / "map_marker.png")
    m = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    m.paste(Image.open(CHARUI / "map_marker.png"), (6, 6))
    m.save(CHARUI / "map_marker.png")

    # One scale for all four (from the pointing arm's height), so the hands match each other.
    point = key_green(Image.open(SRC / "hand_point.png"))
    point_h = point.getchannel("A").point(lambda v: 255 if v > 40 else 0).getbbox()
    scale = HAND_SCALE * HAND_SIZE[1] * 1.04 / (point_h[3] - point_h[1])
    for move in ("point", "rock", "paper", "scissors"):
        hand(SRC / f"hand_{move}.png", CHARUI / "hands" / f"hand_{move}.png", scale)

    for p in [ART / "fullbody.png", REF / "rest_site_sitting_flat.png", *sorted(CHARUI.glob("*.png")),
              *sorted((CHARUI / "hands").glob("*.png"))]:
        print(f"{p.relative_to(ROOT)}: {Image.open(p).size}")


if __name__ == "__main__":
    main()
