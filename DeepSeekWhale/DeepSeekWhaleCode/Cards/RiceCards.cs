using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

// 🍚 干饭流: build 饱腹 (Fullness) with 吃白饭, cash it in with 开工, and get stronger while 吃饱.

/// <summary>Shared bits of the eat archetype.</summary>
internal static class Rice
{
    public static IHoverTip FullnessTip => HoverTipFactory.FromPower<FullnessPower>();

    public static bool IsFull(CardModel card) => FullnessPower.IsFull(card.Owner.Creature);

    /// <summary>Hits of a "开工 全部" multi-hit card: 1, plus 1 per 2 Fullness spent.</summary>
    public static int HitsFor(int spent) => 1 + spent / 2;
}

// ---- Basic ----

/// <summary>吃白饭: gain 5 (8) Block, 吃白饭 3.</summary>
public sealed class RiceBowl() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move), new DynamicVar("Eat", 3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await FullnessPower.Eat(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>开工: deal 6, +3 per Fullness spent; 开工 3 (4).</summary>
public sealed class GetToWork() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy), IWorkCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Work", 3m),
        new CalculationBaseVar(6m),
        new ExtraDamageVar(3m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) =>
                FullnessPower.Spendable(card.Owner.Creature, card.DynamicVars["Work"].IntValue)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
        await FullnessPower.Work(choiceContext, Owner.Creature, DynamicVars["Work"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Work"].UpgradeValueBy(1m);
}

// ---- Common ----

/// <summary>加一碗: 吃白饭 2 (3), draw 1.</summary>
public sealed class AnotherBowl() : DeepSeekWhaleCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Eat", 2m), new CardsVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FullnessPower.Eat(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, this);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars["Eat"].UpgradeValueBy(1m);
}

/// <summary>干饭人: deal 8 (11), 吃白饭 2.</summary>
public sealed class Foodie() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move), new DynamicVar("Eat", 2m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_bite", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        await FullnessPower.Eat(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>饭后甜点: gain 7 (10) Block; while Full, 4 (5) more.</summary>
public sealed class Dessert() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move), new DynamicVar("FullBonus", 4m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];
    protected override bool ShouldGlowGoldInternal => Rice.IsFull(this);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (Rice.IsFull(this))
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["FullBonus"].BaseValue, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["FullBonus"].UpgradeValueBy(1m);
    }
}

/// <summary>红烧肉优惠: 吃白饭 4, draw 1. Upgrade: costs 0.</summary>
public sealed class BraisedPork() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Eat", 4m), new CardsVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FullnessPower.Eat(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, this);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>一口闷: 开工 全部 — deal 4 (5), once more per 2 Fullness spent.</summary>
public sealed class BottomsUp() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy), IWorkCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        int spent = await FullnessPower.Work(choiceContext, Owner.Creature, FullnessPower.All);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(Rice.HitsFor(spent)).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}

/// <summary>饭点到了: deal 9 (11); while Full, 4 (5) more.</summary>
public sealed class MealTime() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move), new ExtraDamageVar(4m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];
    protected override bool ShouldGlowGoldInternal => Rice.IsFull(this);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        decimal damage = DynamicVars.Damage.BaseValue + (Rice.IsFull(this) ? DynamicVars.ExtraDamage.BaseValue : 0);
        await DamageCmd.Attack(damage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}

/// <summary>筷子连击: deal 4 (5) twice; while Full, three times.</summary>
public sealed class Chopsticks() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];
    protected override bool ShouldGlowGoldInternal => Rice.IsFull(this);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(Rice.IsFull(this) ? 3 : 2).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}

// ---- Uncommon ----

/// <summary>吃饱了才干活: deal 14 (18); while Full, twice.</summary>
public sealed class WorkWhenFull() : DeepSeekWhaleCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];
    protected override bool ShouldGlowGoldInternal => Rice.IsFull(this);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(Rice.IsFull(this) ? 2 : 1).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>白饭加特林: 开工 全部 — deal 4 (5) to all enemies, once more per 2 Fullness spent.</summary>
public sealed class RiceGatling() : DeepSeekWhaleCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies), IWorkCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int spent = await FullnessPower.Work(choiceContext, Owner.Creature, FullnessPower.All);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(Rice.HitsFor(spent)).FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}

/// <summary>外卖到了: next turn, 吃白饭 5 (7) and draw 2.</summary>
public sealed class TakeoutArrived() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Eat", 5m), new CardsVar(TakeoutPower.Draw)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PowerCmd.Apply<TakeoutPower>(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Eat"].UpgradeValueBy(2m);
}

// ---- Rare ----

/// <summary>鲸吞: deal 15 (18). Fatal: 吃白饭 5 and gain 3 (4) max HP. Exhaust.</summary>
public sealed class Gulp() : DeepSeekWhaleCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override bool CanBeGeneratedInCombat => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(15m, ValueProp.Move), new MaxHpVar(3m), new DynamicVar("Eat", 5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Fatal), Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        bool fatalCounts = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_bite", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        if (fatalCounts && attack.Results.SelectMany(r => r).Any(r => r.WasTargetKilled))
        {
            await FullnessPower.Eat(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, this);
            await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.MaxHp.UpgradeValueBy(1m);
    }
}

/// <summary>满汉全席: 吃白饭 up to the cap; 开工 cards cost 1 less this turn. Exhaust. Upgrade: costs 0.</summary>
public sealed class Feast() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        int missing = FullnessPower.Cap(creature) - FullnessPower.Of(creature);
        await FullnessPower.Eat(choiceContext, creature, missing, this);
        await PowerCmd.Apply<FeastPower>(choiceContext, creature, 1, creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

// ---- Added with the full pool ----

/// <summary>打包带走: 开工 全部 — gain 3 (4) Block per 饱腹 spent.</summary>
public sealed class DoggyBag() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IWorkCard
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PerFull", 3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];
    protected override bool ShouldGlowGoldInternal => FullnessPower.Of(Owner.Creature) > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int spent = await FullnessPower.Work(choiceContext, Owner.Creature, FullnessPower.All);
        if (spent > 0)
            await CreatureCmd.GainBlock(Owner.Creature, spent * DynamicVars["PerFull"].BaseValue, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars["PerFull"].UpgradeValueBy(1m);
}

/// <summary>大胃王: every 吃白饭 eats 1 more. Upgrade: costs 0.</summary>
public sealed class BigEater() : SinglePowerCard<BigEaterPower>(1, CardRarity.Uncommon)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>干饭王: whenever you 吃白饭, deal that much damage to a random enemy. Upgrade: costs 1.</summary>
public sealed class RiceKing() : SinglePowerCard<RiceKingPower>(2, CardRarity.Rare)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Rice.FullnessTip];
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
