using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speedIncreaseAmount = 3f; // Süreli olduğu için 0.5 az kalır, 3 falan yap ki hissettirsin
    public float duration = 30f;           // 30 Saniye kuralı

    [Header("Ses Ayarları")]
    public AudioClip speedSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character player = other.GetComponent<Character>();

            if (player != null)
            {
                // 1. SES ÇÖZÜMÜ: Sesi yerde değil, karakterin kulağının dibinde (2D) tam güç çalıyoruz!
                if (speedSound != null && player.audioSource != null)
                {
                    player.audioSource.PlayOneShot(speedSound, soundVolume);
                }
                else if (speedSound != null)
                {
                    // Eğer karakterde hoparlör yoksa mecbur eski yöntemle çalar
                    AudioSource.PlayClipAtPoint(speedSound, transform.position, soundVolume);
                }

                // 2. SÜRE ÇÖZÜMÜ: Kalıcı hız yerine, 30 saniyelik geçici hızı tetikliyoruz!
                player.ApplyTemporarySpeedBoost(speedIncreaseAmount, duration);

                Destroy(gameObject);
            }
        }
    }
}