using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings (на суше)")]
    public float moveSpeedH = 3f;
    public float moveSpeedV = 5f;
    public float jumpForce = 5f;
    public int maxJumps = 1;
    public float groundCheckDistance = 0.3f;
    public float heightOffset = 0.5f;
    public LayerMask groundLayer;
    public float rotationSpeed = 10f;
    public float speedMultiplier = 1.8f;

    [Header("Stamina (¬ыносливость) Settings for Sprint")]
    // ћаксимальное значение выносливости; можно установить значение из PlayerData
    [SerializeField] private float maxStamina;
    // “екущее значение выносливости
    [SerializeField] private float currentStamina;
    // —тоимость ускорени€: расход выносливости за секунду при спринте
    public float sprintStaminaCostRate = 0.5f;
    // —корость восстановлени€ выносливости (единиц в секунду) когда спринт не активен
    public float staminaRegenerationRate = 1f;

    [Header("Stamina UI")]
    // —сылка на UI-элемент полоски выносливости (аналогично healthBar)
    public Image staminaBar;
    // —сылка на текстовое поле дл€ отображени€ значений выносливости
    public Text staminaText;


    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Camera playerCamera;

    private Rigidbody rb;
    private int jumpsRemaining;
    private bool isGrounded;
    private Vector3 movementInput;
    private PlayerAnimationController animationController;
    private const string StaminaKey = "PlayerStamina";

    public float CurrentSpeed
    {
        get
        {
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            return horizontalVelocity.magnitude;
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        jumpsRemaining = maxJumps;
        animationController = GetComponent<PlayerAnimationController>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        maxStamina = PlayerPrefs.GetInt(StaminaKey, 15);
        currentStamina = PlayerPrefs.GetInt(StaminaKey);
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            //  урсор над UI Ц можно не обрабатывать игровой ввод
            return;
        }

        GroundCheck();
        ProcessInput();
        HandleJump();

        // ќбновление UI выносливости каждый кадр
        UpdateStaminaUI();
    }

    private void FixedUpdate()
    {
        Move();
        ApplyGravity();
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        RotatePlayer();
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
        if (isGrounded)
            jumpsRemaining = maxJumps;
    }
    private void ProcessInput()
    {
        // ѕолучаем базовый ввод без учета ускорени€
        float baseHorizontal = 0f;
        float baseVertical = 0f;
        if (InputUtils.GetKey(InputSettings.Instance.MoveRightKey))
            baseHorizontal += moveSpeedH;
        if (InputUtils.GetKey(InputSettings.Instance.MoveLeftKey))
            baseHorizontal -= moveSpeedH;
        if (InputUtils.GetKey(InputSettings.Instance.MoveForwardKey))
            baseVertical += moveSpeedV;
        if (InputUtils.GetKey(InputSettings.Instance.MoveBackwardKey))
            baseVertical -= moveSpeedV;

        // ќпредел€ем, включен ли спринт
        bool runKeyPressed = InputUtils.GetKey(InputSettings.Instance.RunModifierKey);
        if (runKeyPressed && currentStamina >= 1f)
        {
            // ≈сли спринт активен, устанавливаем множитель дл€ ускорени€ и расходуем выносливость
            speedMultiplier = 1.8f;
            currentStamina -= sprintStaminaCostRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
        }
        else
        {
            // ≈сли спринт не активен или выносливость ниже 1, сбрасываем множитель и восстанавливаем выносливость
            speedMultiplier = 1f;
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenerationRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }

        // ѕримен€ем множитель к базовому вводу
        float horizontal = baseHorizontal * speedMultiplier;
        float vertical = baseVertical * speedMultiplier;

        // –ассчитываем направление движени€ с учетом направлени€ камеры
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        movementInput = cameraRight * horizontal + cameraForward * vertical;

        // ќбновление анимаций движени€
        UpdateAnimations(horizontal, vertical);
    }

    private void Move()
    {
        Vector3 targetPosition = rb.position + movementInput * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    private void RotatePlayer()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementInput.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    private void UpdateAnimations(float horizontal, float vertical)
    {
        // ≈сли сейчас проигрываетс€ анимаци€ атаки, не обновл€ем анимации движени€
        if (animationController.animator.GetCurrentAnimatorStateInfo(0).IsName(animationController.animationStates.Attack1))
        {
            return;
        }

        AnimationStates states = animationController.animationStates;

        if (isGrounded)
        {
            bool isRunning = InputUtils.GetKey(InputSettings.Instance.RunModifierKey);
            if (!isRunning)
            {
                if (vertical > 0)
                    animationController.PlayAnimation(states.WalkForward);
                else if (vertical < 0)
                    animationController.PlayAnimation(states.WalkBackward);
                else if (horizontal > 0)
                    animationController.PlayAnimation(states.WalkRight);
                else if (horizontal < 0)
                    animationController.PlayAnimation(states.WalkLeft);
                else
                    animationController.PlayAnimation(states.Idle);
            }
            else
            {
                if (vertical > 0)
                    animationController.PlayAnimation(states.RunForward);
                else if (vertical < 0)
                    animationController.PlayAnimation(states.RunBackward);
                else if (horizontal > 0)
                    animationController.PlayAnimation(states.RunRight);
                else if (horizontal < 0)
                    animationController.PlayAnimation(states.RunLeft);
                else
                    animationController.PlayAnimation(states.Idle);
            }
        }
    }

    private void HandleJump()
    {
        AnimationStates states = animationController.animationStates;

        if (InputUtils.GetKeyDown(InputSettings.Instance.JumpKey))
        {
            if (jumpsRemaining > 0)
            {
                animationController.PlayAnimation(states.Jump);
                rb.velocity = new Vector3(rb.velocity.x,
                    Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y),
                    rb.velocity.z);
                jumpsRemaining--;
            }
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
            rb.velocity += Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
    }

    private void UpdateStaminaUI()
    {
        // ќбновл€ем значение полоски выносливости
        if (staminaBar != null)
        {
            staminaBar.fillAmount = currentStamina / maxStamina;
        }
        // ќбновл€ем текстовое поле (округл€ем до целого числа)
        if (staminaText != null)
        {
            staminaText.text = Mathf.RoundToInt(currentStamina) + " / " + Mathf.RoundToInt(maxStamina);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (groundCheck != null)
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * 50f, transform.position + Vector3.down * 100f);
    }
}
