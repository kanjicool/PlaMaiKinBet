using UnityEngine;
using System.Collections.Generic;

// เอาไว้กำหนดน้ำหนักว่าแต่ละระดับ โอกาสออกเป็นตัวเลขเท่าไหร่
[System.Serializable]
public class RarityWeightRate
{
    public FishRarity rarity;
    public float spawnWeight;
}

public class IslandFishSpawner : MonoBehaviour
{
    [Header("Global Rarity Weights (ค่าน้ำหนักโอกาสออก)")]
    public List<RarityWeightRate> rarityWeights = new List<RarityWeightRate>()
    {
        new RarityWeightRate { rarity = FishRarity.Common, spawnWeight = 100f },
        new RarityWeightRate { rarity = FishRarity.Uncommon, spawnWeight = 50f },
        new RarityWeightRate { rarity = FishRarity.Rare, spawnWeight = 20f },
        new RarityWeightRate { rarity = FishRarity.Epic, spawnWeight = 5f },
        new RarityWeightRate { rarity = FishRarity.Legendary, spawnWeight = 1f }
    };

    private List<FishSpawnPoint> spawnPoints = new List<FishSpawnPoint>();

    private void Awake()
    {
        spawnPoints = new List<FishSpawnPoint>(GetComponentsInChildren<FishSpawnPoint>());
    }

    public List<FishData> GetAvailableFishOnIsland()
    {
        List<FishData> available = new List<FishData>();
        FishSpawnPoint[] points = GetComponentsInChildren<FishSpawnPoint>();

        foreach (FishSpawnPoint point in points)
        {
            if (point.allowedFish == null) continue;
            foreach (FishSpawnEntry entry in point.allowedFish)
            {
                if (entry.fishData != null && !available.Contains(entry.fishData))
                {
                    available.Add(entry.fishData);
                }
            }
        }
        return available;
    }

    public void SpawnEcosystem(FishData questFish, int guaranteedQuestAmount)
    {
        if (spawnPoints.Count == 0) return;

        int questFishSpawned = 0;

        // สลับจุดเกิด
        List<FishSpawnPoint> shuffledPoints = new List<FishSpawnPoint>(spawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            FishSpawnPoint temp = shuffledPoints[i];
            int randomIndex = Random.Range(i, shuffledPoints.Count);
            shuffledPoints[i] = shuffledPoints[randomIndex];
            shuffledPoints[randomIndex] = temp;
        }

        foreach (FishSpawnPoint point in shuffledPoints)
        {
            if (point.allowedFish == null || point.allowedFish.Length == 0) continue;

            if (Random.Range(0f, 100f) <= point.spawnChance)
            {
                int amountToSpawn = point.GetCalculatedSpawnAmount();

                for (int i = 0; i < amountToSpawn; i++)
                {
                    FishData fishToSpawn = null;
                    bool canSpawnQuestFish = System.Array.Exists(point.allowedFish, e => e.fishData == questFish);

                    if (canSpawnQuestFish && questFishSpawned < guaranteedQuestAmount)
                    {
                        fishToSpawn = questFish;
                        questFishSpawned++;
                    }
                    else
                    {
                        fishToSpawn = GetRandomFishByRarity(point);
                    }

                    SpawnFish(point, fishToSpawn);
                }
            }
        }

        // Safety net โควต้าเควสต์
        int safetyNet = 0;
        while (questFishSpawned < guaranteedQuestAmount && safetyNet < 50)
        {
            FishSpawnPoint randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            bool canSpawn = System.Array.Exists(randomPoint.allowedFish, e => e.fishData == questFish);

            if (canSpawn)
            {
                SpawnFish(randomPoint, questFish);
                questFishSpawned++;
            }
            safetyNet++;
        }
    }

    private FishData GetRandomFishByRarity(FishSpawnPoint point)
    {
        float totalWeight = 0f;
        List<FishData> validFishes = new List<FishData>();
        List<float> validWeights = new List<float>();

        foreach (FishSpawnEntry entry in point.allowedFish)
        {
            if (entry.fishData == null) continue;

            // ค้นหาน้ำหนักจากเรทที่เราตั้งไว้
            float weight = 10f;
            foreach (var rate in rarityWeights)
            {
                if (rate.rarity == entry.rarity)
                {
                    weight = rate.spawnWeight;
                    break;
                }
            }

            validFishes.Add(entry.fishData);
            validWeights.Add(weight);
            totalWeight += weight;
        }

        if (validFishes.Count == 0) return null;

        float randomRoll = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < validFishes.Count; i++)
        {
            currentWeight += validWeights[i];
            if (randomRoll <= currentWeight)
            {
                return validFishes[i];
            }
        }

        return validFishes[0];
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

        FishController controller = spawnedFish.GetComponent<FishController>();
        if (controller != null) controller.Initialize(fishData);

        spawnedFish.transform.SetParent(this.transform);
    }
}