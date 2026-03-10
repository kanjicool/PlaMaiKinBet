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

            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(true);
                UpdatePromptUI();

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