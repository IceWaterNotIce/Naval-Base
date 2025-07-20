using UnityEngine;
using UnityEngine.UI;

public class PlayerShipControlUI : MonoBehaviour
{
    public Button fullSpeedButton, threeQuarterSpeedButton, halfSpeedButton, quarterSpeedButton, stopButton;
    public Button rotateLeftButton, rotateHalfLeftButton, rotateNoneButton, rotateHalfRightButton, rotateRightButton;
    public Button moveToPositionButton;
    public Button deselectShipButton;
    
    [Header("Move To Position")]
    public Image positionIcon;
    public Canvas shipUICanvas;
    public float iconDisplayDuration = 2f;
    
    private PlayerShip ship;
    private Coroutine iconDisplayCoroutine;

    void Start()
    {
        fullSpeedButton.onClick.AddListener(() => SetSpeed(1f));
        threeQuarterSpeedButton.onClick.AddListener(() => SetSpeed(0.75f));
        halfSpeedButton.onClick.AddListener(() => SetSpeed(0.5f));
        quarterSpeedButton.onClick.AddListener(() => SetSpeed(0.25f));
        stopButton.onClick.AddListener(() => SetSpeed(0f));

        rotateLeftButton.onClick.AddListener(() => SetRotation(-1f));
        rotateHalfLeftButton.onClick.AddListener(() => SetRotation(-0.5f));
        rotateNoneButton.onClick.AddListener(() => SetRotation(0f));
        rotateHalfRightButton.onClick.AddListener(() => SetRotation(0.5f));
        rotateRightButton.onClick.AddListener(() => SetRotation(1f));

        moveToPositionButton.onClick.AddListener(EnableMoveToPositionMode);
        
        if (positionIcon != null)
        {
            positionIcon.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (ship == null) return;

        if (Input.GetMouseButtonDown(0) && moveToPositionButton.interactable)
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0;

            Debug.Log($"Mouse Position: {mousePosition}");
            Debug.Log($"World Position: {worldPosition}");
            
            if (ship != null)
            {
                ship.MoveToPosition(worldPosition);
                ShowPositionIcon(worldPosition);
            }
        }
    }

    public void SetShip(PlayerShip newShip)
    {
        ship = newShip;
    }

    private void SetSpeed(float speedMultiplier)
    {
        if (ship != null)
        {
            ship.TargetSpeed = ship.maxSpeed * speedMultiplier;
        }
    }

    private void SetRotation(float rotationMultiplier)
    {
        if (ship != null)
        {
            ship.TargetRotationSpeed = ship.maxRotateSpeed * rotationMultiplier;
        }
    }

    private void EnableMoveToPositionMode()
    {
        Debug.Log("Move to position mode enabled.");
    }

    private void ShowPositionIcon(Vector3 worldPosition)
    {
        if (positionIcon == null || shipUICanvas == null) return;

        positionIcon.rectTransform.anchoredPosition = worldPosition;
        positionIcon.gameObject.SetActive(true);
    }
}