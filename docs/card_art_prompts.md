# 卡图生成提示词（v2：游戏原版画风）

v1 的提示词在 `archive_v1/card.md`。v2 改成杀戮尖塔 2 原版卡图的扁平赛璐璐画风：粗黑描边、平涂、3–4 种颜色、单一大主体、纯色背景。
`tools/batch_generate.py` 会给每张卡附上 `art_reference/game_style_reference.jpg`（原版卡图拼图）作为画风参考，标了 👤 的再附上角色设定图。

这份文件由 `tools/write_card_md.py` 生成。

---

## 通用提示词

```text
Card illustration in the exact art style of the deckbuilding game Slay the Spire 2 - the attached style reference sheet shows the game's own card art, match it closely. Flat cel-shaded 2D digital painting with thick dark outlines, flat color fills with only one hard-edged shadow tone and one highlight per color, a very limited palette of 3 to 4 saturated colors, chunky simplified shapes and almost no fine detail. One big simple subject that fills most of the frame and reads instantly at thumbnail size; close crops of a hand, a tail or an object are welcome. Dynamic diagonal composition. Show motion with a few bold graphic shapes (speed streaks, splash blobs, impact bursts), never with particles. Plain background: one saturated color or a simple two-tone gradient that contrasts with the subject, with at most one or two big abstract shapes. No sparkles, no bokeh, no glitter, no lens flare, no glow haze, no realistic lighting, no detailed anime rendering, no busy scenery. Landscape 4:3. No text, letters, numbers, card frame, border, UI or watermark.
```

👤 卡额外加这一句：

```text
The whale girl from the attached character sheet, redrawn simplified in that same flat game style: blue wavy hair with a small ahoge, white frilled maid headband, dark navy whale-fin ears, navy maid dress with a white apron, a big navy-and-white whale tail. Keep her face simple like the game's characters; she can be partly out of frame.
```

---

## 基础

### 打击 · `strike_whale` 👤
```text
Background: warm orange. The whale girl swings her big whale tail sideways in a hard slap, a bold white speed streak behind it.
```

### 防御 · `defend_whale` 👤
```text
Background: teal green. The whale girl crouches behind a round silver serving tray held up like a shield.
```

### 吃白饭 · `rice_bowl` 👤
```text
Background: sunny yellow. Close-up of the whale girl's hands lifting a huge white bowl heaped with plain rice, chopsticks stuck in the top.
```

### 开工 · `get_to_work` 👤
```text
Background: red. The whale girl rolls up her sleeve and punches forward, a cartoon impact burst at her fist.
```

---

## 普通

### 加一碗 · `another_bowl`
```text
Background: light orange. A hand holding an empty rice bowl out for a refill while a rice paddle drops a big scoop of rice into it.
```

### 干饭人 · `foodie` 👤
```text
Background: crimson. The whale girl lunges forward chomping a giant rice ball, a bite-shaped impact burst.
```

### 饭后甜点 · `dessert`
```text
Background: pink. A big wobbling pudding on a plate with a cherry on top, simple and chunky.
```

### 红烧肉优惠 · `braised_pork`
```text
Background: deep orange. A clay pot of glossy braised pork cubes with a bright red discount tag tied to the handle.
```

### 一口闷 · `bottoms_up` 👤
```text
Background: purple. The whale girl tips a huge bowl back and gulps it down in one go, rice flying out in chunky shapes.
```

### 饭点到了 · `meal_time`
```text
Background: yellow. A big round wall clock with both hands pointing at twelve, a fork and spoon crossed in front of it.
```

### 筷子连击 · `chopsticks`
```text
Background: red orange. A pair of chopsticks jabbing forward fast, three bold speed streaks behind them.
```

### 带薪摸鱼 · `paid_slack` 👤
```text
Background: mint green. The whale girl lazily lounges on an office chair, holding a small fish on a fishing line like a toy.
```

