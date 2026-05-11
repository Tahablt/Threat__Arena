using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Ayarlari")]
    public float slimeSpawnRate = 2f;
    public float rammusSpawnRate = 8f;
    public float spawnRadius = 10f;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        InvokeRepeating("SpawnSlime", 1f, slimeSpawnRate);
        InvokeRepeating("SpawnRammus", 1f, rammusSpawnRate);
    }

    void SpawnSlime()
    {
        SpawnEnemy(EnemyType.Slime);
    }

    void SpawnRammus()
    {
        SpawnEnemy(EnemyType.Turtle);
    }

    void SpawnEnemy(EnemyType type)
    {
        if (EnemyPool.Instance == null) return;
        
        GameObject enemy = EnemyPool.Instance.GetEnemy(type);

        if (enemy != null)
        {
            enemy.transform.position = GetRandomSpawnPosition();
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * spawnRadius;
        // Yari capi belirlerken kati bir Zemin Cizgisi (0) veriyoruz. 
        // Sayet Raycast ve character merkezi onlari havada kilitliyorsa en kesin cozum Y = 0 demektir.
        // Eger modelinin pivot'u (merkezi) tam ortasindaysa 0.5f gibi ufak bir offset gerekirse buraya ekle.
        return new Vector3(player.position.x + randomPoint.x, 0.5f, player.position.z + randomPoint.y);
    }
}
