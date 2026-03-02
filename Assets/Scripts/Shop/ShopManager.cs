using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("UI Panels")]
    public GameObject shopUI;
    public GameObject dialogueUI;

    public PlayerInventory player;

    [Header("Item for Sale")]
    public List<ItemData> itemsForSale;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        // ... (โค้ด Update เดิม)
    }

    public void OpenDialogue()
    {
        dialogueUI.SetActive(true);
        shopUI.SetActive(false);
    }

    public void CloseDialogue()
    {
        dialogueUI.SetActive(false);
    }

    public void ChooseToBuyItems()
    {
        CloseDialogue();
        OpenShop();
    }

    public void ChooseToLeave()
    {
        CloseDialogue();
    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
    }

    // --- แก้ไขตรงนี้ เพื่อเช็กว่าปุ่มทำงานจริงไหม ---
    public void OnBuyButtonClicked(ItemData itemToBuy)
    {
        Debug.Log("1. มีการคลิกปุ่มซื้อแล้ว! กำลังเช็กว่ามีคนซื้อไหม...");

        // เช็กว่าเผลอลืมใส่ช่อง Player ใน Inspector หรือเปล่า
        if (player == null)
        {
            Debug.LogError("พัง! หาตัวผู้เล่นไม่เจอ: คุณลืมลาก Player มาใส่ใน ShopManager หรือเปล่า?");
            return;
        }

        // เช็กว่าปุ่มส่งข้อมูลไอเทมมาให้ถูกไหม
        if (itemToBuy == null)
        {
            Debug.LogError("พัง! ไม่มีข้อมูลไอเทมส่งมา: ไปเช็กที่ OnClick() ของปุ่มซื้ออีกทีครับ");
            return;
        }

        Debug.Log($"2. ข้อมูลถูกต้อง! กำลังจะหักเงินผู้เล่นเพื่อซื้อ: {itemToBuy.itemName}");

        // สั่งให้ Player ซื้อ
        player.BuyItem(itemToBuy);
    }
}