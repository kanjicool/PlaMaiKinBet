using UnityEngine;
using UnityEngine.AI; // เพิ่มเข้ามาเพื่อใช้ NavMesh
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnData
{
    public string enemyName;
    public GameObject prefab;
    [Range(1, 100)] public int spawnWeight = 10;
    public bool nightOnly = false;
}

public class MultiTypePointSpawner : MonoBehaviour
{
    [Header("Optimization")]
    public float activationRange = 50f; // ระยะที่ Player ต้องเข้าใกล้ถึงจะเริ่มทำงาน
    public float distanceCheckInterval = 1f;
    private Transform playerTransform;
    private bool isPlayerNearby = false;
    private float nextDistanceCheckTime;

    [Header("References")]
    public LightingManager lightingManager;
    public List<EnemySpawnData> enemyPool = new List<EnemySpawnData>();
    public Transform[] manualSpawnPoints;

    [Header("Jitter Settings")]
    public float spawnJitter = 1.5f; // รัศมีการสุ่มขยับจุดเกิด

    [Header("Local Limits")]
    public int maxEnemies = 5;
    public float spawnInterval = 5f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private float spawnTimer;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (lightingManager == null || enemyPool.Count == 0 || manualSpawnPoints.Length == 0 || playerTransform == null) return;

        // 1. ระบบประหยัด CPU: เช็คระยะห่างผู้เล่นเป็นช่วงๆ
        if (Time.time >= nextDistanceCheckTime)
        {
            isPlayerNearby = Vector3.Distance(transform.position, playerTransform.position) <= activationRange;
            nextDistanceCheckTime = Time.time + distanceCheckInterval;
        }

        if (!isPlayerNearby) return; // ถ้าอยู่ไกลเกินไป ไม่ต้องทำอะไรต่อ

        // 2. ระบบนับถอยหลังการเกิด
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0;
            activeEnemies.RemoveAll(e => e == null);

            if (activeEnemies.Count < maxEnemies)
            {
                SpawnRandomEnemy();
            }
        }
    }

    void SpawnRandomEnemy()
    {
        bool isNight = lightingManager.IsNight();
        List<EnemySpawnData> eligibleEnemies = new List<EnemySpawnData>();

        // กรองมอนสเตอร์ตามเวลา
        foreach (var enemy in enemyPool)
        {
            if (isNight || !enemy.nightOnly)
            {
                eligibleEnemies.Add(enemy);
            }
        }

        if (eligibleEnemies.Count == 0) return;

        // สุ่มเลือกมอนสเตอร์ (Weight) และจุดเกิด
        EnemySpawnData selectedData = GetWeightedRandomEnemyData(eligibleEnemies);
        Transform selectedPoint = manualSpawnPoints[Random.Range(0, manualSpawnPoints.Length)];

        if (selectedPoint != null && selectedData != null)
        {
            // คำนวณตำแหน่ง Jitter (สุ่มรอบจุด)
            Vector3 targetPos = selectedPoint.position + (Random.insideUnitSphere * spawnJitter);
            targetPos.y = selectedPoint.position.y;

            // ตรวจสอบ NavMesh เพื่อไม่ให้เกิดในโขดหิน
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, spawnJitter + 1f, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }

            // เสกมอนสเตอร์ (รอบเดียว)
            GameObject newEnemy = Instantiate(selectedData.prefab, targetPos, selectedPoint.rotation);
            activeEnemies.Add(newEnemy);

            // ส่งค่า dieAtDawn ตามที่ตั้งค่าไว้ใน Pool
            EnemyController controller = newEnemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.dieAtDawn = selectedData.nightOnly;
            }
        }
    }

    // แก้ไขให้คืนค่าเป็นข้อมูลตัวแปรทั้งหมดเพื่อเอาไปใช้ต่อได้
    EnemySpawnData GetWeightedRandomEnemyData(List<EnemySpawnData> candidates)
    {
        int totalWeight = 0;
        foreach (var enemy in candidates) totalWeight += enemy.spawnWeight;

        int randomValue = Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        foreach (var enemy in candidates)
        {
            currentWeightSum += enemy.spawnWeight;
            if (randomValue < currentWeightSum) return enemy;
        }
        return null;
    }

    // รวม OnDrawGizmos เป็นอันเดียว
    private void OnDrawGizmos()
    {
        // 1. วาดระยะ Activation (สีเหลือง)
        Gizmos.color = Color.yellow;
        DrawCircle(transform.position, activationRange, 50);

        // 2. วาดจุดเกิดและ Jitter (สีชมพู)
        Gizmos.color = new Color(1, 0, 1, 0.5f);
        foreach (Transform p in manualSpawnPoints)
        {
            if (p != null)
            {
                Gizmos.DrawSphere(p.position, 0.3f);
                DrawCircle(p.position, spawnJitter, 20);
                Gizmos.DrawLine(p.position, p.position + Vector3.up * 2f);

                // เส้นเชื่อมไปยัง Spawner หลัก
                Gizmos.color = new Color(1, 1, 1, 0.1f);
                Gizmos.DrawLine(transform.position, p.position);
                Gizmos.color = new Color(1, 0, 1, 0.5f);
            }
        }
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        if (radius <= 0) return;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}