using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestStatus
{
    NotStarted,
    InProgress,
    ReadyToComplete,
    Completed,
    Failed
}
public enum QuestLine
{
    Main,
    Additional
}
[Serializable]
public class Quest
{
    public string questID;
    public string questName;
    public string description;
    public List<QuestObjective> objectives;
    public QuestStatus status = QuestStatus.NotStarted;
    public int reward;
    public QuestObjective.ObjectiveType type;
    public QuestData questData;
    public QuestLine questLine;

    // ����, ������������, ����� �� ��������� ����� �������������
    public bool canAutoEnded = false;
    public bool IsCompleted()
    {
        try
        {
            if (objectives == null)
            {
                Debug.LogWarning($"IsCompleted: objectives равен null для квеста '{questName}'");
                return false;
            }
            
            foreach (var objective in objectives)
            {
                if (objective == null)
                {
                    Debug.LogWarning($"IsCompleted: обнаружена null-цель в квесте '{questName}'");
                    continue;
                }
                
                if (!objective.isCompleted)
                    return false;
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка в IsCompleted: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }
    //    ���������� ���� �����
    public void CheckQuestCompletion()
    {
        try
        {
            // Проверяем, выполнены ли все цели
            if (IsCompleted())
            {
                // Если все цели выполнены, меняем статус квеста
                status = QuestStatus.ReadyToComplete;
                Debug.Log($"Квест '{questName}' готов к завершению.");
                
                // Если квест имеет флаг автозавершения
                if (canAutoEnded)
                {
                    Debug.Log($"Квест '{questName}' имеет флаг автозавершения и будет автоматически завершен.");
                    
                    // Проверяем, есть ли у квеста следующий квест в цепочке
                    if (questData != null && questData.acceptedQuestData != null)
                    {
                        Debug.Log($"Квест '{questName}' имеет следующий квест в цепочке.");
                        
                        if (QuestManager.Instance != null)
                        {
                            // Если это квест типа Talk, используем специальный метод
                            if (HasObjectiveOfType(QuestObjective.ObjectiveType.Talk))
                            {
                                Debug.Log($"Квест '{questName}' является квестом типа Talk, используем CompleteTalkQuest");
                                QuestManager.Instance.CompleteTalkQuest(this);
                            }
                            else
                            {
                                Debug.Log($"Квест '{questName}' не является квестом типа Talk, используем AdvanceQuestChain");
                                QuestManager.Instance.AdvanceQuestChain(questID);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"QuestManager.Instance равен null, невозможно продвинуть цепочку квестов для '{questName}'");
                        }
                    }
                    else
                    {
                        // Если следующего квеста нет, просто завершаем текущий
                        if (QuestManager.Instance != null)
                        {
                            Debug.Log($"Квест '{questName}' не имеет следующего квеста, просто завершаем его");
                            
                            // Если это квест типа Talk, используем специальный метод
                            if (HasObjectiveOfType(QuestObjective.ObjectiveType.Talk))
                            {
                                Debug.Log($"Квест '{questName}' является квестом типа Talk, используем CompleteTalkQuest");
                                QuestManager.Instance.CompleteTalkQuest(this);
                            }
                            else
                            {
                                Debug.Log($"Квест '{questName}' не является квестом типа Talk, используем CompleteQuestAutomatically");
                                QuestManager.Instance.CompleteQuestAutomatically(this);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"QuestManager.Instance равен null, невозможно автоматически завершить квест '{questName}'");
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка в CheckQuestCompletion: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    // Метод для проверки наличия целей определенного типа
    public bool HasObjectiveOfType(QuestObjective.ObjectiveType type)
    {
        if (objectives == null)
            return false;
            
        foreach (var objective in objectives)
        {
            if (objective != null && objective.type == type)
                return true;
        }
        return false;
    }
}
