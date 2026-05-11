using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI Paneli")]
    public GameObject levelUpPanel;
    public UpgradeCard[] cards;

    [Header("Veri")]
    public PlayerSaveData saveData;
    public PlayerHealth playerHealth;
    public Character playerCharacter;

    private List<ItemData> rastgeleItemler;

    private void Start()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }

    public void ShowUpgradeMenu()
    {
        Time.timeScale = 0;
        levelUpPanel.SetActive(true);

        rastgeleItemler = new List<ItemData>();
        List<ItemData> availableItems = new List<ItemData>(DataManager.Instance.tumEsyalar);

        for (int i = 0; i < cards.Length; i++)
        {
            int randomIndex = Random.Range(0, availableItems.Count);
            ItemData selected = availableItems[randomIndex];
            rastgeleItemler.Add(selected);
            availableItems.RemoveAt(randomIndex);

            cards[i].Set(selected, () => OnClick_SelectButton(selected));
        }
    }

    public void OnClick_SelectButton(ItemData data)
    {
        saveData.AddItem(data.id);
        ApplyItemEffect(data.itemType);

        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        Time.timeScale = 1;
    }

    private void ApplyItemEffect(ItemTypes type)
    {
        if (playerCharacter == null)
        {
            playerCharacter = FindFirstObjectByType<Character>();
        }

        switch (type)
        {
            case ItemTypes.Health:
                if (playerHealth != null) playerHealth.Heal(50);
                break;

            case ItemTypes.Sword:
                // Kılıç hasarını artır (Kılıç zaten elde duruyor)
                if (playerCharacter != null) playerCharacter.IncreaseDamage(5f);
                break;

            case ItemTypes.Zone:
                // AuraWeapon scriptini bul ve aktif et/geliştir
                AuraWeapon aura = playerCharacter.GetComponentInChildren<AuraWeapon>(true);
                if (aura != null)
                {
                    if (!aura.gameObject.activeSelf) aura.gameObject.SetActive(true);
                    else { aura.IncreaseRange(0.5f); aura.damage += 2f; }
                }
                break;

            case ItemTypes.Bow:
                // Megabonk Mantığı: Kılıç durur, Ok sistemi ek yetenek olarak açılır
                // Karakterin üzerinde "BowSystem" adında bir script olduğunu varsayıyoruz
                BowSystem bow = playerCharacter.GetComponentInChildren<BowSystem>(true);

                if (bow != null)
                {
                    if (!bow.gameObject.activeSelf)
                    {
                        // İlk alımda ok sistemini çalıştır (otomatik ateş etmeye başlar)
                        bow.gameObject.SetActive(true);
                        Debug.Log("Ok Sistemi Aktif Edildi! Otomatik ateş başlıyor.");
                    }
                    else
                    {
                        // Tekrar seçilirse okların hızını veya hasarını artır
                        bow.fireRate -= 0.1f; // Daha hızlı ateş et
                        bow.arrowDamage += 3f; // Daha çok hasar ver
                        Debug.Log("Ok Atış Hızı ve Hasarı Geliştirildi!");
                    }
                }
                else
                {
                    Debug.LogError("HATA: Karakterin altında BowSystem (Ok Atma Scripti) bulunamadı!");
                }
                break;

            default:
                Debug.Log("Bu tür için özellik tanımlanmadı: " + type.ToString());
                break;
        }
    }
}