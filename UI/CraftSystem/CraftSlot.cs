using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftSlot : MonoBehaviour, IDropHandler
{
    public Image icon;
    public string itemName; // ���������� ������ ������ ������� Item

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot invSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (invSlot != null && !string.IsNullOrEmpty(invSlot.itemName))
        {
            // ������� ��� �������� � ��� ������
            SetItem(invSlot.itemName, invSlot.icon.sprite);

            // �������� �������� ������
            CraftingManager craftingManager = FindObjectOfType<CraftingManager>();
            if (craftingManager != null)
                craftingManager.CheckCrafting();
        }
    }

    public void SetItem(string newItemName, Sprite newIcon)
    {
        itemName = newItemName;
        icon.sprite = newIcon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        itemName = null;
        icon.sprite = null;
        icon.enabled = false;
    }
}
