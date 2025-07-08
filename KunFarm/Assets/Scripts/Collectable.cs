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
            if (ChickenManager.Instance != null)
            {
                float managerHatchTime = ChickenManager.Instance.GetDefaultHatchTime();
                if (hatchTimer == 0f)
                {
                    hatchTime = managerHatchTime;
                }
            }
            else
            {
                if (hatchTime <= 0f)
                {
                    hatchTime = 180f; // Fallback default
                }
            }
            
            if (ChickenManager.Instance != null && ChickenManager.Instance.GetChickenPrefab() != null)
            {
                hatchCoroutine = StartCoroutine(HatchEgg());
            }
        }
    }

    private IEnumerator HatchEgg()
    {
        float remainingTime = RemainingHatchTime;
        if (remainingTime <= 0f)
        {
            SpawnChickenFromEgg();
            yield break;
        }
        
        while (hatchTimer < hatchTime)
        {
            yield return null;
            hatchTimer += Time.deltaTime;
        }
        
        SpawnChickenFromEgg();
    }
    
    private void SpawnChickenFromEgg()
    {
        Vector3 spawnPos = transform.position;
        GameObject newChicken = null;
        
        if (ChickenManager.Instance != null)
        {
            newChicken = ChickenManager.Instance.SpawnChicken(spawnPos);
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
            Destroy(this.gameObject);
        }
    }
}

public enum CollectableType
{
    NONE, EGG, WHEAT, GRAPE, APPLETREE, WHEATSEED, GRAPESEED, APPLETREESEED,
    SHOVEL_TOOL, HAND_TOOL, WATERING_CAN_TOOL, APPLE, AXE_TOOL,
}
