using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public enum BossState { SLEEPING, HUNGRY, ANGRY, RAMPAGING }

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;

    [Header("References")]
    public Transform player;
    public Transform hubIsland;
    public GameObject[] islandPrefabs;
    public CompassDirection compass;

    [Header("Quest System & UI")]
    public FishData currentQuestFish;
    public int currentQuestAmount = 1; // จำนวนปลาที่ต้องการ
    public TextMeshProUGUI bossQuestText; // ข้อความ Canvas ลอยหน้าบอส

    [Header("Wave & Progression")]
    public int currentWave = 1;
    public GameObject currentQuestIsland;
    private int lastIslandIndex = -1;

    [Header("Boss State Machine")]
    public BossState bossState = BossState.SLEEPING;
    public float timeScale = 1f;
    public float daysSinceFed = 0f;
    public float hungryThreshold = 2f;
    public float angryThreshold = 4f;
    public float rampageThreshold = 5f;

    [Header("Boss References")]
    public BossRobotController bossRobot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        UpdateBossStateMachine();
    }

    #region Boss State Machine
    private void UpdateBossStateMachine()
    {
        daysSinceFed += Time.deltaTime * timeScale;

        if (bossState == BossState.SLEEPING && daysSinceFed >= hungryThreshold)
            ChangeBossState(BossState.HUNGRY);
        else if (bossState == BossState.HUNGRY && daysSinceFed >= angryThreshold)
            ChangeBossState(BossState.ANGRY);
        else if (bossState == BossState.ANGRY && daysSinceFed >= rampageThreshold)
            ChangeBossState(BossState.RAMPAGING);
    }

    private void ChangeBossState(BossState newState)
    {
        bossState = newState;
        switch (newState)
        {
            case BossState.SLEEPING:
                daysSinceFed = 0f;
                UpdateBossUI("Zzz...");
                break;
            case BossState.HUNGRY:
                UpdateBossUI($"HUNGRY! >>> {currentQuestFish?.fishName} : {currentQuestAmount}");
                break;
            case BossState.ANGRY:
                UpdateBossUI($"<color=red>ANGRY!</color>\n {currentQuestFish?.fishName} {currentQuestAmount}");
                break;
            case BossState.RAMPAGING:
                UpdateBossUI("<color=red>ERROR! TARGET LOCKED!</color>");

                if (bossRobot != null && player != null)
                {
                    bossRobot.StartRampage(player);
                }

                break;
        }
    }

    public void UpdateBossUI(string message)
    {
        if (bossQuestText != null)
        {
            bossQuestText.text = message;
        }
    }

    public void ResetBossState()
    {
        ChangeBossState(BossState.SLEEPING);
    }

    public void TryFeedBoss()
    {
        if (currentQuestFish == null) return;

        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory == null) return;

        int currentAmount = inventory.GetItemCount(currentQuestFish.fishItemData);

        if (currentAmount >= currentQuestAmount)
        {
            Debug.Log($"ให้อาหารบอสสำเร็จ!");

            inventory.ConsumeItems(currentQuestFish.fishItemData, currentQuestAmount);

            ChangeBossState(BossState.SLEEPING);

            if (currentQuestIsland != null) Destroy(currentQuestIsland);

            currentWave++;
            Invoke("StartNextWave", 3f); // หน่วงเวลา 3 วินาทีก่อนเริ่ม Wave ใหม่ ให้ผู้เล่นได้พักหายใจ
        }
        else
        {
            Debug.Log($"ของไม่พอ! ตอนนี้มี {currentAmount}/{currentQuestAmount} ตัว");
            UpdateBossUI($"ยังไม่พอ!\nต้องการ: {currentQuestFish.fishName}\nขาดอีก: {currentQuestAmount - currentAmount} ตัว");
        }
    }
    #endregion


    #region Wave & Radial Spawning
    public void StartNextWave()
    {
        if (islandPrefabs == null || islandPrefabs.Length == 0) return;

        int randomIslandIndex = Random.Range(0, islandPrefabs.Length);
        if (islandPrefabs.Length > 1 && randomIslandIndex == lastIslandIndex)
            randomIslandIndex = (randomIslandIndex + Random.Range(1, islandPrefabs.Length)) % islandPrefabs.Length;
        lastIslandIndex = randomIslandIndex;

        GameObject selectedIslandPrefab = islandPrefabs[randomIslandIndex];

        IslandFishSpawner spawnerPrefab = selectedIslandPrefab.GetComponent<IslandFishSpawner>();
        System.Collections.Generic.List<FishData> availableFishOnIsland = spawnerPrefab.GetAvailableFishOnIsland();

        if (availableFishOnIsland.Count == 0) return;

        // บอสสุ่มชนิดปลา และ สุ่มจำนวน (เช่น Wave ท้ายๆ อาจจะขอ 2-4 ตัว)
        currentQuestFish = availableFishOnIsland[Random.Range(0, availableFishOnIsland.Count)];

        // คำนวณความยาก: ยิ่ง Wave ลึก ยิ่งขอจำนวนเยอะขึ้น (ปรับได้ตามชอบ)
        int minFish = 1 + (currentWave / 3);
        int maxFish = 3 + (currentWave / 2);
        currentQuestAmount = Random.Range(minFish, maxFish);

        // รีเซ็ตสถานะบอส และอัปเดตป้ายเควสต์
        bossState = BossState.SLEEPING;
        daysSinceFed = 0f;
        UpdateBossUI($"{currentQuestFish.fishName} : {currentQuestAmount}");

        float minDist = 400f, maxDist = 600f;
        if (currentWave >= 4 && currentWave <= 6) { minDist = 600f; maxDist = 900f; }
        else if (currentWave >= 7) { minDist = 900f; maxDist = 1200f; }

        float currentSpawnDistance = Random.Range(minDist, maxDist);
        float randomAngle = Random.Range(0f, 360f);
        Vector3 spawnDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        Vector3 spawnPos = hubIsland.position + (spawnDirection.normalized * currentSpawnDistance);
        spawnPos.y = 0;

        currentQuestIsland = Instantiate(selectedIslandPrefab, spawnPos, Quaternion.identity);

        IslandFishSpawner spawnedIslandScript = currentQuestIsland.GetComponent<IslandFishSpawner>();
        if (spawnedIslandScript != null)
        {
            spawnedIslandScript.SpawnEcosystem(currentQuestFish, currentQuestAmount + 2);
        }

        if (compass != null) compass.SetTarget(currentQuestIsland.transform);
    }
    #endregion
}