using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]

    public float maxHealth = 100f;
    public float currentHealth;

    private bool isDead = false;
    private Animator anim;

    void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + "Health: " + currentHealth);

        if (anim != null) anim.SetTrigger("getHit");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        var enemyAI = GetComponent<EnemyHealth>();
        if (enemyAI != null) enemyAI.enabled = false;

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) anim.SetBool("isDead", true);

        Debug.Log(gameObject.name + " has died!");

        Destroy(gameObject, 5f);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
