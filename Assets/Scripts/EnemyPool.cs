using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public EnemyType enemyType; // AÇILIR MENÜ
    public GameObject prefab;
    public int poolSize = 20;
}

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    public List<PoolItem> poolItems;

    // Arka plan sözlüðümüz de artýk yazýyla deðil, Enum (Liste) ile çalýþýyor
    private Dictionary<EnemyType, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<EnemyType, Queue<GameObject>>();

        foreach (var item in poolItems)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < item.poolSize; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(item.enemyType, objectPool);
        }
    }

    public GameObject GetEnemy(EnemyType type)
    {
        if (!poolDictionary.ContainsKey(type) || poolDictionary[type].Count == 0)
        {
            Debug.LogWarning(type + " havuzunda mob kalmadý!");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[type].Dequeue();
        objectToSpawn.SetActive(true);
        return objectToSpawn;
    }

    public void ReturnEnemy(GameObject enemy, EnemyType type)
    {
        enemy.SetActive(false);
        if (poolDictionary.ContainsKey(type))
        {
            poolDictionary[type].Enqueue(enemy);
        }
    }
}