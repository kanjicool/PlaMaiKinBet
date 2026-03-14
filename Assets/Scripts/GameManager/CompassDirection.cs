using UnityEngine;

public class CompassDirection : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform targetIsland;
    public RectTransform compassUI;

    private Transform mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (player == null || targetIsland == null || compassUI == null)
        {
            if (compassUI != null) compassUI.gameObject.SetActive(false);
            return;
        }

        if (mainCameraTransform == null)
        {
            if (Camera.main != null) mainCameraTransform = Camera.main.transform;
            else return;
        }

        if (!compassUI.gameObject.activeSelf)
            compassUI.gameObject.SetActive(true);

        // 1. หาระยะทางจากตัวผู้เล่น(หรือเรือ) ไปยังเกาะเป้าหมาย
        Vector3 directionToTarget = targetIsland.position - player.position;
        directionToTarget.y = 0; 
        directionToTarget.Normalize(); 

        // 2. หาหน้ากล้อง (แทนการใช้หน้าผู้เล่น)
        Vector3 cameraForward = mainCameraTransform.forward;
        cameraForward.y = 0; 
        cameraForward.Normalize();

        // 3. คำนวณมุมระหว่าง หน้ากล้อง กับ ทิศทางเกาะ
        float angle = Vector3.SignedAngle(cameraForward, directionToTarget, Vector3.up);

        // 4. หมุน UI เข็มทิศ
        compassUI.localEulerAngles = new Vector3(0, 0, -angle);
    }

    public void SetTarget(Transform newTarget)
    {
        targetIsland = newTarget;
    }
}