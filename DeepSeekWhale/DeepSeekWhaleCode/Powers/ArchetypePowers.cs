using DeepSeekWhale.DeepSeekWhaleCode.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Powers;

/// <summary>Base for powers whose Amount is the effect size and that stack by adding.</summary>
public abstract class WhaleCounterPower : DeepSeekWhalePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected Task DamageAllEnemies(decimal amount) =>
        CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.CombatState.HittableEnemies, amount,
            ValueProp.Unpowered, Owner);
}

/// <summary>Base for on/off powers.</summary>
public abstract class WhaleFlagPower : DeepSeekWhalePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}

// ---- 🧠 Think ----

/// <summary>思维链: gain Amount Block whenever you play an upgraded card.</summary>
public sealed class ChainOfThoughtPower : WhaleCounterPower
{
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !cardPlay.Card.IsUpgraded) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}

/// <summary>测试时计算: at end of turn, upgrade Amount random upgradable cards in hand.</summary>
public sealed class TestTimeComputePower : WhaleCounterPower
{
    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player is not { } player) return Task.CompletedTask;
        var pool = Whale.Hand(player).Where(c => c.IsUpgradable).ToList();
        if (pool.Count == 0) return Task.CompletedTask;
        Flash();
        var rng = player.RunState.Rng.CombatCardSelection;
        for (int i = 0; i < Amount && pool.Count > 0; i++)
        {
            var card = rng.NextItem(pool)!;
            pool.Remove(card);
            CardCmd.Upgrade(card);
        }
        return Task.CompletedTask;
    }
}

// ---- 💬 Comments ----

/// <summary>水军: 刷屏 Amount at the start of each turn.</summary>
public sealed class ShillArmyPower : WhaleCounterPower
{
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner) || Owner.Player is not { } player) return;
        Flash();
        await Whale.AddComments(player, combatState, Amount);
    }
}

/// <summary>防扎堆: whenever you play a 评论, gain Amount Block.</summary>
public sealed class AntiCrowdingPower : WhaleCounterPower
{
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not Comment || cardPlay.Card.Owner != Owner.Player) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}

/// <summary>6710亿参数: playing a 评论 deals Amount damage to a random enemy.</summary>
public sealed class ParameterCountPower : WhaleCounterPower
{
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not Comment || cardPlay.Card.Owner != Owner.Player || Owner.Player is not { } player) return;
        Flash();
        await Whale.DamageRandomEnemy(choiceContext, player, Owner.CombatState, Amount, null);
    }
}

/// <summary>开源权重: 评论 deal double damage (checked in <see cref="Comment.DamageFor"/>).</summary>
public sealed class OpenWeightsPower : WhaleFlagPower;

/// <summary>DeepSeek时刻: each turn gain Amount energy and shuffle Amount Server Busy into your draw pile.</summary>
public sealed class DeepSeekMomentPower : WhaleCounterPower
{
    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await PlayerCmd.GainEnergy(Amount, player);
        await CardPileCmd.AddToCombatAndPreview<ServerBusy>(Owner, PileType.Draw, Amount, player, CardPilePosition.Random);
    }
}

// ---- ⚙️ General ----

/// <summary>鲸脂: gain Amount Block at the end of each turn.</summary>
public sealed class BlubberPower : WhaleCounterPower
{
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
