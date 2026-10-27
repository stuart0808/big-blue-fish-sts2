"""Build Spine 4.2 rigs for the whale girl from single illustrations.

Each picture becomes one weighted mesh (a regular grid) bound to a handful of bones, so the
animations bend the illustration instead of moving cut-out parts (no holes to repaint).
Two rigs are defined below:

  whale_girl       combat and shop (art/charui/fullbody.png, standing)
  whale_girl_rest  rest site      (art_reference/rest_site_sitting_flat.png, sitting by the fire)

Each goes to DeepSeekWhale/spine/<name>/ as <name>.png (the atlas page), <name>.atlas and
<name>.spine-json. All positions are in pixels of the source image (y down); they are
converted to Spine units (y up, root at 0) on output.

Usage: python make_spine_rig.py [rig name ...]   (default: all rigs)
"""

import json
import math
import shutil
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Callable

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
GRID = 48   # mesh cell size in image px
FPS = 15    # keys per second when sampling animation curves


def smooth(a: float, b: float, x: float) -> float:
    """0 at a, 1 at b (a may be greater than b), smoothstep in between."""
    t = min(1.0, max(0.0, (x - a) / (b - a)))
    return t * t * (3 - 2 * t)


def blend(base: dict, other: dict, w: float) -> dict:
    if w <= 0:
        return base
    out = {k: v * (1 - w) for k, v in base.items()}
    for k, v in other.items():
        out[k] = out.get(k, 0) + v * w
    return out


def bands(py: float, stops: list[tuple[str, float, float]]) -> dict[str, float]:
    """Vertical weight bands: [(bone, start_y, end_y)], blending between consecutive bones."""
    for (b0, _, e0), (b1, s1, _) in zip(stops, stops[1:]):
        if py < e0:
            return {b0: 1.0}
        if py < s1:
            t = smooth(e0, s1, py)
            return {b0: 1 - t, b1: t}
    return {stops[-1][0]: 1.0}


# ---------------------------------------------------------------- animation helpers

def times(duration: float) -> list[float]:
    steps = max(2, round(duration * FPS))
    return [duration * k / steps for k in range(steps + 1)]


def rotate(d: float, fn) -> list[dict]:
    return [{"time": round(t, 4), "value": round(fn(t), 3)} for t in times(d)]


def translate(d: float, fx, fy) -> list[dict]:
    return [{"time": round(t, 4), "x": round(fx(t), 2), "y": round(fy(t), 2)} for t in times(d)]


def scale(d: float, fx, fy) -> list[dict]:
    return [{"time": round(t, 4), "x": round(fx(t), 4), "y": round(fy(t), 4)} for t in times(d)]


def wave(period: float, amp: float, phase: float = 0.0, base: float = 0.0):
    return lambda t: base + amp * math.sin(2 * math.pi * (t / period + phase))


def keys(*points):
    """Piecewise smooth curve through (time, value) points (cosine easing between them)."""
    def f(t):
        if t <= points[0][0]:
            return points[0][1]
        for (t0, v0), (t1, v1) in zip(points, points[1:]):
            if t <= t1:
                u = (t - t0) / (t1 - t0) if t1 > t0 else 1
                u = (1 - math.cos(math.pi * u)) / 2
                return v0 + (v1 - v0) * u
        return points[-1][1]
    return f


ZERO = lambda t: 0.0  # noqa: E731
ONE = lambda t: 1.0  # noqa: E731


# ---------------------------------------------------------------- rig definition

@dataclass
class Rig:
    name: str
    src: Path
    origin: tuple[int, int]           # root bone, image px
    scale: float                      # image px -> Spine units
    bones: dict[str, tuple]           # name -> (parent, image x, image y)
    weights: Callable[[float, float], dict[str, float]]
    animations: dict

    def to_spine(self, ix: float, iy: float) -> tuple[float, float]:
        return (ix - self.origin[0]) * self.scale, (self.origin[1] - iy) * self.scale

    def bone_world(self, name: str) -> tuple[float, float]:
        return self.to_spine(self.bones[name][1], self.bones[name][2])


# ---- combat / shop: standing pose (art/charui/fullbody.png, 1024x1536; flat-style redraw)

