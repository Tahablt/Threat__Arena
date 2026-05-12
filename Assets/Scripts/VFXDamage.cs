using UnityEngine;
using System.Collections.Generic;

public class VFXDamage : MonoBehaviour
{
    [Header("Saldırı Ayarları")]
    public float damage = 20f;

    private List<Collider> alreadyHit = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        // 1. AŞAMA: Fizik çalışıyor mu? (VFX bir şeye fiziksel olarak değiyor mu?)
        Debug.Log("<color=yellow>VFX ÇARPTI:</color> " + other.gameObject.name);

        // 2. AŞAMA: Tag doğru mu?
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("<color=green>TAG DOĞRU!</color> Düşman tag'i algılandı.");

            // 3. AŞAMA: Enemy scriptini bulabiliyor mu? 
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = other.GetComponentInParent<Enemy>();
            }

            if (enemy != null)
            {
                if (!alreadyHit.Contains(other))
                {
                    Debug.Log("<color=cyan>BAŞARILI!</color> Hasar veriliyor: " + enemy.gameObject.name);
                    enemy.TakeDamage(damage);
                    alreadyHit.Add(other);
                }
            }
            else
            {
                Debug.Log("<color=red>HATA:</color> Objenin Tag'i Enemy ama üzerinde Enemy scripti YOK!");
            }
        }
        else
        {
            Debug.Log("<color=orange>UYARI:</color> Çarpılan objenin Tag'i Enemy DEĞİL. Etiket: " + other.tag);
        }
    }
}