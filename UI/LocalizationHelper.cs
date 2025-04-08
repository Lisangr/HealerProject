using UnityEngine;
using YG;

public static class LocalizationHelper
{
    public static string GetSystemLanguageCode()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            string currentLang = YandexGame.EnvironmentData.language.ToLower();
            switch (currentLang)
            {
                case "ru": return "ru";
                case "en": return "en";
                case "tr": return "tr";
                case "de": return "de";
                case "es": return "es";
                case "it": return "it";
                case "fr": return "fr";
                default: return "en";
            }
        }
        else
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.English: return "en";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.French: return "fr";
                default: return "en";
            }
        }
    }
}
