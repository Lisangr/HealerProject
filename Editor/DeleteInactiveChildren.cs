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
        GUILayout.Label("Удаление неактивных дочерних объектов", EditorStyles.boldLabel);

        GUILayout.Space(10);
        GUILayout.Label("Перетащите объекты сюда:", EditorStyles.boldLabel);

        // Область для Drag-and-Drop
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Перетащите объекты сюда");

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
                            // Если объект еще не в списке, добавляем его
                            if (!targetObjects.Contains(go))
                            {
                                targetObjects.Add(go);
                            }
                        }
                    }
                }
                break;
        }

        // Отображение списка объектов
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        for (int i = 0; i < targetObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            targetObjects[i] = (GameObject)EditorGUILayout.ObjectField(targetObjects[i], typeof(GameObject), true);
            if (GUILayout.Button("Удалить", GUILayout.Width(60)))
            {
                targetObjects.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        // Кнопка для удаления неактивных детей
        if (GUILayout.Button("Удалить неактивные дети"))
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

        // Получаем все дочерние объекты, включая неактивные
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
