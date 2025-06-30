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
    private int currentUserId = 1; // Default user ID, should be set from login
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
            Debug.Log($"[FarmManager] Using default user ID: {currentUserId}");
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
            ClearExistingPlants();
            
            // First restore base tile states from server
            tileManager.RestoreTileStates(farmData.tileStates);
            
            // Then restore plants - they will set correct tile states based on maturity
            tileManager.RestorePlants(farmData.plants);
            
            Debug.Log("[FarmManager] Restore order: TileStates first, then Plants (plants override tile states if needed)");
            
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
