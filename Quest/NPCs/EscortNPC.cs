using UnityEngine;
using System;

public class EscortNPC : MonoBehaviour
{
    [Header("Настройки следования")]
    public float followDistance = 4f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("Настройки высоты")]
    public float heightSmoothFactor = 5f;
    public float rayLength = 10f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.5f;

    [Header("Точка назначения (опционально)")]
    public Transform destinationPoint;
    public float destinationSwitchDistance = 5f;

    public string associatedQuestID;

    private Transform playerTransform;
    private bool escortActive = false;
    private bool questCompleted = false;
    private bool followPlayer = true;

    // Событие, которое оповещает об окончании этапа сопровождения
    public static event Action<EscortNPC> OnEscortCompleteEvent;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogError("Игрок не найден! Проверьте тег 'Player'.");
    }

    void Update()
    {
        if (!escortActive || playerTransform == null || questCompleted)
            return;

        // Определяем, следовать ли за игроком или двигаться к точке назначения
        if (destinationPoint != null)
        {
            float distancePlayerToDestination = Vector3.Distance(playerTransform.position, destinationPoint.position);
            followPlayer = distancePlayerToDestination >= destinationSwitchDistance;
        }
        else
        {
            followPlayer = true;
        }

        // Целевая позиция
        Vector3 targetPosition = followPlayer
            ? playerTransform.position - playerTransform.forward * followDistance
            : destinationPoint.position;

        // Горизонтальное перемещение: сохраняем текущую Y
        Vector3 currentPos = transform.position;
        Vector3 targetPosFlat = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);
        Vector3 direction = (targetPosFlat - currentPos).normalized;
        Vector3 desiredVelocity = direction * moveSpeed * Time.deltaTime;

        // Перемещение NPC
        transform.position = new Vector3(currentPos.x + desiredVelocity.x, currentPos.y, currentPos.z + desiredVelocity.z);

        // Поворот по горизонтали
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Если в режиме движения к точке и NPC почти достиг её (по горизонтали)
        if (!followPlayer && destinationPoint != null)
        {
            float horizontalDistance = Vector3.Distance(
                new Vector3(currentPos.x, 0, currentPos.z),
                new Vector3(destinationPoint.position.x, 0, destinationPoint.position.z));
            if (horizontalDistance < 1f)
            {
                questCompleted = true;
                OnEscortComplete();
                return;
            }
        }

        // Корректировка высоты через Raycast
        float newY = currentPos.y;
        Vector3 rayOrigin = new Vector3(currentPos.x, currentPos.y + 2f, currentPos.z);
        RaycastHit hit;
        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red, 1f);
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer))
        {
            newY = Mathf.Lerp(currentPos.y, hit.point.y, heightSmoothFactor * Time.deltaTime);
        }
        else
        {
            Debug.LogWarning("Raycast не обнаружил землю на слое groundLayer!");
        }
        // Сохраняем только Y
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // Вызывается при принятии квеста
    public void ActivateEscort()
    {
        escortActive = true;
        Debug.Log($"NPC {gameObject.name} начал следовать за игроком");
    }

    private void OnEscortComplete()
    {
        Debug.Log("Квест сопровождения завершен! NPC достиг пункта назначения.");
        escortActive = false;
        // Здесь, например, можно попробовать получить QuestGiver и изменить статус квеста
        Quest quest = GetComponent<QuestGiver>()?.quest;
        if (quest != null)
        {
            quest.status = QuestStatus.ReadyToComplete;
        }
        // Уведомляем систему через событие
        OnEscortCompleteEvent?.Invoke(this);
    }
}
