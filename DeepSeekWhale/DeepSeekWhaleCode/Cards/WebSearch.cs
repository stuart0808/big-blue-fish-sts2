using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DeepSeekWhale.DeepSeekWhaleCode.Cards;

/// <summary>联网搜索 (Think): choose a card from the draw pile and put it into hand. Upgrade: costs 0.</summary>
public sealed class WebSearch() : ThinkCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var picked = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(Owner), Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
        if (picked != null) await CardPileCmd.Add(picked, PileType.Hand);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
