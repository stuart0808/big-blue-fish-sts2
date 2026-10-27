"""Draw the whale girl's energy orb in the base game's flat style.

The combat energy counter stacks five 128x128 layers (BaseLib's CustomEnergyCounter):
  1 flat disc, 2 and 3 rotating swirls, 4 thick ring, 5 thin outline (the same structure as the
  base-game orbs), with a scalloped water-wave rim instead of a vanilla polygon. The flattened orb also becomes the small energy icons
  used in card text and on card costs.

Outputs into DeepSeekWhale/DeepSeekWhale/images/charui/:
  energy/layer_1.png ... layer_5.png   (256x256 each, shown at 128)
  big_energy.png (74x74), text_energy.png (24x24)

Usage: python make_energy_orb.py [preview.png]
"""

import math
import sys
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "DeepSeekWhale" / "DeepSeekWhale" / "images" / "charui"

S = 1024          # drawing size (supersampled 4x)
C = S / 2
LAYER = 256

OUTLINE = (20, 32, 79)        # dark outline / ring, like the base-game orbs' near-black edges
NAVY = (27, 42, 107)
BASE = (61, 107, 240)         # flat fill (the character colour 4D6BFE, a touch darker)
SHADE = (44, 80, 196)         # one hard-edged shadow tone
SWIRL = (74, 124, 246)       # rotating swirls stay close to the fill, as in the base game
FOAM = (96, 150, 252)
BEVEL = (92, 136, 240)
RING = (52, 86, 200)
SCALLOPS = 8                  # wavy rim: a little bowl of water rather than a vanilla polygon


def canvas() -> Image.Image:
    return Image.new("RGBA", (S, S), (0, 0, 0, 0))


def rim_shape(r: float, depth: float = 0.045, turn: float = 0.0) -> list[tuple[float, float]]:
    """A circle with a soft scalloped (wave) edge."""
    pts = []
    for k in range(720):
        a = 2 * math.pi * k / 720
        rr = r * (1 + depth * math.cos(SCALLOPS * (a + turn)))
        pts.append((C + rr * math.cos(a), C + rr * math.sin(a)))
    return pts


def blob(r: float, arms: int, twist: float, sharp: float) -> list[tuple[float, float]]:
    """A swirling splat like the base-game orbs' rotating layers: short curved arms around a core."""
    pts = []
    for k in range(1440):
        a = 2 * math.pi * k / 1440
        bump = ((math.sin(arms * a) + 1) / 2) ** sharp
        rr = r * (0.70 + 0.30 * bump)
        a2 = a + twist * (rr / r - 0.7)
        pts.append((C + rr * math.cos(a2), C + rr * math.sin(a2)))
    return pts


def back_disc() -> Image.Image:
    """Layer 1: flat fill with one hard shadow crescent, inside a dark outline."""
    im = canvas()
    d = ImageDraw.Draw(im)
    d.polygon(rim_shape(430), fill=OUTLINE + (255,))
    d.polygon(rim_shape(410), fill=BASE + (255,))
    shadow = canvas()
    sd = ImageDraw.Draw(shadow)
    sd.polygon(rim_shape(410), fill=SHADE + (255,))
    cut = Image.new("L", (S, S), 0)
    ImageDraw.Draw(cut).ellipse((C - 470, C - 470, C + 330, C + 330), fill=255)   # light from the top left
    shadow.putalpha(Image.composite(Image.new("L", (S, S), 0), shadow.getchannel("A"), cut))
    im.alpha_composite(shadow)
    return im


def swirl() -> Image.Image:
    """Layer 2 (rotates): a big flat water swirl."""
    im = canvas()
    ImageDraw.Draw(im).polygon(blob(350, 6, 0.9, 1.6), fill=SWIRL + (255,))
    return im


def foam() -> Image.Image:
    """Layer 3 (rotates the other way): a smaller pale swirl."""
    im = canvas()
    ImageDraw.Draw(im).polygon(blob(220, 5, -1.1, 1.4), fill=FOAM + (255,))
    return im


