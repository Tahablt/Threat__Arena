using UnityEngine;
using TMPro;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance;

    [Header("UI Elementleri (Oyun İçi)")]
    public TextMeshProUGUI killCountText;

    [Header("UI Elementleri (Game Over Paneli)")]
    public TextMeshProUGUI killCountTextPanel;
    public TextMeshProUGUI timeSurvivedTextPanel;
    public TextMeshProUGUI levelTextPanel;

    // --- TAKİP EDİLEN DEĞERLER ---
    private int totalKills = 0;
    private int bestKills = 0; // Sadece hafızada tutar, ekranda göstermez

    private float timeSurvived = 0f;
    private int currentLevel = 1;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        bestKills = PlayerPrefs.GetInt("BestKills", 0);
        UpdateInGameUI();
    }

    private void Update()
    {
        // Oyun bitmediği sürece zamanı say
        if (!isGameOver)
        {
            timeSurvived += Time.deltaTime;
        }
    }

    // ---------------------------------------------
    // ------------- OYUN İÇİ İŞLEMLER -------------
    // ---------------------------------------------

    public void AddKill()
    {
        if (isGameOver) return;

        totalKills++;
        UpdateInGameUI();

        // Rekoru arka planda sessizce kaydet
        if (totalKills > bestKills)
        {
            bestKills = totalKills;
            PlayerPrefs.SetInt("BestKills", bestKills);
        }
    }

    // BAŞKA SCRİPTTEN ÇAĞRILACAK OLAN LEVEL GÜNCELLEME FONKSİYONU
    public void UpdateLevel(int newLevel)
    {
        currentLevel = newLevel;
    }

    private void UpdateInGameUI()
    {
        if (killCountText != null) killCountText.text = "Kesilen Mob: " + totalKills;
    }

    // ---------------------------------------------
    // ------------- OYUN SONU (GAME OVER) ---------
    // ---------------------------------------------

    public void TriggerGameOver()
    {
        isGameOver = true; // Sayacı durdur

        if (killCountTextPanel != null)
            killCountTextPanel.text = "Skor : " + totalKills;

        if (levelTextPanel != null)
            levelTextPanel.text = "Level : " + currentLevel;

        if (timeSurvivedTextPanel != null)
        {
            int minutes = Mathf.FloorToInt(timeSurvived / 60);
            int seconds = Mathf.FloorToInt(timeSurvived % 60);
            timeSurvivedTextPanel.text = string.Format("Hayatta Kalinan Sure : {0:00}:{1:00}", minutes, seconds);
        }
    }

    public int GetTotalKills()
    {
        return totalKills;
    }
}