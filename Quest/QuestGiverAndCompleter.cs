using System.Collections.Generic;
using UnityEngine;

public class QuestGiverAndCompleter : MonoBehaviour
{
    // ID квестов, которые этот NPC может завершать
    public string[] questIDsToComplete;

    // Квесты, которые этот NPC может выдать после завершения предыдущих
    public Quest[] questsToGive;

    // Таймаут между взаимодействиями с NPC
    [Tooltip("Время в секундах, которое должно пройти между взаимодействиями с NPC")]
    public float interactionCooldown = 1.5f;
    private float lastInteractionTime = -1000f; // Инициализация отрицательным значением для первого взаимодействия

    // Принудительное завершение квестов
    [Tooltip("Если включено, квесты будут завершаться даже если они не готовы к завершению, но НЕ БУДУТ завершаться в статусе InProgress, если цели не выполнены")]
    public bool forceCompleteQuests = true;

    // Словарь: ID завершенного квеста -> индекс следующего квеста для выдачи
    private Dictionary<string, int> questProgressMap = new Dictionary<string, int>();

    // Для предотвращения мгновенного завершения квестов
    private Dictionary<string, float> questReceiveTime = new Dictionary<string, float>();
    private float minTimeBeforeCanComplete = 1.0f; // Минимальное время (в секундах) между получением и завершением квеста
    
    // Ссылка на компонент эскорта на этом же NPC (если он есть)
    private EscortNPC escortComponent;

    private void Start()
    {
        // Инициализация словаря
        for (int i = 0; i < questIDsToComplete.Length; i++)
        {
            questProgressMap[questIDsToComplete[i]] = i;
        }
        
        // Получаем компонент эскорта на этом же объекте
        escortComponent = GetComponent<EscortNPC>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {
            // Проверяем, прошло ли достаточно времени с последнего взаимодействия
            if (Time.time - lastInteractionTime < interactionCooldown)
            {
                Debug.Log($"Слишком быстрое взаимодействие. Подождите {interactionCooldown} секунд.");
                return;
            }
            
            lastInteractionTime = Time.time; // Обновляем время последнего взаимодействия
            ProcessQuests();
        }
    }

