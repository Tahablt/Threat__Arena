using UnityEngine;
using System.Collections;

public class BowSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject arrowPrefab;      
    public Transform firePoint;         
    public float fireRate = 1.5f;       
    public float range = 10f;           
    public float arrowDamage = 10f;     
    public float arrowSpeed = 25f;      

    public int arrowsPerShot = 1;       
    public float timeBetweenArrows = 0.15f; 

    [Header("Ses Ayarlari")]
    public AudioClip bowFireSound;      
    private AudioSource audioSource;    

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
            StartCoroutine(ShootBurst());
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void FindNearestEnemy()
    {
        // --- OPTİMİZASYON: Tüm haritayı değil, sadece etrafındaki küreyi tarar ---
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = hitCollider.transform;
                }
            }
        }

        target = nearestEnemy;
    }

    IEnumerator ShootBurst()
    {
        for (int i = 0; i < arrowsPerShot; i++)
        {
            if (target == null) FindNearestEnemy();
            if (target == null) break; 

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

            yield return new WaitForSeconds(timeBetweenArrows);
        }
    }
}