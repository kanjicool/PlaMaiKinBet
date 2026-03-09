using UnityEngine;
using System.Collections.Generic;

public class IslandFishSpawner : MonoBehaviour
{
    private List<FishSpawnPoint> spawnPoints = new List<FishSpawnPoint>();

    private void Awake()
    {
        spawnPoints = new List<FishSpawnPoint>(GetComponentsInChildren<FishSpawnPoint>());
    }

    // ฟังก์ชันนี้ให้ GameLoopManager เรียกเพื่อถามว่า "เกาะนี้มีปลาอะไรอยู่บ้าง?"
    public List<FishData> GetAvailableFishOnIsland()
    {
        List<FishData> available = new List<FishData>();

        // ถ้าใช้ Prefab (ยังไม่ Awake) เราต้องดึง Component สดๆ
        FishSpawnPoint[] points = GetComponentsInChildren<FishSpawnPoint>();

        foreach (FishSpawnPoint point in points)
        {
            if (point.allowedFish == null) continue;
            foreach (FishData fish in point.allowedFish)
            {
                if (!available.Contains(fish)) available.Add(fish);
            }
        }
        return available;
    }

    // ฟังก์ชันสำหรับเสกระบบนิเวศปลาทั้งเกาะ
    public void SpawnEcosystem(FishData questFish, int guaranteedQuestAmount)
    {
        if (spawnPoints.Count == 0) return;

        int questFishSpawned = 0;

        // สลับจุดเกิดแบบสุ่ม
        List<FishSpawnPoint> shuffledPoints = new List<FishSpawnPoint>(spawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            FishSpawnPoint temp = shuffledPoints[i];
            int randomIndex = Random.Range(i, shuffledPoints.Count);
            shuffledPoints[i] = shuffledPoints[randomIndex];
            shuffledPoints[randomIndex] = temp;
        }

        // วนเสกปลาตามจุดต่างๆ
        foreach (FishSpawnPoint point in shuffledPoints)
        {
            if (point.allowedFish == null || point.allowedFish.Length == 0) continue;

            if (Random.Range(0f, 100f) <= point.spawnChance)
            {
                FishData fishToSpawn = null;

                // เช็กว่าจุดนี้สามารถเสกปลาเควสต์ได้ไหม
                bool canSpawnQuestFish = System.Array.Exists(point.allowedFish, f => f == questFish);

                // ถ้าจุดนี้เกิดปลาเควสต์ได้ และปลาเควสต์ยังไม่ครบจำนวน ให้ล็อกเป้าเสกปลาเควสต์ก่อนเลย
                if (canSpawnQuestFish && questFishSpawned < guaranteedQuestAmount)
                {
                    fishToSpawn = questFish;
                    questFishSpawned++;
                }
                else
                {
                    // ถ้าโควตาเควสต์ครบแล้ว หรือจุดนี้เสกปลาเควสต์ไม่ได้ ให้สุ่มปลาปกติจากลิสต์ของจุดนั้น
                    fishToSpawn = point.allowedFish[Random.Range(0, point.allowedFish.Length)];
                }

                SpawnFish(point, fishToSpawn);
            }
        }

        // เซฟตี้: ถ้าดวงซวย ปลาเควสต์เกิดไม่ครบตามขั้นต่ำ ให้สุ่มหาจุดที่เสกได้แล้วบังคับเสกเพิ่ม
        int safetyNet = 0;
        while (questFishSpawned < guaranteedQuestAmount && safetyNet < 50)
        {
            FishSpawnPoint randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            bool canSpawn = System.Array.Exists(randomPoint.allowedFish, f => f == questFish);

            if (canSpawn)
            {
                SpawnFish(randomPoint, questFish);
                questFishSpawned++;
            }
            safetyNet++;
        }

        Debug.Log($"[FishSpawner] สร้างระบบนิเวศบนเกาะสำเร็จ มีปลาเควสต์เกิด {questFishSpawned} ตัว");
    }

    private void SpawnFish(FishSpawnPoint point, FishData fishData)
    {
        if (fishData == null || fishData.fishPrefab == null) return;

        Vector2 randomOffset = Random.insideUnitCircle * point.spawnRadius;
        Vector3 finalSpawnPos = new Vector3(
            point.transform.position.x + randomOffset.x,
            point.transform.position.y,
            point.transform.position.z + randomOffset.y
        );

        GameObject spawnedFish = Instantiate(fishData.fishPrefab, finalSpawnPos, Quaternion.identity);

        // อย่าลืมยัด FishData ให้ปลาด้วย!
        FishController controller = spawnedFish.GetComponent<FishController>();
        if (controller != null) controller.Initialize(fishData);

        spawnedFish.transform.SetParent(this.transform);
    }
}