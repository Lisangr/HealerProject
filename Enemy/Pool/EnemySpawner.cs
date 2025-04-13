using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnSettings
{
    public Enemy enemyPrefab;         // Префаб врага, который будет появляться
    public int maxEnemies = 1;        // Максимальное количество одновременно активных врагов
    public Transform[] spawnPoints;   // Точки, в которых будут появляться враги

    [Tooltip("Задержка между спавнами врагов после первого спавна")]
    public float spawnDelayMin = 10f;

    [Tooltip("Максимальная задержка между спавнами врагов после первого спавна (для рандома)")]
    public float spawnDelayMax = 10f;

    [Tooltip("Начальная задержка перед первым спавном врагов")]
    public float initialSpawnDelayMin = 1f;

    [Tooltip("Максимальная начальная задержка перед первым спавном врагов")]
    public float initialSpawnDelayMax = 2f;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemySpawnSettings> enemySpawnSettings;
    [SerializeField] private float globalCheckInterval = 1f; // Интервал глобальной проверки (например, 1 секунда)

    // Таймеры для каждого типа врага
    private float[] spawnTimers;
    private float[] currentSpawnDelays;
    // Флаги, обозначающие, что враг уже был заспавнен хотя бы один раз
    private bool[] hasSpawnedOnce;

    private void Start()
    {
        int count = enemySpawnSettings.Count;
        spawnTimers = new float[count];
        currentSpawnDelays = new float[count];
        hasSpawnedOnce = new bool[count];

        for (int i = 0; i < count; i++)
        {
            currentSpawnDelays[i] = Random.Range(enemySpawnSettings[i].initialSpawnDelayMin, enemySpawnSettings[i].initialSpawnDelayMax);
            hasSpawnedOnce[i] = false;
        }

        StartCoroutine(SpawnEnemiesCoroutine());
    }

    private IEnumerator SpawnEnemiesCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < enemySpawnSettings.Count; i++)
            {
                spawnTimers[i] += globalCheckInterval;
                EnemySpawnSettings settings = enemySpawnSettings[i];

                if (CountActiveEnemies(settings.enemyPrefab) < settings.maxEnemies)
                {
                    if (spawnTimers[i] >= currentSpawnDelays[i])
                    {
                        if (settings.spawnPoints != null && settings.spawnPoints.Length > 0)
                        {
                            // Выбираем случайную точку из массива
                            int index = Random.Range(0, settings.spawnPoints.Length);
                            Transform spawnPoint = settings.spawnPoints[index];

                            // Проверка, что выбранная точка не равна null
                            if (spawnPoint == null)
                            {
                                Debug.LogError($"SpawnPoint под индексом {index} для префаба {settings.enemyPrefab.name} равен null!");
                            }
                            else
                            {
                                SpawnEnemyAt(spawnPoint, settings.enemyPrefab);
                            }
                        }

                        // Сброс таймера спавна
                        spawnTimers[i] = 0f;
                        // Обновление задержки спавна (для первого спавна или последующих)
                        currentSpawnDelays[i] = Random.Range(settings.spawnDelayMin, settings.spawnDelayMax);
                        hasSpawnedOnce[i] = true;
                    }
                }
                else
                {
                    // Если количество активных врагов превышает maxEnemies — сбрасываем таймер
                    spawnTimers[i] = 0f;
                }
            }
            yield return new WaitForSeconds(globalCheckInterval);
        }
    }

    private void SpawnEnemyAt(Transform spawnPoint, Enemy enemyPrefab)
    {
        EnemyPool pool = FindObjectOfType<EnemyPool>();
        if (pool == null)
        {
            return;
        }

        Enemy enemy = pool.GetEnemy(enemyPrefab);
        if (enemy == null)
        {
            return;
        }

        // Устанавливаем позицию и поворот на основе spawnPoint
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;

        // Активируем объект
        enemy.gameObject.SetActive(true);

        // Инициализируем систему здоровья (если есть)
        var healthSystem = enemy.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.RefreshFromConfig();
        }

        // Сбрасываем физическое состояние (скорость и вращение)
        var rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private int CountActiveEnemies(Enemy enemyPrefab)
    {
        int count = 0;
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy e in allEnemies)
        {
            if (e.gameObject.activeInHierarchy && e.OriginalPrefab == enemyPrefab)
            {
                count++;
            }
        }
        return count;
    }
}
