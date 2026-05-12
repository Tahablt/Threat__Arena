using UnityEngine;
using System.Collections.Generic;

public class AuraWeapon : MonoBehaviour
{
    [Header("Aura Ayarları")]
    public float damage = 10f;
    public float damageInterval = 0.5f;
    public float auraRange = 3f; // Bu değer Transform Scale'ini belirleyecek

    private float timer;
    private List<Enemy> enemiesInRange = new List<Enemy>();

    void OnEnable()
    {
        enemiesInRange.Clear();
        UpdateAuraScale();
    }

    void Update()
    {
        // 1. Hasar Zamanlayıcısı
        timer += Time.deltaTime;
        if (timer >= damageInterval)
        {
            ApplyAuraDamage();
            timer = 0f;
        }

        // 2. Görseli Döndürme (Eğer Sprite Kullanırsan Diye Ufak Bir Eklenti)
        // Eğer görselin dönmesini istemiyorsan bu satırı silebilirsin.
        transform.Rotate(Vector3.forward * 100f * Time.deltaTime);
    }

    void ApplyAuraDamage()
    {
        // Listeyi sondan başa doğru tarıyoruz (Çünkü içeriden eleman siliyoruz)
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            Enemy currentEnemy = enemiesInRange[i];

            // Düşman tamamen silinmişse VEYA havuza geri gönderilmişse (kapanmışsa) listeden çıkar
            if (currentEnemy == null || !currentEnemy.gameObject.activeInHierarchy)
            {
                enemiesInRange.RemoveAt(i);
                continue; // Bir sonraki düşmana geç
            }

            // Düşman hayattaysa hasar ver
            currentEnemy.TakeDamage(damage);
        }
    }

    public void IncreaseRange(float amount)
    {
        auraRange += amount;
        UpdateAuraScale();
    }

    private void UpdateAuraScale()
    {
        // Auranın hem görselinin hem de Sphere Collider'ının büyümesini sağlar
        transform.localScale = new Vector3(auraRange, auraRange, auraRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        // "Enemy" tag'i kullanmak yerine doğrudan Enemy bileşenini aramak daha güvenlidir.
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