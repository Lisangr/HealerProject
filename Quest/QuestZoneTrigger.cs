using System.Collections.Generic;
using UnityEngine;

public class QuestZoneTrigger : MonoBehaviour
{
    public string questIDToComplete = "quest02"; // ID ������, ������� ����� ���������

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
            // ������� �������� ����� � ������ ID
            Quest questToComplete = QuestManager.Instance.activeQuests.Find(q => q.questID == questIDToComplete);
            if (questToComplete != null)
            {
                Debug.Log($"����� ����� � ���� ���������� ������: {questToComplete.questName}");

                // ��������� ���������� � ��������� ������
                QuestData nextQuestData = null;
                QuestLine currentQuestLine = questToComplete.questLine;

                if (questToComplete.questData != null && questToComplete.questData.acceptedQuestData != null)
                {
                    nextQuestData = questToComplete.questData.acceptedQuestData;
                }

                // ��������� ������� �����
                QuestManager.Instance.CompleteQuestAutomatically(questToComplete);

                // ��������� ��������� �����, ���� �� ����
                if (nextQuestData != null)
                {
                    // ���� ����������� ���� ����� StartNextQuest
                    // QuestManager.Instance.StartNextQuest(nextQuestData, currentQuestLine);

                    // ��� �������� ����� � ��������� ���
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