### 我去吃饭 · `lunch_break` 👤
```text
Background: light blue. The whale girl walking away from a desk with a lunch box, waving goodbye over her shoulder.
```

### 顺手一拳 · `casual_jab` 👤
```text
Background: orange. The whale girl, eyes half closed and bored, casually throws a lazy jab with one hand, small impact star.
```

### 缓存命中 · `cache_hit`
```text
Background: cyan. A glowing file folder snapping shut around a lightning bolt, a big check mark.
```

### 回声定位 · `echolocation`
```text
Background: dark teal. Three bold concentric sound wave arcs spreading out from a small whale silhouette.
```

### 联网搜索 · `web_search`
```text
Background: royal blue. A big magnifying glass over a simple globe made of thick grid lines.
```

### 提示词 · `prompt`
```text
Background: violet. A large speech bubble with a blinking text cursor block inside, no letters.
```

### Token刷屏 · `token_flood`
```text
Background: magenta. A waterfall of chunky round coins stamped with a whale shape, pouring down diagonally.
```

### 本鲸来也 · `whale_is_here` 👤
```text
Background: bright red. The whale girl leaps into frame feet first with a heroic pose, a bold radial burst behind her.
```

### 前排占座 · `front_row_seat` 👤
```text
Background: orange. The whale girl sits proudly in the front seat of a row of chairs, arms crossed, smug.
```

### 讨要Token · `beg_for_tokens` 👤
```text
Background: gold yellow. The whale girl holding out both hands with puppy eyes toward a single big shiny coin.
```

### 才不是胖 · `not_fat` 👤
```text
Background: pink. The whale girl puffs her cheeks and crosses her arms, turning away, a big angry vein mark.
```

### 不许叫我胖 · `dont_call_me_fat` 👤
```text
Background: red. The whale girl smashes a bathroom scale with her whale tail, the scale cracking.
```

### 傲娇嘴甜 · `sweet_talk` 👤
```text
Background: lavender. The whale girl looks away blushing while pointing a finger, a small heart and an angry mark floating together.
```

### 汇报工作 · `report_work` 👤
```text
Background: teal. The whale girl stands at attention holding up a clipboard with a big check mark.
```

---

## 罕见

### 吃饱了才干活 · `work_when_full` 👤
```text
Background: orange red. The whale girl with a round full belly swings a giant rolled-up newspaper overhead for a heavy blow.
```

### 白饭加特林 · `rice_gatling`
```text
Background: yellow. A gatling gun made of rice bowls firing a stream of white rice balls diagonally.
```

### 外卖到了 · `takeout_arrived`
```text
Background: red. A delivery box with a whale logo bursting open with steam shapes, a scooter wheel at the edge.
```

### 保温模式 · `keep_warm`
```text
Background: warm orange. A round electric rice cooker with a glowing orange 'keep warm' light and wavy heat lines.
```

### 打包带走 · `doggy_bag`
```text
Background: brown orange. A takeout box being folded shut over leftover rice, a bold shield shape behind it.
```

### 大胃王 · `big_eater` 👤
```text
Background: magenta. The whale girl with her mouth wide open swallowing a tower of stacked rice bowls.
```

### 准点下班 · `clock_out` 👤
```text
Background: sunset orange. The whale girl dashes out of an office door at full speed, a clock showing six behind her.
```

### 留着明天用 · `save_for_tomorrow`
```text
Background: night purple. A glass jar holding a glowing yellow energy orb, lid being screwed shut.
```

### 假装很忙 · `look_busy` 👤
```text
Background: green. The whale girl hides behind a tall stack of papers, peeking out with one eye.
```

### 午睡 · `nap` 👤
```text
Background: soft blue. The whale girl asleep face-down on a desk, a big Z-shaped zigzag bubble rising (no letters, just a zigzag shape).
```

