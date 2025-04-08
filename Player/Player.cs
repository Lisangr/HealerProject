using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [Header("Main Elements")]
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private int playerIndex = 0;

    [Header("UI Elements")]
    public Image healthBar;
    public Text hpText;
    public GameObject defeatPanel;
    public Image damageImage; // ���� ��� ����������� ������� �����

    [Header("Attack VFX")]
    [SerializeField] private GameObject attackVFXPrefab; // ������ ������� �����
    [SerializeField] private Transform attackVFXSpawnPoint; // ����� ������ �������

    [Header("Melee Attack Settings")]
    [SerializeField] private float sphereCastCenterYOffset = 1f; // �������� ������ ��������� �� Y �� ������� ������
    [SerializeField] private float sphereCastRadius = 2f;        // ������ ����� ��� �������� ������������
    [SerializeField] private float sphereCastEndRadius = 2.5f;     // ������ ����� � ����� (��� Gizmos)

    [Header("Melee Hit VFX")]
    [SerializeField] private GameObject meleeHitVFXPrefab;

    [Header("Damage Effect")]
    [SerializeField] private float damageFlashDuration = 0.1f; // ������������ �������
    [SerializeField] private Color damageColor = new Color(1, 0, 0, 0.1f); // ���� �������

    [Header("Attack Audio Settings")]
    [SerializeField] private AudioSource rangedAttackAudioSource; // ������ �� ������� AudioSource ��� ������� ����
    [SerializeField] private AudioSource meleeAttackAudioSource;  // ������ �� ������� AudioSource ��� �������� ���
    [SerializeField] private AudioClip bowDrawClip;       // ���� ����������� ������
    [SerializeField] private AudioClip arrowFlightClip;   // ���� ����� ������
    [SerializeField] private AudioClip swordSwingClip;    // ���� ������ ����

    // ��������� ���������� � ������
    private bool isInCombat = false;
    private float combatTimer = 0f;
    [SerializeField] private float combatCooldown = 3f;
    private PlayerData currentPlayerData;
    public int currentHealth;
    private int maxHealth;
    private Coroutine damageCoroutine;
    private PlayerAnimationController animationController;
    private PlayerMovement movement;
    private float attackCooldown = 0f;
    private PlayerSaveSystem saveSystem;
    private ExpBar expBar; // ��� ������� � ����� � ������
    private int originalHealthStat;

    private const string StaminaKey = "PlayerStamina";
    private const string DefenceKey = "PlayerDefence";
    private const string BaseHealthKey = "BaseHealthText";
    private const string BaseDamageKey = "BaseDamage";
    private const string BaseDefense = "BaseDefense";
    private const string BaseMoveSpeed = "BaseMoveSpeed";
    private const string BaseIntellectKey = "BaseTimeIntellect";

    public delegate void TameAction(int exp);
    public static event TameAction OnEnemyTame;

    [Header("Группа")]
    [SerializeField] private List<CompanionNPC> companions = new List<CompanionNPC>();
    [SerializeField] private float companionDetectionRadius = 5f;

    // Добавляем переменную для текущей цели
    private Enemy currentTarget;

    [Header("Health System")]
    [SerializeField] private HealthSystem healthSystem;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        healthSystem = GetComponent<HealthSystem>();
        animationController = GetComponent<PlayerAnimationController>();

        // Инициализация системы сохранения
        saveSystem = FindObjectOfType<PlayerSaveSystem>();
        if (saveSystem == null)
            saveSystem = gameObject.AddComponent<PlayerSaveSystem>();

        expBar = FindObjectOfType<ExpBar>();

        // Сначала инициализируем базовые данные из ScriptableObject
        InitializePlayerData();
        
        // Теперь загружаем сохраненные характеристики из PlayerPrefs
        LoadCharacterStats();

        // Сохраняем оригинальное базовое здоровье для бонусов
        int baseHealth = currentPlayerData.health;
        originalHealthStat = baseHealth;

        // Обновляем здоровье с учетом возможных бонусов
        int baseStamina = currentPlayerData.stamina;
        int currentStamina = PlayerPrefs.GetInt(StaminaKey, currentPlayerData.stamina);
        int staminaBonus = (currentStamina - baseStamina) * 10;

        if (staminaBonus > 0) {
            healthSystem.SetMaxHealth(baseHealth + staminaBonus);
            healthSystem.SetCurrentHealth(healthSystem.GetMaxHealth());
            UpdateHealthDisplay();
            Debug.Log($"Применен бонус здоровья: +{staminaBonus} от выносливости");
        }

        if (damageImage != null)
            damageImage.color = Color.clear;

        // Запуск корутин восстановления здоровья и создания снапшотов
        StartCoroutine(RegenerationCoroutine());
        StartCoroutine(DelayedRestore());
        StartCoroutine(SnapshotCoroutine());

        // Подписываемся на событие смерти в HealthSystem
        if (healthSystem != null)
        {
            healthSystem.OnDeath.AddListener(Die);
        }
    }    

    void Update()
    {
        if (isInCombat)
        {
            combatTimer -= Time.deltaTime;
            if (combatTimer <= 0f)
                isInCombat = false;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ������ ��� UI � ����� �� ������������ ������� ����
            return;
        }

        HandleAttackInput();
        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        if (InputUtils.GetKeyDown(InputSettings.Instance.TakeMonsterKey))
        {
            TameMonster();
        }

        // Проверка нажатия F для добавления НПС в группу
        if (Input.GetKeyDown(KeyCode.F))
        {
            CheckForNearbyCompanions();
        }

        // Передача текущей цели всем компаньонам
        if (currentTarget != null)
        {
            foreach (var companion in companions)
            {
                companion.SetTarget(currentTarget);
            }
        }
    }

    private bool isAttacking = false;
    private void HandleAttackInput()
    {
        //     .
        if (movement != null && movement.CurrentSpeed > 1f)
            return;

        if (Input.GetMouseButton(0))
        {
            if (!isAttacking && attackCooldown <= 0f)
            {
                //    (,  )
                if (playerIndex >= 5 && playerIndex <= 20)
                {
                    StartCoroutine(MeleeAttackRoutine());
                }
                else
                {
                    StartCoroutine(RangedAttackRoutine());
                }
                attackCooldown = currentPlayerData.attackCooldown;
            }
        }
    }
    
    private IEnumerator MeleeAttackRoutine()
    {
        isInCombat = true;
        combatTimer = combatCooldown;

        isAttacking = true;
        animationController.PlayAnimation(animationController.animationStates.Attack1);
        yield return null;

        MeleeAttack();

        yield return WaitForAnimation(animationController.animationStates.Attack1);
        isAttacking = false;
    }

    private IEnumerator RangedAttackRoutine()
    {
        isInCombat = true;
        combatTimer = combatCooldown;

        isAttacking = true;
        animationController.PlayAnimation(animationController.animationStates.Attack1);
        yield return null;

        Attack();

        yield return WaitForAnimation(animationController.animationStates.Attack1);
        isAttacking = false;
    }

    private IEnumerator WaitForAnimation(string stateName)
    {
        AnimatorStateInfo stateInfo = animationController.animator.GetCurrentAnimatorStateInfo(0);
        while (!stateInfo.IsName(stateName))
        {
            yield return null;
            stateInfo = animationController.animator.GetCurrentAnimatorStateInfo(0);
        }
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animationController.animator.GetCurrentAnimatorStateInfo(0);
        }
    }    
    // ����� �������� ��� (��������)
    private Enemy FindNearestTarget()
    {
        float detectionRange = currentPlayerData.attackRange;
        int enemyLayerMask = LayerMask.GetMask("Enemy");
        
        Debug.Log($"Поиск цели: дальность {detectionRange}, маска слоя: {enemyLayerMask}");
        
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, detectionRange, enemyLayerMask);
        Debug.Log($"Найдено потенциальных целей: {enemyColliders.Length}");
        
        Enemy nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in enemyColliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                Debug.Log($"Проверка врага {enemy.name}: дистанция {distance}");
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                    Debug.Log($"Новая ближайшая цель: {enemy.name} на дистанции {distance}");
                }
            }
        }
        
        if (nearestEnemy == null)
        {
            Debug.Log("Цель не найдена в радиусе атаки");
        }
        else
        {
            Debug.Log($"Выбрана цель: {nearestEnemy.name} на дистанции {minDistance}");
        }
        
        return nearestEnemy;
    }

    private void Attack()
    {
        // Воспроизведение звука натягивания лука
        if (rangedAttackAudioSource != null && bowDrawClip != null)
            rangedAttackAudioSource.PlayOneShot(bowDrawClip);

        // Находим ближайшую цель
        Enemy targetEnemy = FindNearestTarget();
        Debug.Log($"Дальняя атака: найдена цель: {(targetEnemy != null ? targetEnemy.name : "null")}");
        currentTarget = targetEnemy; // Сохраняем текущую цель

        if (targetEnemy != null)
        {
            Debug.Log($"Дальняя атака: Наносим урон {currentPlayerData.attack} врагу {targetEnemy.name}");
            //    �����
            Vector3 directionToEnemy = (targetEnemy.transform.position - transform.position).normalized;
            directionToEnemy.y = 0; // ������� �������� �� ���������, ����� ����� �� ����������
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToEnemy), 0.1f);

            // ������������ ���������������� ����� ��� ����������� �������
            StartCoroutine(HighlightEnemy(targetEnemy));

            // ������� VFX-������, ������� ��������� ������������ � ������� ����
            Vector3 spawnPos = attackVFXSpawnPoint.position;
            Vector3 direction = (targetEnemy.transform.position - spawnPos).normalized;

            ProjectileMoveScript projectile;
            if (ArrowsPool.Instance != null)
            {
                projectile = ArrowsPool.Instance.GetArrow();
            }
            else
            {
                projectile = Instantiate(attackVFXPrefab, spawnPos, Quaternion.LookRotation(direction))
                    .GetComponent<ProjectileMoveScript>();
            }

            projectile.transform.position = spawnPos;
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            // ������� ������� ���������������� �����
            projectile.SetTarget(targetEnemy);

            // ������� ���� ��������������� ���� (���� ����� �����, ����� ������ ��������� ����)
            targetEnemy.TakeDamage(currentPlayerData.attack);
            Debug.Log($"Дальняя атака: Урон нанесен");
        }
        else
        {
            Debug.Log("��������� ���� �� �������. ������� �����.");
            // ���� ���� ��� � ����� ����������� �������� ����� ��� ������� ����������
        }

        // ����������� ���� ����� ������
        if (rangedAttackAudioSource != null && arrowFlightClip != null)
            rangedAttackAudioSource.PlayOneShot(arrowFlightClip);
    }
    /// <summary>
    /// �������� ������������ �����, ������� ���� ��� ���������.
    /// </summary>
    private IEnumerator HighlightEnemy(Enemy enemy)
    {
        Renderer enemyRenderer = enemy.GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            // ������ ������� ���������, ����� �� ������ �������� (���� �� ������, Unity ������������� ���������)
            Material mat = enemyRenderer.material;
            Color originalColor = mat.color;
            mat.color = Color.yellow; // ���� ���������
            yield return new WaitForSeconds(0.2f);
            mat.color = originalColor;
        }
    }
    // ����� �������� ���
    private void MeleeAttack()
    {
        Debug.Log("Ближняя атака: Начало атаки");
        // Воспроизведение звука взмаха меча
        if (meleeAttackAudioSource != null && swordSwingClip != null)
            meleeAttackAudioSource.PlayOneShot(swordSwingClip);

        float maxDistance = currentPlayerData.attackRange;
        Ray attackRay = GetAttackRayFromPlayer();

        Debug.DrawLine(attackRay.origin, attackRay.origin + attackRay.direction * maxDistance, Color.green, 2f);
        RaycastHit[] hits = Physics.SphereCastAll(attackRay, sphereCastRadius, maxDistance);

        Enemy targetEnemy = null;
        float minDistanceAlongRay = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector3 toEnemy = enemy.transform.position - attackRay.origin;
                float t = Vector3.Dot(toEnemy, attackRay.direction);

                if (t < 0 || t > maxDistance)
                    continue;

                float effectiveRadius = Mathf.Lerp(sphereCastRadius, sphereCastEndRadius, t / maxDistance);
                Vector3 pointOnRay = attackRay.origin + attackRay.direction * t;
                float distToRay = Vector3.Distance(enemy.transform.position, pointOnRay);

                if (distToRay <= effectiveRadius && t < minDistanceAlongRay)
                {
                    minDistanceAlongRay = t;
                    targetEnemy = enemy;
                }
            }
        }

        currentTarget = targetEnemy; // Сохраняем текущую цель

        if (targetEnemy != null)
        {
            Debug.Log($"Ближняя атака: Наносим урон {currentPlayerData.attack} врагу {targetEnemy.name}");
            Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);

            Debug.Log($"Ближняя атака: Наносим урон {currentPlayerData.attack} врагу {targetEnemy.name}");
            targetEnemy.TakeDamage(currentPlayerData.attack);
            Debug.Log($"Ближняя атака: Урон нанесен");

            if (meleeHitVFXPrefab != null)
                Instantiate(meleeHitVFXPrefab, targetEnemy.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("    melee-.");
        }
    }

    private Ray GetAttackRayFromPlayer()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Vector3 origin = transform.position + Vector3.up * sphereCastCenterYOffset;
        Plane playerPlane = new Plane(Vector3.up, origin);
        float distance;
        Vector3 attackDirection;
        if (playerPlane.Raycast(cameraRay, out distance))
        {
            Vector3 hitPoint = cameraRay.GetPoint(distance);
            attackDirection = (hitPoint - origin).normalized;
            if (Vector3.Dot(attackDirection, transform.forward) < 0)
                attackDirection = transform.forward;
        }
        else
        {
            attackDirection = transform.forward;
        }
        return new Ray(origin, attackDirection);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && Camera.main != null && currentPlayerData != null)
        {
            float maxDistance = currentPlayerData.attackRange;

            //    ( )
            Ray cameraRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(cameraRay.origin, cameraRay.origin + cameraRay.direction * maxDistance);
            Gizmos.DrawWireSphere(cameraRay.origin, 0.1f); //    

            //   melee-   ( )
            Ray attackRay = GetAttackRayFromPlayer();
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackRay.origin, sphereCastRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(attackRay.origin, attackRay.origin + attackRay.direction * maxDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackRay.origin + attackRay.direction * maxDistance, sphereCastEndRadius);
        }
    }

    public int GetAttackDamage()
    {
        return currentPlayerData.attack;
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
            Debug.Log($"Инициализирован игрок {playerIndex} из конфига. " + 
                     $"Базовые характеристики: Сила {currentPlayerData.power}, Ловкость {currentPlayerData.dexterity}, " +
                     $"Выносливость {currentPlayerData.stamina}, Защита {currentPlayerData.defence}, Интеллект {currentPlayerData.intellect}");
        }
        else
        {
            Debug.LogError($"Нет игрока с индексом {playerIndex} в конфиге!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentPlayerData == null) return;

        // Устанавливаем состояние боя
        isInCombat = true;
        combatTimer = combatCooldown;

        // Расчет текущей защиты
        int currentDefense = PlayerPrefs.GetInt(DefenceKey, currentPlayerData.defense);
        float defenseReduction = Mathf.Clamp(currentDefense / 2000f, 0f, 1f);
        int actualDamage = Mathf.RoundToInt(damage * (1f - defenseReduction));

        healthSystem.TakeDamage(actualDamage);
        UpdateHealthDisplay();

        if (damageImage != null)
        {
            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);
            damageCoroutine = StartCoroutine(ShowDamageEffect());
        }
    }

    private IEnumerator ShowDamageEffect()
    {
        float elapsed = 0f;
        damageImage.color = damageColor;
        while (elapsed < damageFlashDuration)
        {
            damageImage.color = Color.Lerp(damageColor, Color.clear, elapsed / damageFlashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        damageImage.color = Color.clear;
    }
    #region Health Restore
    public void RestoreHealthByPercentage(float percentage)
    {
        int healthToRestore = Mathf.RoundToInt(healthSystem.GetMaxHealth() * percentage);
        healthSystem.Heal(healthToRestore);
        UpdateHealthDisplay();
        Debug.Log($"Восстановлено {healthToRestore} здоровья ({percentage * 100}% от максимального). Текущее здоровье: {healthSystem.GetCurrentHealth()}");
    }
    public void IncreaseMaxHealthByPercentage(float percentage)
    {
        int buffAmount = Mathf.RoundToInt(originalHealthStat * percentage);
        int newMaxHealth = originalHealthStat + buffAmount;
        
        healthSystem.SetMaxHealth(newMaxHealth);
        healthSystem.Heal(buffAmount);
        UpdateHealthDisplay();
        
        Debug.Log($"Увеличено максимальное здоровье на {percentage * 100}% ({buffAmount} ед.). Новое максимальное здоровье: {healthSystem.GetMaxHealth()}");
        
        StartCoroutine(RemoveMaxHealthBuffAfterTime(buffAmount, 60f));
    }
    private IEnumerator RemoveMaxHealthBuffAfterTime(int buffAmount, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        healthSystem.SetMaxHealth(originalHealthStat);
        if (healthSystem.GetCurrentHealth() > healthSystem.GetMaxHealth())
            healthSystem.SetCurrentHealth(healthSystem.GetMaxHealth());
        
        UpdateHealthDisplay();
        Debug.Log($"Бафф здоровья снят: -{buffAmount} ед. Новое максимальное здоровье: {healthSystem.GetMaxHealth()}");
    }

    //       
    public void RestoreHealthFixed(int healthPoints)
    {
        if (healthSystem.GetCurrentHealth() >= healthSystem.GetMaxHealth()) return;
        
        healthSystem.Heal(healthPoints);
        UpdateHealthDisplay();
        Debug.Log($"Восстановлено {healthPoints} здоровья. Текущее здоровье: {healthSystem.GetCurrentHealth()}");
    }

    //    
    private IEnumerator RegenerationCoroutine()
    {
        float regenTimer = 0f;

        while (true)
        {
            if (!isInCombat && healthSystem.GetCurrentHealth() < healthSystem.GetMaxHealth())
            {
                regenTimer += 1f;
                int interval = (int)(regenTimer / 5f);
                int restoreAmount = Mathf.RoundToInt(healthSystem.GetMaxHealth() * currentPlayerData.regeneration * Mathf.Pow(2, interval));
                
                healthSystem.Heal(restoreAmount);
                UpdateHealthDisplay();
                Debug.Log($"Регенерация здоровья: +{restoreAmount}. Текущее здоровье: {healthSystem.GetCurrentHealth()}/{healthSystem.GetMaxHealth()}");
            }
            else
            {
                regenTimer = 0f;
                if (isInCombat)
                {
                    Debug.Log($"Регенерация остановлена - игрок в бою. Таймер боя: {combatTimer:F1}");
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateHealthDisplay()
    {
        healthBar.fillAmount = healthSystem.GetHealthPercentage();
        hpText.text = $"{healthSystem.GetCurrentHealth()} / {healthSystem.GetMaxHealth()}";
        Debug.Log($"Обновление интерфейса здоровья: {healthSystem.GetCurrentHealth()} / {healthSystem.GetMaxHealth()}");
    }
    #endregion

    #region UpdateStats
    public void UpdateMaxHealth(int currentStamina)
    {
        maxHealth = currentStamina * 10;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentPlayerData.health = maxHealth;
        currentPlayerData.stamina = currentStamina;  // Сохраняем значение выносливости в ScriptableObject

        UpdateHealthDisplay();
        
        PlayerPrefs.SetInt(BaseHealthKey, maxHealth);
        SaveCharacterStats();  // Сохраняем все характеристики, включая stamina
        Debug.Log($"Обновлено здоровье: {maxHealth} (Выносливость: {currentStamina})");
    }

    public void UpdateAttack(int currentPower)
    {
        int newAttack = currentPower * 2;
        currentPlayerData.attack = newAttack;
        currentPlayerData.power = currentPower;  // Сохраняем значение силы в ScriptableObject

        PlayerPrefs.SetInt(BaseDamageKey, newAttack);
        SaveCharacterStats();  // Сохраняем все характеристики, включая power
        Debug.Log($"Обновлено значение атаки: {newAttack} (Сила: {currentPower})");
    }

    public void UpdateMoveSpeed(int currentDextery)
    {
        float newSpeed = currentDextery / 3f;
        currentPlayerData.moveSpeed = newSpeed;
        currentPlayerData.dexterity = currentDextery;  // Сохраняем значение ловкости в ScriptableObject

        PlayerPrefs.SetFloat(BaseMoveSpeed, newSpeed);
        SaveCharacterStats();  // Сохраняем все характеристики, включая dexterity
        Debug.Log($"Обновлена скорость: {newSpeed} (Ловкость: {currentDextery})");
    }

    public void UpdateDefense(int additionalDefenseUnits)
    {
        int newDevense = additionalDefenseUnits * 6;       
        currentPlayerData.defense = newDevense;
        currentPlayerData.defence = additionalDefenseUnits;  // Сохраняем значение защиты в ScriptableObject

        PlayerPrefs.SetInt(BaseDefense, newDevense);
        SaveCharacterStats();  // Сохраняем все характеристики, включая defence
        Debug.Log($"Обновлена защита: {newDevense} )");
    }

    public void UpdateIntellect(int additionalIntellect)
    {
        int newIntellect = additionalIntellect;
        currentPlayerData.intellect = newIntellect;

        PlayerPrefs.SetInt(BaseIntellectKey, newIntellect);
        SaveCharacterStats();  // Сохраняем все характеристики, включая intellect
        Debug.Log($"Обновлен интеллект: {newIntellect} )");
    }
    #endregion

    // Новый метод для сохранения базовых характеристик персонажа в PlayerPrefs
    public void SaveCharacterStats()
    {
        if (currentPlayerData == null) return;
        
        PlayerPrefs.SetInt("PlayerPower", currentPlayerData.power);
        PlayerPrefs.SetInt("PlayerDextery", currentPlayerData.dexterity);
        PlayerPrefs.SetInt("PlayerStamina", currentPlayerData.stamina);
        PlayerPrefs.SetInt("PlayerDefence", currentPlayerData.defence);
        PlayerPrefs.SetInt("PlayerIntellect", currentPlayerData.intellect);
        
        // Сохраняем и производные характеристики
        PlayerPrefs.SetInt(BaseDamageKey, currentPlayerData.attack);
        PlayerPrefs.SetFloat(BaseMoveSpeed, currentPlayerData.moveSpeed);
        PlayerPrefs.SetInt(BaseHealthKey, currentPlayerData.health);
        PlayerPrefs.SetInt(BaseDefense, currentPlayerData.defense);
        PlayerPrefs.SetInt(BaseIntellectKey, currentPlayerData.intellect);
        
        PlayerPrefs.Save();
        Debug.Log("Все характеристики персонажа сохранены в PlayerPrefs");
    }
    
    // Новый метод для загрузки базовых характеристик персонажа из PlayerPrefs
    public void LoadCharacterStats()
    {
        if (currentPlayerData == null) return;
        
        // Проверяем, есть ли сохраненные данные в PlayerPrefs
        bool hasSavedData = PlayerPrefs.HasKey("PlayerPower");
        
        if (hasSavedData)
        {
            // Загружаем базовые характеристики из PlayerPrefs
            currentPlayerData.power = PlayerPrefs.GetInt("PlayerPower", currentPlayerData.power);
            currentPlayerData.dexterity = PlayerPrefs.GetInt("PlayerDextery", currentPlayerData.dexterity);
            currentPlayerData.stamina = PlayerPrefs.GetInt("PlayerStamina", currentPlayerData.stamina);
            currentPlayerData.defence = PlayerPrefs.GetInt("PlayerDefence", currentPlayerData.defence);
            currentPlayerData.intellect = PlayerPrefs.GetInt("PlayerIntellect", currentPlayerData.intellect);
            Debug.Log("Загружены сохраненные данные из PlayerPrefs.");
        }
        else
        {
            // Если данных нет, сохраняем исходные значения из ScriptableObject в PlayerPrefs
            SaveCharacterStats();
            Debug.Log("Исходные значения характеристик из ScriptableObject сохранены в PlayerPrefs");
        }
        
        // Пересчитываем производные характеристики на основе базовых
        currentPlayerData.attack = currentPlayerData.power * 2; // Пересчитываем из силы
        currentPlayerData.moveSpeed = currentPlayerData.dexterity / 3f; // Пересчитываем из ловкости
        currentPlayerData.health = currentPlayerData.stamina * 10; // Пересчитываем из выносливости
        currentPlayerData.defense = currentPlayerData.defence * 6; // Пересчитываем из защиты

        // Сохраняем производные характеристики в PlayerPrefs
        PlayerPrefs.SetInt(BaseHealthKey, currentPlayerData.health);
        PlayerPrefs.SetInt(BaseDefense, currentPlayerData.defense);
        PlayerPrefs.SetFloat(BaseMoveSpeed, currentPlayerData.moveSpeed);
        PlayerPrefs.SetInt(BaseDamageKey, currentPlayerData.attack);
        PlayerPrefs.SetInt(BaseIntellectKey, currentPlayerData.intellect);
        PlayerPrefs.Save();
        
        // Обновляем здоровье
        maxHealth = currentPlayerData.health;
        currentHealth = maxHealth;
        UpdateHealthDisplay();
        
        Debug.Log($"Загружены характеристики: Сила {currentPlayerData.power}, Ловкость {currentPlayerData.dexterity}, " +
                  $"Выносливость {currentPlayerData.stamina}, Защита {currentPlayerData.defence}, Интеллект {currentPlayerData.intellect}");
        Debug.Log($"Пересчитаны производные: Атака {currentPlayerData.attack}, Здоровье {currentPlayerData.health}, " +
                  $"Скорость {currentPlayerData.moveSpeed}, Защита {currentPlayerData.defense}");
    }

    #region DieAndRespawn
    private IEnumerator SnapshotCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);
            Debug.Log($"Создание снапшота в {Time.time}");
            RecordSnapshot();            
        }
    }

    private void RecordSnapshot()
    {
        PlayerSnapshot snapshot = new PlayerSnapshot();
        snapshot.timeStamp = Time.time;
        snapshot.position = transform.position;
        snapshot.rotation = transform.rotation;
        snapshot.currentHealth = currentHealth;

        if (expBar != null)
        {
            snapshot.currentExp = expBar.GetCurrentExp();
            snapshot.currentLevel = expBar.GetCurrentLevel();
            snapshot.statPoints = expBar.GetStatPoints();
            snapshot.expForLevelUp = expBar.GetExpForLevelUp();
        }
        saveSystem.RecordSnapshot(snapshot);
        Debug.Log($"Создан снапшот: позиция {snapshot.position}, здоровье {snapshot.currentHealth}");
    }
    private IEnumerator DelayedRestore()
    {
        // ��� �������, ����� ��� ������� ������ ������������������
        yield return new WaitForSeconds(0.1f);

        if (PlayerPrefs.GetInt("ContinueGame", 0) == 1)
        {
            if (PlayerSaveSystem.HasSnapshots())
            {
                PlayerSnapshot snapshot = saveSystem.GetLatestSnapshot();
                if (snapshot != null)
                {
                    transform.position = snapshot.position;
                    transform.rotation = snapshot.rotation;
                    currentHealth = snapshot.currentHealth;
                    UpdateHealthDisplay();

                    if (expBar != null)
                    {
                        expBar.SetExperience(snapshot.currentExp, snapshot.currentLevel, snapshot.statPoints, snapshot.expForLevelUp);
                    }
                }
            }
            PlayerPrefs.SetInt("ContinueGame", 0);
            PlayerPrefs.Save();
        }
    }   

    private void Die()
    {
        // Получаем снапшот 30 секунд назад
        PlayerSnapshot snapshot = saveSystem.GetSnapshotForRewind(30f);
        if (snapshot != null)
        {
            Debug.Log($"Найден снапшот для отката. Позиция: {snapshot.position}, Здоровье: {snapshot.currentHealth}");
            StartCoroutine(Respawn(snapshot));
        }
        else
        {
            Debug.LogWarning("Не найден снапшот для отката!");
            // Если снапшота нет, восстанавливаем максимальное здоровье
            currentHealth = maxHealth;
            UpdateHealthDisplay();
        }
    }

    private IEnumerator Respawn(PlayerSnapshot snapshot)
    {
        // Задержка перед восстановлением (например, 1 секунда)
        yield return new WaitForSeconds(1f);

        // Восстанавливаем позицию, поворот и здоровье
        transform.position = snapshot.position;
        transform.rotation = snapshot.rotation;
        
        // Восстанавливаем здоровье через HealthSystem
        if (healthSystem != null)
        {
            healthSystem.SetCurrentHealth(snapshot.currentHealth);
            currentHealth = snapshot.currentHealth;
            UpdateHealthDisplay();
        }
        else
        {
            currentHealth = snapshot.currentHealth;
            UpdateHealthDisplay();
        }

        // Штрафной опыт: теряется 10% от накопленного опыта,
        // но текущий опыт не может упасть ниже 0 для текущего уровня
        if (expBar != null)
        {
            int xpLoss = Mathf.CeilToInt(snapshot.currentExp * 0.1f);
            
            // Ограничиваем потерю опыта так, чтобы он не упал ниже 0 для текущего уровня
            // 0 для текущего уровня - это значит, что опыт = 0, но уровень не меняется
            int minExp = 0; // Минимальный опыт для текущего уровня
            int newExp = Mathf.Max(minExp, snapshot.currentExp - xpLoss);
            
            Debug.Log($"Потеря опыта при смерти: -{xpLoss} XP. Новый опыт: {newExp}/{snapshot.expForLevelUp}");
            
            // Устанавливаем опыт, сохраняя текущий уровень, стат-поинты и порог опыта
            expBar.SetExperience(newExp, snapshot.currentLevel, snapshot.statPoints, snapshot.expForLevelUp);
        }

        isInCombat = false;
        Debug.Log($"Восстановлен игрок из снапшота с потерей 10% опыта. Здоровье: {currentHealth}/{maxHealth}");
    }

    #endregion
    public void SwitchPlayer(int newIndex)
    {
        if (playerConfig.players.Count > newIndex && newIndex >= 0)
        {
            playerIndex = newIndex;
            InitializePlayerData();
            ReloadPlayerStats();
        }
    }

    private void ReloadPlayerStats()
    {
        maxHealth = currentPlayerData.health;
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }
    public void TameMonster()
    {
        CameraTargetSelector selector = Camera.main.GetComponent<CameraTargetSelector>();
        if (selector != null && selector.CurrentTarget != null)
        {
            Enemy targetEnemy = selector.CurrentTarget;
            int playerIntellect = currentPlayerData.intellect;
            // ������� ������: ��������� 10 ���� 20% �����, �������:
            float chancePercent = (playerIntellect / 50.0f) * 100f;
            int roll = Random.Range(0, 101); // ���������� ����� �� 0 �� 100 ������������

            Debug.Log($"����: {chancePercent}%, ������: {roll}");

            if (roll <= chancePercent)
            {
                float duration = playerIntellect; // ������������ ����������, ��������, ����� ����������
                targetEnemy.Tame(duration);
                OnEnemyTame?.Invoke(1);
                Debug.Log($"�������� ������ {targetEnemy.name} �� {duration} ������.");
            }
            else
            {
                OnEnemyTame?.Invoke(0);
                Debug.Log("�� ������� ��������� �������.");
            }
        }
        else
        {
            Debug.Log("��� ������� ������ ��� ���� ��� ����������.");
        }
    }

    private void CheckForNearbyCompanions()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, companionDetectionRadius);
        foreach (var collider in colliders)
        {
            CompanionNPC companion = collider.GetComponent<CompanionNPC>();
            if (companion != null && !companions.Contains(companion))
            {
                companion.JoinGroup();
                companions.Add(companion);
            }
        }
    }

    // Добавляем метод для сброса текущей цели
    private void ResetCurrentTarget()
    {
        currentTarget = null;
        // Оповещаем компаньонов о сбросе цели
        foreach (var companion in companions)
        {
            companion.SetTarget(null);
        }
    }

    public void RemoveCompanion(CompanionNPC companion)
    {
        if (companions.Contains(companion))
        {
            companions.Remove(companion);
            Debug.Log($"Компаньон {companion.GetNPCName()} удален из группы игрока");
        }
    }

    public List<CompanionNPC> GetCompanions()
    {
        return companions;
    }

    private void OnDestroy()
    {
        // Отписываемся от события смерти
        if (healthSystem != null)
        {
            healthSystem.OnDeath.RemoveListener(Die);
        }
    }
}
