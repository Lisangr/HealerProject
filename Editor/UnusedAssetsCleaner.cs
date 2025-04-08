using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UnusedAssetsCleaner : EditorWindow
{
    // Список найденных неиспользуемых активов (путей к файлам)
    private List<string> unusedAssetPaths = new List<string>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Unused Assets Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<UnusedAssetsCleaner>("Unused Assets Cleaner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Сканирование неиспользуемых файлов в папке Assets", EditorStyles.boldLabel);

        if (GUILayout.Button("Сканировать"))
        {
            ScanForUnusedAssets();
        }

        if (unusedAssetPaths != null && unusedAssetPaths.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Найдено " + unusedAssetPaths.Count + " неиспользуемых файлов:", EditorStyles.label);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            foreach (string assetPath in unusedAssetPaths)
            {
                GUILayout.Label(assetPath);
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("Удалить неиспользуемые файлы"))
            {
                if (EditorUtility.DisplayDialog("Подтверждение удаления",
                    "Вы уверены, что хотите удалить найденные файлы?\nПеред удалением сделайте резервную копию проекта!",
                    "Да", "Отмена"))
                {
                    DeleteUnusedAssets();
                }
            }
        }
    }

    private void ScanForUnusedAssets()
    {
        unusedAssetPaths.Clear();

        // 1. Получаем все сцены в папке Assets
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        List<string> scenePaths = sceneGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // 2. Собираем все используемые активы (сцены + их зависимости)
        HashSet<string> usedAssetPaths = new HashSet<string>();

        // Считаем, что сами сцены – используемые активы
        foreach (string scenePath in scenePaths)
        {
            usedAssetPaths.Add(scenePath);
        }

        // Для каждой сцены получаем все зависимости и добавляем в множество
        foreach (string scenePath in scenePaths)
        {
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            foreach (string dep in dependencies)
            {
                usedAssetPaths.Add(dep);
            }
        }

        // 3. Добавляем активы из папки Resources, т.к. они могут использоваться через Resources.Load
        string resourcesFolder = Application.dataPath + "/Resources";
        if (Directory.Exists(resourcesFolder))
        {
            string[] resourceFiles = Directory.GetFiles(resourcesFolder, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta")).ToArray();

            foreach (string filePath in resourceFiles)
            {
                // Преобразуем абсолютный путь в путь относительно Assets
                string assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace("\\", "/");
                usedAssetPaths.Add(assetPath);
            }
        }

        // 4. Получаем список всех активов в папке Assets
        string[] allAssetGUIDs = AssetDatabase.FindAssets("", new[] { "Assets" });
        List<string> allAssetPaths = allAssetGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // 5. Отбираем те, которые не входят в список используемых
        foreach (string assetPath in allAssetPaths)
        {
            // Пропускаем папки
            if (AssetDatabase.IsValidFolder(assetPath))
                continue;

            if (!usedAssetPaths.Contains(assetPath))
            {
                unusedAssetPaths.Add(assetPath);
            }
        }

        Debug.Log("Сканирование завершено. Найдено " + unusedAssetPaths.Count + " неиспользуемых файлов.");
    }

    private void DeleteUnusedAssets()
    {
        int deletedCount = 0;
        foreach (string assetPath in unusedAssetPaths)
        {
            bool success = AssetDatabase.DeleteAsset(assetPath);
            if (success)
            {
                deletedCount++;
            }
            else
            {
                Debug.LogWarning("Не удалось удалить: " + assetPath);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Удаление завершено", $"Удалено {deletedCount} файлов.", "OK");
        unusedAssetPaths.Clear();
    }
}
