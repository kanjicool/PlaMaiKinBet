using System.Diagnostics;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Camera Target")]
    [SerializeField] private Transform cameraTarget;

    [Header("Offset Settings")]
    [SerializeField] private Vector3 normalOffset = new Vector3(0, 1.5f, 0); // ตำแหน่งปกติ
    [SerializeField] private Vector3 aimOffset = new Vector3(1f, 1.5f, 0);   // ตำแหน่งตอนเล็ง (เยื้องขวา)
    [SerializeField] private float offsetLerpSpeed = 10f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    [Header("Input Settings")]
    [Tooltip("Right Click")]
    [SerializeField] private InputActionReference rotateAction;
    [SerializeField] private InputActionReference zoomAction;
    [Tooltip("Shift Lock")]
    [SerializeField] private InputActionReference toggleAimAction;

    [Header("UI Elements")]
    [SerializeField] private GameObject crosshairUI;

    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private CinemachineInputAxisController axisController;

    private float targetZoom;
    private float currentZoom;
    public bool IsAiming { get; private set; }

    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        Debug.Log($">>> GGGGGGGGGG axisController : {axisController}");

        if (cam != null)
        {
            if (axisController == null)
            {
                axisController = GetComponent<CinemachineInputAxisController>();

            }
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

        if (cameraTarget != null) cameraTarget.localPosition = normalOffset;

        IsAiming = false;
        if (crosshairUI != null) crosshairUI.SetActive(false);
    }

    private void OnEnable()
    {
        rotateAction?.action.Enable();
        zoomAction?.action.Enable();
        toggleAimAction?.action.Enable();
    }

    private void OnDisable()
    {
        rotateAction?.action.Disable();
        zoomAction?.action.Disable();
        toggleAimAction?.action.Disable();
    }

    void Update()
    {
        HandleToggleMode();
        HandleCameraRotationAndCursor();
        HandleCameraOffset();
        HandleZoom();
    }

    void HandleToggleMode()
    {
        if (toggleAimAction != null && toggleAimAction.action.WasPressedThisFrame())
        {
            IsAiming = !IsAiming;
            Debug.Log($">>> IsAiming : {IsAiming}");
            if (crosshairUI != null) crosshairUI.SetActive(IsAiming);
        }
    }

    void HandleCameraRotationAndCursor()
    {
        Debug.Log($">>> axisController BF : {axisController}");

        if (axisController == null) return;

        Debug.Log($">>> axisController AF : {axisController}");

        bool isRightClicking = rotateAction != null && rotateAction.action.IsPressed();        

        bool shouldRotateCamera = IsAiming || isRightClicking;
        axisController.enabled = shouldRotateCamera;

        if (shouldRotateCamera)
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

    void HandleCameraOffset()
    {
        if (cameraTarget == null) return;

        Vector3 targetLocalPos = IsAiming ? aimOffset : normalOffset;
        cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, targetLocalPos, Time.deltaTime * offsetLerpSpeed);
    }

    void HandleZoom()
    {
        if (orbital == null || zoomAction == null) return;
        float scrollInput = zoomAction.action.ReadValue<float>();
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            float direction = scrollInput > 0 ? -1 : 1;
            targetZoom += direction * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minDistance, maxDistance);
        }
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;
    }
}