def combat_weights(px: float, py: float) -> dict[str, float]:
    w = bands(py, [("head", 0, 400), ("chest", 480, 650), ("hips", 760, 1000), ("root", 1150, 9999)])
    # Long hair flowing to the left of the head swings around the hair bone.
    if py < 720 and px < 560:
        d = math.hypot(px - 615, py - 315)
        w = blend(w, {"hair": 1.0}, smooth(140, 260, d) * smooth(720, 640, py))
    # Outstretched right arm and the water swirl in front of it.
    if 260 < py < 720:
        w = blend(w, {"arm": 1.0}, smooth(690, 770, px) * smooth(260, 320, py) * smooth(720, 660, py))
    # Tail: left of the skirt, between the hair tips (y ~760) and the knee (y ~1185); the leg and
    # shoe below it must stay on the root.
    if px < 530 and 745 < py < 1195:
        tail = (bands(-px, [("tail1", -9999, -420), ("tail2", -260, -260), ("tail3", -150, 0)]))
        fade = smooth(745, 775, py) * smooth(1195, 1170, py)
        w = blend(w, tail, smooth(530, 470, px) * fade)
    return w


def idle(period: float, sway: float, droop: float = 0.0) -> dict:
    p = period
    return {"bones": {
        "hips": {"translate": translate(p, ZERO, wave(p, 3 * sway, 0.25, -3 * sway))},
        "chest": {"scale": scale(p, ONE, wave(p, 0.012 * sway, 0.25, 1.0)),
                  "rotate": rotate(p, wave(p, 0.8 * sway, 0.1, -droop * 0.3))},
        "head": {"rotate": rotate(p, wave(p, 1.6 * sway, 0.15, -droop))},
        "hair": {"rotate": rotate(p, wave(p, 2.5 * sway, 0.35))},
        "arm": {"rotate": rotate(p, wave(p, 2.0 * sway, 0.2, -droop * 0.8)),
                "translate": translate(p, ZERO, wave(p, 4 * sway, 0.3))},
        "tail1": {"rotate": rotate(p, wave(p, 4 * sway, 0.0))},
        "tail2": {"rotate": rotate(p, wave(p, 6 * sway, 0.12))},
        "tail3": {"rotate": rotate(p, wave(p, 9 * sway, 0.24))},
    }}


def attack() -> dict:
    d = 0.6
    return {"bones": {
        "root": {"translate": translate(d, keys((0, 0), (0.12, -25), (0.26, 110), (0.6, 0)), ZERO)},
        "chest": {"rotate": rotate(d, keys((0, 0), (0.12, 6), (0.26, -10), (0.6, 0)))},
        "head": {"rotate": rotate(d, keys((0, 0), (0.12, 3), (0.26, -6), (0.6, 0)))},
        "hair": {"rotate": rotate(d, keys((0, 0), (0.2, 4), (0.36, 8), (0.6, 0)))},
        "arm": {"rotate": rotate(d, keys((0, 0), (0.12, 10), (0.26, -18), (0.6, 0)))},
        "tail1": {"rotate": rotate(d, keys((0, 0), (0.12, 18), (0.3, -22), (0.6, 0)))},
        "tail2": {"rotate": rotate(d, keys((0, 0), (0.15, 22), (0.33, -28), (0.6, 0)))},
        "tail3": {"rotate": rotate(d, keys((0, 0), (0.18, 26), (0.36, -32), (0.6, 0)))},
    }}


def cast() -> dict:
    d = 0.7
    return {"bones": {
        "hips": {"translate": translate(d, ZERO, keys((0, 0), (0.25, 10), (0.7, 0)))},
        "chest": {"rotate": rotate(d, keys((0, 0), (0.25, 5), (0.7, 0)))},
        "head": {"rotate": rotate(d, keys((0, 0), (0.25, 4), (0.7, 0)))},
        "arm": {"rotate": rotate(d, keys((0, 0), (0.25, 22), (0.45, 18), (0.7, 0)))},
        "hair": {"rotate": rotate(d, keys((0, 0), (0.3, -5), (0.7, 0)))},
        "tail3": {"rotate": rotate(d, keys((0, 0), (0.3, 10), (0.7, 0)))},
    }}


def hurt() -> dict:
    d = 0.45
    return {
        "bones": {
            "root": {"translate": translate(d, keys((0, 0), (0.07, -40), (0.45, 0)), ZERO)},
            "chest": {"rotate": rotate(d, keys((0, 0), (0.07, 9), (0.45, 0)))},
            "head": {"rotate": rotate(d, keys((0, 0), (0.07, 8), (0.45, 0)))},
            "hair": {"rotate": rotate(d, keys((0, 0), (0.12, -6), (0.45, 0)))},
            "tail2": {"rotate": rotate(d, keys((0, 0), (0.1, -12), (0.45, 0)))},
            "tail3": {"rotate": rotate(d, keys((0, 0), (0.12, -16), (0.45, 0)))},
        },
        "slots": {"body": {"rgba": [
            {"time": 0, "color": "ffffffff"}, {"time": 0.05, "color": "ff9a9aff"}, {"time": 0.3, "color": "ffffffff"}]}},
    }


