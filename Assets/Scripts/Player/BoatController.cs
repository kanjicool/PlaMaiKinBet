using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Engine Settings")]
    public Transform motorPosition; // ตำแหน่งเครื่องยนต์ (ใส่ Empty Object ไว้ท้ายเรือ)
    public float forwardForce = 2500f;
    public float reverseForce = 1000f;
    public float turnTorque = 1500f;
    public float maxSpeed = 15f;

    private Rigidbody rb;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        // อ่านค่า Input (สมมติว่าใช้ WASD จาก Player.Move ตัวเดิม)
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. การเดินหน้า - ถอยหลัง (ประยุกต์แรงดันจากท้ายเรือ)
        if (moveInput.y > 0)
        {
            rb.AddForceAtPosition(transform.forward * forwardForce * moveInput.y * Time.fixedDeltaTime, motorPosition.position);
        }
        else if (moveInput.y < 0)
        {
            rb.AddForceAtPosition(transform.forward * reverseForce * moveInput.y * Time.fixedDeltaTime, motorPosition.position);
        }

        // 2. การเลี้ยว (จะเลี้ยวได้สมูทเมื่อเรือมีความเร็ว หรือกำลังกดเดินหน้า)
        if (Mathf.Abs(moveInput.y) > 0.1f || rb.linearVelocity.magnitude > 2f)
        {
            // ถ้ากำลังถอยหลัง ให้กลับทิศทางการเลี้ยว (เพื่อความสมจริงแบบขับรถ/เรือ)
            float directionMultiplier = moveInput.y < 0 ? -1f : 1f;
            float turnFactor = moveInput.x * turnTorque * directionMultiplier * Time.fixedDeltaTime;

            rb.AddRelativeTorque(0f, turnFactor, 0f);
        }

        // 3. จำกัดความเร็วสูงสุด
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            // คงทิศทางเดิมไว้ แต่ลดความเร็วลงมาที่ maxSpeed
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}