### 慢思考 · `slow_thinking`
```text
Background: deep blue. A huge snail with a brain-shaped shell charging forward, bold speed lines.
```

### 推理模型 · `reasoner`
```text
Background: purple. A chain of three big linked gears turning, the last gear hitting forward like a hammer.
```

### 反思 · `reflection` 👤
```text
Background: silver blue. The whale girl looking into a hand mirror, her reflection looking back thoughtfully.
```

### 少样本学习 · `few_shot`
```text
Background: teal. Three simple cards fanned out with one bright arrow learning from them.
```

### 长上下文 · `long_context`
```text
Background: dark green. An extremely long paper scroll unrolling across the whole frame in a big curve.
```

### 思维链 · `chain_of_thought`
```text
Background: royal purple. A thick chain made of thought-bubble links curving across the frame.
```

### 一本正经 · `confident` 👤
```text
Background: orange. The whale girl in tiny glasses lectures with a pointer stick, very serious, a crooked chart behind her.
```

### 评论区沦陷 · `comment_flood`
```text
Background: hot pink. A huge pile of speech bubbles bursting out of a phone screen and flooding the frame.
```

### 热评第一 · `hot_comment`
```text
Background: gold. A single speech bubble on a winner's podium with a big flaming thumbs-up.
```

### 复制粘贴 · `copy_paste`
```text
Background: lime green. Two identical speech bubbles stamping out a third copy like a rubber stamp.
```

### 水军 · `shill_army`
```text
Background: olive green. A marching row of small identical cartoon fish holding protest signs (blank signs).
```

### 防扎堆 · `anti_crowding`
```text
Background: teal. A big round shield deflecting a crowd of small speech bubbles.
```

### 鲸脂 · `blubber` 👤
```text
Background: pink. The whale girl hugging her big soft whale tail like a pillow, a bold shield outline around her.
```

### 本鲸生气了 · `getting_angry` 👤
```text
Background: red. The whale girl stomping with steam blowing out of her ears, a big angry vein mark.
```

### 气鼓鼓 · `puffed` 👤
```text
Background: crimson. The whale girl with cheeks puffed up like a pufferfish winding up a huge punch.
```

### 哼！ · `hmph` 👤
```text
Background: pink purple. The whale girl turning her head away with her nose up, eyes closed, a puff of air from her nose.
```

### 炸毛 · `bristle` 👤
```text
Background: orange red. The whale girl with all her hair standing straight up like spikes, a sharp starburst shockwave around her.
```

### 注意力机制 · `attention`
```text
Background: yellow. A huge staring eye with a target crosshair pupil, bold rays pointing at it.
```

### 联想 · `associate`
```text
Background: violet. A light bulb connected by thick lines to three floating card shapes.
```

### 混合专家 · `mixture_of_experts`
```text
Background: multicolor four-quadrant split (orange, green, blue, purple). Six small round robot heads arranged in a circle around a big glowing core.
```

### 知识蒸馏 · `distillation`
```text
Background: teal. A glass distillation flask dripping a single bright drop into a small vial.
```

### 服务器扩容 · `scale_up`
```text
Background: steel blue. A server rack growing taller with new blocks stacking on top, a big up arrow.
```

### 降本增效 · `cost_cut`
```text
Background: green. Big scissors cutting a price tag in half, a bright energy spark at the cut.
```

### 开源 · `open_source`
```text
Background: light green. An open treasure chest with a big shield shape rising out of it.
```

### 主人请用 · `serve_master` 👤
```text
Background: warm pink. The whale girl bowing politely while holding out a tray with a teacup.
```

### 鱼片守护 · `fish_guard` 👤
```text
Background: teal. The whale girl spreading her arms protectively, a large fish-shaped shield in front of her.
```

---

## 稀有

### 鲸吞 · `gulp`
```text
Background: deep red. A gigantic whale mouth opening wide from the bottom of the frame, about to swallow a small monster.
```

