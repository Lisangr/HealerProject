using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // ������ ����� �������������, ����� �� ����������� � ����������
public class EnemyData
{
    [Header("Enemy Settings")]
    [Tooltip("�������� ��������� �����")]
    public string enemyName;            // ��� �����
    public GameObject prefab;           // ������ �����
    public int health;                  // �������� �����
    public int experience;              // ���� �� �������� �����

    [Header("Attack Settings")]
    [Tooltip("��������� �����, ��������� � ���������� ������")]
    public int damage;                  // ���� �����
    public float attackRange;           // ��������� ����� �����
    public float detectionRange = 10f;  // ������ ����������� �����
    public float attackCouldown = 1f;   // ����������� �����
    public float moveSpeed;             // �������� ������������ �����
    public float rotationSpeed = 10f;   // �������� ��������

    [Header("Localization")]
    [Tooltip("�������������� ������ ��� �����")]
    public List<EnemyLocalization> localizations;
}