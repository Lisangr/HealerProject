using UnityEngine;
using UnityEditor;

public class MeshPainterTool : EditorWindow
{
    // ��������� �����
    private float brushSize = 1f;
    // ��������� �������� (���������� �������� �� ������� �������)
    private float density = 1f;

    // ������ �������� � �� ������������
    public GameObject[] prefabs;
    public float[] prefabWeights; // ��� ��� ������� ������� (����������� ��� ���������)

    // ����� ����:
    // ��������, � ������� ����� ����������� ��������� �������
    public Transform spawnParent;
    // ������� ������ � ���� ����� (� ����������� �������) ������� �� ������� ����� ����������, ����� �� ������������
    public Collider spawnArea;

    private bool isPainting = false;

    [MenuItem("Tools/Mesh Painter")]
    public static void ShowWindow()
    {
        GetWindow<MeshPainterTool>("Mesh Painter");
    }

    private void OnGUI()
    {
        GUILayout.Label("��������� �����", EditorStyles.boldLabel);
        brushSize = EditorGUILayout.FloatField("������ �����", brushSize);
        density = EditorGUILayout.FloatField("���������", density);

        // ����������� �������� �������� � �����
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        SerializedProperty weightsProperty = serializedObject.FindProperty("prefabWeights");
        EditorGUILayout.PropertyField(prefabsProperty, true);
        EditorGUILayout.PropertyField(weightsProperty, true);

        // ����� ���� ��� �������� ������ � ������� ������
        spawnParent = (Transform)EditorGUILayout.ObjectField("�������� ������", spawnParent, typeof(Transform), true);
        spawnArea = (Collider)EditorGUILayout.ObjectField("������� ������", spawnArea, typeof(Collider), true);

        serializedObject.ApplyModifiedProperties();

        // ��������� ��������
        if (prefabs == null || prefabWeights == null || prefabs.Length == 0 || prefabWeights.Length == 0)
        {
            EditorGUILayout.HelpBox("����������, �������� ������� � �� ����.", MessageType.Warning);
        }
        else if (prefabs.Length != prefabWeights.Length)
        {
            EditorGUILayout.HelpBox("������� �������� � �� ����� ������ ����� ���������� �����.", MessageType.Error);
        }

        // ������ ��� ��������� ��������� � �����
        if (!isPainting)
        {
            if (GUILayout.Button("������ ��������"))
            {
                isPainting = true;
                SceneView.duringSceneGui += OnSceneGUI;
            }
        }
        else
        {
            if (GUILayout.Button("���������� ���������"))
            {
                isPainting = false;
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        // ����������� ������� ������� � ���
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // ���� ������� ������ ������, ���������, ��������� �� hit.point ������ � ������
            if (spawnArea != null && !spawnArea.bounds.Contains(hit.point))
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(hit.point, hit.normal, brushSize);
                Handles.Label(hit.point, "����� ������� �� ������� ������� ������", EditorStyles.boldLabel);
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
                    // ����� ��������: ������� ��� ������� � ������� �����
                    if (prefabs != null)
                    {
                        Collider[] colliders = Physics.OverlapSphere(hit.point, brushSize);
                        foreach (Collider col in colliders)
                        {
                            GameObject go = col.gameObject;
                            // ���������, ������������� �� ������ ��������� ������� ������� �������� (������ ��������)
                            if (PrefabUtility.GetCorrespondingObjectFromSource(go) == prefabs[0])
                            {
                                Undo.DestroyObjectImmediate(go);
                            }
                        }
                    }
                }
                else if (!e.alt)
                {
                    // ����� ���������: ��������, ��� hit.point ��������� ������ ������� ������ (���� ������)
                    if (spawnArea != null && !spawnArea.bounds.Contains(hit.point))
                    {
                        Debug.LogWarning("����� �� ��������� ������� ������. ���������� ���������� �������.");
                        e.Use();
                        return;
                    }

                    // ���������: ������ ��������� �������� � �������� �����
                    if (prefabs != null && prefabs.Length > 0)
                    {
                        int count = Mathf.CeilToInt(Mathf.PI * brushSize * brushSize * density);
                        for (int i = 0; i < count; i++)
                        {
                            // ���������� ��������� �������� ������ ���������� �����, ����������� �� ������� �����
                            Vector2 randomCircle = Random.insideUnitCircle * brushSize;

                            // ������ ����� ��� ���������, ���������������� hit.normal
                            Vector3 tangent = Vector3.Cross(hit.normal, Vector3.up);
                            if (tangent == Vector3.zero)
                            {
                                tangent = Vector3.Cross(hit.normal, Vector3.right);
                            }
                            tangent.Normalize();
                            Vector3 bitangent = Vector3.Cross(hit.normal, tangent);

                            Vector3 offset = tangent * randomCircle.x + bitangent * randomCircle.y;
                            // ���������� ������� �������� �������
                            Vector3 spawnPosition = hit.point + offset;

                            // ���� ������� ������ �������, ���������, ��� ������� ������ ������ ��
                            if (spawnArea != null && !spawnArea.bounds.Contains(spawnPosition))
                            {
                                Debug.Log($"������� ��� ������ {spawnPosition} ��� ������� ������. �������.");
                                continue;
                            }

                            // �������� �� ����������� � ��� ���������� ��������� (������ ��������)
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
                                // ������� ��������������� �������, ���� ������� �����������
                                Vector2 newRandomCircle = Random.insideUnitCircle * brushSize;
                                spawnPosition = hit.point + tangent * newRandomCircle.x + bitangent * newRandomCircle.y;
                                if (spawnArea != null && !spawnArea.bounds.Contains(spawnPosition))
                                {
                                    Debug.Log($"������� ��� ������ {spawnPosition} ��� ������� ������ ����� ���������. �������.");
                                    continue;
                                }
                            }

                            // ����� ���������� ������� � ������ �����������
                            GameObject prefabToSpawn = GetRandomPrefab();
                            if (prefabToSpawn == null)
                            {
                                Debug.LogError("�� ������ ���������� ������ ��� ������.");
                                continue;
                            }

                            // ������ ��������� ���������� ������� � ������������ ��� ��� ����������� ������ ��������
                            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                            Undo.RegisterCreatedObjectUndo(obj, "Place Object");
                            obj.transform.position = spawnPosition;

                            // ���� ������ �������� ������, ����� ���
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

    // ������� ������ ���������� ������� � ������ ������������
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
