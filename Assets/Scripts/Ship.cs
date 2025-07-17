using UnityEngine;
using UnityEngine.UI;
using DamageTextHelper; // 引入傷害數字顯示的命名空間
using System.Collections.Generic; // 引入列表支持

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
    [SerializeField] // 顯示在 Inspector 中
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
        Auto,    // 自動導航

        Position
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

    // 新增：移動模式相關
    public enum MovementMode
    {
        SpeedAndRotation,   // 使用目標速度和旋轉速度
        SpeedAndAngle,      // 使用目標速度和目標角度
        SpeedAndPosition,   // 使用目標速度和目標位置
        SpeedAndTarget,     // 使用目標速度和目標物件
        RandomMovement      // 新增：在矩形範圍內隨機移動
    }

    [Header("Movement Settings")]
    public MovementMode currentMovementMode = MovementMode.SpeedAndRotation;
    public float positionReachThreshold = 0.1f;
    public float angleReachThreshold = 5f;

    // 新增矩形範圍設定
    [Header("Random Movement Settings")]
    public Rect movementBounds = new Rect(-5, -5, 10, 10); // 預設移動範圍 (x,y,width,height)
    public float randomMovementChangeInterval = 3f; // 目標位置變更間隔(秒)
    private float randomMovementTimer = 0f;
    private Vector2 randomTargetPosition; // 隨機移動的目標位置

    // 新增粒子系統設定
    [Header("Particle System Settings")]
    public ParticleSystem shipParticleSystem; // 粒子系統

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

    private List<Vector2> movementHistory = new List<Vector2>(); // 記錄移動路徑
    private const int maxHistoryPoints = 100; // 最大記錄點數

    protected virtual void Update()
    {
        UpdateMovement(); // 更新移動邏輯
        UpdateHealthUIPosition(); // 確保 UI 跟隨船艦

        // 更新粒子系統的生命週期
        if (shipParticleSystem != null)
        {
            var mainModule = shipParticleSystem.main;
            mainModule.startLifetime = currentSpeed * 10; // 粒子生命週期根據速度調整
        }

        // 記錄移動路徑
        if (Time.frameCount % 3 == 0) // 每3幀記錄一次以減少性能開銷
        {
            movementHistory.Add(transform.position);
            if (movementHistory.Count > maxHistoryPoints)
            {
                movementHistory.RemoveAt(0); // 移除最早的記錄點
            }
        }
    }

    protected virtual void DestroyShip()
    {
        movementHistory.Clear(); // 清除歷史數據
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


    public void MoveToPosition(Vector2 position)
    {
        currentMovementMode = MovementMode.SpeedAndPosition;
        TargetPosition = position;
        TargetSpeed = maxSpeed;
    }

    public void FollowTarget(Transform targetTransform)
    {
        currentMovementMode = MovementMode.SpeedAndTarget;
        target = targetTransform;
        TargetSpeed = maxSpeed;
    }

    // 新增方法：開始隨機移動
    public void StartRandomMovement()
    {
        currentMovementMode = MovementMode.RandomMovement;
        TargetSpeed = maxSpeed * 0.5f; // 使用一半的最大速度進行隨機移動
        GenerateRandomTargetPosition(); // 生成第一個隨機目標位置
    }

    // 新增方法：生成隨機目標位置
    private void GenerateRandomTargetPosition()
    {
        randomTargetPosition = new Vector2(
            Random.Range(movementBounds.xMin, movementBounds.xMax),
            Random.Range(movementBounds.yMin, movementBounds.yMax)
        );
        Debug.Log($"New random target position: {randomTargetPosition}");
    }

    protected virtual void UpdateMovement()
    {
        switch (currentMovementMode)
        {
            case MovementMode.SpeedAndRotation:
                MoveWithSpeedAndRotation();
                break;
            case MovementMode.SpeedAndAngle:
                MoveWithSpeedAndAngle();
                break;
            case MovementMode.SpeedAndPosition:
                MoveWithSpeedAndPosition();
                break;
            case MovementMode.SpeedAndTarget:
                MoveWithSpeedAndTarget();
                break;
            case MovementMode.RandomMovement: // 新增：隨機移動處理
                MoveRandomly();
                break;
        }
    }

    // 模式1: 使用目標速度和旋轉速度移動
    protected virtual void MoveWithSpeedAndRotation()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, TargetSpeed, acceleration * Time.deltaTime);
        currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, TargetRotationSpeed * (currentSpeed / maxSpeed), rotationAcceleration * Time.deltaTime); // 根據速度調整旋轉速度

        rb.linearVelocity = transform.right * currentSpeed;
        rb.angularVelocity = currentRotateSpeed;
    }

    // 模式2: 使用目標速度和目標角度移動
    protected virtual void MoveWithSpeedAndAngle()
    {
        if (TargetAzimuthAngle < 0) return;

        currentSpeed = Mathf.MoveTowards(currentSpeed, TargetSpeed, acceleration * Time.deltaTime);
        float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.z, TargetAzimuthAngle);

        if (Mathf.Abs(angleDifference) > angleReachThreshold)
        {
            float targetRotSpeed = Mathf.Sign(angleDifference) * maxRotateSpeed * (currentSpeed / maxSpeed);
            currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, targetRotSpeed, rotationAcceleration * Time.deltaTime);
        }
        else
        {
            currentRotateSpeed = 0;
            transform.rotation = Quaternion.Euler(0, 0, TargetAzimuthAngle); // 更新船艦方向
        }

        rb.linearVelocity = transform.right * currentSpeed; // 使用船艦方向更新速度
        rb.angularVelocity = currentRotateSpeed;
    }

    // 模式3: 使用目標速度和目標位置移動
    protected virtual void MoveWithSpeedAndPosition()
    {
        if (TargetPosition == Vector2.zero) return;

        Vector2 direction = (TargetPosition - (Vector2)transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        TargetAzimuthAngle = targetAngle;
        MoveWithSpeedAndAngle(); // 重用角度移動邏輯

        // 檢查是否到達目標位置
        if (Vector2.Distance(transform.position, TargetPosition) < positionReachThreshold)
        {
            TargetSpeed = 0;
            TargetPosition = Vector2.zero;
        }
    }

    // 模式4: 使用目標速度和目標物件移動
    protected virtual void MoveWithSpeedAndTarget()
    {
        if (target == null) return;
        
        TargetPosition = target.position;
        MoveWithSpeedAndPosition(); // 重用位置移動邏輯
    }

    // 新增方法：隨機移動邏輯
    protected virtual void MoveRandomly()
    {
        // 更新計時器
        randomMovementTimer += Time.deltaTime;
        
        // 如果到達間隔時間或已到達目標位置，生成新的隨機目標
        if (randomMovementTimer >= randomMovementChangeInterval || 
            Vector2.Distance(transform.position, randomTargetPosition) < positionReachThreshold)
        {
            randomMovementTimer = 0f;
            GenerateRandomTargetPosition();
        }
        
        // 使用現有的位置移動邏輯
        TargetPosition = randomTargetPosition;
        MoveWithSpeedAndPosition();
    }

    protected virtual void OnDrawGizmos()
    {
        // 繪製移動範圍
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(movementBounds.center, movementBounds.size);

        if (rb != null)
        {
            // 繪製已移動的實際路徑（紅色）
            Gizmos.color = Color.red;
            for (int i = 1; i < movementHistory.Count; i++)
            {
                Gizmos.DrawLine(movementHistory[i - 1], movementHistory[i]);
            }

            // 繪製未來移動預測路徑（綠色）
            Gizmos.color = Color.green;
            Vector2 currentPosition = transform.position;
            Vector2 direction = transform.right.normalized;
            float timeStep = 0.1f;
            int steps = Mathf.CeilToInt(2f / timeStep);

            for (int i = 0; i < steps; i++)
            {
                Vector2 nextPosition = currentPosition + direction * currentSpeed * timeStep;
                Gizmos.DrawLine(currentPosition, nextPosition);
                currentPosition = nextPosition;
                float rotationDelta = currentRotateSpeed * timeStep;
                direction = Quaternion.Euler(0, 0, rotationDelta) * direction;
            }
        }
    }
}
