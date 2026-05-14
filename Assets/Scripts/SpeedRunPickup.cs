using UnityEngine;
using DG.Tweening; // DOTween kütüphanesini dahil ediyoruz

public class SpeedRunPickup : MonoBehaviour
{
    [Header("Dönme ve Süzülme Ayarları (Idle)")]
    public float rotationDuration = 3f;  // Kendi etrafında tam tur süresi
    public float floatHeight = 0.5f;      // Ne kadar yukarı çıkacak
    public float floatDuration = 1.5f;   // Yukarı-aşağı gidiş süresi

    [Header("Toplanma Animasyonu Ayarları")]
    public float scaleDuration = 0.4f;   // Küçülüp yok olma süresi

    private bool isCollected = false;

    private void Start()
    {
        // =========================================================
        // 1. BAŞLANGIÇ (IDLE) ANİMASYONLARI (Magnet ile Aynı)
        // =========================================================

        // Kendi etrafında sürekli dönme
        transform.DOLocalRotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);

        // Yukarı-aşağı sürekli süzülme (Yoyo efekti)
        float targetY = transform.localPosition.y + floatHeight;
        transform.DOLocalMoveY(targetY, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece oyuncu çarparsa ve henüz toplanmadıysa çalış
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;

            // --- HATA ÖNLEME ---
            // Elastic animasyon eksi değere düşerse BoxCollider hata vermesin diye çarpışmayı kapat
            if (GetComponent<Collider>() != null)
            {
                GetComponent<Collider>().enabled = false;
            }

            // =========================================================
            // 2. HIZLI KOŞMA OBJESİNİN TOPLANMA ANİMASYONU
            // =========================================================

            // Üzerindeki dönme ve süzülme animasyonlarını durdur
            transform.DOKill();

            // Olduğu yerde InOutElastic ile küçülerek yok ol
            transform.DOScale(Vector3.zero, scaleDuration)
                .SetEase(Ease.InOutElastic)
                .OnComplete(() =>
                {
                    // Buraya ileride oyuncuya hız verme kodunu ekleyebilirsin.
                    // Örneğin: other.GetComponent<PlayerMovement>().ApplySpeedBoost();

                    Destroy(gameObject); // Obje sil
                });
        }
    }

    private void OnDestroy()
    {
        // Obje silinirken arkada çalışan tween kalmasın, hafıza sızıntısını önle
        transform.DOKill();
    }
}