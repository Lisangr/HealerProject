using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    // Необязательно: идентификатор объекта для идентификации типа или конкретного предмета
    public string itemID;

    // Определяем делегат и событие, которое будет вызвано при сборе объекта
    public delegate void ItemCollectedHandler(CollectibleItem item);
    public static event ItemCollectedHandler OnItemCollected;

    private void OnTriggerStay(Collider other)
    {
        // Проверяем, что игрок (тег "Player") находится в зоне триггера
        if (other.CompareTag("Player"))
        {
            // При нажатии клавиши F вызываем событие
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log($"[CollectibleItem] Собран предмет: {itemID}");
                OnItemCollected?.Invoke(this);
                // Можно удалить объект после сбора
                Destroy(gameObject);
            }
        }
    }
}
