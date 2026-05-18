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
    public Transform swordTransform;
    public float scaleSize = 0.2f;

    [Header("Ses Ayarları")]
    public AudioSource uiAudioSource; 
    public AudioClip levelUpSound;    

    private List<ItemData> rastgeleItemler;
    
    // Çoklu tıklama bug'ını çözen güvenlik kilidi
    private bool isUpgrading = false; 

    private void Start()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }

    public void ShowUpgradeMenu()
    {
        Time.timeScale = 0;
        levelUpPanel.SetActive(true);
        
        isUpgrading = true;

        if (uiAudioSource != null && levelUpSound != null)
        {
            uiAudioSource.ignoreListenerPause = true;
            uiAudioSource.PlayOneShot(levelUpSound);
        }

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
        if (!isUpgrading) return; 
        
        isUpgrading = false; 

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
                }
                break;

            case ItemTypes.Zone:
                AuraWeapon aura = playerCharacter.GetComponentInChildren<AuraWeapon>(true);
                if (aura != null)
                {
                    if (!aura.gameObject.activeSelf) aura.gameObject.SetActive(true);
                    else 
                    {
                        aura.IncreaseRange(0.1f);   
                        aura.IncreaseDamage(10f);   
                    }
                }
                break;

            case ItemTypes.Bow:
                BowSystem bow = playerCharacter.GetComponentInChildren<BowSystem>(true);

                if (bow != null)
                {
                    if (!bow.gameObject.activeSelf)
                    {
                        bow.gameObject.SetActive(true);
                    }
                    else
                    {
                        // --- FİRE RATE SABİT BIRAKILDI ---
                        bow.arrowDamage += 3f; // Hasar 3 artar
                        bow.bowLevel += 1;     // Yan yana atılacak ok sayısı 1 artar

                        Debug.Log($"[OK UPGRADE] Seviye: {bow.bowLevel} | Yeni Hasar: {bow.arrowDamage} | Atış Süresi Sabit Kaldı: {bow.fireRate} sn");
                    }
                }
                break;

            default:
                Debug.Log("Bu tür için özellik tanımlanmadı: " + type.ToString());
                break;
        }
    }
}