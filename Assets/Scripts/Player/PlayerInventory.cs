using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerInventory : MonoBehaviour
{
    [Header("Shop & Money")]
    public int money = 500;
    public List<ItemData> myItems = new List<ItemData>();

    [Header("Hotbar Slots")]
    public Transform handTransform;
    public GameObject[] itemSlots = new GameObject[6];

    [Header("UI System")]
    public GameObject inventoryMenu;
    public Image[] hotbarIcons;
    public Image[] inventoryIcons;
    public RectTransform selectionHighlight;

    private InputSystem_Actions inputActions;
    private int currentItemIndex = -1;
    private bool isInventoryOpen = false;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Slot1.performed += ctx => EquipItem(0);
        inputActions.Player.Slot2.performed += ctx => EquipItem(1);
        inputActions.Player.Slot3.performed += ctx => EquipItem(2);
        inputActions.Player.Slot4.performed += ctx => EquipItem(3);
        inputActions.Player.Slot5.performed += ctx => EquipItem(4);
        inputActions.Player.Slot6.performed += ctx => EquipItem(5);
    }

    private void Start()
    {
        if (inventoryMenu != null) inventoryMenu.SetActive(false);
        UpdateInventoryUI();
    }

    private void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    private void OnEnable() { inputActions.Enable(); }
    private void OnDisable() { inputActions.Disable(); }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryMenu.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            UpdateInventoryUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void UpdateInventoryUI()
    {
        // 1. อัปเดต Hotbar Icons
        for (int i = 0; i < hotbarIcons.Length; i++)
        {
            if (i < itemSlots.Length && itemSlots[i] != null)
            {
                ItemHolder itemHolder = itemSlots[i].GetComponent<ItemHolder>();
                if (itemHolder != null && itemHolder.itemData != null)
                {
                    hotbarIcons[i].sprite = itemHolder.itemData.icon;
                    hotbarIcons[i].color = Color.white; // มั่นใจว่าสีปกติ
                    hotbarIcons[i].enabled = true;
                }
            }
            else
            {
                // แทนที่จะสั่ง enabled = false ซึ่งจะทำให้ตำแหน่งเพี้ยนหรือหายไป
                // ให้ใช้การปรับสีให้โปร่งใสแทน (Alpha = 0) เพื่อให้ Object ยัง Active อยู่
                hotbarIcons[i].sprite = null;
                hotbarIcons[i].color = new Color(0, 0, 0, 0);
                hotbarIcons[i].enabled = true; // เปิดไว้เพื่อให้เอา position ได้
            }
        }

        // ... (ส่วนอัปเดต Inventory Icons คงเดิม) ...

        // 3. อัปเดตตำแหน่ง Highlight
        if (selectionHighlight != null)
        {
            if (currentItemIndex >= 0 && currentItemIndex < hotbarIcons.Length)
            {
                selectionHighlight.gameObject.SetActive(true);

                // แนะนำ: ให้ย้ายไปเกาะที่ตำแหน่งของ "Slot" (Parent ของ Icon) 
                // เพราะตำแหน่งจะนิ่งกว่าและตรงกลางเป๊ะกว่าครับ
                selectionHighlight.position = hotbarIcons[currentItemIndex].transform.parent.position;
            }
            else
            {
                selectionHighlight.gameObject.SetActive(false);
            }
        }
    }

    private void EquipItem(int index)
    {
        if (index >= itemSlots.Length) return;

        // ปิดของชิ้นเก่าก่อน (ถ้ามี)
        if (currentItemIndex != -1 && currentItemIndex < itemSlots.Length && itemSlots[currentItemIndex] != null)
        {
            itemSlots[currentItemIndex].SetActive(false);
        }

        // ถ้ากดช่องเดิมที่ใส่อยู่ ให้ถือว่า "เก็บ" (Un-equip)
        if (currentItemIndex == index)
        {
            currentItemIndex = -1;
        }
        else
        {
            // เลือกช่องใหม่ (ยอมให้เลือกช่องว่างได้)
            currentItemIndex = index;
            if (itemSlots[currentItemIndex] != null)
            {
                itemSlots[currentItemIndex].SetActive(true);
            }
        }

        UpdateInventoryUI();
    }

    public bool BuyItem(ItemData item)
    {
        if (money >= item.price && myItems.Count < inventoryIcons.Length)
        {
            money -= item.price;
            myItems.Add(item);
            Debug.Log($"ซื้อ {item.itemName} สำเร็จ! เงินเหลือ: {money}");

            UpdateHotbarAfterPurchase(item);
            UpdateInventoryUI();
            return true;
        }
        else
        {
            Debug.Log("เงินไม่พอ หรือกระเป๋าเต็ม!");
            return false;
        }
    }

    private void UpdateHotbarAfterPurchase(ItemData item)
    {
        if (item.itemPrefab == null) return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                GameObject spawnedItem = Instantiate(item.itemPrefab, handTransform);

                ItemHolder holder = spawnedItem.GetComponent<ItemHolder>();
                if (holder == null) holder = spawnedItem.AddComponent<ItemHolder>();
                holder.itemData = item;

                spawnedItem.transform.localPosition = Vector3.zero;
                spawnedItem.transform.localRotation = Quaternion.identity;
                spawnedItem.SetActive(false);

                itemSlots[i] = spawnedItem;

                if (currentItemIndex == -1) EquipItem(i);
                return;
            }
        }
    }

    public GameObject GetHeldItem()
    {
        if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
        {
            return itemSlots[currentItemIndex];
        }
        return null;
    }

    public void SellItem(GameObject itemToSell, int price)
    {
        money += price;
        Debug.Log($"ขายของสำเร็จ! ได้เงินมา {price} เหรียญ ตอนนี้มีเงินทั้งหมด: {money}");

        if (currentItemIndex != -1)
        {
            itemSlots[currentItemIndex] = null;
            currentItemIndex = -1;
        }

        ItemHolder holder = itemToSell.GetComponent<ItemHolder>();
        if (holder != null && myItems.Contains(holder.itemData))
        {
            myItems.Remove(holder.itemData);
        }

        Destroy(itemToSell);
        UpdateInventoryUI();
    }

    // ลบฟังก์ชัน AddFishToInventory ทิ้งไปเลยครับ เพราะตกปลาเราใช้ AddCaughtFishToHotbar อยู่แล้ว จะได้ไม่รก

    public void AddCaughtFishToHotbar(ItemData fishItem)
    {
        if (fishItem == null) return;

        // ลองหาช่องว่างใน Hotbar ก่อน
        bool addedToHotbar = false;
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                GameObject spawnedFish = Instantiate(fishItem.itemPrefab, handTransform);
                ItemHolder holder = spawnedFish.GetComponent<ItemHolder>() ?? spawnedFish.AddComponent<ItemHolder>();
                holder.itemData = fishItem;

                spawnedFish.transform.localPosition = Vector3.zero;
                spawnedFish.transform.localRotation = Quaternion.identity;
                spawnedFish.SetActive(false);

                itemSlots[i] = spawnedFish;
                myItems.Add(fishItem);

                Debug.Log($"ตกได้ {fishItem.itemName} เก็บเข้า Hotbar ช่อง {i}");
                addedToHotbar = true;
                break;
            }
        }

        // ถ้า Hotbar เต็ม แต่ช่อง Inventory (myItems) ยังไม่เต็ม
        if (!addedToHotbar)
        {
            if (myItems.Count < inventoryIcons.Length)
            {
                myItems.Add(fishItem);
                Debug.Log($"Hotbar เต็ม! เก็บ {fishItem.itemName} เข้า Inventory แทน");
            }
            else
            {
                Debug.Log("กระเป๋าเต็มสนิท! ปลาหลุดมือไปแล้ว");
            }
        }

        UpdateInventoryUI();
    }

    public int SellAllFish()
    {
        int totalEarnings = 0;
        int fishCount = 0;
        List<ItemData> itemsToRemove = new List<ItemData>();

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {
                ItemHolder itemHolder = itemSlots[i].GetComponent<ItemHolder>();
                FishHolder fishHolder = itemSlots[i].GetComponent<FishHolder>();

                // 🌟 ทริคใหม่: ไม่ต้องพึ่งคำว่า "fish" ใน itemtype แล้ว!
                // ถ้าของชิ้นนี้มีทั้ง ItemHolder และ FishHolder (เพราะ Prefab ปลามันติด FishHolder มาด้วย)
                // ก็ฟันธงได้เลยว่ามันคือ "ปลา" แน่นอน
                if (itemHolder != null && itemHolder.itemData != null && fishHolder != null)
                {
                    int itemPrice = itemHolder.itemData.price; // ดึงราคาจาก ItemData ที่เดียวพอ

                    totalEarnings += itemPrice;
                    fishCount++;
                    itemsToRemove.Add(itemHolder.itemData);

                    Destroy(itemSlots[i]);
                    itemSlots[i] = null;

                    if (currentItemIndex == i)
                    {
                        currentItemIndex = -1;
                    }
                }
            }
        }

        foreach (var item in itemsToRemove)
        {
            myItems.Remove(item);
        }

        if (totalEarnings > 0)
        {
            money += totalEarnings;
            Debug.Log($"ขายปลาไปทั้งหมด {fishCount} ตัว ได้เงินมา {totalEarnings} เหรียญ ตอนนี้มีเงินทั้งหมด: {money}");
        }

        UpdateInventoryUI();
        return totalEarnings;
    }
}