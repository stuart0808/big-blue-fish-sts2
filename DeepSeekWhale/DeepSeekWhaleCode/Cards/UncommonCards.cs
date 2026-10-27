using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

// Uncommon attacks and skills added in v1.0 (the uncommon powers are in PowerCards.cs).

// ---- 🌊 Deep ----

// ---- 💨 Breath ----

// ---- 🧠 Think ----

/// <summary>慢思考 (Think): deal 14 (20).</summary>
public sealed class SlowThinking() : ThinkCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6m);
}

/// <summary>推理模型: deal 6, +3 (4) per other upgraded card in hand.</summary>
public sealed class Reasoner() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(6m),
        new ExtraDamageVar(3m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) => Whale.UpgradedInHand(card.Owner, except: card)),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(1m);
}

/// <summary>反思: upgrade every card in your hand for this combat. Exhaust. Upgrade: costs 0.</summary>
public sealed class Reflection() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = Whale.Hand(Owner).Where(c => c.IsUpgradable).ToList();
        if (cards.Count > 0) CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>少样本学习 (Think): draw 2 (3).</summary>
public sealed class FewShot() : ThinkCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

// ---- 💬 Comments / AI memes ----

/// <summary>一本正经: deal 8 (11), 刷屏 1.</summary>
public sealed class Confident() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move), new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Comment>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await Whale.AddComments(Owner, CombatState, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>混合专家: pick 2 of 3 (4) random experts; their effects happen right away.</summary>
public sealed class MixtureOfExperts() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public const int Picks = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Options", 3m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var factories = new List<Func<CardModel>>
        {
            () => CombatState.CreateCard<ExpertAttack>(Owner),
            () => CombatState.CreateCard<ExpertGuard>(Owner),
            () => CombatState.CreateCard<ExpertScout>(Owner),
            () => CombatState.CreateCard<ExpertEater>(Owner),
            () => CombatState.CreateCard<ExpertBreaker>(Owner),
            () => CombatState.CreateCard<ExpertWeakener>(Owner),
        };
        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var options = new List<CardModel>();
        for (int i = 0; i < DynamicVars["Options"].IntValue && factories.Count > 0; i++)
        {
            var factory = rng.NextItem(factories)!;
            factories.Remove(factory);
            options.Add(factory());
        }

        var chosen = await CardSelectCmd.FromSimpleGrid(choiceContext, options, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, Picks));
        foreach (var expert in chosen.OfType<ExpertCard>())
            await expert.Apply(choiceContext, Owner, CombatState, this);
    }

    protected override void OnUpgrade() => DynamicVars["Options"].UpgradeValueBy(1m);
}

/// <summary>知识蒸馏: exhaust a card in hand; add a copy that costs 0 and has Ethereal and Exhaust. Upgrade: costs 0.</summary>
public sealed class Distillation() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var picked = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => c != this, this)).FirstOrDefault();
        if (picked == null) return;

        var distilled = picked.CreateClone();
        distilled.EnergyCost.SetThisCombat(0);
        CardCmd.ApplyKeyword(distilled, CardKeyword.Ethereal, CardKeyword.Exhaust);
        await CardCmd.Exhaust(choiceContext, picked);
        await CardPileCmd.AddGeneratedCardToCombat(distilled, PileType.Hand, Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>服务器扩容: exhaust every status card and 评论 in hand; draw 1 and gain 3 (5) Block for each.</summary>
public sealed class ScaleUp() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3m, ValueProp.Unpowered)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<Comment>(), HoverTipFactory.FromCard<ServerBusy>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var fuel = Whale.Hand(Owner).Where(c => c != this && (c.Type == CardType.Status || c is Comment)).ToList();
        if (fuel.Count == 0) return;
        foreach (var card in fuel) await CardCmd.Exhaust(choiceContext, card);
        await CardPileCmd.Draw(choiceContext, fuel.Count, Owner);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue * fuel.Count, ValueProp.Unpowered, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>降本增效: exhaust a card in hand, gain 1 (2) energy.</summary>
public sealed class CostCut() : DeepSeekWhaleCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var picked = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => c != this, this)).FirstOrDefault();
        if (picked == null) return;
        await CardCmd.Exhaust(choiceContext, picked);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1m);
}

// ---- ⚙️ General ----

/// <summary>注意力机制: deal 5 (7) and apply 1 Vulnerable (2 if the target is already Vulnerable).</summary>
public sealed class Attention() : DeepSeekWhaleCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5m, ValueProp.Move), new PowerVar<VulnerablePower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        bool wasVulnerable = cardPlay.Target.HasPower<VulnerablePower>();
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (!cardPlay.Target.IsAlive) return;
        decimal amount = DynamicVars[nameof(VulnerablePower)].BaseValue * (wasVulnerable ? 2 : 1);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, amount, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>联想: choose 1 of 3 random Big Blue Fish cards; it is free this turn. Exhaust. Upgrade: no Exhaust.</summary>
public sealed class Associate() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pool = Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint);
        var cards = CardFactory.GetDistinctForCombat(Owner, pool, 3, Owner.RunState.Rng.CombatCardGeneration).ToList();
        var picked = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, Owner, canSkip: true);
        if (picked == null) return;
        picked.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(picked, PileType.Hand, Owner);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
