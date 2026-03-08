using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // เพิ่ม Library สำหรับจัดการ UI (Image)

public class PlayerInventory : MonoBehaviour
{
    [Header("Shop & Money")]
    public int money = 500;
    public List<ItemData> myItems = new List<ItemData>();

    [Header("Hotbar Slots")]
    public Transform handTransform;
    public GameObject[] itemSlots = new GameObject[6];

    [Header("UI System")]
    public GameObject inventoryMenu;    // ลาก InventoryMenu มาใส่ (เพื่อให้เปิด/ปิดได้)
    public Image[] hotbarIcons;        // ลาก Image 6 อันใน Hotbar Panel มาใส่
    public Image[] inventoryIcons;     // ลาก Image 28 อันใน GridItem มาใส่

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
        // ซ่อนหน้าต่างกระเป๋าตอนเริ่มเกม และอัปเดต UI ให้โชว์เบ็ดตกปลาใน Hotbar
        if (inventoryMenu != null) inventoryMenu.SetActive(false);
        UpdateInventoryUI();
    }

    private void Update()
    {
        // กด B เพื่อเปิด-ปิดกระเป๋า
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
            Cursor.lockState = CursorLockMode.None; // โชว์เมาส์
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // ซ่อนเมาส์กลับเข้าเกม
            Cursor.visible = false;
        }
    }

    // ฟังก์ชันสำหรับรีเฟรชรูปภาพในช่อง UI ทั้งหมด
    public void UpdateInventoryUI()
    {
        // 1. อัปเดต Hotbar (6 ช่องข้างล่าง)
        for (int i = 0; i < hotbarIcons.Length; i++)
        {
            if (i < itemSlots.Length && itemSlots[i] != null)
            {
                ItemHolder itemHolder = itemSlots[i].GetComponent<ItemHolder>();
                FishHolder fishHolder = itemSlots[i].GetComponent<FishHolder>();

                // ถ้าเป็นไอเทมปกติ
                if (itemHolder != null && itemHolder.itemData != null)
                {
                    hotbarIcons[i].sprite = itemHolder.itemData.icon;
                    hotbarIcons[i].enabled = true;
                }
                // ถ้าเป็นปลา 🌟 (เพิ่มตรงนี้เข้ามา)
                else if (fishHolder != null && fishHolder.fishData != null)
                {
                    hotbarIcons[i].sprite = fishHolder.fishData.fishIcon; // ใช้รูปจาก FishData
                    hotbarIcons[i].enabled = true;
                }
                else
                {
                    hotbarIcons[i].enabled = false;
                }
            }
            else
            {
                hotbarIcons[i].enabled = false;
            }
        }

        // 2. อัปเดตช่องกระเป๋า (28 ช่อง)
        for (int i = 0; i < inventoryIcons.Length; i++)
        {
            if (i < myItems.Count)
            {
                inventoryIcons[i].sprite = myItems[i].icon;
                inventoryIcons[i].enabled = true;
            }
            else
            {
                inventoryIcons[i].enabled = false;
            }
        }
    }

    private void EquipItem(int index)
    {
        if (index >= itemSlots.Length || itemSlots[index] == null) return;

        if (currentItemIndex == index)
        {
            itemSlots[index].SetActive(false);
            currentItemIndex = -1;
        }
        else
        {
            if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
            {
                itemSlots[currentItemIndex].SetActive(false);
            }
            itemSlots[index].SetActive(true);
            currentItemIndex = index;
        }
        UpdateInventoryUI(); // อัปเดต UI เมื่อสลับของ
    }

    public bool BuyItem(ItemData item)
    {
        // เช็คว่ามีเงินพอ และ กระเป๋ายังไม่เต็ม (28 ช่อง)
        if (money >= item.price && myItems.Count < inventoryIcons.Length)
        {
            money -= item.price;
            myItems.Add(item);
            Debug.Log($"ซื้อ {item.itemName} สำเร็จ! เงินเหลือ: {money}");

            UpdateHotbarAfterPurchase(item);
            UpdateInventoryUI(); // อัปเดต UI
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
        UpdateInventoryUI(); // อัปเดต UI
    }

    public void AddFishToInventory(GameObject fishPrefab, FishData fishData)
    {
        if (fishPrefab == null) return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                GameObject spawnedFish = Instantiate(fishPrefab, handTransform);

                FishHolder holder = spawnedFish.GetComponent<FishHolder>();
                if (holder == null) holder = spawnedFish.AddComponent<FishHolder>();
                holder.fishData = fishData;

                spawnedFish.transform.localPosition = Vector3.zero;
                spawnedFish.transform.localRotation = Quaternion.identity;
                spawnedFish.SetActive(false);

                itemSlots[i] = spawnedFish;

                Debug.Log($"ตกได้ {fishData.fishName} และเก็บเข้าช่อง {i + 1} แล้ว!");

                if (currentItemIndex == -1) EquipItem(i);
                UpdateInventoryUI(); // อัปเดต UI
                return;
            }
        }
        Debug.Log("กระเป๋าเต็ม! ไม่มีที่เก็บปลา");
    }

    public void AddCaughtFishToHotbar(ItemData fishItem)
    {
        if (fishItem == null || fishItem.itemPrefab == null) return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                GameObject spawnedFish = Instantiate(fishItem.itemPrefab, handTransform);

                ItemHolder holder = spawnedFish.GetComponent<ItemHolder>();
                if (holder == null) holder = spawnedFish.AddComponent<ItemHolder>();
                holder.itemData = fishItem;

                spawnedFish.transform.localPosition = Vector3.zero;
                spawnedFish.transform.localRotation = Quaternion.identity;
                spawnedFish.SetActive(false);

                itemSlots[i] = spawnedFish;
                myItems.Add(fishItem);

                Debug.Log($"ตกได้ {fishItem.itemName} เก็บเข้าช่องที่ {i}");

                if (currentItemIndex == -1) EquipItem(i);
                UpdateInventoryUI(); // อัปเดต UI
                return;
            }
        }
        Debug.Log("กระเป๋าเต็ม! ปลาหลุดมือไปแล้ว");
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
                int itemPrice = 0;
                bool isSellable = false;

                FishHolder fishHolder = itemSlots[i].GetComponent<FishHolder>();
                // 1. ถ้าไอเทมใช้ระบบ FishHolder ให้ถือว่าเป็นปลาและขายได้เลย
                if (fishHolder != null && fishHolder.fishData != null)
                {
                    itemPrice = fishHolder.fishData.price;
                    isSellable = true;
                }
                else
                {
                    ItemHolder itemHolder = itemSlots[i].GetComponent<ItemHolder>();
                    // 2. ถ้าไอเทมใช้ระบบ ItemData ให้เช็คว่า itemtype เป็น "fish" เท่านั้นถึงจะขายได้
                    // ใช้ .ToLower() เพื่อป้องกันบัคกรณีพิมพ์พิมพ์เล็กพิมพ์ใหญ่สลับกัน (เช่น "Fish", "FISH")
                    if (itemHolder != null && itemHolder.itemData != null && !string.IsNullOrEmpty(itemHolder.itemData.itemtype) && itemHolder.itemData.itemtype.ToLower() == "fish")
                    {
                        itemPrice = itemHolder.itemData.price;
                        itemsToRemove.Add(itemHolder.itemData);
                        isSellable = true;
                    }
                }

                // ถ้าตรวจผ่านเงื่อนไขว่าเป็นปลา (isSellable = true) ให้ขายทิ้ง
                if (isSellable)
                {
                    totalEarnings += itemPrice;
                    fishCount++;
                    Destroy(itemSlots[i]);
                    itemSlots[i] = null;

                    if (currentItemIndex == i)
                    {
                        currentItemIndex = -1;
                    }
                }
            }
        }

        // เอาปลาที่ขายไปแล้วออกจาก List กระเป๋าหลัก
        foreach (var item in itemsToRemove)
        {
            myItems.Remove(item);
        }

        // เพิ่มเงินและสรุปผล
        if (totalEarnings > 0)
        {
            money += totalEarnings;
            Debug.Log($"ขายปลาไปทั้งหมด {fishCount} ตัว ได้เงินมา {totalEarnings} เหรียญ ตอนนี้มีเงินทั้งหมด: {money}");
        }

        UpdateInventoryUI(); // รีเฟรชหน้าจอหลังจากขายเสร็จ
        return totalEarnings;
    }
}