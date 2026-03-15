using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/Item data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
    public int price;

    [Header("Shop Settings")]
    [Tooltip("จำนวนสินค้าสูงสุดที่มีขายในร้านค้าต่อการ restock หนึ่งครั้ง")]
    public int maxStock = 1;

    // 🌟 ส่วนที่เพิ่มเข้ามาสำหรับระบบตกปลา
    [Header("Fishing System")]
    public bool isFishingRod = false; // ติ๊กถูกถ้าเป็นเบ็ด
    public bool isBait = false;       // ติ๊กถูกถ้าเป็นเหยื่อ

    [Header("Bait Settings")]
    public int baitTier = 1;                 // ระดับเหยื่อ (1-6)
    public float biteChanceBonus = 10f;      // โบนัส % ทำให้ปลางับเบ็ดง่ายขึ้น
    public float escapePowerReduction = 0f;  // ลดความเก่งของปลาในมินิเกม (ทำให้หลุดยากขึ้น)

    [Header("Animation Settings")]
    [Tooltip("0=Bare hands, 1=Holding FishingRob, 2=Holding Fish, 3=Sword Idle, 4=Pistol Aim, 5=Rifle Idle, 6=items Idle")]
    public int holdAnimID = 1;

    public float equipDelay = 0.5f;

    [Header("Hold Settings")]
    public Vector3 holdPositionOffset;
    public Vector3 holdRotationOffset;

    [Header("Audio Settings")]
    public AudioClip equipSound;

    public Vector3 attackHoldPositionOffset;
    public Vector3 attackHoldRotationOffset;

    [Header("Combat & Gun Settings")]
    public float attackDamage = 20f;
    public float attackRange = 100f;

    [Header("Gun FX Settings")]
    public float muzzleScale = 0.01f;
    public AudioClip shootSound;
    public GameObject muzzleFlashPrefab;
    public GameObject bulletTrailPrefab;

    public bool isGun = false;
    public bool isAutomatic = false;
    public int bulletsPerShot = 1;
    public float bulletSpread = 0f;
    public float fireRate = 0.2f;
    public GameObject hitEffectPrefab;

    [Header("Consumable / Healing Settings")]
    public bool isConsumable = false;
    public float useItemHoldTime = 2.0f;
    public float healAmount = 20f;
    public AudioClip useSound;

    // 🔦 หมวดหมู่ใหม่สำหรับไฟฉาย
    [Header("Flashlight Settings")]
    public bool isFlashlight = false;       // ติ๊กถูกถ้าไอเทมนี้คือไฟฉาย
    public float lightIntensity = 10f;      // ความสว่างของไฟฉาย
    public float lightRange = 30f;          // ระยะการส่องสว่าง
    public Color lightColor = Color.white;  // สีของแสงไฟ
    public AudioClip toggleSound;           // เสียงคลิกตอนกดเปิด-ปิดไฟฉาย
}