using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Character : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxMoveSpeed = 15f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashInvincibilityDuration = 0.5f;

    [Header("Combat Settings")]
    public float attackDamage = 10f;
    public float vfxScaleMultiplier = 1f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip swordSwingSound;

    [Header("References")]
    [SerializeField] private FixedJoystick movementJoystick;
    [SerializeField] private Button dashButton;
    [SerializeField] private Button fireButton;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject dashEffectPrefab;

    private CharacterController characterController;
    private Animator animator;

    private Vector3 moveDirection;
    private Vector3 velocity;
    private float currentSpeed;

    private bool isDashing = false;
    private bool canDash = true;
    private float dashEndTime;
    private float dashCooldownEndTime;
    private Vector3 dashDirection;

    private Vector2 joystickInput;
    private bool fireInput;

    public System.Action OnDashStart;
    public System.Action OnDashEnd;
    public System.Action OnFire;

    // --- YENÝ SÝSTEM ÝÇÝN DEÐÝÞKENLER (Süre Uzatma) ---
    private bool isSpeedBoostActive = false;
    private float speedBoostEndTime = 0f;
    private float appliedBoostAmount = 0f;
    // --------------------------------------------------

    void Start()
    {
        InitializeComponents();
        SetupUIButtons();
    }

    void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        if (movementJoystick == null) Debug.LogError("Joystick atanmamýþ!");

        currentSpeed = moveSpeed;
    }

    void SetupUIButtons()
    {
        if (dashButton != null) dashButton.onClick.AddListener(OnDashButtonPressed);
        if (fireButton != null) fireButton.onClick.AddListener(OnFireButtonPressed);
    }

    // --- UPGRADE FONKSÝYONLARI ---
    public void IncreaseDamage(float amount)
    {
        attackDamage += amount;
        Debug.Log("Saldýrý Gücü Arttý: " + attackDamage);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        if (moveSpeed < maxMoveSpeed)
        {
            moveSpeed += amount;
            moveSpeed = Mathf.Min(moveSpeed, maxMoveSpeed);
            currentSpeed = moveSpeed;
            Debug.Log("Hareket Hýzý Arttý: " + moveSpeed);
        }
        else
        {
            Debug.Log("Maksimum hýza zaten ulaþýldý!");
        }
    }

    public void ReduceDashCooldown(float amount)
    {
        dashCooldown = Mathf.Max(0.2f, dashCooldown - amount);
        Debug.Log("Dash Bekleme Süresi Azaldý: " + dashCooldown);
    }

    public void IncreaseVFXScale(float amount)
    {
        vfxScaleMultiplier += amount;
        Debug.Log("Kýlýç Boyu Arttý! Yeni Çarpan: " + vfxScaleMultiplier);
    }

    // --- SÜRESÝ UZAYAN (STACKLENMEYEN) HIZ SÝSTEMÝ ---
    public void ApplyTemporarySpeedBoost(float amount, float duration)
    {
        if (isSpeedBoostActive)
        {
            // Zaten aktifse sadece süreyi uzat
            speedBoostEndTime += duration;
            Debug.Log("Hýz Botu süresi uzatýldý! Yeni bitiþe kalan süre: " + (speedBoostEndTime - Time.time));
        }
        else
        {
            // Ýlk kez alýnýyorsa hýzý artýr ve sayacý baþlat
            isSpeedBoostActive = true;
            appliedBoostAmount = amount;
            speedBoostEndTime = Time.time + duration;

            moveSpeed += amount;
            currentSpeed = moveSpeed;
            Debug.Log("Hýz Botu Ýlk Kez Aktif! Yeni Hýz: " + moveSpeed);

            StartCoroutine(SpeedBoostRoutine());
        }
    }

    private IEnumerator SpeedBoostRoutine()
    {
        // Zamanlayýcý: Mevcut zaman, bitiþ zamanýna ulaþana kadar bekler
        while (Time.time < speedBoostEndTime)
        {
            yield return null; // Bir sonraki kareye (frame) kadar bekle
        }

        // Süre dolunca hýzý BÝR KERE eski haline getir
        moveSpeed -= appliedBoostAmount;
        currentSpeed = moveSpeed;
        isSpeedBoostActive = false;
        appliedBoostAmount = 0f;

        Debug.Log("Hýz Botu Süresi Doldu. Hýz Eski Haline Döndü: " + moveSpeed);
    }
    // ---------------------------------------------------

    void Update()
    {
        HandleInput();

        if (!isDashing)
        {
            HandleMovement();

            if (!characterController.isGrounded)
            {
                velocity.y = -0.5f;
            }
            else
            {
                velocity.y = 0f;
            }
        }
        else
        {
            HandleDash();
        }

        ApplyMovement();
        UpdateAnimations();
        UpdateDashCooldown();
    }

    void HandleInput()
    {
        if (movementJoystick != null)
        {
            joystickInput = new Vector2(movementJoystick.Horizontal, movementJoystick.Vertical);
            if (joystickInput.magnitude > 1f) joystickInput.Normalize();
        }
    }

    void HandleMovement()
    {
        if (joystickInput.magnitude > 0.1f)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0; right.y = 0;
            forward.Normalize(); right.Normalize();

            moveDirection = (forward * joystickInput.y + right * joystickInput.x).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            velocity.x = moveDirection.x * currentSpeed;
            velocity.z = moveDirection.z * currentSpeed;
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * 10f);
            velocity.z = Mathf.Lerp(velocity.z, 0, Time.deltaTime * 10f);
        }
    }

    void ApplyMovement()
    {
        characterController.Move(velocity * Time.deltaTime);
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", joystickInput.magnitude);
            animator.SetBool("IsGrounded", characterController.isGrounded);
            animator.SetBool("IsDashing", isDashing);
        }
    }

    void UpdateDashCooldown()
    {
        if (!canDash && Time.time >= dashCooldownEndTime)
        {
            canDash = true;
            if (dashButton != null) dashButton.interactable = true;
        }
    }

    void OnDashButtonPressed() { if (canDash && !isDashing) StartDash(); }
    void OnFireButtonPressed() { fireInput = true; HandleFire(); }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashEndTime = Time.time + dashDuration;
        dashCooldownEndTime = Time.time + dashCooldown;

        if (joystickInput.magnitude > 0.1f) dashDirection = moveDirection.normalized;
        else dashDirection = transform.forward;

        velocity.x = dashDirection.x * dashSpeed;
        velocity.z = dashDirection.z * dashSpeed;

        StartCoroutine(InvincibilityDuringDash());

        if (dashEffectPrefab != null) Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
        if (dashButton != null) dashButton.interactable = false;

        OnDashStart?.Invoke();
    }

    IEnumerator InvincibilityDuringDash() { yield return new WaitForSeconds(dashInvincibilityDuration); }

    void HandleDash()
    {
        if (Time.time >= dashEndTime) EndDash();
        else
        {
            velocity.x = dashDirection.x * dashSpeed;
            velocity.z = dashDirection.z * dashSpeed;
            velocity.y = 0;
        }
    }

    void EndDash()
    {
        isDashing = false;
        currentSpeed = moveSpeed;
        velocity.x *= 0.5f; velocity.z *= 0.5f;
        OnDashEnd?.Invoke();
    }

    void HandleFire()
    {
        if (fireInput)
        {
            OnFire?.Invoke();
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            if (audioSource != null && swordSwingSound != null)
            {
                audioSource.PlayOneShot(swordSwingSound);
            }

            fireInput = false;
        }
    }

    public bool IsDashing() { return isDashing; }
    public bool CanDash() { return canDash; }
    public Vector3 GetMoveDirection() { return moveDirection; }
    public float GetMoveSpeed() { return currentSpeed; }
}