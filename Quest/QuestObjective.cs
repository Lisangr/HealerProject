using System;
using UnityEngine;

[Serializable]
public class QuestObjective
{
    public string objectiveDescription;
    public bool isCompleted = false;

    // ��� ����: ��������, ��������, �������������, ���� ��� ������
    public enum ObjectiveType { Kill, Escort, Collect, Talk }
    public ObjectiveType type;

    // ��������� ��� ������������ ���������� ����
    public int requiredAmount;
    public int currentAmount;

    // ����� ���� ��� �������� ���� ��� �������� (��������, ID ��� ��� ����)
    public string targetID;

    // ���� ��� ��������������� ���������� ������
    public bool canAutoEnded = false;

    public void UpdateProgress(int amount, Quest quest)
    {
        try
        {
            if (quest == null)
            {
                Debug.LogError("UpdateProgress: quest равен null");
                return;
            }
            
            currentAmount += amount;
            if (currentAmount >= requiredAmount)
            {
                isCompleted = true;
                Debug.Log($"Цель '{objectiveDescription}' выполнена!");
            }

            // Если цель завершена и поле canAutoEnded установлено, обновляем весь квест
            if (isCompleted && canAutoEnded)
            {
                Debug.Log($"Цель типа {type} с автозавершением выполнена, выполняем CheckQuestCompletion()");
                quest.CheckQuestCompletion();
                
                // Для целей типа Talk с автозавершением, сразу делаем квест готовым к завершению
                if (type == ObjectiveType.Talk)
                {
                    Debug.Log($"Цель типа Talk выполнена, устанавливаем статус квеста ReadyToComplete");
                    quest.status = QuestStatus.ReadyToComplete;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка в UpdateProgress: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
