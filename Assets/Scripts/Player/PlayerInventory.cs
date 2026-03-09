using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Shop & Money")]
    public int money = 500;
    // 🌟 เปลี่ยนจาก ItemData มาเก็บ GameObject ตัวเป็นๆ เพื่อให้สคริปต์ไอเทมไม่หาย
    public List<GameObject> myItems = new List<GameObject>();

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
                slotUI.itemIcon = hotbarIcons[i]; // 🌟 ผูกตัวแปรไอคอน

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
                slotUI.itemIcon = inventoryIcons[i]; // 🌟 ผูกตัวแปรไอคอน

                Transform lockIcon = slotObj.transform.Find("LockIcon");
                if (lockIcon != null) slotUI.lockImage = lockIcon.GetComponent<Image>();
            }
        }
    }

    private void Start()
    {
        // 🌟 จองพื้นที่ให้ช่องกระเป๋าเท่ากับจำนวน UI เป๊ะๆ (เริ่มต้นจะเป็น null ทั้งหมด)
        if (myItems.Count != inventoryIcons.Length)
        {
            myItems = new List<GameObject>(new GameObject[inventoryIcons.Length]);
        }

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
        if (goldText != null) goldText.text = "Gold : " + money;

        // 1. อัปเดต Hotbar Icons
        for (int i = 0; i < hotbarIcons.Length; i++)
        {
            if (i < itemSlots.Length && itemSlots[i] != null)
            {
                ItemHolder itemHolder = itemSlots[i].GetComponent<ItemHolder>();
                if (itemHolder != null && itemHolder.itemData != null)
                {
                    hotbarIcons[i].sprite = itemHolder.itemData.icon;
                    hotbarIcons[i].color = Color.white;
                    hotbarIcons[i].enabled = true;
                }
            }
            else
            {
                hotbarIcons[i].sprite = null;
                hotbarIcons[i].color = new Color(0, 0, 0, 0); // โปร่งใส
                hotbarIcons[i].enabled = true;
            }
        }

        // 2. อัปเดต Inventory Icons
        for (int i = 0; i < inventoryIcons.Length; i++)
        {
            if (i < myItems.Count && myItems[i] != null)
            {
                ItemHolder holder = myItems[i].GetComponent<ItemHolder>();
                if (holder != null && holder.itemData != null)
                {
                    inventoryIcons[i].sprite = holder.itemData.icon;
                    inventoryIcons[i].color = Color.white;
                    inventoryIcons[i].enabled = true;
                }
            }
            else
            {
                inventoryIcons[i].sprite = null;
                inventoryIcons[i].color = new Color(0, 0, 0, 0); // ซ่อนรูปถ้าช่องว่าง
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

        if (currentItemIndex != -1 && currentItemIndex < itemSlots.Length && itemSlots[currentItemIndex] != null)
        {
            itemSlots[currentItemIndex].SetActive(false);
        }

        if (currentItemIndex == index)
        {
            currentItemIndex = -1;
        }
        else
        {
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
        bool hasHotbarSpace = false;
        for (int i = 0; i < itemSlots.Length; i++)
            if (itemSlots[i] == null) { hasHotbarSpace = true; break; }

        int emptyInventoryIndex = -1;
        for (int i = 0; i < myItems.Count; i++)
        {
            if (myItems[i] == null) { emptyInventoryIndex = i; break; }
        }

        if (money >= item.price && (hasHotbarSpace || emptyInventoryIndex != -1))
        {
            money -= item.price;

            if (hasHotbarSpace)
            {
                UpdateHotbarAfterPurchase(item);
            }
            else
            {
                // 🌟 เสกไอเทมขึ้นมาเลย แล้วซ่อนไว้ลงกระเป๋า
                GameObject spawnedItem = Instantiate(item.itemPrefab, handTransform);
                ItemHolder holder = spawnedItem.GetComponent<ItemHolder>() ?? spawnedItem.AddComponent<ItemHolder>();
                holder.itemData = item;
                spawnedItem.SetActive(false);

                myItems[emptyInventoryIndex] = spawnedItem;
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
                ItemHolder holder = spawnedItem.GetComponent<ItemHolder>() ?? spawnedItem.AddComponent<ItemHolder>();
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
        Debug.Log($"ขายของสำเร็จ! ได้เงินมา {price} เหรียญ");

        if (currentItemIndex != -1 && itemSlots[currentItemIndex] == itemToSell)
        {
            itemSlots[currentItemIndex] = null;
            currentItemIndex = -1;
        }
        else // เช็คช่องอื่นใน Hotbar เผื่อไม่ได้ถืออยู่
        {
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i] == itemToSell) itemSlots[i] = null;
            }
        }

        // หาให้เจอว่าไอเทมนี้อยู่ช่องไหนในกระเป๋า แล้วเคลียร์เป็น null
        for (int i = 0; i < myItems.Count; i++)
        {
            if (myItems[i] == itemToSell)
            {
                myItems[i] = null;
                break;
            }
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

        if (!addedToHotbar)
        {
            int emptyInventoryIndex = -1;
            for (int i = 0; i < myItems.Count; i++)
            {
                if (myItems[i] == null) { emptyInventoryIndex = i; break; }
            }

            if (emptyInventoryIndex != -1)
            {
                GameObject spawnedFish = Instantiate(fishItem.itemPrefab, handTransform);
                ItemHolder holder = spawnedFish.GetComponent<ItemHolder>() ?? spawnedFish.AddComponent<ItemHolder>();
                holder.itemData = fishItem;
                spawnedFish.SetActive(false);

                myItems[emptyInventoryIndex] = spawnedFish;
                Debug.Log($"Hotbar เต็ม! เก็บ {fishItem.itemName} เข้า Inventory ช่องที่ {emptyInventoryIndex}");
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
                SlotUI slotUI = hotbarIcons[i].transform.parent.GetComponent<SlotUI>();
                if (slotUI != null && slotUI.isLocked) continue;

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

        // 2. Inventory
        for (int i = 0; i < myItems.Count; i++)
        {
            if (myItems[i] == null) continue;

            SlotUI slotUI = inventoryIcons[i].transform.parent.GetComponent<SlotUI>();
            bool isLocked = slotUI != null && slotUI.isLocked;
            bool isFish = myItems[i].GetComponent<FishHolder>() != null;

            if (isFish && !isLocked)
            {
                ItemHolder holder = myItems[i].GetComponent<ItemHolder>();
                totalEarnings += holder.itemData.price;
                fishCount++;

                Destroy(myItems[i]); // 🌟 ลบทิ้งเมื่อได้เงิน
                myItems[i] = null; // คืนที่ว่างให้กระเป๋า
            }
        }

        if (totalEarnings > 0) money += totalEarnings;
        UpdateInventoryUI();
        return totalEarnings;
    }

    // 🌟 3 ฟังก์ชันด้านล่างคือหัวใจสำคัญในการสลับ GameObject ข้ามไปมา
    public void SwapItems(SlotUI fromSlot, SlotUI toSlot)
    {
        GameObject fromObj = GetGameObjectFromSlot(fromSlot);
        GameObject toObj = GetGameObjectFromSlot(toSlot);

        SetGameObjectToSlot(fromSlot, toObj);
        SetGameObjectToSlot(toSlot, fromObj);

        // สลับสถานะแม่กุญแจ
        bool tempLock = fromSlot.isLocked;
        fromSlot.isLocked = toSlot.isLocked;
        if (fromSlot.lockImage != null) fromSlot.lockImage.enabled = fromSlot.isLocked;

        toSlot.isLocked = tempLock;
        if (toSlot.lockImage != null) toSlot.lockImage.enabled = toSlot.isLocked;

        UpdateInventoryUI();
    }

    private GameObject GetGameObjectFromSlot(SlotUI slot)
    {
        if (slot.slotType == SlotUI.SlotType.Hotbar)
            return itemSlots[slot.slotIndex];
        else if (slot.slotType == SlotUI.SlotType.Inventory)
            return slot.slotIndex < myItems.Count ? myItems[slot.slotIndex] : null;
        return null;
    }

    private void SetGameObjectToSlot(SlotUI slot, GameObject itemObj)
    {
        if (slot.slotType == SlotUI.SlotType.Hotbar)
        {
            itemSlots[slot.slotIndex] = itemObj;
            if (itemObj != null)
            {
                itemObj.transform.SetParent(handTransform); // เอาไปถือในมือ
                itemObj.SetActive(currentItemIndex == slot.slotIndex); // โชว์ถ้าถือช่องนั้นอยู่
            }
        }
        else if (slot.slotType == SlotUI.SlotType.Inventory)
        {
            if (slot.slotIndex < myItems.Count)
            {
                myItems[slot.slotIndex] = itemObj;
                if (itemObj != null)
                {
                    itemObj.transform.SetParent(handTransform);
                    itemObj.SetActive(false); // ซ่อนทันทีเมื่อลงกระเป๋า
                }
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