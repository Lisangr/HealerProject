using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    // �������������: ������������� ������� ��� ������������� ���� ��� ����������� ��������
    public string itemID;

    // ���������� ������� � �������, ������� ����� ������� ��� ����� �������
    public delegate void ItemCollectedHandler(CollectibleItem item);
    public static event ItemCollectedHandler OnItemCollected;

    private void OnTriggerStay(Collider other)
    {
        // ���������, ��� ����� (��� "Player") ��������� � ���� ��������
        if (other.CompareTag("Player"))
        {
            // ��� ������� ������� F �������� �������
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log($"[CollectibleItem] ������ �������: {itemID}");
                OnItemCollected?.Invoke(this);
                // ����� ������� ������ ����� �����
                Destroy(gameObject);
            }
        }
    }
}
