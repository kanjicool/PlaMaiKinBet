using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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

    [Header("Toggle Settings")]
    [SerializeField] private InputActionReference toggleRotateAction; 
    private bool isToggleActive = false;

    [Header("UI Elements")]
    [SerializeField] private GameObject crosshairUI;

    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private CinemachineInputAxisController axisController;

    private float targetZoom;
    private float currentZoom;
    public bool IsInRotationMode => axisController != null && axisController.enabled;


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

        SetRotationState(false);
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

        if (toggleRotateAction != null && toggleRotateAction.action.WasPressedThisFrame())
        {
            isToggleActive = !isToggleActive;
        }

        bool shouldRotate = rotateAction.action.IsPressed() || isToggleActive;

        if (axisController.enabled != shouldRotate)
        {
            SetRotationState(shouldRotate);
        }
    }

    void SetRotationState(bool state)
    {
        if (axisController != null) axisController.enabled = state;

        if (state)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (crosshairUI != null) crosshairUI.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (crosshairUI != null) crosshairUI.SetActive(false);
        }
    }

    void HandleZoom()
    {
        if (orbital == null || zoomAction == null) return;

        float scrollInput = zoomAction.action.ReadValue<float>();

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