using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PrefabIconGenerator : EditorWindow
{
    // Список префабов для генерации иконок
    private List<GameObject> prefabsToProcess = new List<GameObject>();
    private Vector2 scrollPos;

    // Настройки снимка
    private int resolution = 256;
    private string iconsFolder = "Assets/Resources/Icons";

    [MenuItem("Tools/Prefab Icon Generator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabIconGenerator>("Prefab Icon Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Генерация иконок для префабов", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        resolution = EditorGUILayout.IntField("Разрешение", resolution);

        EditorGUILayout.HelpBox("Перетащите префабы сюда, либо нажмите кнопку 'Load From Folder'", MessageType.Info);
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Перетащите префабы сюда");
        HandleDragAndDrop(dropArea);

        if (prefabsToProcess.Count > 0)
        {
            EditorGUILayout.LabelField("Выбранные префабы:");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
            foreach (var prefab in prefabsToProcess)
            {
                EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
            }
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Load From Folder"))
        {
            LoadPrefabsFromFolder("Assets/_1Idyllic Fantasy Nature/Prefabs");
        }

        if (GUILayout.Button("Generate Icons"))
        {
            GenerateIcons();
        }
    }

    private void HandleDragAndDrop(Rect dropArea)
    {
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
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject)
                        {
                            AddPrefab(draggedObject as GameObject);
                        }
                    }
                }
                break;
        }
    }

    private void AddPrefab(GameObject prefab)
    {
        if (!prefabsToProcess.Contains(prefab))
        {
            prefabsToProcess.Add(prefab);
        }
    }

    private void LoadPrefabsFromFolder(string folderPath)
    {
        prefabsToProcess.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
                prefabsToProcess.Add(prefab);
        }
        Debug.Log("Загружено префабов: " + prefabsToProcess.Count);
    }

    private void GenerateIcons()
    {
        // Создаем папку, если её нет
        if (!Directory.Exists(iconsFolder))
        {
            Directory.CreateDirectory(iconsFolder);
            AssetDatabase.Refresh();
        }

        // Создаем временную сцену
        var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        // Настраиваем ambient light
        RenderSettings.ambientLight = Color.white;

        // Создаем камеру
        GameObject camGO = new GameObject("TempCamera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.backgroundColor = Color.white;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.orthographic = true;
        cam.orthographicSize = 0.3f;

        // Позиционирование камеры: расстояние 10, угол 30° сверху вниз
        float distance = 5f;
        float angleRad = 40f * Mathf.Deg2Rad;
        float height = distance * Mathf.Sin(angleRad);      // 10 * 0.5 = 5
        float horizontal = distance * Mathf.Cos(angleRad);    // 10 * 0.866 = 8.66
        camGO.transform.position = new Vector3(0, height, -horizontal);
        // Теперь направляем камеру точно на центр сцены (где находится префаб)
        camGO.transform.LookAt(Vector3.zero);

        // Создаем Directional Light
        GameObject lightGO = new GameObject("TempLight");
        Light lightComp = lightGO.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1.5f; // Повышаем интенсивность
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Настройка RenderTexture
        RenderTexture rt = new RenderTexture(resolution, resolution, 24);
        cam.targetTexture = rt;

        foreach (GameObject prefab in prefabsToProcess)
        {
            // Создаем экземпляр префаба в центре сцены
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, tempScene);
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;

            // Смещаем префаб вниз на 0.25 единицы
            instance.transform.position += new Vector3(0, -0.02f, 0);

            // Рендерим сцену камерой
            cam.Render();

            // Читаем пиксели из RenderTexture
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            tex.Apply();

            // Сохраняем PNG-файл
            byte[] bytes = tex.EncodeToPNG();
            string filePath = Path.Combine(iconsFolder, prefab.name + ".png");
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Icon saved: " + filePath);

            // Удаляем экземпляр префаба
            DestroyImmediate(instance);
        }

        // Очистка
        cam.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
        DestroyImmediate(rt);
        DestroyImmediate(camGO);
        DestroyImmediate(lightGO);

        // Закрываем временную сцену
        EditorSceneManager.CloseScene(tempScene, true);

        AssetDatabase.Refresh();
        Debug.Log("Генерация иконок завершена.");
    }
}
