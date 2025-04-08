using UnityEngine;

public class CraftingBookUI : MonoBehaviour
{
    [Header("База рецептов")]
    public CraftingRecipesDatabase recipesDatabase;

    [Header("Префаб рецепта")]
    public GameObject recipePrefab;

    [Header("Контейнер для рецептов (например, ScrollView Content)")]
    public Transform recipesContainer;

    private void Start()
    {
        PopulateRecipes();
    }

    void PopulateRecipes()
    {
        foreach (Transform child in recipesContainer)
        {
            Destroy(child.gameObject); // Очищаем, если было открыто раньше
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
