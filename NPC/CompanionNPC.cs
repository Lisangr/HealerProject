using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class CompanionNPC : MonoBehaviour
{
    [Header("Конфигурация")]
    [SerializeField] private NPCConfig npcConfig;
    [SerializeField] private string npcID; // Уникальный идентификатор этого НПС
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

        if (currentTarget != null)
        {
            HandleAttack();
        }
        else
        {
            FollowPlayer();
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    private void FollowPlayer()
    {
        if (player == null) return;

        // Вычисляем целевую позицию
        Vector3 targetPosition = player.transform.position - player.transform.forward * _npcData.followDistance;
        
        // Проверяем и корректируем высоту
        targetPosition = AdjustHeight(targetPosition);
        
        // Вычисляем направление движения
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Плавное изменение скорости
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        targetSpeed = Mathf.Clamp(distanceToTarget, minFollowDistance, maxFollowDistance) / maxFollowDistance * _npcData.moveSpeed;
        
        // Плавное ускорение и замедление
        if (distanceToTarget > 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }
        
        // Плавное движение
        Vector3 desiredVelocity = direction * currentSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, smoothMovementSpeed * Time.deltaTime);
        
        // Применяем движение
        transform.position += currentVelocity * Time.deltaTime;
        
        // Плавный поворот
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
        }

        // Анимация движения
        if (animator != null)
        {
            animator.SetBool("IsMoving", currentSpeed > 0.1f);
        }
    }

    private Vector3 AdjustHeight(Vector3 position)
    {
        // Проверяем землю под компаньоном
        RaycastHit hit;
        Vector3 rayStart = position + Vector3.up * groundCheckDistance;
        
        if (Physics.SphereCast(rayStart, groundCheckRadius, Vector3.down, out hit, groundCheckDistance * 2, groundLayer))
        {
            // Сохраняем последнюю валидную позицию
            lastValidPosition = new Vector3(position.x, hit.point.y, position.z);
            
            // Плавно изменяем высоту
            float newY = Mathf.Lerp(transform.position.y, hit.point.y, heightSmoothFactor * Time.deltaTime);
            position.y = newY;
            
            isGrounded = true;
        }
        else
        {
            // Если не нашли землю, используем последнюю валидную позицию
            position = lastValidPosition;
            isGrounded = false;
        }
        
        return position;
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация проверки земли
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckDistance, groundCheckRadius);
        Gizmos.DrawLine(transform.position + Vector3.up * groundCheckDistance, 
                       transform.position + Vector3.up * groundCheckDistance + Vector3.down * groundCheckDistance * 2);
    }

    private void HandleAttack()
    {
        if (currentTarget == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        
        if (distanceToTarget <= _npcData.attackRange)
        {
            // Поворот к цели
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _npcData.rotationSpeed * Time.deltaTime);

            // Атака
            if (attackTimer <= 0)
            {
                Attack();
            }
        }
        else
        {
            // Движение к цели
            Vector3 targetPosition = currentTarget.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _npcData.moveSpeed * Time.deltaTime);
        }
    }

    private void Attack()
    {
        if (currentTarget == null) return;

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
        currentTarget.TakeDamage(_npcData.attackDamage);
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