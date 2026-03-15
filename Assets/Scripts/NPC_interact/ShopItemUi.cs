using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;
    public Button buyButton;
    public TextMeshProUGUI buttonText;

    private int currentStock;
    private bool isUnlimited; // เช็คว่าเป็นไอเทมไม่จำกัดไหม

    public void Setup(ItemData data, ShopManager manager)
    {
        InitializeStock(data);
        UpdateUI(data);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => {
            if (isUnlimited || currentStock > 0)
            {
                manager.OnBuyButtonClicked(data);
                if (!isUnlimited) currentStock--;
                UpdateStockDisplay();
            }
        });
    }

    public void SetupBait(ItemData data, BaitShopManager manager)
    {
        InitializeStock(data);
        UpdateUI(data);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => {
            if (isUnlimited || currentStock > 0)
            {
                manager.OnBuyButtonClicked(data);
                if (!isUnlimited) currentStock--;
                UpdateStockDisplay();
            }
        });
    }

    private void InitializeStock(ItemData data)
    {
        currentStock = data.maxStock;
        // ถ้าตั้งค่า maxStock เป็น -1 หรือน้อยกว่า จะถือว่าเป็นของไม่จำกัด
        isUnlimited = (currentStock < 0);
    }

    private void UpdateUI(ItemData data)
    {
        if (iconImage != null) iconImage.sprite = data.icon;
        if (titleText != null) titleText.text = data.itemName;

        // 🌟 ถ้าราคาเป็น 0 ให้โชว์คำว่า Free ถ้าไม่ใช่ก็โชว์ราคาปกติ
        if (priceText != null) priceText.text = data.price == 0 ? "Free" : "Price : " + data.price;

        UpdateStockDisplay();
    }

    private void UpdateStockDisplay()
    {
        if (stockText != null)
        {
            if (isUnlimited)
            {
                stockText.text = "Stock: Unlimited"; // หรือใช้คำว่า Unlimited
                stockText.color = Color.black; // เปลี่ยนสีให้ดูพิเศษหน่อย
            }
            else if (currentStock <= 0)
            {
                stockText.text = "<color=red>Out of Stock</color>";
            }
            else
            {
                stockText.text = "Stock: " + currentStock;
                stockText.color = Color.black;
            }
        }

        // สถานะปุ่ม
        if (!isUnlimited && currentStock <= 0)
        {
            buyButton.interactable = false;
            if (buttonText != null) buttonText.text = "Sold Out";
        }
        else
        {
            buyButton.interactable = true;
            if (buttonText != null) buttonText.text = "Buy";
        }
    }
}