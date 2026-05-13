using UnityEngine;
using System.Collections.Generic;

public class ArrowPool : MonoBehaviour
{
    public static ArrowPool Instance; // Diğer scriptlerden kolayca ulaşmak için

    [Header("Havuz Ayarları")]
    public GameObject arrowPrefab;
    public int initialPoolSize = 20; // Başlangıçta kaç ok hazırda beklesin?

    private Queue<GameObject> pooledArrows = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
        // Başlangıçta okları oluştur ve sakla
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewArrow();
        }
    }

    private void CreateNewArrow()
    {
        GameObject obj = Instantiate(arrowPrefab);
        obj.SetActive(false); // Görünmez yap
        obj.transform.SetParent(transform); // Kalabalık yapmasın diye havuzun içine koy
        pooledArrows.Enqueue(obj);
    }

    // Havuzdan ok çekme
    public GameObject GetArrow()
    {
        if (pooledArrows.Count == 0)
        {
            CreateNewArrow(); // Havuz boşaldıysa yeni bir tane ekle
        }

        GameObject obj = pooledArrows.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    // Oku havuza geri gönderme
    public void ReturnArrow(GameObject obj)
    {
        obj.SetActive(false);
        pooledArrows.Enqueue(obj);
    }
}