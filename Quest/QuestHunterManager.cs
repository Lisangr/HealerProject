using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using YG;
using System; // Добавляем для использования Action

public class QuestHunterManager : MonoBehaviour
{
    public static QuestHunterManager Instance;

    // Событие, вызываемое при завершении квеста охоты
    public event Action<KillQuestData> OnKillQuestCompleted;

    // Список активных и завершённых Kill-квестов (используем KillQuestData)
    public List<KillQuestData> activeKillQuests = new List<KillQuestData>();
    public List<KillQuestData> completedKillQuests = new List<KillQuestData>();

    // Словарь для отслеживания прогресса по каждому квесту (ключ – questID)
    private Dictionary<string, int> killProgress = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохраняем между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Подписываемся на событие убийства врага
        Enemy.OnEnemyKilled += OnEnemyKilledHandler;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= OnEnemyKilledHandler;
    }

    // Запуск квеста охоты на монстров
    public void StartKillQuest(KillQuestData killQuestData)
    {
        if (!activeKillQuests.Contains(killQuestData))
        {
            activeKillQuests.Add(killQuestData);
            killProgress[killQuestData.questID] = 0;
            Debug.Log($"Kill quest '{killQuestData.questName}' started.");

            // Обновляем отображение охотничьих квестов
            QuestDisplay.Instance.UpdateKillQuestsDisplay(activeKillQuests, completedKillQuests);
        }
    }

    // Обработчик события убийства врага
    private void OnEnemyKilledHandler(string enemyName)
    {
        // Обновляем прогресс для всех активных Kill-квестов, если имя врага совпадает
        foreach (KillQuestData quest in activeKillQuests.ToList())
        {
            if (quest.targetEnemyName == enemyName)
            {
                killProgress[quest.questID] = killProgress[quest.questID] + 1;
                Debug.Log($"Progress for quest '{quest.questName}': {killProgress[quest.questID]}/{quest.requiredKillCount}");

                // Если количество убийств достигло или превысило требуемое, завершаем квест
                if (killProgress[quest.questID] >= quest.requiredKillCount)
                {
                    CompleteKillQuest(quest);
                }
            }
        }
    }

    // Метод завершения квеста охоты на монстров
    public void CompleteKillQuest(KillQuestData quest)
    {
        // Если квест уже завершён, повторное завершение не выполняется
        if (completedKillQuests.Contains(quest))
            return;

        // Используем метод GetSystemLanguageCode() для определения языка системы
        string currentLang = GetSystemLanguageCode();
        string completionMessage = quest.GetCompletionMessage(currentLang);

        Debug.Log($"Kill quest '{quest.questName}' completed. Reward: {quest.reward}. {completionMessage}");
        QuestDisplay.Instance.SetQuestText(completionMessage);

        // Переносим квест из активных в завершённые
        activeKillQuests.Remove(quest);
        completedKillQuests.Add(quest);

        // Вызываем событие завершения квеста охоты для начисления опыта
        OnKillQuestCompleted?.Invoke(quest);

        // Обновляем отображение охотничьих квестов после завершения
        QuestDisplay.Instance.UpdateKillQuestsDisplay(activeKillQuests, completedKillQuests);

        // Запускаем корутину для очистки надписи через 3 секунды
        StartCoroutine(ClearQuestTextAfterDelay(3f));
    }

    private IEnumerator ClearQuestTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        QuestDisplay.Instance.SetQuestText("");
    }
    // Метод для получения кода языка на основе системного языка
    private string GetSystemLanguageCode()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            // Определяем текущий язык
            string currentLang = YandexGame.EnvironmentData.language;
            switch (currentLang)
            {
                case "Ru":
                    return "Ru";
                case "en":
                    return "en";
                case "tr":
                    return "tr";
                case "de":
                    return "de";
                case "es":
                    return "es";
                case "it":
                    return "it";
                case "fr":
                    return "fr";
                default:
                    return "Ru"; // Значение по умолчанию
            }
        }
        else
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                    return "Ru";
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Turkish:
                    return "tr";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.French:
                    return "fr";
                default:
                    return "en"; // Значение по умолчанию
            }
        }
    }
}
