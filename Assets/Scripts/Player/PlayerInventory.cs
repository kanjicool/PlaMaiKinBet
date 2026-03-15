using System.Collections; // 🌟 เพิ่มบรรทัดนี้เพื่อใช้งาน Coroutine
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Shop & Money")]
    public int money = 500;
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

    // 🌟 ตัวแปรแสดงชื่อไอเทม และระบบตั้งเวลา
    public TextMeshProUGUI heldItemNameText;
    public float nameDisplayDuration = 2.5f; // เวลาที่จะให้ชื่อโชว์ค้างไว้ (หน่วยเป็นวินาที)
    private Coroutine hideNameCoroutine; // ตัวแปรเก็บ Coroutine เพื่อใช้ยกเลิกถ้ารีบเปลี่ยนของ

    [Header("Bait System")]
    public GameObject baitSlotUI;
    public Image baitIcon;
    public GameObject currentBaitItem;

    private AudioSource audioSource;
    private InputSystem_Actions inputActions;
    private int currentItemIndex = -1;
    public bool isInventoryOpen = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

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
        for (int i = 0; i < hotbarIcons.Length; i++)
        {
            if (hotbarIcons[i] != null)
            {
                GameObject slotObj = hotbarIcons[i].transform.parent.gameObject;
                SlotUI slotUI = slotObj.GetComponent<SlotUI>();
                if (slotUI == null) slotUI = slotObj.AddComponent<SlotUI>();

                slotUI.slotType = SlotUI.SlotType.Hotbar;
                slotUI.slotIndex = i;
                slotUI.itemIcon = hotbarIcons[i];

                Transform lockIcon = slotObj.transform.Find("LockIcon");
                if (lockIcon != null) slotUI.lockImage = lockIcon.GetComponent<Image>();
            }
        }

        for (int i = 0; i < inventoryIcons.Length; i++)
        {
            if (inventoryIcons[i] != null)
            {
                GameObject slotObj = inventoryIcons[i].transform.parent.gameObject;
                SlotUI slotUI = slotObj.GetComponent<SlotUI>();
                if (slotUI == null) slotUI = slotObj.AddComponent<SlotUI>();

                slotUI.slotType = SlotUI.SlotType.Inventory;
                slotUI.slotIndex = i;
                slotUI.itemIcon = inventoryIcons[i];

                Transform lockIcon = slotObj.transform.Find("LockIcon");
                if (lockIcon != null) slotUI.lockImage = lockIcon.GetComponent<Image>();
            }
        }

        if (baitIcon != null)
        {
            GameObject slotObj = baitIcon.transform.parent.gameObject;
            SlotUI slotUI = slotObj.GetComponent<SlotUI>();
            if (slotUI == null) slotUI = slotObj.AddComponent<SlotUI>();

            slotUI.slotType = SlotUI.SlotType.Bait;
            slotUI.slotIndex = 0;
            slotUI.itemIcon = baitIcon;

            Transform lockIcon = slotObj.transform.Find("LockIcon");
            if (lockIcon != null) slotUI.lockImage = lockIcon.GetComponent<Image>();
        }
    }

    private void Start()
    {
        if (myItems.Count != inventoryIcons.Length)
        {
            myItems = new List<GameObject>(new GameObject[inventoryIcons.Length]);
        }

        if (inventoryMenu != null) inventoryMenu.SetActive(false);

        // ซ่อนข้อความไว้ตั้งแต่เริ่มเกม
        if (heldItemNameText != null) heldItemNameText.gameObject.SetActive(false);

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
                hotbarIcons[i].color = new Color(0, 0, 0, 0);
                hotbarIcons[i].enabled = true;
            }
        }

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
                inventoryIcons[i].color = new Color(0, 0, 0, 0);
                inventoryIcons[i].enabled = true;
            }
        }

        bool isHoldingFishingRod = false;

        // 🌟 2. อัปเดตการแสดงชื่อไอเทมแบบชั่วคราว
        if (heldItemNameText != null)
        {
            if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
            {
                ItemHolder holder = itemSlots[currentItemIndex].GetComponent<ItemHolder>();
                if (holder != null && holder.itemData != null)
                {
                    heldItemNameText.text = holder.itemData.name;
                    heldItemNameText.gameObject.SetActive(true); // เปิดให้มองเห็นข้อความ

                    // ยกเลิกการนับเวลาเก่า (ถ้ามี) แล้วเริ่มนับถอยหลังซ่อนข้อความใหม่
                    if (hideNameCoroutine != null) StopCoroutine(hideNameCoroutine);
                    hideNameCoroutine = StartCoroutine(HideItemNameRoutine());

                    if (holder.itemData.isFishingRod) isHoldingFishingRod = true;
                }
            }
            else
            {
                heldItemNameText.gameObject.SetActive(false); // ซ่อนข้อความถ้าไม่ได้ถืออะไร
            }
        }

        if (baitSlotUI != null)
        {
            baitSlotUI.SetActive(isHoldingFishingRod);

            if (baitIcon != null)
            {
                if (currentBaitItem != null)
                {
                    ItemHolder baitHolder = currentBaitItem.GetComponent<ItemHolder>();
                    if (baitHolder != null && baitHolder.itemData != null)
                    {
                        baitIcon.sprite = baitHolder.itemData.icon;
                        baitIcon.color = Color.white;
                        baitIcon.enabled = true;
                    }
                }
                else
                {
                    baitIcon.sprite = null;
                    baitIcon.color = new Color(0, 0, 0, 0);
                    baitIcon.enabled = true;
                }
            }
        }

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

    // 🌟 3. ฟังก์ชัน Coroutine สำหรับนับเวลาถอยหลังซ่อนชื่อไอเทม
    private IEnumerator HideItemNameRoutine()
    {
        yield return new WaitForSeconds(nameDisplayDuration); // รอเวลาตามที่ตั้งไว้

        if (heldItemNameText != null)
        {
            heldItemNameText.gameObject.SetActive(false); // ปิดการแสดงผลข้อความ
        }
    }

    public void EquipItem(int index)
    {
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.IsAttacking()) return;

        PlayerController playerCtrl = FindFirstObjectByType<PlayerController>();
        if (playerCtrl != null && playerCtrl.IsBusy) return;

        if (index >= itemSlots.Length) return;

        if (audioSource != null) audioSource.Stop();

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
                ApplyItemTransform(itemSlots[currentItemIndex]);
                itemSlots[currentItemIndex].SetActive(true);

                ItemHolder holder = itemSlots[currentItemIndex].GetComponent<ItemHolder>();
                if (holder != null && holder.itemData != null && holder.itemData.equipSound != null)
                {
                    audioSource.PlayOneShot(holder.itemData.equipSound);
                }
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
                GameObject spawnedItem = Instantiate(item.itemPrefab, handTransform);
                ItemHolder holder = spawnedItem.GetComponent<ItemHolder>() ?? spawnedItem.AddComponent<ItemHolder>();
                holder.itemData = item;
                spawnedItem.SetActive(false);

                myItems[emptyInventoryIndex] = spawnedItem;
            }

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

                spawnedItem.transform.SetParent(handTransform);
                spawnedItem.transform.localPosition = item.holdPositionOffset;
                spawnedItem.transform.localRotation = Quaternion.Euler(item.holdRotationOffset);
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

        if (currentItemIndex != -1 && itemSlots[currentItemIndex] == itemToSell)
        {
            itemSlots[currentItemIndex] = null;
            currentItemIndex = -1;
        }
        else
        {
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i] == itemToSell) itemSlots[i] = null;
            }
        }

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
                    Destroy(itemSlots[i]);
                    itemSlots[i] = null;
                    if (currentItemIndex == i) currentItemIndex = -1;
                }
            }
        }

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

                Destroy(myItems[i]);
                myItems[i] = null;
            }
        }

        if (totalEarnings > 0) money += totalEarnings;
        UpdateInventoryUI();
        return totalEarnings;
    }

    public void SwapItems(SlotUI fromSlot, SlotUI toSlot)
    {
        GameObject fromObj = GetGameObjectFromSlot(fromSlot);
        GameObject toObj = GetGameObjectFromSlot(toSlot);

        SetGameObjectToSlot(fromSlot, toObj);
        SetGameObjectToSlot(toSlot, fromObj);

        bool tempLock = fromSlot.isLocked;
        fromSlot.isLocked = toSlot.isLocked;
        if (fromSlot.lockImage != null) fromSlot.lockImage.enabled = fromSlot.isLocked;

        toSlot.isLocked = tempLock;
        if (toSlot.lockImage != null) toSlot.lockImage.enabled = toSlot.isLocked;

        UpdateInventoryUI();
    }

    public GameObject GetGameObjectFromSlot(SlotUI slot)
    {
        if (slot.slotType == SlotUI.SlotType.Hotbar)
            return itemSlots[slot.slotIndex];
        else if (slot.slotType == SlotUI.SlotType.Inventory)
            return slot.slotIndex < myItems.Count ? myItems[slot.slotIndex] : null;
        else if (slot.slotType == SlotUI.SlotType.Bait)
            return currentBaitItem;

        return null;
    }

    private void SetGameObjectToSlot(SlotUI slot, GameObject itemObj)
    {
        if (slot.slotType == SlotUI.SlotType.Hotbar)
        {
            itemSlots[slot.slotIndex] = itemObj;
            if (itemObj != null)
            {
                itemObj.transform.SetParent(handTransform);
                ApplyItemTransform(itemObj);
                itemObj.SetActive(currentItemIndex == slot.slotIndex);
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
                    ApplyItemTransform(itemObj);
                    itemObj.SetActive(false);
                }
            }
        }
        else if (slot.slotType == SlotUI.SlotType.Bait)
        {
            currentBaitItem = itemObj;
            if (itemObj != null)
            {
                itemObj.transform.SetParent(handTransform);
                itemObj.SetActive(false);
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

    public int GetItemCount(ItemData itemToCheck)
    {
        int count = 0;
        if (itemToCheck == null) return count;

        foreach (GameObject slotObj in itemSlots)
        {
            if (slotObj != null)
            {
                ItemHolder holder = slotObj.GetComponent<ItemHolder>();
                if (holder != null && holder.itemData == itemToCheck) count++;
            }
        }

        foreach (GameObject itemObj in myItems)
        {
            if (itemObj != null)
            {
                ItemHolder holder = itemObj.GetComponent<ItemHolder>();
                if (holder != null && holder.itemData == itemToCheck) count++;
            }
        }
        return count;
    }

    public void ConsumeItems(ItemData itemToConsume, int amountToConsume)
    {
        int removedCount = 0;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (removedCount >= amountToConsume) break;

            if (itemSlots[i] != null)
            {
                ItemHolder holder = itemSlots[i].GetComponent<ItemHolder>();
                if (holder != null && holder.itemData == itemToConsume)
                {
                    Destroy(itemSlots[i]);
                    itemSlots[i] = null;
                    if (currentItemIndex == i) currentItemIndex = -1;
                    removedCount++;
                }
            }
        }

        for (int i = 0; i < myItems.Count; i++)
        {
            if (removedCount >= amountToConsume) break;

            if (myItems[i] != null)
            {
                ItemHolder holder = myItems[i].GetComponent<ItemHolder>();
                if (holder != null && holder.itemData == itemToConsume)
                {
                    Destroy(myItems[i]);
                    myItems[i] = null;
                    removedCount++;
                }
            }
        }

        UpdateInventoryUI();
    }

    public int GetCurrentHoldAnimID()
    {
        if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
        {
            ItemHolder holder = itemSlots[currentItemIndex].GetComponent<ItemHolder>();
            if (holder != null && holder.itemData != null)
            {
                return holder.itemData.holdAnimID;
            }
        }
        return 0;
    }

    private void ApplyItemTransform(GameObject itemObj)
    {
        if (itemObj == null) return;

        ItemHolder holder = itemObj.GetComponent<ItemHolder>();
        if (holder != null && holder.itemData != null)
        {
            itemObj.transform.localPosition = holder.itemData.holdPositionOffset;
            itemObj.transform.localRotation = Quaternion.Euler(holder.itemData.holdRotationOffset);
        }
    }

    public bool PickupGroundItem(GameObject groundItem)
    {
        ItemHolder holder = groundItem.GetComponent<ItemHolder>();
        if (holder == null || holder.itemData == null) return false;

        int hotbarIndex = -1;
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null) { hotbarIndex = i; break; }
        }

        int invIndex = -1;
        if (hotbarIndex == -1)
        {
            for (int i = 0; i < myItems.Count; i++)
            {
                if (myItems[i] == null) { invIndex = i; break; }
            }
        }

        if (hotbarIndex != -1 || invIndex != -1)
        {
            Rigidbody rb = groundItem.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            Collider coll = groundItem.GetComponent<Collider>();
            if (coll != null) coll.enabled = false;

            groundItem.transform.SetParent(handTransform);
            ApplyItemTransform(groundItem);
            groundItem.SetActive(false);

            if (hotbarIndex != -1)
            {
                itemSlots[hotbarIndex] = groundItem;
                if (currentItemIndex == -1) EquipItem(hotbarIndex);
            }
            else
            {
                myItems[invIndex] = groundItem;
            }

            UpdateInventoryUI();
            return true;
        }

        return false;
    }

    public void DropHeldItem()
    {
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.IsAttacking()) return;

        if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
        {
            GameObject itemToDrop = itemSlots[currentItemIndex];

            itemSlots[currentItemIndex] = null;
            currentItemIndex = -1;

            itemToDrop.transform.SetParent(null);
            itemToDrop.SetActive(true);

            Collider coll = itemToDrop.GetComponent<Collider>();
            if (coll != null) coll.enabled = true;

            Rigidbody rb = itemToDrop.GetComponent<Rigidbody>();
            if (rb == null) rb = itemToDrop.AddComponent<Rigidbody>();

            Vector3 throwDirection = transform.forward + Vector3.up * 0.5f;
            rb.AddForce(throwDirection * 3f, ForceMode.Impulse);

            UpdateInventoryUI();
        }
    }

    public void UseSlashTransform()
    {
        if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
        {
            GameObject heldItem = itemSlots[currentItemIndex];
            ItemHolder holder = heldItem.GetComponent<ItemHolder>();

            if (holder != null && holder.itemData != null)
            {
                heldItem.transform.localPosition = holder.itemData.attackHoldPositionOffset;
                heldItem.transform.localRotation = Quaternion.Euler(holder.itemData.attackHoldRotationOffset);
            }
        }
    }

    public void ResetToHoldTransform()
    {
        if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
        {
            ApplyItemTransform(itemSlots[currentItemIndex]);
        }
    }

    public void ForceUnequip()
    {
        if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
        {
            itemSlots[currentItemIndex].SetActive(false);
            currentItemIndex = -1;
            UpdateInventoryUI();

            PlayerController ctrl = GetComponent<PlayerController>();
            if (ctrl != null) ctrl.SendMessage("UpdateHoldAnimation", SendMessageOptions.DontRequireReceiver);
        }
    }

    public ItemData GetCurrentBaitData()
    {
        if (currentBaitItem != null)
        {
            ItemHolder holder = currentBaitItem.GetComponent<ItemHolder>();
            if (holder != null) return holder.itemData;
        }
        return null;
    }

    public void ConsumeBait()
    {
        if (currentBaitItem != null)
        {
            Destroy(currentBaitItem);
            currentBaitItem = null;
            UpdateInventoryUI();
        }
    }

    public void ConsumeHeldItem()
    {
        // 1. ดึง GameObject ของที่ถืออยู่ในมืออกมา
        GameObject heldObject = GetHeldItem();

        if (heldObject != null)
        {
            // 2. ทำลายโมเดลยาที่ถืออยู่ทิ้ง
            Destroy(heldObject);

            // 3. (สำคัญ) ลบข้อมูลออกจากระบบ Inventory ของคุณด้วย 
            // สมมติว่าคุณใช้ตัวแปรชื่อ currentEquippedItem หรือคล้ายๆ กันให้เคลียร์เป็น null
            // และถ้าคุณมี List ในกระเป๋า ก็อย่าลืมสั่ง Remove ออกด้วยครับ

            Debug.Log("Item consumed and removed from hand.");
        }
    }
}