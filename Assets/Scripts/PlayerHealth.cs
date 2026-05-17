using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Ayarlar")]
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isDead = false;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject gameOverPanel;

    [Header("Vurus Hissiyati (Player Hit Flash)")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;

    private Renderer[] meshRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;
    private MaterialPropertyBlock propBlock;

    private void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        UpdateHealthBar();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        meshRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[meshRenderers.Length];
        propBlock = new MaterialPropertyBlock();

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].sharedMaterial != null && meshRenderers[i].sharedMaterial.HasProperty("_Color"))
            {
                originalColors[i] = meshRenderers[i].sharedMaterial.color;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].sharedMaterial.HasProperty("_Color"))
            {
                meshRenderers[i].GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", hitColor);
                meshRenderers[i].SetPropertyBlock(propBlock);
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].sharedMaterial.HasProperty("_Color"))
            {
                meshRenderers[i].GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", originalColors[i]);
                meshRenderers[i].SetPropertyBlock(propBlock);
            }
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
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
        DOVirtual.DelayedCall(.5f, () => { Time.timeScale = 0; });

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}