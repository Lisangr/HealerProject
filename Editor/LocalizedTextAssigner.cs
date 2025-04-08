using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class LocalizedTextAssigner : EditorWindow
{
    [MenuItem("Tools/Assign LocalizedText To Text Fields")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(LocalizedTextAssigner), false, "LocalizedText Assigner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Assign LocalizedText Component", EditorStyles.boldLabel);
        if (GUILayout.Button("Scan Scene and Add LocalizedText"))
        {
            AddLocalizedTextToAll();
        }
    }

    private void AddLocalizedTextToAll()
    {
        int countAdded = 0;

        // Обрабатываем Legacy UI Text поля.
        Text[] legacyTexts = Resources.FindObjectsOfTypeAll<Text>();
        foreach (Text txt in legacyTexts)
        {
            // Фильтруем объекты, чтобы обрабатывать только те, которые принадлежат сцене (не ассеты-предпочеты)
            if (txt.gameObject.hideFlags == HideFlags.None && txt.gameObject.scene.isLoaded)
            {
                if (txt.GetComponent<LocalizedText>() == null)
                {
                    Undo.AddComponent<LocalizedText>(txt.gameObject);
                    countAdded++;
                }
            }
        }

        // Обрабатываем поля TextMeshProUGUI.
        TextMeshProUGUI[] tmpTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (TextMeshProUGUI tmp in tmpTexts)
        {
            if (tmp.gameObject.hideFlags == HideFlags.None && tmp.gameObject.scene.isLoaded)
            {
                if (tmp.GetComponent<LocalizedText>() == null)
                {
                    Undo.AddComponent<LocalizedText>(tmp.gameObject);
                    countAdded++;
                }
            }
        }

        EditorUtility.DisplayDialog("LocalizedText Assigner",
            $"Added LocalizedText component to {countAdded} text objects.", "OK");
    }
}
