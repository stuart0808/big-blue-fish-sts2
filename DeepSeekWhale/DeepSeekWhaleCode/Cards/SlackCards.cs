using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

// 🐟 摸鱼流: leave energy unspent on purpose — 摸鱼 effects and Think upgrades pay for it.

/// <summary>
/// A card with a 摸鱼 effect: if it is still in hand at the end of your turn and you have unspent
/// energy, <see cref="OnSlack"/> runs (before the hand is discarded).
/// </summary>
public abstract class SlackCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    DeepSeekWhaleCard(cost, type, rarity, target)
{
    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        if ((Owner.PlayerCombatState?.Energy ?? 0) <= 0) return;
        for (int i = 0; i < Slack.Times(Owner.Creature); i++)
            await OnSlack(choiceContext);
    }

    protected abstract Task OnSlack(PlayerChoiceContext choiceContext);
}

// ---- Common ----

/// <summary>带薪摸鱼: Retain. Gain 6 (8) Block. 摸鱼: gain 4 (6) Block.</summary>
public sealed class PaidSlack() : SlackCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move), new DynamicVar("SlackBlock", 4m)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

    protected override Task OnSlack(PlayerChoiceContext choiceContext) =>
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars["SlackBlock"].BaseValue, ValueProp.Unpowered, null);

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["SlackBlock"].UpgradeValueBy(2m);
    }
}

/// <summary>我去吃饭: end your turn. Next turn, gain 1 energy and draw 2 (3).</summary>
public sealed class LunchBreak() : DeepSeekWhaleCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new EnergyVar(LunchBreakPower.Energy)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<LunchBreakPower>(choiceContext, Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>顺手一拳: deal 4 (6). 摸鱼: deal 4 (6) to a random enemy.</summary>
public sealed class CasualJab() : SlackCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override Task OnSlack(PlayerChoiceContext choiceContext) =>
        CombatState is { } combatState
            ? Whale.DamageRandomEnemy(choiceContext, Owner, combatState, DynamicVars.Damage.BaseValue, this)
            : Task.CompletedTask;

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

// ---- Powers (both archetypes) ----

/// <summary>保温模式: 吃白饭 2 at the start of each turn. Upgrade: Innate.</summary>
public sealed class KeepWarm() : SinglePowerCard<KeepWarmPower>(1, CardRarity.Uncommon, 2m)
{
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}

/// <summary>饭桶: Fullness cap +5; 开工 cards deal 50% more damage. Upgrade: costs 0.</summary>
public sealed class RiceBucket() : SinglePowerCard<RiceBucketPower>(1, CardRarity.Rare)
{
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>准点下班: 摸鱼 — 4 (6) damage to all enemies per unspent energy at the end of your turn.</summary>
public sealed class ClockOut() : SinglePowerCard<ClockOutPower>(1, CardRarity.Uncommon, 4m)
{
    protected override void OnUpgrade() => UpgradeAmountBy(2m);
}

/// <summary>留着明天用: keep up to 1 (2) unspent energy for the next turn.</summary>
public sealed class SaveForTomorrow() : SinglePowerCard<SaveForTomorrowPower>(1, CardRarity.Uncommon, 1m)
{
    protected override void OnUpgrade() => UpgradeAmountBy(1m);
}

/// <summary>峰谷定价: odd turns cost +1 but hit and block x1.5; even turns cost -1. Upgrade: costs 1.</summary>
public sealed class PeakValley() : SinglePowerCard<PeakValleyPower>(2, CardRarity.Rare)
{
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

// ---- Added with the full pool ----

/// <summary>假装很忙: gain 8 (11) Block. 摸鱼: draw 2 more cards next turn.</summary>
public sealed class LookBusy() : SlackCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move), new CardsVar(2)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

    protected override Task OnSlack(PlayerChoiceContext choiceContext) =>
        PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>午睡: gain 12 (16) Block. End your turn.</summary>
public sealed class Nap() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(12m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4m);
}

/// <summary>躺平: 摸鱼 — gain 5 (7) Block per unspent energy at the end of your turn.</summary>
public sealed class LieFlat() : SinglePowerCard<LieFlatPower>(1, CardRarity.Rare, 5m)
{
    protected override void OnUpgrade() => UpgradeAmountBy(2m);
}

/// <summary>摸鱼大师: your 摸鱼 effects happen twice. Upgrade: costs 1.</summary>
public sealed class SlackMaster() : SinglePowerCard<SlackMasterPower>(2, CardRarity.Rare)
{
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
