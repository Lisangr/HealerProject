using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Hero Settings")]
    public GameObject prefab;       // Префаб героя
    public int health;              // Здоровье героя
    public int defense;             // Защита героя (броня)
    public float moveSpeed;         // Скорость передвижения героя

    [Header("Hero Characters")]
    public int power;               // Сила
    public int dexterity;           // Ловкость
    public int stamina;             // Выносливость
    public int defence;             // Защита
    public int intellect = 10;      // Интеллект (по умолчанию 10)

    [Header("Regeneration Settings")]
    public float regeneration = 0.02f; // Регенерация здоровья: 2% от максимального здоровья в секунду (начальная настройка)

    [Header("Attack Settings")]
    public int attack;              // Атака героя
    public float attackCooldown;    // Время перезарядки атаки
    public float attackRange;       // Дальность атаки
    public float detectionRange;    // Дальность обнаружения врагов
}
