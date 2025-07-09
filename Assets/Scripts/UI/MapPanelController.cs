using UnityEngine;

public class MapPanelController : MonoBehaviour
{
    public GameObject mapPanel; // 參考地圖面板 UI

    void Start()
    {
        if (mapPanel != null)
        {
            mapPanel.SetActive(false);
        }
    }

    // 你可以根據需求添加顯示/隱藏地圖面板的方法
    public void ShowMapPanel()
    {
        if (mapPanel != null)
            mapPanel.SetActive(true);
    }

    public void HideMapPanel()
    {
        if (mapPanel != null)
            mapPanel.SetActive(false);
    }
}
