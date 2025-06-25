using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NavalBaseController : MonoBehaviour, IPointerClickHandler
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
    public TechTreeManager techTreeManager; // Reference to TechTreeManager
    public int level = 1; // Naval base level
    public int maxLevel = 10; // Maximum level
    public Text levelText; // Reference to the UI Text element for level display
    public int levelUpGoldCost = 100; // Gold cost for leveling up
    public GameObject detailPanel; // Reference to the detail panel UI
    public Text detailText; // Reference to the Text element in the detail panel
    public Button closeButton; // Reference to the close button in the detail panel
    public Button levelUpButton; // Reference to the level-up button in the detail panel

    private float fireTimer;

    void Start()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false); // Hide the detail panel at the start of the game
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => ToggleDetailPanel(false)); // Add listener to close button
        }

        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(LevelUp); // Add listener to level-up button
        }
        //SpriteOutlineHelper.AddMouseOverHighlight(gameObject); // Add mouse over highlight effect
    }

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
        UpdateLevelUI(); // Update the level UI
        UpdateDetailPanel(); // Update the detail panel
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
            Vector3 targetPos = enemy.position;

            if (techTreeManager != null && techTreeManager.basicShooting.isUnlocked)
            {
                Debug.Log($"Shooting at enemy {enemy.name} at position {targetPos}"); // Log enemy position for debugging

                if (techTreeManager.dynamicTracking.isUnlocked)
                {
                    Rigidbody2D enemyRB = enemy.GetComponent<Rigidbody2D>();
                    if (enemyRB != null && enemyRB.linearVelocity != Vector2.zero)
                    {
                        Vector2 enemyVelocity = enemyRB.linearVelocity; // Get enemy linearVelocity
                        float timeToTarget = Vector3.Distance(firePoint.position, enemy.position) / ammoScript.speed; // Predict time to target
                        targetPos += new Vector3(enemyVelocity.x, enemyVelocity.y, 0) * timeToTarget; // Predict target position
                    }
                    else
                    {
                        Debug.LogWarning($"Enemy {enemy.name} has zero linearVelocity or no Rigidbody2D component.");
                    }
                }

                if (techTreeManager.intelligentCorrection.isUnlocked)
                {
                    RaycastHit2D hit = Physics2D.Raycast(firePoint.position, (targetPos - firePoint.position).normalized);
                    if (hit.collider != null)
                    {
                        targetPos = new Vector3(hit.point.x, hit.point.y, 0) + Vector3.Reflect((targetPos - firePoint.position).normalized, new Vector3(hit.normal.x, hit.normal.y, 0)); // Adjust for obstacles
                    }
                }
            }

            ammoScript.SetTarget(targetPos, "Enemy");
            ammoManager.RegisterAmmo(ammo);
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

    public void LevelUp()
    {
        if (gold >= levelUpGoldCost && level < maxLevel)
        {
            gold -= levelUpGoldCost; // Deduct gold for leveling up
            level++; // Increase level
            levelUpGoldCost += 50; // Increase the cost for the next level
            maxHealth += 20; // Increase max health
            detectionRadius += 2f; // Increase detection radius
            health = maxHealth; // Restore health to max
            UpdateGoldUI(); // Update gold UI
            UpdateHealthUI(); // Update health UI
            UpdateLevelUI(); // Update level UI
            Debug.Log($"Naval Base leveled up to {level}!");
        }
        else
        {
            Debug.LogWarning("Not enough gold or already at max level!");
        }
    }

    public void UpdateLevelUI()
    {
        if (level < 1) level = 1; // Ensure level is at least 1
        if (levelText != null)
        {
            levelText.text = $"Level: {level}"; // Update the level text
        }
    }

    public void UpdateDetailPanel()
    {
        if (detailPanel != null && detailText != null)
        {
            detailText.text = $"Level: {level}\n" +
                              $"Gold: {gold}\n" +
                              $"Health: {health}/{maxHealth}\n" +
                              $"Detection Radius: {detectionRadius:F1}";
        }
    }

    public void ToggleDetailPanel(bool isVisible)
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(isVisible); // Show or hide the detail panel
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleDetailPanel(true); // Show the detail panel when the naval base is clicked
    }

    private void HandleDestruction()
    {
        Debug.Log("Naval Base Destroyed!"); // Log destruction
        // Add additional logic for game over or naval base destruction
    }
}