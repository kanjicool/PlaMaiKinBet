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
    public int maxStock = 1; // เพิ่มส่วนนี้สำหรับระบบจำกัดจำนวนซื้อ

    [Header("Animation Settings")]
    [Tooltip("0=Bare hands, 1=Holding FishingRob, 2=Holding Fish, 3=Sword Idle, 4=Pistol Aim, 5=Rifle Idle")]
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
}