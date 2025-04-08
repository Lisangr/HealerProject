using UnityEngine;
using UnityEditor;

public class ObjectSpawnerAroundChildren : EditorWindow
{
    public GameObject targetObject; // ������, � �������� ����� ����������� �����
    public GameObject[] prefabs; // �������, ������� ����� ���������
    public int prefabsCount = 8; // ���������� ��������, ������� ����� ��������� ������ ������� �������
    public float radius = 2f; // ������ ��� ������������� ��������
    public GameObject parentObject; // ����� �������� ��� ������������ ��������

    [MenuItem("Tools/Spawn Prefabs Around Children")]
    public static void ShowWindow()
    {
        GetWindow<ObjectSpawnerAroundChildren>("Spawn Prefabs Around Children");
    }

    private void OnGUI()
    {
        GUILayout.Label("��������� ���������� ��������", EditorStyles.boldLabel);
        targetObject = (GameObject)EditorGUILayout.ObjectField("������� ������", targetObject, typeof(GameObject), true);
        prefabsCount = EditorGUILayout.IntField("���������� �������� ������ ������� �������", prefabsCount);
        radius = EditorGUILayout.FloatField("������ ����������", radius);
        parentObject = (GameObject)EditorGUILayout.ObjectField("�������� ��� ������������ ��������", parentObject, typeof(GameObject), true);

        // ������ ��������
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        EditorGUILayout.PropertyField(prefabsProperty, true);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("���������� ������� ������ �����"))
        {
            PlacePrefabsAroundChildren();
        }
    }

    private void PlacePrefabsAroundChildren()
    {
        if (targetObject == null || prefabs == null || prefabs.Length == 0 || parentObject == null)
        {
            Debug.LogError("����������, ������� ������� ������, ������� � ��������.");
            return;
        }

        // �������� ���� ����� �������� �������
        Transform[] children = targetObject.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child == targetObject.transform) // ���������� ��� ������
                continue;

            // ����������� ������� ������ �������� ��������� �������
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

            // �������� ��������� ������ �� �������
            GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

            // ������� ��������� �������
            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            prefabInstance.transform.position = child.position + offset;
            prefabInstance.transform.rotation = Quaternion.LookRotation(child.position - prefabInstance.transform.position);

            // �������� ��������� ������ � ������������ ������
            prefabInstance.transform.SetParent(parentObject.transform);

            Undo.RegisterCreatedObjectUndo(prefabInstance, "Place Prefab");
        }
    }
}
