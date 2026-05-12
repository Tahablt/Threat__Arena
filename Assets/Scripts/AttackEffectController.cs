using UnityEngine;

public class AttackEffectController : MonoBehaviour
{
    [Header("Efekt Ayarları")]
    public GameObject slashEffectPrefab; // Adım 3'teki prefab'ı buraya ata
    public Transform spawnPoint;         // Adım 1.3'teki SlashVFX_SpawnPoint'i ata

    // Bu fonksiyon Animation Event tarafından tetiklenecek
    public void SpawnSlashEffect()
    {
        if (slashEffectPrefab == null || spawnPoint == null) return;

        // Efekti oluştur
        GameObject effect = Instantiate(slashEffectPrefab, spawnPoint.position, spawnPoint.rotation);

        // Opsiyonel: Efekti bir süre sonra koda gerek kalmadan DestroyAfterTime ile silmek daha iyidir
    }
}