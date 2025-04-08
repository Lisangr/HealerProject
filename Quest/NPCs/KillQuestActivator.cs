using UnityEngine;
using System.Collections.Generic;

public class KillQuestActivator : MonoBehaviour
{
    [Tooltip("ScriptableObject � ������� ������ ��� ������� ���� ������")]
    public KillQuestData killQuestData;

    // ����������� ��������� ��� ������������ ����������� ������ �� ��� questID
    private static HashSet<string> registeredQuestIDs = new HashSet<string>();

    // ��������� ������� ������� ��� ������� ����������
    private int currentKillCount = 0;

    // ����, �����������, ��� ���� ��������� ��� ��������� ���������� ������
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
        // ���� ��������� ����� ��� ��������, �������
        if (QuestHunterManager.Instance.completedKillQuests.Contains(killQuestData))
            return;

        // ���� ��� �������� �������� �����������, �������
        if (questCompleted)
            return;

        // ���� ������ ���� ������������� ������� ����
        if (enemyName == killQuestData.targetEnemyName)
        {
            // ������������ ����� ������ ��� ������ �������� ������� �����
            if (!registeredQuestIDs.Contains(killQuestData.questID))
            {
                QuestHunterManager.Instance.StartKillQuest(killQuestData);
                registeredQuestIDs.Add(killQuestData.questID);
                Debug.Log("Kill quest registered on first kill event.");
            }

            currentKillCount++;
            Debug.Log($"Enemy '{enemyName}' destroyed. Kill count: {currentKillCount}/{killQuestData.requiredKillCount}");

            // ���� ���������� ��������� ���������� �������, ���������� �������� � ���������� ������
            if (currentKillCount >= killQuestData.requiredKillCount)
            {
                questCompleted = true; // �������� �������� ����������
                Debug.Log("Kill quest objective met. Notifying QuestHunterManager to complete kill quest.");
                QuestHunterManager.Instance.CompleteKillQuest(killQuestData);
                // ��������� ���������, ����� ���������� ������� �� ��������������
                this.enabled = false;
            }
        }
    }
}
