using UnityEngine;

public class FishWanderAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float swimRadius = 5f;
    public float normalSpeed = 2f;
    public float fastSpeed = 4f;
    public float turnSpeed = 3f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 4f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isSwimming = false;
    private float waitTimer = 0f;


    private bool isAttracted = false;
    private Transform baitTarget;

    private Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        anim = GetComponentInChildren<Animator>();
        SetNewTargetPostion();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAttracted && baitTarget != null)
        {
            SwimTowardsBait();
        }
        else if (isSwimming)
        {
            SwimTowardsTarget();
        }
        else
        {
            WaitAndThink();
        }
        
    }

    public void AttractToBait(Transform bait)
    {
        if (isAttracted) return;

        isAttracted = true;
        baitTarget = bait;
        Debug.Log(gameObject.name + " Found Bait Let go Fast!!!");

        if (anim != null) anim.SetInteger("SwimState", 2);
    }

    private void SwimTowardsBait()
    {
        Vector3 directionToBait = (baitTarget.position - transform.position).normalized;
        if (directionToBait != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToBait);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }

        transform.position = Vector3.MoveTowards(transform.position, baitTarget.position, fastSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, baitTarget.position) < 0.5f)
        {
            isAttracted = false;
            if (anim != null)
            {
                anim.SetTrigger("Impulse");
                anim.SetInteger("SwimState", 0);
            }
            baitTarget.SendMessage("OnFishBite", this, SendMessageOptions.DontRequireReceiver);

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
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, normalSpeed * Time.deltaTime);

        // Check target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            isSwimming = false;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);

            if (anim != null) anim.SetInteger("SwimState", 0);
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

        if (anim != null) anim.SetInteger("SwimState", 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, swimRadius);
    }

}
