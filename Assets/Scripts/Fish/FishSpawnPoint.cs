using UnityEngine;

public enum FishRarity
{
    Common, Uncommon, Rare, Epic, Legendary
}

[System.Serializable]
public class FishSpawnEntry
{
    public FishData fishData;
    public FishRarity rarity;
}

public class FishSpawnPoint : MonoBehaviour
{
    [Header("Fish Settings")]
    public FishSpawnEntry[] allowedFish;

    [Header("Spawn Settings")]
    [Range(0f, 100f)]
    public float spawnChance = 100f;
    public float spawnRadius = 2f;

    [Tooltip("จำนวนปลาที่จะเกิดต่อ 1 หน่วยรัศมี (เช่น รัศมี 10 * Density 1.5 = ปลา 15 ตัว)")]
    public float fishDensity = 1.0f;

    [Tooltip("จำกัดจำนวนปลาสูงสุดต่อจุด (กันปลาล้นจอ)")]
    public int maxSpawnLimit = 20;

    public int GetCalculatedSpawnAmount()
    {
        // เปลี่ยนมาใช้สูตรคูณธรรมดา: รัศมี x ความหนาแน่น (กะตัวเลขด้วยตาเปล่าง่ายกว่าเยอะครับ)
        int amount = Mathf.RoundToInt(spawnRadius * fishDensity);

        // บังคับว่าต้องมีปลาอย่างน้อย 1 ตัว และห้ามเกินค่า maxSpawnLimit ที่ตั้งไว้
        return Mathf.Clamp(amount, 1, maxSpawnLimit);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }
}