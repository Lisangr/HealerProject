using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Config", menuName = "Loot Config", order = 56)]
public class LootConfig : ScriptableObject
{
    public List<LootItem> items = new List<LootItem>();

    [ContextMenu("Reload Items from Resources")]
    public void ReloadItemsFromResources()
    {
        LootItem[] loadedItems = Resources.LoadAll<LootItem>("Loot"); // изменено путь с "Items" на "Loot"
        items.Clear();
        items.AddRange(loadedItems);
        EditorUtility.SetDirty(this);
        Debug.Log($"Loaded {loadedItems.Length} items from Resources/Loot");
    }

    [ContextMenu("Export to CSV")]
    public void ExportToCSV()
    {
        string path = EditorUtility.SaveFilePanel("Export to CSV", "", "loot_items.csv", "csv");
        if (string.IsNullOrEmpty(path))
            return;

        using (var writer = new StreamWriter(path))
        {
            writer.WriteLine("ItemName,Category,SpawnChance,Defense,Description");
            foreach (var item in items)
            {
                if (item.data == null || item.data.sourceItem == null)
                    continue;

                var src = item.data.sourceItem;
                writer.WriteLine($"{src.itemName},{item.data.category},{item.data.spawnChance},{item.data.defense},\"{item.data.description}\"");
            }
        }
        Debug.Log($"Exported {items.Count} items to {path}");
    }

    [ContextMenu("Import from CSV")]
    public void ImportFromCSV()
    {
        string path = EditorUtility.OpenFilePanel("Import from CSV", "", "csv");
        if (string.IsNullOrEmpty(path))
            return;

        string itemsPath = "Assets/Resources/Items";
        string lootPath = "Assets/Resources/Loot";

        Directory.CreateDirectory(itemsPath);
        Directory.CreateDirectory(lootPath);

        using (var reader = new StreamReader(path))
        {
            string header = reader.ReadLine();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(',');
                if (values.Length < 5)
                    continue;

                string itemName = values[0];
                string category = values[1];
                float spawnChance = float.Parse(values[2]);
                int defense = int.Parse(values[3]);
                string description = values[4].Trim('"');

                // === Создаем Item ===
                Item item = ScriptableObject.CreateInstance<Item>();
                item.itemName = itemName;

                string itemAssetPath = Path.Combine(itemsPath, $"{itemName}.asset");
                AssetDatabase.CreateAsset(item, itemAssetPath);

                // === Создаем LootItem, ссылающийся на Item ===
                LootItem lootItem = ScriptableObject.CreateInstance<LootItem>();
                lootItem.data = new LootData
                {
                    sourceItem = item,
                    spawnChance = spawnChance,
                    category = category,
                    defense = defense,
                    description = description
                };

                string lootAssetPath = Path.Combine(lootPath, $"{itemName}_Loot.asset");
                AssetDatabase.CreateAsset(lootItem, lootAssetPath);

                items.Add(lootItem);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Import completed!");
    }
}
