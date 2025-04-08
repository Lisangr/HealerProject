using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftingRecipeData
{
    [Tooltip("Список названий предметов, необходимых для крафта (порядок не важен)")]
    public List<string> requiredItems;  // Список объектов Item, требуемых для крафта

    [Tooltip("Данные результата крафта")]
    public string resultItem;  // Результат крафта

    [Tooltip("Иконка результата крафта")]
    public Sprite resultIcon; // Добавленное поле для иконки результата
}

[CreateAssetMenu(menuName = "Crafting/Crafting Recipes Database", fileName = "NewCraftingRecipesDatabase")]
public class CraftingRecipesDatabase : ScriptableObject
{
    public List<CraftingRecipeData> recipes;
}
