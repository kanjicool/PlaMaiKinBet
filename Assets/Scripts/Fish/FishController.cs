using UnityEngine;
using Bitgem.VFX.StylisedWater;

public class FishController : MonoBehaviour
{
    [Header("Fish Information")]
    public FishData myData;

    private enum FishState { Wander, Flee, ChaseBait, Hooked, Caught }
    private FishState currentState = FishState.Wander;

    [Header("Movement Settings")]
    public float swimRadius = 5f;
    public float normalSpeed = 2f;
    public float turnSpeed = 3f;
    public float waterSurfaceY = 0f;
    public float swimDepth = 1.5f;

    [Header("Detection (Layer & Tags)")]
    public LayerMask scareLayer;
    public string baitTag = "Bait";

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float waitTimer = 0f;

    private Transform currentBait;
    private Animator anim;

    void Start ()
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
                {
                    currentState = FishState.Wander;
                }
                break;

            case FishState.ChaseBait:
                if (currentBait != null)
                {
                    targetPosition = currentBait.position;
                    HandleMovement(normalSpeed * 1.5f);

                    if (Vector3.Distance(transform.position, currentBait.position) < 0.5)
                    {
                        BiteBait();
                    }
                }
                else
                {
                    currentState = FishState.Wander;
                }
                break;
            case FishState.Hooked:
                break;
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
            if (col.CompareTag(baitTag))
            {
                Bobber bobber = col.GetComponent<Bobber>();
                if (bobber != null && bobber.isInWater)
                {
                    currentBait = col.transform;
                    currentState = FishState.ChaseBait;
                    Debug.Log($"{myData.fishName} เห็นเหยื่อแล้ว!");
                    if (anim != null) anim.SetInteger("SwimState", 2);
                    break;
                }
            }
        }
    }

    private void FleeFrom(Vector3 scaryPosition)
    {
        currentState = FishState.Flee;
        Vector3 fleeDirection = (transform.position - scaryPosition).normalized;
        Vector3 potentialTarget = transform.position + (fleeDirection * myData.fleeRaius * 1.5f);

        targetPosition = EnsureUnderwater(potentialTarget);
        if (anim != null) anim.SetInteger("SwimState", 2);
    }

    private Vector3 EnsureUnderwater(Vector3 pos)
    {
        if (pos.y > waterSurfaceY - swimDepth)
        {
            pos.y = waterSurfaceY - swimDepth;
        }
        
        return pos;
    }

    private void BiteBait()
    {
        currentState = FishState.Hooked;
        Debug.Log($"{myData.fishName} งับเหยื่อแล้ว!!");
        if (anim != null) anim.SetTrigger("isHooked");

        if (currentBait != null && currentBait.TryGetComponent<Bobber>(out Bobber bobber))
        {
            bobber.OnFishBite(this);
        }
    }

    private void HandleMovement(float speed)
    {
        // 1. เช็คระยะห่างในแนวราบ (X, Z) ละเว้นแกน Y เพื่อป้องกันปลาว่ายติดพื้นดินใต้น้ำ
        Vector3 currentPos2D = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos2D = new Vector3(targetPosition.x, 0, targetPosition.z);
        float distanceToTarget = Vector3.Distance(currentPos2D, targetPos2D);

        // 2. ถ้าถึงเป้าหมายแล้ว ให้หยุดพัก
        if (currentState == FishState.Wander && distanceToTarget < 0.5f)
        {
            if (anim != null) anim.SetInteger("SwimState", 0); // หยุดอนิเมชัน

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                SetNewTargetPosition();
            }
        }
        // 3. ถ้ายังไม่ถึงเป้าหมาย ให้ว่ายต่อไป
        else
        {
            // บังคับเล่นอนิเมชันว่ายน้ำ
            if (anim != null && currentState == FishState.Wander) anim.SetInteger("SwimState", 1);

            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
            }

            // เคลื่อนที่ไปหาเป้าหมาย
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    public void Initialize(FishData data)
    {
        myData = data;
        Debug.Log($"Fish {myData.fishName} is spawn!");
    }

    public void BiteHook()
    {
        currentState = FishState.Hooked;
        Debug.Log($"Fish bit the hook! EscapePower = {myData.escapePower}");
        if (anim != null)
        {
            anim.SetTrigger("isHooked");
        }

    }

    public void Struggle()
    {
        if (currentState == FishState.Hooked)
        {
            Debug.Log("Struggle");
        }
    }

    private void SetNewTargetPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * swimRadius;
        Vector3 randomPos = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        targetPosition = EnsureUnderwater(randomPos);
        waitTimer = Random.Range(1f, 4f);
        if (anim != null) anim.SetInteger("SwimState", 1);
    }

    private void OnDrawGizmosSelected()
    {
        if (myData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, myData.detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, myData.fleeRaius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(targetPosition, 0.2f);
    }
}
