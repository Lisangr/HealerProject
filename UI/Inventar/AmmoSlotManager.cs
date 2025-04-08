using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AmmoSlotManager : MonoBehaviour
{
    public static AmmoSlotManager Instance;

    // Привяжите в инспекторе слоты в нужном порядке (например, слева направо)
    public List<AmmoSlot> ammoSlots;
    private InventoryUI inventoryUIManager;

    private void Awake()
    {
        inventoryUIManager = FindObjectOfType<InventoryUI>(); // Находим InventoryUI
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        // Если курсор над UI, можно не обрабатывать клавиши
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            ProcessSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ProcessSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            ProcessSlot(2);
    }

    private void ProcessSlot(int index)
    {
        if (index < 0 || index >= ammoSlots.Count)
            return;

        AmmoSlot slot = ammoSlots[index];

        if (!slot.IsSlotEmpty())
        {
            Debug.Log($"Используем предмет из слота {index + 1}: {slot.itemName}");
            UseAmmoSlot(index);
        }
        else
        {
            // Получаем выбранное имя предмета из InventoryUI
            string selectedItemName = inventoryUIManager.GetSelectedItemName();
            if (!string.IsNullOrEmpty(selectedItemName))
            {
                // Загружаем иконку из Resources/Icons
                Sprite iconSprite = Resources.Load<Sprite>($"Icons/{selectedItemName}");
                slot.SetItem(selectedItemName, iconSprite);
                Debug.Log($"Предмет {selectedItemName} назначен в слот {index + 1}");
            }
            else
            {
                Debug.Log("Нет выбранного предмета для переноса");
            }
        }
    }
    public void UseAmmoSlot(int index)
    {
        if (index < 0 || index >= ammoSlots.Count)
            return;

        AmmoSlot slot = ammoSlots[index];

        if (!slot.IsSlotEmpty())
        {
            // Получаем имя предмета из слота
            string itemName = slot.itemName;

            // Находим объект Player (можно также хранить ссылку заранее)
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                // В зависимости от имени предмета, восстанавливаем определённый процент здоровья
                if (itemName.Equals("Potion_01"))
                {
                    player.RestoreHealthByPercentage(0.1f);
                    Debug.Log("Использована Potion_01: восстановлено 10% здоровья.");
                }
                else if (itemName.Equals("Potion_03"))
                {
                    player.RestoreHealthByPercentage(0.3f);
                    Debug.Log("Использована Potion_03: восстановлено 30% здоровья.");
                }
                else if (itemName.Equals("Potion_Red_01"))
                {
                    player.RestoreHealthByPercentage(0.6f);
                    Debug.Log("Использована Potion_Red_01: восстановлено 60% здоровья.");
                }
                if (itemName.Equals("Potion_02"))
                {
                    player.IncreaseMaxHealthByPercentage(0.1f);
                    Debug.Log("Использована Potion_02: максимальное здоровье увеличено на 10%.");
                }
                else if (itemName.Equals("Potion_04"))
                {
                    player.IncreaseMaxHealthByPercentage(0.3f);
                    Debug.Log("Использована Potion_04: максимальное здоровье увеличено на 30%.");
                }
                else if (itemName.Equals("Potion_Blue_01"))
                {
                    player.IncreaseMaxHealthByPercentage(0.6f);
                    Debug.Log("Использована Potion_Blue_01: максимальное здоровье увеличено на 60%.");
                }
                else
                {
                    Debug.Log("Используемый предмет не является зельем восстановления.");
                }
            }

            // Уменьшаем количество предметов в инвентаре
            if (ItemPickup.itemInventory.TryGetValue(itemName, out int quantity))
            {
                if (quantity > 1)
                {
                    ItemPickup.itemInventory[itemName] = quantity - 1;
                }
                else
                {
                    ItemPickup.itemInventory.Remove(itemName);
                    slot.ClearSlot();
                }
            }

            // Обновляем UI
            inventoryUIManager.UpdateUI();
        }
    }
}