using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // เพิ่มเพื่อใช้ OrderBy

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("UI Panels")]
    public GameObject shopUI;
    public GameObject dialogueUI;
    public PlayerInventory player;

    [Header("Shop Settings")]
    public List<ItemData> allPossibleItems;
    public int amountOfItemsToSell = 5;

    [Header("Restock Timer")]
    public float restockTimeInMinutes = 5f;
    public TextMeshProUGUI countdownText;

    [Header("Spawning References")]
    public Transform contentPanel;
    public GameObject shopItemPrefab;

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
        // 1. ล้างของเก่า
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        if (allPossibleItems.Count == 0 || shopItemPrefab == null) return;

        // 2. สุ่มของมาเก็บไว้ใน List ชั่วคราวก่อน
        List<ItemData> selectedItems = new List<ItemData>();
        for (int i = 0; i < amountOfItemsToSell; i++)
        {
            int randomIndex = Random.Range(0, allPossibleItems.Count);
            selectedItems.Add(allPossibleItems[randomIndex]);
        }

        // 3. เรียงลำดับจากราคาน้อยไปมาก (OrderBy)
        // ** หมายเหตุ: ต้องมีตัวแปรชื่อ price ใน ItemData **
        var sortedItems = selectedItems.OrderBy(item => item.price).ToList();

        // 4. สร้าง UI จากรายการที่เรียงแล้ว
        foreach (var item in sortedItems)
        {
            GameObject newItem = Instantiate(shopItemPrefab, contentPanel);
            newItem.transform.localScale = Vector3.one;

            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(item, this);
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
        player.BuyItem(itemToBuy);
    }
}