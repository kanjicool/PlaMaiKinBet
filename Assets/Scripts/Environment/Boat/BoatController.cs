using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BoatController : MonoBehaviour
{
    [Header("Boat Stats")]
    public float acceleration = 30f;
    public float maxSpeed = 15f;
    public float turnSpeed = 15f;

    [Header("State")]
    public bool isPlayerDriving = false;

    [Header("VFX & SFX")]
    public ParticleSystem wakeParticle;

    public float minWakeSize = 0.5f;    // ขนาดคลื่นตอนขับช้าๆ
    public float maxWakeSize = 2.0f;

    public AudioSource engineAudio;     
    public float minPitch = 0.8f;       
    public float maxPitch = 1.5f;      

    private Rigidbody rb;
    private Vector2 moveInput;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new InputSystem_Actions();

        if (engineAudio == null) engineAudio = GetComponent<AudioSource>();
        engineAudio.loop = true;
        engineAudio.Play();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        if (!isPlayerDriving)
        {
            moveInput = Vector2.zero;
            UpdateEffects(0f);
            return;
        }

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        float currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        float speedPercent = currentSpeed / maxSpeed;

        UpdateEffects(speedPercent);
    }

    private void UpdateEffects(float speedPercent)
    {
        // 1. จัดการ SFX (เสียงเครื่องยนต์)
        if (engineAudio != null)
        {
            if (isPlayerDriving && Mathf.Abs(moveInput.y) > 0.1f)
            {
                engineAudio.volume = Mathf.Lerp(engineAudio.volume, 1f, Time.deltaTime * 5f);
                engineAudio.pitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);
            }
            else
            {
                engineAudio.volume = Mathf.Lerp(engineAudio.volume, 0.3f, Time.deltaTime * 2f);
                engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, minPitch, Time.deltaTime * 2f);
            }
        }

        // 2. จัดการ VFX (คลื่นน้ำ)
        if (wakeParticle != null)
        {
            var emission = wakeParticle.emission;
            var main = wakeParticle.main;

            // ปรับตัวเลขจาก 0.1f เป็น 0.05f เพื่อให้คลื่นน้ำทำงานไวขึ้นตั้งแตะคันเร่งนิดเดียว
            if (speedPercent > 0.05f && isPlayerDriving)
            {
                if (!emission.enabled) emission.enabled = true;

                // เราเอา Rate over Time ออกไปแล้ว ให้ Rate over Distance ใน Inspector ทำงานแทน

                // ปรับขนาดคลื่นตามความเร็ว (เพื่อให้คลื่นใหญ่ขึ้นตอนขับเร็ว)
                main.startSizeMultiplier = Mathf.Lerp(minWakeSize, maxWakeSize, speedPercent);
            }
            else
            {
                if (emission.enabled) emission.enabled = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isPlayerDriving) return;

        Vector3 boatForward = transform.forward; 
        boatForward.y = 0;
        boatForward.Normalize();

        if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            Vector3 force = boatForward * moveInput.y * acceleration;
            rb.AddForce(force, ForceMode.Force);
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            float turnDirection = moveInput.y < 0 ? -1f : 1f;
            rb.AddTorque(Vector3.up * moveInput.x * turnSpeed * turnDirection, ForceMode.Force);
        }
    }

}