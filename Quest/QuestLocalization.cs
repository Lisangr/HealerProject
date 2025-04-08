using UnityEngine;

[System.Serializable]
public class QuestDialogue
{
    public string languageCode; // "RU", "EN", "DE" � �.�.
    public string[] phrases;    // ������ ���� ��� ����� �����
}

[System.Serializable]
public class QuestCompletionLocalization
{
    public string languageCode; // ��������, "RU", "EN", "DE" � �.�.
    [TextArea]
    public string completionMessage;
    [TextArea]
    public string completionDialogue; // Диалоговая реплика при завершении квеста
}

[System.Serializable]
public class QuestTitleLocalization
{
    public string languageCode; // "RU", "EN", "DE" � �.�.
    public string title;        // �������������� �������� ������
}

[System.Serializable]
public class QuestDescriptionLocalization
{
    public string languageCode; // "RU", "EN", "DE" � �.�.
    [TextArea]
    public string description;  // �������������� �������� ������
}

[System.Serializable]
public class EnemyLocalization
{
    [Tooltip("��� ����� (��������, ru, en, de � �.�.)")]
    public string languageCode;
    [Tooltip("�������������� ��� �����")]
    public string localizedName;
}

[System.Serializable]
public class ItemLocalization
{
    [Tooltip("��� ����� (��������, ru, en, de � �.�.)")]
    public string languageCode;
    [Tooltip("�������������� �������� ��������")]
    public string localizedName;
}