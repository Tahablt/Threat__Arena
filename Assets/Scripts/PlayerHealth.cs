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
    [SerializeField] private AudioClip deathSound;

    private Renderer[] meshRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;
    private MaterialPropertyBlock propBlock;
    private Animator animator;
    private AudioSource audioSource;

    private void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        UpdateHealthBar();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

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

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

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

        // KillManager'daki Oyun Sonu fonksiyonunu çağırarak süre ve skoru yazdırıyoruz
        if (KillManager.Instance != null)
        {
            KillManager.Instance.TriggerGameOver();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1); // 1 Numaralı Game sahnesini yeniden yükler
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0); // 0 Numaralı Ana Menü sahnesini yükler
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