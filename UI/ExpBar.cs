using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExpBar : MonoBehaviour
{
    public Image expImage;
    public Text expText;
    public Text lvlText; // Текст для отображения уровня

    [Header("Level Up Effects")]
    [SerializeField] private GameObject levelUpVFXPrefab; // Префаб эффекта повышения уровня

    // Очки, выдаваемые при повышении уровня
    private int statPoints = 0;
    [SerializeField] private int defaultStatPoints = 3;

    private int expForLevelUp = 400;
    private int currentExp;
    private int currentLevel = 1; // Начальный уровень
    public int GetCurrentExp() { return currentExp; }
    public int GetCurrentLevel() { return currentLevel; }
    public int GetStatPoints() { return statPoints; }
    public int GetExpForLevelUp() { return expForLevelUp; }

    private const string LvlKey = "PlayerLvl";
    private const string ExpKey = "PlayerExp";
    private const string StatPointKey = "StatPoints";
    private const string ExpForLevelUpKey = "ExpForLevelUp";

    private void Start()
    {
        LoadExperience();
        UpdateUI();

        // Убеждаемся, что эффект Buff отключен при старте игры
        DisableBuffEffectAtStart();

        Debug.Log($"Start: Level {currentLevel}, Exp {currentExp}/{expForLevelUp}, StatPoints {statPoints}");
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDeath += AddExperience;

        // Подписка на события от менеджеров квестов
        SubscribeToQuestManagers();
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= AddExperience;

        // Отписка от событий
        UnsubscribeFromQuestManagers();
    }

    private void SubscribeToQuestManagers()
    {
        // Подписка на основной менеджер квестов
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
        else
        {
            StartCoroutine(TrySubscribeToQuestManager());
        }

        // Подписка на менеджер квестов охоты
        if (QuestHunterManager.Instance != null)
        {
            QuestHunterManager.Instance.OnKillQuestCompleted += HandleKillQuestCompleted;
        }
        else
        {
            StartCoroutine(TrySubscribeToQuestHunterManager());
        }
    }

    private void UnsubscribeFromQuestManagers()
    {
        // Отписка от основного менеджера квестов
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }

        // Отписка от менеджера квестов охоты
        if (QuestHunterManager.Instance != null)
        {
            QuestHunterManager.Instance.OnKillQuestCompleted -= HandleKillQuestCompleted;
        }
    }

    // Попытка подписаться на QuestManager с задержкой
    private IEnumerator TrySubscribeToQuestManager()
    {
        int attempts = 0;
        while (QuestManager.Instance == null && attempts < 10)
        {
            yield return new WaitForSeconds(0.5f);
            attempts++;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
            Debug.Log("ExpBar: Успешная подписка на события QuestManager");
        }
        else
        {
            Debug.LogWarning("ExpBar: Не удалось найти QuestManager.Instance после нескольких попыток");
        }
    }

    // Попытка подписаться на QuestHunterManager с задержкой
    private IEnumerator TrySubscribeToQuestHunterManager()
    {
        int attempts = 0;
        while (QuestHunterManager.Instance == null && attempts < 10)
        {
            yield return new WaitForSeconds(0.5f);
            attempts++;
        }

        if (QuestHunterManager.Instance != null)
        {
            QuestHunterManager.Instance.OnKillQuestCompleted += HandleKillQuestCompleted;
            Debug.Log("ExpBar: Успешная подписка на события QuestHunterManager");
        }
        else
        {
            Debug.LogWarning("ExpBar: Не удалось найти QuestHunterManager.Instance после нескольких попыток");
        }
    }

    // Обработчик завершения обычного квеста
    private void HandleQuestCompleted(Quest quest)
    {
        if (quest != null)
        {
            int reward = quest.reward;
            AddExperienceInternal(reward);
            Debug.Log($"Получена награда за квест '{quest.questName}': +{reward} опыта. Новый опыт: {currentExp}/{expForLevelUp}");
        }
    }

    // Обработчик завершения квеста охоты
    private void HandleKillQuestCompleted(KillQuestData quest)
    {
        if (quest != null)
        {
            int reward = quest.reward;
            AddExperienceInternal(reward);
            Debug.Log($"Получена награда за квест охоты '{quest.questName}': +{reward} опыта. Новый опыт: {currentExp}/{expForLevelUp}");
        }
    }

    // Метод вызывается при получении опыта за убийство врага
    private void AddExperience(int exp)
    {
        AddExperienceInternal(exp);
        Debug.Log($"Получен опыт за убийство врага: +{exp} опыта. Новый опыт: {currentExp}/{expForLevelUp}");
    }

    // Общий метод для добавления опыта
    private void AddExperienceInternal(int exp)
    {
        currentExp += exp;

        // Если набрано достаточно опыта для повышения уровня, выполняем цикл level-up
        while (currentExp >= expForLevelUp)
        {
            LevelUp();
        }

        SaveExperience();
        UpdateUI();
    }

    // Публичный метод для ручного добавления опыта (можно вызвать из других скриптов)
    public void AddExperienceManually(int exp, string source = "неизвестно")
    {
        AddExperienceInternal(exp);
        Debug.Log($"Получен опыт из источника '{source}': +{exp} опыта. Новый опыт: {currentExp}/{expForLevelUp}");
    }

    private void LevelUp()
    {
        currentExp -= expForLevelUp; // Перенос остатка опыта
        currentLevel++;              // Увеличиваем уровень

        // Проигрываем VFX эффект повышения уровня
        PlayLevelUpEffect();

        // Если уровень кратен 5, выдаём 5 очков, иначе стандартное количество
        if (currentLevel % 5 == 0)
        {
            statPoints += 5;
            Debug.Log($"LevelUp: Новый уровень {currentLevel} (кратен 5). +5 очков. Всего очков: {statPoints}");
        }
        else
        {
            statPoints += defaultStatPoints;
            Debug.Log($"LevelUp: Новый уровень {currentLevel}. +{defaultStatPoints} очко. Всего очков: {statPoints}");
        }

        // Увеличиваем требуемый опыт для следующего уровня по разной формуле для разных диапазонов уровней
        if (currentLevel <= 10)
        {
            expForLevelUp = Mathf.CeilToInt(expForLevelUp * 1.2f); // Рост быстро с уменьшением на 15%
        }
        else if (currentLevel <= 20)
        {
            expForLevelUp = Mathf.CeilToInt(expForLevelUp * 1.1f); // Рост медленнее с уменьшением на 15%
        }
        else if (currentLevel <= 30)
        {
            expForLevelUp = Mathf.CeilToInt(expForLevelUp * 1.05f); // Еще медленнее с уменьшением на 15%
        }
        else if (currentLevel <= 40)
        {
            expForLevelUp = Mathf.CeilToInt(expForLevelUp * 1.15f); // Снова ускоряется с уменьшением на 15%
        }
        else
        {
            expForLevelUp = Mathf.CeilToInt(expForLevelUp * 1.07f); // Медленнее, но все равно растет с уменьшением на 15%
        }

        Debug.Log($"Новый порог опыта: {expForLevelUp}");
        SaveExperience(); // Сохраняем изменения после повышения уровня
    }

    // Метод для проигрывания эффекта повышения уровня
    private void PlayLevelUpEffect()
    {
        // Получаем позицию игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Ищем объект с именем "Buff" среди дочерних объектов игрока
            Transform buffTransform = player.transform.Find("Buff");
            
            // Если объект "Buff" найден
            if (buffTransform != null)
            {
                // Деактивируем объект сначала, чтобы гарантировать, что он не активен 
                // при начале игры и активируется только при повышении уровня
                buffTransform.gameObject.SetActive(true);
                
                // Получаем компонент ParticleSystem
                ParticleSystem particleSystem = buffTransform.GetComponent<ParticleSystem>();
                
                if (particleSystem != null)
                {
                    // Играем систему частиц
                    particleSystem.Play();
                    
                    Debug.Log($"Проигран эффект повышения уровня на игроке: {currentLevel}");
                }
                
                // Запускаем корутину для деактивации объекта через 2 секунды, независимо от наличия ParticleSystem
                StartCoroutine(DeactivateBuffEffect(buffTransform.gameObject, 2f));
            }
            // Если объект "Buff" не найден, но есть префаб
            else if (levelUpVFXPrefab != null)
            {
                // Создаем эффект в позиции игрока как раньше
                Vector3 spawnPosition = player.transform.position + Vector3.up; // Немного выше игрока
                GameObject vfxInstance = Instantiate(levelUpVFXPrefab, spawnPosition, Quaternion.identity);
                
                // Уничтожаем эффект через 2 секунды
                Destroy(vfxInstance, 2f);
                
                Debug.Log($"Создан временный эффект повышения уровня (префаб): {currentLevel}");
            }
            else
            {
                Debug.LogWarning("Не найден объект Buff у игрока и не задан префаб эффекта.");
            }
        }
        else
        {
            Debug.LogWarning("Не удалось найти игрока для проигрывания эффекта повышения уровня");
            
            // Если игрок не найден и есть префаб, создаем эффект в позиции ExpBar (UI)
            if (levelUpVFXPrefab != null)
            {
                GameObject vfxInstance = Instantiate(levelUpVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfxInstance, 2f);
            }
        }
    }
    
    // Корутина для деактивации эффекта Buff через заданное время
    private IEnumerator DeactivateBuffEffect(GameObject buffObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (buffObject != null)
        {
            buffObject.SetActive(false);
            Debug.Log("Эффект Buff деактивирован через " + delay + " секунд");
        }
    }

    private void UpdateUI()
    {
        if (expImage != null)
            expImage.fillAmount = (float)currentExp / expForLevelUp;

        if (expText != null)
            expText.text = currentExp + " / " + expForLevelUp;

        if (lvlText != null)
            lvlText.text = currentLevel.ToString();
    }

    private void SaveExperience()
    {
        PlayerPrefs.SetInt(ExpKey, currentExp);
        PlayerPrefs.SetInt(LvlKey, currentLevel);
        PlayerPrefs.SetInt(ExpForLevelUpKey, expForLevelUp);
        PlayerPrefs.SetInt(StatPointKey, statPoints);
        PlayerPrefs.Save();
    }

    private void LoadExperience()
    {
        currentExp = PlayerPrefs.GetInt(ExpKey, 0);
        currentLevel = PlayerPrefs.GetInt(LvlKey, 1);
        statPoints = PlayerPrefs.GetInt(StatPointKey, 0);
        expForLevelUp = PlayerPrefs.GetInt(ExpForLevelUpKey, expForLevelUp);
        Debug.Log($"LoadExperience: Уровень {currentLevel}, Опыт {currentExp}, СтатПоинты {statPoints}, Порог опыта {expForLevelUp}");
    }

    // Метод для восстановления состояния опыта
    public void SetExperience(int exp, int level, int statPoints, int expForLevelUp)
    {
        currentExp = exp;
        currentLevel = level;
        this.statPoints = statPoints;
        this.expForLevelUp = expForLevelUp;
        UpdateUI();
    }

    // Метод для обновления количества стат-поинтов
    public void UpdateStatPoints(int newStatPoints)
    {
        statPoints = newStatPoints;
        Debug.Log($"ExpBar: Обновлено количество стат-поинтов: {statPoints}");
        SaveExperience(); // Сохраняем изменения
    }

    // Метод для отключения эффекта Buff при старте игры
    private void DisableBuffEffectAtStart()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Transform buffTransform = player.transform.Find("Buff");
            if (buffTransform != null)
            {
                buffTransform.gameObject.SetActive(false);
                Debug.Log("Эффект Buff деактивирован при старте игры");
            }
        }
    }
}