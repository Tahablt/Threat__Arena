using UnityEngine;
using DG.Tweening; 
using UnityEngine.Audio; 
using UnityEngine.UI; 
using UnityEngine.SceneManagement; // Sahneler arası geçiş için YENİ EKLENDİ

public class MenuController : MonoBehaviour
{
    [Header("Ana Menü Butonları")]
    public RectTransform[] buttons; 
    public float animationDuration = 0.5f;

    [Header("Ayarlar Paneli (UI)")]
    public GameObject settingsPanel;     
    public RectTransform settingsWindow; 

    [Header("Ses Ayarları (Audio Mixer)")]
    public AudioMixer mainMixer; 
    public Slider musicSlider;   
    public Slider vfxSlider;     
    
    [Header("Buton Efektleri")]
    public AudioSource buttonClickSource; 

    void Start()
    {
        if(settingsPanel != null) settingsPanel.SetActive(false);
        if(settingsWindow != null) settingsWindow.localScale = Vector3.zero;

        foreach (var btn in buttons)
        {
            btn.localScale = Vector3.zero;
        }

        AnimateMenu();

        if(musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if(vfxSlider != null) vfxSlider.onValueChanged.AddListener(SetVFXVolume);
    }

    void AnimateMenu()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].DOScale(1f, animationDuration)
                .SetDelay(i * 0.15f) 
                .SetEase(Ease.OutBack); 
        }
    }

    public void OnButtonClickScale(RectTransform btn)
    {
        btn.DOScale(0.9f, 0.1f).OnComplete(() => btn.DOScale(1f, 0.1f));

        if (buttonClickSource != null)
        {
            buttonClickSource.Play();
        }
    }

    // ---------------------------------------------
    // ---------- OYUN BAŞLATMA VE ÇIKIŞ -----------
    // ---------------------------------------------

    // "Başla" butonuna bağlanacak
    public void PlayGame()
    {
        // 1. İndeksteki sahneyi (Oyun sahnemizi) yükler
        SceneManager.LoadScene(1);
    }

    // "Çıkış" butonuna bağlanacak
    public void ExitGame()
    {
        // Unity Editöründe çıkış yapıldığını görmek için konsola yazdırıyoruz
        Debug.Log("Oyundan Çıkılıyor..."); 
        
        // Build alınmış (telefona yüklenmiş) oyunu tamamen kapatır
        Application.Quit(); 
    }

    // ---------------------------------------------
    // ---------- AYARLAR MENÜSÜ İŞLEMLERİ ---------
    // ---------------------------------------------

    public void OpenSettings()
    {
        settingsPanel.SetActive(true); 
        settingsWindow.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    public void CloseSettings()
    {
        settingsWindow.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() => 
        {
            settingsPanel.SetActive(false); 
        });
    }

    // ---------------------------------------------
    // ------------- SES İŞLEMLERİ -----------------
    // ---------------------------------------------

    public void SetMusicVolume(float sliderValue)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
    }

    public void SetVFXVolume(float sliderValue)
    {
        mainMixer.SetFloat("VFXVol", Mathf.Log10(sliderValue) * 20);
    }
}