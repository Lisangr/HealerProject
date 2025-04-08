using UnityEngine;
using UnityEngine.UI;

public class LootUI : MonoBehaviour
{
    public Image itemImage; // Ссылка на UI Image компонент

    public void UpdateLootDisplay(LootData loot)
    {
        if (itemImage == null)
        {
            Debug.LogError("Item image reference is null!");
            return;
        }

        itemImage.sprite = loot.GetIcon(); // ✅

    }
}
