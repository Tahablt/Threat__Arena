using UnityEngine;

public class BowSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject arrowPrefab;    // Fırlatılacak ok prefabı
    public Transform firePoint;       // Okun çıkacağı nokta
    public float fireRate = 1.5f;     // Kaç saniyede bir ok atılsın?
    public float range = 10f;         // Düşman arama menzili
    public float arrowDamage = 10f;   // Okun hasarı

    private float fireCountdown = 0f;
    private Transform target;

    void Update()
    {
        // Hedef yoksa veya hedef menzil dışındaysa yeni hedef ara
        if (target == null || Vector3.Distance(transform.position, target.position) > range)
        {
            FindNearestEnemy();
        }

        // Ateş etme zamanı geldiyse ve hedef varsa
        if (fireCountdown <= 0f && target != null)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void FindNearestEnemy()
    {
        // Sahnedeki "Enemy" tag'ine sahip tüm objeleri bul
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    void Shoot()
    {
        // Oku oluştur ve hedefe yönelt
        GameObject arrowGO = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);

        // Okun içindeki script'e hedefi ve hasarı gönder
        Arrow arrow = arrowGO.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.Seek(target, arrowDamage);
        }
    }
}