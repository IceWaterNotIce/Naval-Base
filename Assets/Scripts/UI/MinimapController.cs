using UnityEngine;

public class MinimapController : MonoBehaviour
{
    public Transform player; // Reference to the player's transform

    void LateUpdate()
    {
        if (player != null)
        {
           
            transform.position = player.position; // Set the minimap position to the player's position

        }
    }
}
