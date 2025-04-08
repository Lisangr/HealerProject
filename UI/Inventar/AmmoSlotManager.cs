using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AmmoSlotManager : MonoBehaviour
{
    public static AmmoSlotManager Instance;

    // ��������� � ���������� ����� � ������ ������� (��������, ����� �������)
    public List<AmmoSlot> ammoSlots;
    private InventoryUI inventoryUIManager;

    private void Awake()
    {
        inventoryUIManager = FindObjectOfType<InventoryUI>(); // ������� InventoryUI
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        // ���� ������ ��� UI, ����� �� ������������ �������
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
            Debug.Log($"���������� ������� �� ����� {index + 1}: {slot.itemName}");
            UseAmmoSlot(index);
        }
        else
        {
            // �������� ��������� ��� �������� �� InventoryUI
            string selectedItemName = inventoryUIManager.GetSelectedItemName();
            if (!string.IsNullOrEmpty(selectedItemName))
            {
                // ��������� ������ �� Resources/Icons
                Sprite iconSprite = Resources.Load<Sprite>($"Icons/{selectedItemName}");
                slot.SetItem(selectedItemName, iconSprite);
                Debug.Log($"������� {selectedItemName} �������� � ���� {index + 1}");
            }
            else
            {
                Debug.Log("��� ���������� �������� ��� ��������");
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
            // �������� ��� �������� �� �����
            string itemName = slot.itemName;

            // ������� ������ Player (����� ����� ������� ������ �������)
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                // � ����������� �� ����� ��������, ��������������� ����������� ������� ��������
                if (itemName.Equals("Potion_01"))
                {
                    player.RestoreHealthByPercentage(0.1f);
                    Debug.Log("������������ Potion_01: ������������� 10% ��������.");
                }
                else if (itemName.Equals("Potion_03"))
                {
                    player.RestoreHealthByPercentage(0.3f);
                    Debug.Log("������������ Potion_03: ������������� 30% ��������.");
                }
                else if (itemName.Equals("Potion_Red_01"))
                {
                    player.RestoreHealthByPercentage(0.6f);
                    Debug.Log("������������ Potion_Red_01: ������������� 60% ��������.");
                }
                if (itemName.Equals("Potion_02"))
                {
                    player.IncreaseMaxHealthByPercentage(0.1f);
                    Debug.Log("������������ Potion_02: ������������ �������� ��������� �� 10%.");
                }
                else if (itemName.Equals("Potion_04"))
                {
                    player.IncreaseMaxHealthByPercentage(0.3f);
                    Debug.Log("������������ Potion_04: ������������ �������� ��������� �� 30%.");
                }
                else if (itemName.Equals("Potion_Blue_01"))
                {
                    player.IncreaseMaxHealthByPercentage(0.6f);
                    Debug.Log("������������ Potion_Blue_01: ������������ �������� ��������� �� 60%.");
                }
                else
                {
                    Debug.Log("������������ ������� �� �������� ������ ��������������.");
                }
            }

            // ��������� ���������� ��������� � ���������
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

            // ��������� UI
            inventoryUIManager.UpdateUI();
        }
    }
}