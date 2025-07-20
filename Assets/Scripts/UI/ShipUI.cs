using UnityEngine;

public class ShipUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas WorldSpaceCanvas;
    public Canvas StaticCanvas;
    private Ship LinkedShip;

    private void Awake()
    {
        FindShipInParent();
    }

    private void FindShipInParent()
    {
        LinkedShip = GetComponentInParent<Ship>();
        if (LinkedShip == null)
        {
            Debug.LogWarning("No Ship component found in parent hierarchy.");
        }
    }

    protected virtual void Update()
    {
        if (LinkedShip != null)
        {
            UpdateUIPosition();
        }
    }

    public void UpdateUIPosition()
    {
        if (WorldSpaceCanvas != null)
        {
            WorldSpaceCanvas.transform.rotation = Quaternion.identity;
        }

        if (StaticCanvas != null)
        {
            // Set the position of StaticCanvas to the world space position without linkship transform
            StaticCanvas.transform.position = Vector3.zero; // Corrected to Vector3.zero
            StaticCanvas.transform.rotation = Quaternion.identity; // Maintain fixed rotation
        }
    }
}