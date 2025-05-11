using UnityEngine;
using System.Linq;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Main Info")]
    public string enemyName;
    public EnemyConfig enemyConfig;         
    public int prefabIndex;
    public PlayerAnimationController animationController;

    private EnemyData _enemyData;
    private EnemyPool _enemyPool;
    private int _currentHealth;
    private int _maxHealth;
    private Transform playerTransform;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private float waitDuration = 0f;

    private Vector3 _spawnPosition; 
    [SerializeField] private float patrolRadius = 10f;
    private bool isMovingToRandomPoint;
    private Vector3 randomPoint;

    private bool hasForcedDestination = false;
    private Vector3 forcedDestination;

    public delegate void DeathAction(int exp);
    public static event DeathAction OnEnemyDeath;

    public delegate void DeathWithPositionAction(Vector3 playerPosition);
    public static event DeathWithPositionAction OnEnemyDeathWithPosition;

    public delegate void DAction();
    public static event DAction OnEnemyDestroy;

    public delegate void EnemyKilledAction(string enemyName);
    public static event EnemyKilledAction OnEnemyKilled;
    public Enemy OriginalPrefab { get; set; }
    public bool isTamed = false;
    private bool isDead = false;

    [Header("Health System")]
    [SerializeField] private HealthSystem healthSystem;

    #region Инициализация

    void Start()
    {
        // Определяем имя врага по имени объекта (убираем "(Clone)")
        enemyName = gameObject.name.Replace("(Clone)", "").Trim();
        
        // Проверяем наличие конфига
        if (enemyConfig == null)
        {
            return;
        }
        
        _enemyData = enemyConfig.enemies.FirstOrDefault(e => e.enemyName == enemyName);
        if (_enemyData == null)
        {
            return;
        }

        // Проверка и инициализация HealthSystem
        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = gameObject.AddComponent<HealthSystem>();
            }
        }

        // Настраиваем HealthSystem
        var healthSystemComponent = healthSystem as HealthSystem;
        if (healthSystemComponent != null)
        {
            // Устанавливаем тип существа
            healthSystemComponent.SetEntityType(HealthSystem.EntityType.Enemy);
            // Передаем конфиг
            healthSystemComponent.SetEnemyConfig(enemyConfig);
            // Обновляем здоровье из конфига
            healthSystemComponent.RefreshFromConfig();
        }

        // Подписываемся на событие смерти
        if (healthSystem != null)
        {
            healthSystem.OnDeath.AddListener(OnHealthSystemDeath);
        }

        // Находим игрока на сцене
        Player player = FindObjectOfType<Player>();
        if (player != null)
            playerTransform = player.transform;      

        // Находим пул врагов
        if (_enemyPool == null)
        {
            _enemyPool = FindObjectOfType<EnemyPool>();           
        }

        // Сохраняем исходную позицию для патрулирования
        _spawnPosition = transform.position;
        
        // Инициализируем контроллер анимации
        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }
    }
    #endregion
    private float attackTimer = 0f;
    void Update()
    {
        // Если враг мертв, пропускаем обновление
        if (isDead)
        {
            return;
        }
        
        // Обновление таймера атаки
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        // Проверка на наличие необходимых компонентов
        if (_enemyData == null)
        {
            // Если данные еще не инициализированы, пытаемся сделать это
            if (enemyConfig != null)
            {
                _enemyData = enemyConfig.enemies.FirstOrDefault(e => e.enemyName == enemyName);
                if (_enemyData == null)
                {
                    // Если не удалось найти данные, логируем ошибку и пропускаем обновление
                    return;
                }
            }
            else
            {
                // Если конфига нет, пропускаем обновление
                return;
            }
        }

        // Если игрок не найден, ищем его
        if (playerTransform == null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
                playerTransform = player.transform;
            else
                return; // Если игрока нет, пропускаем обновление
        }
        
        // Если враг приручён, выполняем только союзное поведение
        if (isTamed)
        {
            ExecuteTamedBehavior();
            return;
        }

        // --- Обычная логика поведения врага (для не приручённых) ---
        Vector3 target = Vector3.zero;
        bool isAttacking = false;

        if (hasForcedDestination)
        {
            target = new Vector3(forcedDestination.x, transform.position.y, forcedDestination.z);
            if (Vector3.Distance(transform.position, target) < 0.5f)
                hasForcedDestination = false;
        }
        else
        {
            Vector3 playerPosFlat = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            float distanceToPlayer = Vector3.Distance(transform.position, playerPosFlat);

            if (distanceToPlayer <= _enemyData.detectionRange)
            {
                target = playerPosFlat;
                if (distanceToPlayer <= _enemyData.attackRange)
                    isAttacking = true;
            }
            else
            {
                if (!isMovingToRandomPoint && !isWaiting)
                {
                    randomPoint = _spawnPosition + new Vector3(Random.Range(-patrolRadius, patrolRadius), 0, Random.Range(-patrolRadius, patrolRadius));
                    isMovingToRandomPoint = true;
                }
                target = new Vector3(randomPoint.x, transform.position.y, randomPoint.z);
                if (isMovingToRandomPoint && Vector3.Distance(transform.position, target) < 0.5f)
                {
                    isMovingToRandomPoint = false;
                    isWaiting = true;
                    waitDuration = Random.Range(5f, 10f);
                    waitTimer = 0f;
                }
            }
        }

        if (!isAttacking)
        {
            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (animationController != null)
                {
                    animationController.PlayAnimation(animationController.animationStates.Idle);
                }
                if (waitTimer >= waitDuration)
                {
                    isWaiting = false;
                }
                return;
            }

            float step = _enemyData.moveSpeed * Time.deltaTime;
            Vector3 newPos = Vector3.MoveTowards(transform.position, target, step);

            // Проверяем землю под врагом
            int groundLayer = LayerMask.GetMask("Ground"); // Предполагаем, что земля находится на слое "Ground"
            RaycastHit hit;
            if (Physics.Raycast(newPos + Vector3.up * 2f, Vector3.down, out hit, 5f, groundLayer))
            {
                float heightSmoothFactor = 2f; // Уменьшаем фактор для более плавной корректировки
                newPos.y = Mathf.Lerp(transform.position.y, hit.point.y, heightSmoothFactor * Time.deltaTime);
            }

            Vector3 movementDirection = (newPos - transform.position).normalized;
            if (movementDirection != Vector3.zero)
            {
                float correctionAngle = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection) * Quaternion.Euler(0, correctionAngle, 0);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _enemyData.rotationSpeed * Time.deltaTime);
            }

            transform.position = newPos;
            if (animationController != null)
            {
                animationController.PlayAnimation(animationController.animationStates.WalkForward);
            }
        }
        else
        {
            Vector3 attackDirection = (target - transform.position).normalized;
            if (attackDirection != Vector3.zero)
            {
                float correctionAngle = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(attackDirection) * Quaternion.Euler(0, correctionAngle, 0);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _enemyData.rotationSpeed * Time.deltaTime);
            }

            if (attackTimer <= 0f)
            {
                if (animationController != null)
                {
                    animationController.PlayAnimation(animationController.animationStates.Attack1);
                }
                ApplyDamage();
                attackTimer = _enemyData.attackCouldown;
            }
        }
    }
    private void ExecuteTamedBehavior()
    {
        GameObject[] enemyTargets = GameObject.FindGameObjectsWithTag("Enemy");
        Enemy tameTarget = null;
        float minDist = Mathf.Infinity;
        foreach (GameObject go in enemyTargets)
        {
            if (go == this.gameObject || go.CompareTag("Player") || go.CompareTag("Ally"))
                continue;

            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < minDist)
            {
                minDist = d;
                tameTarget = go.GetComponent<Enemy>();
            }
        }

        if (tameTarget != null)
        {
            Vector3 direction = (tameTarget.transform.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            if (Vector3.Distance(transform.position, tameTarget.transform.position) > _enemyData.attackRange)
            {
                float step = _enemyData.moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, tameTarget.transform.position, step);
                animationController.PlayAnimation(animationController.animationStates.WalkForward);
            }
            else
            {
                if (attackTimer <= 0f)
                {
                    animationController.PlayAnimation(animationController.animationStates.Attack1);
                    tameTarget.TakeDamage(_enemyData.damage);
                    attackTimer = _enemyData.attackCouldown;
                }
            }
        }
        else
        {
            animationController.PlayAnimation(animationController.animationStates.Idle);
        }
    }

    // Метод получения урона
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        if (_enemyData == null)
        {
            return;
        }

        if (healthSystem == null)
        {
            return;
        }
        
        if (damage < 0)
        {
            damage = 0;
        }

        healthSystem.TakeDamage(damage);
    }

    private void OnHealthSystemDeath()
    {
        if (isDead) return;
        isDead = true;

        // Отключаем коллайдеры
        var colliders = GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // Настраиваем Rigidbody для эффекта падения
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Проигрываем анимацию смерти
        if (animationController != null)
        {
            animationController.PlayAnimation(animationController.animationStates.Die);
        }

        // Вызываем события смерти
        if (OnEnemyDeath != null)
        {
            OnEnemyDeath(_enemyData.experience);
        }

        if (OnEnemyDeathWithPosition != null)
        {
            OnEnemyDeathWithPosition(transform.position);
        }

        if (OnEnemyDestroy != null)
        {
            OnEnemyDestroy();
        }

        if (OnEnemyKilled != null)
        {
            OnEnemyKilled(enemyName);
        }

        // Возвращаем врага в пул через некоторое время
        StartCoroutine(ReturnToPoolAfterDelay(3f));
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (_enemyPool != null)
        {
            _enemyPool.ReturnEnemy(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        OnEnemyDeathWithPosition += OnDeathWithPosition;
        _spawnPosition = transform.position;
        
        // Дополнительная проверка и инициализация при активации
        if (_enemyPool == null)
        {
            _enemyPool = FindObjectOfType<EnemyPool>();
        }
        
        if (playerTransform == null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
                playerTransform = player.transform;
        }
        
        // Проверяем и инициализируем контроллер анимации
        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }

        // Проверяем подписку на событие смерти
        if (healthSystem != null && !healthSystem.OnDeath.GetPersistentEventCount().Equals(1))
        {
            healthSystem.OnDeath.RemoveListener(OnHealthSystemDeath);
            healthSystem.OnDeath.AddListener(OnHealthSystemDeath);
        }
    }

    private void OnDisable()
    {
        OnEnemyDeathWithPosition -= OnDeathWithPosition;
        
        // Отписываемся от события смерти
        if (healthSystem != null)
        {
            healthSystem.OnDeath.RemoveListener(OnHealthSystemDeath);
        }
    }
      
    public static void OnDeathWithPosition(Vector3 playerPosition)
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(playerPosition, 30f);
        foreach (Collider enemyCollider in nearbyEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.forcedDestination = playerPosition;
                enemy.hasForcedDestination = true;
            }
        }
    }
    public void ApplyDamage()
    {
        // Проверяем, есть ли кто-то в радиусе атаки
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _enemyData.attackRange);
        foreach (var hitCollider in hitColliders)
        {
            // Проверяем игрока
            Player player = hitCollider.GetComponent<Player>();
            if (player != null)
            {
                // Поворачиваемся к игроку
                Vector3 direction = (player.transform.position - transform.position).normalized;
                direction.y = 0; // Игнорируем вертикальную составляющую
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
                
                player.TakeDamage(_enemyData.damage);
                return; // Если нашли игрока, наносим урон только ему
            }

            // Проверяем компаньона
            CompanionNPC companion = hitCollider.GetComponent<CompanionNPC>();
            if (companion != null)
            {
                // Поворачиваемся к компаньону
                Vector3 direction = (companion.transform.position - transform.position).normalized;
                direction.y = 0; // Игнорируем вертикальную составляющую
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
                
                companion.TakeDamage(_enemyData.damage);
                return; // Если нашли компаньона, наносим урон только ему
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        
        Vector3 spawnPos = Application.isPlaying ? _spawnPosition : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPos, patrolRadius);
    }
    public void Tame(float duration)
    {
        if (isTamed) return; 
        isTamed = true;

        gameObject.tag = "Ally";
        StartCoroutine(UntameAfterDuration(duration));
    }

    private IEnumerator UntameAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        isTamed = false;

        gameObject.tag = "Enemy";
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath.RemoveListener(OnHealthSystemDeath);
        }
        OnEnemyDeathWithPosition -= OnDeathWithPosition;
        StopAllCoroutines();
    }

    // При выключении игры или сцены - обрабатываем уничтожение объекта
    private void OnApplicationQuit()
    {
        // Сбрасываем пул, чтобы избежать обращения к нему после выключения приложения
        _enemyPool = null;
    }
}
