using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<Enemy> enemyPrefabs; // Префабы врагов
    [SerializeField] private int initialPoolSize = 10; // Размер пула для каждого типа
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
        // Убираем "(Clone)" из имени
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
                return pool.Dequeue();
            }
            else
            {
                return InstantiateEnemy(enemyPrefab);
            }
        }
        Debug.LogError($"Prefab {enemyPrefab.name} not in pool!");
        return null;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        Enemy prefab = enemy.OriginalPrefab;
        if (enemyPools.ContainsKey(prefab))
        {
            enemy.gameObject.SetActive(false);
            enemyPools[prefab].Enqueue(enemy);
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