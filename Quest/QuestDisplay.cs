using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class QuestDisplay : MonoBehaviour
{
    public static QuestDisplay Instance { get; private set; }  // Singleton
    public QuestData questData; // Ссылка на ScriptableObject с данными квеста
    public Text questTextField; // UI-текстовое поле для вывода описания квеста
    public GameObject dialogPanel; // Панель для отображения диалогов

    // Свойство для проверки наличия текста
    public bool HasText => !string.IsNullOrEmpty(questTextField?.text);

    // Время отображения диалога в секундах
    public float dialogDisplayTime = 5f;
    private float currentDialogTimer = 0f;
    private bool isDialogActive = false;
    private float dialogActivationDelay = 0.5f; // Задержка перед активацией обработки клавиши (в секундах)
    private float currentActivationDelay = 0f;
    private string[] currentDialogues; // Массив текущих фраз диалога
    private int currentDialogueIndex = 0; // Индекс текущей фразы
    private bool isKeyPressed = false; // Флаг для отслеживания состояния клавиши

    // Контейнеры для главных квестов
    public Transform mainActiveQuestsContainer;
    public Transform mainCompletedQuestsContainer;

    // Контейнеры для побочных квестов
    public Transform additionalActiveQuestsContainer;
    public Transform additionalCompletedQuestsContainer;

    // Контейнеры для квестов охоты
    public Transform hunterActiveQuestsContainer;
    public Transform hunterCompletedQuestsContainer;

    // Префабы для отображения записи квеста
    public GameObject activeQuestPrefab;
    public GameObject completedQuestPrefab;

    private void Awake()
    {
        // Если экземпляр ещё не установлен, устанавливаем его; иначе уничтожаем дубликат
        if (Instance == null)
        {
            Instance = this;
            // При необходимости можно сделать DontDestroyOnLoad, если UI должен сохраняться между сценами:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        questTextField.text = "";
        isDialogActive = false;
        
        // Скрываем панель диалога при старте
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // Обработка активного диалога
        if (isDialogActive)
        {
            // Если активна задержка, уменьшаем таймер задержки
            if (currentActivationDelay > 0)
            {
                currentActivationDelay -= Time.deltaTime;
                return; // Не обрабатываем другие действия с диалогом, пока активна задержка
            }
            
            // Проверяем нажатие клавиши E для перехода к следующей фразе или скрытия диалога
            bool keyDown = InputUtils.GetKey(InputSettings.Instance.TakeKey);
            
            if (keyDown && !isKeyPressed)
            {
                isKeyPressed = true;
                
                if (currentDialogues != null && currentDialogueIndex < currentDialogues.Length - 1)
                {
                    // Если есть следующая фраза, показываем её
                    currentDialogueIndex++;
                    DisplayCurrentDialogue();
                    // Установим задержку перед обработкой следующего нажатия
                    currentActivationDelay = dialogActivationDelay;
                    Debug.Log($"Показана следующая фраза диалога: {currentDialogueIndex + 1}/{currentDialogues.Length}");
                }
                else
                {
                    // Если фраз больше нет, скрываем диалог
                    HideQuestText();
                    Debug.Log("Диалог скрыт по нажатию клавиши " + InputSettings.Instance.TakeKey);
                }
            }
            else if (!keyDown && isKeyPressed)
            {
                // Сбрасываем флаг, когда клавиша отпущена
                isKeyPressed = false;
            }
            
            // Обновляем таймер
            currentDialogTimer -= Time.deltaTime;
            
            // Если время истекло, скрываем диалог
            if (currentDialogTimer <= 0)
            {
                HideQuestText();
                Debug.Log("Диалог скрыт автоматически по таймеру");
            }
        }
    }

    // Метод для отображения текущей фразы диалога
    private void DisplayCurrentDialogue()
    {
        if (currentDialogues != null && currentDialogueIndex < currentDialogues.Length)
        {
            questTextField.text = currentDialogues[currentDialogueIndex];
            // Сбрасываем таймер для новой фразы
            currentDialogTimer = dialogDisplayTime;
            Debug.Log($"Отображена фраза {currentDialogueIndex + 1}/{currentDialogues.Length}: {currentDialogues[currentDialogueIndex]}");
        }
    }

    // Метод для установки одиночной фразы диалога
    public void SetQuestText(string text)
    {
        // Проверяем, что текст не пустой
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("Попытка установить пустой текст диалога");
            return;
        }
        
        // Создаем массив из одной фразы
        currentDialogues = new string[] { text };
        currentDialogueIndex = 0;
        
        // Устанавливаем текст
        questTextField.text = text;
        Debug.Log($"Установлен текст диалога: {text}");
        
        // Активируем панель диалога
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
            Debug.Log("Панель диалога активирована");
        }
        else
        {
            Debug.LogWarning("Панель диалога не назначена в инспекторе");
        }
        
        // Запускаем таймер и устанавливаем задержку активации
        isDialogActive = true;
        isKeyPressed = false; // Сбрасываем флаг состояния клавиши
        currentDialogTimer = dialogDisplayTime;
        currentActivationDelay = dialogActivationDelay;
        Debug.Log($"Диалог будет отображаться {dialogDisplayTime} секунд. Переход к следующей фразе или закрытие - клавиша E (через {dialogActivationDelay} секунд)");
    }
    
    // Новый метод для установки множества фраз диалога
    public void SetQuestDialogues(string[] dialogues)
    {
        // Проверяем, что массив не пустой
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning("Попытка установить пустой массив диалогов");
            return;
        }
        
        // Сохраняем массив фраз
        currentDialogues = dialogues;
        currentDialogueIndex = 0;
        
        // Отображаем первую фразу
        DisplayCurrentDialogue();
        
        // Активируем панель диалога
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
            Debug.Log("Панель диалога активирована");
        }
        else
        {
            Debug.LogWarning("Панель диалога не назначена в инспекторе");
        }
        
        // Запускаем таймер и устанавливаем задержку активации
        isDialogActive = true;
        isKeyPressed = false; // Сбрасываем флаг состояния клавиши
        currentDialogTimer = dialogDisplayTime;
        currentActivationDelay = dialogActivationDelay;
        Debug.Log($"Диалог из {dialogues.Length} фраз будет отображаться. Переход к следующей фразе или закрытие - клавиша E (через {dialogActivationDelay} секунд)");
    }
    
    public void HideQuestText()
    {
        // Очищаем текст
        questTextField.text = "";
        
        // Деактивируем панель диалога
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        // Сбрасываем флаг, таймер и диалоги
        isDialogActive = false;
        isKeyPressed = false; // Сбрасываем флаг состояния клавиши
        currentDialogues = null;
        currentDialogueIndex = 0;
        Debug.Log("Диалог скрыт");
    }

    // Обновление отображения всех квестов: активных и выполненных, для обеих веток
    public void UpdateQuestsDisplay(List<Quest> activeQuests, List<Quest> completedQuests)
    {
        // Используем системный язык
        string currentLang = GetSystemLanguageCode();

        // Очистка контейнеров для главных квестов
        foreach (Transform child in mainActiveQuestsContainer)
            Destroy(child.gameObject);
        foreach (Transform child in mainCompletedQuestsContainer)
            Destroy(child.gameObject);

        // Очистка контейнеров для побочных квестов
        foreach (Transform child in additionalActiveQuestsContainer)
            Destroy(child.gameObject);
        foreach (Transform child in additionalCompletedQuestsContainer)
            Destroy(child.gameObject);

        // Вывод главных активных квестов с использованием компонента QuestEntryUI
        foreach (var quest in activeQuests.Where(q => q.questLine == QuestLine.Main))
        {
            GameObject questEntry = Instantiate(activeQuestPrefab, mainActiveQuestsContainer);
            QuestEntryUI entryUI = questEntry.GetComponent<QuestEntryUI>();
            if (entryUI != null)
            {
                entryUI.Setup(quest, currentLang);
            }
        }

        // Вывод побочных активных квестов
        foreach (var quest in activeQuests.Where(q => q.questLine == QuestLine.Additional))
        {
            GameObject questEntry = Instantiate(activeQuestPrefab, additionalActiveQuestsContainer);
            QuestEntryUI entryUI = questEntry.GetComponent<QuestEntryUI>();
            if (entryUI != null)
            {
                entryUI.Setup(quest, currentLang);
            }
        }

        // Вывод главных выполненных квестов (отображаем только название, локализованное)
        foreach (var quest in completedQuests.Where(q => q.questLine == QuestLine.Main))
        {
            GameObject questEntry = Instantiate(completedQuestPrefab, mainCompletedQuestsContainer);
            Text questNameText = questEntry.GetComponentInChildren<Text>();
            if (questNameText != null)
            {
                // Если QuestData присутствует, используем локализованное название, иначе стандартное
                string localizedTitle = quest.questData != null
                    ? quest.questData.GetLocalizedTitle(currentLang)
                    : quest.questName;
                questNameText.text = localizedTitle;
            }
        }

        // Вывод побочных выполненных квестов (отображаем только название, локализованное)
        foreach (var quest in completedQuests.Where(q => q.questLine == QuestLine.Additional))
        {
            GameObject questEntry = Instantiate(completedQuestPrefab, additionalCompletedQuestsContainer);
            Text questNameText = questEntry.GetComponentInChildren<Text>();
            if (questNameText != null)
            {
                string localizedTitle = quest.questData != null
                    ? quest.questData.GetLocalizedTitle(currentLang)
                    : quest.questName;
                questNameText.text = localizedTitle;
            }
        }
    }
    public void UpdateKillQuestsDisplay(List<KillQuestData> activeKillQuests, List<KillQuestData> completedKillQuests)
    {
        // Используем системный язык
        string currentLang = GetSystemLanguageCode();

        // Очистка контейнеров для квестов охоты (Hunter)
        foreach (Transform child in hunterActiveQuestsContainer)
            Destroy(child.gameObject);
        foreach (Transform child in hunterCompletedQuestsContainer)
            Destroy(child.gameObject);

        // Вывод активных kill-квестов
        foreach (var quest in activeKillQuests)
        {
            GameObject questEntry = Instantiate(activeQuestPrefab, hunterActiveQuestsContainer);
            QuestEntryUI entryUI = questEntry.GetComponent<QuestEntryUI>();
            if (entryUI != null)
            {
                // Передаем объект killQuest и текущий язык
                entryUI.Setup(quest, currentLang);
            }
        }

        // Вывод завершённых kill-квестов – отображаем только название (локализованное, если есть)
        foreach (var quest in completedKillQuests)
        {
            GameObject questEntry = Instantiate(completedQuestPrefab, hunterCompletedQuestsContainer);
            Text questNameText = questEntry.GetComponentInChildren<Text>();
            if (questNameText != null)
            {
                // Если в KillQuestData реализован метод GetLocalizedTitle, можно сделать так:
                string localizedTitle = quest.GetLocalizedTitle(currentLang);
            }
        }
    }
    // Метод для получения кода языка на основе системного языка
    private string GetSystemLanguageCode()
    {
        return LocalizationHelper.GetSystemLanguageCode();
    }
}
