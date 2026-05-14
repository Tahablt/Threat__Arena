using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speedIncreaseAmount = 3f; // Süreli olduğu için hızı daha fazla verebilirsin (Örn: 3)
    public float duration = 30f;           // Hız botu kaç saniye aktif kalacak?

    private void OnTriggerEnter(Collider other)
    {
        // Sadece Player çarptığında çalışsın
        if (other.CompareTag("Player"))
        {
            // Karakter scriptini bul
            Character player = other.GetComponent<Character>();

            if (player != null)
            {
                // DÜZELTİLEN KISIM: Artık kalıcı hızı değil, 30 saniyelik yeni fonksiyonu çağırıyoruz!
                player.ApplyTemporarySpeedBoost(speedIncreaseAmount, duration);

                // Belki bir ses çalabilirsin (AudioSource varsa)
                // AudioSource.PlayClipAtPoint(speedSound, transform.position);

                // Nesneyi yok et
                Destroy(gameObject);
            }
        }
    }
}