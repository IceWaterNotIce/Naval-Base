using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float maxSpeed = 2f; // Maximum movement speed
    public float acceleration = 0.5f; // Acceleration rate
    public float deceleration = 0.5f; // Deceleration rate
    public float rotationSpeed = 100f; // Rotation speed
    public Transform target; // Target for the enemy to move towards
    public int health = 5; // Enemy health
    public int maxHealth = 5; // Maximum health
    public Slider healthSlider; // Reference to the UI Slider element for health display
    public Text healthText; // Reference to the UI Text element for health display
    public Canvas healthCanvas; // Reference to the Canvas containing the health UI
    public int attackDamage = 1; // Damage dealt to the naval base
    public float attackInterval = 1f; // Interval between attacks
    private float attackTimer;

    private float currentSpeed = 0f; // Current movement speed

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
        HandleAttack(); // Handle attacking the naval base
    }

    private void MoveTowardsTarget()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;

            // Smooth rotation towards the target
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Adjust speed based on distance to the target
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > 0.5f) // Accelerate if far from the target
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
            }
            else // Decelerate if close to the target
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
            }

            // Move the enemy
            transform.position += transform.right * currentSpeed * Time.deltaTime;
        }
    }

    private void HandleAttack()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= 0.5f) // Check if close to the naval base
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                NavalBaseController navalBase = target.gameObject.GetComponent<NavalBaseController>();
                if (navalBase != null)
                {
                    navalBase.TakeDamage(attackDamage); // Deal damage to the naval base
                }
                attackTimer = 0f; // Reset attack timer
            }
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

