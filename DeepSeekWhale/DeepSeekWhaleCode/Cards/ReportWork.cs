using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

/// <summary>汇报工作 (multiplayer only): every player (you too) draws 1, you 吃白饭 1 (3).</summary>
public sealed class ReportWork() : DeepSeekWhaleCard(0, CardType.Skill, CardRarity.Common, TargetType.AllAllies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1), new DynamicVar("Eat", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FullnessPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var allies = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(c => c is { IsAlive: true, IsPlayer: true } && c != Owner.Creature);
        foreach (var ally in allies)
            await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, DynamicVars.Cards.BaseValue, ally.Player!, this);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await FullnessPower.Eat(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, this);
    }

    protected override void OnUpgrade() => DynamicVars["Eat"].UpgradeValueBy(2m);
}
