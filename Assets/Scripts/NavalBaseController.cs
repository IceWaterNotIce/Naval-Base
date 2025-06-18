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
        }
    }

    public void AddGold(int amount) // Ensure this method is public
    {
        gold += amount; // Increase gold by the specified amount
        UpdateGoldUI(); // Update the UI whenever gold changes
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {gold}"; // Update the text element
        }
    }
}