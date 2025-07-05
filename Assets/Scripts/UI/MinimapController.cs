using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Camera minimapCamera; // 指定小地圖相機
    public float zoomStep = 5f; // 每次縮放的單位
    public float minSize = 10f; // 最小縮放
    public float maxSize = 100f; // 最大縮放
    public Button zoomInButton; // 縮小按鈕
    public Button zoomOutButton; // 放大按鈕

    void Start()
    {
        // 綁定按鈕事件（可於 Inspector 設定，這裡保險程式碼也加上）
        if (zoomInButton != null)
            zoomInButton.onClick.AddListener(ZoomIn);
        if (zoomOutButton != null)
            zoomOutButton.onClick.AddListener(ZoomOut);
    }

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position; // Set the minimap position to the player's position
        }
    }

    public void ZoomIn()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = Mathf.Max(minimapCamera.orthographicSize - zoomStep, minSize);
        }
    }

    public void ZoomOut()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = Mathf.Min(minimapCamera.orthographicSize + zoomStep, maxSize);
        }
    }
}
