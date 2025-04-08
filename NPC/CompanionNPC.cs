using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class CompanionNPC : MonoBehaviour
{
    [Header("Конфигурация")]
    [SerializeField] private NPCConfig npcConfig;
    [SerializeField] public string npcID; // Делаем поле публичным
    private NPCData _npcData; // Кэшированные данные НПС
    
    [Header("Компоненты")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform attackPoint;
    
    [Header("UI")]
    [SerializeField] private GameObject joinPrompt; // UI подсказка для присоединения
    [SerializeField] private Text nameText; // Текст для отображения имени НПС
    [SerializeField] private GameObject companionUIPrefab;
    [SerializeField] private Transform companionUIContainer; // Родительский объект для UI элементов

    // События для обновления UI
    public UnityEvent<int> OnHealthChanged = new UnityEvent<int>();
    public UnityEvent<CompanionNPC> OnJoinedGroup = new UnityEvent<CompanionNPC>();

    private Player player;
    private bool isInGroup = false;
    private int currentHealth;
    private float attackTimer = 0f;
    private Enemy currentTarget;
    private bool isAttacking = false;
    private bool isPlayerNearby = false;

    private CompanionUI companionUI;

    [Header("Настройки движения")]
    [SerializeField] private float smoothRotationSpeed = 5f;
    [SerializeField] private float smoothMovementSpeed = 5f;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckDistance = 1.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float heightSmoothFactor = 5f;
    [SerializeField] private float minFollowDistance = 2f;
    [SerializeField] private float maxFollowDistance = 4f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float deceleration = 4f;
    [SerializeField] private float rayLength = 10f; // Добавляем длину луча для проверки высоты

    private Vector3 currentVelocity;
    private float currentSpeed;
    private float targetSpeed;
    private bool isGrounded;
    private Vector3 lastValidPosition;

    private void Start()
    {
        if (npcConfig == null)
        {
            Debug.LogError($"NPCConfig не назначен для НПС {gameObject.name}!");
            return;
        }

        // Получаем данные НПС из конфига по ID
        _npcData = npcConfig.GetNPCDataByID(npcID);
        
        if (_npcData == null)
        {
            Debug.LogError($"НПС с ID {npcID} не найден в конфиге!");
            return;
        }

        // Инициализация
        currentHealth = _npcData.maxHealth;
        player = FindObjectOfType<Player>();
        
        // Отображение имени НПС
        if (nameText != null)
        {
            nameText.text = _npcData.npcName;
        }
        
        if (joinPrompt != null)
            joinPrompt.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F) && !isInGroup)
        {
            JoinGroup();
        }

        if (!isInGroup || player == null) return;

        // Сначала следуем за игроком
        FollowPlayer();

        // Ищем ближайшего врага в радиусе обнаружения
        Enemy nearestEnemy = FindNearestEnemyInRange();
        
        // Если есть враг в радиусе обнаружения, атакуем его
        if (nearestEnemy != null)
        {
            HandleCombat(nearestEnemy);
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    private Enemy FindNearestEnemyInRange()
    {
        Enemy nearestEnemy = null;
        float minDistance = float.MaxValue;

        // Находим всех врагов
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemyObj in enemies)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null && !enemy.isTamed) // Проверяем, что враг не приручен
            {
                float distance = Vector3.Distance(transform.position, enemyObj.transform.position);
                // Проверяем, находится ли враг в радиусе обнаружения
                if (distance <= _npcData.detectionRange && distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }

        return nearestEnemy;
    }

    private void HandleCombat(Enemy target)
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        
        if (distanceToTarget <= _npcData.attackRange)
        {
            // Поворот к цели
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _npcData.rotationSpeed * Time.deltaTime);

            // Атака
            if (attackTimer <= 0)
            {
                Attack(target);
            }
        }
    }

    private void Attack(Enemy target)
    {
        if (target == null) return;

        attackTimer = _npcData.attackCooldown;
        isAttacking = true;

        // Воспроизведение анимации атаки
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Визуальные эффекты
        if (_npcData.attackVFXPrefab != null && attackPoint != null)
        {
            Instantiate(_npcData.attackVFXPrefab, attackPoint.position, attackPoint.rotation);
        }

        // Звук атаки
        if (audioSource != null && _npcData.attackSound != null)
        {
            audioSource.PlayOneShot(_npcData.attackSound);
        }

        // Нанесение урона
        target.TakeDamage(_npcData.attackDamage);
    }

    private void FollowPlayer()
    {
        if (player == null) return;

        // Вычисляем целевую позицию позади игрока
        Vector3 targetPosition = player.transform.position - player.transform.forward * _npcData.followDistance;
        
        // Сохраняем текущую позицию для расчетов
        Vector3 currentPos = transform.position;
        
        // Вычисляем плоское направление движения (без учета Y)
        Vector3 targetPosFlat = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);
        Vector3 direction = (targetPosFlat - currentPos).normalized;
        
        // Рассчитываем дистанцию до цели
        float distanceToTarget = Vector3.Distance(currentPos, targetPosFlat);
        
        // Плавное изменение скорости
        targetSpeed = Mathf.Clamp(distanceToTarget, minFollowDistance, maxFollowDistance) / maxFollowDistance * _npcData.moveSpeed;
        
        if (distanceToTarget > 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }
        
        // Вычисляем желаемую скорость движения
        Vector3 desiredVelocity = direction * currentSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, smoothMovementSpeed * Time.deltaTime);
        
        // Применяем горизонтальное движение
        Vector3 newPosition = new Vector3(
            currentPos.x + currentVelocity.x * Time.deltaTime,
            currentPos.y,
            currentPos.z + currentVelocity.z * Time.deltaTime
        );
        
        // Проверяем и корректируем высоту
        float newY = currentPos.y;
        Vector3 rayOrigin = new Vector3(newPosition.x, newPosition.y + 2f, newPosition.z);
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer))
        {
            newY = Mathf.Lerp(currentPos.y, hit.point.y, heightSmoothFactor * Time.deltaTime);
            lastValidPosition = new Vector3(newPosition.x, hit.point.y, newPosition.z);
            isGrounded = true;
        }
        else
        {
            // Если не нашли землю, используем последнюю валидную позицию
            newY = lastValidPosition.y;
            isGrounded = false;
        }
        
        // Применяем финальную позицию с учетом высоты
        transform.position = new Vector3(newPosition.x, newY, newPosition.z);
        
        // Поворот в направлении движения
        if (currentVelocity.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
        }

        // Обновляем анимацию
        if (animator != null)
        {
            animator.SetBool("IsMoving", currentSpeed > 0.1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация проверки земли для отладки
        Gizmos.color = Color.yellow;
        Vector3 rayOrigin = transform.position + Vector3.up * 2f;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * rayLength);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckDistance, groundCheckRadius);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Вызываем событие изменения здоровья
        OnHealthChanged?.Invoke(currentHealth);
        
        // Визуальные эффекты при получении урона
        if (_npcData.hitVFXPrefab != null)
        {
            Instantiate(_npcData.hitVFXPrefab, transform.position, Quaternion.identity);
        }

        if (audioSource != null && _npcData.hitSound != null)
        {
            audioSource.PlayOneShot(_npcData.hitSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Обработка смерти НПС
        isInGroup = false;
        
        // Уничтожаем UI элемент
        if (companionUI != null)
        {
            Destroy(companionUI.gameObject);
        }
        
        // Здесь можно добавить анимацию смерти, эффекты и т.д.
        gameObject.SetActive(false);
    }

    public void JoinGroup()
    {
        if (!isInGroup)
        {
            isInGroup = true;
            if (joinPrompt != null)
                joinPrompt.SetActive(false);
                
            // Создаем UI элемент для компаньона
            if (companionUIPrefab != null && companionUIContainer != null)
            {
                GameObject uiInstance = Instantiate(companionUIPrefab, companionUIContainer);
                companionUI = uiInstance.GetComponent<CompanionUI>();
                if (companionUI != null)
                {
                    companionUI.Initialize(this);
                }
            }
                
            // Можно добавить визуальный эффект присоединения
            if (audioSource != null && _npcData.hitSound != null)
                audioSource.PlayOneShot(_npcData.hitSound);
                
            // Вызываем событие присоединения к группе
            OnJoinedGroup?.Invoke(this);
            Debug.Log($"{_npcData.npcName} присоединился к группе!");
        }
    }

    public void SetTarget(Enemy target)
    {
        if (target != currentTarget)
        {
            currentTarget = target;
            isAttacking = false;
            attackTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInGroup)
        {
            isPlayerNearby = true;
            if (joinPrompt != null)
                joinPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (joinPrompt != null)
                joinPrompt.SetActive(false);
        }
    }

    // Методы для получения информации о НПС
    public string GetNPCName()
    {
        return _npcData.npcName;
    }

    public int GetMaxHealth()
    {
        return _npcData.maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void RemoveFromGroup()
    {
        if (isInGroup)
        {
            isInGroup = false;
            
            // Уничтожаем UI элемент
            if (companionUI != null)
            {
                Destroy(companionUI.gameObject);
            }
            
            // Удаляем себя из списка компаньонов игрока
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.RemoveCompanion(this);
            }
            
            // Можно добавить визуальный эффект выхода из группы
            if (audioSource != null && _npcData.hitSound != null)
                audioSource.PlayOneShot(_npcData.hitSound);
                
            Debug.Log($"{_npcData.npcName} покинул группу!");
        }
    }
} 