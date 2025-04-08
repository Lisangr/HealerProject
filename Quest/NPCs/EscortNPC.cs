using UnityEngine;
using System;

public class EscortNPC : MonoBehaviour
{
    [Header("��������� ����������")]
    public float followDistance = 4f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("��������� ������")]
    public float heightSmoothFactor = 5f;
    public float rayLength = 10f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.5f;

    [Header("����� ���������� (�����������)")]
    public Transform destinationPoint;
    public float destinationSwitchDistance = 5f;

    public string associatedQuestID;

    private Transform playerTransform;
    private bool escortActive = false;
    private bool questCompleted = false;
    private bool followPlayer = true;

    // �������, ������� ��������� �� ��������� ����� �������������
    public static event Action<EscortNPC> OnEscortCompleteEvent;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogError("����� �� ������! ��������� ��� 'Player'.");
    }

    void Update()
    {
        if (!escortActive || playerTransform == null || questCompleted)
            return;

        // ����������, ��������� �� �� ������� ��� ��������� � ����� ����������
        if (destinationPoint != null)
        {
            float distancePlayerToDestination = Vector3.Distance(playerTransform.position, destinationPoint.position);
            followPlayer = distancePlayerToDestination >= destinationSwitchDistance;
        }
        else
        {
            followPlayer = true;
        }

        // ������� �������
        Vector3 targetPosition = followPlayer
            ? playerTransform.position - playerTransform.forward * followDistance
            : destinationPoint.position;

        // �������������� �����������: ��������� ������� Y
        Vector3 currentPos = transform.position;
        Vector3 targetPosFlat = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);
        Vector3 direction = (targetPosFlat - currentPos).normalized;
        Vector3 desiredVelocity = direction * moveSpeed * Time.deltaTime;

        // ����������� NPC
        transform.position = new Vector3(currentPos.x + desiredVelocity.x, currentPos.y, currentPos.z + desiredVelocity.z);

        // ������� �� �����������
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // ���� � ������ �������� � ����� � NPC ����� ������ � (�� �����������)
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

        // ������������� ������ ����� Raycast
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
            Debug.LogWarning("Raycast �� ��������� ����� �� ���� groundLayer!");
        }
        // ��������� ������ Y
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // ���������� ��� �������� ������
    public void ActivateEscort()
    {
        escortActive = true;
        Debug.Log($"NPC {gameObject.name} ����� ��������� �� �������");
    }

    private void OnEscortComplete()
    {
        Debug.Log("����� ������������� ��������! NPC ������ ������ ����������.");
        escortActive = false;
        // �����, ��������, ����� ����������� �������� QuestGiver � �������� ������ ������
        Quest quest = GetComponent<QuestGiver>()?.quest;
        if (quest != null)
        {
            quest.status = QuestStatus.ReadyToComplete;
        }
        // ���������� ������� ����� �������
        OnEscortCompleteEvent?.Invoke(this);
    }
}
