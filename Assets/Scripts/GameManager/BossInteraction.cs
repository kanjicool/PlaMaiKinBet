using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))] // บังคับว่า GameObject นี้ต้องมี Collider
public class BossInteraction : MonoBehaviour
{
    [Header("UI Prompt (Optional)")]
    [Tooltip("ลาก GameObject หรือ Canvas ที่เขียนว่า 'กด E เพื่อส่งปลา' มาใส่ตรงนี้")]
    public GameObject interactPromptUI;

    private bool isPlayerNear = false;

    private void Start()
    {
        // ซ่อนป้ายกด E ไว้ก่อนตอนเริ่มเกม
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // เช็กว่าคนที่เข้ามาชนคือ Player หรือไม่
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            // เปิดโชว์ป้ายกด E
            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(true);
            }

            if (GameLoopManager.Instance != null && GameLoopManager.Instance.currentQuestFish != null)
            {
                Debug.Log($"[Boss] เข้าใกล้บอสแล้ว! กด 'E' เพื่อส่ง {GameLoopManager.Instance.currentQuestFish.fishName}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;

            // ซ่อนป้ายกด E เมื่อผู้เล่นเดินออกไป
            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(false);
            }

            Debug.Log("[Boss] ผู้เล่นออกจากระยะส่งเควสต์");
        }
    }

    private void Update()
    {
        // ถ้าผู้เล่นไม่ได้อยู่ใกล้บอส ก็ไม่ต้องทำอะไร
        if (!isPlayerNear) return;

        // เช็กการกดปุ่ม E (ดึงจาก Keyboard ปัจจุบันของ New Input System)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (GameLoopManager.Instance != null)
            {
                // เรียกฟังก์ชันส่งปลาที่เราเขียนไว้ใน GameLoopManager
                GameLoopManager.Instance.TryFeedBoss();
            }
        }
    }
}