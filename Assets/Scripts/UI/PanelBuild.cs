using UnityEngine;

public class PanelBuild : PopupPanel
{
    // 這裡可以加入建造面板專屬的欄位與方法

    protected override void Awake()
    {
        base.Awake();
        // 可在此初始化建造面板相關內容
    }

    public override void Show()
    {
        base.Show();
        // 顯示建造面板時的額外操作
    }

    public override void Hide()
    {
        base.Hide();
        // 隱藏建造面板時的額外操作
    }
}
