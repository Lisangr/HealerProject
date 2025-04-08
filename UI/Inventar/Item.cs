using System.Collections.Generic;
using UnityEngine;
using YG;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName = "New Item";
    public GameObject prefab = null;
    public Sprite icon = null;
    public int quantity = 1;

    [Header("Localization")]
    [Tooltip("Ћокализованные данные дл€ предмета")]
    public List<ItemLocalization> localizations;

    /// <summary>
    /// ¬озвращает локализованное название предмета согласно системному €зыку.
    /// ≈сли дл€ €зыка нет локализации, возвращаетс€ базовое название.
    /// </summary>
    public string GetLocalizedItemName()
    {
        string langCode = LocalizationHelper.GetSystemLanguageCode();
        if (localizations == null)
        {
            return itemName;
        }

        foreach (var loc in localizations)
        {
            if (loc.languageCode.ToLower() == langCode.ToLower())
            {
                return loc.localizedName;
            }
        }

        return itemName;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    // ћетод дл€ определени€ кода €зыка (использу€ данные YandexGame или Application.systemLanguage)
    private string GetSystemLanguageCode()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            string currentLang = YandexGame.EnvironmentData.language.ToLower();
            switch (currentLang)
            {
                case "ru":
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
                    return "en";
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
                    return "en";
            }
        }
    }
}