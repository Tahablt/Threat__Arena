using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections; // Coroutine için eklendi

public class PlayerHealth : MonoBehaviour
{
    [Header("Ayarlar")]
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isDead = false;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject gameOverPanel;

    // --- YENÝ EKLENEN KISIM: Vuruþ Hissiyatý (Hit Flash) ---
    [Header("Vurus Hissiyati (Player Hit Flash)")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;

    private Renderer[] meshRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;
    // -------------------------------------------------------

    private void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        UpdateHealthBar();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // --- RENDERER VE ORÝJÝNAL RENKLERÝ HAFIZAYA AL ---
        meshRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = meshRenderers[i].material.color;
            }
        }
        // -------------------------------------------------
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        // --- KIRMIZI PARLAMA EFEKTÝNÝ BAÞLAT ---
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
        // ---------------------------------------

        if (currentHealth <= 0) Die();
    }

    // --- ZAMANLAYICI: RENGÝ DEÐÝÞTÝR VE GERÝ AL ---
    // --- ZAMANLAYICI: RENGÝ DEÐÝÞTÝR VE GERÝ AL ---
    private IEnumerator FlashRoutine()
    {
        // 1. Karakterin tüm parçalarýný kýrmýzý yap
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            // GÜVENLÝK KONTROLÜ: Parça silinmiþse (null ise) atla!
            if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
            {
                meshRenderers[i].material.color = hitColor;
            }
        }

        // 2. Belirlenen süre kadar bekle
        yield return new WaitForSeconds(flashDuration);

        // 3. Orijinal renklerine geri döndür
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            // GÜVENLÝK KONTROLÜ: Parça silinmiþse (null ise) atla!
            if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
            {
                meshRenderers[i].material.color = originalColors[i];
            }
        }
    }
    // ----------------------------------------------
    // ----------------------------------------------

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
        Debug.Log("Can yenilendi! Mevcut Can: " + currentHealth);
    }

    void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.DOValue(currentHealth, 0.5f).SetEase(Ease.OutCubic);
        }
    }

    private void Die()
    {
        isDead = true;
        DOVirtual.DelayedCall(.5f, () =>
        {
            Time.timeScale = 0;
        });

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}