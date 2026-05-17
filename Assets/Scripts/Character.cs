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

    // --- YEN� S�STEM ���N DE���KENLER (S�re Uzatma) ---
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
        if (movementJoystick == null) Debug.LogError("Joystick atanmam��!");

        currentSpeed = moveSpeed;
    }

    void SetupUIButtons()
    {
        if (dashButton != null) dashButton.onClick.AddListener(OnDashButtonPressed);
        if (fireButton != null) fireButton.onClick.AddListener(OnFireButtonPressed);
    }

    // --- UPGRADE FONKS�YONLARI ---
    public void IncreaseDamage(float amount)
    {
        attackDamage += amount;
        Debug.Log("Sald�r� G�c� Artt�: " + attackDamage);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        if (moveSpeed < maxMoveSpeed)
        {
            moveSpeed += amount;
            moveSpeed = Mathf.Min(moveSpeed, maxMoveSpeed);
            currentSpeed = moveSpeed;
            Debug.Log("Hareket H�z� Artt�: " + moveSpeed);
        }
        else
        {
            Debug.Log("Maksimum h�za zaten ula��ld�!");
        }
    }

    public void ReduceDashCooldown(float amount)
    {
        dashCooldown = Mathf.Max(0.2f, dashCooldown - amount);
        Debug.Log("Dash Bekleme S�resi Azald�: " + dashCooldown);
    }

    public void IncreaseVFXScale(float amount)
    {
        vfxScaleMultiplier += amount;
        Debug.Log("K�l�� Boyu Artt�! Yeni �arpan: " + vfxScaleMultiplier);
    }

    // --- S�RES� UZAYAN (STACKLENMEYEN) HIZ S�STEM� ---
    public void ApplyTemporarySpeedBoost(float amount, float duration)
    {
        if (isSpeedBoostActive)
        {
            // Zaten aktifse sadece s�reyi uzat
            speedBoostEndTime += duration;
            Debug.Log("H�z Botu s�resi uzat�ld�! Yeni biti�e kalan s�re: " + (speedBoostEndTime - Time.time));
        }
        else
        {
            // �lk kez al�n�yorsa h�z� art�r ve sayac� ba�lat
            isSpeedBoostActive = true;
            appliedBoostAmount = amount;
            speedBoostEndTime = Time.time + duration;

            moveSpeed += amount;
            currentSpeed = moveSpeed;
            Debug.Log("H�z Botu �lk Kez Aktif! Yeni H�z: " + moveSpeed);

            StartCoroutine(SpeedBoostRoutine());
        }
    }

    private IEnumerator SpeedBoostRoutine()
    {
        // Zamanlay�c�: Mevcut zaman, biti� zaman�na ula�ana kadar bekler
        while (Time.time < speedBoostEndTime)
        {
            yield return null; // Bir sonraki kareye (frame) kadar bekle
        }

        // S�re dolunca h�z� B�R KERE eski haline getir
        moveSpeed -= appliedBoostAmount;
        currentSpeed = moveSpeed;
        isSpeedBoostActive = false;
        appliedBoostAmount = 0f;

        Debug.Log("H�z Botu S�resi Doldu. H�z Eski Haline D�nd�: " + moveSpeed);
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
        // 1. �nce Joystick verilerini alal�m (Ekrana dokunuluyorsa)
        float h = 0f;
        float v = 0f;

        if (movementJoystick != null)
        {
            h = movementJoystick.Horizontal;
            v = movementJoystick.Vertical;
        }

        // 2. Klavye verilerini alal�m (W, A, S, D veya Y�n Tu�lar�)
        float keyboardH = Input.GetAxisRaw("Horizontal");
        float keyboardV = Input.GetAxisRaw("Vertical");

        // 3. E�er klavyeden bir tu�a bas�l�yorsa, Joystick'i ez ve Klavyeyi kullan
        if (Mathf.Abs(keyboardH) > 0.1f || Mathf.Abs(keyboardV) > 0.1f)
        {
            h = keyboardH;
            v = keyboardV;
        }

        // 4. Sonucu karaktere ilet
        joystickInput = new Vector2(h, v);
        if (joystickInput.magnitude > 1f) joystickInput.Normalize();

        // --- B�LG�SAYAR TEST� ���N KLAVYE KISAYOLLARI ---

        // Bo�luk (Space) tu�u ile Dash atma
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canDash && !isDashing) StartDash();
        }

        // Fare Sol T�k ile Vuru� yapma
        if (Input.GetMouseButtonDown(0))
        {
            fireInput = true;
            HandleFire();
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
