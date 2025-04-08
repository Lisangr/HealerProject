using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class LegacyTextFontChanger : EditorWindow
{
    // ������� ����� ��� ������
    private Font targetFont;

    [MenuItem("Tools/Legacy Text Font Changer")]
    public static void ShowWindow()
    {
        GetWindow<LegacyTextFontChanger>("Legacy Text Font Changer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("�������� ����� ��� Legacy UI Text", EditorStyles.boldLabel);
        targetFont = (Font)EditorGUILayout.ObjectField("������� �����", targetFont, typeof(Font), false);

        if (GUILayout.Button("����������� � ��������"))
        {
            if (targetFont == null)
            {
                Debug.LogWarning("������� ������� �����!");
                return;
            }
            ReplaceLegacyTextFont();
        }
    }

    private void ReplaceLegacyTextFont()
    {
        // ������� ��� ������� ���� Text � ����������� ������
        Text[] texts = Resources.FindObjectsOfTypeAll<Text>();
        int count = 0;
        foreach (Text t in texts)
        {
            // ���������� �������, ������� �� �������� ������ �������� ��������
            if (EditorUtility.IsPersistent(t.gameObject))
                continue;

            Undo.RecordObject(t, "Change Legacy Text Font");
            t.font = targetFont;
            EditorUtility.SetDirty(t);
            count++;
        }
        Debug.Log("�������� ������� � " + count + " Legacy UI Text �����������.");
    }
}
