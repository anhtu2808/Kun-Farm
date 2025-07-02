using System.Collections;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public CollectableType type;
    private GameObject chickenPrefab;
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
            // Load prefab gà
            chickenPrefab = Resources.Load<GameObject>("Prelabs/Chicken");

            if (chickenPrefab == null)
            {
                Debug.LogWarning("Không tìm thấy prefab 'Prelabs/Chicken' trong Resources!");
            }
            else
            {
                StartCoroutine(HatchEgg());
            }
        }
    }

    private IEnumerator HatchEgg()
{
    // Đợi 10 giây (hoặc 120f cho 2 phút)
    yield return new WaitForSeconds(10f);

    if (chickenPrefab != null)
    {
        // Tạo vị trí mới với z = -10
        Vector3 spawnPos = new Vector3(0f, 0f, -10f);
        Instantiate(chickenPrefab, spawnPos, Quaternion.identity);
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
