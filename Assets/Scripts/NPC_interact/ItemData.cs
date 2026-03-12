using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/Item data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
    public int price;

    [Header("Animation Settings")]
    [Tooltip("0=Bare hands, 1=Holding FishingRob, 2=Holding Fish, 3=Sword Idle, 4=Pistol Aim, 5=Rifle Idle")]
    public int holdAnimID = 1;

    [Header("Hold Settings")]
    public Vector3 holdPositionOffset;
    public Vector3 holdRotationOffset;

    public Vector3 attackHoldPositionOffset;
    public Vector3 attackHoldRotationOffset;

    [Header("Combat & Gun Settings")]
    public float attackDamage = 20f;
    public float attackRange = 100f;

    [Header("Gun FX Settings")]
    public AudioClip shootSound; // เสียงตอนยิง
    public GameObject muzzleFlashPrefab; // ประกายไฟปลายปืน
    public GameObject bulletTrailPrefab; // เส้นกระสุน (ใช้ Line Renderer)


    public bool isGun = false;
    public bool isAutomatic = false; // ปืนกล (กดค้างได้ไหม)
    public int bulletsPerShot = 1; // จำนวนกระสุนต่อการกด 1 ครั้ง (ลูกซองใส่ 5-8)
    public float bulletSpread = 0f; // ความบานของเป้า (ปืนพก/ไรเฟิล = 0 หรือ 0.02, ลูกซอง = 0.1)
    public float fireRate = 0.2f; // ความเร็วในการยิง (แทน Attack Cooldown เดิม)
    public GameObject hitEffectPrefab; // (Optional) เอฟเฟกต์รอยกระสุน/ประกายไฟตอนยิงโดน
}   