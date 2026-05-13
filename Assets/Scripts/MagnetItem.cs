using UnityEngine;

public class MagnetItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Sadece Player çarptığında çalışsın
        if (other.CompareTag("Player"))
        {
            // Sahnede o an aktif olan tüm XP'leri bul
            XPMagnet[] allActiveXPs = FindObjectsByType<XPMagnet>(FindObjectsSortMode.None);

            // Hepsine "Karaktere Doğru Uç" emri ver
            foreach (XPMagnet xp in allActiveXPs)
            {
                xp.StartGlobalMagnet(other.transform);
            }

            Debug.Log("MIKNATIS ALINDI! Sahnede " + allActiveXPs.Length + " adet XP karaktere uçuyor.");

            // Mıknatıs objesini yokederek sahneden kaldır
            Destroy(gameObject);
        }
    }
}