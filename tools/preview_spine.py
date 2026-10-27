"""Render frames of the generated whale-girl Spine rig without Spine or the game.

Implements just enough of Spine's pose math (bone rotate/translate/scale timelines, weighted
mesh skinning, slot colour) to check that the animations look right.

Usage: python preview_spine.py <out.jpg> [--rig whale_girl_rest] [anim:frames ...]   e.g. idle_loop:4 attack:6
"""

import json
import math
import sys
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent
SPINE = ROOT / "DeepSeekWhale" / "spine"
CELL = 300           # preview cell size in px
VIEW_SCALE = 0.32    # Spine units -> preview px


def lerp_keys(keys: list[dict], t: float, field: str, default: float) -> float:
    if not keys:
        return default
    if t <= keys[0].get("time", 0):
        return keys[0].get(field, default)
    for a, b in zip(keys, keys[1:]):
        ta, tb = a.get("time", 0), b.get("time", 0)
        if t <= tb:
            u = (t - ta) / (tb - ta) if tb > ta else 1
            return a.get(field, default) + (b.get(field, default) - a.get(field, default)) * u
    return keys[-1].get(field, default)


def mat(x, y, rot, sx, sy):
    c, s = math.cos(math.radians(rot)), math.sin(math.radians(rot))
    return (c * sx, -s * sy, x, s * sx, c * sy, y)


def mul(p, q):
    a, b, c, d, e, f = p
    g, h, i, j, k, l_ = q
    return (a * g + b * j, a * h + b * k, a * i + b * l_ + c, d * g + e * j, d * h + e * k, d * i + e * l_ + f)


def pose(skel: dict, anim: dict, t: float) -> dict[str, tuple]:
    world = {}
    for bone in skel["bones"]:
        tl = anim.get("bones", {}).get(bone["name"], {})
        rot = bone.get("rotation", 0) + lerp_keys(tl.get("rotate", []), t, "value", 0)
        tx = bone.get("x", 0) + lerp_keys(tl.get("translate", []), t, "x", 0)
        ty = bone.get("y", 0) + lerp_keys(tl.get("translate", []), t, "y", 0)
        sx = lerp_keys(tl.get("scale", []), t, "x", 1)
        sy = lerp_keys(tl.get("scale", []), t, "y", 1)
        local = mat(tx, ty, rot, sx, sy)
        parent = bone.get("parent")
        world[bone["name"]] = mul(world[parent], local) if parent else local
    return world


def skin_vertices(skel: dict, mesh: dict, world: dict) -> list[tuple[float, float]]:
    names = [b["name"] for b in skel["bones"]]
    v, out, i = mesh["vertices"], [], 0
    while i < len(v):
        n = v[i]
        i += 1
        x = y = 0.0
        for _ in range(n):
            bone, lx, ly, w = names[v[i]], v[i + 1], v[i + 2], v[i + 3]
            a, b, c, d, e, f = world[bone]
            x += (a * lx + b * ly + c) * w
            y += (d * lx + e * ly + f) * w
            i += 4
        out.append((x, y))
    return out


def alpha_at(anim: dict, t: float) -> float:
    keys = anim.get("slots", {}).get("body", {}).get("rgba", [])
    if not keys:
        return 1.0
    vals = [{"time": k.get("time", 0), "a": int(k["color"][6:8], 16) / 255} for k in keys]
    return lerp_keys(vals, t, "a", 1.0)


