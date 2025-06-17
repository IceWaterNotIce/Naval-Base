using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 5f; // Movement speed
    public Transform target; // Target for the enemy to move towards

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize target if needed
        if (target == null)
        {
            target = GameObject.FindWithTag("NavalBase").transform; // Assuming NavalBase has a tag
        }
    }

    // Update is called once per frame
    void Update()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime; // Move enemy towards target
        }
    }
}
