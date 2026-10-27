using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

/// <summary>
/// 服务器繁忙: Unplayable, Ethereal status card made by R1满血版. Lives in the base game's status
/// pool, not the whale's, so it can never show up as a card reward.
/// </summary>
[Pool(typeof(StatusCardPool))]
public sealed class ServerBusy() : WhaleArtCard(-1, CardType.Status, CardRarity.Status, TargetType.None)
{
    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];
}
