using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float swimSpeed = 3f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    public float jumpCooldown = 0.25f;
    private float lastJumpTime;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 15f;
    public float staminaRegenRate = 10f;
    public float staminaRecoveryThreshold = 25f;
    public Slider staminaSlider;

    private float currentStamina;
    private bool isSprintingInput;
    private bool isSprinting;
    private bool isExhausted;

    [Header("Water System")]
    public float waterDrag = 2f;
    public float swimUpKey = 1f; // แรงว่ายขึ้น (Space)
    public float swimDownKey = 1f; // แรงว่ายลง (Ctrl)
    public float surfaceOffset = 1f;

    public float swimStartDepth = 1.2f; // ระดับความลึกที่จะเปลี่ยนเป็นท่าว่ายน้ำ
    public float swimStopDepth = 0.3f;  // ระดับความตื้นที่จะเปลี่ยนกลับเป็นท่ายืน/เดิน (ต้องน้อยกว่า surfaceOffset)

    private bool isInWater;
    private int waterOverlapCount = 0;
    private float waterSurfaceY;
    private float originalDrag;
    private bool isSwimming;

    [Header("Shore Assist System")]
    private float currentSurfaceOffset;
    private bool isTouchingGround;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.1f;

    [Header("ScreenEffectManager")]
    public ScreenEffectManager effectManager;

    [Header("Climbing System")]
    public float climbSpeed = 3f;
    public float climbCheckDistance = 0.6f;
    public LayerMask climbableLayer;

    public Vector3 climbRayOffset = new Vector3(0, 1f, 0);

    [Header("Climbing Visual Correction")]
    public Transform visualModel; // ลาก GameObject ที่เป็นตัวโมเดล (Mesh) มาใส่
    public Vector3 visualRotationOffset = new Vector3(0, 180f, 0); // ชดเชยองศาหันหน้า
    public Vector3 visualPositionOffset = new Vector3(0, 0, 0);    // ชดเชยตำแหน่งเยื้องศูนย์

    private Quaternion originalVisualLocalRot;
    private Vector3 originalVisualLocalPos;

    private bool isClimbing;
    private RaycastHit climbHit;

    private InputSystem_Actions inputActions;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;

    private bool isMoving;
    private bool isJumping;

    private Animator animator;
    private ThirdPersonCameraController cameraCtrl;
    private float currentMoveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.Jump.performed += context => Jump();
        inputActions.Player.Sprint.performed += ctx => isSprintingInput = true;
        inputActions.Player.Sprint.canceled += ctx => isSprintingInput = false;

        originalDrag = rb.linearDamping;
        currentStamina = maxStamina;
        currentMoveSpeed = walkSpeed;
        currentSurfaceOffset = surfaceOffset;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }

        if (visualModel != null)
        {
            originalVisualLocalRot = visualModel.localRotation;
            originalVisualLocalPos = visualModel.localPosition;
        }

    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Start()
    {
        cameraCtrl = FindFirstObjectByType<ThirdPersonCameraController>();
    }

    private void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        HandleStamina();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        CheckWaterDepth(); // เรียกใช้เช็กความลึกของน้ำตลอดเวลา
        CheckClimbing();

        if (isSwimming)
        {
            float targetOffset = isTouchingGround ? 0f : surfaceOffset;
            currentSurfaceOffset = Mathf.Lerp(currentSurfaceOffset, targetOffset, Time.fixedDeltaTime * 5f);
        }

        if (isClimbing)
        {
            HandleClimbMovement();
        }
        else
        {
            HandleMovement();
        }

        if (isGrounded && isJumping && rb.linearVelocity.y <= 0.1f)
        {
            isJumping = false;
        }
    }

    // ===================== STAMINA =====================
    private void HandleStamina()
    {
        if (currentStamina <= 0f)
        {
            isExhausted = true;
            if (isClimbing) StopClimbing();
        }
        else if (currentStamina >= staminaRecoveryThreshold)
        {
            isExhausted = false;
        }

        isSprinting = isSprintingInput && moveInput.magnitude > 0.1f && !isSwimming && !isClimbing && !isExhausted;

        bool isMovingOnWall = isClimbing && moveInput.magnitude > 0.1f;

        if (isSprinting || isMovingOnWall)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentMoveSpeed = isClimbing ? climbSpeed : sprintSpeed;
        }
        else
        {
            if (!isClimbing)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
            currentMoveSpeed = isSwimming ? swimSpeed : walkSpeed;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (staminaSlider != null)
            staminaSlider.value = currentStamina;

        animator.SetBool("sprint", isSprinting);
    }

    // ===================== MOVEMENT =====================
    private void HandleMovement()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        if (isSwimming)
        {
            HandleSwimMovement(cameraForward, cameraRight);
            animator.SetBool("run", false); // ปิดการวิ่งเมื่ออยู่ในน้ำ
            return;
        }

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
        isMoving = moveDirection.magnitude >= 0.1f;

        if (cameraCtrl != null && cameraCtrl.IsAiming)
        {
            Quaternion targetRotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            rb.linearVelocity = isMoving
                ? new Vector3(moveDirection.x * currentMoveSpeed, rb.linearVelocity.y, moveDirection.z * currentMoveSpeed)
                : new Vector3(0, rb.linearVelocity.y, 0);
        }
        else
        {
            if (isMoving)
            {
                rb.linearVelocity = new Vector3(moveDirection.x * currentMoveSpeed, rb.linearVelocity.y, moveDirection.z * currentMoveSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.fixedDeltaTime);
            }
            else
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }

        animator.SetBool("run", isMoving);
    }

    private void HandleSwimMovement(Vector3 camForward, Vector3 camRight)
    {
        Vector3 horizontalDir = (camForward * moveInput.y + camRight * moveInput.x);
        horizontalDir.y = 0;

        float verticalInput = 0f;
        if (inputActions.Player.Jump.IsPressed()) verticalInput += 1f;
        if (inputActions.Player.Crouch.IsPressed()) verticalInput -= 1f;

        Vector3 swimDir = horizontalDir.normalized * swimSpeed + Vector3.up * (verticalInput * swimSpeed);

        if (transform.position.y + currentSurfaceOffset >= waterSurfaceY && (swimDir.y > 0 || isTouchingGround))
        {
            swimDir.y = 0;

            Vector3 targetPosition = transform.position;
            targetPosition.y = waterSurfaceY - currentSurfaceOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * 10f);
        }

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, swimDir, Time.fixedDeltaTime * 5f);

        isMoving = swimDir.magnitude > 0.1f;

        if (horizontalDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(horizontalDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // ===================== JUMP =====================
    private void Jump()
    {
        if (isSwimming || isClimbing) return;

        if (Time.time < lastJumpTime + jumpCooldown) return;

        if (isGrounded)
        {
            lastJumpTime = Time.time;
            
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            animator.SetTrigger("jump");
            isJumping = true;
        }
    }

    // ===================== GROUND CHECK =====================
    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
        //animator.SetBool("isGrounded", isGrounded);
    }

    // ===================== GROUND COLLISION =====================
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isTouchingGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isTouchingGround = false;
        }
    }

    // ===================== WATER TRIGGER =====================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            waterOverlapCount++;
            isInWater = true; 
            waterSurfaceY = other.bounds.max.y;

            if (effectManager != null)
            {
                effectManager.SetWaterState(true, waterSurfaceY);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            waterOverlapCount--;

            if (waterOverlapCount <= 0)
            {
                waterOverlapCount = 0;
                isInWater = false;

                if (isSwimming)
                {
                    StopSwimming();
                }

                if (effectManager != null)
                    effectManager.SetWaterState(false);
            }


        }
    }

    // ===================== DEPTH CHECK =====================
    private void CheckWaterDepth()
    {
        if (isInWater)
        {
            float currentDepth = waterSurfaceY - transform.position.y;

            if (currentDepth >= swimStartDepth && !isSwimming && !isClimbing)
            {
                StartSwimming();
            }
            else if (currentDepth < swimStopDepth && isSwimming)
            {
                StopSwimming();
            }
        }
    }

    private void StartSwimming()
    {
        isSwimming = true;
        rb.linearDamping = waterDrag;
        animator.SetBool("swim", true);
    }

    private void StopSwimming()
    {
        isSwimming = false;
        rb.linearDamping = originalDrag;
        animator.SetBool("swim", false);
    }

    private void CheckClimbing()
    {
        if (isExhausted)
        {
            if (isClimbing) StopClimbing();
            return;
        }

        Vector3 rayOrigin = transform.position + climbRayOffset;
        bool hitWall = Physics.Raycast(rayOrigin, transform.forward, out climbHit, climbCheckDistance, climbableLayer);

        if (hitWall && moveInput.y > 0.1f)
        {
            if (!isClimbing) StartClimbing();
        }
        else if (!hitWall || (isClimbing && isGrounded && moveInput.y < -0.1f))
        {
            if (isClimbing) StopClimbing();
        }
    }

    private void StartClimbing()
    {
        if (isSwimming)
        {
            StopSwimming();
        }

        isClimbing = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;

        transform.rotation = Quaternion.LookRotation(-climbHit.normal);

        if (visualModel != null)
        {
            visualModel.localRotation = originalVisualLocalRot * Quaternion.Euler(visualRotationOffset);
            visualModel.localPosition = originalVisualLocalPos + visualPositionOffset;
        }

        //animator.SetBool("climb", true);
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.useGravity = true;
        //animator.SetBool("climb", false);

        if (visualModel != null)
        {
            visualModel.localRotation = originalVisualLocalRot;
            visualModel.localPosition = originalVisualLocalPos;
        }
    }

    private void HandleClimbMovement()
    {
        Vector3 climbDirection = (transform.up * moveInput.y + transform.right * moveInput.x).normalized;

        rb.linearVelocity = climbDirection * climbSpeed;

        isMoving = climbDirection.magnitude >= 0.1f;
        //animator.SetBool("climbMove", isMoving); 
    }

    // ===================== DEBUG GIZMOS =====================
    private void OnDrawGizmos()
    {
        Vector3 rayOrigin = transform.position + climbRayOffset;

        Gizmos.color = Application.isPlaying && isClimbing ? Color.green : Color.red;
        Gizmos.DrawRay(rayOrigin, transform.forward * climbCheckDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(rayOrigin + transform.forward * climbCheckDistance, 0.05f);
    }
}