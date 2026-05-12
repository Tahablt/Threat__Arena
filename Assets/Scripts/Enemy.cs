using UnityEngine;
using System.Collections; // Coroutine (Zamanlayıcı) için gerekli

public enum EnemyType { Slime, Turtle }

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Mob Ayarlari")]
    public EnemyType myType;
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float stopDistance = 1.2f;

    // --- YENİ EKLENEN KISIM: Zorluk Ayarları ---
    [Header("Zorluk Ayarlari")]
    public float healthIncreasePerMinute = 20f; // Her 1 dakikada max cana eklenecek miktar
    private float baseMaxHealth;                // Havuz bozulmasın diye ilk canı tutacağımız hafıza
    // -------------------------------------------

    [Header("Saldiri Ayarlari")]
    public float damageToPlayer = 10f;
    public float attackInterval = 1f;
    private float lastAttackTime;

    [Header("XP Ayarlari")]
    public GameObject xpPrefab;

    [Header("Vurus Hissiyati (Hit Flash)")]
    public Color hitColor = Color.darkRed;      // Hasar yiyince bürüneceği renk
    public float flashDuration = 0.15f;         // Kırmızı kalma süresi

    private float currentHealth;
    private bool isDead = false;
    private Transform player;
    private PlayerHealth playerHealth;
    private float stopDistanceSqr;
    private Animator anim;
    private WaveManager waveManager;
    private Rigidbody rb;

    // --- HIT FLASH DEĞİŞKENLERİ ---
    private Renderer[] meshRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // İlk baştaki canı güvenli bir yere kaydediyoruz (Object Pool tuzağını engeller)
        baseMaxHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponentInChildren<PlayerHealth>();
        }

        waveManager = FindFirstObjectByType<WaveManager>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        stopDistanceSqr = stopDistance * stopDistance;

        if (rb != null)
        {
            rb.mass = 50f;
            rb.linearDamping = 5f;
            rb.angularDamping = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        // --- RENDERER VE ORİJİNAL RENKLERİ HAFIZAYA AL ---
        meshRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            // Eğer materyalin bir ana rengi (_Color) varsa onu kaydet
            if (meshRenderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = meshRenderers[i].material.color;
            }
        }
    }

    private void OnEnable()
    {
        // --- ZAMANA GÖRE CAN ARTIRMA SİSTEMİ ---
        // Oyunun başından beri kaç dakika geçtiğini hesapla
        float minutesPassed = Time.timeSinceLevelLoad / 60f;

        // Yeni Max Can = Orijinal Can + (Geçen Dakika * Dakika Başına Artış)
        maxHealth = baseMaxHealth + (minutesPassed * healthIncreasePerMinute);

        currentHealth = maxHealth; // Canı yeni limite göre tam doldur
        // ---------------------------------------

        isDead = false;
        lastAttackTime = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (anim != null) anim.SetBool("isMoving", true);

        // --- HAVUZDAN DOĞARKEN RENGİ SIFIRLA (Kırmızı doğmasını engeller) ---
        ResetColor();
    }

    private void Update()
    {
        if (player == null || isDead) return;

        float distanceSqr = (player.position - transform.position).sqrMagnitude;

        if (distanceSqr > stopDistanceSqr)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            direction.Normalize();

            if (rb != null)
            {
                rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);
            }
            else
            {
                transform.position += direction * moveSpeed * Time.deltaTime;
            }

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);

            if (anim != null) anim.SetBool("isMoving", true);
        }
        else
        {
            if (rb != null) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            if (anim != null) anim.SetBool("isMoving", false);

            if (Time.time >= lastAttackTime + attackInterval)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToPlayer);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // --- KIRMIZI PARLAMA EFEKTİNİ BAŞLAT ---
        if (flashCoroutine != null) StopCoroutine(flashCoroutine); // Üst üste vurulursa eskisini iptal et
        flashCoroutine = StartCoroutine(FlashRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- ZAMANLAYICI: RENGİ DEĞİŞTİR VE GERİ AL ---
    private IEnumerator FlashRoutine()
    {
        // 1. Tüm parçaları kırmızı yap
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].material.HasProperty("_Color"))
            {
                meshRenderers[i].material.color = hitColor;
            }
        }

        // 2. Belirlenen süre kadar bekle
        yield return new WaitForSeconds(flashDuration);

        // 3. Orijinal renklerine geri döndür
        ResetColor();
    }

    // --- RENK SIFIRLAMA FONKSİYONU ---
    private void ResetColor()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].material.HasProperty("_Color"))
            {
                meshRenderers[i].material.color = originalColors[i];
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (KillManager.Instance != null)
        {
            KillManager.Instance.AddKill();
        }

        if (XPPool.Instance != null && (xpPrefab != null || XPPool.Instance.xpPrefab != null))
        {
            GameObject xp = XPPool.Instance.GetXP(xpPrefab);
            if (xp != null)
            {
                xp.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
            }
            else
            {
                Debug.LogError("🔴 DIKKAT: XPPool aktif ama dondurecek XP Prefab bulamadi!");
            }
        }
        else if (xpPrefab != null)
        {
            Instantiate(xpPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z), Quaternion.identity);
        }

        if (waveManager != null) waveManager.OnEnemyDefeated();

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        // Havuza dönerken de ne olur ne olmaz rengini sıfırlayalım
        ResetColor();

        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.ReturnEnemy(this.gameObject, myType);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}