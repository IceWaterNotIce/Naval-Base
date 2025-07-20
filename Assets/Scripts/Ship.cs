using UnityEngine;
using DamageTextHelper;
using System.Collections.Generic;
using System;
using System.Collections;

public class Ship : MonoBehaviour
{
    // ===== 基本屬性 =====
    [SerializeField]
    private string m_shipName; // 讓 ShipName 可在 Inspector 中設定
    public string ShipName
    {
        get => m_shipName;
        set
        {
            m_shipName = value;
            OnNameChanged?.Invoke(m_shipName); // 觸發名稱變化事件
        }
    }

    // ===== 血量 (Health) 相關 =====
    [SerializeField]
    private float m_health; // 讓 Health 可在 Inspector 中設定
    public float Health
    {
        get => m_health;
        set
        {
            m_health = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(m_health); // 觸發健康值變化事件
        }
    }
    public int maxHealth = 5;

    // ===== 速度 (Speed) 相關 =====
    [Header("Speed Settings")]

    [SerializeField]
    protected float m_currentSpeed;
    public float CurrentSpeed
    {
        get => m_currentSpeed;
        set => m_currentSpeed = Mathf.Clamp(value, 0, maxSpeed);
    }

    [SerializeField]
    private float m_targetSpeed; // 讓 TargetSpeed 可在 Inspector 中設定
    public float TargetSpeed
    {
        get => m_targetSpeed;
        set => m_targetSpeed = Mathf.Clamp(value, 0, maxSpeed);
    }

    public float maxSpeed = 2f;
    public float acceleration = 0.5f;



    // ===== 旋轉 (Rotation) 相關 =====
    [Header("Rotation Settings")]
    [SerializeField]
    private float m_targetRotationSpeed; // 讓 TargetRotationSpeed 可在 Inspector 中設定
    public float TargetRotationSpeed
    {
        get => m_targetRotationSpeed;
        set
        {
            if (m_navigationMode != NavigationMode.Manual)
            {
                Debug.LogWarning("Cannot set TargetRotationSpeed when NavigationMode is not Manual.");
                return;
            }
            m_targetAzimuthAngle = -1f;
            m_targetRotationSpeed = Mathf.Clamp(value, -maxRotateSpeed, maxRotateSpeed);
        }
    }

    public float rotationAcceleration = 10f;
    public float maxRotateSpeed = 200f;

    [SerializeField]
    protected float currentRotateSpeed = 0f;


