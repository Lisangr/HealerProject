using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
    [Header("����� ��� ������")]
    public CraftSlot[] craftSlots;

    [Header("������� ������ (�� �����)")]
    public CraftingRecipesDatabase recipesDatabase;

    [Header("UI ��� ����������")]
    public Image resultIcon;

    public void CheckCrafting()
    {
        // �������� ����� ��������� �� ������ ������
        List<string> currentItems = new List<string>();
        foreach (var slot in craftSlots)
        {
            if (!string.IsNullOrEmpty(slot.itemName))
                currentItems.Add(slot.itemName);
        }

        // �������� �� �������� �� ����
        foreach (CraftingRecipeData recipe in recipesDatabase.recipes)
        {
            if (MatchesRecipe(currentItems, recipe.requiredItems))
            {
                Inventory inventory = FindObjectOfType<Inventory>();
                if (inventory != null)
                {
                    // �������� ��� ���������� ������ �, ��������, ��� ������ �� ������� ����� (��������, �� ����)
                    // ����� ��������������, ��� inventory.GetItemByName ���������� ������ Item, �� �������� ����� ����� icon,
                    // �� ���� ��� ����� ������������ ������ string, ����� ���������� ������ ��������� ������
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

        // ���� ������ �� ������ � ���������� UI ����������
        if (resultIcon != null)
        {
            resultIcon.sprite = null;
            resultIcon.enabled = false;
        }
    }
    public void CraftItem()
    {
        // �������� ����� ��������� �� ������ ������
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
                // ������� ������ ����������� ������� �� ���������
                foreach (string requiredItemName in matchingRecipe.requiredItems)
                {
                    inventory.Remove(requiredItemName);
                }

                // �������� ������ Item ���������� ������ � ��������� ��� � ���������
                Item craftedItem = inventory.GetItemByName(matchingRecipe.resultItem);
                if (craftedItem != null)
                {
                    inventory.Add(craftedItem);
                }
                else
                {
                    Debug.LogWarning("�����: �� ������ Item ��� " + matchingRecipe.resultItem);
                }
            }
            else
            {
                Debug.LogWarning("�����: �� ������ ������ ���������.");
            }

            // ������� ����� ������ � ���������� UI ����������
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
            Debug.Log("��� ����������� ������� ��� ������.");
        }
    }

    // ����� ��������� ������� � �������� ������ ��������� (��� ����� �������)
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
