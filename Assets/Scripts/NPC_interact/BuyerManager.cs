using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyerManager : MonoBehaviour
{
    public static BuyerManager instance;

    [Header("UI Panels")]
    public GameObject dialogueUI; // ลากออบเจกต์ DialogueSellerUI มาใส่แค่ช่องนี้พอ

    public PlayerInventory player;

    private TextMeshProUGUI dialogueText;
    private GameObject sellButton;
    private GameObject checkPriceButton;

    private void Awake()
    {
        if (instance == null) instance = this;

        // สั่งให้ระบบค้นหา UI ย่อยที่อยู่ข้างใน dialogueUI อัตโนมัติ
        if (dialogueUI != null)
        {
            Transform titleTransform = dialogueUI.transform.Find("title");
            if (titleTransform != null) dialogueText = titleTransform.GetComponent<TextMeshProUGUI>();

            Transform sellTransform = dialogueUI.transform.Find("sell");
            if (sellTransform != null) sellButton = sellTransform.gameObject;

            Transform checkPriceTransform = dialogueUI.transform.Find("checkprice");
            if (checkPriceTransform != null) checkPriceButton = checkPriceTransform.gameObject;
        }
    }

    public void OpenDialogue()
    {
        dialogueUI.SetActive(true);

        if (dialogueText != null) dialogueText.text = "Do you have anything for sale, kid?";

        // เปิดโชว์ปุ่มทั้งหมดตั้งแต่เริ่มคุยเลย
        if (sellButton != null) sellButton.SetActive(true);
        if (checkPriceButton != null) checkPriceButton.SetActive(true);
    }

    public void CloseDialogue()
    {
        dialogueUI.SetActive(false);
    }

    public void ChooseToLeave()
    {
        CloseDialogue();
    }

    public void ChooseToBuyItems()
    {
        CloseDialogue();
    }

    // --- ทำงานเมื่อกดปุ่ม Check Price ---
    public void CheckItemOnHand()
    {
        if (player == null) return;

        GameObject heldItem = player.GetHeldItem();

        // 1. ถ้าไม่ได้ถือของอะไรเลย
        //if (heldItem == null)
        //{
        //    if (dialogueText != null) dialogueText.text = "You are not holding anything. Please equip an item first.";
        //    return;
        //}

        // 2. ส่งของในมือไปเช็กราคา
        int currentItemPrice = GetItemPrice(heldItem, out string itemName);

        // 3. เปลี่ยนข้อความตามราคาที่เช็กได้
        if (currentItemPrice > 0)
        {
            if (dialogueText != null) dialogueText.text = $"Oh! That's a {itemName}... I'll buy it for {currentItemPrice} coins. Deal?";
        }
        else
        {
            if (dialogueText != null) dialogueText.text = "Hmm... I don't want to buy that. Show me something else.";
        }
    }

    // --- ทำงานเมื่อกดปุ่ม Sell ---
    public void OnSellButtonClicked()
    {
        if (player == null) return;

        GameObject heldItem = player.GetHeldItem();

        // 1. ถ้าไม่ได้ถือของอะไรเลย -> ให้ขายปลา (FishData) ทั้งหมดในกระเป๋า
        if (heldItem == null)
        {
            int totalFishEarnings = player.SellAllFish();

            if (totalFishEarnings > 0)
            {
                if (dialogueText != null)
                    dialogueText.text = $"Wow! I bought all your fish for {totalFishEarnings} coins. Anything else?";
            }
            else
            {
                if (dialogueText != null)
                    dialogueText.text = "You are not holding anything, and you don't have any fish to sell.";
            }
            return; // จบการทำงานตรงนี้เลย ไม่ต้องทำบรรทัดล่างต่อ
        }

        // 2. ถ้าถือของอยู่ -> ให้ขายเฉพาะของที่ถือบนมือเท่านั้น
        int currentItemPrice = GetItemPrice(heldItem, out string itemName);

        if (currentItemPrice > 0)
        {
            player.SellItem(heldItem, currentItemPrice);
            if (dialogueText != null) dialogueText.text = $"Sold {itemName} for {currentItemPrice} coins! Anything else?";
        }
        else
        {
            // ถ้าถือของอยู่ แต่เป็นของที่ขายไม่ได้ (ราคา 0 หรือไม่มีข้อมูล)
            if (dialogueText != null) dialogueText.text = "I don't buy that kind of stuff.";
        }
    }

    // --- ฟังก์ชันตัวช่วย: ลดความซ้ำซ้อนของโค้ดในการดึงชื่อและราคา ---
    private int GetItemPrice(GameObject item, out string itemName)
    {
        itemName = "";
        int price = 0;

        ItemHolder itemHolder = item.GetComponent<ItemHolder>();    
        FishHolder fishHolder = item.GetComponent<FishHolder>();

        if (itemHolder != null && itemHolder.itemData != null)
        {
            itemName = itemHolder.itemData.itemName;
            price = itemHolder.itemData.price;
        }
        else if (fishHolder != null && fishHolder.fishData != null)
        {
            itemName = fishHolder.fishData.fishName;
            price = fishHolder.fishData.price;
        }

        return price; // ส่งราคาคืนกลับไปให้คนเรียก
    }
}