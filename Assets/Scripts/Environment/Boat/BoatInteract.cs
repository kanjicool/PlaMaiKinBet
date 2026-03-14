using UnityEngine;
using UnityEngine.InputSystem;

public class BoatInteract : MonoBehaviour
{
    [Header("References")]
    public BoatController boatController;
    public Transform seatPosition;
    public Transform exitPosition;

    [Header("Respawn Settings")]
    public Transform boatSpawnPoint;

    [Header("Camera Settings")]
    public ThirdPersonCameraController cameraController;
    public Transform boatCamTarget; 
    public Transform playerCamTarget;

    [Header("UI Settings")]
    public GameObject interactUI;

    private bool isPlayerNear = false;
    private bool isPlayerDriving = false;

    private GameObject playerObject;
    private PlayerController playerController;
    private Rigidbody playerRb;
    private Collider playerCollider;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        if (boatController == null) boatController = GetComponent<BoatController>();

        inputActions = new InputSystem_Actions();

        inputActions.Player.Interact.performed += context => TryInteract();
    }

    private void Start()
    {
        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void TryInteract()
    {
        if (isPlayerDriving)
        {
            ExitBoat();
        }
        else if (isPlayerNear && playerObject != null)
        {
            EnterBoat();
        }
    }

    private void LateUpdate()
    {
        if (isPlayerDriving && playerObject != null)
        {
            playerObject.transform.position = seatPosition.position;
            playerObject.transform.rotation = seatPosition.rotation;
        }
    }

    private void EnterBoat()
    {
        isPlayerDriving = true;
        boatController.isPlayerDriving = true;

        playerController.enabled = false;
        playerRb.isKinematic = true;
        playerCollider.enabled = false;

        if (cameraController != null && boatCamTarget != null)
        {
            cameraController.SetDrivingMode(true, boatCamTarget);
        }

        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }

        playerController.SetDrivingState(true);

    }

    private void ExitBoat()
    {
        isPlayerDriving = false;
        boatController.isPlayerDriving = false;


        playerObject.transform.position = exitPosition.position;
        playerObject.transform.rotation = exitPosition.rotation;

        playerController.enabled = true;
        playerRb.isKinematic = false;
        playerCollider.enabled = true;

        playerRb.linearVelocity = Vector3.zero;

        if (cameraController != null && playerCamTarget != null)
        {
            cameraController.SetDrivingMode(false, playerCamTarget);
        }

        if (interactUI != null && isPlayerNear)
        {
            interactUI.SetActive(true);
        }

        playerController.SetDrivingState(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerObject = other.gameObject;

            playerController = playerObject.GetComponent<PlayerController>();
            playerRb = playerObject.GetComponent<Rigidbody>();
            playerCollider = playerObject.GetComponent<Collider>();

            if (interactUI != null && !isPlayerDriving)
            {
                interactUI.SetActive(true);
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerDriving)
        {
            isPlayerNear = false;
            playerObject = null;
            playerController = null;
            playerRb = null;
            playerCollider = null;

            if (interactUI != null)
            {
                interactUI.SetActive(false);
            }

        }
    }

    public void ForceExitBoat()
    {
        if (!isPlayerDriving) return;

        isPlayerDriving = false;
        boatController.isPlayerDriving = false;

        if (playerController != null) playerController.enabled = true;
        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.linearVelocity = Vector3.zero;
        }
        if (playerCollider != null) playerCollider.enabled = true;

        if (cameraController != null && playerCamTarget != null)
        {
            cameraController.SetDrivingMode(false, playerCamTarget);
        }

        isPlayerNear = false;
        if (interactUI != null) interactUI.SetActive(false);
    }

    public void RespawnBoat()
    {
        if (boatSpawnPoint != null)
        {
            Transform boatTransform = boatController.transform;

            Rigidbody rb = boatController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            boatTransform.position = boatSpawnPoint.position;
            boatTransform.rotation = boatSpawnPoint.rotation;
        }
    }

    public void TeleportBoatTo(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            Transform boatTransform = boatController.transform;

            Rigidbody rb = boatController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            boatTransform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);

            Debug.Log("<color=cyan>วาร์ปเรือมาที่แท่นเรียกสำเร็จ!</color>");
        }
    }
}