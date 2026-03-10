using UnityEngine;

public class BossRobotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float chaseSpeed = 30f;
    public float returnSpeed = 40f;
    public float hoverHeight = 2f;

    private Vector3 originalHubPosition;
    private Quaternion originalHubRotation;
    private Transform targetPlayer;

    private enum BossPhase { Idle, Chasing, Returning }
    private BossPhase currentPhase = BossPhase.Idle;


    void Start()
    {
        originalHubPosition = transform.position;
        originalHubRotation = transform.rotation;
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        switch (currentPhase)
        {
            case BossPhase.Idle:
                float newY = originalHubPosition.y + Mathf.Sin(Time.time * 2f) * 0.5f;
                transform.position = new Vector3(originalHubPosition.x, newY, originalHubPosition.z);
                break;

            case BossPhase.Chasing:
                if (targetPlayer != null)
                {
                    Vector3 targetPos = targetPlayer.position;
                    targetPos.y = Mathf.Max(targetPos.y, hoverHeight);

                    transform.position = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
                    transform.LookAt(targetPos);
                }
                break;


            case BossPhase.Returning:
                transform.position = Vector3.MoveTowards(transform.position, originalHubPosition, returnSpeed * Time.deltaTime);

                Vector3 directionToHub = (originalHubPosition - transform.position).normalized;
                if (directionToHub != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToHub), Time.deltaTime * 5f);
                }

                if (Vector3.Distance(transform.position, originalHubPosition) < 0.1f)
                {
                    transform.position = originalHubPosition;
                    transform.rotation = originalHubRotation; 
                    currentPhase = BossPhase.Idle;

                    if (GameLoopManager.Instance != null)
                    {
                        GameLoopManager.Instance.ResetBossState();
                    }
                }
                break;

        }
    }

    public void StartRampage(Transform playerTransform)
    {
        targetPlayer = playerTransform;
        currentPhase = BossPhase.Chasing;
        Debug.Log("ËØè¹Â¹µìºÍÊ¾Øè§à¢éÒâ¨ÁµÕ!!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentPhase == BossPhase.Chasing && (other.CompareTag("Player") || other.CompareTag("Boat")))
        {
            Debug.Log("ËØè¹Â¹µìºÍÊª¹¼ÙéàÅè¹áÅéÇ GAME OVER!");

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(9999f);
            }

            currentPhase = BossPhase.Returning;
            targetPlayer = null;
        }
    }
}
