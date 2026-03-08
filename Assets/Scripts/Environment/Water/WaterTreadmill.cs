using UnityEngine;

public class WaterTreadmill : MonoBehaviour
{
    [Header("References")]
    public Transform player;      
    public GameObject waterPrefab; 

    [Header("Settings")]
    public float tileSize = 100f;
    public float offsetSurface = -9.5f;


    private GameObject[,] waterTiles = new GameObject[5, 5];

    private Vector2Int currentGridPos;

    void Start()
    {
        for (int x = -2; x <= 2; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                Vector3 spawnPos = new Vector3(x * tileSize, offsetSurface, z * tileSize);

                waterTiles[x + 2, z + 2] = Instantiate(waterPrefab, spawnPos, Quaternion.identity, transform);
            }
        }

        InitGridPosition();
    }

    void Update()
    {
        if (player == null) return;

        Vector2Int newGridPos = new Vector2Int(
            Mathf.RoundToInt(player.position.x / tileSize),
            Mathf.RoundToInt(player.position.z / tileSize)
        );

        if (newGridPos != currentGridPos)
        {
            currentGridPos = newGridPos;
            ShiftWaterTiles();
        }
    }

    void InitGridPosition()
    {
        if (player == null) return;
        currentGridPos = new Vector2Int(
            Mathf.RoundToInt(player.position.x / tileSize),
            Mathf.RoundToInt(player.position.z / tileSize)
        );
    }

    void ShiftWaterTiles()
    {
        for (int x = -2; x <= 2; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                Vector3 newPos = new Vector3(
                    (currentGridPos.x + x) * tileSize,
                    offsetSurface, 
                    (currentGridPos.y + z) * tileSize
                );

                // อัปเดตตำแหน่งแผ่นน้ำเดิมให้วาร์ปมาวางรอบๆ ผู้เล่น
                waterTiles[x + 2, z + 2].transform.position = newPos;
            }
        }
    }
}