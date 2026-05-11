using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour
{
    [Header("Ayarlar")]
    public float maxHealth = 100f;
    public float currentHealth; // Dýþarýdan eriþmek için public yaptým
    private bool isDead = false;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject gameOverPanel;


    private void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        UpdateHealthBar();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0) Die();
    }

    // --- UpgradeManager'ýn kullandýðý Heal fonksiyonu ---
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
            //healthSlider.value = currentHealth;

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