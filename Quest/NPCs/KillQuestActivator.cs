using UnityEngine;
using System.Collections.Generic;

public class KillQuestActivator : MonoBehaviour
{
    [Tooltip("ScriptableObject с данными квеста для данного типа врагов")]
    public KillQuestData killQuestData;

    // Статическая коллекция для отслеживания регистрации квеста по его questID
    private static HashSet<string> registeredQuestIDs = new HashSet<string>();

    // Локальный счётчик убийств для данного компонента
    private int currentKillCount = 0;

    // Флаг, указывающий, что этот компонент уже обработал завершение квеста
    private bool questCompleted = false;

    private void OnEnable()
    {
        Enemy.OnEnemyKilled += OnEnemyKilledHandler;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= OnEnemyKilledHandler;
    }
    private void OnEnemyKilledHandler(string enemyName)
    {
        // Если глобально квест уже завершён, выходим
        if (QuestHunterManager.Instance.completedKillQuests.Contains(killQuestData))
            return;

        // Если уже локально помечено завершённым, выходим
        if (questCompleted)
            return;

        // Если убитый враг соответствует нужному типу
        if (enemyName == killQuestData.targetEnemyName)
        {
            // Регистрируем квест только при первом убийстве нужного врага
            if (!registeredQuestIDs.Contains(killQuestData.questID))
            {
                QuestHunterManager.Instance.StartKillQuest(killQuestData);
                registeredQuestIDs.Add(killQuestData.questID);
                Debug.Log("Kill quest registered on first kill event.");
            }

            currentKillCount++;
            Debug.Log($"Enemy '{enemyName}' destroyed. Kill count: {currentKillCount}/{killQuestData.requiredKillCount}");

            // Если достигнуто требуемое количество убийств, уведомляем менеджер о завершении квеста
            if (currentKillCount >= killQuestData.requiredKillCount)
            {
                questCompleted = true; // локально помечаем завершение
                Debug.Log("Kill quest objective met. Notifying QuestHunterManager to complete kill quest.");
                QuestHunterManager.Instance.CompleteKillQuest(killQuestData);
                // Отключаем компонент, чтобы дальнейшие события не обрабатывались
                this.enabled = false;
            }
        }
    }
}
