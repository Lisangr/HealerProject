using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Item", menuName = "Inventory/Loot Item")]
public class LootItem : ScriptableObject
{
    public LootData data = new LootData();
}

[System.Serializable]
public class LootData
{
    [Tooltip("Ссылка на основной Item, откуда брать имя, иконку и локализации")]
    public Item sourceItem;

    public float spawnChance;
    public string category = null;
    public int defense = 0;
    public string description;

    //public List<ItemLocalization> localizations;

    public string GetLocalizedItemName()
    {
        if (sourceItem == null) return "Unnamed";

        string currentLang = LocalizationHelper.GetSystemLanguageCode().ToLower();
        foreach (var loc in sourceItem.localizations)
        {
            if (loc.languageCode.ToLower() == currentLang)
                return loc.localizedName;
        }
        return sourceItem.itemName;
    }

    public Sprite GetIcon()
    {
        return sourceItem != null ? sourceItem.icon : null;
    }

}