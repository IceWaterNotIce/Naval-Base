using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AmmoManager ammoManager; // Reference to AmmoManager
    public EnemyManager enemyManager; // Reference to EnemyManager
    public NavalBaseController navalBaseController; // Reference to NavalBaseController

    public Text gameTimeText; // Reference to the UI Text element for game time display

    private List<Vector3> savedAmmoPositions = new List<Vector3>();
    private List<string> savedAmmoTargetTags = new List<string>(); // Save ammo target tags
    private List<Vector3> savedAmmoDirections = new List<Vector3>(); // Save ammo directions
    private List<Vector3> savedEnemyPositions = new List<Vector3>();
    private List<int> savedEnemyHealth = new List<int>(); // Save enemy health
    private List<int> savedEnemyPrefabIndices = new List<int>(); // Save enemy prefab indices
    private int savedGold;
    private int savedNavalBaseHealth; // Save naval base health
    private int savedNavalBaseMaxHealth; // Save naval base max health

    private string saveFilePath;
    private float gameTime; // Track the elapsed game time

    void Start()
    {
        saveFilePath = Path.Combine(Application.streamingAssetsPath, "GameData.json");
        LoadGame(); // Load game data when the game starts
    }

    void OnApplicationQuit()
    {
        SaveGame(); // Save game data when the application quits
    }

    void Update()
    {
        gameTime += Time.deltaTime; // Increment game time
        UpdateGameTimeUI(); // Update the game time UI
    }

    public void SaveGame()
    {
        // Save ammo positions, target tags, and directions
        savedAmmoPositions.Clear();
        savedAmmoTargetTags.Clear();
        savedAmmoDirections.Clear();
        foreach (GameObject ammo in ammoManager.GetActiveAmmo())
        {
            if (ammo != null)
            {
                savedAmmoPositions.Add(ammo.transform.position);
                Ammo ammoScript = ammo.GetComponent<Ammo>();
                if (ammoScript != null)
                {
                    savedAmmoTargetTags.Add(ammoScript.targetTag);
                    savedAmmoDirections.Add(ammoScript.GetDirection());
                }
            }
        }

        // Save enemy positions, health, and prefab indices
        savedEnemyPositions.Clear();
        savedEnemyHealth.Clear();
        savedEnemyPrefabIndices.Clear();
        foreach (GameObject enemy in enemyManager.GetActiveEnemies())
        {
            if (enemy != null)
            {
                savedEnemyPositions.Add(enemy.transform.position);
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    savedEnemyHealth.Add(enemyScript.health);
                }

                int prefabIndex = enemyManager.enemyPrefabs.IndexOf(enemyScript.gameObject);
                savedEnemyPrefabIndices.Add(prefabIndex >= 0 ? prefabIndex : 0); // Save prefab index, default to 0 if not found
            }
        }

        // Save naval base gold and health
        savedGold = navalBaseController.gold;
        savedNavalBaseHealth = navalBaseController.health;
        savedNavalBaseMaxHealth = navalBaseController.maxHealth; // Save max health

        // Create a save object
        GameData saveData = new GameData
        {
            ammoPositions = savedAmmoPositions,
            ammoTargetTags = savedAmmoTargetTags,
            ammoDirections = savedAmmoDirections,
            enemyPositions = savedEnemyPositions,
            enemyHealth = savedEnemyHealth,
            enemyPrefabIndices = savedEnemyPrefabIndices,
            navalBaseGold = savedGold,
            navalBaseHealth = savedNavalBaseHealth,
            navalBaseMaxHealth = savedNavalBaseMaxHealth, // Save max health
            gameTime = gameTime // Save game time
        };

        // Serialize and save to file
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json); // Save JSON to file
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            // Read and deserialize the file
            string json = File.ReadAllText(saveFilePath);
            GameData loadedData = JsonUtility.FromJson<GameData>(json);

            // Clear current ammo and enemies
            ammoManager.ClearAmmo();
            enemyManager.ClearEnemies();

            // Reload ammo
            for (int i = 0; i < loadedData.ammoPositions.Count; i++)
            {
                GameObject ammo = Instantiate(ammoManager.ammoPrefab, loadedData.ammoPositions[i], Quaternion.identity);
                ammoManager.RegisterAmmo(ammo);

                Ammo ammoScript = ammo.GetComponent<Ammo>();
                if (ammoScript != null && i < loadedData.ammoTargetTags.Count && i < loadedData.ammoDirections.Count)
                {
                    ammoScript.targetTag = loadedData.ammoTargetTags[i];
                    ammoScript.SetDirection(loadedData.ammoDirections[i]);
                }
            }

            // Reload enemies and their health
            for (int i = 0; i < loadedData.enemyPositions.Count; i++)
            {
                Vector3 position = loadedData.enemyPositions[i];
                int prefabIndex = i < loadedData.enemyPrefabIndices.Count ? loadedData.enemyPrefabIndices[i] : 0; // Default to index 0 if no data
                GameObject enemyPrefab = enemyManager.enemyPrefabs[Mathf.Clamp(prefabIndex, 0, enemyManager.enemyPrefabs.Count - 1)];
                GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
                enemyManager.RegisterEnemy(enemy);

                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null && i < loadedData.enemyHealth.Count)
                {
                    enemyScript.health = loadedData.enemyHealth[i];
                    enemyScript.UpdateHealthUI(); // Update health UI
                }
            }

            // Reload naval base gold and health
            navalBaseController.gold = loadedData.navalBaseGold;
            navalBaseController.health = loadedData.navalBaseHealth;
            navalBaseController.maxHealth = loadedData.navalBaseMaxHealth; // Reload max health
            navalBaseController.UpdateGoldUI(); // Update gold UI
            navalBaseController.UpdateHealthUI(); // Update health UI

            // Reload game time
            gameTime = loadedData.gameTime;
        }
    }

    private void UpdateGameTimeUI()
    {
        if (gameTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            gameTimeText.text = $"Time: {minutes:D2}:{seconds:D2}"; // Format as MM:SS
        }
    }

    public void GoBackToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // Load the specified scene
    }
}

[System.Serializable]
public class GameData
{
    public List<Vector3> ammoPositions;
    public List<string> ammoTargetTags; // Save ammo target tags
    public List<Vector3> ammoDirections; // Save ammo directions
    public List<Vector3> enemyPositions;
    public List<int> enemyHealth; // Save enemy health
    public List<int> enemyPrefabIndices; // Save enemy prefab indices
    public int navalBaseGold; // Save naval base gold
    public int navalBaseHealth; // Save naval base health
    public int navalBaseMaxHealth; // Save naval base max health
    public float gameTime; // Save game time
}
