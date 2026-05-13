using UnityEngine;
using DG.Tweening;

public class Magnet : MonoBehaviour
{
    [Header("Dönme ve Süzülme Ayarları")]
    public float rotationDuration = 3f;
    public float floatHeight = 0.5f;
    public float floatDuration = 1.5f;

    [Header("Magnet Toplanma Ayarları")]
    public float scaleDuration = 0.4f;

    [Header("XP (Havuz) Çekim Ayarları")]
    public string poolObjectName = "XPool";
    public float xpMoveDuration = 0.8f;      // Daha yavaş ve smooth olması için süre artırıldı
    public float xpScaleDuration = 0.3f;
    public float targetYOffset = 1.2f;       // Oyuncunun ne kadar üstüne gidecekler
    public float delayBetweenXps = 0.05f;    // XP'lerin sırayla gelmesi için aradaki gecikme

    private bool isCollected = false;

    private void Start()
    {
        // Başlangıç animasyonları
        transform.DOLocalRotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

        float targetY = transform.localPosition.y + floatHeight;
        transform.DOLocalMoveY(targetY, floatDuration)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;

            GameObject xPool = GameObject.Find(poolObjectName);
            if (xPool != null)
            {
                int index = 0;
                foreach (Transform child in xPool.transform)
                {
                    if (child.gameObject.activeInHierarchy)
                    {
                        // Her XP için küçük bir gecikme hesapla (Sırayla uçmaları için)
                        float delay = index * delayBetweenXps;
                        MoveXpToPlayer(child, other.transform, xPool.transform, delay);
                        index++;
                    }
                }
            }

            // Magnetin kendi yok olma animasyonu
            transform.DOKill();
            transform.DOScale(Vector3.zero, scaleDuration)
                .SetEase(Ease.InOutElastic)
                .OnComplete(() => Destroy(gameObject));
        }
    }

    private void MoveXpToPlayer(Transform xp, Transform player, Transform poolRoot, float delay)
    {
        xp.DOKill();

        // Oyuncuyu takip etmesi için önce player'ın child'ı yapıyoruz
        // worldPositionStays: true sayesinde yerinden zıplamaz
        xp.SetParent(player, true);

        Sequence xpSequence = DOTween.Sequence();

        // 1. Gecikme ekle (Hepsini aynı anda başlatmaz)
        xpSequence.AppendInterval(delay);

        // 2. Hareket animasyonu (LocalMove kullanarak oyuncunun kafasına hedefler)
        xpSequence.Append(xp.DOLocalMove(new Vector3(0, targetYOffset, 0), xpMoveDuration)
            .SetEase(Ease.OutQuad)); // OutQuad daha yumuşak bir yavaşlama sağlar

        // 3. Küçülme animasyonu
        xpSequence.Join(xp.DOScale(Vector3.zero, xpScaleDuration).SetDelay(xpMoveDuration * 0.7f));

        xpSequence.OnComplete(() =>
        {
            xp.gameObject.SetActive(false);
            xp.SetParent(poolRoot);
            xp.localScale = Vector3.one;
        });
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}