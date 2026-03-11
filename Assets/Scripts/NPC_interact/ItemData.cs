using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/Item data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
    public int price;

    [Header("Animation Settings")]
    [Tooltip("0=Bare hands, 1=Common items/knife/torch, 2=fishing rod, 3=pistol, 4=long gun/shotgun")]
    public int holdAnimID = 1;

    [Header("Hold Settings")]
    public Vector3 holdPositionOffset;
    public Vector3 holdRotationOffset;
}   