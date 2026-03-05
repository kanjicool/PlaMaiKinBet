using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
    public int price;
}   