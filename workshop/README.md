# Steam Workshop

Workspace files for Mega Crit's [mod uploader](https://github.com/megacrit/sts2-mod-uploader) (item [3828381886](https://steamcommunity.com/sharedfiles/filedetails/?id=3828381886)).

To publish an update: build the mod, copy `DeepSeekWhale.dll`, `DeepSeekWhale.json`, `DeepSeekWhale.pck` and the `spine/` folder into a `content/` folder here, edit `changeNote` in `workshop.json`, then run `ModUploader upload -w <this folder>`. `mod_id.txt` makes it update this item instead of creating a new one.
