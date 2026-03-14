using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class BossInteraction : MonoBehaviour
{
    [Header("UI Prompt (Optional)")]
    public GameObject interactPromptUI;

    [Header("Boss Reference")]
    public BossRobotController bossController;

    private bool isPlayerNear = false;

    private void Start()
    {
        if (bossController == null)
        {
            bossController = GetComponentInParent<BossRobotController>();
        }

        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            Debug.Log($"bossController : {bossController}");

            if (bossController != null)
            {
                bossController.isPlayerNear = true;
                bossController.interactPlayerTransform = other.transform;
            }

            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(true);
                UpdatePromptUI();
            }

            // --- ส่วนที่แก้ไข ---
            // เปลี่ยนจากการดึง currentQuestFish ตัวเดียว มาเป็นการวนลูปเอาชื่อปลาทั้งหมดใน List มาแสดง
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.currentQuests.Count > 0)
            {
                string questList = "";
                foreach (var quest in GameLoopManager.Instance.currentQuests)
                {
                    questList += $"{quest.fish.fishName} ({quest.amount} ตัว), ";
                }

                // ตัดลูกน้ำอันสุดท้ายออกเพื่อความสวยงาม
                questList = questList.TrimEnd(',', ' ');

                Debug.Log($"[Boss] เข้าใกล้บอสแล้ว! กด 'E' เพื่อส่ง: {questList}");
            }
            // -------------------
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;

            if (bossController != null)
            {
                bossController.isPlayerNear = false;
                bossController.interactPlayerTransform = null;
            }

            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!isPlayerNear) return;

        UpdatePromptUI();

        if (GameLoopManager.Instance != null && GameLoopManager.Instance.bossState == BossState.RAMPAGING)
        {
            return;
        }

        // ตรวจสอบการกดปุ่ม E
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.TryFeedBoss();
            }
        }
    }

    private void UpdatePromptUI()
    {
        if (interactPromptUI == null) return;

        bool canInteract = GameLoopManager.Instance != null && GameLoopManager.Instance.bossState != BossState.RAMPAGING;

        interactPromptUI.SetActive(canInteract);
    }
}