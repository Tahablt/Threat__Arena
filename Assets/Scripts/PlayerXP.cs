using UnityEngine;
using UnityEngine.UI;

public class PlayerXP : MonoBehaviour
{
    [Header("XP Ayarları")]
    public int currentLevel = 1;
    public float currentXP = 0f;
    public float xpToNextLevel = 100f;

    [Header("UI")]
    public Image xpBarImage;

    // --- EKSİK OLAN KISIM BURASIYDI ---
    [Header("Managerlar")]
    public UpgradeManager upgradeManager;

    private void Start() 
    { 
        if (xpBarImage == null)
        {
            Debug.LogError("🔴 DIKKAT: PlayerXP icindeki xpBarImage bos! Lutfen Unity editorunden karakterindeki PlayerXP scriptine UI XP barinin 'DOLAN RENKLI KISMINI (FILL)' surukle! Arka plani eklersen calismaz!");
        }

        currentXP = 0f; 
        UpdateXPBar(); 
    }

    public void AddXP(float amount)
    {
        currentXP += amount;

        // Seviye atlama kontrolü
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        UpdateXPBar();
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.Round(xpToNextLevel * 1.2f);

        Debug.Log("LEVEL UP! Yeni Seviye: " + currentLevel);

        // Upgrade menüsünü aç
        if (upgradeManager != null)
        {
            upgradeManager.ShowUpgradeMenu();
        }
        else
        {
            Debug.LogError("PlayerXP içerisinde UpgradeManager atanmamış!");
        }
    }

    void UpdateXPBar()
    {
        if (xpBarImage != null)
        {
            xpBarImage.fillAmount = currentXP / xpToNextLevel;
        }
    }
}
