using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UnusedAssetsCleanerWithExclusions : EditorWindow
{
    // ������ ����������� ����� (���� ������������ Assets, �������� "Assets/Editor")
    private List<string> excludedFolders = new List<string>() { "Assets/Editor" };
    // ��������� ������������ � ������ ��������� �������������� �������
    private List<AssetItem> unusedAssets = new List<AssetItem>();
    private Vector2 scrollPos;
    private string newExcludedFolder = "";

    // ����� ��� �������� ���������� �� ������ � ����� ������ ��� ��������
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
        GUILayout.Label("��������� ���������� �����", EditorStyles.boldLabel);

        // ����� ������ ����������� �����
        for (int i = 0; i < excludedFolders.Count; i++)
        {
            GUILayout.BeginHorizontal();
            excludedFolders[i] = EditorGUILayout.TextField(excludedFolders[i]);
            if (GUILayout.Button("�������", GUILayout.Width(70)))
            {
                excludedFolders.RemoveAt(i);
                i--;
            }
            GUILayout.EndHorizontal();
        }

        // ���� ��� ���������� ����� ����� � ����������
        GUILayout.BeginHorizontal();
        newExcludedFolder = EditorGUILayout.TextField(newExcludedFolder);
        if (GUILayout.Button("��������", GUILayout.Width(70)))
        {
            if (!string.IsNullOrEmpty(newExcludedFolder) && !excludedFolders.Contains(newExcludedFolder))
            {
                excludedFolders.Add(newExcludedFolder);
                newExcludedFolder = "";
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ������ ������� ������������
        if (GUILayout.Button("����������� �������������� ������"))
        {
            ScanForUnusedAssets();
        }

        GUILayout.Space(10);

        // ���� ������������ ��������� � ������ ���� �� ���� �����
        if (unusedAssets != null && unusedAssets.Count > 0)
        {
            GUILayout.Label("������� " + unusedAssets.Count + " �������������� �������:", EditorStyles.boldLabel);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            foreach (AssetItem item in unusedAssets)
            {
                GUILayout.BeginHorizontal();
                item.selected = EditorGUILayout.Toggle(item.selected, GUILayout.Width(20));
                GUILayout.Label(item.path);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            // ������ �������� ��������� �������
            if (GUILayout.Button("������� ��������� ������"))
            {
                if (EditorUtility.DisplayDialog("������������� ��������",
                    "�� �������, ��� ������ ������� ��������� ������?\n����� ��������� �������� ��������� ����� �������!",
                    "��", "������"))
                {
                    DeleteSelectedAssets();
                }
            }
        }
        else
        {
            GUILayout.Label("��� ����������� ������������ ��� �� ������� �������������� �������.");
        }
    }

    private void ScanForUnusedAssets()
    {
        unusedAssets.Clear();

        // 1. �������� ��� ����� � ����� Assets
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        List<string> scenePaths = sceneGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // 2. �������� ����������� ��� ������ �����
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

        // 3. ��������� ������ �� ����� Resources (������������ ����� Resources.Load)
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

        // 4. �������� ������ ���� ������� � ����� Assets
        string[] allGUIDs = AssetDatabase.FindAssets("", new[] { "Assets" });
        List<string> allAssets = allGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // 5. �������� ������, ������� ��� � ������������, � ������� �� ��������� � ����������� ������
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
        Debug.Log("������������ ���������. ������� " + unusedAssets.Count + " �������������� �������.");
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
                    Debug.LogWarning("�� ������� �������: " + item.path);
                }
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("�������� ���������", $"������� {deletedCount} �������.", "OK");
        // ��������� ���������� ������������ ����� ��������
        ScanForUnusedAssets();
    }
}
