using UnityEngine;
using DG.Tweening;

public class SpeedRunPickup : MonoBehaviour
{
    [Header("Güçlendirme Ayarları")]
    public float speedIncreaseAmount = 3f;
    public float duration = 30f;

    [Header("Dönme ve Süzülme Ayarları (Idle)")]
    public float rotationDuration = 3f;
    public float floatHeight = 0.5f;
    public float floatDuration = 1.5f;

    [Header("Toplanma Animasyonu Ayarları")]
    public float scaleDuration = 0.4f;

    [Header("Ses Ayarları")]
    public AudioClip speedSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private bool isCollected = false;

    private void Start()
    {
        // 1. Kendi etrafında sürekli dönme
        transform.DOLocalRotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);

        // 2. Yukarı-aşağı sürekli süzülme (Yoyo efekti)
        float targetY = transform.localPosition.y + floatHeight;
        transform.DOLocalMoveY(targetY, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            Character player = other.GetComponent<Character>();

            // Eğer çarpan objede Character scripti varsa işlemleri başlat
            if (player != null)
            {
                isCollected = true;

                // Çarpışmayı kapatarak birden fazla kez tetiklenmesini ve animasyon hatalarını önle
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                // --- 1. SES VE GÜÇLENDİRME İŞLEMLERİ (ANINDA) ---

                // Sesi çal (Karakterin audio source'u varsa oradan, yoksa pozisyondan)
                if (speedSound != null)
                {
                    if (player.audioSource != null)
                    {
                        player.audioSource.PlayOneShot(speedSound, soundVolume);
                    }
                    else
                    {
                        AudioSource.PlayClipAtPoint(speedSound, transform.position, soundVolume);
                    }
                }

                // Geçici hızı oyuncuya uygula
                player.ApplyTemporarySpeedBoost(speedIncreaseAmount, duration);


                // --- 2. ANİMASYON VE YOK OLMA İŞLEMLERİ ---

                // Mevcut dönme/süzülme animasyonlarını durdur
                transform.DOKill();

                // InOutElastic ile küçülme animasyonunu başlat ve bitince objeyi sil
                transform.DOScale(Vector3.zero, scaleDuration)
                    .SetEase(Ease.InOutElastic)
                    .OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
            }
        }
    }

    private void OnDestroy()
    {
        // Obje silinirken (veya sahne değişirken) arkada çalışan tween kalmasın
        transform.DOKill();
    }
}