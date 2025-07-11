using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

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
    private bool isRestarting = false; // Flag to prevent save during restart

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

    /// <summary>
    /// Set restart flag to prevent save operations during game restart
    /// </summary>
    public void SetRestarting(bool restarting)
    {
        isRestarting = restarting;
        if (restarting)
            Debug.Log("[FarmManager] Restart mode enabled - save operations disabled");
        else
            Debug.Log("[FarmManager] Restart mode disabled - save operations re-enabled");
    }

    public void SaveFarmState()
    {
        if (tileManager == null || ApiClient.Instance == null || isRestarting)
        {
            if (isRestarting)
                Debug.LogWarning("[FarmManager] Skipping save during restart to avoid MissingReferenceException");
            return;
        }

        var farmData = FarmSaveData.FromTileManager(tileManager, currentUserId);
        ApiClient.Instance.SaveFarmState(farmData, success => OnSaveComplete(success));
    }

    public void LoadFarmState()
    {
        if (ApiClient.Instance == null)
            return;

        isLoading = true;
        ApiClient.Instance.LoadFarmState(currentUserId, OnLoadComplete);
    }

    private void OnSaveComplete(bool success)
    {
        OnSaveComplete(success, string.Empty);
    }

    private void OnSaveComplete(bool success, string responseBody)
    {
        // no debug handling
    }

    private void TryGetUserIdFromLogin()
    {
        if (!hasTriedLogin)
        {
            hasTriedLogin = true;
            currentUserId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        }
    }

    private void OnLoadComplete(FarmSaveData farmData)
    {
        if (farmData != null && tileManager != null)
        {
            // Clear existing content
            ClearExistingPlants();
            ClearExistingChickensAndEggs();
            
            tileManager.RestoreTileStates(farmData.tileStates);
            tileManager.RestorePlants(farmData.plants);
            RestoreChickens(farmData.chickensStateJson);
            RestoreEggs(farmData.eggsStateJson);

            isLoading = false;
            loadCompleteTime = Time.time;
        }
        else
        {
            isLoading = false;
        }
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
                tileManager.SetTileState(kvp.Key, TileState.Dug);
        }
    }

    private void ClearExistingChickensAndEggs()
    {
        var existingChickens = FindObjectsOfType<ChickenWalk>();
        foreach (var chicken in existingChickens)
        {
            if (chicken != null)
            {
                if (ChickenManager.Instance != null)
                    ChickenManager.Instance.UnregisterChicken(chicken);
                Destroy(chicken.gameObject);
            }
        }
        
        var existingEggs = FindObjectsOfType<Collectable>();
        foreach (var collectable in existingEggs)
        {
            if (collectable != null && collectable.type == CollectableType.EGG)
                Destroy(collectable.gameObject);
        }
    }

    private void RestoreChickens(string chickensStateJson)
    {
        if (string.IsNullOrEmpty(chickensStateJson) || ChickenManager.Instance == null)
            return;

        var wrapper = JsonUtility.FromJson<ChickenStatesWrapper>(chickensStateJson);
        if (wrapper?.chickens == null || wrapper.chickens.Count == 0)
            return;

        foreach (var chickenSaveState in wrapper.chickens)
        {
            Vector3 position = new Vector3(chickenSaveState.positionX, chickenSaveState.positionY, chickenSaveState.positionZ);
            var restoredChicken = ChickenManager.Instance.SpawnChicken(position);
            if (restoredChicken != null)
            {
                var chickenData = restoredChicken.GetComponent<ChickenData>() ?? restoredChicken.AddComponent<ChickenData>();
                chickenData.chickenId = chickenSaveState.chickenId;
                restoredChicken.name = chickenSaveState.chickenId;

                var chickenWalk = restoredChicken.GetComponent<ChickenWalk>();
                if (chickenWalk != null)
                {
                    ChickenManager.Instance.RegisterChicken(chickenWalk, chickenSaveState.chickenId);
                    var state = ChickenManager.Instance.GetChickenState(chickenSaveState.chickenId);
                    state.position = position;
                    state.isFed = chickenSaveState.isFed;
                    state.feedEndTime = chickenSaveState.feedEndTime;
                    state.totalEggsLaid = chickenSaveState.totalEggsLaid;
                    state.currentEggLayInterval = chickenSaveState.currentEggLayInterval;
                    state.baseEggLayInterval = chickenSaveState.baseEggLayInterval;
                    state.isMoving = chickenSaveState.isMoving;
                    state.speedMultiplier = chickenSaveState.speedMultiplier;
                    state.isBoosted = chickenSaveState.isBoosted;

                    chickenWalk.eggLayInterval = chickenSaveState.currentEggLayInterval;
                    chickenWalk.SetEggLayTimer(chickenSaveState.eggLayTimer);
                }
            }
        }
    }

    private void RestoreEggs(string eggsStateJson)
    {
        if (string.IsNullOrEmpty(eggsStateJson) || ChickenManager.Instance == null)
            return;

        var wrapper = JsonUtility.FromJson<EggStatesWrapper>(eggsStateJson);
        if (wrapper?.eggs == null || wrapper.eggs.Count == 0)
            return;

        foreach (var eggSaveState in wrapper.eggs)
        {
            Vector3 position = new Vector3(eggSaveState.positionX, eggSaveState.positionY, eggSaveState.positionZ);
            var restoredEgg = ChickenManager.Instance.SpawnEgg(position);
            if (restoredEgg != null)
            {
                restoredEgg.name = eggSaveState.eggId;
                var collectable = restoredEgg.GetComponent<Collectable>();
                if (collectable != null)
                {
                    collectable.type = CollectableType.EGG;
                    collectable.HatchTime = eggSaveState.hatchTime;
                    collectable.HatchTimer = eggSaveState.hatchTimer;
                }
            }
        }
    }

    public void SaveAndQuit()
    {
        SaveFarmState();
    }

    [ContextMenu("Clear Farm State")]
    public void ClearFarmState()
    {
        if (tileManager != null)
        {
            ClearExistingPlants();
            var allTileStates = tileManager.GetAllTileStates();
            foreach (var key in allTileStates.Keys.ToList())
                tileManager.SetTileState(key, TileState.Undug);
        }

        ClearExistingChickensAndEggs();
        SaveFarmState();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveEnabled && !isLoading && !isRestarting && (loadCompleteTime == 0f || Time.time - loadCompleteTime > 5f))
            SaveFarmState();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && autoSaveEnabled && !isRestarting)
        {
            float timeSinceLoad = loadCompleteTime > 0f ? Time.time - loadCompleteTime : 0f;
            if (!isLoading && (loadCompleteTime == 0f || timeSinceLoad > 5f))
            {
                SaveFarmState();
            }
        }
    }
}
