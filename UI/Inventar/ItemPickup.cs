using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    [HideInInspector] public string itemName;
    [HideInInspector] public int itemQuantity = 1;
    [HideInInspector] public string uniqueID;
    public static Dictionary<string, int> itemInventory = new Dictionary<string, int>(); // Словарь для хранения предметов
    private InventoryUI inventoryUIManager;

    private void Start()
    {
        inventoryUIManager = FindObjectOfType<InventoryUI>(); // Находим InventoryUIManager
        itemName = item.itemName;
        uniqueID = Guid.NewGuid().ToString(); // Генерация уникального идентификатора
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && (ClickHandler.tempID == uniqueID) && (ClickHandler.distance <= 8f))
        {
            
            // Проверяем, есть ли предмет уже в инвентаре
            if (itemInventory.ContainsKey(itemName))
            {
                // Если предмет уже есть в словаре, увеличиваем его количество
                itemInventory[itemName] += itemQuantity;
                Debug.Log("item " + itemName + " added to inventory " + itemQuantity);
            }
            else
            {
                // Если предмета нет в словаре, добавляем его
                itemInventory.Add(itemName, itemQuantity);
                Debug.Log("NEW item " + itemName + " added to inventory " + itemQuantity);
            }

            // Обновляем UI инвентаря
            if (inventoryUIManager != null)
            {
                inventoryUIManager.UpdateUI();
            }else
            {
                Debug.Log("        inventoryUIManager = FindObjectOfType<InventoryUI>(); // Не находим InventoryUIManager");
            }

                // Удаляем объект из сцены
                Destroy(gameObject);
        }
    }

}