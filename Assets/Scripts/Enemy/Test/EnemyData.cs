using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy AI/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Health Settings")]
    public float maxHealth = 100f;

    [Header("Patrol Settings")]
    public float walkPointRange = 10f;
    public float patrolSpeed = 2f;

    [Header("Chase Settings")]
    public float chaseSpeed = 7f;
    public float acceleration = 12f;
    public float sightRange = 15f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float timeBetweenAttacks = 2f;
    public float damage = 10f;
    public float attackDamage = 10f;
}
