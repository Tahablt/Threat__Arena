using UnityEngine;
using System.Collections.Generic;

public class AuraWeapon : MonoBehaviour
{
    [Header("Aura Ayarları")]
    public float damage = 10f;
    public float damageInterval = 0.5f;
    public float auraRange = 1.5f; // Artik bu deger dogrudan Collider Radius'u olacak!

    [Header("Görsel Referans")]
    public Transform visualEffectTransform; // Freeze circle objesini buraya sürükleyeceğiz

    private float timer;
    private List<Enemy> enemiesInRange = new List<Enemy>();
    private SphereCollider auraCollider; // Collider referansımız

    void Awake()
    {
        // Obj üzerindeki Sphere Collider'ı otomatik bulur
        auraCollider = GetComponent<SphereCollider>();
    }

    void OnEnable()
    {
        enemiesInRange.Clear();
        UpdateAuraScale();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= damageInterval)
        {
            ApplyAuraDamage();
            timer = 0f;
        }
    }

    void ApplyAuraDamage()
    {
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            Enemy currentEnemy = enemiesInRange[i];
            if (currentEnemy == null || !currentEnemy.gameObject.activeInHierarchy)
            {
                enemiesInRange.RemoveAt(i);
                continue;
            }
            currentEnemy.TakeDamage(damage);
        }
    }

    public void UpgradeAura()
    {
        IncreaseDamage(10f);
        IncreaseRange(0.1f);
    }

    public void IncreaseDamage(float amount)
    {
        damage += amount;
    }

    public void IncreaseRange(float amount)
    {
        auraRange += amount;
        UpdateAuraScale();
    }

    private void UpdateAuraScale()
    {
        // 1. FİZİKSEL DÜZELTME: Ana objenin scale değerini sabit tutup, doğrudan Collider yarıçapını büyüterek hatayı engelliyoruz!
        if (auraCollider != null)
        {
            auraCollider.radius = auraRange;
        }

        // 2. GÖRSEL DÜZELTME: Eğer görsel efektin de büyümesini istiyorsan sadece çocuk görseli büyütüyoruz
        if (visualEffectTransform != null)
        {
            visualEffectTransform.localScale = new Vector3(auraRange, auraRange, auraRange);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
        }
    }
}