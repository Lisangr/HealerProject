using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;
    public Inventory inventory;
    public GameObject inventorySlotPrefab;

    InventorySlot[] slots;

    void Awake()
    {
        inventory = FindObjectOfType<Inventory>();
        UpdateUI();
    }

    public void UpdateUI()
    {
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        int i = 0;
        foreach (var item in ItemPickup.itemInventory)
        {
            if (i < slots.Length)
            {
                slots[i].index = i; // Установка индекса слота
                slots[i].AddItem(item.Key, item.Value);
                i++;
            }
        }

        // Очищаем оставшиеся слоты
        for (; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
        }
    }
    // Пример метода для получения выбранного предмета в виде строки
    public string GetSelectedItemName()
    {
        // Здесь можно реализовать вашу логику выбора предмета.
        // Например, если у вас выделен какой-то InventorySlot, возвращаем его имя.
        // Для простоты вернём имя первого непустого слота:
        foreach (var slot in slots)
        {
            if (!string.IsNullOrEmpty(slot.itemName))
                return slot.itemName;
        }
        return null;
    }
}
