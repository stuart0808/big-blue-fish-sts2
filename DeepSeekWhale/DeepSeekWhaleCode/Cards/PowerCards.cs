using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

/// <summary>
/// A Power card whose whole effect is applying one power to its owner. The amount comes from a
/// "Power" dynamic var (shown in text as {Power}); it defaults to 1 for powers that don't stack.
/// </summary>
public abstract class SinglePowerCard<TPower>(int cost, CardRarity rarity, decimal amount = 1m) :
    DeepSeekWhaleCard(cost, CardType.Power, rarity, TargetType.Self) where TPower : PowerModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", amount)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TPower>(choiceContext, Owner.Creature, DynamicVars["Power"].BaseValue, Owner.Creature, this);
    }

    protected void UpgradeAmountBy(decimal delta) => DynamicVars["Power"].UpgradeValueBy(delta);
}

// ---- Uncommon ----

/// <summary>长上下文: see <see cref="LongContextPower"/>. Upgrade: costs 2.</summary>
public sealed class LongContext() : SinglePowerCard<LongContextPower>(3, CardRarity.Uncommon)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>思维链: gain 2 (3) Block whenever you play an upgraded card.</summary>
public sealed class ChainOfThought() : SinglePowerCard<ChainOfThoughtPower>(1, CardRarity.Uncommon, 2m)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    protected override void OnUpgrade() => UpgradeAmountBy(1m);
}

/// <summary>鲸脂: gain 4 (6) Block at the end of each turn.</summary>
public sealed class Blubber() : SinglePowerCard<BlubberPower>(1, CardRarity.Uncommon, 4m)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    protected override void OnUpgrade() => UpgradeAmountBy(2m);
}

// ---- Rare ----

/// <summary>深度思考: see <see cref="DeepThinkPower"/>. Upgrade: costs 1.</summary>
public sealed class DeepThink() : SinglePowerCard<DeepThinkPower>(2, CardRarity.Rare)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>测试时计算: at end of turn, upgrade 1 (2) random cards in hand.</summary>
public sealed class TestTimeCompute() : SinglePowerCard<TestTimeComputePower>(1, CardRarity.Rare)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    protected override void OnUpgrade() => UpgradeAmountBy(1m);
}

/// <summary>6710亿参数: playing a 评论 deals 3 (5) damage to a random enemy.</summary>
public sealed class ParameterCount() : SinglePowerCard<ParameterCountPower>(2, CardRarity.Rare, 3m)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Comment>()];
    protected override void OnUpgrade() => UpgradeAmountBy(2m);
}

/// <summary>开源权重: 评论 deal double damage. Upgrade: costs 0.</summary>
public sealed class OpenWeights() : SinglePowerCard<OpenWeightsPower>(1, CardRarity.Rare)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Comment>()];
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>DeepSeek时刻: gain 1 energy each turn, shuffle a Server Busy into your draw pile. Upgrade: costs 2.</summary>
public sealed class DeepSeekMoment() : SinglePowerCard<DeepSeekMomentPower>(3, CardRarity.Rare)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<ServerBusy>()];
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
