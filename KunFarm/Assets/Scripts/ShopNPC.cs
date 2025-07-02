using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 2f;
    public string npcName = "Shop Keeper";
    
    [Header("UI")]
    public NPCPrompt npcPrompt;
    
    private Transform player;
    private bool playerNear = false;
    
    void Start()
    {
        // Tìm player
        player = FindObjectOfType<Player>()?.transform;
        
        // Setup collision
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
        
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
        
        // NPCPrompt tự cập nhật position trong Update() của nó
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
        SimpleNotificationPopup.Show($"Talking to {npcName}...");
        // TODO: Add shop logic here
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
} 