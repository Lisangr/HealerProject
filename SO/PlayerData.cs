using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Hero Settings")]
    public GameObject prefab;       // ������ �����
    public int health;              // �������� �����
    public int defense;             // ������ ������ (��������)
    public float moveSpeed;         // �������� ������������ �����

    [Header("Hero Characters")]
    public int power;               // ����
    public int dexterity;           // ��������
    public int stamina;             // ������������
    public int defence;           // ������
    public int intellect = 10;

    [Header("Regeneration Settings")]
    public float regeneration = 0.02f; // �������������� ��������: 2% �� ��������� � ������� (������� ��������)

    [Header("Attack Settings")]
    public int attack;              // ���� �����
    public float attackCouldown;    // �������� ����� �����
    public float attackRange;       // ��������� ����� �����
    public float detectionRange;    // ��������� ����������� �����
}
