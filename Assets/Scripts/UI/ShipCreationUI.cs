using UnityEngine;
using UnityEngine.UI;

public class ShipCreationUI : MonoBehaviour
{
    public Button CreateButton;
    public Button FormBattleLineButton; // 新增按鈕
    public PlayerShipManager ShipManager;
    public NavalBaseController NavalBaseController; // 引用 NavalBaseController
    public Text ErrorText; // 顯示錯誤訊息的 UI
    public Text shipInfoText; // 新增：顯示船隻資訊的文字

    private void Start()
    {
        CreateButton.onClick.AddListener(OnCreateButtonClicked);
        FormBattleLineButton.onClick.AddListener(OnFormBattleLineButtonClicked); // 綁定按鈕事件
        UpdateGoldText();
    }

    private void OnCreateButtonClicked()
    {
        bool success = ShipManager.CreateShipFromPrefab(); // 使用 NavalBaseController 的位置作為船隻創建位置
        if (!success)
        {
            ErrorText.text = "金幣不足，無法創建船隻！";
        }
        else
        {
            ErrorText.text = ""; // 清除錯誤訊息
            UpdateGoldText();
            UpdateShipInfoText(); // 更新船隻資訊顯示
        }
    }

    private void OnFormBattleLineButtonClicked()
    {
        Vector3 startPosition = Vector3.zero; // 起始位置
        Vector3 direction = Vector3.right; // 排列方向
        float spacing = 2f; // 間距
        ShipManager.FormBattleLine(startPosition, direction, spacing); // 呼叫排列方法
    }

    private void UpdateGoldText()
    {
        NavalBaseController.UpdateGoldUI(); // 通過 NavalBaseController 更新金幣顯示
    }

    private void UpdateShipInfoText()
    {
        if (ShipManager.playerShips.Count > 0)
        {
            PlayerShip lastCreatedShip = ShipManager.playerShips[^1].GetComponent<PlayerShip>();
            if (lastCreatedShip != null)
            {
                shipInfoText.text = $"Name: {lastCreatedShip.ShipName}\nSpeed: {lastCreatedShip.maxSpeed}\nRotation Speed: {lastCreatedShip.maxRotateSpeed}"; // 顯示最後創建的船隻資訊
            }
        }
    }
}