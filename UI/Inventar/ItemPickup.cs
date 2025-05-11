using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    [HideInInspector] public string itemName;
    [HideInInspector] public int itemQuantity = 1;
    [HideInInspector] public string uniqueID;
    public static Dictionary<string, int> itemInventory = new Dictionary<string, int>(); // ������� ��� �������� ���������
    private InventoryUI inventoryUIManager;

    private void Start()
    {
        inventoryUIManager = FindObjectOfType<InventoryUI>(); // ������� InventoryUIManager
        itemName = item.itemName;
        uniqueID = Guid.NewGuid().ToString(); // ��������� ����������� ��������������
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && (ClickHandler.tempID == uniqueID) && (ClickHandler.distance <= 8f))
        {
            
            // ���������, ���� �� ������� ��� � ���������
            if (itemInventory.ContainsKey(itemName))
            {
                // ���� ������� ��� ���� � �������, ����������� ��� ����������
                itemInventory[itemName] += itemQuantity;
                Debug.Log("item " + itemName + " added to inventory " + itemQuantity);
            }
            else
            {
                // ���� �������� ��� � �������, ��������� ���
                itemInventory.Add(itemName, itemQuantity);
                Debug.Log("NEW item " + itemName + " added to inventory " + itemQuantity);
            }

            // ��������� UI ���������
            if (inventoryUIManager != null)
            {
                inventoryUIManager.UpdateUI();
            }else
            {
                Debug.Log("        inventoryUIManager = FindObjectOfType<InventoryUI>(); // �� ������� InventoryUIManager");
            }

                // ������� ������ �� �����
                Destroy(gameObject);
        }
    }

}