using System.Collections.Generic;
using UnityEngine;

public class XPPool : MonoBehaviour
{
    public static XPPool Instance;

    public GameObject xpPrefab; // Burayi editor'den doldurmasan bile artik cokmeyecek.
    public int poolSize = 50;

    private Queue<GameObject> pool;
    private bool isInitialized = false;

    private void Awake()
    {
        Instance = this;
        pool = new Queue<GameObject>();

        // Eger Editor icerisinden xpPrefab suruklenmediyse, oyunun basinda cokmesini engelliyoruz!
        if (xpPrefab != null)
        {
            InitializePool(xpPrefab);
        }
    }

    public void InitializePool(GameObject fallbackPrefab)
    {
        if (isInitialized || fallbackPrefab == null) return;
        
        // Eger bastan xpPrefab atanmadiysa, dusmandan alarak kendisini programli olarak tamir eder.
        xpPrefab = fallbackPrefab;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject xp = Instantiate(xpPrefab, transform);
            xp.SetActive(false);
            pool.Enqueue(xp);
        }
        
        isInitialized = true;
    }

    public GameObject GetXP(GameObject fallbackPrefab)
    {
        // Havuz baslangicta kurulamadisa, ucan kuştan bile prefab alip kendini otomatik tamir eder
        if (!isInitialized && fallbackPrefab != null)
        {
            InitializePool(fallbackPrefab);
        }

        while (pool.Count > 0)
        {
            GameObject xp = pool.Dequeue();
            if (xp != null)
            {
                xp.SetActive(true);
                return xp;
            }
        }

        // Havuz tukendiyse (100 dusman ayni anda öldü vs), sistemi cökertmemek icin manuel yarat.
        if (xpPrefab != null)
        {
            GameObject newXp = Instantiate(xpPrefab, transform);
            newXp.SetActive(true);
            return newXp;
        }
        else if (fallbackPrefab != null)
        {
            GameObject newXp = Instantiate(fallbackPrefab, transform);
            newXp.SetActive(true);
            return newXp;
        }

        return null;
    }

    public void ReturnXP(GameObject xp)
    {
        xp.SetActive(false);
        pool.Enqueue(xp);
    }
}
