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

        Rigidbody bobberRb = currentBobber.GetComponent<Rigidbody>();
        if (bobberRb != null)
        {
            Vector3 forceDirection = (Camera.main.transform.forward * currentCharge) + (Vector3.up * upwardForce);

            bobberRb.AddForce(forceDirection, ForceMode.Impulse);
        }

        //Debug.Log($"ปาเหยื่อออกไปด้วยแรง: {currentCharge:F1}");
        currentCharge = 0f; // รีเซ็ตค่าพลัง
    }
}
