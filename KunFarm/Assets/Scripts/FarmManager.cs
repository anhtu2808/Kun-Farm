using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;
using UnityEngine.Networking;

public class FarmManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TileManager tileManager;
    [SerializeField] private Player player;

    [Header("Save/Load Controls (Optional)")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    [Header("Settings")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 30f; // 30 seconds

    private float lastAutoSaveTime;
    private int currentUserId = 0; // Will be loaded from PlayerPrefs
    private bool hasTriedLogin = false;
    private bool isLoading = false; // Flag to prevent save during load
    private float loadCompleteTime = 0f; // Time when load completed

    void Start()
    {
        if (tileManager == null)
            tileManager = FindObjectOfType<TileManager>();
        if (player == null)
            player = FindObjectOfType<Player>();

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveFarmState);
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadFarmState);

        TryGetUserIdFromLogin();
        Invoke(nameof(LoadFarmState), 1f);
    }

    void Update()
    {
        if (autoSaveEnabled && Time.timeScale > 0 && Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            // Don't auto-save if loading or within 5 seconds after load complete
            if (!isLoading && (loadCompleteTime == 0f || Time.time - loadCompleteTime > 5f))
            {
                SaveFarmState();
                lastAutoSaveTime = Time.time;
            }
        }
    }

    public void SetUserId(int userId)
    {
        currentUserId = userId;
    }

    public void SaveFarmState()
    {
        if (tileManager == null || ApiClient.Instance == null)
        {
            Debug.LogWarning("TileManager or ApiClient not found!");
            return;
        }

        var farmData = FarmSaveData.FromTileManager(tileManager, currentUserId);
        Debug.Log($"[FarmManager] Saving farm state for user {currentUserId} - {farmData.tileStates.Count} tiles, {farmData.plants.Count} plants");
        
        // Debug detailed save data
        foreach (var tile in farmData.tileStates)
        {
            Debug.Log($"[FarmManager] Saving tile: ({tile.x}, {tile.y}, {tile.z}) - state: {tile.state}");
        }
        foreach (var plant in farmData.plants)
        {
            Debug.Log($"[FarmManager] Saving plant: ({plant.x}, {plant.y}, {plant.z}) - cropType: '{plant.cropType}', stage: {plant.currentStage}, mature: {plant.isMature}");
        }
        
        // Use a lambda to match Action<bool> signature
        ApiClient.Instance.SaveFarmState(farmData, success => OnSaveComplete(success));
    }

    public void LoadFarmState()
    {
        if (ApiClient.Instance == null)
        {
            Debug.LogWarning("ApiClient not found!");
            return;
        }

        isLoading = true; // Set loading flag
        Debug.Log("[FarmManager] Starting farm state load...");
        ApiClient.Instance.LoadFarmState(currentUserId, OnLoadComplete);
    }

    // Wrapper to match ApiClient callback signature Action<bool>
    private void OnSaveComplete(bool success)
    {
        // Call full handler without response body
        OnSaveComplete(success, string.Empty);
    }

    // Detailed handler that accepts response body if available
    private void OnSaveComplete(bool success, string responseBody)
    {
        if (success)
        {
            Debug.Log($"[FarmManager] ✅ Farm state saved successfully for user {currentUserId}!");
        }
        else
        {
            Debug.LogError($"[FarmManager] ❌ Failed to save farm state for user {currentUserId}! Response: {responseBody}");
        }
    }

    private void TryGetUserIdFromLogin()
    {
        if (!hasTriedLogin)
        {
            hasTriedLogin = true;
            currentUserId = PlayerPrefs.GetInt("PLAYER_ID", 0);
            if (currentUserId > 0)
            {
                Debug.Log($"[FarmManager] Loaded user ID from PlayerPrefs: {currentUserId}");
            }
            else
            {
                Debug.LogWarning("[FarmManager] No valid user ID found in PlayerPrefs");
            }
        }
    }

    private void OnLoadComplete(FarmSaveData farmData)
    {
        Debug.Log("=== FARM LOAD COMPLETE DEBUG START ===");
        Debug.Log($"farmData null: {farmData == null}");
        Debug.Log($"tileManager null: {tileManager == null}");
        Debug.Log($"currentUserId: {currentUserId}");
        Debug.Log($"Time.time: {Time.time}");
        
        if (farmData != null)
        {
            Debug.Log($"farmData.userId: {farmData.userId}");
            Debug.Log($"farmData.tileStates count: {farmData.tileStates?.Count ?? 0}");
            Debug.Log($"farmData.plants count: {farmData.plants?.Count ?? 0}");
            Debug.Log($"farmData.chickensStateJson length: {farmData.chickensStateJson?.Length ?? 0}");
            Debug.Log($"farmData.eggsStateJson length: {farmData.eggsStateJson?.Length ?? 0}");
            
            if (farmData.tileStates != null)
            {
                for (int i = 0; i < Math.Min(3, farmData.tileStates.Count); i++)
                {
                    var tile = farmData.tileStates[i];
                    Debug.Log($"Sample tile {i}: ({tile.x}, {tile.y}, {tile.z}) state: {tile.state}");
                }
            }
        }
        
        if (tileManager != null)
        {
            Debug.Log($"TileManager GameObject active: {tileManager.gameObject.activeInHierarchy}");
            Debug.Log($"TileManager enabled: {tileManager.enabled}");
            // Test if GetTilemap() works
            try 
            {
                var tilemap = tileManager.GetTilemap();
                Debug.Log($"GetTilemap() result: {tilemap != null}");
            }
            catch (Exception e)
            {
                Debug.LogError($"GetTilemap() error: {e.Message}");
            }
        }
        
        if (farmData != null && tileManager != null)
        {
            Debug.Log($"[FarmManager] ✅ Farm state loaded successfully for user {currentUserId}!");
            
            // Clear existing farm content
            ClearExistingPlants();
            ClearExistingChickensAndEggs();
            
            // First restore base tile states from server
            tileManager.RestoreTileStates(farmData.tileStates);
            
            // Then restore plants - they will set correct tile states based on maturity
            tileManager.RestorePlants(farmData.plants);
            
            // Restore chickens and eggs
            RestoreChickens(farmData.chickensStateJson);
            RestoreEggs(farmData.eggsStateJson);
            
            Debug.Log("[FarmManager] Restore order: TileStates -> Plants -> Chickens -> Eggs");
            
            // Clear loading flag and set completion time
            isLoading = false;
            loadCompleteTime = Time.time;
            Debug.Log("[FarmManager] Farm restore completed, auto-save disabled for 5 seconds");
        }
        else
        {
            Debug.LogWarning($"[FarmManager] ❌ Failed to load farm state or empty data for user {currentUserId}.");
            isLoading = false; // Clear flag even on failure
        }
        
        Debug.Log("=== FARM LOAD COMPLETE DEBUG END ===");
    }

    private void ClearExistingPlants()
    {
        if (tileManager == null) return;

        var allPlants = tileManager.GetAllPlants();
        foreach (var plant in allPlants.Values)
        {
            if (plant != null)
                Destroy(plant);
        }

        var allTileStates = tileManager.GetAllTileStates();
        foreach (var kvp in allTileStates)
        {
            if (kvp.Value == TileState.Planted || kvp.Value == TileState.Harvested)
            {
                tileManager.SetTileState(kvp.Key, TileState.Dug);
            }
        }
    }

    private void ClearExistingChickensAndEggs()
    {
        // Clear existing chickens
        var existingChickens = FindObjectsOfType<ChickenWalk>();
        foreach (var chicken in existingChickens)
        {
            if (chicken != null)
            {
                // Unregister từ ChickenManager trước khi destroy
                if (ChickenManager.Instance != null)
                {
                    ChickenManager.Instance.UnregisterChicken(chicken);
                }
                Destroy(chicken.gameObject);
            }
        }
        
        // Clear existing eggs
        var existingEggs = FindObjectsOfType<Collectable>();
        foreach (var collectable in existingEggs)
        {
            if (collectable != null && collectable.type == CollectableType.EGG)
            {
                Destroy(collectable.gameObject);
            }
        }
        
        Debug.Log("[FarmManager] Cleared all existing chickens and eggs");
    }

    private void RestoreChickens(string chickensStateJson)
    {
        Debug.Log($"[FarmManager] RestoreChickens called with: '{chickensStateJson}' (length: {chickensStateJson?.Length ?? -1})");
        
        // Handle null, empty, or invalid JSON cases
        if (string.IsNullOrEmpty(chickensStateJson) || 
            chickensStateJson.Trim() == "null" || 
            chickensStateJson.Trim() == "[]" ||
            chickensStateJson.Trim() == "{}")
        {
            Debug.Log("[FarmManager] No chickens to restore (null/empty/invalid data)");
            return;
        }
        
        if (ChickenManager.Instance == null)
        {
            Debug.LogWarning("[FarmManager] ChickenManager not found, cannot restore chickens");
            return;
        }
        
        try
        {
            // Validate JSON format before parsing
            chickensStateJson = chickensStateJson.Trim();
            if (!chickensStateJson.StartsWith("{"))
            {
                Debug.LogWarning($"[FarmManager] Invalid chickens JSON format: '{chickensStateJson}' - skipping restore");
                return;
            }
            
            var wrapper = JsonUtility.FromJson<ChickenStatesWrapper>(chickensStateJson);
            if (wrapper?.chickens == null || wrapper.chickens.Count == 0) 
            {
                Debug.Log("[FarmManager] Empty chickens data after parsing");
                return;
            }
            
            Debug.Log($"[FarmManager] Restoring {wrapper.chickens.Count} chickens...");
            
            foreach (var chickenSaveState in wrapper.chickens)
            {
                Vector3 position = new Vector3(chickenSaveState.positionX, chickenSaveState.positionY, chickenSaveState.positionZ);
                GameObject restoredChicken = ChickenManager.Instance.SpawnChicken(position);
                
                if (restoredChicken != null)
                {
                    // Set chicken ID
                    var chickenData = restoredChicken.GetComponent<ChickenData>();
                    if (chickenData == null)
                    {
                        chickenData = restoredChicken.AddComponent<ChickenData>();
                    }
                    chickenData.chickenId = chickenSaveState.chickenId;
                    restoredChicken.name = chickenSaveState.chickenId;
                    
                    // Restore chicken state trong ChickenManager
                    var chickenWalk = restoredChicken.GetComponent<ChickenWalk>();
                    if (chickenWalk != null)
                    {
                        ChickenManager.Instance.RegisterChicken(chickenWalk, chickenSaveState.chickenId);
                        
                        // Get và update state
                        var chickenState = ChickenManager.Instance.GetChickenState(chickenSaveState.chickenId);
                        if (chickenState != null)
                        {
                            chickenState.position = position;
                            chickenState.isFed = chickenSaveState.isFed;
                            chickenState.feedEndTime = chickenSaveState.feedEndTime;
                            chickenState.totalEggsLaid = chickenSaveState.totalEggsLaid;
                            chickenState.currentEggLayInterval = chickenSaveState.currentEggLayInterval;
                            chickenState.baseEggLayInterval = chickenSaveState.baseEggLayInterval;
                            chickenState.isMoving = chickenSaveState.isMoving;
                            chickenState.speedMultiplier = chickenSaveState.speedMultiplier;
                            chickenState.isBoosted = chickenSaveState.isBoosted;
                        }
                        
                        // Update timing settings nếu cần
                        chickenWalk.eggLayInterval = chickenSaveState.currentEggLayInterval;
                        // Restore egg lay timer
                        chickenWalk.SetEggLayTimer(chickenSaveState.eggLayTimer);
                    }
                    
                    Debug.Log($"[FarmManager] Restored chicken: {chickenSaveState.chickenId} at {position} with eggLayTimer: {chickenSaveState.eggLayTimer:F1}s");
                }
                else
                {
                    Debug.LogError($"[FarmManager] Failed to spawn chicken: {chickenSaveState.chickenId}");
                }
            }
            
            Debug.Log($"[FarmManager] ✅ Successfully restored {wrapper.chickens.Count} chickens");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FarmManager] Error restoring chickens: {e.Message}\nJSON: '{chickensStateJson}'");
        }
    }

    private void RestoreEggs(string eggsStateJson)
    {
        Debug.Log($"[FarmManager] RestoreEggs called with: '{eggsStateJson}' (length: {eggsStateJson?.Length ?? -1})");
        
        // Handle null, empty, or invalid JSON cases
        if (string.IsNullOrEmpty(eggsStateJson) || 
            eggsStateJson.Trim() == "null" || 
            eggsStateJson.Trim() == "[]" ||
            eggsStateJson.Trim() == "{}")
        {
            Debug.Log("[FarmManager] No eggs to restore (null/empty/invalid data)");
            return;
        }
        
        if (ChickenManager.Instance == null)
        {
            Debug.LogWarning("[FarmManager] ChickenManager not found, cannot restore eggs");
            return;
        }
        
        try
        {
            // Validate JSON format before parsing
            eggsStateJson = eggsStateJson.Trim();
            if (!eggsStateJson.StartsWith("{"))
            {
                Debug.LogWarning($"[FarmManager] Invalid eggs JSON format: '{eggsStateJson}' - skipping restore");
                return;
            }
            
            var wrapper = JsonUtility.FromJson<EggStatesWrapper>(eggsStateJson);
            if (wrapper?.eggs == null || wrapper.eggs.Count == 0) 
            {
                Debug.Log("[FarmManager] Empty eggs data after parsing");
                return;
            }
            
            Debug.Log($"[FarmManager] Restoring {wrapper.eggs.Count} eggs...");
            
            foreach (var eggSaveState in wrapper.eggs)
            {
                Vector3 position = new Vector3(eggSaveState.positionX, eggSaveState.positionY, eggSaveState.positionZ);
                GameObject restoredEgg = ChickenManager.Instance.SpawnEgg(position);
                
                if (restoredEgg != null)
                {
                    restoredEgg.name = eggSaveState.eggId;
                    
                    // Restore egg properties including timing
                    var collectable = restoredEgg.GetComponent<Collectable>();
                    if (collectable != null)
                    {
                        collectable.type = CollectableType.EGG;
                        // Restore timing data
                        collectable.HatchTime = eggSaveState.hatchTime;
                        collectable.HatchTimer = eggSaveState.hatchTimer;
                    }
                    
                    Debug.Log($"[FarmManager] Restored egg: {eggSaveState.eggId} at {position}, hatchTimer: {eggSaveState.hatchTimer:F1}s/{eggSaveState.hatchTime:F1}s");
                }
                else
                {
                    Debug.LogError($"[FarmManager] Failed to spawn egg: {eggSaveState.eggId}");
                }
            }
            
            Debug.Log($"[FarmManager] ✅ Successfully restored {wrapper.eggs.Count} eggs");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FarmManager] Error restoring eggs: {e.Message}\nJSON: '{eggsStateJson}'");
        }
    }

    public void SaveAndQuit()
    {
        SaveFarmState();
        // Application.Quit();
    }

    // Method to clear corrupted farm data and reset to clean state
    [ContextMenu("Clear Farm State")]
    public void ClearFarmState()
    {
        if (tileManager != null)
        {
            ClearExistingPlants();
            // Reset all tiles to Undug
            var allTileStates = tileManager.GetAllTileStates();
            foreach (var kvp in allTileStates.Keys.ToList())
            {
                tileManager.SetTileState(kvp, TileState.Undug);
            }
        }
        
        // Clear chickens và eggs
        ClearExistingChickensAndEggs();
        
        // Save empty state to server
        SaveFarmState();
        Debug.Log("[FarmManager] Farm state cleared and saved!");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveEnabled && !isLoading && (loadCompleteTime == 0f || Time.time - loadCompleteTime > 5f))
            SaveFarmState();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && autoSaveEnabled)
        {
            float timeSinceLoad = loadCompleteTime > 0f ? Time.time - loadCompleteTime : 0f;
            Debug.Log($"[FarmManager] OnApplicationFocus: isLoading={isLoading}, timeSinceLoad={timeSinceLoad:F2}s, loadCompleteTime={loadCompleteTime:F2}");
            
            if (!isLoading && (loadCompleteTime == 0f || timeSinceLoad > 5f))
            {
                SaveFarmState();
            }
            else
            {
                Debug.Log("[FarmManager] Save blocked: Too soon after load or still loading");
            }
        }
    }
}
