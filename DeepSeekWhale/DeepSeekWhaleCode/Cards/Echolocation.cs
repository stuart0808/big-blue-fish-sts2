using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

/// <summary>回声定位 (Think): look at the top 3 (4) cards of the draw pile, put 1 into hand, discard the rest.</summary>
public sealed class Echolocation() : ThinkCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);
        var looked = PileType.Draw.GetPile(Owner).Cards.Take(DynamicVars.Cards.IntValue).ToList();
        if (looked.Count == 0) return;

        var picked = (await CardSelectCmd.FromSimpleGrid(choiceContext, looked, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
        if (picked != null) await CardPileCmd.Add(picked, PileType.Hand);
        var rest = looked.Where(c => c != picked).ToList();
        if (rest.Count > 0) await CardPileCmd.Add(rest, PileType.Discard);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
