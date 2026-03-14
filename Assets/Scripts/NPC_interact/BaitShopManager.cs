using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // จำเป็นต้องใช้เพื่อการเรียงลำดับ (OrderBy)

public class BaitShopManager : MonoBehaviour
{
    public static BaitShopManager instance;

    [Header("UI Panels")]
    public GameObject shopUI;
    public GameObject dialogueUI;
    public PlayerInventory player;

    [Header("Bait Shop Settings")]
    public List<ItemData> allBaitItems;
    // หมายเหตุ: amountOfBaitsToSell ไม่ถูกใช้งานแล้วเพราะแสดงทั้งหมด

    [Header("Spawning References")]
    public Transform contentPanel;
    public GameObject shopItemPrefab;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        // สร้างรายการสินค้าทันทีที่เริ่มเกม
        RefreshBaitShop();
    }

    private void RefreshBaitShop()
    {
        // 1. ล้าง UI เก่าทิ้งก่อน (ถ้ามี)
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        if (allBaitItems == null || allBaitItems.Count == 0 || shopItemPrefab == null) return;

        // 2. เรียงลำดับไอเทมทั้งหมดใน List ตามราคาจากน้อยไปมาก
        // ** ตรวจสอบว่าใน ItemData มีตัวแปรชื่อ price นะครับ **
        var sortedBaits = allBaitItems.OrderBy(bait => bait.price).ToList();

        // 3. สร้าง UI ของไอเทมทุกชิ้นที่มีอยู่ใน List
        foreach (ItemData bait in sortedBaits)
        {
            GameObject newItem = Instantiate(shopItemPrefab, contentPanel);
            newItem.transform.localScale = Vector3.one;

            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            if (itemUI != null)
            {
                // ส่งข้อมูลไปที่ ShopItemUI
                itemUI.SetupBait(bait, this);
            }
        }
    }

    public void OpenDialogue() { dialogueUI.SetActive(true); shopUI.SetActive(false); }
    public void CloseDialogue() { dialogueUI.SetActive(false); }
    public void ChooseToBuyItems() { CloseDialogue(); OpenShop(); }
    public void ChooseToLeave() { CloseDialogue(); }
    public void OpenShop() { shopUI.SetActive(true); }
    public void CloseShop() { shopUI.SetActive(false); }

    public void OnBuyButtonClicked(ItemData baitToBuy)
    {
        if (player == null || baitToBuy == null) return;
        Debug.Log($"ซื้อเหยื่อ: {baitToBuy.itemName}");
        player.BuyItem(baitToBuy);
    }
}