using UnityEngine;

public class CraftingBookUI : MonoBehaviour
{
    [Header("���� ��������")]
    public CraftingRecipesDatabase recipesDatabase;

    [Header("������ �������")]
    public GameObject recipePrefab;

    [Header("��������� ��� �������� (��������, ScrollView Content)")]
    public Transform recipesContainer;

    private void Start()
    {
        PopulateRecipes();
    }

    void PopulateRecipes()
    {
        foreach (Transform child in recipesContainer)
        {
            Destroy(child.gameObject); // �������, ���� ���� ������� ������
        }

        foreach (var recipe in recipesDatabase.recipes)
        {
            GameObject recipeGO = Instantiate(recipePrefab, recipesContainer);
            RecipeUI recipeUI = recipeGO.GetComponent<RecipeUI>();
            if (recipeUI != null)
            {
                recipeUI.SetRecipe(recipe);
            }
        }
    }
}
