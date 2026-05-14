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

    [Header("Kılıç Büyüme Ayarları")]
    [Tooltip("Büyümesini istediğin kılıç nesnesini buraya sürükle.")]
    public Transform swordTransform;
    [Tooltip("Kılıç her seçildiğinde X, Y ve Z ekseninde ne kadar büyüyecek?")]
    public float scaleSize = 0.2f;

    [Header("Ses Ayarları")]
    public AudioSource uiAudioSource; // Level Up panelinde sesi çalacak alet
    public AudioClip levelUpSound;    // Level Up olduğunda çalacak ses dosyası

    private List<ItemData> rastgeleItemler;

    private void Start()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }

    public void ShowUpgradeMenu()
    {
        Time.timeScale = 0;
        levelUpPanel.SetActive(true);

        // --- YENİ EKLENEN KISIM: SESİ ÇAL ---
        // Oyun durduğu için (Time.timeScale = 0) sesin kesilmesini engelliyoruz
        if (uiAudioSource != null && levelUpSound != null)
        {
            uiAudioSource.ignoreListenerPause = true;
            uiAudioSource.PlayOneShot(levelUpSound);
        }
        // ------------------------------------

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
                if (playerCharacter != null)
                {
                    playerCharacter.IncreaseDamage(5f);
                    playerCharacter.IncreaseVFXScale(0.15f);
                }

                if (swordTransform != null)
                {
                    swordTransform.localScale += new Vector3(scaleSize, scaleSize, scaleSize);
                    Debug.Log("Kılıç büyütüldü! Yeni Boyut: " + swordTransform.localScale);
                }
                else
                {
                    Debug.LogWarning("DİKKAT: Büyütülecek kılıç nesnesi (swordTransform) Inspector'da atanmamış!");
                }
                break;

            case ItemTypes.Zone:
                AuraWeapon aura = playerCharacter.GetComponentInChildren<AuraWeapon>(true);
                if (aura != null)
                {
                    if (!aura.gameObject.activeSelf) aura.gameObject.SetActive(true);
                    else { aura.IncreaseRange(0.5f); aura.damage += 2f; }
                }
                break;

            case ItemTypes.Bow:
                BowSystem bow = playerCharacter.GetComponentInChildren<BowSystem>(true);

                if (bow != null)
                {
                    if (!bow.gameObject.activeSelf)
                    {
                        bow.gameObject.SetActive(true);
                        Debug.Log("Ok Sistemi Aktif Edildi! Otomatik ateş başlıyor.");
                    }
                    else
                    {
                        bow.fireRate = Mathf.Max(0.2f, bow.fireRate - 0.05f);
                        bow.arrowDamage += 3f;
                        bow.arrowsPerShot += 1;

                        Debug.Log("Ok Geliştirildi! Yeni Ok Sayısı: " + bow.arrowsPerShot);
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