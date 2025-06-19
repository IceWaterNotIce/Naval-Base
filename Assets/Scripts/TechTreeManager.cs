using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PredictionTech {
    public bool isUnlocked = false;
}

public class TechTreeManager : MonoBehaviour {
    // 科技樹狀態
    public PredictionTech basicShooting = new PredictionTech();
    public PredictionTech dynamicTracking = new PredictionTech();
    public PredictionTech intelligentCorrection = new PredictionTech();
    
    public int techPoints = 0;
    public Button basicShootingButton;
    public Button dynamicTrackingButton;
    public Button intelligentCorrectionButton;

    private string savePath;

    void Start() {
        savePath = Path.Combine(Application.streamingAssetsPath, "TechTreeSave.json");
        LoadTechTree();
        UpdateUI();
    }

    //=== 科技解鎖方法 ===//
    public void UnlockBasicShooting() {
        if (techPoints >= 2 && !basicShooting.isUnlocked)
        {
            basicShooting.isUnlocked = true;
            techPoints -= 2;
            SaveTechTree();
            UpdateUI();
        }
    }

    public void UnlockDynamicTracking() {
        if (techPoints >= 3 && basicShooting.isUnlocked && !dynamicTracking.isUnlocked)
        {
            dynamicTracking.isUnlocked = true;
            techPoints -= 3;
            SaveTechTree();
            UpdateUI();
        }
    }

    public void UnlockIntelligentCorrection() {
        if(techPoints >= 4 && dynamicTracking.isUnlocked && !intelligentCorrection.isUnlocked) {
            intelligentCorrection.isUnlocked = true;
            techPoints -= 4;
            SaveTechTree();
            UpdateUI();
        }
    }

    //=== 存檔系統 ===//
    [System.Serializable]
    private class SaveData {
        public bool basicShooting;
        public bool dynamicTracking;
        public bool intelligentCorrection;
        public int savedTechPoints;
    }

    void SaveTechTree() {
        SaveData data = new SaveData {
            basicShooting = basicShooting.isUnlocked,
            dynamicTracking = dynamicTracking.isUnlocked,
            intelligentCorrection = intelligentCorrection.isUnlocked,
            savedTechPoints = techPoints
        };
        File.WriteAllText(savePath, JsonUtility.ToJson(data));
    }

    void LoadTechTree() {
        if(File.Exists(savePath)) {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
            basicShooting.isUnlocked = data.basicShooting;
            dynamicTracking.isUnlocked = data.dynamicTracking;
            intelligentCorrection.isUnlocked = data.intelligentCorrection;
            techPoints = data.savedTechPoints;
        }
    }

    //=== UI 更新 ===//
    void UpdateUI() {
        basicShootingButton.interactable = techPoints >= 2 && !basicShooting.isUnlocked;
        dynamicTrackingButton.interactable = techPoints >= 3 && basicShooting.isUnlocked && !dynamicTracking.isUnlocked;
        intelligentCorrectionButton.interactable = techPoints >= 4 && dynamicTracking.isUnlocked && !intelligentCorrection.isUnlocked;
        
        // 視覺化已解鎖狀態
        basicShootingButton.GetComponent<Image>().color = basicShooting.isUnlocked ? Color.green : Color.white;
        dynamicTrackingButton.GetComponent<Image>().color = dynamicTracking.isUnlocked ? Color.green : Color.white;
        intelligentCorrectionButton.GetComponent<Image>().color = intelligentCorrection.isUnlocked ? Color.green : Color.white;
    }

    //=== 射擊系統調用接口 ===//
    public bool IsPredictiveShootingEnabled() {
        return basicShooting.isUnlocked;
    }

    public Vector3 GetPredictedTargetPosition(Transform enemy, Vector3 firePosition, float projectileSpeed) {
        if(!basicShooting.isUnlocked) return enemy.position;

        Vector3 predictedPos = enemy.position;
        
        // 動態追蹤預測
        if(dynamicTracking.isUnlocked) {
            Rigidbody enemyRB = enemy.GetComponent<Rigidbody>();
            if(enemyRB != null) {
                float distance = Vector3.Distance(firePosition, enemy.position);
                predictedPos += enemyRB.linearVelocity * (distance / projectileSpeed);
            }
        }
        
        return predictedPos;
    }
}