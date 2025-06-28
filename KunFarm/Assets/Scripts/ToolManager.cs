using UnityEngine;

/// <summary>
/// Quản lý tools trong toolbar và tương tác với UI
/// </summary>
public class ToolManager : MonoBehaviour
{
    [Header("Tools Setup")]
    [SerializeField] private ToolData[] toolDataArray; // Mảng tools để setup trong editor
    
    [Header("References")]
    [SerializeField] private Toolbar_UI toolbarUI;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private Animator playerAnimator;

    private Tool[] tools; // Mảng tools trong toolbar (9 slots)
    private int selectedToolIndex = 0;
    
    public Tool SelectedTool => selectedToolIndex < tools.Length ? tools[selectedToolIndex] : null;

    void Start()
    {
        InitializeTools();
        UpdateToolbarUI();
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
        if (tileManager == null || !tileManager.IsInteractable(cellPosition))
            return;

        Tool currentTool = SelectedTool;
        if (currentTool != null && currentTool.CanUse(cellPosition, tileManager))
        {
            // Use the tool
            currentTool.Use(cellPosition, tileManager);
            
            // Handle tool consumption
            if (currentTool.IsConsumable())
            {
                bool stillUsable = currentTool.ConsumeOnUse();
                
                // If tool is depleted, remove it
                if (!stillUsable)
                {
                    tools[selectedToolIndex] = null;
                }
                
                // Update display để show quantity mới hoặc xóa tool
                UpdateToolbarDisplay();
            }
        }
    }

    private void UpdatePlayerAnimation()
    {
        if (playerAnimator != null && SelectedTool != null)
        {
            playerAnimator.SetInteger("ToolIndex", SelectedTool.animatorToolIndex);
        }
    }

    private void UpdateToolbarUI()
    {
        if (toolbarUI == null) return;

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
} 