using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Add this line for Tilemap support

public class EnemyManager : MonoBehaviour
{
    private List<GameObject> activeEnemies = new List<GameObject>();

    public List<GameObject> enemyPrefabs; // List of enemy prefabs
    public Transform navalBase; // Reference to the naval base
    public float minSpawnDistance = 5f; // Minimum distance from the naval base
    public float maxSpawnDistance = 15f; // Maximum distance from the naval base
    public float spawnInterval = 5f; // Interval between enemy spawns
    public Tilemap oceanTileMap; // Reference to the Ocean Tilemap
    public Tilemap landTileMap; // Reference to the Land Tilemap

    private float spawnTimer;

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
        if (enemyPrefabs.Count > 0 && oceanTileMap != null && landTileMap != null)
        {
            for (int attempt = 0; attempt < 10; attempt++) // Try up to 10 times to find a valid spawn position
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized; // Random direction
                float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance); // Random distance within range
                Vector3 spawnPosition = navalBase.position + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;

                Vector3Int tilePosition = oceanTileMap.WorldToCell(spawnPosition);

                // Ensure the spawn position is in an ocean tile and not on a land tile
                if (oceanTileMap.GetTile(tilePosition) != null && landTileMap.GetTile(tilePosition) == null)
                {
                    GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)]; // Select a random enemy prefab
                    GameObject enemy = Instantiate(randomEnemyPrefab, spawnPosition, Quaternion.identity, transform); // Set as child of EnemyManager
                    RegisterEnemy(enemy);
                    return; // Exit after successfully spawning an enemy
                }
            }

            Debug.LogWarning("Failed to find a valid ocean tile for enemy spawn after 10 attempts.");
        }
    }
}
