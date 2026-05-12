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

        // Efektin scale değerini spawnPoint'in scale değerine eşitliyoruz
        effect.transform.localScale = spawnPoint.localScale;

        /* ÖNEMLİ NOT: 
        Eğer spawnPoint karakterin bir alt objesiyse (örneğin el kemiğine bağlıysa) 
        ve karakterinin veya üst objelerin scale değerleri 1'den farklıysa, 
        dünya koordinatlarındaki gerçek boyutu (world scale) almak için aşağıdaki 
        kodu kullanman daha sağlıklı sonuç verecektir:
        
        effect.transform.localScale = spawnPoint.lossyScale;
        */
    }
}