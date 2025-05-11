using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<Enemy> enemyPrefabs; // Список префабов врагов
    [SerializeField] private int initialPoolSize = 10;   // Начальный размер пула для каждого типа врага
    [SerializeField] private Transform enemiesParent;    // Родительский объект для созданных врагов

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

    private Enemy InstantiateEnemy(Enemy prefab)
    {
        Enemy enemy = Instantiate(prefab, enemiesParent);
        enemy.OriginalPrefab = prefab;
        enemy.gameObject.SetActive(false);
        enemy.name = enemy.name.Replace("(Clone)", "").Trim();

        return enemy;
    }

    public Enemy GetEnemy(Enemy enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            return null;
        }

        // Если пула для данного префаба ещё нет, создаём его
        if (!enemyPools.ContainsKey(enemyPrefab))
        {
            Queue<Enemy> newPool = new Queue<Enemy>();
            for (int i = 0; i < initialPoolSize; i++)
            {
                Enemy enemy = InstantiateEnemy(enemyPrefab);
                newPool.Enqueue(enemy);
            }
            enemyPools[enemyPrefab] = newPool;
        }

        if (enemyPools.TryGetValue(enemyPrefab, out Queue<Enemy> pool))
        {
            if (pool.Count > 0)
            {
                Enemy enemy = pool.Dequeue();
                return enemy;
            }
            else
            {
                return InstantiateEnemy(enemyPrefab);
            }
        }

        return null;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        Enemy prefab = enemy.OriginalPrefab;
        if (prefab == null)
        {
            return;
        }

        // Если пула для этого префаба ещё нет, создаём его
        if (!enemyPools.ContainsKey(prefab))
        {
            enemyPools[prefab] = new Queue<Enemy>();
        }

        Queue<Enemy> pool = enemyPools[prefab];
        if (pool.Contains(enemy))
        {
            return;
        }

        // Сбрасываем состояние врага перед возвратом в пул
        ResetEnemyState(enemy);

        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
    }

    private void ResetEnemyState(Enemy enemy)
    {
        if (enemy == null) return;

        // Сбрасываем позицию и поворот
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;

        // Сбрасываем здоровье
        var healthSystem = enemy.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.SetCurrentHealth(healthSystem.GetMaxHealth());
        }

        // Сбрасываем физические параметры
        var rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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
