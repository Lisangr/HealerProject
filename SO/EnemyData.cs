using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Делаем класс сериализуемым, чтобы он отображался в инспекторе
public class EnemyData
{
    [Header("Enemy Settings")]
    [Tooltip("Основные настройки врага")]
    public string enemyName;            // Имя врага
    public GameObject prefab;           // Префаб врага
    public int health;                  // Здоровье врага
    public int experience;              // Опыт за убийство врага

    [Header("Attack Settings")]
    [Tooltip("Настройки врага, связанные с атакующими данным")]
    public int damage;                  // Урон врага
    public float attackRange;           // Дальность атаки врага
    public float detectionRange = 10f;  // Радиус обнаружения целей
    public float attackCouldown = 1f;   // Перезарядка атаки
    public float moveSpeed;             // Скорость передвижения врага
    public float rotationSpeed = 10f;   // Скорость поворота

    [Header("Localization")]
    [Tooltip("Локализованные данные для врага")]
    public List<EnemyLocalization> localizations;
}