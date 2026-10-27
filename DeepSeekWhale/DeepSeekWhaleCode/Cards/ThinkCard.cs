using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

/// <summary>
/// 思考 (Think): the card has Retain, and if it is still in hand at the end of the turn it
/// upgrades itself for the rest of the combat.
/// </summary>
public abstract class ThinkCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    DeepSeekWhaleCard(cost, type, rarity, target)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    public override bool HasTurnEndInHandEffect => true;

    protected override Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        if (IsUpgradable) CardCmd.Upgrade(this);
        return Task.CompletedTask;
    }
}
