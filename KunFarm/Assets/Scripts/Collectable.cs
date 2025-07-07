using System.Collections;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public CollectableType type;
    public Sprite icon;
    public Rigidbody2D rb2d;

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
            // Kiểm tra xem ChickenManager có thể load được prefab gà không
            if (ChickenManager.Instance != null && ChickenManager.Instance.GetChickenPrefab() != null)
            {
                StartCoroutine(HatchEgg());
            }
            else
            {
                Debug.LogWarning("Không tìm thấy ChickenManager hoặc prefab gà!");
            }
        }
    }

    private IEnumerator HatchEgg()
    {
        // Lấy thời gian nở từ ChickenManager
        float hatchTime = 180f; // fallback default
        if (ChickenManager.Instance != null)
        {
            hatchTime = ChickenManager.Instance.GetDefaultHatchTime();
        }
        
        Debug.Log($"Trứng sẽ nở sau {hatchTime} giây");
        yield return new WaitForSeconds(hatchTime);

        // Tạo gà mới tại vị trí trứng hiện tại - sử dụng ChickenManager
        Vector3 spawnPos = transform.position;
        GameObject newChicken = null;
        
        if (ChickenManager.Instance != null)
        {
            newChicken = ChickenManager.Instance.SpawnChicken(spawnPos);
        }

        if (newChicken != null)
        {
            Debug.Log("Trứng đã nở thành gà tại vị trí: " + spawnPos);
        }
        else
        {
            Debug.LogWarning("Không thể tạo gà từ trứng!");
        }

        Destroy(this.gameObject);
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
