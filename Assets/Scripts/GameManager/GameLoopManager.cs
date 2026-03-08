using UnityEngine;
using UnityEngine.InputSystem;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;

    [Header("References")]
    public Transform player;

    public GameObject[] islandPrefabs;
    public CompassDirection compass;

    [Header("Spawn Settings")]
    public float minSpawnDistance = 400f;
    public float maxSpawnDistance = 800f;

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

        if (islandPrefabs == null || islandPrefabs.Length == 0)
        {
            return;
        }

        int randomIslandIndex = Random.Range(0, islandPrefabs.Length);
        GameObject selectedIslandPrefab = islandPrefabs[randomIslandIndex];

        float currentSpawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

        float randomAngle = Random.Range(-45f, 45f);
        Vector3 spawnDirection = Quaternion.Euler(0, randomAngle, 0) * player.forward;
        
        
        Vector3 spawnPos = player.position + (spawnDirection.normalized * currentSpawnDistance);
        spawnPos.y = 0;

        targetIsland = Instantiate(selectedIslandPrefab, spawnPos, Quaternion.identity);

        if (compass != null) compass.SetTarget(targetIsland.transform);

        Debug.Log($"สุ่มได้เกาะแบบที่ {randomIslandIndex}! เกิดแล้วที่ระยะ {currentSpawnDistance:F0} หน่วย!");
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

        ThirdPersonCameraController camController = FindFirstObjectByType<ThirdPersonCameraController>();
        if (camController != null)
        {
            camController.OnTargetWarped(offset);
        }

        currentIsland = newIsland;
        targetIsland = null;

        if (compass != null) compass.SetTarget(null);

        Debug.Log("วาร์ปโลกกลับศูนย์กลางสำเร็จ! เริ่ม Wave ถัดไป");
    }


}