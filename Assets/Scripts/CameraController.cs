using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f; // 移動速度
    public float zoomSpeed = 5f; // 縮放速度
    public float minZoom = 5f; // 最小縮放
    public float maxZoom = 50f; // 最大縮放

    private Vector3 lastMousePosition;

    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(2)) // 中鍵按下
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2)) // 中鍵拖動
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
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
