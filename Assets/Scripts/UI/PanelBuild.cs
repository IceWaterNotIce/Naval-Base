using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PanelBuild : PopupPanel
{
    // 用 List 管理多個子面板
    [SerializeField] private List<GameObject> childPanels = new List<GameObject>();

    // 當前開啟的子面板
    private GameObject currentChildPanel;

    // ScrollView 物件
    [SerializeField] private GameObject scrollView;

    protected override void Awake()
    {
        base.Awake();
        // 預設隱藏所有子面板
        foreach (var panel in childPanels)
        {
            if (panel) panel.SetActive(false);
        }
        currentChildPanel = null;

        // 自動為 ScrollView 下的所有 Button 綁定事件
        if (scrollView != null)
        {
            var buttons = scrollView.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length && i < childPanels.Count; i++)
            {
                int idx = i; // 避免閉包問題
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() => OnClickChildPanelBtn(idx));
            }
        }
    }

    public override void Show()
    {
        base.Show();
        // 顯示建造面板時的額外操作
    }

    public override void Hide()
    {
        base.Hide();
        // 隱藏所有子面板
        foreach (var panel in childPanels)
        {
            if (panel) panel.SetActive(false);
        }
        currentChildPanel = null;
    }

    // 供 ScrollView 按鈕呼叫的方法
    public void OnClickChildPanelBtn(int index)
    {
        if (index < 0 || index >= childPanels.Count) return;
        GameObject targetPanel = childPanels[index];

        if (targetPanel == null) return;

        // 如果已經開啟則關閉，否則切換
        if (currentChildPanel == targetPanel)
        {
            targetPanel.SetActive(false);
            currentChildPanel = null;
        }
        else
        {
            if (currentChildPanel != null)
                currentChildPanel.SetActive(false);
            targetPanel.SetActive(true);
            currentChildPanel = targetPanel;
        }
    }
}
