using UnityEngine;
using UnityEngine.UI;

public class PlayerShipControlUI : MonoBehaviour
{
    public GameObject controlPanel; // The UI panel for controlling the ship
    public Text shipNameText; // 新增：顯示船隻名稱的文字
    public Text maxSpeedText; // 新增：顯示最大速度的文字
    public Text maxRotationSpeedText; // 新增：顯示最大旋轉速度的文字
    public Text attackDamageText; // 新增：顯示攻擊傷害的文字
    public Text attackIntervalText; // 新增：顯示攻擊間隔的文字
    public Text levelText; // 新增：顯示等級的文字
    public Button fullSpeedButton, threeQuarterSpeedButton, halfSpeedButton, quarterSpeedButton, stopButton;
    public Button rotateLeftButton, rotateHalfLeftButton, rotateNoneButton, rotateHalfRightButton, rotateRightButton;
    public Button DeselectShipButton; // New button for deselecting the ship

    [Header("Camera Follow")]
    public Camera mainCamera;
    public float followSize = 10f; // 可在 Inspector 設定
    public float cameraFollowSmoothTime = 0.3f;

    // 新增：取消相機跟隨按鈕
    public Button cancelFollowButton;

    private PlayerShip selectedShip;
    private Vector3 cameraVelocity = Vector3.zero;
    private bool isFollowingShip = false;

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

        // 新增：綁定取消相機跟隨按鈕
        if (cancelFollowButton != null)
        {
            cancelFollowButton.onClick.AddListener(CancelCameraFollow);
        }
    }

    void Update()
    {
        if (selectedShip != null)
        {
            if (!controlPanel.activeSelf)
            {
                controlPanel.SetActive(true); // Show the control panel when a ship is selected
            }
            UpdateShipInfo(); // 更新船隻資訊

            // 相機跟隨
            if (isFollowingShip && mainCamera != null)
            {
                // 禁用 MainCameraController 的平移，但允許縮放
                var camController = mainCamera.GetComponent<MainCameraController>();
                if (camController != null)
                {
                    // 關閉平移
                    camController.enabled = false;
                    // 允許縮放
                    float scroll = Input.GetAxis("Mouse ScrollWheel");
                    if (scroll != 0f)
                    {
                        mainCamera.orthographicSize = Mathf.Clamp(
                            mainCamera.orthographicSize - scroll * camController.zoomSpeed,
                            camController.minZoom,
                            camController.maxZoom
                        );
                    }
                }

                Vector3 targetPos = selectedShip.transform.position;
                Vector3 camTarget = new Vector3(targetPos.x, targetPos.y, mainCamera.transform.position.z);
                mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, camTarget, ref cameraVelocity, cameraFollowSmoothTime);

                // 平滑調整相機 size
                if (mainCamera.orthographic)
                {
                    mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, followSize, Time.deltaTime * 5f);
                }
            }
            else if (mainCamera != null)
            {
                // 若未跟隨，恢復 MainCameraController
                var camController = mainCamera.GetComponent<MainCameraController>();
                if (camController != null && !camController.enabled)
                    camController.enabled = true;
            }
        }
        else
        {
            if (controlPanel.activeSelf)
            {
                controlPanel.SetActive(false); // Hide the control panel when no ship is selected
            }
            isFollowingShip = false;

            // 確保恢復 MainCameraController
            if (mainCamera != null)
            {
                var camController = mainCamera.GetComponent<MainCameraController>();
                if (camController != null && !camController.enabled)
                    camController.enabled = true;
            }
        }
    }

    public void SelectShip(PlayerShip ship)
    {
        Debug.Log($"Ship {ship.name} selected."); // Debug log
        selectedShip = ship;
        controlPanel.SetActive(true); // Ensure the control panel is shown
        UpdateShipInfo(); // 更新船隻資訊

        // 啟動相機跟隨
        if (mainCamera == null)
            mainCamera = Camera.main;
        isFollowingShip = true;
    }

    public void DeselectShip()
    {
        Debug.Log("Ship deselected."); // Debug log
        selectedShip = null;
        controlPanel.SetActive(false); // Hide the control panel
        isFollowingShip = false;
    }

    // 新增：取消相機跟隨的方法
    public void CancelCameraFollow()
    {
        isFollowingShip = false;
        // 取消跟隨時恢復 MainCameraController
        if (mainCamera != null)
        {
            var camController = mainCamera.GetComponent<MainCameraController>();
            if (camController != null && !camController.enabled)
                camController.enabled = true;
        }
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
            selectedShip.targetRotationSpeed = selectedShip.maxRotateSpeed * rotationMultiplier;
        }
    }

    private void UpdateShipInfo()
    {
        if (selectedShip != null)
        {
            shipNameText.text = $"Name: {selectedShip.ShipName}";
            maxSpeedText.text = $"Max Speed: {selectedShip.maxSpeed}";
            maxRotationSpeedText.text = $"Max Rotation Speed: {selectedShip.maxRotateSpeed}";
            attackDamageText.text = $"Attack Damage: {selectedShip.attackDamage}";
            attackIntervalText.text = $"Attack Interval: {selectedShip.attackInterval}s";
            levelText.text = $"Level: {selectedShip.level}"; // 顯示等級
        }
    }
}
