using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Image icon;
    public Button removeButton;
    public string itemName;
    public int itemQuantity;
    public int index; // Индекс текущего слота нужно для очищения InventoryUI
    public Text quantityText; // Добавляем текстовое поле для отображения количества предметов

    public Item item;
    private Vector3 originalPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private InventoryUI inventoryUI;
    private bool isOutsideInventory;
    private Vector3 currentMousePosition;
    private GameObject draggingIcon;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryUI = FindObjectOfType<InventoryUI>();
        removeButton.onClick.AddListener(OnRemoveButton); // Добавляем обработчик события для кнопки удаления
    }

    private void Update()
    {
        // Получаем координаты курсора в экранных координатах
        Vector3 screenPosition = Input.mousePosition;

        // Создаем луч от камеры через позицию курсора
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // Проверяем пересечение луча с землей (слой "Ground")
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            currentMousePosition = hit.point;
        }
    }

    public void AddItem(string newItemName, int quantity)
    {
        itemName = newItemName;
        itemQuantity = quantity;

        // Предполагается, что иконки хранятся в Resources/Icons
        icon.sprite = Resources.Load<Sprite>($"Icons/{itemName}");
        if (icon.sprite != null)
        {
            icon.enabled = true;
            removeButton.interactable = true;
            quantityText.text = itemQuantity.ToString(); // Обновляем текстовое поле с количеством предметов
        }
        else
        {
            Debug.LogWarning($"Icon for item '{itemName}' not found in Resources/Icons.");
            icon.enabled = false;
            removeButton.interactable = false;
            quantityText.text = ""; // Очищаем текстовое поле
        }
    }

    public void ClearSlot()
    {
        itemName = null;
        itemQuantity = 0;
        icon.sprite = null;
        icon.enabled = false;
        removeButton.interactable = false;
        quantityText.text = ""; // Очищаем текстовое поле
    }

    public void OnRemoveButton()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        inventory.Remove(itemName);
        ClearSlot();
        inventoryUI.UpdateUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRemoveButton();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        canvasGroup.blocksRaycasts = false;

        // Создаем временный объект для визуализации перетаскиваемого предмета
        draggingIcon = Instantiate(new GameObject("Dragging Icon"), transform.position, transform.rotation);
        draggingIcon.transform.SetParent(inventoryUI.transform, false);
        draggingIcon.transform.SetAsLastSibling();

        Image draggingIconImage = draggingIcon.AddComponent<Image>();
        draggingIconImage.sprite = icon.sprite;
        draggingIconImage.raycastTarget = false;

        RectTransform draggingIconRect = draggingIcon.GetComponent<RectTransform>();
        draggingIconRect.sizeDelta = new Vector2(50, 50); // Размер иконки
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingIcon != null)
        {
            draggingIcon.transform.position = eventData.position;
        }

        // Проверяем, находится ли слот за пределами инвентаря
        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryUI.GetComponent<RectTransform>(), Input.mousePosition))
        {
            isOutsideInventory = true;
        }
        else
        {
            isOutsideInventory = false;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (draggingIcon != null)
        {
            Destroy(draggingIcon);
        }

        // Выполняем Raycast для определения объекта под курсором
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        // Если под курсором найден слот крафта
        foreach (RaycastResult result in results)
        {
            CraftSlot craftSlot = result.gameObject.GetComponent<CraftSlot>();
            if (craftSlot != null)
            {
                // Передаём предмет в слот крафта
                craftSlot.SetItem(itemName, icon.sprite);

                itemQuantity -= 1;
                if (itemQuantity <= 0)
                {
                    ClearSlot();
                }
                inventoryUI.UpdateUI();
                return; // Завершаем выполнение, так как обработали дроп на крафт
            }
        }

        // Если не найден CraftSlot, проверяем на обмен с другим слотом инвентаря
        foreach (RaycastResult result in results)
        {
            InventorySlot targetSlot = result.gameObject.GetComponent<InventorySlot>();
            if (targetSlot != null && targetSlot != this)
            {
                // Обмен местами предметов
                string tempItemName = targetSlot.itemName;
                int tempItemQuantity = targetSlot.itemQuantity;
                targetSlot.AddItem(itemName, itemQuantity);
                AddItem(tempItemName, tempItemQuantity);
                return;
            }
        }

        // Если курсор находится за пределами инвентаря — сбрасываем предмет на землю
        if (isOutsideInventory)
        {
            transform.position = originalPosition;
            transform.SetParent(originalParent);
            itemQuantity -= 1;
            CreateDroppedItem();
            ClearSlot();
            inventoryUI.UpdateUI();
        }
    }

    private void CreateDroppedItem()
    {
        if (!string.IsNullOrEmpty(itemName))
        {
            // Загружаем префаб из папки Resources/Prefabs
            GameObject prefab = Resources.Load<GameObject>($"Items/{itemName}");
            if (prefab != null)
            {
                GameObject droppedItem = Instantiate(prefab);
                droppedItem.transform.position = currentMousePosition;
            }
            else
            {
                Debug.LogWarning($"Prefab for item '{itemName}' not found in Resources/Prefabs.");
            }
        }
        ///
        // Удаляем предмет из словаря
        if (ItemPickup.itemInventory.TryGetValue(itemName, out int quantity))
        {
            if (quantity > 1)
            {
                quantity -= 1; // Уменьшаем количество на 1
                ItemPickup.itemInventory[itemName] = quantity; // Обновляем количество в словаре
            }
            else if (quantity == 1)
            {
                ItemPickup.itemInventory.Remove(itemName); // Удаляем из словаря, если количество 1
            }
        }
    }
}