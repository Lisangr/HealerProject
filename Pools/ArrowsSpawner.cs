using UnityEngine;

public class ArrowsSpawner : MonoBehaviour
{
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Camera mainCamera; // ������ ��� ����������� �����������

    // ����� ��� ������ ������ � ����������� �������
    public void SpawnArrow()
    {
        // �������� ������ �� ����
        ProjectileMoveScript arrow = ArrowsPool.Instance.GetArrow();

        // ������ ������� ��� ������
        arrow.transform.position = arrowSpawnPoint.position;

        // ��������� ����������� �� �������
        Vector3 direction = GetCursorDirection();

        // ������ ����������� ������
        arrow.transform.rotation = Quaternion.LookRotation(direction);
    }

    // ����� ��� ���������� ����������� �������� ������ � ������� �������
    private Vector3 GetCursorDirection()
    {
        // �������� ������� ������� � ������������ ������
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ����������, �������� �� ��� � ����� (��������, �� ����������� ����� ��� ������ ������)
        if (Physics.Raycast(ray, out hit))
        {
            // ��������� ����������� �� ����� ������ �� �������, ���� ��������� ������
            Vector3 direction = hit.point - arrowSpawnPoint.position;
            direction.y = 0; // ���������� ������������ ����������, ����� ������ ������ �������������
            return direction.normalized; // ���������� ��������������� �����������
        }

        // ���� ��� ������ �� �����, ���������� ������ ����� �� ���������
        return arrowSpawnPoint.forward;
    }
}
