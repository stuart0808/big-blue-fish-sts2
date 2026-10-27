using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using DeepSeekWhale.DeepSeekWhaleCode.Character;
using DeepSeekWhale.DeepSeekWhaleCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

/// <summary>
/// Loads card art from this mod's resources, keyed by the card's class name in snake_case
/// (images/card_portraits/big/strike_whale.png and the small card_portraits/strike_whale.png).
/// Token cards inherit this directly; pool cards go through <see cref="DeepSeekWhaleCard"/>.
/// </summary>
public abstract class WhaleArtCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Normal art: 1000x760 (500x380 also works, it is scaled). Small variant: 250x190.
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
}

/// <summary>Base class for the Big Blue Fish's own cards; puts every subclass in her card pool.</summary>
[Pool(typeof(WhaleGirlCardPool))]
public abstract class DeepSeekWhaleCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    WhaleArtCard(cost, type, rarity, target);
