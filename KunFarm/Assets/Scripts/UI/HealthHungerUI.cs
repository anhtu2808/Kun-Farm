using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Controller cho Health và Hunger bars - Pattern giống MoneyUI
/// </summary>
public class HealthHungerUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Color healthColor = Color.red;
    [SerializeField] private bool useStaticHealthColor = true;
    
    [Header("Hunger Bar")]  
    [SerializeField] private Image hungerBarFill;
    [SerializeField] private Color hungerFullColor = Color.yellow;
    [SerializeField] private Color hungerLowColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private float hungerLowThreshold = 0.5f;
    
    [Header("Animation")]
    [SerializeField] private bool enableSmoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;
    


    private float targetHealthFill = 1f;
    private float targetHungerFill = 1f;
    private float currentHealthFill = 1f;
    private float currentHungerFill = 1f;
    
    void Awake()
    {
        if (healthBarFill == null)
        {
            healthBarFill = GameObject.Find("HealthBarFill")?.GetComponent<Image>();
        }
        
        if (hungerBarFill == null)
        {
            hungerBarFill = GameObject.Find("FoodBarFill")?.GetComponent<Image>();
            
            if (hungerBarFill == null)
                hungerBarFill = GameObject.Find("HungerBarFill")?.GetComponent<Image>();
            
            if (hungerBarFill == null)
                hungerBarFill = GameObject.Find("Hunger Bar Fill")?.GetComponent<Image>();
                
            if (hungerBarFill == null)
                hungerBarFill = GameObject.Find("Food Bar Fill")?.GetComponent<Image>();
                

        }
        

    }
    
    void OnEnable()
    {
        PlayerStats.OnHealthChanged += UpdateHealthBar;
        PlayerStats.OnHungerChanged += UpdateHungerBar;
    }
    
    void Start()
    {
        ResetToFullColors();
    }
    
    void OnDisable()
    {
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
        
        if (useStaticHealthColor)
        {
            healthBarFill.color = healthColor;
        }
        else
        {
            healthBarFill.color = healthColor;
        }
    }
    
    private void ApplyHungerVisuals()
    {
        if (hungerBarFill == null) return;
        
        hungerBarFill.fillAmount = currentHungerFill;
        
        if (currentHungerFill <= hungerLowThreshold)
        {
            float colorLerpFactor = currentHungerFill / hungerLowThreshold;
            hungerBarFill.color = Color.Lerp(hungerLowColor, hungerFullColor, colorLerpFactor);
        }
        else
        {
            hungerBarFill.color = hungerFullColor;
        }
    }
    
    public void ResetToFullColors()
    {
        if (healthBarFill != null)
        {
            healthBarFill.color = healthColor;
        }
        
        if (hungerBarFill != null)
        {
            hungerBarFill.color = hungerFullColor;
        }
    }
    
    public void ForceUpdateBars(float healthPercent, float hungerPercent)
    {
        targetHealthFill = healthPercent;
        targetHungerFill = hungerPercent;
        currentHealthFill = healthPercent;
        currentHungerFill = hungerPercent;
        
        ApplyHealthVisuals();
        ApplyHungerVisuals();
    }
    
    [ContextMenu("Test Health Low (Red)")]
    public void TestHealthLow()
    {
        UpdateHealthBar(20f, 100f);
    }
    
    [ContextMenu("Test Hunger Low (Orange)")]
    public void TestHungerLow()
    {
        UpdateHungerBar(30f, 100f);
    }
    
    [ContextMenu("Test Hunger 50% (Transition)")]
    public void TestHunger50()
    {
        UpdateHungerBar(50f, 100f);
    }
    
    [ContextMenu("Test Full Stats")]
    public void TestFullStats()
    {
        UpdateHealthBar(100f, 100f);
        UpdateHungerBar(100f, 100f);
    }
    
    [ContextMenu("Reset Colors (Red/Yellow)")]
    public void TestResetColors()
    {
        ResetToFullColors();
    }
    

} 