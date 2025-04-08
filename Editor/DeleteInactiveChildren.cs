using UnityEngine;
using UnityEditor;

public class DeleteInactiveChildren : EditorWindow
{
    // ������ ��������, ������� ����� ���������
    public GameObject[] targetObjects;

    [MenuItem("Tools/Delete Inactive Children")]
    public static void ShowWindow()
    {
        GetWindow<DeleteInactiveChildren>("Delete Inactive Children");
    }

    private void OnGUI()
    {
        // �������� ������������� �������
        if (targetObjects == null)
        {
            targetObjects = new GameObject[0];
        }

        // ���������� ���� ��� ������� ��������, ���� ����� ������������� �������
        EditorGUILayout.LabelField("Parent Objects");

        // ���������� ObjectField ��� ����������� ������� �������� �������
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i] = (GameObject)EditorGUILayout.ObjectField($"Object {i + 1}", targetObjects[i], typeof(GameObject), true);
        }

        // ������ ��� ���������� ����� �������� � ������
        if (GUILayout.Button("Add Object"))
        {
            // ����������� ������ ������� �� 1 � ��������� ����� ������
            ArrayUtility.Add(ref targetObjects, null);
        }

        // ������ ��� �������� ���������� �����
        if (GUILayout.Button("Delete Inactive Children"))
        {
            if (targetObjects != null && targetObjects.Length > 0)
            {
                foreach (var targetObject in targetObjects)
                {
                    if (targetObject != null)
                    {
                        DeleteInactiveObjects(targetObject);
                    }
                    else
                    {
                        Debug.LogWarning("One of the objects in the array is null.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Please assign parent objects to scan.");
            }
        }
    }

    private static void DeleteInactiveObjects(GameObject parentObject)
    {
        // ��������, ��� ������ ����������
        if (parentObject == null)
        {
            Debug.LogError("Parent object is null.");
            return;
        }

        // �������� ��� �������� �������
        Transform[] children = parentObject.GetComponentsInChildren<Transform>(true);

        // �������� �� ���� �������� ��������
        foreach (Transform child in children)
        {
            // ���� ������ �� ������� � ��������, ������� ���
            if (!child.gameObject.activeInHierarchy)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        Debug.Log($"Inactive children for {parentObject.name} have been deleted.");
    }
}
