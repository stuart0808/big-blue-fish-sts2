using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

// 😤 傲娇流: "我才不是胖！" Stand behind Block and 鲸脂, turn debuffs and hits into 恼羞成怒, then pay it
// back with one big attack. 鲸脂 itself is in PowerCards.cs.

/// <summary>Shared bits of the rage archetype.</summary>
internal static class Tsun
{
    public static IHoverTip RageTip => HoverTipFactory.FromPower<FlusteredPower>();

    public static Task Rage(CardModel card, PlayerChoiceContext choiceContext, decimal amount) =>
        FlusteredPower.Gain(choiceContext, card.Owner.Creature, amount, card);
}

// ---- Common ----

/// <summary>才不是胖: gain 8 (11) Block, 恼羞成怒 1.</summary>
public sealed class NotFat() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move), new DynamicVar("Rage", 1m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tsun.RageTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await Tsun.Rage(this, choiceContext, DynamicVars["Rage"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>不许叫我胖: deal 8 (11); if you have a debuff, deal it again.</summary>
public sealed class DontCallMeFat() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];
    protected override bool ShouldGlowGoldInternal => HasDebuff;

    private bool HasDebuff => Owner.Creature.Powers.Any(p => p.Type == PowerType.Debuff);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(HasDebuff ? 2 : 1).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>傲娇嘴甜: apply 1 (2) Weak to all enemies, 恼羞成怒 1 (2).</summary>
public sealed class SweetTalk() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1m), new DynamicVar("Rage", 1m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>(), Tsun.RageTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState.HittableEnemies.ToList())
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars[nameof(WeakPower)].BaseValue, Owner.Creature, this);
        await Tsun.Rage(this, choiceContext, DynamicVars["Rage"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(WeakPower)].UpgradeValueBy(1m);
        DynamicVars["Rage"].UpgradeValueBy(1m);
    }
}

// ---- Uncommon ----

/// <summary>哼！: 恼羞成怒 2 (3), apply 1 Weak to yourself.</summary>
public sealed class Hmph() : DeepSeekWhaleCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Rage", 2m), new PowerVar<WeakPower>(1m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tsun.RageTip, HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Tsun.Rage(this, choiceContext, DynamicVars["Rage"].BaseValue);
        await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, DynamicVars[nameof(WeakPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Rage"].UpgradeValueBy(1m);
}

/// <summary>气鼓鼓: deal 12 (16); 恼羞成怒 adds twice as much to this card.</summary>
public sealed class Puffed() : DeepSeekWhaleCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tsun.RageTip];
    protected override bool ShouldGlowGoldInternal => Owner.Creature.HasPower<FlusteredPower>();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>炸毛: deal 9 (12) to all enemies, then 恼羞成怒 1.</summary>
public sealed class Bristle() : DeepSeekWhaleCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move), new DynamicVar("Rage", 1m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tsun.RageTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await Tsun.Rage(this, choiceContext, DynamicVars["Rage"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>本鲸生气了: whenever you get a debuff or take unblocked attack damage, 恼羞成怒 1. Upgrade: costs 1.</summary>
public sealed class GettingAngry() : SinglePowerCard<GettingAngryPower>(2, CardRarity.Uncommon)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tsun.RageTip];
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

// ---- Rare ----

/// <summary>这是鲸脂！: gain Block equal to 3 x 饱腹 (at least 10); your Block is kept next turn. Upgrade: costs 1.</summary>
public sealed class ThisIsBlubber() : DeepSeekWhaleCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PerFull", 3m), new DynamicVar("Min", 10m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FullnessPower>(), HoverTipFactory.FromPower<BlurPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal block = Math.Max(DynamicVars["Min"].BaseValue, DynamicVars["PerFull"].BaseValue * FullnessPower.Of(Owner.Creature));
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
        await PowerCmd.Apply<BlurPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>记仇本: attacks only use up half of your 恼羞成怒. Upgrade: costs 0.</summary>
public sealed class GrudgeBook() : SinglePowerCard<GrudgeBookPower>(1, CardRarity.Rare)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tsun.RageTip];
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
