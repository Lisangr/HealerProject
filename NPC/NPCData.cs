using UnityEngine;

[System.Serializable]
public class NPCData
{
    [Header("Идентификация")]
    public string npcID; // Уникальный идентификатор НПС
    public string npcName;
    public string npcDescription;

    [Header("Основные характеристики")]
    public int maxHealth = 100;
    public int attackDamage = 10;
    public float moveSpeed = 3f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float followDistance = 3f;
    public float rotationSpeed = 5f;
    public float detectionRange = 5f; // Радиус обнаружения врагов

    [Header("Визуальные эффекты")]
    public GameObject attackVFXPrefab;
    public GameObject hitVFXPrefab;
    public AudioClip attackSound;
    public AudioClip hitSound;
} 