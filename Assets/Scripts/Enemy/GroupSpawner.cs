using UnityEngine;
using System.Collections.Generic;


public class GroupSpawner : MonoBehaviour
{
    public GameObject enemyPrefabs;
    public int maxEnemies = 10;
    public float spawnInterval = 3f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private float timer;

    // Update is called once per frame
    void Update()
    {
        activeEnemies.RemoveAll(item => item == null);

        timer += Time.deltaTime;

        if (timer >= spawnInterval && activeEnemies.Count < maxEnemies)
        {
            SpawnAtRandomPoint();
            timer = 0;
        }
    }

    void SpawnAtRandomPoint()
    {
        int childCount = transform.childCount;

        if (childCount == 0)
        {
            Debug.LogWarning("No spawn point in Manager");
            return;
        }

        int randomIndex = Random.Range(0, childCount);

        Transform selectedPoint = transform.GetChild(randomIndex);

        GameObject newEnemy = Instantiate(enemyPrefabs, selectedPoint.position, selectedPoint.rotation);

        activeEnemies.Add(newEnemy);
    }

}
