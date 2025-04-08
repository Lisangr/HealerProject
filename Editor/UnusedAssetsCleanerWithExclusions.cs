using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UnusedAssetsCleanerWithExclusions : EditorWindow
{
    // Список исключаемых папок (пути относительно Assets, например "Assets/Editor")
    private List<string> excludedFolders = new List<string>() { "Assets/Editor" };
    // Результат сканирования – список найденных неиспользуемых активов
    private List<AssetItem> unusedAssets = new List<AssetItem>();
    private Vector2 scrollPos;
    private string newExcludedFolder = "";

    // Класс для хранения информации по активу и флага выбора для удаления
    private class AssetItem
    {
        public string path;
        public bool selected;

        public AssetItem(string path)
        {
            this.path = path;
            selected = true;
        }
    }

    [MenuItem("Tools/Unused Assets Cleaner With Exclusions")]
    public static void ShowWindow()
    {
        GetWindow<UnusedAssetsCleanerWithExclusions>("Unused Assets Cleaner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Настройки исключения папок", EditorStyles.boldLabel);

        // Вывод списка исключаемых папок
        for (int i = 0; i < excludedFolders.Count; i++)
        {
            GUILayout.BeginHorizontal();
            excludedFolders[i] = EditorGUILayout.TextField(excludedFolders[i]);
            if (GUILayout.Button("Удалить", GUILayout.Width(70)))
            {
                excludedFolders.RemoveAt(i);
                i--;
            }
            GUILayout.EndHorizontal();
        }

        // Поле для добавления новой папки в исключения
        GUILayout.BeginHorizontal();
        newExcludedFolder = EditorGUILayout.TextField(newExcludedFolder);
        if (GUILayout.Button("Добавить", GUILayout.Width(70)))
        {
            if (!string.IsNullOrEmpty(newExcludedFolder) && !excludedFolders.Contains(newExcludedFolder))
            {
                excludedFolders.Add(newExcludedFolder);
                newExcludedFolder = "";
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Кнопка запуска сканирования
        if (GUILayout.Button("Сканировать неиспользуемые активы"))
        {
            ScanForUnusedAssets();
        }

        GUILayout.Space(10);

        // Если сканирование выполнено и найден хотя бы один актив
        if (unusedAssets != null && unusedAssets.Count > 0)
        {
            GUILayout.Label("Найдено " + unusedAssets.Count + " неиспользуемых активов:", EditorStyles.boldLabel);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            foreach (AssetItem item in unusedAssets)
            {
                GUILayout.BeginHorizontal();
                item.selected = EditorGUILayout.Toggle(item.selected, GUILayout.Width(20));
                GUILayout.Label(item.path);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            // Кнопка удаления выбранных активов
            if (GUILayout.Button("Удалить выбранные активы"))
            {
                if (EditorUtility.DisplayDialog("Подтверждение удаления",
                    "Вы уверены, что хотите удалить выбранные активы?\nПеред удалением сделайте резервную копию проекта!",
                    "Да", "Отмена"))
                {
                    DeleteSelectedAssets();
                }
            }
        }
        else
        {
            GUILayout.Label("Нет результатов сканирования или не найдено неиспользуемых активов.");
        }
    }

    private void ScanForUnusedAssets()
    {
        unusedAssets.Clear();

        // 1. Получаем все сцены в папке Assets
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        List<string> scenePaths = sceneGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // 2. Собираем зависимости для каждой сцены
        HashSet<string> usedAssets = new HashSet<string>();
        foreach (string scenePath in scenePaths)
        {
            usedAssets.Add(scenePath);
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            foreach (string dep in dependencies)
            {
                usedAssets.Add(dep);
            }
        }

        // 3. Добавляем активы из папки Resources (используются через Resources.Load)
        string resourcesPath = Application.dataPath + "/Resources";
        if (Directory.Exists(resourcesPath))
        {
            string[] resourceFiles = Directory.GetFiles(resourcesPath, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta")).ToArray();
            foreach (string file in resourceFiles)
            {
                string assetPath = "Assets" + file.Replace(Application.dataPath, "").Replace("\\", "/");
                usedAssets.Add(assetPath);
            }
        }

        // 4. Получаем список всех активов в папке Assets
        string[] allGUIDs = AssetDatabase.FindAssets("", new[] { "Assets" });
        List<string> allAssets = allGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // 5. Отбираем активы, которых нет в используемых, и которые не находятся в исключённых папках
        foreach (string assetPath in allAssets)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                continue;

            if (!usedAssets.Contains(assetPath))
            {
                bool isExcluded = false;
                foreach (string excl in excludedFolders)
                {
                    if (!string.IsNullOrEmpty(excl) && assetPath.StartsWith(excl))
                    {
                        isExcluded = true;
                        break;
                    }
                }
                if (!isExcluded)
                {
                    unusedAssets.Add(new AssetItem(assetPath));
                }
            }
        }
        Debug.Log("Сканирование завершено. Найдено " + unusedAssets.Count + " неиспользуемых активов.");
    }

    private void DeleteSelectedAssets()
    {
        int deletedCount = 0;
        foreach (AssetItem item in unusedAssets)
        {
            if (item.selected)
            {
                bool success = AssetDatabase.DeleteAsset(item.path);
                if (success)
                {
                    deletedCount++;
                }
                else
                {
                    Debug.LogWarning("Не удалось удалить: " + item.path);
                }
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Удаление завершено", $"Удалено {deletedCount} активов.", "OK");
        // Обновляем результаты сканирования после удаления
        ScanForUnusedAssets();
    }
}
