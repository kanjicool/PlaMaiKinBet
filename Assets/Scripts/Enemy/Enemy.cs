using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
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

    public enum EnemyState {  Patrolling, Chasing, Attacking};
    public EnemyState currentState;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        playerInsight = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInsight && !playerInAttackRange) Patrolling();
        if (playerInsight && !playerInAttackRange) Chasing();
        if (playerInsight && playerInAttackRange) Attacking();
    }

    private void Patrolling()
    {
        currentState = EnemyState.Patrolling;
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
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;

    }

    private void Chasing()
    {
        currentState = EnemyState.Chasing;
        agent.SetDestination(player.position);
    }

    private void Attacking()
    {
        currentState = EnemyState.Attacking;
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        Debug.Log("Enemy Attacking");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }


}
