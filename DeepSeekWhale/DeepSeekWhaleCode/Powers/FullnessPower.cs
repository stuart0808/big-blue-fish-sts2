using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Powers;

/// <summary>Relics and powers that react when the owner becomes Full (饱腹 reaches <see cref="FullnessPower.FullAt"/>).</summary>
public interface IFullListener
{
    Task AfterBecameFull();
}

/// <summary>Powers that react to every 吃白饭 (amount = what was eaten, overflow included).</summary>
public interface IEatListener
{
    Task AfterEat(PlayerChoiceContext choiceContext, int amount);
}

/// <summary>
/// 饱腹 (Fullness): the whale's core resource, 0 to <see cref="Cap"/>, kept between turns.
/// 吃白饭 (<see cref="Eat"/>) adds to it; what would go over the cap turns into 2 Block per point
/// ("这不是胖，是鲸脂"). 开工 (<see cref="Work"/>) spends it. At <see cref="FullAt"/> or more the
/// owner is 吃饱 (Full). The static helpers are the one place cards, powers and relics change it.
/// </summary>
public sealed class FullnessPower : DeepSeekWhalePower
{
    public const int BaseCap = 10;
    public const int FullAt = 6;
    public const int OverflowBlock = 2;

    /// <summary>"开工 全部": spend all Fullness.</summary>
    public const int All = 999;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public static int Of(Creature creature) => creature.GetPowerAmount<FullnessPower>();

    public static int Cap(Creature creature) =>
        BaseCap + (creature.HasPower<RiceBucketPower>() ? RiceBucketPower.CapBonus : 0);

    public static bool IsFull(Creature creature) => Of(creature) >= FullAt;

    /// <summary>How much a "开工 <paramref name="max"/>" would spend right now.</summary>
    public static int Spendable(Creature creature, int max) => Math.Min(Of(creature), max);

    /// <summary>吃白饭 X (+大胃王): gain Fullness up to the cap; each point over it becomes 2 Block.</summary>
    public static async Task Eat(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
    {
        if ((int)amount <= 0) return;
        int add = (int)amount + (creature.GetPower<BigEaterPower>()?.Amount ?? 0);
        int before = Of(creature);
        int gain = Math.Min(add, Math.Max(0, Cap(creature) - before));
        int overflow = add - gain;
        if (gain > 0)
            await PowerCmd.Apply<FullnessPower>(choiceContext, creature, gain, creature, source);
        if (overflow > 0)
            await CreatureCmd.GainBlock(creature, overflow * OverflowBlock, ValueProp.Unpowered, null);

        if (before < FullAt && Of(creature) >= FullAt)
        {
            var relics = creature.Player?.Relics.OfType<IFullListener>() ?? [];
            foreach (var listener in creature.Powers.OfType<IFullListener>().Concat(relics).ToList())
                await listener.AfterBecameFull();
        }
        foreach (var listener in creature.Powers.OfType<IEatListener>().ToList())
            await listener.AfterEat(choiceContext, add);
    }

    /// <summary>开工 X: spend up to <paramref name="max"/> Fullness. Returns the amount spent.</summary>
    public static async Task<int> Work(PlayerChoiceContext choiceContext, Creature creature, int max)
    {
        var power = creature.GetPower<FullnessPower>();
        int spend = Spendable(creature, max);
        if (power == null || spend <= 0) return 0;
        if (spend >= power.Amount)
            await PowerCmd.Remove(power);
        else
            await PowerCmd.ModifyAmount(choiceContext, power, -spend, creature, null);
        return spend;
    }
}
