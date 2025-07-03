using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 2f;
    public string npcName = "Shop Keeper";
    public string shopMessage = "Welcome to the online shop!";
    
    [Header("UI")]
    public NPCPrompt npcPrompt;
    
    private Transform player;
    private bool playerNear = false;
    private UIManager uiManager;
    
    void Start()
    {
        // Tìm player
        player = FindObjectOfType<Player>()?.transform;
        
        // Tìm UIManager
        uiManager = UIManager.Instance;
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        
        // Setup collision
        if (GetComponent<Collider2D>() == null)
        {
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true; // Đặt thành trigger để không block player
        }
        
        if (GetComponent<Rigidbody2D>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        // Setup prompt
        if (npcPrompt != null)
            npcPrompt.SetTarget(transform);
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Check proximity
        if (distance <= interactionRange && !playerNear)
        {
            playerNear = true;
            ShowPrompt();
        }
        else if (distance > interactionRange && playerNear)
        {
            playerNear = false;
            HidePrompt();
        }
        
        // Handle interaction
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }
    
    void ShowPrompt()
    {
        if (npcPrompt != null)
            npcPrompt.Show();
        Debug.Log($"Player near {npcName}");
    }
    
    void HidePrompt()
    {
        if (npcPrompt != null)
            npcPrompt.Hide();
    }
    
    void Interact()
    {
        Debug.Log($"Interacting with {npcName}!");
        
        // Hiển thị thông báo
        SimpleNotificationPopup.Show($"{shopMessage}");
        
        // Mở OnlineSellShop thông qua UIManager
        if (uiManager != null)
        {
            // Đóng tất cả UI trước khi mở shop mới
            uiManager.CloseAllUIs();
            
            // Mở Online Sell Shop (giống nhấn phím O)
            uiManager.OpenUI(UIManager.UIType.SellShop);
            Debug.Log($"[ShopNPC] Opened Online Sell Shop via {npcName}");
        }
        else
        {
            Debug.LogWarning($"[ShopNPC] UIManager not found! Cannot open Online Sell Shop for {npcName}");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Vẽ interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Vẽ màu xanh dương cho Online Sell Shop
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
} 