using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DeleteInactiveChildren : EditorWindow
{
    private List<GameObject> targetObjects = new List<GameObject>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Delete Inactive Children")]
    public static void ShowWindow()
    {
        GetWindow<DeleteInactiveChildren>("Delete Inactive Children");
    }

    private void OnGUI()
    {
        GUILayout.Label("�������� ���������� �������� ��������", EditorStyles.boldLabel);

        GUILayout.Space(10);
        GUILayout.Label("���������� ������� ����:", EditorStyles.boldLabel);

        // ������� ��� Drag-and-Drop
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "���������� ������� ����");

        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object dragged in DragAndDrop.objectReferences)
                    {
                        if (dragged is GameObject go)
                        {
                            // ���� ������ ��� �� � ������, ��������� ���
                            if (!targetObjects.Contains(go))
                            {
                                targetObjects.Add(go);
                            }
                        }
                    }
                }
                break;
        }

        // ����������� ������ ��������
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        for (int i = 0; i < targetObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            targetObjects[i] = (GameObject)EditorGUILayout.ObjectField(targetObjects[i], typeof(GameObject), true);
            if (GUILayout.Button("�������", GUILayout.Width(60)))
            {
                targetObjects.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        // ������ ��� �������� ���������� �����
        if (GUILayout.Button("������� ���������� ����"))
        {
            foreach (var targetObject in targetObjects)
            {
                if (targetObject != null)
                {
                    DeleteInactiveObjects(targetObject);
                }
            }
        }
    }

    private static void DeleteInactiveObjects(GameObject parentObject)
    {
        if (parentObject == null)
        {
            Debug.LogError("Parent object is null.");
            return;
        }

        // �������� ��� �������� �������, ������� ����������
        Transform[] children = parentObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (!child.gameObject.activeInHierarchy)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        Debug.Log($"Inactive children for {parentObject.name} have been deleted.");
    }
}
