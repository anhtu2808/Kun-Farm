using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Quản lý Health và Hunger của Player - Pattern giống Wallet
/// </summary>
[RequireComponent(typeof(Transform))]
public class PlayerStats : MonoBehaviour
{
    [Header("Stats Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float baseHungerDecreaseRate = 0.33f; // Base rate (chậm 3 lần)
    [SerializeField] private float baseHealthDecreaseRate = 1.67f; // Base rate (chậm 3 lần)
    [SerializeField] private float movementMultiplier = 2f; // Nhân 2 khi di chuyển
    
    [Header("Speed Penalty")]
    [SerializeField] private float speedPenalty50Percent = 1f; // Giảm 1 đơn vị speed khi hunger <= 50%
    [SerializeField] private float speedPenalty30Percent = 2f; // Giảm thêm 2 đơn vị speed khi hunger <= 30%
    
    [Header("Runtime Data")]
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float currentHunger = 100f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("Auto Save")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float saveThreshold = 5f; // Save khi health hoặc hunger thay đổi >= 5 points
    
    // Properties
    public float Health => currentHealth;
    public float Hunger => currentHunger;
    public float MaxHealth => maxHealth;
    public float MaxHunger => maxHunger;
    public float HealthPercent => currentHealth / maxHealth;
    public float HungerPercent => currentHunger / maxHunger;
    
    // Events - Pattern giống Wallet
    public static event Action<float, float> OnHealthChanged; // current, max
    public static event Action<float, float> OnHungerChanged; // current, max
    public static event Action OnPlayerDied;
    
    // Speed modifier để Movement sử dụng
    public float SpeedModifier { get; private set; } = 1f;
    
    // References
    private Movement playerMovement;
    private bool isDead = false;
    
    // Auto save tracking
    private float lastSavedHealth = 100f;
    private float lastSavedHunger = 100f;
    private int userId = 0;
    
    void Awake()
    {
        playerMovement = GetComponent<Movement>();
        
        // Khởi tạo full stats
        currentHealth = maxHealth;
        currentHunger = maxHunger;
    }
    
    void Start()
    {
        // Lấy userId từ PlayerPrefs
        userId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        
        // Trigger initial UI update
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHungerChanged?.Invoke(currentHunger, maxHunger);
        
        // Initialize save tracking
        lastSavedHealth = currentHealth;
        lastSavedHunger = currentHunger;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerStats] Initialized - Health: {currentHealth}/{maxHealth}, Hunger: {currentHunger}/{maxHunger}, UserId: {userId}");
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        UpdateHunger();
        UpdateHealth();
        UpdateSpeedModifier();
        
        // Auto save khi có thay đổi lớn
        if (autoSaveEnabled && ShouldSave())
        {
            SaveToServer();
        }
        
        if (showDebugInfo && Time.frameCount % 60 == 0) // Log mỗi giây
        {
            bool isMoving = playerMovement != null && playerMovement.IsMoving();
            string movingText = isMoving ? "Moving (2x)" : "Idle (1x)";
            Debug.Log($"[PlayerStats] H: {currentHealth:F1}/{maxHealth} ({HealthPercent:P1}) | F: {currentHunger:F1}/{maxHunger} ({HungerPercent:P1}) | Speed: {SpeedModifier:F2}x | {movingText}");
        }
    }
    
    private void UpdateHunger()
    {
        if (currentHunger > 0)
        {
            // Tính tốc độ giảm hunger dựa trên di chuyển
            bool isMoving = playerMovement != null && playerMovement.IsMoving();
            float actualRate = isMoving ? baseHungerDecreaseRate * movementMultiplier : baseHungerDecreaseRate;
            
            currentHunger -= actualRate * Time.deltaTime;
            currentHunger = Mathf.Max(0, currentHunger);
            OnHungerChanged?.Invoke(currentHunger, maxHunger);
        }
    }
    
    private void UpdateHealth()
    {
        // Health chỉ giảm khi hunger = 0
        if (currentHunger <= 0 && currentHealth > 0)
        {
            // Tính tốc độ giảm health dựa trên di chuyển
            bool isMoving = playerMovement != null && playerMovement.IsMoving();
            float actualRate = isMoving ? baseHealthDecreaseRate * movementMultiplier : baseHealthDecreaseRate;
            
            currentHealth -= actualRate * Time.deltaTime;
            currentHealth = Mathf.Max(0, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Check death
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }
    }
    
    private void UpdateSpeedModifier()
    {
        float hungerPercent = HungerPercent;
        float newSpeedModifier = 1f;
        
        if (hungerPercent <= 0.3f) // 30% or less
        {
            newSpeedModifier = 1f - (speedPenalty50Percent + speedPenalty30Percent) / 5f; // Chia 5 để convert thành multiplier
        }
        else if (hungerPercent <= 0.5f) // 50% or less
        {
            newSpeedModifier = 1f - speedPenalty50Percent / 5f;
        }
        
        SpeedModifier = Mathf.Clamp(newSpeedModifier, 0.1f, 1f); // Tối thiểu 10% speed
        
        // Update Movement component
        if (playerMovement != null)
        {
            playerMovement.SetSpeedModifier(SpeedModifier);
        }
    }
    
    private void Die()
    {
        isDead = true;
        Debug.Log("[PlayerStats] Player died!");
        OnPlayerDied?.Invoke();
    }
    
    // Public methods để test hoặc restore
    public void RestoreHealth(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerStats] Health restored by {amount}. Current: {currentHealth}/{maxHealth}");
        }
    }
    
