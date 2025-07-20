using UnityEngine;

public class PlayerShipUI : ShipUI
{
    public PlayerShipControlUI controlUI;
    public PlayerShipDetailUI detailUI;
    
    private PlayerShip selectedShip;
    private Camera mainCamera;
    
    [Header("Camera Follow")]
    public float followSize = 10f;
    public float cameraFollowSmoothTime = 0.3f;
    private Vector3 cameraVelocity = Vector3.zero;
    private bool isFollowingShip = false;

    void Start()
    {
        mainCamera = Camera.main;
        controlUI.gameObject.SetActive(false);
        detailUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (selectedShip != null)
        {
            UpdateCameraFollow();
        }
    }

    public void SelectShip(PlayerShip ship)
    {
        selectedShip = ship;
        controlUI.gameObject.SetActive(true);
        detailUI.gameObject.SetActive(true);
        
        controlUI.SetShip(ship);
        detailUI.SetShip(ship);
        
        isFollowingShip = true;
    }

    public void DeselectShip()
    {
        selectedShip = null;
        controlUI.gameObject.SetActive(false);
        detailUI.gameObject.SetActive(false);
        isFollowingShip = false;
    }

    public void CancelCameraFollow()
    {
        isFollowingShip = false;
    }

    private void UpdateCameraFollow()
    {
        if (!isFollowingShip || mainCamera == null) return;

        var camController = mainCamera.GetComponent<MainCameraController>();
        if (camController != null)
        {
            camController.enabled = false;
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
        mainCamera.transform.position = Vector3.SmoothDamp(
            mainCamera.transform.position, 
            camTarget, 
            ref cameraVelocity, 
            cameraFollowSmoothTime
        );

        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize, 
                followSize, 
                Time.deltaTime * 5f
            );
        }
    }
}