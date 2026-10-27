using DeepSeekWhale.DeepSeekWhaleCode.Cards;
using DeepSeekWhale.DeepSeekWhaleCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Powers;

// 😤 傲娇流: "才不是胖！" — store up 恼羞成怒, then let it all out on one attack.

/// <summary>
/// 恼羞成怒: your next attack card deals Amount x 3 more damage on every hit (x 4 with 呆毛, and the
/// bonus is doubled on 气鼓鼓). Played attack cards use it up — only the stacks that were there
/// before the card was played, so rage gained by that card carries on. 记仇本 halves it instead.
/// </summary>
public sealed class FlusteredPower : WhaleCounterPower
{
    public const int PerStack = 3;

    private int _spending;

    public static Task Gain(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source) =>
        amount > 0 ? PowerCmd.Apply<FlusteredPower>(choiceContext, creature, amount, creature, source) : Task.CompletedTask;

    private int BonusPerStack => PerStack + (Owner.Player?.Relics.OfType<Ahoge>().Any() == true ? Ahoge.Extra : 0);

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner || !props.IsPoweredAttack() || cardSource is not { Type: CardType.Attack } card
            || card.Owner != Owner.Player)
            return 0m;
        return Amount * BonusPerStack * (card is Puffed ? 2 : 1);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.Owner == Owner.Player) _spending = Amount;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_spending <= 0 || cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Attack) return;
        int spent = Owner.HasPower<GrudgeBookPower>() ? _spending - _spending / 2 : _spending;
        _spending = 0;
        if (spent >= Amount)
            await PowerCmd.Remove(this);
        else
            await PowerCmd.ModifyAmount(choiceContext, this, -spent, Owner, null);
    }
}

/// <summary>本鲸生气了: whenever you get a debuff or take unblocked attack damage, 恼羞成怒 Amount.</summary>
public sealed class GettingAngryPower : WhaleCounterPower
{
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || power.Type != PowerType.Debuff || amount <= 0) return;
        Flash();
        await FlusteredPower.Gain(choiceContext, Owner, Amount, null);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || dealer == Owner || result.UnblockedDamage <= 0 || !props.IsPoweredAttack()) return;
        Flash();
        await FlusteredPower.Gain(choiceContext, Owner, Amount, null);
    }
}

/// <summary>记仇本: attacks only use up half of your 恼羞成怒 (rounded up).</summary>
public sealed class GrudgeBookPower : WhaleFlagPower;
