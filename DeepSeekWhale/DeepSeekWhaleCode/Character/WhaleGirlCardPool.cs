using BaseLib.Abstracts;
using DeepSeekWhale.DeepSeekWhaleCode.Extensions;
using Godot;

namespace DeepSeekWhale.DeepSeekWhaleCode.Character;

public class WhaleGirlCardPool : CustomCardPoolModel
{
    public override string Title => WhaleGirl.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // The card frame is the base game's frame tinted by an HSV shader built from this colour.
    public override Color ShaderColor => WhaleGirl.Color;

    //Color of small card icons
    public override Color DeckEntryCardColor => WhaleGirl.Color;

    public override bool IsColorless => false;
}
