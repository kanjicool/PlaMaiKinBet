using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class FishingRod : MonoBehaviour
{
    [Header("Rod References")]
    public Transform rodTip;
    public GameObject bobberPrefab;

    [Header("Casting Settings")]
    public float maxCastForce = 25f;
    public float chargeSpeed = 20f;
    public float upwardForce = 5f;

    [Header("Line Settings")]
    public float maxLineDistance = 30f;

    private LineRenderer lineRenderer;
    private GameObject currentBobberObj;
    private Bobber activeBobber;
    private InputSystem_Actions inputActions;

    private bool isCharging = false;
    private float currentCharge = 0f;
    private int chargeDirection = 1;

    private FishController currentHookedFish;

    private PlayerFishing playerFishing;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        inputActions = new InputSystem_Actions();
        inputActions.Player.Fire.started += OnFireStarted;
        inputActions.Player.Fire.canceled += OnFireCanceled;

        playerFishing = GetComponentInParent<PlayerFishing>();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        if (currentBobberObj != null || currentHookedFish != null) lineRenderer.enabled = true;
        if (playerFishing != null) playerFishing.SetEquippedState(true);
    }

    private void OnDisable()
    {
        inputActions.Disable();
        lineRenderer.enabled = false;
        isCharging = false;
        if (playerFishing != null) playerFishing.SetEquippedState(false);

        CancelFishing();
    }

    private void OnDestroy() { inputActions.Dispose(); }

    private void Update()
    {
        UpdateLineRenderer();
        HandleCharging();

        CheckFishingConditions();
    }

    private void CheckFishingConditions()
    {
        if (transform.parent == null && currentBobberObj != null)
        {
            CancelFishing();
            return;
        }

        if (currentBobberObj != null)
        {
            float distance = Vector3.Distance(transform.root.position, currentBobberObj.transform.position);

            if (distance > maxLineDistance)
            {
                CancelFishing();
            }
        }
    }

    private void UpdateLineRenderer()
    {
        if (!lineRenderer.enabled) return;

        lineRenderer.SetPosition(0, rodTip.position);

        if (currentBobberObj != null)
            lineRenderer.SetPosition(1, currentBobberObj.transform.position);
        else if (currentHookedFish != null)
            lineRenderer.SetPosition(1, currentHookedFish.transform.position);
    }

    private void HandleCharging()
    {
        if (!isCharging) return;

        currentCharge += chargeSpeed * chargeDirection * Time.deltaTime;
        if (currentCharge >= maxCastForce)
        {
            currentCharge = maxCastForce;
            chargeDirection = -1;
        }
        else if (currentCharge <= 0)
        {
            currentCharge = 0;
            chargeDirection = 1;
        }

        UIManager.Instance.UpdateCastBar(currentCharge, maxCastForce);

        float chargePercentage = currentCharge / maxCastForce;
        if (playerFishing != null)
        {
            playerFishing.UpdateChargeAnimation(chargePercentage);
        }
    }

    private void OnFireStarted(InputAction.CallbackContext context)
    {
        if (!gameObject.activeInHierarchy) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null && inventory.isInventoryOpen)
        {
            return;
        }

        if (currentBobberObj != null || currentHookedFish != null)
        {
            CancelFishing();
        }
        else
        {
            isCharging = true;
            currentCharge = 0f;
            chargeDirection = 1;
            UIManager.Instance.ShowCastBar();

            if (playerFishing != null) playerFishing.StartCharging();
        }
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        if (!isCharging) return;
        isCharging = false;
        UIManager.Instance.HideCastBar();

        if (playerFishing != null) playerFishing.ExecuteCast();

        CastLine();
    }

    private void CastLine()
    {
        currentBobberObj = Instantiate(bobberPrefab, rodTip.position, Quaternion.identity);
        activeBobber = currentBobberObj.GetComponent<Bobber>();
        lineRenderer.enabled = true;

        if (activeBobber != null)
        {
            activeBobber.OnFishBitten += HandleFishBite;

            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory != null)
            {
                ItemData baitData = inventory.GetCurrentBaitData();

                // 🌟 ใส่ Debug มาเช็คว่าเจอเหยื่อไหม
                if (baitData != null)
                {
                    Debug.Log("<color=green>คันเบ็ด: เจอเหยื่อชื่อ " + baitData.name + " กำลังจะสร้างโมเดลห้อยทุ่น!</color>");

                    if (baitData.itemPrefab != null)
                    {
                        activeBobber.SetBaitVisual(baitData.itemPrefab);
                    }
                    else
                    {
                        Debug.LogWarning("<color=red>คันเบ็ด: เจอเหยื่อ แต่คุณลืมใส่ Item Prefab ในไฟล์ ItemData ของเหยื่อตัวนี้!</color>");
                    }
                }
                else
                {
                    Debug.Log("<color=yellow>คันเบ็ด: ไม่มีเหยื่อในช่อง (Bait Slot ว่างเปล่า)</color>");
                }
            }
        }

        if (currentBobberObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Vector3 forceDir = (Camera.main.transform.forward * currentCharge) + (Vector3.up * upwardForce);
            rb.AddForce(forceDir, ForceMode.Impulse);
        }
        currentCharge = 0f;
    }

    private void HandleFishBite(FishController fish)
    {
        Debug.Log("2. คันเบ็ด (FishingRod) รับทราบจากทุ่น กำลังจะเปิดมินิเกม!");

        // 🌟 หักเหยื่อออกจากช่อง UI ตอนปลากินทันที
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.ConsumeBait();
        }

        currentHookedFish = fish;

        FishingMiniGame.Instance.StartMiniGame(
            fish.myData.escapePower,
            OnMinigameWin,
            OnMinigameLose
        );
    }

    private void OnMinigameWin()
    {
        if (currentBobberObj != null) Destroy(currentBobberObj);

        Transform pullTarget = transform.parent != null ? transform.parent : transform;

        currentHookedFish.StartReeling(pullTarget, () => {
            lineRenderer.enabled = false;

            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory != null && currentHookedFish.myData.fishItemData != null)
            {
                inventory.AddCaughtFishToHotbar(currentHookedFish.myData.fishItemData);
                CheckQuestCompletion(currentHookedFish);
            }

            Destroy(currentHookedFish.gameObject);
            currentHookedFish = null;
        });
    }

    private void OnMinigameLose()
    {
        CancelFishing();
        if (currentHookedFish != null)
        {
            currentHookedFish.Escape();
            currentHookedFish = null;
        }
    }

    private void CancelFishing()
    {
        if (currentBobberObj != null) Destroy(currentBobberObj);
        lineRenderer.enabled = false;
        isCharging = false;

        if (playerFishing != null) playerFishing.CancelFishing();
    }

    private void CheckQuestCompletion(FishController fish)
    {
        if (GameLoopManager.Instance == null || GameLoopManager.Instance.currentQuests.Count == 0) return;

        bool isQuestFish = false;

        foreach (var quest in GameLoopManager.Instance.currentQuests)
        {
            if (fish.myData.fishItemData == quest.fish.fishItemData)
            {
                isQuestFish = true;
                break;
            }
        }

        if (isQuestFish)
        {
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory == null) return;

            bool allQuestsCompleted = true;

            foreach (var quest in GameLoopManager.Instance.currentQuests)
            {
                int currentAmount = inventory.GetItemCount(quest.fish.fishItemData);
                if (currentAmount < quest.amount)
                {
                    allQuestsCompleted = false;
                    break;
                }
            }

            if (allQuestsCompleted)
            {
                Debug.Log("ได้ปลาครบตามเควสต์แล้ว! กลับไปหาบอสกันเถอะ!");
                if (GameLoopManager.Instance.compass != null)
                {
                    GameLoopManager.Instance.compass.SetTarget(GameLoopManager.Instance.hubIsland);
                }
            }
        }
    }
}