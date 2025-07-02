using UnityEngine;
using System.Collections;
using System;

public class GameLoader : MonoBehaviour
{
    public static GameLoader Instance { get; private set; }

    [SerializeField] private string loadPath = "/game/load";
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool showDebugInfo = true;

    [Serializable]
    private class PlayerStateServerResponse
    {
        public int code;
        public string message;
        public PlayerStateData data;
    }

    [Serializable]
    private class PlayerStateData
    {
        public int userId;
        public int money;
        public float posX;
        public float posY;
        public float posZ;
        public float health;
        public float hunger;
    }

    [Serializable]
    private class ToolbarServerResponse
    {
        public int code;
        public string message;
        public ToolbarData data;
    }

    [Serializable]
    private class ToolbarData
    {
        public int playerStateId;
        public ToolSlotData[] tools;
        public string lastSaved;
    }

    [Serializable]
    private class ToolSlotData
    {
        public int slotIndex;
        public string toolType;
        public string toolName;
        public int quantity;
        public int animatorToolIndex;
        public string iconPath;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (autoLoadOnStart)
        {
            // Bắt đầu thử load với retry mechanism
            StartCoroutine(LoadGameStateWithRetry());
        }
    }

    private IEnumerator LoadGameStateWithRetry()
    {
        int maxRetries = 10;
        float retryDelay = 0.5f;
        
        for (int i = 0; i < maxRetries; i++)
        {
            if (ApiClient.Instance != null)
            {
                if (showDebugInfo)
                    Debug.Log("[GameLoader] ApiClient found, starting load");
                LoadGameState();
                yield break;
            }
            
            if (showDebugInfo)
                Debug.Log($"[GameLoader] ApiClient not ready, retry {i + 1}/{maxRetries}");
            
            yield return new WaitForSeconds(retryDelay);
        }
        
        Debug.LogError("[GameLoader] Failed to find ApiClient after retries!");
    }

    public void LoadGameState()
    {
        int userId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (userId <= 0)
        {
            if (showDebugInfo)
                Debug.LogWarning("[GameLoader] No valid user ID found, skipping load");
            return;
        }

        if (ApiClient.Instance == null)
        {
            if (showDebugInfo)
                Debug.LogError("[GameLoader] ApiClient not found!");
            return;
        }

        string fullPath = $"{loadPath}/{userId}";
        if (showDebugInfo)
            Debug.Log($"[GameLoader] Loading game state for user {userId}");

        StartCoroutine(ApiClient.Instance.Get(
            fullPath,
            onSuccess: HandleLoadSuccess,
            onError: HandleLoadError
        ));
    }

    private void HandleLoadSuccess(string responseJson)
    {
        try
        {
            if (string.IsNullOrEmpty(responseJson))
            {
                if (showDebugInfo)
                    Debug.LogWarning("[GameLoader] Empty response from server");
                return;
            }

            var response = JsonUtility.FromJson<PlayerStateServerResponse>(responseJson);
            
            if (response == null || response.data == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning("[GameLoader] Invalid response format");
                return;
            }

            // Load player position
            var player = FindObjectOfType<Player>();
            if (player != null)
            {
                Vector3 loadedPosition = new Vector3(response.data.posX, response.data.posY, response.data.posZ);
                player.transform.position = loadedPosition;
                
                if (showDebugInfo)
                    Debug.Log($"[GameLoader] Loaded player position: {loadedPosition}");
            }

            // Load player money
            var wallet = FindObjectOfType<Wallet>();
            if (wallet != null)
            {
                wallet.SetMoney(response.data.money);
                
                if (showDebugInfo)
                    Debug.Log($"[GameLoader] Loaded player money: {response.data.money}");
            }

            // Load player health and hunger
            var playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.LoadFromServer(response.data.health, response.data.hunger);
                
                if (showDebugInfo)
                    Debug.Log($"[GameLoader] Loaded player stats - Health: {response.data.health}, Hunger: {response.data.hunger}");
            }

            // Load toolbar after player state
            LoadToolbar(response.data.userId);

            if (showDebugInfo)
                Debug.Log($"[GameLoader] Successfully loaded game state for user {response.data.userId}");

        }
        catch (Exception e)
        {
            if (showDebugInfo)
                Debug.LogError($"[GameLoader] Error parsing load response: {e.Message}");
        }
    }

    private void HandleLoadError(string error)
    {
        if (showDebugInfo)
            Debug.LogWarning($"[GameLoader] Failed to load game state: {error}");
    }

    private void LoadToolbar(int userId)
    {
        if (userId <= 0)
        {
            if (showDebugInfo)
                Debug.LogWarning("[GameLoader] Invalid userId for toolbar loading");
            return;
        }

        if (ApiClient.Instance == null)
        {
            if (showDebugInfo)
                Debug.LogError("[GameLoader] ApiClient not found for toolbar loading!");
            return;
        }

        string toolbarPath = $"/game/toolbar/load/{userId}";
        if (showDebugInfo)
            Debug.Log($"[GameLoader] Loading toolbar for user {userId}");

        StartCoroutine(ApiClient.Instance.Get(
            toolbarPath,
            onSuccess: HandleToolbarLoadSuccess,
            onError: HandleToolbarLoadError
        ));
    }

    private void HandleToolbarLoadSuccess(string responseJson)
    {
        try
        {
            if (string.IsNullOrEmpty(responseJson))
            {
                if (showDebugInfo)
                    Debug.LogWarning("[GameLoader] Empty toolbar response from server");
                return;
            }

            var response = JsonUtility.FromJson<ToolbarServerResponse>(responseJson);
            
            if (response == null || response.data == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning("[GameLoader] Invalid toolbar response format");
                return;
            }

            // Convert server response to Unity format
            var toolbarData = new ToolbarSaveData();
            toolbarData.userId = response.data.playerStateId;
            
            if (response.data.tools != null)
            {
                foreach (var serverTool in response.data.tools)
                {
                    var toolSlot = new ToolSlotSaveData
                    {
                        slotIndex = serverTool.slotIndex,
                        toolType = serverTool.toolType,
                        toolName = serverTool.toolName,
                        quantity = serverTool.quantity,
                        animatorToolIndex = serverTool.animatorToolIndex,
                        iconPath = serverTool.iconPath
                    };
                    toolbarData.tools.Add(toolSlot);
                }
            }

            // Apply to ToolManager
            var toolManager = FindObjectOfType<ToolManager>();
            if (toolManager != null)
            {
                toolManager.LoadFromServer(toolbarData);
                
                if (showDebugInfo)
                    Debug.Log($"[GameLoader] ✅ Toolbar loaded successfully with {toolbarData.tools.Count} tools");
            }
            else
            {
                if (showDebugInfo)
                    Debug.LogWarning("[GameLoader] ToolManager not found, toolbar not applied");
            }
        }
        catch (Exception e)
        {
            if (showDebugInfo)
                Debug.LogError($"[GameLoader] Error parsing toolbar response: {e.Message}");
        }
    }

    private void HandleToolbarLoadError(string error)
    {
        if (showDebugInfo)
            Debug.LogWarning($"[GameLoader] Failed to load toolbar: {error}");
    }
} 