using System.Collections;
using UnityEngine;
using TMPro; // YENİ: TextMeshPro kütüphanesini ekledik

[System.Serializable]
public class Wave
{
    public string waveName;
    public int enemyCount;
    public float spawnRate;
    public EnemyType[] allowedEnemies;
}

public class WaveManager : MonoBehaviour
{
    public Wave[] waves;
    public Transform[] spawnPoints;

    [Header("Arayüz (UI) Ayarları")]
    public TextMeshProUGUI timerText; // YENİ: Ekrana süreyi yazdıracağımız Text referansı

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;

    private float gameTimer = 0f;
    private bool isHordeModeActive = false;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        // 1. Zamanı saniye cinsinden saydır
        gameTimer += Time.deltaTime;

        // YENİ: 2. Ekranda süreyi güncelle (Dakika:Saniye formatında)
        UpdateTimerUI();

        // 3. Dakika 4 (240 Saniye) Kontrolü - Sürü Başlangıcı
        if (gameTimer >= 240f && !isHordeModeActive)
        {
            TriggerHordeMode();
        }

        // 4. Dalgaları otomatik atlat
        if (!isHordeModeActive && waves.Length > 0)
        {
            currentWaveIndex = Mathf.FloorToInt(gameTimer / 60f);
            currentWaveIndex = Mathf.Clamp(currentWaveIndex, 0, waves.Length - 1);
        }
    }

    // YENİ: Süreyi ekrana yazdıran fonksiyon
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Toplam saniyeyi dakika ve saniyeye böl
            int minutes = Mathf.FloorToInt(gameTimer / 60F);
            int seconds = Mathf.FloorToInt(gameTimer - minutes * 60);

            // "00:00" formatında yazdır (Örn: 03:05, 04:20)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (waves.Length == 0) yield break;

            Wave currentWave = waves[currentWaveIndex];
            int spawnAmount = isHordeModeActive ? 3 : 1;

            for (int i = 0; i < spawnAmount; i++)
            {
                SpawnEnemy(currentWave);
            }

            float delay = 1f / currentWave.spawnRate;
            if (isHordeModeActive) delay /= 2f;

            yield return new WaitForSeconds(delay);
        }
    }

    void TriggerHordeMode()
    {
        isHordeModeActive = true;
        Debug.Log("4. DAKİKA DOLDU! SÜRÜ GELİYOR!");
        currentWaveIndex = waves.Length - 1;

        // Opsiyonel: 4. Dakika geldiğinde süre yazısı kırmızı olsun
        if (timerText != null) timerText.color = Color.red;
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
            Vector2 randomSpread = Random.insideUnitCircle * 1.5f;

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
        if (enemiesAlive < 0) enemiesAlive = 0;
    }
}