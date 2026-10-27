using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace DeepSeekWhale.DeepSeekWhaleCode.Character;

/// <summary>
/// The Architect (end of the run) reads <c>Dialogue.EndAttackers</c> when you proceed, but only the base-game
/// characters have Architect dialogue built in, so a character without any crashed there and the run never
/// ended. Her lines live in localization/*/ancients.json (THE_ARCHITECT.talk.DEEPSEEKWHALE-WHALE_GIRL.*, read by
/// BaseLib); this is the safety net if none were picked: a stand-in so the final attack and the win still happen.
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "WinRun")]
internal static class ArchitectDialoguePatch
{
    [HarmonyPrefix]
    private static void Prefix(TheArchitect __instance)
    {
        var dialogue = Traverse.Create(__instance).Property("Dialogue");
        if (dialogue.GetValue() != null) return;
        MainFile.Logger.Warn("The Architect had no dialogue for this character; using a stand-in so the run can end.");
        dialogue.SetValue(new AncientDialogue("") { EndAttackers = ArchitectAttackers.Both });
    }
}
