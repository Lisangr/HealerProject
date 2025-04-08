using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class HealthSystem : MonoBehaviour
{
    public enum EntityType
    {
        Player,
        Enemy,
        Companion
    }

    [Header("Тип существа")]
    [SerializeField] private EntityType entityType;

    [Header("Настройки здоровья")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Ссылки на конфигурации")]
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private EnemyConfig enemyConfig;
    [SerializeField] private NPCConfig npcConfig;

    [Header("События")]
    public UnityEvent<int> OnHealthChanged = new UnityEvent<int>();
    public UnityEvent OnDeath = new UnityEvent();

    private void Awake()
    {
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        switch (entityType)
        {
            case EntityType.Player:
                if (playerConfig != null && playerConfig.players.Count > 0)
                {
                    PlayerData playerData = playerConfig.players[0]; // Предполагаем, что используется первый игрок
                    maxHealth = playerData.health;
                }
                else
                {
                    Debug.LogError($"HealthSystem на {gameObject.name}: не задан PlayerConfig или нет данных игрока!");
                }
                break;

            case EntityType.Enemy:
                if (enemyConfig != null)
                {
                    string enemyName = gameObject.name.Replace("(Clone)", "").Trim();
                    EnemyData enemyData = enemyConfig.enemies.FirstOrDefault(e => e.enemyName == enemyName);
                    if (enemyData != null)
                    {
                        maxHealth = enemyData.health;
                    }
                    else
                    {
                        Debug.LogError($"HealthSystem на {gameObject.name}: не найдены данные врага в EnemyConfig!");
                    }
                }
                else
                {
                    Debug.LogError($"HealthSystem на {gameObject.name}: не задан EnemyConfig!");
                }
                break;

            case EntityType.Companion:
                if (npcConfig != null)
                {
                    string npcName = gameObject.name.Replace("(Clone)", "").Trim();
                    NPCData npcData = npcConfig.npcs.FirstOrDefault(n => n.npcName == npcName);
                    if (npcData != null)
                    {
                        maxHealth = npcData.maxHealth; // Используем maxHealth вместо health
                    }
                    else
                    {
                        Debug.LogError($"HealthSystem на {gameObject.name}: не найдены данные NPC в NPCConfig!");
                    }
                }
                else
                {
                    Debug.LogError($"HealthSystem на {gameObject.name}: не задан NPCConfig!");
                }
                break;
        }

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        // Здесь можно добавить дополнительную логику смерти
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        if (newMaxHealth < 0)
        {
            Debug.LogWarning("Попытка установить отрицательное максимальное здоровье");
            return;
        }
        
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void SetCurrentHealth(int newCurrentHealth)
    {
        if (newCurrentHealth < 0)
        {
            Debug.LogWarning("Попытка установить отрицательное текущее здоровье");
            return;
        }
        
        currentHealth = Mathf.Min(newCurrentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void RefreshFromConfig()
    {
        InitializeHealth();
    }
} 