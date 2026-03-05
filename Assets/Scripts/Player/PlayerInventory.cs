using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Shop & Money")]
    public int money = 500;
    public List<ItemData> myItems = new List<ItemData>();

    [Header("Hotbar Slots")]
    public Transform handTransform; // เพิ่มบรรทัดนี้: ตำแหน่งที่ของจะไปโผล่ในมือ
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

            // เรียกใช้ฟังก์ชันนำไอเทมเข้า Hotbar
            UpdateHotbarAfterPurchase(item);

            return true;
        }
        else
        {
            Debug.Log("เงินไม่พอ!");
            return false;
        }
    }

    // --- อัปเดตฟังก์ชันนี้ ---
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

                // เอาของใส่ช่อง
                itemSlots[i] = spawnedItem;
                Debug.Log($"นำ {item.itemName} ใส่ใน Hotbar ช่องที่ {i + 1} แล้ว!");

                // 👇 --- เพิ่ม 4 บรรทัดนี้เข้าไป --- 👇
                if (currentItemIndex == -1) // เช็กว่าถ้าตอนนี้ไม่ได้ถืออะไรอยู่เลย
                {
                    EquipItem(i); // สั่งให้หยิบของที่เพิ่งซื้อขึ้นมาถือทันที!
                }

                return;
            }
        }
        Debug.Log("Hotbar เต็มแล้ว! (ไม่มีช่องว่าง)");
    }
}