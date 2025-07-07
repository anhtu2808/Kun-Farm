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
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
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
        if (showDebugInfo)
            Debug.Log("[ChickenManager] Initialized with auto-save: " + autoSave);
            
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
        
        if (_chickenPrefab == null)
            Debug.LogError($"[ChickenManager] Không tìm thấy prefab gà tại: Resources/{chickenPrefabPath}");
        
        if (_eggPrefab == null)
            Debug.LogError($"[ChickenManager] Không tìm thấy prefab trứng tại: Resources/{eggPrefabPath}");
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
        
        if (isValid && showDebugInfo)
            Debug.Log("[ChickenManager] Tất cả resources đều hợp lệ!");
        else if (!isValid)
            Debug.LogError("[ChickenManager] Một số resources không hợp lệ!");
            
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
        
        if (showDebugInfo)
            Debug.Log($"[ChickenManager] Đã tạo gà {chickenId} tại: {position}");
            
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
            
        if (showDebugInfo)
            Debug.Log($"[ChickenManager] Đã tạo trứng tại: {position}");
            
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
        
        if (showDebugInfo)
            Debug.Log($"[ChickenManager] Đã đăng ký gà: {chickenId}");
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
            if (showDebugInfo)
                Debug.Log($"[ChickenManager] Đã hủy đăng ký gà: {chickenData.chickenId}");
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
        
        if (showDebugInfo)
            Debug.Log($"[ChickenManager] Gà {chickenId} đã được cho ăn! Tăng tốc đẻ trứng {feedingSpeedBoost * 100}% trong {feedingDuration}s");
        
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
                
                if (showDebugInfo)
                    Debug.Log($"[ChickenManager] Hiệu ứng feeding cho gà {chickenId} đã hết hạn");
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
        
        // TODO: Gửi dữ liệu lên database
        // Hiện tại lưu vào PlayerPrefs để test
        foreach (var kvp in chickenStates)
        {
            string json = JsonUtility.ToJson(kvp.Value);
            PlayerPrefs.SetString($"ChickenState_{kvp.Key}", json);
        }
        
        PlayerPrefs.Save();
        
        if (showDebugInfo)
            Debug.Log($"[ChickenManager] Đã save {chickenStates.Count} chicken states");
    }
    
    public void LoadAllChickenStates()
    {
        // TODO: Load từ database
        // Hiện tại load từ PlayerPrefs để test
        chickenStates.Clear();
        
        // Đây là cách tạm thời, trong thực tế sẽ có API để lấy danh sách
        // For now, we'll leave this empty and let chickens register themselves
        
        if (showDebugInfo)
            Debug.Log("[ChickenManager] Đã load chicken states");
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
        // Có thể thêm các thông tin khác như health, hunger, v.v.
    }
    
    #endregion

    #region Public API
    
    public ChickenState GetChickenState(string chickenId)
    {
        return chickenStates.ContainsKey(chickenId) ? chickenStates[chickenId] : null;
        }
        
    public List<ChickenWalk> GetAllChickens()
    {
        allChickens.RemoveAll(c => c == null); // Clean up null references
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
        
        if (showDebugInfo)
            Debug.Log($"[ChickenManager] Đã buộc {GetTotalChickenCount()} gà đẻ trứng");
    }
    
    #endregion

    #region Timing Settings API
    
    /// <summary>
    /// Lấy thời gian đẻ trứng mặc định
    /// </summary>
    public float GetDefaultEggLayInterval()
    {
        return defaultEggLayInterval;
    }
    
    /// <summary>
    /// Đặt thời gian đẻ trứng mặc định
    /// </summary>
    public void SetDefaultEggLayInterval(float newInterval)
    {
        defaultEggLayInterval = newInterval;
        
        // Cập nhật tất cả gà hiện tại
        foreach (var chicken in allChickens)
        {
            if (chicken != null && !chicken.IsBoosted())
            {
                chicken.UpdateEggLayInterval(defaultEggLayInterval);
            }
        }
        
        if (showDebugInfo)
            Debug.Log($"[ChickenManager] Đã đặt thời gian đẻ trứng mặc định: {defaultEggLayInterval}s");
        }
        
    /// <summary>
    /// Lấy thời gian nở trứng mặc định
    /// </summary>
    public float GetDefaultHatchTime()
    {
        return defaultHatchTime;
    }
    
    /// <summary>
    /// Đặt thời gian nở trứng mặc định
    /// </summary>
    public void SetDefaultHatchTime(float newHatchTime)
    {
        defaultHatchTime = newHatchTime;
        
        if (showDebugInfo)
            Debug.Log($"[ChickenManager] Đã đặt thời gian nở trứng mặc định: {defaultHatchTime}s");
    }
    
    /// <summary>
    /// Get feeding speed boost percentage (0.5 = 50% faster)
    /// </summary>
    public float GetFeedingSpeedBoost()
    {
        return feedingSpeedBoost;
    }
    
    /// <summary>
    /// Get feeding duration in seconds
    /// </summary>
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
    
    // Thêm thông tin chi tiết
    public float currentEggLayTimer; // Thời gian đã đếm cho lần đẻ trứng tiếp theo
    public float currentEggLayInterval; // Thời gian đẻ trứng hiện tại (có thể bị ảnh hưởng bởi feeding)
    public float baseEggLayInterval; // Thời gian đẻ trứng gốc
    public bool isMoving;
    public Vector2 moveDirection;
    public float speedMultiplier;
    public bool isBoosted;
    
    // Metadata
    public float savedAtGameTime; // Time.time khi save
    public string sceneName; // Scene nào gà được save
}

/// <summary>
/// Component đính kèm vào mỗi con gà để lưu trữ ID
/// </summary>
public class ChickenData : MonoBehaviour
{
    public string chickenId;
} 