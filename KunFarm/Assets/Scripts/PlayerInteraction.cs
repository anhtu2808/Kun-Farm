using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Simplified player interaction system using ToolManager and direct collectable planting
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap interactableMap;
    [SerializeField] private ToolManager toolManager;
    [SerializeField] private Player player;
    [SerializeField] private Inventory inventoryUI;

    void Start()
    {
        // Auto-find references if not assigned
        if (player == null)
            player = FindObjectOfType<Player>();
        if (toolManager == null)
            toolManager = FindObjectOfType<ToolManager>();
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<Inventory>();
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Don't handle input if tool is already being used
        if (toolManager != null && toolManager.IsUsingTool())
            return;

        // Don't handle input if inventory is open
        if (inventoryUI != null && inventoryUI.IsOpen)
            return;

        Vector3Int targetCell = Vector3Int.zero;
        bool shouldInteract = false;

        // Mouse interaction
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetCell = interactableMap.WorldToCell(mouseWorldPos);
            shouldInteract = true;
        }
        // Space key interaction (at player position)
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            targetCell = interactableMap.WorldToCell(transform.position);
            shouldInteract = true;
        }

        if (shouldInteract)
        {
            HandleInteraction(targetCell);
        }
    }
    
    private void HandleInteraction(Vector3Int targetCell)
    {
        // Chỉ dùng tool từ toolbar, không dùng inventory
        if (toolManager != null)
        {
            Tool selectedTool = toolManager.SelectedTool;
            if (selectedTool != null && selectedTool.CanUse(targetCell, GameManager.instance.tileManager))
            {
                // Use ToolManager.TriggerToolUse() instead of UseTool() để support animations
                toolManager.TriggerToolUse(targetCell);
            }
        }
    }

} 