using UnityEngine;

public class Ammo : MonoBehaviour
{
    private Vector3 targetPosition; // Store the target's position
    public float speed = 10f;
    public float maxDistance = 20f; // Maximum distance ammo can travel
    public int damage = 1; // Damage dealt by the ammo
    public string targetTag; // Store the target's tag

    private Vector3 startPosition; // Starting position of the ammo
    private Vector3 direction; // Direction toward the target

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position; // Record the starting position
        if (targetPosition != Vector3.zero)
        {
            direction = (targetPosition - startPosition).normalized; // Calculate direction once
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (direction != Vector3.zero)
        {
            transform.position += direction * speed * Time.deltaTime; // Move ammo toward the stored direction
        }

        // Check if ammo has moved too far
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject); // Destroy the ammo
        }
    }

    public void SetTarget(Transform enemyTarget, string tag)
    {
        if (enemyTarget != null)
        {
            targetPosition = enemyTarget.position; // Store the enemy's position once
            targetTag = tag; // Store the target's tag
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag)) // Check if collided object matches the target tag
        {
            if (targetTag == "Enemy")
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage); // Deal damage to the enemy
                }
            }
            else if (targetTag == "NavalBase")
            {
                NavalBaseController navalBase = collision.GetComponent<NavalBaseController>();
                if (navalBase != null)
                {
                    navalBase.TakeDamage(damage); // Deal damage to the naval base
                }
            }
            Destroy(gameObject); // Destroy the ammo
        }
    }

    public Vector3 GetDirection()
    {
        return direction; // Return the current direction
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized; // Set the direction
    }
}
