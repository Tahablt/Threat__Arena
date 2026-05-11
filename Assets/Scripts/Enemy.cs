using UnityEngine;

public enum EnemyType { Slime, Turtle }

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Mob Ayarlari")]
    public EnemyType myType;
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float stopDistance = 1.2f;

    [Header("Saldiri Ayarlari")]
    public float damageToPlayer = 10f;
    public float attackInterval = 1f;
    private float lastAttackTime;

    [Header("XP Ayarlari")]
    public GameObject xpPrefab;

    private float currentHealth;
    private bool isDead = false;
    private Transform player;
    private PlayerHealth playerHealth;
    private float stopDistanceSqr;
    private Animator anim;
    private WaveManager waveManager;
    private Rigidbody rb;

    private void Awake()
    {
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
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
        lastAttackTime = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (anim != null) anim.SetBool("isMoving", true);
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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // --- ÖLDÜRÜLEN MOB SAYISINI ARTIR ---
        if (KillManager.Instance != null)
        {
            KillManager.Instance.AddKill();
        }
        // -----------------------------------

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