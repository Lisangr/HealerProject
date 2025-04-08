using System.Collections.Generic;
using UnityEngine;

public class SpellsPool : MonoBehaviour
{
    public static SpellsPool Instance;

    [Header("Pool Settings")]
    [SerializeField] private ProjectileMoveScript spellPrefab; // Префаб снаряда (стрелы)
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform bulletsParent; // Родитель для снарядов в иерархии

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
            ProjectileMoveScript arrow = Instantiate(spellPrefab, bulletsParent);
            arrow.gameObject.SetActive(false);
            arrowPool.Enqueue(arrow);
        }
    }

    // Получаем снаряд (стрелу) из пула
    public ProjectileMoveScript GetArrow()
    {
        ProjectileMoveScript arrow;
        if (arrowPool.Count > 0)
        {
            arrow = arrowPool.Dequeue();
        }
        else
        {
            // Если в пуле не осталось стрел, создаем новый
            arrow = Instantiate(spellPrefab, bulletsParent);
        }
        arrow.gameObject.SetActive(true);
        return arrow;
    }

    // Возвращаем снаряд (стрелу) обратно в пул
    public void ReturnArrow(ProjectileMoveScript arrow)
    {
        arrow.gameObject.SetActive(false);
        arrow.transform.SetParent(bulletsParent);
        arrowPool.Enqueue(arrow);
    }
}
