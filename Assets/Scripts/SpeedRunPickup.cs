using UnityEngine;
using DG.Tweening;

public class SpeedRunPickup : MonoBehaviour
{
    [Header("Güçlendirme Ayarları")]
    public float speedIncreaseAmount = 3f;
    public float duration = 30f;

    [Header("Dönme ve Süzülme Ayarları")]
    public float rotationDuration = 3f;
    public float floatHeight = 0.5f;
    public float floatDuration = 1.5f;

    [Header("Toplanma Animasyonu Ayarları")]
    public float scaleDuration = 0.15f;

    [Header("Ses Ayarları")]
    public AudioClip speedSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private bool isCollected = false;

    private void Start()
    {
        // Sürekli dönme
        transform.DOLocalRotate(
            new Vector3(0, 360, 0),
            rotationDuration,
            RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);

        // Yukarı aşağı süzülme
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

            if (player != null)
            {
                isCollected = true;

                // Collider kapat
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                // Ses çal
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

                // Speed boost uygula
                player.ApplyTemporarySpeedBoost(speedIncreaseAmount, duration);

                // Aktif tweenleri tamamen temizle
                transform.DOComplete();
                transform.DOKill();

                // Hızlı küçülüp yok ol
                transform.DOScale(Vector3.zero, scaleDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
            }
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}