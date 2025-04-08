using UnityEngine;
using YG;

public class QuestLoader : MonoBehaviour
{
    public string jsonFileName = "Quest1"; // ���������, ��� ��� ����� ��������� (��� ����������)
    public QuestData loadedQuestData;

    void Start()
    {
        // �������� ���������� ������ �� ����� Resources/Quests
        TextAsset jsonTextAsset = Resources.Load<TextAsset>("Quests/" + jsonFileName);
        if (jsonTextAsset != null)
        {
            // ������� ����� ��������� ScriptableObject
            loadedQuestData = ScriptableObject.CreateInstance<QuestData>();
            // ��������� ��� ������� �� JSON
            JsonUtility.FromJsonOverwrite(jsonTextAsset.text, loadedQuestData);

            // �������� ��������� ���� ����� ��� �����
            string currentLang = GetSystemLanguageCode();
            if (QuestDisplay.Instance != null)
            {
                QuestDisplay.Instance.questData = loadedQuestData;
                // ���� ����� ������� ������ ����� �������:
                string[] dialogues = loadedQuestData.GetLocalizedDialogues(currentLang);
                if (dialogues.Length > 0)
                {
                    QuestDisplay.Instance.SetQuestText(dialogues[0]);
                }
            }
            else
            {
                Debug.LogWarning("QuestDisplay.Instance ����� null. ���������, ��� ������ � QuestDisplay ��� �������� � �������.");
            }
        }
        else
        {
            Debug.LogError("���� ������ �� ������ � Resources/Quests/" + jsonFileName);
        }
    }

    private string GetSystemLanguageCode()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            // ���������� ������� ����
            string currentLang = YandexGame.EnvironmentData.language;
            switch (currentLang)
            {
                case "Ru":
                    return "Ru";
                case "en":
                    return "en";
                case "tr":
                    return "tr";
                case "de":
                    return "de";
                case "es":
                    return "es";
                case "it":
                    return "it";
                case "fr":
                    return "fr";
                default:
                    return "Ru"; // �������� �� ���������
            }
        }
        else
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                    return "Ru";
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Turkish:
                    return "tr";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.French:
                    return "fr";
                default:
                    return "en"; // �������� �� ���������
            }
        }
    }

}
