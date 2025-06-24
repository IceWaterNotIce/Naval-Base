using UnityEngine;
using UnityEngine.UI;

public class PlayerShipControlUI : MonoBehaviour
{
    public GameObject controlPanel; // The UI panel for controlling the ship
    public Button fullSpeedButton, threeQuarterSpeedButton, halfSpeedButton, quarterSpeedButton, stopButton;
    public Button rotateLeftButton, rotateHalfLeftButton, rotateNoneButton, rotateHalfRightButton, rotateRightButton;
    public Button DeselectShipButton; // New button for deselecting the ship

    private PlayerShip selectedShip;

    void Start()
    {
        controlPanel.SetActive(false); // Hide the control panel initially

        // Bind speed control buttons
        fullSpeedButton.onClick.AddListener(() => SetSpeed(1f));
        threeQuarterSpeedButton.onClick.AddListener(() => SetSpeed(0.75f));
        halfSpeedButton.onClick.AddListener(() => SetSpeed(0.5f));
        quarterSpeedButton.onClick.AddListener(() => SetSpeed(0.25f));
        stopButton.onClick.AddListener(() => SetSpeed(0f));

        // Bind rotation control buttons
        rotateLeftButton.onClick.AddListener(() => SetRotation(-1f));
        rotateHalfLeftButton.onClick.AddListener(() => SetRotation(-0.5f));
        rotateNoneButton.onClick.AddListener(() => SetRotation(0f));
        rotateHalfRightButton.onClick.AddListener(() => SetRotation(0.5f));
        rotateRightButton.onClick.AddListener(() => SetRotation(1f));

        // Bind deselect button
        DeselectShipButton.onClick.AddListener(DeselectShip);
    }

    void Update()
    {
        if (selectedShip != null)
        {
            if (!controlPanel.activeSelf)
            {
                controlPanel.SetActive(true); // Show the control panel when a ship is selected
            }
        }
        else
        {
            if (controlPanel.activeSelf)
            {
                controlPanel.SetActive(false); // Hide the control panel when no ship is selected
            }
        }
    }

    public void SelectShip(PlayerShip ship)
    {
        Debug.Log($"Ship {ship.name} selected."); // Debug log
        selectedShip = ship;
        controlPanel.SetActive(true); // Ensure the control panel is shown
    }

    public void DeselectShip()
    {
        Debug.Log("Ship deselected."); // Debug log
        selectedShip = null;
        controlPanel.SetActive(false); // Hide the control panel
    }

    private void SetSpeed(float speedMultiplier)
    {
        if (selectedShip != null)
        {
            selectedShip.targetSpeed = selectedShip.maxSpeed * speedMultiplier;
        }
    }

    private void SetRotation(float rotationMultiplier)
    {
        if (selectedShip != null)
        {
            selectedShip.targetRotationSpeed = selectedShip.maxSpeed * rotationMultiplier * 100f; // Adjust rotation speed
        }
    }
}
