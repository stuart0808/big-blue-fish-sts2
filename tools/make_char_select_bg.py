"""Turn a 16:9 splash image into the 2560x1200 character-select background.

The game does not show the 2560x1200 canvas centred. In character_select_screen.tscn the
background sits in a container offset (-388, -80) and scaled 1.1x, and each character's bg
scene is anchored at (-960, -521) from that container's centre. Working that through, a
1920x1080 screen shows roughly x 149-1894, y 48-1030 of the canvas; the rest (mostly a
wide strip on the right) is overscan.

The source is scaled to cover that visible box (with a little bleed); a source that is not
16:9 is cropped, mostly from the bottom so heads stay in frame. The overscan is filled
with a mirrored copy of the edges that fades into a blurred, darkened version further out.

Usage: python make_char_select_bg.py <source image> <output.png>
"""

import sys

import numpy as np
from PIL import Image, ImageEnhance, ImageFilter

OUT_W, OUT_H = 2560, 1200
VISIBLE = (130, 37, 1915, 1042)  # box the source is scaled to cover, slightly larger than visible
TOP_BIAS = 0.05  # when the source is taller than the box, share of the excess cut from the top
SOFTEN_DISTANCE = 120  # px outside the source where the fill becomes fully blurred
DARKEN_DISTANCE = 300  # px outside the source where the fill becomes plain deep water
DEEP_WATER = (8, 14, 36)


def main(src_path: str, out_path: str) -> None:
    src = Image.open(src_path).convert("RGB")
    x0, y0, x1, y1 = VISIBLE
    box_w, box_h = x1 - x0, y1 - y0
    scale = max(box_w / src.width, box_h / src.height)
    scaled = src.resize((round(src.width * scale), round(src.height * scale)), Image.Resampling.LANCZOS)
    cut_x = (scaled.width - box_w) // 2
    cut_y = round((scaled.height - box_h) * TOP_BIAS)
    content = scaled.crop((cut_x, cut_y, cut_x + box_w, cut_y + box_h))

    pad = ((y0, OUT_H - y0 - content.height), (x0, OUT_W - x1), (0, 0))
    sharp = np.pad(np.asarray(content), pad, mode="symmetric")
    sharp_img = Image.fromarray(sharp)
    soft = np.asarray(ImageEnhance.Brightness(sharp_img.filter(ImageFilter.GaussianBlur(24))).enhance(0.6))

    ys, xs = np.mgrid[0:OUT_H, 0:OUT_W]
    dx = np.maximum(np.maximum(x0 - xs, xs - (x1 - 1)), 0)
    dy = np.maximum(np.maximum(y0 - ys, ys - (y0 + content.height - 1)), 0)
    dist = np.maximum(dx, dy)[..., None]
    t = np.clip(dist / SOFTEN_DISTANCE, 0, 1) ** 0.5
    # Far out (the wide right strip), sink into the deep-water colour so no mirrored figure shows.
    t_dark = np.clip(dist / DARKEN_DISTANCE, 0, 1)

    out = sharp * (1 - t) + soft * t
    out = (out * (1 - t_dark) + np.array(DEEP_WATER) * t_dark).astype(np.uint8)
    Image.fromarray(out).save(out_path, optimize=True)
    print(f"{out_path}: {OUT_W}x{OUT_H} (source scaled x{scale:.2f} into x {x0}-{x1}, y {y0}-{y0 + content.height})")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
