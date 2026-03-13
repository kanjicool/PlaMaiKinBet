using UnityEngine;

public class FishController : MonoBehaviour
{
    [Header("Fish Information")]
    public FishData myData;

    public enum FishState { Wander, Flee, ChaseBait, HookedAndWait, CaughtReeling }
    public FishState currentState = FishState.Wander;

    [Header("Movement Settings")]
    public float swimRadius = 5f;
    public float normalSpeed = 2f;
    public float turnSpeed = 3f;
    public float waterSurfaceY = 0f;
    public float swimDepth = 1.5f;

    [Header("Bait Inspection")]
    private float nibbleTimer = 0f;
    private Vector3 baitOffset;


    [Header("Detection")]
    public LayerMask scareLayer;
    public string baitTag = "Bait";

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float waitTimer = 0f;

    private Transform currentBait;
    private Animator anim;
    private Transform playerTarget;
    private System.Action onCatchComplete;

    private Vector3 hookPosition;
    private Vector3 struggleTarget;
    private float struggleTimer = 0f;
    private Rigidbody bobberRb;

    void Start()
    {
        startPosition = transform.position;
        anim = GetComponent<Animator>();
        SetNewTargetPosition();
    }

    private void Update()
    {
        if (myData == null) return;

        switch (currentState)
        {
            case FishState.Wander:
                CheckSurroundings();
                HandleMovement(normalSpeed);
                break;

            case FishState.Flee:
                HandleMovement(normalSpeed * myData.fleeSpeedMultiplier);
                if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
                    currentState = FishState.Wander;
                break;

            case FishState.ChaseBait:
                if (currentBait != null && currentBait.TryGetComponent<Bobber>(out Bobber bobber) && !bobber.isBitten)
                {
                    nibbleTimer -= Time.deltaTime;

                    if (nibbleTimer > 0f)
                    {
                        // --- เฟสที่ 1: ว่ายวนตอดดูเหยื่อ (Inspect) ---
                        Vector3 inspectTarget = currentBait.position + baitOffset;
                        targetPosition = EnsureUnderwater(inspectTarget);

                        HandleMovement(normalSpeed * 0.7f);

                        // ถ้าว่ายมาถึงจุดวนแล้ว ให้สุ่มจุดวนใหม่ (ทำให้ปลาว่ายขยับไปมารอบทุ่นเรื่อยๆ)
                        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
                        {
                            GenerateBaitOffset();
                        }
                    }
                    else
                    {
                        // --- เฟสที่ 2: หมดเวลาตัดสินใจ พุ่งเข้าฮุบเหยื่อ! (Strike) ---
                        targetPosition = currentBait.position;

                        HandleMovement(normalSpeed * 2.5f);
                        if (anim != null) anim.SetInteger("SwimState", 2);

                        if (Vector3.Distance(transform.position, currentBait.position) < 0.3f)
                        {
                            BiteBait();
                        }
                    }
                }
                else
                {
                    currentState = FishState.Wander;
                    currentBait = null;
                    SetNewTargetPosition(); 
                }
                break;

            case FishState.HookedAndWait:
                HandleStruggling();
                break;

            case FishState.CaughtReeling:
                HandleReelingIn();
                break;
        }
    }

    private void HandleMovement(float speed)
    {
        Vector3 currentPos2D = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos2D = new Vector3(targetPosition.x, 0, targetPosition.z);

        if (currentState == FishState.Wander && Vector3.Distance(currentPos2D, targetPos2D) < 0.5f)
        {
            if (anim != null) anim.SetInteger("SwimState", 0);
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) SetNewTargetPosition();
        }
        else
        {
            if (anim != null && currentState == FishState.Wander) anim.SetInteger("SwimState", 1);

            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * turnSpeed);

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    private void HandleReelingIn()
    {
        if (playerTarget == null) return;

        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        if (directionToPlayer != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 10f);

        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, 15f * Time.deltaTime);