def emblem() -> Image.Image:
    """Layer 4: thick ring with a lighter bevel on the top-left, cut open in the middle."""
    im = canvas()
    d = ImageDraw.Draw(im)
    d.polygon(rim_shape(470), fill=OUTLINE + (255,))
    d.polygon(rim_shape(452), fill=RING + (255,))
    bevel = canvas()
    ImageDraw.Draw(bevel).polygon(rim_shape(452), fill=BEVEL + (255,))
    keep = Image.new("L", (S, S), 0)
    ImageDraw.Draw(keep).pieslice((0, 0, S, S), 160, 290, fill=255)            # top-left arc only
    bevel.putalpha(Image.composite(bevel.getchannel("A"), Image.new("L", (S, S), 0), keep))
    inner = Image.new("L", (S, S), 0)
    ImageDraw.Draw(inner).polygon(rim_shape(400), fill=255)
    bevel.putalpha(Image.composite(Image.new("L", (S, S), 0), bevel.getchannel("A"), inner))
    im.alpha_composite(bevel)
    hole = Image.new("L", (S, S), 255)
    ImageDraw.Draw(hole).polygon(rim_shape(368), fill=0)
    d2 = ImageDraw.Draw(im)
    d2.polygon(rim_shape(382), fill=OUTLINE + (255,))
    im.putalpha(Image.composite(im.getchannel("A"), Image.new("L", (S, S), 0), hole))
    return im


def rim() -> Image.Image:
    """Layer 5: a thin faint outer line and one hard highlight dash, like the base-game top layer."""
    im = canvas()
    d = ImageDraw.Draw(im)
    d.line(rim_shape(488) + [rim_shape(488)[0]], fill=OUTLINE + (120,), width=8)
    for a0, a1 in ((212, 228), (236, 241)):               # two short hard highlight dashes
        d.arc((C - 436, C - 436, C + 436, C + 436), a0, a1, fill=(205, 228, 255, 255), width=14)
    return im


def icon() -> Image.Image:
    """Small energy icon (card costs, card text): just the scalloped disc, outline and one highlight."""
    im = canvas()
    d = ImageDraw.Draw(im)
    d.polygon(rim_shape(470, depth=0.06), fill=OUTLINE + (255,))
    d.polygon(rim_shape(400, depth=0.06), fill=BASE + (255,))
    d.ellipse((C - 230, C - 260, C - 70, C - 120), fill=(190, 225, 255, 255))
    return im


def shrink(im: Image.Image, size: int) -> Image.Image:
    return im.resize((size, size), Image.Resampling.LANCZOS)


def main(preview: str | None) -> None:
    layers = [back_disc(), swirl(), foam(), emblem(), rim()]
    (OUT / "energy").mkdir(parents=True, exist_ok=True)
    for n, im in enumerate(layers, 1):
        shrink(im, LAYER).save(OUT / "energy" / f"layer_{n}.png")

    flat = icon()
    flat = flat.crop(flat.getbbox())
    side = max(flat.size)
    square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    square.paste(flat, ((side - flat.width) // 2, (side - flat.height) // 2))
    shrink(square, 74).save(OUT / "big_energy.png")
    shrink(square, 24).save(OUT / "text_energy.png")
    print("wrote energy/layer_1..5.png, big_energy.png, text_energy.png")

    if preview:
        sheet = Image.new("RGBA", (6 * 140, 240), (40, 44, 60, 255))
        for n, im in enumerate(layers):
            sheet.alpha_composite(shrink(im, 128), (n * 140 + 6, 6))
        sheet.alpha_composite(shrink(square, 128), (5 * 140 + 6, 6))
        sheet.alpha_composite(shrink(square, 74), (6, 150))
        sheet.alpha_composite(shrink(square, 24), (100, 175))
        sheet.convert("RGB").save(preview)


if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else None)
