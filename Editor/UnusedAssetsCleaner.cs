using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UnusedAssetsCleaner : EditorWindow
{
    // ������ ��������� �������������� ������� (����� � ������)
    private List<string> unusedAssetPaths = new List<string>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Unused Assets Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<UnusedAssetsCleaner>("Unused Assets Cleaner");
    }

    private void OnGUI()
    {
        GUILayout.Label("������������ �������������� ������ � ����� Assets", EditorStyles.boldLabel);

        if (GUILayout.Button("�����������"))
        {
            ScanForUnusedAssets();
        }

        if (unusedAssetPaths != null && unusedAssetPaths.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("������� " + unusedAssetPaths.Count + " �������������� ������:", EditorStyles.label);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            foreach (string assetPath in unusedAssetPaths)
            {
                GUILayout.Label(assetPath);
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("������� �������������� �����"))
            {
                if (EditorUtility.DisplayDialog("������������� ��������",
                    "�� �������, ��� ������ ������� ��������� �����?\n����� ��������� �������� ��������� ����� �������!",
                    "��", "������"))
                {
                    DeleteUnusedAssets();
                }
            }
        }
    }

    private void ScanForUnusedAssets()
    {
        unusedAssetPaths.Clear();

        // 1. �������� ��� ����� � ����� Assets
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        List<string> scenePaths = sceneGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // 2. �������� ��� ������������ ������ (����� + �� �����������)
        HashSet<string> usedAssetPaths = new HashSet<string>();

        // �������, ��� ���� ����� � ������������ ������
        foreach (string scenePath in scenePaths)
        {
            usedAssetPaths.Add(scenePath);
        }

        // ��� ������ ����� �������� ��� ����������� � ��������� � ���������
        foreach (string scenePath in scenePaths)
        {
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            foreach (string dep in dependencies)
            {
                usedAssetPaths.Add(dep);
            }
        }

        // 3. ��������� ������ �� ����� Resources, �.�. ��� ����� �������������� ����� Resources.Load
        string resourcesFolder = Application.dataPath + "/Resources";
        if (Directory.Exists(resourcesFolder))
        {
            string[] resourceFiles = Directory.GetFiles(resourcesFolder, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta")).ToArray();

            foreach (string filePath in resourceFiles)
            {
                // ����������� ���������� ���� � ���� ������������ Assets
                string assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace("\\", "/");
                usedAssetPaths.Add(assetPath);
            }
        }

        // 4. �������� ������ ���� ������� � ����� Assets
        string[] allAssetGUIDs = AssetDatabase.FindAssets("", new[] { "Assets" });
        List<string> allAssetPaths = allAssetGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // 5. �������� ��, ������� �� ������ � ������ ������������
        foreach (string assetPath in allAssetPaths)
        {
            // ���������� �����
            if (AssetDatabase.IsValidFolder(assetPath))
                continue;

            if (!usedAssetPaths.Contains(assetPath))
            {
                unusedAssetPaths.Add(assetPath);
            }
        }

        Debug.Log("������������ ���������. ������� " + unusedAssetPaths.Count + " �������������� ������.");
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
                Debug.LogWarning("�� ������� �������: " + assetPath);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("�������� ���������", $"������� {deletedCount} ������.", "OK");
        unusedAssetPaths.Clear();
    }
}
