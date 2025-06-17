using UnityEngine;

public class Ammo : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime; // Ensure ammo moves toward target
        }
    }

    public void SetTarget(Transform enemyTarget)
    {
        target = enemyTarget; // Ensure target is set correctly
    }

    private void OnTriggerEnter2D(Collider2D collision) // Change to OnTriggerEnter2D
    {
        if (collision.CompareTag("Enemy")) // Check if collided object is an enemy
        {
            NavalBaseController navalBase = FindObjectOfType<NavalBaseController>();
            if (navalBase != null)
            {
                navalBase.AddGold(1); // Ensure this method is called correctly
            }
            Destroy(collision.gameObject); // Destroy the enemy
            Destroy(gameObject); // Destroy the ammo
        }
    }
}
