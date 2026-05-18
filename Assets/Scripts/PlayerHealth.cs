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

    public bool IsDead => isDead;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject gameOverPanel;

    [Header("Vurus Hissiyati (Player Hit Flash)")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;

    [Header("Ses Efektleri")]
    [SerializeField] private AudioClip deathSound; // YENİ: Ölüm ses klibi alanı

    private Renderer[] meshRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;
    private MaterialPropertyBlock propBlock;
    private Animator animator; // YENİ: Ölme animasyonunu tetiklemek için animator referansı
    private AudioSource audioSource; // YENİ: Ölme sesini çalmak için ses kaynağı referansı

    private void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        UpdateHealthBar();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Referansları alıyoruz
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

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
            healthSlider.DOValue(currentHealth, 0.25f).SetEase(Ease.OutCubic).SetLink(healthSlider.gameObject);
        }
    }

    private void Die()
    {
        isDead = true;

        // YENİ: Ölme ses efekti oynatımı
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // YENİ: Ölüm animasyonunun tetiklenmesi
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        DOVirtual.DelayedCall(2f, ShowDie, true).SetLink(gameObject);
    }

    public void ShowDie()
    {
        Time.timeScale = 0; 
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (healthSlider != null)
        {
            healthSlider.DOKill();
        }
        transform.DOKill();
    }
}