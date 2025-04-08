using UnityEngine;
using UnityEditor;

public class UpdateTextureSettings : EditorWindow
{
    // �������� ������������� ������� �������� (�� ��������� 512)
    private int maxTextureSize = 512;

    [MenuItem("Tools/�������� ��������� �������")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UpdateTextureSettings), false, "���������� �������");
    }

    private void OnGUI()
    {
        GUILayout.Label("��������� ���������� �������", EditorStyles.boldLabel);
        maxTextureSize = EditorGUILayout.IntField("����. ������ ��������", maxTextureSize);

        if (GUILayout.Button("�������� ��� ��������"))
        {
            UpdateAllTextures();
        }
    }

    private void UpdateAllTextures()
    {
        // ������� ��� ������ ���� Texture
        string[] guids = AssetDatabase.FindAssets("t:Texture");
        int processedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                bool isModified = false;

                // ������������� ������������ ������ ��������
                if (importer.maxTextureSize != maxTextureSize)
                {
                    importer.maxTextureSize = maxTextureSize;
                    isModified = true;
                }

                // ��������� ����������
                if (importer.textureCompression != TextureImporterCompression.Compressed)
                {
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    isModified = true;
                }

                if (!importer.crunchedCompression)
                {
                    importer.crunchedCompression = true;
                    isModified = true;
                }

                if (importer.compressionQuality != 100)
                {
                    importer.compressionQuality = 100;
                    isModified = true;
                }

                // ��������� ������������� ��������� ��� ��������� "Default"
                TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Default");
                if (!platformSettings.overridden)
                {
                    platformSettings.overridden = true;
                    isModified = true;
                }
                // ������������� ������ �������� � RGBA Crunched DXT5 (BC3)
                if (platformSettings.format != TextureImporterFormat.DXT5Crunched)
                {
                    platformSettings.format = TextureImporterFormat.DXT5Crunched;
                    isModified = true;
                }
                importer.SetPlatformTextureSettings(platformSettings);

                if (isModified)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    processedCount++;
                }
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("���������� �������", $"��������� {processedCount} �������(�).", "OK");
    }
}
