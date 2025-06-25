using UnityEngine;

public class MinimapController : MonoBehaviour
{
    public Transform player; // Reference to the player's transform

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y; // Keep the camera's height constant
            newPosition.x = transform.position.x; // Keep the camera's x position constant
            transform.position = newPosition;
        }
    }
}
