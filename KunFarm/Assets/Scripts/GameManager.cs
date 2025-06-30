using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    public bool autoSave = true;
    public bool showDebugInfo = false;

    public static GameManager instance;

    public ItemManager itemManager;
    public TileManager tileManager;
    public FarmManager farmManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of GameManager detected. Destroying the new instance.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
        itemManager = GetComponent<ItemManager>();
        tileManager = GetComponent<TileManager>();
        farmManager = GetComponent<FarmManager>();
        
        // Ensure components exist
        EnsureGameSaver();
        EnsureFarmManager();
    }

    private void Start()
    {
        // Any initial setup
    }

    private void EnsureGameSaver()
    {
        if (GameSaver.Instance == null)
        {
            GameObject gameSaverGO = new GameObject("GameSaver");
            gameSaverGO.AddComponent<GameSaver>();
            DontDestroyOnLoad(gameSaverGO);
        }
    }

    private void EnsureFarmManager()
    {
        if (farmManager == null)
        {
            farmManager = gameObject.AddComponent<FarmManager>();
            Debug.Log("FarmManager added to GameManager for auto-save functionality");
        }
    }
}
