using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Game Over Manager - Singleton pattern giống GameManager
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    
    [Header("Settings")]
    [SerializeField] private string gameOverMessage = "GAME OVER";
    [SerializeField] private string restartButtonText = "Play again";
    [SerializeField] private bool pauseGameOnDeath = true;
    [SerializeField] private float fadeInDuration = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Singleton pattern - giống GameManager
    public static GameOverManager Instance { get; private set; }
    
    private bool isGameOver = false;
    private PlayerStats playerStats;
    
    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameOverManager] Multiple instances detected. Destroying new instance.");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Auto-find UI components theo hierarchy của bạn
        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
        }
        
        if (restartButton == null)
        {
            // Tìm button PlayAgain
            GameObject playAgainObj = GameObject.Find("PlayAgain");
            if (playAgainObj != null)
            {
                restartButton = playAgainObj.GetComponent<Button>();
            }
        }
        
        if (gameOverText == null)
        {
            // Tìm GameOverText trực tiếp
            GameObject gameOverTextObj = GameObject.Find("GameOverText");
            if (gameOverTextObj != null)
            {
                gameOverText = gameOverTextObj.GetComponent<TMP_Text>();
            }
        }
        
        if (gameOverCanvasGroup == null && gameOverPanel != null)
        {
            // Tìm CanvasGroup component trên GameOverPanel
            gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            
            // Nếu không có, tự tạo CanvasGroup component
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
                if (showDebugLogs)
                {
                    Debug.Log("[GameOverManager] Auto-created CanvasGroup on GameOverPanel");
                }
            }
        }
        
        // Setup initial state
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    void Start()
    {
        // Find PlayerStats reference
        playerStats = FindObjectOfType<PlayerStats>();
        
        // Setup UI
        SetupUI();
        
        if (showDebugLogs)
        {
            Debug.Log($"[GameOverManager] Initialized - Panel: {gameOverPanel != null}, Button: {restartButton != null}, Text: {gameOverText != null}, CanvasGroup: {gameOverCanvasGroup != null}");
        }
    }
    
    void OnEnable()
    {
        // Subscribe to player death event
        PlayerStats.OnPlayerDied += HandlePlayerDeath;
    }
    
    void OnDisable()
    {
        // Unsubscribe from events
        PlayerStats.OnPlayerDied -= HandlePlayerDeath;
    }
    
    private void SetupUI()
    {
        // Setup game over text
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
        }
        
        // Setup restart button
        if (restartButton != null)
        {
            // Support both UI.Text and TextMeshPro
            var buttonText = restartButton.GetComponentInChildren<Text>();
            var buttonTextTMP = restartButton.GetComponentInChildren<TMP_Text>();
            
            if (buttonText != null)
            {
                buttonText.text = restartButtonText;
            }
            else if (buttonTextTMP != null)
            {
                buttonTextTMP.text = restartButtonText;
            }
            
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
        
        // Setup canvas group for fade effects
        if (gameOverCanvasGroup == null && gameOverPanel != null)
        {
            gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        }
    }
    
    private void HandlePlayerDeath()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[GameOverManager] Player died, showing game over screen");
        }
        
        StartCoroutine(ShowGameOverScreen());
    }
    
    private IEnumerator ShowGameOverScreen()
    {
        // Pause game if enabled
        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }
        
        // Show panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Fade in effect
        if (gameOverCanvasGroup != null)
        {
            float elapsedTime = 0f;
            gameOverCanvasGroup.alpha = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime; // Use unscaled time since game might be paused
                gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            gameOverCanvasGroup.alpha = 1f;
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[GameOverManager] Game over screen displayed");
        }
    }
    
    public void RestartGame()
    {
        if (showDebugLogs)
        {
            Debug.Log("[GameOverManager] Restarting game...");
        }
        
        StartCoroutine(RestartGameCoroutine());
    }
    
    private IEnumerator RestartGameCoroutine()
    {
        // Set restart flag to prevent save operations during restart
        var farmManager = FindObjectOfType<FarmManager>();
        if (farmManager != null)
        {
            farmManager.SetRestarting(true);
        }
        
        // Resume time scale
        Time.timeScale = 1f;
        
        // Get user ID for reset
        int userId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        
        if (userId > 0 && ApiClient.Instance != null)
        {
            // Call API to reset game state
            bool resetSuccess = false;
            bool resetCompleted = false;
            
            if (showDebugLogs)
            {
                Debug.Log($"[GameOverManager] Resetting game data for user {userId}");
            }
            
            ApiClient.Instance.ResetGame(userId, (success) => {
                resetSuccess = success;
                resetCompleted = true;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[GameOverManager] Reset API completed - Success: {success}");
                }
            });
            
            // Wait for API call to complete
            float timeout = 5f;
            float elapsed = 0f;
            
            while (!resetCompleted && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            
            if (showDebugLogs)
            {
                if (resetCompleted && resetSuccess)
                {
                    Debug.Log("[GameOverManager] Game reset successful, resetting local state");
                }
                else
                {
                    Debug.LogWarning($"[GameOverManager] Reset API issue (completed: {resetCompleted}, success: {resetSuccess}), still resetting local state");
                }
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("[GameOverManager] No valid user ID or ApiClient, resetting local state anyway");
            }
        }
        
        // Reset local game state without reloading scene
        ResetLocalGameState();
        
        // Re-enable save operations after restart is complete
        var farmManagerAfterReset = FindObjectOfType<FarmManager>();
        if (farmManagerAfterReset != null)
        {
            farmManagerAfterReset.SetRestarting(false);
        }
        
        yield return null;
    }
    
    private void ResetLocalGameState()
    {
        if (showDebugLogs)
        {
            Debug.Log("[GameOverManager] Resetting local game state");
        }
        
        // 1. Reset player stats to full
        if (playerStats != null)
        {
            playerStats.Revive(); // This should restore health and hunger to full
            isGameOver = false;
        }
        
        // 2. Reset player position to spawn point
        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 0f, 0f); // Spawn position
            
            if (showDebugLogs)
            {
                Debug.Log("[GameOverManager] Reset player position to spawn point");
            }
        }
        
        // 3. Reset money to 0
        var wallet = FindObjectOfType<Wallet>();
        if (wallet != null)
        {
            wallet.SetMoney(1000);
            
            if (showDebugLogs)
            {
                Debug.Log("[GameOverManager] Reset money to 0");
            }
        }
        
        // 4. Clear farm tiles and plants
        var farmManager = FindObjectOfType<FarmManager>();
        if (farmManager != null)
        {
            // Use existing ClearFarmState method
            farmManager.ClearFarmState();
            
            if (showDebugLogs)
            {
                Debug.Log("[GameOverManager] Cleared farm state using FarmManager.ClearFarmState()");
            }
        }
        
        // 5. Clear inventory slots
        var inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            // Clear all 27 slots individually
            for (int i = 0; i < inventory.slots.Count; i++)
            {
                inventory.ClearSlot(i);
            }
            
            // Refresh inventory UI
            inventory.Refresh();
            
            if (showDebugLogs)
            {
                Debug.Log($"[GameOverManager] Cleared all {inventory.slots.Count} inventory slots");
            }
        }
        
        // 6. Clear toolbar slots (except hand tool at slot 0)
        var toolManager = FindObjectOfType<ToolManager>();
        if (toolManager != null)
        {
            // Clear all toolbar slots (slots 1-8, keep hand tool at slot 0)
            for (int i = 1; i < 9; i++)
            {
                toolManager.SetToolAtIndex(i, null);
            }
            
            // Ensure hand tool is at slot 0
            toolManager.EnsureHandTool();
            
            // Update toolbar display
            toolManager.UpdateToolbarDisplay();
            
            if (showDebugLogs)
            {
                Debug.Log("[GameOverManager] Cleared toolbar slots (except hand tool at slot 0)");
            }
        }
        
        // 7. Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[GameOverManager] Local game state reset completed - game should be playable again");
        }
    }
    
    // Public methods for external access
    public void ShowGameOver()
    {
        HandlePlayerDeath();
    }
    
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    public void ResetGameOverState()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    // Context menu for testing
    [ContextMenu("Test Game Over")]
    private void TestGameOver()
    {
        ShowGameOver();
    }
} 