using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;


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
    public TextMeshProUGUI goldText;

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

        // ++ เรียกใช้ระบบติดตั้ง Slot อัตโนมัติ ++
        AutoSetupSlotUI();
    }

    private void AutoSetupSlotUI()
    {
        // 1. จัดการ Hotbar
        for (int i = 0; i < hotbarIcons.Length; i++)
        {
            if (hotbarIcons[i] != null)
            {
                GameObject slotObj = hotbarIcons[i].transform.parent.gameObject;
                SlotUI slotUI = slotObj.GetComponent<SlotUI>();
                if (slotUI == null) slotUI = slotObj.AddComponent<SlotUI>();

                slotUI.slotType = SlotUI.SlotType.Hotbar;
                slotUI.slotIndex = i;
                slotUI.itemIcon = hotbarIcons[i]; // 🌟 ผูก Image ไอเทมเข้ากับ Slot อัตโนมัติ

                Transform lockIcon = slotObj.transform.Find("LockIcon");
                if (lockIcon != null) slotUI.lockImage = lockIcon.GetComponent<Image>();
            }
        }

        // 2. จัดการ Inventory
        for (int i = 0; i < inventoryIcons.Length; i++)
        {
            if (inventoryIcons[i] != null)
            {
                GameObject slotObj = inventoryIcons[i].transform.parent.gameObject;
                SlotUI slotUI = slotObj.GetComponent<SlotUI>();
                if (slotUI == null) slotUI = slotObj.AddComponent<SlotUI>();

                slotUI.slotType = SlotUI.SlotType.Inventory;
                slotUI.slotIndex = i;
                slotUI.itemIcon = inventoryIcons[i]; // 🌟 ผูก Image ไอเทมเข้ากับ Slot อัตโนมัติ

                Transform lockIcon = slotObj.transform.Find("LockIcon");
                if (lockIcon != null) slotUI.lockImage = lockIcon.GetComponent<Image>();
            }
        }
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
        if (goldText != null)
        {
            goldText.text = "Gold : " + money;
        }
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
                // แทนที่จะสั่ง enabled = false ให้ใช้การปรับสีให้โปร่งใสแทน (Alpha = 0)
                hotbarIcons[i].sprite = null;
                hotbarIcons[i].color = new Color(0, 0, 0, 0);
                hotbarIcons[i].enabled = true;
            }
        }

        // 2. อัปเดต Inventory Icons (ส่วนนี้ที่หายไป เอากลับมาแล้วครับ!)
        for (int i = 0; i < inventoryIcons.Length; i++)
        {
            if (i < myItems.Count)
            {
                inventoryIcons[i].sprite = myItems[i].icon;
                inventoryIcons[i].color = Color.white; // ทำให้สีสว่างเต็ม 100% จะได้เห็นรูป
                inventoryIcons[i].enabled = true;
            }
            else
            {
                inventoryIcons[i].sprite = null;
                inventoryIcons[i].enabled = true;
            }
        }

        // 3. อัปเดตตำแหน่ง Highlight
        if (selectionHighlight != null)
        {
            if (currentItemIndex >= 0 && currentItemIndex < hotbarIcons.Length)
            {
                selectionHighlight.gameObject.SetActive(true);
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
        // 1. เช็คว่ามีช่องว่างตรงไหนบ้าง
        bool hasHotbarSpace = false;
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null) { hasHotbarSpace = true; break; }
        }
        bool hasInventorySpace = myItems.Count < inventoryIcons.Length;

        // 2. ถ้าเงินพอ และมีที่ว่าง
        if (money >= item.price && (hasHotbarSpace || hasInventorySpace))
        {
            money -= item.price; // หักเงิน

            if (hasHotbarSpace)
            {
                UpdateHotbarAfterPurchase(item); // ใส่บนมือ (ไม่ต้องใส่ใน myItems แล้ว)
            }
            else
            {
                myItems.Add(item); // มือเต็ม เอาเข้ากระเป๋า
            }

            Debug.Log($"ซื้อ {item.itemName} สำเร็จ! เงินเหลือ: {money}");
            UpdateInventoryUI();
            return true;
        }
        else
        {
            Debug.Log("เงินไม่พอ หรือกระเป๋าเต็มสนิท!");
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
                return; // จบการทำงานทันทีที่หาช่องเจอ
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

    public void AddCaughtFishToHotbar(ItemData fishItem)
    {
        if (fishItem == null) return;

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

                Debug.Log($"ตกได้ {fishItem.itemName} เก็บเข้า Hotbar ช่อง {i}");
                addedToHotbar = true;
                break;
            }
        }

        // ถ้า Hotbar เต็ม ค่อยเก็บเข้า Inventory
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

        // 1. Hotbar
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {
                // 🌟 เช็คก่อนว่าช่องนี้ล็อกอยู่ไหม
                SlotUI slotUI = hotbarIcons[i].transform.parent.GetComponent<SlotUI>();
                if (slotUI != null && slotUI.isLocked) continue; // ล็อกอยู่ให้ข้ามไปเลย!

                FishHolder fishHolder = itemSlots[i].GetComponent<FishHolder>();
                if (fishHolder != null)
                {
                    totalEarnings += itemSlots[i].GetComponent<ItemHolder>().itemData.price;
                    fishCount++;
                    Destroy(itemSlots[i]);
                    itemSlots[i] = null;
                    if (currentItemIndex == i) currentItemIndex = -1;
                }
            }
        }

        // 2. Inventory (ใช้วิธีสร้างรายการใหม่ เพื่อไม่ให้กุญแจรวนเวลาของเลื่อน)
        List<ItemData> keptItems = new List<ItemData>();
        List<bool> keptLocks = new List<bool>();

        for (int i = 0; i < myItems.Count; i++)
        {
            SlotUI slotUI = inventoryIcons[i].transform.parent.GetComponent<SlotUI>();
            bool isLocked = slotUI != null && slotUI.isLocked;
            bool isFish = myItems[i].itemPrefab != null && myItems[i].itemPrefab.GetComponent<FishHolder>() != null;

            if (isFish && !isLocked) // ถ้าเป็นปลา และ ไม่ได้ล็อก = ขาย!
            {
                totalEarnings += myItems[i].price;
                fishCount++;
            }
            else // ถ้าไม่ใช่ปลา หรือ โดนล็อกไว้ = เก็บไว้ในกระเป๋าต่อ!
            {
                keptItems.Add(myItems[i]);
                keptLocks.Add(isLocked);
            }
        }

        myItems = keptItems; // อัปเดตกระเป๋าให้เหลือแค่ของที่ไม่ได้ขาย

        // 🌟 อัปเดตกุญแจในกระเป๋าใหม่ทั้งหมดให้ตรงกับของที่ร่นขึ้นมา
        for (int i = 0; i < inventoryIcons.Length; i++)
        {
            SlotUI slotUI = inventoryIcons[i].transform.parent.GetComponent<SlotUI>();
            if (slotUI != null)
            {
                slotUI.isLocked = (i < keptLocks.Count) ? keptLocks[i] : false;
                if (slotUI.lockImage != null) slotUI.lockImage.enabled = slotUI.isLocked;
            }
        }

        if (totalEarnings > 0) money += totalEarnings;
        UpdateInventoryUI();
        return totalEarnings;
    }

    public void SwapItems(SlotUI fromSlot, SlotUI toSlot)
    {
        ItemData fromData = GetItemDataFromSlot(fromSlot);
        ItemData toData = GetItemDataFromSlot(toSlot);

        // สลับข้อมูลไอเทม
        SetItemDataToSlot(fromSlot, toData);
        SetItemDataToSlot(toSlot, fromData);

        // 🌟 สลับสถานะแม่กุญแจให้ตามไอเทมไปด้วย!
        bool tempLock = fromSlot.isLocked;
        fromSlot.isLocked = toSlot.isLocked;
        if (fromSlot.lockImage != null) fromSlot.lockImage.enabled = fromSlot.isLocked;

        toSlot.isLocked = tempLock;
        if (toSlot.lockImage != null) toSlot.lockImage.enabled = toSlot.isLocked;

        UpdateInventoryUI();
    }

    private ItemData GetItemDataFromSlot(SlotUI slot)
    {
        if (slot.slotType == SlotUI.SlotType.Hotbar)
        {
            if (itemSlots[slot.slotIndex] != null)
                return itemSlots[slot.slotIndex].GetComponent<ItemHolder>()?.itemData;
        }
        else if (slot.slotType == SlotUI.SlotType.Inventory)
        {
            if (slot.slotIndex < myItems.Count)
                return myItems[slot.slotIndex];
        }
        return null;
    }

    private void SetItemDataToSlot(SlotUI slot, ItemData item)
    {
        if (slot.slotType == SlotUI.SlotType.Hotbar)
        {
            // ทำลายของเก่าที่อยู่ในมือ
            if (itemSlots[slot.slotIndex] != null)
            {
                Destroy(itemSlots[slot.slotIndex]);
                itemSlots[slot.slotIndex] = null;
            }

            // ถ้ามีของใหม่ให้เสกขึ้นมาใส่มือ
            if (item != null && item.itemPrefab != null)
            {
                GameObject spawned = Instantiate(item.itemPrefab, handTransform);
                ItemHolder holder = spawned.GetComponent<ItemHolder>() ?? spawned.AddComponent<ItemHolder>();
                holder.itemData = item;
                spawned.transform.localPosition = Vector3.zero;
                spawned.transform.localRotation = Quaternion.identity;

                spawned.SetActive(currentItemIndex == slot.slotIndex); // เปิดแสดงผลถ้าถือช่องนี้อยู่
                itemSlots[slot.slotIndex] = spawned;
            }
        }
        else if (slot.slotType == SlotUI.SlotType.Inventory)
        {
            // เนื่องจากกระเป๋าเป็นแบบ List (เรียงติดกันเสมอ) 
            if (slot.slotIndex < myItems.Count)
            {
                if (item != null) myItems[slot.slotIndex] = item; // แทนที่ตำแหน่งเดิม
                else myItems.RemoveAt(slot.slotIndex); // ดึงออก
            }
            else if (item != null)
            {
                myItems.Add(item); // ถ้าลากไปวางช่องว่างท้ายๆ ให้เอาไปต่อแถว
            }
        }
    }

    public bool IsHeldItemLocked()
    {
        if (currentItemIndex != -1 && currentItemIndex < hotbarIcons.Length)
        {
            SlotUI slotUI = hotbarIcons[currentItemIndex].transform.parent.GetComponent<SlotUI>();
            return slotUI != null && slotUI.isLocked;
        }
        return false;
    }
}