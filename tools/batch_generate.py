"""Batch-generate card / relic / potion art with the Codex CLI's image generation.

Reads every entry in card.md (heading "### [遗物 · |药水 · |能力 · ]<中文名> · `key` [👤]" followed by a
```text prompt block), prepends the shared prompt, attaches the game style reference sheet to every
card (and the character sheet for 👤 entries), and asks `codex exec` to save the result as
art_generated_v2/<key>.png.

Already-generated keys are skipped, so the script can be stopped and re-run at any time.
It stops scheduling new jobs as soon as Codex reports a usage or rate limit.

Usage: python batch_generate.py [--workers 3] [--only key1,key2] [--limit N]
Run tools/import_art.py art_generated_v2 afterwards to put the images into the mod.
"""

import argparse
import re
import subprocess
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "art_generated_v2"
SHEET = ROOT / "art_reference" / "character_sheet.png"
STYLE = ROOT / "art_reference" / "game_style_reference.jpg"
LOG = OUT / "_batch_log.txt"
JOB_TIMEOUT = 600
RETRIES = 2
LIMIT_WORDS = ("usage limit", "rate limit", "quota", "too many requests", "429")

stop = threading.Event()
log_lock = threading.Lock()


def code_block_after(lines: list[str], i: int) -> tuple[str, int]:
    """Return the first ```text block at or after line i, and the line after it."""
    while i < len(lines) and not lines[i].startswith("```"):
        i += 1
    j = i + 1
    while j < len(lines) and not lines[j].startswith("```"):
        j += 1
    return "\n".join(lines[i + 1:j]).strip(), j + 1


def parse_card_md() -> tuple[dict, list[dict]]:
    lines = (ROOT / "card.md").read_text(encoding="utf-8").splitlines()
    shared, entries, i = {}, [], 0
    while i < len(lines):
        line = lines[i]
        if line.startswith("## 通用提示词"):
            shared["card"], i = code_block_after(lines, i)
            shared["character"], i = code_block_after(lines, i)
            continue
        if line.startswith("### 图标通用提示词"):
            shared["icon"], i = code_block_after(lines, i)
            continue
        if line.startswith("### 能力图标通用提示词"):
            shared["power_icon"], i = code_block_after(lines, i)
            continue
        m = re.match(r"^### (?:(遗物|药水|能力) · )?(.+?) · `([a-z0-9_]+)`(.*)$", line)
        if m:
            prompt, i = code_block_after(lines, i + 1)
            entries.append({
                "kind": {"遗物": "relic", "药水": "potion", "能力": "power"}.get(m.group(1), "card"),
                "name": m.group(2).strip(),
                "key": m.group(3),
                "character": "👤" in m.group(4),
                "prompt": prompt,
            })
            continue
        i += 1
    return shared, entries


def build_request(shared: dict, e: dict) -> str:
    if e["kind"] == "card":
        aspect = "landscape 4:3"
        prompt = " ".join(p for p in (shared["card"], shared["character"] if e["character"] else "", e["prompt"]) if p)
    else:
        aspect = "square 1:1, transparent background if possible"
        base = shared["power_icon"] if e["kind"] == "power" else shared["icon"]
        prompt = f'{base} {e["prompt"]}'
    return (
        f"Generate exactly one image with your image generation tool ({aspect}). Prompt: {prompt}\n\n"
        f"After generating, save the image as a PNG file at {OUT.name}/{e['key']}.png "
        f"(create the folder if needed). Do not edit any other files. Reply with just the saved path."
    )


def log(msg: str) -> None:
    line = f"{time.strftime('%H:%M:%S')} {msg}"
    with log_lock:
        print(line, flush=True)
        with LOG.open("a", encoding="utf-8") as f:
            f.write(line + "\n")


def run_one(shared: dict, e: dict) -> str:
    target = OUT / f"{e['key']}.png"
    for attempt in range(1, RETRIES + 2):
        if stop.is_set():
            return "stopped"
        # The prompt goes before -i: -i takes a list of files and would swallow a prompt after it.
        cmd = ["codex", "exec", "--skip-git-repo-check", "--sandbox", "workspace-write", "--ephemeral",
               build_request(shared, e)]
        refs = ([STYLE] if e["kind"] == "card" else []) + ([SHEET] if e["character"] else [])
        if refs:
            cmd += ["-i", *(str(r.relative_to(ROOT)) for r in refs)]
        try:
            res = subprocess.run(cmd, cwd=ROOT, capture_output=True, text=True, timeout=JOB_TIMEOUT,
                                 stdin=subprocess.DEVNULL)
            output = (res.stdout + res.stderr).lower()
        except subprocess.TimeoutExpired:
            output = "timeout"
        if target.exists() and target.stat().st_size > 10_000:
            log(f"OK    {e['key']} ({e['name']})")
            return "ok"
        if any(w in output for w in LIMIT_WORDS):
            stop.set()
            log(f"LIMIT {e['key']}: Codex reported a usage/rate limit, stopping. Re-run later to continue.")
            return "limit"
        log(f"RETRY {e['key']} attempt {attempt} failed: {output[-200:]!r}")
    log(f"FAIL  {e['key']} ({e['name']})")
    return "fail"


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--workers", type=int, default=3)
    ap.add_argument("--only", default="")
    ap.add_argument("--limit", type=int, default=0)
    args = ap.parse_args()

    OUT.mkdir(exist_ok=True)
    shared, entries = parse_card_md()
    only = {k.strip() for k in args.only.split(",") if k.strip()}
    todo = [e for e in entries
            if (not only or e["key"] in only) and not (OUT / f"{e['key']}.png").exists()]
    if args.limit:
        todo = todo[:args.limit]
    log(f"START {len(todo)} to generate ({len(entries)} in card.md), {args.workers} workers")

    results: dict[str, int] = {}
    with ThreadPoolExecutor(max_workers=args.workers) as pool:
        futures = [pool.submit(run_one, shared, e) for e in todo]
        for fut in as_completed(futures):
            r = fut.result()
            results[r] = results.get(r, 0) + 1
    done = sum(1 for e in entries if (OUT / f"{e['key']}.png").exists())
    log(f"END   {results}; {done}/{len(entries)} images now in {OUT.name}/")


if __name__ == "__main__":
    main()
