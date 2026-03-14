using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    public void Setup(ItemData data, ShopManager manager)
    {
        if (iconImage != null) iconImage.sprite = data.icon;
        if (titleText != null) titleText.text = data.itemName;
        if (priceText != null) priceText.text = "Price : " + data.price;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => manager.OnBuyButtonClicked(data));
    }

    public void SetupBait(ItemData data, BaitShopManager manager)
    {
        if (iconImage != null) iconImage.sprite = data.icon;
        if (titleText != null) titleText.text = data.itemName;
        if (priceText != null) priceText.text = "Price : " + data.price;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => manager.OnBuyButtonClicked(data));
    }
}