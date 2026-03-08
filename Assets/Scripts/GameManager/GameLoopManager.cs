using UnityEngine;
using UnityEngine.InputSystem;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;

    [Header("References")]
    public Transform player;
    public GameObject islandPrefab;
    public CompassDirection compass;

    [Header("Spawn Settings")]
    public float spawnDistance = 500f;

    [Header("Wave State")]
    public GameObject currentIsland;
    private GameObject targetIsland;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UnlockNextIsland()
    {
        if (targetIsland != null) return; 

        float randomAngle = Random.Range(-45f, 45f);
        Vector3 spawnDirection = Quaternion.Euler(0, randomAngle, 0) * player.forward;
        Vector3 spawnPos = player.position + (spawnDirection.normalized * spawnDistance);
        spawnPos.y = 0;

        targetIsland = Instantiate(islandPrefab, spawnPos, Quaternion.identity);

        if (compass != null) compass.SetTarget(targetIsland.transform);

        Debug.Log("เกาะใหม่เกิดแล้วที่ระยะ " + spawnDistance + " หน่วย! ขับเรือไปเลย");
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            UnlockNextIsland();
        }
    }

    public void OnReachNewIsland(GameObject newIsland)
    {
        Debug.Log("ถึงเกาะใหม่แล้ว! กำลังวาร์ปดึงโลกกลับศูนย์กลาง (Floating Origin)...");

        if (currentIsland != null)
        {
            Destroy(currentIsland);
        }

        Vector3 offset = -newIsland.transform.position;
        offset.y = 0;

        newIsland.transform.position += offset;

        player.position += offset;

        currentIsland = newIsland;
        targetIsland = null;

        if (compass != null) compass.SetTarget(null);

        Debug.Log("วาร์ปโลกกลับศูนย์กลางสำเร็จ! เริ่ม Wave ถัดไป");
    }
}