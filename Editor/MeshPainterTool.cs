using UnityEngine;
using UnityEditor;

public class MeshPainterTool : EditorWindow
{
    // Настройки кисти
    private float brushSize = 1f;
    // Плотность объектов (количество объектов на единицу площади)
    private float density = 1f;

    // Массив префабов и их вероятностей
    public GameObject[] prefabs;
    public float[] prefabWeights; // Вес для каждого префаба (вероятность его появления)

    // Новые поля:
    // Родитель, в котором будут размещаться созданные объекты
    public Transform spawnParent;
    // Область спавна – если кисть (и создаваемые объекты) выходят за пределы этого коллайдера, спавн не производится
    public Collider spawnArea;

    private bool isPainting = false;

    [MenuItem("Tools/Mesh Painter")]
    public static void ShowWindow()
    {
        GetWindow<MeshPainterTool>("Mesh Painter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Настройки кисти", EditorStyles.boldLabel);
        brushSize = EditorGUILayout.FloatField("Размер кисти", brushSize);
        density = EditorGUILayout.FloatField("Плотность", density);

        // Отображение массивов префабов и весов
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        SerializedProperty weightsProperty = serializedObject.FindProperty("prefabWeights");
        EditorGUILayout.PropertyField(prefabsProperty, true);
        EditorGUILayout.PropertyField(weightsProperty, true);

        // Новые поля для родителя спавна и области спавна
        spawnParent = (Transform)EditorGUILayout.ObjectField("Родитель спавна", spawnParent, typeof(Transform), true);
        spawnArea = (Collider)EditorGUILayout.ObjectField("Область спавна", spawnArea, typeof(Collider), true);

        serializedObject.ApplyModifiedProperties();

        // Валидация массивов
        if (prefabs == null || prefabWeights == null || prefabs.Length == 0 || prefabWeights.Length == 0)
        {
            EditorGUILayout.HelpBox("Пожалуйста, добавьте префабы и их веса.", MessageType.Warning);
        }
        else if (prefabs.Length != prefabWeights.Length)
        {
            EditorGUILayout.HelpBox("Массивы префабов и их весов должны иметь одинаковую длину.", MessageType.Error);
        }

        // Запуск или остановка рисования в сцене
        if (!isPainting)
        {
            if (GUILayout.Button("Начать рисовать"))
            {
                isPainting = true;
                SceneView.duringSceneGui += OnSceneGUI;
            }
        }
        else
        {
            if (GUILayout.Button("Остановить рисование"))
            {
                isPainting = false;
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        // Преобразуем позицию курсора в луч
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Если область спавна задана, проверяем, находится ли hit.point внутри её границ
            if (spawnArea != null && !spawnArea.bounds.Contains(hit.point))
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(hit.point, hit.normal, brushSize);
                Handles.Label(hit.point, "Кисть выходит за пределы области спавна", EditorStyles.boldLabel);
            }
            else
            {
                Handles.color = e.shift ? Color.red : Color.green;
                Handles.DrawWireDisc(hit.point, hit.normal, brushSize);
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (e.shift)
                {
                    // Режим удаления: находим все объекты в радиусе кисти
                    if (prefabs != null)
                    {
                        Collider[] colliders = Physics.OverlapSphere(hit.point, brushSize);
                        foreach (Collider col in colliders)
                        {
                            GameObject go = col.gameObject;
                            // Проверяем, соответствует ли объект исходному префабу первого элемента (пример проверки)
                            if (PrefabUtility.GetCorrespondingObjectFromSource(go) == prefabs[0])
                            {
                                Undo.DestroyObjectImmediate(go);
                            }
                        }
                    }
                }
                else if (!e.alt)
                {
                    // Режим рисования: проверка, что hit.point находится внутри области спавна (если задана)
                    if (spawnArea != null && !spawnArea.bounds.Contains(hit.point))
                    {
                        Debug.LogWarning("Кисть за пределами области спавна. Невозможно разместить объекты.");
                        e.Use();
                        return;
                    }

                    // Рисование: создаём несколько объектов в пределах кисти
                    if (prefabs != null && prefabs.Length > 0)
                    {
                        int count = Mathf.CeilToInt(Mathf.PI * brushSize * brushSize * density);
                        for (int i = 0; i < count; i++)
                        {
                            // Генерируем случайное смещение внутри единичного круга, масштабируя по размеру кисти
                            Vector2 randomCircle = Random.insideUnitCircle * brushSize;

                            // Создаём базис для плоскости, перпендикулярной hit.normal
                            Vector3 tangent = Vector3.Cross(hit.normal, Vector3.up);
                            if (tangent == Vector3.zero)
                            {
                                tangent = Vector3.Cross(hit.normal, Vector3.right);
                            }
                            tangent.Normalize();
                            Vector3 bitangent = Vector3.Cross(hit.normal, tangent);

                            Vector3 offset = tangent * randomCircle.x + bitangent * randomCircle.y;
                            // Определяем позицию создания объекта
                            Vector3 spawnPosition = hit.point + offset;

                            // Если область спавна указана, проверяем, что позиция спавна внутри неё
                            if (spawnArea != null && !spawnArea.bounds.Contains(spawnPosition))
                            {
                                Debug.Log($"Позиция для спавна {spawnPosition} вне области спавна. Пропуск.");
                                continue;
                            }

                            // Проверка на пересечение с уже созданными объектами (пример проверки)
                            Collider[] colliders = Physics.OverlapSphere(spawnPosition, 0.5f);
                            bool isOverlap = false;
                            foreach (var col in colliders)
                            {
                                if (PrefabUtility.GetCorrespondingObjectFromSource(col.gameObject) == prefabs[0])
                                {
                                    isOverlap = true;
                                    break;
                                }
                            }
                            if (isOverlap)
                            {
                                // Попытка скорректировать позицию, если имеется пересечение
                                Vector2 newRandomCircle = Random.insideUnitCircle * brushSize;
                                spawnPosition = hit.point + tangent * newRandomCircle.x + bitangent * newRandomCircle.y;
                                if (spawnArea != null && !spawnArea.bounds.Contains(spawnPosition))
                                {
                                    Debug.Log($"Позиция для спавна {spawnPosition} вне области спавна после коррекции. Пропуск.");
                                    continue;
                                }
                            }

                            // Выбор случайного префаба с учетом вероятности
                            GameObject prefabToSpawn = GetRandomPrefab();
                            if (prefabToSpawn == null)
                            {
                                Debug.LogError("Не найден подходящий префаб для спавна.");
                                continue;
                            }

                            // Создаём экземпляр выбранного префаба и регистрируем его для возможности отмены действия
                            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                            Undo.RegisterCreatedObjectUndo(obj, "Place Object");
                            obj.transform.position = spawnPosition;

                            // Если указан родитель спавна, задаём его
                            if (spawnParent != null)
                            {
                                obj.transform.SetParent(spawnParent);
                            }
                        }
                    }
                }
                e.Use();
            }
        }
        sceneView.Repaint();
    }

    // Функция выбора случайного префаба с учетом вероятностей
    private GameObject GetRandomPrefab()
    {
        if (prefabs == null || prefabs.Length == 0)
            return null;

        float totalWeight = 0;
        foreach (float weight in prefabWeights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        for (int i = 0; i < prefabs.Length; i++)
        {
            cumulativeWeight += prefabWeights[i];
            if (randomValue <= cumulativeWeight)
            {
                return prefabs[i];
            }
        }
        return prefabs[prefabs.Length - 1];
    }
}
