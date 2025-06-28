using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Simplified player interaction system using ToolManager and direct collectable planting
/// </summary>
public class NewPlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap interactableMap;
    [SerializeField] private ToolManager toolManager;
    [SerializeField] private Player player;

    void Start()
    {
        // Auto-find references if not assigned
        if (player == null)
            player = FindObjectOfType<Player>();
        if (toolManager == null)
            toolManager = FindObjectOfType<ToolManager>();
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
        // Đầu tiên thử dùng tool từ toolbar
        bool toolUsed = false;
        if (toolManager != null)
        {
            Tool selectedTool = toolManager.SelectedTool;
            if (selectedTool != null && selectedTool.CanUse(targetCell, GameManager.instance.tileManager))
            {
                // Use ToolManager.TriggerToolUse() instead of UseTool() để support animations
                toolManager.TriggerToolUse(targetCell);
                toolUsed = true;
            }
        }
        
        // Nếu tool không thể dùng hoặc không có tool, thử dùng collectable từ inventory
        if (!toolUsed && CanPlantAtCell(targetCell))
        {
            TryPlantFromInventory(targetCell);
        }
    }
    
    private bool CanPlantAtCell(Vector3Int cellPosition)
    {
        return GameManager.instance.tileManager.GetTileState(cellPosition) == TileState.Dug;
    }
    
    private void TryPlantFromInventory(Vector3Int targetCell)
    {
        if (player?.inventory == null) return;
        
        // Tìm collectable đầu tiên trong inventory có thể plant
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            var slot = player.inventory.slots[i];
            if (slot.count > 0 && ToolHelpers.CanPlantDirectly(slot.type))
            {
                // Thử plant bằng collectable này
                if (ToolHelpers.PlantFromCollectable(slot.type, targetCell, GameManager.instance.tileManager))
                {
                    // Plant thành công, remove 1 item từ inventory
                    player.inventory.Remove(i);
                    
                    // Refresh inventory UI if available
                    var inventoryUI = FindObjectOfType<InventoryUI>();
                    if (inventoryUI != null)
                        inventoryUI.Refresh();
                    
                    break; // Chỉ plant 1 cây
                }
            }
        }
    }
} 