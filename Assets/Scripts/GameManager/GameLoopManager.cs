using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BossState { SLEEPING, HUNGRY, ANGRY, RAMPAGING }

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;

    [Header("References")]
    public Transform player;
    public Transform hubIsland; 
    public GameObject[] islandPrefabs;
    public CompassDirection compass;

    [Header("Quest System")]
    public FishData[] allAvailableFish; // ใส่ FishData ทั้งหมดที่มีในเกมที่นี่
    public FishData currentQuestFish;   // ปลาที่บอสอยากกินใน Wave นี้

    [Header("Wave & Progression")]
    public int currentWave = 1;
    public GameObject currentQuestIsland;
    public bool hasFishForBoss = false;
    private int lastIslandIndex = -1;

    [Header("Boss State Machine")]
    public BossState bossState = BossState.SLEEPING;
    public float timeScale = 1f; // 1 วินาทีจริง = 1 วันในเกม (ปรับได้)
    public float daysSinceFed = 0f;
    public float hungryThreshold = 2f; // วันที่เริ่มหิว
    public float angryThreshold = 4f;  // วันที่เริ่มโกรธ
    public float rampageThreshold = 5f; // วันที่ออกอาละวาด (1 วันจริงหลังโกรธ)

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // เริ่มเกมมาให้ชี้เข็มทิศไปที่ศูนย์กลางก่อน (หรือเริ่ม Wave 1 เลยก็ได้)
        StartNextWave();
    }

    void Update()
    {
        UpdateBossStateMachine();
        HandleDebugInputs();
    }

    #region Boss State Machine
    private void UpdateBossStateMachine()
    {
        // เพิ่มเวลา (จำลองเป็นวัน)
        daysSinceFed += Time.deltaTime * timeScale;

        // เช็ค State หิว
        if (bossState == BossState.SLEEPING && daysSinceFed >= hungryThreshold)
        {
            ChangeBossState(BossState.HUNGRY);
        }
        // เช็ค State โกรธ
        else if (bossState == BossState.HUNGRY && daysSinceFed >= angryThreshold)
        {
            ChangeBossState(BossState.ANGRY);
        }
        // เช็ค State อาละวาด
        else if (bossState == BossState.ANGRY && daysSinceFed >= rampageThreshold)
        {
            ChangeBossState(BossState.RAMPAGING);
        }
    }

    private void ChangeBossState(BossState newState)
    {
        bossState = newState;
        switch (newState)
        {
            case BossState.SLEEPING:
                Debug.Log("Boss: Zzz... (รีเซ็ตเวลา)");
                daysSinceFed = 0f;
                break;
            case BossState.HUNGRY:
                Debug.Log("Boss: เริ่มหิวแล้ว! (แสดง Warning UI / เปลี่ยนเพลง)");
                break;
            case BossState.ANGRY:
                Debug.Log("Boss: โกรธมาก! (เกาะสั่น / น้ำเปลี่ยนเป็นสีแดง)");
                break;
            case BossState.RAMPAGING:
                Debug.Log("Boss: RAMPAGING! Leviathan ออกล่าผู้เล่น!");
                // TODO: โค้ดเสก Leviathan ไล่ล่าผู้เล่น
                break;
        }
    }

    public void TryFeedBoss()
    {
        if (currentQuestFish == null) return;

        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory == null) return;

        // เช็กว่าผู้เล่นมีปลา ItemData ตรงกับที่เควสต์ต้องการไหม
        if (inventory.HasItem(currentQuestFish.fishItemData))
        {
            Debug.Log($"ให้อาหารบอสด้วย {currentQuestFish.fishName} สำเร็จ! ผ่าน Wave {currentWave}!");

            // ลบปลาออกจากกระเป๋า
            inventory.ConsumeItem(currentQuestFish.fishItemData);

            ChangeBossState(BossState.SLEEPING);

            if (currentQuestIsland != null) Destroy(currentQuestIsland);

            currentWave++;
            StartNextWave();
        }
        else
        {
            Debug.Log($"บอสไม่กิน! บอสต้องการ: {currentQuestFish.fishName} ไปหามาใหม่!");
        }
    }


    public void FeedBoss()
    {
        if (!hasFishForBoss) return;

        Debug.Log($"ให้อาหารบอสสำเร็จใน Wave ที่ {currentWave}! บอสกลับไปนอนแล้ว");
        hasFishForBoss = false;
        ChangeBossState(BossState.SLEEPING);

        // ลบเกาะเควสต์เก่าทิ้ง (หรือจะเก็บไว้เป็นประวัติศาสตร์ก็ได้)
        if (currentQuestIsland != null) Destroy(currentQuestIsland);

        currentWave++;
        StartNextWave();
    }
    #endregion


    #region Wave & Radial Spawning
    public void StartNextWave()
    {
        if (islandPrefabs == null || islandPrefabs.Length == 0) return;

        // 1. สุ่มเลือก Prefab เกาะ (ยังไม่เสกจริง)
        int randomIslandIndex = Random.Range(0, islandPrefabs.Length);
        if (islandPrefabs.Length > 1 && randomIslandIndex == lastIslandIndex)
            randomIslandIndex = (randomIslandIndex + Random.Range(1, islandPrefabs.Length)) % islandPrefabs.Length;
        lastIslandIndex = randomIslandIndex;

        GameObject selectedIslandPrefab = islandPrefabs[randomIslandIndex];

        // 2. ถาม Prefab เกาะนี้ว่า มีปลาอะไรอาศัยอยู่บ้าง?
        IslandFishSpawner spawnerPrefab = selectedIslandPrefab.GetComponent<IslandFishSpawner>();
        List<FishData> availableFishOnIsland = spawnerPrefab.GetAvailableFishOnIsland();

        if (availableFishOnIsland.Count == 0)
        {
            Debug.LogError($"เกาะ {selectedIslandPrefab.name} ไม่มีข้อมูล FishData เลย! ไปตั้งค่าที่ FishSpawnPoint ก่อนครับ");
            return; // หยุดทำงาน ป้องกันบัก
        }

        // 3. บอสเลือกปลาเควสต์ จากลิสต์ปลาที่มีบนเกาะนั้นๆ เท่านั้น!
        currentQuestFish = availableFishOnIsland[Random.Range(0, availableFishOnIsland.Count)];

        Debug.Log($"--- เริ่ม Wave {currentWave} ---");
        Debug.Log($"[เควสต์] บอสต้องการกิน: {currentQuestFish.fishName}!");

        // 4. คำนวณระยะทาง
        float minDist = 400f, maxDist = 600f;
        if (currentWave >= 4 && currentWave <= 6) { minDist = 600f; maxDist = 900f; }
        else if (currentWave >= 7) { minDist = 900f; maxDist = 1200f; }

        float currentSpawnDistance = Random.Range(minDist, maxDist);
        float randomAngle = Random.Range(0f, 360f);
        Vector3 spawnDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        Vector3 spawnPos = hubIsland.position + (spawnDirection.normalized * currentSpawnDistance);
        spawnPos.y = 0;

        // 5. สร้างเกาะของจริง
        currentQuestIsland = Instantiate(selectedIslandPrefab, spawnPos, Quaternion.identity);

        // 6. สั่งให้เกาะเสกระบบนิเวศ (เสกทั้งปลาทั่วไป และบังคับเสกปลาเควสต์ 3 ตัว)
        IslandFishSpawner spawnedIslandScript = currentQuestIsland.GetComponent<IslandFishSpawner>();
        if (spawnedIslandScript != null)
        {
            spawnedIslandScript.SpawnEcosystem(currentQuestFish, 3);
        }

        // 7. ชี้เข็มทิศ
        if (compass != null) compass.SetTarget(currentQuestIsland.transform);
    }

    public void OnReachQuestIsland()
    {
        Debug.Log("ถึงเกาะเป้าหมายแล้ว! (จำลองการทำเควสต์ด้วยการกดปุ่ม Enter)");
        // ระบบรอให้ผู้เล่นกดตกปลา (ทำใน HandleDebugInputs)
    }

    public void CatchFishCompleted()
    {
        if (hasFishForBoss) return;

        Debug.Log("ตกปลาสำเร็จ! ได้ของที่บอสต้องการแล้ว รีบกลับ Hub!");
        hasFishForBoss = true;

        // ชี้เข็มทิศกลับไปที่ Hub Island
        if (compass != null) compass.SetTarget(hubIsland);
    }
    #endregion

    private void HandleDebugInputs()
    {
        if (Keyboard.current == null) return;

        // กด Enter เพื่อจำลองว่า 'ตกปลาเสร็จแล้ว' (ใช้ตอนอยู่บนเกาะเป้าหมาย)
        if (Keyboard.current.enterKey.wasPressedThisFrame && currentQuestIsland != null)
        {
            CatchFishCompleted();
        }

        // กด P เพื่อข้าม Wave (ทดสอบ)
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            FeedBoss(); // จำลองว่าให้อาหารเลยเพื่อความรวดเร็ว
        }
    }
}