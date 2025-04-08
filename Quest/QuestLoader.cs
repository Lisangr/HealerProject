using UnityEngine;
using YG;

public class QuestLoader : MonoBehaviour
{
    public string jsonFileName = "Quest1"; // Убедитесь, что имя файла совпадает (без расширения)
    public QuestData loadedQuestData;

    void Start()
    {
        // Загрузка текстового ассета из папки Resources/Quests
        TextAsset jsonTextAsset = Resources.Load<TextAsset>("Quests/" + jsonFileName);
        if (jsonTextAsset != null)
        {
            // Создаем новый экземпляр ScriptableObject
            loadedQuestData = ScriptableObject.CreateInstance<QuestData>();
            // Заполняем его данными из JSON
            JsonUtility.FromJsonOverwrite(jsonTextAsset.text, loadedQuestData);

            // Получаем системный язык через наш метод
            string currentLang = GetSystemLanguageCode();
            if (QuestDisplay.Instance != null)
            {
                QuestDisplay.Instance.questData = loadedQuestData;
                // Если нужно вывести первую фразу диалога:
                string[] dialogues = loadedQuestData.GetLocalizedDialogues(currentLang);
                if (dialogues.Length > 0)
                {
                    QuestDisplay.Instance.SetQuestText(dialogues[0]);
                }
            }
            else
            {
                Debug.LogWarning("QuestDisplay.Instance равен null. Проверьте, что канвас с QuestDisplay уже загружен и активен.");
            }
        }
        else
        {
            Debug.LogError("Файл квеста не найден в Resources/Quests/" + jsonFileName);
        }
    }

    private string GetSystemLanguageCode()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            // Определяем текущий язык
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
                    return "Ru"; // Значение по умолчанию
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
                    return "en"; // Значение по умолчанию
            }
        }
    }

}
