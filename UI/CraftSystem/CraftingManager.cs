using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
    [Header("Слоты для крафта")]
    public CraftSlot[] craftSlots;

    [Header("Рецепты крафта (из книги)")]
    public CraftingRecipesDatabase recipesDatabase;

    [Header("UI для результата")]
    public Image resultIcon;

    public void CheckCrafting()
    {
        // Собираем имена предметов из слотов крафта
        List<string> currentItems = new List<string>();
        foreach (var slot in craftSlots)
        {
            if (!string.IsNullOrEmpty(slot.itemName))
                currentItems.Add(slot.itemName);
        }

        // Проходим по рецептам из базы
        foreach (CraftingRecipeData recipe in recipesDatabase.recipes)
        {
            if (MatchesRecipe(currentItems, recipe.requiredItems))
            {
                Inventory inventory = FindObjectOfType<Inventory>();
                if (inventory != null)
                {
                    // Получаем имя результата крафта и, возможно, его иконку из другого места (например, из базы)
                    // Здесь предполагается, что inventory.GetItemByName возвращает объект Item, из которого можно взять icon,
                    // но если вам нужно использовать только string, можно доработать логику получения иконки
                    Item resultItem = inventory.GetItemByName(recipe.resultItem);
                    if (resultItem != null && resultIcon != null)
                    {
                        resultIcon.sprite = resultItem.icon;
                        resultIcon.enabled = true;
                    }
                }
                return;
            }
        }

        // Если рецепт не найден – сбрасываем UI результата
        if (resultIcon != null)
        {
            resultIcon.sprite = null;
            resultIcon.enabled = false;
        }
    }
    public void CraftItem()
    {
        // Собираем имена предметов из слотов крафта
        List<string> currentItems = new List<string>();
        foreach (var slot in craftSlots)
        {
            if (!string.IsNullOrEmpty(slot.itemName))
                currentItems.Add(slot.itemName);
        }

        CraftingRecipeData matchingRecipe = null;
        foreach (CraftingRecipeData recipe in recipesDatabase.recipes)
        {
            if (MatchesRecipe(currentItems, recipe.requiredItems))
            {
                matchingRecipe = recipe;
                break;
            }
        }

        if (matchingRecipe != null)
        {
            Inventory inventory = FindObjectOfType<Inventory>();
            if (inventory != null)
            {
                // Удаляем каждый необходимый предмет из инвентаря
                foreach (string requiredItemName in matchingRecipe.requiredItems)
                {
                    inventory.Remove(requiredItemName);
                }

                // Получаем объект Item результата крафта и добавляем его в инвентарь
                Item craftedItem = inventory.GetItemByName(matchingRecipe.resultItem);
                if (craftedItem != null)
                {
                    inventory.Add(craftedItem);
                }
                else
                {
                    Debug.LogWarning("Крафт: не найден Item для " + matchingRecipe.resultItem);
                }
            }
            else
            {
                Debug.LogWarning("Крафт: не найден объект инвентаря.");
            }

            // Очищаем слоты крафта и сбрасываем UI результата
            foreach (var slot in craftSlots)
            {
                slot.ClearSlot();
            }
            if (resultIcon != null)
            {
                resultIcon.sprite = null;
                resultIcon.enabled = false;
            }

            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.UpdateUI();
            }
        }
        else
        {
            Debug.Log("Нет подходящего рецепта для крафта.");
        }
    }

    // Метод сравнения рецепта и текущего набора предметов (без учета порядка)
    private bool MatchesRecipe(List<string> currentItems, List<string> requiredItems)
    {
        if (currentItems.Count != requiredItems.Count)
            return false;

        Dictionary<string, int> currentCount = new Dictionary<string, int>();
        foreach (string item in currentItems)
        {
            if (currentCount.ContainsKey(item))
                currentCount[item]++;
            else
                currentCount[item] = 1;
        }

        Dictionary<string, int> requiredCount = new Dictionary<string, int>();
        foreach (string item in requiredItems)
        {
            if (requiredCount.ContainsKey(item))
                requiredCount[item]++;
            else
                requiredCount[item] = 1;
        }

        foreach (var kvp in requiredCount)
        {
            if (!currentCount.ContainsKey(kvp.Key) || currentCount[kvp.Key] < kvp.Value)
                return false;
        }
        return true;
    }
}