def die() -> dict:
    d = 1.3
    return {
        "bones": {
            "root": {"rotate": rotate(d, keys((0, 0), (0.15, 6), (0.9, -14))),
                     "translate": translate(d, keys((0, 0), (0.15, -20), (0.9, -35)), keys((0, 0), (0.9, -25)))},
            "head": {"rotate": rotate(d, keys((0, 0), (0.9, -12)))},
            "hair": {"rotate": rotate(d, keys((0, 0), (0.9, 6)))},
            "arm": {"rotate": rotate(d, keys((0, 0), (0.9, -25)))},
            "tail1": {"rotate": rotate(d, keys((0, 0), (0.9, -10)))},
            "tail3": {"rotate": rotate(d, keys((0, 0), (0.9, -20)))},
        },
        "slots": {"body": {"rgba": [
            {"time": 0, "color": "ffffffff"}, {"time": 0.5, "color": "ffffffff"}, {"time": 1.3, "color": "ffffff00"}]}},
    }


COMBAT = Rig(
    name="whale_girl",
    src=ROOT / "art" / "charui" / "fullbody.png",
    origin=(640, 1480),     # between the shoes
    scale=0.58,             # ~825 units tall, like the base-game rigs
    bones={
        "root": (None, 640, 1480),
        "hips": ("root", 640, 760),
        "chest": ("hips", 640, 560),
        "head": ("chest", 625, 430),
        "hair": ("head", 470, 200),
        "arm": ("chest", 705, 525),
        "tail1": ("hips", 470, 1050),
        "tail2": ("tail1", 300, 1090),
        "tail3": ("tail2", 150, 960),
    },
    weights=combat_weights,
    animations={
        "idle_loop": idle(2.4, 1.0),
        "relaxed_loop": idle(3.2, 0.7),
        "low_health_loop": idle(3.0, 0.6, droop=5.0),
        "attack": attack(),
        "cast": cast(),
        "hurt": hurt(),
        "die": die(),
    },
)


# ---- rest site: sitting on a rock by the fire (art_reference/rest_site_sitting_flat.png, 1086x1448)

def rest_weights(px: float, py: float) -> dict[str, float]:
    w = bands(py, [("head", 0, 620), ("chest", 720, 950), ("body", 1050, 1150), ("root", 1230, 9999)])
    # Long hair left of the face sways from the top of the head.
    if px < 680 and py < 1160:
        d = math.hypot(px - 700, py - 480)
        w = blend(w, {"hair": 1.0}, smooth(170, 300, d) * smooth(1160, 1060, py))
    # Tail curled on the ground to the left of the rock.
    if px < 540 and (py > 1150 or (px < 280 and py > 1040)):
        tail = bands(-px, [("tail1", -9999, -440), ("tail2", -320, -300), ("tail3", -190, 0)])
        w = blend(w, tail, smooth(560, 470, px))
    return w


def campfire(period: float) -> dict:
    p = period
    return {"bones": {
        "chest": {"scale": scale(p, ONE, wave(p, 0.014, 0.25, 1.0))},
        "head": {"rotate": rotate(p, keys((0, 0), (p * 0.45, -2.5), (p * 0.6, -2.0), (p, 0)))},
        "hair": {"rotate": rotate(p, wave(p, 1.8, 0.3))},
        "tail1": {"rotate": rotate(p, wave(p, 2.0, 0.0))},
        "tail2": {"rotate": rotate(p, wave(p, 3.5, 0.15))},
        "tail3": {"rotate": rotate(p, wave(p, 6.0, 0.3))},
    }}


REST = Rig(
    name="whale_girl_rest",
    src=ROOT / "art_reference" / "rest_site_sitting_flat.png",
    origin=(800, 1440),     # bottom of the rock, under her shoes
    scale=0.46,             # ~650 units tall, fits the base-game rest-site hitbox
    bones={
        "root": (None, 800, 1440),
        "body": ("root", 790, 1080),
        "chest": ("body", 760, 860),
        "head": ("chest", 720, 680),
        "hair": ("head", 600, 300),
        "tail1": ("body", 440, 1170),
        "tail2": ("tail1", 300, 1250),
        "tail3": ("tail2", 170, 1300),
    },
    weights=rest_weights,
    animations={name: campfire(3.6) for name in
                ("overgrowth_loop", "hive_loop", "glory_loop", "idle_loop", "relaxed_loop")},
)

