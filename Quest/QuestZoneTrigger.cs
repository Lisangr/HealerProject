using System.Collections.Generic;
using UnityEngine;

public class QuestZoneTrigger : MonoBehaviour
{
    public string questIDToComplete = "quest02"; // ID квеста, который нужно завершить

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CompleteQuestIfActive();
        }
    }

    private void CompleteQuestIfActive()
    {
        if (QuestManager.Instance != null)
        {
            // Находим активный квест с нужным ID
            Quest questToComplete = QuestManager.Instance.activeQuests.Find(q => q.questID == questIDToComplete);
            if (questToComplete != null)
            {
                Debug.Log($"Игрок вошел в зону завершения квеста: {questToComplete.questName}");

                // Сохраняем информацию о следующем квесте
                QuestData nextQuestData = null;
                QuestLine currentQuestLine = questToComplete.questLine;

                if (questToComplete.questData != null && questToComplete.questData.acceptedQuestData != null)
                {
                    nextQuestData = questToComplete.questData.acceptedQuestData;
                }

                // Завершаем текущий квест
                QuestManager.Instance.CompleteQuestAutomatically(questToComplete);

                // Запускаем следующий квест, если он есть
                if (nextQuestData != null)
                {
                    // Если используете свой метод StartNextQuest
                    // QuestManager.Instance.StartNextQuest(nextQuestData, currentQuestLine);

                    // Или создайте квест и запустите его
                    Quest nextQuest = new Quest
                    {
                        questID = nextQuestData.questID,
                        questName = nextQuestData.questName,
                        description = nextQuestData.description,
                        reward = 100,
                        questData = nextQuestData,
                        questLine = currentQuestLine
                    };

                    QuestManager.Instance.StartQuest(nextQuest);
                }
            }
        }
    }
}


