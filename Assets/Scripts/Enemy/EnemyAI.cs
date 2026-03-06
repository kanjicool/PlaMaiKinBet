using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Settings")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    [Header("Patrol Settings")]
    public float walkPointRange = 10f;
    private Vector3 walkPoint;
    private bool walkPointSet;

    [Header("State Detection")]
    public float sightRange = 15f;
    public float attackRange = 2f;
    private bool playerInsight, playerInAttackRange;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 7f;
    public float acceleration = 12f;


    [Header("Attack Settings")]
    public float timeBetweenAttacks = 2f;
    private bool alreadyAttacked;

    private Animator anim;
    private bool isChasing = false;


    public enum EnemyState {  Patrolling, Chasing, Attacking};
    public EnemyState currentState;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        anim.SetFloat("speed", agent.velocity.magnitude);
        anim.SetBool("isChasing", isChasing);

        playerInsight = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (playerInsight && playerInAttackRange) 
        {
            Debug.Log("Attck");
            isChasing = true; 
            Attacking();
        }
        else if (playerInsight && !playerInAttackRange) 
        {
            Debug.Log("Chasing");
            isChasing = true;
            Chasing();
        }
        else 
        {
            Debug.Log("Patrolling");
            isChasing = false; 
            Patrolling();
        }

        anim.SetFloat("speed", agent.velocity.magnitude);
        anim.SetBool("isChasing", isChasing);

    }

    private void Patrolling()
    {
        currentState = EnemyState.Patrolling;

        agent.speed = patrolSpeed;

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        Vector3 randomTarget = new Vector3(transform.position.x + randomX, transform.position.y + 10f, transform.position.z + randomZ);

        if (Physics.Raycast(randomTarget, Vector3.down, 20f, whatIsGround))
        {
            RaycastHit hit;
            if (Physics.Raycast(randomTarget, Vector3.down, out hit, 20f, whatIsGround))
            {
                walkPoint = hit.point;
                walkPointSet = true;
            }
        }

    }

    private void Chasing()
    {
        currentState = EnemyState.Chasing;

        agent.speed = chaseSpeed;
        agent.acceleration = acceleration;
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
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }

        Debug.Log("Enemy Attacking");
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }


}
