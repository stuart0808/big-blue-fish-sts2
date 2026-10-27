using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Powers;

/// <summary>长上下文: the owner's hand is not discarded at the end of turn.</summary>
public sealed class LongContextPower : DeepSeekWhalePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldFlush(Player player) => player != Owner.Player;
}

/// <summary>深度思考: at end of turn the whole hand is kept and every card in it is upgraded.</summary>
public sealed class DeepThinkPower : DeepSeekWhalePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldFlush(Player player) => player != Owner.Player;

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player?.PlayerCombatState is not { } state) return Task.CompletedTask;
        var upgradable = state.Hand.Cards.Where(c => c.IsUpgradable).ToList();
        if (upgradable.Count == 0) return Task.CompletedTask;
        Flash();
        foreach (var card in upgradable) CardCmd.Upgrade(card);
        return Task.CompletedTask;
    }
}
