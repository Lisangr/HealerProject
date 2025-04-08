using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class LegacyTextFontChanger : EditorWindow
{
    // Целевой шрифт для замены
    private Font targetFont;

    [MenuItem("Tools/Legacy Text Font Changer")]
    public static void ShowWindow()
    {
        GetWindow<LegacyTextFontChanger>("Legacy Text Font Changer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Заменить шрифт для Legacy UI Text", EditorStyles.boldLabel);
        targetFont = (Font)EditorGUILayout.ObjectField("Целевой шрифт", targetFont, typeof(Font), false);

        if (GUILayout.Button("Сканировать и заменить"))
        {
            if (targetFont == null)
            {
                Debug.LogWarning("Укажите целевой шрифт!");
                return;
            }
            ReplaceLegacyTextFont();
        }
    }

    private void ReplaceLegacyTextFont()
    {
        // Находим все объекты типа Text в загруженных сценах
        Text[] texts = Resources.FindObjectsOfTypeAll<Text>();
        int count = 0;
        foreach (Text t in texts)
        {
            // Пропускаем объекты, которые не являются частью сценовой иерархии
            if (EditorUtility.IsPersistent(t.gameObject))
                continue;

            Undo.RecordObject(t, "Change Legacy Text Font");
            t.font = targetFont;
            EditorUtility.SetDirty(t);
            count++;
        }
        Debug.Log("Заменено шрифтов в " + count + " Legacy UI Text компонентах.");
    }
}
