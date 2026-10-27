using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

// 💬 刷屏流: flood the hand with 0-cost 评论 (each one hits harder the more of them you hold), then
// cash the crowd in. Token刷屏, 一本正经, 6710亿参数 and 开源权重 live in the v1 files.

/// <summary>Shared bits of the comment archetype.</summary>
internal static class Flood
{
    public static IHoverTip CommentTip => HoverTipFactory.FromCard<Comment>();
}

// ---- Common ----

/// <summary>本鲸来也: deal 7 (10), 刷屏 1.</summary>
public sealed class WhaleIsHere() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move), new CardsVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await Whale.AddComments(Owner, CombatState, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>前排占座: gain 6 (8) Block, 刷屏 2.</summary>
public sealed class FrontRowSeat() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move), new CardsVar(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await Whale.AddComments(Owner, CombatState, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>讨要Token: 刷屏 1 (2); then, with 3 or more 评论 in hand, gain 1 energy.</summary>
public sealed class BegForTokens() : DeepSeekWhaleCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public const int Needed = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];
    protected override bool ShouldGlowGoldInternal => Whale.CommentsInHand(Owner) + DynamicVars.Cards.IntValue >= Needed;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Whale.AddComments(Owner, CombatState, DynamicVars.Cards.IntValue);
        if (Whale.CommentsInHand(Owner) >= Needed)
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

// ---- Uncommon ----

/// <summary>评论区沦陷: 刷屏 until your hand is full. Exhaust. Upgrade: costs 0.</summary>
public sealed class CommentFlood() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Whale.AddComments(Owner, CombatState, CardPile.MaxCardsInHand - Whale.Hand(Owner).Count);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>热评第一: deal 6, +2 (3) per 评论 in hand.</summary>
public sealed class HotComment() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(6m),
        new ExtraDamageVar(2m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) => Whale.CommentsInHand(card.Owner)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(1m);
}

/// <summary>复制粘贴: 刷屏 once for each 评论 in your hand. Upgrade: costs 0.</summary>
public sealed class CopyPaste() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];
    protected override bool ShouldGlowGoldInternal => Whale.CommentsInHand(Owner) > 0;

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Whale.AddComments(Owner, CombatState, Whale.CommentsInHand(Owner));

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>水军: 刷屏 1 at the start of each turn. Upgrade: Innate.</summary>
public sealed class ShillArmy() : SinglePowerCard<ShillArmyPower>(1, CardRarity.Uncommon)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}

/// <summary>防扎堆: whenever you play a 评论, gain 2 (3) Block.</summary>
public sealed class AntiCrowding() : SinglePowerCard<AntiCrowdingPower>(1, CardRarity.Uncommon, 2m)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];
    protected override void OnUpgrade() => UpgradeAmountBy(1m);
}

// ---- Rare ----

/// <summary>
/// 大肥鱼消除计划: exhaust every 评论 in your hand, draw pile and discard pile; deal 5 (7) to all
/// enemies once for each.
/// </summary>
public sealed class WhaleClearPlan() : DeepSeekWhaleCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Flood.CommentTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var comments = CardPile.GetCards(Owner, PileType.Hand, PileType.Draw, PileType.Discard).OfType<Comment>().ToList();
        if (comments.Count == 0) return;
        foreach (var card in comments) await CardCmd.Exhaust(choiceContext, card);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)
            .WithHitCount(comments.Count)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
