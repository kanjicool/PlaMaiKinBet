using UnityEngine;
using UnityEngine.InputSystem; // สำคัญ! ต้อง import

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float lookSensitivity = 1f;
    public Transform cameraTarget; // จุดที่กล้องจะหมุนตาม (เช่น หัวไหล่ หรือ หัว)

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.1f;

    // ตัวแปรสำหรับเก็บ Input Class ที่ Generate มา
    private InputSystem_Actions inputActions;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;

    // --- 1. Setup Input System ---
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // สร้าง Instance ของ Input Class
        inputActions = new InputSystem_Actions();

        // Setup การกระโดด (ใช้แบบ Event/Callback ดีกว่า Check ใน Update)
        // Player คือชื่อ Action Map, Jump คือชื่อ Action
        inputActions.Player.Jump.performed += context => Jump();
    }

    private void OnEnable()
    {
        // ต้อง Enable ก่อนถึงจะรับค่าได้
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    // --- 2. Main Loop ---
    private void Update()
    {
        // อ่านค่า Input แบบ Realtime (Polling)
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // หมุนตัวละคร/กล้อง (Look)
        HandleRotation();
    }

    private void FixedUpdate()
    {
        // คำนวณ Physics การเดิน (Move)
        HandleMovement();

        // เช็คพื้น
        CheckGrounded();
    }

    // --- 3. Logic Functions ---

    private void HandleMovement()
    {
        // แปลง Input (x, y) ให้เป็นทิศทางของโลก (World Space) ตามมุมกล้อง
        // เพื่อให้กด W แล้วเดินไปข้างหน้าตามที่กล้องมอง
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // ตัดแกน Y ทิ้งเพื่อให้เดินระนาบพื้น
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        // Apply Velocity (รักษาค่า Y เดิมไว้ เพื่อให้แรงโน้มถ่วงทำงาน)
        Vector3 velocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    private void HandleRotation()
    {
        // ส่วนนี้อาจต้องปรับถ้าใช้ Cinemachine (ดูหมายเหตุด้านล่าง)
        // หมุนตัวละครตามทิศที่เดิน (Optional: ถ้าอยากให้หันหน้าไปทางที่เดิน)
        /*
        if (moveInput != Vector2.zero)
        {
            Vector3 direction = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (direction != Vector3.zero) 
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.15f);
        }
        */
    }

    private void Jump()
    {
        if (isGrounded)
        {
            // Reset Velocity แกน Y ก่อนกระโดดเพื่อให้โดดสูงเท่าเดิมเสมอ
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void CheckGrounded()
    {
        // ยิง Raycast ลงพื้น
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
    }
}