using System.Collections;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public CollectableType type;
    public Sprite icon;
    public Rigidbody2D rb2d;

    [Header("Egg Hatching Settings")]
    [SerializeField] private float hatchTime = 0f; // Will be set from ChickenManager, default fallback
    [SerializeField] private float hatchTimer = 0f; // Thời gian đã đếm
    
    // Public properties để save/restore
    public float HatchTime 
    { 
        get => hatchTime; 
        set => hatchTime = value; 
    }
    
    public float HatchTimer 
    { 
        get => hatchTimer; 
        set => hatchTimer = value; 
    }
    
    public float RemainingHatchTime => Mathf.Max(0f, hatchTime - hatchTimer);
    
    private Coroutine hatchCoroutine;

    private void Awake()
    {
        // Get existing Rigidbody2D or add one if missing
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 0f; // No gravity for farming items
            rb2d.drag = 2f; // Add drag so items slow down naturally
            rb2d.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        }
    }
    private void Start()
    {
        GetComponent<SpriteRenderer>().sortingOrder = 5;
        if (type == CollectableType.EGG)
        {
            // Luôn lấy default hatch time từ ChickenManager khi khởi tạo egg
            if (ChickenManager.Instance != null)
            {
                float managerHatchTime = ChickenManager.Instance.GetDefaultHatchTime();
                // Chỉ override nếu không phải restored từ save data (hatchTimer = 0)
                if (hatchTimer == 0f)
                {
                    hatchTime = managerHatchTime;
                    Debug.Log($"[Collectable] Egg {name} sử dụng ChickenManager hatch time: {hatchTime}s");
                }
                else
                {
                    Debug.Log($"[Collectable] Egg {name} restored từ save data - HatchTime: {hatchTime}s, Timer: {hatchTimer}s");
                }
            }
            else
            {
                // Fallback nếu ChickenManager không tồn tại và chưa set hatchTime
                if (hatchTime <= 0f)
                {
                    hatchTime = 180f; // Fallback default
                }
                Debug.LogWarning($"[Collectable] ChickenManager not found, egg {name} sử dụng fallback hatch time: {hatchTime}s");
            }
            
            // Kiểm tra xem ChickenManager có thể load được prefab gà không
            if (ChickenManager.Instance != null && ChickenManager.Instance.GetChickenPrefab() != null)
            {
                hatchCoroutine = StartCoroutine(HatchEgg());
            }
            else
            {
                Debug.LogWarning("Không tìm thấy ChickenManager hoặc prefab gà!");
            }
        }
    }

    private IEnumerator HatchEgg()
    {
        Debug.Log($"[Collectable] Trứng {name} bắt đầu nở - HatchTime: {hatchTime}s, Current Timer: {hatchTimer}s");
        
        // Nếu đã có progress từ save data, continue từ đó
        float remainingTime = RemainingHatchTime;
        
        if (remainingTime <= 0f)
        {
            // Trứng đã sẵn sàng nở
            Debug.Log($"[Collectable] Trứng {name} sẵn sàng nở ngay lập tức!");
            SpawnChickenFromEgg();
            yield break;
        }
        
        // Update timer trong quá trình chờ
        float startTime = Time.time;
        while (hatchTimer < hatchTime)
        {
            yield return null; // Wait for next frame
            hatchTimer += Time.deltaTime;
            
            // Optional: Debug log progress every 30 seconds
            if (Time.time - startTime >= 30f)
            {
                Debug.Log($"[Collectable] Trứng {name} nở progress: {(hatchTimer/hatchTime*100):F1}% ({RemainingHatchTime:F1}s remaining)");
                startTime = Time.time;
            }
        }
        
        // Trứng đã nở
        SpawnChickenFromEgg();
    }
    
    private void SpawnChickenFromEgg()
    {
        // Tạo gà mới tại vị trí trứng hiện tại - sử dụng ChickenManager
        Vector3 spawnPos = transform.position;
        GameObject newChicken = null;
        
        if (ChickenManager.Instance != null)
        {
            newChicken = ChickenManager.Instance.SpawnChicken(spawnPos);
        }

        if (newChicken != null)
        {
            Debug.Log($"[Collectable] Trứng {name} đã nở thành gà tại vị trí: {spawnPos}");
        }
        else
        {
            Debug.LogWarning($"[Collectable] Không thể tạo gà từ trứng {name}!");
        }

        Destroy(this.gameObject);
    }
    
    /// <summary>
    /// Force egg to hatch immediately (for testing or special events)
    /// </summary>
    public void ForceHatch()
    {
        if (type == CollectableType.EGG && hatchCoroutine != null)
        {
            StopCoroutine(hatchCoroutine);
            hatchTimer = hatchTime; // Set to completed
            SpawnChickenFromEgg();
        }
    }
    
    /// <summary>
    /// Reset hatch timer (for testing or special events)
    /// </summary>
    public void ResetHatchTimer()
    {
        hatchTimer = 0f;
        if (type == CollectableType.EGG && hatchCoroutine == null)
        {
            hatchCoroutine = StartCoroutine(HatchEgg());
        }
    }

    [Header("Pickup Settings")]
    public bool requiresInteraction = true;
    
    private bool playerNearby = false;
    private Player nearbyPlayer = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player && requiresInteraction)
        {
            playerNearby = true;
            nearbyPlayer = player;
        }
        else if (player && !requiresInteraction)
        {
            // Legacy auto-pickup behavior
            player.inventory.Add(this, 1);
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player && requiresInteraction)
        {
            playerNearby = false;
            nearbyPlayer = null;
        }
    }

    void Update()
    {
        if (requiresInteraction && playerNearby && nearbyPlayer != null && Input.GetKeyDown(KeyCode.E))
        {
            PickupItem();
        }
    }

    private void PickupItem()
    {
        if (nearbyPlayer != null)
        {
            nearbyPlayer.inventory.Add(this, 1);
            Debug.Log($"[Collectable] Picked up {type} manually");
            Destroy(this.gameObject);
        }
    }
}

public enum CollectableType
{
    NONE, EGG, WHEAT, GRAPE, APPLETREE, WHEATSEED, GRAPESEED, APPLETREESEED,
    SHOVEL_TOOL, HAND_TOOL, APPLE
}
