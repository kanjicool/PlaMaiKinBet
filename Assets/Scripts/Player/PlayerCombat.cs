using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerCombat : MonoBehaviour
{
    [Header("Health System")]
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider;
    public Transform spawnPoint;

    [Header("Life System")]
    public int maxLives = 3;
    private int currentLives;
    public TextMeshProUGUI livesText;

    [Header("Damge Effect")]
    public Renderer[] playerRenderers;
    public Material damageMaterial;
    public float flashDuration = 0.15f;
    //private Color[] originalColors;
    private Material[][] originalMaterials;

    [Header("Consumable / Healing Settings")]
    public float useItemHoldTime = 2.0f;
    private float currentHoldTimer = 0f;
    private bool isUsingItem = false;

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

        currentLives = maxLives;
        UpdateLivesUI();

        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (playerRenderers != null && playerRenderers.Length > 0)
        {
            originalMaterials = new Material[playerRenderers.Length][];
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null)
                {
                    originalMaterials[i] = playerRenderers[i].materials;
                }
            }
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

            float delay = (heldItem != null) ? heldItem.equipDelay : 0.5f;
            nextAttackTime = Time.time + delay;

            return; 
        }



        // 1 = เบ็ดตกปลา, 2 = ถือปลา (แก้ไขเลขให้ตรงกับเกมของคุณ)
        if (holdType == 1 || holdType == 2)
        {
            return;
        }

        if (heldItem != null && heldItem.isConsumable)
        {
            if (inputActions.Player.Attack.IsPressed()) // เช็คว่ากดค้างอยู่ไหม
            {
                isUsingItem = true;
                currentHoldTimer += Time.deltaTime;

                Debug.Log($"กำลังใช้ยา... ({currentHoldTimer:F1}/{useItemHoldTime}s)");

                if (currentHoldTimer >= useItemHoldTime)
                {
                    UseConsumable(heldItem);
                    currentHoldTimer = 0f;
                    isUsingItem = false;
                }
                return; // 🛑 สำคัญ: return ออกไปเลยเพื่อไม่ให้มันไปทำงานส่วน Melee ด้านล่างขณะกดยา
            }
            else
            {
                if (currentHoldTimer > 0)
                {
                    Debug.Log("ยกเลิกการใช้ยา");
                    currentHoldTimer = 0f;
                    isUsingItem = false;
                }
            }
        }

        // แยกการทำงานระหว่าง ปืน กับ อาวุธระยะประชิด (ดาบ/มือเปล่า)
        if (heldItem != null && heldItem.isGun)
        {
            if (heldItem.isConsumable)
            { 
                Debug.Log("ถือยาาาาาา");
            }

            Debug.Log("ถือปืนนนนนน");

            if (heldItem.isAutomatic && inputActions.Player.Attack.IsPressed())
            {
                ShootGun(heldItem, holdType);
            }
            else if (!heldItem.isAutomatic && inputActions.Player.Attack.WasPressedThisFrame())
            {
                ShootGun(heldItem, holdType);
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
    private void ShootGun(ItemData gunData, int holdType)
    {
        nextAttackTime = Time.time + gunData.fireRate;

        UseSlashTransform();

        if (holdType == 5)
        {
            animator.SetTrigger("shootRifle"); 
        }
        //else if (holdType == 6)
        //{
        //    animator.SetTrigger("shootShotgun");
        //}
        //else
        //{
        //    animator.SetTrigger("shootGun");
        //}

        if (audioSource != null && gunData.shootSound != null)
            audioSource.PlayOneShot(gunData.shootSound);

        CancelInvoke(nameof(ResetToHoldTransform));
        Invoke(nameof(ResetToHoldTransform), gunData.fireRate);

        Transform firePoint = inventory.handTransform;
        GameObject heldGun = inventory.GetHeldItem();
        if (heldGun != null)
        {
            Transform barrel = FindChildRecursive(heldGun.transform, "Barrel");
            if (barrel != null) firePoint = barrel;
        }

        if (gunData.muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(gunData.muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);

            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;

            flash.transform.localScale = new Vector3(gunData.muzzleScale, gunData.muzzleScale, gunData.muzzleScale);

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
        StartCoroutine(DamageFlashRoutine());
        if (currentHealth <= 0) DieAndRespawn();
    }

    private void DieAndRespawn()
    {
        currentLives--;
        UpdateLivesUI();

        if (UIManager.Instance != null && GameLoopManager.Instance != null)
        {
            UIManager.Instance.ShowDeathScreen(GameLoopManager.Instance.currentWave, currentLives);
        }
    }

    public void ExecuteRespawn()
    {
        BoatInteract[] allBoats = FindObjectsByType<BoatInteract>(FindObjectsSortMode.None);
        foreach (BoatInteract boat in allBoats)
        {
            boat.ForceExitBoat();
            boat.RespawnBoat();
        }

        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (spawnPoint != null)
        {
            // วาร์ปกลับจุดเกิด
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
    }


    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"LIVES: {currentLives}";
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

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;

            Transform found = FindChildRecursive(child, childName);
            if (found != null) return found;
        }
        return null;
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (playerRenderers == null || playerRenderers.Length == 0 || damageMaterial == null) yield break;

        // เปลี่ยนทุกชิ้นส่วนเป็น Material สีแดง
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                Material[] flashMats = new Material[playerRenderers[i].materials.Length];
                for (int j = 0; j < flashMats.Length; j++)
                {
                    flashMats[j] = damageMaterial; // สวมทับสีแดง
                }
                playerRenderers[i].materials = flashMats;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // คืนค่า Material เดิม
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null && originalMaterials[i] != null)
            {
                playerRenderers[i].materials = originalMaterials[i];
            }
        }
    }

    // ===================== HEALING SYSTEM =====================
    private void UseConsumable(ItemData itemData)
    {
        if (currentHealth >= maxHealth)
        {
            Debug.Log("เลือดเต็มแล้ว");
            return;
        }

        Heal(itemData.healAmount);

        // เล่นเสียงฮีลสำเร็จ
        if (audioSource != null && itemData.useSound != null)
            audioSource.PlayOneShot(itemData.useSound);

        // ลบยาออกจากมือ
        if (inventory != null)
            inventory.ConsumeHeldItem();

        Debug.Log("ใช้ยาสำเร็จ!");
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // ไม่ให้เลือดเกิน Max

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}