    [SerializeField]
    private float m_targetAzimuthAngle = -1f; // 讓 TargetAzimuthAngle 可在 Inspector 中設定
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
            m_targetAzimuthAngle = Mathf.Repeat(value, 360f);
        }
    }

    // ===== 目標與物理 =====
    [Header("Target and Physics")]
    public Transform target;
    public Rigidbody2D rb;
    [SerializeField]
    private Vector2 m_targetPosition; // 讓 TargetPosition 可在 Inspector 中設定
    public Vector2 TargetPosition
    {
        get => m_targetPosition;
        set => m_targetPosition = value;
    }

    // ===== 導航模式相關 =====
    public enum NavigationMode { Manual, Auto, Position }
    private NavigationMode m_navigationMode = NavigationMode.Manual;
    public NavigationMode NavMode
    {
        get => m_navigationMode;
        set => m_navigationMode = value;
    }

    public enum RotationMode { None = -1, Manual, Auto }
    private RotationMode m_rotationMode = RotationMode.None;
    public RotationMode RotateMode
    {
        get => m_rotationMode;
        set => m_rotationMode = value;
    }

    public enum MovementMode
    {
        SpeedAndRotation, SpeedAndAngle, SpeedAndPosition,
        SpeedAndTarget, RandomMovement
    }

    [Header("Movement Settings")]
    public MovementMode currentMovementMode = MovementMode.SpeedAndRotation;
    public float positionReachThreshold = 0.1f;
    public float angleReachThreshold = 5f;

    [Header("Random Movement Settings")]
    public Rect movementBounds = new Rect(-5, -5, 10, 10);
    public float randomMovementChangeInterval = 3f;
    private float randomMovementTimer = 0f;
    private Vector2 randomTargetPosition;

    private List<Vector2> movementHistory = new List<Vector2>();
    private const int maxHistoryPoints = 100;

    public event Action<float> OnHealthChanged; // 健康值變化事件
    public event Action<string> OnNameChanged;  // 名稱變化事件

    public virtual void Initialize(string name, float speed, float health)
    {
        ShipName = name;
        TargetSpeed = speed;
        Health = health;
    }

    public virtual void TakeDamage(int damage)
    {
        Health -= damage;
        Health = Mathf.Clamp(Health, 0, maxHealth);

        Debug.Log($"This position: {this.gameObject.transform.position}, Health: {Health}");
        DamageTextManager.Instance.ShowDamage(damage, this.gameObject.transform.position, false);

        if (Health <= 0)
        {
            DestroyShip();
        }
    }

    protected virtual void Update()
    {
        UpdateMovement();

        if (Time.frameCount % 3 == 0)
        {
            movementHistory.Add(transform.position);
            if (movementHistory.Count > maxHistoryPoints)
            {
                movementHistory.RemoveAt(0);
            }
        }
    }

    protected virtual void DestroyShip()
    {
        movementHistory.Clear();
        Destroy(gameObject);
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

    public void StartRandomMovement()
    {
        currentMovementMode = MovementMode.RandomMovement;
        TargetSpeed = maxSpeed * 0.5f;
        GenerateRandomTargetPosition();
    }

    private void GenerateRandomTargetPosition()
    {
        randomTargetPosition = new Vector2(
            UnityEngine.Random.Range(movementBounds.xMin, movementBounds.xMax),
            UnityEngine.Random.Range(movementBounds.yMin, movementBounds.yMax)
        );
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
            case MovementMode.RandomMovement:
                MoveRandomly();
                break;
        }
    }

    protected virtual void MoveWithSpeedAndRotation()
    {
        m_currentSpeed = Mathf.MoveTowards(m_currentSpeed, TargetSpeed, acceleration * Time.deltaTime);
        currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, TargetRotationSpeed * (m_currentSpeed / maxSpeed), rotationAcceleration * Time.deltaTime);

        rb.linearVelocity = transform.right * m_currentSpeed;
        rb.angularVelocity = currentRotateSpeed;
    }

    protected virtual void MoveWithSpeedAndAngle()
    {
        if (TargetAzimuthAngle < 0) return;

        m_currentSpeed = Mathf.MoveTowards(m_currentSpeed, TargetSpeed, acceleration * Time.deltaTime);
        float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.z, TargetAzimuthAngle);

        if (Mathf.Abs(angleDifference) > angleReachThreshold)
        {
            float targetRotSpeed = Mathf.Sign(angleDifference) * maxRotateSpeed * (m_currentSpeed / maxSpeed);
            currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, targetRotSpeed, rotationAcceleration * Time.deltaTime);
        }
        else
        {
            currentRotateSpeed = 0;
            transform.rotation = Quaternion.Euler(0, 0, TargetAzimuthAngle);
        }

        rb.linearVelocity = transform.right * m_currentSpeed;
        rb.angularVelocity = currentRotateSpeed;
    }

    protected virtual void MoveWithSpeedAndPosition()
    {
        if (TargetPosition == Vector2.zero) return;

        // 計算方向與距離
        Vector2 direction = (TargetPosition - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, TargetPosition);

        // 計算減速所需的距離
        float decelerationDistance = (m_currentSpeed * m_currentSpeed) / (2f * acceleration);

        // 如果距離小於減速距離，開始減速
        if (distance <= decelerationDistance)
        {
            TargetSpeed = 0f; // 設定目標速度為 0，讓船艦平滑減速
        }
        else
        {
            TargetSpeed = maxSpeed; // 否則保持最高速度
        }

        // 如果已經非常接近目標位置，強制停止
        if (distance < positionReachThreshold)
        {
            TargetSpeed = 0;
            TargetPosition = Vector2.zero;
            currentMovementMode = MovementMode.SpeedAndRotation; // 切換回速度和旋轉模式
            return;
        }

        // 正常移動邏輯
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        TargetAzimuthAngle = targetAngle;
        MoveWithSpeedAndAngle();
    }

    protected virtual void MoveWithSpeedAndTarget()
    {
        if (target == null) return;
        TargetPosition = target.position;
        MoveWithSpeedAndPosition();
    }

    protected virtual void MoveRandomly()
    {
        randomMovementTimer += Time.deltaTime;

        if (randomMovementTimer >= randomMovementChangeInterval ||
            Vector2.Distance(transform.position, randomTargetPosition) < positionReachThreshold)
        {
            randomMovementTimer = 0f;
            GenerateRandomTargetPosition();
        }

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
                Vector2 nextPosition = currentPosition + direction * m_currentSpeed * timeStep;
                Gizmos.DrawLine(currentPosition, nextPosition);
                currentPosition = nextPosition;
                float rotationDelta = currentRotateSpeed * timeStep;
                direction = Quaternion.Euler(0, 0, rotationDelta) * direction;
            }
        }
    }

    protected virtual void Awake()
    {
        if (Health <= 0)
        {
            Debug.LogWarning($"{ShipName} has no health and will be destroyed.");
            DestroyShip(); // 如果健康值為 0 或更低，銷毀船艦
        }
    }
}
