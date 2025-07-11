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
    [SerializeField] private string restartButtonText = "Chơi lại";
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
        // Resume time scale
        Time.timeScale = 1f;
        
        // Option 1: Revive player in current scene
        if (playerStats != null)
        {
            playerStats.Revive();
            isGameOver = false;
            
            // Hide game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            if (showDebugLogs)
            {
                Debug.Log("[GameOverManager] Player revived in current scene");
            }
        }
        // Option 2: Reload current scene
        else
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
            
            if (showDebugLogs)
            {
                Debug.Log($"[GameOverManager] Reloading scene: {currentSceneName}");
            }
        }
        
        yield return null;
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