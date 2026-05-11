using UnityEngine;
using System.Collections.Generic;

public class AuraWeapon : MonoBehaviour
{
    public float damage = 10f;
    public float damageInterval = 0.5f;
    public float auraRange = 3f;

    private float timer;
    private List<Enemy> enemiesInRange = new List<Enemy>();

    void OnEnable()
    {
        // Obje açıldığında listeyi temizle ve boyutu ayarla
        enemiesInRange.Clear();
        UpdateAuraScale();
        Debug.Log("Aura Objesi Aktif Edildi!");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= damageInterval)
        {
            ApplyAuraDamage();
            timer = 0;
        }
    }

    void ApplyAuraDamage()
    {
        if (enemiesInRange.Count > 0)
        {
            Debug.Log("Menzildeki düşman sayısı: " + enemiesInRange.Count);
        }

        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] != null && enemiesInRange[i].gameObject.activeInHierarchy)
            {
                enemiesInRange[i].TakeDamage(damage);
            }
            else
            {
                enemiesInRange.RemoveAt(i);
            }
        }
    }

    public void IncreaseRange(float amount)
    {
        auraRange += amount;
        UpdateAuraScale();
    }

    private void UpdateAuraScale()
    {
        transform.localScale = new Vector3(auraRange, auraRange, auraRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
                Debug.Log("Düşman auraya girdi: " + other.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemiesInRange.Remove(enemy);
                Debug.Log("Düşman auradan çıktı.");
            }
        }
    }
}