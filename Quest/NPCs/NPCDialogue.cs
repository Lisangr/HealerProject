using UnityEngine;
using System;

public class NPCDialogue : MonoBehaviour
{
    [Header("Порядок диалогов")]
    [Tooltip("Порядковый номер разговора (нумерация с 1). NPC с большим числом должен говорить позже.")]
    public int conversationOrder = 1;

    [Header("Локализованные диалоги")]
    public QuestDialogue[] dialogues;

    [Header("Локализация завершающих сообщений")]
    public QuestCompletionLocalization[] completionLocalizations;

    [Header("Настройки квеста")]
    [Tooltip("Список ID квестов, для которых этот NPC выступает в разговоре")]
    public string[] associatedQuestIDs;

    // Флаг, показывает, что с этим NPC уже говорили
    private bool hasTalked = false;
    
    // Задержка между нажатиями, чтобы избежать множественных взаимодействий
    private float interactionCooldown = 0.5f; 
    private float lastInteractionTime = -1f;
    
    public string GetDialogueText(string language)
    {
        QuestDialogue dialogue = Array.Find(dialogues, d => d.languageCode.ToUpper() == language.ToUpper());
        if (dialogue == null && dialogues.Length > 0)
        {
            dialogue = Array.Find(dialogues, d => d.languageCode.ToUpper() == "EN") ?? dialogues[0];
        }
        return dialogue != null && dialogue.phrases.Length > 0 ? dialogue.phrases[0] : "";
    }

    public string GetCompletionText(string language)
    {
        QuestCompletionLocalization localization = Array.Find(completionLocalizations,
            loc => loc.languageCode.ToUpper() == language.ToUpper());
        if (localization == null && completionLocalizations.Length > 0)
        {
            localization = Array.Find(completionLocalizations,
                loc => loc.languageCode.ToUpper() == "EN") ?? completionLocalizations[0];
        }
        return localization != null ? localization.completionMessage : "";
    }
    public void Interact()
    {
        // Проверка кулдауна между взаимодействиями
        if (Time.time - lastInteractionTime < interactionCooldown)
        {
            return;
        }
        lastInteractionTime = Time.time;

        string currentLang = LocalizationHelper.GetSystemLanguageCode();

        // Проверяем наличие QuestManager
        if (QuestManager.Instance == null)
        {
            Debug.LogError("NPCDialogue: QuestManager.Instance равен null");
            return;
        }
        
        // Проверяем наличие QuestDisplay
        if (QuestDisplay.Instance == null)
        {
            Debug.LogError("NPCDialogue: QuestDisplay.Instance равен null");
            return;
        }

        Debug.Log($"NPCDialogue: Игрок взаимодействует с NPC #{conversationOrder}");
        
        // Проверяем, подходит ли этот NPC для текущего шага диалога
        /*if (QuestManager.Instance.currentTalkStep == conversationOrder)
        {
            if (!hasTalked)
            {
                hasTalked = true;
                Debug.Log($"NPCDialogue: Первое взаимодействие с NPC #{conversationOrder}");
                
                // Получаем текст диалога и проверяем его наличие
                string dialogueText = GetDialogueText(currentLang);
                if (string.IsNullOrEmpty(dialogueText))
                {
                    Debug.LogWarning($"NPCDialogue: Пустой текст диалога для NPC #{conversationOrder}");
                    dialogueText = "...";
                }
                
                // Показываем диалог
                QuestDisplay.Instance.SetQuestText(dialogueText);
                Debug.Log($"NPCDialogue: Показан диалог для NPC #{conversationOrder}");

                // Обрабатываем ассоциированные квесты
                bool anyQuestUpdated = ProcessAssociatedQuests();
                
                // Вызываем событие разговора независимо от обновления квестов
                QuestManager.Instance.OnTalkEvent();
                Debug.Log($"NPCDialogue: Вызвано событие Talk, обновлены квесты: {(anyQuestUpdated ? "да" : "нет")}");
            }
            else
            {
                // Если уже говорили, выводим сообщение о завершении разговора
                string completionText = GetCompletionText(currentLang);
                
                // Проверка на пустой текст
                if (string.IsNullOrEmpty(completionText))
                {
                    Debug.LogWarning($"NPCDialogue: Пустой текст завершения для NPC #{conversationOrder}");
                    completionText = "...";
                }
                
                QuestDisplay.Instance.SetQuestText(completionText);
                Debug.Log($"NPCDialogue: Показано завершающее сообщение для NPC #{conversationOrder}");
                
                // Проверяем, есть ли квесты, которые можно завершить после повторного разговора
                CompleteTalkQuestsIfReady();
            }
        }
        else
        {
            // NPC не подходит для текущего шага - показываем стандартное сообщение
            Debug.Log($"NPCDialogue: Порядок разговора NPC #{conversationOrder} не совпадает с текущим шагом {QuestManager.Instance.currentTalkStep}");
            
            // Можно добавить какое-то стандартное сообщение, когда игрок разговаривает с NPC не по порядку
            QuestDisplay.Instance.SetQuestText("Этот персонаж сейчас не готов к разговору.");
        }*/
    }
    
