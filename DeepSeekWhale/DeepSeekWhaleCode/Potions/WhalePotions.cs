using DeepSeekWhale.DeepSeekWhaleCode.Cards;
using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DeepSeekWhale.DeepSeekWhaleCode.Potions;

/// <summary>一碗白饭 (common): 吃白饭 6.</summary>
public sealed class RiceBowlPotion : DeepSeekWhalePotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Eat", 6m)];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FullnessPower>()];

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target) =>
        FullnessPower.Eat(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, null);
}

/// <summary>外卖 (uncommon): heal 10 and 吃白饭 3.</summary>
public sealed class TakeoutPotion : DeepSeekWhalePotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(10m), new DynamicVar("Eat", 3m)];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FullnessPower>()];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars["Heal"].BaseValue);
        await FullnessPower.Eat(choiceContext, Owner.Creature, DynamicVars["Eat"].BaseValue, null);
    }
}

/// <summary>评论机器人 (uncommon): 刷屏 5.</summary>
public sealed class CommentBotPotion : DeepSeekWhalePotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5)];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Comment>()];

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target) =>
        Owner.Creature.CombatState is { } combatState
            ? Whale.AddComments(Owner, combatState, DynamicVars.Cards.IntValue)
            : Task.CompletedTask;
}