RIGS = {r.name: r for r in (COMBAT, REST)}


# ---------------------------------------------------------------- mesh and output

def normalised(w: dict[str, float]) -> dict[str, float]:
    items = sorted(((k, v) for k, v in w.items() if v > 0.01), key=lambda kv: -kv[1])[:4]
    total = sum(v for _, v in items)
    return {k: v / total for k, v in items}


def build_mesh(rig: Rig, img_w: int, img_h: int) -> dict:
    cols, rows = math.ceil(img_w / GRID) + 1, math.ceil(img_h / GRID) + 1
    xs = [min(i * GRID, img_w) for i in range(cols)]
    ys = [min(j * GRID, img_h) for j in range(rows)]
    names = list(rig.bones)

    # Spine wants the hull (outline) vertices first, in order around the edge.
    perimeter = ([(i, 0) for i in range(cols)] + [(cols - 1, j) for j in range(1, rows)]
                 + [(i, rows - 1) for i in range(cols - 2, -1, -1)] + [(0, j) for j in range(rows - 2, 0, -1)])
    interior = [(i, j) for j in range(1, rows - 1) for i in range(1, cols - 1)]
    order = perimeter + interior
    index = {cell: n for n, cell in enumerate(order)}

    uvs, vertices = [], []
    for i, j in order:
        px, py = xs[i], ys[j]
        uvs += [round(px / img_w, 5), round(py / img_h, 5)]
        wx, wy = rig.to_spine(px, py)
        w = normalised(rig.weights(px, py))
        vertices.append(len(w))
        for bone, weight in w.items():
            bx, by = rig.bone_world(bone)
            vertices += [names.index(bone), round(wx - bx, 2), round(wy - by, 2), round(weight, 4)]

    triangles = []
    for j in range(rows - 1):
        for i in range(cols - 1):
            a, b = index[(i, j)], index[(i + 1, j)]
            c, d = index[(i, j + 1)], index[(i + 1, j + 1)]
            triangles += [a, c, b, b, c, d]
    return {"type": "mesh", "uvs": uvs, "triangles": triangles, "vertices": vertices,
            "hull": len(perimeter), "width": img_w, "height": img_h}


def build(rig: Rig) -> None:
    out = ROOT / "DeepSeekWhale" / "spine" / rig.name
    out.mkdir(parents=True, exist_ok=True)
    img_w, img_h = Image.open(rig.src).size
    shutil.copy2(rig.src, out / f"{rig.name}.png")
    (out / f"{rig.name}.atlas").write_text(
        f"{rig.name}.png\nsize:{img_w},{img_h}\nfilter:Linear,Linear\nbody\nbounds:0,0,{img_w},{img_h}\n",
        encoding="utf-8")

    bones = []
    for name, (parent, _, _) in rig.bones.items():
        bone = {"name": name}
        if parent:
            px, py = rig.bone_world(parent)
            wx, wy = rig.bone_world(name)
            bone.update({"parent": parent, "x": round(wx - px, 2), "y": round(wy - py, 2)})
        bones.append(bone)
    mesh = build_mesh(rig, img_w, img_h)
    left, top = rig.to_spine(0, 0)
    right, bottom = rig.to_spine(img_w, img_h)
    skeleton = {
        "skeleton": {"hash": rig.name, "spine": "4.2.43", "x": round(left, 2), "y": round(bottom, 2),
                     "width": round(right - left, 2), "height": round(top - bottom, 2), "images": "./", "audio": ""},
        "bones": bones,
        "slots": [{"name": "body", "bone": "root", "attachment": "body"}],
        "skins": [{"name": "default", "attachments": {"body": {"body": mesh}}}],
        "animations": rig.animations,
    }
    (out / f"{rig.name}.spine-json").write_text(json.dumps(skeleton, separators=(",", ":")), encoding="utf-8")
    print(f"{rig.name}: {len(mesh['uvs']) // 2} vertices, {len(mesh['triangles']) // 3} triangles, "
          f"{len(bones)} bones, animations: {', '.join(rig.animations)}")


if __name__ == "__main__":
    for name in sys.argv[1:] or list(RIGS):
        build(RIGS[name])
