using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Controller cho Health và Hunger bars - Pattern giống MoneyUI
/// </summary>
public class HealthHungerUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Color healthFullColor = Color.green;
    [SerializeField] private Color healthLowColor = Color.red;
    [SerializeField] private float healthLowThreshold = 0.3f;
    
    [Header("Hunger Bar")]  
    [SerializeField] private Image hungerBarFill;
    [SerializeField] private Color hungerFullColor = Color.yellow;
    [SerializeField] private Color hungerLowColor = new Color(1f, 0.5f, 0f); // Orange color
    [SerializeField] private float hungerLowThreshold = 0.5f;
    
    [Header("Animation")]
    [SerializeField] private bool enableSmoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Private fields for smooth transition
    private float targetHealthFill = 1f;
    private float targetHungerFill = 1f;
    private float currentHealthFill = 1f;
    private float currentHungerFill = 1f;
    
    void Awake()
    {
        // Tự động tìm UI elements nếu chưa assign
        if (healthBarFill == null)
        {
            healthBarFill = GameObject.Find("HealthBarFill")?.GetComponent<Image>();
        }
        
        if (hungerBarFill == null)
        {
            hungerBarFill = GameObject.Find("FoodBarFill")?.GetComponent<Image>();
        }
        
        // Validate references
        if (healthBarFill == null)
        {
            Debug.LogWarning("[HealthHungerUI] HealthBarFill not found! Please assign manually.");
        }
        
        if (hungerBarFill == null)
        {
            Debug.LogWarning("[HealthHungerUI] HungerBarFill not found! Please assign manually.");
        }
    }
    
    void OnEnable()
    {
        // Subscribe to events - Pattern giống MoneyUI
        PlayerStats.OnHealthChanged += UpdateHealthBar;
        PlayerStats.OnHungerChanged += UpdateHungerBar;
    }
    
    void OnDisable()
    {
        // Unsubscribe from events
        PlayerStats.OnHealthChanged -= UpdateHealthBar;
        PlayerStats.OnHungerChanged -= UpdateHungerBar;
    }
    
    void Update()
    {
        if (enableSmoothTransition)
        {
            AnimateBars();
        }
    }
    
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;
        
        float healthPercent = currentHealth / maxHealth;
        targetHealthFill = healthPercent;
        
        if (!enableSmoothTransition)
        {
            currentHealthFill = targetHealthFill;
            ApplyHealthVisuals();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[HealthHungerUI] Health updated: {currentHealth:F1}/{maxHealth} ({healthPercent:P1})");
        }
    }
    
    private void UpdateHungerBar(float currentHunger, float maxHunger)
    {
        if (hungerBarFill == null) return;
        
        float hungerPercent = currentHunger / maxHunger;
        targetHungerFill = hungerPercent;
        
        if (!enableSmoothTransition)
        {
            currentHungerFill = targetHungerFill;
            ApplyHungerVisuals();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[HealthHungerUI] Hunger updated: {currentHunger:F1}/{maxHunger} ({hungerPercent:P1})");
        }
    }
    
    private void AnimateBars()
    {
        bool healthChanged = false;
        bool hungerChanged = false;
        
        // Animate Health Bar
        if (Mathf.Abs(currentHealthFill - targetHealthFill) > 0.01f)
        {
            currentHealthFill = Mathf.Lerp(currentHealthFill, targetHealthFill, transitionSpeed * Time.deltaTime);
            healthChanged = true;
        }
        else if (currentHealthFill != targetHealthFill)
        {
            currentHealthFill = targetHealthFill;
            healthChanged = true;
        }
        
        // Animate Hunger Bar
        if (Mathf.Abs(currentHungerFill - targetHungerFill) > 0.01f)
        {
            currentHungerFill = Mathf.Lerp(currentHungerFill, targetHungerFill, transitionSpeed * Time.deltaTime);
            hungerChanged = true;
        }
        else if (currentHungerFill != targetHungerFill)
        {
            currentHungerFill = targetHungerFill;
            hungerChanged = true;
        }
        
        // Apply visuals if changed
        if (healthChanged) ApplyHealthVisuals();
        if (hungerChanged) ApplyHungerVisuals();
    }
    
    private void ApplyHealthVisuals()
    {
        if (healthBarFill == null) return;
        
        healthBarFill.fillAmount = currentHealthFill;
        
        // Color transition based on health level
        if (currentHealthFill <= healthLowThreshold)
        {
            healthBarFill.color = Color.Lerp(healthLowColor, healthFullColor, currentHealthFill / healthLowThreshold);
        }
        else
        {
            healthBarFill.color = healthFullColor;
        }
    }
    
    private void ApplyHungerVisuals()
    {
        if (hungerBarFill == null) return;
        
        hungerBarFill.fillAmount = currentHungerFill;
        
        // Color transition based on hunger level
        if (currentHungerFill <= hungerLowThreshold)
        {
            hungerBarFill.color = Color.Lerp(hungerLowColor, hungerFullColor, currentHungerFill / hungerLowThreshold);
        }
        else
        {
            hungerBarFill.color = hungerFullColor;
        }
    }
    
    // Public methods for testing
    [ContextMenu("Test Health Low")]
    public void TestHealthLow()
    {
        UpdateHealthBar(20f, 100f);
    }
    
    [ContextMenu("Test Hunger Low")]
    public void TestHungerLow()
    {
        UpdateHungerBar(30f, 100f);
    }
    
    [ContextMenu("Test Full Stats")]
    public void TestFullStats()
    {
        UpdateHealthBar(100f, 100f);
        UpdateHungerBar(100f, 100f);
    }
} 