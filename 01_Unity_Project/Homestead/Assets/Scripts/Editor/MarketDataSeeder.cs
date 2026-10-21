using System.IO;
using UnityEditor;
using UnityEngine;

// Homestead > Market > Seed MarketData: creates Resources/Market/MarketData.asset (or refills it) from MarketCatalog's default rows and
// groups, which were generated from the baseline TradingPost price tables, so no price is ever retyped. Run it once; after that the asset
// is the thing to tune. Running it again RESETS the asset to the defaults, so it asks first when the asset already has rows.
public static class MarketDataSeeder
{
    const string Folder = "Assets/Resources/Market";
    const string AssetPath = Folder + "/MarketData.asset";

    [MenuItem("Homestead/Market/Seed MarketData")]
    public static void Seed()
    {
        MarketData asset = AssetDatabase.LoadAssetAtPath<MarketData>(AssetPath);
        if (asset != null && asset.rows != null && asset.rows.Length > 0 && !Application.isBatchMode &&
            !EditorUtility.DisplayDialog("Seed MarketData", "MarketData already has rows. Reset every price and group rule to the defaults?", "Reset", "Cancel"))
            return;

        if (asset == null)
        {
            Directory.CreateDirectory(Folder);
            asset = ScriptableObject.CreateInstance<MarketData>();
            AssetDatabase.CreateAsset(asset, AssetPath);
        }

        asset.Seed();
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Market] Seeded {AssetPath}: {asset.rows.Length} rows, {asset.groups.Length} groups.");
    }
}
