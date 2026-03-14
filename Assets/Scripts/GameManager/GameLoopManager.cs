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
                UpdateBossUI($"HUNGRY!\n{GetQuestString()}");
                break;
            case BossState.ANGRY:
                UpdateBossUI($"<color=red>ANGRY!</color>\n{GetQuestString()}");
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

    private string GetQuestString()
    {
        string text = "";
        foreach (var quest in currentQuests)
        {
            text += $"- {quest.fish.fishName} : {quest.amount} ตัว\n";
        }
        return text;
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
            Debug.Log("ให้อาหารบอสสำเร็จ!");
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
            UpdateBossUI(missingText);
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

        int typesRequired = 1;
        if (currentWave >= 4) typesRequired = 2;
        if (currentWave >= 9) typesRequired = 3;

        typesRequired = Mathf.Min(typesRequired, availableFish.Count);

        for (int i = 0;i < typesRequired;i++)
        {
            FishSpawnEntry entry = availableFish[i];

            int baseAmount = 1;
            switch (entry.rarity)
            {
                case FishRarity.Common: baseAmount = Random.Range(2, 5); break;     
                case FishRarity.Uncommon: baseAmount = Random.Range(1, 3); break;
                case FishRarity.Rare: baseAmount = 1; break;                        
                case FishRarity.Epic: baseAmount = 1; break;
                case FishRarity.Legendary: baseAmount = 1; break;
            }

            int waveMultiplier = (currentWave / 3);
            int finalAmount = baseAmount + waveMultiplier;

            currentQuests.Add(new QuestTarget
            {
                fish = entry.fishData,
                amount = finalAmount
            });
        }

        if (currentQuestIsland != null)
        {
            currentQuestIsland.SpawnEcosystem(currentQuests);
        }

        bossState = BossState.SLEEPING;
        daysSinceFed = 0f;
        UpdateBossUI($"{GetQuestString()}");

        if (compass != null) compass.SetTarget(currentQuestIsland.transform);
    }
    #endregion
}