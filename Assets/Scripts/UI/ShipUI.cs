using UnityEngine;

public class ShipUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas WorldSpaceCanvas;
  

    private Ship m_linkedShip;
    public Ship LinkedShip
    {
        get => m_linkedShip;
        set
        {
            m_linkedShip = value;
            if (m_linkedShip != null)
            {
                SetCanvasEventCamera();
            }
        }
    }

    private void Awake()
    {
        if (WorldSpaceCanvas == null)
        {
            WorldSpaceCanvas = GetComponent<Canvas>();
        }
    }

    public void Initialize(Ship ship)
    {
        m_linkedShip = ship;
        SetCanvasEventCamera();
    }

    private void Update()
    {
        if (m_linkedShip != null)
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
    }

    public void SetCanvasEventCamera()
    {
        if (WorldSpaceCanvas != null && Camera.main != null)
        {
            WorldSpaceCanvas.worldCamera = Camera.main;
        }
    }
}