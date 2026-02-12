using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference rotateAction;
    [SerializeField] private InputActionReference zoomAction;

    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private CinemachineInputAxisController axisController;

    private float targetZoom;
    private float currentZoom;

    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        axisController = GetComponent<CinemachineInputAxisController>();

        if (cam != null)
        {
            orbital = cam.GetComponent<CinemachineOrbitalFollow>();
        }
    }

    void Start()
    {
        if (orbital != null)
        {
            currentZoom = orbital.Radius;
            targetZoom = currentZoom;
        }


        if (axisController != null) axisController.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnEnable()
    {
        rotateAction?.action.Enable();
        zoomAction?.action.Enable();
    }

    private void OnDisable()
    {
        rotateAction?.action.Disable();
        zoomAction?.action.Disable();
    }

    void Update()
    {
        HandleRotationInput(); 
        HandleZoom();
    }

    void HandleRotationInput()
    {
        if (axisController == null || rotateAction == null) return;

        // เช็คสถานะการกดค้าง (Hold)
        bool isRightClickHeld = rotateAction.action.IsPressed();

        // 1. ควบคุมการหมุนกล้อง (เปิด/ปิด Axis Controller)
        if (axisController.enabled != isRightClickHeld)
        {
            axisController.enabled = isRightClickHeld;

            // 2. (Optional) จัดการ Cursor เมาส์
            // ถ้ากดค้าง -> ล็อกเมาส์และซ่อน (เพื่อให้หมุนได้ต่อเนื่องไม่หลุดจอ)
            // ถ้าปล่อย -> ปล่อยเมาส์และแสดง
            if (isRightClickHeld)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void HandleZoom()
    {
        if (orbital == null || zoomAction == null) return;

        float scrollInput = zoomAction.action.ReadValue<float>();

        // Normalize ค่า Scroll (Input System บางทีส่งค่ามาเยอะ หรือน้อย ขึ้นอยู่กับ Hardware)
        // ใช้ Mathf.Sign หรือ Clamp เพื่อให้ค่าคงที่
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            float direction = scrollInput > 0 ? -1 : 1; // สลับทิศทางตามความถนัด
            targetZoom += direction * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minDistance, maxDistance);
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;
    }
}