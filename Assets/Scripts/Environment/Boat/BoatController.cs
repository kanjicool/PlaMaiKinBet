using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Boat Stats")]
    public float acceleration = 30f;
    public float maxSpeed = 15f;
    public float turnSpeed = 15f;

    [Header("State")]
    public bool isPlayerDriving = false; // เปิด/ปิดเมื่อผู้เล่นขึ้นเรือ

    private Rigidbody rb;
    private Vector2 moveInput;

    // หากคุณแยก Action Map สำหรับเรือ ให้ดึง InputSystem_Actions มาใช้คล้าย PlayerController
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        if (!isPlayerDriving)
        {
            moveInput = Vector2.zero;
            return;
        }

        // อ่านค่า Input (W/S = moveInput.y, A/D = moveInput.x)
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!isPlayerDriving) return;

        // --- เดินหน้าและถอยหลัง ---
        if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            // ดันไปข้างหน้าตามทิศของเรือ
            Vector3 force = transform.forward * moveInput.y * acceleration;
            rb.AddForce(force, ForceMode.Force);
        }

        // ล็อกความเร็วไม่ให้เกิน maxSpeed
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        // --- การเลี้ยว ---
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            // ทำให้เลี้ยวได้เป็นธรรมชาติมากขึ้น (ถ้าถอยหลัง พวงมาลัยจะสลับทิศเหมือนรถ)
            float turnDirection = moveInput.y < 0 ? -1f : 1f;
            rb.AddTorque(Vector3.up * moveInput.x * turnSpeed * turnDirection, ForceMode.Force);
        }
    }
}