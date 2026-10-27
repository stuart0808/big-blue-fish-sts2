"""Import generated card / relic / potion art into the mod.

Reads the name table from card.md (headings like "### 打击 · `strike_whale`",
"### 遗物 · 鲸须 · `baleen`", "### 药水 · 深海药水 · `deep_sea_potion`"), then for every image in
<input dir> whose file name is a Chinese card name or an English key:

  card    -> card_portraits/big/<key>.png (1000x760) and card_portraits/<key>.png (250x190),
             center-cropped to that 4:3-ish shape
  relic   -> relics/<key>.png, relics/<key>_outline.png (128) and relics/big/<key>.png (256)
  potion  -> potions/<key>.png and potions/outline/<key>.png (256)
  power   -> powers/<key>.png (128) and powers/big/<key>.png (256)

Icons without transparency get their flat green (#00FF00) background keyed out.
Originals are copied to art_reference/cards/ for safekeeping.

Usage: python import_art.py <input dir>
"""

import re
import shutil
import sys
from pathlib import Path

from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
IMAGES = ROOT / "DeepSeekWhale" / "DeepSeekWhale" / "images"
ARCHIVE = ROOT / "art_reference" / "cards"
EXTS = {".png", ".jpg", ".jpeg", ".webp"}

CARD_BIG, CARD_SMALL = (1000, 760), (250, 190)
ICON_MARGIN = 0.07


def name_table() -> dict[str, tuple[str, str]]:
    """Map Chinese name and English key -> (kind, key)."""
    table = {}
    for line in (ROOT / "card.md").read_text(encoding="utf-8").splitlines():
        m = re.match(r"^### (?:(遗物|药水|能力) · )?(.+?) · `([a-z0-9_]+)`", line)
        if not m:
            continue
        kind = {"遗物": "relic", "药水": "potion", "能力": "power"}.get(m.group(1), "card")
        zh, key = m.group(2).strip(), m.group(3)
        table[zh.lower()] = table[key] = (kind, key)
    return table


def cover(im: Image.Image, size: tuple[int, int]) -> Image.Image:
    w, h = size
    scale = max(w / im.width, h / im.height)
    im = im.resize((round(im.width * scale), round(im.height * scale)), Image.Resampling.LANCZOS)
    left, top = (im.width - w) // 2, (im.height - h) // 2
    return im.crop((left, top, left + w, top + h))


def key_out_green(im: Image.Image) -> Image.Image:
    im = im.convert("RGBA")
    if im.getchannel("A").getextrema()[0] < 250:
        return im  # already has transparency
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, _ = px[x, y]
            if g > 150 and g > r * 1.6 and g > b * 1.6:
                px[x, y] = (r, g, b, 0)
            elif g > max(r, b):
                px[x, y] = (r, max(r, b), b, 255)  # remove green spill on edges
    return im


def square_icon(im: Image.Image) -> Image.Image:
    im = im.crop(im.getbbox())
    side = round(max(im.size) / (1 - 2 * ICON_MARGIN))
    canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    canvas.paste(im, ((side - im.width) // 2, (side - im.height) // 2))
    return canvas


def silhouette(im: Image.Image) -> Image.Image:
    alpha = im.getchannel("A").point(lambda a: 255 if a > 32 else 0)
    grow = max(3, round(im.width * 0.02)) | 1
    alpha = alpha.filter(ImageFilter.MaxFilter(grow)).filter(ImageFilter.GaussianBlur(1))
    white = Image.new("RGBA", im.size, (255, 255, 255, 255))
    white.putalpha(alpha)
    return white


def save(im: Image.Image, path: Path, size: tuple[int, int]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    im.resize(size, Image.Resampling.LANCZOS).save(path)


def import_one(src: Path, kind: str, key: str) -> None:
    im = Image.open(src)
    if kind == "card":
        art = cover(im.convert("RGB"), CARD_BIG)
        save(art, IMAGES / "card_portraits" / "big" / f"{key}.png", CARD_BIG)
        save(art, IMAGES / "card_portraits" / f"{key}.png", CARD_SMALL)
    elif kind == "relic":
        icon = square_icon(key_out_green(im))
        save(icon, IMAGES / "relics" / f"{key}.png", (128, 128))
        save(silhouette(icon), IMAGES / "relics" / f"{key}_outline.png", (128, 128))
        save(icon, IMAGES / "relics" / "big" / f"{key}.png", (256, 256))
    elif kind == "power":
        icon = square_icon(key_out_green(im))
        save(icon, IMAGES / "powers" / f"{key}.png", (128, 128))
        save(icon, IMAGES / "powers" / "big" / f"{key}.png", (256, 256))
    else:
        icon = square_icon(key_out_green(im))
        save(icon, IMAGES / "potions" / f"{key}.png", (256, 256))
        save(silhouette(icon), IMAGES / "potions" / "outline" / f"{key}.png", (256, 256))
    ARCHIVE.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, ARCHIVE / f"{key}{src.suffix.lower()}")


def main(input_dir: str) -> None:
    table = name_table()
    done, skipped = set(), []
    for src in sorted(Path(input_dir).expanduser().iterdir()):
        if src.suffix.lower() not in EXTS:
            continue
        hit = table.get(src.stem.strip().lower())
        if not hit:
            skipped.append(src.name)
            continue
        import_one(src, *hit)
        done.add(hit)
        print(f"  {hit[0]:6} {hit[1]:24} <- {src.name}")

    print(f"\nImported {len(done)} image(s).")
    if skipped:
        print("Not matched to any name in card.md:", ", ".join(skipped))
    have = {p.stem for p in (IMAGES / "card_portraits" / "big").glob("*.png")}
    missing = sorted({k for kind, k in table.values() if kind == "card"} - have)
    print(f"Cards still on placeholder art: {len(missing)}")


if __name__ == "__main__":
    main(sys.argv[1])
