using UnityEngine;
using TMPro;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance;

    [Header("UI Elementleri")]
    public TextMeshProUGUI killCountText;     // Ekranda "Kills: 0" yazacak yer
    public GameObject newRecordPanel;         // Rekor kırıldığında açılacak Canvas
    public TextMeshProUGUI highRecordText;    // Rekor panelindeki yazı

    private int totalKills = 0;
    private int bestKills = 0;
    private bool recordAnnounced = false;

    private void Awake()
    {
        Instance = this;
        // Kayıtlı en iyi öldürme sayısını çek
        bestKills = PlayerPrefs.GetInt("BestKills", 0);
        UpdateUI();
        if (newRecordPanel != null) newRecordPanel.SetActive(false);
    }

    public void AddKill()
    {
        totalKills++;
        UpdateUI();

        // Eğer mevcut öldürme sayısı eski rekoru geçtiyse
        if (totalKills > bestKills)
        {
            if (!recordAnnounced && bestKills > 0) // İlk oyunda değil, sadece rekor geçilince
            {
                recordAnnounced = true;
                ShowRecordUI();
            }

            bestKills = totalKills;
            PlayerPrefs.SetInt("BestKills", bestKills); // Yeni rekoru kaydet
        }
    }

    void UpdateUI()
    {
        if (killCountText != null) killCountText.text = "Kills: " + totalKills;
    }

    void ShowRecordUI()
    {
        if (newRecordPanel != null)
        {
            newRecordPanel.SetActive(true);
            if (highRecordText != null) highRecordText.text = "NEW RECORD!";
            Invoke("HideRecordUI", 3f); // 3 saniye sonra kapat
        }
    }

    void HideRecordUI()
    {
        if (newRecordPanel != null) newRecordPanel.SetActive(false);
    }
}