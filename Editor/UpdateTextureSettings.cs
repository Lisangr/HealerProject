using UnityEngine;
using UnityEditor;

public class UpdateTextureSettings : EditorWindow
{
    // Значение максимального размера текстуры (по умолчанию 512)
    private int maxTextureSize = 512;

    [MenuItem("Tools/Обновить настройки текстур")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UpdateTextureSettings), false, "Обновление текстур");
    }

    private void OnGUI()
    {
        GUILayout.Label("Настройки обновления текстур", EditorStyles.boldLabel);
        maxTextureSize = EditorGUILayout.IntField("Макс. размер текстуры", maxTextureSize);

        if (GUILayout.Button("Обновить все текстуры"))
        {
            UpdateAllTextures();
        }
    }

    private void UpdateAllTextures()
    {
        // Находим все ассеты типа Texture
        string[] guids = AssetDatabase.FindAssets("t:Texture");
        int processedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                bool isModified = false;

                // Устанавливаем максимальный размер текстуры
                if (importer.maxTextureSize != maxTextureSize)
                {
                    importer.maxTextureSize = maxTextureSize;
                    isModified = true;
                }

                // Настройки компрессии
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

                // Обновляем платформенные настройки для платформы "Default"
                TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Default");
                if (!platformSettings.overridden)
                {
                    platformSettings.overridden = true;
                    isModified = true;
                }
                // Устанавливаем формат текстуры — RGBA Crunched DXT5 (BC3)
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
        EditorUtility.DisplayDialog("Обновление текстур", $"Обновлено {processedCount} текстур(а).", "OK");
    }
}
