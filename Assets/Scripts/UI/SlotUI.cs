using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public enum SlotType { Hotbar, Inventory }
    
    [Header("Slot Info")]
    public SlotType slotType;
    public int slotIndex;
    public Image itemIcon;

    [Header("Lock System")]
    public bool isLocked = false;
    // 🌟 เปลี่ยนจาก GameObject เป็น Image
    public Image lockImage; 

    private PlayerInventory inventory;
    private static SlotUI slotBeingDragged;
    private static GameObject ghostIcon;

    private void Start()
    {
        inventory = FindFirstObjectByType<PlayerInventory>();
        
        // 🌟 ซ่อนรูปกุญแจตอนเริ่มเกม (ใช้ .enabled แทน .SetActive)
        if (lockImage != null) lockImage.enabled = false;
    }

    // --- 1. ระบบดับเบิ้ลคลิกเพื่อล็อก ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2) 
        {
            isLocked = !isLocked;
            
            // 🌟 โชว์/ซ่อน รูปกุญแจตามสถานะ
            if (lockImage != null) lockImage.enabled = isLocked;
            
            Debug.Log($"[SlotUI] {(isLocked ? "ล็อก" : "ปลดล็อก")} ช่อง {slotType} ที่ {slotIndex}");
        }
    }

    // --- 2. เริ่มลาก (ส่วนนี้เหมือนเดิม) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked || inventory == null) return;

        // 🌟 เปลี่ยนมาใช้ itemIcon ตรงๆ แทนการ Find
        if (itemIcon == null || itemIcon.sprite == null || itemIcon.color.a == 0) return;

        slotBeingDragged = this;

        ghostIcon = new GameObject("GhostIcon");
        ghostIcon.transform.SetParent(inventory.inventoryMenu.transform.parent);
        ghostIcon.transform.SetAsLastSibling();

        Image ghostImage = ghostIcon.AddComponent<Image>();
        ghostImage.sprite = itemIcon.sprite; // 🌟 ใช้ itemIcon
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
            inventory.SwapItems(slotBeingDragged, this);
        }
    }
}