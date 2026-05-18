using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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
    [SerializeField] private float attackDuration = 0.5f; 

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip swordSwingSound;

    [Header("References")]
    [SerializeField] private FixedJoystick movementJoystick;
    [SerializeField] private Button dashButton;
    [SerializeField] private Button fireButton;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject dashEffectPrefab;
    [SerializeField] private Slider speedSlider; // YENİ: Slider eklendi (C# standartlarına göre küçük harfle başlattım)

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
    private bool isAttacking = false; 

    public System.Action OnDashStart;
    public System.Action OnDashEnd;
    public System.Action OnFire;

    // --- SÜRE UZATMA SİSTEMİ İÇİN DEĞİŞKENLER ---
    private bool isSpeedBoostActive = false;
    private float speedBoostEndTime = 0f;
    private float appliedBoostAmount = 0f;
    private float currentSpeedBoostMaxDuration = 0f; // YENİ: Slider'ın maksimum değerini ayarlamak için toplam süreyi tutar
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
        if (movementJoystick == null) Debug.LogError("Joystick atanmamış!");

        currentSpeed = moveSpeed;

        // YENİ: Oyun başladığında UI'ları kapalı tut
        if (speedSlider != null) speedSlider.gameObject.SetActive(false);
    }

    void SetupUIButtons()
    {
        if (dashButton != null) dashButton.onClick.AddListener(OnDashButtonPressed);
        if (fireButton != null) fireButton.onClick.AddListener(OnFireButtonPressed);
    }

    // --- UPGRADE FONKSİYONLARI ---
    public void IncreaseDamage(float amount)
    {
        attackDamage += amount;
        Debug.Log("Saldırı Gücü Arttı: " + attackDamage);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        if (moveSpeed < maxMoveSpeed)
        {
            moveSpeed += amount;
            moveSpeed = Mathf.Min(moveSpeed, maxMoveSpeed);
            currentSpeed = moveSpeed;
            Debug.Log("Hareket Hızı Arttı: " + moveSpeed);
        }
        else
        {
            Debug.Log("Maksimum hıza zaten ulaşıldı!");
        }
    }

    public void ReduceDashCooldown(float amount)
    {
        dashCooldown = Mathf.Max(0.2f, dashCooldown - amount);
        Debug.Log("Dash Bekleme Süresi Azaldı: " + dashCooldown);
    }

    public void IncreaseVFXScale(float amount)
    {
        vfxScaleMultiplier += amount;
        Debug.Log("Kılıç Boyu Arttı! Yeni Çarpan: " + vfxScaleMultiplier);
    }

    // --- SÜRESİ UZAYAN (STACKLENMEYEN) HIZ SİSTEMİ ---
    public void ApplyTemporarySpeedBoost(float amount, float duration)
    {
        if (isSpeedBoostActive)
        {
            speedBoostEndTime += duration;
            currentSpeedBoostMaxDuration += duration; // Slider'ın sınırını da genişlet

            if (speedSlider != null) speedSlider.maxValue = currentSpeedBoostMaxDuration;
            
            Debug.Log("Hız Botu süresi uzatıldı! Yeni bitişe kalan süre: " + (speedBoostEndTime - Time.time));
        }
        else
        {
            isSpeedBoostActive = true;
            appliedBoostAmount = amount;
            speedBoostEndTime = Time.time + duration;
            currentSpeedBoostMaxDuration = duration; // Yeni alınan botun süresini Slider'ın max değeri yap

            if (speedSlider != null)
            {
                speedSlider.maxValue = currentSpeedBoostMaxDuration;
                speedSlider.value = currentSpeedBoostMaxDuration;
            }

            moveSpeed += amount;
            currentSpeed = moveSpeed;
            Debug.Log("Hız Botu İlk Kez Aktif! Yeni Hız: " + moveSpeed);

            StartCoroutine(SpeedBoostRoutine());
        }
    }

    private IEnumerator SpeedBoostRoutine()
    {
        while (Time.time < speedBoostEndTime)
        {
            yield return null; 
        }

        moveSpeed -= appliedBoostAmount;
        currentSpeed = moveSpeed;
        isSpeedBoostActive = false;
        appliedBoostAmount = 0f;

        Debug.Log("Hız Botu Süresi Doldu. Hız Eski Haline Döndü: " + moveSpeed);
    }

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
        
        // UI Güncellemesi
        UpdateSpeedBoostUI(); 
    }

    void HandleInput()
    {
        float h = 0f;
        float v = 0f;

        if (movementJoystick != null)
        {
            h = movementJoystick.Horizontal;
            v = movementJoystick.Vertical;
        }

        float keyboardH = Input.GetAxisRaw("Horizontal");
        float keyboardV = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(keyboardH) > 0.1f || Mathf.Abs(keyboardV) > 0.1f)
        {
            h = keyboardH;
            v = keyboardV;
        }

        joystickInput = new Vector2(h, v);
        if (joystickInput.magnitude > 1f) joystickInput.Normalize();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canDash && !isDashing) StartDash();
        }

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
        if (fireInput && !isAttacking)
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

            StartCoroutine(AttackCooldownRoutine());
        }
        
        fireInput = false; 
    }

    private void UpdateSpeedBoostUI()
    {
        if (isSpeedBoostActive)
        {
            float remainingTime = speedBoostEndTime - Time.time;

            if (remainingTime > 0)
            {
                // UI Elemanları kapalıysa aç
                    
                if (speedSlider != null && !speedSlider.gameObject.activeSelf) 
                    speedSlider.gameObject.SetActive(true);

                // Yazıyı Güncelle
                
                // Slider'ı Güncelle (Float değeri doğrudan atanır)
                if (speedSlider != null)
                    speedSlider.value = remainingTime;
            }
        }
        else
        {
                
            if (speedSlider != null && speedSlider.gameObject.activeSelf) 
                speedSlider.gameObject.SetActive(false);
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }

    public bool IsDashing() { return isDashing; }
    public bool CanDash() { return canDash; }
    public Vector3 GetMoveDirection() { return moveDirection; }
    public float GetMoveSpeed() { return currentSpeed; }
}