using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

// 🤝 女仆流 (multiplayer only): the maid looks after her 主人. 汇报工作, 开源, 蒸馏传授 and 一起干饭
// (GroupBreath) are in their own files.

/// <summary>主人请用: an ally gains 8 (11) Block and draws 1.</summary>
public sealed class ServeMaster() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move), new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { IsAlive: true } ally || ally.Player is not { } player) return;
        await CreatureCmd.GainBlock(ally, DynamicVars.Block, cardPlay);
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, DynamicVars.Cards.BaseValue, player, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>鱼片守护: you and an ally each gain 6 (9) Block.</summary>
public sealed class FishGuard() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (cardPlay.Target is { IsAlive: true } ally && ally != Owner.Creature)
            await CreatureCmd.GainBlock(ally, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
