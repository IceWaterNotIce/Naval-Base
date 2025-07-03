using UnityEngine;
using UnityEngine.UI;
using DamageTextHelper; // 引入傷害數字顯示的命名空間

public class Ship : MonoBehaviour
{
    // ===== 基本屬性 =====
    public string ShipName;  // 船艦名稱

    // ===== 血量 (Health) 相關 =====
    public float Health;         // 當前血量
    public int maxHealth = 5;    // 最大血量
    public Slider healthSlider;  // 血條 UI Slider
    public Text healthText;      // 血量文字 UI
    public Canvas healthCanvas;  // 血條畫布

    // ===== 速度 (Speed) 相關 =====
    public float targetSpeed;          // 當前速度（可被外部修改）
    public float maxSpeed = 2f;  // 最大移動速度
    public float acceleration = 0.5f; // 加速度
    protected float currentSpeed = 0f; // 內部計算的當前速度

    // ===== 旋轉 (Rotation) 相關 =====
    public float targetRotationSpeed = 100f; // 目標旋轉速度（度/秒）
    public float rotationAcceleration = 10f; // 旋轉加速度（度/秒^2）
    public float maxRotateSpeed = 200f; // 最大旋轉速度
    protected float currentRotateSpeed = 0f; // 當前旋轉速度

    public float targetAzimuthAngle = -1f ; // 目標方位角（度）


    // ===== 目標與物理 =====
    public Transform target;   // 追蹤目標
    public Rigidbody2D rb;     // 物理引擎組件


    // ===== 方法 =====
    public virtual void Initialize(string name, float speed, float health)
    {
        ShipName = name;
        targetSpeed = speed;
        Health = health;
    }

    public virtual void TakeDamage(int damage)
    {
        Health -= damage;
        Health = Mathf.Clamp(Health, 0, maxHealth); // 確保血量不低於 0

        UpdateHealthUI(); // Update health display
        Debug.Log($"This position: {this.gameObject.transform.position}, Health: {Health}"); // Debug log
        DamageTextManager.Instance.ShowDamage(damage, this.gameObject.transform.position, false); // 顯示傷害數字

        if (Health <= 0)
        {
            DestroyShip();
        }
    }

    protected virtual void DestroyShip()
    {
        Destroy(gameObject);
    }

    public void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = Health / (float)maxHealth; // 更新滑塊值
        }
        if (healthText != null)
        {
            healthText.text = $"{Health}/{maxHealth}"; // 更新血量文字
        }
    }

    public void UpdateHealthUIPosition()
    {
        if (healthCanvas != null)
        {
            healthCanvas.transform.rotation = Quaternion.identity; // Prevent rotation
        }
    }

    public void SetCanvasEventCamera()
    {
        if (healthCanvas != null && Camera.main != null)
        {
            healthCanvas.worldCamera = Camera.main; // Set the event camera for the canvas
        }
    }

    protected virtual void Update()
    {
        MoveForward(); // Call movement logic
        HandleRotation(); // Handle rotation logic
        UpdateHealthUIPosition(); // Ensure health UI follows the ship
    }

    protected void HandleRotation()
    {
        if (targetAzimuthAngle != -1f) // 如果目標方位角被設定
        {
            targetAzimuthAngle = Mathf.Repeat(targetAzimuthAngle, 360f); // 確保範圍在 0 到 360
            targetRotationSpeed = 0; // 重置旋轉速度
            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.z, targetAzimuthAngle);
            currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, angleDifference, rotationAcceleration * Time.deltaTime);
            if (Mathf.Abs(angleDifference) < 0.1f) // 如果接近目標角度
            {
                currentRotateSpeed = 0; // 停止旋轉
                targetAzimuthAngle = -1f; // 重置目標方位角
            }
            else
            {
                rb.angularVelocity = currentRotateSpeed; // Apply rotation speed
            }
        }
        else if (targetRotationSpeed != 0) // 如果目標旋轉速度被設定
        {
            targetAzimuthAngle = -1f; // 重置目標方位角
            currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, targetRotationSpeed, rotationAcceleration * Time.deltaTime);
            rb.angularVelocity = currentRotateSpeed; // Apply rotation speed
        }
    }

    protected virtual void MoveForward()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, targetRotationSpeed, acceleration * Time.deltaTime);

        if (rb != null)
        {
            rb.linearVelocity = transform.right * currentSpeed; // Move the ship forward based on its current speed
            rb.angularVelocity = currentRotateSpeed; // Apply rotation speed
        }
    }
}
