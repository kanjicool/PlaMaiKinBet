using UnityEngine;
using UnityEngine.AI;

public class SimpleChaser : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        // ตรวจสอบว่ามีผู้เล่น และ ตัวศัตรูยืนอยู่บนพื้น NavMesh แล้วจริงๆ ถึงจะสั่งให้เดิน
        if (player != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }
}