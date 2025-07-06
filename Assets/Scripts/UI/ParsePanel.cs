using UnityEngine;
using UnityEngine.UI;

public class ParsePanel : PopupPanel
{
    [Header("UI References")]


    // 新增：遊戲相關按鈕
    [Header("Game Control Buttons")]
    public Button parseGameButton;
    public Button continueGameButton;
    public Button quitGameButton;
    public Button startNewGameButton;

    private bool isExpanded = true;

    void Start()
    {

        // 新增：遊戲控制按鈕事件掛載（僅掛載，不實作）
        if (parseGameButton != null)
            parseGameButton.onClick.AddListener(OnParseGame);

        if (continueGameButton != null)
            continueGameButton.onClick.AddListener(OnContinueGame);

        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(OnQuitGame);

        if (startNewGameButton != null)
            startNewGameButton.onClick.AddListener(OnStartNewGame);
    }

    private void OnParseGame()
    {
        // TODO: 實作暫停遊戲
    }

    private void OnContinueGame()
    {
        // TODO: 實作繼續遊戲
    }

    private void OnQuitGame()
    {
        // TODO: 實作離開遊戲
    }

    private void OnStartNewGame()
    {
        // TODO: 實作開始新遊戲
    }
}
