/*using System.Collections.Generic;
using UnityEngine;

public class DroppedLoot : MonoBehaviour
{
    public LootConfig lootConfig; // Ссылка на конфиг с лутом
    public List<LootUI> lootUISlots;

    private void Awake()
    {
        if (lootUISlots == null || lootUISlots.Count == 0)
        {
            Debug.LogError("No LootUI slots assigned!");
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDestroy += SpawnLoot;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroy -= SpawnLoot;
    }

    private void SpawnLoot()
    {
        if (lootConfig == null || lootConfig.items.Count == 0)
        {
            Debug.LogError("Loot Config is not set or empty!");
            return;
        }

        List<LootData> selectedItems = SelectLootWithChance(lootUISlots.Count);
        for (int i = 0; i < selectedItems.Count; i++)
        {
            var selectedItem = selectedItems[i];
            if (selectedItem != null && i < lootUISlots.Count)
            {
                UpdateInventory(selectedItem);
                UpdateLootUI(i, selectedItem);
            }
        }
    }

    private List<LootData> SelectLootWithChance(int maxItems)
    {
        List<LootData> eligibleLoot = new List<LootData>();

        // Фильтрация предметов по шансу
        foreach (LootItem lootItem in lootConfig.items)
        {
            if (Random.Range(0f, 100f) <= lootItem.data.spawnChance)
            {
                eligibleLoot.Add(lootItem.data);
            }
        }

        // Выбор случайных предметов из подходящих
        return GetRandomItems(eligibleLoot, maxItems);
    }

    private List<LootData> GetRandomItems(List<LootData> source, int count)
    {
        List<LootData> selected = new List<LootData>();
        for (int i = 0; i < Mathf.Min(count, source.Count); i++)
        {
            int randomIndex = Random.Range(0, source.Count);
            selected.Add(source[randomIndex]);
            source.RemoveAt(randomIndex);
        }
        return selected;
    }

    private void UpdateInventory(LootData item)
    {
        if (ItemPickup.itemInventory.ContainsKey(item.itemName))
        {
            ItemPickup.itemInventory[item.itemName]++;
        }
        else
        {
            ItemPickup.itemInventory.Add(item.itemName, 1);
        }

        Debug.Log($"Added {item.itemName} to inventory. Total now: {ItemPickup.itemInventory[item.itemName]}");
        ItemPickup.LogInventoryContents();
    }

    private void UpdateLootUI(int slotIndex, LootData item)
    {
        var lootUI = lootUISlots[slotIndex];
        if (lootUI != null)
        {
            if (!lootUI.gameObject.activeSelf)
            {
                lootUI.gameObject.SetActive(true);
            }

            if (item.icon != null)
            {
                lootUI.UpdateLootDisplay(item);
            }
        }
    }
}*/