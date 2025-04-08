using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewKillQuestData", menuName = "Quests/KillQuestData")]
public class KillQuestData : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;
    public int reward;

    [Tooltip("�������� ����� (��� ��� ID), ������� ������ ��������� � ���������� targetID � QuestObjective")]
    public string targetEnemyName;

    [Tooltip("���������� ������, ������� ����� ����� ��� ���������� ������")]
    public int requiredKillCount;

    [Header("����������� ���������� ������")]
    public List<QuestCompletionLocalization> completionLocalizations;
    public List<QuestTitleLocalization> titleLocalizations;
    public List<QuestDescriptionLocalization> descriptionLocalizations;

    // ����� ��������� ��������������� ��������� � ����� ������
    public string GetCompletionMessage(string lang)
    {
        QuestCompletionLocalization localization = completionLocalizations.Find(
            loc => loc.languageCode.ToUpper() == lang.ToUpper());
        if (localization != null)
        {
            return localization.completionMessage;
        }
        localization = completionLocalizations.Find(
            loc => loc.languageCode.ToUpper() == "EN");
        return localization != null ? localization.completionMessage : "Quest completed! Congratulations!";
    }
    public string GetLocalizedTitle(string lang)
    {
        QuestTitleLocalization localization = titleLocalizations.Find(t => t.languageCode.ToUpper() == lang.ToUpper());
        if (localization != null)
            return localization.title;

        // ���� �� ������ ������� �� ������ �����, ������� "EN" ��� ���������� �������� �� ���������
        localization = titleLocalizations.Find(t => t.languageCode.ToUpper() == "EN");
        return localization != null ? localization.title : questName;
    }

    // ����� ��� ��������� ��������������� �������� ������
    public string GetLocalizedDescription(string lang)
    {
        QuestDescriptionLocalization localization = descriptionLocalizations.Find(d => d.languageCode.ToUpper() == lang.ToUpper());
        if (localization != null)
            return localization.description;

        localization = descriptionLocalizations.Find(d => d.languageCode.ToUpper() == "EN");
        return localization != null ? localization.description : description;
    }
}