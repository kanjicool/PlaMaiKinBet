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
    public bool isPlayerDriving = false;

    private Rigidbody rb;
    private Vector2 moveInput;
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

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!isPlayerDriving) return;

        Vector3 boatForward = transform.forward; // หัวเรือชี้ +X
        boatForward.y = 0;
        boatForward.Normalize();

        // --- เดินหน้าและถอยหลัง ---
        if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            Vector3 force = boatForward * moveInput.y * acceleration;
            rb.AddForce(force, ForceMode.Force);
        }

        // --- ล็อกความเร็ว ---
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        // --- เลี้ยวเรือ ---
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            float turnDirection = moveInput.y < 0 ? -1f : 1f;
            rb.AddTorque(Vector3.up * moveInput.x * turnSpeed * turnDirection, ForceMode.Force);
        }
    }

}