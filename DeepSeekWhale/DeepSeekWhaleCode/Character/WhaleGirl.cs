using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using DeepSeekWhale.DeepSeekWhaleCode.Cards;
using DeepSeekWhale.DeepSeekWhaleCode.Extensions;
using DeepSeekWhale.DeepSeekWhaleCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace DeepSeekWhale.DeepSeekWhaleCode.Character;

/// <summary>
/// The DeepSeek whale girl. Combat, rest-site and merchant visuals are our own Spine rigs
/// (WhaleGirlSkeleton); <see cref="PlaceholderID"/> only covers what BaseLib has no hook for.
/// </summary>
public class WhaleGirl : PlaceholderCharacterModel
{
    public const string CharacterId = "WhaleGirl";

    public static readonly Color Color = new("4D6BFE");

    public override string PlaceholderID => "silent";

    // The Silent's card trail and character-select wipe are green; the Defect's blue ones suit her.
    public override string CustomTrailPath => SceneHelper.GetScenePath("vfx/card_trail_defect");
    public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/defect_transition_mat.tres";

    public override Color NameColor => Color;
    public override Color MapDrawingColor => Color;
    public override Color RemoteTargetingLineColor => Color;
    public override Color RemoteTargetingLineOutline => new("1B2A6B");
    public override Color EnergyLabelOutlineColor => new("1B2A6B");
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 72;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeWhale>(),
        ModelDb.Card<StrikeWhale>(),
        ModelDb.Card<StrikeWhale>(),
        ModelDb.Card<StrikeWhale>(),
        ModelDb.Card<DefendWhale>(),
        ModelDb.Card<DefendWhale>(),
        ModelDb.Card<DefendWhale>(),
        ModelDb.Card<DefendWhale>(),
        ModelDb.Card<RiceBowl>(),
        ModelDb.Card<GetToWork>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<RiceCooker>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<WhaleGirlCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<WhaleGirlRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<WhaleGirlPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomCharacterSelectBg => "scenes/char_select_bg_whale_girl.tscn".ResourcePath();
    // An empty SpineSprite that WhaleGirlMerchantRigPatch fills with the combat rig.
    public override string CustomMerchantAnimPath => "scenes/whale_girl_merchant.tscn".ResourcePath();

    // Our own five-layer energy orb (tools/make_energy_orb.py) instead of the Silent's scene.
    public override string CustomEnergyCounterPath => null!;
    public override CustomEnergyCounter? CustomEnergyCounter =>
        new(layer => $"charui/energy/layer_{layer}.png".ImagePath(), new Color("1B2A6B"), new Color("6ED2FF"));
    public override string CustomIconTexturePath => "character_icon.png".CharacterUiPath();
    public override string CustomIconOutlineTexturePath => "character_icon_outline.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker.png".CharacterUiPath();

    // Treasure-room hands (pointing at a relic, and the multiplayer rock-paper-scissors for it).
    public override string CustomArmPointingTexturePath => "hands/hand_point.png".CharacterUiPath();
    public override string CustomArmRockTexturePath => "hands/hand_rock.png".CharacterUiPath();
    public override string CustomArmPaperTexturePath => "hands/hand_paper.png".CharacterUiPath();
    public override string CustomArmScissorsTexturePath => "hands/hand_scissors.png".CharacterUiPath();
}
