using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftingRecipeData
{
    [Tooltip("������ �������� ���������, ����������� ��� ������ (������� �� �����)")]
    public List<string> requiredItems;  // ������ �������� Item, ��������� ��� ������

    [Tooltip("������ ���������� ������")]
    public string resultItem;  // ��������� ������

    [Tooltip("������ ���������� ������")]
    public Sprite resultIcon; // ����������� ���� ��� ������ ����������
}

[CreateAssetMenu(menuName = "Crafting/Crafting Recipes Database", fileName = "NewCraftingRecipesDatabase")]
public class CraftingRecipesDatabase : ScriptableObject
{
    public List<CraftingRecipeData> recipes;
}
