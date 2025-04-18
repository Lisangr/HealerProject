using UnityEngine;
using UnityEditor;
using System.IO;

public class LootItemCreator : EditorWindow
{
    [MenuItem("Tools/Create Loot Items")]
    static void Init()
    {
        CreateLootItems();
    }

    static void CreateLootItems()
    {
        string resourcesPath = "Assets/Resources";
        string iconsPath = Path.Combine(resourcesPath, "Icons");
        string itemsPath = Path.Combine(resourcesPath, "Items");
        string lootPath = Path.Combine(resourcesPath, "Loot");

        // ������� ����� ���� �� ���
        if (!AssetDatabase.IsValidFolder(resourcesPath))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(itemsPath))
            AssetDatabase.CreateFolder(resourcesPath, "Items");
        if (!AssetDatabase.IsValidFolder(lootPath))
            AssetDatabase.CreateFolder(resourcesPath, "Loot");

        // �������� ��� ������� �� ����� Icons
        string[] spriteGUIDs = AssetDatabase.FindAssets("t:Sprite", new[] { iconsPath });

        foreach (string guid in spriteGUIDs)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null) continue;

            string itemName = sprite.name;

            // ===== ������� Item =====
            string itemAssetPath = Path.Combine(itemsPath, $"{itemName}.asset");
            Item newItem = ScriptableObject.CreateInstance<Item>();
            newItem.itemName = itemName;
            newItem.icon = sprite;
            AssetDatabase.CreateAsset(newItem, itemAssetPath);

            // ===== ������� LootItem, ����������� �� ���� Item =====
            string lootAssetPath = Path.Combine(lootPath, $"{itemName}_Loot.asset");
            LootItem newLootItem = ScriptableObject.CreateInstance<LootItem>();
            newLootItem.data = new LootData
            {
                sourceItem = newItem,
                spawnChance = 100f,
                category = "Default",
                defense = 0,
                description = "Autogenerated item"
            };

            AssetDatabase.CreateAsset(newLootItem, lootAssetPath);
            Debug.Log($"Created Item + LootItem for: {itemName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Created {spriteGUIDs.Length} item pairs!", "OK");
    }
}
