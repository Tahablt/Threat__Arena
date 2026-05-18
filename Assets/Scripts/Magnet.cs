using UnityEngine;
using DG.Tweening;
using System.Collections.Generic; // Listeler için bu kütüphaneyi ekledik

public class Magnet : MonoBehaviour
{
    [Header("Dönme ve Süzülme Ayarları")]
    public float rotationDuration = 3f;
    public float floatHeight = 0.5f;
    public float floatDuration = 1.5f;

    [Header("Magnet Toplanma Ayarları")]
    public float scaleDuration = 0.4f;

    [Header("XP (Havuz) Çekim Ayarları")]
    public string poolObjectName = "XPPool";
    public float xpMoveDuration = 0.8f;
    public float xpScaleDuration = 0.3f;
    public float targetYOffset = 1.2f;
    public float delayBetweenXps = 0.05f;

    [Header("Ses Ayarları")]
    public AudioClip magnetSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private bool isCollected = false;

    private void Start()
    {
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
            if (magnetSound != null)
            {
                Character playerChar = other.GetComponent<Character>();
                if (playerChar != null && playerChar.audioSource != null)
                {
                    playerChar.audioSource.PlayOneShot(magnetSound, soundVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(magnetSound, transform.position, soundVolume);
                }
            }

            GetComponent<Collider>().enabled = false;

            GameObject xPool = GameObject.Find(poolObjectName);
            if (xPool != null)
            {
                // ÇÖZÜM: Önce sahnedeki tüm aktif XP'leri güvenli bir List'e kopyalıyoruz
                List<Transform> activeXPs = new List<Transform>();
                foreach (Transform child in xPool.transform)
                {
                    if (child.gameObject.activeInHierarchy)
                    {
                        activeXPs.Add(child);
                    }
                }

                // Daha sonra oluşturduğumuz bu listeden sırayla çekim işlemini başlatıyoruz
                // Böylece objenin parent'ı değişse bile döngü (sayım) bozulmuyor!
                for (int i = 0; i < activeXPs.Count; i++)
                {
                    float delay = i * delayBetweenXps;
                    MoveXpToPlayer(activeXPs[i], other.transform, xPool.transform, delay);
                }
            }

            transform.DOKill();
            transform.DOScale(Vector3.zero, scaleDuration)
                .SetEase(Ease.InOutElastic)
                .OnComplete(() => Destroy(gameObject));
        }
    }

    private void MoveXpToPlayer(Transform xp, Transform player, Transform poolRoot, float delay)
    {
        xp.DOKill();
        Vector3 originalScale = xp.localScale;
        xp.SetParent(player, true); 

        Sequence xpSequence = DOTween.Sequence();
        xpSequence.AppendInterval(delay);
        xpSequence.Append(xp.DOLocalMove(new Vector3(0, targetYOffset, 0), xpMoveDuration).SetEase(Ease.OutQuad));
        xpSequence.Join(xp.DOScale(Vector3.zero, xpScaleDuration).SetDelay(xpMoveDuration * 0.7f));

        xpSequence.OnComplete(() =>
        {
            // Boyutu ve konumu sıfırla
            xp.SetParent(poolRoot);
            xp.localScale = originalScale;
            
            // DİKKAT: XP'yi manuel olarak kapatmak yerine Havuz'un kendi sistemine teslim ediyoruz!
            if (XPPool.Instance != null)
            {
                XPPool.Instance.ReturnXP(xp.gameObject);
            }
            else
            {
                xp.gameObject.SetActive(false);
            }
        });
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}