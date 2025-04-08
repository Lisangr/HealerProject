using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AmmoSlot : MonoBehaviour, IDropHandler
{
    public Image icon;
    public string itemName; // Используем строку вместо объекта Item

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot invSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (invSlot != null && !string.IsNullOrEmpty(invSlot.itemName))
        {
            // Передаём имя предмета и его иконку
            SetItem(invSlot.itemName, invSlot.icon.sprite);
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

    // Метод проверки пустоты слота
    public bool IsSlotEmpty()
    {
        return string.IsNullOrEmpty(itemName);
    }

    // Возвращает имя предмета в слоте
    public string GetItem()
    {
        return itemName;
    }
}
