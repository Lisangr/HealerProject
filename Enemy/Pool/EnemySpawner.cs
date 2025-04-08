using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnSettings
{
    public Enemy enemyPrefab;         // ������ �����, ��� �� ������ � ����
    public int maxEnemies = 1;        // ������������ ����� �������� ������ ������� ����
    public Transform[] spawnPoints;   // �����, ��� ����� ������������ ����

    [Tooltip("����������� ����� �� ������ ����� ��������")]
    public float spawnDelayMin = 10f;

    [Tooltip("������������ ����� �� ������ (���� �������� ������ ������������, ���������� ��������� �����)")]
    public float spawnDelayMax = 10f;

    [Tooltip("����������� ����� �� ������� ������")]
    public float initialSpawnDelayMin = 1f;

    [Tooltip("������������ ����� �� ������� ������")]
    public float initialSpawnDelayMax = 2f;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemySpawnSettings> enemySpawnSettings;
    [SerializeField] private float globalCheckInterval = 1f; // �������� �������� (��������, 1 �������)

    // ������� ��� �������� � ������� ��������� �������� ��� ������� ���� �����
    private float[] spawnTimers;
    private float[] currentSpawnDelays;
    // ����, �����������, ��� ������ ����� ��� ��������� ��� ������� ����
    private bool[] hasSpawnedOnce;

    private void Start()
    {
        // �������������� �������, ������ ������� ����� ���������� ��������
        int count = enemySpawnSettings.Count;
        spawnTimers = new float[count];
        currentSpawnDelays = new float[count];
        hasSpawnedOnce = new bool[count];

        // ������������� ��� ������� ���� ����� ��������� �������� ������
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
                            Debug.LogWarning($"�� ������ ����� ������ ��� ����� {settings.enemyPrefab.name}");
                        }
                        // ���������� ������ ��� ����� ����
                        spawnTimers[i] = 0f;
                        // ���� ������ ����� ��� ��������� � ���������� ������� ��������, ����� ���������� ������������ ���������
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
                    // ���� ����� �������� ������ �������� ���������, ���������� ������
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
            Debug.LogError($"Не удалось получить врага из пула для префаба {enemyPrefab.name}");
            return;
        }

        // Устанавливаем позицию и поворот
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;
        
        // Активируем объект
        enemy.gameObject.SetActive(true);
        
        // Инициализируем врага
        var healthSystem = enemy.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.RefreshFromConfig();
        }

        // Сбрасываем состояние врага
        var rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"Создан враг {enemy.name} в позиции {spawnPoint.position}");
    }

    //       (   OriginalPrefab)
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