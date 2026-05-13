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

    [Header("Zorluk Ayarlari")]
    public float healthIncreasePerMinute = 20f; // Her 1 dakikada max cana eklenecek miktar
    private float baseMaxHealth;                // Havuz bozulmasın diye ilk canı tutacağımız hafıza

    [Header("Saldiri Ayarlari")]
    public float damageToPlayer = 10f;
    public float attackInterval = 1f;
    private float lastAttackTime;

    [Header("XP Ayarlari")]
    public GameObject xpPrefab;

    [Header("Vurus Hissiyati (Hit Flash)")]
    public Color hitColor = Color.darkRed;      // Hasar yiyince bürüneceği renk
    public float flashDuration = 0.15f;         // Kırmızı kalma süresi

    // --- YENİ EKLENEN KISIM: ÖLÜM SONRASI ŞANSLI SPAWN ---
    [Header("Ölüm Sonrası Spawn")]
    public GameObject deathSpawnPrefab;         // Öldüğünde çıkacak olan obje (Patlama, ceset, loot vb.)

    [Range(0f, 100f)]
    public float spawnChance = 10f;             // Çıkma ihtimali (Varsayılan: %10)
    // -----------------------------------------------------

    private float currentHealth;
    private bool isDead = false;
    private Transform player;
    private PlayerHealth playerHealth;
    private float stopDistanceSqr;
    private Animator anim;
    private WaveManager waveManager;
    private Rigidbody rb;

    private Renderer[] meshRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;

    private void Awake()
    {
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

        meshRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = meshRenderers[i].material.color;
            }
        }
    }

    private void OnEnable()
    {
        float minutesPassed = Time.timeSinceLevelLoad / 60f;
        maxHealth = baseMaxHealth + (minutesPassed * healthIncreasePerMinute);
        currentHealth = maxHealth;

        isDead = false;
        lastAttackTime = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (anim != null) anim.SetBool("isMoving", true);

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

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].material.HasProperty("_Color"))
            {
                meshRenderers[i].material.color = hitColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        ResetColor();
    }

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

        // --- YENİ EKLENEN KISIM: %10 ŞANSLA SPAWN İŞLEMİ ---
        if (deathSpawnPrefab != null)
        {
            // 0 ile 100 arasında rastgele bir sayı çek
            float randomValue = Random.Range(0f, 100f);

            // Çekilen sayı belirlediğimiz şansa eşit veya daha küçükse objeyi yarat
            if (randomValue <= spawnChance)
            {
                Instantiate(deathSpawnPrefab, transform.position, Quaternion.identity);
            }
        }
        // -----------------------------------------------------

        if (waveManager != null) waveManager.OnEnemyDefeated();

        ReturnToPool();
    }

    private void ReturnToPool()
    {
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