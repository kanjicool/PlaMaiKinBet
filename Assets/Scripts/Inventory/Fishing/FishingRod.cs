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
        if (currentBobber != null && lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, rodTip.position);
            lineRenderer.SetPosition(1, currentBobber.transform.position);
        }

        if (isCharging)
        {
            currentCharge += chargeSpeed * chargeDirection * Time.deltaTime;

            // ถ้าพลังถึงจุดสูงสุด ให้สลับทิศทางให้เกจลดลง
            if (currentCharge >= maxCastForce)
            {
                currentCharge = maxCastForce;
                chargeDirection = -1;
            }
            // ถ้าพลังลดลงมาถึง 0 ให้สลับทิศทางให้เกจเพิ่มขึ้น
            else if (currentCharge <= 0)
            {
                currentCharge = 0;
                chargeDirection = 1;
            }

            // ปริ้นค่าดูใน Console ก่อน (เดี๋ยวเราค่อยเอาไปผูกกับ UI หลอดพลังทีหลัง)
            //Debug.Log($"กำลังชาร์จพลัง... {currentCharge:F1}");
        }
    }

    private void StartCasting()
    {
        if (!gameObject.activeInHierarchy) return;

        // ถ้ามีทุ่นอยู่ในฉากอยู่แล้ว ให้ดึงเบ็ดกลับมาแทนที่จะปาใหม่
        if (currentBobber != null)
        {
            Destroy(currentBobber);
            lineRenderer.enabled = false;
            isCharging = false; // ป้องกันบัคชาร์จค้าง
        }
        else
        {
            // ถ้ายังไม่มีทุ่น ให้เริ่มเข้าสู่สถานะชาร์จพลัง
            isCharging = true;
            currentCharge = 0f;
            chargeDirection = 1;
        }
    }

    private void ReleaseCast()
    {
        if (!isCharging) return;

        isCharging = false;

        // 1. เสกทุ่นออกมาที่ "ปลายไม้เบ็ด" ก่อน
        currentBobber = Instantiate(bobberPrefab, rodTip.position, Quaternion.identity);
        lineRenderer.enabled = true;

        // 2. ดึง Rigidbody ของทุ่นมาเพื่อใส่แรงผลัก
        Rigidbody bobberRb = currentBobber.GetComponent<Rigidbody>();
        if (bobberRb != null)
        {
            // คำนวณทิศทาง: พุ่งไปข้างหน้ากล้อง + งัดขึ้นบนนิดหน่อย
            Vector3 forceDirection = (Camera.main.transform.forward * currentCharge) + (Vector3.up * upwardForce);

            // ใส่แรงผลักแบบ Impulse (ผลักตู้มเดียว)
            bobberRb.AddForce(forceDirection, ForceMode.Impulse);
        }

        //Debug.Log($"ปาเหยื่อออกไปด้วยแรง: {currentCharge:F1}");
        currentCharge = 0f; // รีเซ็ตค่าพลัง
    }
}
