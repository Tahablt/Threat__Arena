using System.Collections;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public string waveName;
    public int enemyCount;
    public float spawnRate;

    public EnemyType[] allowedEnemies; // A�ILIR MEN� L�STES�
}

public class WaveManager : MonoBehaviour
{
    public Wave[] waves;
    public Transform[] spawnPoints;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;

    private void Start()
    {
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        yield return new WaitForSeconds(2f);

        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log("T�m dalgalar bitti! Oyuncu Kazand�!");
            yield break;
        }

        Wave currentWave = waves[currentWaveIndex];

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            SpawnEnemy(currentWave);
            yield return new WaitForSeconds(1f / currentWave.spawnRate);
        }
    }

    void SpawnEnemy(Wave currentWave)
    {
        if (EnemyPool.Instance == null) return;

        EnemyType randomType = currentWave.allowedEnemies[Random.Range(0, currentWave.allowedEnemies.Length)];
        GameObject enemy = EnemyPool.Instance.GetEnemy(randomType);

        if (enemy == null) return;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Dusmanlarin ayni noktada ust uste binmesini ve fizikten dolayi havaya firlamasini engellemek icin offset
            Vector2 randomSpread = Random.insideUnitCircle * 1.5f;

            // Havadan dusmelerini engellemek icin Y = 0.5f veriyoruz (ayni zamanda yerin icine girip firlamamalari icin)
            enemy.transform.position = new Vector3(
                randomSpawnPoint.position.x + randomSpread.x, 
                0.5f, 
                randomSpawnPoint.position.z + randomSpread.y
            );
            enemy.transform.rotation = randomSpawnPoint.rotation;
        }

        enemiesAlive++;
    }

    public void OnEnemyDefeated()
    {
        enemiesAlive--;

        if (enemiesAlive <= 0)
        {
            enemiesAlive = 0;
            currentWaveIndex++;
            StartCoroutine(StartWave());
        }
    }
}