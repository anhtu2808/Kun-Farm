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
    [SerializeField] private float chickenFeedingDistance = 2.0f; // Maximum distance to feed chickens (2D)

    [Header("References")]
    [SerializeField] private Toolbar_UI toolbarUI;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private Movement playerMovement; // Changed from playerAnimator to playerMovement
    [SerializeField] private Inventory inventoryUI;

    private Tool[] tools; // Mảng tools trong toolbar (9 slots)
    private int selectedToolIndex = 0;
    private bool isUsingTool = false;

    [Header("Auto-Save Settings")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 30f; // 30 seconds
    [SerializeField] private bool showDebugInfo = false;

    // Auto-save tracking
    private bool hasToolbarChanges = false;
    private float lastAutoSaveTime = 0f;
    private int currentUserId = 0;

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

        // Get user ID from PlayerPrefs
        currentUserId = PlayerPrefs.GetInt("PLAYER_ID", 0);

        // Đảm bảo Hand Tool luôn có ở slot đầu tiên
        EnsureHandTool();


    }

    void Update()
    {
        // Listen for toolbar selection changes
        CheckToolSelection();

        // Listen for food eating (Space key)
        CheckFoodEating();

        // Auto-save toolbar if changes detected
        CheckAutoSave();
    }

    private void InitializeTools()
    {
        tools = new Tool[9]; // 9 slots như trong Toolbar_UI

        // Luôn đặt Hand Tool ở slot đầu tiên (index 0) với icon từ ToolData
        tools[0] = CreateHandToolWithIcon();

        // Initialize tools từ ToolData array (bắt đầu từ slot 1)
        for (int i = 0; i < toolDataArray.Length && i + 1 < tools.Length; i++)
        {
            if (toolDataArray[i] != null)
            {
                // Skip HandTool ToolData vì đã được set ở slot 0
                if (toolDataArray[i].toolType != ToolType.Hand)
                {
                    tools[i + 1] = toolDataArray[i].CreateTool();
                }
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

    private void CheckFoodEating()
    {
        // Don't allow eating if inventory is open or tool is being used
        if (inventoryUI != null && inventoryUI.IsOpen)
            return;
        if (isUsingTool)
            return;

        // Check for Space key input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Tool currentTool = SelectedTool;
            if (currentTool is FoodTool foodTool && foodTool.quantity > 0)
            {
                // Check if this is wheat - use for chicken feeding only
                if (IsWheatFood(foodTool))
                {
                    HandleWheatFeeding(foodTool);
                }
                else
                {
                    // For other foods (Apple, Grape), player eats normally
                    foodTool.EatFood();
                    HandleToolConsumption(foodTool);
                }
            }
        }
    }

    private bool IsWheatFood(FoodTool foodTool)
    {
        return foodTool.toolName != null &&
               (foodTool.toolName.ToLower().Contains("wheat") ||
                foodTool.toolName.ToLower().Contains("lúa"));
    }

    private void HandleWheatFeeding(FoodTool wheatTool)
    {
        // Find the nearest chicken within feeding distance
        ChickenWalk nearestChicken = FindNearestChicken();

        if (nearestChicken != null)
        {
            // Feed the chicken with wheat
            bool feedSuccess = nearestChicken.Feed(CollectableType.WHEAT);

            if (feedSuccess)
            {
                // Consume the wheat from toolbar
                HandleToolConsumption(wheatTool);

                // Get feeding info from ChickenManager for detailed message
                if (ChickenManager.Instance != null)
                {
                    float speedBoost = ChickenManager.Instance.GetFeedingSpeedBoost();
                    float duration = ChickenManager.Instance.GetFeedingDuration();
                    float baseInterval = ChickenManager.Instance.GetDefaultEggLayInterval();

                    float timeReduced = baseInterval * speedBoost; // seconds saved
                    int durationMinutes = Mathf.RoundToInt(duration / 60f);
                    int boostPercent = Mathf.RoundToInt(speedBoost * 100f);

                    // Show detailed notification
                    SimpleNotificationPopup.Show($"🌾 Fed chicken with {wheatTool.toolName}!\n⚡ Egg laying speed +{boostPercent}% ({timeReduced:F0}s faster)\n⏰ Effect lasts {durationMinutes} minutes");
                }
                else
                {
                    // Fallback message
                    SimpleNotificationPopup.Show($"Fed chicken with {wheatTool.toolName}!");
                }


            }

        }
        else
        {
            // No chicken nearby - show message with distance info
            SimpleNotificationPopup.Show($"🐔 No chicken nearby to feed!\n📏 Move within {chickenFeedingDistance:F1} units of a chicken");
        }
    }

    private ChickenWalk FindNearestChicken()
    {
        if (ChickenManager.Instance == null || playerMovement == null)
            return null;

        Vector3 playerPos = playerMovement.transform.position;
        ChickenWalk nearestChicken = null;
        float nearestDistance = float.MaxValue;

        // Get all chickens from ChickenManager
        var allChickens = ChickenManager.Instance.GetAllChickens();

        foreach (var chicken in allChickens)
        {
            if (chicken == null) continue;

            Vector3 chickenPos = chicken.transform.position;

            // Calculate 2D distance (ignore Z axis) for proper 2D game distance
            Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.y);
            Vector2 chickenPos2D = new Vector2(chickenPos.x, chickenPos.y);
            float distance = Vector2.Distance(playerPos2D, chickenPos2D);

            // Check if within feeding distance and closer than previous
            if (distance <= chickenFeedingDistance && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestChicken = chicken;
            }
        }

        return nearestChicken;
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
            return;
        }

        Tool currentTool = SelectedTool;
        if (currentTool != null && currentTool.CanUse(cellPosition, tileManager))
        {
            // Don't use food tools through normal interaction (only through Space key)
            if (currentTool is FoodTool)
            {
                return;
            }

            // Check if this is a shovel tool for hoeing animation
            if (currentTool is ShovelTool)
            {
                StartCoroutine(PlayHoeAnimation(cellPosition, currentTool));
            }
            // Check if this is a watering can tool for watering animation
            else if (currentTool is WateringCanTool)
            {
                StartCoroutine(PlayWateringAnimation(cellPosition, currentTool));
            }
            else if (currentTool is AxeTool axe)
            {
                StartCoroutine(PlayChopAnimation(cellPosition, axe));
            }
            else
            {
                // Use other tools immediately
                currentTool.Use(cellPosition, tileManager);
                HandleToolConsumption(currentTool);
            }
        }
    }

    private IEnumerator PlayChopAnimation(Vector3Int cellPosition, AxeTool axe)
    {
        isUsingTool = true;

        if (playerMovement != null && playerMovement.GetAnimator() != null)
        {
            var animator = playerMovement.GetAnimator();

            // Tương tự: lấy hướng từ player tới ô cây
            Vector3 playerPos = playerMovement.transform.position;
            Vector3 targetPos = tileManager.GetTilemap().GetCellCenterWorld(cellPosition);
            Vector3 directionToTarget = (targetPos - playerPos).normalized;

            // Nếu gần quá thì dùng hướng đang quay mặt
            Vector3 finalDirection = (Vector3.Distance(playerPos, targetPos) < 0.5f)
                ? playerMovement.GetFacingDirection()
                : directionToTarget;

            // Xác định trigger animation dựa trên hướng
            string trigger;
            if (Mathf.Abs(finalDirection.x) > Mathf.Abs(finalDirection.y))
                trigger = finalDirection.x > 0 ? "Chop_Right" : "Chop_Left";
            else
                trigger = finalDirection.y > 0 ? "Chop_Up" : "Chop_Down";

            // Reset hết rồi set trigger
            animator.ResetTrigger("Chop_Down");
            animator.ResetTrigger("Chop_Left");
            animator.ResetTrigger("Chop_Right");
            animator.ResetTrigger("Chop_Up");
            animator.SetTrigger(trigger);

            // Chờ animation (giả sử mỗi clip ~0.5s, có thể điều chỉnh)
            float waitTime = 0.5f;
            yield return new WaitForSeconds(waitTime);

            // Thực hiện hành động chặt
            axe.Use(cellPosition, tileManager);
            HandleToolConsumption(axe);
        }
        else
        {
            // Fallback nếu không có animator
            axe.Use(cellPosition, tileManager);
            HandleToolConsumption(axe);
        }

        isUsingTool = false;
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

    private IEnumerator PlayWateringAnimation(Vector3Int cellPosition, Tool tool)
    {
        isUsingTool = true;

        // Debug log to see if method is called
        Debug.Log($"[ToolManager] PlayWateringAnimation called with tool: {tool?.toolName ?? "null"}");

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

            // Determine watering direction using 2D blend tree coordinates
            Vector2 wateringBlendDirection = GetHoeBlendDirection(finalDirection); // Reuse same logic

            // Debug logs for animation parameters
            Debug.Log($"[ToolManager] Setting animation parameters - horizontal: {wateringBlendDirection.x}, vertical: {wateringBlendDirection.y}");
            Debug.Log($"[ToolManager] Setting isWatering to true");

            // Set animator parameters for 2D blend tree
            animator.SetFloat("horizontal", wateringBlendDirection.x);
            animator.SetFloat("vertical", wateringBlendDirection.y);
            animator.SetBool("isWatering", true); // Assumes you have this bool parameter

            // Apply animation speed if specified
            if (hoeAnimationSpeed != 1f)
            {
                animator.speed = hoeAnimationSpeed;
            }

            // Wait for animation to play (dynamic based on animation speed)
            float waitTime = 0.7f / hoeAnimationSpeed; // Watering takes slightly longer
            Debug.Log($"[ToolManager] Waiting {waitTime} seconds for watering animation");
            yield return new WaitForSeconds(waitTime);

            // Reset animation speed to normal
            animator.speed = 1f;

            // Use the tool
            Debug.Log($"[ToolManager] Using watering tool on position: {cellPosition}");
            tool.Use(cellPosition, tileManager);
            HandleToolConsumption(tool);

            // Stop watering animation
            Debug.Log("[ToolManager] Setting isWatering to false");
            animator.SetBool("isWatering", false);
        }
        else
        {
            // Fallback if no animator
            Debug.LogWarning("[ToolManager] No animator found, using tool without animation");
            tool.Use(cellPosition, tileManager);
            HandleToolConsumption(tool);
        }

        isUsingTool = false;
        Debug.Log("[ToolManager] PlayWateringAnimation completed");
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

            // Mark toolbar as changed for auto-save
            hasToolbarChanges = true;

            // Update display để show quantity mới hoặc xóa tool
            UpdateToolbarDisplay();

            // Đảm bảo Hand Tool luôn có ở slot đầu tiên (nếu bị xóa do lỗi)
            EnsureHandTool();
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

    /// <summary>
    /// Set tool tại index cụ thể
    /// </summary>
    public void SetToolAtIndex(int index, Tool tool)
    {
        if (index >= 0 && index < tools.Length)
        {
            // Đặc biệt bảo vệ slot 0 - chỉ cho phép HandTool
            if (index == 0 && tool != null && !(tool is HandTool))
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[ToolManager] Cannot place {tool.GetType().Name} at slot 0. Slot 0 is reserved for HandTool.");
                return;
            }

            tools[index] = tool;

            // Mark toolbar as changed for auto-save
            hasToolbarChanges = true;

            if (showDebugInfo)
                Debug.Log($"[ToolManager] Tool changed at index {index}: {tool?.toolName ?? "null"}");
        }
    }

    /// <summary>
    /// Get tool tại index cụ thể
    /// </summary>
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

        // Calculate 2D distance (ignore Z axis) for proper 2D game distance
        Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.y);
        Vector2 targetPos2D = new Vector2(targetPos.x, targetPos.y);
        float distance = Vector2.Distance(playerPos2D, targetPos2D);

        return distance <= maxInteractionDistance;
    }

    private void CheckAutoSave()
    {
        if (!autoSaveEnabled || !hasToolbarChanges || currentUserId <= 0)
            return;

        if (Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            SaveToolbar();
            lastAutoSaveTime = Time.time;
            hasToolbarChanges = false;
        }
    }

    public void SaveToolbar()
    {
        if (currentUserId <= 0)
        {
            if (showDebugInfo)
                Debug.LogWarning("[ToolManager] Cannot save toolbar: invalid userId");
            return;
        }

        if (ApiClient.Instance == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("[ToolManager] Cannot save toolbar: ApiClient not found");
            return;
        }

        var toolbarData = ToolbarSaveData.FromToolManager(this, currentUserId);
        string json = JsonUtility.ToJson(toolbarData);

        if (showDebugInfo)
            Debug.Log($"[ToolManager] Saving toolbar for user {currentUserId}");

        StartCoroutine(ApiClient.Instance.PostJson(
            "/game/toolbar/save",
            json,
            onSuccess: HandleSaveSuccess,
            onError: HandleSaveError
        ));
    }

    private void HandleSaveSuccess(string responseJson)
    {
        if (showDebugInfo)
            Debug.Log("[ToolManager] ✅ Toolbar saved successfully");
    }

    private void HandleSaveError(string error)
    {
        if (showDebugInfo)
            Debug.LogError($"[ToolManager] ❌ Failed to save toolbar: {error}");
    }

    public void LoadFromServer(ToolbarSaveData toolbarData)
    {
        if (toolbarData != null)
        {
            toolbarData.ApplyToToolManager(this);

            if (showDebugInfo)
                Debug.Log($"[ToolManager] ✅ Toolbar loaded from server with {toolbarData.tools.Count} tools");
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("[ToolManager] No toolbar data received from server");
        }
    }

    /// <summary>
    /// Tạo Hand Tool với icon từ ToolData
    /// </summary>
    private Tool CreateHandToolWithIcon()
    {
        // Tìm HandTool ToolData trong toolDataArray
        ToolData handToolData = null;
        for (int i = 0; i < toolDataArray.Length; i++)
        {
            if (toolDataArray[i] != null && toolDataArray[i].toolType == ToolType.Hand)
            {
                handToolData = toolDataArray[i];
                break;
            }
        }

        // Nếu tìm thấy ToolData, sử dụng nó để tạo tool với icon
        if (handToolData != null)
        {
            Tool handTool = handToolData.CreateTool();
            if (showDebugInfo)
                Debug.Log($"[ToolManager] Created HandTool with icon: {handTool.toolIcon?.name ?? "null"}");
            return handTool;
        }

        // Fallback: tạo HandTool mới và thử load icon từ Resources
        var fallbackHand = new HandTool();

        // Thử load icon từ các path có thể
        Sprite handIcon = Resources.Load<Sprite>("Sprites/hand_icon") ??
                         Resources.Load<Sprite>("Tools/hand_icon") ??
                         Resources.Load<Sprite>("hand_icon");

        if (handIcon != null)
        {
            fallbackHand.toolIcon = handIcon;
            if (showDebugInfo)
                Debug.Log($"[ToolManager] Created HandTool with fallback icon: {handIcon.name}");
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning("[ToolManager] No icon found for HandTool!");
        }

        return fallbackHand;
    }

    /// <summary>
    /// Đảm bảo Hand Tool luôn có ở slot đầu tiên (index 0)
    /// </summary>
    public void EnsureHandTool()
    {
        if (tools == null || tools.Length == 0) return;

        // Kiểm tra nếu slot 0 không có tool hoặc không phải HandTool
        if (tools[0] == null || !(tools[0] is HandTool))
        {
            // Tìm HandTool ở slot khác (nếu có)
            HandTool existingHand = null;
            int existingHandIndex = -1;

            for (int i = 1; i < tools.Length; i++)
            {
                if (tools[i] is HandTool handTool)
                {
                    existingHand = handTool;
                    existingHandIndex = i;
                    break;
                }
            }

            // Nếu có HandTool ở slot khác, move về slot 0
            if (existingHand != null)
            {
                tools[existingHandIndex] = tools[0]; // Move tool ở slot 0 sang slot cũ của hand
                tools[0] = existingHand;

                if (showDebugInfo)
                    Debug.Log($"[ToolManager] Moved HandTool from slot {existingHandIndex} to slot 0");
            }
            else
            {
                // Nếu không có HandTool nào, tạo mới ở slot 0 với icon
                tools[0] = CreateHandToolWithIcon();

                if (showDebugInfo)
                    Debug.Log("[ToolManager] Created new HandTool with icon at slot 0");
            }

            // Update display
            UpdateToolbarDisplay();
            hasToolbarChanges = true; // Mark for save
        }
    }
}
