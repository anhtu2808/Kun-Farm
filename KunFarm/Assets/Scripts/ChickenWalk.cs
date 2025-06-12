using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug; // Thêm alias này để chỉ định rõ sử dụng Debug từ UnityEngine

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
    public bool reverseFlipDirection = true; // Mặc định bật để sửa lỗi gà đi lùi

    [Header("Environment Detection")]
    public float obstacleDetectionRadius = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isMoving = false;
    private bool isInitialized = false;

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
                // Thử tìm trong con
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
        StartCoroutine(ChickenAI());
    }

    void Update()
    {
        UpdateAnimation();
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
                spriteRenderer.flipX = !reverseFlipDirection; // Đảo ngược logic
            }
            else if (moveDirection.x > 0)
            {
                // Sử dụng biến để quyết định hướng flip
                spriteRenderer.flipX = reverseFlipDirection; // Đảo ngược logic
            }
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
}