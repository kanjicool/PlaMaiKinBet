using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("UI Panels")]
    public GameObject shopUI;
    public GameObject dialogueUI;
    public PlayerInventory player;

    [Header("Shop Settings")]
    public List<ItemData> allPossibleItems;
    public int amountOfItemsToSell = 5; // ปรับจำนวนชิ้นที่จะโชว์ได้เลย

    [Header("Restock Timer")]
    public float restockTimeInMinutes = 5f;
    public TextMeshProUGUI countdownText;

    [Header("Spawning References (ระบบอัตโนมัติ)")]
    public Transform contentPanel;
    public GameObject shopItemPrefab; // เอา Prefab มาใส่ตรงนี้

    private float currentTimer;

    private void Awake() { if (instance == null) instance = this; }

    private void Start()
    {
        currentTimer = restockTimeInMinutes * 60f;
        RefreshShop();
    }

    private void Update()
    {
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            currentTimer = restockTimeInMinutes * 60f;
            RefreshShop();
        }
    }

    private void UpdateTimerUI()
    {
        if (countdownText == null) return;
        int minutes = Mathf.FloorToInt(currentTimer / 60F);
        int seconds = Mathf.FloorToInt(currentTimer - minutes * 60);
        countdownText.text = string.Format("Restock in: {0:00}:{1:00}", minutes, seconds);
    }

    private void RefreshShop()
    {
        // 1. ล้างของเก่าทิ้งให้เกลี้ยง
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        if (allPossibleItems.Count == 0 || shopItemPrefab == null) return;

        // 2. สุ่มของและเสก UI ขึ้นมาใหม่ตามจำนวน
        for (int i = 0; i < amountOfItemsToSell; i++)
        {
            int randomIndex = Random.Range(0, allPossibleItems.Count);
            ItemData randomItem = allPossibleItems[randomIndex];

            // เสก Prefab เข้าไปใน Content
            GameObject newItem = Instantiate(shopItemPrefab, contentPanel);

            // *** ป้องกันบั๊ก Unity บีบ UI จนพัง ***
            newItem.transform.localScale = Vector3.one;

            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(randomItem, this);
            }
        }
    }

    public void OpenDialogue() { dialogueUI.SetActive(true); shopUI.SetActive(false); }
    public void CloseDialogue() { dialogueUI.SetActive(false); }
    public void ChooseToBuyItems() { CloseDialogue(); OpenShop(); }
    public void ChooseToLeave() { CloseDialogue(); }
    public void OpenShop() { shopUI.SetActive(true); }
    public void CloseShop() { shopUI.SetActive(false); }

    public void OnBuyButtonClicked(ItemData itemToBuy)
    {
        if (player == null || itemToBuy == null) return;
        Debug.Log($"หักเงินผู้เล่นเพื่อซื้อ: {itemToBuy.itemName}");
        player.BuyItem(itemToBuy);
    }
}