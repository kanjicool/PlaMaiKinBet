using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(LineRenderer))]
public class FishingRod : MonoBehaviour
{
    [Header("Rod References")]
    public Transform rodTip;
    public GameObject bobberPrefab;

    private LineRenderer lineRenderer;
    private GameObject currentBobber;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        inputActions = new InputSystem_Actions();

        inputActions.Player.Fire.performed += ctx => HandleCasting();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        if (currentBobber != null) lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (currentBobber != null && lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, rodTip.position);
            lineRenderer.SetPosition(1, currentBobber.transform.position);
        }

    }

    private void HandleCasting()
    {
        if (!gameObject.activeInHierarchy) return;

        if (currentBobber != null)
        {
            Destroy(currentBobber);
            lineRenderer.enabled = false;
        }
        else
        {
            Vector3 spawnPosition = rodTip.position + (Camera.main.transform.forward * 5f);
            currentBobber = Instantiate(bobberPrefab, spawnPosition, Quaternion.identity);
            lineRenderer.enabled = true;
        }
    }

}
