using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<GameObject> activeEnemies = new List<GameObject>();

    public GameObject enemyPrefab; // Prefab for spawning enemies
    public Transform spawnPoint; // Point where enemies are spawned
    public float spawnInterval = 5f; // Interval between enemy spawns

    private float spawnTimer;

    public Transform navalBase; // Reference to the naval base
    public float minSpawnDistance = 5f; // Minimum distance from the naval base
    public float maxSpawnDistance = 15f; // Maximum distance from the naval base

    public void RegisterEnemy(GameObject enemy)
    {
        activeEnemies.Add(enemy);
    }

    public List<GameObject> GetActiveEnemies() // Ensure this method is public
    {
        return activeEnemies; // Return the list of active enemies
    }

    public void ClearEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy); // Destroy all active enemies
            }
        }
        activeEnemies.Clear(); // Clear the list
    }

    public List<GameObject> GetEnemiesInRange(Vector3 position, float radius)
    {
        List<GameObject> enemiesInRange = new List<GameObject>();
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null && Vector3.Distance(position, enemy.transform.position) <= radius)
            {
                enemiesInRange.Add(enemy); // Ensure valid enemies are added
            }
        }
        return enemiesInRange;
    }

    public void CleanupEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i); // Ensure null enemies are removed
            }
        }
    }

    void Update()
    {
        CleanupEnemies();
        HandleEnemySpawning();
    }

    private void HandleEnemySpawning()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized; // Random direction
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance); // Random distance within range
        Vector3 spawnPosition = navalBase.position + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        RegisterEnemy(enemy);
    }
}
