using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class CompanionUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elements")]
    [SerializeField] private Text nameText;
    [SerializeField] private Text healthText;
    [SerializeField] private Image healthBarFill;
    
    [Header("Health Bar Colors")]
    [SerializeField] private Color fullHealthColorTop = new Color(0.2f, 1f, 0.2f); // Светло-зеленый
    [SerializeField] private Color fullHealthColorBottom = new Color(0f, 0.8f, 0f); // Темно-зеленый
    [SerializeField] private Color lowHealthColorTop = new Color(1f, 0.2f, 0.2f); // Светло-красный
    [SerializeField] private Color lowHealthColorBottom = new Color(0.8f, 0f, 0f); // Темно-красный
    [SerializeField] private float colorTransitionSpeed = 5f; // Скорость изменения цвета

    private CompanionNPC companion;
    private int maxHealth;
    private UIGradient healthBarGradient;
    private Color32 currentTopColor;
    private Color32 currentBottomColor;
    private Coroutine colorUpdateCoroutine;
    private bool isSelected = false;
    public void Initialize(CompanionNPC companionNPC)
    {
        companion = companionNPC;
        maxHealth = companionNPC.GetMaxHealth();

        // Убедитесь, что UI обновляется при изменении здоровья
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
            healthBarGradient = healthBarFill.GetComponent<UIGradient>();
            if (healthBarGradient == null)
                healthBarGradient = healthBarFill.gameObject.AddComponent<UIGradient>();

            currentTopColor = fullHealthColorTop;
            currentBottomColor = fullHealthColorBottom;
            healthBarGradient.SetColors(currentTopColor, currentBottomColor);
        }

        // Подписываемся на событие изменения здоровья
        companion.OnHealthChanged.AddListener(UpdateHealth);

        // Обновляем текст здоровья
        UpdateHealthText(maxHealth);
    }

    public void UpdateHealth(int currentHealth)
    {
        if (healthBarFill != null)
        {
            // Обновляем полоску здоровья
            float healthPercent = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = healthPercent;

            // Обновляем градиент
            UpdateGradientColors(healthPercent);
        }

        // Обновляем текст здоровья
        UpdateHealthText(currentHealth);
    }

    private void UpdateGradientColors(float healthPercent)
    {
        if (healthBarGradient != null)
        {
            // Останавливаем предыдущую корутину, если она запущена
            if (colorUpdateCoroutine != null)
                StopCoroutine(colorUpdateCoroutine);

            // Запускаем новую корутину для плавного изменения цвета
            Color32 targetTopColor = Color32.Lerp(lowHealthColorTop, fullHealthColorTop, healthPercent);
            Color32 targetBottomColor = Color32.Lerp(lowHealthColorBottom, fullHealthColorBottom, healthPercent);
            
            colorUpdateCoroutine = StartCoroutine(UpdateColorsCoroutine(targetTopColor, targetBottomColor));
        }
    }

    private IEnumerator UpdateColorsCoroutine(Color32 targetTop, Color32 targetBottom)
    {
        while (!ColorsEqual(currentTopColor, targetTop) || !ColorsEqual(currentBottomColor, targetBottom))
        {
            currentTopColor = Color32.Lerp(currentTopColor, targetTop, Time.deltaTime * colorTransitionSpeed);
            currentBottomColor = Color32.Lerp(currentBottomColor, targetBottom, Time.deltaTime * colorTransitionSpeed);
            
            healthBarGradient.SetColors(currentTopColor, currentBottomColor);
            
            yield return null;
        }
    }

    private bool ColorsEqual(Color32 a, Color32 b)
    {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }

    private void UpdateHealthText(int currentHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void Update()
    {
        if (isSelected && Input.GetKeyDown(KeyCode.Delete))
        {
            RemoveFromGroup();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = true;
        // Можно добавить визуальную индикацию выбора
        // Например, изменить цвет рамки или добавить эффект выделения
    }

    private void RemoveFromGroup()
    {
        if (companion != null)
        {
            // Отписываемся от событий
            companion.OnHealthChanged.RemoveListener(UpdateHealth);
            
            // Удаляем компаньона из группы
            companion.RemoveFromGroup();
            
            // Сбрасываем фокус с UI элементов
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            // Блокируем курсор и делаем его невидимым
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Уничтожаем UI элемент
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Сбрасываем выбор при уничтожении UI
        isSelected = false;
        
        if (companion != null)
        {
            companion.OnHealthChanged.RemoveListener(UpdateHealth);
        }
        
        // Дополнительная проверка для сброса курсора
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
} 