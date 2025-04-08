using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GridSpawner : MonoBehaviour
{
    [Header("Настройки сетки")]
    public float gridSpacing = 2.0f; // Расстояние между точками сетки по оси X и Z
    public int gridSizeX = 10; // Количество точек по оси X
    public int gridSizeZ = 10; // Количество точек по оси Z

    [Header("Префабы для спавна")]
    public GameObject[] prefabArray; // Массив префабов
    public float spawnFrequency = 1f; // Частота спавна (в секундах)

    [Header("Ограничения")]
    public int maxObjectsToSpawn = 100; // Максимальное количество объектов для спавна

    // Параметры для случайного размещения
    [Range(0f, 1f)]
    public float spawnChance = 0.5f; // Шанс появления объекта в точке сетки
    [Range(0f, 1f)]
    public float skipChance = 0.5f; // Вероятность пропуска тайла

    // Укажите слой для Ground объектов
    public LayerMask groundLayer;

    // Добавляем маску для игнорирования других слоев, например, Default
    public LayerMask ignoreLayers;

    private List<Vector3> validSpawnPositions = new List<Vector3>(); // Список позиций, куда можно заспавнить объекты
    private int spawnedObjectsCount = 0; // Счётчик заспавненных объектов
    private Transform parentObject; // Родительский объект для хранения заспавленных префабов

    // Этот метод будет вызываться кнопкой в редакторе
    public void SpawnPrefabsInEditor()
    {
        // Сбрасываем счетчик заспавненных объектов перед новым запуском
        spawnedObjectsCount = 0;

        // Проверяем, не превышает ли количество объектов на сцене допустимый максимум
        if (spawnedObjectsCount >= maxObjectsToSpawn)
        {
            Debug.LogWarning($"Превышено максимальное количество объектов для спавна! Уже заспавнено {spawnedObjectsCount} из {maxObjectsToSpawn}.");
            return;
        }

        validSpawnPositions.Clear(); // Очищаем список валидных позиций перед новым запуском

        // Создаем родительский объект для хранения заспавненных объектов
        if (parentObject == null)
        {
            parentObject = new GameObject("SpawnedObjects").transform;
        }
        else
        {
            // Очищаем родительский объект, если он уже существует
            foreach (Transform child in parentObject)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // Получаем начальную позицию для сетки из позиции объекта, на котором висит скрипт
        Vector3 startPosition = transform.position;

        // Строим сетку с помощью лучей
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // Генерируем точку для спавна с учётом позиции объекта
                Vector3 spawnCheckPosition = new Vector3(startPosition.x + x * gridSpacing, 1000f, startPosition.z + z * gridSpacing);

                // Пускаем луч вниз от каждой позиции
                RaycastHit hit;
                if (Physics.Raycast(spawnCheckPosition, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                {
                    // Проверяем, что луч не пересекает другие слои, указанные в ignoreLayers
                    if (((1 << hit.collider.gameObject.layer) & ignoreLayers) != 0)
                    {
                        continue;
                    }

                    // Если луч пересекает землю, сохраняем точку в список
                    validSpawnPositions.Add(hit.point);
                    Debug.Log($"Луч пересёк объект на высоте Y: {hit.point.y} в позиции {hit.point}");
                }
            }
        }

        // Теперь спавним объекты в этих позициях с учетом плотности и вероятности
        foreach (Vector3 position in validSpawnPositions)
        {
            if (spawnedObjectsCount >= maxObjectsToSpawn)
            {
                Debug.LogWarning($"Превышено максимальное количество объектов для спавна! Уже заспавнено {spawnedObjectsCount} из {maxObjectsToSpawn}.");
                break;
            }

            // Пропускаем тайлы с вероятностью skipChance
            if (Random.value <= skipChance)
            {
                continue;
            }

            // Спавним объекты с вероятностью spawnChance
            if (Random.value <= spawnChance)
            {
                // Внесем случайное смещение для X и Z, чтобы предотвратить выстраивание объектов в одну линию
                float randomOffsetX = Random.Range(-gridSpacing / 2f, gridSpacing / 2f);
                float randomOffsetZ = Random.Range(-gridSpacing / 2f, gridSpacing / 2f);

                // Корректируем позицию с учетом смещения
                Vector3 adjustedPosition = new Vector3(position.x + randomOffsetX, position.y, position.z + randomOffsetZ);

                // Выбираем случайный префаб
                GameObject prefabToSpawn = prefabArray[Random.Range(0, prefabArray.Length)];

#if UNITY_EDITOR
            // Инстанцируем префаб в сцене и устанавливаем его родителем
            GameObject spawnedObject = PrefabUtility.InstantiatePrefab(prefabToSpawn, parentObject) as GameObject;
            if (spawnedObject != null)
            {
                spawnedObject.transform.position = adjustedPosition;
                spawnedObject.transform.rotation = Quaternion.identity;
            }
#endif

                // Увеличиваем счётчик заспавненных объектов
                spawnedObjectsCount++;
            }
        }
    }


    // Рисуем сетку для визуализации в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        // Получаем начальную позицию для сетки из позиции объекта, на котором висит скрипт
        Vector3 startPosition = transform.position;

        // Рисуем сетку, используя сохранённые валидные позиции
        foreach (Vector3 position in validSpawnPositions)
        {
            Gizmos.DrawWireSphere(position, 0.1f); // Рисуем маленький кружок в каждой точке
        }
    }
}





































