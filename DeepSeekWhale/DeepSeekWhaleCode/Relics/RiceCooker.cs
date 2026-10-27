using DeepSeekWhale.DeepSeekWhaleCode.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace DeepSeekWhale.DeepSeekWhaleCode.Relics;

/// <summary>
/// 电饭煲 and its upgrade: at the start of each combat, 吃白饭 <see cref="StartEat"/>. At the end of
/// your turn, 吃白饭 2 for each unspent energy — the 摸鱼 (slacking off) core of the character.
/// </summary>
public abstract class RiceCookerBase : DeepSeekWhaleRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected abstract decimal StartEat { get; }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Eat", StartEat), new DynamicVar("PerEnergy", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FullnessPower>()];

    public override async Task BeforeCombatStart()
    {
        Flash();
        await FullnessPower.Eat(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["Eat"].BaseValue, null);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner.Creature)) return;
        int energy = Owner.PlayerCombatState?.Energy ?? 0;
        if (energy <= 0) return;
        Flash();
        for (int i = 0; i < Slack.Times(Owner.Creature); i++)
            await FullnessPower.Eat(choiceContext, Owner.Creature, energy * DynamicVars["PerEnergy"].BaseValue, null);
    }
}

/// <summary>电饭煲 (starter).</summary>
public sealed class RiceCooker : RiceCookerBase
{
    protected override decimal StartEat => 2m;
}

/// <summary>满血电饭煲: what 欧洛巴斯之触 turns 电饭煲 into (吃白饭 4 at combat start).</summary>
public sealed class FullRiceCooker : RiceCookerBase
{
    protected override decimal StartEat => 4m;
}

/// <summary>欧洛巴斯之触 only knows the vanilla starter relics (others become a useless 头环): upgrade 电饭煲 properly.</summary>
[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
internal static class TouchOfOrobasRiceCookerPatch
{
    [HarmonyPrefix]
    private static bool Prefix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is not RiceCooker) return true;
        __result = ModelDb.Relic<FullRiceCooker>().ToMutable();
        return false;
    }
}
