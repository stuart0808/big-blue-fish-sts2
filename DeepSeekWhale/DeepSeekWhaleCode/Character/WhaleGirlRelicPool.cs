using BaseLib.Abstracts;
using DeepSeekWhale.DeepSeekWhaleCode.Extensions;
using Godot;

namespace DeepSeekWhale.DeepSeekWhaleCode.Character;

public class WhaleGirlRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => WhaleGirl.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}