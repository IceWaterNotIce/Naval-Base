using UnityEngine;
using DamageTextHelper;

public class DamageTextSample2d : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 每秒呼叫一次 ApplyDamage
        InvokeRepeating("CallApplyDamage", 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CallApplyDamage()
    {
        // 產生隨機傷害值和爆擊狀態
        int randomDamage = Random.Range(10, 100);
        bool randomCritical = Random.Range(0f, 1f) < 0.3f; // 30% 爆擊機率
        ApplyDamage(randomDamage, randomCritical);
    }

    void ApplyDamage(int damage, bool isCritical)
    {
        Vector3 hitPosition = transform.position + Vector3.up; // 在物件上方顯示
        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.ShowDamage(damage, hitPosition, isCritical);
        }
        else
        {
            Debug.LogError("DamageTextManager.Instance is null! 請確認場景中有 DamageTextManager。");
        }
    }
}
