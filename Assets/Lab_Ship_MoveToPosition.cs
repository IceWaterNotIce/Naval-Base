using UnityEngine;

public class Lab_Ship_MoveToPosition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //find the ship in the scene
        Ship ship = FindObjectOfType<Ship>();
        if (ship != null)
        {
            // Move the ship to a specific position
            Vector2 targetPosition = new Vector2(5, 0); // Example target position
            ship.MoveToPosition(targetPosition);
        }
        else
        {
            Debug.LogWarning("No Ship component found in the scene.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 start = new Vector3(5, -10, 0); // Start point of the line
        Vector3 end = new Vector3(5, 10, 0);   // End point of the line
        Gizmos.DrawLine(start, end);
    }
}
