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
        if (item.itemPrefab == null)
        {
            Debug.LogWarning($"ไอเทม {item.itemName} ไม่มี 3D Model ให้เสก!");
            return;
        }

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                GameObject spawnedItem = Instantiate(item.itemPrefab, handTransform);

                spawnedItem.transform.localPosition = Vector3.zero;
                spawnedItem.transform.localRotation = Quaternion.identity;

                spawnedItem.SetActive(false);

                itemSlots[i] = spawnedItem;
                Debug.Log($"นำ {item.itemName} ใส่ใน Hotbar ช่องที่ {i + 1} แล้ว!");

                if (currentItemIndex == -1)
                {
                    EquipItem(i);
                }

                return;
            }
        }
        Debug.Log("Hotbar เต็มแล้ว! (ไม่มีช่องว่าง)");
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
}