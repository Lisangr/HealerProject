using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Hero Settings")]
    public GameObject prefab;       // Префаб героя
    public int health;              // Здоровье героя
    public int defense;             // Защита игрока (итоговая)
    public float moveSpeed;         // Скорость передвижения героя

    [Header("Hero Characters")]
    public int power;               // сила
    public int dexterity;           // ловкость
    public int stamina;             // выносливость
    public int defence;           // защита
    public int intellect = 10;

    [Header("Regeneration Settings")]
    public float regeneration = 0.02f; // Восстановление здоровья: 2% от максимума в секунду (базовое значение)

    [Header("Attack Settings")]
    public int attack;              // Урон героя
    public float attackCouldown;    // Скорость атаки героя
    public float attackRange;       // Дальность атаки героя
    public float detectionRange;    // Дальность обнаружения врага
}
