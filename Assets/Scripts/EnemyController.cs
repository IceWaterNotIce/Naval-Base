using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float speed = 5f; // Movement speed
    public Transform target; // Target for the enemy to move towards
    public int health = 3; // Enemy health
    public int maxHealth = 3; // Maximum health
    public Slider healthSlider; // Reference to the UI Slider element for health display
    public Text healthText; // Reference to the UI Text element for health display
    public Canvas healthCanvas; // Reference to the Canvas containing the health UI

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize target if needed
        if (target == null)
        {
            target = GameObject.FindWithTag("NavalBase").transform; // Assuming NavalBase has a tag
        }

        SetCanvasEventCamera(); // Set the event camera for the canvas
        UpdateHealthUI(); // Initialize health display
    }

    // Update is called once per frame
    void Update()
    {
        MoveTowardsTarget();
        UpdateHealthUIPosition(); // Ensure the UI follows the enemy's position
    }

    private void MoveTowardsTarget()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime; // Move enemy towards target

            // Rotate enemy to face the target
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage; // Reduce health by the damage amount
        health = Mathf.Clamp(health, 0, maxHealth); // Ensure health does not go below 0 or above maxHealth
        UpdateHealthUI(); // Update health display
        if (health <= 0)
        {
            Die(); // Handle enemy death
        }
    }

    private void UpdateHealthUI()
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

    private void UpdateHealthUIPosition()
    {
        if (healthCanvas != null)
        {
            healthCanvas.transform.rotation = Quaternion.identity; // Prevent rotation
        }
    }

    private void SetCanvasEventCamera()
    {
        if (healthCanvas != null && Camera.main != null)
        {
            healthCanvas.worldCamera = Camera.main; // Set the event camera for the canvas
        }
    }

    private void Die()
    {
        Destroy(gameObject); // Destroy the enemy
    }
}

