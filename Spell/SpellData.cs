using UnityEngine;

[System.Serializable]
public class SpellData
{
    [Header("Идентификация")]
    public string spellID; // Уникальный идентификатор заклинания
    public string spellName;
    public string spellDescription;

    [Header("Основные характеристики")]
    public float power = 10f; // Сила заклинания
    public float manaCost = 10f; // Затраты маны
    public float cooldown = 5f; // Время перезарядки
    public float castTime = 1f; // Время произнесения
    public float range = 10f; // Дальность действия
    public float aoeRadius = 3f;

    [Header("Визуальные эффекты")]
    public GameObject castVFXPrefab; // Эффект при произнесении
    public GameObject impactVFXPrefab; // Эффект при попадании
    public AudioClip castSound; // Звук произнесения
    public AudioClip impactSound; // Звук попадания

    [Header("Настройки группы")]
    public bool affectAllGroupMembers = false; // Влияет ли на всех членов группы
    public bool requireGroupTarget = false; // Требуется ли цель из группы
    public bool canTargetSelf = true; // Можно ли применить на себя
    public bool canTargetEnemies = true; // Можно ли применить на врагов
    public bool canTargetAllies = true; // Можно ли применить на союзников

    [Header("Дополнительные эффекты")]
    public float duration = 0f; // Длительность эффекта (если есть)
    public float tickInterval = 0f; // Интервал между тиками (для периодических эффектов)
    public bool isHealing = false; // Является ли заклинание лечебным
    public bool isBuff = false; // Является ли заклинанием усиления
    public bool isDebuff = false; // Является ли заклинанием ослабления
} 