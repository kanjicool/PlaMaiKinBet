using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public enum SlotType { Hotbar, Inventory, Bait }

    [Header("Slot Info")]
    public SlotType slotType;
    public int slotIndex;
    public Image itemIcon;

    [Header("Lock System")]
    public bool isLocked = false;
    public Image lockImage;

    private PlayerInventory inventory;
    private static SlotUI slotBeingDragged;
    private static GameObject ghostIcon;

    private void Start()
    {
        inventory = FindFirstObjectByType<PlayerInventory>();

        if (lockImage != null) lockImage.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 🌟 1. เช็คถ้าคลิก 1 ครั้ง และเป็นช่อง Hotbar ให้ทำการถือไอเทม
        if (eventData.clickCount == 1)
        {
            if (slotType == SlotType.Hotbar && inventory != null)
            {
                // โค้ดจะอนุญาตให้สลับถือของ/เก็บของได้เลยเมื่อคลิก
                inventory.EquipItem(slotIndex);
            }
        }
        // 2. เช็คถ้าดับเบิ้ลคลิก (โค้ดเดิม) เพื่อล็อก/ปลดล็อกไอเทม
        else if (eventData.clickCount == 2)
        {
            if (itemIcon == null || itemIcon.sprite == null || itemIcon.color.a == 0)
            {
                Debug.LogWarning("ช่องว่าง ไม่สามารถล็อกได้!");
                return;
            }

            isLocked = !isLocked;

            if (lockImage != null) lockImage.enabled = isLocked;

            Debug.Log($"[SlotUI] {(isLocked ? "ล็อก" : "ปลดล็อก")} ช่อง {slotType} ที่ {slotIndex}");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked || inventory == null) return;

        if (itemIcon == null || itemIcon.sprite == null || itemIcon.color.a == 0) return;

        slotBeingDragged = this;

        ghostIcon = new GameObject("GhostIcon");
        ghostIcon.transform.SetParent(inventory.inventoryMenu.transform.parent);
        ghostIcon.transform.SetAsLastSibling();

        Image ghostImage = ghostIcon.AddComponent<Image>();
        ghostImage.sprite = itemIcon.sprite;
        ghostImage.raycastTarget = false;

        RectTransform ghostRect = ghostIcon.GetComponent<RectTransform>();
        ghostRect.sizeDelta = itemIcon.rectTransform.sizeDelta;
        ghostRect.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null) ghostIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            Destroy(ghostIcon);
            slotBeingDragged = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (slotBeingDragged != null && slotBeingDragged != this && !isLocked)
        {
            if (this.slotType == SlotType.Bait)
            {
                GameObject draggedObj = inventory.GetGameObjectFromSlot(slotBeingDragged);
                if (draggedObj != null)
                {
                    ItemHolder holder = draggedObj.GetComponent<ItemHolder>();
                    if (holder == null || holder.itemData == null || !holder.itemData.isBait)
                    {
                        Debug.LogWarning("ช่องนี้ใส่ได้เฉพาะเหยื่อตกปลาเท่านั้น!");
                        return;
                    }
                }
            }

            if (slotBeingDragged.slotType == SlotType.Bait)
            {
                GameObject targetObj = inventory.GetGameObjectFromSlot(this);
                if (targetObj != null)
                {
                    ItemHolder targetHolder = targetObj.GetComponent<ItemHolder>();
                    if (targetHolder != null && targetHolder.itemData != null && !targetHolder.itemData.isBait)
                    {
                        Debug.LogWarning("สลับออกได้กับช่องว่าง หรือเหยื่อด้วยกันเท่านั้น!");
                        return;
                    }
                }
            }

            inventory.SwapItems(slotBeingDragged, this);
        }
    }
}