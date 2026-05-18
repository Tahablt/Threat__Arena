using UnityEngine;
using TMPro;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance;

    [Header("UI Elementleri")]
    public TextMeshProUGUI killCountText;     // Ekranda "Kesilen Mob: 0" yazacak yer
    public GameObject newRecordPanel;         // Rekor kırıldığında açılacak Canvas
    public TextMeshProUGUI highRecordText; 
    public TextMeshProUGUI killCountTextPanel;    

    private int totalKills = 0;
    private int bestKills = 0;
    private bool recordAnnounced = false;

    private void Awake()
    {
        // --- 1. GÜVENLİ SINGLETON ---
        // Sahnede birden fazla KillManager olmasını kesin olarak engeller
        if (Instance == null) 
        {
            Instance = this;
        } 
        else 
        {
            Destroy(gameObject);
            return;
        }

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
            // İlk oyunda değil, sadece var olan bir rekor geçilince paneli göster
            if (!recordAnnounced && bestKills > 0) 
            {
                recordAnnounced = true;
                ShowRecordUI();
            }

            bestKills = totalKills;
            PlayerPrefs.SetInt("BestKills", bestKills); // Yeni rekoru anında kaydet
        }
    }

    private void UpdateUI()
    {
        if (killCountText != null) killCountText.text = "Kesilen Mob: " + totalKills;
        if (killCountTextPanel != null) killCountTextPanel.text = "Skor: " + totalKills;
    }

    private void ShowRecordUI()
    {
        if (newRecordPanel != null)
        {
            newRecordPanel.SetActive(true);
            if (highRecordText != null) highRecordText.text = "YENİ REKOR!";
            
            // --- 2. OPTİMİZASYON: String yerine nameof kullanımı ---
            // İleride HideRecordUI adını değiştirirsen Unity artık seni uyaracaktır.
            Invoke(nameof(HideRecordUI), 3f); 
        }
    }

    private void HideRecordUI()
    {
        if (newRecordPanel != null) newRecordPanel.SetActive(false);
    }

    // YENİ: Başka scriptlerden (örneğin oyun sonu ekranında) toplam skoru çekebilmen için eklendi
    public int GetTotalKills()
    {
        return totalKills;
    }
}