    private void ProcessQuests()
    {
        bool questCompleted = false;

        // Сначала проверяем, можем ли мы завершить какой-то квест
        foreach (string questID in questIDsToComplete)
        {
            Quest questToComplete = QuestManager.Instance.activeQuests.Find(q => q.questID == questID);
            if (questToComplete != null)
            {
                // Проверяем, когда был получен этот квест (для предотвращения мгновенного завершения)
                if (!questReceiveTime.ContainsKey(questID))
                {
                    // Если это первое взаимодействие с квестом, запоминаем время
                    questReceiveTime[questID] = Time.time;
                }
                
                // Проверяем, прошло ли минимальное время с момента получения квеста
                float timeSinceReceived = Time.time - questReceiveTime[questID];
                if (timeSinceReceived < minTimeBeforeCanComplete)
                {
                    Debug.Log($"Квест {questID} был получен слишком недавно. Необходимо подождать.");
                    continue;
                }

                bool canComplete = false;
                
                // Специальная обработка для квеста quest07
                if (questID == "quest07")
                {
                    Debug.Log($"ProcessQuests: Обнаружен специальный квест quest07, устанавливаем его сразу в статус ReadyToComplete");
                    questToComplete.status = QuestStatus.ReadyToComplete;
                    
                    // Убедимся, что все цели выполнены
                    if (questToComplete.objectives != null)
                    {
                        foreach (var objective in questToComplete.objectives)
                        {
                            if (objective != null && objective.type == QuestObjective.ObjectiveType.Talk)
                            {
                                objective.currentAmount = objective.requiredAmount;
                                objective.isCompleted = true;
                            }
                        }
                    }
                    
                    canComplete = true;
                }
                // Обычная обработка для других квестов
                else if (questToComplete.status == QuestStatus.ReadyToComplete || 
                    questToComplete.status == QuestStatus.Completed)
                {
                    canComplete = true;
                    Debug.Log($"Квест {questToComplete.questID} готов к завершению.");
                }
                else if (questToComplete.status == QuestStatus.InProgress)
                {
                    // Проверяем, выполнены ли все цели квеста
                    bool allObjectivesCompleted = questToComplete.IsCompleted();
                    
                    if (allObjectivesCompleted)
                    {
                        canComplete = true;
                        Debug.Log($"Все цели квеста {questToComplete.questID} выполнены.");
                    }
                    else if (forceCompleteQuests)
                    {
                        // Проверяем quest02 или любой квест типа Talk отдельно, чтобы можно было их сдать
                        if (questID == "quest02" || questToComplete.HasObjectiveOfType(QuestObjective.ObjectiveType.Talk))
                        {
                            canComplete = true;
                            Debug.Log($"Принудительное завершение квеста {questID}, так как это quest02 или квест типа Talk");
                            
                            // Для квестов типа Talk, устанавливаем все цели как выполненные
                            if (questToComplete.HasObjectiveOfType(QuestObjective.ObjectiveType.Talk))
                            {
                                foreach (var objective in questToComplete.objectives)
                                {
                                    if (objective != null && objective.type == QuestObjective.ObjectiveType.Talk)
                                    {
                                        objective.currentAmount = objective.requiredAmount;
                                        objective.isCompleted = true;
                                    }
                                }
                                // Устанавливаем статус квеста как готовый к завершению
                                questToComplete.status = QuestStatus.ReadyToComplete;
                            }
                        }
                        else
                        {
                            // Выводим информацию о прогрессе квеста
                            Debug.Log($"Квест {questToComplete.questID} не выполнен. Статус: {questToComplete.status}");
                            if (questToComplete.objectives != null)
                            {
                                foreach (var objective in questToComplete.objectives)
                                {
                                    if (objective != null)
                                    {
                                        Debug.Log($"- Цель: {objective.objectiveDescription}, " +
                                                 $"Прогресс: {objective.currentAmount}/{objective.requiredAmount}");
                                    }
                                }
                            }
                            
                            canComplete = false;  // Нельзя завершить квест, если его цели не выполнены
                        }
                    }
                    else
                    {
                        // Выводим информацию о прогрессе квеста
                        Debug.Log($"Квест {questToComplete.questID} не выполнен. Статус: {questToComplete.status}");
                        if (questToComplete.objectives != null)
                        {
                            foreach (var objective in questToComplete.objectives)
                            {
                                if (objective != null)
                                {
                                    Debug.Log($"- Цель: {objective.objectiveDescription}, " +
                                             $"Прогресс: {objective.currentAmount}/{objective.requiredAmount}");
                                }
                            }
                        }
                        
                        canComplete = false;  // Нельзя завершить квест, если его цели не выполнены
                    }
                }
                else if (forceCompleteQuests)
                {
                    // Принудительное завершение применяется для других статусов
                    canComplete = true;
                    Debug.Log($"Принудительное завершение квеста {questToComplete.questID}.");
                }
                
                if (!canComplete)
                {
                    Debug.Log($"Квест {questToComplete.questID} еще не готов к завершению.");
                    continue; // Пропускаем квест, если он не готов к завершению
                }
                
                // Завершаем квест
                Debug.Log($"Завершаем квест {questToComplete.questID}. Текущий статус: {questToComplete.status}");
                
                // Проверяем, является ли квест квестом типа Talk
                if (questToComplete.HasObjectiveOfType(QuestObjective.ObjectiveType.Talk))
                {
                    Debug.Log($"Квест {questToComplete.questID} - это квест типа Talk, используем специальный метод CompleteTalkQuest");
                    QuestManager.Instance.CompleteTalkQuest(questToComplete);
                }
                else
                {
                    QuestManager.Instance.CompleteQuest(questToComplete);
                }
                
                questCompleted = true;
                
                // После завершения квеста запоминаем время получения следующего
                // Это важно для предотвращения мгновенного завершения цепочки
                string nextQuestID = null;
                if (questProgressMap.TryGetValue(questID, out int nextQuestIndex) &&
                    nextQuestIndex < questsToGive.Length)
                {
                    nextQuestID = questsToGive[nextQuestIndex].questID;
                    questReceiveTime[nextQuestID] = Time.time;
                    
                    Debug.Log($"Выдаем следующий квест {questsToGive[nextQuestIndex].questID}");
                    Quest nextQuest = questsToGive[nextQuestIndex];
                    QuestManager.Instance.StartQuest(nextQuest);
                    
                    // Проверяем, является ли следующий квест квестом эскорта и если да, активируем эскорт
                    CheckAndActivateEscort(nextQuest.questID);
                }

                break; // Обрабатываем только один квест за раз
            }
        }

        // Если никакой квест не был завершен, проверяем, можем ли мы выдать новый квест
        if (!questCompleted)
        {
            // Проверяем каждый квест для выдачи
            for (int i = 0; i < questsToGive.Length; i++)
            {
                Quest questToGive = questsToGive[i];

                // Проверяем, не активен ли уже этот квест
                bool isActive = QuestManager.Instance.activeQuests.Exists(q => q.questID == questToGive.questID);

                // Проверяем, не завершен ли уже этот квест
                bool isCompleted = QuestManager.Instance.completedQuests.Exists(q => q.questID == questToGive.questID);

                // Если квест подходящий квест существует, и он не активен и не завершен, выдаем его
                if (!isActive && !isCompleted)
                {
                    // Проверяем, завершен ли предыдущий квест (если это не первый квест)
                    bool canGive = (i == 0); // Первый квест всегда можно выдать

                    if (i > 0)
                    {
                        string prevQuestID = questIDsToComplete[i - 1];
                        canGive = QuestManager.Instance.completedQuests.Exists(q => q.questID == prevQuestID);
                    }

                    if (canGive)
                    {
                        // Запоминаем время получения квеста
                        questReceiveTime[questToGive.questID] = Time.time;
                        
                        Debug.Log($"Выдаем квест {questToGive.questID}");
                        QuestManager.Instance.StartQuest(questToGive);
                        
                        // Проверяем, является ли квест квестом эскорта и если да, активируем эскорт
                        CheckAndActivateEscort(questToGive.questID);
                        
                        break;
                    }
                }
            }
        }
    }
    
    // Метод для проверки и активации эскорта
    private void CheckAndActivateEscort(string questID)
    {
        // Проверяем, есть ли на этом NPC компонент EscortNPC
        if (escortComponent != null)
        {
            // Проверяем, совпадает ли ID квеста с ID квеста эскорта
            if (escortComponent.associatedQuestID == questID || questID == "quest05")
            {
                Debug.Log($"Активируем эскорт для квеста {questID}");
                escortComponent.ActivateEscort();
            }
        }
    }
}