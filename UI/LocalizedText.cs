using UnityEngine;
// Для Legacy UI
using UnityEngine.UI;
// Для TextMesh Pro
using TMPro;
using YG;
public class LocalizedText : MonoBehaviour
{
    // Тексты для разных языков (вы можете расширить список при необходимости)
    [TextArea] public string russianText;
    [TextArea] public string englishText;
    [TextArea] public string turkishText;
    [TextArea] public string spanishText;
    [TextArea] public string italianText;
    [TextArea] public string germanText;

    // Ссылки на возможные UI-компоненты
    private Text legacyText;             // Legacy UI (UnityEngine.UI.Text)
    private TextMeshProUGUI tmpText;     // TextMesh Pro (TMPro.TextMeshProUGUI)

    private void Awake()
    {
        // Пытаемся получить компонент Legacy UI Text
        legacyText = GetComponent<Text>();
        // Пытаемся получить компонент TextMeshPro
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            // Определяем текущий язык
            string currentLang = YandexGame.EnvironmentData.language;
            // По умолчанию используем английский
            string textToDisplay = russianText;

            // Подставляем нужный текст в зависимости от языка
            switch (currentLang)
            {
                case "en":
                    textToDisplay = englishText;
                    break;
                case "tr":
                    textToDisplay = turkishText;
                    break;
                case "de":
                    textToDisplay = germanText;
                    break;
                case "es":
                    textToDisplay = spanishText;
                    break;
                case "it":
                    textToDisplay = italianText;
                    break;
                    // Для остальных случаев оставляем textToDisplay = englishText
            }

            // Устанавливаем текст в соответствующий UI-компонент
            if (legacyText != null)
            {
                legacyText.text = textToDisplay;
            }
            else if (tmpText != null)
            {
                tmpText.text = textToDisplay;
            }
            else
            {
                Debug.LogWarning("Не найден компонент Text или TextMeshProUGUI на объекте!");
            }
        }
        else
        {
            // Определяем текущий язык
            string currentLang = GetSystemLanguageCode();
            // По умолчанию используем английский
            string textToDisplay = englishText;

            // Подставляем нужный текст в зависимости от языка
            switch (currentLang)
            {
                case "Ru":
                    textToDisplay = russianText;
                    break;
                case "tr":
                    textToDisplay = turkishText;
                    break;
                case "de":
                    textToDisplay = germanText;
                    break;
                case "es":
                    textToDisplay = spanishText;
                    break;
                case "it":
                    textToDisplay = italianText;
                    break;
                    // Для остальных случаев оставляем textToDisplay = englishText
            }

            // Устанавливаем текст в соответствующий UI-компонент
            if (legacyText != null)
            {
                legacyText.text = textToDisplay;
            }
            else if (tmpText != null)
            {
                tmpText.text = textToDisplay;
            }
            else
            {
                Debug.LogWarning("Не найден компонент Text или TextMeshProUGUI на объекте!");
            }
        }
    }

    // Метод для получения кода языка на основе системного языка
    private string GetSystemLanguageCode()
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
            default:
                return "en"; // Значение по умолчанию
        }
    }

}
