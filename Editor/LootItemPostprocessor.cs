using UnityEditor;
using UnityEngine;

public class LootItemPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var assetPath in importedAssets)
        {
            if (assetPath.Contains("Resources/Items"))
            {
                LootConfig config = Resources.Load<LootConfig>("LootConfig");
                if (config != null)
                {
                    config.items.Clear();
                    config.items.AddRange(Resources.LoadAll<LootItem>("Items"));
                    EditorUtility.SetDirty(config);
                    Debug.Log("LootConfig updated with new items.");
                }
            }
        }
    }
}