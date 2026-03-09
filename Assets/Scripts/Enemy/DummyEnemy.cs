using UnityEngine;
using UnityEngine.UI; // [NEW] ต้องใช้สำหรับ UI

public class DummyEnemy : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider; // [NEW] หลอดเลือดบนหัวศัตรู

    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    private float nextAttackTime;

    private Transform player;
    private PlayerController playerLogic;

    private void Start()
    {
        currentHealth = maxHealth;

        // [NEW] ตั้งค่าเริ่มต้นให้หลอดเลือด
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        Debug.Log($">>>>>> Enemy: {playerObj}");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerLogic = playerObj.GetComponent<PlayerController>();
            Debug.Log("Enemy: หาตัว Player เจอแล้ว พร้อมบวก!");
        }
        else
        {
            Debug.LogWarning("Enemy: หา Player ไม่เจอ! ลืมตั้ง Tag 'Player' ให้ตัวละครหลักหรือเปล่า?"); // [เช็กจุดที่ 2]
        }
    }

    private void Update()
    {
        // [NEW] ทำให้หลอดเลือดหันหน้าเข้าหากล้องเสมอ (Billboard)
        if (healthSlider != null)
        {
            healthSlider.transform.LookAt(healthSlider.transform.position + Camera.main.transform.rotation * Vector3.forward,
                                          Camera.main.transform.rotation * Vector3.up);
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //Debug.Log("ระยะห่างจาก Player: " + distanceToPlayer);

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        Debug.Log(">>> ATK");

        nextAttackTime = Time.time + attackCooldown;
        if (playerLogic != null)
        {
            playerLogic.TakeDamage(attackDamage);
            Debug.Log("ดัมมี่โจมตี Player!");
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("ดัมมี่โดนตี! เลือดเหลือ: " + currentHealth);

        // [NEW] อัปเดตหลอดเลือด
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("ดัมมี่ตาย (รอเกิดใหม่เพื่อเป็นเป้าซ้อมต่อ)");
        currentHealth = maxHealth;

        // [NEW] รีเซ็ตหลอดเลือดเมื่อเกิดใหม่
        if (healthSlider != null) healthSlider.value = currentHealth;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}