/*using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GridSpawner : MonoBehaviour
{
    [Header("Настройки сетки")]
    public float gridSpacing = 2.0f; // Расстояние между точками сетки по оси X и Z
    public int gridSizeX = 10; // Количество точек по оси X
    public int gridSizeZ = 10; // Количество точек по оси Z

    [Header("Префабы для спавна")]
    public GameObject[] prefabArray; // Массив префабов
    public float spawnFrequency = 1f; // Частота спавна (в секундах)

    [Header("Ограничения")]
    public int maxObjectsToSpawn = 100; // Максимальное количество объектов для спавна

    // Параметры для случайного размещения
    [Range(0f, 1f)]
    public float spawnChance = 0.5f; // Шанс появления объекта в точке сетки
    [Range(0f, 1f)]
    public float skipChance = 0.5f; // Вероятность пропуска тайла

    // Укажите слой для Ground объектов
    public LayerMask groundLayer;

    // Добавляем маску для игнорирования других слоев, например, Default
    public LayerMask ignoreLayers;

    private List<Vector3> validSpawnPositions = new List<Vector3>(); // Список позиций, куда можно заспавнить объекты
    private int spawnedObjectsCount = 0; // Счётчик заспавненных объектов
    private Transform parentObject; // Родительский объект для хранения заспавленных префабов

    // Этот метод будет вызываться кнопкой в редакторе
    public void SpawnPrefabsInEditor()
    {
        // Сбрасываем счетчик заспавненных объектов перед новым запуском
        spawnedObjectsCount = 0;

        // Проверяем, не превышает ли количество объектов на сцене допустимый максимум
        if (spawnedObjectsCount >= maxObjectsToSpawn)
        {
            Debug.LogWarning($"Превышено максимальное количество объектов для спавна! Уже заспавнено {spawnedObjectsCount} из {maxObjectsToSpawn}.");
            return;
        }

        validSpawnPositions.Clear(); // Очищаем список валидных позиций перед новым запуском

        // Создаем родительский объект для хранения заспавненных объектов
        if (parentObject == null)
        {
            parentObject = new GameObject("SpawnedObjects").transform;
        }
        else
        {
            // Очищаем родительский объект, если он уже существует
            foreach (Transform child in parentObject)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // Получаем начальную позицию для сетки из позиции объекта, на котором висит скрипт
        Vector3 startPosition = transform.position;

        // Строим сетку с помощью лучей
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // Генерируем точку для спавна с учётом позиции объекта
                Vector3 spawnCheckPosition = new Vector3(startPosition.x + x * gridSpacing, 1000f, startPosition.z + z * gridSpacing);

                // Пускаем луч вниз от каждой позиции
                RaycastHit hit;
                if (Physics.Raycast(spawnCheckPosition, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                {
                    // Проверяем, что луч не пересекает другие слои, указанные в ignoreLayers
                    if (((1 << hit.collider.gameObject.layer) & ignoreLayers) != 0)
                    {
                        // Если пересекает, пропускаем этот тайл
                        continue;
                    }

                    // Если луч пересекает землю, сохраняем точку в список
                    validSpawnPositions.Add(hit.point);
                    Debug.Log($"Луч пересёк объект на высоте Y: {hit.point.y} в позиции {hit.point}");
                }
            }
        }

        // Теперь спавним объекты в этих позициях с учетом плотности и вероятности
        foreach (Vector3 position in validSpawnPositions)
        {
            if (spawnedObjectsCount >= maxObjectsToSpawn)
            {
                Debug.LogWarning($"Превышено максимальное количество объектов для спавна! Уже заспавнено {spawnedObjectsCount} из {maxObjectsToSpawn}.");
                break;
            }

            // Пропускаем тайлы с вероятностью skipChance
            if (Random.value <= skipChance)
            {
                continue;
            }

            // Спавним объекты с вероятностью spawnChance
            if (Random.value <= spawnChance)
            {
                // Внесем случайное смещение для X и Z, чтобы предотвратить выстраивание объектов в одну линию
                float randomOffsetX = Random.Range(-gridSpacing / 2f, gridSpacing / 2f);
                float randomOffsetZ = Random.Range(-gridSpacing / 2f, gridSpacing / 2f);

                // Корректируем позицию с учетом смещения
                Vector3 adjustedPosition = new Vector3(position.x + randomOffsetX, position.y, position.z + randomOffsetZ);

                // Выбираем случайный префаб
                GameObject prefabToSpawn = prefabArray[Random.Range(0, prefabArray.Length)];

                // Спавним объект и устанавливаем его родителем
                GameObject spawnedObject = Instantiate(prefabToSpawn, adjustedPosition, Quaternion.identity);
                spawnedObject.transform.SetParent(parentObject); // Устанавливаем родительский объект для управления

                // Увеличиваем счётчик заспавненных объектов
                spawnedObjectsCount++;
            }
        }
    }

    // Рисуем сетку для визуализации в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        // Получаем начальную позицию для сетки из позиции объекта, на котором висит скрипт
        Vector3 startPosition = transform.position;

        // Рисуем сетку, используя сохранённые валидные позиции
        foreach (Vector3 position in validSpawnPositions)
        {
            Gizmos.DrawWireSphere(position, 0.1f); // Рисуем маленький кружок в каждой точке
        }
    }
}*/