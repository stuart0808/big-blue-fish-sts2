using BaseLib.Abstracts;
using BaseLib.Utils;
using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

/// <summary>Shared helpers for the whale's cards and powers.</summary>
public static class Whale
{
    public static Creature? RandomEnemy(Player player, ICombatState combatState) =>
        player.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);

    public static IReadOnlyList<CardModel> Hand(Player player) => PileType.Hand.GetPile(player).Cards;

    public static int CommentsInHand(Player player) => Hand(player).Count(c => c is Comment);

    public static int UpgradedInHand(Player player, CardModel? except = null) =>
        Hand(player).Count(c => c.IsUpgraded && c != except);

    /// <summary>刷屏 X: add X 评论 to the player's hand (the game sends extras to the discard pile when the hand is full).</summary>
    public static async Task AddComments(Player player, ICombatState combatState, int count)
    {
        if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;
        var cards = Enumerable.Range(0, count).Select(_ => (CardModel)combatState.CreateCard<Comment>(player)).ToList();
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player);
    }

    public static Task DamageRandomEnemy(PlayerChoiceContext choiceContext, Player player, ICombatState combatState,
        decimal amount, CardModel? source)
    {
        var target = RandomEnemy(player, combatState);
        return target == null
            ? Task.CompletedTask
            : CreatureCmd.Damage(choiceContext, target, amount, ValueProp.Unpowered, player.Creature, source, null);
    }
}

/// <summary>
/// 评论: 0-cost Ethereal, Exhaust token. Deals 2 damage to a random enemy, +1 for each other 评论
/// in hand. Doubled by 开源权重.
/// </summary>
[Pool(typeof(TokenCardPool))]
public sealed class Comment() : WhaleArtCard(0, CardType.Skill, CardRarity.Token, TargetType.None)
{
    public const int BaseDamage = 2;
    public const int PerOtherComment = 1;

    public override int MaxUpgradeLevel => 0;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust];

    /// <summary>Damage this 评论 deals right now (it has already left the hand when played).</summary>
    public static decimal DamageFor(Player player) =>
        (BaseDamage + PerOtherComment * Whale.CommentsInHand(player)) * (player.Creature.HasPower<OpenWeightsPower>() ? 2 : 1);

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Whale.DamageRandomEnemy(choiceContext, Owner, CombatState, DamageFor(Owner), this);
}

/// <summary>
/// The six options shown by 混合专家. They are never played; the card only shows them in a
/// selection grid and then runs <see cref="Apply"/> on the chosen ones.
/// </summary>
[Pool(typeof(TokenCardPool))]
public abstract class ExpertCard() : WhaleArtCard(0, CardType.Skill, CardRarity.Token, TargetType.None)
{
    public override int MaxUpgradeLevel => 0;

    public abstract Task Apply(PlayerChoiceContext choiceContext, Player player, ICombatState combatState, CardModel source);

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Apply(choiceContext, Owner, CombatState, this);
}

public sealed class ExpertAttack : ExpertCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Unpowered)];

    public override Task Apply(PlayerChoiceContext choiceContext, Player player, ICombatState combatState, CardModel source) =>
        Whale.DamageRandomEnemy(choiceContext, player, combatState, DynamicVars.Damage.BaseValue, source);
}

public sealed class ExpertGuard : ExpertCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Unpowered)];

    public override Task Apply(PlayerChoiceContext choiceContext, Player player, ICombatState combatState, CardModel source) =>
        CreatureCmd.GainBlock(player.Creature, DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
}

public sealed class ExpertScout : ExpertCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override Task Apply(PlayerChoiceContext choiceContext, Player player, ICombatState combatState, CardModel source) =>
        CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, player);
}

public sealed class ExpertEater : ExpertCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Eat", 3m)];

    public override Task Apply(PlayerChoiceContext choiceContext, Player player, ICombatState combatState, CardModel source) =>
        FullnessPower.Eat(choiceContext, player.Creature, DynamicVars["Eat"].BaseValue, source);
}

public sealed class ExpertBreaker : ExpertCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(1m)];

    public override async Task Apply(PlayerChoiceContext choiceContext, Player player, ICombatState combatState, CardModel source)
    {
        if (Whale.RandomEnemy(player, combatState) is { } target)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, DynamicVars[nameof(VulnerablePower)].BaseValue, player.Creature, source);
    }
}

public sealed class ExpertWeakener : ExpertCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1m)];

    public override async Task Apply(PlayerChoiceContext choiceContext, Player player, ICombatState combatState, CardModel source)
    {
        if (Whale.RandomEnemy(player, combatState) is { } target)
            await PowerCmd.Apply<WeakPower>(choiceContext, target, DynamicVars[nameof(WeakPower)].BaseValue, player.Creature, source);
    }
}
