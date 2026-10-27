#!/bin/bash
# Build the mod and install it into the game's mods folder (dll + json + pck).
# Close the game first: a running game keeps the dll open.
set -euo pipefail
cd "$(dirname "$0")"

export DOTNET_ROOT="${DOTNET_ROOT:-/opt/homebrew/Cellar/dotnet@9/9.0.122/libexec}"
GODOT="$HOME/Applications/Godot_mono_4.5.1.app/Contents/MacOS/Godot"

if pgrep -f "SlayTheSpire2.app" >/dev/null; then
    echo "Slay the Spire 2 is running; close it first." >&2
    exit 1
fi

# Re-import changed images so they end up in the .pck.
"$GODOT" --headless --path . --import >/dev/null 2>&1 || true
# publish = build the dll, copy dll/json to mods/, then export the .pck with Godot.
dotnet publish DeepSeekWhale.csproj 2>&1 | grep -E "error|Build succeeded|Copying|Exporting" || true

MODS="$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/DeepSeekWhale"
# The Spine rig is loaded from plain files beside the DLL, not from the .pck.
rsync -a --delete --exclude '*.import' spine/ "$MODS/spine/"
ls -la "$MODS"
