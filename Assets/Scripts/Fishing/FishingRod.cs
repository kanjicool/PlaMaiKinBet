using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(LineRenderer))]
public class FishingRod : MonoBehaviour
{
    [Header("Rod References")]
    public Transform rodTip;
    public GameObject bobberPrefab;

    [Header("Casting Settings (ÃÐººªÒÃì¨¾ÅÑ§)")]
    public float maxCastForce = 25f;  // áÃ§»ÒÊÙ§ÊØ´
    public float chargeSpeed = 20f;   // ¤ÇÒÁàÃçÇã¹¡ÒÃªÒÃì¨à¡¨
    public float upwardForce = 5f;

    private LineRenderer lineRenderer;
    private GameObject currentBobber;
    private InputSystem_Actions inputActions;

    private bool isCharging = false;
    private float currentCharge = 0f;
    private int chargeDirection = 1;

    private FishController currentHookedFish;

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
    }

    private void OnFireStarted(InputAction.CallbackContext context)
    {
        StartCasting();
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        ReleaseCast();
    }

    private void OnDestroy()
    {
        inputActions.Player.Fire.started -= OnFireStarted;
        inputActions.Player.Fire.canceled -= OnFireCanceled;

        inputActions.Dispose();
    }


    private void OnEnable()
    {
        inputActions.Enable();
        if (currentBobber != null) lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        lineRenderer.enabled = false;
        isCharging = false;
    }

    private void Update()
    {
        if (lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, rodTip.position);

            if (currentBobber != null)
            {
                lineRenderer.SetPosition(1, currentBobber.transform.position);
            }
            else if (currentHookedFish != null)
            {
                lineRenderer.SetPosition(1, currentHookedFish.transform.position);
            }
        }

        if (isCharging)
        {
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
            //Debug.Log($"¡ÓÅÑ§ªÒÃì¨¾ÅÑ§... {currentCharge:F1}");
        }
    }

    private void StartCasting()
    {
        if (!gameObject.activeInHierarchy) return;

        if (currentBobber != null)
        {
            Destroy(currentBobber);
            lineRenderer.enabled = false;
            isCharging = false;
            UIManager.Instance.HideCastBar();
        }
        else
        {
            isCharging = true;
            currentCharge = 0f;
            chargeDirection = 1;
            UIManager.Instance.ShowCastBar();
        }
    }

    private void ReleaseCast()
    {
        if (!isCharging) return;
        isCharging = false;
        UIManager.Instance.HideCastBar();

        currentBobber = Instantiate(bobberPrefab, rodTip.position, Quaternion.identity);
        lineRenderer.enabled = true;

        Bobber bobberScript = currentBobber.GetComponent<Bobber>();
        if (bobberScript != null)
        {
            bobberScript.myRod = this;
        }

        Rigidbody bobberRb = currentBobber.GetComponent<Rigidbody>();
        if (bobberRb != null)
        {
            Vector3 forceDirection = (Camera.main.transform.forward * currentCharge) + (Vector3.up * upwardForce);
            bobberRb.AddForce(forceDirection, ForceMode.Impulse);
        }

        //Debug.Log($"»ÒàËÂ×èÍÍÍ¡ä»´éÇÂáÃ§: {currentCharge:F1}");
        currentCharge = 0f; // ÃÕà«çµ¤èÒ¾ÅÑ§
    }
    public void CatchSuccess(FishController fish)
    {
        if (currentBobber != null)
        {
            Destroy(currentBobber); // Åº·Øè¹·Ôé§
        }

        currentHookedFish = fish;
        lineRenderer.enabled = true; // à»Ô´àÊé¹àÍç¹äÇé´Ö§»ÅÒ

        Transform pullTarget = transform.parent != null ? transform.parent : transform;

        fish.StartReeling(pullTarget, () => {

            // --- ÊÔè§·Õè¨Ðà¡Ô´¢Öé¹àÁ×èÍ»ÅÒÁÒ¶Ö§µÑÇ ---
            lineRenderer.enabled = false;
            currentHookedFish = null;

            // ´Ö§ Script Inventory ¢Í§¼ÙéàÅè¹ÁÒà¾×èÍà¡çº»ÅÒ
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();

            if (inventory == null)
            {
                Debug.LogError("ºÑê¡: ËÒ PlayerInventory äÁèà¨Í! µÃÇ¨ÊÍºÇèÒµÑÇ¼ÙéàÅè¹ÁÕÊ¤ÃÔ»µì¹ÕéÍÂÙèäËÁ");
                return;
            }

            if (fish.myData.fishItemData == null)
            {
                Debug.LogError($"ºÑê¡: »ÅÒ¡ÓÅÑ§¨Ðà¢éÒ¡ÃÐà»ëÒáÅéÇ áµè¤Ø³Å×ÁãÊè ItemData ãËé¡Ñº {fish.myData.fishName} ã¹Ë¹éÒ Inspector!");
                return;
            }

            //inventory.myItems.Add(fish.myData.fishItemData);
            inventory.AddCaughtFishToHotbar(fish.myData.fishItemData);
            Debug.Log($"+++ à¡çº {fish.myData.fishName} à¢éÒ¡ÃÐà»ëÒÊÓàÃç¨! µÍ¹¹ÕéÁÕ¢Í§·Ñé§ËÁ´ {inventory.myItems.Count} ªÔé¹ +++");

            // TODO: àÃÕÂ¡ GameManager à¾×èÍà¾ÔèÁ EXP/ÍÑ»à´µà¤ÇÊ
            // GameManager.Instance.AddExp(10);
            Destroy(fish.gameObject);
        });
    }

    public void CatchFail()
    {
        if (currentBobber != null)
        {
            Destroy(currentBobber); // ·ÓÅÒÂ·Øè¹·Ôé§
        }

        lineRenderer.enabled = false; // »Ô´ÊÒÂàÍç¹
        currentHookedFish = null;
        Debug.Log("à¡çºÊÒÂàºç´... àµÃÕÂÁµÑÇµ¡ãËÁè");
    }
}
