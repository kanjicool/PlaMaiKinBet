using UnityEngine;
using UnityEngine.InputSystem;


public class BoatInteract : MonoBehaviour
{
    [Header("References")]
    public BoatController boatController;
    public Transform seatPosition;
    public Transform exitPosition;

    private bool isPlayerNear = false;
    private bool isPlayerDriving = false;

    private GameObject playerObject;
    private PlayerController playerController;
    private Rigidbody playerRb;
    private Collider playerCollider;

    private InputSystem_Actions inputActions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
