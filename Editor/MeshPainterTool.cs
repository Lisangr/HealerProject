using UnityEngine;
using UnityEditor;

public class MeshPainterTool : EditorWindow
{
    // Настройки кисти
    private float brushSize = 1f;
    // Плотность объектов (количество объектов на единицу площади)
    private float density = 1f;

    // Массив префабов и их вероятности
    public GameObject[] prefabs;
    public float[] prefabWeights; // Вес для каждого префаба (вероятность его появления)

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

        // Выводим массив префабов
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        SerializedProperty weightsProperty = serializedObject.FindProperty("prefabWeights");

        EditorGUILayout.PropertyField(prefabsProperty, true);
        EditorGUILayout.PropertyField(weightsProperty, true);
        serializedObject.ApplyModifiedProperties();

        // Проверка на null или пустоту массивов
        if (prefabs == null || prefabWeights == null || prefabs.Length == 0 || prefabWeights.Length == 0)
        {
            EditorGUILayout.HelpBox("Пожалуйста, добавьте префабы и их веса в массивы.", MessageType.Warning);
        }
        else if (prefabs.Length != prefabWeights.Length)
        {
            EditorGUILayout.HelpBox("Массивы префабов и их весов должны иметь одинаковую длину.", MessageType.Error);
        }

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
            // Меняем цвет кисти в зависимости от режима (рисование – зелёный, удаление – красный)
            Handles.color = e.shift ? Color.red : Color.green;
            Handles.DrawWireDisc(hit.point, hit.normal, brushSize);

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
                            // Проверяем, соответствует ли объект источнику выбранного префаба
                            if (PrefabUtility.GetCorrespondingObjectFromSource(go) == prefabs[0])
                            {
                                Undo.DestroyObjectImmediate(go);
                            }
                        }
                    }
                }
                else if (!e.alt)
                {
                    // Режим рисования: создаём несколько объектов в пределах кисти
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

                            // Проверка на пересечение
                            Collider[] colliders = Physics.OverlapSphere(spawnPosition, 0.5f);  // Радиус проверки
                            bool isOverlap = false;

                            foreach (var col in colliders)
                            {
                                if (PrefabUtility.GetCorrespondingObjectFromSource(col.gameObject) == prefabs[0])
                                {
                                    isOverlap = true;
                                    break;
                                }
                            }

                            // Если есть пересечение, смещаем позицию
                            if (isOverlap)
                            {
                                // Попробуем сместить объект в случайном направлении
                                Vector2 newRandomCircle = Random.insideUnitCircle * brushSize;
                                spawnPosition = hit.point + tangent * newRandomCircle.x + bitangent * newRandomCircle.y;
                            }

                            // Выбор случайного префаба с учетом вероятности
                            GameObject prefabToSpawn = GetRandomPrefab();

                            // Создаём экземпляр выбранного префаба и регистрируем его для возможности отмены действия
                            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                            Undo.RegisterCreatedObjectUndo(obj, "Place Object");
                            obj.transform.position = spawnPosition;
                        }
                    }
                }
                e.Use();
            }
        }
        sceneView.Repaint();
    }

    // Функция для выбора случайного префаба с учетом вероятности
    private GameObject GetRandomPrefab()
    {
        if (prefabs == null || prefabs.Length == 0)
            return null;

        // Нормализуем вероятности
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

        return prefabs[prefabs.Length - 1];  // На случай, если случайное значение больше суммы весов
    }
}
