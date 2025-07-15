using UnityEngine;
using UnityEngine.UI;
using DamageTextHelper; // 引入傷害數字顯示的命名空間

public class Ship : MonoBehaviour
{
    // ===== 基本屬性 =====
    private string m_shipName; // 私有字段
    public string ShipName // 公共屬性
    {
        get => m_shipName;
        set
        {
            m_shipName = value;
            if (nameText != null)
            {
                nameText.text = m_shipName; // 更新 UI
            }
        }
    }

    // ===== 血量 (Health) 相關 =====
    private float m_health; // 私有字段
    public float Health // 公共屬性
    {
        get => m_health;
        set
        {
            m_health = Mathf.Clamp(value, 0, maxHealth); // 確保血量在範圍內
            UpdateHealthUI(); // 更新 UI
        }
    }
    public int maxHealth = 5;    // 最大血量
    public Slider healthSlider;  // 血條 UI Slider
    public Text healthText;      // 血量文字 UI


    // 新增：船艦名稱顯示 UI

    public Canvas UICanvas;
    public Text nameText;

    // ===== 速度 (Speed) 相關 =====
    private float m_targetSpeed; // 私有字段
    public float TargetSpeed // 公共屬性
    {
        get => m_targetSpeed;
        set
        {
            m_targetSpeed = Mathf.Clamp(value, 0, maxSpeed); // 確保速度在範圍內
            #if UNITY_EDITOR
            Debug.Log($"TargetSpeed set to {m_targetSpeed}"); // Debug log
            #endif
        }
    }

    public float maxSpeed = 2f;  // 最大移動速度
    public float acceleration = 0.5f; // 加速度
    protected float currentSpeed = 0f; // 內部計算的當前速度

    // ===== 旋轉 (Rotation) 相關 =====
    private float m_targetRotationSpeed; // 私有字段
    public float TargetRotationSpeed // 公共屬性
    {
        get => m_targetRotationSpeed;
        set
        {

            if (m_navigationMode != NavigationMode.Manual)
            {
                Debug.LogWarning("Cannot set TargetRotationSpeed when NavigationMode is not Manual.");
                return;
            }
            m_targetAzimuthAngle = -1f; // 重置目標方位角
            m_targetRotationSpeed = Mathf.Clamp(value, -maxRotateSpeed, maxRotateSpeed); // 確保旋轉速度在範圍內
            Debug.Log($"TargetRotationSpeed set to {m_targetRotationSpeed}"); // Debug log
        }
    }

    public float rotationAcceleration = 10f; // 旋轉加速度（度/秒^2）
    public float maxRotateSpeed = 200f; // 最大旋轉速度
    protected float currentRotateSpeed = 0f; // 當前旋轉速度


    private float m_targetAzimuthAngle = -1f; // 私有字段
    public float TargetAzimuthAngle
    {
        get => m_targetAzimuthAngle;
        set
        {
            if (m_navigationMode != NavigationMode.Manual)
            {
                Debug.LogWarning("Cannot set TargetAzimuthAngle when NavigationMode is not Manual.");
                return;
            }
            m_targetAzimuthAngle = Mathf.Repeat(value, 360f); // 確保範圍在 0 到 360
            Debug.Log($"TargetAzimuthAngle set to {m_targetAzimuthAngle}"); // Debug log
        }
    }

    // ===== 目標與物理 =====
    public Transform target;   // 追蹤目標
    public Rigidbody2D rb;     // 物理引擎組件

    private Vector2 m_targetPosition = Vector2.zero; // 私有字段
    public Vector2 TargetPosition // 公共屬性
    {
        get => m_targetPosition;
        set => m_targetPosition = value;
    }

    // ===== 導航模式相關 =====
    public enum NavigationMode
    {
        Manual, // 手動控制
        Auto    // 自動導航
    }

    private NavigationMode m_navigationMode = NavigationMode.Manual; // 私有字段
    public NavigationMode NavMode // 公共屬性
    {
        get => m_navigationMode;
        set => m_navigationMode = value;
    }

    // 新增：旋轉模式相關
    public enum RotationMode
    {
        None = -1,
        Manual,
        Auto
    }

    private RotationMode m_rotationMode = RotationMode.None; // 使用枚舉替代魔術數字
    public RotationMode RotateMode // 公共屬性
    {
        get => m_rotationMode;
        set => m_rotationMode = value;
    }

    // ===== 方法 =====
    public virtual void Initialize(string name, float speed, float health)
    {
        ShipName = name;
        TargetSpeed = speed;
        Health = health;
        // 新增：初始化名稱顯示
        if (nameText != null)
        {
            nameText.text = ShipName;
        }
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
        // 新增：更新名稱顯示
        if (nameText != null)
        {
            nameText.text = ShipName;
        }
    }

    public void UpdateHealthUIPosition()
    {
        if (UICanvas != null)
        {
            UICanvas.transform.rotation = Quaternion.identity; // Prevent rotation
        }
    }

    public void SetCanvasEventCamera()
    {
        if (UICanvas != null && Camera.main != null)
        {
            UICanvas.worldCamera = Camera.main; // Set the event camera for the canvas
        }
    }

    public void MoveToPosition(Vector2 targetPosition)
    {
        Debug.Log($"Ship {name} moving to position {targetPosition}.");
        TargetAzimuthAngle = Mathf.Atan2(targetPosition.y - transform.position.y, targetPosition.x - transform.position.x) * Mathf.Rad2Deg;
        TargetSpeed = maxSpeed; // 設置最大速度移動
    }

    protected virtual void Update()
    {
        MoveForward(); // Call movement logic
        HandleRotation(); // Handle rotation logic
        UpdateHealthUIPosition(); // Ensure health UI follows the ship
    }

    protected void HandleRotation()
    {
        if (RotateMode == RotationMode.Manual) // 如果目標方位角被設定
        {
            TargetAzimuthAngle = Mathf.Repeat(TargetAzimuthAngle, 360f); // 確保範圍在 0 到 360
            TargetRotationSpeed = 0; // 重置旋轉速度
            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.z, TargetAzimuthAngle);
            if (Mathf.Abs(angleDifference) > 5f) // 加入緩衝區
            {
                currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, 
                    Mathf.Sign(angleDifference) * maxRotateSpeed, 
                    rotationAcceleration * Time.deltaTime);
            }
            else if (Mathf.Abs(angleDifference) < 0.1f) // 如果接近目標角度
            {
                currentRotateSpeed = 0; // 停止旋轉
                RotateMode = RotationMode.None; // 重置旋轉模式
            }
            else
            {
                rb.angularVelocity = currentRotateSpeed; // Apply rotation speed
            }
        }
        else
        {
            currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, TargetRotationSpeed, rotationAcceleration * Time.deltaTime);
            rb.angularVelocity = currentRotateSpeed; // Apply rotation speed
        }
    }

    protected virtual void MoveForward()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, TargetSpeed, acceleration * Time.deltaTime);
        currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, TargetRotationSpeed, acceleration * Time.deltaTime);

        if (rb != null)
        {
            rb.linearVelocity = transform.right * currentSpeed; // Move the ship forward based on its current speed
            rb.angularVelocity = currentRotateSpeed; // Apply rotation speed

            // 修正屬性名稱
            if (NavMode == NavigationMode.Auto && TargetPosition != Vector2.zero && Vector2.Distance(transform.position, TargetPosition) < 0.1f)
            {
                TargetSpeed = 0; // 停止移動
                TargetPosition = Vector2.zero; // 重置目標位置
            }
        }
    }
}
