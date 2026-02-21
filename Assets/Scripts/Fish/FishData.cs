using UnityEngine;

[CreateAssetMenu(fileName = "Fish", menuName = "Scriptable Objects/FishData")]
public class FishData : ScriptableObject
{
    [Header("Basic Info")]
    public string fishName;
    public GameObject fishPrefab;
    public int price;

    [Header("Difficulty Settings")]
    public float escapePower = 5f;
    public float stamina = 100f;
    public float biteDelay = 2f;
}
