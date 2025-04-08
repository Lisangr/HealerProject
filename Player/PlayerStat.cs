using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [Header("Main Elements")]
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private int playerIndex;
    [SerializeField] private PlayerData currentPlayerData;

    private int currentPower; // 1 ������� = 2 ������� �����
    private int currentDextery; // 1/3 �� �������� = 1 ������� ��������
    private int currentStamina; // 1 ������� = 10 ��
    private int currentDefence; // 1 ������� = 6 ������
    private int currentIntellect; //1 ������� = 1 ������� ����������

    private Player player;
    private const string PowerKey = "PlayerPower";
    private const string DexteryKey = "PlayerDextery";
    private const string StaminaKey = "PlayerStamina";
    private const string DefenceKey = "PlayerDefence";
    private const string IntellectKey = "PlayerIntellect";
    private const string StatPointKey = "StatPoints";

    private const string BaseHealthKey = "BaseHealthText";
    private const string BaseDamageKey = "BaseDamage";
    private const string BaseDefense = "BaseDefense";
    private const string BaseMoveSpeed = "BaseMoveSpeed";
    private const string BaseIntellectKey = "BaseTimeIntellect";

    void Start()
    {
        InitializePlayerData();
        player = GetComponent<Player>();

        // Если не удалось найти компонент Player на этом объекте, попробуем найти его в сцене
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogError("Не удалось найти компонент Player! Обновление характеристик не будет работать.");
                return; // Выходим из метода, чтобы избежать NullReferenceException
            }
        }

        // Проверяем, есть ли сохраненные данные в PlayerPrefs
        bool hasSavedData = PlayerPrefs.HasKey(PowerKey);
        
        if (hasSavedData)
        {
            // Загружаем сохраненные статы из PlayerPrefs
            currentPower = PlayerPrefs.GetInt(PowerKey, currentPlayerData.power);
            currentDextery = PlayerPrefs.GetInt(DexteryKey, currentPlayerData.dexterity);
            currentStamina = PlayerPrefs.GetInt(StaminaKey, currentPlayerData.stamina);
            currentDefence = PlayerPrefs.GetInt(DefenceKey, currentPlayerData.defence);
            currentIntellect = PlayerPrefs.GetInt(IntellectKey, currentPlayerData.intellect);

            // Обновляем только базовые характеристики в ScriptableObject
            currentPlayerData.power = currentPower;
            currentPlayerData.dexterity = currentDextery;
            currentPlayerData.stamina = currentStamina;
            currentPlayerData.defence = currentDefence;
            currentPlayerData.intellect = currentIntellect;
            
            Debug.Log($"PlayerStat: Загружены сохраненные характеристики: Сила {currentPower}, Ловкость {currentDextery}, " +
                     $"Выносливость {currentStamina}, Защита {currentDefence}, Интеллект {currentIntellect}");
        }
        else
        {
            // Если данных нет, используем исходные значения из ScriptableObject
            currentPower = currentPlayerData.power;
            currentDextery = currentPlayerData.dexterity;
            currentStamina = currentPlayerData.stamina;
            currentDefence = currentPlayerData.defence;
            currentIntellect = currentPlayerData.intellect;
            
            // Сохраняем исходные значения в PlayerPrefs
            PlayerPrefs.SetInt(PowerKey, currentPower);
            PlayerPrefs.SetInt(DexteryKey, currentDextery);
            PlayerPrefs.SetInt(StaminaKey, currentStamina);
            PlayerPrefs.SetInt(DefenceKey, currentDefence);
            PlayerPrefs.SetInt(IntellectKey, currentIntellect);
            PlayerPrefs.Save();
            
            Debug.Log($"PlayerStat: Используются исходные характеристики из ScriptableObject: Сила {currentPower}, Ловкость {currentDextery}, " +
                     $"Выносливость {currentStamina}, Защита {currentDefence}, Интеллект {currentIntellect}");
        }
        
        // Не вызываем player.LoadCharacterStats() - пусть Player сам загрузит свои характеристики
    }

    private void InitializePlayerData()
    {
        if (playerConfig == null)
        {
            // Попробуем загрузить конфигурацию из ресурсов, если не назначена вручную
            playerConfig = Resources.Load<PlayerConfig>("PlayerConfig");
            
            if (playerConfig == null)
            {
                Debug.LogError("PlayerConfig не назначен и не найден в ресурсах!");
                return;
            }
            else
            {
                Debug.Log("PlayerConfig успешно загружен из ресурсов.");
            }
        }

        if (playerConfig.players.Count > playerIndex && playerIndex >= 0)
        {
            currentPlayerData = playerConfig.players[playerIndex];
            Debug.Log($"PlayerStat: Инициализирован игрок {playerIndex} из конфига. " + 
                      $"Базовые характеристики: Сила {currentPlayerData.power}, Ловкость {currentPlayerData.dexterity}, " +
                      $"Выносливость {currentPlayerData.stamina}, Защита {currentPlayerData.defence}, Интеллект {currentPlayerData.intellect}");
        }
        else
        {
            Debug.LogError($"Нет игрока с индексом {playerIndex} в конфиге!");
        }
    }
    #region Update Stats Metod for Buttons
    public void CalculatePower()
    {
        int points = PlayerPrefs.GetInt("StatPoints", 0);
        if (points > 0)
        {
            points--;
            int currentPower = PlayerPrefs.GetInt("PlayerPower", 0);
            currentPower++;
            
            // Обновляем значение в ScriptableObject
            currentPlayerData.power = currentPower;
            
            PlayerPrefs.SetInt("PlayerPower", currentPower);
            PlayerPrefs.SetInt("StatPoints", points);
            PlayerPrefs.Save();
            
            Debug.Log($"Добавлена 1 единица силы. Осталось очков: {points}");
            
            // Вызываем метод обновления игрока и сохранения всех характеристик
            if (player != null)
            {
                player.UpdateAttack(currentPower);
                player.SaveCharacterStats();
            }
        }
        else
        {
            PlayerPrefs.DeleteKey(StatPointKey);
            Debug.Log("Нет свободных очков!");
        }       
    }

    public void CalculateDextery()
    {
        int points = PlayerPrefs.GetInt("StatPoints", 0);
        if (points > 0)
        {
            points--;
            int currentDex = PlayerPrefs.GetInt("PlayerDextery", 0);
            currentDex++;
            
            // Обновляем значение в ScriptableObject
            currentPlayerData.dexterity = currentDex;
            
            PlayerPrefs.SetInt("PlayerDextery", currentDex);
            PlayerPrefs.SetInt("StatPoints", points);
            PlayerPrefs.Save();
            
            Debug.Log($"Добавлена 1 единица ловкости. Осталось очков: {points}");
            
            // Вызываем метод обновления игрока и сохранения всех характеристик
            if (player != null)
            {
                player.UpdateMoveSpeed(currentDex);
                player.SaveCharacterStats();
            }
        }
        else
        {
            PlayerPrefs.DeleteKey(StatPointKey);
            Debug.Log("Нет свободных очков!");
        }
    }

    public void CalculateStamina()
    {
        int points = PlayerPrefs.GetInt("StatPoints", 0);
        if (points > 0)
        {
            points--;
            int currentStamina = PlayerPrefs.GetInt("PlayerStamina", 0);
            currentStamina++;
            
            // Обновляем значение в ScriptableObject
            currentPlayerData.stamina = currentStamina;
            
            PlayerPrefs.SetInt("PlayerStamina", currentStamina);
            PlayerPrefs.SetInt("StatPoints", points);
            PlayerPrefs.Save();
            
            Debug.Log($"Добавлена 1 единица выносливости. Осталось очков: {points}");
            
            // Вызываем метод обновления игрока и сохранения всех характеристик
            if (player != null)
            {
                player.UpdateMaxHealth(currentStamina);
                player.SaveCharacterStats();
            }
        }
        else
        {
            PlayerPrefs.DeleteKey(StatPointKey);
            Debug.Log("Нет свободных очков!");
        }
    }

    public void CalculateDefence()
    {
        int points = PlayerPrefs.GetInt("StatPoints", 0);
        if (points > 0)
        {
            points--;
            int currentDefence = PlayerPrefs.GetInt("PlayerDefence", 0);
            currentDefence++;
            
            // Обновляем значение в ScriptableObject
            currentPlayerData.defence = currentDefence;
            
            PlayerPrefs.SetInt("PlayerDefence", currentDefence);
            PlayerPrefs.SetInt("StatPoints", points);
            PlayerPrefs.Save();
            
            Debug.Log($"Добавлена 1 единица защиты. Осталось очков: {points}");
            
            // Вызываем метод обновления игрока и сохранения всех характеристик
            if (player != null)
            {
                player.UpdateDefense(currentDefence);
                player.SaveCharacterStats();
            }
        }
        else
        {
            PlayerPrefs.DeleteKey(StatPointKey);
            Debug.Log("Нет свободных очков!");
        }
    }

    public void CalculateIntellect()
    {
        int points = PlayerPrefs.GetInt("StatPoints", 0);
        if (points > 0)
        {
            points--;
            int currentIntellect = PlayerPrefs.GetInt("PlayerIntellect", 0);
            currentIntellect++;
            
            // Обновляем значение в ScriptableObject
            currentPlayerData.intellect = currentIntellect;
            
            PlayerPrefs.SetInt("PlayerIntellect", currentIntellect);
            PlayerPrefs.SetInt("StatPoints", points);
            PlayerPrefs.Save();
            
            Debug.Log($"Добавлена 1 единица интеллекта. Осталось очков: {points}");
            
            // Вызываем метод обновления игрока и сохранения всех характеристик
            if (player != null)
            {
                player.UpdateIntellect(currentIntellect);
                player.SaveCharacterStats();
            }
        }
        else
        {
            PlayerPrefs.DeleteKey(StatPointKey);
            Debug.Log("Нет свободных очков!");
        }
    }
    #endregion
}