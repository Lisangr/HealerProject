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
    [SerializeField] private Color fullHealthColorTop = new Color(0.2f, 1f, 0.2f);
    [SerializeField] private Color fullHealthColorBottom = new Color(0f, 0.8f, 0f);
    [SerializeField] private Color lowHealthColorTop = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color lowHealthColorBottom = new Color(0.8f, 0f, 0f);
    [SerializeField] private float colorTransitionSpeed = 5f;

    private CompanionNPC companion;
    private HealthSystem companionHealthSystem; // Ссылка на компонент HealthSystem
    private int maxHealth;
    private UIGradient healthBarGradient;
    private Color32 currentTopColor;
    private Color32 currentBottomColor;
    private Coroutine colorUpdateCoroutine;
    private bool isSelected = false;

    // Убираем метод OnEnable — мы будем вызывать Initialize извне после создания UI.
    // private void OnEnable() { ... }

    public void Initialize(CompanionNPC companionNPC)
    {
        // Сохраняем ссылку на компаньона
        companion = companionNPC;

        // Получаем компонент HealthSystem с объекта компаньона
        companionHealthSystem = companion.GetComponent<HealthSystem>();
        if (companionHealthSystem != null)
        {
            // Подписываемся на событие обновления здоровья из HealthSystem
            companionHealthSystem.OnHealthChanged.AddListener(UpdateHealth);
            maxHealth = companionHealthSystem.GetMaxHealth();
            UpdateHealth(companionHealthSystem.GetCurrentHealth());
        }
        else
        {
            Debug.LogWarning("HealthSystem не найден на объекте компаньона " + companionNPC.gameObject.name);
        }

        // Устанавливаем текст с именем компаньона
        if (nameText != null)
        {
            nameText.text = companionNPC.GetNPCName();
        }

        // Инициализируем визуальное представление полоски здоровья
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
            healthBarGradient = healthBarFill.GetComponent<UIGradient>();
            if (healthBarGradient == null)
            {
                healthBarGradient = healthBarFill.gameObject.AddComponent<UIGradient>();
            }
            currentTopColor = fullHealthColorTop;
            currentBottomColor = fullHealthColorBottom;
            healthBarGradient.SetColors(currentTopColor, currentBottomColor);
        }
    }

    public void UpdateHealth(int currentHealth)
    {
        if (healthBarFill != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = healthPercent;
            UpdateGradientColors(healthPercent);
        }
        UpdateHealthText(currentHealth);
    }

    private void UpdateGradientColors(float healthPercent)
    {
        if (healthBarGradient != null)
        {
            if (colorUpdateCoroutine != null)
                StopCoroutine(colorUpdateCoroutine);

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
            healthText.text = currentHealth.ToString();
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
    }

    private void RemoveFromGroup()
    {
        if (companion != null)
        {
            if (companionHealthSystem != null)
            {
                companionHealthSystem.OnHealthChanged.RemoveListener(UpdateHealth);
            }
            companion.RemoveFromGroup();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        isSelected = false;
        if (companionHealthSystem != null)
        {
            companionHealthSystem.OnHealthChanged.RemoveListener(UpdateHealth);
        }
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
