using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class PrefabRenamer : EditorWindow
{
    private List<GameObject> prefabs = new List<GameObject>();
    private Vector2 scrollPosition;
    // Текст, который будем удалять из имени ассета. По умолчанию "decimate_".
    private string removeText = "decimated_";

    [MenuItem("Tools/Prefab Renamer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabRenamer>("Prefab Renamer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Настройки переименования", EditorStyles.boldLabel);

        // Поле ввода для удаляемого текста
        removeText = EditorGUILayout.TextField("Удаляемый текст:", removeText);

        GUILayout.Space(10);
        GUILayout.Label("Перетащите префабы сюда:", EditorStyles.boldLabel);

        // Область для Drag-and-Drop
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Перетащите префабы сюда");

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
                            string assetPath = AssetDatabase.GetAssetPath(go);
                            if (!string.IsNullOrEmpty(assetPath) && !prefabs.Contains(go))
                            {
                                prefabs.Add(go);
                            }
                        }
                    }
                }
                break;
        }

        // Отображение списка префабов
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        for (int i = 0; i < prefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("Удалить", GUILayout.Width(60)))
            {
                prefabs.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        // Кнопка запуска переименования
        if (GUILayout.Button("Переименовать префабы"))
        {
            RenamePrefabs();
        }
    }

    private void RenamePrefabs()
    {
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(prefab);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    // Проверяем, начинается ли имя с указанного текста
                    if (!string.IsNullOrEmpty(removeText) && fileName.StartsWith(removeText))
                    {
                        string newFileName = fileName.Substring(removeText.Length);
                        // Переименование ассета
                        AssetDatabase.RenameAsset(assetPath, newFileName);
                        Debug.Log($"Префаб '{fileName}' переименован в '{newFileName}'");
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
