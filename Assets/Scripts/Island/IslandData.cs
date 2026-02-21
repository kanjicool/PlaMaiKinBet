using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FishSpawnRate
{
    public FishData fish;
    public int weight;
}

[CreateAssetMenu(fileName = "New Island", menuName = "fishing System/Island Data")]
public class IslandData : ScriptableObject
{
    [Header("Island Infor")]
    public string islandName;

    [Header("Endemic Fishes")]
    public List<FishSpawnRate> availableFishes;
}