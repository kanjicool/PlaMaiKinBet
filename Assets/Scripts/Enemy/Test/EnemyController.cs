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

        // Update Animations
        anim.SetFloat("speed", agent.velocity.magnitude);
        anim.SetBool("isChasing", isChasing);
    }

    // --- LOGIC METHODS ---

    private void Patrolling()
    {
        currentState = EnemyState.Patrolling;
        isChasing = false;
        agent.speed = data.patrolSpeed;

        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet) agent.SetDestination(walkPoint);

        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;
    }

    private void Chasing()
    {
        currentState = EnemyState.Chasing;
        isChasing = true;
        agent.speed = data.chaseSpeed;
        agent.acceleration = data.acceleration;
        agent.SetDestination(player.position);
    }

    private void Attacking()
    {
        currentState = EnemyState.Attacking;
        agent.SetDestination(transform.position);
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
        Vector3 targetPos = new Vector3(transform.position.x + randomX, transform.position.y + 10f, transform.position.z + randomZ);

        if (Physics.Raycast(targetPos, Vector3.down, out RaycastHit hit, 20f, whatIsGround))
        {
            walkPoint = hit.point;
            walkPointSet = true;
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
}
