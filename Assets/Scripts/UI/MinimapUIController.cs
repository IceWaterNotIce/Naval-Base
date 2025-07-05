using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapUIController : MonoBehaviour
{
    public RawImage minimapImage; // 小地圖 RawImage
    public Button zoomInButton;   // 縮小按鈕
    public Button zoomOutButton;  // 放大按鈕
    public Camera mainCamera;     // 主攝影機

    private MinimapCameraController minimapCameraController;

    void Start()
    {
        // 取得或建立 MinimapCameraController
        minimapCameraController = FindFirstObjectByType<MinimapCameraController>();
        if (minimapCameraController == null)
        {
            Debug.LogError("MinimapCameraController not found in scene!");
            return;
        }

        // 綁定按鈕事件
        if (zoomInButton != null)
            zoomInButton.onClick.AddListener(minimapCameraController.ZoomIn);
        if (zoomOutButton != null)
            zoomOutButton.onClick.AddListener(minimapCameraController.ZoomOut);

        // 綁定小地圖點擊事件
        if (minimapImage != null)
        {
            EventTrigger trigger = minimapImage.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = minimapImage.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnMinimapClick((PointerEventData)data); });
            trigger.triggers.Add(entry);
        }
    }

    private void OnMinimapClick(PointerEventData eventData)
    {
        if (mainCamera == null || minimapImage == null || minimapCameraController == null)
            return;

        RectTransform rectTransform = minimapImage.rectTransform;
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        // 轉換 localCursor 為 0~1 範圍 (UV)
        Vector2 rectSize = rectTransform.rect.size;
        Vector2 uv = new Vector2(
            (localCursor.x + rectSize.x * 0.5f) / rectSize.x,
            (localCursor.y + rectSize.y * 0.5f) / rectSize.y
        );

        // 透過 MinimapCameraController 取得世界座標
        Vector2 worldPos = minimapCameraController.GetWorldPositionFromUV(uv);
        Vector3 newCamPos = new Vector3(worldPos.x, worldPos.y, mainCamera.transform.position.z);
        mainCamera.transform.position = newCamPos;
    }
}