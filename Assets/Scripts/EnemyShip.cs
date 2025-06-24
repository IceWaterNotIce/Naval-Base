using UnityEngine;
using UnityEngine.UI;

public class EnemyShip : Ship
{
    public float minDistanceToNavalBase = 3f; // Minimum distance to the naval base
    public float orbitSpeed = 1f; // Speed at which the enemy orbits the naval base
    public int goldReward = 1; // Gold rewarded when the enemy dies
    public Transform firePoint; // Point where ammo is fired

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Initialize(string name, float speed, float health)
    {
        base.Initialize(name, speed, health);
        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D component
        if (target == null)
        {
            target = GameObject.FindWithTag("NavalBase").transform; // Assuming NavalBase has a tag
        }
        UpdateHealthUI(); // Initialize health display
    }

    void Start()
    {
        if (target == null)
        {
            target = GameObject.FindWithTag("NavalBase").transform; // Assuming NavalBase has a tag
        }

        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D component
        SetCanvasEventCamera(); // Set the event camera for the canvas
        UpdateHealthUI(); // Initialize health display
    }

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
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > minDistanceToNavalBase && distance <= 5f) // Within attack range but not too close
            {
                OrbitAroundNavalBase(); // Move around the naval base
            }
            else if (distance > 5f) // Too far from the naval base
            {
                Vector3 direction = (target.position - transform.position).normalized;

                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);

                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
                rb.linearVelocity = transform.right * currentSpeed; // 使用 linearVelocity
            }
            else // Too close to the naval base
            {
                Vector3 direction = (transform.position - target.position).normalized;

                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);

                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
                rb.linearVelocity = transform.right * currentSpeed; // 使用 linearVelocity
            }
        }
    }

    private void OrbitAroundNavalBase()
    {
        if (target != null)
        {
            Vector3 direction = (transform.position - target.position).normalized;

            // Calculate orbit direction (perpendicular to the direction to the naval base)
            Vector3 orbitDirection = new Vector3(-direction.y, direction.x, 0).normalized;

            float targetAngle = Mathf.Atan2(orbitDirection.y, orbitDirection.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);

            rb.linearVelocity = transform.right * orbitSpeed; // 使用 linearVelocity
        }
    }

    private void HandleAttack()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= 5f) // Check if within attack range
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                ShootAmmoAtNavalBase(); // Shoot ammo at the naval base
                attackTimer = 0f; // Reset attack timer
            }
        }
    }

    private void ShootAmmoAtNavalBase()
    {
        if (ammoPrefab != null && firePoint != null)
        {
            GameObject ammo = Instantiate(ammoPrefab, firePoint.position, firePoint.rotation);
            if (ammo.TryGetComponent<Ammo>(out Ammo ammoScript)) // Ensure Ammo component exists
            {
                ammoScript.SetTarget(target, "NavalBase"); // Pass Transform and tag
                Debug.Log($"Ammo shot at NavalBase with targetTag: NavalBase"); // Debug log
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        UpdateHealthUI(); // Update health display
        if (Health <= 0)
        {
            RewardPlayer(); // Handle enemy death
        }
    }

    private void RewardPlayer()
    {
        NavalBaseController navalBase = GameObject.FindWithTag("NavalBase").GetComponent<NavalBaseController>();
        if (navalBase != null)
        {
            navalBase.AddGold(goldReward); // Add gold to the naval base
        }
    }

    public void UpdateHealthUI() // 將此方法設為 public
    {
        if (healthSlider != null)
        {
            healthSlider.value = Health / (float)maxHealth; // 更新滑塊值
        }
        if (healthText != null)
        {
            healthText.text = $"{Health}/{maxHealth}"; // 更新血量文字
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
}

