using System.IO;
using UnityEngine;

[System.Serializable]
public class PredictionTech {
    public bool isUnlocked = false;
}

public class TechTreeManager : MonoBehaviour {
    // 科技樹狀態
    public PredictionTech basicShooting = new PredictionTech();
    public PredictionTech dynamicTracking = new PredictionTech();
    public PredictionTech intelligentCorrection = new PredictionTech();
    
    public NavalBaseController navalBaseController; // Reference to NavalBaseController

    private string savePath;

    void Start() {
        savePath = Path.Combine(Application.streamingAssetsPath, "TechTreeSave.json");
        LoadTechTree();
    }

    //=== 科技解鎖方法 ===//
    public void UnlockBasicShooting() {
        if (navalBaseController.gold >= 2 && !basicShooting.isUnlocked)
        {
            basicShooting.isUnlocked = true;
            navalBaseController.gold -= 2; // Deduct gold
            navalBaseController.UpdateGoldUI(); // Update gold UI
            SaveTechTree();
        }
    }

    public void UnlockDynamicTracking() {
        if (navalBaseController.gold >= 3 && basicShooting.isUnlocked && !dynamicTracking.isUnlocked)
        {
            dynamicTracking.isUnlocked = true;
            navalBaseController.gold -= 3; // Deduct gold
            navalBaseController.UpdateGoldUI(); // Update gold UI
            SaveTechTree();
        }
    }

    public void UnlockIntelligentCorrection() {
        if (navalBaseController.gold >= 4 && dynamicTracking.isUnlocked && !intelligentCorrection.isUnlocked)
        {
            intelligentCorrection.isUnlocked = true;
            navalBaseController.gold -= 4; // Deduct gold
            navalBaseController.UpdateGoldUI(); // Update gold UI
            SaveTechTree();
        }
    }

    //=== 存檔系統 ===//
    [System.Serializable]
    private class SaveData {
        public bool basicShooting;
        public bool dynamicTracking;
        public bool intelligentCorrection;
    }

    void SaveTechTree() {
        SaveData data = new SaveData {
            basicShooting = basicShooting.isUnlocked,
            dynamicTracking = dynamicTracking.isUnlocked,
            intelligentCorrection = intelligentCorrection.isUnlocked
        };
        File.WriteAllText(savePath, JsonUtility.ToJson(data));
    }

    void LoadTechTree() {
        if(File.Exists(savePath)) {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
            basicShooting.isUnlocked = data.basicShooting;
            dynamicTracking.isUnlocked = data.dynamicTracking;
            intelligentCorrection.isUnlocked = data.intelligentCorrection;
        }
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