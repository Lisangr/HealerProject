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
        if (enemyPrefab == null)
        {
            Debug.LogError("Попытка получить врага из null префаба!");
            return null;
        }

        // Если префаба нет в пуле, создаем для него новый пул
        if (!enemyPools.ContainsKey(enemyPrefab))
        {
            Debug.Log($"Создаем новый пул для префаба {enemyPrefab.name}");
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
                Debug.Log($"Извлечение врага {enemy.name} из пула. Осталось в пуле: {pool.Count}");
                return enemy;
            }
            else
            {
                Debug.Log($"Создание нового врага {enemyPrefab.name}, так как пул пуст");
                return InstantiateEnemy(enemyPrefab);
            }
        }

        Debug.LogError($"Не удалось создать или получить пул для префаба {enemyPrefab.name}!");
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
        if (prefab == null)
        {
            Debug.LogError($"У врага {enemy.name} не задан OriginalPrefab!");
            return;
        }

        // Если пула для этого префаба еще нет, создаем его
        if (!enemyPools.ContainsKey(prefab))
        {
            Debug.Log($"Создаем новый пул для префаба {prefab.name}");
            enemyPools[prefab] = new Queue<Enemy>();
        }

        Queue<Enemy> pool = enemyPools[prefab];
        if (pool.Contains(enemy))
        {
            Debug.LogWarning($"Враг {enemy.name} уже находится в пуле");
            return;
        }

        // Сбрасываем состояние врага перед возвратом в пул
        ResetEnemyState(enemy);
        
        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
        
        Debug.Log($"Враг {enemy.name} успешно возвращен в пул. Текущий размер пула: {pool.Count}");
    }

    private void ResetEnemyState(Enemy enemy)
    {
        if (enemy == null) return;

        // Сбрасываем все необходимые параметры врага
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;
        
        // Сбрасываем здоровье
        var healthSystem = enemy.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.SetCurrentHealth(healthSystem.GetMaxHealth());
        }

        // Сбрасываем другие компоненты, если необходимо
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