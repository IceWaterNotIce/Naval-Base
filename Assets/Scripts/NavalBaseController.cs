using UnityEngine;
using UnityEngine.UI;

public class NavalBaseController : MonoBehaviour
{
    public GameObject ammoPrefab; // Prefab for ammo
    public Transform firePoint; // Point where ammo is fired
    public float fireInterval = 0.2f; // Interval between shots
    public LayerMask enemyLayer; // Layer for detecting enemies
    public float detectionRadius = 10f; // Radius to detect enemies
    public int gold = 0; // Naval base gold
    public Text goldText; // Reference to the UI Text element
    public int health = 100; // Naval base health
    public int maxHealth = 100; // Maximum health
    public Text healthText; // Reference to the UI Text element for health display
    public Slider healthSlider; // Reference to the UI Slider element for health display
    public AmmoManager ammoManager; // Reference to AmmoManager

    private float fireTimer;

    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            DetectAndShootEnemies(); // Ensure this method is called correctly
            fireTimer = 0f;
        }
        UpdateGoldUI(); // Update the gold UI
        UpdateHealthUI(); // Update the health UI
    }

    private void DetectAndShootEnemies()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        foreach (Collider2D enemy in enemies)
        {
            if (enemy != null) // Ensure enemy is valid before shooting
            {
                ShootAmmoAtEnemy(enemy.transform);
            }
        }
    }

    private void ShootAmmoAtEnemy(Transform enemy)
    {
        GameObject ammo = Instantiate(ammoPrefab, firePoint.position, firePoint.rotation);
        if (ammo.TryGetComponent<Ammo>(out Ammo ammoScript)) // Ensure Ammo component exists
        {
            ammoScript.SetTarget(enemy);
            ammoManager.RegisterAmmo(ammo); // Register the ammo with AmmoManager
        }
    }

    public void AddGold(int amount) // Ensure this method is public
    {
        gold += amount; // Increase gold by the specified amount
        UpdateGoldUI(); // Update the UI whenever gold changes
    }

    public void TakeDamage(int damage)
    {
        health -= damage; // Reduce health by the damage amount
        health = Mathf.Clamp(health, 0, maxHealth); // Ensure health does not go below 0 or above maxHealth
        UpdateHealthUI(); // Update the health UI
        if (health <= 0)
        {
            HandleDestruction(); // Handle naval base destruction
        }
    }

    public void UpdateGoldUI() // Change to public so it can be accessed externally
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {gold}"; // Update the text element
        }
    }

    public void UpdateHealthUI() // Change to public so it can be accessed externally
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)health / maxHealth; // Update the slider value
        }
        if (healthText != null)
        {
            healthText.text = $"{health}/{maxHealth}"; // Update the health text
        }
    }

    private void HandleDestruction()
    {
        Debug.Log("Naval Base Destroyed!"); // Log destruction
        // Add additional logic for game over or naval base destruction
    }
}