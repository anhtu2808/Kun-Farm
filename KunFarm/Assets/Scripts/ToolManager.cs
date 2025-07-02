using UnityEngine;
using System.Collections;

/// <summary>
/// Quản lý tools trong toolbar và tương tác với UI
/// </summary>
public class ToolManager : MonoBehaviour
{
    [Header("Tools Setup")]
    [SerializeField] private ToolData[] toolDataArray; // Mảng tools để setup trong editor
    
    [Header("Animation Settings")]
    [SerializeField] private float hoeAnimationSpeed = 0.7f; // Adjustable animation speed
    
    [Header("Interaction Settings")]
    [SerializeField] private float maxInteractionDistance = 1.5f; // Maximum distance to interact with tiles
    
    [Header("References")]
    [SerializeField] private Toolbar_UI toolbarUI;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private Movement playerMovement; // Changed from playerAnimator to playerMovement
    [SerializeField] private Inventory inventoryUI;

    private Tool[] tools; // Mảng tools trong toolbar (9 slots)
    private int selectedToolIndex = 0;
    private bool isUsingTool = false;
    
    public Tool SelectedTool => tools != null && selectedToolIndex < tools.Length ? tools[selectedToolIndex] : null;

    void Awake()
    {
        InitializeTools();
    }

    void Start()
    {
        UpdateToolbarUI();
        
        // Auto-find components if not assigned
        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<Inventory>();
    }

    void Update()
    {
        // Listen for toolbar selection changes
        CheckToolSelection();
    }

    private void InitializeTools()
    {
        tools = new Tool[9]; // 9 slots như trong Toolbar_UI
        
        // Initialize tools từ ToolData array
        for (int i = 0; i < toolDataArray.Length && i < tools.Length; i++)
        {
            if (toolDataArray[i] != null)
            {
                tools[i] = toolDataArray[i].CreateTool();
            }
        }
    }

