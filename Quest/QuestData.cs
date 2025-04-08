using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/QuestData")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string questName;
    public string description; // �������� �� ���������, ���� ����������� �� �������                              
    public QuestData acceptedQuestData; // ������ �� ��������� �����
    public string completionDialogue; // Диалоговая реплика при завершении квеста

    public List<QuestDialogue> dialogues;
    public List<QuestCompletionLocalization> completionLocalizations;
    public List<QuestTitleLocalization> titleLocalizations;
    public List<QuestDescriptionLocalization> descriptionLocalizations;

    // ����� ��� ��������� �������������� ���������� ���� (��� ����)
    public string[] GetLocalizedDialogues(string lang)
    {
        QuestDialogue dialogue = dialogues.Find(d => d.languageCode.ToUpper() == lang.ToUpper());
        if (dialogue != null)
        {
            return dialogue.phrases;
        }
        dialogue = dialogues.Find(d => d.languageCode.ToUpper() == "EN");
        return dialogue != null ? dialogue.phrases : new string[0];
    }

    // ����� ����� ��� ��������� ��������� � ����� ������ �� ������ �����
    public string GetCompletionMessage(string lang)
    {
        QuestCompletionLocalization localization = completionLocalizations.Find(loc => loc.languageCode.ToUpper() == lang.ToUpper());
        if (localization != null)
        {
            return localization.completionMessage;
        }
        localization = completionLocalizations.Find(loc => loc.languageCode.ToUpper() == "EN");
        return localization != null ? localization.completionMessage : "Quest completed! Congratulations!";
    }
    // ����� ��� ��������� ��������������� �������� ������
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

    // Метод для получения локализованной диалоговой реплики при завершении квеста
    public string GetLocalizedCompletionDialogue(string lang)
    {
        QuestCompletionLocalization localization = completionLocalizations.Find(loc => loc.languageCode.ToUpper() == lang.ToUpper());
        if (localization != null && !string.IsNullOrEmpty(localization.completionDialogue))
        {
            return localization.completionDialogue;
        }
        
        // Если не найдена локализация для указанного языка, ищем для английского
        localization = completionLocalizations.Find(loc => loc.languageCode.ToUpper() == "EN");
        if (localization != null && !string.IsNullOrEmpty(localization.completionDialogue))
        {
            return localization.completionDialogue;
        }
        
        // Если локализованная реплика не найдена, возвращаем стандартную
        return completionDialogue;
    }
}