        if (Vector3.Distance(transform.position, playerTarget.position) < 1.0f)
        {
            onCatchComplete?.Invoke();
        }
    }

    private void CheckSurroundings()
    {
        Collider[] scaryObjects = Physics.OverlapSphere(transform.position, myData.fleeRaius, scareLayer);
        if (scaryObjects.Length > 0)
        {
            FleeFrom(scaryObjects[0].transform.position);
            return;
        }

        Collider[] nearbyBaits = Physics.OverlapSphere(transform.position, myData.detectionRadius);
        foreach (var col in nearbyBaits)
        {
            if (col.CompareTag(baitTag) && col.TryGetComponent<Bobber>(out Bobber bobber) && bobber.isInWater && !bobber.isBitten)
            {
                currentBait = col.transform;
                currentState = FishState.ChaseBait;

                nibbleTimer = UnityEngine.Random.Range(1.5f, 4.0f);
                GenerateBaitOffset();

                if (anim != null) anim.SetInteger("SwimState", 2);
                break;
            }
        }
    }

    private void BiteBait()
    {
        if (currentBait != null && currentBait.TryGetComponent<Bobber>(out Bobber bobber))
        {
            if (bobber.ReceiveFishBite(this))
            {
                currentState = FishState.HookedAndWait;
                if (anim != null) anim.SetTrigger("isHooked");

                hookPosition = transform.position;
                struggleTarget = hookPosition;
                struggleTimer = 0f;
                bobberRb = currentBait.GetComponent<Rigidbody>();
            }
            else
            {
                currentState = FishState.Wander;
                currentBait = null;
            }
        }
        else
        {
            currentState = FishState.Wander;
        }


    }

    public void StartReeling(Transform target, System.Action onComplete)
    {
        currentState = FishState.CaughtReeling;
        playerTarget = target;
        onCatchComplete = onComplete;

        if (TryGetComponent<Collider>(out Collider col)) col.enabled = false;
        if (anim != null) anim.SetInteger("SwimState", 1);
    }

    public void Escape()
    {
        currentState = FishState.Flee;
        Vector3 fleeDirection = (transform.position - Camera.main.transform.position).normalized;
        targetPosition = EnsureUnderwater(transform.position + (fleeDirection * myData.fleeRaius * 3f));

        if (anim != null) anim.SetInteger("SwimState", 2);
        Invoke(nameof(ResetToWander), 3f);
    }

    private void FleeFrom(Vector3 scaryPosition)
    {
        currentState = FishState.Flee;
        Vector3 fleeDir = (transform.position - scaryPosition).normalized;
        targetPosition = EnsureUnderwater(transform.position + (fleeDir * myData.fleeRaius * 1.5f));
        if (anim != null) anim.SetInteger("SwimState", 2);
    }

    public void Initialize(FishData data) { myData = data; }

    private void ResetToWander()
    {
        currentState = FishState.Wander;
        SetNewTargetPosition();
    }

    private void SetNewTargetPosition()
    {
        Vector2 rand = Random.insideUnitCircle * swimRadius;
        targetPosition = EnsureUnderwater(startPosition + new Vector3(rand.x, 0f, rand.y));
        waitTimer = Random.Range(1f, 4f);
        if (anim != null) anim.SetInteger("SwimState", 1);
    }

    private Vector3 EnsureUnderwater(Vector3 pos)
    {
        if (pos.y > waterSurfaceY - swimDepth) pos.y = waterSurfaceY - swimDepth;
        return pos;
    }

    private void HandleStruggling()
    {
        struggleTimer -= Time.deltaTime;
        if (struggleTimer <= 0f)
        {
            Vector2 rand = Random.insideUnitCircle * 1.5f;
            float randomDepth = Random.Range(-0.5f, -2.5f);

            struggleTarget = EnsureUnderwater(hookPosition + new Vector3(rand.x, randomDepth, rand.y));
            struggleTimer = Random.Range(0.15f, 0.4f); 
        }

        Vector3 direction = (struggleTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * turnSpeed * 4f);
        }

        float struggleSpeed = normalSpeed * myData.fleeSpeedMultiplier * 1.5f;
        transform.position = Vector3.MoveTowards(transform.position, struggleTarget, struggleSpeed * Time.deltaTime);

        if (bobberRb != null)
        {
            Vector3 targetBobberPos = new Vector3(transform.position.x, transform.position.y + 0.8f, transform.position.z);
            targetBobberPos.y = Mathf.Clamp(targetBobberPos.y, waterSurfaceY - 1.0f, waterSurfaceY);
            bobberRb.MovePosition(Vector3.Lerp(bobberRb.position, targetBobberPos, Time.deltaTime * 15f));

            Vector3 pullDir = (targetBobberPos - bobberRb.position).normalized;
            if (pullDir != Vector3.zero)
            {
                bobberRb.rotation = Quaternion.Slerp(bobberRb.rotation, Quaternion.LookRotation(pullDir), Time.deltaTime * 10f);
            }



            Vector3 pullDirection = (transform.position - currentBait.position).normalized;

            float pullForce = myData.escapePower * 3f;

            bobberRb.AddForce(pullDirection * pullForce * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    private void GenerateBaitOffset()
    {
        Vector2 rand = UnityEngine.Random.insideUnitCircle * 0.8f;

        float randomDepth = UnityEngine.Random.Range(-0.1f, -0.6f);

        baitOffset = new Vector3(rand.x, randomDepth, rand.y);
    }


    // ==========================================
    // DEBUG GIZMOS
    // ==========================================
    private void OnDrawGizmosSelected()
    {
        Vector3 centerPos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(centerPos, swimRadius);

        if (myData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, myData.detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, myData.fleeRaius);
        }

        if (Application.isPlaying)
        {
            if (currentState == FishState.HookedAndWait)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(hookPosition, 1.5f); // 1.5f คือระยะดิ้นที่เราตั้งไว้
                Gizmos.DrawSphere(struggleTarget, 0.1f);
                Gizmos.DrawLine(transform.position, struggleTarget);
            }
            else
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(targetPosition, 0.15f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
        }
    }
}