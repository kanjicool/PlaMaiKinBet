using UnityEngine;

[CreateAssetMenu(fileName = "Fish", menuName = "Scriptable Objects/FishData")]
public class FishData : ScriptableObject
{
    [Header("Basic Info")]
    public string fishName;
    public GameObject fishPrefab;
    public Sprite fishIcon;
    public int price;

    public ItemData fishItemData;

    [Header("Difficulty Settings")]
    public float escapePower = 5f;
    public float stamina = 100f;
    public float biteDelay = 2f;

    [Header("AI Settings")]
    public float detectionRadius = 10f;
    public float fleeRaius = 5f;
    public float fleeSpeedMultiplier = 2.5f;
}
