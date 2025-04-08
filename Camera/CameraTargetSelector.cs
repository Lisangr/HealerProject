using UnityEngine;

public class CameraTargetSelector : MonoBehaviour
{
    [Header("��������� ������ ����")]
    [Tooltip("������������ ��������� ��� ������ ����")]
    public float maxTargetDistance = 100f;
    [Tooltip("����, � ������� ��������� �����")]
    public LayerMask targetLayerMask;

    // ������� ��������� ����
    public Enemy CurrentTarget { get; private set; }

    void Update()
    {
        SelectTarget();
    }

    // �������� ����, �� ������� ��������� ����� ������
    private void SelectTarget()
    {
        // ������� ��� �� ������ ������
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // ���� ��� ������������ � �������� � �������� ����
        if (Physics.Raycast(ray, out hit, maxTargetDistance, targetLayerMask))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                CurrentTarget = enemy;
                return;
            }
        }
        // ���� ���� �� �������, ���������� ��������
        CurrentTarget = null;
    }

    // ��� ����������� ����� ���������� ��� � ���������
    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;
        Gizmos.color = Color.red;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * maxTargetDistance);
    }
}