### 饭桶 · `rice_bucket`
```text
Background: gold. A huge wooden rice barrel overflowing with rice, a crown on top.
```

### 满汉全席 · `feast`
```text
Background: imperial red. A long banquet table seen from above covered with many simple bowls and plates.
```

### 干饭王 · `rice_king` 👤
```text
Background: gold. The whale girl wearing a crown, holding a giant spoon like a scepter, sitting on a mountain of rice.
```

### 峰谷定价 · `peak_valley`
```text
Background: half orange half purple diagonal split. A bold zigzag price line with a sun on the high peak and a moon in the low valley.
```

### 躺平 · `lie_flat` 👤
```text
Background: mint green. The whale girl lying flat on the floor like a starfish, completely relaxed.
```

### 摸鱼大师 · `slack_master` 👤
```text
Background: aqua. The whale girl in sunglasses on a beach chair holding two fishing rods, each with a fish.
```

### 深度思考 · `deep_think` 👤
```text
Background: indigo. The whale girl with eyes closed and a big glowing brain shape above her head.
```

### 长考 · `long_think`
```text
Background: dark purple. A giant hourglass smashing down like a hammer, sand bursting out.
```

### 顿悟时刻 · `aha_moment` 👤
```text
Background: bright yellow. The whale girl with a huge light bulb flashing above her head, eyes wide open.
```

### 测试时计算 · `test_time_compute`
```text
Background: teal. A stopwatch fused with a calculator, with a gear turning in the middle.
```

### 6710亿参数 · `parameter_count`
```text
Background: electric blue. A massive cube built from countless small cubes, one corner cracking with lightning.
```

### 开源权重 · `open_weights`
```text
Background: orange. A big dumbbell with an open padlock hanging on it.
```

### 大肥鱼消除计划 · `whale_clear_plan`
```text
Background: purple. A giant hand snapping its fingers, a shockwave wiping away a cloud of speech bubbles.
```

### 这是鲸脂！ · `this_is_blubber` 👤
```text
Background: pink. The whale girl flexing proudly, her soft tummy glowing as a solid shield shape.
```

### 记仇本 · `grudge_book`
```text
Background: dark red. A small black notebook with a padlock and an angry face sticker on the cover.
```

### R1满血版 · `full_blood_model` 👤
```text
Background: red. The whale girl blasting forward wreathed in a huge solid flame shape, full power.
```

### 宕机重启 · `reboot`
```text
Background: orange. A power button symbol cracking open with a big shockwave ring.
```

### DeepSeek时刻 · `deep_seek_moment` 👤
```text
Background: royal blue. The whale girl on top of a rising graph line, triumphant, arms up.
```

### 价格屠夫 · `price_butcher`
```text
Background: red. A butcher's cleaver chopping a price tag in two.
```

### 蒸馏传授 · `teach_distill` 👤
```text
Background: teal. The whale girl handing a small glowing vial to another hand reaching in from the edge.
```

### 一起干饭 · `group_breath`
```text
Background: warm yellow. Several hands lifting rice bowls together in a toast from different sides of the frame.
```

---

## 衍生牌

### 评论 · `comment`
```text
Background: white and pink. A single chunky speech bubble with a small whale tail sticking out of it.
```

### 服务器繁忙 · `server_busy`
```text
Background: gray. A server rack with a spinning loading circle on its front, smoke puffs.
```

### 攻击专家 · `expert_attack`
```text
Background: red. A small round robot head with a sword.
```

### 防御专家 · `expert_guard`
```text
Background: teal. A small round robot head behind a shield.
```

### 侦察专家 · `expert_scout`
```text
Background: green. A small round robot head with a telescope.
```

### 干饭专家 · `expert_eater`
```text
Background: yellow. A small round robot head eating from a rice bowl.
```

### 破甲专家 · `expert_breaker`
```text
Background: orange. A small round robot head swinging a hammer at a cracked shield.
```

