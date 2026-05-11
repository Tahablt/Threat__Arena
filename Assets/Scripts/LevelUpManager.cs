using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    public GameObject levelUpPanel; // UI Panel
    public GameObject player; // Kýlýç/Oyuncu özelliklerini artýrmak için

    public void ShowLevelUpPanel()
    {
        Time.timeScale = 0; // Oyunu durdur
        levelUpPanel.SetActive(true);
    }

    // Butonlarýn çaðýracaðý fonksiyonlar
    public void SelectUpgrade(int upgradeType)
    {
        // 0: Hasarý Artýr, 1: Hýzý Artýr, 2: Canýný Yenile
        if (upgradeType == 0) player.GetComponent<MeleeWeapon>().damage += 5f;
        // ... (Diðer özellikler)

        ResumeGame();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        levelUpPanel.SetActive(false);
    }
}