    public void RestoreHunger(float amount)
    {
        if (isDead) return;
        
        currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
        OnHungerChanged?.Invoke(currentHunger, maxHunger);
        
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerStats] Hunger restored by {amount}. Current: {currentHunger}/{maxHunger}");
        }
    }
    
    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        SpeedModifier = 1f;
        
        // Trigger events trước
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHungerChanged?.Invoke(currentHunger, maxHunger);
        
        // Force reset UI màu sắc sau 1 frame để đảm bảo UI đã update
        StartCoroutine(ForceResetUIColors());
        
        if (playerMovement != null)
        {
            playerMovement.SetSpeedModifier(SpeedModifier);
        }
        
        Debug.Log("[PlayerStats] Player revived!");
    }
    
    /// <summary>
    /// Set health và hunger từ server data
    /// </summary>
    public void LoadFromServer(float health, float hunger)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        currentHunger = Mathf.Clamp(hunger, 0f, maxHunger);
        
        // Update tracking values để tránh auto-save ngay sau load
        lastSavedHealth = currentHealth;
        lastSavedHunger = currentHunger;
        
        // Update speed modifier based on loaded hunger
        UpdateSpeedModifier();
        
        // Trigger events để update UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHungerChanged?.Invoke(currentHunger, maxHunger);
        
        Debug.Log($"[PlayerStats] Loaded from server - Health: {currentHealth}/{maxHealth}, Hunger: {currentHunger}/{maxHunger}");
    }
    
    private System.Collections.IEnumerator ForceResetUIColors()
    {
        yield return null; // Đợi 1 frame
        
        // Tìm và reset UI colors
        HealthHungerUI healthUI = FindObjectOfType<HealthHungerUI>();
        if (healthUI != null)
        {
            healthUI.ForceUpdateBars(1f, 1f); // Force update to 100%
            healthUI.ResetToFullColors(); // Reset colors
            
            if (showDebugInfo)
            {
                Debug.Log("[PlayerStats] UI colors force reset after revive");
            }
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có nên save không dựa trên threshold
    /// </summary>
    private bool ShouldSave()
    {
        if (userId <= 0) return false;
        
        float healthChange = Mathf.Abs(currentHealth - lastSavedHealth);
        float hungerChange = Mathf.Abs(currentHunger - lastSavedHunger);
        
        return healthChange >= saveThreshold || hungerChange >= saveThreshold;
    }
    
    /// <summary>
    /// Save health và hunger lên server
    /// </summary>
    private void SaveToServer()
    {
        if (userId <= 0 || GameSaver.Instance == null)
            return;
        
        // Lấy position từ player
        Vector3 position = transform.position;
        
        // Lấy money từ wallet
        int money = 0;
        var wallet = FindObjectOfType<Wallet>();
        if (wallet != null)
        {
            money = wallet.Money;
        }
        
        var saveData = new PlayerSaveData
        {
            userId = userId,
            money = money,
            posX = position.x,
            posY = position.y,
            posZ = position.z,
            health = currentHealth,
            hunger = currentHunger
        };
        
        GameSaver.Instance.SaveGame(saveData);
        
        // Update tracking values
        lastSavedHealth = currentHealth;
        lastSavedHunger = currentHunger;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerStats] Auto-saved to server - Health: {currentHealth:F1}, Hunger: {currentHunger:F1}");
        }
    }
} 