### 削弱专家 · `expert_weakener`
```text
Background: purple. A small round robot head pointing a drooping wilted flower.
```

---

## 遗物和药水图标

### 图标通用提示词

```text
Game item icon in the flat cel-shaded style of Slay the Spire 2: a single object, centered, filling about 80% of the frame, thick dark outline, flat colors with one hard shadow tone, 3 to 4 colors, chunky simple shapes readable at small size. Transparent background (or flat pure green #00FF00 if transparency is not supported). No text.
```

### 遗物 · 电饭煲 · `rice_cooker`
```text
A cute round white electric rice cooker with a small whale emblem.
```

### 遗物 · 满血电饭煲 · `full_rice_cooker`
```text
A cute round golden electric rice cooker with steam puffs and a flame emblem.
```

### 遗物 · 外卖红包 · `takeout_red_packet`
```text
A red envelope with a gold coin and a tiny chopstick pair printed on it.
```

### 遗物 · 优惠时段 · `off_peak_hours`
```text
A small alarm clock with a moon on its face and a discount tag.
```

### 遗物 · 呆毛 · `ahoge`
```text
A single curled strand of blue hair standing up, springy.
```

### 遗物 · 女仆装 · `maid_outfit`
```text
A folded navy maid dress with a white frilled apron on top.
```

### 药水 · 一碗白饭 · `rice_bowl_potion`
```text
A potion bottle shaped like a rice bowl, filled with white rice.
```

### 药水 · 外卖 · `takeout_potion`
```text
A small takeout soup cup with a lid and a straw, red label.
```

### 药水 · 评论机器人 · `comment_bot_potion`
```text
A potion bottle shaped like a small robot head, filled with speech bubbles.
```

---

## 能力图标

### 能力图标通用提示词

```text
Status effect icon in the flat style of Slay the Spire 2: one single bold symbol, centered, filling about 80% of the frame, thick dark outline, flat colors, at most 3 colors, very simple shapes that stay readable at 32 pixels. Transparent background (or flat pure green #00FF00 if transparency is not supported). No text, no letters, no numbers, no frame.
```

### 能力 · 饱腹 · `fullness_power`
```text
A white rice bowl, heaped.
```

### 能力 · 饭桶 · `rice_bucket_power`
```text
A wooden rice barrel.
```

### 能力 · 保温模式 · `keep_warm_power`
```text
A rice cooker with heat lines.
```

### 能力 · 外卖到了 · `takeout_power`
```text
A delivery box.
```

### 能力 · 满汉全席 · `feast_power`
```text
A crossed spoon and chopsticks.
```

### 能力 · 我去吃饭 · `lunch_break_power`
```text
A lunch box.
```

### 能力 · 准点下班 · `clock_out_power`
```text
A clock with a running figure arrow.
```

### 能力 · 留着明天用 · `save_for_tomorrow_power`
```text
A jar with an energy orb.
```

### 能力 · 峰谷定价 · `peak_valley_power`
```text
A zigzag line with a sun and a moon.
```

### 能力 · 恼羞成怒 · `flustered_power`
```text
An angry vein mark.
```

### 能力 · 本鲸生气了 · `getting_angry_power`
```text
A steaming kettle.
```

### 能力 · 记仇本 · `grudge_book_power`
```text
A small black notebook.
```

### 能力 · 水军 · `shill_army_power`
```text
A small fish holding a blank sign.
```

### 能力 · 防扎堆 · `anti_crowding_power`
```text
A shield with a speech bubble.
```

### 能力 · 大胃王 · `big_eater_power`
```text
An open mouth with a rice ball.
```

### 能力 · 干饭王 · `rice_king_power`
```text
A crown on a rice bowl.
```

### 能力 · 躺平 · `lie_flat_power`
```text
A pillow.
```

### 能力 · 摸鱼大师 · `slack_master_power`
```text
Two crossed fishing rods.
```
