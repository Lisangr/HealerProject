using static QuestObjective;
using UnityEngine;
using YG;

public class QuestGiver : MonoBehaviour
{
    public Quest quest;
    public QuestData questData;

    // ����� ���� ��� �������� ���������� ������
    public QuestData acceptedQuestData;

    private int currentPhraseIndex = 0;
    private NPCDialogue dialogueComponent;

    private void Awake()
    {
        dialogueComponent = GetComponent<NPCDialogue>();
    }

    public void GiveQuest()
    {
        if (quest != null && quest.status == QuestStatus.NotStarted)
        {
            QuestManager.Instance.StartQuest(quest);
            // ������������� ����, ����� ��������� ����� ���������� �������������
            quest.canAutoEnded = true;

            // ���� ����� ���� Escort, ���������� �������������
            if (quest.type == ObjectiveType.Escort)
            {
                EscortNPC escort = GetComponent<EscortNPC>();
                if (escort != null)
                {
                    escort.ActivateEscort();
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {
            QuestDisplay display = QuestDisplay.Instance;

            if (quest.status == QuestStatus.NotStarted)
            {
                GiveQuest();
                currentPhraseIndex = 0;
            }
            else if (quest.status == QuestStatus.ReadyToComplete)
            {
                // Если это квест типа Talk, используем специальный метод
                if (quest.HasObjectiveOfType(QuestObjective.ObjectiveType.Talk))
                {
                    Debug.Log($"QuestGiver: Завершаем квест типа Talk '{quest.questName}' с помощью CompleteTalkQuest");
                    QuestManager.Instance.CompleteTalkQuest(quest);
                }
                else
                {
                    QuestManager.Instance.CompleteQuest(quest);
                }
                
                // Используем системный язык
                string currentLang = GetSystemLanguageCode();
                string completionMessage = questData.GetCompletionMessage(currentLang);
                display.SetQuestText(completionMessage);
                return;
            }

            // �������� �������������� ����� �� ������ ���������� �����
            string currentLangForDialogues = GetSystemLanguageCode();
            string[] localizedPhrases = questData.GetLocalizedDialogues(currentLangForDialogues);
            if (localizedPhrases != null && currentPhraseIndex < localizedPhrases.Length)
            {
                display.SetQuestText(localizedPhrases[currentPhraseIndex]);
                currentPhraseIndex++;
            }
        }
        else if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueComponent != null)
            {
                dialogueComponent.Interact();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestDisplay.Instance.SetQuestText("");
        }
    }

    // ����� ��� ��������� ���� ����� �� ������ ���������� �����
    private string GetSystemLanguageCode()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            // ���������� ������� ����
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
                    return "Ru"; // �������� �� ���������
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
                    return "en"; // �������� �� ���������
            }
        }
    }
}