def render(skel: dict, page: Image.Image, anim_name: str, t: float) -> Image.Image:
    anim = skel["animations"][anim_name]
    mesh = skel["skins"][0]["attachments"]["body"]["body"]
    pts = skin_vertices(skel, mesh, pose(skel, anim, t))
    W, H = page.size
    uvs = [(mesh["uvs"][k] * W, mesh["uvs"][k + 1] * H) for k in range(0, len(mesh["uvs"]), 2)]
    to_view = lambda p: (CELL * 0.55 + p[0] * VIEW_SCALE, CELL * 0.95 - p[1] * VIEW_SCALE)  # noqa: E731
    dst = [to_view(p) for p in pts]

    canvas = Image.new("RGBA", (CELL, CELL), (24, 32, 60, 255))
    tri = mesh["triangles"]
    for k in range(0, len(tri), 3):
        ids = tri[k:k + 3]
        D = [dst[i] for i in ids]
        S = [uvs[i] for i in ids]
        (x0, y0), (x1, y1), (x2, y2) = D
        det = (x1 - x0) * (y2 - y0) - (x2 - x0) * (y1 - y0)
        if abs(det) < 1e-6:
            continue
        # affine map from canvas (x, y) to page (u, v)
        coef = []
        for comp in (0, 1):
            s0, s1, s2 = S[0][comp], S[1][comp], S[2][comp]
            a = ((s1 - s0) * (y2 - y0) - (s2 - s0) * (y1 - y0)) / det
            b = ((s2 - s0) * (x1 - x0) - (s1 - s0) * (x2 - x0)) / det
            coef += [a, b, s0 - a * x0 - b * y0]
        bx0, by0 = max(0, int(min(p[0] for p in D)) - 1), max(0, int(min(p[1] for p in D)) - 1)
        bx1, by1 = min(CELL, int(max(p[0] for p in D)) + 2), min(CELL, int(max(p[1] for p in D)) + 2)
        if bx1 <= bx0 or by1 <= by0:
            continue
        a, b, c, d, e, f = coef
        patch = page.transform((bx1 - bx0, by1 - by0), Image.Transform.AFFINE,
                               (a, b, c + a * bx0 + b * by0, d, e, f + d * bx0 + e * by0), Image.Resampling.BILINEAR)
        mask = Image.new("L", patch.size, 0)
        ImageDraw.Draw(mask).polygon([(p[0] - bx0, p[1] - by0) for p in D], fill=255, outline=255)
        alpha = Image.composite(patch.getchannel("A"), Image.new("L", patch.size, 0), mask)
        patch.putalpha(alpha)
        canvas.alpha_composite(patch, (bx0, by0))
    fade = alpha_at(anim, t)
    if fade < 1:
        canvas = Image.blend(Image.new("RGBA", canvas.size, (24, 32, 60, 255)), canvas, fade)
    ImageDraw.Draw(canvas).text((6, 4), f"{anim_name} t={t:.2f}", fill=(255, 255, 120, 255))
    return canvas


def duration(anim: dict) -> float:
    end = 0.0
    for tl in anim.get("bones", {}).values():
        for keys in tl.values():
            end = max(end, max(k.get("time", 0) for k in keys))
    return end


def main(out: str, specs: list[str]) -> None:
    rig = "whale_girl"
    if specs[:1] == ["--rig"]:
        rig, specs = specs[1], specs[2:]
    skel = json.loads((SPINE / rig / f"{rig}.spine-json").read_text(encoding="utf-8"))
    page = Image.open(SPINE / rig / f"{rig}.png").convert("RGBA")
    page = page.resize((page.width // 2, page.height // 2), Image.Resampling.LANCZOS)
    specs = specs or [f"{a}:4" for a in skel["animations"]]
    rows = []
    for spec in specs:
        name, n = spec.split(":")
        d = duration(skel["animations"][name])
        rows.append([render(skel, page, name, d * k / (int(n) - 1 if int(n) > 1 else 1)) for k in range(int(n))])
    cols = max(len(r) for r in rows)
    sheet = Image.new("RGB", (cols * CELL, len(rows) * CELL), (10, 12, 20))
    for j, row in enumerate(rows):
        for i, im in enumerate(row):
            sheet.paste(im.convert("RGB"), (i * CELL, j * CELL))
    sheet.save(out, quality=88)
    print(out)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2:])
