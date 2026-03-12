using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("Health System")]
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider;
    public Transform spawnPoint;

    [Header("Melee Combat")]
    public float baseAttackDamage = 20f;
    public float attackRange = 0.8f;
    public Transform attackPoint;
    public string enemyTag = "Enemy";
    public float attackCooldown = 0.4f;
    public float comboFinishCooldown = 1.2f;
    public float comboResetTime = 1.2f;
    public float attackLockDuration = 0.35f;

    private float nextAttackTime = 0f;
    private float lastAttackTime = 0f;
    private int currentComboStep = 0;

    private int lastHoldType = -1;

    [Header("References")]
    private Animator animator;
    private PlayerInventory inventory;
    private PlayerController controller;
    private InputSystem_Actions inputActions;
    private AudioSource audioSource;
    private Rigidbody rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        inventory = GetComponent<PlayerInventory>();
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        inputActions = new InputSystem_Actions();
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Start()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (controller == null)
        {
            controller = FindFirstObjectByType<PlayerController>();
        }
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        HandleCombatInput();
    }

    private void HandleCombatInput()
    {
        if (inventory != null && inventory.isInventoryOpen) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (controller != null && controller.IsBusy) return;
        if (Time.time < nextAttackTime) return;

        ItemData heldItem = GetCurrentItemData();
        int holdType = inventory != null ? inventory.GetCurrentHoldAnimID() : 0;

        if (holdType != lastHoldType)
        {
            currentComboStep = 0; 
            lastHoldType = holdType; 

            nextAttackTime = Time.time + 0.5f;
        }

        // 1 = เบ็ดตกปลา, 2 = ถือปลา (แก้ไขเลขให้ตรงกับเกมของคุณ)
        if (holdType == 1 || holdType == 2)
        {

            // ถ้าถือของพวกนี้อยู่ ไม่ต้องรับคำสั่งคลิกซ้ายเลย ให้หยุดฟังก์ชันนี้ไปเลย
            return;
        }

        // แยกการทำงานระหว่าง ปืน กับ อาวุธระยะประชิด (ดาบ/มือเปล่า)
        if (heldItem != null && heldItem.isGun)
        {
            if (heldItem.isAutomatic && inputActions.Player.Attack.IsPressed())
            {
                ShootGun(heldItem);
            }
            else if (!heldItem.isAutomatic && inputActions.Player.Attack.WasPressedThisFrame())
            {
                ShootGun(heldItem);
            }
        }
        else
        {
            if (inputActions.Player.Attack.WasPressedThisFrame())
            {
                PerformMeleeAttack(holdType, heldItem);
            }
        }
    }

    // ===================== MELEE SYSTEM =====================
    private void PerformMeleeAttack(int holdType, ItemData weaponData)
    {
        if (Time.time - lastAttackTime > comboResetTime) currentComboStep = 0;

        currentComboStep++;
        float currentHitCooldown = attackCooldown;

        float finalDamage = (weaponData != null) ? weaponData.attackDamage : baseAttackDamage; ;

        if (holdType == 0) // หมัด
        {
            if (currentComboStep > 4) currentComboStep = 1;
            animator.SetTrigger("punch" + currentComboStep);

            if (currentComboStep == 4)
            {
                finalDamage *= 2f;
                currentHitCooldown = comboFinishCooldown;
            }
            
        }
        else if (holdType == 3) // ดาบ (อ้างอิง ID จากของคุณ)
        {
            if (currentComboStep > 4) currentComboStep = 1;
            animator.SetTrigger("sword" + currentComboStep);

            if (currentComboStep == 4)
            {
                finalDamage *= 2f;
                currentHitCooldown = comboFinishCooldown;
            }
        }
        else return;

        lastAttackTime = Time.time;
        nextAttackTime = Time.time + currentHitCooldown;

        ApplyMeleeDamage(finalDamage);

        Debug.Log($"โจมตีด้วยท่าที่ {currentComboStep} ทำดาเมจ: {finalDamage}");
    }

    private void ApplyMeleeDamage(float damage)
    {
        if (attackPoint != null)
        {
            Collider[] hitObjects = Physics.OverlapSphere(attackPoint.position, attackRange);
            foreach (Collider hitObject in hitObjects)
            {
                if (hitObject.CompareTag(enemyTag))
                {
                    DummyEnemy dummy = hitObject.GetComponent<DummyEnemy>();
                    if (dummy != null) dummy.TakeDamage(damage);

                    EnemyController enemy = hitObject.GetComponent<EnemyController>();
                    if (enemy != null) enemy.TakeDamage(damage);

                }
            }
        }
    }

    // ===================== GUN SYSTEM =====================
    private void ShootGun(ItemData gunData)
    {
        nextAttackTime = Time.time + gunData.fireRate;
        //animator.SetTrigger("shoot");

        if (audioSource != null && gunData.shootSound != null)
            audioSource.PlayOneShot(gunData.shootSound);

        Transform firePoint = inventory.handTransform;
        GameObject heldGun = inventory.GetHeldItem();
        if (heldGun != null)
        {
            Transform barrel = heldGun.transform.Find("Barrel");
            if (barrel != null) firePoint = barrel;
        }

        if (gunData.muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(gunData.muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
            Destroy(flash, 0.1f);
        }

        Transform camTransform = Camera.main.transform;

        for (int i = 0; i < gunData.bulletsPerShot; i++)
        {
            Vector3 shootDirection = camTransform.forward;
            if (gunData.bulletSpread > 0f)
            {
                shootDirection += new Vector3(
                    Random.Range(-gunData.bulletSpread, gunData.bulletSpread),
                    Random.Range(-gunData.bulletSpread, gunData.bulletSpread),
                    Random.Range(-gunData.bulletSpread, gunData.bulletSpread)
                );
            }

            Vector3 hitPosition;
            if (Physics.Raycast(camTransform.position, shootDirection, out RaycastHit hit, gunData.attackRange))
            {
                hitPosition = hit.point;
                if (hit.collider.CompareTag(enemyTag))
                {
                    DummyEnemy dummy = hit.collider.GetComponent<DummyEnemy>();
                    if (dummy != null) dummy.TakeDamage(gunData.attackDamage);

                    EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                    if (enemy != null) enemy.TakeDamage(gunData.attackDamage);
                }

                if (gunData.hitEffectPrefab != null)
                {
                    GameObject impact = Instantiate(gunData.hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
                    if (hitRenderer != null)
                    {
                        ParticleSystem ps = impact.GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            var main = ps.main;
                            main.startColor = hitRenderer.material.color;
                        }
                    }
                    Destroy(impact, 2f);
                }
            }
            else
            {
                hitPosition = camTransform.position + (shootDirection * gunData.attackRange);
            }

            if (gunData.bulletTrailPrefab != null)
            {
                StartCoroutine(SpawnBulletTrail(gunData.bulletTrailPrefab, firePoint.position, hitPosition));
            }
        }
    }

    private IEnumerator SpawnBulletTrail(GameObject trailPrefab, Vector3 startPos, Vector3 endPos)
    {
        GameObject trail = Instantiate(trailPrefab, startPos, Quaternion.identity);
        LineRenderer line = trail.GetComponent<LineRenderer>();
        if (line != null)
        {
            line.SetPosition(0, startPos);
            line.SetPosition(1, endPos);
        }
        yield return new WaitForSeconds(0.05f);
        Destroy(trail);
    }

    // ===================== UTILITIES & EVENTS =====================
    public ItemData GetCurrentItemData()
    {
        if (inventory != null && inventory.GetHeldItem() != null)
        {
            ItemHolder holder = inventory.GetHeldItem().GetComponent<ItemHolder>();
            if (holder != null) return holder.itemData;
        }
        return null;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (currentHealth <= 0) DieAndRespawn();
    }

    private void DieAndRespawn()
    {
        currentHealth = maxHealth;
        rb.linearVelocity = Vector3.zero;
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
    }

    // Animation Events
    public void UseSlashTransform()
    {
        if (inventory != null)
        {
            inventory.UseSlashTransform();
        }
    }

    public void ResetToHoldTransform()
    {
        if (inventory != null)
        {
            inventory.ResetToHoldTransform();
        }
    }
    public bool IsAttacking() { return Time.time < lastAttackTime + attackLockDuration; }

    public void PlayerDealDamage()
    {
        ItemData heldItem = GetCurrentItemData(); // แก้ตัวแปรให้สะกดถูก
        float finalDamage = (heldItem != null) ? heldItem.attackDamage : baseAttackDamage;
        ApplyMeleeDamage(finalDamage);
    }

}