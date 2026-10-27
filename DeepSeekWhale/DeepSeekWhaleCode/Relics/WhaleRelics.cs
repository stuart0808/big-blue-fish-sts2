using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using DeepSeekWhale.DeepSeekWhaleCode.Cards;
using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace DeepSeekWhale.DeepSeekWhaleCode.Relics;

/// <summary>外卖红包 (common): the first time you become Full each combat, draw 2.</summary>
public sealed class TakeoutRedPacket : DeepSeekWhaleRelic, IFullListener
{
    private bool _usedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FullnessPower>()];

    public override Task BeforeCombatStart()
    {
        _usedThisCombat = false;
        return Task.CompletedTask;
    }

    public async Task AfterBecameFull()
    {
        if (_usedThisCombat) return;
        _usedThisCombat = true;
        Flash();
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), DynamicVars.Cards.BaseValue, Owner);
    }
}

/// <summary>显卡 (uncommon): at the start of each combat, 刷屏 2.</summary>
public sealed class Gpu : DeepSeekWhaleRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Comment>()];

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CombatState is not { } combatState) return;
        Flash();
        await Whale.AddComments(Owner, combatState, DynamicVars.Cards.IntValue);
    }
}

/// <summary>优惠时段 (rare): at the start of your even-numbered turns, gain 1 energy (峰谷定价's off-peak hours).</summary>
public sealed class OffPeakHours : DeepSeekWhaleRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || (Owner.PlayerCombatState?.TurnNumber ?? 1) % 2 != 0) return;
        Flash();
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }
}

/// <summary>备用服务器 (rare): whenever you draw a Server Busy, exhaust it and draw 1.</summary>
public sealed class BackupServer : DeepSeekWhaleRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<ServerBusy>()];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card is not ServerBusy || card.Owner != Owner) return;
        Flash();
        await CardCmd.Exhaust(choiceContext, card);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
}

/// <summary>呆毛 (uncommon): each stack of 恼羞成怒 adds 1 more damage (read by FlusteredPower).</summary>
public sealed class Ahoge : DeepSeekWhaleRelic
{
    public const int Extra = 1;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FlusteredPower>()];
}

/// <summary>女仆装 (rare, multiplayer only): the first time each turn you play a card on an ally, gain 6 Block.</summary>
public sealed class MaidOutfit : DeepSeekWhaleRelic
{
    private int _lastTurn = -1;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered)];

    public override bool IsAllowed(IRunState runState) => runState.Players.Count > 1;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card.TargetType != TargetType.AnyAlly) return;
        int turn = Owner.PlayerCombatState?.TurnNumber ?? 0;
        if (turn == _lastTurn) return;
        _lastTurn = turn;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
    }

    public override Task BeforeCombatStart()
    {
        _lastTurn = -1;
        return Task.CompletedTask;
    }
}
