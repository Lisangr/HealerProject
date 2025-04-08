using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public int space = 10;
    public List<Item> allItems; // Список всех возможных предметов

    public bool Add(Item item)
    {
        if (ItemPickup.itemInventory.ContainsKey(item.itemName))
        {
            ItemPickup.itemInventory[item.itemName]++;
            return true;
        }
        else if (ItemPickup.itemInventory.Count < space)
        {
            ItemPickup.itemInventory.Add(item.itemName, 1);
            return true;
        }
        else
        {
            Debug.Log("Not enough room.");
            return false;
        }
    }

    public void Remove(string itemName)
    {
        if (ItemPickup.itemInventory.ContainsKey(itemName))
        {
            ItemPickup.itemInventory[itemName]--;
            if (ItemPickup.itemInventory[itemName] <= 0)
            {
                ItemPickup.itemInventory.Remove(itemName);
            }
        }
    }

    public Item GetItemByName(string itemName)
    {
        // Предполагается, что у вас есть список всех возможных предметов
        foreach (Item item in allItems)
        {
            if (item.itemName == itemName)
            {
                return item;
            }
        }
        return null;
    }
}
