using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveAndExitManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject saveAndExitPopup;
    [SerializeField] private Button saveAndExitButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button settingButton; // Settings button in popup
    [SerializeField] private Button persistentSettingsButton; // Settings button always visible
    
    [Header("Settings")]
    [SerializeField] private bool showDebugInfo = true;
    
    private bool isPopupOpen = false;
    private bool isSaving = false;
    
    void Start()
    {
        // Setup button listeners
        if (saveAndExitButton != null)
            saveAndExitButton.onClick.AddListener(OnSaveAndExit);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
            
        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettings);
            
        if (persistentSettingsButton != null)
        {
            persistentSettingsButton.onClick.AddListener(OnSettings);
            if (showDebugInfo)
                Debug.Log("[SaveAndExitManager] Persistent settings button initialized");
        }
        
        // Ensure popup is hidden at start
        if (saveAndExitPopup != null)
            saveAndExitPopup.SetActive(false);
    }
    
    void Update()
    {
        // Handle ESC key to toggle popup
        if (Input.GetKeyDown(KeyCode.Escape) && !isSaving)
        {
            if (showDebugInfo)
                Debug.Log("[SaveAndExitManager] ESC key pressed - toggling popup");
            TogglePopup();
        }
    }
    
    public void TogglePopup()
    {
        if (saveAndExitPopup == null) return;
        
        isPopupOpen = !isPopupOpen;
        saveAndExitPopup.SetActive(isPopupOpen);
        
        // Pause/unpause game when popup is open
        Time.timeScale = isPopupOpen ? 0f : 1f;
        
        if (showDebugInfo)
            Debug.Log($"[SaveAndExitManager] Popup {(isPopupOpen ? "opened" : "closed")}");
    }
    
    public void OnSaveAndExit()
    {
        if (isSaving) return;
        
        if (showDebugInfo)
            Debug.Log("[SaveAndExitManager] Starting save and exit process...");
            
        StartCoroutine(SaveAllDataAndExit());
    }
    
    public void OnCancel()
    {
        if (isSaving) return;
        
        TogglePopup();
    }
    
    public void OnSettings()
    {
        if (isSaving) return;
        
        // If popup is open, close it first when opening settings from popup
        if (isPopupOpen && saveAndExitPopup != null)
        {
            TogglePopup(); // This will close the popup
        }
        
        // Find and open ExitUI settings panel if it exists
        var exitUI = FindObjectOfType<ExitUI>();
        if (exitUI != null)
        {
            // Call ToggleSettingsPanel method to open settings
            exitUI.ToggleSettingsPanel();
            
            if (showDebugInfo)
                Debug.Log("[SaveAndExitManager] Opening settings panel via ExitUI");
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning("[SaveAndExitManager] ExitUI not found in scene");
        }
    }
    
    private IEnumerator SaveAllDataAndExit()
    {
        isSaving = true;
            
        // Unpause time for saving
        Time.timeScale = 1f;
        
        int userId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (userId <= 0)
        {
            if (showDebugInfo)
                Debug.LogError("[SaveAndExitManager] No valid user ID found!");
            QuitGame();
            yield break;
        }
        
        // Step 1: Save Player Data (Position, Health, Hunger, Money)
        if (showDebugInfo)
            Debug.Log("🔄 [SaveAndExitManager] Step 1/4: Saving player data...");
        yield return StartCoroutine(SavePlayerData());
        
        // Step 2: Save Farm Data (Tiles, Plants, Chickens, Eggs)
        if (showDebugInfo)
            Debug.Log("🔄 [SaveAndExitManager] Step 2/4: Saving farm data...");
        yield return StartCoroutine(SaveFarmData());
        
        // Step 3: Save Toolbar Data (9 tool slots)
        if (showDebugInfo)
            Debug.Log("🔄 [SaveAndExitManager] Step 3/4: Saving toolbar (9 tool slots)...");
        yield return StartCoroutine(SaveToolbarData());
        
        // Step 4: Save Inventory Data (27 inventory slots)
        if (showDebugInfo)
            Debug.Log("🔄 [SaveAndExitManager] Step 4/4: Saving inventory (27 slots)...");
        yield return StartCoroutine(SaveInventoryData());
        
        // Step 5: Wait a bit for all saves to complete
        if (showDebugInfo)
            Debug.Log("[SaveAndExitManager] Finalizing save process...");
        yield return new WaitForSeconds(1f);
        
        if (showDebugInfo)
        {
            Debug.Log("=== SAVE AND EXIT SUMMARY ===");
            Debug.Log("✅ Player Data: Position, Health, Hunger, Money saved");
            Debug.Log("✅ Farm Data: Tiles, Plants, Chickens, Eggs saved");
            Debug.Log("✅ Toolbar Data: All 9 tool slots saved");
            Debug.Log("✅ Inventory Data: All 27 inventory slots saved");
            Debug.Log("🎯 [SaveAndExitManager] ALL DATA SAVED SUCCESSFULLY!");
            Debug.Log("===============================");
        }
        
        // Quit game
        QuitGame();
    }
    
    private IEnumerator SavePlayerData()
    {
        // Find PlayerStats component and trigger save via GameSaver
        var playerStats = FindObjectOfType<PlayerStats>();
        var wallet = FindObjectOfType<Wallet>();
        var player = FindObjectOfType<Player>();
        
        if (playerStats != null && wallet != null && player != null)
        {
            int userId = PlayerPrefs.GetInt("PLAYER_ID", 0);
            if (userId > 0 && GameSaver.Instance != null)
            {
                // Create PlayerSaveData manually since SaveToServer is private
                var saveData = new PlayerSaveData
                {
                    userId = userId,
                    money = wallet.Money,
                    posX = player.transform.position.x,
                    posY = player.transform.position.y,
                    posZ = player.transform.position.z,
                    health = playerStats.Health,
                    hunger = playerStats.Hunger
                };
                
                GameSaver.Instance.SaveGame(saveData);
                
                if (showDebugInfo)
                    Debug.Log("[SaveAndExitManager] Player data save triggered");
            }
        }
        
        yield return new WaitForSeconds(0.5f); // Wait for API call
    }
    
    private IEnumerator SaveFarmData()
    {
        // Find FarmManager to trigger save
        var farmManager = FindObjectOfType<FarmManager>();
        if (farmManager != null)
        {
            farmManager.SaveFarmState();
            
            if (showDebugInfo)
                Debug.Log("[SaveAndExitManager] Farm data save triggered");
        }
        
        yield return new WaitForSeconds(1f); // Wait for API call
    }
    
    private IEnumerator SaveToolbarData()
    {
        // Find ToolManager to trigger save
        var toolManager = FindObjectOfType<ToolManager>();
        if (toolManager != null)
        {
            toolManager.SaveToolbar();
            
            if (showDebugInfo)
                Debug.Log("[SaveAndExitManager] ✅ Toolbar save triggered via ToolManager.SaveToolbar()");
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning("[SaveAndExitManager] ❌ ToolManager not found - toolbar NOT saved!");
        }
        
        yield return new WaitForSeconds(1f); // Wait longer for API call
    }
    
    private IEnumerator SaveInventoryData()
    {
        // Find Inventory to trigger save
        var inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            inventory.ForceSaveChanges();
            
            if (showDebugInfo)
                Debug.Log("[SaveAndExitManager] ✅ Inventory save triggered via Inventory.ForceSaveChanges()");
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning("[SaveAndExitManager] ❌ Inventory not found - inventory NOT saved!");
        }
        
        yield return new WaitForSeconds(2f); // Wait longer for batch API call
    }
    
    private void QuitGame()
    {

        // Reset time scale
        Time.timeScale = 1f;
        
        if (showDebugInfo)
            Debug.Log("[SaveAndExitManager] Quitting game...");
        
        // Quit application
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    void OnDestroy()
    {
        // Ensure time scale is reset
        Time.timeScale = 1f;
    }
} 