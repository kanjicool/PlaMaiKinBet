using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BossState { SLEEPING, HUNGRY, ANGRY, RAMPAGING }

[System.Serializable]
public class QuestTarget
{
    public FishData fish;
    public int amount;
}

[System.Serializable]
public class QuestRarityProgression
{
    public FishRarity rarity;
    public float baseWeight;           // โอกาสที่บอสจะขอใน Wave 1
    public float weightChangePerWave;  // โอกาสที่จะเพิ่ม/ลด ต่อ 1 Wave
    public float minWeight = 0f;       // โอกาสต่ำสุด (เช่น ไม่ต่ำกว่า 0%)
    public float maxWeight = 100f;     // โอกาสสูงสุด

    public float GetCurrentWeight(int wave)
    {
        float weight = baseWeight + (weightChangePerWave * (wave - 1));
        return Mathf.Clamp(weight, minWeight, maxWeight);
    }
}

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;

    [Header("References")]
    public Transform player;
    public Transform hubIsland;
    public CompassDirection compass;

    [Header("World Islands")]
    public IslandFishSpawner[] sceneIslands;

    [Header("Quest System & UI")]
    public List<QuestTarget> currentQuests = new List<QuestTarget>();
    public TextMeshProUGUI bossQuestText;

    [Header("UI Layout Settings")]
    [Tooltip("HUNGRY, ANGRY")]
    public Vector2 textPosWithSlots = new Vector2(0, 100);
    [Tooltip("SLEEPING, RAMPAGING")]
    public Vector2 textPosCentered = new Vector2(0, 0);

    [Header("Quest Slots UI")]
    public Transform questSlotsContainer; // GameObject ที่ติด Layout Group
    public GameObject questSlotPrefab;    // Prefab ที่มีสคริปต์ QuestSlotUI
    public GameObject scrollViewObject;



    [Header("Wave & Progression")]
    public int currentWave = 1;
    public IslandFishSpawner currentQuestIsland;
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

    [Header("Wave Progression Tuning")]
    [Tooltip("Every few waves, the boss asks for one more type of fish.")]
    public int wavesToIncreaseType = 4;
    [Tooltip("Maximum number of fish types the boss can request at once")]
    public int maxFishTypesRequired = 5;

    [Header("Rarity Weights per Wave")]
    public List<QuestRarityProgression> questRarityProgressions = new List<QuestRarityProgression>();

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

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            DebugBypassWave();
        }
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
                UpdateQuestUI("...Zzz...", false);
                if (ScreenEffectManager.Instance != null) ScreenEffectManager.Instance.ClearBossPressure();
                break;
            case BossState.HUNGRY:
                UpdateQuestUI(">:( HUNGRY!", true);
                if (ScreenEffectManager.Instance != null) ScreenEffectManager.Instance.ClearBossPressure();
                break;
            case BossState.ANGRY:
                UpdateQuestUI("<color=red>ANGRY!</color>", true);
                if (ScreenEffectManager.Instance != null) ScreenEffectManager.Instance.TriggerBossAngry();
                break;
            case BossState.RAMPAGING:
                UpdateQuestUI("<color=red>ERROR! TARGET LOCKED!</color>", false);

                if (ScreenEffectManager.Instance != null) ScreenEffectManager.Instance.TriggerBossRampage();

                if (bossRobot != null && player != null)
                {
                    bossRobot.StartRampage(player);
                }
                break;
        }
    }

    public void UpdateQuestUI(string mainMessage, bool showFishSlots)
    {
        if (bossQuestText != null)
        {
            bossQuestText.text = mainMessage;

            RectTransform textRect = bossQuestText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchoredPosition = showFishSlots ? textPosWithSlots : textPosCentered;
            }
        }

        if (questSlotsContainer != null)
        {
            foreach (Transform child in questSlotsContainer)
            {
                Destroy(child.gameObject);
            }

            scrollViewObject.SetActive(false);


            if (showFishSlots && questSlotPrefab != null)
            {
                scrollViewObject.SetActive(true);

                foreach (var quest in currentQuests)
                {
                    GameObject slotObj = Instantiate(questSlotPrefab, questSlotsContainer);

                    slotObj.transform.localScale = Vector3.one;

                    QuestSlotUI slotUI = slotObj.GetComponent<QuestSlotUI>();

                    if (slotUI != null)
                    {
                        slotUI.SetupSlot(quest.fish.fishIcon, $"{quest.fish.fishName} x{quest.amount}");
                    }
                }
            }
        }
    }

    public void ResetBossState()
    {
        ChangeBossState(BossState.SLEEPING);
    }

    public void TryFeedBoss()
    {
        if (currentQuests.Count == 0) return;

        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory == null) return;

        bool hasAllItems = true;
        string missingText = "Still not enough!";

        foreach (var quest in currentQuests)
        {
            int currentAmount = inventory.GetItemCount(quest.fish.fishItemData);
            if (currentAmount < quest.amount)
            {
                hasAllItems = false;
                missingText += $"You're still missing {quest.fish.fishName} : {quest.amount - currentAmount}\n";
            }
        }

        if (hasAllItems)
        {
            foreach (var quest in currentQuests)
            {
                inventory.ConsumeItems(quest.fish.fishItemData, quest.amount);
            }

            ChangeBossState(BossState.SLEEPING);

            currentWave++;
            Invoke("StartNextWave", 3f);
        }

        else
        {
            Debug.Log("ของไม่พอ!");
            UpdateQuestUI($"<color=orange>{missingText}</color>", true);
        }
    }
    #endregion


    #region Wave Generation
    public void StartNextWave()
    {
        if (sceneIslands == null || sceneIslands.Length == 0)
        {
            Debug.LogError("ยังไม่ได้ลาก Scene Islands ใส่ใน GameLoopManager!");
            return;
        }

        int randomIslandIndex = Random.Range(0, sceneIslands.Length);
        if (sceneIslands.Length > 1 && randomIslandIndex == lastIslandIndex)
        {
            randomIslandIndex = (randomIslandIndex + Random.Range(1, sceneIslands.Length)) % sceneIslands.Length;
        }

        lastIslandIndex = randomIslandIndex;
        currentQuestIsland = sceneIslands[randomIslandIndex];

        List<FishSpawnEntry> availableFish = currentQuestIsland.GetAvailableFishEntries();
        if (availableFish.Count == 0) return;

        for (int i = 0; i < availableFish.Count; i++)
        {
            FishSpawnEntry temp = availableFish[i];
            int rand = Random.Range(i, availableFish.Count);
            availableFish[i] = availableFish[rand];
            availableFish[rand] = temp;
        }

        currentQuests.Clear();

        int typesRequired = 1 + ((currentWave - 1) / wavesToIncreaseType);
        typesRequired = Mathf.Clamp(typesRequired, 1, Mathf.Min(maxFishTypesRequired, availableFish.Count));

        List<FishSpawnEntry> pool = new List<FishSpawnEntry>(availableFish);

        typesRequired = Mathf.Min(typesRequired, availableFish.Count);

        for (int i = 0; i < typesRequired; i++)
        {
            FishSpawnEntry pickedFish = PickRandomFishByWaveWeight(pool, currentWave);

            if (pickedFish != null)
            {
                // คำนวณ "จำนวนตัว" แบบสมการ (ไม่ Hardcode แล้ว!)
                // ใช้สมการถอดรูท (Sqrt) เพื่อให้ช่วงแรกขอเพิ่มไว แต่ช่วง Wave ลึกๆ จำนวนจะไม่เฟ้อจนเกินไป
                int baseAmount = (pickedFish.rarity == FishRarity.Common) ? 3 :
                                 (pickedFish.rarity == FishRarity.Uncommon) ? 2 : 1;

                int waveBonus = Mathf.FloorToInt(Mathf.Sqrt(currentWave));
                int finalAmount = baseAmount + waveBonus;

                currentQuests.Add(new QuestTarget
                {
                    fish = pickedFish.fishData,
                    amount = finalAmount
                });

                // เอาปลาที่เลือกแล้วออกจากตะกร้า จะได้ไม่สุ่มได้ปลาชนิดเดิมซ้ำ
                pool.Remove(pickedFish);
            }
        }

        string debugMsg = $"<color=cyan>[DEBUG Wave {currentWave}]</color> บอสต้องการปลาทั้งหมด {currentQuests.Count} ชนิด ได้แก่:\n";
        foreach (var quest in currentQuests)
        {
            debugMsg += $"- {quest.fish.fishName} : จำนวน {quest.amount} ตัว\n";
        }

        debugMsg += $"<color=grey>(ข้อมูลเกาะ: มีปลาที่สามารถเกิดได้รวม {availableFish.Count} ชนิด)</color>";

        Debug.Log(debugMsg);

        if (currentQuestIsland != null)
        {
            currentQuestIsland.SpawnEcosystem(currentQuests);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWaveText(currentWave);
        }

        ChangeBossState(BossState.SLEEPING);

        if (compass != null) compass.SetTarget(currentQuestIsland.transform);
    }

    private FishSpawnEntry PickRandomFishByWaveWeight(List<FishSpawnEntry> pool, int wave)
    {
        float totalWeight = 0f;
        List<float> weights = new List<float>();

        // คำนวณน้ำหนักของปลาแต่ละตัวในกอง
        foreach (var entry in pool)
        {
            float w = 10f; // ค่าตั้งต้นกันเหนียว
            foreach (var prog in questRarityProgressions)
            {
                if (prog.rarity == entry.rarity)
                {
                    w = prog.GetCurrentWeight(wave);
                    break;
                }
            }
            weights.Add(w);
            totalWeight += w;
        }

        // Safety Net
        if (totalWeight <= 0) return pool[Random.Range(0, pool.Count)];

        // สุ่มตัวเลข 0 ถึง น้ำหนักรวม
        float randomVal = Random.Range(0f, totalWeight);
        float currentTotal = 0f;

        // เช็คว่าตกที่ปลาตัวไหน
        for (int i = 0; i < pool.Count; i++)
        {
            currentTotal += weights[i];
            if (randomVal <= currentTotal) return pool[i];
        }

        return pool[0];
    }


    #endregion

    private void DebugBypassWave()
    {
        Debug.Log($"<color=yellow>[DEBUG] ข้าม Wave {currentWave} ไปยัง {currentWave + 1}</color>");

        // 1. ล้างปลาในเกาะปัจจุบันทิ้งก่อน (ป้องกันปลากระจุกตัวตอนกด P รัวๆ)
        if (currentQuestIsland != null)
        {
            currentQuestIsland.ClearOldFishes();
        }

        // 2. สั่งบวก Wave และเริ่มการสุ่ม Wave ใหม่ทันทีแบบไม่ต้องรอดีเลย์
        currentWave++;
        StartNextWave();
    }
}