    // Метод для обработки ассоциированных квестов
    private bool ProcessAssociatedQuests()
    {
        // Проверяем наличие ассоциированных квестов
        if (associatedQuestIDs == null || associatedQuestIDs.Length == 0)
        {
            Debug.LogWarning($"NPCDialogue: Для NPC #{conversationOrder} не указаны ассоциированные квесты");
            return false;
        }
        
        // Логируем все ассоциированные квесты
        Debug.Log($"NPCDialogue: NPC #{conversationOrder} имеет {associatedQuestIDs.Length} ассоциированных квестов");
        foreach (string id in associatedQuestIDs)
        {
            Debug.Log($"NPCDialogue: Ассоциированный квест: {id}");
        }
        
        bool anyQuestUpdated = false;
        
        // Обрабатываем каждый квест
        foreach (string questID in associatedQuestIDs)
        {
            Debug.Log($"NPCDialogue: Обрабатываем квест {questID}");
            
            // Получаем квест для проверки
            Quest quest = QuestManager.Instance.GetActiveQuest(questID);
            if (quest == null)
            {
                Debug.LogWarning($"NPCDialogue: Квест {questID} не найден среди активных квестов");
                continue;
            }
            
            // Проверяем статус квеста
            Debug.Log($"NPCDialogue: Текущий статус квеста {questID}: {quest.status}");
            
            // Специальная обработка для квеста quest07
            if (questID == "quest07")
            {
                Debug.Log($"NPCDialogue: Обнаружен особый квест quest07, устанавливаем его сразу в статус ReadyToComplete");
                quest.status = QuestStatus.ReadyToComplete;
                
                // Обновляем цели квеста, чтобы они считались выполненными
                if (quest.objectives != null)
                {
                    foreach (var objective in quest.objectives)
                    {
                        if (objective != null && objective.type == QuestObjective.ObjectiveType.Talk)
                        {
                            objective.currentAmount = objective.requiredAmount;
                            objective.isCompleted = true;
                        }
                    }
                }
                
                // Можем сразу завершить квест, если он имеет флаг автозавершения
                if (quest.canAutoEnded)
                {
                    Debug.Log($"NPCDialogue: Автоматически завершаем квест {questID} (quest07)");
                    QuestManager.Instance.CompleteTalkQuest(quest);
                }
                
                anyQuestUpdated = true;
                continue; // Переходим к следующему квесту
            }
            
            // Проверяем, есть ли у квеста цели типа Talk
            if (!quest.HasObjectiveOfType(QuestObjective.ObjectiveType.Talk))
            {
                Debug.LogWarning($"NPCDialogue: Квест {questID} не имеет целей типа Talk");
                continue;
            }
            
            // Обновляем прогресс для квеста
            bool updated = QuestManager.Instance.UpdateQuestProgress(questID, QuestObjective.ObjectiveType.Talk, 1);
            
            if (updated)
            {
                anyQuestUpdated = true;
                Debug.Log($"NPCDialogue: Успешно обновлен прогресс квеста {questID}");
                
                // Получаем квест снова после обновления
                quest = QuestManager.Instance.GetActiveQuest(questID);
                
                // Проверяем, готов ли квест к завершению
                if (quest != null && quest.IsCompleted())
                {
                    Debug.Log($"NPCDialogue: Квест {questID} готов к завершению");
                    
                    // Если квест можно завершить автоматически, завершаем его сразу
                    if (quest.canAutoEnded)
                    {
                        Debug.Log($"NPCDialogue: Автоматически завершаем квест {questID}");
                        QuestManager.Instance.CompleteTalkQuest(quest);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"NPCDialogue: Не удалось обновить прогресс квеста {questID}");
            }
        }
        
        return anyQuestUpdated;
    }
    
    // Метод для завершения готовых квестов при повторном разговоре
    private void CompleteTalkQuestsIfReady()
    {
        if (associatedQuestIDs == null || associatedQuestIDs.Length == 0)
        {
            return;
        }
        
        foreach (string questID in associatedQuestIDs)
        {
            Quest quest = QuestManager.Instance.GetActiveQuest(questID);
            if (quest != null && (quest.status == QuestStatus.ReadyToComplete || quest.IsCompleted()))
            {
                Debug.Log($"NPCDialogue: Завершаем готовый квест {questID} при повторном разговоре");
                QuestManager.Instance.CompleteTalkQuest(quest);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Проверяем нажатие клавиши взаимодействия
            if (InputUtils.GetKey(InputSettings.Instance.ActionKey))
            {
                Debug.Log($"NPCDialogue: Нажата клавиша {InputSettings.Instance.ActionKey} для взаимодействия с NPC #{conversationOrder}");
                Interact();
            }
            else if (QuestDisplay.Instance != null && !QuestDisplay.Instance.HasText)
            {
                // Отображаем подходящую подсказку в зависимости от порядкового номера NPC
               // string promptText;
                
                //if (QuestManager.Instance.currentTalkStep == conversationOrder)
               // {
               //     promptText = $"Нажмите {InputSettings.Instance.ActionKey} для разговора с NPC";
              //  }
              //  else
             //   {
             //      promptText = "Этот персонаж сейчас не готов к разговору.";
              //  }
                
               // QuestDisplay.Instance.SetQuestText(promptText);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"NPCDialogue: Игрок вошел в зону взаимодействия с NPC #{conversationOrder}");
            
            // Отображаем подходящую подсказку в зависимости от порядкового номера NPC
            if (QuestDisplay.Instance != null && !QuestDisplay.Instance.HasText)
            {
                string promptText;
                
                /*if (QuestManager.Instance != null && QuestManager.Instance.currentTalkStep == conversationOrder)
                {
                    promptText = $"Нажмите {InputSettings.Instance.ActionKey} для разговора с NPC";
                }
                else
                {
                    promptText = "Этот персонаж сейчас не готов к разговору.";
                }
                
                QuestDisplay.Instance.SetQuestText(promptText);*/
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && QuestDisplay.Instance != null)
        {
            // Очищаем только подсказки, не затрагивая другие диалоги
            string currentText = QuestDisplay.Instance.questTextField?.text ?? "";
            bool isPrompt = currentText.Contains($"Нажмите {InputSettings.Instance.ActionKey} для разговора с NPC") || 
                           currentText.Contains("Этот персонаж сейчас не готов к разговору.");
            
            if (isPrompt)
            {
                QuestDisplay.Instance.HideQuestText();
                Debug.Log($"NPCDialogue: Игрок покинул зону взаимодействия с NPC #{conversationOrder}, подсказка скрыта.");
            }
        }
    }
}
