"""Generate the whale girl's character art (combat / rest site / character select / hands / map marker)
in Slay the Spire 2's own flat cel-shaded style, with the Codex CLI's image generation.

Old (anime-style) sources are attached as pose references, so the new pictures keep the same
pose and framing and the Spine rigs in make_spine_rig.py only need small adjustments. Results go
to art_generated_v2/character/<key>.png; existing files are skipped.

Usage: python gen_character_art.py [key ...]     (default: every job, hands last)
Then:  python process_character_art.py
"""

import subprocess
import sys
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "art_generated_v2" / "character"
REF = ROOT / "art_reference"

STYLE = (
    "Art style: exactly the style of the game Slay the Spire 2 - the attached reference sheets show the game's "
    "own card art and character-select portraits; match them closely. Flat cel-shaded 2D painting, thick dark "
    "outlines, flat color fills with one hard-edged shadow tone and one highlight, limited palette, chunky "
    "simplified shapes, a simple face with minimal detail (no big glossy anime eyes, no blush gradients), hair as "
    "a few big flat locks instead of many fine strands, no gradients, no sparkles, no glow, no bubbles, no "
    "particles. No text, letters, numbers, frame, border, UI or watermark."
)
CHARACTER = (
    "The character is the whale girl from the attached character sheet: blue wavy hair with a small ahoge, white "
    "frilled maid headband with small blue bows, dark navy whale-fin ears, navy long-sleeved maid dress with a "
    "white apron and a small whale emblem, white frilly socks, navy shoes, a big navy-and-white whale tail."
)
GREEN = "Background: flat pure green #00FF00 everywhere around her, with no shadow or ground on it."

JOBS = {
    "fullbody": dict(aspect="portrait 2:3", refs=["fullbody_original.png", "bust", "character_sheet.png"], prompt=(
        "Redraw the whale girl from the first attached image in exactly the same pose, framing, proportions and "
        "position on the canvas: full body standing, feet near the bottom, long hair flowing out to the left, her "
        "right arm stretched out to the right with an open palm, the whale tail curving out behind her on the "
        "left. Leave out the water swirl around her hand. " + GREEN)),
    "rest_sitting": dict(aspect="portrait 3:4", refs=["rest_site_sitting.png", "bust", "character_sheet.png"], prompt=(
        "Redraw the whale girl from the first attached image in exactly the same pose, framing and position on "
        "the canvas: sitting on a rock with her hands in her lap, relaxed and a little sleepy, long hair down her "
        "back, the whale tail curled on the ground to the left of the rock. Keep the rock, as a simple flat "
        "grey-brown shape. " + GREEN)),
    "char_select_bg": dict(aspect="landscape 3:2", refs=["char_select_bg_original.png", "bust", "character_sheet.png"], prompt=(
        "Redraw the attached splash image in the flat game style, same composition: the whale girl on the right "
        "half reaching an open hand toward the viewer, long hair and the whale tail flowing, underwater. Turn the "
        "underwater background into a few big flat shapes: a deep blue to teal two-tone backdrop, simple dark "
        "rock pillars, one large simple whale silhouette in the upper left, a few flat light shafts from the top. "
        "Keep the left half fairly empty and calm.")),
    "bust": dict(aspect="portrait 2:3", refs=["character_sheet.png"], prompt=(
        "Character-select portrait: a tight head-and-shoulders crop of the whale girl like the attached Slay the "
        "Spire 2 character portraits - her head fills the upper two thirds, cut off at the chest, turned three "
        "quarters to the right with a small smug smile, one whale-fin ear and the maid headband clearly visible. "
        "Flat solid bright cyan-blue background (#3FA9F5).")),
    "map_marker": dict(aspect="square 1:1", refs=[], prompt=(
        "A single small game icon: a navy-and-white whale tail standing upright, simple bold shape, thick dark "
        "outline, flat colors, centered and filling about 80% of the frame. " + GREEN)),
    "hand_point": dict(aspect="portrait 2:3", refs=["game_hand_reference.jpg", "character_sheet.png"], prompt=(
        "One forearm and hand, drawn like the attached Slay the Spire 2 multiplayer hand images: the forearm rises "
        "straight up from the bottom edge, centered, the hand at the top with only the index finger pointing up. "
        "The sleeve is the whale girl's navy maid-dress sleeve with a white frilled cuff at the wrist, pale skin, "
        "and a small navy-and-white whale-tail charm hanging from the cuff. Tall narrow composition, the arm about "
        "a third of the image width. " + GREEN)),
    "hand_rock": dict(aspect="portrait 2:3", refs=["hand_point", "game_hand_reference.jpg"], prompt=(
        "The same forearm, sleeve, cuff and charm as the first attached image, same position and size, but the "
        "hand is a closed fist (rock in rock-paper-scissors). " + GREEN)),
    "hand_paper": dict(aspect="portrait 2:3", refs=["hand_point", "game_hand_reference.jpg"], prompt=(
        "The same forearm, sleeve, cuff and charm as the first attached image, same position and size, but the "
        "hand is open flat with the fingers spread, palm facing the viewer (paper in rock-paper-scissors). " + GREEN)),
    "hand_scissors": dict(aspect="portrait 2:3", refs=["hand_point", "game_hand_reference.jpg"], prompt=(
        "The same forearm, sleeve, cuff and charm as the first attached image, same position and size, but the "
        "index and middle fingers are spread in a V (scissors in rock-paper-scissors). " + GREEN)),
}
STYLE_REFS = ["game_style_reference.jpg", "game_portrait_reference.jpg"]
BUST_MATCH = (" Draw her in exactly the same flat style as the attached bust portrait (bust.png): thick dark "
              "outlines, hair as a few big flat locks with one hard shadow tone and no gradient, a simple face, "
              "flat dress shapes with no fine lines or lace detail.")


def ref_path(name: str) -> Path:
    return OUT / f"{name}.png" if "." not in name else REF / name


def run(key: str) -> str:
    job = JOBS[key]
    target = OUT / f"{key}.png"
    if target.exists():
        return f"skip  {key}"
    refs = [ref_path(r) for r in job["refs"]] + [REF / r for r in STYLE_REFS]
    match = BUST_MATCH if "bust" in job["refs"] else ""
    prompt = f"{job['prompt']}{match} {CHARACTER if key not in ('map_marker',) else ''} {STYLE}"
    request = (f"Generate exactly one image with your image generation tool ({job['aspect']}). The attached images "
               f"are, in order: {', '.join(r.name for r in refs)}. Prompt: {prompt}\n\nAfter generating, save the "
               f"image as a PNG file at {target.relative_to(ROOT)}. Do not edit any other files. Reply with just the "
               f"saved path.")
    cmd = ["codex", "exec", "--skip-git-repo-check", "--sandbox", "workspace-write", "--ephemeral", request,
           "-i", *(str(r.relative_to(ROOT)) for r in refs)]
    for _ in range(3):
        subprocess.run(cmd, cwd=ROOT, capture_output=True, text=True, timeout=900, stdin=subprocess.DEVNULL)
        if target.exists() and target.stat().st_size > 10_000:
            return f"OK    {key}"
    return f"FAIL  {key}"


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    keys = sys.argv[1:] or list(JOBS)
    base = {"bust", "hand_point"}            # the others are drawn from these two
    first = [k for k in keys if k in base or (k == "map_marker")]
    later = [k for k in keys if k not in first]
    for group in (first, later):
        with ThreadPoolExecutor(max_workers=4) as pool:
            for line in pool.map(run, group):
                print(line, flush=True)


if __name__ == "__main__":
    main()
