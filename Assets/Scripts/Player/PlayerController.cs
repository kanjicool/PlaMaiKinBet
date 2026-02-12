using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;
    public Transform cameraTarget; 

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.1f;

    private InputSystem_Actions inputActions;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private bool isRuning;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        inputActions = new InputSystem_Actions();
        inputActions.Player.Jump.performed += context => Jump();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

    }

    private void FixedUpdate()
    {
        HandleMovement();
        CheckGrounded();
    }


    private void HandleMovement()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            Vector3 velocity = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        
            isRuning = true;
                
        }
        else
        {
            isRuning = false;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        
        }

        animator.SetBool("run", isRuning);


    }


    private void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            animator.SetTrigger("jump");
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
    }

}