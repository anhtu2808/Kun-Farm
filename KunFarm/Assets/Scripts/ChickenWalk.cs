﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public class ChickenWalk : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1.0f;
    public float minWanderTime = 1.0f;
    public float maxWanderTime = 5.0f;
    public float minIdleTime = 1.0f;
    public float maxIdleTime = 3.0f;

    [Header("Components")]
    [Tooltip("Sẽ tự tìm nếu không được gán")]
    public Animator animator;

    [Header("Sprite Settings")]
    [Tooltip("Đánh dấu để đảo ngược hướng flip sprite")]
    public bool reverseFlipDirection = true;

    [Header("Environment Detection")]
    public float obstacleDetectionRadius = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Egg Laying Settings")]
    public GameObject eggPrefab; // Prefab của quả trứng
    public float eggLayInterval = 180f; // Thời gian giữa mỗi lần đẻ trứng (giây)
    public bool layEggsOnlyWhenIdle = true; // Chỉ đẻ trứng khi đứng yên
    public Vector3 eggOffset = new Vector3(0, 0, 0); // Spawn trứng tại vị trí của gà
    public int maxEggsPerChicken = 5; // Số lượng trứng tối đa mỗi con gà

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isMoving = false;
    private bool isInitialized = false;
    private float eggLayTimer = 0f; // Bộ đếm thời gian cho việc đẻ trứng
    private List<GameObject> eggsLaid = new List<GameObject>(); // Danh sách các quả trứng đã đẻ

    // Feeding system variables
    private float currentEggLayInterval; // Thời gian đẻ trứng hiện tại (có thể bị ảnh hưởng bởi feeding)
    private float speedMultiplier = 1f; // Nhân tốc độ đẻ trứng (1 = bình thường, <1 = nhanh hơn)


    void Awake()
    {
        InitializeComponents();
    }
    void InitializeComponents()
    {
        // Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            if (showDebugInfo) Debug.Log("Thêm Rigidbody2D cho " + gameObject.name);
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            if (showDebugInfo) Debug.LogWarning("Không tìm thấy SpriteRenderer trên " + gameObject.name);
        }

        // Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    if (showDebugInfo) Debug.LogWarning("Không tìm thấy Animator trên " + gameObject.name);
                }
                else if (showDebugInfo)
                {
                    Debug.Log("Đã tìm thấy Animator trong con của " + gameObject.name);
                }
            }
        }

        // Tự động gán prefab trứng nếu chưa có - sử dụng ChickenManager
        if (eggPrefab == null)
        {
            if (ChickenManager.Instance != null)
            {
                eggPrefab = ChickenManager.Instance.GetEggPrefab();
            }

            if (eggPrefab == null)
            {
                Debug.LogWarning("Không thể tìm thấy Prefab trứng trong Resources!");
            }
            else if (showDebugInfo)
            {
                Debug.Log("Đã tự động gán prefab trứng từ ChickenManager cho " + gameObject.name);
            }
        }

        isInitialized = true;
    }


    // Phương thức để truyền Animator từ bên ngoài
    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;
        if (showDebugInfo) Debug.Log("Đã thiết lập Animator cho " + gameObject.name);
    }

    void Start()
    {
        if (!isInitialized) InitializeComponents();
        
        // Khởi tạo feeding system - sử dụng thời gian từ ChickenManager
        if (ChickenManager.Instance != null)
        {
            eggLayInterval = ChickenManager.Instance.GetDefaultEggLayInterval();
        }
        currentEggLayInterval = eggLayInterval;
        
        // Đăng ký với ChickenManager nếu có
        if (ChickenManager.Instance != null)
        {
            string chickenId = "Chicken_" + System.DateTime.Now.Ticks + "_" + GetInstanceID();
            ChickenManager.Instance.RegisterChicken(this, chickenId);
        }
        
        StartCoroutine(ChickenAI());

        // Khởi tạo timer đẻ trứng với giá trị ngẫu nhiên để tránh tất cả gà đẻ cùng lúc
        eggLayTimer = Random.Range(0f, currentEggLayInterval * 0.5f);
    }

    void OnDestroy()
    {
        // Hủy đăng ký khỏi ChickenManager
        if (ChickenManager.Instance != null)
        {
            ChickenManager.Instance.UnregisterChicken(this);
        }
    }

    void Update()
    {
        UpdateAnimation();
        UpdateEggLaying();
    }

    void UpdateAnimation()
    {
        // Animation control
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        // Flip sprite based on movement direction
        if (spriteRenderer != null)
        {
            if (moveDirection.x < 0)
            {
                // Sử dụng biến để quyết định hướng flip
                spriteRenderer.flipX = !reverseFlipDirection;
            }
            else if (moveDirection.x > 0)
            {
                // Sử dụng biến để quyết định hướng flip
                spriteRenderer.flipX = reverseFlipDirection;
            }
        }
    }

    void UpdateEggLaying()
    {
        // Cập nhật bộ đếm thời gian đẻ trứng
        eggLayTimer += Time.deltaTime;

        // Kiểm tra xem đã đến thời gian đẻ trứng chưa (sử dụng currentEggLayInterval có thể bị ảnh hưởng bởi feeding)
        if (eggLayTimer >= currentEggLayInterval)
        {
            // Nếu chỉ đẻ trứng khi đứng yên và đang di chuyển, thì chưa đẻ
            if (layEggsOnlyWhenIdle && isMoving)
            {
                return;
            }

            // Đẻ trứng
            LayEgg();

            // Reset bộ đếm thời gian
            eggLayTimer = 0f;
        }
    }

    void LayEgg()
    {
        // Kiểm tra giới hạn số trứng
        if (maxEggsPerChicken > 0 && eggsLaid.Count >= maxEggsPerChicken)
        {
            // Xóa quả trứng cũ nhất
            if (eggsLaid[0] != null)
            {
                Destroy(eggsLaid[0]);
            }
            eggsLaid.RemoveAt(0);
        }

        // Tạo quả trứng mới tại vị trí của gà (với offset) - sử dụng ChickenManager
        Vector3 eggPosition = transform.position + eggOffset;
        GameObject newEgg = null;
        
        if (ChickenManager.Instance != null)
        {
            newEgg = ChickenManager.Instance.SpawnEgg(eggPosition);
        }

        if (newEgg != null)
        {
        // Thêm vào danh sách quản lý
        eggsLaid.Add(newEgg);

        // Đặt tên cho dễ nhận biết
        newEgg.name = "Egg_" + gameObject.name + "_" + eggsLaid.Count;

        if (showDebugInfo) Debug.Log("Gà " + gameObject.name + " đã đẻ một quả trứng!");
        }
        else
        {
            Debug.LogWarning("Không thể đẻ trứng vì ChickenManager không tìm thấy prefab!");
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (isMoving)
        {
            // Kiểm tra va chạm với chướng ngại vật
            CheckForObstacles();

            // Di chuyển gà
            rb.velocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void CheckForObstacles()
    {
        if (obstacleDetectionRadius <= 0) return;

        // Kiểm tra va chạm với chướng ngại vật bằng CircleCast
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            obstacleDetectionRadius,
            moveDirection,
            obstacleDetectionRadius,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            if (showDebugInfo) Debug.Log("Phát hiện chướng ngại vật: " + hit.collider.name);

            // Đổi hướng di chuyển
            ChangeDirection();
        }
    }

    void ChangeDirection()
    {
        // Chọn một hướng ngẫu nhiên mới
        float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

        if (showDebugInfo) Debug.Log("Đổi hướng di chuyển: " + moveDirection);
    }

    IEnumerator ChickenAI()
    {
        while (true)
        {
            // Trạng thái đứng yên
            isMoving = false;
            if (showDebugInfo) Debug.Log("Gà đứng yên");

            // Đợi một khoảng thời gian ngẫu nhiên trong trạng thái đứng yên
            yield return new WaitForSeconds(UnityEngine.Random.Range(minIdleTime, maxIdleTime));

            // Trạng thái di chuyển
            isMoving = true;

            // Chọn hướng di chuyển ngẫu nhiên
            ChangeDirection();

            if (showDebugInfo) Debug.Log("Gà bắt đầu di chuyển");

            // Di chuyển trong một khoảng thời gian ngẫu nhiên
            yield return new WaitForSeconds(UnityEngine.Random.Range(minWanderTime, maxWanderTime));
        }
    }

    // Hiển thị vùng phát hiện chướng ngại vật trong Editor
    void OnDrawGizmosSelected()
    {
        if (obstacleDetectionRadius <= 0) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, obstacleDetectionRadius);

        // Vẽ hướng di chuyển
        if (isMoving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + moveDirection * obstacleDetectionRadius);
        }

        // Vẽ vị trí đẻ trứng
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + eggOffset, 0.1f);
    }

    // Phương thức public để can thiệp từ bên ngoài nếu cần
    public void SetMovementDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    public void SetMovingState(bool moving)
    {
        isMoving = moving;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public Vector2 GetMovementDirection()
    {
        return moveDirection;
    }

    // Phương thức mới để buộc gà đẻ trứng ngay lập tức
    public void ForceLayEgg()
    {
        LayEgg();
    }

    // Phương thức mới để lấy thời gian còn lại trước khi đẻ trứng tiếp theo
    public float GetTimeUntilNextEgg()
    {
        return Mathf.Max(0, currentEggLayInterval - eggLayTimer);
    }

    /// <summary>
    /// Get current egg lay timer (public accessor cho save/restore)
    /// </summary>
    public float GetEggLayTimer()
    {
        return eggLayTimer;
    }
    
    /// <summary>
    /// Set egg lay timer (public setter cho save/restore)
    /// </summary>
    public void SetEggLayTimer(float timer)
    {
        eggLayTimer = Mathf.Clamp(timer, 0f, currentEggLayInterval);
        if (showDebugInfo)
            Debug.Log($"[{gameObject.name}] Egg lay timer set to: {eggLayTimer:F1}s (Interval: {currentEggLayInterval:F1}s)");
    }
    
    /// <summary>
    /// Get current egg lay interval (public accessor)
    /// </summary>
    public float GetCurrentEggLayInterval()
    {
        return currentEggLayInterval;
    }
    
    /// <summary>
    /// Force reset egg lay timer to 0 (for testing or events)
    /// </summary>
    public void ResetEggLayTimer()
    {
        eggLayTimer = 0f;
        if (showDebugInfo)
            Debug.Log($"[{gameObject.name}] Egg lay timer reset to 0");
    }

    #region Feeding System Methods

    /// <summary>
    /// Áp dụng speed boost từ feeding
    /// </summary>
    /// <param name="multiplier">Multiplier cho tốc độ (< 1 = nhanh hơn, > 1 = chậm hơn)</param>
    public void ApplySpeedBoost(float multiplier)
    {
        speedMultiplier = multiplier;
        currentEggLayInterval = eggLayInterval * speedMultiplier;
        
        if (showDebugInfo)
            Debug.Log($"[{gameObject.name}] Speed boost áp dụng: {multiplier:F2}x, Thời gian đẻ trứng mới: {currentEggLayInterval:F1}s");
    }

    /// <summary>
    /// Loại bỏ speed boost, trở về bình thường
    /// </summary>
    public void RemoveSpeedBoost()
    {
        speedMultiplier = 1f;
        currentEggLayInterval = eggLayInterval;
        
        if (showDebugInfo)
            Debug.Log($"[{gameObject.name}] Speed boost đã hết hạn, trở về bình thường: {currentEggLayInterval:F1}s");
    }

    /// <summary>
    /// Kiểm tra xem gà có đang được boost không
    /// </summary>
    public bool IsBoosted()
    {
        return speedMultiplier != 1f;
    }

    /// <summary>
    /// Lấy speed multiplier hiện tại
    /// </summary>
    public float GetCurrentSpeedMultiplier()
    {
        return speedMultiplier;
    }

    /// <summary>
    /// Cập nhật thời gian đẻ trứng mặc định từ ChickenManager
    /// </summary>
    public void UpdateEggLayInterval(float newInterval)
    {
        eggLayInterval = newInterval;
        
        // Chỉ cập nhật nếu không đang boost
        if (!IsBoosted())
        {
            currentEggLayInterval = eggLayInterval;
        }
        
        if (showDebugInfo)
            Debug.Log($"[{gameObject.name}] Thời gian đẻ trứng cập nhật: {eggLayInterval}s");
    }

    /// <summary>
    /// Cho gà ăn - gọi từ player hoặc UI
    /// </summary>
    /// <param name="foodType">Loại thức ăn</param>
    /// <returns>True nếu thành công</returns>
    public bool Feed(CollectableType foodType)
    {
        if (ChickenManager.Instance != null)
        {
            return ChickenManager.Instance.FeedChicken(this, foodType);
        }
        
        Debug.LogWarning("[ChickenWalk] Không thể cho ăn vì không tìm thấy ChickenManager!");
        return false;
    }

    /// <summary>
    /// Kiểm tra xem gà có đang được feed không
    /// </summary>
    public bool IsFed()
    {
        if (ChickenManager.Instance != null)
        {
            ChickenData chickenData = GetComponent<ChickenData>();
            if (chickenData != null)
            {
                return ChickenManager.Instance.IsChickenFed(chickenData.chickenId);
            }
        }
        return false;
    }

    /// <summary>
    /// Lấy thông tin trạng thái gà
    /// </summary>
    public string GetStatusInfo()
    {
        string status = $"Gà {gameObject.name}\n";
        status += $"Đang di chuyển: {(isMoving ? "Có" : "Không")}\n";
        status += $"Thời gian đẻ trứng tiếp theo: {GetTimeUntilNextEgg():F1}s\n";
        status += $"Speed multiplier: {speedMultiplier:F2}x\n";
        status += $"Đã đẻ: {eggsLaid.Count}/{maxEggsPerChicken} trứng\n";
        status += $"Được feed: {(IsFed() ? "Có" : "Không")}";
        
        return status;
    }

    #endregion

}