using UnityEngine;
using UnityEngine.UI;

public class ShipCreationUI : MonoBehaviour
{
    public Button CreateButton;
    public ShipManager ShipManager;
    public NavalBaseController NavalBaseController; // 引用 NavalBaseController
    public Text ErrorText; // 顯示錯誤訊息的 UI

    private void Start()
    {
        CreateButton.onClick.AddListener(OnCreateButtonClicked);
        UpdateGoldText();
    }

    private void OnCreateButtonClicked()
    {
        bool success = ShipManager.CreateShipFromPrefab(Vector3.zero);
        if (!success)
        {
            ErrorText.text = "金幣不足，無法創建船隻！";
        }
        else
        {
            ErrorText.text = ""; // 清除錯誤訊息
            UpdateGoldText();
        }
    }

    private void UpdateGoldText()
    {
        NavalBaseController.UpdateGoldUI(); // 通過 NavalBaseController 更新金幣顯示
    }
}