using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Shop & Money")]
    public int money = 500;
    public List<ItemData> myItems = new List<ItemData>();

    [Header("Hotbar Slots")]
    public Transform handTransform;
    public GameObject[] itemSlots = new GameObject[6];

    private InputSystem_Actions inputActions;
    private int currentItemIndex = -1;

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

    private void OnEnable() { inputActions.Enable(); }
    private void OnDisable() { inputActions.Disable(); }

    private void EquipItem(int index)
    {
        if (index >= itemSlots.Length || itemSlots[index] == null) return;

        if (currentItemIndex == index)
        {
            itemSlots[index].SetActive(false);
            currentItemIndex = -1;
            return;
        }

        if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
        {
            itemSlots[currentItemIndex].SetActive(false);
        }

        itemSlots[index].SetActive(true);
        currentItemIndex = index;
    }

    public bool BuyItem(ItemData item)
    {
        if (money >= item.price)
        {
            money -= item.price;
            myItems.Add(item);
            Debug.Log($"ซื้อ {item.itemName} สำเร็จ! เงินเหลือ: {money}");

            UpdateHotbarAfterPurchase(item);

            return true;
        }
        else
        {
            Debug.Log("เงินไม่พอ!");
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
                // 1. เสกไอเทมจาก Prefab ที่อยู่ใน ScriptableObject
                GameObject spawnedItem = Instantiate(item.itemPrefab, handTransform);

                // 2. ฝังข้อมูล ItemData กลับเข้าไป (เพื่อให้ระบบขายดึงข้อมูลไปใช้ได้)
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

    // --- ส่วนที่ใช้สำหรับระบบขายของ ---
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
    }

    public void AddFishToInventory(GameObject fishPrefab, FishData fishData)
    {
        if (fishPrefab == null) return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            // หาช่องว่างใน Hotbar
            if (itemSlots[i] == null)
            {
                // 1. สร้างตัวปลาขึ้นมาในมือ (HandTransform)
                GameObject spawnedFish = Instantiate(fishPrefab, handTransform);

                // 2. ใส่ข้อมูล FishHolder เพื่อให้ BuyerManager เช็คราคาขายได้
                FishHolder holder = spawnedFish.GetComponent<FishHolder>();
                if (holder == null) holder = spawnedFish.AddComponent<FishHolder>();
                holder.fishData = fishData;

                // 3. ตั้งค่าตำแหน่งและปิดไว้ก่อน (จะโชว์เมื่อกดเลขช่องนั้นๆ)
                spawnedFish.transform.localPosition = Vector3.zero;
                spawnedFish.transform.localRotation = Quaternion.identity;
                spawnedFish.SetActive(false);

                // 4. เก็บลงช่อง Slot
                itemSlots[i] = spawnedFish;

                Debug.Log($"ตกได้ {fishData.fishName} และเก็บเข้าช่อง {i + 1} แล้ว!");

                // ถ้าตอนนี้ไม่ได้ถืออะไรอยู่ ให้ถือปลาตัวนี้เลย
                if (currentItemIndex == -1) EquipItem(i);

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
            if (itemSlots[i] == null) // หาช่องว่างใน Hotbar
            {
                // 1. เสกปลาออกมาในมือ (HandTransform)
                GameObject spawnedFish = Instantiate(fishItem.itemPrefab, handTransform);

                // 2. ฝังข้อมูล ItemData (เพื่อให้พ่อค้าเช็คราคาขายได้)
                ItemHolder holder = spawnedFish.GetComponent<ItemHolder>();
                if (holder == null) holder = spawnedFish.AddComponent<ItemHolder>();
                holder.itemData = fishItem;

                // 3. ตั้งค่าตำแหน่งปลาในมือ
                spawnedFish.transform.localPosition = Vector3.zero;
                spawnedFish.transform.localRotation = Quaternion.identity;
                spawnedFish.SetActive(false); // ปิดไว้ก่อนจนกว่าจะเลือกใช้ช่องนี้

                // 4. เก็บลงในลิสต์ Slot และ List ข้อมูลหลัก
                itemSlots[i] = spawnedFish;
                myItems.Add(fishItem);

                Debug.Log($"ตกได้ {fishItem.itemName} เก็บเข้าช่องที่ {i}");

                // ถ้าตอนนี้ไม่ได้ถืออะไรอยู่ ให้ถือปลาตัวนี้ทันที
                if (currentItemIndex == -1) EquipItem(i);

                return;
            }
        }
        Debug.Log("กระเป๋าเต็ม! ปลาหลุดมือไปแล้ว");
    }
    // --- เพิ่มฟังก์ชันนี้เข้าไปใน PlayerInventory.cs ---
    public int SellAllFish()
    {
        int totalEarnings = 0;
        int fishCount = 0;

        // สร้าง List ไว้เก็บไอเทมที่จะลบออกจาก myItems เพื่อป้องกัน Error ตอนวนลูป
        List<ItemData> itemsToRemove = new List<ItemData>();

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {
                int itemPrice = 0;
                bool isSellable = false;

                // 1. ลองเช็กว่าเป็น FishHolder (เผื่อในอนาคตคุณใช้ Fish Data)
                FishHolder fishHolder = itemSlots[i].GetComponent<FishHolder>();
                if (fishHolder != null && fishHolder.fishData != null)
                {
                    itemPrice = fishHolder.fishData.price;
                    isSellable = true;
                }
                // 2. ถ้าไม่ใช่ ลองเช็กว่าเป็น ItemHolder (สำหรับ Goldfish ในรูปปัจจุบัน)
                else
                {
                    ItemHolder itemHolder = itemSlots[i].GetComponent<ItemHolder>();

                    // ต้องมีข้อมูล ItemData และชื่อต้องไม่มีคำว่า "Rod" เพื่อป้องกันการขายเบ็ดตกปลา
                    if (itemHolder != null && itemHolder.itemData != null && !itemHolder.itemData.name.Contains("Rod"))
                    {
                        itemPrice = itemHolder.itemData.price;
                        itemsToRemove.Add(itemHolder.itemData); // จดไว้เพื่อไปลบออกจาก List myItems หลัก
                        isSellable = true;
                    }
                }

                // ถ้าไอเทมช่องนี้เข้าเงื่อนไขว่าขายได้
                if (isSellable)
                {
                    totalEarnings += itemPrice;
                    fishCount++;

                    // ทำลายออบเจกต์ทิ้งและล้างช่อง Slot
                    Destroy(itemSlots[i]);
                    itemSlots[i] = null;

                    // เคลียร์สถานะการถือ หากบังเอิญช่องนั้นเป็นช่องที่เลือกอยู่
                    if (currentItemIndex == i)
                    {
                        currentItemIndex = -1;
                    }
                }
            }
        }

        // อัปเดตลบของออกจาก List myItems ในกระเป๋า
        foreach (var item in itemsToRemove)
        {
            myItems.Remove(item);
        }

        // เพิ่มเงินถ้ายอดรวมมากกว่า 0
        if (totalEarnings > 0)
        {
            money += totalEarnings;
            Debug.Log($"ขายปลาไปทั้งหมด {fishCount} ตัว ได้เงินมา {totalEarnings} เหรียญ ตอนนี้มีเงินทั้งหมด: {money}");
        }

        return totalEarnings;
    }
}