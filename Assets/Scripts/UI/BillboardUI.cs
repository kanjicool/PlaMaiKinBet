using UnityEngine;

public class BillboardUI : MonoBehaviour
{

    private Camera mainCamera;


    void Start()
    {
        mainCamera = Camera.main;   
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }

    void Update()
    {
        
    }
}
