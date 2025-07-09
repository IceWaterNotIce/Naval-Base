using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public float panSpeed = 20f; // 移動速度
    public float zoomSpeed = 5f; // 縮放速度
    public float minZoom = 5f; // 最小縮放
    public float maxZoom = 50f; // 最大縮放

    private Vector3 lastMousePosition;
    private Vector3 targetPosition;
    private bool isPanning = false;
    public float panSmoothTime = 0.15f; // 平滑時間
    private Vector3 panVelocity = Vector3.zero;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        HandlePan();
        HandleZoom();

        // 平滑移動到 targetPosition
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref panVelocity, panSmoothTime);
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(2)) // 中鍵按下
        {
            lastMousePosition = Input.mousePosition;
            isPanning = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        if (Input.GetMouseButton(2) && isPanning) // 中鍵拖動
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Camera camera = GetComponent<Camera>();
            float scale = camera != null ? camera.orthographicSize / 10f : 1f; // 根據相機縮放調整移動量
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * scale * Time.deltaTime;
            targetPosition += move;
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Camera camera = GetComponent<Camera>();
            if (camera != null)
            {
                camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }
    }
}
