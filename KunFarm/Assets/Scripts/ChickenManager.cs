using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager quản lý tất cả gà trong game - MonoBehaviour
/// Tính năng: Spawn gà/trứng, feeding system, save/load state
/// </summary>
public class ChickenManager : MonoBehaviour
{
    [Header("Resource Settings")]
    [SerializeField] private string chickenPrefabPath = "Prelabs/Chicken";
    [SerializeField] private string eggPrefabPath = "Prelabs/Collectable";
    
    [Header("Timing Settings")]
    [SerializeField] private float defaultEggLayInterval = 180f; // Thời gian đẻ trứng mặc định (giây)
    [SerializeField] private float defaultHatchTime = 180f; // Thời gian nở trứng mặc định (giây)
    
    [Header("Feeding System")]
    [SerializeField] private float feedingSpeedBoost = 0.5f; // Giảm 50% thời gian đẻ trứng
    [SerializeField] private float feedingDuration = 300f; // Hiệu ứng kéo dài 5 phút
    [SerializeField] private CollectableType wheatType = CollectableType.WHEAT;
    
    [Header("Save System")]
    [SerializeField] private bool autoSave = true;
    [SerializeField] private float autoSaveInterval = 60f; // Auto save mỗi 1 phút
    
    
    // Singleton pattern
    public static ChickenManager Instance { get; private set; }
    
    // Cache prefabs
    private GameObject _chickenPrefab;
    private GameObject _eggPrefab;
    
    // Quản lý trạng thái gà
    private Dictionary<string, ChickenState> chickenStates = new Dictionary<string, ChickenState>();
    private List<ChickenWalk> allChickens = new List<ChickenWalk>();
    
    // Coroutines
    private Coroutine autoSaveCoroutine;

    #region Unity Lifecycle
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPrefabs();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Bắt đầu auto save nếu được bật
        if (autoSave)
        {
            StartAutoSave();
        }
        
