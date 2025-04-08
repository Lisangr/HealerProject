using UnityEngine;
using UnityEngine.UI;

public class RecipeUI : MonoBehaviour
{
    [Header("UI элементы")]
    public Text recipeNameText;
    public Image resultIconImage;
    public Transform ingredientsContainer;
    public GameObject ingredientIconPrefab;

    public void SetRecipe(CraftingRecipeData recipe)
    {
        // Получаем ссылку на объект инвентаря (или на базу всех предметов)
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory not found in scene!");
            return;
        }

        // Получаем объект Item для результата крафта по имени
        Item resultItem = inventory.GetItemByName(recipe.resultItem);
        if (resultItem != null)
        {
            recipeNameText.text = resultItem.GetLocalizedItemName();
            resultIconImage.sprite = resultItem.GetIcon();
            resultIconImage.enabled = true;
        }
        else
        {
            Debug.LogWarning("Результат крафта не найден: " + recipe.resultItem);
            recipeNameText.text = "";
            resultIconImage.sprite = null;
            resultIconImage.enabled = false;
        }

        // Очищаем старые иконки ингредиентов
        foreach (Transform child in ingredientsContainer)
        {
            Destroy(child.gameObject);
        }

        // Для каждого ингредиента (хранятся как строка) получаем объект Item
        foreach (string ingredientName in recipe.requiredItems)
        {
            Item ingredientItem = inventory.GetItemByName(ingredientName);
            if (ingredientItem == null)
            {
                Debug.LogWarning("Ингредиент не найден: " + ingredientName);
                continue;
            }

            GameObject ingredientGO = Instantiate(ingredientIconPrefab, ingredientsContainer);
            Image iconImage = ingredientGO.GetComponent<Image>();
            if (iconImage != null)
                iconImage.sprite = ingredientItem.GetIcon();

            Text nameText = ingredientGO.GetComponentInChildren<Text>();
            if (nameText != null)
                nameText.text = ingredientItem.GetLocalizedItemName();
        }
    }
}
