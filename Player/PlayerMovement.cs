using UnityEngine;
using UnityEngine.EventSystems;

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

    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Camera playerCamera;

    private Rigidbody rb;
    private int jumpsRemaining;
    private bool isGrounded;
    private Vector3 movementInput;
    private PlayerAnimationController animationController;
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
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Курсор над UI – можно не обрабатывать игровой ввод
            return;
        }

        GroundCheck();
        ProcessInput();
        HandleJump();
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
        bool isRunning = InputUtils.GetKey(InputSettings.Instance.RunModifierKey);
        speedMultiplier = isRunning ? 1.8f : 1f;

        float horizontal = 0f;
        float vertical = 0f;
        if (InputUtils.GetKey(InputSettings.Instance.MoveRightKey))
            horizontal += moveSpeedH * speedMultiplier;
        if (InputUtils.GetKey(InputSettings.Instance.MoveLeftKey))
            horizontal -= moveSpeedH * speedMultiplier;
        if (InputUtils.GetKey(InputSettings.Instance.MoveForwardKey))
            vertical += moveSpeedV * speedMultiplier;
        if (InputUtils.GetKey(InputSettings.Instance.MoveBackwardKey))
            vertical -= moveSpeedV * speedMultiplier;

        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        movementInput = cameraRight * horizontal + cameraForward * vertical;

        // Обновление анимаций (без плавания)
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
        // Если сейчас проигрывается анимация атаки, не обновляем анимации движения
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (groundCheck != null)
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * 50f, transform.position + Vector3.down * 100f);
    }
}
