using UnityEngine;
using UnityEngine.UI;

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
    public float deceleration = 0.5f; // 減速度
    protected float currentSpeed = 0f; // 內部計算的當前速度

    // ===== 旋轉 (Rotation) 相關 =====
    public float rotationSpeed = 100f; // 旋轉速度（度/秒）

    // ===== 戰鬥系統 =====
    public int attackDamage = 1;      // 攻擊傷害
    public float attackInterval = 1f; // 攻擊間隔（秒）
    protected float attackTimer;      // 攻擊計時器
    public GameObject ammoPrefab;     // 子彈預製體

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
        UpdateHealthUIPosition(); // Ensure health UI follows the ship
    }

    protected virtual void MoveForward()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        if (rb != null)
        {
            rb.linearVelocity = transform.right * currentSpeed; // Move the ship forward based on its current speed
        }
    }
}
