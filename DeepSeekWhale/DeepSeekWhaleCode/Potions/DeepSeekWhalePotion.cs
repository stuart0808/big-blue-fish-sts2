using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using DeepSeekWhale.DeepSeekWhaleCode.Character;
using DeepSeekWhale.DeepSeekWhaleCode.Extensions;

namespace DeepSeekWhale.DeepSeekWhaleCode.Potions;

[Pool(typeof(WhaleGirlPotionPool))]
public abstract class DeepSeekWhalePotion : CustomPotionModel
{
	public override string? CustomPackedImagePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
	public override string? CustomPackedOutlinePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}