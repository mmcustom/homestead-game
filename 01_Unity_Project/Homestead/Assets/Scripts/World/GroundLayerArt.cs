using UnityEngine;

// Art for the terrain layers the game paints at run time (Art_Needed_List.md: world_ground, 2026-11-19): snow and leaf
// litter (SeasonalGround), wet clay and creek sand (CreekBanks). Each is a tileable set in Resources/Art named
// ground_<id>_basecolor, _normal and _mask (R metallic 0, G AO, B height for the terrain's height blend, A smoothness).
// Without the art the callers keep their own generated textures. Art textures are assets: never Destroy them.
public static class GroundLayerArt
{
    public const string Snow = "snow", LeafLitter = "leaf_litter", WetClay = "wet_clay", CreekSand = "creek_sand";
    const string Prefix = "ground_";

    // Puts the art on the layer (keeping its name and tile size). False, and the layer untouched, when there is no art.
    public static bool Apply(TerrainLayer layer, string id)
    {
        if (layer == null)
            return false;
        var baseColour = Resources.Load<Texture2D>($"Art/{Prefix}{id}_basecolor");
        if (baseColour == null)
            return false;
        layer.diffuseTexture = baseColour;
        layer.normalMapTexture = Resources.Load<Texture2D>($"Art/{Prefix}{id}_normal");
        layer.maskMapTexture = Resources.Load<Texture2D>($"Art/{Prefix}{id}_mask");
        layer.normalScale = 1f;
        layer.metallic = 0f;
        layer.smoothness = 1f; // the mask's alpha carries it
        layer.diffuseRemapMin = Vector4.zero;
        layer.diffuseRemapMax = Vector4.one;
        layer.maskMapRemapMin = Vector4.zero;
        layer.maskMapRemapMax = Vector4.one;
        return true;
    }

    // True for a texture loaded by Apply (an asset, which must not be destroyed).
    public static bool IsArt(Texture texture) => texture != null && texture.name.StartsWith(Prefix);
}
