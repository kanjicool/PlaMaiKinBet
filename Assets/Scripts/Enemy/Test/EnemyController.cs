using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public EnemyData data;

    [Header("References")]
    private NavMeshAgent agent;
    private Transform player;
    private Animator anim;
    public LayerMask whatIsGround, whatIsPlayer;

    private float currentHealth;
    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool alreadyAttacked;
    private bool isDead = false;
    private bool isChasing = false;

    // --- เพิ่มตัวแปรสำหรับระบบยืนพัก ---
    private bool isWaiting = false;
    private float waitTimer = 0f;

    public enum EnemyState { Patrolling, Chasing, Attacking };
    public EnemyState currentState;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (data != null) currentHealth = data.maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning(gameObject.name + " can not found NavMesh!");
            return;
        }

        // Check Ranges
        bool playerInSight = Physics.CheckSphere(transform.position, data.sightRange, whatIsPlayer);
        bool playerInAttackRange = Physics.CheckSphere(transform.position, data.attackRange, whatIsPlayer);

        // State Machine
        if (playerInSight && playerInAttackRange) Attacking();
        else if (playerInSight && !playerInAttackRange) Chasing();
        else Patrolling();

        // Update Animations (ความเร็วจะสัมพันธ์กับ agent.velocity อัตโนมัติ ถ้ายืนนิ่งค่าจะเป็น 0)
        anim.SetFloat("speed", agent.velocity.magnitude);
        anim.SetBool("isChasing", isChasing);
    }

    // --- LOGIC METHODS ---

    private void Patrolling()
    {
        currentState = EnemyState.Patrolling;
        isChasing = false;
        agent.speed = data.patrolSpeed;

        // 1. ถ้าระบบกำลังสั่งให้หยุดรอ ให้หักลบเวลาไปเรื่อยๆ
        if (isWaiting)
        {
            agent.isStopped = true; // สั่งเบรก
            waitTimer -= Time.deltaTime;

            // หมดเวลายืนรอ
            if (waitTimer <= 0f)
            {
                isWaiting = false;
            }
            return; // ไม่ต้องทำอะไรต่อตราบใดที่ยังรออยู่
        }

        // 2. ถ้าไม่ได้รอ ก็อนุญาตให้เดินได้
        agent.isStopped = false;

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);

            // เช็คว่าถ้าเส้นทางนี้ "ไปไม่ถึง"
            if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.Log("Path blocked! Finding new route...");
                walkPointSet = false;
                agent.ResetPath();
            }
        }

        // 3. เช็คว่าเดินถึงเป้าหมายหรือยัง (ใช้ stoppingDistance เพื่อความเนียนเวลาเบียดกัน)
        if (walkPointSet && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            walkPointSet = false;

            // สั่งให้เริ่มรอ และสุ่มเวลายืนพักระหว่าง 2 ถึง 4 วินาที (ปรับเปลี่ยนได้ตามต้องการ)
            isWaiting = true;
            waitTimer = Random.Range(2f, 4f);
        }
    }

    private void Chasing()
    {
        currentState = EnemyState.Chasing;
        isChasing = true;

        // ยกเลิกสถานะรอ และอนุญาตให้วิ่ง
        isWaiting = false;
        agent.isStopped = false;

        agent.speed = data.chaseSpeed;
        agent.acceleration = data.acceleration;
        agent.SetDestination(player.position);
    }

    private void Attacking()
    {
        currentState = EnemyState.Attacking;

        // หยุดเดินเพื่อไม่ให้เบียดรวมร่างกับมอนสเตอร์ตัวอื่นตอนล้อมตี
        agent.isStopped = true;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (!alreadyAttacked)
        {
            anim.SetTrigger("attack");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), data.timeBetweenAttacks);
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-data.walkPointRange, data.walkPointRange);
        float randomX = Random.Range(-data.walkPointRange, data.walkPointRange);
        Vector3 randomPos = transform.position + new Vector3(randomX, 0, randomZ);

        NavMeshHit hit;
        // หาพื้นที่สีฟ้าที่ใกล้ที่สุดในรัศมี 5 เมตร
        if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
        else
        {
            walkPointSet = false; // ถ้าสุ่มไม่ได้ ให้ผ่านไปก่อน ค่อยหาใหม่เฟรมหน้า
        }
    }

    private void ResetAttack() => alreadyAttacked = false;

    // --- HEALTH SYSTEM ---

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        anim.SetTrigger("getHit");
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true;
        anim.SetBool("isDead", true);
        Destroy(gameObject, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // ล้างเส้นทางเดิมทิ้งทันที
            agent.ResetPath();

            // ไม่ต้องรอยืนพักตอนชนน้ำ ให้สุ่มจุดหาทางหนีใหม่ทันที
            isWaiting = false;
            walkPointSet = false;

            SearchWalkPoint();

            if (walkPointSet)
            {
                agent.SetDestination(walkPoint);
            }
        }
    }
}