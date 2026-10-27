using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace DeepSeekWhale.DeepSeekWhaleCode.Character;

/// <summary>
/// The whale girl's own Spine rigs, used in place of the borrowed Silent ones.
///
/// Rigs are plain Spine files shipped beside the mod DLL in spine/&lt;name&gt;/ (built by
/// tools/make_spine_rig.py): the spine-godot module loads an .atlas and a .spine-json straight
/// from disk, so no Spine editor export or pck import is needed. A missing or broken rig is
/// logged and the borrowed one stays.
/// </summary>
internal static class WhaleGirlSkeleton
{
    public const string CombatRig = "whale_girl";
    public const string RestSiteRig = "whale_girl_rest";

    /// <summary>Name of the SpineSprite in our shop scene, so the shop patch can recognise it.</summary>
    public const string MerchantSpriteName = "WhaleGirlSpine";

    private static readonly Dictionary<string, MegaSkeletonDataResource?> Cache = new();

    /// <summary>Load the rigs up front so a broken file shows in the log at startup, not mid-run.</summary>
    public static void Preload()
    {
        foreach (var rig in new[] { CombatRig, RestSiteRig })
        {
            try { Get(rig); }
            catch (Exception e) { MainFile.Logger.Warn($"Spine rig {rig} preload failed: {e.Message}"); }
        }
    }

    public static MegaSkeletonDataResource? Get(string name)
    {
        if (!Cache.TryGetValue(name, out var rig)) Cache[name] = rig = Load(name);
        return rig;
    }

    private static MegaSkeletonDataResource? Load(string name)
    {
        var modDir = Path.GetDirectoryName(typeof(WhaleGirlSkeleton).Assembly.Location);
        if (modDir == null) return null;
        var dir = Path.Combine(modDir, "spine", name);
        var atlasPath = Path.Combine(dir, $"{name}.atlas");
        var skeletonPath = Path.Combine(dir, $"{name}.spine-json");
        if (!File.Exists(atlasPath) || !File.Exists(skeletonPath))
        {
            MainFile.Logger.Warn($"spine/{name}/ not found beside the mod DLL; keeping the placeholder rig.");
            return null;
        }

        var atlas = ClassDB.Instantiate("SpineAtlasResource").AsGodotObject();
        if (atlas == null || atlas.Call("load_from_atlas_file", atlasPath).AsInt64() != 0)
        {
            MainFile.Logger.Warn($"Spine atlas failed to load: {atlasPath}");
            return null;
        }
        var skeletonFile = ClassDB.Instantiate("SpineSkeletonFileResource").AsGodotObject();
        if (skeletonFile == null || skeletonFile.Call("load_from_file", skeletonPath).AsInt64() != 0)
        {
            MainFile.Logger.Warn($"Spine skeleton failed to load: {skeletonPath}");
            return null;
        }
        var data = ClassDB.Instantiate("SpineSkeletonDataResource").AsGodotObject();
        if (data == null) return null;
        data.Set("atlas_res", atlas);
        data.Set("skeleton_file_res", skeletonFile);
        MainFile.Logger.Info($"Loaded Spine rig {name} from {dir}");
        return new MegaSkeletonDataResource(data);
    }

    /// <summary>Point a SpineSprite node at one of our rigs. Never throws.</summary>
    public static bool Apply(Node? node, string rigName, string? startAnimation)
    {
        try
        {
            if (node == null || node.GetClass() != "SpineSprite" || Get(rigName) is not { } rig) return false;
            var sprite = new MegaSprite(node);
            sprite.SetSkeletonDataRes(rig);
            if (startAnimation != null)
            {
                try { sprite.GetAnimationState()?.SetAnimation(startAnimation, true, 0); }
                catch { /* the owning node starts its own animation */ }
            }
            return true;
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Applying Spine rig {rigName} failed, keeping the placeholder: {e.Message}");
            return false;
        }
    }
}

