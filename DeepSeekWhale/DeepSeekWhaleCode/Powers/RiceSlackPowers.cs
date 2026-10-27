using DeepSeekWhale.DeepSeekWhaleCode.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Powers;

/// <summary>Cards that 开工 (spend Fullness); 满汉全席 makes them cheaper for a turn.</summary>
public interface IWorkCard;

/// <summary>Unspent energy of the power's owner right now (for 摸鱼 effects).</summary>
public static class Slack
{
    public static int UnspentEnergy(Creature owner) => owner.Player?.PlayerCombatState?.Energy ?? 0;

    /// <summary>How many times a 摸鱼 effect happens: twice with 摸鱼大师.</summary>
    public static int Times(Creature owner) => owner.HasPower<SlackMasterPower>() ? 2 : 1;
}

// ---- 🍚 Eat ----

/// <summary>饭桶: Fullness cap +5 (read by FullnessPower); 开工 cards deal 50% more damage.</summary>
public sealed class RiceBucketPower : WhaleFlagPower
{
    public const int CapBonus = 5;
    public const decimal WorkDamageMultiplier = 1.5m;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        return dealer == Owner && cardSource is IWorkCard && props.IsPoweredAttack() ? WorkDamageMultiplier : 1m;
    }
}

/// <summary>保温模式: 吃白饭 Amount at the start of each turn.</summary>
public sealed class KeepWarmPower : WhaleCounterPower
{
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        await FullnessPower.Eat(new ThrowingPlayerChoiceContext(), Owner, Amount, null);
    }
}

/// <summary>外卖到了: at the start of the next turn, 吃白饭 Amount and draw 2; then goes away.</summary>
public sealed class TakeoutPower : WhaleCounterPower
{
    public const int Draw = 2;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner) || Owner.Player is not { } player) return;
        Flash();
        var ctx = new ThrowingPlayerChoiceContext();
        await FullnessPower.Eat(ctx, Owner, Amount, null);
        await CardPileCmd.Draw(ctx, Draw, player);
        await PowerCmd.Remove(this);
    }
}

/// <summary>大胃王: every 吃白饭 eats Amount more (read by FullnessPower.Eat).</summary>
public sealed class BigEaterPower : WhaleCounterPower;

/// <summary>干饭王: whenever you 吃白饭, deal that much damage to a random enemy (Amount times).</summary>
public sealed class RiceKingPower : WhaleCounterPower, IEatListener
{
    public async Task AfterEat(PlayerChoiceContext choiceContext, int amount)
    {
        if (Owner.Player is not { } player || amount <= 0) return;
        Flash();
        for (int i = 0; i < Amount; i++)
            await Whale.DamageRandomEnemy(choiceContext, player, Owner.CombatState, amount, null);
    }
}

/// <summary>满汉全席: this turn, 开工 cards cost 1 less. Removed at the end of the turn.</summary>
public sealed class FeastPower : WhaleFlagPower
{
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card is not IWorkCard || card.Owner != Owner.Player || originalCost <= 0) return false;
        modifiedCost = originalCost - 1;
        return true;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}

// ---- 🐟 Slack ----

/// <summary>我去吃饭: at the start of the next turn, gain 1 energy and draw Amount; then goes away.</summary>
public sealed class LunchBreakPower : WhaleCounterPower
{
    public const int Energy = 1;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner) || Owner.Player is not { } player) return;
        Flash();
        await PlayerCmd.GainEnergy(Energy, player);
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, player);
        await PowerCmd.Remove(this);
    }
}

/// <summary>准点下班 (摸鱼): at the end of your turn, Amount damage to all enemies per unspent energy.</summary>
public sealed class ClockOutPower : WhaleCounterPower
{
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        int energy = Slack.UnspentEnergy(Owner);
        if (energy <= 0) return;
        Flash();
        for (int i = 0; i < Slack.Times(Owner); i++)
            await DamageAllEnemies(Amount * energy);
    }
}

/// <summary>躺平 (摸鱼): at the end of your turn, gain Amount Block per unspent energy.</summary>
public sealed class LieFlatPower : WhaleCounterPower
{
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        int energy = Slack.UnspentEnergy(Owner);
        if (energy <= 0) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount * energy * Slack.Times(Owner), ValueProp.Unpowered, null);
    }
}

/// <summary>摸鱼大师: your 摸鱼 effects happen twice (read through <see cref="Slack.Times"/>).</summary>
public sealed class SlackMasterPower : WhaleFlagPower;

/// <summary>留着明天用: up to Amount unspent energy carries over to the next turn.</summary>
public sealed class SaveForTomorrowPower : WhaleCounterPower
{
    private int _saved;

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) _saved = Math.Min(Amount, Slack.UnspentEnergy(Owner));
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner) || _saved <= 0 || Owner.Player is not { } player) return;
        Flash();
        int saved = _saved;
        _saved = 0;
        await PlayerCmd.GainEnergy(saved, player);
    }
}

/// <summary>
/// 峰谷定价: odd turns are "peak" — your cards cost 1 more, and their damage and Block are x1.5;
/// even turns are "off-peak" — your cards cost 1 less.
/// </summary>
public sealed class PeakValleyPower : WhaleFlagPower
{
    public const decimal PeakMultiplier = 1.5m;

    private bool IsPeak => (Owner.Player?.PlayerCombatState?.TurnNumber ?? 1) % 2 == 1;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner != Owner.Player || originalCost < 0) return false;
        modifiedCost = IsPeak ? originalCost + 1 : Math.Max(0, originalCost - 1);
        return modifiedCost != originalCost;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner || cardSource == null || !props.IsPoweredAttack() || !IsPeak) return 1m;
        return PeakMultiplier;
    }

    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || cardSource == null || !IsPeak) return 1m;
        return PeakMultiplier;
    }
}
