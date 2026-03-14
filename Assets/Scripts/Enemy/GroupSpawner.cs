using UnityEngine;
using System.Collections.Generic;

public class GroupSpawner : MonoBehaviour
{
    public GameObject enemyPrefabs;

    [Header("Spawn Limits")]
    public int maxActiveEnemies = 10; // เปลี่ยนชื่อให้ชัดเจน: จำนวนที่อนุญาตให้อยู่บนฉากพร้อมกัน
    public int totalMaxSpawns = 20;   // ใหม่: จำนวนศัตรูสูงสุดที่จะเกิดได้ "ทั้งหมด" จากจุดนี้
    public float spawnInterval = 3f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private float timer;

    // ใหม่: ตัวแปรคอยจดจำว่าเสกศัตรูออกไปกี่ตัวแล้ว
    private int totalSpawnedCount = 0;

    void Update()
    {
        // 1. ถ้าเราเสกศัตรูออกไปครบโควต้าทั้งหมดที่ตั้งไว้แล้ว (เช่น ครบ 20 ตัว) 
        // ให้หยุดการทำงานของ Update() ทันที Spawner ตัวนี้จะไม่สร้างใครอีกต่อไป
        if (totalSpawnedCount >= totalMaxSpawns)
        {
            return;
        }

        // เคลียร์รายชื่อศัตรูที่ตายแล้วออกจากลิสต์ (เพื่อคืนพื้นที่)
        activeEnemies.RemoveAll(item => item == null);

        timer += Time.deltaTime;

        // 2. เช็คเวลา และเช็คว่าจำนวนศัตรูบนฉากตอนนี้ยังไม่ล้น (น้อยกว่า 10)
        if (timer >= spawnInterval && activeEnemies.Count < maxActiveEnemies)
        {
            SpawnAtRandomPoint();
            timer = 0;
        }
    }

    void SpawnAtRandomPoint()
    {
        int childCount = transform.childCount;

        if (childCount == 0)
        {
            Debug.LogWarning("No spawn point in Manager");
            return;
        }

        int randomIndex = Random.Range(0, childCount);
        Transform selectedPoint = transform.GetChild(randomIndex);
        GameObject newEnemy = Instantiate(enemyPrefabs, selectedPoint.position, selectedPoint.rotation);

        activeEnemies.Add(newEnemy);

        // 3. ทุกครั้งที่เสกตัวใหม่สำเร็จ ให้บวกตัวนับจำนวนรวมขึ้น 1
        totalSpawnedCount++;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform spawnPoint = transform.GetChild(i);
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
        }
    }
}