/// <summary>
/// Swap the body the Silent visuals scene was built with. Patched on the character (not the creature)
/// so every place that builds her figure gets it: combat, the fake merchant event, game over, unlocks.
/// </summary>
[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.CreateVisuals))]
internal static class WhaleGirlCombatRigPatch
{
    [HarmonyPostfix]
    private static void Postfix(CharacterModel __instance, NCreatureVisuals? __result)
    {
        if (__result == null || __instance is not WhaleGirl) return;
        WhaleGirlSkeleton.Apply(__result.GetNodeOrNull("%Visuals"), WhaleGirlSkeleton.CombatRig, "idle_loop");
    }
}

/// <summary>
/// Shop: our merchant scene (scenes/whale_girl_merchant.tscn) carries a placeholder child named
/// <see cref="WhaleGirlSkeleton.MerchantSpriteName"/>. BaseLib's scene auto-conversion turns that
/// SpineSprite into a plain Node2D, so a real SpineSprite is created here and put in its place
/// (the shop node animates child 0) before the node starts its animation.
/// </summary>
[HarmonyPatch(typeof(NMerchantCharacter), nameof(NMerchantCharacter._Ready))]
internal static class WhaleGirlMerchantRigPatch
{
    // Must run before BaseLib's own _Ready prefix, which skips nodes whose child 0 is not a SpineSprite.
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    private static void Prefix(NMerchantCharacter __instance)
    {
        try
        {
            var first = __instance.GetChildCount() > 0 ? __instance.GetChild(0) : null;
            MainFile.Logger.Info($"Shop character ready: {__instance.GetChildCount()} children, " +
                                 $"child 0 = {first?.GetClass() ?? "none"} '{first?.Name}'");
            // Base-game shop characters always have a SpineSprite as child 0. BaseLib's conversion of our
            // scene instead nests our whole scene root (a plain Node2D wrapping the sprite) as child 0,
            // so replace that wrapper with a real SpineSprite carrying the combat rig.
            if (first is not Node2D wrapper || first.GetClass() == "SpineSprite") return;
            var inner = wrapper.FindChild(WhaleGirlSkeleton.MerchantSpriteName, true, false) as Node2D;
            if (inner == null && wrapper.Name.ToString() != WhaleGirlSkeleton.MerchantSpriteName) return;
            if (ClassDB.Instantiate("SpineSprite").AsGodotObject() is not Node2D sprite) return;

            sprite.Name = WhaleGirlSkeleton.MerchantSpriteName;
            sprite.Position = wrapper.Position + (inner?.Position ?? Vector2.Zero);
            sprite.Scale = wrapper.Scale * (inner?.Scale ?? Vector2.One);
            __instance.RemoveChild(wrapper);
            wrapper.QueueFree();
            __instance.AddChild(sprite);
            __instance.MoveChild(sprite, 0);
            MainFile.Logger.Info($"Shop: replaced '{wrapper.Name}' with a SpineSprite (scale {sprite.Scale})");
            WhaleGirlSkeleton.Apply(sprite, WhaleGirlSkeleton.CombatRig, null);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Shop rig setup failed: {e.Message}");
        }
    }
}

/// <summary>Rest site: the borrowed Silent campfire scene gets the sitting rig.</summary>
[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
internal static class WhaleGirlRestSiteRigPatch
{
    // Placement of the sitting rig inside the Silent rest-site scene (its local units).
    private static readonly Vector2 SpritePosition = new(40, 270);
    private static readonly Vector2 SpriteScale = Vector2.One;

    [HarmonyPrefix]
    private static void Prefix(NRestSiteCharacter __instance)
    {
        if (__instance.Player?.Character is not WhaleGirl) return;
        foreach (var child in __instance.GetChildren())
        {
            if (child is not Node2D node || node.GetClass() != "SpineSprite") continue;
            if (!WhaleGirlSkeleton.Apply(node, WhaleGirlSkeleton.RestSiteRig, null)) continue;
            node.Position = SpritePosition;
            node.Scale = new Vector2(Mathf.Sign(node.Scale.X) * SpriteScale.X, SpriteScale.Y);
        }
    }
}
