using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Transform target;
    private float damage;
    
    // Okun havada uçuş hızı. BowSystem fırlattığı an buraya kendi belirlediğin hızı (örn: 25) yazar.
    public float speed; 
    
    [Header("Model Ayarları")]
    public float yRotationOffset = 90f; 

    [Header("Yaşam Süresi")]
    public float lifeTime = 5f; 

    // Okun gideceği yönü tutan vektör.
    private Vector3 moveDirection;
    
    [Header("Yeni Hedef Arama")]
    public float searchRadius = 20f; 
    public LayerMask enemyLayer; 
    
    private float searchTimer = 0f; 
    private static Collider[] sharedColliders = new Collider[30]; 

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if(col != null) col.isTrigger = true;
    }

    // --- SEEK FONKSİYONU ---
    // BowSystem oku fırlattığı anda çalışır. Hedefi, hasarı ve HIZI (speed) ok'a teslim eder.
    public void Seek(Transform _target, float _damage, float _speed)
    {
        target = _target;
        damage = _damage;
        
        // BowSystem'den gelen 25f (veya belirlediğin değer) artık bu okun uçuş hızı oldu.
        speed = _speed; 
        
        searchTimer = 0f; 
        
        if (target != null)
        {
            // Düşmanın merkezini al (Ayaklarını değil, çarpışma kutusunun tam ortasını)
            Vector3 targetPos = GetTargetCenter();
            Vector3 lookPosition = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            Vector3 dir = lookPosition - transform.position;
            
            // Eğer düşmanla aramızda belirli bir mesafe varsa hedefe dön
            if (dir.sqrMagnitude > 0.1f)
            {
                moveDirection = dir.normalized;
                transform.rotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, yRotationOffset, 0);
            }
            else
            {
                // Düşman DİBİMİZDEYSE yön hesaplaması yapma, dümdüz fırla.
                moveDirection = transform.forward; 
                transform.rotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, yRotationOffset, 0);
            }
        }
        else
        {
            moveDirection = transform.right; 
        }
        
        // 5 saniye sonra oku havuza (Pool) geri gönder
        Invoke(nameof(DeactivateArrow), lifeTime);
    }

    void Update()
    {
        // Hedef yoksa veya öldüyse yeni hedef ara
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            searchTimer -= Time.deltaTime;
            
            if (searchTimer <= 0f)
            {
                FindNewTargetOptimized(); 
                searchTimer = 0.25f; 
            }
        }

        // Hedef aktifse yönünü ona doğru sürekli güncelle (Güdüm)
        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector3 targetPos = GetTargetCenter();
            Vector3 lookPosition = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            Vector3 dirToTarget = lookPosition - transform.position;

            // Ok uçarken düşman dibine girerse yönünü bozmasın diye mesafe kontrolü
            if (dirToTarget.sqrMagnitude > 0.1f) 
            {
                moveDirection = dirToTarget.normalized;
                transform.rotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, yRotationOffset, 0);
            }
            
            // Failsafe: Ok hedefin içindeyse manuel vur
            if (dirToTarget.sqrMagnitude < 0.3f) 
            {
                HitTarget(target.GetComponent<Collider>());
            }
        }

        // --- HAREKET ETTİRME KISMI ---
        // İŞTE SPEED BURADA KULLANILIYOR! 
        // Okun bakacağı yön (moveDirection) ile okun uçuş hızını (speed) çarpıyoruz.
        // Time.deltaTime ile çarpmamızın sebebi ise hareketin FPS'e bağlı kalmadan her cihazda akıcı olmasıdır.
        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    // Düşmanın ayak ucunu değil, gerçek fiziksel merkezini hedefler
    private Vector3 GetTargetCenter()
    {
        if (target == null) return transform.position;
        
        Collider col = target.GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds.center; 
        }
        
        return target.position; 
    }

    // Ok fiziksel olarak bir şeye çarptığında tetiklenir
    private void OnTriggerEnter(Collider other)
    {
        HitTarget(other);
    }

    // Hasar verme ve oku yok etme işlemleri
    private void HitTarget(Collider other)
    {
        if (other == null) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            DeactivateArrow(); 
        }
    }

    // Etraftaki en yakın düşmanı FPS düşürmeden (NonAlloc) tarar
    void FindNewTargetOptimized()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, searchRadius, sharedColliders, enemyLayer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        for (int i = 0; i < hitCount; i++)
        {
            if (!sharedColliders[i].gameObject.activeInHierarchy) continue;

            float distanceToEnemy = Vector3.Distance(transform.position, sharedColliders[i].transform.position);
            
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = sharedColliders[i].transform;
            }
        }

        target = nearestEnemy;
    }

    // Oku kapatıp havuza gönderir
    void DeactivateArrow()
    {
        CancelInvoke(); 
        gameObject.SetActive(false); 
    }

    // Obje kapatıldığında güvenlik önlemi olarak sayaçları durdurur
    void OnDisable()
    {
        CancelInvoke();
    }
}