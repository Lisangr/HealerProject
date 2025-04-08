using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<Enemy> enemyPrefabs; // ������� ������
    [SerializeField] private int initialPoolSize = 10; // ������ ���� ��� ������� ����
    [SerializeField] private Transform enemiesParent;

    private Dictionary<Enemy, Queue<Enemy>> enemyPools = new Dictionary<Enemy, Queue<Enemy>>();

    private void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (Enemy prefab in enemyPrefabs)
        {
            Queue<Enemy> pool = new Queue<Enemy>();
            for (int i = 0; i < initialPoolSize; i++)
            {
                Enemy enemy = InstantiateEnemy(prefab);
                pool.Enqueue(enemy);
            }
            enemyPools[prefab] = pool;
        }
    }

    /* private Enemy InstantiateEnemy(Enemy prefab)
     {
         Enemy enemy = Instantiate(prefab, enemiesParent);
         enemy.OriginalPrefab = prefab;
         enemy.gameObject.SetActive(false);
         return enemy;
     }*/
    private Enemy InstantiateEnemy(Enemy prefab)
    {
        Enemy enemy = Instantiate(prefab, enemiesParent);
        enemy.OriginalPrefab = prefab;
        enemy.gameObject.SetActive(false);
        // ������� "(Clone)" �� �����
        enemy.name = enemy.name.Replace("(Clone)", "").Trim();
        //enemy.name = prefab.name;

        return enemy;
    }


    public Enemy GetEnemy(Enemy enemyPrefab)
    {
        if (enemyPools.TryGetValue(enemyPrefab, out Queue<Enemy> pool))
        {
            if (pool.Count > 0)
            {
                Enemy enemy = pool.Dequeue();
                Debug.Log($"Извлечение врага {enemy.name} из пула. Осталось в пуле: {pool.Count}");
                return enemy;
            }
            else
            {
                Debug.Log("Создание нового врага, так как пул пуст");
                return InstantiateEnemy(enemyPrefab);
            }
        }
        Debug.LogError($"Prefab {enemyPrefab.name} not in pool!");
        return null;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("Попытка вернуть null врага в пул");
            return;
        }

        Debug.Log($"Возврат врага {enemy.name} в пул");
        
        Enemy prefab = enemy.OriginalPrefab;
        if (enemyPools.ContainsKey(prefab))
        {
            Queue<Enemy> pool = enemyPools[prefab];
            if (pool.Contains(enemy))
            {
                Debug.LogWarning($"Враг {enemy.name} уже находится в пуле");
                return;
            }

            enemy.gameObject.SetActive(false);
            pool.Enqueue(enemy);
            
            Debug.Log($"Враг {enemy.name} успешно возвращен в пул. Текущий размер пула: {pool.Count}");
        }
        else
        {
            Destroy(enemy.gameObject);
        }
    }

    public void SetPoolSize(int newSize)
    {
        foreach (var kvp in enemyPools)
        {
            Queue<Enemy> pool = kvp.Value;
            while (pool.Count < newSize)
            {
                pool.Enqueue(InstantiateEnemy(kvp.Key));
            }
            while (pool.Count > newSize)
            {
                Enemy enemy = pool.Dequeue();
                Destroy(enemy.gameObject);
            }
        }
    }
}