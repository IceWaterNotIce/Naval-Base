using UnityEngine;
using UnityEngine.UI;

public class PanelTechTree : PopupPanel
{
    public TechTreeManager techTreeManager;
    public Button basicShootingButton;
    public Button dynamicTrackingButton;
    public Button intelligentCorrectionButton;
    public Button closeTechTreeButton;

    private void Start()
    {
        UpdateUI();
        if (basicShootingButton != null)
            basicShootingButton.onClick.AddListener(OnBasicShootingClicked);
        if (dynamicTrackingButton != null)
            dynamicTrackingButton.onClick.AddListener(OnDynamicTrackingClicked);
        if (intelligentCorrectionButton != null)
            intelligentCorrectionButton.onClick.AddListener(OnIntelligentCorrectionClicked);
        if (closeTechTreeButton != null)
            closeTechTreeButton.onClick.AddListener(Hide);
    }

    private void OnBasicShootingClicked()
    {
        techTreeManager.UnlockBasicShooting();
        UpdateUI();
    }

    private void OnDynamicTrackingClicked()
    {
        techTreeManager.UnlockDynamicTracking();
        UpdateUI();
    }

    private void OnIntelligentCorrectionClicked()
    {
        techTreeManager.UnlockIntelligentCorrection();
        UpdateUI();
    }

    private void UpdateUI()
    {
        var navalBaseController = techTreeManager.navalBaseController;
        basicShootingButton.interactable = navalBaseController.gold >= 2 && !techTreeManager.basicShooting.isUnlocked;
        dynamicTrackingButton.interactable = navalBaseController.gold >= 3 && techTreeManager.basicShooting.isUnlocked && !techTreeManager.dynamicTracking.isUnlocked;
        intelligentCorrectionButton.interactable = navalBaseController.gold >= 4 && techTreeManager.dynamicTracking.isUnlocked && !techTreeManager.intelligentCorrection.isUnlocked;

        basicShootingButton.GetComponent<Image>().color = techTreeManager.basicShooting.isUnlocked ? Color.green : Color.white;
        dynamicTrackingButton.GetComponent<Image>().color = techTreeManager.dynamicTracking.isUnlocked ? Color.green : Color.white;
        intelligentCorrectionButton.GetComponent<Image>().color = techTreeManager.intelligentCorrection.isUnlocked ? Color.green : Color.white;
    }

    public override void Show()
    {
        base.Show();
        UpdateUI();
    }
}
