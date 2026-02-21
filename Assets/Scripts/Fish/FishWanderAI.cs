using UnityEngine;

public class FishWanderAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float swimRadius = 5f;
    public float swimSpeed = 2f;
    public float turnSpeed = 3f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 4f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isSwimming = false;
    private float waitTimer = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        SetNewTargetPostion();
    }

    // Update is called once per frame
    void Update()
    {
        if (isSwimming)
        {
            SwimTowardsTarget();
        }
        else
        {
            WaitAndThink();
        }
        
    }

    private void SwimTowardsTarget()
    {
        // Find target direction
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Turn around to target
        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }

        // swimming
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, swimSpeed * Time.deltaTime);

        // Check target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            isSwimming = false;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    private void WaitAndThink()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            SetNewTargetPostion();
        }
    }

    private void SetNewTargetPostion()
    {
        Vector2 randomCicle = Random.insideUnitCircle * swimRadius;
        targetPosition = startPosition + new Vector3(randomCicle.x, 0f, randomCicle.y);
        isSwimming = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, swimRadius);
    }

}
