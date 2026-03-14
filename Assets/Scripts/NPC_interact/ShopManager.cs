using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("UI Panels")]
    public GameObject shopUI;
    public GameObject dialogueUI;
    public PlayerInventory player;

    [Header("Feedback UI")]
    public TextMeshProUGUI notificationText;
    public float notifyDuration = 2f;

    [Header("Shop Settings")]
    public List<ItemData> permanentItems;
    public List<ItemData> allPossibleItems;
    public int amountOfRandomItemsToSell = 3;

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
        if (notificationText != null) notificationText.gameObject.SetActive(false);
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
        foreach (Transform child in contentPanel) { Destroy(child.gameObject); }

        List<ItemData> itemsToShow = new List<ItemData>();
        if (permanentItems != null) itemsToShow.AddRange(permanentItems);

        if (allPossibleItems != null && allPossibleItems.Count > 0)
        {
            int countToRandom = Mathf.Min(amountOfRandomItemsToSell, allPossibleItems.Count);
            List<ItemData> tempPool = new List<ItemData>(allPossibleItems);
            for (int i = 0; i < countToRandom; i++)
            {
                int randomIndex = Random.Range(0, tempPool.Count);
                itemsToShow.Add(tempPool[randomIndex]);
                tempPool.RemoveAt(randomIndex);
            }
        }

        var sortedItems = itemsToShow.OrderBy(item => item.price).ToList();

        foreach (var item in sortedItems)
        {
            GameObject newItem = Instantiate(shopItemPrefab, contentPanel);
            newItem.transform.localScale = Vector3.one;
            newItem.GetComponent<ShopItemUI>().Setup(item, this);
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

    public bool OnBuyButtonClicked(ItemData itemToBuy)
    {
        if (player.money >= itemToBuy.price)
        {
            player.BuyItem(itemToBuy);
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