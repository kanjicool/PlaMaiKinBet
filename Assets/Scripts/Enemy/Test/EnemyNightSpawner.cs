using UnityEngine;

public class EnemyNightSpawner : MonoBehaviour
{
    [Header("References")]
    public LightingManager lightingManager;
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    [Header("Settings")]
    public float spawnInterval = 5f;
    public int maxEnemiesAtNight = 10;
    public string enemyTag = "Enemy";

    private float timer;

    void Update()
    {
        if (lightingManager == null)
        {
            Debug.LogWarning("ลืมลาก LightingManager มาใส่ในหน้า Inspector ครับ!");
            return;
        }

        // 🌟 จุดเช็คที่ 1: ดูว่าระบบมองว่าเป็นกลางคืนหรือยัง
        if (lightingManager.IsNight())
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0;
                TrySpawnEnemy();
            }
        }
        else
        {
            // ถ้าไม่ใช่กลางคืน ให้รีเซ็ต Timer รอไว้
            timer = 0;
        }
    }

    void TrySpawnEnemy()
    {
        // 🌟 จุดเช็คที่ 2: นับจำนวนมอนสเตอร์ในฉาก
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        int currentEnemies = enemies.Length;

        Debug.Log($"[Spawner] ตอนนี้กลางคืนแล้ว! มีมอนสเตอร์ในฉาก: {currentEnemies} ตัว (จำกัดที่ {maxEnemiesAtNight})");

        if (currentEnemies < maxEnemiesAtNight)
        {
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("ลืมใส่ Spawn Points ในลิสต์ครับ!");
                return;
            }

            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (sp != null)
            {
                Instantiate(enemyPrefab, sp.position, sp.rotation);
                Debug.Log("<color=cyan>สำเร็จ!</color> เสกมอนสเตอร์ออกมาแล้ว");
            }
        }
    }
}