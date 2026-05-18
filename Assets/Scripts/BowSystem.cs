using UnityEngine;
using System.Collections;

public class BowSystem : MonoBehaviour
{
    [Header("Yay Ayarlari")]
    public GameObject arrowPrefab;      
    public Transform firePoint;         
    [Tooltip("Atışlar arasındaki saniye aralığı. Azaldıkça daha hızlı ateş eder.")]
    public float fireRate = 4f; // İSTEDİĞİN GİBİ 4 SANİYEYE AYARLANDI       
    public float range = 10f;           
    public float arrowDamage = 10f;     
    public float arrowSpeed = 25f;      

    [Header("LEVEL SİSTEMİ (Çoklu Atış)")]
    public int bowLevel = 1;            
    public float spreadDistance = 0.5f; 

    [Header("Radar ve Optimizasyon")]
    public LayerMask enemyLayer; 
    private static Collider[] sharedColliders = new Collider[50]; 

    [Header("Ses Ayarlari")]
    public AudioClip bowFireSound;      
    private AudioSource audioSource;    

    private float fireCountdown = 0f;
    private Transform target;

    void Start()
    {
        audioSource = GetComponentInParent<AudioSource>();
        // İlk doğuşta hemen ateş edebilsin diye countdown sıfırlanır
        fireCountdown = 0f;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy || Vector3.Distance(transform.position, target.position) > range)
        {
            FindNearestEnemy();
        }

        if (fireCountdown <= 0f && target != null && target.gameObject.activeInHierarchy)
        {
            ShootArrows();
            fireCountdown = fireRate; 
        }

        fireCountdown -= Time.deltaTime;
    }

    void FindNearestEnemy()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, range, sharedColliders, enemyLayer);
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

    void ShootArrows()
    {
        if (target == null || !target.gameObject.activeInHierarchy) FindNearestEnemy();
        if (target == null || !target.gameObject.activeInHierarchy) return; 

        for (int j = 0; j < bowLevel; j++)
        {
            GameObject arrowGO = ArrowPool.Instance.GetArrow();
            if (arrowGO == null) continue;
            
            float offset = (j - (bowLevel - 1) / 2f) * spreadDistance;
            Vector3 spawnPos = firePoint.position + firePoint.right * offset;

            arrowGO.transform.position = spawnPos;
            arrowGO.transform.rotation = firePoint.rotation;
            
            Arrow arrow = arrowGO.GetComponent<Arrow>();
            if (arrow != null)
            {
                arrow.Seek(target, arrowDamage, arrowSpeed);
            }
        }

        if (audioSource != null && bowFireSound != null)
        {
            audioSource.PlayOneShot(bowFireSound);
        }
    }
}