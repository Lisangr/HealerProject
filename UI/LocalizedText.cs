using UnityEngine;
// ��� Legacy UI
using UnityEngine.UI;
// ��� TextMesh Pro
using TMPro;
using YG;
public class LocalizedText : MonoBehaviour
{
    // ������ ��� ������ ������ (�� ������ ��������� ������ ��� �������������)
    [TextArea] public string russianText;
    [TextArea] public string englishText;
    [TextArea] public string turkishText;
    [TextArea] public string spanishText;
    [TextArea] public string italianText;
    [TextArea] public string germanText;

    // ������ �� ��������� UI-����������
    private Text legacyText;             // Legacy UI (UnityEngine.UI.Text)
    private TextMeshProUGUI tmpText;     // TextMesh Pro (TMPro.TextMeshProUGUI)

    private void Awake()
    {
        // �������� �������� ��������� Legacy UI Text
        legacyText = GetComponent<Text>();
        // �������� �������� ��������� TextMeshPro
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            // ���������� ������� ����
            string currentLang = YandexGame.EnvironmentData.language;
            // �� ��������� ���������� ����������
            string textToDisplay = russianText;

            // ����������� ������ ����� � ����������� �� �����
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
                    // ��� ��������� ������� ��������� textToDisplay = englishText
            }

            // ������������� ����� � ��������������� UI-���������
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
                Debug.LogWarning("�� ������ ��������� Text ��� TextMeshProUGUI �� �������!");
            }
        }
        else
        {
            // ���������� ������� ����
            string currentLang = GetSystemLanguageCode();
            // �� ��������� ���������� ����������
            string textToDisplay = englishText;

            // ����������� ������ ����� � ����������� �� �����
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
                    // ��� ��������� ������� ��������� textToDisplay = englishText
            }

            // ������������� ����� � ��������������� UI-���������
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
                Debug.LogWarning("�� ������ ��������� Text ��� TextMeshProUGUI �� �������!");
            }
        }
    }

    // ����� ��� ��������� ���� ����� �� ������ ���������� �����
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
                return "en"; // �������� �� ���������
        }
    }

}
