using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    public Camera minimapCamera; // 小地圖攝影機
    public float zoomStep = 5f; // 每次縮放的單位
    public float minSize = 10f; // 最小縮放
    public float maxSize = 100f; // 最大縮放

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

    /// <summary>
    /// 將小地圖的 UV 座標 (0~1) 轉換為世界座標
    /// </summary>
    public Vector2 GetWorldPositionFromUV(Vector2 uv)
    {
        if (minimapCamera == null)
            return Vector2.zero;

        // 以相機中心與正交 size 計算世界範圍
        float size = minimapCamera.orthographicSize;
        float aspect = minimapCamera.aspect;
        Vector2 center = new Vector2(minimapCamera.transform.position.x, minimapCamera.transform.position.y);
        float halfWidth = size * aspect;
        float halfHeight = size;

        Vector2 min = center - new Vector2(halfWidth, halfHeight);
        Vector2 max = center + new Vector2(halfWidth, halfHeight);

        float worldX = Mathf.Lerp(min.x, max.x, uv.x);
        float worldY = Mathf.Lerp(min.y, max.y, uv.y);
        return new Vector2(worldX, worldY);
    }
}