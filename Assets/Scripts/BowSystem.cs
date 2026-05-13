using UnityEngine;
using System.Collections; // Coroutine için eklendi

public class BowSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject arrowPrefab;      // Fırlatılacak ok prefabı
    public Transform firePoint;         // Okun çıkacağı nokta
    public float fireRate = 1.5f;       // Kaç saniyede bir ok atılsın?
    public float range = 10f;           // Düşman arama menzili
    public float arrowDamage = 10f;     // Okun hasarı
    public float arrowSpeed = 25f;      // Okun gidiş hızı

    // --- YENİ EKLENEN KISIM: Çoklu Ok Ayarları ---
    public int arrowsPerShot = 1;       // Tek seferde atılacak ok sayısı
    public float timeBetweenArrows = 0.15f; // Okların peş peşe çıkma süresi
    // ---------------------------------------------

    [Header("Ses Ayarlari")]
    public AudioClip bowFireSound;      // Ok fırlatma sesi
    private AudioSource audioSource;    // Karakterdeki hoparlör

    private float fireCountdown = 0f;
    private Transform target;

    void Start()
    {
        audioSource = GetComponentInParent<AudioSource>();
    }

    void Update()
    {
        if (target == null || Vector3.Distance(transform.position, target.position) > range)
        {
            FindNearestEnemy();
        }

        if (fireCountdown <= 0f && target != null)
        {
            // Artık Shoot() yerine zamanlayıcılı Seri Atış fonksiyonunu başlatıyoruz
            StartCoroutine(ShootBurst());
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void FindNearestEnemy()
    {
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

    // --- YENİ EKLENEN KISIM: SERİ ATIŞ SİSTEMİ ---
    IEnumerator ShootBurst()
    {
        // arrowsPerShot (Ok Sayısı) kadar döngüye gir
        for (int i = 0; i < arrowsPerShot; i++)
        {
            // Eğer ilk ok düşmanı öldürdüyse, havaya sıkmamak için yeni düşman ara!
            if (target == null) FindNearestEnemy();
            if (target == null) break; // Etrafta hiç düşman kalmadıysa atışı kes

            GameObject arrowGO = ArrowPool.Instance.GetArrow();
            arrowGO.transform.position = firePoint.position;
            arrowGO.transform.rotation = firePoint.rotation;
            Arrow arrow = arrowGO.GetComponent<Arrow>();

            if (arrow != null)
            {
                arrow.Seek(target, arrowDamage, arrowSpeed);
            }

            if (audioSource != null && bowFireSound != null)
            {
                audioSource.PlayOneShot(bowFireSound);
            }

            // Bir sonraki oku atmadan önce 0.15 saniye bekle (Makineli tüfek efekti)
            yield return new WaitForSeconds(timeBetweenArrows);
        }
    }
    // ---------------------------------------------
}