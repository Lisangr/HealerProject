using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnSettings
{
    public Enemy enemyPrefab;         // ѕрефаб врага, как он указан в пуле
    public int maxEnemies = 1;        // ћаксимальное число активных врагов данного типа
    public Transform[] spawnPoints;   // “очки, где может заспавнитьс€ враг

    [Tooltip("ћинимальное врем€ до спавна после убийства")]
    public float spawnDelayMin = 10f;

    [Tooltip("ћаксимальное врем€ до спавна (если значение больше минимального, выбираетс€ случайное врем€)")]
    public float spawnDelayMax = 10f;

    [Tooltip("ћинимальное врем€ до первого спавна")]
    public float initialSpawnDelayMin = 1f;

    [Tooltip("ћаксимальное врем€ до первого спавна")]
    public float initialSpawnDelayMax = 2f;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemySpawnSettings> enemySpawnSettings;
    [SerializeField] private float globalCheckInterval = 1f; // »нтервал проверки (например, 1 секунда)

    // ћассивы дл€ таймеров и текущих пороговых значений дл€ каждого типа врага
    private float[] spawnTimers;
    private float[] currentSpawnDelays;
    // ‘лаг, указывающий, что первый спавн уже произошЄл дл€ каждого типа
    private bool[] hasSpawnedOnce;

    private void Start()
    {
        // »нициализируем массивы, размер которых равен количеству настроек
        int count = enemySpawnSettings.Count;
        spawnTimers = new float[count];
        currentSpawnDelays = new float[count];
        hasSpawnedOnce = new bool[count];

        // ”станавливаем дл€ каждого типа врага начальную задержку спавна
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
                            Transform spawnPoint = settings.spawnPoints[Random.Range(0, settings.spawnPoints.Length)];
                            SpawnEnemyAt(spawnPoint, settings.enemyPrefab);
                        }
                        else
                        {
                            Debug.LogWarning($"Ќе заданы точки спавна дл€ врага {settings.enemyPrefab.name}");
                        }
                        // —брасываем таймер дл€ этого типа
                        spawnTimers[i] = 0f;
                        // ≈сли первый спавн уже произошЄл Ц используем обычные задержки, иначе продолжаем использовать начальные
                        if (!hasSpawnedOnce[i])
                        {
                            hasSpawnedOnce[i] = true;
                            currentSpawnDelays[i] = Random.Range(settings.spawnDelayMin, settings.spawnDelayMax);
                        }
                        else
                        {
                            currentSpawnDelays[i] = Random.Range(settings.spawnDelayMin, settings.spawnDelayMax);
                        }
                    }
                }
                else
                {
                    // ≈сли число активных врагов достигло максимума, сбрасываем таймер
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
            Debug.LogError("EnemyPool не найден на сцене!");
            return;
        }

        Enemy enemy = pool.GetEnemy(enemyPrefab);
        if (enemy == null)
        {
            Debug.LogError("Ќе удалось получить врага из пула.");
            return;
        }

        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;
        enemy.gameObject.SetActive(true);
    }

    // ѕодсчитываем количество активных врагов данного типа (проверка по свойству OriginalPrefab)
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