using UnityEngine;
using UnityEngine.Audio; 
using UnityEngine.UI; 
using DG.Tweening; 

public class InGameSettings : MonoBehaviour
{
    [Header("UI Panelleri")]
    public GameObject settingsPanel;     // Komple arkaplanı karartan Canvas Paneli
    public RectTransform settingsWindow; // Küçülüp büyüyecek olan asıl pencere (Kutu)

    [Header("Ses Ayarları (Audio Mixer)")]
    public AudioMixer mainMixer; 
    public Slider musicSlider;   
    public Slider vfxSlider;     

    private bool isGamePaused = false;

    void Start()
    {
        // Başlangıçta paneli kapalı tut ve ölçeği sıfırla
        if(settingsPanel != null) settingsPanel.SetActive(false);
        if(settingsWindow != null) settingsWindow.localScale = Vector3.zero;

        // --- HAFIZADAN KAYITLI SES DEĞERLERİNİ ÇEK ---
        if (musicSlider != null)
        {
            float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.value = savedMusic;
            SetMusicVolume(savedMusic); 
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (vfxSlider != null)
        {
            float savedVFX = PlayerPrefs.GetFloat("VFXVolume", 1f);
            vfxSlider.value = savedVFX;
            SetVFXVolume(savedVFX); 
            vfxSlider.onValueChanged.AddListener(SetVFXVolume);
        }
    }

    // --- DIŞLİ BUTONUNA VEYA DIRECT AÇMAYA ATANACAK FONKSİYON ---
    public void OpenSettings()
    {
        if (isGamePaused) return; // Zaten açıksa tetikleme

        isGamePaused = true;
        settingsPanel.SetActive(true); 
        
        // Önce ölçeği sıfırla (ne olur ne olmaz) sonra büyüt
        settingsWindow.localScale = Vector3.zero;
        settingsWindow.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        
        // Animasyon başladıktan sonra oyunu durdurabiliriz
        Time.timeScale = 0f; 
    }

    // --- KAPATMA (X) BUTONUNA ATANACAK FONKSİYON ---
    public void CloseSettings()
    {
        if (!isGamePaused) return; // Zaten kapalıysa tetikleme

        isGamePaused = false;
        
        // Pencereyi küçült, bitince paneli kapat ve zamanı akıt
        settingsWindow.DOScale(0f, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => 
        {
            settingsPanel.SetActive(false); 
            Time.timeScale = 1f; 
        });
    }

    // Alternatif olarak tek butonla aç/kapat yapmak istersen (örn: ESC tuşu için)
    public void ToggleSettings()
    {
        if (isGamePaused) CloseSettings();
        else OpenSettings();
    }

    // ---------------------------------------------
    // ------------- SES İŞLEMLERİ -----------------
    // ---------------------------------------------

    public void SetMusicVolume(float sliderValue)
    {
        float value = Mathf.Max(0.0001f, sliderValue); 
        mainMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }

    public void SetVFXVolume(float sliderValue)
    {
        float value = Mathf.Max(0.0001f, sliderValue);
        mainMixer.SetFloat("VFXVol", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("VFXVolume", sliderValue);
    }
}