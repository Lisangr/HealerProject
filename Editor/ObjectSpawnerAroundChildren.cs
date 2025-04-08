using UnityEngine;
using UnityEditor;

public class ObjectSpawnerAroundChildren : EditorWindow
{
    public GameObject targetObject; // Объект, у которого будем сканировать детей
    public GameObject[] prefabs; // Префабы, которые будем размещать
    public int prefabsCount = 8; // Количество префабов, которые будем размещать вокруг каждого ребенка
    public float radius = 2f; // Радиус для распределения префабов
    public GameObject parentObject; // Новый родитель для заспавненных объектов

    [MenuItem("Tools/Spawn Prefabs Around Children")]
    public static void ShowWindow()
    {
        GetWindow<ObjectSpawnerAroundChildren>("Spawn Prefabs Around Children");
    }

    private void OnGUI()
    {
        GUILayout.Label("Настройки размещения префабов", EditorStyles.boldLabel);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Целевой объект", targetObject, typeof(GameObject), true);
        prefabsCount = EditorGUILayout.IntField("Количество префабов вокруг каждого ребенка", prefabsCount);
        radius = EditorGUILayout.FloatField("Радиус размещения", radius);
        parentObject = (GameObject)EditorGUILayout.ObjectField("Родитель для заспавненных объектов", parentObject, typeof(GameObject), true);

        // Массив префабов
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        EditorGUILayout.PropertyField(prefabsProperty, true);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Разместить префабы вокруг детей"))
        {
            PlacePrefabsAroundChildren();
        }
    }

    private void PlacePrefabsAroundChildren()
    {
        if (targetObject == null || prefabs == null || prefabs.Length == 0 || parentObject == null)
        {
            Debug.LogError("Пожалуйста, укажите целевой объект, префабы и родителя.");
            return;
        }

        // Получаем всех детей целевого объекта
        Transform[] children = targetObject.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child == targetObject.transform) // Пропускаем сам объект
                continue;

            // Располагаем префабы вокруг текущего дочернего объекта
            PlacePrefabsAroundChild(child);
        }
    }

    private void PlacePrefabsAroundChild(Transform child)
    {
        float angleStep = 360f / prefabsCount;
        for (int i = 0; i < prefabsCount; i++)
        {
            float angle = i * angleStep;
            Vector3 offset = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle)) * radius;

            // Выбираем случайный префаб из массива
            GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

            // Создаем экземпляр префаба
            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            prefabInstance.transform.position = child.position + offset;
            prefabInstance.transform.rotation = Quaternion.LookRotation(child.position - prefabInstance.transform.position);

            // Помещаем созданный объект в родительский объект
            prefabInstance.transform.SetParent(parentObject.transform);

            Undo.RegisterCreatedObjectUndo(prefabInstance, "Place Prefab");
        }
    }
}
