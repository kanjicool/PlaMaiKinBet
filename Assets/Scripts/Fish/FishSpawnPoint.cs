using UnityEngine;

public class FishSpawnPoint : MonoBehaviour
{
    [Header("Fish Settings")]
    public FishData[] allowedFish;

    [Header("Spawn Settings")]
    [Range(0f, 100f)]
    public float spawnChance = 100f;
    public float spawnRadius = 2f;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }
}