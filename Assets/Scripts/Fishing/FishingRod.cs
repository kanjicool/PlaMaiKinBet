using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(LineRenderer))]
public class FishingRod : MonoBehaviour
{
    [Header("Rod References")]
    public Transform rodTip;
    public GameObject bobberPrefab;

    [Header("Casting Settings (ระบบชาร์จพลัง)")]
    public float maxCastForce = 25f;  // แรงปาสูงสุด
    public float chargeSpeed = 20f;   // ความเร็วในการชาร์จเกจ
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

        inputActions.Player.Fire.started += ctx => StartCasting();
        inputActions.Player.Fire.canceled += ctx => ReleaseCast();
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
            //Debug.Log($"กำลังชาร์จพลัง... {currentCharge:F1}");
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

        //Debug.Log($"ปาเหยื่อออกไปด้วยแรง: {currentCharge:F1}");
        currentCharge = 0f; // รีเซ็ตค่าพลัง
    }
    public void CatchSuccess(FishController fish)
    {
        if (currentBobber != null)
        {
            Destroy(currentBobber); // ลบทุ่นทิ้ง
        }

        currentHookedFish = fish;
        lineRenderer.enabled = true; // เปิดเส้นเอ็นไว้ดึงปลา

        Transform pullTarget = transform.parent != null ? transform.parent : transform;

        fish.StartReeling(pullTarget, () => {

            // --- สิ่งที่จะเกิดขึ้นเมื่อปลามาถึงตัว ---
            lineRenderer.enabled = false;
            currentHookedFish = null;

            // ดึง Script Inventory ของผู้เล่นมาเพื่อเก็บปลา
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();

            if (inventory == null)
            {
                Debug.LogError("บั๊ก: หา PlayerInventory ไม่เจอ! ตรวจสอบว่าตัวผู้เล่นมีสคริปต์นี้อยู่ไหม");
                return;
            }

            if (fish.myData.fishItemData == null)
            {
                Debug.LogError($"บั๊ก: ปลากำลังจะเข้ากระเป๋าแล้ว แต่คุณลืมใส่ ItemData ให้กับ {fish.myData.fishName} ในหน้า Inspector!");
                return;
            }

            inventory.myItems.Add(fish.myData.fishItemData);
            Debug.Log($"+++ เก็บ {fish.myData.fishName} เข้ากระเป๋าสำเร็จ! ตอนนี้มีของทั้งหมด {inventory.myItems.Count} ชิ้น +++");

            // TODO: เรียก GameManager เพื่อเพิ่ม EXP/อัปเดตเควส
            // GameManager.Instance.AddExp(10);
        });
    }

    public void CatchFail()
    {
        if (currentBobber != null)
        {
            Destroy(currentBobber); // ทำลายทุ่นทิ้ง
        }

        lineRenderer.enabled = false; // ปิดสายเอ็น
        currentHookedFish = null;
        Debug.Log("เก็บสายเบ็ด... เตรียมตัวตกใหม่");
    }
}