        // Load chicken states từ database (nếu có)
        LoadAllChickenStates();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            // Save trước khi destroy
            SaveAllChickenStates();
            Instance = null;
        }
    }
    
    #endregion

    #region Resource Management
    
    private void LoadPrefabs()
    {
        _chickenPrefab = Resources.Load<GameObject>(chickenPrefabPath);
        _eggPrefab = Resources.Load<GameObject>(eggPrefabPath);
    }
    
    public GameObject GetChickenPrefab()
    {
        if (_chickenPrefab == null)
            LoadPrefabs();
        return _chickenPrefab;
    }
    
    public GameObject GetEggPrefab()
    {
        if (_eggPrefab == null)
            LoadPrefabs();
        return _eggPrefab;
    }
    
    public bool ValidateResources()
    {
        bool isValid = GetChickenPrefab() != null && GetEggPrefab() != null;
        return isValid;
    }
    
    #endregion

    #region Spawn Management
    
    public GameObject SpawnChicken(Vector3 position)
    {
        GameObject chickenPrefab = GetChickenPrefab();
        if (chickenPrefab == null) return null;
        
        position.z = -10f;
        GameObject newChicken = Instantiate(chickenPrefab, position, Quaternion.identity);
        string chickenId = "Chicken_" + System.DateTime.Now.Ticks;
        newChicken.name = chickenId;
        
        // Đăng ký gà mới
        ChickenWalk chickenWalk = newChicken.GetComponent<ChickenWalk>();
        if (chickenWalk != null)
        {
            RegisterChicken(chickenWalk, chickenId);
        }
        
        return newChicken;
    }
    
    public GameObject SpawnEgg(Vector3 position)
    {
        GameObject eggPrefab = GetEggPrefab();
        if (eggPrefab == null) return null;
        
        position.z = 0f;
        GameObject newEgg = Instantiate(eggPrefab, position, Quaternion.identity);
        newEgg.name = "Egg_" + System.DateTime.Now.Ticks;
        
        // Đảm bảo trứng có type là EGG
        Collectable collectableComponent = newEgg.GetComponent<Collectable>();
        if (collectableComponent != null)
        {
            collectableComponent.type = CollectableType.EGG;
        }
        
        return newEgg;
    }
    
    #endregion

    #region Chicken Registration & Management
    
    public void RegisterChicken(ChickenWalk chicken, string chickenId)
    {
        if (chicken == null) return;
        
        // Thêm vào danh sách quản lý
        if (!allChickens.Contains(chicken))
        {
            allChickens.Add(chicken);
        }
        
        // Tạo state mới nếu chưa có
        if (!chickenStates.ContainsKey(chickenId))
        {
            chickenStates[chickenId] = new ChickenState
            {
                chickenId = chickenId,
                position = chicken.transform.position,
                isFed = false,
                feedEndTime = 0f,
                totalEggsLaid = 0,
                lastSaved = System.DateTime.Now,
                currentEggLayTimer = 0f,
                currentEggLayInterval = defaultEggLayInterval,
                baseEggLayInterval = defaultEggLayInterval,
                isMoving = false,
                moveDirection = Vector2.zero,
                speedMultiplier = 1f,
                isBoosted = false,
                savedAtGameTime = Time.time,
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            };
        }
        
        // Gán ID cho gà
        ChickenData chickenData = chicken.GetComponent<ChickenData>();
        if (chickenData == null)
        {
            chickenData = chicken.gameObject.AddComponent<ChickenData>();
        }
        chickenData.chickenId = chickenId;
    }
    
    public void UnregisterChicken(ChickenWalk chicken)
    {
        if (chicken == null) return;
        
        allChickens.Remove(chicken);
        
        // Lấy ID và save state cuối cùng
        ChickenData chickenData = chicken.GetComponent<ChickenData>();
        if (chickenData != null && chickenStates.ContainsKey(chickenData.chickenId))
        {
            SaveChickenState(chickenData.chickenId);
        }
    }
    
    #endregion

    #region Feeding System
    
    public bool FeedChicken(ChickenWalk chicken, CollectableType foodType)
    {
        if (chicken == null || foodType != wheatType)
            return false;
            
        ChickenData chickenData = chicken.GetComponent<ChickenData>();
        if (chickenData == null) return false;
        
        string chickenId = chickenData.chickenId;
        if (!chickenStates.ContainsKey(chickenId))
            return false;
        
        ChickenState state = chickenStates[chickenId];
        
        // Áp dụng hiệu ứng feeding
        state.isFed = true;
        state.feedEndTime = Time.time + feedingDuration;
        
        // Giảm thời gian đẻ trứng hiện tại
        float currentSpeedMultiplier = 1f - feedingSpeedBoost;
        chicken.ApplySpeedBoost(currentSpeedMultiplier);
        
        return true;
    }
    
    public bool IsChickenFed(string chickenId)
    {
        if (!chickenStates.ContainsKey(chickenId))
            return false;
        
        ChickenState state = chickenStates[chickenId];
        return state.isFed && Time.time < state.feedEndTime;
    }

    private void UpdateFeedingEffects()
    {
        foreach (var chicken in allChickens)
        {
            if (chicken == null) continue;
            
            ChickenData chickenData = chicken.GetComponent<ChickenData>();
            if (chickenData == null) continue;
            
            string chickenId = chickenData.chickenId;
            if (!chickenStates.ContainsKey(chickenId)) continue;
            
            ChickenState state = chickenStates[chickenId];
            
            // Kiểm tra nếu hiệu ứng feeding hết hạn
            if (state.isFed && Time.time >= state.feedEndTime)
            {
                state.isFed = false;
                chicken.RemoveSpeedBoost();
            }
        }
    }
    
    #endregion

    #region Save/Load System
    
    private void StartAutoSave()
    {
        if (autoSaveCoroutine != null)
            StopCoroutine(autoSaveCoroutine);
            
        autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
    }
    
    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveAllChickenStates();
        }
    }
    
    public void SaveAllChickenStates()
    {
        // Cập nhật state hiện tại trước khi save
        foreach (var chicken in allChickens)
        {
            if (chicken == null) continue;
            
            ChickenData chickenData = chicken.GetComponent<ChickenData>();
            if (chickenData == null) continue;
            
            UpdateChickenState(chickenData.chickenId, chicken);
        }
        
        // Lưu vào PlayerPrefs để test
        foreach (var kvp in chickenStates)
        {
            string json = JsonUtility.ToJson(kvp.Value);
            PlayerPrefs.SetString($"ChickenState_{kvp.Key}", json);
        }
        
        PlayerPrefs.Save();
    }
    
    public void LoadAllChickenStates()
    {
        // TODO: Load từ database
        chickenStates.Clear();
    }
    
    private void SaveChickenState(string chickenId)
    {
        if (!chickenStates.ContainsKey(chickenId))
            return;
        
        ChickenState state = chickenStates[chickenId];
        state.lastSaved = System.DateTime.Now;
        
        // TODO: Save individual chicken to database
        string json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString($"ChickenState_{chickenId}", json);
    }
    
    private void UpdateChickenState(string chickenId, ChickenWalk chicken)
    {
        if (!chickenStates.ContainsKey(chickenId) || chicken == null)
            return;
        
        ChickenState state = chickenStates[chickenId];
        state.position = chicken.transform.position;
    }
    
    #endregion

    #region Public API
    
    public ChickenState GetChickenState(string chickenId)
    {
        return chickenStates.ContainsKey(chickenId) ? chickenStates[chickenId] : null;
    }
    	
    public List<ChickenWalk> GetAllChickens()
    {
        allChickens.RemoveAll(c => c == null);
        return new List<ChickenWalk>(allChickens);
    }
    	
    public int GetTotalChickenCount()
    {
        return GetAllChickens().Count;
    }
    	
    public void ForceAllChickensLayEggs()
    	{
        foreach (var chicken in GetAllChickens())
        {
            chicken.ForceLayEgg();
        }
    }
    
    #endregion

    #region Timing Settings API
    
    public float GetDefaultEggLayInterval()
    {
        return defaultEggLayInterval;
    }
    
    public void SetDefaultEggLayInterval(float newInterval)
    {
        defaultEggLayInterval = newInterval;
        
        foreach (var chicken in allChickens)
        {
            if (chicken != null && !chicken.IsBoosted())
            {
                chicken.UpdateEggLayInterval(defaultEggLayInterval);
            }
        }
    }
    
    public float GetDefaultHatchTime()
    {
        return defaultHatchTime;
    }
    
    public void SetDefaultHatchTime(float newHatchTime)
    {
        defaultHatchTime = newHatchTime;
    }
    	
    public float GetFeedingSpeedBoost()
    {
        return feedingSpeedBoost;
    }
    
    public float GetFeedingDuration()
    {
        return feedingDuration;
    }
    
    #endregion

    private void Update()
    {
        UpdateFeedingEffects();
    }
}

/// <summary>
/// Data class để lưu trữ trạng thái gà
/// </summary>
[System.Serializable]
public class ChickenState
{
    public string chickenId;
    public Vector3 position;
    public bool isFed;
    public float feedEndTime;
    public int totalEggsLaid;
    public System.DateTime lastSaved;
    
    public float currentEggLayTimer;
    public float currentEggLayInterval;
    public float baseEggLayInterval;
    public bool isMoving;
    public Vector2 moveDirection;
    public float speedMultiplier;
    public bool isBoosted;
    
    public float savedAtGameTime;
    public string sceneName;
}

/// <summary>
/// Component đính kèm vào mỗi con gà để lưu trữ ID
/// </summary>
public class ChickenData : MonoBehaviour
{
    public string chickenId;
}
