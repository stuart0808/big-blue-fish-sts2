using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

// Rare attacks and skills added in v1.0 (the rare powers are in PowerCards.cs).

/// <summary>长考 (Think): deal 28 (38).</summary>
public sealed class LongThink() : ThinkCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(28m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(10m);
}

/// <summary>宕机重启: exhaust every Server Busy you have; deal 7 (10) to all enemies, once more per card exhausted.</summary>
public sealed class Reboot() : DeepSeekWhaleCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<ServerBusy>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var busy = CardPile.GetCards(Owner, PileType.Hand, PileType.Draw, PileType.Discard).OfType<ServerBusy>().ToList();
        foreach (var card in busy) await CardCmd.Exhaust(choiceContext, card);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)
            .WithHitCount(1 + busy.Count)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>顿悟时刻: draw 3 (4) cards and upgrade them for this combat. Exhaust.</summary>
public sealed class AhaMoment() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawn = await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        foreach (var card in drawn.Where(c => c.IsUpgradable).ToList()) CardCmd.Upgrade(card);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>价格屠夫: a card in your hand costs 0 for the rest of combat. Exhaust. Upgrade: costs 0.</summary>
public sealed class PriceButcher() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var picked = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => c != this, this)).FirstOrDefault();
        picked?.EnergyCost.SetThisCombat(0);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>蒸馏传授 (multiplayer only): give an ally a 0-cost, Exhaust copy of a card in your hand. Upgrade: costs 0.</summary>
public sealed class TeachDistill() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target?.Player is not { } ally || ally == Owner) return;
        var picked = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => c != this, this)).FirstOrDefault();
        if (picked == null) return;

        var copy = picked.CreateClone();
        copy.EnergyCost.SetThisCombat(0);
        CardCmd.ApplyKeyword(copy, CardKeyword.Exhaust);
        await CardPileCmd.GiveToAnotherPlayer(copy, ally, PileType.Hand, clonedBy: this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>一起干饭 (multiplayer only): every player gains 1 (2) energy. Exhaust.</summary>
public sealed class GroupBreath() : DeepSeekWhaleCard(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var players = CombatState.GetTeammatesOf(Owner.Creature).Where(c => c is { IsAlive: true, IsPlayer: true });
        foreach (var creature in players)
            if (creature.Player is { } player)
                await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, player);
    }

    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1m);
}
