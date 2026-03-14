using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public float surfaceOffset = 1f;
    public float swimStartDepth = 1.2f;
    public float swimStopDepth = 0.3f;
    private bool isInWater;
    private int waterOverlapCount = 0;
    private float waterSurfaceY;
    private float originalDrag;
    private bool isSwimming;
    private float currentSurfaceOffset;
    private bool isTouchingGround;

    [Header("Ground & Climbing")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.1f;
    public float climbSpeed = 3f;
    public float climbCheckDistance = 0.6f;
    public LayerMask climbableLayer;
    public Vector3 climbRayOffset = new Vector3(0, 1f, 0);
    private bool isClimbing;
    private RaycastHit climbHit;
    private bool isGrounded;

    [Header("Visual Correction")]
    public Transform visualModel;
    public Vector3 visualRotationOffset = new Vector3(0, 180f, 0);
    public Vector3 visualPositionOffset = new Vector3(0, 0, 0);
    private Quaternion originalVisualLocalRot;
    private Vector3 originalVisualLocalPos;

    [Header("Interaction System")]
    public float interactRadius = 1.5f;
    public LayerMask interactableLayer;
    public GameObject interactUI;
    public TextMeshProUGUI interactText;
    private GameObject currentInteractItem;

    [Header("References")]
    public ScreenEffectManager effectManager;
    public PlayerInventory inventory;
    private PlayerCombat combat;

    private int currentHoldType = -1;
    private InputSystem_Actions inputActions;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isMoving;
    private bool isJumping;
    private Animator animator;
    private ThirdPersonCameraController cameraCtrl;
    private float currentMoveSpeed;

    public bool IsBusy => isSwimming || isClimbing;
    public bool IsSwimming => isSwimming;
    public bool IsDriving { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.Jump.performed += context => Jump();
        inputActions.Player.Sprint.performed += ctx => isSprintingInput = true;
        inputActions.Player.Sprint.canceled += ctx => isSprintingInput = false;

        originalDrag = rb.linearDamping;
        currentStamina = maxStamina;
        currentMoveSpeed = walkSpeed;
        currentSurfaceOffset = surfaceOffset;

        if (staminaSlider != null) staminaSlider.maxValue = maxStamina;

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

        if (inventory == null)
        {
            inventory = FindFirstObjectByType<PlayerInventory>();
        }
    }

    private void Update()
    {
        CheckInteractable();

        if (Keyboard.current.fKey.wasPressedThisFrame && currentInteractItem != null)
        {
            if (inventory != null)
            {
                bool success = inventory.PickupGroundItem(currentInteractItem);
                if (success)
                {
                    currentInteractItem = null;
                    if (interactUI != null) interactUI.SetActive(false);
                }
            }
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (inventory != null && !inventory.isInventoryOpen)
            {
                inventory.DropHeldItem();
                UpdateHoldAnimation(); 
            }
        }

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        HandleStamina();
        UpdateHoldAnimation();

    }

    private void FixedUpdate()
    {
        CheckGrounded();
        CheckWaterDepth();
        CheckClimbing();

        if (isSwimming)
        {
            float targetOffset = isTouchingGround ? 0f : surfaceOffset;
            currentSurfaceOffset = Mathf.Lerp(currentSurfaceOffset, targetOffset, Time.fixedDeltaTime * 5f);
        }

        if (isClimbing) HandleClimbMovement();
        else HandleMovement();

        if (isGrounded && isJumping && rb.linearVelocity.y <= 0.1f) isJumping = false;
    }

    private void CheckInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, interactableLayer);
        GameObject closestItem = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            ItemHolder holder = hit.GetComponent<ItemHolder>();
            if (holder != null && holder.itemData != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestItem = hit.gameObject;
                }
            }
        }

        if (closestItem != null)
        {
            currentInteractItem = closestItem;
            if (interactUI != null && !interactUI.activeSelf) interactUI.SetActive(true);
            if (interactText != null) interactText.text = $"Press [F] Pick up {currentInteractItem.GetComponent<ItemHolder>().itemData.itemName}";
        }
        else
        {
            currentInteractItem = null;
            if (interactUI != null && interactUI.activeSelf) interactUI.SetActive(false);
        }
    }

    // ===================== HoldItems =====================

    private void UpdateHoldAnimation()
    {
        if (inventory != null && animator != null)
        {
            int newHoldType = inventory.GetCurrentHoldAnimID();
            if (newHoldType != currentHoldType)
            {
                currentHoldType = newHoldType;
                animator.SetInteger("holdType", currentHoldType);
            }
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
        // 🌟 ถาม PlayerCombat ว่ากำลังติดแอนิเมชันฟันดาบอยู่ไหม ถ้าใช่ก็ให้หยุดเดิน
        if (combat != null && combat.IsAttacking())
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            animator.SetBool("run", false);
            return;
        }

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        if (isSwimming)
        {
            HandleSwimMovement(cameraForward, cameraRight);
            animator.SetBool("run", false);
            return;
        }

        cameraForward.y = 0; cameraRight.y = 0;
        cameraForward.Normalize(); cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
        isMoving = moveDirection.magnitude >= 0.1f;

        if (cameraCtrl != null && cameraCtrl.IsAiming)
        {
            Quaternion targetRotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.linearVelocity = isMoving ? new Vector3(moveDirection.x * currentMoveSpeed, rb.linearVelocity.y, moveDirection.z * currentMoveSpeed) : new Vector3(0, rb.linearVelocity.y, 0);
        }
        else
        {
            if (isMoving)
            {
                rb.linearVelocity = new Vector3(moveDirection.x * currentMoveSpeed, rb.linearVelocity.y, moveDirection.z * currentMoveSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.fixedDeltaTime);
            }
            else rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
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

        if (inventory != null)
        {
            inventory.ForceUnequip();
        }
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

    // ===================== DRIVING =====================
    public void SetDrivingState(bool driving)
    {
        IsDriving = driving;
        if (animator != null)
        {
            animator.SetBool("isDriving", IsDriving);
        }

        if (driving)
        {
            animator.SetBool("run", false);
            animator.SetBool("swim", false);

            if (inventory != null)
            {
                inventory.ForceUnequip();
            }
        }
    }

    public void UseSlashTransform()
    {
        if (inventory != null)
        {
            inventory.UseSlashTransform(); 
        }
    }

    public ItemData GetCurrentItemData()
    {
        if (inventory != null && inventory.GetHeldItem() != null)
        {
            ItemHolder holder = inventory.GetHeldItem().GetComponent<ItemHolder>();
            if (holder != null) return holder.itemData;
        }
        return null;
    }

    public void ResetToHoldTransform()
    {
        if (inventory != null)
        {
            inventory.ResetToHoldTransform(); 
        }
    }

    // ===================== DEBUG GIZMOS =====================
    //private void OnDrawGizmos()
    //{
    //    Vector3 rayOrigin = transform.position + climbRayOffset;

    //    Gizmos.color = Application.isPlaying && isClimbing ? Color.green : Color.red;
    //    Gizmos.DrawRay(rayOrigin, transform.forward * climbCheckDistance);

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(rayOrigin + transform.forward * climbCheckDistance, 0.05f);

    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawWireSphere(transform.position, interactRadius);
    //}
}