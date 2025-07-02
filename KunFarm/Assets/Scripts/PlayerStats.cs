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
    [SerializeField] private float hungerDecreaseRate = 1f; // Hunger giảm mỗi giây
    [SerializeField] private float healthDecreaseRate = 5f; // Health giảm khi đói
    
    [Header("Speed Penalty")]
    [SerializeField] private float speedPenalty50Percent = 1f; // Giảm 1 đơn vị speed khi hunger <= 50%
    [SerializeField] private float speedPenalty30Percent = 2f; // Giảm thêm 2 đơn vị speed khi hunger <= 30%
    
    [Header("Runtime Data")]
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float currentHunger = 100f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
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
    
    void Awake()
    {
        playerMovement = GetComponent<Movement>();
        
        // Khởi tạo full stats
        currentHealth = maxHealth;
        currentHunger = maxHunger;
    }
    
    void Start()
    {
        // Trigger initial UI update
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHungerChanged?.Invoke(currentHunger, maxHunger);
        
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerStats] Initialized - Health: {currentHealth}/{maxHealth}, Hunger: {currentHunger}/{maxHunger}");
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        UpdateHunger();
        UpdateHealth();
        UpdateSpeedModifier();
        
        if (showDebugInfo && Time.frameCount % 60 == 0) // Log mỗi giây
        {
            Debug.Log($"[PlayerStats] H: {currentHealth:F1}/{maxHealth} ({HealthPercent:P1}) | F: {currentHunger:F1}/{maxHunger} ({HungerPercent:P1}) | Speed: {SpeedModifier:F2}x");
        }
    }
    
    private void UpdateHunger()
    {
        if (currentHunger > 0)
        {
            currentHunger -= hungerDecreaseRate * Time.deltaTime;
            currentHunger = Mathf.Max(0, currentHunger);
            OnHungerChanged?.Invoke(currentHunger, maxHunger);
        }
    }
    
    private void UpdateHealth()
    {
        // Health chỉ giảm khi hunger = 0
        if (currentHunger <= 0 && currentHealth > 0)
        {
            currentHealth -= healthDecreaseRate * Time.deltaTime;
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
} 