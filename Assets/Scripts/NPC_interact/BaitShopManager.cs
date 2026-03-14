using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;

public class BaitShopManager : MonoBehaviour
{
    public static BaitShopManager instance;

    [Header("UI Panels")]
    public GameObject shopUI;
    public GameObject dialogueUI;
    public PlayerInventory player;

    [Header("Feedback UI")]
    public TextMeshProUGUI notificationText;
    public float notifyDuration = 2f;

    [Header("Bait Shop Settings")]
    public List<ItemData> allBaitItems;

    [Header("Spawning References")]
    public Transform contentPanel;
    public GameObject shopItemPrefab;

    private void Awake() { if (instance == null) instance = this; }

    private void Start()
    {
        if (notificationText != null) notificationText.gameObject.SetActive(false);
        RefreshBaitShop();
    }

    private void RefreshBaitShop()
    {
        foreach (Transform child in contentPanel) { Destroy(child.gameObject); }
        if (allBaitItems == null || allBaitItems.Count == 0) return;

        var sortedBaits = allBaitItems.OrderBy(bait => bait.price).ToList();

        foreach (ItemData bait in sortedBaits)
        {
            GameObject newItem = Instantiate(shopItemPrefab, contentPanel);
            newItem.transform.localScale = Vector3.one;
            newItem.GetComponent<ShopItemUI>().SetupBait(bait, this);
        }
    }

    // --- เพิ่มฟังก์ชันที่หายไปสำหรับ NPC กลับคืนมา ---
    public void OpenDialogue() { dialogueUI.SetActive(true); shopUI.SetActive(false); }
    public void CloseDialogue() { dialogueUI.SetActive(false); }
    public void ChooseToBuyItems() { CloseDialogue(); OpenShop(); }
    public void ChooseToLeave() { CloseDialogue(); }
    public void OpenShop() { shopUI.SetActive(true); }
    public void CloseShop() { shopUI.SetActive(false); }
    // ------------------------------------------

    public bool OnBuyButtonClicked(ItemData baitToBuy)
    {
        if (player.money >= baitToBuy.price)
        {
            player.BuyItem(baitToBuy);
            return true;
        }
        else
        {
            ShowNotification("Not Enough money");
            return false;
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationText == null) return;
        StopAllCoroutines();
        StartCoroutine(NotifyRoutine(message));
    }

    private IEnumerator NotifyRoutine(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(notifyDuration);
        notificationText.gameObject.SetActive(false);
    }
}