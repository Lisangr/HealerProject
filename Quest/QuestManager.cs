using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System; // Добавляем для использования System.Action

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();
    // Список квестов, которые будут выданы по очереди
    public Queue<Quest> questQueue = new Queue<Quest>();

    // Событие, которое вызывается при завершении квеста
    public event Action<Quest> OnQuestCompleted;
    
    // Событие для подписки на начало квеста
    public event Action<Quest> OnQuestStarted;

    // Ожидаемый порядок разговора и требуемое количество событий
    /*public int currentTalkStep = 1;
    public int requiredTalkEvents = 4;
    public int talkEventCount = 0;*/
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Проверяем наличие QuestDisplay в сцене
        if (QuestDisplay.Instance == null)
        {
            Debug.LogWarning("QuestDisplay.Instance не найден в сцене. Отложим обновление квестов.");
            // Откладываем инициализацию квестов до следующего кадра, 
            // чтобы дать время QuestDisplay инициализироваться
            StartCoroutine(InitializeQuestsNextFrame());
            return;
        }
        
        InitializeQuests();
    }
    
    private System.Collections.IEnumerator InitializeQuestsNextFrame()
    {
        // Ждем следующий кадр
        yield return null;
        
        // Проверяем еще раз
        if (QuestDisplay.Instance == null)
        {
            Debug.LogWarning("QuestDisplay.Instance все еще не найден. Квесты не будут обновлены.");
            yield break;
        }
        
        InitializeQuests();
    }
    
    private void InitializeQuests()
    {
        // Запускаем все квесты, которые были назначены в инспекторе
        if (activeQuests.Count > 0)
        {
            Debug.Log($"При старте сцены найдено {activeQuests.Count} активных квестов");
            
            // Создаем копию списка, чтобы избежать проблем с изменением коллекции
            List<Quest> questsToStart = new List<Quest>(activeQuests);
            
            // Очищаем список активных квестов
            activeQuests.Clear();
            
            // Запускаем каждый квест через метод StartQuest
            foreach (Quest quest in questsToStart)
            {
                StartQuest(quest);
                Debug.Log($"Автоматически запущен квест: {quest.questName}");
            }
        }
        else
        {
            Debug.Log("При старте сцены не найдено активных квестов");
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyKilled += OnEnemyKilled;
        EscortNPC.OnEscortCompleteEvent += OnEscortCompleteHandler;
        CollectibleItem.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= OnEnemyKilled;
        EscortNPC.OnEscortCompleteEvent -= OnEscortCompleteHandler;
        CollectibleItem.OnItemCollected -= HandleItemCollected;
    }
    private void OnEnemyKilled(string enemyName)
    {
        Debug.Log($"OnEnemyKilled вызван для врага: {enemyName}");
        
        if (string.IsNullOrEmpty(enemyName))
        {
            Debug.LogWarning("OnEnemyKilled: получено пустое имя врага");
            return;
        }
        
        if (activeQuests == null || activeQuests.Count == 0)
        {
            Debug.Log("OnEnemyKilled: нет активных квестов");
            return;
        }
        
        // Итерация по копии списка, чтобы избежать ошибки модификации коллекции
        try
        {
            List<Quest> questsToCheck = new List<Quest>(activeQuests);
            Debug.Log($"OnEnemyKilled: проверяем {questsToCheck.Count} активных квестов");
            
            foreach (Quest quest in questsToCheck)
            {
                if (quest == null)
                {
                    Debug.LogWarning("OnEnemyKilled: обнаружен null-квест в активных квестах");
                    continue;
                }
                
                if (quest.objectives == null)
                {
                    Debug.LogWarning($"OnEnemyKilled: у квеста '{quest.questName}' отсутствуют цели (objectives == null)");
                    continue;
                }
                
                bool updated = false;
                foreach (QuestObjective objective in quest.objectives)
                {
                    if (objective == null)
                    {
                        Debug.LogWarning($"OnEnemyKilled: у квеста '{quest.questName}' обнаружена null-цель");
                        continue;
                    }
                    
                    try
                    {
                        if (objective.type == QuestObjective.ObjectiveType.Kill &&
                            objective.targetID == enemyName &&
                            !objective.isCompleted)
                        {
                            Debug.Log($"OnEnemyKilled: найдена подходящая цель для врага {enemyName} в квесте '{quest.questName}'");
                            
                            // Обновляем прогресс
                            objective.UpdateProgress(1, quest);
                            Debug.Log($"Прогресс цели убийства {enemyName}: {objective.currentAmount}/{objective.requiredAmount}");
                            updated = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Ошибка при обновлении прогресса цели: {ex.Message}\n{ex.StackTrace}");
                    }
                }
                
                if (updated)
                {
                    try
                    {
                        if (quest.IsCompleted())
                        {
                            quest.status = QuestStatus.ReadyToComplete;
                            Debug.Log($"Квест '{quest.questName}' готов к завершению. Поговорите с NPC для сдачи квеста.");
                            
                            // Проверка на автоматическое завершение
                            if (quest.canAutoEnded)
                            {
                                try
                                {
                                    // Сначала сохраняем информацию о следующем квесте перед завершением текущего
                                    QuestData nextQuestData = null;
                                    QuestLine currentQuestLine = quest.questLine;
                                    if (quest.questData != null && quest.questData.acceptedQuestData != null)
                                    {
                                        nextQuestData = quest.questData.acceptedQuestData;
                                    }
                                    
                                    // Завершаем текущий квест
                                    CompleteQuestAutomatically(quest);
                                    Debug.Log($"Квест '{quest.questName}' ЗАВЕРШЕН АВТОМАТИЧЕСКИ.");
                                    
                                    // Если есть следующий квест, создаем и запускаем его
                                    if (nextQuestData != null)
                                    {
                                        StartNextQuest(nextQuestData, currentQuestLine);
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogError($"Ошибка при автоматическом завершении квеста: {ex.Message}\n{ex.StackTrace}");
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Ошибка при проверке завершения квеста: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Критическая ошибка в OnEnemyKilled: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnEscortCompleteHandler(EscortNPC escort)
    {
        // Здесь можно обновить квест, который связан с сопровождением.
        // Например, найти квест с типом Escort среди активных и изменить его статус
        Quest escortQuest = activeQuests.FirstOrDefault(q => q.type == QuestObjective.ObjectiveType.Escort);
        if (escortQuest != null)
        {
            escortQuest.status = QuestStatus.ReadyToComplete;
            Debug.Log($"Квест '{escortQuest.questName}' готов к сдаче (сопровождение завершено).");

            if (escortQuest.canAutoEnded)
            {
                try
                {
                    // Сначала сохраняем информацию о следующем квесте перед завершением текущего
                    QuestData nextQuestData = null;
                    QuestLine currentQuestLine = escortQuest.questLine;
                    if (escortQuest.questData != null && escortQuest.questData.acceptedQuestData != null)
                    {
                        nextQuestData = escortQuest.questData.acceptedQuestData;
                    }
                    
                    // Завершаем текущий квест
                    CompleteQuestAutomatically(escortQuest);
                    Debug.Log($"Квест '{escortQuest.questName}' ЗАВЕРШЕН АВТОМАТИЧЕСКИ.");
                    
                    // Если есть следующий квест, создаем и запускаем его
                    if (nextQuestData != null)
                    {
                        StartNextQuest(nextQuestData, currentQuestLine);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Ошибка при автоматическом завершении квеста сопровождения: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
    // Метод обработки события сбора
    private void HandleItemCollected(CollectibleItem item)
    {
        Debug.Log($"[QuestManager] Обработка сбора предмета: {item.itemID}");

        // Создаем копию списка активных квестов для безопасного перебора
        foreach (Quest quest in activeQuests.ToList())
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type == QuestObjective.ObjectiveType.Collect && !objective.isCompleted)
                {
                    objective.UpdateProgress(1, quest);
                    Debug.Log($"Квест '{quest.questName}': {objective.objectiveDescription} - {objective.currentAmount}/{objective.requiredAmount}");
                    // Если цель выполнена, меняем статус квеста на ReadyToComplete
                    if (quest.canAutoEnded)
                    {
                        try
                        {
                            // Сначала сохраняем информацию о следующем квесте перед завершением текущего
                            QuestData nextQuestData = null;
                            QuestLine currentQuestLine = quest.questLine;
                            if (quest.questData != null && quest.questData.acceptedQuestData != null)
                            {
                                nextQuestData = quest.questData.acceptedQuestData;
                            }
                            
                            // Завершаем текущий квест
                            CompleteQuestAutomatically(quest);
                            Debug.Log($"Квест '{quest.questName}' ЗАВЕРШЕН АВТОМАТИЧЕСКИ.");
                            
                            // Если есть следующий квест, создаем и запускаем его
                            if (nextQuestData != null)
                            {
                                StartNextQuest(nextQuestData, currentQuestLine);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"Ошибка при автоматическом завершении квеста сбора: {ex.Message}\n{ex.StackTrace}");
                        }
                    }
                    else if (objective.isCompleted)
                    {
                        quest.status = QuestStatus.ReadyToComplete;
                        Debug.Log($"Квест '{quest.questName}' готов к сдаче (сбор предметов завершён).");
                    }
                }
            }
        }
    }


    // Метод для запуска нового квеста
    public void StartQuest(Quest newQuest)
    {
        Debug.Log($"StartQuest вызван для квеста: {newQuest.questName}");
        
        // Проверяем, существует ли уже квест с таким ID среди активных квестов
        bool questAlreadyActive = activeQuests.Exists(q => q.questID == newQuest.questID);
        
        // Проверяем, существует ли уже квест с таким ID среди завершенных квестов
        bool questAlreadyCompleted = completedQuests.Exists(q => q.questID == newQuest.questID);
        
        if (questAlreadyActive)
        {
            Debug.LogWarning($"StartQuest: Квест с ID {newQuest.questID} уже активен. Пропускаем добавление.");
            return;
        }
        
        if (questAlreadyCompleted)
        {
            Debug.LogWarning($"StartQuest: Квест с ID {newQuest.questID} уже завершен. Пропускаем добавление.");
            return;
        }
        
        if (!activeQuests.Contains(newQuest)) // Дополнительная проверка на дубликат объекта
        {
            newQuest.status = QuestStatus.InProgress;
            activeQuests.Add(newQuest);
            Debug.Log($"Квест '{newQuest.questName}' начат. Всего активных квестов: {activeQuests.Count}");
            
            // Уведомляем подписчиков о начале квеста
            OnQuestStarted?.Invoke(newQuest);
            
            // Проверяем наличие QuestDisplay перед отображением диалога
            if (QuestDisplay.Instance == null)
            {
                Debug.LogWarning($"QuestDisplay.Instance отсутствует. Диалог для квеста '{newQuest.questName}' не будет отображен.");
            }
            else
            {
                // Отображаем диалоговую реплику при начале квеста (если она есть)
                if (newQuest.questData != null && newQuest.questData.dialogues != null && newQuest.questData.dialogues.Count > 0)
                {
                    string currentLang = GetSystemLanguageCode();
                    string[] dialogues = newQuest.questData.GetLocalizedDialogues(currentLang);
                    
                    if (dialogues.Length > 0)
                    {
                        // Если есть несколько фраз, используем SetQuestDialogues
                        if (dialogues.Length > 1)
                        {
                            QuestDisplay.Instance.SetQuestDialogues(dialogues);
                            Debug.Log($"Отображен диалог начала квеста из {dialogues.Length} фраз");
                        }
                        else
                        {
                            // Если только одна фраза, используем SetQuestText
                            QuestDisplay.Instance.SetQuestText(dialogues[0]);
                            Debug.Log($"Отображен диалог начала квеста: {dialogues[0]}");
                        }
                    }
                }
            }
            
            // Обновляем список активных квестов
            UpdateAllQuestsDisplay();
        }
        else
        {
            Debug.LogWarning($"Квест '{newQuest.questName}' уже активен. Не добавляем повторно.");
        }
    }

    public void UpdateAllQuestsDisplay()
    {
        Debug.Log($"UpdateAllQuestsDisplay вызван. QuestDisplay.Instance: {(QuestDisplay.Instance != null ? "есть" : "отсутствует")}");
        if (QuestDisplay.Instance != null)
        {
            QuestDisplay.Instance.UpdateQuestsDisplay(activeQuests, completedQuests);
        }
        else
        {
            Debug.LogWarning("QuestDisplay.Instance отсутствует в сцене. Обновление UI квестов пропущено.");
            
            // Если мы в сцене игры, а не в меню, то это может быть проблемой
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Menu")
            {
                StartCoroutine(TryUpdateDisplayNextFrame());
            }
        }
    }
    
    private System.Collections.IEnumerator TryUpdateDisplayNextFrame()
    {
        // Ждем следующий кадр и пытаемся снова обновить UI
        yield return null;
        
        if (QuestDisplay.Instance != null)
        {
            Debug.Log("QuestDisplay.Instance найден в следующем кадре. Обновляем UI квестов.");
            QuestDisplay.Instance.UpdateQuestsDisplay(activeQuests, completedQuests);
        }
    }

    // Метод для получения активного квеста по ID
    public Quest GetActiveQuest(string questID)
    {
        Quest quest = activeQuests.Find(q => q.questID == questID);
        if (quest == null)
        {
            Debug.LogWarning($"QuestManager: Квест с ID {questID} не найден среди активных квестов");
        }
        return quest;
    }

    // Метод для обновления прогресса целей квеста
    public bool UpdateQuestProgress(string questID, QuestObjective.ObjectiveType objectiveType, int amount)
    {
        // Специальная обработка для квеста quest07
        if (questID == "quest07")
        {
            Debug.Log($"UpdateQuestProgress: Специальная обработка для квеста quest07");
            Quest quest07 = GetActiveQuest(questID);
            
            if (quest07 != null)
            {
                // Сразу отмечаем квест как готовый к завершению
                quest07.status = QuestStatus.ReadyToComplete;
                
                // Отмечаем все цели типа Talk как выполненные
                if (quest07.objectives != null)
                {
                    foreach (var objective in quest07.objectives)
                    {
                        if (objective != null && objective.type == QuestObjective.ObjectiveType.Talk)
                        {
                            objective.currentAmount = objective.requiredAmount;
                            objective.isCompleted = true;
                        }
                    }
                }
                
                Debug.Log($"UpdateQuestProgress: Квест quest07 отмечен как готовый к завершению");
                return true;
            }
            else
            {
                Debug.LogWarning($"UpdateQuestProgress: Квест quest07 не найден среди активных квестов");
                return false;
            }
        }
        
        // Обработка для остальных квестов
        Quest quest = GetActiveQuest(questID);
        if (quest != null)
        {
            bool objectiveUpdated = false;
            int updatedObjectives = 0;
            
            // Логируем начало обновления
            Debug.Log($"UpdateQuestProgress: Обновляем прогресс квеста '{quest.questName}' ({questID}) для целей типа {objectiveType}, кол-во: +{amount}");
            
            // Проверяем все цели квеста
            if (quest.objectives == null || quest.objectives.Count == 0)
            {
                Debug.LogWarning($"UpdateQuestProgress: квест '{quest.questName}' не имеет целей");
                return false;
            }
            
            foreach (var objective in quest.objectives)
            {
                // Проверяем цель на null
                if (objective == null)
                {
                    Debug.LogWarning($"UpdateQuestProgress: обнаружена null-цель в квесте '{quest.questName}'");
                    continue;
                }
                
                // Обновляем только незавершенные цели нужного типа
                if (objective.type == objectiveType && !objective.isCompleted)
                {
                    Debug.Log($"UpdateQuestProgress: найдена подходящая цель '{objective.objectiveDescription}' в квесте '{quest.questName}'");
                    
                    // Обновляем прогресс
                    objective.UpdateProgress(amount, quest);
                    updatedObjectives++;
                    objectiveUpdated = true;
                    
                    Debug.Log($"UpdateQuestProgress: обновлен прогресс цели '{objective.objectiveDescription}' - {objective.currentAmount}/{objective.requiredAmount}");
                    
                    // Если цель типа Talk и она завершена, обрабатываем особым образом
                    if (objectiveType == QuestObjective.ObjectiveType.Talk && objective.isCompleted)
                    {
                        Debug.Log($"UpdateQuestProgress: цель типа Talk выполнена для квеста '{quest.questName}'");
                        
                        // Если квест имеет флаг автозавершения, устанавливаем статус готовности
                        if (quest.canAutoEnded)
                        {
                            Debug.Log($"UpdateQuestProgress: квест '{quest.questName}' имеет флаг автозавершения, устанавливаем статус ReadyToComplete");
                            quest.status = QuestStatus.ReadyToComplete;
                            
                            // НЕ запускаем автоматическое завершение здесь - это будет сделано в OnTalkEvent
                            // Это предотвратит преждевременное завершение квеста и запуск следующего
                        }
                    }
                }
            }
            
            // Логируем итоги обновления
            Debug.Log($"UpdateQuestProgress: обновлено {updatedObjectives} целей в квесте '{quest.questName}'");
            
            if (!objectiveUpdated)
            {
                Debug.LogWarning($"UpdateQuestProgress: не найдена незавершенная цель типа {objectiveType} для квеста '{quest.questName}'");
                return false;
            }
            
            // Проверяем выполнение всех целей квеста
            if (quest.IsCompleted())
            {
                Debug.Log($"UpdateQuestProgress: все цели квеста '{quest.questName}' выполнены");
                
                // Для квестов типа Talk только устанавливаем статус ReadyToComplete
                // НО не запускаем автоматическое завершение - это предотвратит преждевременное завершение
                if (objectiveType == QuestObjective.ObjectiveType.Talk)
                {
                    quest.status = QuestStatus.ReadyToComplete;
                    Debug.Log($"UpdateQuestProgress: квест '{quest.questName}' типа Talk готов к сдаче (только установлен статус)");
                }
                // Для других типов квестов обрабатываем как обычно
                else if (quest.canAutoEnded)
                {
                    quest.status = QuestStatus.ReadyToComplete;
                    Debug.Log($"UpdateQuestProgress: квест '{quest.questName}' другого типа готов к завершению");
                    
                    // Проверяем наличие следующего квеста
                    if (quest.questData != null && quest.questData.acceptedQuestData != null)
                    {
                        Debug.Log($"UpdateQuestProgress: найден следующий квест в цепочке для '{quest.questName}', используем AdvanceQuestChain");
                        AdvanceQuestChain(quest.questID);
                    }
                    else
                    {
                        // Если следующего квеста нет, просто завершаем текущий
                        CompleteQuestAutomatically(quest);
                    }
                }
                else
                {
                    quest.status = QuestStatus.ReadyToComplete;
                    Debug.Log($"UpdateQuestProgress: квест '{quest.questName}' готов к сдаче (требуется обращение к NPC)");
                }
            }
            
            return objectiveUpdated;
        }
        else
        {
            Debug.LogWarning($"UpdateQuestProgress: квест с ID {questID} не найден среди активных квестов");
            return false;
        }
    }

    // Завершение квеста и выдача награды
    public void CompleteQuest(Quest quest)
    {
        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        Debug.Log($"Квест '{quest.questName}' выполнен! Награда: {quest.reward}");
        
        // Уведомляем подписчиков о завершении квеста
        OnQuestCompleted?.Invoke(quest);
        
        UpdateAllQuestsDisplay(); // Обновляем UI после изменения списков
    }
    // Метод, который будет вызываться каждый раз, когда игрок успешно разговаривает с NPC
    public void OnTalkEvent()
    {
        //talkEventCount++;
        //Debug.Log($"OnTalkEvent: Событие разговора #{talkEventCount}, текущий шаг разговора: {currentTalkStep}");

        // Проверка активных квестов с целями Talk
        List<Quest> talkQuests = activeQuests.Where(q => q.HasObjectiveOfType(QuestObjective.ObjectiveType.Talk)).ToList();
        
        if (talkQuests.Count > 0)
        {
            Debug.Log($"OnTalkEvent: Найдено {talkQuests.Count} активных квестов с целями Talk");
            
            // Специальная обработка для квеста quest07
            Quest quest07 = talkQuests.FirstOrDefault(q => q.questID == "quest07");
            if (quest07 != null)
            {
                Debug.Log("OnTalkEvent: Найден специальный квест quest07, устанавливаем его в статус ReadyToComplete");
                // Обновляем статус квеста на готовый к завершению
                quest07.status = QuestStatus.ReadyToComplete;
                
                // Устанавливаем все цели как выполненные
                if (quest07.objectives != null)
                {
                    foreach (var objective in quest07.objectives)
                    {
                        if (objective != null && objective.type == QuestObjective.ObjectiveType.Talk)
                        {
                            objective.currentAmount = objective.requiredAmount;
                            objective.isCompleted = true;
                        }
                    }
                }
                
                // Если квест может быть завершен автоматически, завершаем его
                if (quest07.canAutoEnded)
                {
                    Debug.Log("OnTalkEvent: Автоматически завершаем квест quest07");
                    CompleteTalkQuest(quest07);
                    return; // Выходим, так как квест уже обработан
                }
            }
            
            // Проверяем каждый квест Talk на готовность к завершению
            // Важно: обрабатываем каждый квест по отдельности и ТОЛЬКО если он действительно готов к завершению
            foreach (var quest in talkQuests.ToList())
            {
                if (quest == null)
                {
                    Debug.LogWarning("OnTalkEvent: обнаружен null-квест в списке talkQuests");
                    continue;
                }
                
                // Пропускаем quest07, так как он уже обработан выше
                if (quest.questID == "quest07")
                {
                    continue;
                }
                
                Debug.Log($"OnTalkEvent: Проверяем квест '{quest.questName}', статус: {quest.status}, все цели выполнены: {quest.IsCompleted()}");
                
                // Проверяем, готов ли квест к завершению
                if (quest.status == QuestStatus.ReadyToComplete && quest.canAutoEnded)
                {
                    Debug.Log($"OnTalkEvent: Квест '{quest.questName}' готов к автоматическому завершению");
                    
                    // Проверяем, есть ли у квеста следующий квест в цепочке
                    if (quest.questData != null && quest.questData.acceptedQuestData != null)
                    {
                        // Проверяем, не существует ли уже квест с таким ID среди активных квестов
                        string nextQuestID = quest.questData.acceptedQuestData.questID;
                        bool nextQuestAlreadyActive = activeQuests.Any(q => q.questID == nextQuestID);
                        
                        if (!nextQuestAlreadyActive)
                        {
                            Debug.Log($"OnTalkEvent: Квест '{quest.questName}' имеет следующий квест в цепочке ({nextQuestID}), используем AdvanceQuestChain");
                            AdvanceQuestChain(quest.questID);
                        }
                        else
                        {
                            Debug.LogWarning($"OnTalkEvent: Следующий квест ({nextQuestID}) уже активен. Завершаем текущий квест без запуска следующего");
                            CompleteQuestAutomatically(quest);
                        }
                    }
                    else
                    {
                        // Если следующего квеста нет, просто завершаем текущий
                        Debug.Log($"OnTalkEvent: Квест '{quest.questName}' не имеет следующего квеста, просто завершаем его");
                        CompleteQuestAutomatically(quest);
                    }
                }
                else
                {
                    Debug.Log($"OnTalkEvent: Квест '{quest.questName}' не готов к автоматическому завершению (статус: {quest.status}, canAutoEnded: {quest.canAutoEnded})");
                }
            }
        }
        else
        {
            Debug.Log("OnTalkEvent: Не найдено активных квестов с целями Talk");
        }

        //currentTalkStep++;
        //Debug.Log($"OnTalkEvent: Установлен новый шаг разговора: {currentTalkStep}");
    }

    // Метод завершения квеста типа Talk – теперь сразу завершаем квест, выдаём награду и показываем локализованное сообщение
    // Метод для получения кода языка на основе системного языка
    private string GetSystemLanguageCode()
    {
        return LocalizationHelper.GetSystemLanguageCode();
    }

    public void CompleteTalkQuest()
    {
        Debug.Log("CompleteTalkQuest: Ищем активные квесты с целями Talk для завершения");
        
        // Специальная обработка для квеста quest07
        Quest quest07 = activeQuests.FirstOrDefault(q => q.questID == "quest07");
        if (quest07 != null)
        {
            Debug.Log("CompleteTalkQuest: Найден специальный квест quest07, завершаем его принудительно");
            CompleteTalkQuest(quest07);
            return;
        }
        
        List<Quest> talkQuests = activeQuests.Where(q => q.HasObjectiveOfType(QuestObjective.ObjectiveType.Talk) && 
                                                (q.status == QuestStatus.ReadyToComplete || q.IsCompleted())).ToList();
        
        if (talkQuests.Count == 0)
        {
            Debug.LogWarning("CompleteTalkQuest: Не найдено активных квестов с целями Talk, готовых к завершению");
            return;
        }
        
        Debug.Log($"CompleteTalkQuest: Найдено {talkQuests.Count} квестов Talk, готовых к завершению");
        
        foreach (Quest talkQuest in talkQuests.ToList())
        {
            CompleteTalkQuest(talkQuest);
        }
    }
    
    // Перегрузка метода для завершения конкретного Talk-квеста
    public void CompleteTalkQuest(Quest talkQuest)
    {
        if (talkQuest == null)
        {
            Debug.LogWarning("CompleteTalkQuest: передан null-квест");
            return;
        }
        
        Debug.Log($"CompleteTalkQuest: Обрабатываем квест '{talkQuest.questName}' (ID: {talkQuest.questID})");
        
        // Специальная обработка для квеста quest07
        if (talkQuest.questID == "quest07")
        {
            Debug.Log("CompleteTalkQuest: Принудительно завершаем квест quest07");
            
            // Убедимся, что квест отмечен как готовый к завершению
            talkQuest.status = QuestStatus.ReadyToComplete;
            
            // Убедимся, что все цели выполнены
            if (talkQuest.objectives != null)
            {
                foreach (var objective in talkQuest.objectives)
                {
                    if (objective != null && objective.type == QuestObjective.ObjectiveType.Talk)
                    {
                        objective.currentAmount = objective.requiredAmount;
                        objective.isCompleted = true;
                    }
                }
            }
            
            // Проверяем, есть ли следующий квест
            if (talkQuest.questData != null && talkQuest.questData.acceptedQuestData != null)
            {
                string nextQuestID = talkQuest.questData.acceptedQuestData.questID;
                bool nextQuestAlreadyActive = activeQuests.Any(q => q.questID == nextQuestID);
                bool nextQuestAlreadyCompleted = completedQuests.Any(q => q.questID == nextQuestID);
                
                if (!nextQuestAlreadyActive && !nextQuestAlreadyCompleted)
                {
                    // Отображаем сообщение о завершении, если есть
                    ShowCompletionMessage(talkQuest);
                    
                    // Завершаем текущий квест и запускаем следующий
                    AdvanceQuestChain(talkQuest.questID);
                }
                else
                {
                    // Если следующий квест уже существует, просто завершаем текущий
                    CompleteQuest(talkQuest);
                    ShowCompletionMessage(talkQuest);
                }
            }
            else
            {
                // Если нет следующего квеста, просто завершаем текущий
                CompleteQuest(talkQuest);
                ShowCompletionMessage(talkQuest);
            }
            
            return;
        }
        
        // Проверяем, что квест действительно имеет тип Talk
        if (!talkQuest.HasObjectiveOfType(QuestObjective.ObjectiveType.Talk))
        {
            Debug.LogWarning($"CompleteTalkQuest: Квест '{talkQuest.questName}' не имеет целей типа Talk");
            return;
        }
        
        // Проверяем, готов ли квест к завершению
        if (talkQuest.status == QuestStatus.ReadyToComplete || talkQuest.IsCompleted())
        {
            Debug.Log($"CompleteTalkQuest: Квест '{talkQuest.questName}' готов к завершению");
            
            // Проверяем, есть ли у квеста следующий квест в цепочке
            if (talkQuest.questData != null && talkQuest.questData.acceptedQuestData != null)
            {
                // Проверяем, не существует ли уже квест с таким ID среди активных или завершенных квестов
                string nextQuestID = talkQuest.questData.acceptedQuestData.questID;
                bool nextQuestAlreadyActive = activeQuests.Any(q => q.questID == nextQuestID);
                bool nextQuestAlreadyCompleted = completedQuests.Any(q => q.questID == nextQuestID);
                
                if (!nextQuestAlreadyActive && !nextQuestAlreadyCompleted)
                {
                    Debug.Log($"CompleteTalkQuest: Квест '{talkQuest.questName}' имеет следующий квест в цепочке ({nextQuestID}), используем AdvanceQuestChain");
                    
                    // Отображаем сообщение о завершении
                    ShowCompletionMessage(talkQuest);
                    
                    // Запускаем цепочку квестов
                    AdvanceQuestChain(talkQuest.questID);
                    Debug.Log($"CompleteTalkQuest: Запущена цепочка квестов от '{talkQuest.questName}'");
                }
                else
                {
                    Debug.LogWarning($"CompleteTalkQuest: Следующий квест ({nextQuestID}) уже активен или завершен. Завершаем текущий квест без запуска следующего");
                    
                    // Завершаем квест (выдаем награду, переносим его в список завершённых)
                    CompleteQuest(talkQuest);
                    
                    // Отображаем сообщение о завершении
                    ShowCompletionMessage(talkQuest);
                }
            }
            else
            {
                // Завершаем квест (выдаем награду, переносим его в список завершённых)
                CompleteQuest(talkQuest);
                
                // Отображаем сообщение о завершении
                ShowCompletionMessage(talkQuest);
                
                Debug.Log($"CompleteTalkQuest: Квест '{talkQuest.questName}' завершён после завершения разговоров");
            }
        }
        else
        {
            Debug.Log($"CompleteTalkQuest: Квест '{talkQuest.questName}' не готов к завершению (статус: {talkQuest.status})");
        }
    }
    
    // Вспомогательный метод для отображения сообщения о завершении квеста
    private void ShowCompletionMessage(Quest quest)
    {
        if (QuestDisplay.Instance == null)
        {
            Debug.LogWarning("ShowCompletionMessage: QuestDisplay.Instance отсутствует, не удалось отобразить сообщение");
            return;
        }
        
        // Получаем локализованное сообщение о сдаче квеста
        string completionMessage = "Quest completed!";
        
        if (quest.questData != null)
        {
            string currentLang = GetSystemLanguageCode();
            completionMessage = quest.questData.GetCompletionMessage(currentLang);
            if (string.IsNullOrEmpty(completionMessage))
            {
                completionMessage = "Quest completed!";
            }
        }
        
        QuestDisplay.Instance.SetQuestText(completionMessage);
        Debug.Log($"ShowCompletionMessage: Отображено сообщение о завершении квеста: {completionMessage}");
    }

    // Метод для запуска следующего квеста в цепочке
    private void StartNextQuest(QuestData nextQuestData, QuestLine questLine)
    {
        try
        {
            if (nextQuestData == null)
            {
                Debug.LogError("StartNextQuest: nextQuestData равен null");
                return;
            }
            
            Debug.Log($"StartNextQuest: Создаем новый квест на основе {nextQuestData.questName} (ID: {nextQuestData.questID})");
            
            // Проверяем, существует ли уже квест с таким ID в активных квестах
            bool alreadyActive = activeQuests.Exists(q => q.questID == nextQuestData.questID);
            if (alreadyActive)
            {
                Debug.LogWarning($"StartNextQuest: Квест с ID {nextQuestData.questID} уже активен. Пропускаем создание.");
                return;
            }
            
            // Проверяем, существует ли уже квест с таким ID в завершенных квестах
            bool alreadyCompleted = completedQuests.Exists(q => q.questID == nextQuestData.questID);
            if (alreadyCompleted)
            {
                Debug.LogWarning($"StartNextQuest: Квест с ID {nextQuestData.questID} уже завершен. Пропускаем создание.");
                return;
            }
            
            // Создаем новый квест на основе данных из QuestData
            Quest nextQuest = new Quest
            {
                questID = nextQuestData.questID,
                questName = nextQuestData.questName,
                description = nextQuestData.description,
                questData = nextQuestData,
                questLine = questLine, // Используем переданный тип квеста
                reward = 100, // Базовое значение награды, можно настроить
                status = QuestStatus.NotStarted,
                canAutoEnded = true // По умолчанию делаем квесты в цепочке автозавершаемыми
            };
            
            // Создаем цели квеста на основе типа квеста
            if (nextQuest.objectives == null)
            {
                nextQuest.objectives = new List<QuestObjective>();
                
                // В зависимости от предполагаемого типа квеста, добавляем соответствующую цель
                // Это можно усовершенствовать, добавив явное указание типа и параметров целей в QuestData
                if (nextQuestData.questID.Contains("talk") || nextQuestData.questName.ToLower().Contains("talk"))
                {
                    nextQuest.type = QuestObjective.ObjectiveType.Talk;
                    nextQuest.objectives.Add(new QuestObjective
                    {
                        objectiveDescription = "Поговорить с NPC",
                        type = QuestObjective.ObjectiveType.Talk,
                        requiredAmount = 1,
                        currentAmount = 0
                    });
                }
                else if (nextQuestData.questID.Contains("kill") || nextQuestData.questName.ToLower().Contains("kill"))
                {
                    nextQuest.type = QuestObjective.ObjectiveType.Kill;
                    nextQuest.objectives.Add(new QuestObjective
                    {
                        objectiveDescription = "Убить цель",
                        type = QuestObjective.ObjectiveType.Kill,
                        requiredAmount = 1,
                        currentAmount = 0
                    });
                }
                else if (nextQuestData.questID.Contains("collect") || nextQuestData.questName.ToLower().Contains("collect"))
                {
                    nextQuest.type = QuestObjective.ObjectiveType.Collect;
                    nextQuest.objectives.Add(new QuestObjective
                    {
                        objectiveDescription = "Собрать предметы",
                        type = QuestObjective.ObjectiveType.Collect,
                        requiredAmount = 1,
                        currentAmount = 0
                    });
                }
                else if (nextQuestData.questID.Contains("escort") || nextQuestData.questName.ToLower().Contains("escort"))
                {
                    nextQuest.type = QuestObjective.ObjectiveType.Escort;
                    nextQuest.objectives.Add(new QuestObjective
                    {
                        objectiveDescription = "Сопроводить NPC",
                        type = QuestObjective.ObjectiveType.Escort,
                        requiredAmount = 1,
                        currentAmount = 0
                    });
                }
                else
                {
                    // По умолчанию добавляем цель типа Talk
                    nextQuest.type = QuestObjective.ObjectiveType.Talk;
                    nextQuest.objectives.Add(new QuestObjective
                    {
                        objectiveDescription = "Выполнить задание",
                        type = QuestObjective.ObjectiveType.Talk,
                        requiredAmount = 1,
                        currentAmount = 0
                    });
                }
            }

            // Запускаем квест через метод StartQuest, который содержит дополнительные проверки
            StartQuest(nextQuest);
            Debug.Log($"StartNextQuest: Запущен следующий квест в цепочке: {nextQuest.questName} (ID: {nextQuest.questID})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка при запуске следующего квеста: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void CompleteQuestAutomaticallyAndGiveNext()
    {
        try
        {
            if (activeQuests.Count > 0)
            {
                Quest currentQuest = activeQuests[0];
                if (currentQuest == null)
                {
                    Debug.LogWarning("CompleteQuestAutomaticallyAndGiveNext: currentQuest равен null");
                    return;
                }
                
                // Сначала сохраняем информацию о следующем квесте
                QuestData nextQuestData = null;
                QuestLine currentQuestLine = currentQuest.questLine;
                
                if (currentQuest.questData != null && currentQuest.questData.acceptedQuestData != null)
                {
                    nextQuestData = currentQuest.questData.acceptedQuestData;
                }
                
                // Завершаем текущий квест
                CompleteQuestAutomatically(currentQuest);
                
                // Если есть следующий квест, создаем и запускаем его
                if (nextQuestData != null)
                {
                    StartNextQuest(nextQuestData, currentQuestLine);
                }
            }
            else
            {
                Debug.LogWarning("CompleteQuestAutomaticallyAndGiveNext: activeQuests пуст");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка в CompleteQuestAutomaticallyAndGiveNext: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // Метод для автоматического завершения квеста
    public void CompleteQuestAutomatically(Quest quest)
    {
        if (quest == null)
        {
            Debug.LogError("CompleteQuestAutomatically: quest равен null");
            return;
        }
        
        try
        {
            Debug.Log($"CompleteQuestAutomatically: Завершаем квест '{quest.questName}' (ID: {quest.questID})");
            
            // Проверяем, что квест все еще находится в списке активных (не был завершен другим процессом)
            if (!activeQuests.Contains(quest))
            {
                Debug.LogWarning($"CompleteQuestAutomatically: Квест '{quest.questName}' не найден в списке активных. Возможно, он уже был завершен.");
                return;
            }
            
            // Проверяем, что все цели квеста выполнены
            if (!quest.IsCompleted() && quest.status != QuestStatus.ReadyToComplete)
            {
                Debug.LogWarning($"CompleteQuestAutomatically: Квест '{quest.questName}' не готов к завершению (статус: {quest.status})");
                return;
            }
            
            // Меняем статус квеста на завершённый
            quest.status = QuestStatus.Completed;

            // Перемещаем квест из активных в завершённые
            activeQuests.Remove(quest);
            completedQuests.Add(quest);

            // Логируем завершение квеста
            Debug.Log($"Квест '{quest.questName}' завершён автоматически! Награда: {quest.reward}");
            
            // Уведомляем подписчиков о завершении квеста
            OnQuestCompleted?.Invoke(quest);
            
            // Проверяем наличие QuestDisplay перед отображением диалога
            if (QuestDisplay.Instance == null)
            {
                Debug.LogWarning($"QuestDisplay.Instance отсутствует. Диалог завершения квеста '{quest.questName}' не будет отображен.");
            }
            else
            {
                // Отображаем диалоговую реплику при завершении квеста (если она есть)
                if (quest.questData != null && !string.IsNullOrEmpty(quest.questData.completionDialogue))
                {
                    // Получаем системный язык
                    string currentLang = GetSystemLanguageCode();
                    
                    // Получаем локализованное сообщение
                    string localizedDialogue = quest.questData.GetLocalizedCompletionDialogue(currentLang);
                    
                    if (!string.IsNullOrEmpty(localizedDialogue))
                    {
                        QuestDisplay.Instance.SetQuestText(localizedDialogue);
                        Debug.Log($"Отображен диалог завершения квеста: {localizedDialogue}");
                    }
                }
            }

            // Обновляем отображение всех квестов в UI
            UpdateAllQuestsDisplay();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка в CompleteQuestAutomatically: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void AdvanceQuestChain(string currentQuestID)
    {
        try
        {
            Debug.Log($"AdvanceQuestChain: Начинаем обработку цепочки квестов для квеста с ID {currentQuestID}");
            
            Quest currentQuest = GetActiveQuest(currentQuestID);
            if (currentQuest == null)
            {
                Debug.LogWarning($"AdvanceQuestChain: Не найден активный квест с ID {currentQuestID}");
                return;
            }
            
            // Проверяем, готов ли квест к завершению
            if (currentQuest.status != QuestStatus.ReadyToComplete && !currentQuest.IsCompleted())
            {
                Debug.LogWarning($"AdvanceQuestChain: Квест '{currentQuest.questName}' не готов к завершению (статус: {currentQuest.status})");
                return;
            }
            
            // Проверяем наличие данных для следующего квеста
            if (currentQuest.questData == null)
            {
                Debug.LogWarning($"AdvanceQuestChain: У квеста '{currentQuest.questName}' отсутствует QuestData");
                return;
            }
            
            if (currentQuest.questData.acceptedQuestData == null)
            {
                Debug.LogWarning($"AdvanceQuestChain: У квеста '{currentQuest.questName}' не указан следующий квест (acceptedQuestData)");
                return;
            }
            
            QuestData nextQuestData = currentQuest.questData.acceptedQuestData;
            QuestLine currentQuestLine = currentQuest.questLine;
            
            // Проверяем, существует ли квест с таким ID уже в активных квестах
            bool alreadyActive = activeQuests.Exists(q => q.questID == nextQuestData.questID);
            
            // Проверяем, существует ли квест с таким ID уже в завершенных квестах
            bool alreadyCompleted = completedQuests.Exists(q => q.questID == nextQuestData.questID);
            
            if (alreadyActive)
            {
                Debug.LogWarning($"AdvanceQuestChain: Квест '{nextQuestData.questName}' уже активен. Пропускаем добавление.");
                // Просто завершаем текущий квест, не запуская следующий
                CompleteQuestAutomatically(currentQuest);
                return;
            }
            
            if (alreadyCompleted)
            {
                Debug.LogWarning($"AdvanceQuestChain: Квест '{nextQuestData.questName}' уже завершен. Пропускаем добавление.");
                // Просто завершаем текущий квест, не запуская следующий
                CompleteQuestAutomatically(currentQuest);
                return;
            }
            
            Debug.Log($"AdvanceQuestChain: Квест '{currentQuest.questName}' имеет следующий квест '{nextQuestData.questName}'");
            
            // Завершаем текущий квест
            CompleteQuestAutomatically(currentQuest);
            Debug.Log($"AdvanceQuestChain: Квест '{currentQuest.questName}' завершен");
            
            // Проверяем, что текущий квест действительно завершен
            bool currentQuestCompleted = completedQuests.Exists(q => q.questID == currentQuestID);
            
            // Запускаем следующий квест ТОЛЬКО если текущий был успешно завершен
            if (currentQuestCompleted)
            {
                // Повторно проверяем, не добавлен ли уже следующий квест
                bool nextQuestActive = activeQuests.Exists(q => q.questID == nextQuestData.questID);
                if (!nextQuestActive)
                {
                    StartNextQuest(nextQuestData, currentQuestLine);
                    Debug.Log($"AdvanceQuestChain: Запущен следующий квест в цепочке: '{nextQuestData.questName}'");
                }
                else
                {
                    Debug.LogWarning($"AdvanceQuestChain: Следующий квест '{nextQuestData.questName}' уже был активирован во время завершения текущего");
                }
            }
            else
            {
                Debug.LogWarning($"AdvanceQuestChain: Текущий квест '{currentQuest.questName}' не был завершен. Следующий квест не запущен.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка в AdvanceQuestChain: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // Метод для автоматической обработки цепочки квестов
    public void AdvanceQuestChainAutomatically(Quest currentQuest)
    {
        if (currentQuest == null)
        {
            Debug.LogWarning("AdvanceQuestChainAutomatically: currentQuest равен null");
            return;
        }
        
        Debug.Log($"AdvanceQuestChainAutomatically: Начинаем обработку цепочки для квеста '{currentQuest.questName}'");
        
        // Проверяем, есть ли у квеста следующий квест в цепочке
        if (currentQuest.questData != null && currentQuest.questData.acceptedQuestData != null)
        {
            Debug.Log($"AdvanceQuestChainAutomatically: Найден следующий квест для '{currentQuest.questName}'");
            AdvanceQuestChain(currentQuest.questID);
        }
        else
        {
            Debug.Log($"AdvanceQuestChainAutomatically: Для квеста '{currentQuest.questName}' не указан следующий квест, просто завершаем");
            CompleteQuestAutomatically(currentQuest);
        }
    }
}
