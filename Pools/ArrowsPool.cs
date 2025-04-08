using System.Collections.Generic;
using UnityEngine;

public class ArrowsPool : MonoBehaviour
{
    public static ArrowsPool Instance;

    [Header("Pool Settings")]
    [SerializeField] private ProjectileMoveScript arrowPrefab; // ������ ������� (������)
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform bulletsParent; // �������� ��� �������� � ��������

    private Queue<ProjectileMoveScript> arrowPool = new Queue<ProjectileMoveScript>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            ProjectileMoveScript arrow = Instantiate(arrowPrefab, bulletsParent);
            arrow.gameObject.SetActive(false);
            arrowPool.Enqueue(arrow);
        }
    }

    // �������� ������ (������) �� ����
    public ProjectileMoveScript GetArrow()
    {
        ProjectileMoveScript arrow;
        if (arrowPool.Count > 0)
        {
            arrow = arrowPool.Dequeue();
        }
        else
        {
            // ���� � ���� �� �������� �����, ������� �����
            arrow = Instantiate(arrowPrefab, bulletsParent);
        }
        arrow.gameObject.SetActive(true);
        return arrow;
    }

    // ���������� ������ (������) ������� � ���
    public void ReturnArrow(ProjectileMoveScript arrow)
    {
        arrow.gameObject.SetActive(false);
        arrow.transform.SetParent(bulletsParent);
        arrowPool.Enqueue(arrow);
    }
}
