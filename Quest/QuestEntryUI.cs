using UnityEngine;
using UnityEngine.UI;

public class QuestEntryUI : MonoBehaviour
{
    public Text questNameText;         // ��������� � ���������� Text-��������� ��� ��������
    public Text questDescriptionText;  // ��������� � ���������� Text-��������� ��� ��������

    // ����� ��� ��������� ������ ������ � UI � ������������
    public void Setup(Quest quest, string currentLang)
    {
        Debug.Log($"Setup для квеста: {quest.questName}, QuestData: {(quest.questData != null ? "есть" : "отсутствует")}");
        
        // Если у квеста есть QuestData, используем локализованные данные
        if (quest.questData != null)
        {
            if (questNameText != null)
            {
                string title = quest.questData.GetLocalizedTitle(currentLang);
                questNameText.text = title;
                Debug.Log($"Установлено локализованное название: {title}");
            }
            if (questDescriptionText != null)
            {
                string description = quest.questData.GetLocalizedDescription(currentLang);
                questDescriptionText.text = description;
                Debug.Log($"Установлено локализованное описание: {description}");
            }
        }
        else
        {
            // Если QuestData отсутствует, используем стандартные данные квеста
            if (questNameText != null)
            {
                questNameText.text = quest.questName;
                Debug.Log($"Установлено стандартное название: {quest.questName}");
            }
            if (questDescriptionText != null)
            {
                questDescriptionText.text = quest.description;
                Debug.Log($"Установлено стандартное описание: {quest.description}");
            }
        }
    }
    public void Setup(KillQuestData killQuest, string currentLang)
    {
        // ���� ��������� �����������, ����� ������������ �:
        // ������: ���� � KillQuestData ���� ������ GetLocalizedTitle/GetLocalizedDescription
        if (killQuest != null)
        {
            if (questNameText != null)
                questNameText.text = killQuest.GetLocalizedTitle(currentLang);
            if (questDescriptionText != null)
                questDescriptionText.text = killQuest.GetLocalizedDescription(currentLang);
        }
    }

}
