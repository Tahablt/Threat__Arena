using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{

    [SerializeField]
    private Button button;

    [SerializeField]
    Image image;

    [SerializeField]
    TMP_Text text;


    event Action OnClick;
    void Awake()
    {
        button.onClick.AddListener(Clicked);
    }

    public void Set(ItemData data, Action onClick)
    {
        image.sprite = data.icon;
        text.text = data.itemName;

        OnClick = onClick;

        gameObject.SetActive(true);
    }

    private void Clicked()
    {
        OnClick?.Invoke();
    }
}
