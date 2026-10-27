<p align="center"><img src="DeepSeekWhale/DeepSeekWhale/mod_image.png" width="256" alt="蓝色大肥鱼"></p>

# 蓝色大肥鱼 · Big Blue Fish

**《杀戮尖塔 2》角色模组** · A character mod for *Slay the Spire 2*

[中文](#中文) · [English](#english)

---

## 中文

爱吃大白饭的蓝色大肥鱼来爬塔了。吃饱了才干活，能摸鱼就摸鱼，还绝对不许别人叫她胖。

### 玩法

- **饱腹**：核心资源，上限 10，回合之间保留。**吃白饭** 获得饱腹，吃撑了的部分变成格挡；**开工** 消耗饱腹打出爆发伤害；饱腹 ≥ 6 时 **吃饱**，很多牌会变强。
- **摸鱼**：初始遗物「电饭煲」——回合结束时每剩 1 点能量，吃白饭 2。留能量也有收益，摸不摸鱼是每回合的取舍。
- 四个流派：
  - 🍚 **干饭**：攒饱腹、开工爆发（白饭加特林、饭桶、满汉全席……）
  - 🐟 **摸鱼**：故意留能量（带薪摸鱼、准点下班、峰谷定价……）
  - 💬 **刷屏**：生成 0 费「评论」，评论越多每张越疼（评论区沦陷、大肥鱼消除计划……）
  - 😤 **傲娇**：攒「恼羞成怒」，一拳还回去（才不是胖、气鼓鼓、这是鲸脂！……）
- 🤝 **女仆**：联机专属牌，照顾队友（主人请用、鱼片守护、一起干饭……）
- 共 84 张牌，另有专属遗物、药水，美术按原版画风绘制。完整卡表见 [CARDS.md](CARDS.md)。

### 安装

需要游戏版本 0.107 以上，以及前置模组 **BaseLib**（3.4.7 以上）。

- **Steam 创意工坊**：订阅「蓝色大肥鱼」，会自动提示订阅前置 BaseLib。
- **手动安装**：在 [Releases](../../releases) 下载 zip，把里面 `mods` 文件夹中的 `DeepSeekWhale` 和 `BaseLib` 两个文件夹放进游戏目录的 `mods` 文件夹，然后在游戏的模组菜单里启用它们并重启游戏。
  - Windows：`Steam\steamapps\common\Slay the Spire 2\mods\`
  - macOS：右键游戏 → 管理 → 浏览本地文件，进入 `SlayTheSpire2.app/Contents/MacOS/mods/`

联机时所有玩家都要装同样版本的模组。

### 从源码构建

需要 .NET 9 SDK、[Godot 4.5.1 .NET 版](https://godotengine.org/download/archive/4.5.1-stable/)（更新的 Godot 导出的 .pck 游戏不认）和已安装的游戏。

1. 在 `DeepSeekWhale/Directory.Build.props` 里设置 Godot 路径；游戏路径由 `Sts2PathDiscovery.props` 自动查找。
2. 关掉游戏，运行 `DeepSeekWhale/build.sh`：编译 DLL、导出 PCK 并装进游戏的 `mods` 文件夹。

`tools/` 里是美术流水线脚本（卡图批量生成、角色图处理、Spine 骨骼、能量球绘制）。生成时用作画风参考的原版截图属于 Mega Crit，没有放进仓库。

### 许可

- 代码（`DeepSeekWhale/DeepSeekWhaleCode/`、`tools/`）：MIT 许可，见 [LICENSE](LICENSE)。
- 美术、角色形象、文本：© Stuart Fan，保留所有权利，仅供本模组使用。
- 本模组为粉丝作品，与 Mega Crit 及 DeepSeek 无关。

### 致谢

- [BaseLib](https://github.com/Alchyr/BaseLib-StS2)（Alchyr，MIT 许可）
- Mega Crit 的《杀戮尖塔 2》及官方模组支持

---

## English

The Big Blue Fish loves plain rice. She only works once she's full, slacks off whenever she can, and is absolutely *not* fat.

### Gameplay

- **Fullness**: her core resource, up to 10, kept between turns. **Eat** to gain it (anything over the cap becomes Block), **Work** to spend it on burst damage; at 6 or more she is **Full** and many cards get stronger.
- **Slacking**: her starter relic, the Rice Cooker, Eats 2 for every unspent energy at the end of your turn, so leaving energy unused is a real choice.
- Four archetypes: 🍚 **Eat**, 🐟 **Slack**, 💬 **Comments** (0-cost Comment cards that hit harder the more you hold), 😤 **Tsundere** (store up Flustered, then let it out on one attack).
- 🤝 **Maid**: multiplayer-only cards that look after your allies.
- 84 cards plus her own relics and potions, with art in the base game's style. Full list (Chinese) in [CARDS.md](CARDS.md).

### Install

Requires game version 0.107 or later and **BaseLib** 3.4.7 or later.

- **Steam Workshop**: subscribe to "蓝色大肥鱼 Big Blue Fish"; Steam will offer the required BaseLib too.
- **Manual**: download the zip from [Releases](../../releases), copy the `DeepSeekWhale` and `BaseLib` folders from its `mods` folder into the game's `mods` folder, enable both in the game's Mods menu and restart.

In multiplayer every player needs the same mod versions.

### Building

Needs the .NET 9 SDK, [Godot 4.5.1 .NET](https://godotengine.org/download/archive/4.5.1-stable/) (the game rejects .pck files from newer Godot) and the game. Set the Godot path in `DeepSeekWhale/Directory.Build.props`, close the game and run `DeepSeekWhale/build.sh`.

### License

Code is MIT ([LICENSE](LICENSE)). Art, character design and text © Stuart Fan, all rights reserved. A fan project, not affiliated with Mega Crit or DeepSeek. Thanks to [BaseLib](https://github.com/Alchyr/BaseLib-StS2) by Alchyr.