    private void CheckToolSelection()
    {
        // Don't allow tool selection if inventory is open
        if (inventoryUI != null && inventoryUI.IsOpen)
            return;
            
        int newSelection = -1;
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) newSelection = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) newSelection = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) newSelection = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) newSelection = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) newSelection = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) newSelection = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7)) newSelection = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha8)) newSelection = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha9)) newSelection = 8;
        
        if (newSelection != -1 && newSelection != selectedToolIndex)
        {
            SelectTool(newSelection);
        }
    }

    public void SelectTool(int index, bool updateUI = true)
    {
        if (tools == null)
        {
            Debug.LogWarning("ToolManager: tools array is null, skipping SelectTool");
            return;
        }
        
        if (index >= 0 && index < tools.Length)
        {
            selectedToolIndex = index;
            
            // Update toolbar UI only if requested (to avoid infinite loop)
            if (updateUI && toolbarUI != null)
                toolbarUI.SelectSlot(index);
            
            // Update player animation
            UpdatePlayerAnimation();
        }
    }

    public void UseTool(Vector3Int cellPosition)
    {
        if (tileManager == null || !tileManager.IsInteractable(cellPosition) || isUsingTool)
            return;

        // Don't allow tool use if inventory is open
        if (inventoryUI != null && inventoryUI.IsOpen)
            return;

        // Check distance from player to target cell
        if (!IsWithinInteractionRange(cellPosition))
        {
            Debug.Log("Target is too far away! Move closer to dig.");
            return;
        }

        Tool currentTool = SelectedTool;
        if (currentTool != null && currentTool.CanUse(cellPosition, tileManager))
        {
            // Check if this is a shovel tool for hoeing animation
            if (currentTool is ShovelTool)
            {
                StartCoroutine(PlayHoeAnimation(cellPosition, currentTool));
            }
            else
            {
                // Use other tools immediately
                currentTool.Use(cellPosition, tileManager);
                HandleToolConsumption(currentTool);
            }
        }
    }

    private IEnumerator PlayHoeAnimation(Vector3Int cellPosition, Tool tool)
    {
        isUsingTool = true;
        
        if (playerMovement != null && playerMovement.GetAnimator() != null)
        {
            var animator = playerMovement.GetAnimator();
            
            // Get direction to target for very close targets, otherwise use facing direction
            Vector3 playerPos = playerMovement.transform.position;
            Vector3 targetPos = tileManager.GetTilemap().GetCellCenterWorld(cellPosition);
            Vector3 directionToTarget = (targetPos - playerPos).normalized;
            
            // If target is very close (direction is nearly zero), use player's facing direction
            Vector3 finalDirection;
            if (Vector3.Distance(playerPos, targetPos) < 0.5f)
            {
                // Use player's current facing direction when target is very close
                finalDirection = playerMovement.GetFacingDirection();
            }
            else
            {
                // Use direction to target when target is further away
                finalDirection = directionToTarget;
            }
            
            // Determine hoe direction using 2D blend tree coordinates
            Vector2 hoeBlendDirection = GetHoeBlendDirection(finalDirection);
            
            // Set animator parameters for 2D blend tree
            animator.SetFloat("horizontal", hoeBlendDirection.x);
            animator.SetFloat("vertical", hoeBlendDirection.y);
            animator.SetBool("isHoeing", true);
            
            // Apply animation speed if specified
            if (hoeAnimationSpeed != 1f)
            {
                animator.speed = hoeAnimationSpeed;
            }
            
            // Wait for animation to play (dynamic based on animation speed)
            float waitTime = 0.5f / hoeAnimationSpeed; // Base duration / speed
            yield return new WaitForSeconds(waitTime);
            
            // Reset animation speed to normal
            animator.speed = 1f;
            
            // Use the tool
            tool.Use(cellPosition, tileManager);
            HandleToolConsumption(tool);
            
            // Stop hoeing animation
            animator.SetBool("isHoeing", false);
        }
        else
        {
            // Fallback if no animator
            tool.Use(cellPosition, tileManager);
            HandleToolConsumption(tool);
        }
        
        isUsingTool = false;
    }

    private Vector2 GetHoeBlendDirection(Vector3 direction)
    {
        // Ensure direction is normalized
        direction = direction.normalized;
        
        // Determine primary direction based on larger component
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement is stronger
            if (direction.x > 0)
            {
                return new Vector2(1, 0); // Right
            }
            else
            {
                return new Vector2(-1, 0); // Left
            }
        }
        else
        {
            // Vertical movement is stronger (or equal)
            if (direction.y > 0)
            {
                return new Vector2(0, 1); // Up
            }
            else
            {
                return new Vector2(0, -1); // Down (default)
            }
        }
    }

    private void HandleToolConsumption(Tool tool)
    {
        // Handle tool consumption
        if (tool.IsConsumable())
            {
            bool stillUsable = tool.ConsumeOnUse();
                
                // If tool is depleted, remove it
                if (!stillUsable)
                {
                    tools[selectedToolIndex] = null;
                
                // Force refresh the specific slot to clear DragDropHandler cache
                if (toolbarUI != null)
                {
                    var toolbarSlots = toolbarUI.GetToolbarSlots();
                    if (selectedToolIndex < toolbarSlots.Count)
                    {
                        toolbarSlots[selectedToolIndex].SetEmpty();
                    }
                }
                }
                
                // Update display để show quantity mới hoặc xóa tool
                UpdateToolbarDisplay();
        }
    }

    private void UpdatePlayerAnimation()
    {
        if (playerMovement != null && playerMovement.GetAnimator() != null && SelectedTool != null)
        {
            var animator = playerMovement.GetAnimator();
            animator.SetInteger("ToolIndex", SelectedTool.animatorToolIndex);
        }
        else if (playerMovement != null && playerMovement.GetAnimator() != null)
        {
            var animator = playerMovement.GetAnimator();
            animator.SetInteger("ToolIndex", 0); // Default/no tool
        }
    }

    private void UpdateToolbarUI()
    {
        if (toolbarUI == null || tools == null) return;

        var toolbarSlots = toolbarUI.GetToolbarSlots();
        
        for (int i = 0; i < toolbarSlots.Count && i < tools.Length; i++)
        {
            // Initialize drag drop functionality
            toolbarSlots[i].InitializeDragDrop(SlotType.Toolbar, i);
            
            if (tools[i] != null)
            {
                toolbarSlots[i].SetTool(tools[i]);
            }
            else
            {
                // Force clear the slot to ensure DragDropHandler is properly reset
                toolbarSlots[i].SetEmpty();
            }
        }
    }

    // Public method để các script khác có thể check tool hiện tại
    public bool IsToolSelected(System.Type toolType)
    {
        return SelectedTool?.GetType() == toolType;
    }

    public T GetSelectedTool<T>() where T : Tool
    {
        return SelectedTool as T;
    }

    // Methods for drag & drop support
    public void SetToolAtIndex(int index, Tool tool)
    {
        if (index >= 0 && index < tools.Length)
        {
            tools[index] = tool;
        }
    }

    public Tool GetToolAtIndex(int index)
    {
        if (index >= 0 && index < tools.Length)
        {
            return tools[index];
        }
        return null;
    }

    public void UpdateToolbarDisplay()
    {
        UpdateToolbarUI();
    }

    /// <summary>
    /// Public method để trigger tool use từ scripts khác (như NewPlayerInteraction)
    /// </summary>
    public void TriggerToolUse(Vector3Int cellPosition)
    {
        UseTool(cellPosition);
    }

    /// <summary>
    /// Check if player is currently using a tool (for preventing multiple actions)
    /// </summary>
    public bool IsUsingTool()
    {
        return isUsingTool;
    }

    /// <summary>
    /// Check if target cell is within interaction range of player
    /// </summary>
    private bool IsWithinInteractionRange(Vector3Int cellPosition)
    {
        if (playerMovement == null) return false;
        
        Vector3 playerPos = playerMovement.transform.position;
        Vector3 targetPos = tileManager.GetTilemap().GetCellCenterWorld(cellPosition);
        float distance = Vector3.Distance(playerPos, targetPos);
        
        return distance <= maxInteractionDistance;
    }
} 